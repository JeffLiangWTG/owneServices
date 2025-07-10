using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5GoodsItemPreviousDocumentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var previousDocumentsSplitContainer = userControl.PreviousDocumentsSplitContainer;
				AssertEquals("PreviousDocumentsSplitContainer.Orientation", Orientation.Horizontal, previousDocumentsSplitContainer.Orientation);
				AssertEquals("PreviousDocumentsSplitContainer.SplitterDistance", 260, previousDocumentsSplitContainer.SplitterDistance);
				AssertEquals("PreviousDocumentDynamicLayoutPanel", true, previousDocumentsSplitContainer.Panel2.Contains(userControl.PreviousDocumentDynamicLayoutPanel));
			});
		}

		public void TestGoodsItemPreviousDocumentsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var previousDocumentsSplitContainer = userControl.PreviousDocumentsSplitContainer;
			var previousDocumentsGridUserControl = previousDocumentsSplitContainer.Panel1.FindSingle<GoodsItemPreviousDocumentsGridUserControl>("GoodsItemPreviousDocumentsGridUserControl");

			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, previousDocumentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", previousDocumentsGridUserControl.GetBindingMember());
			});
		}

		public void TestPreviousDocumentDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			CombineAssertions(() =>
			{
				var previousDocumentDynamicLayoutPanel = userControl.PreviousDocumentDynamicLayoutPanel;
				AssertEquals("Dock", DockStyle.Fill, previousDocumentDynamicLayoutPanel.Dock);
				AssertEquals("AutoScroll", true, previousDocumentDynamicLayoutPanel.AutoScroll);
				DynamicLayoutPanelTest.AssertControlsOrder(previousDocumentDynamicLayoutPanel,
					nameof(PreviousDocumentControlBag.TypeCodeFindBox),
					nameof(PreviousDocumentControlBag.ReferenceNumberTextBox),
					nameof(PreviousDocumentControlBag.ItemNumberCalcEdit),
					nameof(PreviousDocumentControlBag.NumOfPackagesDropEdit),
					nameof(PreviousDocumentControlBag.QuantityDropEdit),
					nameof(PreviousDocumentControlBag.ComplementTextBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemPreviousDocumentsTabUserControl();
		}
		Phase5GoodsItemPreviousDocumentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
