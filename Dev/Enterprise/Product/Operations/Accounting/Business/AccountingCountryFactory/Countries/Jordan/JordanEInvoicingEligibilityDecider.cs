using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Jordan
{
	class JordanEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		public string GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return @$"Have Compliance Sub Type: {!transaction.ComplianceSubType.IsEmpty}";
		}

		public bool IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return transaction.Ledger == LedgerTypes.AccountsReceivable &&
					(transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote) &&
					!transaction.ComplianceSubType.IsEmpty;
		}
	}
}
