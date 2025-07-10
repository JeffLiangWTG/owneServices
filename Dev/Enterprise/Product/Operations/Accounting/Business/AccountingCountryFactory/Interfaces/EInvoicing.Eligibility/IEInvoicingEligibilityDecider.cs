using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingEligibilityDecider
	{
		/// <summary>
		/// Returns information on the fields used by IsTransactionEligible for Trace Logs. By default we log Country, Ledger, TransactionType
		/// so this is method returns the details on everything else used in IsTransactionEligible.
		/// </summary>
		string GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction);

		/// <summary>
		/// Returns true if a transaction is eligible for e-Invoicing on post.
		/// </summary>
		bool IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction);
	}
}
