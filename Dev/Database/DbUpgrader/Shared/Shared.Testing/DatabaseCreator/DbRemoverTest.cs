using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared
{
	public sealed class DbRemoverTest : TestCase
	{
		[SnailTest()]
		public void TestDrop()
		{
			string testDbName = "Enterprise.DbUpgrader.Shared.DbRemoverTest.TestDropDb";

			using (var testConn = Db.NewAdminConnection())
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(testConn, testDbName);
					AssertEquals("Database exists?", true, Db.Connection.DatabaseExists(testDbName));
				}
				finally
				{
					new DbRemover(testDbName).Drop(testConn);
				}

				AssertEquals("Database exists?", false, Db.Connection.DatabaseExists(testDbName));
			}
		}

		[SnailTest()]
		public void TestDropKillsOtherConnections()
		{
			string testDbName = "Enterprise.DbUpgrader.Shared.DbRemoverTest.TestDropKills";

			using (var testConn = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(testConn, testDbName);
				AssertEquals("Database exists?", true, Db.Connection.DatabaseExists(testDbName));

				using (var conn2 = Db.NewAdminConnection(testDbName))
				{
					conn2.BeginTransaction();
					conn2.Command("create table Foo(id int not null)").ExecuteNonQuery();

					new DbRemover(testDbName).Drop(testConn);
				}

				AssertEquals("Database exists?", false, Db.Connection.DatabaseExists(testDbName));
			}
		}
	}
}
