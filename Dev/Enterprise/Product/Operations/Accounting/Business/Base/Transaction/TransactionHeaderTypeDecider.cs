using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using TransactionTypes = Enterprise.ZArchitecture.Core.TransactionTypes;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string ledger = row[TransactionHeader.Schema.AH_Ledger].ToString();
			string transactionType = row[TransactionHeader.Schema.AH_TransactionType].ToString();
			string transactionCount = row[TransactionHeader.Schema.AH_TransactionCount].ToString();

			try
			{
				switch (ledger)
				{
					case LedgerTypes.AccountsPayable:
					case LedgerTypes.IncompleteTransactions:
						switch (transactionType)
						{
							case TransactionTypes.Invoice:
							case TransactionTypes.IncompleteInvoice:
								return typeof(APInvoice);
							case TransactionTypes.CreditNote:
							case TransactionTypes.IncompleteCreditNote:
								return typeof(APCreditNote);
							case TransactionTypes.AdjustmentNote:
							case TransactionTypes.IncompleteAdjustmentNote:
								return typeof(APAdjustmentNote);
							case TransactionTypes.Contra:
								return typeof(APContraRow);
							case TransactionTypes.Journal:
								return typeof(APJournal);
							case TransactionTypes.Payment:
								return typeof(APPayment);
							case TransactionTypes.Receipt:
								return typeof(APReceipt);
							case TransactionTypes.Transfer:
								switch (transactionCount)
								{
									case "1":
										return typeof(APTransferFromRow);
									case "2":
										return typeof(APTransferToRow);
									case "3":
										return typeof(APTransferFromRow);
									case "4":
										return typeof(APTransferToRow);
									default:
										throw new ArgumentException(GetInvalidTransactionCountMessage(ledger, transactionType, transactionCount));
								}
							case TransactionTypes.Discount:
								return typeof(APDiscount);
							case TransactionTypes.Overpayment:
								return typeof(APOverpayment);
							case TransactionTypes.ExchangeDifference:
								return typeof(APExchangeDifference);
							default:
								throw new ArgumentException(GetInvalidTransactionTypeMessage(ledger, transactionType));
						}
					case LedgerTypes.AccountsReceivable:
						switch (transactionType)
						{
							case TransactionTypes.Invoice:
								return typeof(ARInvoice);
							case TransactionTypes.CreditNote:
								return typeof(ARCreditNote);
							case TransactionTypes.AdjustmentNote:
								return typeof(ARAdjustmentNote);
							case TransactionTypes.Contra:
								return typeof(ARContraRow);
							case TransactionTypes.Journal:
								return typeof(ARJournal);
							case TransactionTypes.Payment:
								return typeof(ARPayment);
							case TransactionTypes.Receipt:
								return typeof(ARReceipt);
							case TransactionTypes.Transfer:
								switch (transactionCount)
								{
									case "1":
										return typeof(ARTransferFromRow);
									case "2":
										return typeof(ARTransferToRow);
									case "3":
										return typeof(ARTransferFromRow);
									case "4":
										return typeof(ARTransferToRow);
									default:
										throw new ArgumentException(GetInvalidTransactionCountMessage(ledger, transactionType, transactionCount));
								}
							case TransactionTypes.Discount:
								return typeof(ARDiscount);
							case TransactionTypes.Overpayment:
								return typeof(AROverpayment);
							case TransactionTypes.ExchangeDifference:
								return typeof(ARExchangeDifference);
							case TransactionTypes.InvoiceBatch:
								return typeof(InvoiceBatchHeader);
							default:
								throw new ArgumentException(GetInvalidTransactionTypeMessage(ledger, transactionType));
						}

					case LedgerTypes.CashBook:
						switch (transactionType)
						{
							case TransactionTypes.ReceiptBatch:
								return typeof(CashBook.DepositBatch.DepositBatch);
							case TransactionTypes.DirectPayment:
								return (transactionCount == "3" || transactionCount == "6") ? typeof(BankTransferCharge) : typeof(DirectPayment);
							case TransactionTypes.DirectReceipt:
								return typeof(CashBook.DirectReceipt.DirectReceipt);
							case TransactionTypes.Receipt:
							case TransactionTypes.OpeningReceipt:
								return typeof(OpeningReceipt);
							case TransactionTypes.Payment:
							case TransactionTypes.OpeningPayment:
								return typeof(OpeningPayment);
							case TransactionTypes.Transfer:
								if (transactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRow.ToString() ||
									transactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing.ToString())
								{
									return typeof(BankTransferFromRow);
								}
								else if (transactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferToRow.ToString() ||
										 transactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferToRowWhenReversing.ToString())
								{
									return typeof(BankTransferToRow);
								}
								else
								{
									throw new ArgumentException(GetInvalidTransactionCountMessage(ledger, transactionType, transactionCount));
								}
							case TransactionTypes.ExchangeDifference:
								return typeof(CashbookExchangeDiff);
							case TransactionTypes.DDRBatch:
								return typeof(DirectDebitBatchHeader);
							default:
								throw new ArgumentException(GetInvalidTransactionTypeMessage(ledger, transactionType));
						}

					case LedgerTypes.JobCosting:
						switch (transactionType)
						{
							case TransactionTypes.Journal:
								return typeof(JCJournalHeader);
							case TransactionTypes.JobRevenueJournal:
								return typeof(JobRevenueJournal);
							default:
								throw new ArgumentException(GetInvalidTransactionTypeMessage(ledger, transactionType));
						}
					case LedgerTypes.General:
						return GLJournal.TypeDecider.GetTypeForLoad(row, factory);

					case LedgerTypes.UnapprovedPayableTransactions:
						switch (transactionType)
						{
							case TransactionTypes.UAInvoice:
								return typeof(UAInvoice);
							case TransactionTypes.UACreditNote:
								return typeof(UACreditNote);
							default:
								throw new ArgumentException(GetInvalidTransactionTypeMessage(ledger, transactionType));
						}
					case LedgerTypes.TransactionsPendingAllocation:
						return typeof(TransactionPendingAllocation);

					default:
						throw new ArgumentException(string.Format("Ledger '{0}' is not valid. Ledger must be AR, AP, CB, JC or PA", ledger));
				}
			}
			catch (ArgumentException ex)
			{
				ErrorReporter.ReportOnce("TransactionHeaderTypeDecider", string.Format("Problem Transaction PK '{0}'.", row[TransactionHeader.Schema.PK]), ex);
				throw;
			}
		}

		public override Type GetTypeForNew()
		{
			throw new NoConcreteTypeException("New Transaction type cannot be determined");
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		string GetInvalidTransactionTypeMessage(string ledger, string transactionType)
		{
			return string.Format((NoResString)"Transaction Type '{0}' is not a valid {1} Transaction Type", transactionType, ledger);
		}

		string GetInvalidTransactionCountMessage(string ledger, string transactionType, string transactionCount)
		{
			return string.Format(string.Format((NoResString)"Transaction Count {0} is not a valid {1} {2} Type", transactionCount, ledger, transactionType));
		}
	}
}
