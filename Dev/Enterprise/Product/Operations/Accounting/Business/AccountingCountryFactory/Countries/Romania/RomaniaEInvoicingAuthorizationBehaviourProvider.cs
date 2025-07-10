using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class RomaniaEInvoicingAuthorizationBehaviourProvider : EInvoicingAuthorizationBehaviourProvider
	{
		public override void ResetAuthorisationRecordWhenResetPivotStatusToQueued(AccEInvoicingTransactionPivot pivot)
		{
			base.ResetAuthorisationRecordWhenResetPivotStatusToQueued(pivot);

			pivot.ParentTransactionHeader.AH_GovernmentAllocatedID = ZString.Empty;
			pivot.ParentTransactionHeader.AH_ComplianceDocumentDate = ZDate.Empty;
		}
	}
}
