using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class EntryInstructionDetailsLayoutUserControlTest : TestCaseWithFactory
{
	public void TestTradeTypeDropEdit()
		=> control.AssertContainsControl<ZDropEdit>("TradeTypeDropEdit", x => x.WithBindTo("CEI_TradeType"));

	public void TestDeclarationPurposeDropEdit()
		=> control.AssertContainsControl<ZDropEdit>("DeclarationPurposeDropEdit", x => x.WithBindTo("CEI_DeclarationPurpose"));

	public void TestDeclarationPurposeDetailsTextBox()
		=> control.AssertContainsControl<ZTextBox>("DeclarationPurposeDetailsTextBox", x => x.WithBindTo("CEI_DeclarationPurposeDetails"));

	public void TestToWarehouseLabel()
		=> control.AssertContainsControl<ZLabel>("ToWarehouseLabel", x => x.WithCaption("To Warehouse"));

	public void TestToWarehouseUserControl()
		=> control.AssertContainsControl<ToWarehouseUserControl>("ToWarehouseUserControl", x => x.WithBindTo("."));

	public void TestFromWarehouseLabel()
		=> control.AssertContainsControl<ZLabel>("FromWarehouseLabel", x => x.WithCaption("From Warehouse"));

	public void TestFromWarehouseUserControl()
		=> control.AssertContainsControl<FromWarehouseUserControl>("FromWarehouseUserControl", x => x.WithBindTo("."));

	protected override void SetUp()
	{
		base.SetUp();
		control = new EntryInstructionDetailsLayoutUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}

	EntryInstructionDetailsLayoutUserControl control;
}
