using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class RefDatabaseRemoverTest : TestCase
	{
		public void TestDropUnusedRefDatabases()
		{
			var upgradeContext = new Mock<IUpgradeContext>().Object;
			var logger = new MemoryLoggerForTest();

			var mockDb_Main = "MockDbTestDropUnusedRefDatabases";
			var mockDb_Exclusive_1 = mockDb_Main + "_RefDb_Aaa_Aa";
			var mockDb_Exclusive_2 = mockDb_Main + "_RefDb_Bbb_Bb";
			var mockDb_Exclusive_3 = mockDb_Main + "_RefDb_Ccc_Cc";
			var mockDb_Shared = "CW-RefDb-Bbb-Bb-000000";
			var mockDb_SharedAG = "CW-AG-RefDb-ORDWP4-CP1AS1-Ccc-Cc-000000";

			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Exclusive_1, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Exclusive_2, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Exclusive_3, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Shared, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_SharedAG, mockDb_Main);

					using (var connection = Db.NewAdminConnection(Db.ServerName, mockDb_Main))
					{
						// Clears reference database buffers and gets all database list again
						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();

						var expected = new string[] { mockDb_Main, mockDb_Exclusive_1, mockDb_Exclusive_2, mockDb_Exclusive_3, RefDbTableNameResolver.SingleRefDatabaseName };
						AssertContainsExactElementsInAnyOrder("PRECONDITION: All used databases", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());

						new RefDatabaseRemover().DropUnusedDatabases(upgradeContext, connection, logger);

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("No shared DBs => no unused DBs => Used databases have not been changed", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());

						connection.ExecuteNonQuery($@"
							CREATE SYNONYM [RefDbAaaAa_SomeTable] FOR [{mockDb_Exclusive_1}]..[SomeTable];
							CREATE SYNONYM [RefDbBbbBb_SomeTable] FOR [{mockDb_Exclusive_2}]..[SomeTable];
							CREATE SYNONYM [RefDbCccCc_SomeTable] FOR [{mockDb_Exclusive_3}]..[SomeTable];
							");

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("Apply synonyms => Used databases", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());

						new RefDatabaseRemover().DropUnusedDatabases(upgradeContext, connection, logger);

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("No shared DBs => no unused DBs => Used databases have not been changed", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());

						connection.ExecuteNonQuery($@"
							DROP SYNONYM [RefDbBbbBb_SomeTable];
							CREATE SYNONYM [RefDbBbbBb_SomeTable] FOR [{mockDb_Shared}]..[SomeTable];

							DROP SYNONYM [RefDbCccCc_SomeTable];
							CREATE SYNONYM [RefDbCccCc_SomeTable] FOR [{mockDb_SharedAG}]..[SomeTable];
							");

						expected = new string[] { mockDb_Main, mockDb_Exclusive_1, mockDb_Shared, mockDb_SharedAG, RefDbTableNameResolver.SingleRefDatabaseName };

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("Used databases changed to include shared", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());
						AssertEquals("Does unused DB exist?", true, connection.DatabaseExists(mockDb_Exclusive_2));

						new RefDatabaseRemover().DropUnusedDatabases(upgradeContext, connection, logger);

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("Used databases have not been changed", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());
						AssertEquals("Does unused DB exist?", false, connection.DatabaseExists(mockDb_Exclusive_2));
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Exclusive_1, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Exclusive_2, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Exclusive_3, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Shared, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_SharedAG, mockDb_Main);
				}
			}
		}

		public void TestDropRefDbIsAttemptedOnAllAlwaysOnReplicas()
		{
			var upgradeContext = new Mock<IUpgradeContext>().Object;
			var logger = new MemoryLoggerForTest();

			var mockDb_Main = "MockDbTestDropUnusedRefDatabases";
			var mockDb_Exclusive = mockDb_Main + "_RefDb_Bbb_Bb";
			var mockDb_Shared = "CW-RefDb-Bbb-Bb-000000";
			var replicaName = Db.ServerName;

			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Exclusive, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Shared, mockDb_Main);

					using (var connection = Db.NewAdminConnection(Db.ServerName, mockDb_Main))
					{
						var sql = String.Format(@"CREATE SYNONYM [RefDbBbbBb_SomeTable] FOR [{0}]..[SomeTable];", mockDb_Shared);
						connection.ExecuteNonQuery(sql);

						var expected = new string[] { mockDb_Main, mockDb_Shared, RefDbTableNameResolver.SingleRefDatabaseName };
						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("Used databases changed to include shared", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());
						AssertEquals("Does unused DB exist?", true, connection.DatabaseExists(mockDb_Exclusive));

						var dbRemover = new RefDatabaseRemoverForTest();
						dbRemover.Test_IsDbPartOfAlwaysOn = true;
						dbRemover.Test_AlwaysOnReplicaNamesList = new List<string>() { "replicaName1", "replicaName2", "replicaName3", "replicaName4", "replicaName5", "replicaName6" };
						dbRemover.Test_RemoveDatabaseFromAlwaysOnSetup = true;
						AssertNoExceptionThrown(() => dbRemover.DropUnusedDatabases(upgradeContext, connection, logger));

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("Used databases have not been changed", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());
						var expectedLog = new StringBuilder()
							.AppendLine("Removing unused exclusive reference databases due to migrating them to shared reference databases (1)")
							.AppendLine($"> Database [{mockDb_Exclusive}] (1/1) cannot be dropped on server [replicaName1] (1/6). The database state is 'ONLINE'")
							.AppendLine($"> Database [{mockDb_Exclusive}] (1/1) cannot be dropped on server [replicaName2] (2/6). The database state is 'RECOVERING'")
							.AppendLine($"> Database [{mockDb_Exclusive}] (1/1) has been dropped on server [replicaName3] (3/6).")
							.AppendLine($"> Database [{mockDb_Exclusive}] (1/1) cannot be dropped on server [replicaName4] (4/6). The server is unavailable")
							.AppendLine($"> Database [{mockDb_Exclusive}] (1/1) cannot be dropped on server [replicaName5] (5/6). The database state is 'WhateverState'")
							.AppendLine($"> Database [{mockDb_Exclusive}] (1/1) has been dropped on server [replicaName6] (6/6).")
							.AppendLine($"> Database [{mockDb_Exclusive}] (1/1) has been dropped on server [{connection.ServerName}]")
							;
						AssertEquals(expectedLog.ToString(), logger.ToString());
						AssertEquals("Does unused DB exist?", false, connection.DatabaseExists(mockDb_Exclusive));
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Exclusive, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Shared, mockDb_Main);
				}
			}
		}

		sealed class RefDatabaseRemoverForTest : RefDatabaseRemover
		{
			public bool? Test_IsDbPartOfAlwaysOn;
			public List<string> Test_AlwaysOnReplicaNamesList;
			public bool? Test_RemoveDatabaseFromAlwaysOnSetup;
			string replicaBeingProcessed = string.Empty;

			public RefDatabaseRemoverForTest()
			{
				TryCount = 1;
			}

			protected override bool IsDbPartOfAlwaysOn(DbConnection connection, string refDbName)
			{
				if (Test_IsDbPartOfAlwaysOn.HasValue)
				{
					return Test_IsDbPartOfAlwaysOn.Value;
				}
				else
				{
					return base.IsDbPartOfAlwaysOn(connection, refDbName);
				}
			}

			protected override List<string> GetAlwaysOnSecondaryReplicaNamesList(string refDbName)
			{
				if (Test_AlwaysOnReplicaNamesList != null)
				{
					return Test_AlwaysOnReplicaNamesList;
				}
				else
				{
					return base.GetAlwaysOnSecondaryReplicaNamesList(refDbName);
				}
			}

			protected override bool RemoveDatabaseFromAlwaysOnSetup(string refDbName)
			{
				if (Test_RemoveDatabaseFromAlwaysOnSetup.HasValue)
				{
					return Test_RemoveDatabaseFromAlwaysOnSetup.Value;
				}
				else
				{
					return base.RemoveDatabaseFromAlwaysOnSetup(refDbName);
				}
			}

			protected override void DropRefDb(AdminConnection replicaConnection, string refDbName)
			{
				if (replicaBeingProcessed.StartsWith("replicaName", StringComparison.OrdinalIgnoreCase))
				{
					replicaBeingProcessed = string.Empty;
				}
				else
				{
					base.DropRefDb(replicaConnection, refDbName);
				}
			}

			protected override AdminConnection GetAdminConnectionToReplica(string replicaName)
			{
				replicaBeingProcessed = replicaName;
				if (replicaName.Equals("replicaName4", StringComparison.OrdinalIgnoreCase))
				{
					var error = SqlExceptionBuilder.CreateSqlError(11001, byte.MaxValue, byte.MinValue, replicaName, "Unreachable server", "", 0);
					var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					throw SqlExceptionBuilder.CreateSqlException(errors);
				}
				else
				{
					return Db.NewAdminConnection();
				}
			}

			protected override string GetDatabaseStateDescription(AdminConnection replicaConnection, string refDbName)
			{
				switch (replicaBeingProcessed)
				{
					case "replicaName1":
						return "ONLINE";
					case "replicaName2":
						return "RECOVERING";
					case "replicaName3":
						return "RESTORING";
					case "replicaName4":
						return "ONLINE";
					case "replicaName5":
						return "WhateverState";
					case "replicaName6":
						return "RESTORING";
					default:
						return replicaConnection.DatabaseStateDescription(refDbName);
				}
			}
		}
	}
}
