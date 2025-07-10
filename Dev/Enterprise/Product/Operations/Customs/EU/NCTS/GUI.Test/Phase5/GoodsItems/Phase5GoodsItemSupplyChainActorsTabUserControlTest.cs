using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using CusSupplyChainActorReference = Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5GoodsItemSupplyChainActorsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>), userControl.BindingSource.DataSourceType);
		}

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var supplyChainActorSplitContainer = userControl.SupplyChainActorsSplitContainer;
				AssertEquals("SupplyChainActorsSplitContainer.Orientation", Orientation.Horizontal, supplyChainActorSplitContainer.Orientation);
				AssertEquals("SupplyChainActorDynamicLayoutPanel", true, supplyChainActorSplitContainer.Panel2.Contains(userControl.SupplyChainActorDynamicLayoutPanel));
			});
		}

		public void TestSplitterPanelMinSize() => CombineAssertions(() =>
		{
			AssertEquals("Grid min size", 75, userControl.SupplyChainActorsSplitContainer.Panel1MinSize);
			AssertEquals("Details min size", 75, userControl.SupplyChainActorsSplitContainer.Panel2MinSize);
			AssertEquals("Default Details min size", 75, userControl.SupplyChainActorsSplitContainer.Panel2.Height);
		});

		public void TestGoodsItemSupplyChainActorsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var supplyChainActorSplitContainer = userControl.SupplyChainActorsSplitContainer;
			var supplyChainActorGridUserControl = supplyChainActorSplitContainer.Panel1.FindSingle<GoodsItemSupplyChainActorsGridUserControl>("GoodsItemSupplyChainActorsGridUserControl");

			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, supplyChainActorGridUserControl.Dock);
				AssertEquals("BindingMember", ".", supplyChainActorGridUserControl.GetBindingMember());
			});
		}

		public void TestSupplyChainActorDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var supplyChainActorDynamicLayoutPanel = userControl.SupplyChainActorDynamicLayoutPanel;
			AssertEquals("Dock", DockStyle.Fill, supplyChainActorDynamicLayoutPanel.Dock);
			AssertEquals("AutoScroll", true, supplyChainActorDynamicLayoutPanel.AutoScroll);
			DynamicLayoutPanelTest.AssertControlsOrder(supplyChainActorDynamicLayoutPanel,
				nameof(SupplyChainActorControlBag.RoleDropEdit),
				nameof(SupplyChainActorControlBag.ReferenceTextBox),
				nameof(SupplyChainActorControlBag.OwnerOrganisationFindBox));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemSupplyChainActorsTabUserControl();
		}
		Phase5GoodsItemSupplyChainActorsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
