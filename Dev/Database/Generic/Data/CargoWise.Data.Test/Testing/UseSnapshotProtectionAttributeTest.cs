using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Data.Test.Testing
{
	sealed class UseSnapshotProtectionAttributeTest : TransactionedTestCase
	{
		[UseSnapshotProtection(new[] { DatabaseType.BI })]
		public void TestAttributeWithNoOverloads_DBTypes()
		{
			Assert(TestConnection.IsInTransaction);
			DBChangesForTest(TestConnection, Db.AuditDatabaseName);
			DBChangesForTest(TestConnection, Db.EdwDatabaseName);
		}

		[UseSnapshotProtection(new[] { DatabaseType.BI }, skipTransaction: true)]
		public void TestAttributeWithOverloads_DBTypes()
		{
			Assert(!TestConnection.IsInTransaction);
			DBChangesForTest(TestConnection, Db.AuditDatabaseName);
			DBChangesForTest(TestConnection, Db.EdwDatabaseName);
		}

		void DBChangesForTest(DbConnection connection, string dbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				connection.ExecuteNonQuery($@"
INSERT INTO [biadmin].[MasterState] (ParamName, ParamValue)
VALUES('ADM_NUDGE_TIME', '2024-08-07 00:00:00.000')");

				var results = connection.ExecuteScalar($@"
SELECT ParamValue
FROM [biadmin].[MasterState]
WHERE ParamName = 'ADM_NUDGE_TIME';");

				AssertEquals("2024-08-07 00:00:00.000", results);
			}
		}
	}
}
