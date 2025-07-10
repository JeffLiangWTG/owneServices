#if DEBUG

using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Data;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Shared
{
	public static class DBTransformationTestHelper
	{
		public static bool ConstraintExists(DbConnection connection, string tableName, string constraintName)
		{
			return connection.Exists(@"
				FROM
					sys.objects
				WHERE 1=1
					AND type in ('C', 'D', 'F', 'PK', 'UQ')
					AND parent_object_id = OBJECT_ID(@tableName)
					AND name = @constraintName",
				cmd =>
				{
					cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
					cmd.AddParameter("@constraintName", SqlDbType.NVarChar, 128, constraintName);
				});
		}

		public static void DropDefaultConstraintFor(string tableName, string columnName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;

			var defaultName = connection.ExecuteScalar<string>(@"
				SELECT
					name = ISNULL(MIN(name), N'')
				FROM
					sys.default_constraints
				WHERE 1=1
					AND parent_object_id = OBJECT_ID(@tableName, N'U')
					AND COL_NAME(parent_object_id, parent_column_id) = @columnName",
				cmd =>
				{
					cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
					cmd.AddParameter("@columnName", SqlDbType.NVarChar, 128, columnName);
				});

			if (!string.IsNullOrEmpty(defaultName))
			{
				connection.ExecuteNonQuery(Invariant($"ALTER TABLE {tableName.QuoteName()} DROP CONSTRAINT {defaultName.QuoteName()}"));
			}
		}

		public static void DropConstraintIfExists(string tableName, string constraintName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;

			if (ConstraintExists(connection, tableName, constraintName))
			{
				connection.ExecuteNonQuery(Invariant($"ALTER TABLE {tableName.QuoteName()} DROP CONSTRAINT {constraintName.QuoteName()}"));
			}
		}

		public static void ReplaceColumnWithCharOne(string tableName, string columnName, bool defaultValueIsYes = true, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			string sql = string.Format(CultureInfo.InvariantCulture,
@"ALTER TABLE {0}
DROP COLUMN {1}

ALTER TABLE {0}
ADD {1} CHAR(1) NOT NULL DEFAULT('{2}')", tableName, columnName, defaultValueIsYes ? "Y" : "N");
			connection.ExecuteNonQuery(sql);
		}

		public static void DropIndexIfExists(string tableName, string indexName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			if (DbObjectCreator.IndexExists(connection, tableName, indexName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, "DROP INDEX [{0}] ON [{1}]", indexName, tableName);
				using (var cmd = connection.Command(sqlText))
				{
					cmd.ExecuteNonQuery();
				}
			}
			else // in case index is not on a DB table, but on index view
			{
				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
IF exists (SELECT NULL FROM sys.indexes WHERE name = '{0}')
	DROP INDEX {0} ON {1};", indexName, tableName));
			}
		}

		public static void DropColumnIfExists(string tableName, string columnName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			if (DbObjectCreator.ColumnExists(connection, tableName, columnName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, "ALTER TABLE [{0}] DROP COLUMN [{1}]", tableName, columnName);
				using (var cmd = connection.Command(sqlText))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static void DropTableIfExists(string tableName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			if (DbObjectCreator.TableExists(connection, tableName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, "DROP TABLE [{0}]", tableName);

				using (var cmd = connection.Command(sqlText))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static void DropViewIfExists(string viewName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			if (DbObjectCreator.ViewExists(connection, viewName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, "DROP VIEW [{0}]", viewName);

				using (var cmd = connection.Command(sqlText))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static void DropFunctionIfExists(string functionName, DbConnection connection = null)
		{
			connection = connection ?? Db.Connection;
			if (DbObjectCreator.ObjectExists(connection, functionName))
			{
				var sql = "drop function " + functionName;
				connection.ExecuteNonQuery(sql);
			}
		}
	}
}

#endif
