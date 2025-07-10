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
	public class RemoveTagRuleRunSqlStrategy : OffloadingTagRuleRunStrategy
	{
		public RemoveTagRuleRunSqlStrategy(IConnectionProvider connectionProvider, ILogger logger)
			: base(connectionProvider, logger)
		{
		}

		public bool GetNonMatchingTagLinks { get; set; }

		public override ZQuery GetAffectedWorkflowsQuery(TagRule rule, bool asSubQuery = false)
		{
			if (asSubQuery)
			{
				throw new NotImplementedException();
			}

			return GetWorkflowsDeleteTagLinkQuery(rule, GetNonMatchingTagLinks);
		}

		public override long Execute(TagRule rule)
		{
			return Process(rule);
		}

		protected override void ToggleSingleThreadedBit(TagRule rule)
		{
			rule.TGR_RunRemoveQuerySingleThreaded = !rule.TGR_RunRemoveQuerySingleThreaded;
		}

		protected override int GetMaxDopNumber(TagRule rule)
		{
			return rule.TGR_RunRemoveQuerySingleThreaded ? 1 : 0;
		}

		#region Setup

		protected override ITempTableWorker GetWorkflowTempTableWorker(TagRule rule, IDbConnectionForReportingWrapper secondaryConnectionWrapper)
		{
			return new RemoveTagWorkflowWorker(rule, secondaryConnectionWrapper);
		}

		protected class RemoveTagWorkflowWorker : WorkflowTempTableWorker
		{
			internal RemoveTagWorkflowWorker(TagRule rule, IDbConnectionForReportingWrapper connectionWrapper)
				: base(connectionWrapper, rule)
			{
			}

			protected override string CreateTempTableCore()
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)@"
				CREATE TABLE {0}
				(
					{1} INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
					{2} uniqueidentifier,
					{3} uniqueidentifier,
					{4} varchar(3),
					{5} varchar(3),
					{6} uniqueidentifier,
					{7} uniqueidentifier
				)
				", // SQL statement
				   /*0*/ ((ITempTableWorker)this).TempTableName,
				/*1*/ RowNumber,
				/*2*/ TagLinkSchema.Constants.PK,
				/*3*/ TagLinkSchema.Constants.TGL_ParentId,
				/*4*/ TagMagnitudeSchema.Constants.TGM_Code,
				/*5*/ TagDefinitionSchema.Constants.TGD_Code,
				/*6*/ ProcessHeaderSchema.Constants.PK,
				/*7*/ ProcessHeaderSchema.Constants.FH_ParentId
				);
			}

			protected override string TempTableNameCore
			{
				get { return (NoResString)"#WorkflowsToUnTagTempTable"; } // These are temp table names
			}
		}

		protected class RemoveTagHeaderToTag : HeaderToTag
		{
			readonly ZString definitionCode;
			readonly ZString tagCode;
			readonly ZGuid tagLinkParentId;
			readonly ZGuid tagLinkPk;

			public RemoveTagHeaderToTag(ZGuid tagLinkPk, ZGuid tagLinkParentId, ZString tagCode, ZString definitionCode, ZGuid headerPk, ZGuid parentPk)
				: base(headerPk, parentPk)
			{
				this.tagLinkPk = tagLinkPk;
				this.tagLinkParentId = tagLinkParentId;
				this.tagCode = tagCode;
				this.definitionCode = definitionCode;
			}

			public ZString DefinitionCode
			{
				get { return definitionCode; }
			}

			public ZString TagCode
			{
				get { return tagCode; }
			}

			public ZGuid TagLinkParentId
			{
				get { return tagLinkParentId; }
			}

			public ZGuid TagLinkPk
			{
				get { return tagLinkPk; }
			}
		}

		#endregion

		#region PrimaryServer

		protected override IDisposable CreateWorkflowsProcessedTempTable(string nextWorkFlowBatchTempTableName)
		{
			DropTempTable(nextWorkFlowBatchTempTableName);

			var tempTableCreateSql = string.Format(CultureInfo.InvariantCulture, @"
				CREATE TABLE {0}
				(
					{1} uniqueidentifier,
					{2} uniqueidentifier,
					{3} varchar(3),
					{4} varchar(3),
					{5} uniqueidentifier,
					{6} uniqueidentifier
				)
				", // SQL statement
				   /*0*/ nextWorkFlowBatchTempTableName,
				/*1*/ TagLinkSchema.Constants.PK,
				/*2*/ TagLinkSchema.Constants.TGL_ParentId,
				/*3*/ TagMagnitudeSchema.Constants.TGM_Code,
				/*4*/ TagDefinitionSchema.Constants.TGD_Code,
				/*5*/ ProcessHeaderSchema.Constants.PK,
				/*6*/ ProcessHeaderSchema.Constants.FH_ParentId
				);

			Db.Connection.ExecuteNonQuery(tempTableCreateSql); // Working with temp table

			return new DisposableAction(() =>
			{
				DropTempTable(nextWorkFlowBatchTempTableName);
			});
		}

		const string SingleItemInsertFormat = "( {0}, {1}, '{2}', '{3}', {4}, {5} )"; // This is a sql format

		protected override string CreateInsertQueryForPrimaryServer(ITagDto[] valuesToInsert, string tempTableName)
		{
			var valuesInsertClause = string.Join(", ", valuesToInsert
					.Cast<RemoveTagHeaderToTag>()
					.Select(h => string.Format(CultureInfo.InvariantCulture,
						SingleItemInsertFormat,
						h.TagLinkPk.ToSqlGuid(),
						h.TagLinkParentId.ToSqlGuid(),
						h.TagCode,
						h.DefinitionCode,
						h.HeaderPK.ToSqlGuid(),
						h.WorkflowParentID.ToSqlGuid()
						)).ToArray());

			return string.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}) VALUES {7}",
				/*0*/ tempTableName,
				/*1*/ TagLinkSchema.Constants.PK,
				/*2*/ TagLinkSchema.Constants.TGL_ParentId,
				/*3*/ TagMagnitudeSchema.Constants.TGM_Code,
				/*4*/ TagDefinitionSchema.Constants.TGD_Code,
				/*5*/ ProcessHeaderSchema.Constants.PK,
				/*6*/ ProcessHeaderSchema.Constants.FH_ParentId,
				/*7*/ valuesInsertClause);
		}

		protected override QueryTextAndParameters GetResultsQueryTextAndParameters(TagRule rule, string batchTempTable)
		{
			var sqlParameterCollection = new ZSqlParameterCollection();
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesNow, ZDateTime.Now, ProcessHeaderSchema.FH_SystemLastEditTimeUtc);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow, ZDateTime.UtcNow, ProcessHeaderSchema.FH_SystemLastEditTimeUtc);
			sqlParameterCollection.Add(TagRuleRunStrategyBaseConstants.ParameterNames.TagRulePK, rule.PK, TagRuleSchema.PK);

			var sql = string.Format(CultureInfo.InvariantCulture, RemoveTagRuleSQL,
						/*0*/ TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagRuleSQL,
						/*1*/ rule.TGR_Name, // should appear in comments only
						/*2*/ batchTempTable,
						/*3*/ TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesNow,
						/*4*/ TagRuleRunStrategyBaseConstants.ParameterNames.TagRulePK,
						/*5*/ Db.DatabaseName,
						/*6*/ TagRuleRunStrategyBaseConstants.ParameterNames.TagRulesUtcNow);

			return new QueryTextAndParameters(sql, sqlParameterCollection);
		}

		#endregion

		#region SecondaryServer

		protected override QueryTextAndParameters GetWorkflowsToInsertQueryTextAndParameters(TagRule rule, string tempTableName, int maxDoPNumber)
		{
			var tagLinksToDeleteQuery = GetTagLinksToDeleteQuery(rule, GetNonMatchingTagLinks);
			if (tagLinksToDeleteQuery.IsNoResultQuery)
			{
				throw new NoResultQueryException();
			}
			var dataQuery = tagLinksToDeleteQuery.ParameterisedText;

			var sql = string.Format(CultureInfo.InvariantCulture, RemoveTagWorkflowSQL,
				/*0*/ TagRuleRunStrategyBaseConstants.TemplateNames.RemoveTagWorkflowSQL,
				/*1*/ rule.TGR_Name, // should appear in comments only
				/*2*/ tempTableName,
				/*3*/ dataQuery.ParameterisedQueryText,
				/*4*/ StaticFiltersSQL,
				/*5*/ maxDoPNumber == 1 ? (NoResString)"OPTION (MAXDOP 1)" : string.Empty, // SQL needed to apply MAXDOP option
				/*6*/ Db.DatabaseName);

			var queryTextAndParameters = new QueryTextAndParameters(sql, dataQuery.Parameters);

			return BMQueryParameterisationHelper.ReplaceAutoParamNames(queryTextAndParameters, TagRuleRunStrategyBaseConstants.ParameterNames.TagRuleCustomParameter);
		}

		protected override bool TryReadRow(IDataReader result, out ITagDto row)
		{
			var tagLinkPK = result[0];
			var tagLinkParentID = result[1];
			var processHeaderPK = result[4];
			if (tagLinkPK.GetType() == typeof(DBNull) ||
				tagLinkParentID.GetType() == typeof(DBNull) ||
				processHeaderPK.GetType() == typeof(DBNull))
			{
				row = null;
				return false;
			}

			var parentPK = result[5];
			row = new RemoveTagHeaderToTag((Guid)tagLinkPK, (Guid)tagLinkParentID, result.GetString(2), result.GetString(3), (Guid)processHeaderPK,
				parentPK.GetType() == typeof(DBNull)
				? ZGuid.Empty
				: (Guid)parentPK);
			return true;
		}

		protected override QueryTextAndParameters RowProcessSql(string tempTableNameWhichContainsWorkflowsToTag, QueryTextAndParameters whereClause)
		{
			return new QueryTextAndParameters(string.Format(CultureInfo.InvariantCulture, "SELECT {2}, {3}, {4}, {5}, {6}, {7} FROM {0} WHERE {1}",
					/*0*/ tempTableNameWhichContainsWorkflowsToTag,
					/*1*/ whereClause.QueryText,
					/*2*/ TagLinkSchema.Constants.PK,
					/*3*/ TagLinkSchema.Constants.TGL_ParentId,
					/*4*/ TagMagnitudeSchema.Constants.TGM_Code,
					/*5*/ TagDefinitionSchema.Constants.TGD_Code,
					/*6*/ ProcessHeaderSchema.Constants.PK,
					/*7*/ ProcessHeaderSchema.Constants.FH_ParentId
				), whereClause.Parameters); // This is a sql format
		}

		#endregion

		#region SecondaryServerSql

		const string RemoveTagWorkflowSQL = @"
