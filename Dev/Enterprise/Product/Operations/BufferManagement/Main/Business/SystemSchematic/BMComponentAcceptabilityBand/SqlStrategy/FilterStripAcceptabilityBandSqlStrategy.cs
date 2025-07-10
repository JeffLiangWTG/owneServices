using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	abstract class FilterStripAcceptabilityBandSqlStrategy : AcceptabilityBandSqlStrategyBase
	{
		protected FilterStripAcceptabilityBandSqlStrategy(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
			: base(band, parameters)
		{
		}

		protected override (string, ZSqlParameter[]) GetApplicableFilterSql()
		{
			var filterStripSql = GetFilterStripSql();
			var filterStripSqlText = filterStripSql.ParameterisedQueryText;

			var additionalWhereClause = GetAdditionalWhereClause();

			var unionInnerQuerybuilder = new StringBuilder();
			var (baseSql, baseParams) = GetProcessHeaderBaseSql();
			unionInnerQuerybuilder.Append(baseSql);

			var unionClauses = AcceptabilityBandParameters.GetAdditionalUnionClauses();
			unionInnerQuerybuilder.Append(unionClauses);

			return (string.Format(CultureInfo.InvariantCulture, @"
SELECT * FROM (
	{0}
) innerProcessHeaderUnionSql
WHERE {1}
{2}
{3}
",
				/*0*/ unionInnerQuerybuilder,
				/*1*/ string.IsNullOrWhiteSpace(filterStripSqlText) ? "1 = 1" : filterStripSqlText,
				/*2*/ string.IsNullOrWhiteSpace(additionalWhereClause) ? string.Empty : "AND " + additionalWhereClause,
				/*3*/ GetSectionWorkflowFilterClause(addAndIfRequired: true)),
				filterStripSql.Parameters.Concat(baseParams).ToArray());
		}

		ZNonPersistentDataQuery GetFilterStripSql()
		{
			ZNonPersistentDataQuery filterSql = null;

			if (AcceptabilityBandParameters.CacheQuery)
			{
				filterSql = Band.CachedFilterRuleSql;
			}

			if (filterSql == null)
			{
				var query = RelatedModuleFiltersHelper.GetFilterQuerySafe(Band.FilterRule);
				var additionalQuery = AcceptabilityBandParameters.GetAdditionalProcessHeaderFilter();
				query.AddToFilter(additionalQuery);

				filterSql = GetParameterisedQuery(query);

				if (AcceptabilityBandParameters.CacheQuery)
				{
					Band.CachedFilterRuleSql = filterSql;
				}
			}

			return filterSql;
		}

		(string, ZSqlParameter[]) GetProcessHeaderBaseSql()
		{
			if (Band.BAB_FC_Component.IsValid && AcceptabilityBandParameters.ComponentOverridePKs.Length == 0)
			{
				var whereClause = FormattableString.Invariant($"{ProcessHeaderSchema.Constants.FH_FC_CurrentComponent} = @CurrentComponentPK");
				var param = ParameterisationHelper.CreateCurrentComponentParameter(Band);
				return (ProcessHeader.GetBaseSqlQuery(whereClause), new[] { param });
			}
			else
			{
				return (ProcessHeader.GetBaseSqlQuery("1 = 1"), Array.Empty<ZSqlParameter>());
			}
		}

		protected override (string, ZSqlParameter[]) GetMatchingWorkflowsSqlCore()
		{
			var sqlBuilder = new SqlBuilder();
			var (filterSql, filterParams) = GetApplicableFilterSql();
			foreach (var item in filterParams)
			{
				sqlBuilder.ReserveParameter(item);
			}

			var isNullQuery = AcceptabilityBandParameters?.WorkflowPKs != null && AcceptabilityBandParameters.WorkflowPKs.Count == 0;
			var workflowPksParameter = !isNullQuery && AcceptabilityBandParameters?.WorkflowPKs != null ? ZSqlParameter.New("@WorkflowPKs", AcceptabilityBandParameters.WorkflowPKs.ToArray(), ProcessHeaderSchema.PK, isTableValued: true) : null;
			if (workflowPksParameter != null)
			{
				sqlBuilder.ReserveParameter(workflowPksParameter);
			}

			var workflowAndSectionWorkflowCTEs = GetWorkflowAndSectionWorkflowCTE(filterSql, isNullQuery, workflowPksParameter);

			sqlBuilder.Append(FormattableString.Invariant($@"WITH {workflowAndSectionWorkflowCTEs}
SELECT
	DISTINCT(x.FH_PK),
	ProcessHeader.FH_GG_ReleaseGroup,
	ProcessHeader.FH_PlannedDurationInMinutes
FROM {BMComponentAcceptabilityBand.FilteredWorkflowsResultSetName} x
JOIN dbo.ProcessHeader on x.FH_PK = ProcessHeader.FH_PK
WHERE
"));

			var (releaseGroupPredicateSql, releaseGroupPredicateParams) = GetReleaseGroupPredicate(tableAlias: ProcessHeaderSchema.Constants.TableName);
			sqlBuilder.Append(releaseGroupPredicateSql, releaseGroupPredicateParams);

			return (sqlBuilder.ToString(), sqlBuilder.Parameters.ToArray());
		}
	}
}
