using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SupplementaryDeclarantLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSupplementaryDeclarantFilingTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var supplementaryDeclarant = bill.SupplementaryDeclarants.AddNew();
			var list = supplementaryDeclarant.Lookups.SupplementaryDeclarantFilingTypesList;

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, supplementaryDeclarant.Lookups.SupplementaryDeclarantFilingTypesList);
			AssertEquals(true, list.ContainsCode("1"));
			AssertEquals(true, list.ContainsCode("2"));
		}
	}
}
