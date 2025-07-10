
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class CashbookTransactionCollection : TransactionHeaderCollection
	{
		public CashbookTransactionCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public CashbookTransactionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery newQuery = base.CreateRelationshipFilter();
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			filter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.Receipt);
			filter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.Payment);
			newQuery.AddToFilter(filter, JoinCondition.And);

			newQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.DDRBatch);
			newQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.ReceiptBatch);

			return newQuery;
		}
	}
}