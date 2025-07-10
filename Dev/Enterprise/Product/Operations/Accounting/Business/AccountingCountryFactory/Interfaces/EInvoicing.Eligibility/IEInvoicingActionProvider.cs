using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingActionProvider
	{
		void OnEvaluateEligibilityAndQueue(AccTransactionHeader transaction);

		bool SupportPendingInvoiceAction { get; }
	}
}
