using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformations
{
	class OnlineClusterKeyWorkerPopulateStrategy : ClusterKeyWorkerPopulateStrategy
	{
		public OnlineClusterKeyWorkerPopulateStrategy(IUpgradeTaskWorkflowLogger logger, KeyDefinition ckDefinition, UpgradeMode upgradeMode) : base(logger, ckDefinition)
		{
			this.upgradeMode = ValidateUpgradeMode(upgradeMode);
			commandText = GetCommandTextForInitialisation(ckDefinition);
		}

		UpgradeMode ValidateUpgradeMode(UpgradeMode upgradeMode)
		{
			if (upgradeMode == UpgradeMode.Offline)
			{
				throw new ArgumentException($"UpgradeMode [{upgradeMode}] is not valid for {this.GetType().Name}");
			}

			return upgradeMode;
		}

		readonly string commandText;
		readonly UpgradeMode upgradeMode;

		protected override void RunUpdateInBatchesUntilCompletion()
		{
			int updates;
			Guid lastUpdatedFk = Guid.Empty;

			do
			{
				(updates, lastUpdatedFk) = UpdateBatchOfClusterKeys(lastUpdatedFk);

				if (updates > 0)
				{
					logger.ShowInfoMessage($"\t: {updates} rows updated in {ckDefinition.ChildTableName} (last updated FK: {lastUpdatedFk})");
				}
			}
			while (updates > 0);
		}

		(int rowsUpdated, Guid lastUpdatedFk) UpdateBatchOfClusterKeys(Guid previousLastUpdatedFk)
		{
			int rowsUpdated = 0;
			Guid lastUpdatedFk = Guid.Empty;

			Db.Connection.ExecuteReader(
				commandText,
				(cmd) =>
				{
					cmd.AddParameter("@BatchSize", SqlDbType.BigInt, ClusterKeyDbHelper.GetSqlBatchSize(upgradeMode));
					cmd.AddParameter("@PreviousLastUpdatedFk", SqlDbType.UniqueIdentifier, previousLastUpdatedFk);
				},
				(reader) =>
				{
					rowsUpdated = reader.GetInt32(0);
					lastUpdatedFk = (rowsUpdated > 0) ? reader.GetGuid(1) : Guid.Empty;
				}
			);

			return (rowsUpdated, lastUpdatedFk);
		}

		string GetCommandTextForInitialisation(KeyDefinition definition)
		{
			var childTable = definition.ChildTableName;
			var childFkColumn = definition.ChildFkColumnName;
			var childCkColumn = definition.ChildClusterKey;
			var childSystemLastEditTimeColumn = definition.ChildColumn.TableSchema.PK.ColumnPrefix + "_SystemLastEditTimeUtc";
			var childSystemLastEditUserColumn = definition.ChildColumn.TableSchema.PK.ColumnPrefix + "_SystemLastEditUser";
			var parentTable = definition.ParentTableName;
			var parentPkColumn = definition.ParentPkColumnName;
			var parentCkColumn = definition.ParentClusterKey;

			var auditTrail = $", {childSystemLastEditTimeColumn} = GETUTCDATE(), {childSystemLastEditUserColumn} = '~BP'";
			var auditSelect = $" {childSystemLastEditTimeColumn}, {childSystemLastEditUserColumn},";

			if (!ShouldAddAuditTrail(childTable))
			{
				auditTrail = string.Empty;
				auditSelect = string.Empty;
			}

			return $@"
				DECLARE @LastFk UNIQUEIDENTIFIER;

				UPDATE T
					SET
						{childCkColumn} = {parentCkColumn},
						@LastFk = LastFk {auditTrail}
					FROM
					(
						SELECT TOP(@BatchSize)
							{childCkColumn},
							{parentCkColumn},
							{auditSelect}
							LastFk = MAX({childFkColumn}) OVER(ORDER BY {childFkColumn})
						FROM
							[{definition.ChildColumn.TableSchema.SqlSchemaName}].[{childTable}]
							INNER JOIN [{definition.PKColumn.TableSchema.SqlSchemaName}].[{parentTable}]
								ON {childFkColumn} = {parentPkColumn}
						WHERE
							{childCkColumn} = 0
							AND {parentCkColumn} <> 0
							AND {childFkColumn} >= @PreviousLastUpdatedFk
						ORDER BY
							{childFkColumn}
					) AS T
					OPTION (OPTIMIZE FOR (@BatchSize = 1));

				SELECT @@ROWCOUNT, @LastFk;
			";
		}
	}
}
