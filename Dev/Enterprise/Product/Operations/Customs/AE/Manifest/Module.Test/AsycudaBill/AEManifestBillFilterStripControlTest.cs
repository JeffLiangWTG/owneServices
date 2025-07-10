using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Manifest.Business;

namespace Enterprise.Customs.AE.Manifest.Module.Testing;

sealed class AEManifestBillFilterStripControlTest : TestCaseWithFactory
{
	public void TestColumnsExist()
	{
		var collection = new AEManifestBillModuleCollection(Factory);
		var filter = new AEManifestBillFilterStrip();
		using var control = new AEManifestBillFilterStripControl(collection, filter);
		var grid = control.FilteredGrid;
		var splitBillNumber = grid.GetColumnStyle(AsycudaBill.Schema.ABL_SplitBillNumber);
		AssertNotNull("The new added column should exist", splitBillNumber);
		AssertEquals("caption:", splitBillNumber.CaptionResourceString.Caption, "Split Bill Number");
	}
}
