using CargoWise.Bi.Common;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class BiDatabaseCheckerTransactionedTest : TransactionedTestCase
	{
		public void TestBiDbSchemaVersion()
		{
			var auditDbChecker = new BiDatabaseChecker(TestConnection, Db.AuditDatabaseName);
			AssertEquals("Schema upgrade required before dropping Audit databases extended property.", false, auditDbChecker.ForceBiDatabaseUpgrade);

			var edwDbChecker = new BiDatabaseChecker(TestConnection, Db.EdwDatabaseName);
			AssertEquals("Schema upgrade required before dropping EDW databases extended property.", false, edwDbChecker.ForceBiDatabaseUpgrade);

			DataUtils.DropDbExtendedProperty(TestConnection, BiConstants.MainDbSchemaVersionExtPtyName, Db.AuditDatabaseName);
			DataUtils.DropDbExtendedProperty(TestConnection, BiConstants.MainDbSchemaVersionExtPtyName, Db.EdwDatabaseName);

			AssertEquals("Schema upgrade required after dropping Audit databases extended property.", true, auditDbChecker.ForceBiDatabaseUpgrade);
			AssertEquals("Schema upgrade required after dropping EDW databases extended property.", true, edwDbChecker.ForceBiDatabaseUpgrade);
		}
	}
}
