using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ReportAuthorizationsTabUserControlTest : TestCaseWithFactory
{
	public void TestBindingSource()
	{
		using var userControl = new ReportAuthorizationsTabUserControl();
		AssertEquals(typeof(ICusAuthorizationUsageCollection<CusAuthorizationUsage, Business.CusExitReport>), userControl.BindingSource.DataSourceType);
	}
}
