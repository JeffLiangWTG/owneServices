using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemWarehouseTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), userControl.BindingSource.DataSourceType);
		}

		public void TestSupplyChainActorDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var warehouseDynamicLayoutPanel = userControl.WarehouseDynamicLayoutPanel;
			AssertEquals("Dock", DockStyle.Fill, warehouseDynamicLayoutPanel.Dock);
			AssertEquals("AutoScroll", true, warehouseDynamicLayoutPanel.AutoScroll);
			DynamicLayoutPanelTest.AssertControlsOrder(warehouseDynamicLayoutPanel,
				nameof(WarehouseControlBag.PartGuidFindBox),
				nameof(WarehouseControlBag.BondedWhsQuantityCalcDropEdit),
				nameof(WarehouseControlBag.WarehouseEntryNumberTextBox),
				nameof(WarehouseControlBag.WarehouseEntryLineNoCalcEdit),
				nameof(WarehouseControlBag.BondedWHSOrderNumberTextBox),
				nameof(WarehouseControlBag.BondedWHSOrderLineNumberCalcEdit));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemWarehouseTabUserControl();
		}
		Phase5GoodsItemWarehouseTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
