using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class AddTagRuleRunSqlStrategy : OffloadingTagRuleRunStrategy
	{
		public AddTagRuleRunSqlStrategy(IConnectionProvider connectionProvider, ILogger logger)
			: base(connectionProvider, logger)
		{
		}
		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			return GetWorkflowsToTagQuery(rule, asSubQuery);
		}

		public override long Execute(TagRule rule)
		{
			DropTempTable(ProcessHeadersTempTableName);

			var rowsProcessed = Process(rule);

			return rowsProcessed;
		}

		protected override void ToggleSingleThreadedBit(TagRule rule)
		{
			rule.TGR_RunAddQuerySingleThreaded = !rule.TGR_RunAddQuerySingleThreaded;
		}

		protected override int GetMaxDopNumber(TagRule rule)
		{
			return rule.TGR_RunAddQuerySingleThreaded ? 1 : 0;
		}

		#region Setup

		protected override ITempTableWorker GetWorkflowTempTableWorker(TagRule rule, IDbConnectionForReportingWrapper secondaryConnectionWrapper)
		{
			return new AddTagWorkflowWorker(rule, secondaryConnectionWrapper);
		}

		protected class AddTagWorkflowWorker : WorkflowTempTableWorker
		{
			internal AddTagWorkflowWorker(TagRule rule, IDbConnectionForReportingWrapper connectionWrapper)
				: base(connectionWrapper, rule)
			{
			}

			protected override string CreateTempTableCore()
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)@"
				CREATE TABLE {0}
				(
					{1} uniqueidentifier,
					{2} uniqueidentifier,
					{3} INT NOT NULL IDENTITY(1,1) PRIMARY KEY
				)
				", // SQL statement
				   /*0*/ ((ITempTableWorker)this).TempTableName,
				/*1*/ ProcessHeaderSchema.Constants.PK,
				/*2*/ ProcessHeaderSchema.Constants.FH_ParentId,
				/*3*/ RowNumber
				);
			}

			protected override string TempTableNameCore
			{
				get { return (NoResString)"#WorkflowsToTagTempTable"; } // These are temp table names
			}
		}

		protected class AddTagHeaderToTag : HeaderToTag
		{
			public AddTagHeaderToTag(ZGuid headerPk, ZGuid parentPk)
				: base(headerPk, parentPk)
			{
			}
		}

		#endregion

		#region PrimaryServer

		protected override IDisposable CreateWorkflowsProcessedTempTable(string nextWorkFlowBatchTempTableName)
		{
			var tempTableCreateSql = string.Format(CultureInfo.InvariantCulture, @"
				CREATE TABLE {0}
				(
					{1} uniqueidentifier,
					{2} uniqueidentifier
				)
				", // SQL statement
				   /*0*/ nextWorkFlowBatchTempTableName,
				/*1*/ ProcessHeaderSchema.Constants.PK,
				/*2*/ ProcessHeaderSchema.Constants.FH_ParentId);

			Db.Connection.ExecuteNonQuery(tempTableCreateSql); // Working with temp table

			return new DisposableAction(() =>
			{
				DropTempTable(nextWorkFlowBatchTempTableName);
			});
		}

		const string SingleItemInsertFormat = "( {0}, {1} )"; // This is a sql format

		protected override string CreateInsertQueryForPrimaryServer(ITagDto[] valuesToInsert, string tempTableName)
		{
			var valuesInsertClause = string.Join(", ", valuesToInsert.Select(h => string.Format(CultureInfo.InvariantCulture, SingleItemInsertFormat, h.HeaderPK.ToSqlGuid(), h.WorkflowParentID.ToSqlGuid())).ToArray()); // String building Sql

			return string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ({1}, {2}) VALUES {3}", tempTableName, ProcessHeaderSchema.Constants.PK, ProcessHeaderSchema.Constants.FH_ParentId, valuesInsertClause); // String building Sql
		}

		protected override QueryTextAndParameters GetResultsQueryTextAndParameters(TagRule rule, string batchTempTable)
		{
			var sqlParameterCollection = new ZSqlParameterCollection();
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue, rule.TagTemplate.TGL_Magnitude, TagLinkSchema.TGL_Magnitude);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesNow, ZDateTime.Now, ProcessHeaderSchema.FH_SystemLastEditTimeUtc);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow, ZDateTime.UtcNow, ProcessHeaderSchema.FH_SystemLastEditTimeUtc);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagRulePK, rule.PK, TagRuleSchema.PK);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK, rule.TagTemplate.TGL_TGM_Magnitude, TagMagnitudeSchema.PK);

			var sql = string.Format(CultureInfo.InvariantCulture, AddTagRuleSQL,
				/*0*/ TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleSQL,
				/*1*/ rule.TGR_Name, // should appear in comments only
				/*2*/ TagRuleRunStrategyBaseConstants.ParameterNames.TagRulePK,
				/*3*/ ProcessHeadersTempTableName,
				/*4*/ batchTempTable,
				/*5*/ TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesNow,
				/*6*/ TagRuleRunStrategyBaseConstants.ParameterNames.TagLinkMagnitudeValue,
				/*7*/ TagRuleRunStrategyBaseConstants.ParameterNames.TagMagnitudePK,
				/*8*/ Db.DatabaseName,
				/*9*/ TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow);

			return new QueryTextAndParameters(sql, sqlParameterCollection);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "These are temp table names")]
		const string ProcessHeadersTempTableName = "#ProcessHeadersToHaveExclusiveTagsRemoved";

		#endregion

		#region SecondaryServer

		protected override QueryTextAndParameters GetWorkflowsToInsertQueryTextAndParameters(TagRule rule, string tempTableName, int maxDoPNumber)
		{
			var workflowsToTagQuery = GetAffectedWorkflowsQuery(rule);
			if (workflowsToTagQuery.IsNoResultQuery)
			{
				throw new NoResultQueryException();
			}
			var dataQuery = workflowsToTagQuery.ParameterisedText;

			var sql = string.Format(CultureInfo.InvariantCulture, AddTagRuleWorkflowSelectQuery,
				/*0*/ TagRuleRunStrategyBaseConstants.TemplateNames.AddTagRuleWorkflowSelectQuery,
				/*1*/ rule.TGR_Name, // should appear in comments only
				/*2*/ tempTableName,
				/*3*/ dataQuery.ParameterisedQueryText,
				/*4*/ StaticFiltersSQL,
				/*5*/ maxDoPNumber == 1 ? (NoResString)"OPTION (MAXDOP 1)" : string.Empty, // SQL needed to apply MAXDOP option
				/*6*/ Db.DatabaseName);

			return BMQueryParameterisationHelper.ReplaceAutoParamNames(new QueryTextAndParameters(sql, dataQuery.Parameters), TagRuleRunStrategyBaseConstants.ParameterNames.TagRuleCustomParameter);
		}

		protected override bool TryReadRow(IDataReader result, out ITagDto row)
		{
			var processHeaderPK = result[0];
			if (processHeaderPK.GetType() == typeof(DBNull))
			{
				row = null;
				return false;
			}

			var parentPK = result[1];
			row = new AddTagHeaderToTag((Guid)processHeaderPK,
				parentPK.GetType() == typeof(DBNull)
				? ZGuid.Empty
				: (Guid)parentPK);
			return true;
		}

		protected override QueryTextAndParameters RowProcessSql(string tempTableNameWhichContainsWorkflowsToTag, QueryTextAndParameters whereClause)
		{
			return new QueryTextAndParameters(string.Format(CultureInfo.InvariantCulture, "SELECT {2}, {3} FROM {0} WHERE {1}",
					/*0*/ tempTableNameWhichContainsWorkflowsToTag,
					/*1*/ whereClause.QueryText,
					/*2*/ ProcessHeaderSchema.Constants.PK,
					/*3*/ ProcessHeaderSchema.Constants.FH_ParentId
				), whereClause.Parameters); // This is a sql format
		}

		#endregion

		#region SecondaryServerSql

		const string AddTagRuleWorkflowSelectQuery = @"
