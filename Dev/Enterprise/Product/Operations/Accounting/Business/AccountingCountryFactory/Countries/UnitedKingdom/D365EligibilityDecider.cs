using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class D365EligibilityDecider : IEInvoicingEligibilityDecider
	{
		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
			&& (transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote || transaction.TransactionType == TransactionTypes.AdjustmentNote);

		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction) => null;
	}
}
