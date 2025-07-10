using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5GoodsItemSupportingDocumentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var supportingDocumentsSplitContainer = userControl.SupportingDocumentsSplitContainer;
				AssertEquals("SupportingDocumentsSplitContainer.Orientation", Orientation.Horizontal, supportingDocumentsSplitContainer.Orientation);
				AssertEquals("SupportingDocumentsSplitContainer.SplitterDistance", 260, supportingDocumentsSplitContainer.SplitterDistance);
				AssertEquals("SupportingDocumentDynamicLayoutPanel", true, supportingDocumentsSplitContainer.Panel2.Contains(userControl.SupportingDocumentDynamicLayoutPanel));
			});
		}

		public void TestGoodsItemSupportingDocumentsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var supportingDocumentsSplitContainer = userControl.SupportingDocumentsSplitContainer;
			var supportingDocumentsGridUserControl = supportingDocumentsSplitContainer.Panel1.FindSingle<GoodsItemSupportingDocumentsGridUserControl>("GoodsItemSupportingDocumentsGridUserControl");

			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, supportingDocumentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", supportingDocumentsGridUserControl.GetBindingMember());
			});
		}

		public void TestSupportingDocumentLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var supportingDocumentDynamicLayoutPanel = userControl.SupportingDocumentDynamicLayoutPanel;
			AssertEquals("Dock", DockStyle.Fill, supportingDocumentDynamicLayoutPanel.Dock);
			AssertEquals("AutoScroll", true, supportingDocumentDynamicLayoutPanel.AutoScroll);
			DynamicLayoutPanelTest.AssertControlsOrder(supportingDocumentDynamicLayoutPanel,
				nameof(SupportingDocumentControlBag.TypeCodeFindBox),
				nameof(SupportingDocumentControlBag.ReferenceNumberTextBox),
				nameof(SupportingDocumentControlBag.ItemNumberCalcEdit),
				nameof(SupportingDocumentControlBag.ComplementTextBox));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemSupportingDocumentsTabUserControl();
		}
		Phase5GoodsItemSupportingDocumentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
