using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingAuthorizationBehaviourProvider
	{
		void ResetAuthorisationRecordWhenResetPivotStatusToQueued(AccEInvoicingTransactionPivot pivot);
	}
}
