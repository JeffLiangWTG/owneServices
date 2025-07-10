using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class IsraelEInvoicingPivotStatusProvider : IEInvoicingPivotStatusProvider
	{
		public bool CanCreateNotEligibleForEInvoicingPivot(AccTransactionHeader transaction) => false;

		public string GetInitialPivotStatus(ITransactionHeader header)
			=> header != null ? EInvoicingPivotState.Queued : string.Empty;
	}
}
