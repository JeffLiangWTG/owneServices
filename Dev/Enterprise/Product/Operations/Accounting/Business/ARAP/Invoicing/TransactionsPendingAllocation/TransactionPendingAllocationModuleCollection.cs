using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationModuleCollection : TransactionHeaderCollection
	{
		public TransactionPendingAllocationModuleCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public new TransactionPendingAllocation this[int index]
		{
			get { return (TransactionPendingAllocation)Elements[index]; }
		}

		public new TransactionPendingAllocation AddNew()
		{
			return (TransactionPendingAllocation)base.AddNew();
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNewCore_Base();
		}

		protected override bool AllowNewCore
		{
			get { return true; }
		}
	}
}