--
-- Query Template: {0}
-- Database:       {6}
-- RULE NAME:      {1}
--

INSERT INTO {2} (FH_PK, FH_ParentId)
SELECT a.FH_PK, a.FH_ParentId
FROM dbo.ProcessHeader a
WHERE {3}
{4}
{5};

SELECT @@ROWCOUNT;
"; // This is the base sql that will insert. We don't care about column smells because we are manipulating at a db level.

		#endregion

		#region PrimaryServerSql

		const string AddTagRuleSQL = @"
--
-- Query Template: {0}
-- Database:       {8}
-- RULE NAME:      {1}
--

WITH ExclusiveTagDef AS 
(
	SELECT TOP 1 TGD_PK, TGM_RuleRunSequence
		FROM dbo.TagRule 
			INNER JOIN dbo.TagLink  ON TGL_ParentId = TGR_PK AND TGL_ParentTableCode = 'TGR'
			JOIN dbo.TagMagnitude  ON TGL_TGM_Magnitude = TGM_PK
			JOIN dbo.TagDefinition  ON TGM_TGD_Tag = TGD_PK
		WHERE TGD_IsExclusive = 1 AND TGR_PK = {2}
)
SELECT ActualLinksToDelete.TGL_PK, ActualLinksToDelete.TGL_ParentId, TagMag.TGM_Code, TagMag.TGM_Description, TagDef.TGD_Code, TagDef.TGD_Description, FH_PK, FH_ParentId
INTO {3} 
FROM dbo.TagLink ActualLinksToDelete
	INNER JOIN 
	(
		(
			SELECT TGL_PK 
			FROM dbo.TagLink links 
			INNER JOIN {4} headers ON headers.FH_PK = links.TGL_ParentId
			JOIN dbo.TagMagnitude TagMag  ON TGL_TGM_Magnitude = TGM_PK 
			INNER JOIN (
				SELECT * FROM ExclusiveTagDef
				) ExclusiveTagDefs ON TagMag.TGM_TGD_Tag = ExclusiveTagDefs.TGD_PK
			WHERE 
			TGL_ParentTableCode = 'FH'
			AND FH_ParentId IS NOT NULL
			AND TagMag.TGM_RuleRunSequence > ExclusiveTagDefs.TGM_RuleRunSequence
		)
		UNION ALL
		(
			SELECT TGL_PK 
			FROM dbo.TagLink links
				INNER JOIN
				(
					SELECT * FROM {4} headers
					WHERE FH_PK IN
					(
						SELECT TGL_ParentId FROM dbo.TagLink
						LEFT JOIN dbo.TagMagnitude ON TGL_TGM_Magnitude = TGM_PK
						INNER JOIN
							(
								SELECT * FROM ExclusiveTagDef
							) ExclusiveTagDefs ON TagMagnitude.TGM_TGD_Tag = ExclusiveTagDefs.TGD_PK
						GROUP BY TGL_ParentId
						HAVING COUNT(*)> 1
					)
					AND FH_ParentId IS NOT NULL
				) headersWithMultiple ON headersWithMultiple.FH_PK = links.TGL_ParentId
		)

	) TagLinkPksToDelete ON TagLinkPksToDelete.TGL_PK = ActualLinksToDelete.TGL_PK
	INNER JOIN {4} headers ON headers.FH_PK = ActualLinksToDelete.TGL_ParentId
	JOIN dbo.TagMagnitude TagMag ON TGL_TGM_Magnitude = TGM_PK 
	JOIN dbo.TagDefinition TagDef ON TGM_TGD_Tag = TGD_PK

