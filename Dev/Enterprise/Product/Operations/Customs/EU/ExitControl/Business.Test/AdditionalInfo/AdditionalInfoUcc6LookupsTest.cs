using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

sealed class AdditionalInfoUcc6LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestStatusList()
	{
		CombineAssertions(() =>
		{
			var lookups = new AdditionalInfoUcc6Lookups(Factory.New<AdditionalInfo>());
			var statusList = lookups.StatusList;
			AssertType<DiscrepanciesStatusCodeList>("List Type", statusList);
			AssertSame("Cached", statusList, lookups.StatusList);
			AssertEquals("count is correct", 3, statusList.Count);
			AssertEquals("list contains empty", true, statusList.ContainsCode(""));
			AssertEquals("list contains DIF", true, statusList.ContainsCode("DIF"));
			AssertEquals("list contains MIS", true, statusList.ContainsCode("MIS"));
		});
	}
}
