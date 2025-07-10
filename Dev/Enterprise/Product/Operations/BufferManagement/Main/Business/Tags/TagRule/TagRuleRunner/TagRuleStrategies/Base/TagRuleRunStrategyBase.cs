using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public abstract class TagRuleRunStrategyBase
	{
		internal const string StaticFiltersSQL = "and FH_P0_Template is NULL"; // SQL statement

		protected internal TagRuleRunStrategyBase(IConnectionProvider connectionProvider, ILogger logger)
		{
			this.connectionProvider = connectionProvider;
			this.logger = logger;
		}
		readonly IConnectionProvider connectionProvider;
		readonly ILogger logger;

		protected IConnectionProvider ConnectionProvider
		{
			get { return connectionProvider; }
		}

		protected ILogger Logger
		{
			get { return logger; }
		}

		public abstract long Execute(TagRule rule);
		public abstract ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false);

		protected virtual void ToggleSingleThreadedBit(TagRule rule)
		{
		}

		protected void WriteVerificationLog(TagRule rule, bool bitToggled, int maxdopNumber)
		{
			var log = string.Format(CultureInfo.InvariantCulture, (NoResString)"Verified performance of rule [{0}], {1}", // Service task logging
				rule.TGR_Name,
				(bitToggled) ?
					(maxdopNumber == 0) ? (NoResString)"MAXDOP disabled (parallel)." : // Service task logging
(NoResString)"MAXDOP set to 1 (single)." :   // Service task logging
(NoResString)"MAXDOP unchanged.");               // Service task logging
			Logger.Log(LogType.Information, log);
		}

		protected virtual double GetFirstRunTime(TagRule rule, ZDateTime performanceTimestampStart)
		{
			return (ZDateTime.UtcNow - performanceTimestampStart).TotalMilliseconds;
		}

		protected static ZQuery GetTagLinksToDeleteQuery(TagRule rule, bool getNonMatchingTagLinks)
		{
			var query = new ZDBOnlyQuery(typeof(TagLink));
			query.AddToFilter(new ZQuery(TagLinkSchema.TGL_TGM_Magnitude, rule.TagTemplate.TGL_TGM_Magnitude));

			var workflowSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), TagLinkSchema.TGL_ParentId, getNonMatchingTagLinks);
			var workflowQuery = RelatedModuleFiltersHelper.GetFilterQuerySafe(rule.Filter);
			workflowSubQuery.AddToFilter(workflowQuery);

			query.AddSubQuery(workflowSubQuery, JoinCondition.And);

			return query;
		}

		protected static ZQuery GetWorkflowsDeleteTagLinkQuery(TagRule rule, bool getNonMatchingTagLinks)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));

			var workflowSub = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK, getNonMatchingTagLinks);
			var filterRule = RelatedModuleFiltersHelper.GetFilterQuerySafe(rule.Filter);
			workflowSub.AddToFilter(filterRule);

			var tagLinkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			tagLinkSubQuery.AddToFilter(new ZQuery(TagLinkSchema.TGL_TGM_Magnitude, rule.TagTemplate.TGL_TGM_Magnitude));
			query.AddSubQuery(tagLinkSubQuery, JoinCondition.And);

			query.AddSubQuery(ProcessHeaderSchema.PK, workflowSub, JoinCondition.And);

			return query;
		}

		protected static ZQuery GetWorkflowsToTagQuery(TagRule rule, bool asSubQuery = false)
		{
			var query = asSubQuery
				? new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK)
				: new ZDBOnlyQuery(typeof(ProcessHeader));

			var filterRule = RelatedModuleFiltersHelper.GetFilterQuerySafe(rule.Filter);
			query.AddToFilter(filterRule);
			var definition = rule.TagTemplate.Definition;

			if (definition != null && definition.TGD_IsExclusive)
			{
				AddExclusiveTagSubQuery(rule, query);
			}
			else
			{
				AddNonExclusiveTagSubQuery(rule, query);
			}

			return query;
		}

		#region Add Tag Sub Query

		static void AddNonExclusiveTagSubQuery(TagRule rule, ZDBOnlyQuery query)
		{
			var tagLinkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId, notIn: true);
			tagLinkSubQuery.AddToFilter(new ZQuery(TagLinkSchema.TGL_TGM_Magnitude, rule.TagTemplate.TGL_TGM_Magnitude));
			query.AddSubQuery(tagLinkSubQuery, JoinCondition.And);
		}

		static void AddExclusiveTagSubQuery(TagRule rule, ZDBOnlyQuery query)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			subQuery.AddSubQuery(GenerateExclusiveTagSubQuery(rule), JoinCondition.And);
			subQuery.AddAsUnionQuery(GenerateFindDuplicateTagSubQuery(rule), true);

			query.AddSubQuery(subQuery, JoinCondition.And);
		}

		static ZDBOnlySubQuery GenerateExclusiveTagSubQuery(TagRule rule)
		{
			var tagLinkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId, notIn: true);
			var tagMagnitudeSubQuery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagLinkSchema.TGL_TGM_Magnitude);
			tagMagnitudeSubQuery.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, rule.TagTemplate.TagDefinitionPk);
			tagMagnitudeSubQuery.AddToFilter(TagMagnitudeSchema.TGM_RuleRunSequence, SQLComparisonOperator.LessThanOrEqualTo, rule.TagTemplate.Magnitude.TGM_RuleRunSequence);
			tagLinkSubQuery.AddSubQuery(tagMagnitudeSubQuery, JoinCondition.And);
			return tagLinkSubQuery;
		}

		static ZDBOnlySubQuery GenerateFindDuplicateTagSubQuery(TagRule rule)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@tagDefinitionPK", rule.TagTemplate.TagDefinitionPk, TagDefinitionSchema.PK);

			var sql = string.Format(CultureInfo.InvariantCulture,
@"{6} IN ( 
	SELECT {0} FROM {1}
	LEFT JOIN {2} ON {3} = {4}
	WHERE {5} = @tagDefinitionPK
	GROUP BY {0}
	HAVING COUNT(*) > 1
)",
				/*0*/ TagLinkSchema.Constants.TGL_ParentId,
				/*1*/ TagLinkSchema.Constants.TableName,
				/*2*/ TagMagnitudeSchema.Constants.TableName,
				/*3*/ TagLinkSchema.Constants.TGL_TGM_Magnitude,
				/*4*/ TagMagnitudeSchema.Constants.PK,
				/*5*/ TagMagnitudeSchema.Constants.TGM_TGD_Tag,
				/*6*/ ProcessHeaderSchema.Constants.PK);

			var query = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			query.AddFilterAndZSQLParameterCollection(sql, parameters, JoinCondition.Or);
			return query;
		}

		#endregion

		#region Shared Helpers

		internal static void DropTempTable(string name, DbConnection connection)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, DropTableSQL, name);

			connection.ExecuteScalar(sql); // We need to use raw SQL here because it's too slow to use bizos.
		}

		internal static void DropTempTable(string name)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, DropTableSQL, name);

			Db.Connection.ExecuteScalar(sql); // We need to use raw SQL here because it's too slow to use bizos.
		}

		const string DropTableSQL = @"
