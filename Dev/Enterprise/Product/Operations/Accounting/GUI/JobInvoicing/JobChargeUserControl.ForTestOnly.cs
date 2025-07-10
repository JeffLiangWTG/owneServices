#if DEBUG

using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobChargeUserControl
	{
		public ZTextBox ClientContractNumber_ForTestOnly => ClientContractNumberTextBox;

		public ZButton.Bare ClientContractNumberButton_ForTestOnly => ClientContractNumberButton;

		public bool ProcessDialogKey_ForTestOnly(Keys keyData)
		{
			return ProcessDialogKey(keyData);
		}

		public ZStmNotePopupButton AutoRateNotePopupButton2_ForTestOnly
		{
			get { return AutoRateNotePopupButton2; }
			set { AutoRateNotePopupButton2 = value; }
		}

		public ZStmNotePopupButton AutoRateNotePopupButton_ForTestOnly
		{
			get { return AutoRateNotePopupButton; }
			set { AutoRateNotePopupButton = value; }
		}

		public void AutoPopulateButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			AutoPopulateButton_Click(sender, e);
		}

		public void JobChargeBoundGrid_Click_ForTestOnly(object sender, EventArgs e)
		{
			JobChargeBoundGrid_Click(sender, e);
		}

		public ZButton CashAdvanceButton_ForTestOnly => CashAdvanceButton;

		public void CashAdvanceButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			CashAdvanceButton_Click(sender, e);
		}

		public ZGrid JobExRateBoundGrid_ForTestOnly
		{
			get { return JobExRateBoundGrid; }
			set { JobExRateBoundGrid = value; }
		}

		public string CurrentCellMappingName_ForTestOnly => CurrentCellMappingName;

		public ZCodeFindBox OSCostCurrencyCodeFindBox_ForTestOnly
		{
			get { return OSCostCurrencyCodeFindBox; }
			set { OSCostCurrencyCodeFindBox = value; }
		}

		public ZCodeFindBox OSSellAmountCurrencyCodeFindBox_ForTestOnly
		{
			get { return OSSellAmountCurrencyCodeFindBox; }
			set { OSSellAmountCurrencyCodeFindBox = value; }
		}

		public ZGuidFindBox BranchesGuidFindBox_ForTestOnly
		{
			get { return BranchesGuidFindBox; }
			set { BranchesGuidFindBox = value; }
		}

		public ZTabPage DetailsTabPage_ForTestOnly
		{
			get { return DetailsTabPage; }
			set { DetailsTabPage = value; }
		}

		public ZTabPage RevenueTabPage_ForTestOnly
		{
			get { return RevenueTabPage; }
			set { RevenueTabPage = value; }
		}

		public ZTabPage CostTabPage_ForTestOnly
		{
			get { return CostTabPage; }
			set { CostTabPage = value; }
		}

		public void JobChargeBoundGrid_ColourDeciding_ForTestOnly(object sender, ColourDecidingEventArgs e)
		{
			JobChargeBoundGrid_ColourDeciding(sender, e);
		}

		public void JobExRateBoundGrid_ColourDeciding_ForTestOnly(object sender, ColourDecidingEventArgs e)
		{
			JobExRateBoundGrid_ColourDeciding(sender, e);
		}

		public ZTextBox AgentDeclaredCostCurrencyTextBox_ForTestOnly
		{
			get { return AgentDeclaredCostCurrencyTextBox; }
			set { AgentDeclaredCostCurrencyTextBox = value; }
		}

		public ZTextBox AgentDeclaredSellCurrencyTextBox_ForTestOnly
		{
			get { return AgentDeclaredSellCurrencyTextBox; }
			set { AgentDeclaredSellCurrencyTextBox = value; }
		}

		public ZTextBox LocalAgentDeclaredCostCurrencyTextBox_ForTestOnly
		{
			get { return LocalAgentDeclaredCostCurrencyTextBox; }
			set { LocalAgentDeclaredCostCurrencyTextBox = value; }
		}

		public ZTextBox LocalAgentDeclaredSellCurrencyTextBox_ForTestOnly
		{
			get { return LocalAgentDeclaredSellCurrencyTextBox; }
			set { LocalAgentDeclaredSellCurrencyTextBox = value; }
		}

		public ZTextBox AutoRateDescCostTextBox_ForTestOnly
		{
			get { return AutoRateDescCostTextBox; }
			set { AutoRateDescCostTextBox = value; }
		}

		public ZTextBox AutoRateDescRevenueTextBox_ForTestOnly
		{
			get { return AutoRateDescRevenueTextBox; }
			set { AutoRateDescRevenueTextBox = value; }
		}
	}
}

#endif
