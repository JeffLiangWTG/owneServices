using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class GoodsLocationLBUserControlTest : TestCase
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
			AssertEquals("Before the SubLocationOfGoodsAsCountryFindBox", 0, locationQualifierDropEdit.TabIndex);
		});
	}

	public void TestSubLocationOfGoodsAsCountryFindBox()
	{
		var subLocationOfGoodsAsCountryFindBox = control.SubLocationOfGoodsAsCountryFindBox;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 0, true), subLocationOfGoodsAsCountryFindBox.Location);
			AssertEquals("PreBoundMaxLength", 2, subLocationOfGoodsAsCountryFindBox.PreBoundMaxLength);
			AssertEquals("MaxLength", 2, subLocationOfGoodsAsCountryFindBox.MaxLength);
			AssertEquals("After the GoodsLocationDropEdit", 1, subLocationOfGoodsAsCountryFindBox.TabIndex);
		});
	}

	public void TestGoodsLocationDropEdit()
	{
		var goodsLocationDropEdit = control.GoodsLocationDropEdit;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 0, true), goodsLocationDropEdit.Location);
			AssertEquals("Before the LocationOtherInformationTextBox", 2, goodsLocationDropEdit.TabIndex);
		});
	}

	public void TestLocationOtherInformationTextBox()
	{
		var locationOtherInformationTextBox = control.LocationOtherInformationTextBox;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 0, true), locationOtherInformationTextBox.Location);
			AssertEquals("MaxLength", 3, locationOtherInformationTextBox.MaxLength);
			AssertEquals("After the SubLocationOfGoodsAsCountryFindBox", 3, locationOtherInformationTextBox.TabIndex);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new GoodsLocationLBLCUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	GoodsLocationLBLCUserControl control;
}
