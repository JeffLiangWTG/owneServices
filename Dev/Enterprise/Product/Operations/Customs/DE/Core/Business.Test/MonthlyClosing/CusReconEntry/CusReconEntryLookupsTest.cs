using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusReconEntryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryTypeList()
		{
			var entry = Factory.New<CusReconEntry>();
			var lookups = entry.Lookups;
			var list = lookups.EntryTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "AAV, AZ, AZL, VAV, VZA, VZL", list.CodesAsString);
				AssertSame("Cached", list, lookups.EntryTypeList);
			});
		}
	}
}
