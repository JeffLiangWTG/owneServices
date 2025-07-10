using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(ep_ClientDatabases))]
	class ep_ClientDatabasesTest : DbCreateScriptTest
	{
		public void TestDatabaseListsMatch()
		{
			var expectedDbList = Db.Connection.GetDatabases(DatabaseType.All);
			var procDbList = new List<string>();

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				using (var cmd = connection.Command("EXEC " + ScriptToTest.Name))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string dbName = reader["name"].ToString();
						int dbId = (int)reader["database_id"];
						AssertDatabaseId(dbName, dbId);
						procDbList.Add(dbName);
					}
				}
			}

			string missingDbs = string.Join("\r\n\t", expectedDbList.Where(db => !procDbList.Contains(db)));
			Assert($"The following expected databases were not returned by {ScriptToTest.Name}\r\n\t{missingDbs}", missingDbs.Length == 0);

			string unexpectedDbs = string.Join("\r\n\t", procDbList.Where(db => !expectedDbList.Contains(db)));
			Assert($"The following unexpected databases were returned by {ScriptToTest.Name}\r\n\t{unexpectedDbs}", unexpectedDbs.Length == 0);
		}

		public void TestDatabaseListsMatchWhenUsingStaffDbReaderAccount()
		{
			var expectedListOfDatabases = Db.Connection.GetDatabases(DatabaseType.All);
			var actualListOfDatabases = new List<string>();
			var readOnlyStaffUsername = "Test_StaffLogin";
			var readOnlyStaffPassword = "AKompleksPasvord#$%234";

			try
			{
				DropStaffLoginAccountIfExists(readOnlyStaffUsername, expectedListOfDatabases);
				PrepareStaffLoginAccount(readOnlyStaffUsername, readOnlyStaffPassword, Db.Connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef));

				using (var connection = Db.NewExtraConnection(Db.ServerName, Db.DatabaseName, readOnlyStaffUsername, readOnlyStaffPassword))
				{
					using (var cmd = connection.Command("EXEC " + ScriptToTest.Name))
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var dbName = reader["name"].ToString();
							// To be deleted when SRDb migrations are done
							if (!actualListOfDatabases.Contains(dbName))
							{
								actualListOfDatabases.Add(dbName);
							}
						}
					}
				}
				AssertEquals("Expected number of databases", expectedListOfDatabases.Count(), actualListOfDatabases.Count);
			}
			finally
			{
				DropStaffLoginAccountIfExists(readOnlyStaffUsername, expectedListOfDatabases);
			}
		}

		void PrepareStaffLoginAccount(string readOnlyStaffUsername, string readOnlyStaffPassword, IEnumerable<string> expectedListOfDatabases)
		{
			var sqlScript = string.Format(CultureInfo.InvariantCulture, @"
Create Login [{0}] with password = N'{1}',  DEFAULT_DATABASE=[{2}]
DENY VIEW ANY DATABASE TO [{0}]
"
, readOnlyStaffUsername
, readOnlyStaffPassword
, Db.DatabaseName
);
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				connection.ExecuteNonQuery(sqlScript);
			}

			sqlScript = string.Format(CultureInfo.InvariantCulture, @"
CREATE USER [{0}] FOR LOGIN [{0}]
ALTER ROLE [cwReaderRole] ADD MEMBER [{0}]
"
, readOnlyStaffUsername
);
			foreach (var db in expectedListOfDatabases)
			{
				using (var connection = Db.NewAdminConnection(db))
				{
					connection.ExecuteNonQuery(sqlScript);
				}
			}
		}

		void DropStaffLoginAccountIfExists(string readOnlyStaffUsername, IEnumerable<string> expectedListOfDatabases)
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbLoginIfExists(adminConnection, readOnlyStaffUsername);
			}

			foreach (var db in expectedListOfDatabases)
			{
				using (var connection = Db.NewAdminConnection(db))
				{
					string sqlScript = string.Format(CultureInfo.InvariantCulture, "IF exists(SELECT null FROM sys.database_principals WHERE name = '{0}') DROP USER [{0}]", readOnlyStaffUsername);
					connection.ExecuteNonQuery(sqlScript);
				}
			}
		}

		public void TestRunningInTransactionThrowsException()
		{
			AssertExceptionThrown(
				"Running inside a transaction.",
				typeof(SqlException),
				"This procedure must run outside of a transaction context.",
				() => Db.Connection.ExecuteNonQuery("EXEC " + ScriptToTest.Name));
		}

		void AssertDatabaseId(string dbName, int dbId)
		{
			string sqlText = "SELECT db_id('" + dbName + "')";
			int expectedDbId = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Database ID [" + dbName + "]", expectedDbId, dbId);
		}
	}
}

