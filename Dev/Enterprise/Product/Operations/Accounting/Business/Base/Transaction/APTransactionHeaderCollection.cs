using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	[ModuleID(ModuleId.APTransaction)]
	public class APTransactionHeaderCollection : TransactionHeaderCollection
	{
		public APTransactionHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public APTransactionHeaderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public APTransactionHeaderCollection(IQueryClaim queryClaim, ZQuery filter) : base(queryClaim, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			if (fQueryClaim == null)
			{
				result.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			}
			return result;
		}
	}
}