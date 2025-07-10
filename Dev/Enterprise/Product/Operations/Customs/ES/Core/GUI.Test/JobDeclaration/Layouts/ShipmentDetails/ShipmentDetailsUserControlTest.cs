using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class ShipmentDetailsUserControlTest : TestCase
{
	public void TestCaptionRenderingEnabled()
	{
		AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
	}

	public void TestRegionOrTerritoryOfDestinationCodeFindBox()
	{
		AssertType<ZCodeFindBox>(control.RegionOrTerritoryOfDestinationCodeFindBox);
	}

	public void TestRegionOrTerritoryOfDestinationDropEdit()
	{
		AssertType<ZDropEdit>(control.RegionOrTerritoryOfDestinationDropEdit);
	}

	public void TestDestinationStateDropEdit()
	{
		AssertType<ZDropEdit>(control.DestinationStateDropEdit);
	}

	public void TestPartialWriteoffCheckBox()
	{
		AssertType<ZCheckBox>(control.PartialWriteoffCheckBox);
	}

	public void TestGoodsLocationCodeFindBox()
	{
		AssertType<ZCodeFindBox>(control.GoodsLocationCodeFindBox);
	}

	public void TestShipmentDetailsOriginUserControl()
	{
		AssertType<ShipmentDetailsOriginUserControl>(control.ShipmentDetailsOriginUserControl);
	}

	public void TestShipmentDetailsFinalDestinationUserControl()
	{
		AssertType<ShipmentDetailsFinalDestinationUserControl>(control.ShipmentDetailsFinalDestinationUserControl);
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
