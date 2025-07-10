using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

sealed class CusExitConsignmentItemUcc6LookupsTest : BusinessObjectValidationTestCase
{
	public void TestStatusList() => CombineAssertions(() =>
	{
		var lookups = new CusExitConsignmentItemUcc6Lookups(Factory.New<CusExitConsignmentItem>());
		var statusList = lookups.StatusList;
		AssertType<DiscrepanciesStatusCodeList>("List Type", statusList);
		AssertSame("Cached", statusList, lookups.StatusList);
		AssertEquals("count is correct", 3, statusList.Count);
		AssertEquals("list contains empty", true, statusList.ContainsCode(""));
		AssertEquals("list contains DIF", true, statusList.ContainsCode("DIF"));
		AssertEquals("list contains MIS", true, statusList.ContainsCode("MIS"));
	});

	public void TestUCRStatusList() => CombineAssertions(() =>
	{
		var lookups = new CusExitConsignmentItemUcc6Lookups(Factory.New<CusExitConsignmentItem>());
		var statusList = lookups.UCRStatusList;
		AssertType<DiscrepanciesStatusCodeList>("List Type", statusList);
		AssertSame("Cached", statusList, lookups.UCRStatusList);
		AssertEquals("count is correct", 2, statusList.Count);
		AssertEquals("list contains empty", true, statusList.ContainsCode(""));
		AssertEquals("list contains DIF", true, statusList.ContainsCode("DIF"));
		AssertEquals("list does not contain MIS", false, statusList.ContainsCode("MIS"));
	});
}