--
-- Query Template: {0}
-- Database:       {6}
-- RULE NAME:      {1}
--

INSERT INTO {2} (TGL_PK, TGL_ParentId, TGM_Code, TGD_Code, FH_PK, FH_ParentId)
SELECT AllLinks.TGL_PK, AllLinks.TGL_ParentId, TagMag.TGM_Code, TagDef.TGD_Code, FH_PK, FH_ParentId
FROM dbo.TagLink AllLinks
	INNER JOIN 
	( 
		SELECT TGL_PK 
		FROM dbo.TagLink
		WHERE {3}
	) TagLinksToRemove ON TagLinksToRemove.TGL_PK = AllLinks.TGL_PK
	JOIN dbo.TagMagnitude TagMag ON AllLinks.TGL_TGM_Magnitude = TGM_PK
	JOIN dbo.ProcessHeader ON FH_PK = AllLinks.TGL_ParentId
	JOIN dbo.TagDefinition TagDef ON TagMag.TGM_TGD_Tag = TagDef.TGD_PK
WHERE 
AllLinks.TGL_ParentTableCode = 'FH'
{4}
{5}



SELECT @@ROWCOUNT";
		#endregion

		#region PrimaryServerSql

		const string RemoveTagRuleSQL = @"
--
-- Query Template: {0}
-- Database:       {5}
-- RULE NAME:      {1}
--

DELETE dbo.TagLink FROM dbo.TagLink a JOIN {2} b ON a.TGL_PK = b.TGL_PK
SELECT @@ROWCOUNT

DECLARE @UpdatedPKs Table (FH_PK uniqueIdentifier)
INSERT @UpdatedPKs 
	SELECT FH_PK FROM {2}	
	UNION ALL SELECT headers.FH_PK FROM dbo.ProcessHeader headers with (NOLOCK)
	JOIN {2} taggedHeaders ON taggedHeaders.FH_PK = headers.FH_FH_ParentHeader
UPDATE dbo.ProcessHeader
	SET FH_SystemLastEditTimeUtc = {3}
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
	'|ACT=DEL|GRP=' + TGD_Code + '|RUL=' + LOWER(CONVERT(nvarchar(36), {4})) + '|TAG=' + TGM_Code,
	{3},
	{6},
	'~BP',
	'TAG',
	1
FROM {2}
WHERE TGM_Code IS NOT NULL

DELETE FROM {2} -- delete all this batch's records
"; // This is the base sql that will insert. We don't care about column smells because we are manipulating at a db level.

		#endregion
	}
}
