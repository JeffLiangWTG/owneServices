using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class HouseConsignmentDifferencesTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(INctsBillCollection<NctsBill>), userControl.BindingSource.DataSourceType);
		}

		public void TestHouseDetailsBindingMember()
		{
			AssertEquals(".", userControl.HouseDetailsDynamicLayoutPanel.GetBindingMember());
		}

		public void TestBinding_HouseConsignmentSupportingDocumentsPanelUserControl()
		{
			AssertEquals("SupportingDocuments", userControl.HouseConsignmentSupportingDocumentsPanelUserControl.GetBindingMember());
		}

		public void TestBinding_HouseConsignmentAdditionalDocumentsPanelUserControl()
		{
			AssertEquals("AdditionalDocuments", userControl.HouseConsignmentAdditionalDocumentsPanelUserControl.GetBindingMember());
		}

		public void TestBinding_HouseConsignmentPreviousDocumentsPanelUserControl()
		{
			AssertEquals("PreviousDocuments", userControl.HouseConsignmentPreviousDocumentsPanelUserControl.GetBindingMember());
		}

		public void TestTabs()
		{
			var tabControl = userControl.HouseConsignmentDifferencesTabControl;
			var tabPages = tabControl.TabPages;
			AssertArrayEqualsByElements(new[] { "HouseDetailsTabPage", "GoodsItemsTabPage", "SupportingDocumentsTabPage", "AdditionalDocumentsTabPage", "PreviousDocumentsTabPage" }, tabPages.Cast<TabPage>().Select(x => x.Name).ToArray());
		}

		public void TestGoodsItemsTabPage_Caption()
		{
			AssertEquals("Goods Items", userControl.GoodsItemsTabPage.CaptionResourceString.Caption);
		}

		public void TestPhase5GoodsItemDifferencesTabUserControl()
		{
			var phase5GoodsItemDifferencesTabUserControl = userControl.Phase5GoodsItemDifferencesTabUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("Within GoodsItemsTabPage", true, userControl.GoodsItemsTabPage.Controls.Contains(phase5GoodsItemDifferencesTabUserControl));
				AssertEquals("Dock", System.Windows.Forms.DockStyle.Fill, phase5GoodsItemDifferencesTabUserControl.Dock);
				AssertEquals("ArrivalGoodsItems", phase5GoodsItemDifferencesTabUserControl.GetBindingMember());
			});
		}

		public void TestDetailsTabPage_Caption()
		{
			AssertEquals("House Details", userControl.HouseDetailsTabPage.CaptionResourceString.Caption);
		}

		public void TestOverviewDynamicLayoutPanel()
		{
			AssertEquals(System.Windows.Forms.DockStyle.Fill, userControl.HouseDetailsDynamicLayoutPanel.Dock);
		}

		public void TestHouseConsignmentAdditionalDocuments()
		{
			var houseConsignmentAdditionalDocumentsPanelUserControl = userControl.HouseConsignmentAdditionalDocumentsPanelUserControl;
			AssertType<HouseConsignmentAdditionalDocumentsPanelUserControl>("Type", houseConsignmentAdditionalDocumentsPanelUserControl);
		}

		public void TestHouseConsignmentDifferencesSplitContainer()
		{
			var splitContainer = userControl.HouseConsignmentDifferencesSplitContainer;
			AssertEquals("Panel2MinSize", ControlDpiScalingHelper.ScaleToCurrentDpiY(450), splitContainer.Panel2MinSize);
		}

		public void TestHouseConsignmentDifferencesGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var splitContainer = userControl.HouseConsignmentDifferencesSplitContainer;
			var houseConsignmentDifferencesGridUserControl = splitContainer.Panel1.FindSingle<HouseConsignmentDifferencesGridUserControl>("HouseConsignmentDifferencesGridUserControl");

			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, houseConsignmentDifferencesGridUserControl.Dock);
				AssertEquals("BindingMember", ".", houseConsignmentDifferencesGridUserControl.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new HouseConsignmentDifferencesTabUserControl();
		}
		HouseConsignmentDifferencesTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
