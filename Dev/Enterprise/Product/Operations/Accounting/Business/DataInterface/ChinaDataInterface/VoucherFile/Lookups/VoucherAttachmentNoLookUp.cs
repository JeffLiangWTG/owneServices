using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherAttachmentNoLookUp
	{
		readonly AccTransactionHeader Transaction;

		public VoucherAttachmentNoLookUp(AccTransactionHeader transaction)
		{
			this.Transaction = transaction;
		}

		public ZInt GetAttachmentNo()
		{
			if (Transaction == null)
			{
				return 0;
			}

			ZInt attachmentCount = 0;

			if (Transaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				switch (Transaction.AH_TransactionType)
				{
					case TransactionTypes.Invoice:
					case TransactionTypes.CreditNote:
					case TransactionTypes.AdjustmentNote:
					case TransactionTypes.Payment:
					case TransactionTypes.Receipt:
						attachmentCount = 1;
						break;
				}
			}
			else if (Transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				switch (Transaction.AH_TransactionType)
				{
					case TransactionTypes.Payment:
					case TransactionTypes.Receipt:
						attachmentCount = 1;
						break;
				}
			}
			else if (Transaction.AH_Ledger == LedgerTypes.CashBook)
			{
				switch (Transaction.AH_TransactionType)
				{
					case TransactionTypes.DirectPayment:
					case TransactionTypes.DirectReceipt:
					case TransactionTypes.Transfer:
						attachmentCount = 1;
						break;
				}
			}

			return attachmentCount;
		}
	}
}
