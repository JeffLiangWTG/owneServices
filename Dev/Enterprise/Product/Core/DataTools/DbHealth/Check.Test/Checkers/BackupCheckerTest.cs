using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(BackupChecker))]
	sealed class BackupCheckerTest : CheckerTestCaseBase
	{
		public void TestCheckBackupIsBeingPerformedPeriodically()
		{
			BackupCheckerForTesting testChecker = new BackupCheckerForTesting();
			testChecker.overrideIsDbOlderThanTwoDays = true;

			testChecker.BackupHoursOldThresholdOverride = TimeSpan.FromDays(-1);
			DbHealthWarningList testWarningList = new DbHealthWarningList();
			testWarningList.MainDbName = Db.DatabaseName;
			testChecker.CheckBackupIsBeingPerformedPeriodically_Exposed(testWarningList);

			AssertEquals("Warings", 1, testWarningList.Count);
			AssertEquals("Source Type", DatabaseWarning.DatabaseSourceType, testWarningList[0].SourceType);
			AssertEquals("Source", Db.DatabaseName, testWarningList[0].Source);
			AssertStartsWith("Action\r\n" + testWarningList[0].Action, $"{BrandingFactory.Instance.ProductName} databases should be backed up regularly (we recommend daily), usually to a separate physical disk and the backup files then transferred to tape before they are overwritten by the next backup", testWarningList[0].Action);
		}

		public void TestWarning()
		{
			var checker = new BackupChecker();
			DbHealthWarningList testWarningList = new DbHealthWarningList();

			DataTable table = new DataTable();
			table.Columns.Add("BackupFinishDate");
			table.Columns.Add("DatabaseName");
			DataRow backupInfo = table.NewRow();

			backupInfo["BackupFinishDate"] = DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss");
			backupInfo["DatabaseName"] = "TestDatabase";
			table.Rows.Add(backupInfo);

			checker.AddWarningIfBackupIsNotRecent(testWarningList, backupInfo, "dbName", "backup path");

			AssertEquals("There are no warnings", 0, testWarningList.Count);

			table = new DataTable();
			table.Columns.Add("BackupFinishDate");
			table.Columns.Add("DatabaseName");
			backupInfo = table.NewRow();

			backupInfo["BackupFinishDate"] = "2016-10-11 12:13:14";
			backupInfo["DatabaseName"] = "TestDatabase";
			table.Rows.Add(backupInfo);

			checker.AddWarningIfBackupIsNotRecent(testWarningList, backupInfo, "dbName", "backup path");

			AssertEquals("There is only one warning", 1, testWarningList.Count);

			var warningText = @"Database [dbName] is not being backed up periodically.
Last backup was taken on 11-Oct-16 12:13 to path [backup path].";

			AssertEquals("Warning text", warningText, testWarningList[0].Description);
		}

		public void TestWarning_NonBackupHistoryFound()
		{
			var checker = new BackupChecker();
			var testWarningList = new DbHealthWarningList();

			checker.AddWarningIfBackupIsNotRecent(testWarningList, null, "dbName", "backup path");

			AssertEquals("There is only one warning", 1, testWarningList.Count);
			var expectedMessage = @"Database [dbName] has a corrupted backup.
Last backup was taken to path [backup path].";

			AssertEquals(expectedMessage, testWarningList[0].Description);
			AssertEquals(expectedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		public void TestDoCheckBackupIsBeingPerformedPeriodically_SuccessGetFullBackupHistory()
		{
			var checker = new BackupCheckerForNonBackupHistoryFound();
			var warningList = new DbHealthWarningList();
			AssertEquals("Precondition", 0, warningList.Count);

			checker.SuccessGetFullBackup = true;
			checker.DoCheckBackupIsBeingPerformedPeriodicallyForTest(new List<string>() { "Dummy" }, null, warningList);
			AssertEquals(0, warningList.Count);
		}

		public void TestDoCheckBackupIsBeingPerformedPeriodically_FailedGetFullBackupHistory()
		{
			var checker = new BackupCheckerForNonBackupHistoryFound();
			var warningList = new DbHealthWarningList();
			AssertEquals("Precondition", 0, warningList.Count);

			checker.SuccessGetFullBackup = false;
			checker.DoCheckBackupIsBeingPerformedPeriodicallyForTest(new List<string>() { "Dummy" }, null, warningList);
			AssertEquals(1, warningList.Count);
			AssertEquals("Database [Dummy] has a corrupted backup.\r\nLast backup was taken to path [Dummy.bak].", warningList[0].Description);

			ErrorReporter.Instance.Clear();
		}

		public void TestDoCheckBackupIsBeingPerformedPeriodically_SuccessGetCurrentBackupHistory()
		{
			var checker = new BackupCheckerForNonBackupHistoryFound();
			var warningList = new DbHealthWarningList();
			AssertEquals("Precondition", 0, warningList.Count);

			checker.ThrowExceptionWhenGetFullBackup = true;
			checker.SuccessGetCurrentBackup = true;
			checker.DoCheckBackupIsBeingPerformedPeriodicallyForTest(new List<string>() { "Dummy" }, null, warningList);
			AssertEquals(1, warningList.Count);
			AssertEquals("Backup file for the database [Dummy] is used by another process. Checking .CURRENT backup instead. File location: \\Dummy.bak", warningList[0].Description);
		}

		public void TestDoCheckBackupIsBeingPerformedPeriodically_FailedGetCurrentBackupHistory()
		{
			var checker = new BackupCheckerForNonBackupHistoryFound();
			var warningList = new DbHealthWarningList();
			AssertEquals("Precondition", 0, warningList.Count);

			checker.ThrowExceptionWhenGetFullBackup = true;
			checker.DoCheckBackupIsBeingPerformedPeriodicallyForTest(new List<string>() { "Dummy" }, null, warningList);
			AssertEquals(2, warningList.Count);
			AssertEquals("Backup file for the database [Dummy] is used by another process. Checking .CURRENT backup instead. File location: \\Dummy.bak", warningList[0].Description);
			AssertEquals("Database [Dummy] has a corrupted backup.\r\nLast backup was taken to path [Dummy.bak.CURRENT].", warningList[1].Description);

			ErrorReporter.Instance.Clear();
		}

		public void TestWarning_BackupIsCorrupted()
		{
			var checker = new BackupChecker();
			var testWarningList = new DbHealthWarningList();

			var table = new DataTable();
			table.Columns.Add("BackupName");
			table.Columns.Add("BackupFinishDate");
			checker.AddWarningIfBackupIsNotRecent(testWarningList, table.NewRow(), "dbName", "backup path");

			AssertEquals("There is only one warning", 1, testWarningList.Count);
			var expectedMessage = @"Database [dbName] has a corrupted backup.
Last backup was taken to path [backup path].";
			AssertEquals(expectedMessage, testWarningList[0].Description);
			AssertEquals(expectedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}

		public void TestCheckIgnoresDatabasesLessThanTwoDaysOld()
		{
			string testDbName = "TestCheckIgnoresDatabasesLessThanTwoDaysOld-Db1";
			var testChecker = new BackupCheckerForTesting();
			var testWarningList = new DbHealthWarningList();
			testWarningList.MainDbName = testDbName;

			using (var connection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(connection, testDbName);

				try
				{
					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);
					testChecker.CheckBackupIsBeingPerformedPeriodically_Exposed(testWarningList);
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}

			AssertEquals("Warings", 0, testWarningList.Count);
		}
		[UseSnapshotProtection]
		public void TestGetListOfBackupFiles()
		{
			using (TestTemporaryDirectory tempDir = new TestTemporaryDirectory())
			{
				var testPath = Path.Combine(tempDir.Directory.FullName, "TestDir");
				try
				{
					var dbName = Db.DatabaseName;
					if (Directory.Exists(testPath))
					{
						Directory.Delete(testPath, true);
					}
					Env.Registry.BackupReferenceDatabases = true;
					Directory.CreateDirectory(testPath);
					File.WriteAllText(Path.Combine(testPath, dbName + "_20160501153456.trn"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "_SD002_20160501153456.trn"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "_UserRepository_20160501153456.trn"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "4.trn"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + ""), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "_SD001"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "6.txt"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + ".bak"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + ".bak.CURRENT"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "2.bak"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "_sd002.bak"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "_userRepository.bak"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "_userRepository.bak.current"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + ".bak.trn"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "9.trn"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "10.bak"), "");
					File.WriteAllText(Path.Combine(testPath, dbName + "_RefDb_Cmr_AU.bak"), "");

					var files = new List<string>();
					var testChecker = new BackupCheckerForTesting();
					using (var conn = Db.NewAdminConnection())
					{
						files = testChecker.GetListOfBackupFiles(conn, testPath);
					}

					AssertEquals("Number of full backup files", 6, files.Count);
					AssertEquals("Main database backup file exists", true, files.Any(x => string.Compare(x, dbName + ".bak", StringComparison.OrdinalIgnoreCase) == 0));
					AssertEquals("Main database backup file exists", true, files.Any(x => string.Compare(x, dbName + ".bak.CURRENT", StringComparison.OrdinalIgnoreCase) == 0));
					AssertEquals("SD001 backup file doesn't exists", false, files.Any(x => string.Compare(x, dbName + "_SD001.bak", StringComparison.OrdinalIgnoreCase) == 0));
					AssertEquals("SD002 backup file does exists", true, files.Any(x => string.Compare(x, dbName + "_SD002.bak", StringComparison.OrdinalIgnoreCase) == 0));
					AssertEquals("_RefDb_Cmr_AU backup file does exists", true, files.Any(x => string.Compare(x, dbName + "_RefDb_Cmr_AU.bak", StringComparison.OrdinalIgnoreCase) == 0));
					AssertEquals("UserRepository database backup file exists", true, files.Any(x => string.Compare(x, dbName + "_UserRepository.bak", StringComparison.OrdinalIgnoreCase) == 0));
					AssertEquals("UserRepository database backup file exists", true, files.Any(x => string.Compare(x, dbName + "_UserRepository.bak.CURRENT", StringComparison.OrdinalIgnoreCase) == 0));

					Env.Registry.BackupReferenceDatabases = false;
					using (var conn = Db.NewAdminConnection())
					{
						files = testChecker.GetListOfBackupFiles(conn, testPath);
					}
					AssertEquals("_RefDb_Cmr_AU backup file does't exists", false, files.Any(x => string.Compare(x, dbName + "_RefDb_Cmr_AU.bak", StringComparison.OrdinalIgnoreCase) == 0));
				}
				finally
				{
					if (Directory.Exists(testPath))
					{
						Directory.Delete(testPath, true);
					}
				}
			}
		}

		public void TestRestoreHeaderOnly()
		{
			using (TestTemporaryDirectory tempDir = new TestTemporaryDirectory())
			{
				var testDbName = "Test92A2DA3A76E740E08E023441D5245F6A";
				var testPath = Path.Combine(tempDir.Directory.FullName, "TestDir");
				var testBackupFileFullPath = Path.Combine(testPath, testDbName + ".bak");
				var dateTimeNow = DateTime.Now.AddSeconds(-1);

				try
				{
					if (Directory.Exists(testPath))
					{
						Directory.Delete(testPath, true);
					}
					Directory.CreateDirectory(testPath);

					using (var conn = Db.NewAdminConnection())
					{
						AdoTestUtils.CreateDbIfNotExists(conn, testDbName, Db.DatabaseName);
						var sqlScript = string.Format(@"BACKUP DATABASE [{0}] TO DISK = N'{1}' WITH INIT ", testDbName, testBackupFileFullPath);
						using (var cmd = conn.Command(sqlScript))
						{
							cmd.ExecuteNonQuery();
						}

						var backupChecker = new BackupCheckerForTesting();
						var backupInfo = backupChecker.GetBackupInfo_Exposed(conn, testBackupFileFullPath);

						AssertEquals("Backup file restored OK", testDbName, backupInfo.Rows[0]["DatabaseName"].ToString());

						var backupDate = (DateTime)backupInfo.Rows[0]["BackupFinishDate"];
						Assert("Backup is recent", dateTimeNow <= backupDate);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(Db.NewAdminConnection(), testDbName, Db.DatabaseName);
					if (Directory.Exists(testPath))
					{
						Directory.Delete(testPath, true);
					}
				}
			}
		}

		public void TestCannotOpenBackupDeviceErrorIsLoggedAndAttemptToCheckDotCurrentBackupIsPerformed()
		{
			// Arrange
			using (var temp = new TempDirectory())
			{
				const string databaseName = "Test_B74BA030_E10B_4CAF_BC2E_504B826B7AD2";
				var backupFileName = Path.Combine(temp.DirectoryName, $"{databaseName}.bak");

				try
				{
					using (var conn = Db.NewAdminConnection())
					{
						AdoTestUtils.CreateDbIfNotExists(conn, databaseName, Db.DatabaseName);
						conn.ExecuteNonQuery($"BACKUP DATABASE {databaseName} TO DISK = '{backupFileName}' WITH INIT");
						conn.ExecuteNonQuery($"BACKUP DATABASE {databaseName} TO DISK = '{backupFileName}.CURRENT' WITH INIT");

						var testChecker = new BackupCheckerForCannotOpenBackupDeviceTesting(databaseName, temp.DirectoryName);
						var testWarningList = new DbHealthWarningList
						{
							MainDbName = databaseName
						};

						// Act
						using (File.Open(backupFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
						{
							testChecker.Check(testWarningList, conn);
						}

						// Assert
						AssertEquals("Warings", 1, testWarningList.Count);
						AssertEquals("Source Type", DatabaseWarning.DatabaseSourceType, testWarningList[0].SourceType);
						AssertEquals("Source", databaseName, testWarningList[0].Source);
						AssertEquals("Description", $"Backup file for the database [{databaseName}] is used by another process. Checking .CURRENT backup instead. File location: {backupFileName}", testWarningList[0].Description);
						AssertEquals("Action", string.Empty, testWarningList[0].Action);

						testChecker.shouldGetBackupInfoThrowAnError = true;
						testWarningList = new DbHealthWarningList
						{
							MainDbName = databaseName
						};
						testChecker.Check(testWarningList, conn);
						AssertEquals("Number of warnings for corrupted backup files", 1, testWarningList.Count);
						AssertEquals("Source Type", DatabaseWarning.DatabaseSourceType, testWarningList[0].SourceType);
						AssertEquals("Source", databaseName, testWarningList[0].Source);
						AssertEquals("Description", $"{BrandingFactory.Instance.ProductName} failed to check backup file for database [{databaseName}], file location: '{backupFileName}'.\r\n\r\nERROR: Could not find stored procedure 'CommandToThrowAnError'.", testWarningList[0].Description);
						AssertEquals("Action", "Contact your system administrator.", testWarningList[0].Action);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(Db.NewAdminConnection(), databaseName, Db.DatabaseName);
				}
			}
		}

		public void TestIfBakDoesNotExistThenCurrentFileIsProcessed()
		{
			using (var temp = new TempDirectory())
			{
				string testDbName = "Test88AEDB88C429496297686B1AFB963E55";
				var testWarningList = new DbHealthWarningList
				{
					MainDbName = testDbName
				};

				testWarningList.MainDbName = testDbName;
				var backupFileName = Path.Combine(temp.DirectoryName, $"{testDbName}.bak.current");

				using (var connection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName, Db.DatabaseName);

					try
					{
						AdoTestUtils.CreateDbIfNotExists(connection, testDbName);
						var testChecker = new BackupCheckerTestForCheckingCurrentFileWhenBakFileIsMissing(temp.DirectoryName);
						testChecker.CheckBackupIsBeingPerformedDaily_Exposed(testWarningList);
						AssertEquals("Warings", 1, testWarningList.Count);
						AssertEquals($"There is NO backup for database [{testDbName}].", testWarningList[0].Description);

						connection.ExecuteNonQuery($"BACKUP DATABASE {testDbName} TO DISK = '{backupFileName}' WITH INIT");
						testChecker.listOfBackupFiles = new List<string>() { $"{testDbName}.bak.current" };
						testChecker.CheckBackupIsBeingPerformedDaily_Exposed(testWarningList);
						AssertEquals("Warings", 1, testWarningList.Count);
						AssertEquals($"There is NO backup for database [{testDbName}].", testWarningList[0].Description);
					}
					finally
					{
						AdoTestUtils.DropDbIfExists(connection, testDbName, Db.DatabaseName);
					}
				}
			}
		}

		public void TestCannotOpenBackupDeviceErrorAndCannotOpenBackupDotCurrentIsThrown()
		{
			// Arrange
			using (var temp = new TempDirectory())
			{
				const string databaseName = "Test_B74BA030_E10B_4CAF_BC2E_504B826B7AD2";
				var backupFileName = Path.Combine(temp.DirectoryName, $"{databaseName}.bak");

				try
				{
					using (var conn = Db.NewAdminConnection())
					{
						AdoTestUtils.CreateDbIfNotExists(conn, databaseName, Db.DatabaseName);
						conn.ExecuteNonQuery($"BACKUP DATABASE {databaseName} TO DISK = '{backupFileName}' WITH INIT");
						conn.ExecuteNonQuery($"BACKUP DATABASE {databaseName} TO DISK = '{backupFileName}.CURRENT' WITH INIT");

						var testChecker = new BackupCheckerForCannotOpenBackupDeviceTesting(databaseName, temp.DirectoryName);

						var testWarningList = new DbHealthWarningList
						{
							MainDbName = databaseName
						};

						// Act
						using (File.Open(backupFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
						using (File.Open(backupFileName + ".CURRENT", FileMode.Open, FileAccess.ReadWrite, FileShare.None))
						{
							// Assert
							AssertExceptionThrown(typeof(SqlException),
								$"Cannot open backup device '{backupFileName}.CURRENT'. Operating system error 32(The process cannot access the file because it is being used by another process.).\r\nRESTORE HEADERONLY is terminating abnormally.",
								() => testChecker.Check(testWarningList, conn),
								true);
						}
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(Db.NewAdminConnection(), databaseName, Db.DatabaseName);
				}
			}
		}

		public void TestCannotOpenBackupDeviceErrorAndCannotOpenBackupDotCurrentNoFileErrorIsLogged()
		{
			// Arrange
			using (var temp = new TempDirectory())
			{
				const string databaseName = "Test_B74BA030_E10B_4CAF_BC2E_504B826B7AD2";
				var backupFileName = Path.Combine(temp.DirectoryName, $"{databaseName}.bak");

				try
				{
					using (var conn = Db.NewAdminConnection())
					{
						AdoTestUtils.CreateDbIfNotExists(conn, databaseName, Db.DatabaseName);
						conn.ExecuteNonQuery($"BACKUP DATABASE {databaseName} TO DISK = '{backupFileName}' WITH INIT");

						var testChecker = new BackupCheckerForCannotOpenBackupDeviceTesting(databaseName, temp.DirectoryName);

						var testWarningList = new DbHealthWarningList
						{
							MainDbName = databaseName
						};

						// Act
						using (File.Open(backupFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
						{
							testChecker.Check(testWarningList, conn);

							// Assert
							AssertEquals($"Backup file for the database [{databaseName}] is used by another process. Checking .CURRENT backup instead. File location: {backupFileName}", testWarningList[0].Description);
							AssertEquals($"{BrandingFactory.Instance.ProductName} failed to check backup file for database [{databaseName}], file is not found: '{backupFileName}.CURRENT'.", testWarningList[1].Description);
						}
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(Db.NewAdminConnection(), databaseName, Db.DatabaseName);
				}
			}
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new BackupChecker();
		}

		class BackupCheckerForTesting : BackupChecker
		{
			public BackupCheckerForTesting()
				: base()
			{
			}

			protected override TimeSpan GetBackupThresholdConsideringWeekends(DateTime lastBackupTime)
			{
				return BackupHoursOldThresholdOverride;
			}

			internal bool overrideIsDbOlderThanTwoDays;
			protected override bool IsDbOlderThanTwoDays(DbConnection connection, string dbName)
			{
				if (overrideIsDbOlderThanTwoDays)
				{
					return overrideIsDbOlderThanTwoDays;
				}

				return base.IsDbOlderThanTwoDays(connection, dbName);
			}

			public void CheckBackupIsBeingPerformedPeriodically_Exposed(DbHealthWarningList warningList)
			{
				DoCheckBackupIsBeingPerformedPeriodically(new string[] { warningList.MainDbName }, Db.Connection, warningList);
			}

			internal DataTable GetBackupInfo_Exposed(DbConnection connection, string fullBackupPath)
			{
				return base.GetBackupInfo(connection, fullBackupPath);
			}

			public TimeSpan BackupHoursOldThresholdOverride = EnvProxy.IsHostedWithCargowise ? backupThresholdForHosting : backupThresholdForSelfHosting;
		}

		class BackupCheckerForNonBackupHistoryFound : BackupChecker
		{
			public bool SuccessGetFullBackup { get; set; }
			public bool SuccessGetCurrentBackup { get; set; }
			public bool ThrowExceptionWhenGetFullBackup { get; set; }

			protected override DataTable GetBackupInfo(DbConnection connection, string fullBackupPath)
			{
				DataTable result;

				if (fullBackupPath.EndsWith(".bak.CURRENT", StringComparison.OrdinalIgnoreCase))
				{
					result = GetDateTable(SuccessGetCurrentBackup);
				}
				else
				{
					if (ThrowExceptionWhenGetFullBackup)
					{
						throw NewSqlException(3201, @" '\Dummy.bak'. 32 RESTORE HEADERONLY");
					}

					result = GetDateTable(SuccessGetFullBackup);
				}

				return result;
			}

			DataTable GetDateTable(bool hasHistoryInfo)
			{
				var result = new DataTable();
				result.Locale = CultureInfo.InvariantCulture;

				if (hasHistoryInfo)
				{
					result.Columns.Add(new DataColumn("BackupFinishDate", typeof(DateTime)));
					var row = result.NewRow();
					row["BackupFinishDate"] = DateTime.Now;
					result.Rows.Add(row);
				}

				return result;
			}

			internal override List<string> GetListOfBackupFiles(DbConnection connection, string backupPath)
			{
				return new List<string>()
				{
					"Dummy.bak"
				};
			}

			public void DoCheckBackupIsBeingPerformedPeriodicallyForTest(IEnumerable<string> dbNames, DbConnection connection, DbHealthWarningList warningList) => DoCheckBackupIsBeingPerformedPeriodically(dbNames, connection, warningList);

			protected override bool SkipBackupOfNonMainDatabasesWithoutActivity => false;

			static SqlException NewSqlException(int number, string message)
			{
				var collection = Constructor<SqlErrorCollection>();
				var error = Constructor<SqlError>(number, (byte)2, (byte)3, "server name", message, "proc", 100);

				typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(collection, new object[] { error });
				return typeof(SqlException)
					.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
						null,
						CallingConventions.ExplicitThis,
						new[] { typeof(SqlErrorCollection), typeof(string) },
						Array.Empty<ParameterModifier>())
					?.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
			}

			static T Constructor<T>(params object[] p)
			{
				var constructor = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
				return (T)constructor.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
			}
		}

		class BackupCheckerForCannotOpenBackupDeviceTesting : BackupChecker
		{
			readonly List<string> listOfBackupFiles;
			readonly string backupDirectoryPath;

			//[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
			public BackupCheckerForCannotOpenBackupDeviceTesting(string databaseName, string backupDirectoryPath)
			{
				this.backupDirectoryPath = backupDirectoryPath;
				listOfBackupFiles = new List<string> { $"{databaseName}.bak" };
			}

			public void Check(DbHealthWarningList warningList, DbConnection connection)
			{
				DoCheckBackupIsBeingPerformedPeriodically(new[] { warningList.MainDbName }, connection, warningList);
			}

			protected override string BackupDirectoryPath => backupDirectoryPath;
			protected override bool SkipBackupOfNonMainDatabasesWithoutActivity => false;
			internal override List<string> GetListOfBackupFiles(DbConnection connection, string backupPath) => listOfBackupFiles;

			internal bool? shouldGetBackupInfoThrowAnError;
			protected override DataTable GetBackupInfo(DbConnection connection, string fullBackupPath)
			{
				if (shouldGetBackupInfoThrowAnError != null)
				{
					var sqlText = $@"CommandToThrowAnError";
					return Utilities.GetDataTableFromQuery(connection, sqlText);
				}

				return base.GetBackupInfo(connection, fullBackupPath);
			}
		}

		class BackupCheckerTestForCheckingCurrentFileWhenBakFileIsMissing : BackupChecker
		{
			internal List<string> listOfBackupFiles;
			readonly string backupDirectoryPath;

			//[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
			public BackupCheckerTestForCheckingCurrentFileWhenBakFileIsMissing(string backupDirectoryPath)
			{
				this.backupDirectoryPath = backupDirectoryPath;
				listOfBackupFiles = new List<string>();
			}

			protected override string BackupDirectoryPath => backupDirectoryPath;

			internal void CheckBackupIsBeingPerformedDaily_Exposed(DbHealthWarningList testWarningList)
			{
				using (var conn = Db.NewAdminConnection(testWarningList.MainDbName))
				{
					DoCheckBackupIsBeingPerformedPeriodically(new string[] { testWarningList.MainDbName }, conn, testWarningList);
				}
			}

			protected override bool IsDbOlderThanTwoDays(DbConnection connection, string dbName)
			{
				return true;
			}

			protected override bool SkipBackupOfNonMainDatabasesWithoutActivity => false;
			internal override List<string> GetListOfBackupFiles(DbConnection connection, string backupPath) => listOfBackupFiles;
		}
	}
}
