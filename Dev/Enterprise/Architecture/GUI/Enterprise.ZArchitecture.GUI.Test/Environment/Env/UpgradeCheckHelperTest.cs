using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	[UseSnapshotProtection]
	class UpgradeCheckHelperTest : TestCase
	{
		public void TestUpgradeCheckHelperReturnsFalseOnDatabaseUpgradeInProgressException()
		{
			using var adminConnection = Db.NewAdminConnection();
			_ = adminConnection.ResetLockout();
			AssertEquals(DbLockoutState.NoLockout, DbLockout.CheckLockoutState(adminConnection));

			try
			{
				// Arrange
				_ = DbLockout.AcquireLockout(adminConnection, LockoutReason.Upgrade);
				AssertEquals(DbLockoutState.ValidLockout, adminConnection.CheckLockoutState());

				// Act
				// Assert
				_ = AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => new UpgradeCheckHelper().HasBeenUpgraded());
			}
			finally
			{
				_ = DbLockout.ResetLockout(adminConnection);
			}
		}
	}
}
