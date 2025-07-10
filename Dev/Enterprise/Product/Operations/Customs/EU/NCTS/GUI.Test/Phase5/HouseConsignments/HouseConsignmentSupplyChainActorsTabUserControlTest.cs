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
	class HouseConsignmentSupplyChainActorsTabUserControlTest : TestCaseWithFactory
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
				AssertEquals("SupplyChainActorsSplitContainer.Dock", DockStyle.Fill, supplyChainActorSplitContainer.Dock);
				AssertEquals("SupplyChainActorsSplitContainer.Orientation", Orientation.Horizontal, supplyChainActorSplitContainer.Orientation);
				AssertEquals("SupplyChainActorDynamicLayoutPanel within SupplyChainActorsSplitContainer.Panel2", true, supplyChainActorSplitContainer.Panel2.Contains(userControl.SupplyChainActorDynamicLayoutPanel));
			});
		}

		public void TestSplitterPanelMinSize() => CombineAssertions(() =>
		{
			AssertEquals("Grid min size", 75, userControl.SupplyChainActorsSplitContainer.Panel1MinSize);
			AssertEquals("Details min size", 75, userControl.SupplyChainActorsSplitContainer.Panel2MinSize);
			AssertEquals("Default Details min size", 75, userControl.SupplyChainActorsSplitContainer.Panel2.Height);
		});

		public void TestHouseConsignmentSupplyChainActorsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var grid = userControl.SupplyChainActorsSplitContainer.Panel1.FindSingle<HouseConsignmentSupplyChainActorsGridUserControl>();
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, grid.Dock);
				AssertEquals("BindingMember", ".", grid.GetBindingMember());
			});
		}

		public void TestSupplyChainActorDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var supplyChainActorDynamicLayoutPanel = userControl.SupplyChainActorDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, supplyChainActorDynamicLayoutPanel.Dock);
				AssertEquals("AutoScroll", true, supplyChainActorDynamicLayoutPanel.AutoScroll);

				DynamicLayoutPanelTest.AssertControlsOrder(supplyChainActorDynamicLayoutPanel,
					nameof(SupplyChainActorControlBag.RoleDropEdit),
					nameof(SupplyChainActorControlBag.ReferenceTextBox),
					nameof(SupplyChainActorControlBag.OwnerOrganisationFindBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentSupplyChainActorsTabUserControl();
		}
		HouseConsignmentSupplyChainActorsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
