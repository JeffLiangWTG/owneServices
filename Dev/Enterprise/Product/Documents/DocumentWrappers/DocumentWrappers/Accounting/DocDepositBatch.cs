using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.MasterFiles.Business;
using ReceiptTypes = Enterprise.ZArchitecture.Core.ReceiptTypes;

namespace Enterprise.DocumentWrappers
{
	public class DocDepositBatch : DocTransactionHeader
	{
		protected DocDepositBatch(DepositBatch depositBatch, BusinessObjectFactory factoryToWrap)
			: base(depositBatch, factoryToWrap)
		{
		}

		public static DocDepositBatch New(DepositBatch depositBatch, BusinessObjectFactory factoryToWrap)
		{
			if (depositBatch == null)
			{
				return null;
			}
			else
			{
				return new DocDepositBatch(depositBatch, factoryToWrap);
			}
		}

		public override string ToString()
		{
			return DepositBatchNum;
		}

		public ZString DepositBatchNum
		{
			get { return DepositBatch.AH_TransactionNum; }
		}

		public DocBankAccount Account
		{
			get
			{
				AccBankAccount bankAccount = Factory.Load<AccBankAccount>(DepositBatch.AH_AB);
				return DocBankAccount.New(bankAccount, Factory);
			}
		}

		protected override DocBankAccount GetAccountCore()
		{
			return Account;
		}

		public ZDateTime BatchDate
		{
			get { return DepositBatch.AH_InvoiceDate; }
		}
		#region Calculated Fields

		public ZInt TotalChequeCount
		{
			get
			{
				ZInt result = 0;

				foreach (DocTransactionHeader header in TransactionHeaders)
				{
					if (header.ReceiptType == ReceiptTypes.Cheque)
					{
						result++;
					}
				}

				return result;
			}
		}

		protected override ZInt GetTotalChequeCountCore()
		{
			return TotalChequeCount;
		}

		public ZDecimal TotalCashAmount
		{
			get { return GetReceiptAmount(TransactionHeaders, ReceiptTypes.Cash); }
		}

		protected override ZDecimal GetTotalCashAmountCore()
		{
			return TotalCashAmount;
		}

		public ZDecimal TotalCreditCardAmount
		{
			get { return GetReceiptAmount(TransactionHeaders, ReceiptTypes.CreditCard); }
		}

		protected override ZDecimal GetTotalCreditCardAmountCore()
		{
			return TotalCreditCardAmount;
		}

		public ZDecimal TotalChequeAmount
		{
			get { return GetReceiptAmount(TransactionHeaders, ReceiptTypes.Cheque); }
		}

		protected override ZDecimal GetTotalChequeAmountCore()
		{
			return TotalChequeAmount;
		}

		public ZDecimal TotalDirectCreditAmount
		{
			get { return GetReceiptAmount(DirectCreditTransactions, ""); }
		}

		protected override ZDecimal GetTotalDirectCreditAmountCore()
		{
			return TotalDirectCreditAmount;
		}

		protected override ZDecimal GetDepositBatchTotalAmountCoreForBank()
		{
			return TotalCashAmount + TotalCreditCardAmount + TotalChequeAmount;
		}

		protected override ZDecimal GetDepositBatchTotalAmountCore()
		{
			return TotalChequeAmount + TotalCashAmount + TotalCreditCardAmount + TotalDirectCreditAmount;
		}

		#endregion

		#region Collections

		public DocTransactionHeaderCollection TransactionHeaders
		{
			get
			{
				ZInt number = 0;
				DocTransactionHeaderCollection headerCollection = new DocTransactionHeaderCollection(DepositBatch.Factory);
				foreach (TransactionHeader header in DepositBatch.Transactions)
				{
					if (header.Branch != null && header.Branch.Company != null && header.Branch.Company.PK == GlbCompany.CurrentCompany.PK)
					{
						DocTransactionHeader docTransactionHeader = DocTransactionHeader.New(header, Factory);
						headerCollection.Add(docTransactionHeader);
						docTransactionHeader.CalcLineNumber = ++number;
					}
				}
				return headerCollection;
			}
		}

		public DocTransactionHeaderCollection CashTransactions
		{
			get
			{
				ZInt number = 0;
				DocTransactionHeaderCollection cashTransactionCollection = new DocTransactionHeaderCollection(DepositBatch.Factory);
				foreach (DocTransactionHeader transaction in TransactionHeaders)
				{
					if (transaction.ReceiptType == ReceiptTypes.Cash)
					{
						cashTransactionCollection.Add(transaction);
						transaction.CalcLineNumber = ++number;
					}
				}
				return cashTransactionCollection;
			}
		}

		protected override DocTransactionHeaderCollection GetCashTransactionsCore()
		{
			return CashTransactions;
		}

		public DocTransactionHeaderCollection CreditCardTransactions
		{
			get
			{
				ZInt number = 0;
				DocTransactionHeaderCollection creditCardTransactionCollection = new DocTransactionHeaderCollection(DepositBatch.Factory);
				foreach (DocTransactionHeader transaction in TransactionHeaders)
				{
					if (transaction.ReceiptType == ReceiptTypes.CreditCard)
					{
						creditCardTransactionCollection.Add(transaction);
						transaction.CalcLineNumber = ++number;
					}
				}
				return creditCardTransactionCollection;
			}
		}

		protected override DocTransactionHeaderCollection GetCreditCardTransactionsCore()
		{
			return CreditCardTransactions;
		}

		public DocTransactionHeaderCollection ChequeTransactions
		{
			get
			{
				ZInt number = 0;
				DocTransactionHeaderCollection chequeTransactionCollection = new DocTransactionHeaderCollection(DepositBatch.Factory);
				foreach (DocTransactionHeader transaction in TransactionHeaders)
				{
					if (transaction.ReceiptType == ReceiptTypes.Cheque)
					{
						chequeTransactionCollection.Add(transaction);
						transaction.CalcLineNumber = ++number;
					}
				}
				return chequeTransactionCollection;
			}
		}

		protected override DocTransactionHeaderCollection GetChequeTransactionsCore()
		{
			return ChequeTransactions;
		}

		public DocTransactionHeaderCollection DirectCreditTransactions
		{
			get
			{
				DocTransactionHeaderCollection dCRTransactions = new DocTransactionHeaderCollection(DepositBatch.Factory);
				foreach (DocTransactionHeader receipts in TransactionHeaders)
				{
					if (receipts.ReceiptType == ReceiptTypes.DirectCredit ||
						(receipts.ReceiptType != ReceiptTypes.Cash &&
						receipts.ReceiptType != ReceiptTypes.CreditCard &&
						receipts.ReceiptType != ReceiptTypes.Cheque))
					{
						dCRTransactions.Add(receipts);
					}
				}
				return dCRTransactions;
			}
		}

		protected override DocTransactionHeaderCollection GetDirectCreditTransactionsCore()
		{
			return DirectCreditTransactions;
		}

		#endregion

		#region Implementation

		protected ZDecimal GetReceiptAmount(DocTransactionHeaderCollection transactionHeaders, ZString receiptType)
		{
			ZDecimal result = 0M;

			foreach (DocTransactionHeader header in transactionHeaders)
			{
				if (receiptType.IsEmpty || header.ReceiptType == receiptType)
				{
					if (DepositBatch.CurrencyCode == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						result += header.TotalLocalInvoiceAmount;
					}
					else
					{
						result += header.OSTotal;
					}
				}
			}

			return result;
		}

		DepositBatch DepositBatch
		{
			get { return (DepositBatch)WrappedObject; }
		}

		#endregion
	}
}
