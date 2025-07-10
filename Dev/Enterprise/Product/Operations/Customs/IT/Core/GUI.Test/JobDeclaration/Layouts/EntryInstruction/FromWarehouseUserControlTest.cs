using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class FromWarehouseUserControlTest : TestCaseWithFactory
{
	public void TestFromWarehouseIDTextBox()
	{
		var fromWarehouseIDTextBox = control.FromWarehouseIDTextBox;
		CombineAssertions("FromWarehouseIDTextBox", () =>
		{
			AssertType<ZTextBox>(fromWarehouseIDTextBox);
			AssertEquals("ReadOnly", true, fromWarehouseIDTextBox.ReadOnly);
			AssertEquals("ReadOnly", "ZG_FromWarehouseID", fromWarehouseIDTextBox.BindTo);
			AssertEquals("Caption", "ID", fromWarehouseIDTextBox.CaptionResourceString.Caption);
			AssertEquals("Location", ControlDpiScalingHelper.NewScaledPoint(360, 0, true), fromWarehouseIDTextBox.Location);
			AssertEquals("Size", ControlDpiScalingHelper.NewScaledSize(240, 20, true), fromWarehouseIDTextBox.Size);
		});
	}

	public void TestFromWarehouseTypeTextBox()
	{
		var fromWarehouseTypeTextBox = control.FromWarehouseTypeTextBox;
		CombineAssertions("FromWarehouseTypeTextBox", () =>
		{
			AssertType<ZTextBox>(fromWarehouseTypeTextBox);
			AssertEquals("ReadOnly", true, fromWarehouseTypeTextBox.ReadOnly);
			AssertEquals("BindTo", "ZG_FromWarehouseType", fromWarehouseTypeTextBox.BindTo);
			AssertEquals("Caption", "Type", fromWarehouseTypeTextBox.CaptionResourceString.Caption);
			AssertEquals("Location", ControlDpiScalingHelper.NewScaledPoint(325, 0, true), fromWarehouseTypeTextBox.Location);
			AssertEquals("Size", ControlDpiScalingHelper.NewScaledSize(15, 20, true), fromWarehouseTypeTextBox.Size);
		});
	}

	public void TestFromWarehouseAddressControl()
	{
		AssertType<ZAddressControl>(control.FromWarehouseAddressControl);
	}

	public void TestSize()
	{
		AssertEquals("Size", ControlDpiScalingHelper.NewScaledSize(601, 23, true), control.Size);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new FromWarehouseUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	FromWarehouseUserControl control;
}
