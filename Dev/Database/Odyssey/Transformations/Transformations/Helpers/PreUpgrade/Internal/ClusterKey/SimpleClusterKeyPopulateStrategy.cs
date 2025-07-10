using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Transformations
{
	class SimpleClusterKeyPopulateStrategy : ClusterKeyWorkerPopulateStrategy
	{
		public SimpleClusterKeyPopulateStrategy(IUpgradeTaskWorkflowLogger logger, KeyDefinition definition) : base(logger, definition)
		{
			commandText = GetCommandTextForInitialisation(definition);
		}

		readonly string commandText;

		protected override void RunUpdateInBatchesUntilCompletion()
		{
			int updates;

			do
			{
				updates = Db.Connection.ExecuteScalar<int>(commandText);

				if (updates > 0)
				{
					logger.ShowInfoMessage($"\t: {updates} rows updated in {ckDefinition.ChildTableName}");
				}
			}
			while (updates > 0);
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

			if (!ShouldAddAuditTrail(childTable))
			{
				auditTrail = string.Empty;
			}

			return $@"
				UPDATE {childTable}
				SET	{childCkColumn} = {parentCkColumn} {auditTrail}
				FROM
					[{definition.ChildColumn.TableSchema.SqlSchemaName}].[{childTable}]
					INNER JOIN [{definition.PKColumn.TableSchema.SqlSchemaName}].[{parentTable}]
						ON {childFkColumn} = {parentPkColumn}
				WHERE
					{childCkColumn} = 0
					AND {parentCkColumn} <> 0;
				SELECT @@ROWCOUNT;";
		}
	}
}
