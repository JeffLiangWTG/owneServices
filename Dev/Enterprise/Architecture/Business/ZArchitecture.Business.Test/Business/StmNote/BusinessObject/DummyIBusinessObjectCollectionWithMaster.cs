using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyIBusinessObjectCollectionWithMaster : BusinessObjectCollection<StmNote>, IBusinessObjectCollectionWithMaster
	{
		readonly BusinessObject parent;

		public DummyIBusinessObjectCollectionWithMaster(BusinessObject parent, BusinessObjectFactory factory) : base(factory)
		{
			this.parent = parent;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = new ZQuery();

			query.AddToFilter(StmNoteSchema.ST_ParentID, parent.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, "DummyDependentBizo");

			var stmNoteQuery = new StmNoteQuery();
			stmNoteQuery.AddToFilter(query);
			return stmNoteQuery;
		}

		public BusinessObject Master => parent;
	}
}
