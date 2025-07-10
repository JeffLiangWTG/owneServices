#if DEBUG

using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation
{
	public partial class BankReconcilationForm
	{
		public void HandleReallocateCheckNumber_ForTestOnly(object sender, EventArgs e)
		{
			HandleReallocateCheckNumber(sender, e);
		}

		public void BankStatementButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			BankStatementButton_Click(sender, e);
		}

		public ZTextBox TextSearchFilterTextBox_ForTestOnly
		{
			get { return TextSearchFilterTextBox; }
			set { TextSearchFilterTextBox = value; }
		}

		public ZGrid BankReconGrid_ForTestOnly
		{
			get { return BankReconGrid; }
			set { BankReconGrid = value; }
		}

		public ZButton FindFilterButton_ForTestOnly
		{
			get { return FindFilterButton; }
			set { FindFilterButton = value; }
		}

		public ZCalcEdit ClosingBalanceCalcEdit_ForTestOnly
		{
			get { return ClosingBalanceCalcEdit; }
			set { ClosingBalanceCalcEdit = value; }
		}

		public ZButton BankStatementButton_ForTestOnly
		{
			get { return BankStatementButton; }
			set { BankStatementButton = value; }
		}

		public ZButton BankTransactionButton_ForTestOnly => BankTransactionButton;
	}
}

#endif
