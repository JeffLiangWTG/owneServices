using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ShipmentDetailsOriginUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
	}

	public void TestOriginFindBox()
	{
		var originFindBox = control.OriginFindBox;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), originFindBox.Location);
			AssertEquals("Before the GoodsOriginDropEdit", 0, originFindBox.TabIndex);
		});
	}

	public void TestGoodsOriginDropEdit()
	{
		var goodsOriginDropEdit = control.GoodsOriginDropEdit;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 0, true), goodsOriginDropEdit.Location);
			AssertEquals("Before the Date of departure", 1, goodsOriginDropEdit.TabIndex);
		});
	}

	public void TestEstimatedDepartureDateEdit()
	{
		var estimatedDepartureDateEdit = control.EstimatedDepartureDateEdit;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true), estimatedDepartureDateEdit.Location);
			AssertEquals("After the GoodsOriginDropEdit", 2, estimatedDepartureDateEdit.TabIndex);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ShipmentDetailsOriginUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	ShipmentDetailsOriginUserControl control;
}
