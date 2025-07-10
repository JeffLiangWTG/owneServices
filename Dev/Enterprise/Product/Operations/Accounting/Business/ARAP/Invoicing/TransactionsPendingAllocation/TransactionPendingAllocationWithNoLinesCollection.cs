using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class TransactionPendingAllocationWithNoLinesCollection : InvoicingLineBaseCollection
	{
		public TransactionPendingAllocationWithNoLinesCollection(BusinessObject parent)
			: base(parent)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot add to this collection.");
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery() { IsNoResultQuery = true };
		}

		protected override bool ElementCanBeAdded(BusinessObject bizO)
		{
			return false;
		}
	}
}