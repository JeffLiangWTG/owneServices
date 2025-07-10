using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class RunInExtraTransactionAttributeTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestWithAttribute()
		{
			AssertEquals(true, UseSnapshotProtectionAttribute.IsProtected);
		}

		public void TestWithoutAttribute()
		{
			AssertEquals(false, UseSnapshotProtectionAttribute.IsProtected);
		}

		[UseSnapshotProtection(new[] { DatabaseType.BI })]
		public void TestMultipleDatabaseSnapshots()
		{
			AssertContainsExactElementsInAnyOrder(new[] { Db.AuditDatabaseName, Db.EdwDatabaseName }, UseSnapshotProtectionAttribute.SnapshotDatabases);
		}
	}
}
