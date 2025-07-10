using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class CusExitConsignmentItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList() => AssertEquals(0, Factory.New<CusExitConsignmentItem>().Lookups.StatusList.Count);

		public void TestUCRStatusList() => AssertEquals(0, Factory.New<CusExitConsignmentItem>().Lookups.UCRStatusList.Count);
	}
}
