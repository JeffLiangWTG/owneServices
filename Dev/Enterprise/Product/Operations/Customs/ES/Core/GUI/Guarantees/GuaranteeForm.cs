using System;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.GUI.Guarantees;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class GuaranteeForm : Customs.GUI.GuaranteeForm
	{
		public GuaranteeForm(CusGuaranteeHeader guaranteeHeader, GuaranteeTransactionFilterStripBusinessObject filterStripBusinessObject)
			: base(guaranteeHeader, filterStripBusinessObject)
		{
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("F6FDCC8D-BEFF-4C23-B58F-11BAD5CB9D78", "Show Transactions With Pending Debt"), ShowTransactionsWithPendingDebtMenuItemClick);
		}

		void ShowTransactionsWithPendingDebtMenuItemClick(object sender, EventArgs e)
		{
			var transactions = (BusinessEntity as CusGuaranteeHeader).GetPendingDebtTransactions();
			ShowTransactionsWithPendingDebtForm(transactions);
		}

		protected virtual void ShowTransactionsWithPendingDebtForm(CusGuaranteeLineTransactionCollection transactionCollection)
		{
			var form = new ShowPendingDebtTransactionsForm(transactionCollection);
			ZFormModaliser.ShowDialogAndDispose(form, this);
		}

		protected override Customs.GUI.Guarantees.GuaranteeTransactionFilterControl CreateNewGuaranteeTransactionFilterControl(Customs.Business.CusGuaranteeLineTransactionCollection cusGuaranteeLineTransactions, Customs.GUI.Guarantees.GuaranteeTransactionFilterStripBusinessObject guaranteeTransactionFilterStripBusinessObject) => new GuaranteeTransactionFilterControl((CusGuaranteeLineTransactionCollection)cusGuaranteeLineTransactions, guaranteeTransactionFilterStripBusinessObject);
	}
}
