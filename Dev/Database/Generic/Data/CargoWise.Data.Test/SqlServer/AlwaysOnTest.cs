using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Data.SqlServer.Testing
{
	public class AlwaysOnTest : TransactionedTestCase
	{
		public void TestIsDbPartOfAlwaysOn()
		{
			var mockDbForTest = "ESN_MockDb_For_AlwaysOnTest";

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(mockDbForTest, Db.DatabaseName))
			{
				AssertEquals("DB is part of AlwaysOn", false, AlwaysOn.IsDbPartOfAlwaysOn(connection, mockDbForTest));

				AlwaysOn.AlwaysOnDatabases_ForTest.Value = new List<string>(new[] { mockDbForTest });
				AssertEquals("DB is part of AlwaysOn", true, AlwaysOn.IsDbPartOfAlwaysOn(connection, mockDbForTest));
			}
		}

		public void TestIsPrimaryReplica()
		{
			string mockDbForTest = "AlwaysOnTestDatabase";

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					AssertEquals("Database is on Primary Replica", false, AlwaysOn.IsDbOnPrimaryReplica(connection, mockDbForTest));
					AdoTestUtils.CreateDbIfNotExists(connection, mockDbForTest);
					AssertEquals("Database is on Primary Replica", false, AlwaysOn.IsDbOnPrimaryReplica(connection, mockDbForTest));
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, mockDbForTest);
				}
			}
		}

		public void TestGetAlwaysOnSecondaryReplicaNamesList()
		{
			var mockDbForTest = "ESN_MockDb_For_ReplicaNamesListTest";

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, mockDbForTest, Db.DatabaseName))
			{
				var currentDb = "";
				AlwaysOn.UserAction_ForTest.Value = (conn) =>
				{
					currentDb = conn.ExecuteScalar<string>("SELECT DB_NAME()");
				};

				AssertEquals("AlwaysOn replica count", 0, AlwaysOn.GetAlwaysOnSecondaryReplicaNamesList(mockDbForTest).Count);
				AssertEquals(Db.SqlMasterDb, currentDb);
				AssertEquals("AlwaysOn replica count", 0, AlwaysOn.GetAlwaysOnSecondaryReplicaNamesList(connection, mockDbForTest).Count);
				AssertEquals(Db.SqlMasterDb, currentDb);

				// Arrange
				var primaryReplicaServerName = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(TestConnection.ServerNameReportedByDatabase);
				AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
				{
					new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
					new AlwaysOnReplicaInfo { ReplicaServerName = primaryReplicaServerName, AvailabilityMode = 1 },
				};

				// Act
				// Assert
				AssertArrayEqualsByElements("AlwaysOn secondary replicas"
					, expected: new string[] { "testReplica1" }
					, actual: AlwaysOn.GetAlwaysOnSecondaryReplicaNamesList(mockDbForTest).ToArray());

				AssertArrayEqualsByElements("AlwaysOn secondary replicas"
					, expected: new string[] { "testReplica1" }
					, actual: AlwaysOn.GetAlwaysOnSecondaryReplicaNamesList(connection, mockDbForTest).ToArray());
			}
		}

		public void TestGetAlwaysOnReplicaInfos()
		{
			var mockDbForTest = "ESN_MockDb_For_GetAlwaysOnReplicaInfosTest";

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, mockDbForTest, Db.DatabaseName))
			{
				var currentDb = "";
				AlwaysOn.UserAction_ForTest.Value = (conn) =>
				{
					currentDb = conn.ExecuteScalar<string>("SELECT DB_NAME()");
				};

				AssertEquals("AlwaysOn replica count", 0, AlwaysOn.GetAlwaysOnReplicaInfos(connection, mockDbForTest).Count);
				AssertEquals(Db.SqlMasterDb, currentDb);

				// Arrange
				var primaryReplicaServerName = DataUtils.GetDbSeverFullDomainNameIncludingSqlPort(TestConnection.ServerNameReportedByDatabase);
				var replicas = new List<AlwaysOnReplicaInfo>
				{
					new AlwaysOnReplicaInfo { ReplicaServerName = "testReplica1", AvailabilityMode = 1 },
					new AlwaysOnReplicaInfo { ReplicaServerName = primaryReplicaServerName, AvailabilityMode = 1 },
				};
				AlwaysOn.ReplicaNames_ForTest.Value = replicas;

				// Act
				// Assert
				AssertArrayEqualsByElements("AlwaysOn replicas"
					, expected: new string[] { "testReplica1", primaryReplicaServerName }
					, actual: AlwaysOn.GetAlwaysOnReplicaInfos(connection, mockDbForTest).Select(info => info.ReplicaServerName).ToArray());
				AssertEquals(Db.SqlMasterDb, currentDb);
			}
		}
	}
}
