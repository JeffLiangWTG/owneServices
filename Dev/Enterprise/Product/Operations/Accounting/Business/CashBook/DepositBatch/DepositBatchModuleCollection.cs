using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public class DepositBatchModuleCollection : TransactionHeaderCollection
	{
		public DepositBatchModuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery newQuery = base.CreateRelationshipFilter();
			newQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.CashBook);
			newQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			return newQuery;
		}

		public new DepositBatch this[int i]
		{
			get { return (DepositBatch)Elements[i]; }
		}

		public new DepositBatch AddNew()
		{
			return (DepositBatch)base.AddNew();
		}
	}
}