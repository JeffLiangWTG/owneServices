using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherProviderFactory
	{
		protected BusinessObjectFactory Factory;

		public VoucherProviderFactory(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public static IEnumerable<ZString> SupportList
		{
			get
			{
				yield return TransactionTypes.AdjustmentNote;
				yield return TransactionTypes.Invoice;
				yield return TransactionTypes.CreditNote;
				yield return TransactionTypes.Receipt;
				yield return TransactionTypes.Payment;
				yield return TransactionTypes.Journal;
				yield return TransactionTypes.Transfer;
				yield return TransactionTypes.Contra;
				yield return TransactionTypes.Discount;
				yield return TransactionTypes.Overpayment;
				yield return TransactionTypes.ExchangeDifference;
				yield return TransactionTypes.DirectPayment;
				yield return TransactionTypes.DirectReceipt;
				yield return TransactionTypes.GLStandardJournal;
				yield return TransactionTypes.JobRevenueJournal;
			}
		}

		public VoucherProvider GetProvider(AccTransactionHeader transaction)
		{
			if (transaction == null)
			{
				throw new ArgumentNullException(nameof(transaction));
			}

			VoucherProvider provider = null;

			switch (transaction.AH_TransactionType)
			{
				case TransactionTypes.AdjustmentNote:
				case TransactionTypes.Invoice:
				case TransactionTypes.CreditNote:
					provider = new InvoiceCreditAdjustmentVoucherProvider(transaction);
					break;
				case TransactionTypes.Receipt:
					provider = new ReceiptPaymentVoucherProvider(transaction);
					break;
				case TransactionTypes.Payment:
					provider = new ReceiptPaymentVoucherProvider(transaction);
					break;
				case TransactionTypes.Journal:
					if (IsARAP(transaction))
					{
						provider = new ARAPJournalVoucherProivder(transaction);
					}
					else if (IsJobCosting(transaction))
					{
						provider = new CFXVoucherProvider(transaction);
					}
					break;
				case TransactionTypes.Transfer:
					if (IsARAP(transaction))
					{
						if (transaction.AH_TransactionCount == 1)
						{
							provider = new ARAPTransferVoucherProvider(transaction);
						}
					}
					else if (IsCB(transaction))
					{
						provider = new CBTransferVoucherProvider(transaction);
					}
					break;
				case TransactionTypes.Contra:
					provider = new ARAPContraVoucherProvider(transaction);
					break;
				case TransactionTypes.Discount:
					provider = new DiscountVoucherProvider(transaction);
					break;
				case TransactionTypes.Overpayment:
					provider = new OverpaymentVoucherProvider(transaction);
					break;
				case TransactionTypes.ExchangeDifference:
					if (IsARAP(transaction))
					{
						provider = new ExchangeDiffVoucherProvider(transaction);
					}
					else if (IsCB(transaction))
					{
						provider = new CBExchangeDiffVoucherProvider(transaction);
					}
					break;
				case TransactionTypes.DirectPayment:
				case TransactionTypes.DirectReceipt:
					provider = new DirectReceiptPaymentVoucher(transaction);
					break;
				case TransactionTypes.GLStandardJournal:
					provider = new GLJournalVoucherProvider(transaction);
					break;
				case TransactionTypes.JobRevenueJournal:
					provider = new CFXVoucherProvider(transaction);
					break;
			}

			return provider;
		}

		#region Implementation

		bool IsARAP(AccTransactionHeader transaction)
		{
			return (transaction.AH_Ledger == LedgerTypes.AccountsPayable || transaction.AH_Ledger == LedgerTypes.AccountsReceivable);
		}

		bool IsCB(AccTransactionHeader transaction)
		{
			return (transaction.AH_Ledger == LedgerTypes.CashBook);
		}

		bool IsJobCosting(AccTransactionHeader transaction)
		{
			return (transaction.AH_Ledger == LedgerTypes.JobCosting);
		}

		#endregion
	}
}
