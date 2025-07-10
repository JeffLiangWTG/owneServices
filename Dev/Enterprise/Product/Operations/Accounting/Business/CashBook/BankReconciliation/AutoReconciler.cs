using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook
{
	public partial class AutoReconciler
	{
		public AutoReconciler()
		{
		}

		public void Match(StatementCollection bankStatementsToMatch, BankReconTransCollection reconciliationsToMatch)
		{
			Match(bankStatementsToMatch, reconciliationsToMatch, false);
		}

		public void Match(StatementCollection bankStatementsToMatch, BankReconTransCollection reconciliationsToMatch, bool ignoreRef)
		{
			if (bankStatementsToMatch != null && reconciliationsToMatch != null)
			{
				foreach (Statement statementToMatch in bankStatementsToMatch)
				{
					if (!statementToMatch.IsCleared)
					{
						MatchStatement(statementToMatch, bankStatementsToMatch, reconciliationsToMatch, ignoreRef);
					}
				}
			}
		}

		public void Unmatch(MergedTransactionCollection transactions, IBankReconMergedTransaction matchedTransaction)
		{
			if (matchedTransaction is Statement)
			{
				foreach (IBankReconMergedTransaction transactionToCheck in transactions)
				{
					if (transactionToCheck is BankReconTransaction && transactionToCheck.IsCleared && DoStatementAndReconcilationMatch(matchedTransaction as Statement, transactionToCheck as BankReconTransaction, false))
					{
						transactionToCheck.IsCleared = false;
					}
				}
			}
			else if (matchedTransaction is BankReconTransaction)
			{
				foreach (IBankReconMergedTransaction transactionToCheck in transactions)
				{
					if (transactionToCheck is Statement && transactionToCheck.IsCleared && DoStatementAndReconcilationMatch(transactionToCheck as Statement, matchedTransaction as BankReconTransaction, false))
					{
						transactionToCheck.IsCleared = false;
					}
				}
			}
		}

		#region Implementation

		protected void MatchStatement(Statement statementToMatch, StatementCollection bankStatementsToMatch, BankReconTransCollection transactionsToMatch, bool ignoreRef)
		{
			List<Statement> matchedStatements = new List<Statement>();
			List<BankReconTransaction> matchedTransactions = new List<BankReconTransaction>();

			// TODO Simple linear search. May want to change it to more efficient one.
			foreach (BankReconTransaction transaction in transactionsToMatch)
			{
				if (!transaction.IsCleared && DoStatementAndReconcilationMatch(statementToMatch, transaction, ignoreRef))
				{
					if (!matchedStatements.Contains(statementToMatch))
					{
						matchedStatements.Add(statementToMatch);
					}
					matchedTransactions.Add(transaction);
				}
			}

			if (matchedTransactions.Count > 0)
			{
				foreach (BankReconTransaction transaction in matchedTransactions)
				{
					foreach (Statement statement in bankStatementsToMatch)
					{
						if (!statement.IsCleared && !matchedStatements.Contains(statement) && DoStatementAndReconcilationMatch(statement, transaction, ignoreRef))
						{
							matchedStatements.Add(statement);
						}
					}
				}

				if (matchedStatements.Count == matchedTransactions.Count)
				{
					foreach (Statement statement in matchedStatements)
					{
						statement.AS_IsCleared = true;
					}
					foreach (BankReconTransaction transaction in matchedTransactions)
					{
						transaction.IsCleared = true;
					}
				}
			}
		}

		protected bool DoStatementAndReconcilationMatch(Statement statementToMatch, BankReconTransaction bankRecon, bool ignoreRef)
		{
			return DoesTypeMatch(statementToMatch.AS_Type, bankRecon.AH_TransactionType, bankRecon.AH_Ledger, bankRecon.AH_ReceiptType) &&
				DoesRefMatch(statementToMatch, bankRecon, ignoreRef) &&
				DoesAmountMatch(statementToMatch, bankRecon) &&
				DoesDateMatch(statementToMatch, bankRecon);
		}

		protected bool DoesTypeMatch(ZString statementType, ZString transactionType, ZString ledger, ZString receiptType)
		{
			switch (statementType)
			{
				case "CHQ":
					return ((IsPAY_DPY(transactionType) && receiptType == ReceiptTypes.Cheque) || (transactionType == TransactionTypes.OpeningPayment));
				case "RCB":
				case ReceiptTypes.MiscellaneousReceipt:
					return ((transactionType == TransactionTypes.ReceiptBatch) || (transactionType == TransactionTypes.OpeningReceipt));
				case "TRF":
					return ((transactionType == TransactionTypes.Transfer) && (ledger == LedgerTypes.CashBook));
				case ReceiptTypes.DirectDebit:
					return ((transactionType == TransactionTypes.DDRBatch) && (ledger == LedgerTypes.CashBook)) ||
						(IsPAY_DPY(transactionType) && (receiptType == ReceiptTypes.DirectDebit));
				case ReceiptTypes.EFT:
				case ReceiptTypes.ScheduledEFT:
				case ReceiptTypes.CollectionRequest:
					return (IsPAY_DPY(transactionType) && (receiptType == ReceiptTypes.EFT || receiptType == ReceiptTypes.ScheduledEFT || receiptType == ReceiptTypes.CollectionRequest));
				case ReceiptTypes.CreditCard:
					return (IsPAY_DPY(transactionType) && (receiptType == ReceiptTypes.CreditCard));
				case ReceiptTypes.AccountMaintenanceFee:
				case ReceiptTypes.BankDebitTax:
				case ReceiptTypes.BankDepositFee:
				case ReceiptTypes.InterestPaid:
				case ReceiptTypes.InterestReceived:
				case ReceiptTypes.PeriodicPayment:
				case ReceiptTypes.StampDuty:
				case ReceiptTypes.MiscellaneousFees:
					return (statementType == receiptType);
				default:
					return false;
			}
		}

		protected bool IsPAY_DPY(ZString transactionType)
		{
			return (transactionType == TransactionTypes.Payment) || (transactionType == TransactionTypes.DirectPayment);
		}

		protected bool DoesRefMatch(Statement statementToMatch, BankReconTransaction recocilationToMatch, bool ignoreRef)
		{
			bool result = true;

			if (!ignoreRef && statementToMatch.AS_Type == ReceiptTypes.Cheque)
			{
				result = DoesRefMatchCore(recocilationToMatch.AH_ChequeOrReference, statementToMatch.AS_ChequeOrReference);
			}

			return result;
		}

		protected bool DoesRefMatchCore(ZString ref1, ZString ref2)
		{
			return ref1.TrimStart(' ', '0') == ref2.TrimStart(' ', '0');
		}

		protected bool DoesAmountMatch(Statement statementToMatch, BankReconTransaction recocilationToMatch)
		{
			bool result = false;

			if (statementToMatch.AS_DebitCredit == "DR")
			{
				result = recocilationToMatch.Credit == statementToMatch.StatementDebit;
			}
			else
			{
				result = recocilationToMatch.Debit == statementToMatch.StatementCredit;
			}

			return result;
		}

		protected bool DoesDateMatch(Statement statementToMatch, BankReconTransaction recocilationToMatch)
		{
			bool result = true;

			if (statementToMatch.AS_Type != ReceiptTypes.Cheque)
			{
				result = statementToMatch.AS_StatementDate.Date == recocilationToMatch.AH_InvoiceDate.Date ||
					statementToMatch.AS_StatementDate.Date == recocilationToMatch.AH_PostDate.Date;
			}

			return result;
		}

		#endregion
	}
}
