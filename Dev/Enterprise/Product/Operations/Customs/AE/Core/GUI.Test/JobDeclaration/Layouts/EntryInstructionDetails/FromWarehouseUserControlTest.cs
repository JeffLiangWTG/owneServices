using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class FromWarehouseUserControlTest : TestCaseWithFactory
{
	public void TestFromWarehouseCodeTextBox()
		=> control.AssertContainsControl<ZTextBox>("FromWarehouseCodeTextBox", x => x.WithBindTo("FromWarehouseCode"));

	public void TestFromWarehouseAddressControl()
		=> control.AssertContainsControl<ZAddressControl>("FromWarehouseAddressControl", x => x.WithBindTo("CEI_OA_Warehouse"));

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
