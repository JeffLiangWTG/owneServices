using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class AccountingJournalLineForGeneralLedgerDataCreator : IAccountingJournalLineForGeneralLedgerDataCreator
	{
		public IEnumerable<IAccountingJournalLine> CreateAccountingJournalLines(TransactionHeader transaction, TransactionLine transactionLine, IEnumerable<ZString> validLedgerTypes, IEnumerable<ZString> validTransactionTypes)
		{
			var result = new List<IAccountingJournalLine>();
			BusinessObjectFactory factory;
			var query = new ZQuery();

			if (transaction != null)
			{
				query.AddToFilter(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, transaction.PK);
				factory = transaction.Factory;
			}
			else
			{
				query.AddToFilter(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, transactionLine.PK);
				factory = transactionLine.Factory;
			}
			IEnumerable<AccGeneralLedgerData> generalLedgerTransactionData;

			if (transaction != null
				&& transaction is not TransactionHeaderWithLines
				&& validLedgerTypes.Contains(transaction.AH_Ledger)
				&& validTransactionTypes.Contains(transaction.AH_TransactionType))
			{
				if (transaction.AH_TransactionType == TransactionTypes.Transfer)
				{
					var oppositeTransactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
					oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, transaction.AH_Ledger);
					oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transaction.AH_TransactionType);
					oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
					oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
					var oppositeTransaction = factory.LoadTop1<TransactionHeader>(oppositeTransactionQuery);

					query.AddToFilter(JoinCondition.Or, AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, SQLComparisonOperator.Equal, oppositeTransaction.PK);

					generalLedgerTransactionData = factory.Load<AccGeneralLedgerData>(query);
					result.AddRange(generalLedgerTransactionData.Select(data => new AccountingJournalLineForGeneralLedgerData(data)));
				}
				else if ((transaction.AH_Ledger == LedgerTypes.AccountsReceivable || transaction.AH_Ledger == LedgerTypes.AccountsPayable) && (transaction.AH_TransactionType == TransactionTypes.Contra))
				{
					var oppositeTransactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
					oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transaction.AH_TransactionType);
					oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transaction.AH_TransactionBelongsToGroup);
					oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
					oppositeTransactionQuery.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
					var oppositeTransaction = factory.LoadTop1<TransactionHeader>(oppositeTransactionQuery);

					query.AddToFilter(JoinCondition.Or, AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, SQLComparisonOperator.Equal, oppositeTransaction.PK);
					generalLedgerTransactionData = factory.Load<AccGeneralLedgerData>(query);
					result.AddRange(generalLedgerTransactionData.Select(data => new AccountingJournalLineForGeneralLedgerData(data)));
				}
				else
				{
					generalLedgerTransactionData = factory.Load<AccGeneralLedgerData>(query);
					result.AddRange(generalLedgerTransactionData.Select(data => new AccountingJournalLineForGeneralLedgerData(data)));
				}
			}
			else
			{
				generalLedgerTransactionData = factory.Load<AccGeneralLedgerData>(query);
				result.AddRange(generalLedgerTransactionData.Select(data => new AccountingJournalLineForGeneralLedgerData(data)));
			}
			return result;
		}
	}
}
