using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class CashBookAccountingJournal : AccountingJournal
	{
		public CashBookAccountingJournal(TransactionHeader transaction, ReadOnlyBusinessObjectFactory factory, AccReportingBook reportingBook = null, DataTable generalLedgerTransactionData = null)
			: base(transaction, factory, reportingBook, generalLedgerTransactionData)
		{
			PopulateJournalLines();
		}

		public override bool GroupByLinesWhilePrinting
		{
			get
			{
				if (transaction.AH_TransactionType == TransactionTypes.Transfer)
				{
					return false;
				}
				else
				{
					return base.GroupByLinesWhilePrinting;
				}
			}
		}
		public override Dictionary<ZString, ZString> ApplicableOptionalFields
		{
			get
			{
				if (applicableOptionalFields_innervalue == null)
				{
					applicableOptionalFields_innervalue = new Dictionary<ZString, ZString>();

					if (transaction.AH_Ledger == LedgerTypes.CashBook)
					{
						if (transaction.AH_TransactionType == TransactionTypes.DirectReceipt
							|| transaction.AH_TransactionType == TransactionTypes.DirectPayment
							|| transaction.AH_TransactionType == TransactionTypes.ExchangeDifference)
						{
							applicableOptionalFields_innervalue.Add(BankCodeText, transaction.AH_AB.IsEmpty ? ZString.Empty : Factory.Load<AccBankAccount>(transaction.AH_AB).AB_BankAbbreviation);

							if ((transaction.AH_TransactionType == TransactionTypes.DirectReceipt
								|| transaction.AH_TransactionType == TransactionTypes.DirectPayment)
								&& !transaction.AH_GB_TaxBranch.IsEmpty)
							{
								applicableOptionalFields_innervalue.Add(TaxBranchText, transaction.TaxBranch.GB_Code);
							}
						}

						if (transaction.AH_TransactionType == TransactionTypes.Transfer)
						{
							var transferRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, transaction.AH_Ledger);
							transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Transfer);
							transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
							transferRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
							var relatedTransactions = new TransactionHeaderCollection(Factory, transferRowFilter);
							relatedTransactions.Load();
							if (relatedTransactions.Count == 1 && relatedTransactions[0] is BankTransferRow)
							{
								bool isFromTransaction = (transaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRow) || (transaction.AH_TransactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing);
								var fromTransaction = isFromTransaction ? transaction : relatedTransactions[0];
								var toTransaction = isFromTransaction ? relatedTransactions[0] : transaction;

								applicableOptionalFields_innervalue.Add(Res.GetString("10af697a-6fb8-45d7-a0be-3790e9bc70c4", "FROM BANK"), fromTransaction.BankAccount.AB_Code);
								applicableOptionalFields_innervalue.Add(Res.GetString("4b0071bc-4fbc-4028-8892-902f82ca86b9", "TO BANK"), toTransaction.BankAccount.AB_Code);
							}
						}
					}
				}

				return applicableOptionalFields_innervalue;
			}
		}
		Dictionary<ZString, ZString> applicableOptionalFields_innervalue;

		protected override IEnumerable<ZString> ValidLedgerTypes
		{
			get
			{
				return new ZString[] { LedgerTypes.CashBook };
			}
		}
		protected override IEnumerable<ZString> ValidTransactionTypes
		{
			get
			{
				return new ZString[]
				{
					TransactionTypes.DirectPayment,
					TransactionTypes.DirectReceipt,
					TransactionTypes.Transfer,
					TransactionTypes.ExchangeDifference
				};
			}
		}
	}
}
