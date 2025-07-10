using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.ReferenceDatabases;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class UnsupportedRefDatabaseRemoverTest : TestCase
	{
		[SnailTest]
		public void TestDropUnusedRefDatabases()
		{
			var upgradeContext = new Mock<IUpgradeContext>().Object;
			var logger = new MemoryLoggerForTest();

			var supportedRefDbSuffix = new ReferenceDbUpgradeDirector(upgradeContext, Db.Connection, logger).ExclusiveRefDbSuffixList.First();
			var mockDb_Main = "MockDbTestDropUnsupportedRefDatabases";
			var mockDb_Supported = mockDb_Main + supportedRefDbSuffix;
			var mockDb_Unsupported_Ent_ZA = mockDb_Main + "_RefDb_Ent_ZA";
			var mockDb_Unsupported_Ent_GB = mockDb_Main + "_RefDb_Ent_GB";
			var mockDb_Shared_Ent_GB = "CW-RefDb-Ent-GB-000000";

			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Supported, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Unsupported_Ent_ZA, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Unsupported_Ent_GB, mockDb_Main);
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockDb_Shared_Ent_GB, mockDb_Main);

					using (var connection = Db.NewAdminConnection(Db.ServerName, mockDb_Main))
					{
						connection.ExecuteNonQuery($@"
							CREATE TYPE {TVPHelper.TVP_nvarchar} AS TABLE (Value nvarchar(128) NOT NULL PRIMARY KEY CLUSTERED);
							");

						var supportedRefDbSynonym = supportedRefDbSuffix.Replace("_", "") + "_SomeTable";
						connection.ExecuteNonQuery($@"
							CREATE SYNONYM [{supportedRefDbSynonym}] FOR [{mockDb_Supported}]..[SomeTable];
							CREATE SYNONYM [RefDbEntZA_SomeTable] FOR [{mockDb_Unsupported_Ent_ZA}]..[SomeTable];
							CREATE SYNONYM [RefDbEntGB_SomeTable] FOR [{mockDb_Shared_Ent_GB}]..[SomeTable];
							");

						var expected = new string[] { mockDb_Main, mockDb_Supported, mockDb_Unsupported_Ent_ZA, mockDb_Shared_Ent_GB, RefDbTableNameResolver.SingleRefDatabaseName };

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("PRECONDITION: All databases", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Main));
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Supported));
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Unsupported_Ent_ZA));
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Unsupported_Ent_GB));
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Shared_Ent_GB));
						AssertEquals("DB exists?", true, connection.DatabaseExists(RefDbTableNameResolver.SingleRefDatabaseName));

						new UnsupportedRefDatabaseRemover().DropUnusedDatabases(upgradeContext, connection, logger);

						var expectedLog = new StringBuilder()
							.AppendLine("Removing unsupported reference databases (2)")
							.AppendLine($"> Database [{mockDb_Unsupported_Ent_GB}] (1/2) has been dropped on server [{connection.ServerName}]")
							.AppendLine($"> Database [{mockDb_Unsupported_Ent_ZA}] (2/2) has been dropped on server [{connection.ServerName}]")
							;
						AssertMultilineASCIIEquals(expectedLog.ToString(), logger.ToString());

						expected = new string[] { mockDb_Main, mockDb_Supported, mockDb_Shared_Ent_GB, RefDbTableNameResolver.SingleRefDatabaseName };

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("No unsupported exclusive DBs regardless of synonyms", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Main));
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Supported));
						AssertEquals("DB exists?", false, connection.DatabaseExists(mockDb_Unsupported_Ent_ZA));
						AssertEquals("DB exists?", false, connection.DatabaseExists(mockDb_Unsupported_Ent_GB));
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Shared_Ent_GB));
						AssertEquals("DB exists?", true, connection.DatabaseExists(RefDbTableNameResolver.SingleRefDatabaseName));

						// second run
						logger.Clear();
						new UnsupportedRefDatabaseRemover().DropUnusedDatabases(upgradeContext, connection, logger);

						expectedLog = new StringBuilder()
							.AppendLine("Removing unsupported reference databases (0)")
							.AppendLine("> No databases to drop detected.")
							;
						AssertMultilineASCIIEquals(expectedLog.ToString(), logger.ToString());

						((IPhysicalRefDbLocation)connection).ClearRefDbNameBuffers();
						AssertContainsExactElementsInAnyOrder("No unsupported exclusive DBs regardless of synonyms", expected, connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW).ToList());
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Main));
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Supported));
						AssertEquals("DB exists?", false, connection.DatabaseExists(mockDb_Unsupported_Ent_ZA));
						AssertEquals("DB exists?", false, connection.DatabaseExists(mockDb_Unsupported_Ent_GB));
						AssertEquals("DB exists?", true, connection.DatabaseExists(mockDb_Shared_Ent_GB));
						AssertEquals("DB exists?", true, connection.DatabaseExists(RefDbTableNameResolver.SingleRefDatabaseName));
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Supported, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Unsupported_Ent_ZA, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Unsupported_Ent_GB, mockDb_Main);
					AdoTestUtils.DropDbIfExists(adminConnection, mockDb_Shared_Ent_GB, mockDb_Main);
				}
			}
		}
	}
}
