using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.Wow.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export.Testing
{
	[TestedType(typeof(DeclarationInvoiceFlatFileDataExporter))]
	public class DeclarationInvoiceFlatFileDataExporterTest : FlatFileDataExporterTestCase
	{
		public void TestFileExtensionType()
		{
			FlatFileDataExporter exporter = GetDataExporter();
			AssertEquals("File Extension Type", FileExtensionType.Txt, exporter.FileExtensionType);
		}

		public void TestExportData()
		{
			JobDeclarationCollection collection = (JobDeclarationCollection)GetPopulatedCollectionToSaveAndExport();
			CollectionWrapperBusinessObjectReader collectionReader = new CollectionWrapperBusinessObjectReader(collection);
			MockDeclarationInvoiceFlatFileDataExporter exporter = new MockDeclarationInvoiceFlatFileDataExporter(Factory);
			exporter.ExportData(collectionReader, new NotificationBuffer());
			AssertEquals("Exported File exists", true, File.Exists(exporter.ExportedFileForTesting));
			try
			{
				File.Delete(exporter.ExportedFileForTesting);
			}
			catch (IOException)
			{
			}
		}

		public void TestEnglishDescription()
		{
			DeclarationInvoiceFlatFileDataExporter exporter = new DeclarationInvoiceFlatFileDataExporter(Factory);
			AssertEquals("English Description", "WoolWorths Declaration Invoice Automated Data Export", exporter.EnglishDescription);
		}

		#region Implementation
		public override FlatFileDataExporter GetDataExporter()
		{
			return new DeclarationInvoiceFlatFileDataExporter(Factory);
		}

		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			JobDeclarationCollection result = new JobDeclarationCollection(Factory);
			JobDec = WowSTTestHelper.GetPopulatedDeclaration(Factory);
			result.Add(JobDec);
			return result;
		}

		JobDeclaration? JobDec;
		class MockDeclarationInvoiceFlatFileDataExporter : DeclarationInvoiceFlatFileDataExporter
		{
			public MockDeclarationInvoiceFlatFileDataExporter(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new void ExportData(BusinessObjectReader objectReader, INotifications notificationSubscriber)
			{
				base.ExportData(objectReader, notificationSubscriber);
			}
		}
		#endregion
	}
}
