using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DataTools.DbBackupAndRestore.Business.Common;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore
{
	class DbHeaderOnlyReaderTest : TestCase
	{
		public void TestReadHeaderOnly_ReturnsHeaderInfo()
		{
			// Arrange
			const string databaseName = "DbBackupAndRestoreTest33EAF77F57974243A518E8EF30338C4B";
			using (var tempDir = new TempDirectory())
			{
				var backupFileName = Path.Combine(tempDir.DirectoryName, $"{databaseName}.bak");

				using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(Db.NewAdminConnection(), databaseName)))
				using (var connection = Db.NewAdminConnection())
				{
					AdoTestUtils.CreateDbIfNotExists(connection, databaseName);
					connection.ExecuteNonQuery($"BACKUP DATABASE [{databaseName}] TO DISK = '{backupFileName}' WITH INIT");

					// Act
					var headerInfo = new DbHeaderOnlyReader().ReadHeaderOnly(connection, backupFileName);

					// Assert
					CombineAssertions(() =>
					{
						AssertNotNull(headerInfo);
						AssertEquals(databaseName, headerInfo.DatabaseName);
						Assert(headerInfo.LastLSN > 0);
					});
				}
			}
		}
	}
}
