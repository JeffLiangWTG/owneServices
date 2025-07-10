using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class MaintainMagnitudeTagRuleRunStrategy : TagRuleRunStrategyBase
	{
		public MaintainMagnitudeTagRuleRunStrategy(IConnectionProvider connectionProvider, ILogger logger)
			: base(connectionProvider, logger)
		{
		}

		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			if (asSubQuery)
			{
				throw new NotImplementedException();
			}

			var sqlParameterCollection = new ZSqlParameterCollection();
			var workflowsToTagQueryFilter = GetTagRuleParameterizedSql(rule, sqlParameterCollection);

			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK, rule.TagTemplate.TGL_TGM_Magnitude, TagMagnitudeSchema.PK);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue, rule.TagTemplate.TGL_Magnitude, TagLinkSchema.TGL_Magnitude);

			workflowsToTagQueryFilter = string.IsNullOrEmpty(workflowsToTagQueryFilter) ? "1 = 1" : workflowsToTagQueryFilter;
			var getAffectedWorkflowsSql = $@"
--
-- Query Template: {TagRuleRunStrategyBaseConstants.TemplateNames.SelectAffectedProcessHeadersSql}
-- Database:       {Db.DatabaseName}
-- RULE NAME:      {rule.TGR_Name}
--
FH_PK in (Select FH_PK 
FROM dbo.ProcessHeader headers
	JOIN dbo.TagLink LinksToUpdate ON headers.FH_PK = LinksToUpdate.TGL_ParentId
	JOIN dbo.TagMagnitude TagMag ON TGL_TGM_Magnitude = TGM_PK 
	JOIN dbo.TagDefinition TagDef ON TGM_TGD_Tag = TGD_PK
	WHERE
	TGM_PK = {TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK}
	AND TGL_Magnitude != {TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue}
	AND ({workflowsToTagQueryFilter}))
";

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddFilterAndZSQLParameterCollection(getAffectedWorkflowsSql, sqlParameterCollection);

			return query;
		}

		protected override void ToggleSingleThreadedBit(TagRule rule)
		{
			rule.TGR_RunMagnitudeQuerySingleThreaded = !rule.TGR_RunMagnitudeQuerySingleThreaded;
		}

		protected int GetMaxdopNumber(TagRule rule)
		{
			return rule.TGR_RunMagnitudeQuerySingleThreaded ? 1 : 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public override long Execute(TagRule rule)
		{
			var rowsProcessed = 0L;

			var maxdopNumber = GetMaxdopNumber(rule);
			var (sql, parameters) = GetQueryToRun(rule, maxdopNumber);

			var performanceTimestampStart = ZDateTime.UtcNow;

			using (var ruleExecutionCommand = Db.Connection.Command(sql))
			{
				ruleExecutionCommand.AddParameters(parameters);
				rowsProcessed = (int)ruleExecutionCommand.ExecuteScalar(); // Running Sql is quicker than bizo's
			}

			var firstRun = GetFirstRunTime(rule, performanceTimestampStart);
			RunVerification(rule, maxdopNumber, firstRun);

			return rowsProcessed;
		}

		(string sql, ZSqlParameterCollection parameters) GetQueryToRun(TagRule rule, int maxdopNumber)
		{
			var sqlParameterCollection = new ZSqlParameterCollection();
			var workflowsToTagQueryFilter = GetTagRuleParameterizedSql(rule, sqlParameterCollection);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK, rule.TagTemplate.TGL_TGM_Magnitude, TagLinkSchema.TGL_TGM_Magnitude);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue, rule.TagTemplate.TGL_Magnitude, TagLinkSchema.TGL_Magnitude);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow, ZDateTime.UtcNow, ProcessHeaderSchema.FH_SystemLastEditTimeUtc);

			workflowsToTagQueryFilter = string.IsNullOrEmpty(workflowsToTagQueryFilter) ? "1 = 1" : workflowsToTagQueryFilter;
			var options = maxdopNumber == 1 ? (NoResString)"OPTION (MAXDOP 1)" : string.Empty; // SQL needed to apply MAXDOP option

			var sanitizedFilter = DbCommand.SanitizeExecuteAsReaderFlags(workflowsToTagQueryFilter);
			if (sanitizedFilter != workflowsToTagQueryFilter)
			{
				// We run this to protect ourselves from malicious SQL in any custom filter strips.
				Db.Connection.ExecuteNonQuery($@"select null from dbo.ProcessHeader where 1 = 2 and ({workflowsToTagQueryFilter})", // This is SQL.
					c => c.AddParameters(sqlParameterCollection));
				workflowsToTagQueryFilter = sanitizedFilter;
			}

			var sql = $@"
--
-- Query Template: {TagRuleRunStrategyBaseConstants.TemplateNames.UpdateMagnitudeSql}
-- Database:       {Db.DatabaseName}
-- RULE NAME:      {rule.TGR_Name}
--

DECLARE @LinkUpdateTable TABLE (TGL_PK uniqueidentifier, FH_PK uniqueidentifier)

INSERT @LinkUpdateTable
SELECT LinksToUpdate.TGL_PK, FH_PK
FROM dbo.TagLink LinksToUpdate
	JOIN dbo.ProcessHeader headers ON headers.FH_PK = LinksToUpdate.TGL_ParentId
	JOIN dbo.TagMagnitude TagMag ON TGL_TGM_Magnitude = TGM_PK 
	JOIN dbo.TagDefinition TagDef ON TGM_TGD_Tag = TGD_PK
	WHERE
	TGM_PK = {TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK}
	AND TGL_Magnitude != {TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue}
	AND ({workflowsToTagQueryFilter})
	{StaticFiltersSQL}
	{options};

DECLARE @UpdatedPKs Table (FH_PK uniqueIdentifier)
INSERT @UpdatedPKs 
	SELECT FH_PK FROM @LinkUpdateTable
	UNION ALL SELECT headers.FH_PK FROM dbo.ProcessHeader headers with (NOLOCK)
	JOIN @LinkUpdateTable taggedHeaders ON taggedHeaders.FH_PK = headers.FH_FH_ParentHeader
UPDATE dbo.ProcessHeader
	SET FH_SystemLastEditTimeUtc = {TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow}
FROM dbo.ProcessHeader WITH (INDEX([PK_UX__FH_PK]))
WHERE FH_PK IN (SELECT FH_PK FROM @UpdatedPKs)

SELECT @@ROWCOUNT
UPDATE dbo.TagLink
	SET TGL_Magnitude = {TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue}
WHERE TGL_PK IN 
	(SELECT TGL_PK FROM @LinkUpdateTable)
"; // This is the base sql that will insert. We don't care about column smells because we are manipulating at a db level.

			return (sql, sqlParameterCollection);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void RunVerification(TagRule rule, int maxdopNumber, double firstRun)
		{
			if (rule.ShouldRunPerformanceVerification)
			{
				var bitToggled = false;
				var (sql, parameters) = GetQueryToRun(rule, 1 - maxdopNumber);

				var performanceTimestampStart = ZDateTime.UtcNow;

				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameters(parameters);
					command.ExecuteNonQuery();
				}

				var secondRun = (ZDateTime.UtcNow - performanceTimestampStart).TotalMilliseconds;
				if (secondRun * (double)BMSRegistry.Instance.ThreadedQuerySlownessThresholdFactor.Value < firstRun)
				{
					ToggleSingleThreadedBit(rule);
					bitToggled = true;
				}
				rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow;
				WriteVerificationLog(rule, bitToggled, 1 - maxdopNumber);
			}
		}
	}
}
