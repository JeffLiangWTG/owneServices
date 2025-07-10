using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

sealed class CusExitSealLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestStatusList() => AssertEquals(0, Factory.New<CusExitSeal>().Lookups.StatusList.Count);
}
