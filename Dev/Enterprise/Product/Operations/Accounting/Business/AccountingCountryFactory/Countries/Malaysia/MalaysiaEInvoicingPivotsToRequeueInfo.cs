using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Countries.Malaysia
{
	class MalaysiaEInvoicingPivotsToRequeueInfo : IEInvoicingPivotsToRequeueFilterProvider
	{
		public Func<AccEInvoicingTransactionPivot, bool> GetPivotsToRequeueFilter(BusinessObjectFactory factory)
		{
			var pivotStatusesEligibleForRequeuing = new List<ZString> { EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Failed };
			Func<AccEInvoicingTransactionPivot, bool> pivotsToRequeueFilterFunction = delegate(AccEInvoicingTransactionPivot pivot)
			{
				if (pivot.AIP_ActionType != EInvoicingPivotActionType.Submit)
				{
					return false;
				}
				if (!pivotStatusesEligibleForRequeuing.Contains(pivot.AIP_Status))
				{
					return false;
				}
				if (pivot.AIP_Status == EInvoicingPivotState.Failed)
				{
					var transactionHeader = factory.Load<AccTransactionHeader>(pivot.AIP_ParentID);
					return transactionHeader?.AH_GovernmentAllocatedID.IsEmpty ?? true;
				}
				return true;
			};
			return pivotsToRequeueFilterFunction;
		}

		public string GetPivotsToRequeueMessage()
		{
			return Res.GetString("8c4efa67-32ea-4ebe-85e3-e7f183a1859b",
				@"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:
- 'FAL' - Fail and E-Reporting Government # is blank, or
- 'BER' - Batched with errors.");
		}
	}
}
