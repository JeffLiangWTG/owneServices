using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource;
using NUnit.Framework;

namespace CargoWise.Database.ExtendedProperties.Test
{
	class ExtPropertyTest : TransactionedTestCase
	{
		public void TestDbExtendedProperty()
		{
			var mainDatabase = TestConnection.CurrentDatabase;
			var secondDatabase = mainDatabase + "_SD001";
			var property = "TestDbExtendedProperty";
			var emptyValue = " ";

			AssertEquals("Initial property value", null, ExtProperty.Database.Select(TestConnection, property));
			AssertEquals("Initial property value", null, ExtProperty.Database.Select(TestConnection, mainDatabase, property));
			AssertEquals("Initial property value", null, ExtProperty.Database.Select(TestConnection, secondDatabase, property));

			ExtProperty.Database.Update(TestConnection, property, "abc");
			AssertEquals("Property value", "abc", ExtProperty.Database.Select(TestConnection, property));
			AssertEquals("Property value", "abc", ExtProperty.Database.Select(TestConnection, mainDatabase, property));
			AssertEquals("Property value", null, ExtProperty.Database.Select(TestConnection, secondDatabase, property));

			ExtProperty.Database.Update(TestConnection, secondDatabase, property, "abc_SD");
			AssertEquals("Property value", "abc", ExtProperty.Database.Select(TestConnection, property));
			AssertEquals("Property value", "abc", ExtProperty.Database.Select(TestConnection, mainDatabase, property));
			AssertEquals("Property value", "abc_SD", ExtProperty.Database.Select(TestConnection, secondDatabase, property));

			ExtProperty.Database.Update(TestConnection, property, "xyz");
			ExtProperty.Database.Update(TestConnection, secondDatabase, property, "xyz_SD");
			AssertEquals("Property value", "xyz", ExtProperty.Database.Select(TestConnection, property));
			AssertEquals("Property value", "xyz_SD", ExtProperty.Database.Select(TestConnection, secondDatabase, property));

			ExtProperty.Database.Update(TestConnection, property, " ");
			ExtProperty.Database.Update(TestConnection, secondDatabase, property, null);
			AssertEquals("Property value", string.Empty, ExtProperty.Database.Select(TestConnection, property));
			AssertEquals("Property value", string.Empty, ExtProperty.Database.Select(TestConnection, secondDatabase, property));

			ExtProperty.Database.Delete(TestConnection, property);
			ExtProperty.Database.Delete(TestConnection, secondDatabase, property);
			AssertEquals("Property is dropped", null, ExtProperty.Database.Select(TestConnection, property));
			AssertEquals("Property is dropped", null, ExtProperty.Database.Select(TestConnection, secondDatabase, property));

			// Defaults
			ExtProperty.Database.Update(TestConnection, property, "abc");
			ExtProperty.Database.Update(TestConnection, secondDatabase, property, "abc_SD");

			AssertEquals("Default Property value", null, ExtProperty.Database.Select(TestConnection, null));
			AssertEquals("Default Property value", null, ExtProperty.Database.Select(TestConnection, emptyValue));
			AssertEquals("Default Property value", null, ExtProperty.Database.Select(TestConnection, secondDatabase, null));
			AssertEquals("Default Property value", null, ExtProperty.Database.Select(TestConnection, secondDatabase, emptyValue));

			// Exceptions
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Database.Update(TestConnection, null, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Database.Update(TestConnection, emptyValue, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Database.Update(TestConnection, secondDatabase, null, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Database.Update(TestConnection, secondDatabase, emptyValue, "xyz"));

			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Database.Delete(TestConnection, null));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Database.Delete(TestConnection, emptyValue));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Database.Delete(TestConnection, secondDatabase, null));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Database.Delete(TestConnection, secondDatabase, emptyValue));
		}

		public void TestTableExtendedProperty()
		{
			var mainDatabase = TestConnection.CurrentDatabase;
			var secondDatabase = mainDatabase + "_SD001";
			var property = "TestTableExtendedProperty";
			var schema = Db.SqlDbOwnerSchema;
			var table = "TestTable";
			var emptyValue = " ";

			AssertEquals("Initial property value", null, ExtProperty.Table.Select(TestConnection, schema, table, property));

			ExtProperty.Table.Update(TestConnection, schema, table, property, "abc");
			AssertEquals("Property value", "abc", ExtProperty.Table.Select(TestConnection, schema, table, property));

			ExtProperty.Table.Update(TestConnection, schema, table, property, "xyz");
			AssertEquals("Property value", "xyz", ExtProperty.Table.Select(TestConnection, schema, table, property));

			ExtProperty.Table.Update(TestConnection, schema, table, property, " ");
			AssertEquals("Property value", string.Empty, ExtProperty.Table.Select(TestConnection, schema, table, property));

			ExtProperty.Table.Delete(TestConnection, schema, table, property);
			AssertEquals("Is Property dropped?", null, ExtProperty.Table.Select(TestConnection, schema, table, property));

			// Defaults
			ExtProperty.Table.Update(TestConnection, schema, table, property, "abc");

			AssertEquals("Default Property value", null, ExtProperty.Table.Select(TestConnection, null, table, property));
			AssertEquals("Default Property value", null, ExtProperty.Table.Select(TestConnection, emptyValue, table, property));
			AssertEquals("Default Property value", null, ExtProperty.Table.Select(TestConnection, schema, null, property));
			AssertEquals("Default Property value", null, ExtProperty.Table.Select(TestConnection, schema, emptyValue, property));
			AssertEquals("Default Property value", null, ExtProperty.Table.Select(TestConnection, schema, table, null));
			AssertEquals("Default Property value", null, ExtProperty.Table.Select(TestConnection, schema, table, emptyValue));

			// Exceptions
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Update(TestConnection, null, table, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Update(TestConnection, emptyValue, table, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Update(TestConnection, schema, null, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Update(TestConnection, schema, emptyValue, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Update(TestConnection, schema, table, null, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Update(TestConnection, schema, table, emptyValue, "xyz"));

			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Delete(TestConnection, null, table, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Delete(TestConnection, emptyValue, table, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Delete(TestConnection, schema, null, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Table.Delete(TestConnection, schema, emptyValue, property));
		}

		public void TestColumnExtendedProperty()
		{
			var mainDatabase = TestConnection.CurrentDatabase;
			var secondDatabase = mainDatabase + "_SD001";
			var property = "TestColumnExtendedProperty";
			var schema = Db.SqlDbOwnerSchema;
			var table = "TestTable";
			var column = "TestColumn";
			var emptyValue = " ";

			AssertEquals("Initial property value", null, ExtProperty.Column.Select(TestConnection, schema, table, column, property));

			ExtProperty.Column.Update(TestConnection, schema, table, column, property, "abc");
			AssertEquals("Property value", "abc", ExtProperty.Column.Select(TestConnection, schema, table, column, property));

			ExtProperty.Column.Update(TestConnection, schema, table, column, property, "xyz");
			AssertEquals("Property value", "xyz", ExtProperty.Column.Select(TestConnection, schema, table, column, property));

			ExtProperty.Column.Update(TestConnection, schema, table, column, property, " ");
			AssertEquals("Property value", string.Empty, ExtProperty.Column.Select(TestConnection, schema, table, column, property));

			ExtProperty.Column.Delete(TestConnection, schema, table, column, property);
			AssertEquals("Is Property dropped?", null, ExtProperty.Column.Select(TestConnection, schema, table, column, property));

			// Defaults
			ExtProperty.Column.Update(TestConnection, schema, table, column, property, "abc");

			AssertEquals("Default Property value", null, ExtProperty.Column.Select(TestConnection, null, table, column, property));
			AssertEquals("Default Property value", null, ExtProperty.Column.Select(TestConnection, emptyValue, table, column, property));
			AssertEquals("Default Property value", null, ExtProperty.Column.Select(TestConnection, schema, null, column, property));
			AssertEquals("Default Property value", null, ExtProperty.Column.Select(TestConnection, schema, emptyValue, column, property));
			AssertEquals("Default Property value", null, ExtProperty.Column.Select(TestConnection, schema, table, null, property));
			AssertEquals("Default Property value", null, ExtProperty.Column.Select(TestConnection, schema, table, emptyValue, property));
			AssertEquals("Default Property value", null, ExtProperty.Column.Select(TestConnection, schema, table, column, null));
			AssertEquals("Default Property value", null, ExtProperty.Column.Select(TestConnection, schema, table, column, emptyValue));

			// Exceptions
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Update(TestConnection, null, table, column, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Update(TestConnection, emptyValue, table, column, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Update(TestConnection, schema, null, column, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Update(TestConnection, schema, emptyValue, column, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Update(TestConnection, schema, table, null, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Update(TestConnection, schema, table, emptyValue, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Update(TestConnection, schema, table, column, null, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Update(TestConnection, schema, table, column, emptyValue, "xyz"));

			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Delete(TestConnection, null, table, column, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Delete(TestConnection, emptyValue, table, column, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Delete(TestConnection, schema, null, column, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Delete(TestConnection, schema, emptyValue, column, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Delete(TestConnection, schema, table, null, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Column.Delete(TestConnection, schema, table, emptyValue, property));
		}

		public void TestIndexExtendedProperty()
		{
			var mainDatabase = TestConnection.CurrentDatabase;
			var secondDatabase = mainDatabase + "_SD001";
			var property = "TestIndexExtendedProperty";
			var schema = Db.SqlDbOwnerSchema;
			var table = "TestTable";
			var index = "TestIndex";
			var emptyValue = " ";

			AssertEquals("Initial property value", null, ExtProperty.Index.Select(TestConnection, schema, table, index, property));
			AssertEquals("Initial property value", null, ExtProperty.Index.Select(TestConnection, mainDatabase, schema, table, index, property));
			AssertEquals("Initial property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, index, property));

			ExtProperty.Index.Update(TestConnection, schema, table, index, property, "abc");
			AssertEquals("Property value", "abc", ExtProperty.Index.Select(TestConnection, schema, table, index, property));
			AssertEquals("Property value", "abc", ExtProperty.Index.Select(TestConnection, mainDatabase, schema, table, index, property));
			AssertEquals("Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, index, property));

			ExtProperty.Index.Update(TestConnection, secondDatabase, schema, table, index, property, "abc_SD");
			AssertEquals("Property value", "abc", ExtProperty.Index.Select(TestConnection, schema, table, index, property));
			AssertEquals("Property value", "abc", ExtProperty.Index.Select(TestConnection, mainDatabase, schema, table, index, property));
			AssertEquals("Property value", "abc_SD", ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, index, property));

			ExtProperty.Index.Update(TestConnection, schema, table, index, property, "xyz");
			ExtProperty.Index.Update(TestConnection, secondDatabase, schema, table, index, property, "xyz_SD");
			AssertEquals("Property value", "xyz", ExtProperty.Index.Select(TestConnection, schema, table, index, property));
			AssertEquals("Property value", "xyz_SD", ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, index, property));

			ExtProperty.Index.Update(TestConnection, schema, table, index, property, " ");
			ExtProperty.Index.Update(TestConnection, secondDatabase, schema, table, index, property, null);
			AssertEquals("Property value", string.Empty, ExtProperty.Index.Select(TestConnection, schema, table, index, property));
			AssertEquals("Property value", string.Empty, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, index, property));

			ExtProperty.Index.Delete(TestConnection, schema, table, index, property);
			ExtProperty.Index.Delete(TestConnection, secondDatabase, schema, table, index, property);
			AssertEquals("Is Property dropped?", null, ExtProperty.Index.Select(TestConnection, schema, table, index, property));
			AssertEquals("Is Property dropped?", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, index, property));

			// Defaults
			ExtProperty.Index.Update(TestConnection, schema, table, index, property, "abc");
			ExtProperty.Index.Update(TestConnection, secondDatabase, schema, table, index, property, "abc_SD");

			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, null, table, index, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, emptyValue, table, index, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, schema, null, index, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, schema, emptyValue, index, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, schema, table, null, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, schema, table, emptyValue, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, schema, table, index, null));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, schema, table, index, emptyValue));

			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, null, table, index, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, emptyValue, table, index, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, null, index, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, emptyValue, index, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, null, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, emptyValue, property));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, index, null));
			AssertEquals("Default Property value", null, ExtProperty.Index.Select(TestConnection, secondDatabase, schema, table, index, emptyValue));

			// Exceptions
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, null, table, index, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, emptyValue, table, index, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, schema, null, index, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, schema, emptyValue, index, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, schema, table, null, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, schema, table, emptyValue, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, schema, table, index, null, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, schema, table, index, emptyValue, "xyz"));

			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, secondDatabase, null, table, index, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, secondDatabase, emptyValue, table, index, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, secondDatabase, schema, null, index, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, secondDatabase, schema, emptyValue, index, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, secondDatabase, schema, table, null, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, secondDatabase, schema, table, emptyValue, property, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, secondDatabase, schema, table, index, null, "xyz"));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Update(TestConnection, secondDatabase, schema, table, index, emptyValue, "xyz"));

			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, null, table, index, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, emptyValue, table, index, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, schema, null, index, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, schema, emptyValue, index, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, schema, table, null, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, schema, table, emptyValue, property));

			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, secondDatabase, null, table, index, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, secondDatabase, emptyValue, table, index, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, secondDatabase, schema, null, index, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, secondDatabase, schema, emptyValue, index, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, secondDatabase, schema, table, null, property));
			AssertExceptionThrown<ArgumentException>(() => ExtProperty.Index.Delete(TestConnection, secondDatabase, schema, table, emptyValue, property));
		}

		public void TestTableDefinitionActual()
		{
			var expected = GetTableDefinitionFromFile();
			AssertMultilineASCIIEquals("TableDefinition from database must be in sync with Schema_Main.sql", expected.ToUpperInvariant(), ExtProperty.TableDefinition.ToUpperInvariant());
		}

		public void TestTableDefinitionConstant()
		{
			var expected = GetTableDefinitionFromFile();
			AssertMultilineASCIIEquals("Const TableDefinition must be in sync with Schema_Main.sql", expected.ToUpperInvariant(), ExtProperty.ConstTableDefinition.ToUpperInvariant());
		}

		public void TestIsTableDefinitionMatched()
		{
			var mainDatabase = ((ICurrentDbControl)TestConnection).InitialDatabase;
			CombineAssertions(() =>
			{
				var allDatabases = TestConnection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW & ~DatabaseType.SingleSharedRef);
				foreach (var dbName in allDatabases)
				{
					var expected = dbName == mainDatabase || RefDbTableNameResolver.IsSharedDatabase(dbName);
					AssertEquals(string.Format(CultureInfo.InvariantCulture, "Database '{0}' matched", dbName), expected, ExtProperty.IsTableDefinitionMatched(TestConnection, dbName));
				}
			});
		}

		public void TestGetUpgradeCommands()
		{
			CombineAssertions(() =>
			{
				var upgradeScripts = ExtProperty.GetUpgradeCommands().ToList();

				AssertEquals("Number of Upgrade Scripts", 2, upgradeScripts.Count);
				AssertEquals("Audit Columns", ExtProperty.AddAuditColumns(), upgradeScripts[0]);
				AssertEquals("Audit Indexes", ExtProperty.AddAuditIndexes(), upgradeScripts[1]);
			});
		}

		[UseSnapshotProtection]
		public void TestUpgradeRequired()
		{
			const string dbName = "TestUpgradeRequired";
			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbName);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, dbName);
				}

				using (var adminConnection = Db.NewAdminConnection(dbName))
				{
					var upgradeRequired = ExtProperty.UpgradeRequired(adminConnection);
					AssertEquals("No table - should upgrade", true, upgradeRequired);

					var sql = @"CREATE TABLE [STMEXTENDEDPROPERTY] (
 [SEP_CLASS] VARCHAR(60) NOT NULL DEFAULT '',
 [SEP_DATABASENAMESUFFIX] VARCHAR(128) NOT NULL DEFAULT '',
 [SEP_SCHEMANAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_MAJOROBJECTNAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_MINOROBJECTNAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_NAME] VARCHAR(200) NOT NULL DEFAULT '',
 [SEP_VALUE] VARCHAR(200) NOT NULL DEFAULT ''
);";
					adminConnection.ExecuteNonQuery(sql);

					upgradeRequired = ExtProperty.UpgradeRequired(adminConnection);
					AssertEquals("Table exists without Audit columns - should upgrade", true, upgradeRequired);

					adminConnection.ExecuteNonQuery(ExtProperty.AddAuditColumns());

					upgradeRequired = ExtProperty.UpgradeRequired(adminConnection);
					AssertEquals("Table exists with Audit columns - no upgrade", false, upgradeRequired);
				}
			}
			finally
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestUpgradeDatabase_Full()
		{
			const string dbName = "TestUpgradeFull";
			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbName);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, dbName);
				}

				using (var adminConnection = Db.NewAdminConnection(dbName))
				{
					var tblExists = DataUtils.ObjectExists(adminConnection, ExtProperty.TableName);
					AssertEquals("Pre-req 1: No table", false, tblExists);
					var colCountSql = @"SELECT COUNT(*) FROM sys.objects so JOIN sys.columns sc ON so.[object_id] = sc.[object_id] WHERE so.[type] = 'U' AND so.[name] = 'StmExtendedProperty';";
					var indexCountSql = "SELECT COUNT(*) FROM sys.indexes WHERE [name] LIKE '%[_]SEP[_]%'";

					CombineAssertions(() =>
					{
						ExtProperty.UpgradeDatabase(adminConnection);

						var counts = adminConnection.ExecuteScalar<int>(colCountSql);
						AssertEquals("Table with 11 columns created", 11, counts);

						counts = adminConnection.ExecuteScalar<int>(indexCountSql);
						AssertEquals("3 SEP_ indexes created", 3, counts);

						AssertNoExceptionThrown("Running upgrade twice should not cause errors", () =>
						{
							ExtProperty.UpgradeDatabase(adminConnection);
						});
					});
				}
			}
			finally
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbName);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestUpgradeDatabase_Partial()
		{
			const string dbName = "TestUpgradePartial";
			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbName);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, dbName);
				}

				using (var adminConnection = Db.NewAdminConnection(dbName))
				{
					var sql = @"CREATE TABLE [STMEXTENDEDPROPERTY] (
 [SEP_CLASS] VARCHAR(60) NOT NULL DEFAULT '',
 [SEP_DATABASENAMESUFFIX] VARCHAR(128) NOT NULL DEFAULT '',
 [SEP_SCHEMANAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_MAJOROBJECTNAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_MINOROBJECTNAME] NVARCHAR(128) NOT NULL DEFAULT '',
 [SEP_NAME] VARCHAR(200) NOT NULL DEFAULT '',
 [SEP_VALUE] VARCHAR(200) NOT NULL DEFAULT ''
);";
					adminConnection.ExecuteNonQuery(sql);

					var colCountSql = @"SELECT COUNT(*) FROM sys.objects so JOIN sys.columns sc ON so.[object_id] = sc.[object_id] WHERE so.[type] = 'U' AND so.[name] = 'StmExtendedProperty' AND sc.[name] LIKE 'SEP[_]SYSTEM%';";
					var indexCountSql = "SELECT COUNT(*) FROM sys.indexes WHERE [name] IN ('NR_RX__SEP_SYSTEMCREATETIMEUTC','NR_RX__SEP_SYSTEMLASTEDITTIMEUTC')";

					var counts = adminConnection.ExecuteScalar<int>(colCountSql);
					AssertEquals("Pre-req 1: No SEP_SYSTEM columns", 0, counts);
					counts = adminConnection.ExecuteScalar<int>(indexCountSql);
					AssertEquals("Pre-req 2: No SEP_SYSTEM indexes", 0, counts);

					CombineAssertions(() =>
					{
						ExtProperty.UpgradeDatabase(adminConnection);

						counts = adminConnection.ExecuteScalar<int>(colCountSql);
						AssertEquals("4 SEP_SYSTEM columns added", 4, counts);

						counts = adminConnection.ExecuteScalar<int>(indexCountSql);
						AssertEquals("2 SEP_SYSTEM indexes added", 2, counts);

						AssertNoExceptionThrown("Running upgrade scripts twice should not cause errors", () =>
						{
							ExtProperty.UpgradeDatabase(adminConnection);
						});
					});
				}
			}
			finally
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, dbName);
				}
			}
		}

		#region Implementation

		string GetTableDefinitionFromFile()
		{
			var definition = new StringBuilder();

			var schema_main_script = new ScriptManager().MaindDbSchemaScript;
			var pattern = @"^.*" + ExtProperty.TableName + @"[^;]*;";
			var options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Compiled;
			foreach (Match match in Regex.Matches(schema_main_script, pattern, options))
			{
				var statement = Regex.Replace(match.Value, @"\r\n(?:\r\n)+;", "\r\n;", options);

				if (definition.Length > 0)
				{
					definition.AppendLine();
				}

				definition.Append(statement);
			}

			return definition.ToString();
		}

		#endregion // Implementation
	}
}
