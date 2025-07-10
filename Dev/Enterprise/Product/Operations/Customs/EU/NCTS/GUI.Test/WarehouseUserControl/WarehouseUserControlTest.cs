using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class WarehouseUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), userControl.BindingSource.DataSourceType);
		}

		public void TestPartGuidFindBox()
		{
			var partGuidFindBox = userControl.PartGuidFindBox;

			AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.BY_OP_Part), partGuidFindBox.BindTo);
		}

		public void TestBondedWhsQuantityCalcDropEdit()
		{
			var bondedWhsQuantityCalcDropEdit = userControl.BondedWhsQuantityCalcDropEdit;

			AssertEquals("BindToBindToAmount", nameof(NctsDepartureCargoDesc.BY_BondedWhsQuantity), bondedWhsQuantityCalcDropEdit.BindToAmount);
			AssertEquals("BindToUnit", nameof(NctsDepartureCargoDesc.BY_BondedWhsUnitQty), bondedWhsQuantityCalcDropEdit.BindToUnit);
		}

		public void TestWarehouseEntryNumberTextBox()
		{
			var warehouseEntryNumberTextBox = userControl.WarehouseEntryNumberTextBox;

			AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.BY_WarehouseEntryNumber), warehouseEntryNumberTextBox.BindTo);
		}

		public void TestWarehouseEntryLineNoCalcEdit()
		{
			var warehouseEntryLineNoCalcEdit = userControl.WarehouseEntryLineNoCalcEdit;

			AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.BY_WarehouseEntryLineNo), warehouseEntryLineNoCalcEdit.BindTo);
		}

		public void TestBondedWHSOrderNumberTextBox()
		{
			var whsOrderNumberTextBox = userControl.BondedWHSOrderNumberTextBox;

			AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.BY_BondedWHSOrderNumber), whsOrderNumberTextBox.BindTo);
		}

		public void TestBondedWHSOrderLineNumberCalcEdit()
		{
			var whsOrderLineNumberCalcEdit = userControl.BondedWHSOrderLineNumberCalcEdit;

			AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.BY_BondedWHSOrderLineNumber), whsOrderLineNumberCalcEdit.BindTo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new WarehouseUserControl();
		}
		WarehouseUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
