using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingPreEligibilityProvider
	{
		/// <summary>
		/// Determines if we should consider the transaction for eligibilty criteria.
		/// The EInvoicingPreEligibilityProvider considers any transaction with AH_PostDate on or after the compliance date to be eligible.
		/// Return true to continue evaluating eligibility criteria, or false to not evaluate the transaction.
		/// </summary>
		bool CanEvaluateByComplianceDate(AccTransactionHeader transaction, DateTime complianceDate);

		/// <summary>
		/// Determines if we should consider the transaction for eligibilty criteria.
		/// The EInvoicingPreEligibilityProvider considers any transaction being posted and if they are already in the database.
		/// Return true to continue evaluating eligibility criteria, or false to not evaluate the transaction.
		/// </summary>
		bool CanEvaluateByTransaction(AccTransactionHeader transaction);
	}
}
