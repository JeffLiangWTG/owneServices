using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Module.Testing;

[TestedType(typeof(AEManifestBillFilterStrip))]
sealed class AEManifestBillFilterStripTest : FilterStripBusinessObjectTestCase
{
	public void TestSplitBillNumberFilter()
	{
		const string expectedBillNumber = "123456";
		const string unExpectedBillNumber = "654321";

		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		bill.ABL_SplitBillNumber = expectedBillNumber;
		Factory.Save();

		CombineAssertions(() =>
		{
			var filterObj = new AEManifestBillFilterStrip();
			var filter = (ModuleTextFilter)filterObj[AEManifestBillFilterStrip.AeFilterConstants.SplitBillNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = expectedBillNumber;
			var filterQuery = filterObj.Filter;
			Assert("matched bill", bill.MatchesFilter(filterQuery));

			filter.Property = unExpectedBillNumber;
			filterQuery = filterObj.Filter;
			Assert("unmatched bill", !bill.MatchesFilter(filterQuery));

			var billSecond = Factory.New<AsycudaBill>();
			billSecond.ABL_SplitBillNumber = expectedBillNumber;
			filter.Property = expectedBillNumber;
			filterQuery = filterObj.Filter;
			Assert("matched bill: for the second bill", bill.MatchesFilter(filterQuery));
		});
	}
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new AEManifestBillFilterStrip();
}
