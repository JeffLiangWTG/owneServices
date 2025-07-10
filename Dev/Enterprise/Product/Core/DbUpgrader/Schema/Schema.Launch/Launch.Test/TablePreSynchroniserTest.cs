using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	sealed class TablePreSynchroniserDependenciesTest : TestCase
	{
		public void TestRefreshDependentScripts()
		{
			// Arrange
			var testDbName = $"TempDb_{nameof(TestRefreshDependentScripts)}";
			using (RefreshDependentScriptsHelper.Prepare(testDbName))
			{
				RefreshDependentScriptsHelper.CreateDependencies();

				var manager = new UpgradeManagerForTestWithOutputBuffer();

				// Act
				TablePreSynchroniser.RefreshDependentScripts(Db.Connection, testDbName, "dbo", "TableA", manager);

				// Assert
				AssertEquals(
					"No exception",
					false,
					manager.OutputTextCollection.Cast<string>().Any(x => x.Contains("Skipped")));

				AssertEquals(
					"Stored procedure works well again",
					"ABCDEFGHIJ1",
					RefreshDependentScriptsHelper.ExecuteStoredProcedure("abcdefghij"));

				var sqlEvents = SqlEventTracker.Instance.SqlEventList;
				var getRefreshScriptEvent = sqlEvents
					.Where(x => x.StartsWith("-- TablePreSynchroniser.RefreshDependentScripts(Get)"))
					.ToArray();
				AssertEquals("One statement to get the refresh script", 1, getRefreshScriptEvent.Length);
				AssertContains("AND d.referenced_id = OBJECT_ID(@TableName, N'U')", getRefreshScriptEvent[0]);
				AssertContains("[@TableName = \"[dbo].[TableA]\", NVarChar(4000)]", getRefreshScriptEvent[0]);

				var executeRefreshScriptEvent = sqlEvents
					.Where(x => x.StartsWith("-- TablePreSynchroniser.RefreshDependentScripts(Execute)"))
					.ToArray();
				AssertEquals("One statement to execute the refresh script", 1, executeRefreshScriptEvent.Length);
				AssertContains("sys.sp_refreshsqlmodule @FullName;", executeRefreshScriptEvent[0]);
				AssertContains("[@FullName = \"[dbo].[to_upper]\", NVarChar(257)]", executeRefreshScriptEvent[0]);
			}
		}

		public void TestRefreshDependentScriptsInUserRepository()
		{
			// Arrange
			var testDbName = $"TempDb{nameof(TestRefreshDependentScripts)}";
			var userRepoDbName = testDbName + DbUserRepository.RepositoryDbSuffix;
			using (RefreshDependentScriptsHelper.Prepare(testDbName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(userRepoDbName, Db.DatabaseName))
			{
				RefreshDependentScriptsHelper.CreateDependencies();
				RefreshDependentScriptsHelper.CreateUserRepositoryDependencies(testDbName, userRepoDbName);

				var manager = new UpgradeManagerForTestWithOutputBuffer();

				// Act
				var connection = Db.Connection;
				TablePreSynchroniser.RefreshDependentScripts(connection, testDbName, "dbo", "TableA", manager);

				// Assert
				CombineAssertions(() =>
				{
					// Views should all work now...
					AssertNoExceptionThrown("TableAView", () => connection.ExecuteNonQuery("select * from TableAView"));
					AssertNoExceptionThrown("UserTableA", () => connection.ExecuteNonQuery($"select * from {userRepoDbName}.dbo.UserTableA"));
					AssertNoExceptionThrown("UserTableAView", () => connection.ExecuteNonQuery($"select * from {userRepoDbName}.dbo.UserTableAView"));
					AssertNoExceptionThrown("UserLocalView", () => connection.ExecuteNonQuery($"select * from {userRepoDbName}.dbo.UserLocalView"));
					AssertNoExceptionThrown("TableASynonymView", () => connection.ExecuteNonQuery($"select * from {userRepoDbName}.dbo.TableASynonymView"));

					var allLogs = string.Join("; ", manager.OutputTextCollection);
					AssertNotContains("Skipped", allLogs);
				});

				var sqlEvents = SqlEventTracker.Instance.SqlEventList;
				CombineAssertions(() =>
				{
					var getRefreshScriptEvent = sqlEvents
						.Where(x => x.StartsWith("-- TablePreSynchroniser.RefreshDependentScripts(Get)"))
						.ToArray();
					AssertEquals("One statement to get the refresh script", 1, getRefreshScriptEvent.Length);
					AssertContains("AND d.referenced_id = OBJECT_ID(@TableName, N'U')", getRefreshScriptEvent[0]);
					AssertContains("[@TableName = \"[dbo].[TableA]\", NVarChar(4000)]", getRefreshScriptEvent[0]);
				});

				CombineAssertions(() =>
				{
					var executeRefreshScriptEvent = sqlEvents
						.Where(x => x.StartsWith("-- TablePreSynchroniser.RefreshDependentScripts(Execute)"))
						.OrderBy(x => x)
						.ToArray();
					AssertEquals("statements to execute the refresh script", 2, executeRefreshScriptEvent.Length);

					AssertContains("sys.sp_refreshsqlmodule @FullName;", executeRefreshScriptEvent[0]);
					AssertContains("[@FullName = \"[dbo].[TableAView]\", NVarChar(257)]", executeRefreshScriptEvent[0]);

					AssertContains("[@FullName = \"[dbo].[to_upper]\", NVarChar(257)]", executeRefreshScriptEvent[1]);
				});

				CombineAssertions(() =>
				{
					var getRefreshUserRepositoryScriptEvent = sqlEvents
						.Where(x => x.StartsWith("-- TablePreSynchroniser.RefreshOtherDbDependentScripts(Get)"))
						.OrderBy(x => x)
						.ToArray();
					AssertEquals("statements to get the refresh script", 3, getRefreshUserRepositoryScriptEvent.Length);

					AssertEquals("DbName = \"TempDbTestRefreshDependentScripts\", SchemaName = \"dbo\", ObjectName = \"TableA\"",
						ExtractSqlParamsFromEvent(getRefreshUserRepositoryScriptEvent[0]));

					AssertEquals("DbName = \"TempDbTestRefreshDependentScripts\", SchemaName = \"dbo\", ObjectName = \"TableAView\"",
						ExtractSqlParamsFromEvent(getRefreshUserRepositoryScriptEvent[1]));

					AssertEquals("DbName = \"TempDbTestRefreshDependentScripts\", SchemaName = \"dbo\", ObjectName = \"to_upper\"",
						ExtractSqlParamsFromEvent(getRefreshUserRepositoryScriptEvent[2]));
				});

				CombineAssertions(() =>
				{
					var executeRefreshUserRepositoryScriptEvent = sqlEvents
						.Where(x => x.StartsWith("-- TablePreSynchroniser.RefreshOtherDbDependentScripts(Execute)"))
						.OrderBy(x => x)
						.ToArray();
					AssertEquals("statements to execute the refresh script", 4, executeRefreshUserRepositoryScriptEvent.Length);

					AssertContains("[@FullName = \"[dbo].[TableASynonymView]\", NVarChar(257)]", executeRefreshUserRepositoryScriptEvent[0]);
					AssertContains("[@FullName = \"[dbo].[UserLocalView]\", NVarChar(257)]", executeRefreshUserRepositoryScriptEvent[1]);
					AssertContains("[@FullName = \"[dbo].[UserTableA]\", NVarChar(257)]", executeRefreshUserRepositoryScriptEvent[2]);
					AssertContains("[@FullName = \"[dbo].[UserTableAView]\", NVarChar(257)]", executeRefreshUserRepositoryScriptEvent[3]);
				});
			}
		}

		static string ExtractSqlParamsFromEvent(string scriptEvent)
		{
			return string.Join(", ", scriptEvent
				.Split(new[] { "[@" }, StringSplitOptions.RemoveEmptyEntries)
				.Skip(1)
				.Select(x => x.Split(',').First()));
		}

		static class RefreshDependentScriptsHelper
		{
			public static IDisposable Prepare(string dbName)
			{
				var dropDbDisposable = CreateDb();
				var useDbDisposable = ((ICurrentDbControl)Db.Connection).UseDatabase(dbName);

				return new DisposableAction(
					() =>
					{
						useDbDisposable.Dispose();
						dropDbDisposable.Dispose();
					});

				IDisposable CreateDb()
				{
					using (var connection = Db.NewAdminConnection())
					{
						return AdoTestUtils.CreateDbDropExistingDisposable(connection, dbName, Db.DatabaseName);
					}
				}
			}

			public static void CreateDependencies()
			{
				Db.Connection.ExecuteNonQuery(@"
CREATE TABLE TableA
(
	Id INT,
	Col2 VARCHAR(10)
);
INSERT INTO TableA (Id, Col2) VALUES (1, 'One');

CREATE TYPE mytype FROM NVARCHAR(5);");

				Db.Connection.ExecuteNonQuery($@"
CREATE PROCEDURE dbo.to_upper @a mytype AS
BEGIN
	select upper(@a) + CAST(count(1) as NVARCHAR) from TableA
END;");

				var spFullName = default(string);
				Db.Connection.ExecuteReader(
					$@"-- Just distinguish from production
{TablePreSynchroniser.GetRefreshScriptFormat}",
					cmd =>
					{
						cmd.AddParameter("@TableName", SqlDbType.NVarChar, "TableA");
					},
					record =>
					{
						spFullName = record.GetString(0) + "." + record.GetString(1);
					});
				AssertEquals("Stored procedure schema name", "dbo.to_upper", spFullName);

				AssertEquals(
					"Stored procedure works well",
					"ABCDE1",
					ExecuteStoredProcedure("abcde"));

				Db.Connection.ExecuteNonQuery(@"-- Increase the length of the alias type.  
sp_rename 'mytype', 'myoldtype', 'userdatatype';
CREATE TYPE mytype FROM NVARCHAR(10);");
				AssertExceptionThrown<SqlException>(() => ExecuteStoredProcedure("abcdefgh"));
			}

			internal static void CreateUserRepositoryDependencies(string dbName, string userRepoDbName)
			{
				var connection = Db.Connection;

				// Add a view in main db of TableA
				connection.ExecuteNonQuery("CREATE View TableAView AS Select * from TableA");

				using (((ICurrentDbControl)connection).UseDatabase(userRepoDbName))
				{
					// Create a synonym for the main table
					connection.ExecuteNonQuery($"CREATE SYNONYM dbo.TableA for {dbName}.dbo.TableA");

					// Add views in user db:
					// - view of TableA
					// - view of the view in main db
					// - a view of the local view of TableA to verify we drill down the dependencies
					// - view using synonym
					connection.ExecuteNonQuery($"CREATE View UserTableA AS Select * from {dbName}.dbo.TableA");
					connection.ExecuteNonQuery($"CREATE View UserTableAView AS Select * from {dbName}.dbo.TableAView");
					connection.ExecuteNonQuery($"CREATE View UserLocalView AS Select * from UserTableA");
					connection.ExecuteNonQuery($"CREATE View TableASynonymView AS Select * from TableA");
				}

				// Drop a column from main tableA so that views that select it will give an error until refreshed
				connection.ExecuteNonQuery("ALTER TABLE TableA DROP COLUMN Col2");

				// Check the error occurs
				AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("select * from TableAView"));
				AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery($"select * from {userRepoDbName}.dbo.UserTableA"));
				AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery($"select * from {userRepoDbName}.dbo.UserTableAView"));
				AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery($"select * from TableASynonymView"));
			}

			public static string ExecuteStoredProcedure(string parameter)
			{
				return Db.Connection.ExecuteScalar<string>($"EXEC dbo.to_upper '{parameter}'");
			}
		}
	}
}
