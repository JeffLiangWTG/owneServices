namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

sealed class CusExitReportUcc6LookupsBaseOnlyTest : CusExitReportLookupsAbstractTest
{
	protected override void SetUp()
	{
		base.SetUp();
		report = Factory.GetUcc6ExitHeader().CusExitReports.AddNew();
		lookups = report.Lookups;
	}
}
