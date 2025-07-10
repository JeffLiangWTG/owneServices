using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	abstract class PercentageAcceptabilityBandSqlStrategy : FilterStripAcceptabilityBandSqlStrategy
	{
		protected PercentageAcceptabilityBandSqlStrategy(BMComponentAcceptabilityBand band, AcceptabilityBandSqlBuilderParameters parameters)
			: base(band, parameters)
		{
		}

		protected (string, ZSqlParameter[]) AsPercentageSql(string valueSelectStatement, string columnToSelectForAllWorkflows)
		{
			var shouldFilterByReleaseGroup = ShouldFilterByReleaseGroup();
			var component = Band.Component;
			var componentParam = component != null ? ParameterisationHelper.CreateComponentPKParameter(Band.BAB_FC_Component) : null;
			var releaseGroupParam = shouldFilterByReleaseGroup ?  ParameterisationHelper.CreateReleaseGroupParameter(AcceptabilityBandParameters) : null;

			var componentPKString = component == null ? "null" : componentParam.ParameterName;
			var releaseGroupPKString = shouldFilterByReleaseGroup ? releaseGroupParam.ParameterName : "null";
			var componentPKCondition = component == null ? string.Empty : FormattableString.Invariant($"AND FH_FC_CurrentComponent = {componentParam.ParameterName}"); // SQL strings do not need translation.
			var releaseGroupPKCondition = shouldFilterByReleaseGroup ? FormattableString.Invariant($"AND FH_GG_ReleaseGroup = {releaseGroupParam.ParameterName}") : string.Empty; // SQL strings do not need translation.

			var (supersetSql, supersetParams) = GetSupersetSql(addAndIfRequired: true);
			return (FormattableString.Invariant($@"
SELECT 
	COALESCE(Value, 0) Value,
	FH_FC_CurrentComponent Component,
	FH_GG_ReleaseGroup ReleaseGroup
FROM 
(
	SELECT 
		{valueSelectStatement} Value,
		{componentPKString} FH_FC_CurrentComponent,
		{releaseGroupPKString} FH_GG_ReleaseGroup
	FROM
	(
		SELECT {columnToSelectForAllWorkflows}
		FROM
		(
			SELECT * FROM dbo.ProcessHeader
			WHERE 1 = 1
				{componentPKCondition}
				{releaseGroupPKCondition}
				{supersetSql}
				{GetSectionWorkflowFilterClause(addAndIfRequired: true)}
				{AcceptabilityBandParameters.GetAdditionalUnionClauses()}
		) h
	) AllWorkflows
) HeadersCount
WHERE Value is not null
"), new ZSqlParameter[] { componentParam, releaseGroupParam }.Concat(supersetParams).WhereNotNull().ToArray());
		}

		protected override (string, ZSqlParameter[]) GetSupersetSql(bool isReleaseGroupPredicate = false, string tableAlias = null, bool addAndIfRequired = true)
		{
			var supersetFilter = Band.GetSupersetFilterQuery();
			var query = GetParameterisedQuery(supersetFilter);
			var supersetSql = query.ParameterisedQueryText;

			if (isReleaseGroupPredicate && !string.IsNullOrEmpty(tableAlias))
			{
				var r = new Regex("[A-Z0-9]{2}_([A-Za-z0-9]|_)+"); // Horrible field name regex.

				var matches = r.Matches(supersetSql).OfType<Match>().Select(x => x.Value).ToArray().Distinct();

				foreach (var match in matches.Where(x => x.StartsWith(ProcessHeaderSchema.Constants.Prefix, StringComparison.Ordinal)))
				{
					supersetSql = supersetSql.Replace(match, tableAlias + "." + match);
				}
			}

			return ((addAndIfRequired ? "AND " : string.Empty) + supersetSql, query.Parameters);
		}

		protected override bool ShouldConvertNullResultToZero => true;
	}
}