if object_id('tempdb..{0}') is not null
drop table {0}
"; // Dropping temp tables if they exist. We should clean up after ourselves

		#endregion

		protected bool ThrowDroppedDbExceptionIfMessageMatches(SqlException ex, params string[] tableNames)
		{
			var match = new DbErrorMatch(ex);

			if (match.ExceptionType == DbErrorType.InvalidObjectName && tableNames.Any(x => ex.Message.Contains(x)))
			{
				throw new DroppedDbConnectionException("The database connection was dropped. Rule was not run.");
			}

			return false;
		}

		protected string GetTagRuleParameterizedSql(TagRule rule, ZSqlParameterCollection parameters)
		{
			return RelatedModuleFiltersHelper.GetFilterQueryParameterized(rule.Filter, parameters);
		}

		[Serializable]
		public class NoResultQueryException : Exception
		{
			internal NoResultQueryException()
				: base()
			{
			}

#if NETFRAMEWORK
			protected NoResultQueryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}
	}

	public static class TagRuleRunStrategyBaseConstants
	{
		public static class TemplateNames
		{
			public const string AddTagRuleSQL = "AddTagRuleSQL";
			public const string AddTagRuleWorkflowSelectQuery = "AddTagRuleWorkflowSelectQuery";
			public const string RemoveTagRuleSQL = "RemoveTagRuleSQL";
			public const string RemoveTagWorkflowSQL = "RemoveTagWorkflowSQL";
			public const string SelectAffectedProcessHeadersSql = "SelectAffectedProcessHeadersSql";
			public const string UpdateMagnitudeSql = "UpdateMagnitudeSql";
			public const string UpdateMagnitudeHeaderSql = "UpdateMagnitudeHeaderSql";
		}

		public static class ParameterNames
		{
			public const string TagLinkMagnitudeValue = "@TagLinkMagnitudeValue";
			public const string TagRuleCustomParameter = "@TagRuleCustomParameter";
			// NB: TIL that a Dop number is different to a Drop number. Not a typo!
			public const string MaxDopNumber = "@MaxDopNumber";
			public const string TagMagnitudePK = "@TagMagnitudePK";
			public const string TagRulesNow = "@TagRulesNow";
			public const string TagRulesUtcNow = "@TagRulesUtcNow";
			public const string TagRulePK = "@TagRulePK";
			public const string BatchLowNumber = "@BatchLowNumber";
			public const string BatchHighNumber = "@BatchHighNumber";
		}
	}
}
