using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using Dat.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed partial class BuildDeployerTest
	{
		sealed class DropDatabaseTest : TestCase
		{
			[ExpectNoExceptions]
			public void TestDatabaseDoesNotExist()
			{
				BuildDeployer.DropDatabase(connection, "whateverdb", logger.Object);
			}

			public void TestDatabaseIsNotJoinedToAg()
			{
				// Arrange
				const string dbName = nameof(TestDatabaseIsNotJoinedToAg);
				using var droppingDbIfExists = AdoTestUtils.CreateDbDropExistingDisposable(connection, dbName);

				var removeDbFromAg = false;
				AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value = _ =>
				{
					removeDbFromAg = true;
					return false;
				};

				// Act
				BuildDeployer.DropDatabase(connection, dbName, logger.Object);

				// Assert
				AssertEquals(false, connection.DatabaseExists(dbName));
				AssertEquals(false, removeDbFromAg);

				// Cleanup
				AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.ResetValue();
			}

			public void TestDatabaseIsJoinedToAg()
			{
				// Arrange
				const string dbName = nameof(TestDatabaseIsJoinedToAg);
				using var droppingDbIfExists = AdoTestUtils.CreateDbDropExistingDisposable(connection, dbName);

				var aonInvocations = new List<string>();
				var dbNameBeingDroppedActuallyDueToAg = string.Empty;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;
				AlwaysOn.UserAction_ForTest.Value = connection =>
				{
					aonInvocations.Add("GetAlwaysOnSecondaryReplicaNamesListOnMasterDb");
				};
				AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value = name =>
				{
					dbNameBeingDroppedActuallyDueToAg = name;
					aonInvocations.Add("RemoveDatabaseFromAlwaysOnSetup");
					return true;
				};

				// Act
				BuildDeployer.DropDatabase(connection, dbName, logger.Object);

				// Assert
				AssertEquals(dbName, dbNameBeingDroppedActuallyDueToAg);
				AssertEquals(false, connection.DatabaseExists(dbName));
				AssertContainsExactElementsInExactOrder(
					"Invoke GetAlwaysOnSecondaryReplicaNamesListOnMasterDb first",
					new[] { "GetAlwaysOnSecondaryReplicaNamesListOnMasterDb", "RemoveDatabaseFromAlwaysOnSetup" },
					aonInvocations);

				// Cleanup
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.ResetValue();
				AlwaysOn.UserAction_ForTest.ResetValue();
				AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.ResetValue();
			}

			AdminConnection connection;
			Mock<ITaskLogger> logger;

			protected override void SetUp()
			{
				base.SetUp();
				connection = Db.NewAdminConnection(Db.SqlMasterDb);
				logger = new Mock<ITaskLogger>();
			}

			protected override void TearDown()
			{
				connection?.Dispose();
				base.TearDown();
			}
		}
	}
}
