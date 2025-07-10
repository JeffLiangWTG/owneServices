using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherNumberLookUp
	{
		protected AccTransactionHeader Transaction;

		public VoucherNumberLookUp(AccTransactionHeader transaction)
		{
			this.Transaction = transaction;
		}

		public ZString GetVoucherNumber()
		{
			ZString voucherNumber = "";
			if (Transaction.AH_Ledger == LedgerTypes.AccountsPayable &&
				(Transaction.AH_TransactionType == TransactionTypes.Invoice ||
				Transaction.AH_TransactionType == TransactionTypes.CreditNote ||
				Transaction.AH_TransactionType == TransactionTypes.AdjustmentNote))
			{
				voucherNumber = Transaction.AH_ConsolidatedInvoiceRef;
			}
			else
			{
				voucherNumber = Transaction.AH_TransactionNum;
			}

			return voucherNumber;
		}
	}
}
