using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Shared
{
	public class DbColumnDependencyRemover
	{
		public DbColumnDependencyRemover(string tableName, string columnName)
			: this(Db.SqlDbOwnerSchema, tableName, columnName)
		{
		}

		public DbColumnDependencyRemover(string tableSchema, string tableName, string columnName)
		{
			this.tableSchema = Argument.NotNullOrEmpty(tableSchema, nameof(tableSchema));
			this.tableName = Argument.NotNullOrEmpty(tableName, nameof(tableName));
			this.columnName = Argument.NotNullOrEmpty(columnName, nameof(columnName), " - Are you sure you didn't mean to call a static method?");
		}

		readonly string tableSchema;
		readonly string tableName;
		readonly string columnName;

		string FullTableName => "[" + tableSchema + "].[" + tableName + "]";

		/// <summary>
		/// Delete column dependent objects.
		/// Uses the database of the passed in connection.
		/// </summary>
		public void DropRelateObjects(DbConnection connection)
		{
			DropRelatedConstraints(connection);
			DropRelatedIndexes(connection);
			DropRelatedUserStatistics(connection);
			DropSchemaBoundReferencingObjects(connection, tableSchema, tableName, columnName);
			DropDependentTriggers(connection);
			DropComputedColumns(connection);
		}

		public void DropRelateObjectsBeforeRenamingColumn(DbConnection connection, string newColumnName)
		{
			DisableTableChangeTracking(connection, tableSchema, tableName);
			DropCheckConstraints(connection);
			DropFilteredIndexes(connection);
			DropFilteredStatistics(connection);
			DropDependentTriggers(connection);
			DropSchemaBoundReferencingObjects(connection, tableSchema, tableName, columnName);
			DropComputedColumns(connection);
			RenameIndexes(connection, newColumnName);
		}

		void DropFilteredIndexes(DbConnection connection)
		{
			var sql = String.Format(@"
DECLARE
	@new_line nchar(2) = CHAR(13) + CHAR(10),
	@stmts    nvarchar(max)

SELECT
	@stmts = ISNULL(@stmts + @new_line, '')
		+ CONCAT('DROP INDEX [', name, '] ON [{0}].[{1}];')
FROM
	sys.indexes
WHERE 1=1
	AND object_id = OBJECT_ID(N'[{0}].[{1}]', N'U')
	AND has_filter = 1
	AND filter_definition LIKE N'%[[]{2}]%'

if (@stmts is NOT NULL)
begin
	--select @stmts
	EXEC (@stmts);
end
"
				, tableSchema
				, tableName
				, columnName
				);

			connection.ExecuteNonQuery(sql);
		}

		void DropFilteredStatistics(DbConnection connection)
		{
			var sql = string.Format(@"
DECLARE
	@new_line nchar(2) = CHAR(13) + CHAR(10),
	@stmts    nvarchar(max)

SELECT
	@stmts = ISNULL(@stmts + @new_line, '')
		+ CONCAT('DROP STATISTICS [{0}].[{1}].[', name, '];')
FROM
	sys.stats
WHERE 1=1
	AND object_id = OBJECT_ID(N'[{0}].[{1}]', N'U')
	AND has_filter = 1
	AND filter_definition LIKE N'%[[]{2}]%'

if (@stmts is NOT NULL)
begin
	--select @stmts
	EXEC (@stmts);
end
"
				, tableSchema
				, tableName
				, columnName
				);

			connection.ExecuteNonQuery(sql);
		}

		void RenameIndexes(DbConnection connection, string newColumnName)
		{
			var sql = string.Format(@"
DECLARE
	@new_line nchar(2) = CHAR(13) + CHAR(10),
	@stmts    nvarchar(max);

SELECT
	@stmts = ISNULL(@stmts + @new_line, '')
		+ CONCAT('EXEC sys.sp_rename N''[{0}].[{1}].[', name, ']'', N''', LEFT(REPLACE(name + N'_', N'_{2}_', N'_{3}_'), LEN(REPLACE(name + N'_', N'_{2}_', N'_{3}_')) - 1), ''', N''INDEX'';')
FROM
	sys.indexes
WHERE 1=1
	AND object_id = OBJECT_ID(N'[{0}].[{1}]', N'U')
	AND CHARINDEX(N'_{2}_', name + N'_') > 0

if (@stmts is NOT NULL)
begin
	--select @stmts
	EXEC (@stmts);
end
"
				, tableSchema   // 0
				, tableName     // 1
				, columnName    // 2
				, newColumnName // 3
				);

			connection.ExecuteNonQuery(sql);
		}

		void DropComputedColumns(DbConnection connection)
		{
			if (string.IsNullOrWhiteSpace(columnName))
			{
				return;
			}

			var sql = @"
SELECT
	referencing.name
FROM
	sys.sql_expression_dependencies AS d
	JOIN sys.columns                AS referencing ON referencing.object_id = d.referencing_id AND referencing.column_id = d.referencing_minor_id
	JOIN sys.columns                AS referenced  ON referenced.object_id = d.referenced_id AND referenced.column_id = d.referenced_minor_id
WHERE 1=1
	AND referenced.object_id = OBJECT_ID(@FullTableName, 'U')
	AND referenced.name = @ColumnName
	AND d.referencing_id = d.referenced_id

";

			var computedColumns = new List<string>();
			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@FullTableName", SqlDbType.NVarChar, 256, FullTableName);
				cmd.AddParameter("@ColumnName", SqlDbType.NVarChar, 128, columnName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						computedColumns.Add((string)reader["name"]);
					}
				}
			}

			foreach (var column in computedColumns)
			{
				new DbColumnDependencyRemover(tableSchema, tableName, column).DropRelateObjects(connection);
				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [{0}].[{1}] DROP COLUMN [{2}]", tableSchema, tableName, column));
			}
		}

		#region Column Level Constraints and Indexes (to be dropped before a column is dropped/altered)

		void DropRelatedConstraints(DbConnection connection)
		{
			DropCheckDefaultAndForeignKeyConstraints(connection);
			DropChildFkConstraints(connection);
			DisableTableChangeTracking(connection, tableSchema, tableName);
			DropPkAndUniqueConstraints(connection);
		}

		void DropCheckConstraints(DbConnection connection)
		{
			var sql = String.Format(CultureInfo.InvariantCulture, @"-- CHECK
SELECT
	constraint_name = chk.name
FROM
	[{0}].sys.schemas                          AS sch
	JOIN [{0}].sys.tables                      AS tab ON tab.schema_id = sch.schema_id
	JOIN [{0}].sys.columns                     AS col ON col.object_id = tab.object_id
	JOIN [{0}].sys.sql_expression_dependencies AS dep ON dep.referenced_id = tab.object_id AND dep.referenced_minor_id = col.column_id
	JOIN [{0}].sys.check_constraints           AS chk ON chk.object_id = dep.referencing_id
WHERE
	sch.name = @tableSchema
	AND tab.name = @tableName
	AND col.name = @columnName
"
				, connection.CurrentDatabase);

			var cmdParams = new[]
			{
				("@columnName", SqlDbType.VarChar, 128, columnName),
				("@tableName", SqlDbType.VarChar, 128, tableName),
				("@tableSchema", SqlDbType.VarChar, 128, (object)tableSchema),
			};

			var checkConstraintsToDrop = DataUtils.GetDataTableFromQuery(connection, sql, cmdParams);
			foreach (DataRow row in checkConstraintsToDrop.Rows)
			{
				var constraintName = (string)row["constraint_name"];
				var sqlText = $"ALTER TABLE [{connection.CurrentDatabase}].[{tableSchema}].[{tableName}] DROP CONSTRAINT [{constraintName}];";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		/// <summary>
		/// 'ALTER TABLE [' + tab.name + '] DROP CONSTRAINT [' + constobj.name + ']'
		/// UNION ALL used for performance reasons (names are unique anyway)
		/// </summary>
		void DropCheckDefaultAndForeignKeyConstraints(DbConnection connection)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				-- CHECK
				SELECT
					constobj.name ConstName
				FROM
					[{0}].sys.check_constraints constobj
					INNER JOIN [{0}].sys.sql_expression_dependencies dep ON dep.referencing_id = constobj.object_id
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = dep.referenced_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.columns col ON col.object_id = tab.object_id AND col.column_id = dep.referenced_minor_id
				WHERE
					sch.name = @tableSchema
					AND tab.name = @tableName
					AND col.name = @columnName
				UNION ALL
				-- DEFAULT
				SELECT
					constobj.name ConstName
				FROM
					[{0}].sys.columns col
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = col.object_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.default_constraints constobj
						ON constobj.parent_object_id = tab.object_id AND constobj.parent_column_id = col.column_id
				WHERE
					sch.name = @tableSchema
					AND tab.name = @tableName
					AND col.name = @columnName
				UNION ALL
				-- FOREIGN KEY
				SELECT
					constobj.name ConstName
				FROM
					[{0}].sys.columns col
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = col.object_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.foreign_key_columns constcol
						ON constcol.parent_object_id = tab.object_id AND constcol.parent_column_id = col.column_id
					INNER JOIN [{0}].sys.foreign_keys constobj
						ON constobj.object_id = constcol.constraint_object_id
				WHERE
					sch.name = @tableSchema
					AND tab.name = @tableName
					AND col.name = @columnName", connection.CurrentDatabase);
			var cmdParams = new[]
			{
				("@columnName", SqlDbType.VarChar, 128, columnName),
				("@tableName", SqlDbType.VarChar, 128, tableName),
				("@tableSchema", SqlDbType.VarChar, 128, (object)tableSchema),
			};
			var checkConstraintsToDrop = DataUtils.GetDataTableFromQuery(connection, sqlText, cmdParams);

			foreach (DataRow row in checkConstraintsToDrop.Rows)
			{
				var constraintName = row["ConstName"].ToString();
				sqlText = $"ALTER TABLE [{connection.CurrentDatabase}].[{tableSchema}].[{tableName}] DROP CONSTRAINT [{constraintName}]";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		/// <summary>
		/// 'ALTER TABLE [' + childTab.name + '] DROP CONSTRAINT [' + constobj.name + ']'
		/// </summary>
		void DropChildFkConstraints(DbConnection connection)
		{
			const string selectChildFkConstraintsToDrop = @"
				SELECT
					constobj.name FkName,
					childSch.name FkTabSchema,
					childTab.name FkTabName
				FROM
					[{0}].sys.columns col
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = col.object_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.foreign_key_columns constcol
						ON constcol.referenced_object_id = tab.object_id AND constcol.referenced_column_id = col.column_id
					INNER JOIN [{0}].sys.foreign_keys constobj
						ON constobj.object_id = constcol.constraint_object_id
					INNER JOIN [{0}].sys.tables childTab ON childTab.object_id = constobj.parent_object_id
					INNER JOIN [{0}].sys.schemas childSch ON childSch.schema_id = childTab.schema_id
				WHERE
					sch.name = @tableSchema
					AND tab.name = @tableName
					AND col.name = @columnName";

			var sqlText = string.Format(CultureInfo.InvariantCulture, selectChildFkConstraintsToDrop, connection.CurrentDatabase);

			var cmdParams = new[]
			{
				("@columnName", SqlDbType.NVarChar, 128, columnName),
				("@tableName", SqlDbType.VarChar, 128, (object)tableName),
				("@tableSchema", SqlDbType.VarChar, 128, tableSchema)
			};

			var referencingFksToDrop = DataUtils.GetDataTableFromQuery(connection, sqlText, cmdParams);

			foreach (DataRow row in referencingFksToDrop.Rows)
			{
				sqlText = $"ALTER TABLE [{connection.CurrentDatabase}].[{row["FkTabSchema"].ToString()}].[{row["FkTabName"].ToString()}] DROP CONSTRAINT [{row["FkName"].ToString()}]";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		/// <summary>
		/// 'ALTER TABLE [db]..[table] DISABLE CHANGE_TRACKING
		/// </summary>
		public static void DisableTableChangeTracking(DbConnection connection, string tableSchema, string tableName)
		{
			var disableChangeTrackingSql = string.Format(CultureInfo.InvariantCulture, @"
				IF exists(
					SELECT null FROM [{0}].sys.tables tab
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.change_tracking_tables ctt ON ctt.object_id = tab.object_id
					WHERE sch.name = @tableSchema AND tab.name = @tableName)
				BEGIN
					ALTER TABLE [{0}].[{1}].[{2}] DISABLE CHANGE_TRACKING;
				END", connection.CurrentDatabase, tableSchema, tableName);

			using (var command = connection.Command(disableChangeTrackingSql))
			{
				command.AddParameter("@tableName", SqlDbType.VarChar, 128, tableName);
				command.AddParameter("@tableSchema", SqlDbType.VarChar, 128, tableSchema);
				command.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// 'ALTER TABLE [' + tab.name + '] DROP CONSTRAINT [' + constobj.name + ']'
		/// </summary>
		void DropPkAndUniqueConstraints(DbConnection connection)
		{
			DropRelatedSpatialIndex(connection);

			const string selectKeyConstraintsToDrop = @"
				SELECT
					constobj.name ConstName
				FROM
					[{0}].sys.columns col
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = col.object_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.key_constraints constobj ON constobj.parent_object_id = tab.object_id
					INNER JOIN [{0}].sys.index_columns indkey
						ON indkey.object_id = tab.object_id AND indkey.column_id = col.column_id AND indkey.index_id = constobj.unique_index_id
				WHERE
					sch.name = @tableSchema
					AND tab.name = @tableName
					AND col.name = @columnName";

			var sqlText = string.Format(CultureInfo.InvariantCulture, selectKeyConstraintsToDrop, connection.CurrentDatabase);

			var cmdParams = new[]
			{
				("@columnName", SqlDbType.VarChar, 128, columnName),
				("@tableName", SqlDbType.VarChar, 128, tableName),
				("@tableSchema", SqlDbType.VarChar, 128, (object)tableSchema)
			};
			var checkConstraintsToDrop = DataUtils.GetDataTableFromQuery(connection, sqlText, cmdParams);

			foreach (DataRow row in checkConstraintsToDrop.Rows)
			{
				sqlText = $"ALTER TABLE [{connection.CurrentDatabase}].[{tableSchema}].[{tableName}] DROP CONSTRAINT [{row["ConstName"].ToString()}]";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void DropRelatedSpatialIndex(DbConnection connection)
		{
			const string selectSpatialIndexesToDrop = @"
				SELECT
					ind.name SpatialIndexName
				FROM
					[{0}].sys.columns col
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = col.object_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.key_constraints constobj ON constobj.parent_object_id = tab.object_id
					INNER JOIN [{0}].sys.index_columns indkey
						ON indkey.object_id = tab.object_id AND indkey.column_id = col.column_id AND indkey.index_id = constobj.unique_index_id
					INNER JOIN [{0}].sys.spatial_indexes ind ON ind.object_id = indkey.object_id
				WHERE
					sch.name = '{1}'
					AND tab.name = '{2}'
					AND col.name = '{3}'";

			var sqlText = String.Format(CultureInfo.InvariantCulture, selectSpatialIndexesToDrop, connection.CurrentDatabase, tableSchema, tableName, columnName);
			var checkSpatialIndexesToDrop = DataUtils.GetDataTableFromQuery(connection, sqlText);

			foreach (DataRow row in checkSpatialIndexesToDrop.Rows)
			{
				var indexName = row["SpatialIndexName"].ToString();
				sqlText = string.Format(CultureInfo.InvariantCulture, "DROP INDEX [{0}] ON [{1}].[{2}]", indexName, tableSchema, tableName);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		public void DropRelatedIndexes(DbConnection connection)
		{
			var indexesToDrop = GetColumnIndexes(connection);

			foreach (DataRow row in indexesToDrop.Rows)
			{
				var indexName = (string)row["IndName"];
				DropRelatedIndexForeignKeys(connection, indexName);

				connection.ExecuteNonQuery($"DROP INDEX {indexName.QuoteName()} ON {tableSchema.QuoteName()}.{tableName.QuoteName()}");
			}
		}

		void DropRelatedIndexForeignKeys(DbConnection connection, string indexName)
		{
			var foreignKeysToDrop = new List<string>();
			connection.ExecuteReader(@"-- DropRelatedIndexForeignKeys
SELECT
	stmt_drop = CONCAT(N''
		, N'ALTER TABLE '
		, QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)), N'.', QUOTENAME(OBJECT_NAME(fk.parent_object_id))
		, N' DROP CONSTRAINT ', QUOTENAME(fk.name)
		, N';'
		)
FROM
	sys.indexes           AS ind
	JOIN sys.foreign_keys AS fk  ON fk.referenced_object_id = ind.object_id AND fk.key_index_id = ind.index_id
WHERE 1=1
	AND ind.object_id = OBJECT_ID(@fullTableName, N'U')
	AND ind.name = @indexName
"
				, cmd =>
				{
					cmd.AddParameter("@fullTableName", SqlDbType.NVarChar, 4000, $"{tableSchema}.{tableName}");
					cmd.AddParameter("@indexName", SqlDbType.NVarChar, 128, indexName);
				}
				, record =>
				{
					foreignKeysToDrop.Add((string)record["stmt_drop"]);
				});

			foreach (var fk_to_drop in foreignKeysToDrop)
			{
				connection.ExecuteNonQuery(fk_to_drop);
			}
		}

		void DropRelatedUserStatistics(DbConnection connection)
		{
			var statsToDrop = GetColumnUserStatistics(connection);

			foreach (DataRow row in statsToDrop.Rows)
			{
				var sqlText = $"DROP STATISTICS [{tableSchema}].[{tableName}].[{row["StatName"].ToString()}]";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		DataTable GetColumnIndexes(DbConnection connection)
		{
			const string selectIndexesToDrop = @"
				SELECT
					ind.name IndName,
					ind.index_id
				FROM
					[{0}].sys.columns col
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = col.object_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.index_columns indkey ON indkey.object_id = col.object_id AND indkey.column_id = col.column_id
					INNER JOIN [{0}].sys.indexes ind ON ind.object_id = col.object_id AND ind.index_id = indkey.index_id
				WHERE
					sch.name = @tableSchema
					AND tab.name = @tableName
					AND col.name = @columnName
					AND ind.type not in (0 {1})
					AND ind.is_primary_key = 0
					AND ind.is_unique_constraint = 0
				UNION
				SELECT
					ind.name IndName,
					ind.index_id
				FROM
					[{0}].sys.columns col
					INNER JOIN [{0}].sys.tables tab ON tab.object_id = col.object_id
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.sql_expression_dependencies indexpdep
						ON indexpdep.referencing_class = 7
						AND indexpdep.referencing_id = col.object_id
						AND indexpdep.referenced_id = col.object_id
						AND indexpdep.referenced_minor_id = col.column_id
					INNER JOIN [{0}].sys.indexes ind ON ind.object_id = indexpdep.referencing_id AND ind.index_id = indexpdep.referencing_minor_id
				WHERE
					sch.name = @tableSchema
					AND tab.name = @tableName
					AND col.name = @columnName
					AND ind.type != 0
					AND ind.is_primary_key = 0
					AND ind.is_unique_constraint = 0
				-- Drop dependent index first (e.g.: dependent XML indexes)
				ORDER BY
					ind.index_id DESC;";

			var sqlText = string.Format(
				CultureInfo.InvariantCulture,
				selectIndexesToDrop,
				connection.CurrentDatabase,
				",5"
			);
			var cmdParams = new[]
			{
				("@columnName", SqlDbType.VarChar, 128, columnName),
				("@tableName", SqlDbType.VarChar, 128, tableName),
				("@tableSchema", SqlDbType.VarChar, 128, (object)tableSchema)
			};

			var result = DataUtils.GetDataTableFromQuery(connection, sqlText, cmdParams);
			return result;
		}

		DataTable GetColumnUserStatistics(DbConnection connection)
		{
			const string selectUserStatsToDrop = @"
				SELECT
					distinct st.name StatName
				FROM
					[{0}].sys.objects tab
					INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
					INNER JOIN [{0}].sys.columns col ON col.object_id = tab.object_id
					INNER JOIN [{0}].sys.stats st ON st.object_id = tab.object_id
					INNER JOIN [{0}].sys.stats_columns stcol ON stcol.object_id = col.object_id AND stcol.column_id = col.column_id
				WHERE
					sch.name = @tableSchema
					AND tab.name = @tableName
					AND col.name = @columnName
					AND st.user_created = 1";

			var sqlText = string.Format(CultureInfo.InvariantCulture, selectUserStatsToDrop, connection.CurrentDatabase);
			var cmdParams = new[]
			{
				("@columnName", SqlDbType.VarChar, 128, columnName),
				("@tableName", SqlDbType.VarChar, 128, tableName),
				("@tableSchema", SqlDbType.VarChar, 128, (object)tableSchema)
			};
			var result = DataUtils.GetDataTableFromQuery(connection, sqlText, cmdParams);
			return result;
		}

		public static void DropSchemaBoundReferencingObjects(DbConnection connection, string tableSchema, string tableName, string columnName = null)
		{
			var sql = UpgraderUtils.GetSchemaBoundObjectsToDropSql(tableSchema, tableName, columnName);
			new BatchRunner().RunCommandsGeneratedByQuery(connection, sql);
		}

		/// <summary>
		/// Triggers are not schema-bound objects but can cause upgrade problems if not removed.
		/// </summary>
		void DropDependentTriggers(DbConnection connection)
		{
			const string sqlTextRaw = @"
				SELECT depObj.name
				FROM [{0}].sys.sql_dependencies dep
						 INNER JOIN [{0}].sys.tables tab ON tab.object_id = dep.referenced_major_id
						 INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = tab.schema_id
						 INNER JOIN [{0}].sys.columns col
									ON col.object_id = dep.referenced_major_id AND col.column_id = dep.referenced_minor_id
						 INNER JOIN [{0}].sys.objects depObj ON depObj.object_id = dep.object_id
				WHERE sch.name = @tableSchema
				  AND tab.name = @tableName
				  AND col.name = @columnName
				  AND depObj.type = 'TR'";

			var sqlText = string.Format(CultureInfo.InvariantCulture, sqlTextRaw, connection.CurrentDatabase);
			var dropCmbBuilder = new StringBuilder();
			var cmdParams = new IStructuralEquatable[]
			{
				("@columnName", SqlDbType.VarChar, 128, (object)columnName),
				("@tableName", SqlDbType.VarChar, 128, (object)tableName),
				("@tableSchema", SqlDbType.VarChar, 128, (object)tableSchema)
			};

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameters(cmdParams);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						dropCmbBuilder.AppendFormat("DROP TRIGGER [{0}].[{1}];", tableSchema, reader[0]).AppendLine();
					}
				}
			}

			if (dropCmbBuilder.Length > 0)
			{
				connection.ExecuteNonQuery(dropCmbBuilder.ToString());
			}
		}

		#endregion
	}
}
