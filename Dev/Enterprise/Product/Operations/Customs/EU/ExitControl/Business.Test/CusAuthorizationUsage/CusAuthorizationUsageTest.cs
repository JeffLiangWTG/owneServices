using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
{
	public void TestAGC_Code_Caption() => AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(authorizationUsage.AGC_CodeInfo).Caption);

	public void TestLookups() => AssertType<CusAuthorizationUsageLookups>(authorizationUsage.Lookups);

	protected override void SetUp()
	{
		base.SetUp();

		var cusExitHeader = Factory.New<CusExitHeader>();
		var cusExitReport = cusExitHeader.CusExitReports.AddNew();
		authorizationUsage = cusExitReport.CusAuthorizationUsages.AddNew();
	}

	CusAuthorizationUsage authorizationUsage;
}
