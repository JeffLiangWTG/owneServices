using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	[ModuleID(ModuleId.ARTransaction)]
	public class ARTransactionHeaderCollection : TransactionHeaderCollection
	{
		public ARTransactionHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ARTransactionHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ARTransactionHeaderCollection(IQueryClaim queryClaim, ZQuery filter)
			: base(queryClaim, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery newQuery = base.CreateRelationshipFilter();
			if (fQueryClaim == null)
			{
				ZQuery ledgerFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
				newQuery.AddToFilter(ledgerFilter, JoinCondition.And);
				newQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, ZArchitecture.Core.TransactionTypes.InvoiceBatch);
			}
			return newQuery;
		}
	}
}