using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Public.Customs
{
	public abstract class BaseMoveLeafTableToAddInfo : DataTransformation
	{
		public override string UserDescription => $"Move {LeafTableName}.* to {MainTableName} AddInfo";

		protected abstract string MainTableName { get; }

		protected abstract string MainTablePrefix { get; }

		protected abstract string LeafTableName { get; }

		protected abstract string LeafTablePrefix { get; }

		string MainTablePKName => MainTablePrefix + "_PK";

		string MainTableClusterKeyName => MainTablePrefix + CargoWise.Schema.Schema.ClusterKeyColumnSuffix;

		string MainTableAddInfoName => MainTablePrefix + "_AddInfo";

		string LeafTableForeignKey => LeafTablePrefix + "_" + MainTablePrefix;

		string LeafTableClusterKeyName => LeafTablePrefix + CargoWise.Schema.Schema.ClusterKeyColumnSuffix;

		protected virtual bool UseClusterKeys => true;

		protected abstract IEnumerable<(string addinfoName, string columnName, SchemaColumnType columnType)> Conversions();

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, LeafTableName)
				&& DbObjectCreator.ColumnExists(Db.Connection, LeafTableName, LeafTableForeignKey)
				&& (!UseClusterKeys || DbObjectCreator.ColumnExists(Db.Connection, LeafTableName, LeafTableClusterKeyName)))
			{
				var conversions = Conversions().Where(c => DbObjectCreator.ColumnExists(Db.Connection, LeafTableName, c.columnName)).ToArray();
				var expressions = new StringBuilder();
				var conditions = new StringBuilder();

				foreach (var conversion in conversions)
				{
					string condition;
					string expression;
					switch (conversion.columnType)
					{
						case SchemaColumnType.Bool:
							condition = $"{conversion.columnName} = 1";
							expression = "'Y'";
							break;
						case SchemaColumnType.Decimal:
						case SchemaColumnType.Int:
						case SchemaColumnType.Short:
							condition = $"{conversion.columnName} <> 0";
							expression = $"CAST({conversion.columnName} AS VARCHAR)";
							break;
						default:
							condition = $"{conversion.columnName} <> ''";
							expression = conversion.columnName;
							break;
					}

					if (conditions.Length > 0)
					{
						conditions.Append("\r\n OR ");
					}
					conditions.Append(condition);

					if (expressions.Length > 0)
					{
						expressions.Append("\r\n + ");
					}
					expressions.Append($"CASE WHEN {condition} THEN '*{conversion.addinfoName}=' + {expression} ELSE '' END");
				}
				var clusterKeyCondition = UseClusterKeys ? $" AND {MainTableClusterKeyName}={LeafTableClusterKeyName}" : "";
				var indexOfUnderscore = MainTableAddInfoName.IndexOf('_');
				var prefix = MainTableAddInfoName.Substring(0, indexOfUnderscore);
				var updateText = $@"{prefix}_SystemLastEditTimeUtc = GETUTCDATE(),
		{prefix}_SystemLastEditUser = '~BP',";
				var sql = $@"
UPDATE {MainTableName}
	SET
		{updateText}
		{MainTableAddInfoName} = {MainTableAddInfoName} + SUBSTRING(
   {expressions}
 , CASE WHEN LEN({MainTableAddInfoName})=0 THEN 2 ELSE 1 END, 4096)
  FROM {LeafTableName}
 INNER JOIN {MainTableName} ON {MainTablePKName}={LeafTableForeignKey}{clusterKeyCondition}
 WHERE {conditions};

TRUNCATE TABLE {LeafTableName};
";
				 Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
