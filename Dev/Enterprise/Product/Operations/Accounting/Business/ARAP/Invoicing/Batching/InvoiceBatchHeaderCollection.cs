using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoiceBatchHeaderCollection : TransactionHeaderCollection
	{
		public InvoiceBatchHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery newQuery = base.CreateRelationshipFilter();
			newQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			newQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.InvoiceBatch);
			return newQuery;
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot directly add to this collection.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new InvoiceBatchHeader AddNew()
		{
			return (InvoiceBatchHeader)base.AddNew();
		}

		public new InvoiceBatchHeader this[int index]
		{
			get { return (InvoiceBatchHeader)Elements[index]; }
		}
	}
}
