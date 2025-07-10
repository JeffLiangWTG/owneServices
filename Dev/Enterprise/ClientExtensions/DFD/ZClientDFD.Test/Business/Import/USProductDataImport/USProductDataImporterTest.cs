using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.DFD.Business.Import.Testing
{
	class USProductDataImporterTest : TestCaseWithFactory
	{
		public void TestImportDataToFactoryCore()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFile = resourceRetriever.SaveResourceToFile(TestResourceName);
				var buffer = new NotificationBuffer();
				int productsCount = Factory.GetDatabaseCount(typeof(MasterFiles.Business.OrgSupplierPart));
				Importer.ImportData(testFile, buffer, SourceInfo.EmptySourceInfo);
				AssertEquals(productsCount, Factory.GetDatabaseCount(typeof(MasterFiles.Business.OrgSupplierPart)));
				Assert(buffer.AsString.Contains("0 products  created."));
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.LegacySystemCode, "AMTR22", RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates));
				var classification = Factory.New<CusClassification>();
				classification.CC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
				classification.CC_LookupCode = "8536490080";
				classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				Factory.Save();
				Importer.ImportData(testFile, buffer, SourceInfo.EmptySourceInfo);
				AssertEquals(productsCount + 2, Factory.GetDatabaseCount(typeof(MasterFiles.Business.OrgSupplierPart)));
				Assert(buffer.AsString.Contains("2 products  created."));
			}
		}

		#region Implementation
		const string TestResourceName = "DSV CHB Product Data Import.csv";
		USProductDataImporter Importer
		{
			get
			{
				return importer ?? (importer = new USProductDataImporter());
			}
		}

		USProductDataImporter importer;
		#endregion
	}
}
