using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class FijiEInvoicingPivotStatusProvider : IEInvoicingPivotStatusProvider
	{
		public bool CanCreateNotEligibleForEInvoicingPivot(AccTransactionHeader header) =>
			header is TransactionHeader transactionHeader &&
			transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable &&
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		/// <summary>
		/// For Fiji, there is no specific status for the initial state.
		/// The status will be returned from the base logic.
		/// </summary>
		public string GetInitialPivotStatus(ITransactionHeader header)
		{
			return string.Empty;
		}
	}
}
