using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupActivityCollectionRelationship : CollectionRelationship
	{
		public IncidentManagementGroupActivityCollectionRelationship(BusinessObject master, bool includeChildren = true, bool includeParents = true) : base(typeof(IncidentManagementLink), GetRelatedActivitiesQuery(master, includeChildren, includeParents))
		{
		}

		public static ZQuery GetRelatedActivitiesQuery(BusinessObject bizObj, bool includeChildren, bool includeParents)
		{
			var query1 = new ZQuery();
			if (includeChildren)
			{
				query1.IsNoResultQuery = true;
			}

			if (includeParents && !includeChildren)
			{
				var query2 = new ZQuery(IncidentManagementLinkSchema.INL_IM_Incident, bizObj.PK);
				query1.AddToFilter(query2, JoinCondition.Or);
			}
			return query1;
		}
	}
}
