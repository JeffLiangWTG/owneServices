using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class RomaniaEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		public string GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction) => null;

		public bool IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
				&& (transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote || transaction.TransactionType == TransactionTypes.AdjustmentNote);
	}
}
