using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class ToWarehouseUserControlTest : TestCaseWithFactory
{
	public void TestToWarehouseCodeTextBox()
		=> control.AssertContainsControl<ZTextBox>("ToWarehouseCodeTextBox", x => x.WithBindTo("ToWarehouseCode"));

	public void TestToWarehouseAddressControl()
		=> control.AssertContainsControl<ZAddressControl>("ToWarehouseAddressControl", x => x.WithBindTo("CEI_OA_Warehouse2"));

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
