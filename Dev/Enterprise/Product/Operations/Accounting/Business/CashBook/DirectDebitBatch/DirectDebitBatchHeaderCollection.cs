using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class DirectDebitBatchHeaderCollection : TransactionHeaderCollection
	{
		public DirectDebitBatchHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DirectDebitBatchHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public DirectDebitBatchHeaderCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, new ZQuery(), company)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery newQuery = base.CreateRelationshipFilter();
			newQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.CashBook);
			newQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.DDRBatch);
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

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return findBoxListProvider ?? (findBoxListProvider = new DirectDebitBatchHeaderFindBoxListProvider(this)); }
		}

		DirectDebitBatchHeaderFindBoxListProvider findBoxListProvider;

		#region DirectDebitBatchHeaderFindBoxListProvider

		public class DirectDebitBatchHeaderFindBoxListProvider : FindBoxListProvider
		{
			public DirectDebitBatchHeaderFindBoxListProvider(DirectDebitBatchHeaderCollection collection)
				: base(collection)
			{
			}

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
			{
				return BizObjsFromCodeWithCompleteFilter(code);
			}
		}

		#endregion
	}
}