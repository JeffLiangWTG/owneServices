using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.DFD.Business.Import.Testing
{
	class USSupplierDataImporterTest : TestCaseWithFactory
	{
		public void TestImportDataToFactoryCore()
		{
			var buffer = new NotificationBuffer();
			int orgHeaderCount = Factory.GetDatabaseCount(typeof(OrgHeader));
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFile = resourceRetriever.SaveResourceToFile(TestResourceName);
				Importer.ImportData(testFile, buffer, SourceInfo.EmptySourceInfo);
			}
			AssertEquals(orgHeaderCount + 6, Factory.GetDatabaseCount(typeof(OrgHeader)));
			Assert(buffer.AsString.Contains("6 organisations created."));
		}

		#region Implementation
		const string TestResourceName = "DSVMIDs.csv";
		USSupplierDataImporter Importer
		{
			get
			{
				return importer ?? (importer = new USSupplierDataImporter());
			}
		}

		USSupplierDataImporter importer;
		#endregion
	}
}
