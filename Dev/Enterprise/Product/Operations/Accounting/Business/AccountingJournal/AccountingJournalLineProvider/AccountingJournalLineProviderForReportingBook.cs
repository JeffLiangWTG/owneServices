using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalLineProviderForReportingBook : AccountingJournalLineProvider
	{
		public AccountingJournalLineProviderForReportingBook(ReadOnlyBusinessObjectFactory factory, TransactionHeader transaction, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes, AccReportingBook reportingBook, DataTable generalLedgerTransactionData)
			: base(factory, validLedgerTypes, validTransactionTypes)
		{
			this.transaction = transaction;
			this.reportingBook = reportingBook;
			this.generalLedgerTransactionData = generalLedgerTransactionData;
		}

		public AccountingJournalLineProviderForReportingBook(TransactionLine transactionLine, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes, AccReportingBook reportingBook, DataTable generalLedgerTransactionData)
			: base(transactionLine.Factory as ReadOnlyBusinessObjectFactory, validLedgerTypes, validTransactionTypes)
		{
			this.transactionLine = transactionLine;
			this.reportingBook = reportingBook;
			this.generalLedgerTransactionData = generalLedgerTransactionData;
		}

		readonly TransactionHeader transaction;
		readonly TransactionLine transactionLine;
		readonly AccReportingBook reportingBook;
		readonly DataTable generalLedgerTransactionData;

		public override IEnumerable<IAccountingJournalLine> GetAccountingJournalLines()
		{
			var result = new List<AccountingJournalLine>();
			if (transaction != null)
			{
				if (transaction is TransactionHeaderWithLines transactionHeaderWithLines)
				{
					if (validLedgerTypes.Contains(transaction.AH_Ledger) && validTransactionTypes.Contains(transaction.AH_TransactionType))
					{
						if (generalLedgerTransactionData != null)
						{
							var rows = generalLedgerTransactionData.Select($"TransactionHeaderID = '{transaction.PK.ToGuid()}' and TaxGLMovementKey is null");
							foreach (var row in rows)
							{
								var line = Factory.Load<AccTransactionLines>((Guid)row["TransactionLineID"]);
								SetMultiSubAccountTypeCode(transaction, line, row);
								line.AL_Desc = transaction.AH_Ledger != LedgerTypes.General ? ZString.Empty : line.AL_Desc;
								result.Add(new AccountingJournalLineForReportingBook(row, reportingBook, line));
							}
						}
					}
					else
					{
						throw new InvalidAccountingJournalOperationException(GetErrorMessageForMissingTransactionType(transaction.AH_Ledger, transaction.AH_TransactionType));
					}
				}
				else
				{
					if (validLedgerTypes.Contains(transaction.AH_Ledger) && validTransactionTypes.Contains(transaction.AH_TransactionType))
					{
						var line = Factory.New<AccTransactionLines>();

						if (transaction.AH_TransactionType == TransactionTypes.Transfer)
						{
							line.AL_Desc = transaction.AH_Ledger == LedgerTypes.AccountsReceivable || transaction.AH_Ledger == LedgerTypes.AccountsPayable ? transaction.AH_Desc : line.AL_Desc;
							if (generalLedgerTransactionData != null)
							{
								var oppositeTransactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, transaction.AH_Ledger);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transaction.AH_TransactionType);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
								var oppositeTransaction = Factory.LoadTop1<TransactionHeader>(oppositeTransactionQuery);
								var rows = generalLedgerTransactionData.Select($"TransactionHeaderID = '{transaction.PK.ToGuid()}' or TransactionHeaderID = '{oppositeTransaction.PK.ToGuid()}'");
								foreach (var row in rows)
								{
									result.Add(new AccountingJournalLineForReportingBook(row, reportingBook, line));
								}
							}
						}
						else if ((transaction.AH_Ledger == LedgerTypes.AccountsReceivable || transaction.AH_Ledger == LedgerTypes.AccountsPayable)
								&& (transaction.AH_TransactionType == TransactionTypes.Contra))
						{
							if (generalLedgerTransactionData != null)
							{
								var oppositeTransactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transaction.AH_TransactionType);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transaction.AH_TransactionBelongsToGroup);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
								oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
								var oppositeTransaction = Factory.LoadTop1<TransactionHeader>(oppositeTransactionQuery);
								var rows = generalLedgerTransactionData.Select($"TransactionHeaderID = '{transaction.PK.ToGuid()}' or TransactionHeaderID = '{oppositeTransaction.PK.ToGuid()}'");
								foreach (var row in rows)
								{
									result.Add(new AccountingJournalLineForReportingBook(row, reportingBook, line));
								}
							}
						}
						else
						{
							if (generalLedgerTransactionData != null)
							{
								var rows = generalLedgerTransactionData.Select($"TransactionHeaderID = '{transaction.PK.ToGuid()}' and TaxGLMovementKey is null");
								foreach (var row in rows)
								{
									result.Add(new AccountingJournalLineForReportingBook(row, reportingBook, line));
								}
							}
						}
					}
					else
					{
						throw new InvalidAccountingJournalOperationException(GetErrorMessageForMissingTransactionType(transaction.AH_Ledger, transaction.AH_TransactionType));
					}
				}
			}
			else if (transactionLine != null)
			{
				if (validTransactionTypes.Contains(this.transactionLine.AL_LineType))
				{
					if (generalLedgerTransactionData != null)
					{
						var rows = generalLedgerTransactionData.Select($"TransactionLineID = '{transactionLine.PK.ToGuid()}'");
						foreach (DataRow row in rows)
						{
							if ((decimal)row["GLAmountLocalBalance"] != 0)
							{
								transactionLine.AL_Desc = ZString.Empty;
								result.Add(new AccountingJournalLineForReportingBook(row, reportingBook, transactionLine));
							}
						}
					}
				}
				else
				{
					throw new InvalidAccountingJournalOperationException(Res.GetString("c6ca6dda-57c9-4943-8696-fc8391378388", "Printing of Accounting Journal is not supported for {0} Line", this.transactionLine.AL_LineType));
				}
			}

			return result;
		}

		void SetMultiSubAccountTypeCode(TransactionHeader transaction, AccTransactionLines transactionLine, DataRow row)
		{
			if (transaction.AH_Ledger == LedgerTypes.AccountsReceivable || transaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				if (((transaction.AH_TransactionType == TransactionTypes.Invoice || transaction.AH_TransactionType == TransactionTypes.CreditNote || transaction.AH_TransactionType == TransactionTypes.AdjustmentNote) && !transactionLine.AL_ReverseDate.IsEmpty && row["GLAccountType"]?.ToString() == "TLG"))
				{
					transactionLine.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(transactionLine as ISupportMultiSubAccounts, transactionLine.Factory);
				}
				else if ((transaction.AH_TransactionType == TransactionTypes.Journal && row["GLAccountType"]?.ToString() == "THG"))
				{
					transactionLine.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(transactionLine as ISupportMultiSubAccounts, transactionLine.Factory);
				}
			}
			else if (transaction.AH_Ledger == LedgerTypes.CashBook && transaction.AH_TransactionType == TransactionTypes.DirectPayment || transaction.AH_TransactionType == TransactionTypes.DirectReceipt)
			{
				transactionLine.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(transactionLine as ISupportMultiSubAccounts, transactionLine.Factory);
			}
			else if (transaction.AH_Ledger == LedgerTypes.General && (transaction.AH_TransactionType == TransactionTypes.GLStandardJournal || transaction.AH_TransactionType == TransactionTypes.GLAutoJournal || transaction.AH_TransactionType == TransactionTypes.GLReversingJournal))
			{
				transactionLine.MultiSubAccountTypeCode = SubAccountHelper.GetMultiSubAccountTypeCode(transactionLine as ISupportMultiSubAccounts, transactionLine.Factory);
			}
		}
	}
}
