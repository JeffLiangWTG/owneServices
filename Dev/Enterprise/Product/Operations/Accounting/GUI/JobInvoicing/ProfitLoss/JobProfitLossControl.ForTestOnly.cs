#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobProfitLossControl
	{
		public ZTabPage DetailsTabPage_ForTestOnly
		{
			get { return DetailsTabPage; }
			set { DetailsTabPage = value; }
		}

		public ZTabPage SummaryTabPage_ForTestOnly
		{
			get { return SummaryTabPage; }
			set { SummaryTabPage = value; }
		}

		public ZGrid ProfitLossGrid_ForTestOnly
		{
			get { return ProfitLossGrid; }
			set { ProfitLossGrid = value; }
		}

		public ZTemplateTabControl TabControl_ForTestOnly
		{
			get { return TabControl; }
			set { TabControl = value; }
		}

		public ZGrid ProfitLossSummaryGrid_ForTestOnly
		{
			get { return ProfitLossSummaryGrid; }
			set { ProfitLossSummaryGrid = value; }
		}

		public ZTabPage GlobalJobCostingTabPage_ForTestOnly
		{
			get { return GlobalJobCostingTabPage; }
			set { GlobalJobCostingTabPage = value; }
		}

		public ZButton JobProfitReportButton_ForTestOnly
		{
			get { return JobProfitReportButton; }
			set { JobProfitReportButton = value; }
		}

		public void PrintJobProfitDocument_ForTestOnly()
		{
			PrintJobProfitDocument();
		}

		public void JobProfitReportButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			JobProfitReportButton_Click(sender, e);
		}

		public ZButton SummaryClearButton_ForTestOnly
		{
			get { return SummaryClearButton; }
			set { SummaryClearButton = value; }
		}

		public ZButton SummaryFindButton_ForTestOnly
		{
			get { return SummaryFindButton; }
			set { SummaryFindButton = value; }
		}

		public JobInvoicingSecurityHelper SecurityHelper_ForTestOnly => SecurityHelper;

		public MultilingualString ReverseWIPAccrualText_ForTestOnly => reverseWIPAccrualText;

		public void ShowWIPAccrualForReversal_ForTestOnly(Business.WIPAccrual.BaseWIPAccrual wIPAccrual, ZController controller)
		{
			ShowWIPAccrualForReversal(wIPAccrual, controller);
		}

		public ZGrid GJCProfitLossGrid_ForTestOnly
		{
			get { return GJCProfitLossGrid; }
			set { GJCProfitLossGrid = value; }
		}

		public void FindButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			FindButton_Click(sender, e);
		}

		public void ProfitLossGrid_ColourDeciding_ForTestOnly(object sender, ColourDecidingEventArgs e)
		{
			ProfitLossGrid_ColourDeciding(sender, e);
		}

		public MultilingualString ViewTransactionText_ForTestOnly => viewTransactionText;

		public void ViewTransaction_ForTestOnly(object sender, EventArgs e)
		{
			ViewTransaction(sender, e);
		}

		public void ReverseWIPAccrual_ForTestOnly(object sender, EventArgs e)
		{
			ReverseWIPAccrual(sender, e);
		}

		public ZPanel GlobalJobCostingTabPanel_ForTestOnly
		{
			get { return GlobalJobCostingTabPanel; }
			set { GlobalJobCostingTabPanel = value; }
		}

		public MenuItem FindMenuItemByText_ForTestOnly(string text)
		{
			return ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.FindByText(text);
		}

		public BusinessObject[] SelectedBusinessObjects_ForTestOnly() => SelectedTransactions;

		public void HandleRegenerateJournalEntries_ForTestOnly(object sender, EventArgs e) => HandleRegenerateJournalEntries(sender, e);

		public void ReSetMenuItem()
		{
			ProfitLossGrid_ForTestOnly.ContextMenu.MenuItems.Clear();
			SetupProfitLossContextMenu();
		}
	}
}

#endif
