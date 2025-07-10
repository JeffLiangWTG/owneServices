using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class GoodsLocationFUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
	}

	public void TestLocationQualifierDropEdit()
	{
		var locationQualifierDropEdit = control.LocationQualifierDropEdit;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), locationQualifierDropEdit.Location);
			AssertEquals("Before the GoodsLocationAddressZDocAddressControl", 0, locationQualifierDropEdit.TabIndex);
		});
	}

	public void TestGoodsLocationAddressZDocAddressControl()
	{
		var goodsLocationAddressZDocAddressControl = control.GoodsLocationAddressZDocAddressControl;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 0, true), goodsLocationAddressZDocAddressControl.Location);
			AssertEquals("After the LocationQualifierDropEdit", 1, goodsLocationAddressZDocAddressControl.TabIndex);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new GoodsLocationFUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	GoodsLocationFUserControl control;
}
