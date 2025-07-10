using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherDebitCreditLookUp
	{
		readonly AccTransactionLines TransactionLine;
		protected AccTransactionHeader Transaction;

		public VoucherDebitCreditLookUp(AccTransactionHeader transaction)
		{
			this.Transaction = transaction;
		}

		public VoucherDebitCreditLookUp(AccTransactionLines transactionLine)
		{
			this.TransactionLine = transactionLine;
			this.Transaction = transactionLine.TransactionHeader;
		}

		protected int GetDebitCreditMultiplier()
		{
			int debitCredit = 1;

			switch (Transaction.AH_TransactionType)
			{
				case TransactionTypes.AdjustmentNote:
				case TransactionTypes.CreditNote:
				case TransactionTypes.Invoice:
					//					if (Transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
					//					{
					//						DebitCredit = -1;
					//					}
					//					else if (Transaction.AH_Ledger == LedgerTypes.AccountsPayable)
					//					{
					//						DebitCredit = 1;						
					//					}
					debitCredit = -1;
					break;
				case TransactionTypes.Receipt:
					debitCredit = -1;
					break;
				case TransactionTypes.Payment:
					debitCredit = -1;
					break;
				case TransactionTypes.Journal:
					debitCredit = -1;
					break;
				case TransactionTypes.Transfer:
					debitCredit = 1;
					break;
				case TransactionTypes.Contra:
					debitCredit = 1;
					break;
				case TransactionTypes.Discount:
					debitCredit = -1;
					break;
				case TransactionTypes.Overpayment:
					debitCredit = -1;
					break;
				case TransactionTypes.ExchangeDifference:
					if (Transaction.AH_Ledger == LedgerTypes.CashBook)
					{
						debitCredit = 1;
					}
					else
					{
						debitCredit = -1;
					}
					break;
				case TransactionTypes.DirectPayment:
					debitCredit = -1;
					break;
				case TransactionTypes.DirectReceipt:
					debitCredit = -1;
					break;
				case TransactionTypes.GLStandardJournal:
					debitCredit = 1;
					break;
				case TransactionTypes.JobRevenueJournal:
					debitCredit = -1;
					break;
			}

			return debitCredit;
		}

		public ZDecimal GetDebit()
		{
			ZDecimal debitAmount = Amount * GetDebitCreditMultiplier();
			return ReturnPositiveOnly(debitAmount);
		}

		public ZDecimal GetCredit()
		{
			ZDecimal creditAmount = Amount * GetDebitCreditMultiplier();
			return ReturnNegativeOnly(creditAmount);
		}

		public ZDecimal GetOSDebit()
		{
			ZDecimal oSDebitAmount = CurrencyCode == Transaction.Company.GC_RX_NKLocalCurrency ? GetDebit() : (ZDecimal)(OSAmount * GetDebitCreditMultiplier());
			return ReturnPositiveOnly(oSDebitAmount);
		}

		public ZDecimal GetOSCredit()
		{
			ZDecimal oSCreditAmount = CurrencyCode == Transaction.Company.GC_RX_NKLocalCurrency ? GetCredit() : (ZDecimal)(OSAmount * GetDebitCreditMultiplier());
			return ReturnNegativeOnly(oSCreditAmount);
		}

		public ZDecimal GetDebitGST()
		{
			ZDecimal debitAmount = GST * GetDebitCreditMultiplier();
			return ReturnPositiveOnly(debitAmount);
		}

		public ZDecimal GetCreditGST()
		{
			ZDecimal creditAmount = GST * GetDebitCreditMultiplier();
			return ReturnNegativeOnly(creditAmount);
		}

		public ZDecimal GetForeignCurrencyAmount()
		{
			return OSAmount >= 0 ? OSAmount : new ZDecimal(-OSAmount); // Always display as positive
		}

		public ZDecimal GetExchangeRate()
		{
			return ExchangeRate;
		}

		public ZString GetCurrencyCode()
		{
			return !CurrencyCode.IsEmpty ? CurrencyCode : GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		public ZString GetTransactionLineDescription()
		{
			ZString result = ZString.Empty;

			if (TransactionLine != null && !TransactionLine.AL_Desc.IsEmpty)
			{
				result = TransactionLine.AL_Desc;
			}
			else if (Transaction != null && !Transaction.AH_Desc.IsEmpty)
			{
				result = Transaction.AH_Desc;
			}

			return result;
		}

		public ZString GetOrganisationCode()
		{
			ZString result = ZString.Empty;

			// Only return Organisation Code for Header
			if (TransactionLine == null && Transaction != null && Transaction.Header != null)
			{
				result = " (" + Transaction.Header.OH_Code + ")";
			}

			return result;
		}

		public ZString GetJobNumber()
		{
			ZString result = ZString.Empty;

			if (TransactionLine != null && TransactionLine.Job != null)
			{
				result = TransactionLine.Job.JH_JobNum;
			}
			else if (Transaction != null && Transaction.Job != null)
			{
				result = Transaction.Job.JH_JobNum;
			}

			return result;
		}

		public ZString GetInvoiceNumber()
		{
			ZString result = new ZString();

			if (Transaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				result = Transaction.AH_TransactionNum;
			}
			else if (Transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				result = Transaction.AH_ConsolidatedInvoiceRef;
			}

			return result;
		}

		protected ZDecimal Amount
		{
			get
			{
				return TransactionLine != null ? TransactionLine.AL_LineAmount : Transaction.AH_InvoiceAmount;
			}
		}

		protected ZDecimal GST
		{
			get
			{
				return TransactionLine != null ? TransactionLine.AL_GSTVAT : Transaction.AH_GSTAmount;
			}
		}

		protected ZDecimal OSAmount
		{
			get
			{
				ZDecimal result = 0;

				if (TransactionLine != null)
				{
					result = TransactionLine.AL_OSAmount;
				}
				else if (Transaction != null)
				{
					result = Transaction.AH_OSTotal;
				}

				return result;
			}
		}

		protected ZDecimal ExchangeRate
		{
			get
			{
				ZDecimal result = 0;
				ZString currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				if (TransactionLine != null)
				{
					result = TransactionLine.AL_ExchangeRate;
					currency = TransactionLine.Branch.Company.GC_RX_NKLocalCurrency;
				}
				else if (Transaction != null)
				{
					result = Transaction.AH_ExchangeRate;
					currency = Transaction.Company.GC_RX_NKLocalCurrency;
				}

				return CurrencyCode == currency ? 1 : result;
			}
		}

		protected ZString CurrencyCode
		{
			get
			{
				return Currency != null ? Currency.RX_Code : ZString.Empty;
			}
		}

		protected RefCurrency Currency
		{
			get
			{
				RefCurrency result = null;
				if (OSAmount == 0m)
				{
					result = GlbCompany.CurrentCompany.LocalCurrency;
				}
				else if (TransactionLine != null && TransactionLine.TransactionCurrency != null)
				{
					result = TransactionLine.TransactionCurrency;
				}
				else if (Transaction != null && Transaction.TransactionCurrency != null)
				{
					result = Transaction.TransactionCurrency;
				}

				return result;
			}
		}

		public ZDecimal ReturnPositiveOnly(ZDecimal amount)
		{
			return amount > 0m ? amount : (ZDecimal)0m;
		}

		public ZDecimal ReturnNegativeOnly(ZDecimal amount)
		{
			return Math.Abs(amount < 0m ? amount : (ZDecimal)0m);
		}

		public ZString ChargeCode
		{
			get
			{
				return TransactionLine != null && TransactionLine.ChargeCode != null ? TransactionLine.ChargeCode.AC_Code : ZString.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString ChargeCodeDesc
		{
			get
			{
				return TransactionLine != null && TransactionLine.ChargeCode != null ? TransactionLine.ChargeCode.AC_Desc : ZString.Empty;
			}
		}

		public ZString ChargeCodeLocalLangDesc
		{
			get
			{
				return TransactionLine != null && TransactionLine.ChargeCode != null ? TransactionLine.ChargeCode.AC_LocalLanguageDescription : ZString.Empty;
			}
		}
	}
}
