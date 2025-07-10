using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public abstract class AcceptabilityBandSqlStrategyBase : IAcceptabilityBandSqlStrategy
	{
		protected BMComponentAcceptabilityBand Band { get; set; }
		protected AcceptabilityBandSqlBuilderParameters AcceptabilityBandParameters { get; set; }

		protected AcceptabilityBandSqlStrategyBase(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
		{
			Band = band;
			AcceptabilityBandParameters = parameters;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is part of a SQL statement")]
		public (string, ZSqlParameter[]) GetAcceptabilityBandSql(bool createInsertQuery = false)
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

			if (workflowAndSectionWorkflowCTEs != null)
			{
				workflowAndSectionWorkflowCTEs = $",\r\n{workflowAndSectionWorkflowCTEs}";
			}

			var (aggregationSql, aggregationParameters) = GetAggregationSql();
			var dataSectionFormat = AcceptabilityBandParameters.IsQueryForValidation
				? @"
				{0}"
				: (NoResString)@",
				Data AS (
					{0}
				)";

			var dataSection = string.Format(CultureInfo.InvariantCulture, dataSectionFormat, aggregationSql);
			var insertQueryComment = createInsertQuery ? "\n-- Insert query for MENT\n" : string.Empty;

			sqlBuilder.Append(FormattableString.Invariant($@"
-- Database: {Db.DatabaseName}{insertQueryComment}
-- Acceptability Band name: [{Band.BAB_Name}], Type: [{Band.BAB_Type}]

declare @Sum decimal(10,2);
declare @RowCount int;

WITH XmlNamespaces (
	DEFAULT 'http://www.cargowise.com/DataVersionLog'
){workflowAndSectionWorkflowCTEs}
"));

			sqlBuilder.Append(@$"{dataSection}
				", aggregationParameters);

			if (!AcceptabilityBandParameters.IsQueryForValidation)
			{
				var hasAdditionalAggregatorColumn = !Band.IsSqlDisabled && BMComponentAcceptabilityBand.IsAdditionalAggregatorColumnPresent(sqlBuilder.ToString());

				if (!createInsertQuery)
				{
					sqlBuilder.Append(GetWrapperSelectClause(hasAdditionalAggregatorColumn));
					sqlBuilder.Append((NoResString)"FROM Data WHERE 1 = 1\r\n");
				}
				else
				{
					sqlBuilder.Append(@"
SELECT 
	convert(decimal(10,2), Value) Value,
	CAST(Component AS uniqueidentifier) Component,
	CAST(ReleaseGroup AS uniqueidentifier) ReleaseGroup
INTO ").Append(Band.TempTableName)
	.Append(@"
FROM Data
WHERE 1 = 1
");
				}

				if (Band.BAB_Type == AcceptabilityBandTypes.Codes.SQL && Band.BAB_FC_Component.IsValid)
				{
					var currentComponent = ParameterisationHelper.CreateCurrentComponentParameter(Band);
					sqlBuilder.Append((NoResString)"AND Component = (@CurrentComponentPK)\r\n", new[] { currentComponent });
				}

				if (ShouldFilterByReleaseGroup())
				{
					var releaseGroupPk = ParameterisationHelper.CreateReleaseGroupParameter(AcceptabilityBandParameters);
					sqlBuilder.Append((NoResString)"AND ReleaseGroup = @ReleaseGroupPK\r\n", new[] { releaseGroupPk });
				}

				if (!hasAdditionalAggregatorColumn)
				{
					sqlBuilder.Append((NoResString)"IF @RowCount > 0 SELECT @Sum AS Value");

					if (ShouldConvertNullResultToZero)
					{
						sqlBuilder.Append((NoResString)" ELSE SELECT 0 AS Value");
					}
				}
			}

			sqlBuilder.Append(System.Environment.NewLine + DbCommand.ExecuteAsReaderFlagComments);

			return (sqlBuilder.ToString(), sqlBuilder.Parameters.ToArray());
		}

		string GetWrapperSelectClause(bool hasAdditionalAggregatorColumn)
		{
			if (hasAdditionalAggregatorColumn)
			{
				return string.Format(CultureInfo.InvariantCulture, @"
	SELECT TOP {0}
	CONVERT(decimal(10, 2), Value) Value,
	AdditionalAggregator
	", AcceptabilityBandParameters.MaximumItems); // This is part of a SQL statement)
			}

			return (NoResString)@"
	SELECT
	@Sum = SUM(CONVERT(decimal(10, 2), Value)),
	@RowCount = COUNT(*)
	"; // This is part of a SQL statement
		}

		protected virtual string GetWorkflowAndSectionWorkflowCTE(string filterSql, bool nullQuery, ZSqlParameter workflowPKs)
		{
			if (nullQuery)
			{
				return FormattableString.Invariant($@"{BMComponentAcceptabilityBand.BoardSectionWorkflowsResultSetName} AS (SELECT NULL as FH_PK WHERE 1=2),
{BMComponentAcceptabilityBand.FilteredWorkflowsResultSetName} AS ({filterSql})");
			}
			else if (workflowPKs == null)
			{
				return FormattableString.Invariant($@"{BMComponentAcceptabilityBand.BoardSectionWorkflowsResultSetName} AS (SELECT * FROM dbo.ProcessHeader),
{BMComponentAcceptabilityBand.FilteredWorkflowsResultSetName} AS ({filterSql})");
			}
			else
			{
				return FormattableString.Invariant($@"{BMComponentAcceptabilityBand.BoardSectionWorkflowsResultSetName} AS (
	SELECT * FROM dbo.ProcessHeader WHERE FH_PK IN ({workflowPKs.ParameterisedSql})
),
{BMComponentAcceptabilityBand.FilteredWorkflowsResultSetName} AS ({filterSql})");
			}
		}

		protected virtual bool ShouldConvertNullResultToZero => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public (string, ZSqlParameter[]) GetMatchingWorkflowCountSql()
		{
			var tuple = GetMatchingWorkflowsSql();

			return (
				string.Format(CultureInfo.InvariantCulture, @"
SELECT 
	FH_GG_ReleaseGroup,
	COUNT(*)
FROM
(
	{0}
) x
GROUP BY FH_GG_ReleaseGroup
", tuple.Item1)
, tuple.Item2);
		}

		public (string, ZSqlParameter[]) GetMatchingWorkflowsSql()
		{
			return GetMatchingWorkflowsSqlCore();
		}

		protected virtual (string, ZSqlParameter[]) GetMatchingWorkflowsSqlCore()
		{
			throw new InvalidOperationException("Cannot find matching workflows for bands not using filter strips");
		}

		protected abstract (string, ZSqlParameter[]) GetApplicableFilterSql();

		protected virtual string GetAdditionalWhereClause()
		{
			return null;
		}

		(string, ZSqlParameter[]) GetAggregationSql()
		{
			if (Band.BAB_Type != AcceptabilityBandTypes.Codes.Aggregate && Band.BAB_Type != AcceptabilityBandTypes.Codes.SQL && !Band.IsSqlDisabled)
			{
				throw new InvalidOperationException("SQL input was not disabled, but custom aggregation is used");
			}

			return GetAggregationSqlCore();
		}

		protected abstract (string, ZSqlParameter[]) GetAggregationSqlCore();

		protected virtual (string, ZSqlParameter[]) GetSupersetSql(bool isReleaseGroupPredicate = false, string tableAlias = null, bool addAndIfRequired = true)
		{
			return (string.Empty, Array.Empty<ZSqlParameter>());
		}

		protected (string, ZSqlParameter[]) GetReleaseGroupPredicate(string tableAlias = null)
		{
			var result = new SqlBuilder();
			bool andRequired = false;
			if (ShouldFilterByReleaseGroup())
			{
				var releaseGroupParam = ParameterisationHelper.CreateReleaseGroupParameter(AcceptabilityBandParameters);
				result.Append(FormattableString.Invariant($"{GetColumnPrefix(tableAlias)}FH_GG_ReleaseGroup = {releaseGroupParam.ParameterisedSql} "), new[] { releaseGroupParam });
				andRequired = true;
			}

			var (supersetSql, supersetParameters) = GetSupersetSql(isReleaseGroupPredicate: true, tableAlias: tableAlias, addAndIfRequired: andRequired);
			result.Append(supersetSql, supersetParameters);

			var resultSql = result.GetSql();
			return (string.IsNullOrEmpty(resultSql) ? "1 = 1" : resultSql, result.Parameters.ToArray());
		}

		protected bool ShouldFilterBySection() => AcceptabilityBandParameters.ShouldFilterBySection && AcceptabilityBandParameters.WorkflowPKs != null;

		protected bool ShouldFilterByReleaseGroup() => AcceptabilityBandParameters.ReleaseGroupPK.IsValid && (AcceptabilityBandParameters.RunAsGoldenRule || AcceptabilityBandParameters.ShouldFilterByReleaseGroup);

		protected string GetSectionWorkflowFilterClause(bool addAndIfRequired, string tableAlias = null) => ShouldFilterBySection()
				? (addAndIfRequired ? "AND " : string.Empty) + string.Format(CultureInfo.InvariantCulture, "{0}FH_PK IN (SELECT FH_PK FROM {1})", GetColumnPrefix(tableAlias), BMComponentAcceptabilityBand.BoardSectionWorkflowsResultSetName)
				: string.Empty;

		protected ZNonPersistentDataQuery GetParameterisedQuery(ZQuery query)
		{
			var builder = new SqlBuilder(SqlBuilder.QueryType.Parameterised);
			var result = query.GetDataQuery(builder);

			return result;
		}

		static string GetColumnPrefix(string tableAlias) =>
			!string.IsNullOrEmpty(tableAlias) ? tableAlias + "." : string.Empty;
	}
}
