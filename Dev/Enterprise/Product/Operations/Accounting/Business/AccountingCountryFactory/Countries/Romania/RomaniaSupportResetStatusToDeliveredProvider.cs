using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class RomaniaSupportResetStatusToDeliveredProvider : ISupportResetStatusToDelivered
	{
		public IEnumerable<AccEInvoicingTransactionPivot> GetEligiblePivotToRequeue(IEnumerable<AccEInvoicingTransactionPivot> pivots)
		{
			var pivotTypesEligibleForRepolling = new List<ZString> { EInvoicingPivotState.Failed, EInvoicingPivotState.BatchedWithError };
			var eligiblePivots = pivots.Where(x => pivotTypesEligibleForRepolling.Contains(x.AIP_Status) && !string.IsNullOrWhiteSpace(x.ParentTransactionHeader.EInvoicingProxy.AuthorisationNumber));
			return eligiblePivots;
		}
	}
}
