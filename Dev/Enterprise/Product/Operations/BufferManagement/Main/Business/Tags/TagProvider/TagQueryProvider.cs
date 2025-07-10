using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class TagQueryProvider
	{
		#region Filters

		public static ZQuery GetTagTaskDefinitionCodeFilter(ZGuid magnitudePK, bool notIn)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			var taskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_FH_ProcessHeader, notIn);
			var linkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			var magnitudeSubQuery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagLinkSchema.TGL_TGM_Magnitude);

			magnitudeSubQuery.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, magnitudePK);
			linkSubQuery.AddSubQuery(magnitudeSubQuery, JoinCondition.And);
			taskSubQuery.AddSubQuery(linkSubQuery, JoinCondition.And);
			query.AddSubQuery(taskSubQuery, JoinCondition.And);

			return query;
		}

		public static ZQuery GetTagTaskMagnitudeFilter(ZGuid magnitudePK, bool notIn)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			var taskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_FH_ProcessHeader, notIn);
			var linkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);

			linkSubQuery.AddToFilter(TagLinkSchema.TGL_TGM_Magnitude, magnitudePK);
			taskSubQuery.AddSubQuery(linkSubQuery, JoinCondition.And);
			query.AddSubQuery(taskSubQuery, JoinCondition.And);

			return query;
		}

		public static ZQuery GetTagDefinitionCodeFilter(ZGuid tagGroupPK, bool notIn, bool includeInherited, ZSqlParameterCollection parameters)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));

			CreateTagDefinitionCodeFilterSqlAndAddToQuery(new List<ZGuid> { tagGroupPK }, parameters, notIn, includeInherited, JoinCondition.And, query);

			return query;
		}

		public static ZQuery GetTagMagnitudeFilter(ZGuid magnitudePK, bool notIn, bool includeInherited, ZSqlParameterCollection parameters, BusinessObjectFactory factory)
		{
			var magnitude = factory.Load<TagMagnitude>(magnitudePK);
			var isExclusive = magnitude?.Definition?.TGD_IsExclusive ?? true;
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			CreateTagMagnitudeFilterSqlAndAddToQuery(new List<ZGuid> { magnitudePK }, parameters, notIn, includeInherited, isExclusive, JoinCondition.And, query);

			return query;
		}

		#endregion

		#region Group Filters

		public static ZQuery GetTagDefinitionCodeFilterForGroup(List<TagQueryProperty> tagQueryProperties, ZSqlParameterCollection parameters)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			var tagGroupingDictionary = GetTagGroupings(tagQueryProperties);

			foreach (var group in tagGroupingDictionary)
			{
				var properties = group.Value;

				if (properties.Count > 0)
				{
					if (group.Key == TagGroupings.IncludeInherited)
					{
						properties.ForEach(p => CreateTagDefinitionCodeFilterSqlAndAddToQuery(new List<ZGuid> { p.PK }, parameters, p.NotIn, p.IncludeInherited, JoinCondition.Or, query));
					}
					else
					{
						CreateTagDefinitionCodeFilterSqlAndAddToQuery(properties.Select(p => p.PK), parameters, group.Key == TagGroupings.IsNotIn, false, JoinCondition.Or, query);
					}
				}
			}

			return query;
		}

		public static ZQuery GetTagMagnitudeFilterForGroup(List<TagMagnitudeQueryProperty> tagQueryProperties, ZSqlParameterCollection parameters, BusinessObjectFactory factory)
		{
			LoadAllTagMagnitudes(tagQueryProperties, factory);

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			var tagGroupingDictionary = GetTagGroupings(tagQueryProperties);

			foreach (var group in tagGroupingDictionary)
			{
				var properties = group.Value;

				if (properties.Count > 0)
				{
					if (group.Key == TagGroupings.IsExclusive || group.Key == TagGroupings.IncludeInherited)
					{
						properties.ForEach(p => CreateTagMagnitudeFilterSqlAndAddToQuery(new List<ZGuid> { p.PK }, parameters, p.NotIn, p.IncludeInherited, p.IsExclusive, JoinCondition.Or, query));
					}
					else
					{
						CreateTagMagnitudeFilterSqlAndAddToQuery(properties.Select(p => p.PK), parameters, group.Key == TagGroupings.IsNotIn, false, false, JoinCondition.Or, query);
					}
				}
			}

			return query;
		}

		#endregion

		#region Impl

		public static void CreateTagMagnitudeFilterSqlAndAddToQuery(IEnumerable<ZGuid> magnitudePKs, ZSqlParameterCollection parameters, bool notIn, bool includeInherited, bool isExclusive, JoinCondition condition, ZQuery query)
		{
			const string baseQuery = @"
				FH_PK {0}
				(
					SELECT FH_PK
					FROM dbo.ProcessHeader
					JOIN dbo.TagLink jobLink 
						ON jobLink.TGL_ParentId = FH_FH_ParentHeader
						AND jobLink.TGL_TGM_Magnitude {1}
					{2}

					UNION ALL

					{3}
				)";

			const string exclusiveTagJobHeaderQuery = @"
					JOIN dbo.TagMagnitude mag ON mag.TGM_PK = jobLink.TGL_TGM_Magnitude
					WHERE 1=1
						AND FH_FH_ParentHeader is not null
						AND NOT EXISTS
						(
							SELECT null
							FROM dbo.TagMagnitude workflowMag
							JOIN dbo.TagLink workflowLink on workflowLink.TGL_TGM_Magnitude = workflowMag.TGM_PK
							WHERE 1=1
								AND workflowLink.TGL_ParentId = FH_PK
								AND workflowMag.TGM_TGD_Tag = mag.TGM_TGD_Tag
								AND workflowMag.TGM_PK <> mag.TGM_PK
						)
					";

			const string nonExclusiveTagJobHeaderQuery = @"
					WHERE FH_FH_ParentHeader is not null
					";
			var allParameterNames = "";

			var pks = magnitudePKs.ToArray();
			foreach (var magnitudePk in pks)
			{
				var parameterName = BMQueryParameterisationHelper.GetNextUniqueParameterName("@TagMag", parameters);
				parameters.Add(parameterName, magnitudePk, TagMagnitudeSchema.PK);
				allParameterNames = string.IsNullOrEmpty(allParameterNames) ? parameterName : string.Concat(allParameterNames, string.Concat(", ", parameterName));
			}
			var parameterCondition = (pks.Length == 1 ? "= " : (NoResString)"in ") + // Sql isn't a res string
				(pks.Length == 1 ? "" : "(") +
				allParameterNames +
				(pks.Length == 1 ? "" : ")");

			var workflowQueryNotInherited = (NoResString)@"
					SELECT TGL_ParentId FROM dbo.TagLink
					WHERE TGL_TGM_Magnitude " + parameterCondition; // Sql isn't a res string

			var workflowQueryInherited = string.Format(CultureInfo.InvariantCulture, @"SELECT FH_PK FROM dbo.GetWorkflowsAndDescendentsFromTagMagnitude({0})", allParameterNames);

			var sql = string.Format(Culture.Invariant, baseQuery,
				notIn ? (NoResString)"not in" : (NoResString)"in", // Sql isn't a res string
				parameterCondition,
				isExclusive ? exclusiveTagJobHeaderQuery : nonExclusiveTagJobHeaderQuery,
				includeInherited ? workflowQueryInherited : workflowQueryNotInherited
			);

			query.AddFilterAndZSQLParameterCollection(sql, parameters, condition);
		}

		public static void CreateTagDefinitionCodeFilterSqlAndAddToQuery(IEnumerable<ZGuid> tagGroupPKs, ZSqlParameterCollection parameters, bool notIn, bool includeInherited, JoinCondition condition, ZQuery query)
		{
			string baseQry = @"
				FH_PK {0}
				(
					SELECT FH_PK
					FROM dbo.ProcessHeader
					JOIN dbo.TagLink on FH_FH_ParentHeader = TGL_ParentId
					JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
					WHERE TGM_TGD_Tag {3}

					UNION ALL
	
					SELECT {1}
					FROM dbo.TagLink
					JOIN dbo.TagMagnitude on TGL_TGM_Magnitude = TGM_PK
					{2}
					WHERE TGM_TGD_Tag {3}
				)";

			var allParameterNames = "";

			var pks = tagGroupPKs.ToArray();
			foreach (var tagGroupPk in pks)
			{
				var parameterName = BMQueryParameterisationHelper.GetNextUniqueParameterName("@TagGroup", parameters);
				parameters.Add(parameterName, tagGroupPk, TagDefinitionSchema.PK);
				allParameterNames = string.IsNullOrEmpty(allParameterNames) ? parameterName : string.Concat(allParameterNames, string.Concat(", ", parameterName));
			}
			var parameterCondition = (pks.Length == 1 ? "= " : (NoResString)"in ") + // Sql isn't a res string
											(pks.Length == 1 ? "" : "(") +
											allParameterNames +
											(pks.Length == 1 ? "" : ")");

			var sql = string.Format(Culture.Invariant, baseQry,
				notIn ? (NoResString)"not in" : (NoResString)"in",// SQL constant
				includeInherited ? "Child.FH_PK" : "TGL_ParentId", // Sql isn't a res string
				includeInherited ? (NoResString)"CROSS APPLY dbo.GetWorkflowDescendentHierarchy(TGL_ParentId, " + BMSRegistry.Instance.MaximumDepthOfAnalyzedWorkflowHierarchy.Value.ToString(Culture.Invariant) + (NoResString)") AS Child" : "", // Sql isn't a res string
				parameterCondition
			);

			query.AddFilterAndZSQLParameterCollection(sql, parameters, condition);
		}

		static void LoadAllTagMagnitudes(List<TagMagnitudeQueryProperty> properties, BusinessObjectFactory factory)
		{
			var magnitudes = factory.Load<TagMagnitude>(new ZQuery(TagMagnitudeSchema.PK, properties.Select(f => f.PK)));

			foreach (var magnitude in magnitudes)
			{
				var property = properties.FirstOrDefault(p => p.PK == magnitude.PK);
				if (property != null)
				{
					property.Magnitude = magnitude;
				}
			}
		}

		static Dictionary<TagGroupings, List<T>> GetTagGroupings<T>(IEnumerable<T> tagQueryProperties) where T : TagQueryProperty
		{
			var tagGroupingDictionary = new Dictionary<TagGroupings, List<T>>();

			foreach (var tagQueryProperty in tagQueryProperties)
			{
				var groupType = tagQueryProperty.GroupType;

				if (!tagGroupingDictionary.ContainsKey(groupType))
				{
					tagGroupingDictionary.Add(groupType, new List<T> { tagQueryProperty });
				}
				else
				{
					tagGroupingDictionary[groupType].Add(tagQueryProperty);
				}
			}

			return tagGroupingDictionary;
		}

		#endregion
	}

	public enum TagGroupings
	{
		IsExclusive = 0,
		IncludeInherited = 1,
		IsIn = 2,
		IsNotIn = 3
	}
}
