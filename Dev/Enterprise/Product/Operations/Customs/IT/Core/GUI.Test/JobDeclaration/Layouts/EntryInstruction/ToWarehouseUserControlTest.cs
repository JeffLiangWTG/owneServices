using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ToWarehouseUserControlTest : TestCaseWithFactory
{
	public void TestToWarehouseIDTextBox()
	{
		var toWarehouseIDTextBox = control.ToWarehouseIDTextBox;
		CombineAssertions("ToWarehouseIDTextBox", () =>
		{
			AssertType<ZTextBox>(toWarehouseIDTextBox);
			AssertEquals("ReadOnly", true, toWarehouseIDTextBox.ReadOnly);
			AssertEquals("ReadOnly", "ZG_ToWarehouseID", toWarehouseIDTextBox.BindTo);
			AssertEquals("Caption", "ID", toWarehouseIDTextBox.CaptionResourceString.Caption);
			AssertEquals("Location", ControlDpiScalingHelper.NewScaledPoint(360, 0, true), toWarehouseIDTextBox.Location);
			AssertEquals("Size", ControlDpiScalingHelper.NewScaledSize(240, 20, true), toWarehouseIDTextBox.Size);
		});
	}

	public void TestToWarehouseTypeTextBox()
	{
		var toWarehouseTypeTextBox = control.ToWarehouseTypeTextBox;
		CombineAssertions("ToWarehouseTypeTextBox", () =>
		{
			AssertType<ZTextBox>(toWarehouseTypeTextBox);
			AssertEquals("ReadOnly", true, toWarehouseTypeTextBox.ReadOnly);
			AssertEquals("BindTo", "ZG_ToWarehouseType", toWarehouseTypeTextBox.BindTo);
			AssertEquals("Caption", "Type", toWarehouseTypeTextBox.CaptionResourceString.Caption);
			AssertEquals("Location", ControlDpiScalingHelper.NewScaledPoint(325, 0, true), toWarehouseTypeTextBox.Location);
			AssertEquals("Size", ControlDpiScalingHelper.NewScaledSize(15, 20, true), toWarehouseTypeTextBox.Size);
		});
	}

	public void TestToWarehouseAddressControl()
	{
		AssertType<ZAddressControl>(control.ToWarehouseAddressControl);
	}

	public void TestSize()
	{
		AssertEquals("Size", ControlDpiScalingHelper.NewScaledSize(601, 23, true), control.Size);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ToWarehouseUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	ToWarehouseUserControl control;
}
