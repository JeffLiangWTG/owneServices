using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingPivotStatusProvider
	{
		string GetInitialPivotStatus(ITransactionHeader header);

		bool CanCreateNotEligibleForEInvoicingPivot(AccTransactionHeader header);
	}
}
