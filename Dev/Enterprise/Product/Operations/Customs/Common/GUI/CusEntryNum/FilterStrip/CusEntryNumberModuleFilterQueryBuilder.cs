using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Common.Module
{
	static class CusEntryNumberModuleFilterQueryBuilder
	{
		internal static ZDBOnlyQuery BuildQuery(Type businessObjectType, ZDBOnlySubQuery cusEntryNumberQuery, ZDBOnlySubQuery[] relatedParentSubQueries)
		{
			ZDBOnlyQuery resultQuery = new ZDBOnlyQuery(businessObjectType);

			if (!cusEntryNumberQuery.IsEmpty)
			{
				if (relatedParentSubQueries == null || relatedParentSubQueries.Length == 0)
				{
					resultQuery.AddSubQuery(cusEntryNumberQuery, JoinCondition.And);
				}
				else
				{
					ZDBOnlySubQuery parentQuery = (ZDBOnlySubQuery)relatedParentSubQueries[0].DeepClone();
					parentQuery.AddSubQuery(cusEntryNumberQuery, JoinCondition.And);

					ZDBOnlySubQuery lastSubQuery = parentQuery;
					for (int i = 1; i < relatedParentSubQueries.Length; i++)
					{
						ZDBOnlySubQuery joiningQuery = (ZDBOnlySubQuery)relatedParentSubQueries[i].DeepClone();
						joiningQuery.AddSubQuery(lastSubQuery, JoinCondition.And);
						lastSubQuery = joiningQuery;
					}

					resultQuery.AddSubQuery(lastSubQuery, JoinCondition.And);
				}
			}

			return resultQuery;
		}
	}
}
