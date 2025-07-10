using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherTypeLookUp
	{
		protected AccTransactionHeader Transaction;
		protected ZString Separator = "-";

		public VoucherTypeLookUp(AccTransactionHeader transaction)
		{
			this.Transaction = transaction;
		}

		public ZString GetVoucherType()
		{
			ZString voucherType = "";

			switch (Transaction.AH_Ledger)
			{
				case LedgerTypes.AccountsPayable:
					voucherType = ChineseLedger.AccountPayable;
					break;
				case LedgerTypes.AccountsReceivable:
					voucherType = ChineseLedger.AccountReceivable;
					break;
				case LedgerTypes.CashBook:
					voucherType = ChineseLedger.Cashbook;
					break;
				case LedgerTypes.General:
					voucherType = ChineseLedger.GeneralLedger;
					break;
				case LedgerTypes.JobCosting:
					voucherType = ChineseLedger.JobCosting;
					break;
			}

			switch (Transaction.AH_TransactionType)
			{
				case TransactionTypes.AdjustmentNote:
					voucherType += Separator + VoucherTypes.Adjustment;
					break;
				case TransactionTypes.Invoice:
					voucherType += Separator + VoucherTypes.Invoice;
					break;
				case TransactionTypes.CreditNote:
					voucherType += Separator + VoucherTypes.CreditNote;
					break;
				case TransactionTypes.Receipt:
					voucherType += Separator + VoucherTypes.Receipt;
					break;
				case TransactionTypes.Payment:
					voucherType += Separator + VoucherTypes.Payment;
					break;
				case TransactionTypes.Journal:
					voucherType += Separator + VoucherTypes.Journal;
					break;
				case TransactionTypes.Transfer:
					voucherType += Separator + VoucherTypes.Transfer;
					break;
				case TransactionTypes.Contra:
					voucherType = VoucherTypes.Contra;
					break;
				case TransactionTypes.Discount:
					voucherType += Separator + VoucherTypes.Discount;
					break;
				case TransactionTypes.Overpayment:
					voucherType += Separator + VoucherTypes.Overpayment;
					break;
				case TransactionTypes.ExchangeDifference:
					voucherType += Separator + VoucherTypes.ExchangeDifference;
					break;
				case TransactionTypes.DirectPayment:
					voucherType += Separator + VoucherTypes.DirectPayment;
					break;
				case TransactionTypes.DirectReceipt:
					voucherType += Separator + VoucherTypes.DirectReceipt;
					break;
				case TransactionTypes.GLStandardJournal:
					voucherType += Separator + VoucherTypes.GeneralJournal;
					break;
				case TransactionTypes.JobRevenueJournal:
					voucherType += Separator + VoucherTypes.JobRevenueJournal;
					break;
			}

			return voucherType;
		}
	}
}
