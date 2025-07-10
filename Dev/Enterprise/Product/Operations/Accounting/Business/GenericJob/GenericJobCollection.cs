using System;

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.GenericJob
{
	public class GenericJobCollection : BusinessObjectCollection<GenericJob>
	{
		public GenericJobCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public GenericJobCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("This collection does not support AddNew operation.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public void AddToRelationshipFilter(ZQuery query, JoinCondition joinCondition)
		{
			if (fRelationshipFilter == null)
			{
				fRelationshipFilter = new ZQuery();
			}
			fRelationshipFilter.AddToFilter(query, joinCondition);
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return fRelationshipFilter ?? base.CreateRelationshipFilter();
		}

		ZQuery fRelationshipFilter;
	}
}
