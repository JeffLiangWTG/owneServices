using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class GoodsLocationDUserControlTest : TestCase
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
			AssertEquals("Before the LocationOfGoodsAsCustomsOfficeFindBox", 0, locationQualifierDropEdit.TabIndex);
		});
	}

	public void TestLocationOfGoodsAsCustomsOfficeFindBox()
	{
		var locationOfGoodsAsCustomsOfficeFindBox = control.LocationOfGoodsAsCustomsOfficeFindBox;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 0, true), locationOfGoodsAsCustomsOfficeFindBox.Location);
			AssertEquals("MaxLength", 8, locationOfGoodsAsCustomsOfficeFindBox.MaxLength);
			AssertEquals("After the LocationQualifierDropEdit", 1, locationOfGoodsAsCustomsOfficeFindBox.TabIndex);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new GoodsLocationDUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	GoodsLocationDUserControl control;
}
