using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.Module.Test.CustomsSummary;
sealed class CustomsSummaryFilterStripLookupsTest : TestCaseWithFactory
{
	public void TestBordereauReceivedStatusList()
	{
		var list = lookups.BordereauReceivedStatusList;
		var expectedCodes = new BordereauReceivedStatusList().GetAllCodesZString();
		AssertContainsExactElementsInAnyOrder(expectedCodes, list.GetAllCodesZString());
	}

	public void TestBordereauChargeTypeList()
	{
		var list = lookups.BordereauChargeTypeList;
		var expectedCodes = new BordereauChargeTypeList().GetAllCodesZString();
		AssertContainsExactElementsInAnyOrder(expectedCodes, list.GetAllCodesZString());
	}

	protected override void SetUp()
	{
		base.SetUp();
		lookups = new CustomsSummaryFilterStripLookups(new CustomsSummaryFilterStripBusinessObject());
	}

	CustomsSummaryFilterStripLookups lookups;
}
