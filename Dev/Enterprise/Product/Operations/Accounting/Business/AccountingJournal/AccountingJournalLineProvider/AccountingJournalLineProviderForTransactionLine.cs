using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalLineProviderForTransactionLine : AccountingJournalLineProvider
	{
		public AccountingJournalLineProviderForTransactionLine(ReadOnlyBusinessObjectFactory factory, TransactionHeaderWithLines transaction, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes)
			: base(factory, validLedgerTypes, validTransactionTypes)
		{
			this.transaction = transaction;
			this.GetAJLines = GetAJLinesForTransactionWithHeader;
		}

		public AccountingJournalLineProviderForTransactionLine(TransactionLine transactionLine, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes)
			: base(transactionLine.Factory as ReadOnlyBusinessObjectFactory, validLedgerTypes, validTransactionTypes)
		{
			this.transactionLine = transactionLine;
			this.GetAJLines = GetAJLinesForTransactionWithoutHeader;
		}

		readonly TransactionHeaderWithLines transaction;
		readonly TransactionLine transactionLine;
		delegate IEnumerable<AccountingJournalLine> AJLinesGetter();
		readonly AJLinesGetter GetAJLines;

		public override IEnumerable<IAccountingJournalLine> GetAccountingJournalLines()
		{
			return GetAJLines();
		}

		IEnumerable<AccountingJournalLine> GetAJLinesForTransactionWithHeader()
		{
			var result = new List<AccountingJournalLine>();
			if (validLedgerTypes.Contains(transaction.AH_Ledger) && validTransactionTypes.Contains(transaction.AH_TransactionType))
			{
				if (transaction.Lines != null)
				{
					foreach (TransactionLine line in transaction.Lines)
					{
						if (line.AL_LineAmount != 0)
						{
							var journalLineCreator = GetLineCreator(line);
							if (journalLineCreator != null)
							{
								result.AddRange(journalLineCreator.CreateAccountJournalLines());
							}
						}
					}
				}
			}
			else
			{
				throw new InvalidAccountingJournalOperationException(GetErrorMessageForMissingTransactionType(transaction.AH_Ledger, transaction.AH_TransactionType));
			}
			return result;
		}

		IEnumerable<AccountingJournalLine> GetAJLinesForTransactionWithoutHeader()
		{
			var result = new List<AccountingJournalLine>();

			if (validTransactionTypes.Contains(this.transactionLine.AL_LineType))
			{
				if (this.transactionLine.AL_LineAmount != 0)
				{
					var journalLineCreator = GetLineCreator(this.transactionLine);
					if (journalLineCreator != null)
					{
						result.AddRange(journalLineCreator.CreateAccountJournalLines());
					}
				}
			}
			else
			{
				throw new InvalidAccountingJournalOperationException(Res.GetString("c6ca6dda-57c9-4943-8696-fc8391378388", "Printing of Accounting Journal is not supported for {0} Line", this.transactionLine.AL_LineType));
			}

			return result;
		}

		AccountingJournalLineCreator GetLineCreator(TransactionLine line)
		{
			AccountingJournalLineCreator creator = null;

			#region Cash Book
			if (transaction != null && transaction.AH_Ledger == LedgerTypes.CashBook && new ZString[] { TransactionTypes.DirectPayment, TransactionTypes.DirectReceipt }.Contains(transaction.AH_TransactionType))
			{
				creator = new DRCDPYAccountingJournalLineCreator(transaction.BankAccount, line, Factory, transaction.AH_GB, transaction.AH_GE);
			}
			#endregion

			#region Journal
			if (transaction != null && transaction.AH_Ledger == LedgerTypes.General)
			{
				creator = GetJournalLineCreator(line);
			}
			#endregion

			#region AR and AP
			else if (transaction != null && (transaction.AH_Ledger == LedgerTypes.AccountsReceivable || transaction.AH_Ledger == LedgerTypes.AccountsPayable)
						&& (transaction.AH_TransactionType == TransactionTypes.Invoice || transaction.AH_TransactionType == TransactionTypes.CreditNote || transaction.AH_TransactionType == TransactionTypes.AdjustmentNote))
			{
				creator = new INVCRDADJAccountingJournalLineCreator(transaction.AH_TransactionType, transaction.AH_Ledger, line, Factory);
			}
			#endregion

			#region JobCosting
			else if (transaction != null && transaction.AH_Ledger == LedgerTypes.JobCosting && (transaction.AH_TransactionType == TransactionTypes.Journal || transaction.AH_TransactionType == TransactionTypes.JobRevenueJournal))
			{
				creator = new JRJJNLAccountingJournalCreator(transaction.AH_TransactionType, line, Factory);
			}
			#endregion

			#region WIPACR
			else if (line.AL_LineType == TransactionLineTypes.WIP || line.AL_LineType == TransactionLineTypes.Accrual)
			{
				creator = new WIPACRAccountingJournalLineCreator(line, Factory);
			}
			#endregion

			if (creator == null)
			{
				throw new InvalidAccountingJournalOperationException(GetErrorMessageForMissingTransactionType(transaction.AH_Ledger, transaction.AH_TransactionType));
			}

			return creator;
		}

		AccountingJournalLineCreator GetJournalLineCreator(TransactionLine line)
		{
			AccountingJournalLineCreator creator = null;
			switch (transaction.AH_TransactionType)
			{
				case TransactionTypes.GLStandardJournal:
					creator = new GJLAccountingJournalLineCreator(line, Factory);
					break;
				case TransactionTypes.GLAutoJournal:
					creator = new AJLAccountingJournalLineCreator(transaction.PostPeriod, transaction.AgePeriod, transaction.PeriodCalculator, line, Factory);
					break;
				case TransactionTypes.GLReversingJournal:
					creator = new RJLAccountingJournalLineCreator(line, Factory);
					break;
				case TransactionTypes.GLNoteJournal:
					creator = new NJLAccountingJournalLineCreator(line, Factory);
					break;
			}
			return creator;
		}
	}
}
