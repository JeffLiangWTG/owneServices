using System;
using System.Globalization;

using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public static class DataTransformationHelper
	{
		public static string GetSqlToSetTransformHasAlreadyRunFlag(string reference)
		{
			return $"INSERT dbo.StmData (SD_PK, SD_Name) VALUES (newid(), '{reference}')";
		}

		public static bool HasTransformPreviouslyRun(string reference)
		{
			using (var command = Db.Connection.Command("SELECT CASE WHEN EXISTS(SELECT * FROM dbo.StmData WHERE SD_Name = @SD_Name) THEN 1 ELSE 0 END AS HasTransformationRun"))
			{
				command.AddParameterBasedOnDbColumn("@SD_Name", reference, StmDataSchema.SD_Name);
				return (int)command.ExecuteScalar() > 0;
			}
		}

		public static IDisposable SuspendInsertAuditTriggerIfExists(string tableName)
		{
			return SuspendTriggerIfExists($"TG_{tableName}_AuditDetailsAreNotMissing_Insert", tableName);
		}

		public static IDisposable SuspendTriggerIfExists(string triggerName, string tableName)
		{
			return new DisposableAction(
				() => Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{0}') BEGIN DISABLE TRIGGER {0} ON {1} END", triggerName, tableName)),
				() => Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{0}') BEGIN ENABLE TRIGGER {0} ON {1} END", triggerName, tableName)));
		}

		public static IDisposable SuspendTrigger(string triggerName, string tableName)
		{
			return new DisposableAction(
				() => Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "DISABLE TRIGGER {0} ON {1}", triggerName, tableName)),
				() => Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "ENABLE TRIGGER {0} ON {1}", triggerName, tableName)));
		}

		public static IDisposable SuspendFkIfExists(string tableSchema, string tableName, string fkName)
		{
			var fkScript = "IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = '{0}') ALTER TABLE [{1}].[{2}] {3} CONSTRAINT [{0}]";

			return new DisposableAction(
				() => Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, fkScript, fkName, tableSchema, tableName, "NOCHECK")),
				() => Db.Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, fkScript, fkName, tableSchema, tableName, "CHECK"))
			);
		}

		public static IDisposable SuspendConstraintCheckingIfExists(DbConnection connection, string tableSchema, string tableName, string constraintName)
		{
			var fkScript = @"
			IF (OBJECT_ID('{0}', 'C') IS NOT NULL)
			BEGIN
				ALTER TABLE [{1}].[{2}]
				{3} CONSTRAINT [{0}]
			END";

			return new DisposableAction(
				() => connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, fkScript, constraintName, tableSchema, tableName, "NOCHECK")),
				() => connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, fkScript, constraintName, tableSchema, tableName, "CHECK"))
			);
		}

		public static string GetLeafTableShrinkingSQL(string schemaName, string leafTableName, string leafTableColumnName, bool isLeafTableColumnNumeric, string targetTableName, string targetAddInfoColumnName)
		{
			var schemaNameAndTargetTable = $"{schemaName}.{targetTableName}";
			var leafTablePrefix = $"{CargoWise.Schema.Schema.GetPrefixFromColumnName(leafTableColumnName)}_";
			var targetTablePrefix = $"{CargoWise.Schema.Schema.GetPrefixFromColumnName(targetAddInfoColumnName)}_";

			var addInfoName = leafTableColumnName.Replace(leafTablePrefix, string.Empty);

			var whereClause = isLeafTableColumnNumeric ? $"child_table.{leafTableColumnName} > 0" : $"child_table.{leafTableColumnName} <> ''";

			var indexOfUnderscore = targetAddInfoColumnName.IndexOf('_');
			var prefix = targetAddInfoColumnName.Substring(0, indexOfUnderscore);

			var updateSql = $@"
UPDATE {schemaNameAndTargetTable}
SET
	{prefix}_SystemLastEditTimeUtc = GETUTCDATE(),
	{prefix}_SystemLastEditUser = '~BP',
	{targetAddInfoColumnName} = CASE
		WHEN LEN({targetAddInfoColumnName})>0 THEN CONCAT({targetAddInfoColumnName},'*','{addInfoName}=',child_table.{leafTableColumnName})
		ELSE CONCAT('{addInfoName}=',child_table.{leafTableColumnName})
	END
FROM {schemaNameAndTargetTable}
	INNER JOIN {schemaName}.{leafTableName} AS child_table ON {leafTablePrefix}{targetTablePrefix.TrimEnd('_')}={targetTablePrefix}PK AND {leafTablePrefix}ClusterKey={targetTablePrefix}ClusterKey
	CROSS APPLY {schemaName}.csfn_GetAddInfoValueFromCodeInlineToReturnEmptyIfNull({targetAddInfoColumnName}, '{addInfoName}') AS {addInfoName}
WHERE {whereClause}
AND {addInfoName}.Value = ''";

			return updateSql;
		}
	}
}
