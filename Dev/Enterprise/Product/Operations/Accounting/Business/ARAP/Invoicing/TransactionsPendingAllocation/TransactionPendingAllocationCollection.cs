using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationCollection : TransactionHeaderCollection
	{
		public TransactionPendingAllocationCollection(TransactionsPendingAllocation allocationParent, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Parent = allocationParent;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((TransactionPendingAllocation)child).SetParentCollection(this);
		}

		public readonly TransactionsPendingAllocation Parent;

		public new TransactionPendingAllocation this[int index]
		{
			get { return (TransactionPendingAllocation)Elements[index]; }
		}

		public new TransactionPendingAllocation AddNew()
		{
			return (TransactionPendingAllocation)base.AddNew();
		}

		public new TransactionPendingAllocation AddNew(Type bizObjType)
		{
			return (TransactionPendingAllocation)base.AddNew(bizObjType);
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
