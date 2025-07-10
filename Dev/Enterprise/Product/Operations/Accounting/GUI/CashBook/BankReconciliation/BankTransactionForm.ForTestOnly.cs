#if DEBUG

using System;
using Enterprise.Accounting.Business.CashBook;

namespace Enterprise.Accounting.GUI.CashBook.BankReconciliation
{
	public partial class BankTransactionForm
	{
		public DirectTransactionsBusinessObject TransactionsBizO_ForTestOnly => TransactionsBizO;

		public void NewDirectReceiptButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			NewDirectReceiptButton_Click(sender, e);
		}

		public void NewDirectPaymentButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			NewDirectPaymentButton_Click(sender, e);
		}

		public void ApplyButton_Click_ForTestOnly(object sender, EventArgs e)
		{
			ApplyButton_Click(sender, e);
		}

		public ZArchitecture.ZGrid DirectPaymentLinesGrid_ForTestOnly
		{
			get { return DirectPaymentLinesGrid; }
		}

		public ZArchitecture.ZGrid DirectReceiptLinesGrid_ForTestOnly
		{
			get { return DirectReceiptLinesGrid; }
		}
	}
}

#endif
