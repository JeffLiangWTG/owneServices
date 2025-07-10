using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class DocStatementForAccountMovement : DocStatement
	{
		public DocStatementForAccountMovement(PrintStatementForAccountMovement printStatement, BusinessObjectFactory factoryToWrap)
			: base(printStatement, factoryToWrap)
		{
		}

		PrintStatementForAccountMovement PrintStatementForAccMovement
		{
			get { return (PrintStatementForAccountMovement)PrintStatement; }
		}

		public override DocTransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					TransactionHeaderCollection transactionHeaders = PrintStatement.Transactions;
					fTransactions = new DocTransactionHeaderCollection(PrintStatement.Factory);
					foreach (AccountMovement currentTransaction in transactionHeaders)
					{
						if (currentTransaction.AH_TransactionType != TransactionTypes.InvoiceBatch)
						{
							DocTransactionHeader transactionWrapper = DocAccountMovement.New(currentTransaction, PrintStatement.Factory);
							transactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = PrintStatement.EndOfPeriod;
							fTransactions.Add(transactionWrapper);
						}
					}
				}
				return fTransactions;
			}
		}

		public override DocTransactionHeaderCollection TransactionsForDueBuckets
		{
			get
			{
				if (fTransactionsForDueBuckets == null)
				{
					TransactionHeaderCollection transactionHeaders = (PrintStatement as PrintStatementForAccountMovement).TransactionForDueBuckets;
					fTransactionsForDueBuckets = new DocTransactionHeaderCollection(PrintStatement.Factory);
					foreach (AccountMovement currentTransaction in transactionHeaders)
					{
						if (currentTransaction.AH_TransactionType != TransactionTypes.InvoiceBatch)
						{
							DocTransactionHeader transactionWrapper = DocAccountMovement.New(currentTransaction, PrintStatement.Factory);
							transactionWrapper.EndOfStatementPeriodForCalculatingMatchedAmount = PrintStatement.EndOfPeriod;
							fTransactionsForDueBuckets.Add(transactionWrapper);
						}
					}
				}
				return fTransactionsForDueBuckets;
			}
		}
		DocTransactionHeaderCollection fTransactionsForDueBuckets;

		public override ZString AccountMovementListingGroupBy
		{
			get { return PrintStatementForAccMovement.GroupBy; }
		}

		public override ZDateTime OpeningBalanceDate
		{
			get
			{
				return PrintStatementForAccMovement.PostDateFrom;
			}
		}

		public override ZDateTime ClosingBalanceDate
		{
			get
			{
				return PrintStatementForAccMovement.PostDateTo;
			}
		}

		public override ZDecimal OpeningBalance
		{
			get
			{
				return PrintStatementForAccMovement.OpeningBalance;
			}
		}

		public override ZDecimal ClosingBalance
		{
			get
			{
				return PrintStatementForAccMovement.ClosingBalance;
			}
		}

		public override ZDecimal StatementBalance
		{
			get
			{
				ZDecimal total = 0M;

				if (TransactionsForDueBuckets != null)
				{
					foreach (DocTransactionHeader header in TransactionsForDueBuckets)
					{
						total += header.Balance;
					}
				}

				return total;
			}
		}

		public override ZString TotalCurrentAmount
		{
			get
			{
				ZString result = ZString.Empty;
				if (TotalCurrentDecimal != 0M)
				{
					result = FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(TotalCurrentDecimal, Currency);
				}
				return result;
			}
		}

		protected override ZDecimal CalculateTotalOverdue()
		{
			ZDecimal totalOverdue = 0M;
			if (TransactionsForDueBuckets != null)
			{
				foreach (DocTransactionHeader transaction in TransactionsForDueBuckets)
				{
					if (transaction.DueDate < PrintStatementForAccMovement.PostDateTo)
					{
						totalOverdue += transaction.Balance;
					}
				}
			}
			return totalOverdue;
		}

		protected override ZDecimal GetTotalDueAmnt()
		{
			ZDecimal totalDue = 0M;
			if (TransactionsForDueBuckets != null)
			{
				foreach (DocTransactionHeader transaction in TransactionsForDueBuckets)
				{
					if (transaction.DueDate >= PrintStatementForAccMovement.PostDateTo)
					{
						totalDue += transaction.Balance;
					}
				}
			}
			return totalDue;
		}

		protected override ZDecimal GetTotalStmntAmnt()
		{
			ZDecimal totalDue = 0M;
			if (TransactionsForDueBuckets != null)
			{
				foreach (DocTransactionHeader transaction in TransactionsForDueBuckets)
				{
					totalDue += transaction.Balance;
				}
			}
			return totalDue;
		}

		protected override ZString GetDueAmountAsFormattedString(bool isAllowed, string dueDateOption, bool isOverdue, int lowerAgeLimit, int upperAgeLimit)
		{
			ZString result = ZString.Empty;
			if (isAllowed)
			{
				result = Res.GetString("eaf19209-f2e5-46bc-b938-4259d9e4cf70", "{0} {1}", FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(GetDueAmount((PrintStatement as PrintStatementForAccountMovement).PostDateTo.ToDateTime(), dueDateOption, isOverdue, lowerAgeLimit, upperAgeLimit, (PrintStatement as PrintStatementForAccountMovement).PostDateTo), Currency), Currency.Code);
			}
			return result;
		}
	}
}
