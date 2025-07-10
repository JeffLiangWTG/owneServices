using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

sealed class CusExitReportUcc6ValidationDeciderTest : TestCase
{
	public void TestValidateCER_LocationLookups() => AssertEquals(true, decider.ValidateCER_LocationLookups);

	public void TestValidateCER_CXC_ConsignmentMrnIsAlreadyBeingUsed() => AssertEquals(true, decider.ValidateCER_CXC_ConsignmentMrnIsAlreadyBeingUsed);

	protected override void SetUp()
	{
		decider = new CusExitReportUcc6ValidationDecider();
	}
	CusExitReportUcc6ValidationDecider decider;
}
