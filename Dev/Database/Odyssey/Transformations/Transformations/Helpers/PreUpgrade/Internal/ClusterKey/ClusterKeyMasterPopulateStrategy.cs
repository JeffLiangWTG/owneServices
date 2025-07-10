using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformations
{
	abstract class ClusterKeyMasterPopulateStrategy
	{
		protected ClusterKeyMasterPopulateStrategy(IUpgradeTaskWorkflowLogger logger, UpgradeMode upgradeMode)
		{
			this.logger = logger;
			var batchSize = ClusterKeyDbHelper.GetSqlBatchSize(upgradeMode);
			sqlBatchTopClause = (batchSize == long.MaxValue) ? "" : $"TOP ({batchSize})";
		}

		readonly IUpgradeTaskWorkflowLogger logger;
		readonly string sqlBatchTopClause;

		public int PopulateClusterKeys(int maxClusterKey)
		{
			logger.ShowInfoMessage($"Populating cluster key column {ClusterKeyColumn}");
			CreateSupportingIndex();

			var cmd = GetScriptToPopulateClusterKeyValuesInSequence(maxClusterKey);
			RunUpdateInBatchesUntilCompletion(cmd);
			var maxValueOnThisTable = ClusterKeyDbHelper.GetMaxClusterKeyValue(Db.Connection, TableName, ClusterKeyColumn);

			return Math.Max(maxClusterKey, maxValueOnThisTable);
		}

		void CreateSupportingIndex()
		{
			ClusterKeyDbHelper.CreateSupportingIndexIfNotExists(Db.Connection, logger, TableName, IndexName, ClusterKeyColumn, includeColumn: null, filter: AdditionalFilter);
		}

		string GetScriptToPopulateClusterKeyValuesInSequence(int maxClusterKey)
		{
			string additionalUpdateFilter = string.IsNullOrWhiteSpace(AdditionalFilter) ? "" : $"AND {AdditionalFilter}";
			var indexOfUnderscore = ClusterKeyColumn.IndexOf('_');
			var prefix = ClusterKeyColumn.Substring(0, indexOfUnderscore);
			var updateColumns = @$",
			{prefix}_SystemLastEditTimeUtc = GETUTCDATE(),
			{prefix}_SystemLastEditUser = '~BP'";
			var additionalSelectColumns = @$"
			{prefix}_SystemLastEditTimeUtc,
			{prefix}_SystemLastEditUser,";

#if DEBUG
			var exclusions = new List<string> { "Z0", "ZD1" };
			if (exclusions.Contains(prefix))
			{
				updateColumns = "";
				additionalSelectColumns = "";
			}
#endif
			return $@"
			DECLARE @maxKeyFromOtherTables INT = {maxClusterKey};
			DECLARE @maxKey INT = ISNULL((SELECT MAX({ClusterKeyColumn}) FROM {TableName}), 0);

			IF (@maxKey < @maxKeyFromOtherTables)
			BEGIN
				SET @maxKey = @maxKeyFromOtherTables;
			END;

			UPDATE {sqlBatchTopClause} T 
				SET
					{ClusterKeyColumn} = ClusterKeyValue{updateColumns}				
			FROM
			(
				SELECT
					{additionalSelectColumns}
					{ClusterKeyColumn}, 
					ClusterKeyValue = ROW_NUMBER() OVER (ORDER BY(SELECT null)) + @maxKey
				FROM [{SqlSchemaName}].[{TableName}] WITH (FORCESEEK, INDEX({IndexName}))
				WHERE {ClusterKeyColumn} = 0
				{additionalUpdateFilter}
			) T;
			SELECT @@ROWCOUNT;";
		}

		void RunUpdateInBatchesUntilCompletion(string cmd)
		{
			int updates;

			do
			{
				updates = Db.Connection.ExecuteScalar<int>(cmd);

				if (updates > 0)
				{
					logger.ShowInfoMessage($"\t: {updates} rows updated in {TableName}");
				}
			}
			while (updates > 0);
		}

		protected abstract string TableName { get; }
		protected abstract string SqlSchemaName { get; }
		protected abstract string ClusterKeyColumn { get; }
		protected abstract string IndexName { get; }
		protected virtual string AdditionalFilter => string.Empty;
	}

	class TopLevelClusterKeyMasterPopulateStrategy : ClusterKeyMasterPopulateStrategy
	{
		public TopLevelClusterKeyMasterPopulateStrategy(IUpgradeTaskWorkflowLogger logger, UpgradeMode upgradeMode, SchemaPKColumn pkColumn) : base(logger, upgradeMode)
		{
			this.pkColumn = pkColumn;
		}

		readonly SchemaPKColumn pkColumn;

		protected override string TableName => pkColumn.TableName;
		protected override string SqlSchemaName => pkColumn.TableSchema.SqlSchemaName;
		protected override string ClusterKeyColumn => pkColumn.ColumnPrefix + CargoWise.Schema.Schema.ClusterKeyColumnSuffix;
		protected override string IndexName => $"IX_{ClusterKeyColumn}";
	}

	class MidLevelClusterKeyMasterPopulateStrategy : ClusterKeyMasterPopulateStrategy
	{
		public MidLevelClusterKeyMasterPopulateStrategy(IUpgradeTaskWorkflowLogger logger, UpgradeMode upgradeMode, KeyDefinition definition) : base(logger, upgradeMode)
		{
			this.definition = definition;
		}

		readonly KeyDefinition definition;

		protected override string TableName => definition.ChildTableName;
		protected override string SqlSchemaName => definition.PKColumn.TableSchema.SqlSchemaName;
		protected override string ClusterKeyColumn => definition.ChildClusterKey;
		protected override string IndexName => $"IX_{ClusterKeyColumn}_Null_{definition.ChildFkColumnName}";
		protected override string AdditionalFilter => definition.ChildColumn.IsNullable ? $"{definition.ChildFkColumnName} IS NULL" : base.AdditionalFilter;
	}
}
