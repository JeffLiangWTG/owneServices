using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class TaxTransactionsLinkedToJobChargeCollection : NonPersistentBusinessObjectCollection<TaxTransactionsLinkedToJobCharge>
	{
		public TaxTransactionsLinkedToJobChargeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
