using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ShipmentDetailsFinalDestinationUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
	}

	public void TestFinalDestinationFindBox()
	{
		var destinationFindBox = control.FinalDestinationFindBox;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), destinationFindBox.Location);
			AssertEquals("Before the GoodsDestinationDropEdit", 0, destinationFindBox.TabIndex);
		});
	}

	public void TestGoodsDestinationDropEdit()
	{
		var goodsDestinationDropEdit = control.GoodsDestinationDropEdit;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 0, true), goodsDestinationDropEdit.Location);
			AssertEquals("Before the Date of arrival", 1, goodsDestinationDropEdit.TabIndex);
		});
	}

	public void TestEstimatedArrivalDateEdit()
	{
		var estimatedArrivalDateEdit = control.EstimatedArrivalDateEdit;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true), estimatedArrivalDateEdit.Location);
			AssertEquals("After the GoodsOriginDropEdit", 2, estimatedArrivalDateEdit.TabIndex);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ShipmentDetailsFinalDestinationUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	ShipmentDetailsFinalDestinationUserControl control;
}
