using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.Business.Testing
{
	public abstract class FlatFileDataExporterTestCase : NonPersistentBusinessObjectTestCase
	{
		public abstract FlatFileDataExporter GetDataExporter();
		public abstract IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport();

		INotifications Notifications
		{
			get { return new NotificationBuffer(); }
		}

		public void TestFlatFileFormatNotNull()
		{
			FlatFileDataExporter exporter = GetDataExporter();
			AssertNotNull(exporter.GetType().ToString() + " returns null for FlatFileFormat. Please specify an appropriate FlatFileFormat (e.g. CsvFlatFileFormat)", exporter.FlatFileFormatForTest);
		}

		public void TestDataAdapterNotNull()
		{
			FlatFileDataExporter exporter = GetDataExporter();
			AssertNotNull(exporter.GetType().ToString() + " returns null for DataAdapter. Please specify an appropriate XmlDataAdapter for your export.", exporter.DataAdapterForTest);
		}

		public void TestGetCoverterNotNull()
		{
			FlatFileDataExporter exporter = GetDataExporter();
			AssertNotNull(exporter.GetType().ToString() + " returns null for CreateConverter(). Please Specify an appropriate FlatFileConverter for your export.", exporter.CreateConverterForTest(Notifications));
		}

		public void TestEnglishDescriptionNotEmpty()
		{
			FlatFileDataExporter exporter = GetDataExporter();
			Assert(exporter.GetType().ToString() + " has an empty EnglishDescription. Please return an appropriate string (e.g. 'Client Name')", !exporter.EnglishDescription.IsEmpty);
		}

		public void TestExport()
		{
			IBusinessObjectCollection collectionToExport = GetPopulatedCollectionToSaveAndExport();
			Assert("Precondition - must be at least one element returned from GetPopulatedCollectionToExport()", collectionToExport.Count > 0);
			Factory.Save();

			CollectionWrapperBusinessObjectReader collectionToExportReader = new CollectionWrapperBusinessObjectReader(collectionToExport);

			string exportedFile = string.Empty;

			try
			{
				FlatFileDataExporter exporter = GetDataExporter();
				exporter.Export(collectionToExportReader, Notifications);
				exportedFile = exporter.ExportedFileForTesting;
				FileInfo fileInfo = new FileInfo(exportedFile);
				Assert("File should have written something. Check that you return the FlatFileRows you are populating in FlatFileConverter.MapExport()", fileInfo.Length > 0);
			}
			finally
			{
				if (File.Exists(exportedFile))
				{
					File.Delete(exportedFile);
				}
			}
		}
	}
}
