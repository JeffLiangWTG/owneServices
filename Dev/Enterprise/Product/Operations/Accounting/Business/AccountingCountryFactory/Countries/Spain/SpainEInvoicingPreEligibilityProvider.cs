using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class SpainEInvoicingPreEligibilityProvider : EInvoicingPreEligibilityProvider
	{
		public override bool CanEvaluateByTransaction(AccTransactionHeader transaction) => true;
	}
}
