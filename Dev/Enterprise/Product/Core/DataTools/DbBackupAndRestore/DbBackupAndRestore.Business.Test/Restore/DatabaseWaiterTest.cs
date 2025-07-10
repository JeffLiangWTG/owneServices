using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CargoWise.Data;
using CargoWise.IO;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Testing.Restore.Helper
{
	class DatabaseWaiterTest : TestCase
	{
		public void TestDatabaseWaiterWaitUntilDatabaseReadyToRestore()
		{
			// Arrange
			var dbWaiterMock = new Mock<DatabaseWaiter>();
			var loggerMock = new Mock<Logger>();
			dbWaiterMock.SetupSequence(m => m.IsDatabaseInDesiredState(It.IsAny<AdminConnection>(), Db.DatabaseName, loggerMock.Object, It.IsAny<List<string>>()))
				.Returns(false)
				.Returns(true);
			var dbWaiter = dbWaiterMock.Object;

			// Act
			using (var connection = Db.NewAdminConnection())
			{
				dbWaiter.WaitUntilDatabaseReadyToRestore(connection, Db.DatabaseName, loggerMock.Object, CancellationToken.None);
			}

			// Assert
			dbWaiterMock.Verify(m => m.IsDatabaseInDesiredState(It.IsAny<AdminConnection>(), Db.DatabaseName, loggerMock.Object, It.IsAny<List<string>>()), Times.Exactly(2));
			AssertNotNullOrEmpty(Db.DatabaseName);
		}

		public void TestDatabaseWaiterWaitUntilDatabaseIsOnline()
		{
			// Arrange
			var dbWaiterMock = new Mock<DatabaseWaiter>();
			var loggerMock = new Mock<Logger>();
			dbWaiterMock.SetupSequence(m => m.IsDatabaseInDesiredState(It.IsAny<AdminConnection>(), Db.DatabaseName, loggerMock.Object, It.IsAny<List<string>>()))
				.Returns(false)
				.Returns(true);
			var dbWaiter = dbWaiterMock.Object;

			// Act
			using (var connection = Db.NewAdminConnection())
			{
				dbWaiter.WaitUntilDatabaseIsOnline(connection, Db.DatabaseName, loggerMock.Object, CancellationToken.None);
			}

			// Assert
			dbWaiterMock.Verify(m => m.IsDatabaseInDesiredState(It.IsAny<AdminConnection>(), Db.DatabaseName, loggerMock.Object, It.IsAny<List<string>>()), Times.Exactly(2));
			AssertNotNullOrEmpty(Db.DatabaseName);
		}

		public void TestIsDatabaseInDesiredState_ReturnsFalseWhenDatabaseIsNotInDesiredState()
		{
			// Arrange
			var dbWaiter = new DatabaseWaiter();
			var loggerMock = new Mock<Logger>();
			bool result;

			// Act
			using (var connection = Db.NewAdminConnection())
			{
				result = dbWaiter.IsDatabaseInDesiredState(connection, Db.DatabaseName, loggerMock.Object, It.IsAny<List<string>>());
			}

			// Assert
			AssertEquals(false, result);
		}

		public void TestIsDatabaseInDesiredState_ReturnsTrueWhenDatabaseDescriptionIsNullOrWhiteSpace()
		{
			// Arrange
			var dbWaiter = new DatabaseWaiter();
			var loggerMock = new Mock<Logger>();
			var dbName = "OfflineDB";
			bool result;

			// Act
			using (var connection = Db.NewAdminConnection())
			{
				result = dbWaiter.IsDatabaseInDesiredState(connection, dbName, loggerMock.Object, new List<string> { });
			}

			// Assert
			Assert(result);
		}

		public void TestIsDatabaseInRestoringState_ReturnsTrueWhenDatabaseDescriptionIsRestoring()
		{
			// Arrange
			var dbWaiter = new DatabaseWaiter();
			var loggerMock = new Mock<Logger>();
			const string databaseName = "DatabaseWaiterTestRestoringState";
			using (var tempDir = new TempDirectory())
			{
				var backupFileName = Path.Combine(tempDir.DirectoryName, $"{databaseName}.bak");
				var tailLogBackupFileName = Path.Combine(tempDir.DirectoryName, $"{databaseName}{Utilities.TransactionLogBackupFileExtension}");

				// Act & Assert
				try
				{
					AdoTestUtils.CreateDbIfNotExists(Db.NewAdminConnection(), databaseName, DbRecoveryModel.Full);

					using (var connection = Db.NewAdminConnection(Db.ServerName, databaseName))
					{
						connection.ExecuteNonQuery("CREATE TABLE t1 (c1 int not null)");
						connection.ExecuteNonQuery($"BACKUP DATABASE [{databaseName}] TO DISK = N'{backupFileName}' WITH INIT");
						connection.ExecuteNonQuery("USE master");

						Assert(!dbWaiter.IsDatabaseInDesiredState(connection, databaseName, loggerMock.Object, new List<string> { "RESTORING" }));

						connection.ExecuteNonQuery($"BACKUP LOG [{databaseName}] TO DISK = N'{tailLogBackupFileName}' WITH NORECOVERY");
						connection.ExecuteNonQuery($"RESTORE DATABASE [{databaseName}] FROM DISK = N'{backupFileName}' WITH NORECOVERY");

						Assert(dbWaiter.IsDatabaseInDesiredState(connection, databaseName, loggerMock.Object, new List<string> { "RESTORING" }));
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(Db.NewAdminConnection(), databaseName);
				}
			}
		}

		public void TestDatabaseWaiterThrowsIfCancelled()
		{
			// Arrange
			var dbWaiterMock = new Mock<DatabaseWaiter>();
			var loggerMock = new Mock<Logger>();
			dbWaiterMock.Setup(m => m.IsDatabaseInDesiredState(It.IsAny<AdminConnection>(), Db.DatabaseName, loggerMock.Object, It.IsAny<List<string>>()))
				.Returns(false);
			var dbWaiter = dbWaiterMock.Object;

			// Act + Assert
			using (var connection = Db.NewAdminConnection())
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.Zero))
			{
				AssertExceptionThrown<OperationCanceledException>("WaitUntilDatabaseInDesiredState Cancelled",
					() => dbWaiter.WaitUntilDatabaseReadyToRestore(connection, Db.DatabaseName, loggerMock.Object, cancellationTokenSource.Token));
			}
			dbWaiterMock.Verify(m => m.IsDatabaseInDesiredState(It.IsAny<AdminConnection>(), Db.DatabaseName, loggerMock.Object, It.IsAny<List<string>>()), Times.Once());
		}

		public void TestDatabaseWaiterWaitUntilDatabaseRemovedFromAvailabilityGroup()
		{
			// Arrange
			var dbWaiter = new DatabaseWaiter();
			var loggerMock = new Mock<Logger>();
			var alwaysOnHelperMock = new Mock<IAlwaysOnHelper>();
			alwaysOnHelperMock.SetupSequence(m => m.IsDbPartOfAlwaysOn(It.IsAny<DbConnection>(), Db.DatabaseName))
				.Returns(true)
				.Returns(false);

			// Act
			using (var connection = Db.NewAdminConnection())
			{
				AssertNoExceptionThrown(() => dbWaiter.WaitUntilDatabaseRemovedFromAvailabilityGroup(connection, Db.DatabaseName, loggerMock.Object, CancellationToken.None, alwaysOnHelperMock.Object));
			}

			// Assert
			alwaysOnHelperMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<DbConnection>(), Db.DatabaseName), Times.Exactly(2));
		}

		public void TestDatabaseWaiterThrowsIfCancelledWhileWaitingForDbRemoval()
		{
			// Arrange
			var dbWaiter = new DatabaseWaiter();
			var loggerMock = new Mock<Logger>();
			var alwaysOnHelperMock = new Mock<IAlwaysOnHelper>();
			alwaysOnHelperMock.Setup(m => m.IsDbPartOfAlwaysOn(It.IsAny<DbConnection>(), Db.DatabaseName)).Returns(true);

			// Act + Assert
			using (var connection = Db.NewAdminConnection())
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.Zero))
			{
				AssertExceptionThrown<OperationCanceledException>("WaitUntilDatabaseRemovedFromAvailabilityGroup Cancelled",
					() => dbWaiter.WaitUntilDatabaseRemovedFromAvailabilityGroup(connection, Db.DatabaseName, loggerMock.Object, cancellationTokenSource.Token, alwaysOnHelperMock.Object));
			}

			alwaysOnHelperMock.Verify(m => m.IsDbPartOfAlwaysOn(It.IsAny<DbConnection>(), Db.DatabaseName), Times.Once);
		}
	}
}
