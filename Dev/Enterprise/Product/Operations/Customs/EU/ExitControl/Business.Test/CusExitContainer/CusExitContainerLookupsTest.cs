using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class CusExitContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatusList() => AssertEquals(0, Factory.New<CusExitContainer>().Lookups.StatusList.Count);
	}
}
