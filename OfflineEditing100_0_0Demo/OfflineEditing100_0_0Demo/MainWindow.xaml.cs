using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OfflineEditing100_0_0Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string outGeodatabasePath = @"C:\Temp\100_0_0Demo\WildlifeLocal.geodatabase";
        string statusMessage = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DownloadGDB_Click(object sender, RoutedEventArgs e)
        {
            CreateOfflineData();
        }

        private async void CreateOfflineData()
        {
            // create a new GeodatabaseSyncTask to create a local version of feature service data
            var featureServiceUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer");
            var gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(featureServiceUri);
            // define an extent for the features to include
            Envelope extent = mv.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
            // get the default parameters for generating a geodatabase
            var generateGdbParams = await gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);
            // set the sync model to per layer
            generateGdbParams.SyncModel = SyncModel.Layer;
            // define the layers and features to include
            var marineLayerId = 0;
            var birdsLayerId = 1;
            var dolphinsOnlyWhereClause = "type = 11";


            // Clear and re-create the layer options
            generateGdbParams.LayerOptions.Clear();
            generateGdbParams.LayerOptions.Add(new GenerateLayerOption(marineLayerId, dolphinsOnlyWhereClause));
            generateGdbParams.LayerOptions.Add(new GenerateLayerOption(birdsLayerId));


            // do not return attachments
            generateGdbParams.ReturnAttachments = false;
            // create the generate geodatabase job, pass in the parameters and an output path for the local geodatabase
            var generateGdbJob = gdbSyncTask.GenerateGeodatabase(generateGdbParams, outGeodatabasePath);
            // handle the JobChanged event to check the status of the job
            generateGdbJob.JobChanged += OnGenerateGdbJobChanged;
            // start the job and report the job ID
            generateGdbJob.Start();
            Console.WriteLine("Submitted job #" + generateGdbJob.ServerJobId + " to create local geodatabase");

           

        }

        // handler for the JobChanged event
        private void OnGenerateGdbJobChanged(object sender, EventArgs e)
        {
            // get the GenerateGeodatabaseJob that raised the event
            var job = sender as GenerateGeodatabaseJob;


            // report error (if any)I bel
            if (job.Error != null)
            {
                Console.WriteLine("Error creating geodatabase: " + job.Error.Message);
                return;
            }


            // check the job status
            if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
            {
                // if the job succeeded, add local data to the map
                //AddLocalLayerToMap();
                Console.WriteLine("Download is complete!");
            }
            else if (job.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
            {
                // report failure
                Console.WriteLine("Unable to create local geodatabase.");
            }
            else
            {
                // job is still running, report last message
                Console.WriteLine(job.Messages[job.Messages.Count - 1].Message);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Geodatabase gdb = await Geodatabase.OpenAsync(@"C:\Temp\100_0_0Demo\WildlifeLocal.geodatabase");

            var table = gdb.GeodatabaseFeatureTables.First();
            await table.LoadAsync();

            var lyr = new FeatureLayer();
            lyr.Name = table.TableName;
            lyr.Id = table.TableName;
            lyr.FeatureTable = table;

            //{
            //    Id = table.TableName,
            //    Name = table.TableName,
            //    //DisplayName = table.Name,
            //    FeatureTable = table
            //};

            

            mv.Map.OperationalLayers.Add(lyr);
        }

        // Map initialization logic is contained in MapViewModel.cs
    }
}