DELETE dbo.TagLink FROM dbo.TagLink a join {3} b on a.TGL_PK = b.TGL_PK

INSERT INTO 
	dbo.StmALog 
		(SL_PK,
		SL_Table,
		SL_Parent,
		SL_Reference,
		SL_EventTime,
		SL_EventTimeUtc,
		SL_GS_NKUser,
		SL_SE_NKEvent,
		SL_FireWorkflow)
SELECT 
	NEWID(),
	'ProcessHeader',
	FH_PK,
	'|ACT=DEL|GRP=' + TGD_Code + '|RUL=' + LOWER(CONVERT(nvarchar(36), {2})) + '|TAG=' + TGM_Code,
	{5},
	{9},
	'~BP',
	'TAG',
	1
FROM {3}
WHERE TGM_Code IS NOT NULL

CREATE TABLE #TaggedWorkflows
(
	PK UNIQUEIDENTIFIER
);

INSERT INTO
	dbo.TagLink 
		(TGL_PK,
		TGL_Description,
		TGL_TGM_Magnitude,
		TGL_Magnitude,
		TGL_RemovedTimeUtc,
		TGL_GS_NKRemovedBy,
		TGL_ParentId,
		TGL_ParentTableCode,
		TGL_SystemCreateTimeUtc,
		TGL_SystemCreateUser,
		TGL_SystemLastEditTimeUtc,
		TGL_SystemLastEditUser)
