using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class SerbiaEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction) => null;

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
				&& (transaction.TransactionType == TransactionTypes.Invoice
					|| transaction.TransactionType == TransactionTypes.CreditNote
					|| transaction.TransactionType == TransactionTypes.AdjustmentNote);
	}
}
