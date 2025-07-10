using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ConsignmentItemsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem>), userControl.BindingSource.DataSourceType);
		}

		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var splitContainer = userControl.ConsignmentItemsSplitContainer;
				AssertEquals("ConsignmentItemsSplitContainer.Dock", DockStyle.Fill, splitContainer.Dock);
				AssertEquals("ConsignmentItemsSplitContainer.Orientation", Orientation.Vertical, splitContainer.Orientation);
			});
		}

		public void TestConsignmentItemsGridUserControl()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(cusExitHeader, "");

			var grid = userControl.ConsignmentItemsSplitContainer.Panel1.FindSingle<ConsignmentItemsGridUserControl>();
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, grid.Dock);
				AssertEquals("BindingMember", ".", grid.GetBindingMember());
			});
		}

		public void TestConsignmentItemPackingAndContainerDynamicLayoutPanel()
		{
			var cusExitHeader = Factory.New<CusExitHeader>();
			userControl.SetDataBinding(cusExitHeader, "");

			var dynamicLayoutPanel = userControl.ConsignmentItemPackingAndContainerDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, dynamicLayoutPanel.Dock);
				AssertEquals("AutoScroll", true, dynamicLayoutPanel.AutoScroll);
				AssertEquals("Is within ConsignmentItemsSplitContainer.Panel2", true, userControl.ConsignmentItemsSplitContainer.Panel2.Contains(dynamicLayoutPanel));
				DynamicLayoutPanelTest.AssertControlsOrder(dynamicLayoutPanel, nameof(ConsignmentItemControlBag.ConsignmentItemPackingDetailsUserControl));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentItemsTabUserControl();
		}
		ConsignmentItemsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
