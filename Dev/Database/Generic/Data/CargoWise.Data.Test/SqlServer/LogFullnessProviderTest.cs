using System;
using System.IO;
using System.Threading;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace CargoWise.Data.SqlServer.Testing
{
	sealed class LogFullnessProviderTest : TestCase
	{
		const string testDatabaseName = "CargoWise.Data.SqlServer.Testing.LogFullnessProviderTest.DB";

		static void CreateTestDatabase(AdminConnection conn)
		{
			AdoTestUtils.DropDbIfExists(conn, testDatabaseName);
			conn.CreateDatabase(testDatabaseName);
			var commandString = Invariant($"ALTER DATABASE [{testDatabaseName}] SET RECOVERY FULL");

			using (var command = conn.Command(commandString))
			{
				command.ExecuteNonQuery();
			}
		}

		static void BackupTestDatabase(AdminConnection connection, string location)
		{
			using (var cmd = connection.Command("EP_BackupDB"))
			{
				cmd.CommandType = System.Data.CommandType.StoredProcedure;
				cmd.AddParameter("@DbName", System.Data.SqlDbType.VarChar, 128, testDatabaseName);
				cmd.AddParameter("@FolderPath", System.Data.SqlDbType.VarChar, 800, Path.GetDirectoryName(location));
				cmd.AddParameter("@FileName", System.Data.SqlDbType.VarChar, 200, Path.GetFileName(location));
				cmd.AddParameter("@BkpType", System.Data.SqlDbType.VarChar, 20, "FULL");
				cmd.AddParameter("@IsCompressed", System.Data.SqlDbType.Bit, true);
				cmd.ExecuteNonQuery();
			}
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("DB Backup")]
		public void TestOverallStuff()
		{
			var databaseBackupPath = string.Empty;

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					CreateTestDatabase(connection);
					var location = connection.GetDbFiles(testDatabaseName)[0];
					Assert($"Database [{testDatabaseName}] is created", File.Exists(location));
					databaseBackupPath = Path.Combine(Path.GetDirectoryName(location), Path.GetFileNameWithoutExtension(location) + "_4LogFullnessTest.BAK");
					BackupTestDatabase(connection, databaseBackupPath);

					Assert($"Check Full Backup is created on: '{databaseBackupPath}'", File.Exists(databaseBackupPath));
					var initialAttempt = RunLogFullnessProvider(DbRecoveryModel.Full);
					AttemptInGettingBacklog attemptAfterInsert = null;
					var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

					do
					{
						var commandString = @"
							IF EXISTS(Select NULL From sys.tables Where name = 'tblTempTableWI') drop table tblTempTableWI;
							select * into tblTempTableWI from sys.objects;
							Insert Into tblTempTableWI select * from tblTempTableWI;
							Insert Into tblTempTableWI select * from tblTempTableWI;
							Insert Into tblTempTableWI select * from tblTempTableWI;
							drop table tblTempTableWI;";

						using (((ICurrentDbControl)connection).UseDatabase(testDatabaseName))
						using (var command = connection.Command(commandString))
						{
							command.ExecuteNonQuery();
						}

						attemptAfterInsert = RunLogFullnessProvider(DbRecoveryModel.Full);
					}
					while (attemptAfterInsert.BacklogResult.BacklogSize <= initialAttempt.BacklogResult.BacklogSize && !cts.Token.IsCancellationRequested);

					Assert("Backlog Size After insert should be greater then backlog before the insert", attemptAfterInsert.BacklogResult.BacklogSize > initialAttempt.BacklogResult.BacklogSize);

					var attemptForSimpleDatabase = RunLogFullnessProvider(DbRecoveryModel.Simple);
					AssertEquals("Backlog Size for simple should always be zero, as we don't care about the logs in this scenario", 0, attemptForSimpleDatabase.BacklogResult.BacklogSize);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDatabaseName);

					if (File.Exists(databaseBackupPath))
					{
						File.Delete(databaseBackupPath);
					}
				}
			}

			AttemptInGettingBacklog RunLogFullnessProvider(DbRecoveryModel dbRecoveryModel)
			{
				var mockServiceProvider = new Mock<IServiceProvider>();
				var originalServiceProvider = GlobalServiceProvider.Instance;
				mockServiceProvider.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(t => originalServiceProvider.GetService(t));
				mockServiceProvider.Setup(x => x.GetService(typeof(IDbRecoveryModelManager))).Returns(DbRecoveryModelManagerMock.WithActual(dbRecoveryModel));

				using (GlobalServiceProvider.Configure(mockServiceProvider.Object))
				{
					return new LogFullnessProvider(testDatabaseName).GetCurrentBacklog();
				}
			}
		}
	}
}
