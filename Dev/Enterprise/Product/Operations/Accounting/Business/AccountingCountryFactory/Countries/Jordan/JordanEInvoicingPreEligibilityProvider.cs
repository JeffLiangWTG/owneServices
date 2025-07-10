
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Jordan
{
	class JordanEInvoicingPreEligibilityProvider : EInvoicingPreEligibilityProvider
	{
		public override bool CanEvaluateByTransaction(AccTransactionHeader transaction) => true;
	}
}
