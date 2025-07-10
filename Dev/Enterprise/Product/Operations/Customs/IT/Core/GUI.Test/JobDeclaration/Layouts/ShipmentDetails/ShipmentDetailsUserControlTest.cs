using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ShipmentDetailsUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
	}

	public void TestShipmentDetailsOriginUserControl()
	{
		AssertType<ShipmentDetailsOriginUserControl>(control.ShipmentDetailsOriginUserControl);
	}

	public void TestShipmentDetailsFinalDestinationUserControl()
	{
		AssertType<ShipmentDetailsFinalDestinationUserControl>(control.ShipmentDetailsFinalDestinationUserControl);
	}

	public void TestLocationQualifierDropEdit()
	{
		AssertType<ZDropEdit>(control.LocationQualifierDropEdit);
	}

	public void TestGoodsLocationDUserControl()
	{
		AssertType<GoodsLocationDUserControl>(control.GoodsLocationDUserControl);
	}

	public void TestGoodsLocationFUserControl()
	{
		AssertType<GoodsLocationFUserControl>(control.GoodsLocationFUserControl);
	}

	public void TestGoodsLocationFCUserControl()
	{
		AssertType<GoodsLocationFCUserControl>(control.GoodsLocationFCUserControl);
	}

	public void TestGoodsLocationLBLCUserControl()
	{
		AssertType<GoodsLocationLBLCUserControl>(control.GoodsLocationLBLCUserControl);
	}

	public void TestSubLocationTextBox()
	{
		AssertType<ZTextBox>(control.SubLocationTextBox);
	}

	public void TestLocationOfGoodsUserControl()
	{
		AssertType<LocationOfGoodsUserControl>(control.LocationOfGoodsUserControl);
		AssertEquals("Width", 322, control.LocationOfGoodsUserControl.Width);
	}

	public void TestZG_DeliveryTermsTextBox()
	{
		AssertType<ZTextBox>(control.AdditionalDeliveryTermsTextBox);
		AssertEquals("AdditionalDeliverTermsTextBox BindTo", "ZG_AdditionalDeliveryTerms", control.AdditionalDeliveryTermsTextBox.BindTo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ShipmentDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ShipmentDetailsUserControl control;
}
