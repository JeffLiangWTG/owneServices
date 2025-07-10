using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class GenPivotCollectionRelationship : CollectionRelationship
	{
		public GenPivotCollectionRelationship(BusinessObject master, string relationType, bool includeChildren = true, bool includeParents = true)
			: base(typeof(GenPivot), GetRelatedActivitiesQuery(master, relationType, includeChildren, includeParents))
		{
		}

		public static ZQuery GetRelatedActivitiesQuery(BusinessObject bizObj, string relationType, bool includeChildren, bool includeParents)
		{
			var result = new ZQuery(GenPivotSchema.XX_RelationType, relationType);
			var related = new ZQuery();

			if (includeChildren)
			{
				var childQuery = new ZQuery(GenPivotSchema.XX_Relation1TableCode, bizObj.TablePrefix);
				childQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, bizObj.PK);
				related.AddToFilter(childQuery, JoinCondition.Or);
			}

			if (includeParents)
			{
				var parentQuery = new ZQuery(GenPivotSchema.XX_Relation2TableCode, bizObj.TablePrefix);
				parentQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, bizObj.PK);
				related.AddToFilter(parentQuery, JoinCondition.Or);
			}

			result.AddToFilter(related, JoinCondition.And);
			return result;
		}
	}
}
