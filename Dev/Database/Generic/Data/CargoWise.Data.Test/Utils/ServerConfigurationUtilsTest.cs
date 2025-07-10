using System;
using System.Collections.Generic;
using CargoWise.Data.SqlServer;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class ServerConfigurationUtilsTest : TestCase, IDisposable
	{
		readonly string testLinkedServerName = "LOOPBACK_TEST_" + Guid.NewGuid().ToString().Replace("-", "");
		readonly AdminConnection connection = Db.NewAdminConnection();

		protected override void SetUp()
		{
			base.SetUp();
			DeleteLinkedServerIfExists();
		}

		void DeleteLinkedServerIfExists()
		{
			connection.ExecuteNonQuery(@"
				IF EXISTS (SELECT * FROM sys.servers WHERE [name] = @LINKEDSERVERNAME) 
				BEGIN 
					EXEC sys.sp_dropserver @LINKEDSERVERNAME, @droplogins='droplogins'
				END",
			cmd => cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, testLinkedServerName));
		}

		protected override void TearDown()
		{
			DeleteLinkedServerIfExists();
			base.TearDown();
		}

		public void TestConfigureServerLoopbackLinkedServer()
		{
			var exists = QueryLinkedServerExists(testLinkedServerName);

			AssertEquals(false, exists);

			new ServerConfigurationUtils(testLinkedServerName).EnsureServerIsConfigured(connection);

			exists = QueryLinkedServerExists(testLinkedServerName);

			AssertEquals(true, exists);
		}

		public void TestConfigureServerFromDatabase()
		{
			var exists = QueryLinkedServerExists(testLinkedServerName);

			AssertEquals(false, exists);

			new ServerConfigurationUtils(testLinkedServerName).ConfigureServerFromDatabase(connection, Db.DatabaseName);

			exists = QueryLinkedServerExists(testLinkedServerName);

			AssertEquals(true, exists);
		}

		[ExpectNoExceptions]
		public void TestConfigureServerFromDatabase_StoredProcedureDoesNotExist()
		{
			var exists = QueryLinkedServerExists(testLinkedServerName);

			AssertEquals(false, exists);

			// master database does not have ConfigureServer stored procedure
			new ServerConfigurationUtils(testLinkedServerName).ConfigureServerFromDatabase(connection, "Master");

			exists = QueryLinkedServerExists(testLinkedServerName);

			AssertEquals(false, exists);
		}

		public void TestEnsureAllServersServersAreConfigured_PartOfAvailablilityGroup()
		{
			var mockDb = "TestDatabase";
			var serverName = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(connection.ServerNameReportedByDatabase);

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, mockDb))
			using (var testConnection = Db.NewAdminConnection(serverName, mockDb))
			{
				// Arrange
				var createTable = @"
					CREATE TABLE ConfigureServerCalls(dt datetime)";
				testConnection.ExecuteNonQuery(createTable);

				var configureProc = @"
					CREATE PROCEDURE [dbo].[ConfigureServer]
							@LINKEDSERVERNAME	sysname = ""LOOPBACK""
	   				AS
					BEGIN
						Insert ConfigureServerCalls values (getDate())
					END";
				testConnection.ExecuteNonQuery(configureProc);

				var replicas = new List<AlwaysOnReplicaInfo>
				{
					new AlwaysOnReplicaInfo { ReplicaServerName = serverName, AvailabilityMode = 1 },
					new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
				};

				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
				AlwaysOn.ReplicaNames_ForTest.Value = replicas;
				bool connectionToPrimary = false;
				bool connectionToSecondary = false;
				//// Act
				var utils = new ServerConfigurationUtils(testLinkedServerName, (sName, dbName) =>
				{
					if (sName == serverName)
					{
						connectionToPrimary = true;
					}
					if (sName == "testReplica1")
					{
						connectionToSecondary = true;
					}
					return Db.NewAdminConnection(serverName, "Master");
				});

				utils.EnsureServerIsConfiguredAndTryConfiguringOtherReplicas(testConnection, out _);

				//// Assert
				var queryCallCount = @"select count(*) from ConfigureServerCalls";
				var callCount = testConnection.ExecuteScalar<int>(queryCallCount);
				Assert(callCount == 2);
				Assert(!connectionToPrimary);
				Assert(connectionToSecondary);
			}
		}

		public void TestEnsureAllServersServersAreConfigured_NotPartOfAvailablilityGroup()
		{
			var mockDb = "TestDatabase";
			var serverName = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(connection.ServerNameReportedByDatabase);
			AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = false;

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, mockDb))
			using (var testConnection = Db.NewAdminConnection(serverName, mockDb))
			{
				// Arrange
				var createTable = @"
					CREATE TABLE ConfigureServerCalls(dt datetime)";
				testConnection.ExecuteNonQuery(createTable);

				var configureProc = @"
					CREATE PROCEDURE [dbo].[ConfigureServer]
							@LINKEDSERVERNAME	sysname = ""LOOPBACK""
	   				AS
					BEGIN
						Insert ConfigureServerCalls values (getDate())
					END";
				testConnection.ExecuteNonQuery(configureProc);

				//// Act
				var utils = new ServerConfigurationUtils(testLinkedServerName, (sName, dbName) =>
				{
					throw new Exception("Should not be here.");
				});

				utils.EnsureServerIsConfiguredAndTryConfiguringOtherReplicas(testConnection, out _);

				//// Assert
				var queryCallCount = @"select count(*) from ConfigureServerCalls";
				var callCount = testConnection.ExecuteScalar<int>(queryCallCount);
				Assert(callCount == 1);
			}
		}

		bool QueryLinkedServerExists(string name)
		{
			return connection.Exists("FROM sys.servers WHERE [name] = @LINKEDSERVERNAME",
				cmd => cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, name));
		}

		public void Dispose()
		{
			connection?.Dispose();
		}
	}
}
