using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Vietnam
{
	class VietnamEInvoicingPreEligibilityProvider : EInvoicingPreEligibilityProvider
	{
		public override bool CanEvaluateByTransaction(AccTransactionHeader transaction) => true;
	}
}
