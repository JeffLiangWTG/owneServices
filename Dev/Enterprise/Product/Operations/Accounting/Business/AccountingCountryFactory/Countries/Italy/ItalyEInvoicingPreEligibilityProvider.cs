using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class ItalyEInvoicingPreEligibilityProvider : EInvoicingPreEligibilityProvider
	{
		public override bool CanEvaluateByTransaction(AccTransactionHeader transaction) =>
			(transaction.IsAPTransactionConvertedFromIncompleteTransaction
				|| transaction.IsAPTransactionConvertedFromUnapprovedTransaction
				|| transaction.IsAPTransactionConvertedFromTransactionPendingAllocation)
			|| (transaction != null && !transaction.IsInDatabase);
	}
}
