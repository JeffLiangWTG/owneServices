using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsGoodsItemsUserControlTest : TestCaseWithFactory
{
	public void TestNctsPreviousDocumentsDynamicUserControl()
	{
		using (var form = new ZForm())
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			form.Controls.Add(control);
			form.Show();
			control.ItemPreviousDocumentsTabPageExposed.Show();
			AssertEquals(typeof(NctsPreviousDocumentsUserControl), control.NctsPreviousDocumentsDynamicUserControlExposed.UserControlType);
		}
	}

	public void TestItemTaxOrFeeTabPage()
	{
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			var itemTaxOrFeeTabPage = control.ItemTaxOrFeeTabPageExposed;
			AssertNotNull("TabPage not null", itemTaxOrFeeTabPage);
			Assert("TabPage visible", itemTaxOrFeeTabPage.TabVisible);

			control.ItemTaxOrFeeTabPageExposed.Show();
			var childTaxOrFeeUserControl = itemTaxOrFeeTabPage.FindSingleOrDefault<NctsTaxOrFeeUserControl>("TaxOrFeeUserControl");
			AssertNotNull("Child TaxOrFeeUserControl not null", childTaxOrFeeUserControl);
			Assert("Child TaxOrFeeUserControl visible", childTaxOrFeeUserControl.Visible);
		}
	}

	public void TestItemDetailsDynamicUserControl()
	{
		using (var form = new ZForm())
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			form.Controls.Add(control);
			form.Show();
			control.ItemDetailsTabPageExposed.Show();
			AssertEquals(typeof(ItemDetailsUserControl), control.GoodsItemLineDetailDynamicUserControlExposed.UserControlType);
		}
	}

	public void TestProcedureCodeFindBox()
	{
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			var procedureCodeFindBox = control.Controls.Find("ProcedureCodeFindBox", true);
			AssertNotNull("Should have the box", procedureCodeFindBox);
		}
	}

	[RequiresSTA]
	public void TestNctsSupportingDocumentsDynamicUserControl()
	{
		using (var form = new ZForm())
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			form.Controls.Add(control);
			form.Show();
			control.SupportingDocumentsTabPageExposed.Show();
			AssertEquals(typeof(NctsSupportingDocumentsUserControl), control.SupportingDocumentsDynamicUserControlExposed.UserControlType);
		}
	}

	public void TestM2LinesTabPage()
	{
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			var m2LinesTabPage = control.M2LinesTabPageExposed;
			AssertNotNull(nameof(m2LinesTabPage), m2LinesTabPage);
			AssertEquals(nameof(m2LinesTabPage.TabVisible), true, m2LinesTabPage.TabVisible);

			control.M2LinesTabPageExposed.Show();
			var groupedPreviousDocumentsUserControl = m2LinesTabPage.FindSingleOrDefault<GroupedPreviousDocumentsUserControl>("GroupedPreviousDocumentsUserControl");
			AssertNotNull(nameof(groupedPreviousDocumentsUserControl), groupedPreviousDocumentsUserControl);
			AssertEquals($"{nameof(groupedPreviousDocumentsUserControl)} visibility", true, groupedPreviousDocumentsUserControl.Visible);
		}
	}

	public void TestGoodsItemsTabControlPagesOrder()
	{
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			var tabControl = control.FindSingleOrDefault<ZTabControl>("GoodsItemsTabControl");
			AssertNotNull(nameof(tabControl), tabControl);

			CombineAssertions(() =>
			{
				AssertEquals("TabPage at 0", "ItemDetailsTabPage", tabControl.TabPages[0].Name);
				AssertEquals("TabPage at 1", "ItemPackagesTabPage", tabControl.TabPages[1].Name);
				AssertEquals("TabPage at 2", "ItemContainersTabPage", tabControl.TabPages[2].Name);
				AssertEquals("TabPage at 3", "SupportingDocumentsTabPage", tabControl.TabPages[3].Name);
				AssertEquals("TabPage at 4", "ItemAdditionalInfosTabPage", tabControl.TabPages[4].Name);
				AssertEquals("TabPage at 4", "RemarksTabPage", tabControl.TabPages[5].Name);
				AssertEquals("TabPage at 5", "ItemPreviousDocumentsTabPage", tabControl.TabPages[6].Name);
				AssertEquals("TabPage at 6", "M2LinesTabPage", tabControl.TabPages[7].Name);
				AssertEquals("TabPage at 7", "ItemSecurityTabPage", tabControl.TabPages[8].Name);
				AssertEquals("TabPage at 8", "ItemTaxOrFeeTabPage", tabControl.TabPages[9].Name);
			});
		}
	}

	[RequiresSTA]
	public void TestPreviousDocumentsTabPageReadOnlyForDepartureMovements()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var goodsItemAccepted = header.MovementHeader.GoodsItems.AddNew();
		goodsItemAccepted.BY_Status = NctsTransitStatusList.Codes.DeclarationAccepted;
		var goodsItemToFix = header.MovementHeader.GoodsItems.AddNew();
		goodsItemToFix.BY_Status = NctsTransitStatusList.Codes.NbRejected;

		header.BH_MessageStatus = EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;
		header.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.NbRejected;

		using (var form = new NctsMovementFormForTesting(header))
		{
			form.Show();
			form.MainTabControl.SelectedTab = form.GoodsItemsTabPageExposed;
			Application.DoEvents();

			var goodsItemsUserControl = (NctsGoodsItemsUserControl)form.GoodsItemsTabPageExposed.Controls.Find("NctsGoodsItemsUserControl", true).FirstOrDefault();
			var goodsItemsGrid = (ZGrid)form.GoodsItemsTabPageExposed.Controls.Find("GoodsItemsGrid", true).FirstOrDefault();

			var goodsItemsTabControl = goodsItemsUserControl.FindSingle<ZTabControl>("GoodsItemsTabControl");
			var previousDocumentsTab = goodsItemsUserControl.FindSingle<ZTabPage>("ItemPreviousDocumentsTabPage");

			goodsItemsTabControl.SelectTab(previousDocumentsTab);
			Application.DoEvents();

			var previousDocumentsGrid = previousDocumentsTab.FindSingle<ZGrid>("PreviousDocumentsGrid");

			goodsItemsGrid.Select(0);
			AssertEquals("Previous Document Grid readonly property", true, previousDocumentsGrid.ReadOnly);

			goodsItemsGrid.Select(1);
			goodsItemsGrid.CurrentRowIndex = 1;
			AssertEquals("Previous Document Grid readonly property", false, previousDocumentsGrid.ReadOnly);
		}
	}

	public void TestRemarksTabPage()
	{
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			var remarksTabPage = control.RemarksTabPageExposed;
			AssertNotNull(nameof(remarksTabPage), remarksTabPage);
			AssertEquals(nameof(remarksTabPage.TabVisible), true, remarksTabPage.TabVisible);

			control.RemarksTabPageExposed.Show();
			var nctsGoodsItemRemarksUserControl = remarksTabPage.FindSingleOrDefault<NctsGoodsItemRemarksUserControl>("NctsGoodsItemRemarksUserControl");
			AssertNotNull(nameof(nctsGoodsItemRemarksUserControl), nctsGoodsItemRemarksUserControl);
			AssertEquals("RemarksTextBox visibility", true, nctsGoodsItemRemarksUserControl.Visible);
		}
	}

	public void TestGoodsItemsGridUserControlType()
	{
		using (var control = new NctsGoodsItemsUserControlForTesting())
		{
			AssertEquals(typeof(GoodsItemsGridUserControl), control.GoodsItemsGridDynamicUserControlExposed.UserControlType);
		}
	}

	class NctsGoodsItemsUserControlForTesting : NctsGoodsItemsUserControl
	{
		public NctsGoodsItemsUserControlForTesting() : base()
		{
		}

		public ZDynamicControlCreationUserControl NctsPreviousDocumentsDynamicUserControlExposed => base.NctsPreviousDocumentsDynamicUserControl;
		public ZTabPage ItemPreviousDocumentsTabPageExposed => base.ItemPreviousDocumentsTabPage;
		public ZTabPage ItemTaxOrFeeTabPageExposed => base.ItemTaxOrFeeTabPage;
		public ZDynamicControlCreationUserControl GoodsItemLineDetailDynamicUserControlExposed => base.GoodsItemLineDetailDynamicUserControl;
		public ZTabPage ItemDetailsTabPageExposed => base.ItemDetailsTabPage;
		public ZDynamicControlCreationUserControl SupportingDocumentsDynamicUserControlExposed => base.SupportingDocumentsDynamicUserControl;
		public ZTabPage SupportingDocumentsTabPageExposed => base.SupportingDocumentsTabPage;
		public ZTabPage M2LinesTabPageExposed => base.M2LinesTabPage;
		public ZTabPage RemarksTabPageExposed => base.RemarksTabPage;
		public ZDynamicControlCreationUserControl GoodsItemsGridDynamicUserControlExposed => base.GoodsItemsGridDynamicUserControl;
	}
}
