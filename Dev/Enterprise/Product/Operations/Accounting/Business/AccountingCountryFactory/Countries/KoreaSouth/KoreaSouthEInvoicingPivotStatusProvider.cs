using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KoreaSouthEInvoicingPivotStatusProvider : IEInvoicingPivotStatusProvider
	{
		public bool CanCreateNotEligibleForEInvoicingPivot(AccTransactionHeader transaction) => false;

		public string GetInitialPivotStatus(ITransactionHeader header)
		{
			if (header != null && header is TransactionHeader transactionHeader
			&& transactionHeader.OriginalTransaction is InvoicingBase originalTransaction
			&& !originalTransaction.IsApprovedByGovt)
			{
				return EInvoicingPivotState.Pending;
			}

			return string.Empty;
		}
	}
}