OUTPUT INSERTED.TGL_ParentId INTO #TaggedWorkflows
SELECT 
	NEWID(), 
	TagTemplate.TGL_Description, 
	TagTemplate.TGL_TGM_Magnitude,
	{6},
	NULL, 
	'', 
	FH_PK, 
	'FH', 
	{5},
	'~BP', 
	{5},
	'~BP'
FROM {4}
CROSS APPLY
(
	SELECT TOP 1 *
	FROM dbo.TagLink Template
	JOIN dbo.TagRule on TGR_PK = Template.TGL_ParentId AND Template.TGL_ParentTableCode = 'TGR'
	WHERE TGR_PK = {2}
) TagTemplate
WHERE FH_ParentId IS NOT NULL
AND FH_PK NOT IN 
(SELECT TGL_ParentId FROM dbo.TagLink WHERE TGL_TGM_Magnitude = {7})

SELECT @@ROWCOUNT

DECLARE @UpdatedPKs Table (FH_PK uniqueIdentifier)
INSERT @UpdatedPKs 
	SELECT FH_PK FROM {4}	
	UNION ALL SELECT headers.FH_PK FROM dbo.ProcessHeader headers with (NOLOCK)
	JOIN {4} taggedHeaders ON taggedHeaders.FH_PK = headers.FH_FH_ParentHeader
UPDATE dbo.ProcessHeader
	SET FH_SystemLastEditTimeUtc = {5}
FROM dbo.ProcessHeader WITH (INDEX([PK_UX__FH_PK]))
WHERE FH_PK IN (SELECT FH_PK FROM @UpdatedPKs)

INSERT INTO 
	dbo.StmALog 
		(SL_PK,
		SL_Table,
		SL_Parent,
		SL_Reference,
		SL_EventTime,
		SL_EventTimeUtc,
		SL_GS_NKUser,
		SL_SE_NKEvent,
		SL_FireWorkflow)
SELECT 
	NEWID(),
	'ProcessHeader',
	FH_PK,
	'|ACT=ADD|GRP=' + TGD_Code + '|RUL=' + LOWER(CONVERT(nvarchar(36), {2})) + '|TAG=' + TGM_Code,
	{5},
	{9},
	'~BP',
	'TAG',
	1
FROM {4}
CROSS APPLY
(
	SELECT TOP 1 * 
	FROM dbo.TagLink Template
	JOIN dbo.TagRule on TGR_PK = Template.TGL_ParentId AND Template.TGL_ParentTableCode = 'TGR'
	JOIN dbo.TagMagnitude ON TGL_TGM_Magnitude = TGM_PK
	JOIN dbo.TagDefinition ON TGM_TGD_Tag = TGD_PK
	WHERE TGR_PK = {2}
) TagTemplate
WHERE TGM_Code IS NOT NULL
AND FH_PK IN (SELECT PK FROM #TaggedWorkflows)


DELETE FROM {4} -- delete all this batch's records
DROP TABLE #TaggedWorkflows
DROP TABLE {3}
"; // This is the base sql that will insert. We don't care about column smells because we are manipulating at a db level.
		#endregion
	}
}
