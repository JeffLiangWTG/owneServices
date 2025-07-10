using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemAdditionalDocumentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			CombineAssertions(() =>
			{
				var goodsItemsSplitContainer = userControl.AdditionalDocumentsSplitContainer;
				AssertEquals("AdditionalDocumentsSplitContainer.Orientation", Orientation.Horizontal, goodsItemsSplitContainer.Orientation);
				AssertEquals("AdditionalDocumentsSplitContainer.Panel2MinSize", 100, goodsItemsSplitContainer.Panel2MinSize);
				AssertEquals("AdditionalDocumentsSplitContainer.Panel2 Default size", 100, goodsItemsSplitContainer.Panel2.Height);
				AssertEquals("AdditionalDocumentDynamicLayoutPanel", true, goodsItemsSplitContainer.Panel2.Contains(userControl.AdditionalDocumentDynamicLayoutPanel));
			});
		}

		public void TestGoodsItemAdditionalDocumentsGridUserControl_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType("A");
			userControl.SetDataBinding(nctsHeader, "");

			var additionalDocumentsSplitContainer = userControl.AdditionalDocumentsSplitContainer;
			var additionalDocumentsGridUserControl = additionalDocumentsSplitContainer.Panel1.FindSingle<ArrivalGoodsItemAdditionalDocumentsGridUserControl>("GoodsItemAdditionalDocumentsGridUserControl");

			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, additionalDocumentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", additionalDocumentsGridUserControl.GetBindingMember());
			});
		}

		public void TestGoodsItemAdditionalDocumentsGridUserControl_Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType("D");
			userControl.SetDataBinding(nctsHeader, "");

			var additionalDocumentsSplitContainer = userControl.AdditionalDocumentsSplitContainer;
			var additionalDocumentsGridUserControl = additionalDocumentsSplitContainer.Panel1.FindSingle<DepartureGoodsItemAdditionalDocumentsGridUserControl>("GoodsItemAdditionalDocumentsGridUserControl");

			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, additionalDocumentsGridUserControl.Dock);
				AssertEquals("BindingMember", ".", additionalDocumentsGridUserControl.GetBindingMember());
			});
		}

		public void TestAdditionalDocumentDynamicLayoutPanel()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			CombineAssertions(() =>
			{
				var additionalDocumentDynamicLayoutPanel = userControl.AdditionalDocumentDynamicLayoutPanel;
				AssertEquals("Dock", DockStyle.Fill, additionalDocumentDynamicLayoutPanel.Dock);
				AssertEquals("AutoScroll", true, additionalDocumentDynamicLayoutPanel.AutoScroll);
				DynamicLayoutPanelTest.AssertControlsOrder(additionalDocumentDynamicLayoutPanel,
					nameof(AdditionalDocumentControlBag.KindDropEdit),
					nameof(AdditionalDocumentControlBag.TypeCodeFindBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemAdditionalDocumentsTabUserControl();
		}
		Phase5GoodsItemAdditionalDocumentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
