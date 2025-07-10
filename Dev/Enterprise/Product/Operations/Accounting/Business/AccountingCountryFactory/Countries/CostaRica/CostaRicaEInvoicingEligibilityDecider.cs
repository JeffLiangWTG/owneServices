using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class CostaRicaEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
			=> null;

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
				&& (transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote)
				&& transaction.HasEligibleComplianceSubType();
	}
}
