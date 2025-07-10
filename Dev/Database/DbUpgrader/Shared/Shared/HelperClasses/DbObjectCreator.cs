namespace Enterprise.DbUpgrader.Shared
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using CargoWise.Common;
	using CargoWise.Data;
	using Enterprise.ChangeDataCapture.Common;

	public static class DbObjectCreator
	{
		public struct DbColumn : IEquatable<DbColumn>
		{
			public readonly string ColumnName;
			public readonly string DataType;
			public readonly int CharacterMaximumLength;

			public DbColumn(string columnName, string dataType, int characterMaximumLength)
			{
				ColumnName = columnName;
				DataType = dataType;
				CharacterMaximumLength = characterMaximumLength;
			}

			public static bool operator ==(DbColumn a, DbColumn b) => Equals(a, b);
			public static bool operator !=(DbColumn a, DbColumn b) => !(a == b);

			public bool Equals(DbColumn other) => ColumnName == other.ColumnName && DataType == other.DataType && CharacterMaximumLength == other.CharacterMaximumLength;
			public override bool Equals(object obj) => !(obj == null || !GetType().Equals(obj.GetType())) && Equals((DbColumn)obj);

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = -1118741485;
					hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ColumnName);
					hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DataType);
					hashCode = hashCode * -1521134295 + CharacterMaximumLength.GetHashCode();
					return hashCode;
				}
			}

			public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Column Name: {0} | Data Type: {1} | Character Maximum Length: {2}", new object[] { ColumnName, DataType, CharacterMaximumLength });
		}

		public struct TableDescriptor
		{
			public string TableSchema { get; }
			public string TableName { get; }

			public TableDescriptor(string tableSchema, string tableName)
			{
				TableSchema = Argument.NotNull(tableSchema, nameof(tableSchema));
				TableName = Argument.NotNull(tableName, nameof(tableName));
			}
		}

		public static bool DatabaseExists(DbConnection connection, string dbName)
		{
			using (var cmd = connection.Command("SELECT CONVERT(bit, CASE WHEN DB_ID(@db_name) is NULL THEN 0 ELSE 1 END)"))
			{
				cmd.AddParameter("@db_name", SqlDbType.NVarChar, 128, dbName);
				return Convert.ToBoolean(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		#region Table

		public static bool TableExists(DbConnection connection, string tableName)
		{
			return TableExists(connection, connection.CurrentDatabase, tableName);
		}

		public static bool TableExists(DbConnection connection, string dbName, string tableName, string tableSchema = null)
		{
			var sqlText = String.Format(@"
				IF (OBJECT_ID('[{0}].[{1}].[{2}]', 'U') is null)
					SELECT 0;
				ELSE
					SELECT 1;",
				dbName,
				(String.IsNullOrWhiteSpace(tableSchema) ? Db.SqlDbOwnerSchema : tableSchema),
				tableName
			);

			using (var cmd = connection.Command(sqlText))
			{
				return Convert.ToBoolean(cmd.ExecuteScalar());
			}
		}

		/// <summary>
		/// Creates a new table if it does not exist on a specific database.
		/// </summary>
		public static void CreateTableIfNotExists(DbConnection connection, string dbName, string tableName, string tableCreationScript)
		{
			CreateTableIfNotExists(connection, dbName, Db.SqlDbOwnerSchema, tableName, tableCreationScript);
		}

		/// <summary>
		/// Creates a new table if it does not exist on a specific database.
		/// </summary>
		public static void CreateTableIfNotExists(DbConnection connection, string dbName, string tableSchema, string tableName, string tableCreationScript)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				if (!TableExists(connection, dbName, tableName, tableSchema: tableSchema))
				{
					connection.ExecuteNonQuery(tableCreationScript);
				}
			}
		}

		public static void CreateTableIfNotExists(DbConnection connection, string tableName, string tableCreationScript)
		{
			if (!TableExists(connection, tableName))
			{
				connection.ExecuteNonQuery(tableCreationScript);
			}
		}

		public static void RenameTable(DbConnection connection, string dbName, string schemaName, string curTableName, string newTableName)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture,
				"EXEC [{0}]..sp_rename '{1}.{2}', '{3}'",
				dbName, schemaName, curTableName, newTableName);
			connection.ExecuteNonQuery(sqlText);
		}

		public static void DropTableIfExists(DbConnection connection, string dbName, string tableName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				DropTableIfExists(connection, tableName);
			}
		}

		public static void DropTableIfExists(DbConnection connection, string tableName)
		{
			if (TableExists(connection, tableName))
			{
				connection.ExecuteNonQuery(FormattableString.Invariant($"DROP TABLE {tableName}"));
			}
		}

		public static void DeleteAllRecordsIfTableExists(DbConnection connection, string tableName)
		{
			if (TableExists(connection, tableName))
			{
				connection.ExecuteNonQuery(FormattableString.Invariant($"DELETE FROM {tableName}"));
			}
		}

		#endregion // Table

		public static bool ViewExists(DbConnection connection, string viewName)
		{
			using (var cmd = connection.Command("IF EXISTS (SELECT null FROM sys.views WHERE name = @viewName) SELECT 1; ELSE SELECT 0;"))
			{
				cmd.AddParameter("@viewName", SqlDbType.NVarChar, 128, viewName);
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool ObjectExists(DbConnection connection, string objectName)
		{
			using (var cmd = connection.Command("SELECT COUNT(*) FROM sys.all_objects where name = @objecteName"))
			{
				cmd.AddParameter("@objecteName", SqlDbType.NVarChar, 128, objectName);
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		#region Column

		public static string GetColumnType(DbConnection connection, string tableName, string columnName)
		{
			string sqlText = @"
SELECT TOP 1 typ.Name FROM sys.tables tab 
INNER JOIN sys.columns col ON tab.object_id = col.object_id
INNER JOIN sys.types typ ON typ.system_type_id = col.system_type_id
WHERE tab.name = @tableName AND col.name = @columnName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				var result = cmd.ExecuteScalar();
				return result == DBNull.Value ? null : (string)result;
			}
		}

		public static (string type, short length) GetColumnTypeAndMaxLength(DbConnection connection, string tableName, string columnName)
		{
			(string type, short length) result = (null, 0);

			string sqlText = @"
SELECT TOP 1 typ.Name, col.Max_Length FROM sys.tables tab 
INNER JOIN sys.columns col ON tab.object_id = col.object_id
INNER JOIN sys.types typ ON typ.system_type_id = col.system_type_id
WHERE tab.name = @tableName AND col.name = @columnName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						result.type = reader[0].ToString();
						result.length = (short)reader[1];
					}
				}
			}

			return result;
		}

		public static IEnumerable<string> GetTableColumns(DbConnection connection, string tableName)
		{
			var result = new List<string>();

			string sqlText = @"
SELECT col.name FROM sys.tables tab 
INNER JOIN sys.columns col ON tab.object_id = col.object_id
WHERE tab.name = @tableName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader[0].ToString());
					}
				}
			}

			return result;
		}

		public static IEnumerable<DbColumn> GetExtendedTableColumns(DbConnection connection, string tableName)
		{
			var result = new List<DbColumn>();

			var sql = @"
select column_name, data_type, (case when character_maximum_length is null then -1 else character_maximum_length end) as character_maximum_length
from information_schema.columns
where table_name = @tableName";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new DbColumn((string)reader["column_name"], (string)reader["data_type"], (int)reader["character_maximum_length"]));
					}
				}
			}

			return result;
		}

		public static IEnumerable<string> GetViewColumns(DbConnection connection, string viewName)
		{
			var result = new List<string>();

			using (var cmd = connection.Command("SELECT column_name FROM information_schema.columns WHERE table_name = @viewName"))
			{
				cmd.AddParameter("@viewName", SqlDbType.NVarChar, 128, viewName);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader[0].ToString());
					}
				}
			}

			return result;
		}

		public static IEnumerable<DbColumn> GetExtendedViewColumns(DbConnection connection, string viewName)
		{
			return GetExtendedTableColumns(connection, viewName);
		}

		public static bool ColumnExists(DbConnection connection, string tableName, string columnName)
			=> ColumnExists(connection, new TableDescriptor(Db.SqlDbOwnerSchema, tableName), columnName);

		public static bool ColumnExists(DbConnection connection, TableDescriptor table, string columnName)
			=> ColumnExists(connection, table, columnName, "tables");

		public static bool ViewColumnExists(DbConnection connection, string viewName, string columnName)
			=> ColumnExists(connection, new TableDescriptor(Db.SqlDbOwnerSchema, viewName), columnName, "views");

		static bool ColumnExists(DbConnection connection, TableDescriptor table, string columnName, string objectTypeTableName)
		{
			var sqlText = FormattableString.Invariant($@"
SELECT count(*) FROM sys.{objectTypeTableName} tab 
INNER JOIN sys.columns col ON tab.object_id = col.object_id
INNER JOIN sys.schemas sch ON sch.schema_id = tab.schema_id
WHERE sch.name = @schemaName AND tab.name = @tableName AND col.name = @columnName
");

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, table.TableSchema);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, table.TableName);
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
			}
		}

		public static bool ColumnExists(DbConnection connection, string dbName, string tableSchema, string tableName, string columnName)
			=> ColumnExists(connection, dbName, tableSchema, tableName, "U", columnName);

		public static bool ViewColumnExists(DbConnection connection, string dbName, string viewSchema, string viewName, string columnName)
			=> ColumnExists(connection, dbName, viewSchema, viewName, "V", columnName);

		static bool ColumnExists(DbConnection connection, string dbName, string objectSchema, string objectName, string objectType, string columnName)
		{
			var sql = FormattableString.Invariant($@"
if (EXISTS(SELECT NULL FROM sys.databases WHERE name = @dbName))
begin
	SELECT CONVERT(bit,
		CASE
			WHEN EXISTS(SELECT NULL FROM [{dbName}].sys.columns WHERE object_id = OBJECT_ID('[{dbName}].[{objectSchema}].[{objectName}]', '{objectType}') AND name = @columnName)
				THEN 1
			ELSE 0
		END);
end else
begin
	SELECT CONVERT(bit, 0)
end
");
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@dbName", SqlDbType.NVarChar, 128, dbName);
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				return (bool)cmd.ExecuteScalar();
			}
		}

		public static bool ColumnsExist(DbConnection connection, string dbName, string schemaName, string tableName,
			string[] columnNames)
		{
			var sql = FormattableString.Invariant($@"
SELECT COUNT(*) FROM [{dbName}].sys.columns WHERE object_id = OBJECT_ID('[{dbName}].[{schemaName}].[{tableName}]', 'U') AND name IN ('{string.Join("', '", columnNames)}')");
			var count = connection.ExecuteScalar<int>(sql);
			return count == columnNames.Length;
		}

		public static bool IndexColumnExists(DbConnection connection, string tableName, string indexName, int columnType, string columnName)
		{
			string sqlText = @"
SELECT count(*) FROM sys.tables tab 
INNER JOIN sys.indexes ind ON tab.object_id = ind.object_id
INNER JOIN sys.index_columns indcol ON indcol.object_id = ind.object_id and indcol.index_id = ind.index_id
INNER JOIN sys.columns col ON col.object_id = ind.object_id and col.column_id = indcol.column_id
WHERE tab.name = @tableName AND ind.name = @indexName
AND indcol.is_included_column = @columnType
AND col.Name = @columnName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				cmd.AddParameter("@columnType", SqlDbType.Bit, columnType);
				cmd.AddParameter("@indexName", SqlDbType.NVarChar, 128, indexName);

				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
			}
		}

		public static bool ComputedColumnExists(DbConnection connection, string schemaName, string tableName, string columnName)
		{
			var sqlText = FormattableString.Invariant($@"
SELECT count(*) FROM sys.columns col
WHERE object_id = OBJECT_ID('[{schemaName}].[{tableName}]', 'U') AND col.Name = @columnName AND col.is_computed = 1
");

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
			}
		}

		/// <summary>
		/// Creates a new column in a given table if it does not exist.
		/// Returns TRUE if column already existed or was created.
		/// </summary>
		public static bool CreateColumnIfNotExists(DbConnection connection, TableDescriptor table, string columnName, string columnDataType, string defaultValue = null)
		{
			if (!ColumnExists(connection, table, columnName))
			{
				if (!TableExists(connection, connection.CurrentDatabase, table.TableName, table.TableSchema))
				{
					return false;
				}

				CreateColumn(connection, table, columnName, columnDataType, defaultValue);
			}

			return true;
		}

		/// <summary>
		/// Creates a new column
		/// </summary>
		public static void CreateColumn(DbConnection connection, TableDescriptor table, string columnName, string columnDataType, string defaultValue = null)
		{
			var defaultConstraint = defaultValue != null && defaultValue.Trim().ToLower() != "null"
				? $" CONSTRAINT {GenerateDefaultColumnConstraintName(table.TableName, columnName)} DEFAULT({defaultValue}) NOT"
				: "";

			var sqlText = $"ALTER TABLE [{table.TableSchema}].[{table.TableName}] ADD {columnName} {columnDataType}{defaultConstraint} NULL";

			connection.ExecuteNonQuery(sqlText);
		}

		public static void CreateColumn(DbConnection connection, string tableName, string columnName, string columnDataType, string defaultValue = null)
			=> CreateColumn(connection, new TableDescriptor(Db.SqlDbOwnerSchema, tableName), columnName, columnDataType, defaultValue);

		/// <summary>
		/// Creates a new column in a given table if it does not exist.
		/// Returns TRUE if column already existed or was created.
		/// </summary>
		public static bool CreateColumnIfNotExists(DbConnection connection, string tableName, string columnName, string columnDataType, string defaultValue = null)
			=> CreateColumnIfNotExists(connection, new TableDescriptor(Db.SqlDbOwnerSchema, tableName), columnName, columnDataType, defaultValue);

		public static void CreateColumnIfNotExists(DbConnection connection, string dbName, string tableName, string columnName, string columnDataType, string defaultValue = null)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				CreateColumnIfNotExists(connection, tableName, columnName, columnDataType, defaultValue);
			}
		}

		public static bool RenameColumn(DbConnection connection, string tableName, string oldColumnName, string newColumnName)
		{
			return RenameColumn(connection, Db.SqlDbOwnerSchema, tableName, oldColumnName, newColumnName);
		}

		public static bool RenameColumn(DbConnection connection, string tableSchema, string tableName, string oldColumnName, string newColumnName)
		{
			if (
				newColumnName != oldColumnName
				&& ColumnExists(connection, connection.CurrentDatabase, tableSchema, tableName, oldColumnName)
				&& (
					!ColumnExists(connection, connection.CurrentDatabase, tableSchema, tableName, newColumnName)
					|| newColumnName.Equals(oldColumnName, StringComparison.OrdinalIgnoreCase)
				)
			)
			{
				try
				{
					RenameColumnCore(connection, tableSchema, tableName, oldColumnName, newColumnName);
				}
				catch (SqlException ex)
				{
					if (new DbErrorMatch(ex).ExceptionType == DbErrorType.CannotAlterColumnBecauseItIsReplicated)
					{
						new CdcTable(tableSchema, tableName).DisableCdcInstances(connection);
						RenameColumnCore(connection, tableSchema, tableName, oldColumnName, newColumnName);
					}
					else
					{
						throw;
					}
				}
				return true;
			}
			return false;
		}

		static void RenameColumnCore(DbConnection connection, string tableSchema, string tableName, string oldColumnName, string newColumnName)
		{
			var sqlText = string.Format(@"EXEC sys.sp_rename '{0}.{1}.{2}', '{3}', 'COLUMN'", tableSchema, tableName, oldColumnName, newColumnName);
			connection.ExecuteNonQuery(sqlText);
		}

		public static bool RenameDefaultColumnConstraintIfExists(DbConnection connection, string tableSchema, string tableName, string columnName, string newDefaultName)
		{
			var sql =
$@"SET NOCOUNT ON;

DECLARE @oldName nvarchar(128) =
(
	SELECT
		def.name 
	FROM
		sys.default_constraints AS def
		JOIN sys.columns        AS col ON col.object_id = def.parent_object_id AND col.column_id = def.parent_column_id
	WHERE 1=1
		AND def.parent_object_id = OBJECT_ID(@schemaName + N'.' + @tableName, 'U')
		AND col.name = @columnName
)

if (@oldName <> @defaultName AND OBJECT_ID(@schemaName + N'.' + @defaultName, 'D') is NULL)
begin
	DECLARE @objname nvarchar(776) = @schemaName + N'.' + @oldName;
	EXEC sys.sp_rename @objname, @defaultName, 'OBJECT';
	SELECT 1;
end
else
begin
	SELECT 0;
end;
";
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, tableSchema);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				cmd.AddParameter("@defaultName", SqlDbType.NVarChar, 128, newDefaultName);
				return (int)cmd.ExecuteScalar() != 0;
			}
		}

		public static string GenerateDefaultColumnConstraintName(string tableName, string columnName)
		{
			var result = "DF_" + tableName + "_" + columnName;
			if (result.Length > 128)
			{
				result = result.Substring(0, 128);
			}
			return result;
		}

		#endregion // Column

		public static bool ForeignKeyExists(DbConnection connection, string tableName, string foreignKeyName)
		{
			string sqlText = @"
SELECT count(*) FROM sys.tables tab 
INNER JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
WHERE tab.name = @tableName AND fk.name = @foreignKeyName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@foreignKeyName", SqlDbType.NVarChar, 128, foreignKeyName);
				return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
			}
		}

		public static void CreateForeignKeyIfNotExists(DbConnection connection, string tableName, string foreignKeyName, string foreignKeyCreationScript)
		{
			if (!ForeignKeyExists(connection, tableName, foreignKeyName))
			{
				connection.ExecuteNonQuery(foreignKeyCreationScript);
			}
		}

		public static bool IndexExists(DbConnection connection, string tableName, string indexName)
		{
			string sqlText = @"
SELECT count(*) FROM sys.tables tab 
INNER JOIN sys.indexes ind ON tab.object_id = ind.object_id
WHERE tab.name = @tableName AND ind.name = @indexName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@indexName", SqlDbType.NVarChar, 128, indexName);
				return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
			}
		}

		public static void CreateIndexIfNotExists(DbConnection connection, string tableName, string indexName, string indexCreationScript)
		{
			if (!IndexExists(connection, tableName, indexName))
			{
				connection.ExecuteNonQuery(indexCreationScript);
			}
		}

		public static bool TriggerExists(DbConnection connection, string tableName, string triggerName)
		{
			string sqlText = @"
SELECT count(*) FROM sys.tables tab 
INNER JOIN sys.triggers tg ON tab.object_id = tg.parent_id
WHERE tab.name = @tableName AND tg.name = @triggerName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@triggerName", SqlDbType.NVarChar, 128, triggerName);

				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
			}
		}

		public static string GetInsteadOfDeleteTriggerName(DbConnection connection, string tableSchema, string tableName)
		{
			string sqlText = @"
SELECT tg.Name
FROM sys.triggers tg
INNER JOIN sys.trigger_events tge ON tg.object_id = tge.object_id
WHERE
	1=1
	AND OBJECT_SCHEMA_NAME(tg.parent_id) = @tableSchemaName
	AND OBJECT_NAME(tg.parent_id) = @tableName
	AND tge.type = 3
	AND tg.is_instead_of_trigger = 1;";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@tableSchemaName", SqlDbType.NVarChar, 128, tableSchema);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				var name = cmd.ExecuteScalar();
				return name?.ToString();
			}
		}

		public static void DropTriggerIfExists(DbConnection connection, string triggerName)
		{
			connection.ExecuteNonQuery(FormattableString.Invariant($"DROP TRIGGER IF EXISTS {triggerName}"));
		}

		public static string GetTriggerDefinition(DbConnection connection, string triggerName)
		{
			return connection.ExecuteScalar<string>($@"
SELECT
	TrgDefinition	= def.definition
FROM
	sys.triggers trg
	JOIN sys.sql_modules AS def ON def.object_id = trg.object_id
WHERE trg.name = '{triggerName}'
");
		}

		#region StoredPrecedureExists

		public static int GetParameterCountOfStoredPrecedure(DbConnection connection, string storePrecedureName)
		{
			string sqlText = @"
SELECT count(*) FROM sys.parameters 
INNER join sys.procedures ON parameters.object_id = procedures.object_id 
INNER join sys.types ON parameters.system_type_id = types.system_type_id AND parameters.user_type_id = types.user_type_id
WHERE procedures.name = @storePrecedureName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@storePrecedureName", SqlDbType.NVarChar, 128, storePrecedureName);
				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		public static bool ParameterOfStoredPrecedureExists(DbConnection connection, string storePrecedureName, string parameterName)
		{
			string sqlText = @"
SELECT count(*) FROM sys.parameters 
INNER join sys.procedures ON parameters.object_id = procedures.object_id 
INNER join sys.types ON parameters.system_type_id = types.system_type_id AND parameters.user_type_id = types.user_type_id
WHERE procedures.name = @storePrecedureName AND parameters.name = @parameterName";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@storePrecedureName", SqlDbType.NVarChar, 128, storePrecedureName);
				cmd.AddParameter("@parameterName", SqlDbType.NVarChar, 128, parameterName);

				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
			}
		}

		public static bool ParameterOfStoredPrecedureExistsWithCorrectTypeAndMaxLengh(DbConnection connection, string storePrecedureName, string parameterName, string type, int schemaMaxLength)
		{
			int maxLength;
			type = type.ToUpper(CultureInfo.InvariantCulture);
			if (type == "NVARCHAR")
			{
				maxLength = schemaMaxLength * 2;
			}
			else if (type == "SMALLDATETIME")
			{
				maxLength = 4;
			}
			else if (type == "DATETIME")
			{
				maxLength = 8;
			}
			else if (type == "UNIQUEIDENTIFIER")
			{
				maxLength = 16;
			}
			else
			{
				maxLength = schemaMaxLength;
			}

			var sqlText = @"
SELECT count(*) FROM sys.parameters 
INNER join sys.procedures ON parameters.object_id = procedures.object_id 
INNER join sys.types ON parameters.system_type_id = types.system_type_id AND parameters.user_type_id = types.user_type_id
WHERE procedures.name = @storePrecedureName AND parameters.name = @parameterName AND types.name = @type AND parameters.max_length = @maxLength";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@storePrecedureName", SqlDbType.NVarChar, 128, storePrecedureName);
				cmd.AddParameter("@parameterName", SqlDbType.NVarChar, 128, parameterName);
				cmd.AddParameter("@type", SqlDbType.NVarChar, 128, type);
				cmd.AddParameter("@maxLength", SqlDbType.SmallInt, maxLength);

				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture) > 0;
			}
		}
		#endregion
	}
}
