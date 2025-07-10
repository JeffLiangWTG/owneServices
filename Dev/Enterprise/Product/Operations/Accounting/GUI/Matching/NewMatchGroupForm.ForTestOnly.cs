#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class NewMatchGroupForm
	{
		public ContinueWithSave ValidateAndSave_ForTestOnly()
		{
			return ValidateAndSave();
		}

		public ZGrid MatchTransactionsGrid_ForTestOnly
		{
			get { return MatchTransactionsGrid; }
			set { MatchTransactionsGrid = value; }
		}

		public void HandleMoveUp_ForTestOnly()
		{
			HandleMoveUp();
		}

		public ZTemplateTabControl MatchingTabControl_ForTestOnly
		{
			get { return MatchingTabControl; }
			set { MatchingTabControl = value; }
		}

		public ZTabPage APJournalsTabPage_ForTestOnly
		{
			get { return APJournalsTabPage; }
			set { APJournalsTabPage = value; }
		}

		public ZGrid APJournalsGrid_ForTestOnly
		{
			get { return APJournalsGrid; }
			set { APJournalsGrid = value; }
		}

		public ZTabPage ARJournalsTabPage_ForTestOnly
		{
			get { return ARJournalsTabPage; }
			set { ARJournalsTabPage = value; }
		}

		public ZGrid ARJournalsGrid_ForTestOnly
		{
			get { return ARJournalsGrid; }
			set { ARJournalsGrid = value; }
		}

		public ZTabPage GridsTabPage_ForTestOnly
		{
			get { return GridsTabPage; }
			set { GridsTabPage = value; }
		}

		public void HandleFind_ForTestOnly()
		{
			HandleFind();
		}

		public ZDisplayGrid UnmatchedTransactionsGrid_ForTestOnly
		{
			get { return UnmatchedTransactionsGrid; }
			set { UnmatchedTransactionsGrid = value; }
		}

		public void HandleAutoSelect_ForTestOnly()
		{
			HandleAutoSelect();
		}

		public ZLabel FilterResultsLabel_ForTestOnly
		{
			get { return FilterResultsLabel; }
			set { FilterResultsLabel = value; }
		}

		public void HandleUnselectAll_ForTestOnly()
		{
			HandleUnselectAll();
		}

		public ZTabPage SettlementOrgsTabPage_ForTestOnly
		{
			get { return SettlementOrgsTabPage; }
			set { SettlementOrgsTabPage = value; }
		}

		public ZGrid OrgInfoGrid_ForTestOnly
		{
			get { return OrgInfoGrid; }
			set { OrgInfoGrid = value; }
		}

		public void HandleSelectAll_ForTestOnly()
		{
			HandleSelectAll();
		}

		public IZForm ShowNewMiscTransactionForm_ForTestOnly(ZString transactionType)
		{
			return ShowNewMiscTransactionForm(transactionType);
		}

		public ZButton OverpaymentButton_ForTestOnly
		{
			get { return OverpaymentButton; }
			set { OverpaymentButton = value; }
		}

		public MenuItem OVPMenuItem_ForTestOnly => OVPMenuItem;

		public ZGroupBox BalanceGroupBox_ForTestOnly
		{
			get { return BalanceGroupBox; }
			set { BalanceGroupBox = value; }
		}

		public ZDisplayGrid CurrencySummaryGrid_ForTestOnly
		{
			get { return CurrencySummaryGrid; }
			set { CurrencySummaryGrid = value; }
		}

		public ZButton ChangePaymentAmountButton_ForTestOnly
		{
			get { return ChangePaymentAmountButton; }
			set { ChangePaymentAmountButton = value; }
		}

		public void ChangePaymentAmountButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			ChangePaymentAmountButton_Click(sender, e);
		}

		public void CurrencySummaryButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			CurrencySummaryButton_Click(sender, e);
		}

		public Business.Base.Matching.MatchingBase FMatchingBase_ForTestOnly
		{
			get { return fMatchingBase; }
			set { fMatchingBase = value; }
		}

		public string CurrencySummaryFormKey_ForTestOnly => CurrencySummaryFormKey;

		public void ViewUnMatchGridTransaction_ForTestOnly(object sender, EventArgs e)
		{
			ViewUnMatchGridTransaction(sender, e);
		}

		public void ViewMatchGridTransaction_ForTestOnly(object sender, EventArgs e)
		{
			ViewMatchGridTransaction(sender, e);
		}

		public void MatchTransactionsGridMenu_Popup_ForTestOnly(object sender, EventArgs e)
		{
			MatchTransactionsGridMenu_Popup(sender, e);
		}

		public MenuItem PayLinesMenuItem_ForTestOnly => PayLinesMenuItem;

		public MenuItem PayLinesMenuSeparator_ForTestOnly => PayLinesMenuSeparator;

		public void PayLines_ForTestOnly(object sender, EventArgs e)
		{
			PayLines(sender, e);
		}

		public void ValidateAll_ForTestOnly(ValidationType type)
		{
			ValidateAll(type);
		}
	}
}

#endif
