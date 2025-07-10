using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class BiDatabaseCheckerNonTransactionedTest : TestCase
	{
		public void TestSchemaUpgradeIfBiDbsDoNotExist()
		{
			using (var conn = Db.NewAdminConnection())
			{
				try
				{
					var auditDbChecker = new BiDatabaseChecker(conn, mainDbName + Db.AuditDatabaseSuffix);
					AssertEquals("Schema upgrade required before creating Audit database.", true, auditDbChecker.ForceBiDatabaseUpgrade);

					var edwDbChecker = new BiDatabaseChecker(conn, mainDbName + Db.EdwDatabaseSuffix);
					AssertEquals("Schema upgrade required before creating EDW database.", true, edwDbChecker.ForceBiDatabaseUpgrade);

					CreateDatabase(conn, mainDbName + Db.AuditDatabaseSuffix);
					CreateDatabase(conn, mainDbName + Db.EdwDatabaseSuffix);

					AssertEquals("Schema upgrade required after creating Audit database.", false, auditDbChecker.ForceBiDatabaseUpgrade);
					AssertEquals("Schema upgrade required after creating EDW database.", false, edwDbChecker.ForceBiDatabaseUpgrade);
				}
				finally
				{
					conn.ExecuteNonQuery($"DROP DATABASE [{mainDbName + Db.AuditDatabaseSuffix}]");
					conn.ExecuteNonQuery($"DROP DATABASE [{mainDbName + Db.EdwDatabaseSuffix}]");
				}
			}
		}

		void CreateDatabase(DbConnection conn, string dbName)
		{
			conn.ExecuteNonQuery(string.Format("IF NOT EXISTS (SELECT null FROM sys.databases WHERE name = '{0}') CREATE DATABASE [{0}]", dbName));

			var mainDbSchemaVersion = SchemaVersion.Application.ToString();
			DataUtils.SaveDbExtendedProperty(conn, BiConstants.MainDbSchemaVersionExtPtyName, mainDbSchemaVersion, dbName);
		}

		const string mainDbName = "TestDbName";
	}
}
