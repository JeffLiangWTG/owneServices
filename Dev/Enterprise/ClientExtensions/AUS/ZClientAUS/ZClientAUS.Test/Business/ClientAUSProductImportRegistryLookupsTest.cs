using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Business.Testing
{
	internal class ClientAUSProductImportRegistryLookupsTest : BusinessObjectLookupsTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestImporterList()
		{
			ClientAUSProductImportRegistry testRegistry = Factory.New<ClientAUSProductImportRegistry>();
			OrgHeaderCollection importerList = testRegistry.ImporterList;
			AssertEquals("Oganisation Collection", typeof(OrgHeaderCollection), importerList.GetType());
			importerList.Load(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			Assert("Oganisation List should have been loaded with data", importerList.Count > 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestSupplierList()
		{
			ClientAUSProductImportRegistry testRegistry = Factory.New<ClientAUSProductImportRegistry>();
			OrgHeaderCollection supplierList = testRegistry.SupplierList;
			AssertEquals("Oganisation Collection", typeof(OrgHeaderCollection), supplierList.GetType());
			supplierList.Load(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			Assert("Oganisation List should have been loaded with data", supplierList.Count > 0);
		}
	}
}
