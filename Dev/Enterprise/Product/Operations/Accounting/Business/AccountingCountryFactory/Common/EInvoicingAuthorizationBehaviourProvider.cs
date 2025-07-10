using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class EInvoicingAuthorizationBehaviourProvider : IEInvoicingAuthorizationBehaviourProvider
	{
		public virtual void ResetAuthorisationRecordWhenResetPivotStatusToQueued(AccEInvoicingTransactionPivot pivot)
		{
			pivot.Factory.Load<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, pivot.AIP_ParentID)).ForEach(x => x.Delete());
		}
	}
}
