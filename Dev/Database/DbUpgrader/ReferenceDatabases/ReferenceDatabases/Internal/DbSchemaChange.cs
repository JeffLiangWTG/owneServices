using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	static class DbSchemaChange
	{
		public static string GetAddColumnIfNotExistsScript(string tableName, string columnName, string columnDeclaration)
		{
			string script = string.Format(@"
				IF NOT EXISTS(
					SELECT * FROM sys.tables tab 
					INNER JOIN sys.columns col ON tab.object_id = col.object_id
					WHERE tab.name = '{0}' AND col.name = '{1}'
				)
				BEGIN
					ALTER TABLE {0} ADD {1} {2}
				END",
				tableName, columnName, columnDeclaration);

			return script;
		}

		public static string GetAddCheckConstranintIfNotExistsScript(string tableName, string constraintName, string constraintDefination)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF NOT EXISTS(
					SELECT NULL FROM sys.tables tab 
					INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
					WHERE tab.name = '{0}' AND ckc.name = '{1}'
				)
				BEGIN
					ALTER TABLE {0} ADD CONSTRAINT {1} CHECK ({2})
				END",
				tableName, constraintName, constraintDefination);

			return script;
		}

		public static string GetAddPrimaryKeyIfNotExistsScript(string tableName, string primaryKeyName, string primaryKeyField)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF NOT EXISTS(
					SELECT NULL FROM information_schema.table_constraints con WHERE con.constraint_Type = 'PRIMARY KEY'
					AND con.table_name = '{0}'
				)
				BEGIN
					ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2});
				END",
				tableName, primaryKeyName, primaryKeyField);

			return script;
		}

		public static string GetAddForeignKeyIfNotExistsScript(string tableName, string foreignKeyName, string foreignKeyField, string foreignKeyReferences)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF NOT EXISTS(
					SELECT NULL FROM sys.tables tab 
					INNER JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
					WHERE tab.name = '{0}' AND fk.name = '{1}'
				)
				BEGIN
					ALTER TABLE {0} WITH CHECK ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}
				END",
				tableName, foreignKeyName, foreignKeyField, foreignKeyReferences);

			return script;
		}

		public static string GetAddDefaultConstranintIfNotExistsScript(string tableName, string constraintName, string constraintDefination, string columnname)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF NOT EXISTS(
					SELECT NULL FROM sys.tables tab 
					INNER JOIN sys.default_constraints ckc ON tab.object_id = ckc.parent_object_id
					WHERE tab.name = '{0}' AND ckc.name = '{1}'
				)
				BEGIN
					ALTER TABLE {0} ADD CONSTRAINT {1} DEFAULT ({2}) FOR {3}
				END",
				tableName, constraintName, constraintDefination, columnname);

			return script;
		}

		public static string GetCreateIndexIfNotExistsScript(string tableName, string indexName, string createScript)
		{
			string script = string.Format(@"
				IF NOT EXISTS(SELECT null FROM sys.tables tab 
								INNER JOIN sys.indexes ind ON tab.object_id = ind.object_id
								WHERE tab.name = '{0}' AND ind.name = '{1}')
				BEGIN
					{2}
				END",
				tableName, indexName, createScript);

			return script;
		}

		public static string GetDropIndexIfExistsScript(string tableName, string indexName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(SELECT null FROM sys.tables tab 
								INNER JOIN sys.indexes ind ON tab.object_id = ind.object_id
								WHERE tab.name = '{0}' AND ind.name = '{1}')
				BEGIN
					DROP INDEX {0}.{1}
				END",
				tableName, indexName);

			return script;
		}

		public static string GetDropTriggerIfExistsScript(string tableName, string triggerName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(SELECT null FROM sys.tables tab 
								INNER JOIN sys.triggers tg ON tab.object_id = tg.parent_id
								WHERE tab.name = '{0}' AND tg.name = '{1}')
				BEGIN
					 DROP TRIGGER {1}
				END",
				tableName, triggerName);

			return script;
		}

		public static string GetDropStoredProcedureIfExistsScript(string storeProcedureName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				if (OBJECT_ID('{0}', 'P') is NOT NULL)
				begin
					DROP PROCEDURE {0}
				end",
				storeProcedureName);

			return script;
		}

		public static string GetCreateTableIfNotExistsScript(string tableName, string createScript)
		{
			string script = string.Format(@"
				IF NOT EXISTS(SELECT null FROM sys.tables tab WHERE tab.name = '{0}')
				BEGIN
					{1}
				END",
				tableName, createScript);

			return script;
		}

		public static void DropAllTables(DbConnection conn)
		{
			var runner = new BatchRunner();

			string dropFkScript = @"
				SELECT
					'ALTER TABLE ' + tab.name + ' DROP CONSTRAINT ' + fk.name
				FROM
					sys.foreign_keys fk
					INNER JOIN sys.tables tab
						ON tab.object_id = fk.parent_object_id
				WHERE
					tab.is_ms_shipped = 0";

			runner.RunCommandsGeneratedByQuery(conn, dropFkScript);

			string dropTableScript = "SELECT 'DROP TABLE ' + tab.name FROM sys.tables tab WHERE tab.is_ms_shipped = 0";

			runner.RunCommandsGeneratedByQuery(conn, dropTableScript);
		}

		public static void CopyAllTablesAndData(DbConnection conn, string sourceDb)
		{
			string sqlText = string.Format(@"
				DECLARE @TableName sysname;
				DECLARE @Cmd nvarchar(max);

				DECLARE TableCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
					SELECT name
					FROM [{0}].sys.tables tab WITH (READPAST, READCOMMITTEDLOCK)
					WHERE tab.is_ms_shipped = 0;

				OPEN TableCursor;
				FETCH NEXT FROM TableCursor INTO @TableName;
				WHILE (@@FETCH_STATUS = 0)
				BEGIN
					SET @Cmd = 'SELECT * INTO [' + @TableName + '] FROM [{0}]..[' + @TableName + ']';
					EXEC (@Cmd);

					FETCH NEXT FROM TableCursor INTO @TableName;
				END
				CLOSE TableCursor;
				DEALLOCATE TableCursor;",
				sourceDb);

			conn.ExecuteNonQuery(sqlText);
		}

		public static void EnsureCorrectColumnCollation(DbConnection conn)
		{
			const string sqlTextRaw = @"
				DECLARE @SqlText nvarchar(max);

				SELECT @SqlText =
					ISNULL(@SqlText, '')
					+ 'ALTER TABLE [' + sch.name + '].[' + tab.name + ']' 
					+ ' ALTER COLUMN [' + col.name + '] ' + typ.name
					+ ' (' + IIF(col.max_length < 0, 'max', convert(varchar(4), col.max_length)) + ')'
					+ ' COLLATE {0} ' + IIF(col.is_nullable = 0, 'NOT', '') + ' NULL;'
				FROM
					sys.tables tab
					INNER JOIN sys.columns col ON col.object_id = tab.object_id
					INNER JOIN sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN sys.types typ ON typ.user_type_id = col.user_type_id
				WHERE
					tab.is_ms_shipped = 0
					AND col.collation_name is not null
					AND col.collation_name <> '{0}'
					AND typ.name in ('char', 'varchar');

				IF (@SqlText is not null) EXEC (@SqlText);";

			conn.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, sqlTextRaw, Db.DatabaseCollation));
		}

		public static void CopyDatabaseExtendedProperties(DbConnection conn, string sourceDb)
		{
			string sqlText = string.Format(@"
				DECLARE @PtyName sysname;
				DECLARE @PtyValue sql_variant;
				DECLARE @OldPty sysname;
				DECLARE @Cmd nvarchar(max);

				DECLARE PtyCursor CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
					SELECT srcPty.name, srcPty.value, oldPty.name
					FROM [{0}].sys.extended_properties srcPty WITH (READPAST, READCOMMITTEDLOCK)
					LEFT JOIN sys.extended_properties oldPty
						ON oldPty.name = srcPty.name
						AND oldPty.class = srcPty.class
					WHERE srcPty.class = 0;

				OPEN PtyCursor;
				FETCH NEXT FROM PtyCursor INTO @PtyName, @PtyValue, @OldPty;
				WHILE (@@FETCH_STATUS = 0)
				BEGIN
					IF (@OldPty is null)
						EXEC sys.sp_addextendedproperty @name = @PtyName, @value = @PtyValue;
					ELSE
						EXEC sys.sp_updateextendedproperty @name = @PtyName, @value = @PtyValue;

					FETCH NEXT FROM PtyCursor INTO @PtyName, @PtyValue, @OldPty;
				END
				CLOSE PtyCursor;
				DEALLOCATE PtyCursor;",
				sourceDb);

			conn.ExecuteNonQuery(sqlText);
		}

		public static string GetDropConstraintIfExistsScript(string constraintType, string tableName, string constraintName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(
					SELECT NULL FROM sys.tables tab 
					INNER JOIN sys.{0} ckc ON tab.object_id = ckc.parent_object_id
					WHERE tab.name = '{1}' AND ckc.name = '{2}'
				)
				BEGIN
					ALTER TABLE {1} DROP CONSTRAINT {2};
				END;",
				constraintType, tableName, constraintName);
			return script;
		}

		public static string GetDropColumnIfExistsScript(string tableName, string columnName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(
					SELECT * FROM sys.tables tab 
					INNER JOIN sys.columns col ON tab.object_id = col.object_id
					WHERE tab.name = '{0}' AND col.name = '{1}'
				)
				BEGIN
					ALTER TABLE {0} DROP COLUMN {1};
				END;",
				tableName, columnName);
			return script;
		}

		public static string GetDropConstraintIfExistsFromColumnNameScript(string tableName, string columnName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @dfname NVARCHAR(MAX),
        @dropsql NVARCHAR(MAX)
SELECT @dfname=name FROM sys.objects WHERE object_id = (SELECT sys.columns.default_object_id FROM sys.objects INNER JOIN sys.columns ON objects.object_id = sys.columns.object_id 
    WHERE upper(sys.columns.name) = '{1}' AND upper(sys.objects.name) = '{0}')
    IF LEN(@dfname)>0
    SET @dropsql='ALTER TABLE {0} DROP CONSTRAINT '+ CAST(@dfname AS NVARCHAR(50))
    IF LEN(@dfname)>0
    EXEC sp_executesql @dropsql", tableName, columnName);
			return script;
		}

		public static string GetRenameColumnIfExistsScript(string tableName, string columnOldName, string columnNewName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
				IF EXISTS(
					SELECT * FROM sys.tables tab 
					INNER JOIN sys.columns col ON tab.object_id = col.object_id
					WHERE tab.name = '{0}' AND col.name = '{1}'
				)
				BEGIN
					EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'
				END;", tableName, columnOldName, columnNewName);
			return script;
		}

		public static string GetDropForeignKeyFromIndexNameScript(DbConnection conn, string indexName)
		{
			var script = string.Format(CultureInfo.InvariantCulture, @"
			SELECT 
				OBJECT_NAME(ForeignKeyColumn.parent_object_id) as TableName,
				ForeignKey.name as ForeignKeyName
				FROM sys.foreign_keys ForeignKey 
				INNER JOIN sys.foreign_key_columns ForeignKeyColumn ON ForeignKeyColumn.referenced_object_id  = ForeignKey.referenced_object_id and ForeignKeyColumn.parent_object_id  = ForeignKey.parent_object_id AND ForeignKey.object_id = ForeignKeyColumn.constraint_object_id
				INNER JOIN sys.index_columns IndexColumn ON ForeignKeyColumn.referenced_object_id = IndexColumn.object_id and ForeignKeyColumn.referenced_column_id = IndexColumn.column_id
				INNER JOIN sys.indexes Indexes ON Indexes.object_id = IndexColumn.object_id AND Indexes.index_id = IndexColumn.index_id AND Indexes.name = '{0}'", indexName);

			var builder = new StringBuilder();
			using (var reader = conn.Command(script).ExecuteReader())
			{
				while (reader.Read())
				{
					var tableName = reader["TableName"].ToString();
					var foreignKeyName = reader["ForeignKeyName"].ToString();
					builder.AppendLine(GetDropConstraintIfExistsScript("foreign_keys", tableName, foreignKeyName));
				}
			}
			return builder.Length == 0 ? string.Empty : builder.ToString();
		}
	}

	#region Script Interfaces

	interface ITableScript
	{
		string TableName { get; }
		string CreateTableScript { get; }
		IReadOnlyList<IndexScript> CreateIndexScripts { get; }
	}

	interface IPopulateData
	{
		string TableName { get; }
		string CsvFileName { get; }
		string ColumnSqlList { get; }
		IReadOnlyList<SqlDbType> ColumnTypes { get; }
	}

	public class IndexScript
	{
		public string IndexName { get; set; }
		public string CreateIndexScript { get; set; }
	}

	#endregion
}
