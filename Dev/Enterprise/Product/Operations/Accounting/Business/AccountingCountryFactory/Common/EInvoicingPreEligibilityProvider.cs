using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

// Default implementation of IEInvoicingPreEligibilityProvider
namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class EInvoicingPreEligibilityProvider : IEInvoicingPreEligibilityProvider
	{
		bool IEInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(AccTransactionHeader transaction, DateTime complianceDate)
			=> CanEvaluateByComplianceDate(transaction, complianceDate);

		public virtual bool CanEvaluateByComplianceDate(AccTransactionHeader transaction, DateTime complianceDate)
			=> complianceDate != DateTime.MinValue && transaction.AH_PostDate.Date >= (ZDate)complianceDate.Date;

		bool IEInvoicingPreEligibilityProvider.CanEvaluateByTransaction(AccTransactionHeader transaction)
			=> CanEvaluateByTransaction(transaction);

		public virtual bool CanEvaluateByTransaction(AccTransactionHeader transaction)
			=> transaction != null && !transaction.IsInDatabase;
	}
}
