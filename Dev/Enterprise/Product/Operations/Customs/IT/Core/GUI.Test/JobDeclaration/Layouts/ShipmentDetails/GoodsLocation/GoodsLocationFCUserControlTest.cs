using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class GoodsLocationFCUserControlTest : TestCase
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
			AssertEquals("After the LocationQualifierDropEdit", 1, subLocationOfGoodsAsCountryFindBox.TabIndex);
		});
	}

	public void TestLocationOfGoodsTextBox()
	{
		var locationOfGoodsTextBox = control.LocationOfGoodsTextBox;
		CombineAssertions(() =>
		{
			AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 0, true), locationOfGoodsTextBox.Location);
			AssertEquals("MaxLength", 7, locationOfGoodsTextBox.MaxLength);
			AssertEquals("After the SubLocationOfGoodsAsCountryFindBox", 2, locationOfGoodsTextBox.TabIndex);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new GoodsLocationFCUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	GoodsLocationFCUserControl control;
}
