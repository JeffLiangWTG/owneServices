using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ReportAuthorizationsTabPageTest : TestCaseWithFactory
{
	public void TestCaption() => AssertEquals("Authorizations", reportAuthorizationsTabPage.Caption.Caption);

	public void TestUserControlType()
	{
		using var userControl = reportAuthorizationsTabPage.CreateUserControl();
		AssertType<ReportAuthorizationsTabUserControl>(userControl);
	}

	public void TestUserControlBindingMember() => AssertEquals(nameof(CusExitReport.CusAuthorizationUsages), reportAuthorizationsTabPage.UserControlBindingMember);

	public void TestIsVisible() => CombineAssertions(() =>
	{
		Assert("TabPage should NOT be visible when exit report is undefined.", !reportAuthorizationsTabPage.IsVisible(null));

		var exitReport = Factory.New<CusExitReport>();
		Assert("TabPage should be visible be when exit report is defined.", reportAuthorizationsTabPage.IsVisible(exitReport));
	});

	protected override void SetUp()
	{
		base.SetUp();
		reportAuthorizationsTabPage = new ReportAuthorizationsTabPage();
	}

	ReportAuthorizationsTabPage reportAuthorizationsTabPage;
}
