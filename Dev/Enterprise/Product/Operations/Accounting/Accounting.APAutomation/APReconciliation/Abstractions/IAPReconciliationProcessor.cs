using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public interface IAPReconciliationProcessor
	{
		APReconciliationProcessingResult Reconcile(AccDraftInvoiceHeader draftTransaction);
	}
}
