using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AppDomainWrappers.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.IO;
using Enterprise.DataTools.DbBackupAndRestore.Business.Common;
using Enterprise.DataTools.DbBackupAndRestore.Testing.Restore;
using Enterprise.DbBackup.Engine;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using log4net;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DbRestoreManagerTest : TestCase
	{
		public void TestGetLastDBBackupDetails()
		{
			using (var tempDir = new TempDirectory())
			{
				var testDbName = "TestTargetDbA6B3DC9D38634206B8500A9C9B2EEF1E";
				var testDBBackupFolder = Path.Combine(tempDir.DirectoryName, "Backup");
				CreateDirectory(testDBBackupFolder);
				var testDBBackupPath = Path.Combine(testDBBackupFolder, "TestTargetDbA6B3DC9D38634206B8500A9C9B2EEF1E.bak");
				var maintenanceManager = new DbMaintenanceManager();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);

						AdoTestUtils.CreateDbIfNotExists(connection, testDbName);

						var (testRecent, testResult) = DbRestoreManager.GetLastDBBackupDetails(Db.ServerName, testDbName);
						Assert(testResult.Contains(string.Format(CultureInfo.CurrentCulture, @"The test database: {0} does not have a backup history or backup file, if the copy to test process fails, test database will be lost, please click on 'No' and go to backup tab to make a backup first.", testDbName)));
						Assert("There should not be a recent backup", !testRecent);

						var backupDBScript = string.Format("BACKUP DATABASE [{0}] TO DISK = '{1}' WITH INIT, COMPRESSION", testDbName, testDBBackupPath);
						using (var cmd = connection.Command(backupDBScript))
						{
							cmd.ExecuteNonQuery();
						}
						(testRecent, testResult) = DbRestoreManager.GetLastDBBackupDetails(Db.ServerName, testDbName);

						var backupFinishTime = "";
						var getBackupDateScript = string.Format(CultureInfo.CurrentCulture, @"SELECT 
				b.backup_finish_date as BKFinishTime
				FROM msdb.dbo.backupset b with(nolock)
				WHERE b.database_name = '{0}'", testDbName);
						using (var cmd = connection.Command(getBackupDateScript))
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								backupFinishTime = reader["BKFinishTime"].ToString();
							}
						}
						AssertEquals("Result msg should be empty", string.Empty, testResult);
						Assert("There should be a recent backup", testRecent);

						var backuBackupExpire = string.Format("UPDATE msdb.dbo.backupset SET backup_finish_date = DATEADD(DAY, -1, backup_finish_date)  WHERE database_name = '{0}'", testDbName);
						using (var cmd = connection.Command(backuBackupExpire))
						{
							cmd.ExecuteNonQuery();
						}

						(testRecent, testResult) = DbRestoreManager.GetLastDBBackupDetails(Db.ServerName, testDbName);

						using (var cmd = connection.Command(getBackupDateScript))
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								backupFinishTime = reader["BKFinishTime"].ToString();
							}
						}

						Assert(testResult.Contains(string.Format(CultureInfo.CurrentCulture, @"The test database: {0} has a backup in {1} created at {2}, please consider if the backup is recent enough, if the copy to test process fails, test database will be lost and you will need to restore from the backup. Do you want to continue?", testDbName, testDBBackupPath, backupFinishTime)));
						Assert("There should not be a recent backup", !testRecent);

						File.Delete(testDBBackupPath);
						(testRecent, testResult) = DbRestoreManager.GetLastDBBackupDetails(Db.ServerName, testDbName);
						Assert(testResult.Contains(string.Format(CultureInfo.CurrentCulture, @"The test database: {0} does not have a backup history or backup file, if the copy to test process fails, test database will be lost, please click on 'No' and go to backup tab to make a backup first.", testDbName)));
						Assert("There should not be a recent backup", !testRecent);
					}
					finally
					{
						maintenanceManager.DropDatabases(Db.ServerName, testDbName, false, false, false);
					}
				}
			}
		}

		public void TestFileExists()
		{
			using (var tempFile = TempFile.NewWithExtension("bak"))
			{
				var result = DbRestoreManager.FileExists(Db.NewAdminConnection(), tempFile.Filename);
				AssertEquals("File exists", true, result);
			}
		}

		public void TestGetRelatedDatabaseBackupFileRegex()
		{
			var manager = new DbRestoreManagerForTesting();

			var regex = manager.GetRelatedDatabaseBackupFileRegex_Exposed("EdiBkp_T-20150730-103547_S-SYD-WLSG-1_D-CALP.bak", FileValidity.ValidStandard, @"_SD[0-9]{3}");
			AssertEquals(@"^EdiBkp_T-20150730-103547_S-SYD-WLSG-1_D-CALP(_SD[0-9]{3})\.bak$", regex.ToString());

			regex = manager.GetRelatedDatabaseBackupFileRegex_Exposed("EdiBkp_T-20150730-103547_S-SYD-WLSG-1_D-CALP.bak", FileValidity.NotValid, @"_SD[0-9]{3}");
			AssertEquals(@"^EdiBkp_T-20150730-103547_S-SYD-WLSG-1_D-CALP(_SD[0-9]{3})\.bak$", regex.ToString());

			regex = manager.GetRelatedDatabaseBackupFileRegex_Exposed("CW1Bkp_T-20150730-103547_S-SYD-WLSG-1_D-CALP.bak", FileValidity.ValidFBK, @"_SD[0-9]{3}");
			AssertEquals(@"^CW1Bkp_T-20150730-103547_S-SYD-WLSG-1_D-CALP(_SD[0-9]{3})\.bak$", regex.ToString());

			regex = manager.GetRelatedDatabaseBackupFileRegex_Exposed("CW1Bkp_T-20150730-103547_S-SYD-WLSG-1_D-CALP.dbk", FileValidity.ValidDBK, @"_SD[0-9]{3}");
			AssertEquals(@"^CW1Bkp_T-20150730-103547_S-SYD-WLSG-1_D-CALP(_SD[0-9]{3})\.dbk$", regex.ToString());
		}

		[UseSnapshotProtection]
		public void TestExtendedProperties()
		{
			var tempDir = @"somthing\oldtemp";
			var extendedDir = @"somthing\newtemp";
			var testLocalBackupFilePath = Path.Combine(tempDir, "testProdCopy.bak");

			var dbFiles = new DbFileInfoCollection
			{
				  new DbFileInfo("TestProdDbCopy", tempDir, DbFileInfo.FileTypeData, testLocalBackupFilePath)
				, new DbFileInfo("TestProdDbCopy_log", tempDir, DbFileInfo.FileTypeLog, testLocalBackupFilePath)
			};

			var manager = new DbRestoreManagerForTesting();

			manager.AddExtendedProperties("MainDbDataFile", extendedDir);

			bool hasExtendedPropForAllFiles;
			var outDbFiles = manager.ApplyExtendedProperties(dbFiles, out hasExtendedPropForAllFiles);

			Assert("Some of file's paths shouldn't be replaced", !hasExtendedPropForAllFiles);
			AssertEquals(extendedDir, outDbFiles[0].FolderPathView);
			AssertNotEquals(extendedDir, outDbFiles[1].FolderPathView);

			manager.AddExtendedProperties("MainDbLogFile", extendedDir);

			outDbFiles = manager.ApplyExtendedProperties(dbFiles, out hasExtendedPropForAllFiles);

			Assert("All file's paths should be replaced", hasExtendedPropForAllFiles);
			AssertEquals(extendedDir, outDbFiles[0].FolderPathView);
			AssertEquals(extendedDir, outDbFiles[1].FolderPathView);
		}

		[UseSnapshotProtection]
		public void TestRecordDbRestoreDetails()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			using (var tempDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "testProdCopy.bak");

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						Assert("Precondition: DB should exist", connection.DatabaseExists(testDbName));
						Assert("Precondition: DB should be available and online", Utilities.GetDatabaseStatus(connection, testDbName) == DatabaseStatus.Online);

						var value = GetStmData(connection, testDbName, "LastDatabaseRestore");
						Assert(value.Contains("<Operation>RestoreWithRecovery</Operation>"));

						value = GetStmData(connection, testDbName, "DataWarehouseServer");
						AssertEquals(null, value);
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRecordDbRestoreDetailsUpdateLastDateTimeToCorrectMainDb()
		{
			// Arrange
			var testDbName = "TestRestoreUserRepository";
			var testMainBackUp = "testProdCopy.bak";
			var testUserRepositoryBackUp = "TestProdCopy_UserRepository.bak";
			using (var tempDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testMainBackUp);
				var dbMainFiles = new DbFileInfoCollection();
				dbMainFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbMainFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));
				dbMainFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbMainFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
				dbMainFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var testLocalUserRepositoryBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testUserRepositoryBackUp);
				var dbUserRepositoryFiles = new DbFileInfoCollection();
				dbUserRepositoryFiles.Add(new DbFileInfo("TestProdDbCopy_UserRepository", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalUserRepositoryBackupFilePath));
				dbUserRepositoryFiles.Add(new DbFileInfo("TestProdDbCopy_UserRepository_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalUserRepositoryBackupFilePath));
				dbUserRepositoryFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbUserRepositoryFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var manager = new DbRestoreManagerForTesting();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbMainFiles, DbRestoreOption.RestoreWithRecovery);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						var initValue = GetStmData(connection, testDbName, "LastDatabaseRestore");

						// Act
						var userRepositoryConfig = new DbServerConfiguration(Db.ServerName, testLocalUserRepositoryBackupFilePath);
						var userRepositoryRestoreSettings = new DbRestoreSettings($"{testDbName}_UserRepository", dbUserRepositoryFiles, DbRestoreOption.RestoreWithRecovery);
						manager.RestoreDatabases(userRepositoryConfig, auditServerConfig, edwServerConfig, userRepositoryRestoreSettings);

						// Assert
						var changedValue = GetStmData(connection, testDbName, "LastDatabaseRestore");
						AssertEquals(initValue, changedValue);
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
						dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript($"{testDbName}_UserRepository");
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		[UseSnapshotProtection, ExpectNoExceptions]
		public void TestRestoreProductionToTest_NonMainDB()
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
				using (var tempDir = new TempDirectory())
				{
					try
					{
						var testBackUpFileName = "testRestore.bak";
						var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

						var manager = new DbRestoreManagerForTesting(isTargetDbMainDb: false);
						var dbFiles = TestDataHelpers.GetAllFilesFromBackupWithTargetDirectory(manager, testLocalBackupFilePath, tempDir.DirectoryName);
						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);
						manager.AddPreserveTestValueColumnIfMissing(connection, testDbName);
						manager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRestoreProductionToTest_CdcDisabledIfAuditAndEdwUnavailable()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			var testDatabase = "testProdCopyCdc.bak";

			using (var tempDir = new TempDirectory())
			using (var newFolderPathDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testDatabase);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);

				using (var logger = new TestLogger())
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeAuditDB, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEdwDB, false);

						manager.AddIsCancelledColumnIfMissing(connection, testDbName);
						manager.AddPasswordHashColumnsIfMissing(connection, testDbName);
						manager.AddStorageDocsToDeleteTableIfMissing(connection, testDbName);
						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						CombineAssertions(() =>
						{
							AssertCdcNotEnabled(connection, testDbName, logger.GetOutputMessage());
							AssertResetCDCIsFlagged(connection, testDbName);
						});
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.Command(dropCommand).ExecuteNonQuery();
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRestoreProductionToTest_CdcNotDisabledIfAuditOrEdwAvailable()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			var testDatabase = "testProdCopyCdc.bak";
			var testAuditDatabase = "testProdCopyCdc_Audit.bak";
			var testEdwDatabase = "testProdCopyCdc_EDW.bak";

			using (var tempDir = new TempDirectory())
			using (var newFolderPathDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testDatabase);
				var testAuditLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testAuditDatabase);
				var testEdwLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testEdwDatabase);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("testProdDBCopy_Audit", tempDir.DirectoryName, DbFileInfo.FileTypeData, testAuditLocalBackupFilePath, dbType: DbFileInfo.DbTypeAuditDB, restoreDbName: "_Audit"));
				dbFiles.Add(new DbFileInfo("testProdDBCopy_Audit_Log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testAuditLocalBackupFilePath, dbType: DbFileInfo.DbTypeAuditDB, restoreDbName: "_Audit"));
				dbFiles.Add(new DbFileInfo("testProdDBCopy_EDW", tempDir.DirectoryName, DbFileInfo.FileTypeData, testEdwLocalBackupFilePath, dbType: DbFileInfo.DbTypeEdwDB, restoreDbName: "_EDW"));
				dbFiles.Add(new DbFileInfo("testProdDBCopy_EDW_Log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testEdwLocalBackupFilePath, dbType: DbFileInfo.DbTypeEdwDB, restoreDbName: "_EDW"));

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);

				var manager = new DbRestoreManagerForTesting();
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeAuditDB, true);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEdwDB, true);

						manager.AddPasswordHashColumnsIfMissing(connection, testDbName);
						manager.AddIsCancelledColumnIfMissing(connection, testDbName);
						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						AssertCdcEnabled(connection, testDbName);
						AssertNotNullOrEmpty("BiAuditServer", Utilities.GetAuditServer(connection, testDbName));
						AssertNotNullOrEmpty("BiDataWarehouseServer", Utilities.GetDataWarehouseServer(connection, testDbName));
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.Command(dropCommand).ExecuteNonQuery();
						dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName + "_Audit");
						connection.Command(dropCommand).ExecuteNonQuery();
						dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName + "_EDW");
						connection.Command(dropCommand).ExecuteNonQuery();
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule", Justification = "Database names")]
		[UseSnapshotProtection]
		public void TestCopyProductionToTest()
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
				using (var tempDir = new TempDirectory())
				{
					var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "testProdCopy.bak");
					var dbFiles = new DbFileInfoCollection
					{
						  new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath)
						, new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath)
					};
					var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);

					var manager = new DbRestoreManagerForTesting();
					manager.AddPasswordHashColumnsIfMissing(connection, testDbName);
					manager.AddStorageDocsToDeleteTableIfMissing(connection, testDbName);
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						manager.DoRestore_Exposed(connection, mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						//
						// Assert values in restored database are the same as in backupfiles
						//
						// Data Transfer Registry Items
						var value = GetStmData(connection, testDbName, "XmlSchemaValidationStrict");
						AssertEquals("XML_Schema_Validation_Strict", value);
						value = GetStmData(connection, testDbName, "ConsolsDataImportDirectory");
						AssertEquals("consols_directory", value);
						// Database Registry Items
						value = GetStmData(connection, testDbName, "BackupFilePath");
						AssertEquals("Backup_File_Path", value);
						// Email Registry Items
						value = GetStmData(connection, testDbName, "MailboxUserName");
						AssertEquals("ProdPop3UserName", value);
						value = GetStmData(connection, testDbName, "MailboxDisplayName");
						AssertEquals("ProdPop3DisplayName", value);
						// System Registration Key Registry Item
						var sysRegKey = LicenceBuilder.GetSystemRegistrationKey(connection, testDbName);
						AssertSystemRegistrationKey(sysRegKey, new Guid("6C082A31-3463-4E9F-B0C8-066FD834574D"), "", "Odyssey", DatabaseTypes.Codes.Test, DatabaseSecurityModePairList.Codes.Locked, "SYD", new DateTime(2007, 8, 22));
						// Company Licence Registry Items
						value = GetStmData(connection, testDbName, FreightNotesLengthNew, new Guid("D438C595-B8D2-4B56-B3A4-3B3CD37D9418"), Guid.Empty);
						AssertEquals(CompanyLicenceFromBackup01, value);
						value = GetStmData(connection, testDbName, FreightNotesLengthNew, new Guid("E7AC2090-BB53-43F9-BFA5-687A7BAE4467"), Guid.Empty);
						AssertEquals(CompanyLicenceFromBackup02, value);
						// Physical Server ID
						value = GetStmData(connection, testDbName, PhysicalServerID);
						AssertEquals(PhysicalServerID, value);
						// NZCustoms Registry Items
						AssertStmDataValue(connection, testDbName, "NZCustomsCertificateCopyToEDocs", Guid.Empty, Guid.Empty, "customs certificate");
						AssertStmDataValue(connection, testDbName, "NZCustomsCertificateCopyToEDocs", new Guid("56A25E2C-DC5B-4516-A508-D4A022CA9A03"), Guid.Empty, "customs certificate with owner");
						AssertStmDataValue(connection, testDbName, "NZDeliveryOrderCopies", new Guid("63752C0E-D53B-4753-84F5-424577382E31"), new Guid("2A5075D4-9A98-4F94-8BA4-16D785F2624F"), "delivery order with owner and department");
						AssertStmDataValue(connection, testDbName, "NZDeliveryOrderCopies", Guid.Empty, new Guid("14E7BE54-DBA9-4819-A8EA-84CBF40E6D1D"), "delivery order with department only");
						//StmServiceHost
						AssertStmServiceHost_HostName(connection, testDbName, "8335b1fa-8dd7-4274-b8f3-82fee6284d55", "iev-wpc-01.corporate.cargowise.com");
						AssertStmServiceHost_HostName(connection, testDbName, "6dec28e9-4616-47ce-868f-8a51c9c2c223", "iev-wiss-1.corporate.cargowise.com");
						//StmScheduleTask
						AssertStmScheduleTask_ScheduleDescription(connection, testDbName, "2a54e902-b1ec-472c-bca8-1050828380a9", "Test Service Provider 4");
						AssertStmScheduleTask_ScheduleDescription(connection, testDbName, "ee7d2b44-7357-44e2-9905-ccbdf75834a2", "Sales - Missing Client Rates");

						// Licence usage registry
						value = GetStmData(connection, testDbName, "WarehouseCountPackages");
						AssertEquals("2009-01-02 01:02:03.456", value);

						// Live contact details are recorded in EDICommunicationsMode table
						AssertEquals(2, GetTableRowCount(connection, testDbName, "EDICommunicationsMode"));

						var values = GetEK_DestinationFromEDICommunicationsMode(connection, testDbName);
						AssertEquals("maminezhad@yahoo.com", values[0]);
						AssertEquals("maminezhad@yahoo.com", values[1]);

						values = GetEK_LoginNameFromEDICommunicationsMode(connection, testDbName);
						AssertEquals("LoginName1", values[0]);
						AssertEquals("LoginName2", values[1]);

						values = GetEK_PasswordFromEDICommunicationsMode(connection, testDbName);
						AssertEquals("Password1", values[0]);
						AssertEquals("Password2", values[1]);

						// Update EDICommunicationsMode table in the test system 
						UpdateEDICommunicationsMode(connection, testDbName, new Guid("D3C62269-DA4E-4DE8-B1BA-477506FAEA57"), "updatedEmail@yahoo.com");

						// Add test system specific EDICommunicationsMode record
						InsertEDICommunicationsMode(connection, testDbName, Guid.NewGuid(), "ACR", "TRX", "EMA", "testSpecific@yahoo.com", "SFF", "testLoginName", "testPassword", Guid.NewGuid(), "oh", "app", "false");
						AssertEquals(3, GetTableRowCount(connection, testDbName, "EDICommunicationsMode"));

						// Add production specific EDICommunicationsMode record
						DeleteEDICommunicationsMode(connection, testDbName, new Guid("4EB583C9-6DC3-47BB-BC3D-117F44BA0AEA"));
						AssertEquals(2, GetTableRowCount(connection, testDbName, "EDICommunicationsMode"));

						//Insert Records into StmUsageData
						var sudPK1 = Guid.NewGuid();
						var sudPK2 = Guid.NewGuid();
						InsertStmUsageData(connection, testDbName, sudPK1, "GTM", "GBC", "1.2", 1, "5okY1UFlKYXdRbe05F9HUyoa8A/8G5S+yT8jgEPAQPRUgMqs/Hzx4IKCjCOQHwOBRVlNH9+alhvViumHD+ND33tH5RyXOnb2gkmce3zEV1VRC4axXC10yfVjhgRe/UcNbQ7GB5nRV/x1chAVOu2aFV7VTrYKjyK7iiZMfk0X811J3xHFd2Fq7InzwlULCT/uW7HBz4b2pYJE8twIDJFRjFf1Lx3xejlkU2ekqzdAqdQPUnXNjsl5O8df9Or/e5/kY3/ZfzLMxjHiVz63JNP80tNEMVCxPfRInhIHwNYGaH7VFiheT7lrUe7f2F9r8bFjrAylP9OwvuaLVe4owhb8bnEtdQXvQyhxXP9oxe0AB19RyFj35IEWDhSZq9UErMj7dyO/u5emmxFbZcZy9xryRfBHS6m4w6Bu8TLPR4bpdfpOm+nRNpFSVbpOwdbIbhgXo/ahOJAGpfodQn+DPdRsPQIFWOuGgJD7NyTcIvUgbRJ4WsYZvUb+SRP+MmvWXk6iXvG88Ibbls5gezXs9t+XHWuzEjZT403DdIVeSJVvQtAsH1t5VnGC1ImaQFF2Wg71yVC27n8cR+2Aq2Cdl1nMj4OSHrnWnQjMIjdqB0aJPHE=");
						InsertStmUsageData(connection, testDbName, sudPK2, "LTM", "STL", "1.2", 0, "2gvkVTXoSSPryVr2aIm4xpa15kyKOc+btAphDrtGJYrFs3N5jZx3hqFRcyANLG20wAggeJ0YztdixB0fcoT9SCgGkRbe5/NFlcTEz/7d9qPRlUSviE8bFfB/tGDN5d97lB8MMm1A5Y3s904H0K4VjoAoAcoCqkx26gKOUmT5fH31jJdDfeUT6JRKuenYTx9a78dKRQkTMfQ99+Ze8vS+3v4xsJTDXGBrrwNzd8ULU4t8KS7i643hfJe/IFQ0IFFlEgpJY+IMXbqjlf8kQI+NueE2Hwsp8HKx2pwFcKoshnpiU8Knru9ewtbnMhdmAAEwAyyvOvblDdBxJUzO+WH/m8Qju3TaJ3bnckVJ7FpZnWIUZy7dbS4jkXvUeVtgywQy2ia71wJ7Af60DSpeNW51iBYhXK6EcYcjid1OGmsKAlFx3/YlQfQNY146FCJMNoMfSVM2wwwXJBSozdhlHV0o2iZc9LWn82QcTOMUKXHYL2doz1QExy4Y4pZh2R/euWahM3aUxEiHcBaoGHnPgb7iipGZ6eiGyQYLAgQdFJbAfQ5BJfzwQj0Cwz5TIgd7RzBhnkkIA6zJ7mVTIeSz5aKAv8LD/BNY87H3kNd+1KLUE9s=");

						//Insert New Values and SD_Name (to Registry) before copying production to test
						InsertStmData(connection, testDbName, "ACDataImportDirectory", "TestACDataImportDirectory", true);
						InsertStmData(connection, testDbName, "ARAPBalancesUpdateImportDirectoryItem", "TestARAPBalancesUpdateImportDirectoryItem", true);
						InsertStmData(connection, testDbName, "DataLoadingModuleOutputDirectory", "TestDataLoadingModuleOutputDirectory", true);
						InsertStmData(connection, testDbName, "ProductsXMLDataImportDirectory", "TestProductsXMLDataImportDirectory", true);

						InsertStmData(connection, testDbName, "BIRDImportBackupDirectory", "TestBIRDImportBackupDirectory", true);
						InsertStmData(connection, testDbName, "BIRDImportDirectory", "TestBIRDImportDirectory", true);
						InsertStmData(connection, testDbName, "ImporterSecurityFilingDataImportDirectory", "TestImporterSecurityFilingDataImportDirectory", true);
						InsertStmData(connection, testDbName, "WarehouseIFSDataImportDirectory", "TestWarehouseIFSDataImportDirectory", true);
						InsertStmData(connection, testDbName, "AUCCompanyCertificateData", "TestAUCCompanyCertificateData", true);

						InsertStmData(connection, testDbName, "SystemLogBatchProcessHighWaterMark", "TestSystemLogBatchProcessHighWaterMark");
						InsertStmData(connection, testDbName, "WorkflowExceptionGenerationHWM", "TestWorkflowExceptionGenerationHWM");
						InsertStmData(connection, testDbName, "WorkflowFieldChangeTriggerHWM", "TestWorkflowFieldChangeTriggerHWM");

						InsertStmData(connection, testDbName, "HASINTERFACECONNECTOR", "TestHASINTERFACECONNECTOR", true);
						InsertStmData(connection, testDbName, "PURGESETTINGS", "TestPURGESETTINGS", true);
						InsertStmData(connection, testDbName, "HASNATIVEXMLCONNECTOR", "TestHASNATIVEXMLCONNECTOR", true);
						InsertStmData(connection, testDbName, "AUSUPPRESSRESENDS", "TestAUSUPPRESSRESENDS", true);
						InsertStmData(connection, testDbName, "AUSUPPRESSCONTRLACKNOWLEDGEMENTS", "TestAUSUPPRESSCONTRLACKNOWLEDGEMENTS", true);
						InsertStmData(connection, testDbName, "SENDCAVIAEHUB", "TestSENDCAVIAEHUB", true);
						InsertStmData(connection, testDbName, "SENDHKISACEVIAEHUB", "TestSENDHKISACEVIAEHUB", true);
						InsertStmData(connection, testDbName, "SENDNZCUSCARVIAEHUB", "TestSENDNZCUSCARVIAEHUB", true);
						InsertStmData(connection, testDbName, "SENDNZCUSDECVIAEHUB", "TestSENDNZCUSDECVIAEHUB", true);
						InsertStmData(connection, testDbName, "SENDCBPVIAEHUB", "TestSENDCBPVIAEHUB", true);
						InsertStmData(connection, testDbName, "EADAPTERINBOUNDAUTHENTICATIONS", "TestEADAPTERINBOUNDAUTHENTICATIONS", true);
						InsertStmData(connection, testDbName, "EADAPTEROUTBOUNDPASSWORD", "TestEADAPTEROUTBOUNDPASSWORD", true);
						InsertStmData(connection, testDbName, "INBOUNDADAPTERSERVICEURL", "TestINBOUNDADAPTERSERVICEURL", true);
						InsertStmData(connection, testDbName, "OUTBOUNDADAPTERSERVICEURL", "TestOUTBOUNDADAPTERSERVICEURL", true);
						InsertStmData(connection, testDbName, "CANSENDCARGOIMPMESSAGESTHROUGHEADAPTER", "TestCANSENDCARGOIMPMESSAGESTHROUGHEADAPTER", true);
						InsertStmData(connection, testDbName, "EHUBGATEWAYSERVERADDRESS", "TestEHUBGATEWAYSERVERADDRESS", true);
						InsertStmData(connection, testDbName, "EHUBERRORSNOTIFICATIONGROUP", "TestEHUBERRORSNOTIFICATIONGROUP", true);
						InsertStmData(connection, testDbName, "SCAVENGINGTASKSETTINGS", "TestSCAVENGINGTASKSETTINGS", true);
						InsertStmData(connection, testDbName, "XMLSERVICEVERBOSELOGGING", "TestXMLSERVICEVERBOSELOGGING", true);
						InsertStmData(connection, testDbName, "AIRLINEFHLMESSAGESRECIPIENTCONFIGURATION", "TestAIRLINEFHLMESSAGESRECIPIENTCONFIGURATION", true);
						InsertStmData(connection, testDbName, "AIRLINEFWBMESSAGESRECIPIENTCONFIGURATION", "TestAIRLINEFWBMESSAGESRECIPIENTCONFIGURATION", true);
						InsertStmData(connection, testDbName, "UNIVERSALXMLALWAYSINCLUDEJOBCOSTINGINUNIVERSALSHIPMENT", "TestUNIVERSALXMLALWAYSINCLUDEJOBCOSTINGINUNIVERSALSHIPMENT", true);
						InsertStmData(connection, testDbName, "DATE2012_11NAMESPACEANDFORMATKICKSIN", "TestDATE2012_11NAMESPACEANDFORMATKICKSIN", true);
						InsertStmData(connection, testDbName, "USEDEFAULTINGOFDATAWHENIMPORTINGUNIVERSALXML", "TestUSEDEFAULTINGOFDATAWHENIMPORTINGUNIVERSALXML", true);
						InsertStmData(connection, testDbName, "UNIVERSALXMLENABLEVERBOSELOGGING", "TestUNIVERSALXMLENABLEVERBOSELOGGING", true);
						InsertStmData(connection, testDbName, "USEBROKERAGEDATAFIRSTWHENEXPORTUNIVERSALXML", "TestUSEBROKERAGEDATAFIRSTWHENEXPORTUNIVERSALXML", true);
						InsertStmData(connection, testDbName, "UNIVERSALXMLUSECOMBINEDREFERENCEANDPARTYIDMATCH", "TestUNIVERSALXMLUSECOMBINEDREFERENCEANDPARTYIDMATCH", true);
						InsertStmData(connection, testDbName, "UNIVERSALXMLUPDATECONSOLDURINGAUTOMATICIMPORT", "TestUNIVERSALXMLUPDATECONSOLDURINGAUTOMATICIMPORT", true);
						InsertStmData(connection, testDbName, "UNIVERSALXMLUPDATECONSOLCONTAINERSDURINGAUTOMATICIMPORT", "TestUNIVERSALXMLUPDATECONSOLCONTAINERSDURINGAUTOMATICIMPORT", true);
						InsertStmData(connection, testDbName, "UNIVERSALXMLUPDATECONSOLROUTINGDURINGAUTOMATICIMPORT", "TestUNIVERSALXMLUPDATECONSOLROUTINGDURINGAUTOMATICIMPORT", true);
						InsertStmData(connection, testDbName, "UNIVERSALXMLUPDATECONSOLSHIPMENSTDURINGAUTOMATICIMPORT", "TestUNIVERSALXMLUPDATECONSOLSHIPMENSTDURINGAUTOMATICIMPORT", true);
						InsertStmData(connection, testDbName, "UNIVERSALXMLUPDATESHIPMENTDURINGAUTOMATICIMPORT", "TestUNIVERSALXMLUPDATESHIPMENTDURINGAUTOMATICIMPORT", true);
						InsertStmData(connection, testDbName, "INBOUNDMESSAGEDISCARDEDNOTIFICATIONGROUP", "TestINBOUNDMESSAGEDISCARDEDNOTIFICATIONGROUP", true);
						InsertStmData(connection, testDbName, "INBOUNDMESSAGEREJECTEDNOTIFICATIONGROUP", "TestINBOUNDMESSAGEREJECTEDNOTIFICATIONGROUP", true);
						InsertStmData(connection, testDbName, "INBOUNDMESSAGEPROCESSEDOKNOTIFICATIONCONFIGURATION", "TestINBOUNDMESSAGEPROCESSEDOKNOTIFICATIONCONFIGURATION", true);
						InsertStmData(connection, testDbName, "INBOUNDMESSAGEPROCESSEDWITHERRORSNOTIFICATIONCONFIGURATION", "TestINBOUNDMESSAGEPROCESSEDWITHERRORSNOTIFICATIONCONFIGURATION", true);
						InsertStmData(connection, testDbName, "INBOUNDMESSAGEPROCESSEDWITHWARNINGSNOTIFICATIONCONFIGURATION", "TestINBOUNDMESSAGEPROCESSEDWITHWARNINGSNOTIFICATIONCONFIGURATION", true);

						InsertStmData(connection, testDbName, "GLOWENTERPRISESERVICESROOTURI", "TestGLOWENTERPRISESERVICESROOTURI", true);

						InsertStmData(connection, testDbName, "ReportingDBServerName", "TestReportingDBServerName", true);

						InsertStmData(connection, testDbName, "DOTNET_VERSION|PC01", "A Val 1", false);
						InsertStmData(connection, testDbName, "DOTNET_VERSION|PC02", "A Val 2", false);
						InsertStmData(connection, testDbName, "DOTNET_VERSION|SRV68", "A Val 68", false);
						InsertStmData(connection, testDbName, "DOTNET_VERSION|SRV79", "A Val 79", false);

						//Insert New Values and SD_Name (to servicetask general Configuration) before copying production to test
						InsertStmData(connection, testDbName, "ServiceManagerTaskABC", "TestServiceManagerTaskABC", true);
						InsertStmData(connection, testDbName, "ServiceManagerTaskABI", "TestServiceManagerTaskABI", true);
						InsertStmData(connection, testDbName, "ServiceManagerTaskACC", "TestServiceManagerTaskACC", true);

						//Insert New Values and SD_Name (To satisfy the CheckMinUpgradeableDbMajorSchemaVersion method) before copying production to test
						InsertStmData(connection, testDbName, "DATABASE_SCHEMA_VERSION", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), true);

						//Insert New Values and SD_Name (to Always On Replica Cache) before copying production to test
						InsertStmData(connection, testDbName, "UseAlwaysOnReplicaCache", "true", false);
						InsertStmData(connection, testDbName, "AlwaysOnReplicaCachedInfos", "test value", true);

						// Change values before copying production to test
						//

						// Data Transfer Registry Items
						UpdateStmData(connection, testDbName, "XmlSchemaValidationStrict", "changed schema validation strict");
						UpdateStmData(connection, testDbName, "ConsolsDataImportDirectory", "changed consols directory");
						// Database Registry Items
						UpdateStmData(connection, testDbName, "BackupFilePath", "changed backup file path");
						// Email Registry Items
						UpdateStmData(connection, testDbName, "MailboxUserName", "ChangedProdPop3UserName");
						UpdateStmData(connection, testDbName, "MailboxDisplayName", "changed prod pop3 name");
						// Licence (System Registration Key) Registry Item
						UpdateStmData(connection, testDbName, FreightNotesHeaderLength, ChangedSysRegKey);
						// Physical Server ID
						UpdateStmData(connection, testDbName, PhysicalServerID, "changed server id");
						// NZCustoms Registry Items
						UpdateStmDataByPk(connection, testDbName, "ECCC3183-0137-4F6F-AD2A-5C9C1B8FBCEE", "changed customs certificate");
						UpdateStmDataByPk(connection, testDbName, "B4DC090A-2DEF-44B3-8A3D-68F7288669B7", "changed customs certificate with owner");
						UpdateStmDataByPk(connection, testDbName, "DACB773B-99C2-485F-BB45-3761C482137A", "changed delivery order with owner and department");
						UpdateStmDataByPk(connection, testDbName, "0DE59922-DDB7-4B95-BF6D-F04CE97D68D6", "changed delivery order with department only");
						//StmServiceHost
						UpdateStmServiceHost_HostName(connection, testDbName, "8335b1fa-8dd7-4274-b8f3-82fee6284d55", "TestHost");
						UpdateStmServiceHost_HostName(connection, testDbName, "6dec28e9-4616-47ce-868f-8a51c9c2c223", "iev-wiss-1.test.host");
						InsertStmServiceHost(connection, testDbName, "C2B7464A-3E1A-4223-9680-C85F473EB57A", "new.test.host");
						//StmScheduleTask
						UpdateStmScheduleTask_ScheduleDescription(connection, testDbName, "2a54e902-b1ec-472c-bca8-1050828380a9", "Test");
						UpdateStmScheduleTask_ScheduleDescription(connection, testDbName, "ee7d2b44-7357-44e2-9905-ccbdf75834a2", "Sales");
						CreateNewBranchAndUpdateStmScheduleTaskBranchFk(connection, testDbName, "5C09CDF2-C37A-452F-AEC9-F4E0F288D7F5");

						var initialNumberFountains = LoadAllNumberFountainsOrderByNameOwner(connection, testDbName);
						AssertEquals("Pre: number fountain count in test backup file", 2, initialNumberFountains.Count);
						AssertEquals("test number fountain value", 1001, initialNumberFountains[0].SN_Value);
						AssertEquals("test number fountain value", 1094, initialNumberFountains[1].SN_Value);

						BumpNumberFountains(connection, testDbName);

						var bumpedNumberFountains = LoadAllNumberFountainsOrderByNameOwner(connection, testDbName);
						AssertEquals("number fountain count ", 2, bumpedNumberFountains.Count);
						AssertEquals("test number fountain value", 1002, bumpedNumberFountains[0].SN_Value);
						AssertEquals("test number fountain value", 1095, bumpedNumberFountains[1].SN_Value);

						PopulateTranslationFeedbackData(connection, testDbName);
						AssertTranslationFeedbackData(connection, testDbName);

						// Assert original StmPrintJob and StmPrintJobCopyRecipient
						AssertEquals(2, GetStmPrintJobCount(connection, testDbName));
						AssertEquals(1, GetStmPrintJobCopyRecipientCount(connection, testDbName));

						// Assert original Active Directory related data
						AssertEquals(2, GetGlbStaffCountWithNonEmptyActiveDirectoryDetails(connection, testDbName));
						AssertEquals(2, GetGlbGroupCountWithNonEmptyActiveDirectoryDetails(connection, testDbName));

						//Insert StorageDocsToDelete test data

						var storageDocsToDeletePK = Guid.NewGuid();
						var storageDocsToDeleteDocumentPK = Guid.NewGuid();

						InsertStorageDocsToDeleteData(connection, testDbName, storageDocsToDeletePK, storageDocsToDeleteDocumentPK);

						//
						// Copy production to test
						//
						manager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						//
						// Assert test DB values after copy
						//

						// Assert queued MailDBItems purged
						var status = GetMailDbStatus(connection, testDbName);
						AssertNotEquals("QUE", status);

						// Assert StmPrintJob and StmPrintJobCopyRecipient are purged
						AssertEquals(0, GetStmPrintJobCount(connection, testDbName));
						AssertEquals(0, GetStmPrintJobCopyRecipientCount(connection, testDbName));

						// Assert registry items preserved

						// Data Transfer Registry Items
						value = GetStmData(connection, testDbName, "XmlSchemaValidationStrict");
						AssertEquals("changed schema validation strict", value);
						value = GetStmData(connection, testDbName, "ConsolsDataImportDirectory");
						AssertEquals("changed consols directory", value);

						value = GetStmData(connection, testDbName, "SystemLogBatchProcessHighWaterMark");
						AssertEquals(null, value);

						value = GetStmData(connection, testDbName, "WorkflowExceptionGenerationHWM");
						AssertEquals(null, value);

						value = GetStmData(connection, testDbName, "WorkflowFieldChangeTriggerHWM");
						AssertEquals(null, value);

						//New Items Added

						value = GetStmData(connection, testDbName, "ACDataImportDirectory");
						AssertEquals("TestACDataImportDirectory", value);

						value = GetStmData(connection, testDbName, "ARAPBalancesUpdateImportDirectoryItem");
						AssertEquals("TestARAPBalancesUpdateImportDirectoryItem", value);

						value = GetStmData(connection, testDbName, "DataLoadingModuleOutputDirectory");
						AssertEquals("TestDataLoadingModuleOutputDirectory", value);

						value = GetStmData(connection, testDbName, "ProductsXMLDataImportDirectory");
						AssertEquals("TestProductsXMLDataImportDirectory", value);

						value = GetStmData(connection, testDbName, "BIRDImportBackupDirectory");
						AssertEquals("TestBIRDImportBackupDirectory", value);

						value = GetStmData(connection, testDbName, "BIRDImportDirectory");
						AssertEquals("TestBIRDImportDirectory", value);

						value = GetStmData(connection, testDbName, "ImporterSecurityFilingDataImportDirectory");
						AssertEquals("TestImporterSecurityFilingDataImportDirectory", value);

						value = GetStmData(connection, testDbName, "WarehouseIFSDataImportDirectory");
						AssertEquals("TestWarehouseIFSDataImportDirectory", value);

						value = GetStmData(connection, testDbName, "AUCCompanyCertificateData");
						AssertEquals("TestAUCCompanyCertificateData", value);

						value = GetStmData(connection, testDbName, "ServiceManagerTaskABC");
						AssertEquals("TestServiceManagerTaskABC", value);

						value = GetStmData(connection, testDbName, "ServiceManagerTaskABI");
						AssertEquals("TestServiceManagerTaskABI", value);

						value = GetStmData(connection, testDbName, "ServiceManagerTaskACC");
						AssertEquals("TestServiceManagerTaskACC", value);

						value = GetStmData(connection, testDbName, "ReportingDBServerName");
						AssertEquals("TestReportingDBServerName", value);

						value = GetStmData(connection, testDbName, "HASINTERFACECONNECTOR");
						AssertEquals("TestHASINTERFACECONNECTOR", value);

						value = GetStmData(connection, testDbName, "PURGESETTINGS");
						AssertEquals("TestPURGESETTINGS", value);

						value = GetStmData(connection, testDbName, "HASNATIVEXMLCONNECTOR");
						AssertEquals("TestHASNATIVEXMLCONNECTOR", value);

						value = GetStmData(connection, testDbName, "AUSUPPRESSRESENDS");
						AssertEquals("TestAUSUPPRESSRESENDS", value);

						value = GetStmData(connection, testDbName, "AUSUPPRESSCONTRLACKNOWLEDGEMENTS");
						AssertEquals("TestAUSUPPRESSCONTRLACKNOWLEDGEMENTS", value);

						value = GetStmData(connection, testDbName, "SENDCAVIAEHUB");
						AssertEquals("TestSENDCAVIAEHUB", value);

						value = GetStmData(connection, testDbName, "SENDHKISACEVIAEHUB");
						AssertEquals("TestSENDHKISACEVIAEHUB", value);

						value = GetStmData(connection, testDbName, "SENDNZCUSCARVIAEHUB");
						AssertEquals("TestSENDNZCUSCARVIAEHUB", value);

						value = GetStmData(connection, testDbName, "SENDNZCUSDECVIAEHUB");
						AssertEquals("TestSENDNZCUSDECVIAEHUB", value);

						value = GetStmData(connection, testDbName, "SENDCBPVIAEHUB");
						AssertEquals("TestSENDCBPVIAEHUB", value);

						value = GetStmData(connection, testDbName, "EADAPTERINBOUNDAUTHENTICATIONS");
						AssertEquals("TestEADAPTERINBOUNDAUTHENTICATIONS", value);

						value = GetStmData(connection, testDbName, "EADAPTEROUTBOUNDPASSWORD");
						AssertEquals("TestEADAPTEROUTBOUNDPASSWORD", value);

						value = GetStmData(connection, testDbName, "INBOUNDADAPTERSERVICEURL");
						AssertEquals("TestINBOUNDADAPTERSERVICEURL", value);

						value = GetStmData(connection, testDbName, "OUTBOUNDADAPTERSERVICEURL");
						AssertEquals("TestOUTBOUNDADAPTERSERVICEURL", value);

						value = GetStmData(connection, testDbName, "CANSENDCARGOIMPMESSAGESTHROUGHEADAPTER");
						AssertEquals("TestCANSENDCARGOIMPMESSAGESTHROUGHEADAPTER", value);

						value = GetStmData(connection, testDbName, "EHUBGATEWAYSERVERADDRESS");
						AssertEquals("TestEHUBGATEWAYSERVERADDRESS", value);

						value = GetStmData(connection, testDbName, "EHUBERRORSNOTIFICATIONGROUP");
						AssertEquals("TestEHUBERRORSNOTIFICATIONGROUP", value);

						value = GetStmData(connection, testDbName, "SCAVENGINGTASKSETTINGS");
						AssertEquals("TestSCAVENGINGTASKSETTINGS", value);

						value = GetStmData(connection, testDbName, "XMLSERVICEVERBOSELOGGING");
						AssertEquals("TestXMLSERVICEVERBOSELOGGING", value);

						value = GetStmData(connection, testDbName, "AIRLINEFHLMESSAGESRECIPIENTCONFIGURATION");
						AssertEquals("TestAIRLINEFHLMESSAGESRECIPIENTCONFIGURATION", value);

						value = GetStmData(connection, testDbName, "AIRLINEFWBMESSAGESRECIPIENTCONFIGURATION");
						AssertEquals("TestAIRLINEFWBMESSAGESRECIPIENTCONFIGURATION", value);

						value = GetStmData(connection, testDbName, "UNIVERSALXMLALWAYSINCLUDEJOBCOSTINGINUNIVERSALSHIPMENT");
						AssertEquals("TestUNIVERSALXMLALWAYSINCLUDEJOBCOSTINGINUNIVERSALSHIPMENT", value);

						value = GetStmData(connection, testDbName, "DATE2012_11NAMESPACEANDFORMATKICKSIN");
						AssertEquals("TestDATE2012_11NAMESPACEANDFORMATKICKSIN", value);

						value = GetStmData(connection, testDbName, "USEDEFAULTINGOFDATAWHENIMPORTINGUNIVERSALXML");
						AssertEquals("TestUSEDEFAULTINGOFDATAWHENIMPORTINGUNIVERSALXML", value);

						value = GetStmData(connection, testDbName, "UNIVERSALXMLENABLEVERBOSELOGGING");
						AssertEquals("TestUNIVERSALXMLENABLEVERBOSELOGGING", value);

						value = GetStmData(connection, testDbName, "USEBROKERAGEDATAFIRSTWHENEXPORTUNIVERSALXML");
						AssertEquals("TestUSEBROKERAGEDATAFIRSTWHENEXPORTUNIVERSALXML", value);

						value = GetStmData(connection, testDbName, "UNIVERSALXMLUSECOMBINEDREFERENCEANDPARTYIDMATCH");
						AssertEquals("TestUNIVERSALXMLUSECOMBINEDREFERENCEANDPARTYIDMATCH", value);

						value = GetStmData(connection, testDbName, "UNIVERSALXMLUPDATECONSOLDURINGAUTOMATICIMPORT");
						AssertEquals("TestUNIVERSALXMLUPDATECONSOLDURINGAUTOMATICIMPORT", value);

						value = GetStmData(connection, testDbName, "UNIVERSALXMLUPDATECONSOLCONTAINERSDURINGAUTOMATICIMPORT");
						AssertEquals("TestUNIVERSALXMLUPDATECONSOLCONTAINERSDURINGAUTOMATICIMPORT", value);

						value = GetStmData(connection, testDbName, "UNIVERSALXMLUPDATECONSOLROUTINGDURINGAUTOMATICIMPORT");
						AssertEquals("TestUNIVERSALXMLUPDATECONSOLROUTINGDURINGAUTOMATICIMPORT", value);

						value = GetStmData(connection, testDbName, "UNIVERSALXMLUPDATECONSOLSHIPMENSTDURINGAUTOMATICIMPORT");
						AssertEquals("TestUNIVERSALXMLUPDATECONSOLSHIPMENSTDURINGAUTOMATICIMPORT", value);

						value = GetStmData(connection, testDbName, "UNIVERSALXMLUPDATESHIPMENTDURINGAUTOMATICIMPORT");
						AssertEquals("TestUNIVERSALXMLUPDATESHIPMENTDURINGAUTOMATICIMPORT", value);

						value = GetStmData(connection, testDbName, "INBOUNDMESSAGEDISCARDEDNOTIFICATIONGROUP");
						AssertEquals("TestINBOUNDMESSAGEDISCARDEDNOTIFICATIONGROUP", value);

						value = GetStmData(connection, testDbName, "INBOUNDMESSAGEREJECTEDNOTIFICATIONGROUP");
						AssertEquals("TestINBOUNDMESSAGEREJECTEDNOTIFICATIONGROUP", value);

						value = GetStmData(connection, testDbName, "INBOUNDMESSAGEPROCESSEDOKNOTIFICATIONCONFIGURATION");
						AssertEquals("TestINBOUNDMESSAGEPROCESSEDOKNOTIFICATIONCONFIGURATION", value);

						value = GetStmData(connection, testDbName, "INBOUNDMESSAGEPROCESSEDWITHERRORSNOTIFICATIONCONFIGURATION");
						AssertEquals("TestINBOUNDMESSAGEPROCESSEDWITHERRORSNOTIFICATIONCONFIGURATION", value);

						value = GetStmData(connection, testDbName, "INBOUNDMESSAGEPROCESSEDWITHWARNINGSNOTIFICATIONCONFIGURATION");
						AssertEquals("TestINBOUNDMESSAGEPROCESSEDWITHWARNINGSNOTIFICATIONCONFIGURATION", value);

						value = GetStmData(connection, testDbName, "GLOWENTERPRISESERVICESROOTURI");
						AssertEquals("TestGLOWENTERPRISESERVICESROOTURI", value);

						// Database Registry Items
						value = GetStmData(connection, testDbName, "BackupFilePath");
						AssertEquals("changed backup file path", value);
						// Email Registry Items
						value = GetStmData(connection, testDbName, "MailboxUserName");
						AssertEquals("ChangedProdPop3UserName", value);
						value = GetStmData(connection, testDbName, "MailboxDisplayName");
						AssertEquals("changed prod pop3 name", value);
						// Licence (System Registration Key) Registry Item
						value = GetStmData(connection, testDbName, FreightNotesHeaderLength);
						AssertEquals(ChangedSysRegKey, value);
						// Physical Server ID
						value = GetStmData(connection, testDbName, PhysicalServerID);
						AssertEquals("changed server id", value);
						// NZCustoms Registry Items
						AssertStmDataValue(connection, testDbName, "NZCustomsCertificateCopyToEDocs", Guid.Empty, Guid.Empty, "changed customs certificate");
						AssertStmDataValue(connection, testDbName, "NZCustomsCertificateCopyToEDocs", new Guid("56A25E2C-DC5B-4516-A508-D4A022CA9A03"), Guid.Empty, "changed customs certificate with owner");
						AssertStmDataValue(connection, testDbName, "NZDeliveryOrderCopies", new Guid("63752C0E-D53B-4753-84F5-424577382E31"), new Guid("2A5075D4-9A98-4F94-8BA4-16D785F2624F"), "changed delivery order with owner and department");
						AssertStmDataValue(connection, testDbName, "NZDeliveryOrderCopies", Guid.Empty, new Guid("14E7BE54-DBA9-4819-A8EA-84CBF40E6D1D"), "changed delivery order with department only");

						//Assert StmServiceHost preserved
						AssertStmServiceHost_HostName("StmServiceHost preserved", connection, testDbName, "8335b1fa-8dd7-4274-b8f3-82fee6284d55", "TestHost");
						AssertStmServiceHost_HostName("StmServiceHost preserved", connection, testDbName, "6dec28e9-4616-47ce-868f-8a51c9c2c223", "iev-wiss-1.test.host");
						AssertStmServiceHost_HostName("StmServiceHost preserved", connection, testDbName, "C2B7464A-3E1A-4223-9680-C85F473EB57A", "new.test.host");

						//Assert StmScheduleTask with StmServiceHost parent preserved
						AssertStmScheduleTask_ScheduleDescription("Test StmScheduleTask which is a service task preserved", connection, testDbName, "2a54e902-b1ec-472c-bca8-1050828380a9", "Test");
						AssertStmScheduleTask_ScheduleDescription("Test StmScheduleTask which is not a service task restored", connection, testDbName, "ee7d2b44-7357-44e2-9905-ccbdf75834a2", "Sales - Missing Client Rates");
						AssertStmScheduleTask_ScheduleDescription("Test StmScheduleTask with inexisting branch FK NOT preserved", connection, testDbName, "5C09CDF2-C37A-452F-AEC9-F4E0F288D7F5", null);

						//AssertStorageDocsToDeletePreserved
						AssertStorageDocsToPreserved("Test StorageDocsToDelete was not preserved", connection, testDbName, storageDocsToDeletePK.ToString(), storageDocsToDeleteDocumentPK.ToString());

						// Licence usage registry updated
						value = GetStmData(connection, testDbName, "WarehouseCountPackages");
						var minutesFromNow = (SqlFormatInfo.FromSqlDateTime(value) - DateTime.UtcNow).TotalMinutes;
						Assert("last report date set to roughly now", minutesFromNow < 50 && minutesFromNow > -15);

						// Always On Replica Cache
						AssertStmDataValue(connection, testDbName, "UseAlwaysOnReplicaCache", Guid.Empty, Guid.Empty, null);
						AssertStmDataValue(connection, testDbName, "AlwaysOnReplicaCachedInfos", Guid.Empty, Guid.Empty, "test value");

						// Use modern Sql security system
						AssertStmDataValue(connection, testDbName, "UseModernSqlSecuritySystem", Guid.Empty, Guid.Empty, null);

						// Company Licence Info modified to adjust to test system
						AssertCompanyLicencesAdjusted(connection, testDbName, "changed server id");

						// StmUsageData records are gone
						AssertEquals(0, GetTableRowCount(connection, testDbName, "StmUsageData"));

						// IncidentApproval are gone
						AssertEquals(0, GetTableRowCount(connection, testDbName, "IncidentApproval"));

						// Live contact details are cleared from EDICommunicationsMode table in production data but kept in test data
						AssertEquals(3, GetTableRowCount(connection, testDbName, "EDICommunicationsMode"));

						values = GetEK_DestinationFromEDICommunicationsMode(connection, testDbName);
						AssertEquals("updatedEmail@yahoo.com", values[0]);
						AssertEquals("", values[1]);
						AssertEquals("testSpecific@yahoo.com", values[2]);

						values = GetEK_LoginNameFromEDICommunicationsMode(connection, testDbName);
						AssertEquals("LoginName1", values[0]);
						AssertEquals("", values[1]);
						AssertEquals("testLoginName", values[2]);

						values = GetEK_PasswordFromEDICommunicationsMode(connection, testDbName);
						AssertEquals("Password1", values[0]);
						AssertEquals("", values[1]);
						AssertEquals("testPassword", values[2]);

						var finalNumberFountains = LoadAllNumberFountainsOrderByNameOwner(connection, testDbName);
						AssertEquals("finalNumberFountains.Count", 2, finalNumberFountains.Count);
						AssertEquals("other number fountain name", "ARInvoiceNo", finalNumberFountains[0].SN_Name);
						AssertEquals("other number fountain value restored", initialNumberFountains[0].SN_Value, finalNumberFountains[0].SN_Value);

						AssertEquals("preserved number fountain name", bumpedNumberFountains[1].SN_Name, finalNumberFountains[1].SN_Name);
						AssertEquals("preserved number fountain value", bumpedNumberFountains[1].SN_Value, finalNumberFountains[1].SN_Value);
						AssertEquals("preserved number fountain owner", bumpedNumberFountains[1].SN_Owner, finalNumberFountains[1].SN_Owner);

						AssertTranslationFeedbackData(connection, testDbName);

						AssertEquals("GlbStaff's GS_ActiveDirectoryObjectGUID and GS_DomainName should have been cleared", 0, GetGlbStaffCountWithNonEmptyActiveDirectoryDetails(connection, testDbName));
						AssertEquals("GlbGroup's GG_ActiveDirectoryObjectGUID and GG_DomainName should have been cleared", 0, GetGlbGroupCountWithNonEmptyActiveDirectoryDetails(connection, testDbName));

						AssertEquals("DataVault database is dropped", true, !Utilities.DatabaseExists(connection, testDbName + "-DataVault"));

						AssertNoDotNetVersionDetailsExist(testDbName);
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToTest_CancelInterchangesAndMessages()
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var testDbName = "TestTargetDb_F5E6220D-1416-4785-9F1E-D5B55C666DFA";

				using (var tempDir = new TempDirectory())
				{
					var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "testProdDbCopyWithInterchagneAndMessage.bak");
					var dbFiles = new DbFileInfoCollection
					{
						  new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath)
						, new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath)
					};
					var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);

					var manager = new DbRestoreManagerForTesting();
					manager.AddPasswordHashColumnsIfMissing(connection, testDbName);
					manager.AddStorageDocsToDeleteTableIfMissing(connection, testDbName);
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						manager.DoRestore_Exposed(connection, mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						Assert(DbObjectCreator.TableExists(connection, testDbName, "EDIInterchange"));
						Assert(DbObjectCreator.TableExists(connection, testDbName, "EDIMessage"));

						var messages = new (Guid PK, string Status)[]
						{
							(GetEDIMessagePK(connection, testDbName, "PND"), "CAN"),
							(GetEDIMessagePK(connection, testDbName, "QUE"), "CAN"),
							(GetEDIMessagePK(connection, testDbName, "HPN"), "CAN"),
							(GetEDIMessagePK(connection, testDbName, "HQU"), "CAN"),
							(GetEDIMessagePK(connection, testDbName, "HPU"), "CAN"),
							(GetEDIMessagePK(connection, testDbName, "AQU"), "CAN"),

							(GetEDIMessagePK(connection, testDbName, "SNT"), "SNT"),
							(GetEDIMessagePK(connection, testDbName, "SUS"), "SUS")
						};

						var interchanges = new (Guid PK, string Status)[]
						{
							(GetEDIInterchangePK(connection, testDbName, "PND"), "CAN"),
							(GetEDIInterchangePK(connection, testDbName, "QUE"), "CAN"),
							(GetEDIInterchangePK(connection, testDbName, "HPN"), "CAN"),
							(GetEDIInterchangePK(connection, testDbName, "HQU"), "CAN"),
							(GetEDIInterchangePK(connection, testDbName, "HPU"), "CAN"),
							(GetEDIInterchangePK(connection, testDbName, "AQU"), "CAN"),

							(GetEDIInterchangePK(connection, testDbName, "SNT"), "SNT"),
							(GetEDIInterchangePK(connection, testDbName, "SUS"), "SUS")
						};

						CombineAssertions(() =>
						{
							messages.ForEach(c => Assert($"Should have the message with {c.Status} for testing.", c.PK != Guid.Empty));
							interchanges.ForEach(c => Assert($"Should have the interchange with {c.Status} for testing.", c.PK != Guid.Empty));
						});

						//Insert New Values and SD_Name (To satisfy the CheckMinUpgradeableDbMajorSchemaVersion method) before copying production to test
						InsertStmData(connection, testDbName, "DATABASE_SCHEMA_VERSION", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), true);

						//
						// Copy production to test
						//
						manager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						//
						// Assert test DB values after copy
						//

						CombineAssertions(() =>
						{
							var message = @"Should change the status to 'CAN' when the previous status is in 'PND', 'QUE', 'HPN', 'HQU', 'HPU', 'AQU'.";

							messages.ForEach(c => AssertEDIMessage(connection, testDbName, c.PK, message, c.Status));
							interchanges.ForEach(c => AssertEDIInterchange(connection, testDbName, c.PK, message, c.Status));
						});
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.Command(dropCommand).ExecuteNonQuery();
					}
				}
			}
		}

		void AssertNoDotNetVersionDetailsExist(string dbName)
		{
			//Assert all 'DOTNET_VERION|%' records are deleted from dbo.StmData
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var vl = GetStmData(connection, dbName, "DOTNET_VERSION|PC01");
				AssertNull("DOTNET_VERSION|PC01 is deleted", GetStmData(connection, dbName, "DOTNET_VERSION|PC01"));
				AssertNull("DOTNET_VERSION|PC02 is deleted", GetStmData(connection, dbName, "DOTNET_VERSION|PC02"));
				AssertNull("DOTNET_VERSION|SRV68 is deleted", GetStmData(connection, dbName, "DOTNET_VERSION|SRV68"));
				AssertNull("DOTNET_VERSION|SRV79 is deleted", GetStmData(connection, dbName, "DOTNET_VERSION|SRV79"));
			}
		}

		public void TestGetActualDatabaseName()
		{
			var targetDbName = "DbToBeRestored";

			using (var tempDir = new TempDirectory())
			{
				var testFileName = "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription.bak";
				var testMainDBBackUpFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testFileName);
				TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription_SD001.bak");
				TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription_RefDb_Trf_CA.bak");
				TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-CW-RefDatabase.bak");

				var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
				var manager = new DbRestoreManager(dbHeaderOnlyReaderMock.Object);
				var dbFiles = manager.GetBackupDbFileInfoCollection(Db.ServerName, testMainDBBackUpFilePath, null, null, null, null);

				AssertEquals("Number of DB Files", 8, dbFiles.Count);
				var expectedDbNames = new[] { targetDbName, targetDbName + "_RefDb_Trf_CA", targetDbName + "_SD001", "CW-RefDatabase" };

				AssertContainsExactElementsInAnyOrder("Database Names",
					expectedDbNames.Concat(expectedDbNames), //each db has 2 files
					dbFiles.Cast<DbFileInfo>().Select(f => manager.GetActualDatabaseName(targetDbName, f)));

				var dbList = dbFiles.Cast<DbFileInfo>().Select(x => manager.GetActualDatabaseName(targetDbName, x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

				AssertEquals("There should be 4 databases", 4, dbList.Count);
				AssertCollectionContains("Main database is in the list", targetDbName, dbList);
				AssertCollectionContains("Reference database is in the list", targetDbName + "_RefDb_Trf_CA", dbList);
				AssertCollectionContains("eDocs database is in the list", targetDbName + "_SD001", dbList);
				AssertCollectionContains("CW-RefDatabase is in the list", "CW-RefDatabase", dbList);
			}
		}

		public void TestEnsureDefaultSingleRefDbIsNotBeingRestore()
		{
			// Arrange
			using var tempDir = new TempDirectory();
			var testFileName = "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription.bak";
			var testMainDbBackUpFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testFileName);
			TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-CW-RefDatabase.bak");

			var mockManager = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>()) { CallBase = true };
			mockManager
				.Protected()
				.Setup(
					"RestorePrimaryReplica",
					ItExpr.IsAny<List<DatabaseDetails>>(),
					ItExpr.IsAny<DbRestoreSettings>()
				)
				.Verifiable();

			var restoreDbFiles = mockManager.Object.GetBackupDbFileInfoCollection(Db.ServerName, testMainDbBackUpFilePath, null, null, null, null);
			var mainServerConfig = new DbServerConfiguration(Db.ServerName, testMainDbBackUpFilePath);
			var auditServerConfig = AuditDbServerConfiguration.Empty;
			var edwServerConfig = EdwDbServerConfiguration.Empty;
			var dbRestoreSettings = new DbRestoreSettings("DbToBeRestored", restoreDbFiles, DbRestoreOption.RestoreWithNoRecovery);

			// Act
			mockManager.Object.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

			// Verify
			mockManager
				.Protected()
				.Verify("RestorePrimaryReplica",
					Times.Never(),
					ItExpr.Is<List<DatabaseDetails>>(details =>
						details.Count == 1 &&
						details[0].DatabaseName == RefDbTableNameResolver.DefaultSingleRefDbName),
					ItExpr.IsAny<DbRestoreSettings>()
				);
		}

		[UseSnapshotProtection]
		public void TestGetFBKFileListWithoutMediaDescription()
		{
			//Test backups Without MediaDescription
			/* Testing:
					* FBK Backup results in a list which is the same as a backup with the tools naming convention
					* Picks up all relevant databases with timestamp within time period
					* Does not add FileValidity.StandardValid files with timestamps within time period
			*/
			using (var tempDir = new TempDirectory())
			{
				var testFile = "OdysseyNoDescription.bak";
				var testFileWithName = "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription.bak";

				foreach (var filePath in new[]
					{
						testFile,
						testFileWithName,
						"OdysseyNoDescription_RefDb_Trf_CA.bak",
						"OdysseyNoDescription_SD001.bak",
						"CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription_RefDb_Trf_CA.bak",
						"CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription_SD001.bak",
					})
				{
					TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, filePath);
				}

				var testData = GetBackupFileListNew(tempDir.DirectoryName, testFile, testFileWithName);
				DoCheck(testData.expectedFileInfoCollection, testData.resultFileInfoCollection);
			}
		}

		[UseSnapshotProtection]
		public void TestGetFBKFileListWithMediaDescription()
		{
			//Test backups With MediaDescription
			/* Testing:
					* FBK Backup results in a list which is the same as a backup with the tools naming convention
					* Picks up all relevant databases with the same MediaDescription
					* Does not add FileValidity.StandardValid files with same MediaDescription
			*/
			using (var tempDir = new TempDirectory())
			{
				var testFile = "OdysseyWithDescription.bak";
				var testFileWithName = "CW1Bkp_T-20151023-111725_S-SYDCO-WJBL-1_D-OdysseyWithDescription.bak";

				foreach (var filePath in new[]
					{
						testFile,
						testFileWithName,
						"OdysseyWithDescription_RefDb_Trf_CA.bak",
						"OdysseyWithDescription_SD001.bak",
						"CW1Bkp_T-20151023-111725_S-SYDCO-WJBL-1_D-OdysseyWithDescription_RefDb_Trf_CA.bak",
						"CW1Bkp_T-20151023-111725_S-SYDCO-WJBL-1_D-OdysseyWithDescription_SD001.bak",
					})
				{
					TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, filePath);
				}

				var testData = GetBackupFileListNew(tempDir.DirectoryName, testFile, testFileWithName);
				DoCheck(testData.resultFileInfoCollection, testData.expectedFileInfoCollection);
			}
		}

		public void TestGetFBKFileListWithAnyOffsetTime()
		{
			// Arrange
			using (var tempDir = new TempDirectory())
			{
				var filepath = $"{Db.DatabaseName}.bak";
				var (newBackupMediaDate, oldBackupMediaDate) = CreateFreshAndStaleBackups(tempDir.DirectoryName, filepath);
				AssertLessThanOrEqualTo("New backup should have recent MediaDate", newBackupMediaDate - DateTime.Now, TimeSpan.FromMinutes(1));
				AssertGreaterThanOrEqualTo("Old backup should have very old MediaDate", DateTime.Now - oldBackupMediaDate, TimeSpan.FromHours(10));

				var expectedList = Directory
					.EnumerateFiles(tempDir.DirectoryName)
					.Select(s => Path.GetFileName(s))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.ToList();

				var manager = new DbRestoreManager(new DbHeaderOnlyReader());
				var backUpFilePath = Path.Combine(tempDir.DirectoryName, filepath);

				// Act
				var result = manager
					.GetBackupDbFileInfoCollection(Db.ServerName, backUpFilePath, null, null, null, null)
					.Cast<DbFileInfo>()
					.Select(info => Path.GetFileName(info.FilePath))
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.ToList();

				// Assert
				AssertGreaterThanOrEqualTo(expectedList.Count, 2);
				AssertContainsExactElementsInAnyOrder(StringComparer.OrdinalIgnoreCase, expectedList, result);
			}

			(DateTime newBackupMediaDate, DateTime oldBackupMediaDate) CreateFreshAndStaleBackups(string directoryName, string fileName)
			{
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					var pathForNewBackup = Path.Combine(directoryName, fileName);
					connection.ExecuteScalar($"BACKUP DATABASE [{Db.DatabaseName}] TO DISK = '{pathForNewBackup}' WITH INIT, COMPRESSION");

					var pathForOldBackup = TestDataHelpers.CopyAndNameTestFileResource(directoryName, "OdysseyNoDescription_SD001.bak", $"{Db.DatabaseName}_SD001.bak");

					return (GetMediaDate(connection, pathForNewBackup), GetMediaDate(connection, pathForOldBackup));
				}
			}

			DateTime GetMediaDate(AdminConnection connection, string path)
			{
				var values = new List<DateTime>();
				connection.ExecuteReader(
					$"RESTORE LABELONLY FROM DISK = '{path}'",
					record => values.Add((DateTime)record["mediadate"]));
				return values.Single();
			}
		}

		// Test is broken since the test files don't exist.
		//[UseSnapshotProtection]
		//public void TestGetDBKFileListWithoutMediaDescription()
		//{
		//	//Test backups Without MediaDescription
		//	/* Testing:
		//			* DBK Backup results in a list which is the same as a backup with the tools naming convention
		//			* Picks up all relevant databases with timestamp within time period
		//			* Does not add FileValidity.StandardValid files with timestamps within time period
		//	*/
		//	var testData = GetBackupFileList("OdysseyNoDescription.dbk", "test_OdysseyNoDescription_Diff.dbk");
		//	DoCheck(testData.resultFileInfoCollection, testData.expectedFileInfoCollection);
		//}

		// Test is broken since the test files don't exist.
		//[UseSnapshotProtection]
		//public void TestGetDBKFileListWithMediaDescription()
		//{
		//	//Test backups With MediaDescription
		//	/* Testing:
		//			* DBK Backup results in a list which is the same as a backup with the tools naming convention
		//			* Picks up all relevant databases with the same MediaDescription
		//			* Does not add FileValidity.StandardValid files with same MediaDescription
		//	*/
		//	var testData = GetBackupFileList("OdysseyWithDescription.dbk", "test_OdysseyWithDescription_Diff.dbk");
		//	DoCheck(testData.resultFileInfoCollection, testData.expectedFileInfoCollection);
		//}

		static (DbFileInfoCollection resultFileInfoCollection, DbFileInfoCollection expectedFileInfoCollection) GetBackupFileListNew(string testBackUpFolder, string filepath, string filepathWithName)
		{
			var manager = new DbRestoreManager(new DbHeaderOnlyReader());
			var testMainDBBackUpFilePath = Path.Combine(testBackUpFolder, filepath);
			var testMainDBBackUpFilePathWithName = Path.Combine(testBackUpFolder, filepathWithName); //same file as above but with backup tool naming

			var result = manager.GetBackupDbFileInfoCollection(Db.ServerName, testMainDBBackUpFilePath, null, null, null, null);
			var expected = manager.GetBackupDbFileInfoCollection(Db.ServerName, testMainDBBackUpFilePathWithName, null, null, null, null);

			return (result, expected);
		}

		void DoCheck(DbFileInfoCollection expected, DbFileInfoCollection result)
		{
			AssertNotEquals(0, expected.Count);
			AssertEquals(expected.Count, result.Count);

			using (var conn = Db.NewAdminConnection())
			{
				for (var i = 0; i < expected.Count; i++)
				{
					var resultSet = new object[13];
					var expectedSet = new object[13];

					Assert(result[i].Visible);
					Assert(expected[i].Visible);

					var sqlText = @"RESTORE LABELONLY FROM DISK = '" + result[i].FilePath + "'";

					using (var cmd = conn.Command(sqlText))
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							reader.GetValues(resultSet);
						}
					}

					sqlText = @"RESTORE LABELONLY FROM DISK = '" + expected[i].FilePath + "'";

					using (var cmd = conn.Command(sqlText))
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							reader.GetValues(expectedSet);
						}
					}

					Assert(resultSet.Length > 0);
					Assert(expectedSet.Length > 0);
					AssertArrayEqualsByElements(expectedSet, resultSet);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToTest_OldRegistryItemIsDeletedBeforeCopyingFromAuxDb()
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
				var testBackUpFileName = "OdysseyWithOldRegistryItem.bak";
				using (var tempDir = new TempDirectory())
				{
					var testLocalBackupFilePath = Path.Combine(tempDir.DirectoryName, testBackUpFileName);
					BackupHelper.CreateDatabaseBackupFromScript(testLocalBackupFilePath, "OdysseyWithOldRegistryItem.sql", "OdysseyWithOldRegistryItem");
					var dbFiles = new DbFileInfoCollection();
					dbFiles.Add(new DbFileInfo("OdysseyWithOldRegistryItem", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
					dbFiles.Add(new DbFileInfo("OdysseyWithOldRegistryItem_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));
					var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);

					var manager = new DbRestoreManagerForTesting();
					manager.AddPasswordHashColumnsIfMissing(connection, testDbName);
					manager.AddStorageDocsToDeleteTableIfMissing(connection, testDbName);
					manager.AddPreserveTestValueColumnIfMissing(connection, testDbName);
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						manager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						Assert(connection.DatabaseExists(testDbName));
						AssertNull(GetStmData(connection, testDbName, "FreightNotesHeaderLength"));
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		public void TestCopyProductionToTest_CheckLogAndRegistry()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";

			using (var tempDir = new TempDirectory())
			{
				var testBackUpFileName = "testRestore.bak";
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("Odyssey_Data", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("Odyssey_Data02", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("Odyssey_Log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);
						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						Assert(connection.DatabaseExists(testDbName));
						AssertStmALog(connection, testDbName, nameof(DbRestoreOption.RestoreWithRecovery), "", "2035.-1");
						AssertRegistry(connection, testDbName, nameof(DbRestoreOption.RestoreWithRecovery), "", "2035.-1");
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		public void TestCopyProductionToTest_WithRecoveryDoesNotReportAnyIssues()
		{
			// Arrange
			const string testDbName = "TestTargetDb_6C7A4D18D8A24917856B659EFBE761B5";

			using (var tempDir = new TempDirectory())
			{
				var testBackUpFileName = "testRestore.bak";
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

				var dbFiles = new DbFileInfoCollection
				{
					new DbFileInfo("Odyssey_Data", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath),
					new DbFileInfo("Odyssey_Data02", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath),
					new DbFileInfo("Odyssey_Log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath),
				};

				var manager = new DbRestoreManagerForTesting();
				var errorMessages = new List<string>();
				manager.OnTaskFailed += message => errorMessages.Add(message);

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				using (new DisposableAction(() => connection.ExecuteNonQuery(CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName))))
				{
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

					// Act
					var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

					manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				}

				// Assert
				AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), errorMessages);
			}
		}

		public void TestCopyProductionToTest_OlderVersionRequired()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			using (var tempDir = new TempDirectory())
			{
				var testBackUpFileName = "testProdCopy.bak";
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						Assert(connection.DatabaseExists(testDbName));

						using (((ICurrentDbControl)connection).UseDatabase(testDbName))
						{
							new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "StmData", "SD_PreserveTestValue").DropRelateObjects(connection);
							const string sqlText = @"ALTER TABLE dbo.StmData DROP COLUMN SD_PreserveTestValue;";
							connection.ExecuteNonQuery(sqlText);
						}

						var messages = new List<string>();
						manager.OnShowInfoMessage += new InformationEvent(message => messages.Add(message));

						dbRestoreSettings.RestoreOption = DbRestoreOption.CopyProdToTest;
						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						AssertCollectionContains(messages, message => message.Contains("Restoring to this database requires Database Backup And Restore tool version 15.10.18.0 or older."));
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		public void TestRestore_Keep_CDC_WithNoRecovery()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			using (var tempDir = new TempDirectory())
			{
				var testBackUpFileName = "testProdWithCdc.bak";
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("testProdWithCdc", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("testProdWithCdc_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);
						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithNoRecovery);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						Assert("Database was not created.", connection.DatabaseExists(testDbName));
						AssertCdcEnabled(connection, testDbName);
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		public void TestRestore_CheckCdc()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			using (var tempDir = new TempDirectory())
			{
				var testBackUpFileName = "testProdWithCdc.bak";
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("testProdWithCdc", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("testProdWithCdc_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();

				using (var logger = new TestLogger())
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						Assert("Database was not created.", connection.DatabaseExists(testDbName));
						AssertCdcNotEnabled(connection, testDbName, logger.GetOutputMessage());
						AssertNullOrEmpty("BiAuditServer", Utilities.GetAuditServer(connection, testDbName));
						AssertNullOrEmpty("BiDataWarehouseServer", Utilities.GetDataWarehouseServer(connection, testDbName));
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		public void TestRestore_CheckCdc_DbNotCdcEnabled()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			using (var tempDir = new TempDirectory())
			{
				var testBackUpFileName = "testProdWithoutCdc.bak";
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("testProdWithoutCdc", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("testProdWithoutCdc_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();

				using (var logger = new TestLogger())
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						Assert("Database was not created.", connection.DatabaseExists(testDbName));
						AssertCdcNotEnabled(connection, testDbName, logger.GetOutputMessage());
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		public void TestRestoreBackupWithInvalidPath()
		{
			var testDbName = "TestTargetDb_BB836453D4543A815F3D504";

			using (var tempDir = new TempDirectory())
			{
				var testBackUpFileName = "testProdWithCdc.bak";
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

				var manager = new DbRestoreManagerForTesting();

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("testProdWithCdc", "Z:\\Some\\Random\\Path\\That\\Should\\Not\\Exist", DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("testProdWithCdc_log", "Z:\\Some\\Random\\Path\\That\\Should\\Not\\Exist", DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath, tempDir.DirectoryName, tempDir.DirectoryName);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						Assert("Database was not created.", connection.DatabaseExists(testDbName));
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		public void TestRestore_BiDatabases()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";

			var testDatabase = "CW1Bkp_T-20161028-110830_S-SYDCO-WJMG-1_D-testBiDatabase.bak";
			var testAuditDatabase = "CW1Bkp_T-20161028-110830_S-SYDCO-WJMG-1_D-testBiDatabase_Audit.bak";
			var testEdwDatabase = "CW1Bkp_T-20161028-110830_S-SYDCO-WJMG-1_D-testBiDatabase_EDW.bak";

			using (var tempDir = new TempDirectory())
			using (var newFolderPathDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testDatabase);
				var testAuditLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testAuditDatabase);
				var testEdwLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testEdwDatabase);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("testBiDatabase", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("testBiDatabase_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("testBiDatabase_Audit", tempDir.DirectoryName, DbFileInfo.FileTypeData, testAuditLocalBackupFilePath, dbType: DbFileInfo.DbTypeAuditDB, restoreDbName: "_Audit"));
				dbFiles.Add(new DbFileInfo("testBiDatabase_Audit_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testAuditLocalBackupFilePath, dbType: DbFileInfo.DbTypeAuditDB, restoreDbName: "_Audit"));
				dbFiles.Add(new DbFileInfo("testBiDatabase_EDW", tempDir.DirectoryName, DbFileInfo.FileTypeData, testEdwLocalBackupFilePath, dbType: DbFileInfo.DbTypeEdwDB, restoreDbName: "_EDW"));
				dbFiles.Add(new DbFileInfo("testBiDatabase_EDW_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testEdwLocalBackupFilePath, dbType: DbFileInfo.DbTypeEdwDB, restoreDbName: "_EDW"));

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

				var manager = new DbRestoreManagerForTesting();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeAuditDB, true);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEdwDB, true);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						CombineAssertions(() =>
						{
							Assert("Database was not created.", connection.DatabaseExists(testDbName));
							Assert("Audit database was not created.", connection.DatabaseExists(testDbName + "_Audit"));
							Assert("EDW database was not created.", connection.DatabaseExists(testDbName + "_EDW"));

							Assert("Main database data file created in incorrect folder path.", !File.Exists(Path.Combine(tempDir.DirectoryName, testDbName + "_Data.mdf")));
							Assert("Main database log file created in incorrect folder path.", !File.Exists(Path.Combine(tempDir.DirectoryName, testDbName + "_Log.ldf")));
							Assert("Audit database data file created in incorrect folder path.", !File.Exists(Path.Combine(tempDir.DirectoryName, testDbName + "_Audit_Data.mdf")));
							Assert("Audit database log file created in incorrect folder path.", !File.Exists(Path.Combine(tempDir.DirectoryName, testDbName + "_Audit_Log.ldf")));
							Assert("EDW database data file created in incorrect folder path.", !File.Exists(Path.Combine(tempDir.DirectoryName, testDbName + "_EDW_Data.mdf")));
							Assert("EDW database log file created in incorrect folder path.", !File.Exists(Path.Combine(tempDir.DirectoryName, testDbName + "_EDW_Log.ldf")));

							Assert("Main database data file does not exist.", File.Exists(Path.Combine(newFolderPathDir.DirectoryName, testDbName + "_Data.mdf")));
							Assert("Main database log file does not exist.", File.Exists(Path.Combine(newFolderPathDir.DirectoryName, testDbName + "_Log.ldf")));
							Assert("Audit database data file does not exist.", File.Exists(Path.Combine(newFolderPathDir.DirectoryName, testDbName + "_Audit_Data.mdf")));
							Assert("Audit database log file does not exist.", File.Exists(Path.Combine(newFolderPathDir.DirectoryName, testDbName + "_Audit_Log.ldf")));
							Assert("EDW database data file does not exist.", File.Exists(Path.Combine(newFolderPathDir.DirectoryName, testDbName + "_EDW_Data.mdf")));
							Assert("EDW database log file does not exist.", File.Exists(Path.Combine(newFolderPathDir.DirectoryName, testDbName + "_EDW_Log.ldf")));
						});
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
						dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName + "_Audit");
						connection.ExecuteNonQuery(dropCommand);
						dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName + "_EDW");
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRestore_DoesNotDeleteBiServerRegistryItemsWhenCopyProdToTest()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";

			var testDatabase = "testProdWithBiServers.bak";

			using (var tempDir = new TempDirectory())
			using (var newFolderPathDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testDatabase);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("TestDb", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("TestDb_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						Assert("Database was not created.", connection.DatabaseExists(testDbName));

						CombineAssertions(() =>
						{
							AssertNotNullOrEmpty("BiAuditServer", Utilities.GetAuditServer(connection, testDbName));
							AssertNotNullOrEmpty("BiDataWarehouseServer", Utilities.GetDataWarehouseServer(connection, testDbName));
							AssertNotNullOrEmpty("BiAnalysisServer", Utilities.GetRegistryValue(connection, testDbName, "BiAnalysisServer"));
							AssertNotNullOrEmpty("BiPowerBiWebPortalUrl", Utilities.GetRegistryValue(connection, testDbName, "BiPowerBiWebPortalUrl"));
						});
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.Command(dropCommand).ExecuteNonQuery();
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRestore_DoesDeleteBiServerRegistryItemsWhenNotCopyProdToTest()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";

			var testDatabase = "testProdWithBiServers.bak";

			using (var tempDir = new TempDirectory())
			using (var newFolderPathDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testDatabase);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("TestDb", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("TestDb_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var manager = new DbRestoreManagerForTesting();
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath, newFolderPathDir.DirectoryName, newFolderPathDir.DirectoryName);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						Assert("Database was not created.", connection.DatabaseExists(testDbName));

						CombineAssertions(() =>
						{
							AssertNullOrEmpty("BiAuditServer", Utilities.GetAuditServer(connection, testDbName));
							AssertNullOrEmpty("BiDataWarehouseServer", Utilities.GetDataWarehouseServer(connection, testDbName));
							AssertNullOrEmpty("BiAnalysisServer", Utilities.GetRegistryValue(connection, testDbName, "BiAnalysisServer"));
							AssertNullOrEmpty("BiPowerBiWebPortalUrl", Utilities.GetRegistryValue(connection, testDbName, "BiPowerBiWebPortalUrl"));
						});
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRestore_PhysicalFileWithDoubleBackslash()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			var testDatabase = "testProdCopy.bak";

			using (var tempDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testDatabase);

				var dataFilePath = tempDir.DirectoryName + @"\\" + testDbName + "_Data.mdf";
				var logFilePath = tempDir.DirectoryName + @"\\" + testDbName + "_Log.ldf";

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						// Restore initial test database with double backslash
						var restoreSql = string.Format(CultureInfo.InvariantCulture, @"
						RESTORE DATABASE [{0}]
						FROM DISK = '{1}'
						WITH REPLACE,
						MOVE '{2}' TO '{3}',
						MOVE '{4}' TO '{5}'",
							testDbName, // 0
							testLocalBackupFilePath, // 1
							"TestProdDbCopy", // 2
							dataFilePath, // 3
							"TestProdDbCopy_log", // 4
							logFilePath); // 5
						connection.ExecuteNonQuery(restoreSql);

						// Restore using DbBackupAndRestore
						var dbFiles = new DbFileInfoCollection();
						dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
						dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));
						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

						var manager = new DbRestoreManagerForTesting();
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						manager.DoRestore_Exposed(connection, mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						Assert("Database should be created.", connection.DatabaseExists(testDbName));

						var assertQuery = @"SELECT physical_name FROM sys.master_files WHERE name = '{0}'";
						AssertEquals(dataFilePath, connection.ExecuteScalar(string.Format(assertQuery, "TestProdDbCopy")));
						AssertEquals(logFilePath, connection.ExecuteScalar(string.Format(assertQuery, "TestProdDbCopy_log")));
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestRestore_DifferentialBackup()
		{
			var testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			var testBackUp = "testProdCopy.bak";
			var testDiffBackUp = "testProdCopy_Diff.dbk";

			using (var tempDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUp);
				var testLocalDiffBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testDiffBackUp);

				var manager = new DbRestoreManagerForTesting();

				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				{
					try
					{
						// Restore with recovery, no differential
						var dbFiles = new DbFileInfoCollection();
						dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
						dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						AssertEquals(DatabaseStatus.Online, Utilities.GetDatabaseStatus(connection, testDbName));

						// Check the original value (it is "B1" in testProdCopy.bak)
						var getChangedValue2 = @"SELECT GB_Code FROM [{0}].[dbo].[GlbBranch]";
						AssertEquals("B1 ", connection.ExecuteScalar(string.Format(getChangedValue2, testDbName)));

						// Always required to restore full backup with norecovery before restore with differential
						dbRestoreSettings.RestoreOption = DbRestoreOption.RestoreWithNoRecovery;
						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						AssertEquals(DatabaseStatus.Restoring, Utilities.GetDatabaseStatus(connection, testDbName));

						// Restore with recovery, differential
						var dbDiffFiles = new DbFileInfoCollection();
						dbDiffFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalDiffBackupFilePath));
						dbDiffFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalDiffBackupFilePath));
						dbDiffFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbDiffFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbDiffFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						dbRestoreSettings = new DbRestoreSettings(testDbName, dbDiffFiles, DbRestoreOption.RestoreWithRecovery);
						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						AssertEquals(DatabaseStatus.Online, Utilities.GetDatabaseStatus(connection, testDbName));

						Assert("Database should be created.", connection.DatabaseExists(testDbName));

						// Check the value has changed (it is "B2" in testProdCopy_diff.dbk.).
						AssertEquals("B2 ", connection.ExecuteScalar(string.Format(getChangedValue2, testDbName)));
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToTestTransformationVersionNoVersionInTest()
		{
			ProductionToTestTransformationVersionAsserts(expectedException: true);
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToTestTransformationVersionSchemaVersionLessThanMinSchema()
		{
			ProductionToTestTransformationVersionAsserts(
				  testTransformationVersion: "100"
				, testMinorTransformationVersion: "10"
				, testSchemaVersion: "1000"
				, expectedException: true);
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToTestTransformationVersionTestVersionIsGreaterThanProd()
		{
			ProductionToTestTransformationVersionAsserts(
				  testTransformationVersion: "100"
				, testMinorTransformationVersion: "10"
				, testSchemaVersion: (DataRegistry.MinUpgradableDbMajorSchemaVersion + 1).ToString()
				, prodTransformationVersion: "99"
				, prodMinorTransformationVersion: "9"
				, prodSchemaVersion: "1200"
				, expectedTransformationVersion: "99"
				, expectedMinorTransformationVersion: "9"
				, expectedSchemaVersion: "1200");
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToTestTransformationVersionTestVersionIsLessThanProd()
		{
			ProductionToTestTransformationVersionAsserts(
				  testTransformationVersion: "100"
				, testMinorTransformationVersion: "10"
				, testSchemaVersion: (DataRegistry.MinUpgradableDbMajorSchemaVersion + 1).ToString()
				, prodTransformationVersion: "101"
				, prodMinorTransformationVersion: "8"
				, prodSchemaVersion: "1200"
				, expectedTransformationVersion: "100"
				, expectedMinorTransformationVersion: "10"
				, expectedSchemaVersion: "1200");
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToTestTransformationVersionTempTableExists()
		{
			CreateTempTable();
			ProductionToTestTransformationVersionAsserts(
				  testTransformationVersion: "100"
				, testMinorTransformationVersion: "10"
				, testSchemaVersion: (DataRegistry.MinUpgradableDbMajorSchemaVersion + 1).ToString()
				, prodTransformationVersion: "101"
				, prodMinorTransformationVersion: "8"
				, prodSchemaVersion: "1200"
				, expectedTransformationVersion: "100"
				, expectedMinorTransformationVersion: "10"
				, expectedSchemaVersion: "1200");
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToTestDatabaseApplicationVersionIsOlderThanDbSchema()
		{
			var version = (SchemaVersion.Application.Major + 1).ToString();

			ProductionToTestTransformationVersionAsserts(testSchemaVersion: version, prodSchemaVersion: version, expectedSchemaVersion: version, expectedException: true);
		}

		void ProductionToTestTransformationVersionAsserts(
			  string testTransformationVersion = null
			, string testMinorTransformationVersion = null
			, string testSchemaVersion = null
			, string prodTransformationVersion = null
			, string prodMinorTransformationVersion = null
			, string prodSchemaVersion = null
			, string expectedTransformationVersion = null
			, string expectedMinorTransformationVersion = null
			, string expectedSchemaVersion = null
			, bool expectedException = false)
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				using (var tempDir = new TempDirectory())
				{
					string testDbName;
					string testLocalBackupFilePath;
					DbFileInfoCollection dbFiles;
					PrepareBackupFiles(tempDir, out testDbName, out testLocalBackupFilePath, out dbFiles);
					var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);

					var manager = new DbRestoreManagerForTesting();
					manager.AddPasswordHashColumnsIfMissing(connection, testDbName);
					manager.AddStorageDocsToDeleteTableIfMissing(connection, testDbName);

					try
					{
						//
						//Restore Test Database
						//
						manager.DoRestore_Exposed(connection, mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						var exceptionCaught = false;
						try
						{
							var deleteAllTransformationVersions = string.Format(DeleteAllTransformationVersions, testDbName);
							// Update test
							connection.ExecuteNonQuery(deleteAllTransformationVersions);
							if (!string.IsNullOrEmpty(testTransformationVersion))
							{
								InsertStmData(connection, testDbName, CopyProductionToTestHelper.TransformationVersion, testTransformationVersion);
							}
							if (!string.IsNullOrEmpty(testMinorTransformationVersion))
							{
								InsertStmData(connection, testDbName, CopyProductionToTestHelper.MinorTransformationVersion, testMinorTransformationVersion);
							}
							if (!string.IsNullOrEmpty(testSchemaVersion))
							{
								InsertStmData(connection, testDbName, CopyProductionToTestHelper.SchemaVersion, testSchemaVersion);
							}

							// Update test after restore prod
							manager.SqlCommandAfterRestoreDatabaseActions.Add(() => connection.ExecuteNonQuery(deleteAllTransformationVersions));
							if (!string.IsNullOrEmpty(prodTransformationVersion))
							{
								manager.SqlCommandAfterRestoreDatabaseActions.Add(() => InsertStmData(connection, testDbName, CopyProductionToTestHelper.TransformationVersion, prodTransformationVersion));
							}
							if (!string.IsNullOrEmpty(prodMinorTransformationVersion))
							{
								manager.SqlCommandAfterRestoreDatabaseActions.Add(() => InsertStmData(connection, testDbName, CopyProductionToTestHelper.MinorTransformationVersion, prodMinorTransformationVersion));
							}
							if (!string.IsNullOrEmpty(prodSchemaVersion))
							{
								manager.SqlCommandAfterRestoreDatabaseActions.Add(() => InsertStmData(connection, testDbName, CopyProductionToTestHelper.SchemaVersion, prodSchemaVersion));
							}

							manager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
						}
						catch (DbBackupAndRestoreException e)
						{
							if (Convert.ToInt32(expectedSchemaVersion) > SchemaVersion.Application.Major)
							{
								AssertEquals(string.Format(CopyProductionToTestHelper.ExceptionIfDbSchemaIsGreaterThanAppVersion, testDbName), e.Message);
							}
							else
							{
								AssertEquals(
									string.Format(CopyProductionToTestHelper.ExceptionIfVersionIsLessThanMinOrNULL, DataRegistry.MinUpgradableDbMajorSchemaVersion, testSchemaVersion ?? "NULL", testDbName)
									, e.Message);
							}
							exceptionCaught = true;
						}

						if (expectedException)
						{
							AssertEquals(exceptionCaught, true);
						}
						else
						{
							//Assert
							// It keeps the min version for Major and Minor but update the SCHEMA version
							var value = GetStmData(connection, testDbName, CopyProductionToTestHelper.TransformationVersion);
							AssertEquals(expectedTransformationVersion, value);
							value = GetStmData(connection, testDbName, CopyProductionToTestHelper.MinorTransformationVersion);
							AssertEquals(expectedMinorTransformationVersion, value);
							value = GetStmData(connection, testDbName, CopyProductionToTestHelper.SchemaVersion);
							AssertEquals(expectedSchemaVersion, value);
						}
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		static void PrepareBackupFiles(TempDirectory tempDir, out string testDbName, out string testLocalBackupFilePath, out DbFileInfoCollection dbFiles)
		{
			testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			var testBackUp = "testProdCopy.bak";
			testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUp);

			dbFiles = new DbFileInfoCollection();
			dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
			dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));
		}

		[UseSnapshotProtection]
		public void TestCopyProductionToNewDatabase()
		{
			using (var connection = Db.NewAdminConnection())
			{
				using (var tempDir = new TempDirectory())
				{
					string testDbName;
					string testLocalBackupFilePath;
					DbFileInfoCollection dbFiles;
					PrepareBackupFiles(tempDir, out testDbName, out testLocalBackupFilePath, out dbFiles);
					var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.CopyProdToTest);
					var manager = new DbRestoreManagerForTesting();
					manager.AddPasswordHashColumnsIfMissing(connection, testDbName);
					manager.AddStorageDocsToDeleteTableIfMissing(connection, testDbName);

					try
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);

						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
						dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

						//copying production to test
						manager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						//assert maildbstatus changed
						var status = GetMailDbStatus(connection, testDbName);
						AssertNotEquals("QUE", status);

						//assert StmPrintJob are gone
						AssertEquals(0, GetStmPrintJobCount(connection, testDbName));

						// Email Registry Items
						AssertNull(GetStmData(connection, testDbName, "MailboxDisplayName"));

						// Licence usage registry updated
						var value = GetStmData(connection, testDbName, "WarehouseCountPackages");
						var minutesFromNow = (SqlFormatInfo.FromSqlDateTime(value) - DateTime.UtcNow).TotalMinutes;
						Assert("last report date set to roughly now", minutesFromNow < 50 && minutesFromNow > -15);

						// Company Licence Info modified to adjust to test system
						AssertCompanyLicencesAdjusted(connection, testDbName, "???");

						// IncidentApproval are gone
						AssertEquals(0, GetTableRowCount(connection, testDbName, "IncidentApproval"));

						var numberFountains = LoadAllNumberFountainsOrderByNameOwner(connection, testDbName);
						AssertEquals("test data contains a number fountain that is not in the preserved list", 1, numberFountains.Count);
						AssertEquals("test number fountain name", "ARInvoiceNo", numberFountains[0].SN_Name);
						AssertEquals("test number fountain value", 1001, numberFountains[0].SN_Value);
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}
		}

		public void TestGetBackupDbFileInfoCollection()
		{
			var testBackUp = "testProdCopy.bak";

			using (var tempDir = new TempDirectory())
			{
				var nonSqlBackupFilePath = Path.Combine(tempDir.DirectoryName, "testProdCopy.txt");
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUp);
				var nonSqlBackupFile = File.CreateText(nonSqlBackupFilePath);
				nonSqlBackupFile.WriteLine("Line 1");
				nonSqlBackupFile.WriteLine("Line 2");
				nonSqlBackupFile.Close();

				var manager = new DbRestoreManager(new DbHeaderOnlyReader());
				var dbFiles = manager.GetBackupDbFileInfoCollection(Db.ServerName, testLocalBackupFilePath, null, null, null, null);

				Assert(dbFiles.Count == 2);
				AssertDbFileInfoIsCorrect(dbFiles, testLocalBackupFilePath);
			}
		}

		public void TestGetDifferentalBackupDbFileInfoCollection()
		{
			var testBackUp = "Odyssey_Diff.dbk";

			using (var tempDir = new TempDirectory())
			{
				var nonSqlBackupFilePath = Path.Combine(tempDir.DirectoryName, "Odyssey_Diff.txt");
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUp);
				var nonSqlBackupFile = File.CreateText(nonSqlBackupFilePath);
				nonSqlBackupFile.WriteLine("Line 1");
				nonSqlBackupFile.WriteLine("Line 2");
				nonSqlBackupFile.Close();

				var manager = new DbRestoreManager(new DbHeaderOnlyReader());
				var dbFiles = manager.GetBackupDbFileInfoCollection(Db.ServerName, testLocalBackupFilePath, null, null, null, null);

				Assert(dbFiles.Count == 2);
				AssertDbFileInfoIsCorrect(dbFiles, testLocalBackupFilePath);
			}
		}

		static void AssertDbFileInfoIsCorrect(DbFileInfoCollection dbFiles, string testLocalBackupFilePath)
		{
			foreach (DbFileInfo db in dbFiles)
			{
				AssertEquals("Database should be main type", DbFileInfo.DbTypeMain, db.DbType);
				AssertEquals("Backup file path is incorrect", testLocalBackupFilePath, db.FilePath);
				AssertEquals("Backup folder path is incorrect", db.FolderPathView, db.FolderPath);
				Assert("Database should be visible", db.Visible);
			}
		}

		void AssertSystemRegistrationKey(DbConnection connection, string testDbName, string expectedDbType, string expectedSecurityMode, string expectedHostedLocation)
		{
			var sysRegKey = LicenceBuilder.GetSystemRegistrationKey(connection, testDbName);
			AssertSystemRegistrationKey(sysRegKey, AdminConnection.ServerSid, connection.ServerInstanceName, testDbName, expectedDbType, expectedSecurityMode, expectedHostedLocation);
		}

		void AssertSystemRegistrationKey(ISystemRegistrationKey sysRegKey, Guid expectedSid,
			string expectedInstanceName, string expectedDbName, string expectedDbType, string expectedSecurityMode, string expectedHostedLocation, DateTime? expectedExpiryDate = null)
		{
			AssertEquals("Server SID", expectedSid, sysRegKey.ServerSid);
			AssertEquals("Server Instance", expectedInstanceName, sysRegKey.DbInstanceName);
			AssertEquals("Database Name", expectedDbName, sysRegKey.DatabaseName);
			AssertEquals("Database Type", expectedDbType, sysRegKey.DatabaseType);
			AssertEquals("DB Security Mode", expectedSecurityMode, sysRegKey.DbSecurityMode);
			AssertEquals("Hosted Location", expectedHostedLocation, sysRegKey.HostedLocation);

			if (expectedExpiryDate == null)
			{
				AssertEquals("System Expiry Date in the future?", true, sysRegKey.SystemExpiryDate > DateTime.UtcNow);
			}
			else
			{
				AssertEquals("System Expiry Date", expectedExpiryDate.Value.Date, sysRegKey.SystemExpiryDate.Date);
				AssertEquals("System Expiry Date", expectedExpiryDate.Value.Hour, sysRegKey.SystemExpiryDate.Hour);
				AssertEquals("System Expiry Date", expectedExpiryDate.Value.Minute, sysRegKey.SystemExpiryDate.Minute);
				AssertEquals("System Expiry Date", expectedExpiryDate.Value.Second, sysRegKey.SystemExpiryDate.Second);
			}
		}

		/// <summary>
		/// Asserts company licences were modified to adjust to test systems
		/// </summary>
		void AssertCompanyLicencesAdjusted(DbConnection connection, string testDbName, string expectedPhysicalServedId)
		{
			var value = GetStmData(connection, testDbName, FreightNotesLengthNew, new Guid("D438C595-B8D2-4B56-B3A4-3B3CD37D9418"), Guid.Empty);
			var xml = LicenceBuilderTest.DecryptedLicenceXml(value);
			AssertNotEquals(CompanyLicenceFromBackup01, value);
			LicenceBuilderTest.AssertPhysicalServerId(xml, expectedPhysicalServedId);

			value = GetStmData(connection, testDbName, FreightNotesLengthNew, new Guid("E7AC2090-BB53-43F9-BFA5-687A7BAE4467"), Guid.Empty);
			xml = LicenceBuilderTest.DecryptedLicenceXml(value);
			AssertNotEquals(CompanyLicenceFromBackup02, value);
			LicenceBuilderTest.AssertPhysicalServerId(xml, expectedPhysicalServedId);
		}

		class NumberFountainInfo
		{
			public string SN_Name;
			public long SN_Value;
			public Guid SN_Owner;
			public int SN_ID;
		}

		List<NumberFountainInfo> LoadAllNumberFountainsOrderByNameOwner(DbConnection connection, string testDbName)
		{
			var sql = @"
select SN_Name, ISNULL(MIN(SG_Value), SN_Value) as SN_Value, SN_Owner, SN_ID
from [{0}]..StmNums
left join [{0}]..StmNumberCache on SG_SN = SN_ID
group by SN_Name, SN_Value, SN_Owner, SN_ID
order by SN_Name, SN_Owner
";

			var result = new List<NumberFountainInfo>();

			using (var reader = connection.Command(string.Format(sql, testDbName)).ExecuteReader())
			{
				while (reader.Read())
				{
					var info = new NumberFountainInfo();
					info.SN_Name = reader[0].ToString();
					info.SN_Value = Convert.ToInt64(reader[1]);
					var owner = reader[2];
					info.SN_Owner = (owner is Guid) ? (Guid)owner : Guid.Empty;
					info.SN_ID = Convert.ToInt32(reader[3]);
					result.Add(info);
				}
			}

			return result;
		}

		void BumpNumberFountains(DbConnection connection, string testDbName)
		{
			// delete lowest number in cache from each fountain
			const string sql = @"
delete [{0}]..StmNumberCache
from [{0}]..StmNumberCache
join (
	select SN = SG_SN, MinValue = Min(SG_Value) from [{0}]..StmNumberCache
	group by SG_SN
) a on SG_SN = SN and SG_Value = MinValue
";
			connection.ExecuteNonQuery(string.Format(sql, testDbName));
		}

		void PopulateTranslationFeedbackData(DbConnection connection, string testDbName)
		{
			var sql = @"delete from [{0}]..StmTranslationFeedbackResource
						   delete from [{0}]..StmTranslationFeedback						   
						   insert into [{0}]..StmTranslationFeedback (XT_PK, XT_EnterpriseCode, XT_DatabaseCode, XT_CompanyCode, XT_ClientStaffInitial, XT_Language, XT_Source, XT_OriginalTranslation, XT_SuggestedTranslation, XT_Comments, XT_Status, XT_StatusTime, XT_SystemCreateTimeUtc)
values ('58E30A50-3CDB-474B-95E0-1EAAC5E32221', 'EDI', 'DAT', 'EDI', 'XXX', 'GRM', 'starts with', 'beginnen mit', 'beginnt mit', '', 'NEW', getdate(), getdate())
						   insert into [{0}]..StmTranslationFeedbackResource (XQ_PK, XQ_ResourceStringKey, XQ_ResourceStringLevel, XQ_MatchType, XQ_XT)
values ('4C9B2AAE-7582-49F3-BA1D-86AFCA86CDCB', 'ComparisonConstants|StartsWith', 'CAP', '*', '58E30A50-3CDB-474B-95E0-1EAAC5E32221')";
			connection.ExecuteNonQuery(string.Format(sql, testDbName));
		}

		void AssertTranslationFeedbackData(DbConnection connection, string testDbName)
		{
			const string sql = "select * from [{0}]..StmTranslationFeedback inner join  [{0}]..StmTranslationFeedbackResource on XQ_XT = XT_PK";
			using (var reader = connection.Command(string.Format(sql, testDbName)).ExecuteReader())
			{
				Assert(reader.Read());
				AssertEquals("GRM", reader["XT_Language"] as string);
				AssertEquals("starts with", reader["XT_Source"] as string);
				AssertEquals("beginnen mit", reader["XT_OriginalTranslation"] as string);
				AssertEquals("beginnt mit", reader["XT_SuggestedTranslation"] as string);
				AssertEquals("ComparisonConstants|StartsWith", reader["XQ_ResourceStringKey"] as string);
				AssertEquals("CAP", reader["XQ_ResourceStringLevel"] as string);
				AssertEquals("*", reader["XQ_MatchType"] as string);
			}
		}

		public void TestCopyProductionToNewDatabaseRequiredRegistryItems()
		{
			var messageMask =
				"\r\n\r\nRequired licence registry item [{0}] not found." +
				"\r\nIt might have been removed or changed." +
				"\r\nPlease adjust Copy Production to Test Script.\r\n";
			Assert(string.Format(messageMask, FreightNotesLengthNew), GetStmData(Db.Connection, Db.DatabaseName, FreightNotesLengthNew) != null);
			Assert(string.Format(messageMask, PhysicalServerID), GetStmData(Db.Connection, Db.DatabaseName, PhysicalServerID) != null);
		}

		public void TestBackupFileDoesNotExist()
		{
			var exceptionMessage = "";
			var manager = new DbRestoreManagerForTesting();
			manager.OnTaskFailed += message => exceptionMessage = message;

			var bkpFilePath = @"C:\inexistentFile.bkp";
			var result = manager.GetBackupDbFileInfoCollection(Db.ServerName, bkpFilePath, null, null, null, null);

			AssertEquals("Should show empty result", 0, result.Count);
			Assert(exceptionMessage.Contains("File may not exist or SQL Server may not have permission to access the file."));
		}

		[UseSnapshotProtection]
		public void TestRefDBsAreNotCreatedWhenRestoringDatabasesThatAreNotMainDb()
		{
			// Arrange
			var originalValueBackupRefDb = false;
			var originalValueSkipBackupIfNoActivity = false;
			var logMessages = new List<string>();
			var loggerMock = new Mock<ILog>();
			loggerMock.Setup(x => x.Info(It.IsAny<object>())).Callback((object msg) => logMessages.Add($"[{DateTime.Now:u}]: {msg}"));
			loggerMock.Setup(x => x.Error(It.IsAny<object>())).Callback((object msg) => logMessages.Add($"[{DateTime.Now:u}]: {msg}"));

			var eDocDb = (new DocManagerDBHelper()).GetDatabaseName(9);

			using (var backupFilesDirectory = new TempDirectory())
			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (new DisposableAction(
				() =>
				{
					originalValueBackupRefDb = Env.Registry.BackupReferenceDatabases;
					originalValueSkipBackupIfNoActivity = Env.Registry.SkipBackupOfNonMainDatabasesWithoutActivity;

					Env.Registry.BackupReferenceDatabases = true;
					Env.Registry.SkipBackupOfNonMainDatabasesWithoutActivity = false;
					Logger.Instance.logger = loggerMock.Object;

					new DocManagerDBHelperTestClass().CreateDatabase(9);
				},
				() =>
				{
					Env.Registry.BackupReferenceDatabases = originalValueBackupRefDb;
					Env.Registry.SkipBackupOfNonMainDatabasesWithoutActivity = originalValueSkipBackupIfNoActivity;

					AdoTestUtils.DropDbIfExists(adminConnection, eDocDb);
				}))
			{
				var dbMaintenancecontroller = new DbMaintenanceController(backupFilesDirectory, Mock.Of<IEmailNotificationSender>(), Mock.Of<ILogger>());
				dbMaintenancecontroller.PerformMaintenance();

				var dirInfo = new DirectoryInfo(backupFilesDirectory);
				var backupFiles = dirInfo
					.EnumerateFiles($"{RefDbTableNameResolver.SingleRefDatabaseName}.bak")
					.Append(dirInfo.EnumerateFiles($"{Db.DatabaseName}_SD*.bak").FirstOrDefault())
					.Append(dirInfo.EnumerateFiles($"{Db.DatabaseName}_RefDb_*.bak").FirstOrDefault())
					.Where(x => x != null)
					.Select(x => x.FullName);

				foreach (var backupFilePath in backupFiles)
				{
					var targetDatabaseName = $"{TestDatabaseName}_{Guid.NewGuid():N}";

					using (var targetDir = new TempDirectory())
					using (DropDatabasesStartsWithNameDisposable(adminConnection, TestDatabaseName))
					{
						// Arrange
						var restoreManager = new DbRestoreManager(new DbHeaderOnlyReader());
						var dbFileInfos = restoreManager.GetBackupDbFileInfoCollection(Db.ServerName, backupFilePath, null, null, null, null);
						foreach (DbFileInfo dbFileInfo in dbFileInfos)
						{
							dbFileInfo.FolderPath = targetDir;
						}

						var mainServerConfig = new DbServerConfiguration(Db.ServerName, backupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(targetDatabaseName, dbFileInfos, DbRestoreOption.RestoreWithRecovery);

						// Act
						restoreManager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

						// Assert
						var otherDatabases = GetDatabaseNamesLike(adminConnection, $"{targetDatabaseName}[_]");
						CombineAssertions(() =>
						{
							AssertEquals($"TargetDatabase: {targetDatabaseName} has been created", true, adminConnection.DatabaseExists(targetDatabaseName));
							AssertEquals($"Other unexpected databases created:{System.Environment.NewLine}{string.Join(System.Environment.NewLine, otherDatabases)}", false, otherDatabases.Any());
							AssertEquals(string.Join(System.Environment.NewLine, logMessages), false, logMessages.Any(x => x.Contains("Database restore failed")));
						});
					}
				}
			}
		}

		static IDisposable DropDatabasesStartsWithNameDisposable(AdminConnection adminConnection, string name)
		{
			var script = Invariant($@"
DECLARE @statement VARCHAR(max) = ''

SELECT @statement = @statement  + char(13) + char(10)
+ 'DROP DATABASE ['+ name + ']'
FROM sys.databases WHERE name like '{name}%'
;

exec (@statement)
;
");

			return new DisposableAction(() =>
			{
				adminConnection.ExecuteNonQuery(script);
			});
		}

		static IList<string> GetDatabaseNamesLike(AdminConnection adminConnection, string name)
		{
			var databaseNames = new List<string>();
			adminConnection.ExecuteReader($"SELECT NAME FROM sys.databases WHERE name like '{name}%'", (IDataRecord record) =>
			{
				databaseNames.Add(record[0].ToString());
			});

			return databaseNames;
		}

		public void TestDbLoginsAreRetainedAfterDatabaseRestore()
		{
			const string backupFile = "testProdCopyWithADLogins.bak";
			var staffDbLoginPattern = $"{DataUtils.ReplaceSqlLikeWildcard(DbUserRepository.GetStaffDbLoginFullPrefix(TestDatabaseName))}%";
			var staffDbLoginPrefix = DbUserRepository.GetStaffDbLoginFullPrefix(TestDatabaseName);
			var staffDbLogins = new List<string>
			{
				$"{staffDbLoginPrefix}Staff_1",
				$"{staffDbLoginPrefix}Staff_2",
				$"{staffDbLoginPrefix}Staff_3",
			};

			// Arrange
			using (var tempDir = new TempDirectory())
			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (CreateDbLoginsDisposable(adminConnection, staffDbLogins))
			using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(adminConnection, TestDatabaseName)))
			{
				var targetDataPath = tempDir.DirectoryName;
				// TODO Suspicious code here. The TestBackupFile "testProdCopy.bak" is used, but it is renamed to "testProdCopyWithADLogins.bak".
				// That new name matches another test file, but that test file is not used anywhere else.
				// Is the wrong file being used?
				var restoreFromBackupFilePath = TestDataHelpers.CopyAndNameTestFileResource(tempDir.DirectoryName, TestBackupFile, backupFile);
				var dbFileInfos = new DbFileInfoCollection
				{
					new DbFileInfo("TestProdDbCopy", targetDataPath, DbFileInfo.FileTypeData, restoreFromBackupFilePath),
					new DbFileInfo("TestProdDbCopy_log", targetDataPath, DbFileInfo.FileTypeLog, restoreFromBackupFilePath)
				};

				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, restoreFromBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFileInfos, DbRestoreOption.CopyProdToTest);

				var existingDbPrincipals = GetServerPrincipalsForDatabase(adminConnection, TestDatabaseName);
				var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
				var restoreManagerMock = new Mock<DbRestoreManager>(dbHeaderOnlyReaderMock.Object) { CallBase = true };
				var restoreManager = restoreManagerMock.Object;
				restoreManagerMock
					.Setup(x => x.ApplyChangeAfterRestoreDatabase(It.IsAny<DbConnection>(), It.IsAny<string>()))
					.Callback((DbConnection connection, string dbName) => CreateColumnsIfNotExists(connection, dbName));

				restoreManagerMock.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()));
				restoreManagerMock.Protected().Setup<bool>(
					"IsTargetDatabaseMainDb",
					ItExpr.IsAny<string>()).Returns(true);

				// Act
				restoreManager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				// Assert
				var dbPrincipalsAfterRestore = GetServerPrincipalsForDatabase(adminConnection, TestDatabaseName);
				AssertContainsExactElementsInAnyOrder(existingDbPrincipals, dbPrincipalsAfterRestore);
				restoreManagerMock.Verify(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			}

			#region Helpers

			IDisposable CreateDbLoginsDisposable(AdminConnection adminConnection, List<string> logins, string defaultDatabase = "master")
			{
				logins.ForEach(loginName =>
				{
					var sql = $@"
IF NOT exists(SELECT null FROM sys.server_principals WHERE name = '{loginName}') CREATE LOGIN {loginName.QuoteName()} WITH PASSWORD = '', CHECK_POLICY = OFF, DEFAULT_DATABASE={defaultDatabase.QuoteName()};";
					adminConnection.ExecuteNonQuery(sql);
				});

				return new DisposableAction(() =>
				{
					logins.ForEach(loginName =>
					{
						AdoTestUtils.DropDbLoginIfExists(adminConnection, loginName);
					});
				});
			}

			List<string> GetServerPrincipalsForDatabase(DbConnection connection, string dbName)
			{
				var serverPrincipals = new List<string>();

				var scriptGetServerPrincipalsForDefaultDatabase = $@"
SELECT name
FROM sys.server_principals 
WHERE 1=1
	AND (
		default_database_name = @dbName
		OR name LIKE '{staffDbLoginPattern}'
	);
";
				connection.ExecuteReader(scriptGetServerPrincipalsForDefaultDatabase,
					command =>
					{
						command.AddParameter("@dbName", SqlDbType.VarChar, 128, dbName);
					},
					record =>
					{
						serverPrincipals.Add((string)record["name"]);
					});

				return serverPrincipals;
			}

			#endregion // Helpers
		}

		public void TestConfirmationPromptsAreCalledDuringRestore()
		{
			using (var tempDir = new TempDirectory())
			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(adminConnection, TestDatabaseName)))
			{
				var targetDataPath = tempDir.DirectoryName;
				var restoreFromBackupFilePath = Path.Combine(tempDir.DirectoryName, TestBackupFile);
				var dbFileInfos = new DbFileInfoCollection
				{
					new DbFileInfo("TestProdDbCopy", targetDataPath, DbFileInfo.FileTypeData, restoreFromBackupFilePath),
					new DbFileInfo("TestProdDbCopy_log", targetDataPath, DbFileInfo.FileTypeLog, restoreFromBackupFilePath)
				};

				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, restoreFromBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFileInfos, DbRestoreOption.CopyProdToTest);

				var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
				var restoreManagerMock = new Mock<DbRestoreManager>(dbHeaderOnlyReaderMock.Object) { CallBase = true };
				bool calledConfirmNoBackupOfTestDb = false;
				bool calledConfirmAuditDBExcluded = false;
				restoreManagerMock.Object.OnConfirmationPrompt = (args) =>
				{
					string expectedMessage = "";
					switch (args.PromptType)
					{
						case PromptType.ConfirmNoBackupOfTestDb:
							expectedMessage = "The test database: DbRestoreManagerTest does not have a backup history or backup file, if the copy to test process fails, test database will be lost, please click on 'No' and go to backup tab to make a backup first.";
							calledConfirmNoBackupOfTestDb = true;
							break;
						case PromptType.ConfirmAuditDBExcluded:
							expectedMessage = "Audit database backup was not selected to synchronise with the main database. Are you sure?";
							calledConfirmAuditDBExcluded = true;
							break;
					}
					AssertEquals(expectedMessage, args.PromptMessage);
					args.Result = ConfirmationPromptResult.Yes;
				};
				var restoreManager = restoreManagerMock.Object;
				restoreManager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				restoreManagerMock.Verify(m => m.AuditSkipRestorePrompt(), Times.Once());

				CombineAssertions(() =>
				{
					Assert("Confirmation prompt 'ConfirmNoBackupOfTestDb' was not called.", calledConfirmNoBackupOfTestDb);
					Assert("Confirmation prompt 'ConfirmAuditDBExcluded' was not called.", calledConfirmAuditDBExcluded);
				});
			}
		}

		public void TestRunDbRestoreSubscribers_WithErrorMsg()
		{
			TestRunDbRestoreSubscribers(true);
		}

		public void TestRunDbRestoreSubscribers_WithoutErrorMsg()
		{
			TestRunDbRestoreSubscribers(false);
		}

		public void TestRunDbRestoreSubscribers_FireOnTaskFailed()
		{
			using (var tempDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, TestBackupFile);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var stringWriter = new StringWriter();
				Console.SetOut(stringWriter);

				var manager = new DbRestoreManagerForTesting();
				var restoreSubscriberMock = new Mock<IDbRestoreSubscriber>();
				restoreSubscriberMock.Setup(x => x.ReadableName).Returns("SubscriberMock");
				restoreSubscriberMock.Setup(x => x.Run(It.IsAny<DbConnection>())).Returns(string.Empty)
					.Callback((DbConnection connection) =>
					{
						throw new Exception("Test Exception");
					});

				using (ObjectFactory.Substitute("DbRestoreSubscribers", new List<IDbRestoreSubscriber> { restoreSubscriberMock.Object }))
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(connection, TestDatabaseName)))
				{
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

					var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFiles, DbRestoreOption.RestoreWithRecovery);

					manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
					Assert("Precondition: DB should exist", connection.DatabaseExists(TestDatabaseName));
					Assert("Precondition: DB should be available and online", Utilities.GetDatabaseStatus(connection, TestDatabaseName) == DatabaseStatus.Online);

					restoreSubscriberMock.Verify(x => x.Run(It.IsAny<DbConnection>()), Times.Once);
					AssertContains("Event: Task Failed", stringWriter.ToString());
				}
			}
		}

		void TestRunDbRestoreSubscribers(bool willLogErrorMsg)
		{
			using (var tempDir = new TempDirectory())
			{
				var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, TestBackupFile);

				var dbFiles = new DbFileInfoCollection();
				dbFiles.Add(new DbFileInfo("TestProdDbCopy", tempDir.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath));
				dbFiles.Add(new DbFileInfo("TestProdDbCopy_log", tempDir.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath));

				var stringWriter = new StringWriter();
				Console.SetOut(stringWriter);

				var manager = new DbRestoreManagerForTesting();
				var restoreSubscriberMock = new Mock<IDbRestoreSubscriber>();
				restoreSubscriberMock.Setup(x => x.ReadableName).Returns("SubscriberMock");
				restoreSubscriberMock.Setup(x => x.Run(It.IsAny<DbConnection>())).Returns(willLogErrorMsg ? "Error" : string.Empty)
					.Callback((DbConnection connection) =>
					{
						AssertType<AdminConnection>("Use AdminConnection", connection);
					});

				using (ObjectFactory.Substitute("DbRestoreSubscribers", new List<IDbRestoreSubscriber> { restoreSubscriberMock.Object }))
				using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
				using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(connection, TestDatabaseName)))
				{
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
					dbFiles.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

					var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
					var auditServerConfig = AuditDbServerConfiguration.Empty;
					var edwServerConfig = EdwDbServerConfiguration.Empty;
					var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFiles, DbRestoreOption.RestoreWithRecovery);

					manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
					Assert("Precondition: DB should exist", connection.DatabaseExists(TestDatabaseName));
					Assert("Precondition: DB should be available and online", Utilities.GetDatabaseStatus(connection, TestDatabaseName) == DatabaseStatus.Online);

					restoreSubscriberMock.Verify(x => x.Run(It.IsAny<DbConnection>()), Times.Once);
					AssertEquals(willLogErrorMsg, stringWriter.ToString().Contains("\r\nThere is an error when running database restored task 'SubscriberMock': Error\r\n"));
				}
			}
		}

		public void TestWrongConstructorParams()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new DbRestoreManager(null));
				AssertEquals("dbHeaderOnlyReader", result.ParamName);
			});
		}

		public void TestGetTransactionLogBackupFileListFromDbPath_SortedByBackupLSN()
		{
			// Arrange
			const string databaseName = "DbBackupAndRestoreTest3998EACF2B294F31B4EC6CCC00DDF595";
			using (var tempDir = new TempDirectory())
			{
				var backupFileName = Path.Combine(tempDir.DirectoryName, $"{databaseName}.bak");

				try
				{
					using (var con = Db.NewAdminConnection())
					{
						AdoTestUtils.CreateDbIfNotExists(con, databaseName, DbRecoveryModel.Full);
					}
					using (var connection = Db.NewAdminConnection(Db.ServerName, databaseName))
					{
						connection.ExecuteNonQuery("CREATE TABLE t1 (c1 int not null)");
						connection.ExecuteNonQuery($"BACKUP DATABASE [{databaseName}] TO DISK = N'{backupFileName}' WITH INIT");
						var expectedFilesList = new[] { CommitTransactionAndGenerateLog(2), CommitTransactionAndGenerateLog(3), CommitTransactionAndGenerateLog(1), };

						string CommitTransactionAndGenerateLog(int fileNumber)
						{
							var logBackupFileName = $"{databaseName}_{fileNumber}.{Utilities.TransactionLogBackupFileExtension}";
							var logBackupFilePath = Path.Combine(tempDir.DirectoryName, logBackupFileName);
							connection.ExecuteNonQuery($"INSERT INTO t1 values ({fileNumber})");
							connection.ExecuteNonQuery($"BACKUP LOG [{databaseName}] TO DISK = N'{logBackupFilePath}' WITH INIT");

							return logBackupFileName;
						}

						// Act
						var restoreManager = new DbRestoreManager(new DbHeaderOnlyReader());
						var files = restoreManager.GetTransactionLogBackupFileListFromDbPath(connection, tempDir);
						var actualFilesList = files.Select(Path.GetFileName).ToArray();

						// Assert
						AssertArrayEqualsByElements(expectedFilesList, actualFilesList);
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(Db.NewAdminConnection(), databaseName);
				}
			}
		}

		public void TestDatabaseNameAndServerName_AreInitialized_EnsureDbLoginsCorrectlyMappedToAllDatabases()
		{
			// Arrange
			var stringBuilder = new StringBuilder();

			using var adminConnection = Db.NewAdminConnection();
			adminConnection.Logins.ToList().ForEach(x => x.EnsureLogin());

			var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
			var restoreManager = new DbRestoreManager(dbHeaderOnlyReaderMock.Object);

			var loggerMock = new Mock<ILog>();
			_ = loggerMock.Setup(x => x.Info(It.IsAny<object>())).Callback((object message) => stringBuilder.AppendLine(message.ToString()));
			Logger.Instance.logger = loggerMock.Object;

			// Act
			using (AdoTestUtils.CreateDbDropExistingDisposable(TestDatabaseName))
			{
				restoreManager.EnsureApplicationLoginHasRightsToRestoredDatabases(Db.ServerName, TestDatabaseName);
			}

			// Assert
			Assert(stringBuilder.ToString(), string.IsNullOrEmpty(stringBuilder.ToString()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")]   // WI00669071 - Do not use System.AppDomain.
		public void TestDatabaseNameAndServerName_AreInitialized_SynchroniseSynonyms()
		{
			using (var appDomainWrapper = new AppDomainWrapper(nameof(DbRestoreManagerTest)))
			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				adminConnection.Logins.ToList().ForEach(x => x.EnsureLogin());

				using (AdoTestUtils.CreateDbDropExistingDisposable(TestDatabaseName))
				using (new DisposableAction(() =>
				{
					new DbMaintenanceManager().DropDatabases(Db.ServerName, TestDatabaseName, false, true, false);
				}))
				{
					var domainData = new Dictionary<string, object>
					{
						{ "ServerName", Db.ServerName },
						{ "DatabaseName", TestDatabaseName }
					};

					var domainReturnData = new Dictionary<string, object>
					{
						{ "LogMessages", null }
					};

					appDomainWrapper.RunActionInAppDomain(() =>
					{
						// Arrange
						var serverName = (string)AppDomain.CurrentDomain.GetData("ServerName");
						var databaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
						var stringBuilder = new StringBuilder();

						var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
						var restoreManager = new DbRestoreManager(dbHeaderOnlyReaderMock.Object);

						var loggerMock = new Mock<ILog>();
						loggerMock.Setup(x => x.Info(It.IsAny<object>())).Callback((object message) =>
							stringBuilder.AppendLine(message.ToString()));
						Logger.Instance.logger = loggerMock.Object;

						// Act
						restoreManager.SynchroniseSynonyms(serverName, databaseName);
						AppDomain.CurrentDomain.SetData("LogMessages", stringBuilder.ToString());
					}, domainData, domainReturnData);

					// Assert
					var messages = (string)domainReturnData["LogMessages"];
					Assert(messages, string.IsNullOrEmpty(messages));
					var referenceDatabases = GetReferenceDatabases(adminConnection, TestDatabaseName);
					Assert("Reference Databases should be created", referenceDatabases.Any());
				}
			}
		}

		IEnumerable<string> GetReferenceDatabases(DbConnection connection, string mainDatabaseName)
		{
			var sqlText = string.Format(@"SELECT name FROM sys.databases WHERE  name like '{0}[_]RefDb[_]___[_]__'", mainDatabaseName);
			return DataUtils.GetListOfValuesFromQuery(connection, sqlText);
		}

		public void TestRegistryItems_ArePreserved_AfterCopyProductionToTest()
		{
			var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
			var restoreManagerMock = new Mock<DbRestoreManager>(dbHeaderOnlyReaderMock.Object) { CallBase = true };
			var restoreManager = restoreManagerMock.Object;
			restoreManagerMock
				.Setup(x => x.ApplyChangeAfterRestoreDatabase(It.IsAny<DbConnection>(), It.IsAny<string>()))
				.Callback((DbConnection connection, string dbName) => CreateColumnsIfNotExists(connection, dbName));
			restoreManagerMock.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()));
			restoreManagerMock.Protected().Setup<bool>(
				"IsTargetDatabaseMainDb",
				ItExpr.IsAny<string>()).Returns(true);

			using (var tempDir = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(connection, TestDatabaseName)))
			{
				var targetDataPath = tempDir.DirectoryName;
				var restoreFromBackupFilePath = TestDataHelpers.CopyTestFileResource(targetDataPath, TestBackupFile);
				var dbFileInfos = new DbFileInfoCollection
				{
					new DbFileInfo("TestProdDbCopy", targetDataPath, DbFileInfo.FileTypeData, restoreFromBackupFilePath),
					new DbFileInfo("TestProdDbCopy_log", targetDataPath, DbFileInfo.FileTypeLog, restoreFromBackupFilePath)
				};

				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, restoreFromBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFileInfos, DbRestoreOption.CopyProdToTest);

				// Arrange
				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				Assert($"{TestDatabaseName} has been restored once as existing test db", Utilities.DatabaseExists(Db.Connection, TestDatabaseName));

				var existRegItems = TestDataHelpers.GetStmData(connection, TestDatabaseName);
				var preConditionedRegItems = TestDataHelpers.PreconditionRegistryItems(connection, TestDatabaseName, existRegItems.Keys);
				var toBeRestoredRegItems = existRegItems.Where(x => preConditionedRegItems.Any(item => item.Key == x.Key && !item.Value));
				TestDataHelpers.AssertPreservedRegistryItems(connection, TestDatabaseName, preConditionedRegItems);

				TestDataHelpers.InsertStmData(connection, TestDatabaseName, "DATABASE_SCHEMA_VERSION", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), true);
				TestDataHelpers.SetDatabaseAsForTest(connection, TestDatabaseName);
				Assert($"{TestDatabaseName} must be set IsTestOrTrainingDb", Utilities.IsTestOrTrainingDb(connection, TestDatabaseName));
				restoreManagerMock.Invocations.Clear();

				// Act				
				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				// Assert
				TestDataHelpers.AssertPreservedRegistryItems(connection, TestDatabaseName, preConditionedRegItems);
				restoreManagerMock.Verify(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			}
		}

		public void TestIntermediate_DataVaultDb_IsDropped()
		{
			// Arrange
			var mainServerConfig = new DbServerConfiguration(Db.ServerName, string.Empty);
			var auditServerConfig = AuditDbServerConfiguration.Empty;
			var edwServerConfig = EdwDbServerConfiguration.Empty;
			var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, new DbFileInfoCollection(), DbRestoreOption.CopyProdToTest);

			var dataVaultDbName = string.Empty;
			var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
			var restoreManagerMock = new Mock<DbRestoreManager>(dbHeaderOnlyReaderMock.Object) { CallBase = true };
			restoreManagerMock.Protected().Setup(
					"PerformCopyProductionToTestPreRestoreSteps",
					ItExpr.IsAny<AdminConnection>(), // AdminConnection
					ItExpr.IsAny<DbRestoreSettings>(), // dbRestoreSettings
					ItExpr.IsAny<string>(), // vaultAuxDbName
					ItExpr.IsAny<bool>() // isNewTestDb
				).Callback((AdminConnection connection, DbRestoreSettings dbRestoreSettings, string vaultAuxDbName, bool isNewTestDb) =>
				{
					dataVaultDbName = vaultAuxDbName;
					AdoTestUtils.CreateDbIfNotExists(connection, dataVaultDbName);
				}).Verifiable();
			restoreManagerMock.Protected().Setup(
					"DoRestore",
					ItExpr.IsAny<AdminConnection>(),
					ItExpr.IsAny<DbServerConfiguration>(),
					ItExpr.IsAny<AuditDbServerConfiguration>(),
					ItExpr.IsAny<EdwDbServerConfiguration>(),
					ItExpr.IsAny<DbRestoreSettings>()
				).Verifiable();
			restoreManagerMock.Protected().Setup(
					"PerformCopyProductionToTestPostRestoreSteps",
					ItExpr.IsAny<AdminConnection>(), // AdminConnection
					ItExpr.IsAny<DbRestoreSettings>(), // dbRestoreSettings
					ItExpr.IsAny<string>(), // vaultAuxDbName
					ItExpr.IsAny<bool>() // isNewTestDb
				).Verifiable();

			try
			{
				// Act
				var restoreManager = restoreManagerMock.Object;
				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				// Assert	
				Assert(!string.IsNullOrEmpty(dataVaultDbName));
				AssertEquals($"{TestDatabaseName}-DataVault", dataVaultDbName);
				Assert(
					$"{dataVaultDbName} must be dropped at the end of restore process.",
					!Utilities.DatabaseExists(Db.Connection, dataVaultDbName));
			}
			finally
			{
				if (!string.IsNullOrEmpty(dataVaultDbName))
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						AdoTestUtils.DropDbIfExists(adminConnection, dataVaultDbName);
					}
				}
			}
		}

		public class TestTrnRestoreCreatesDatabaseAndReportsSuccessEvenIfThereIsLockedTrn : TestCase
		{
			readonly string[] trnFilesBeforeBak = new[] { "TestOdysseyRestoreTrn_00000000000001.trn", "TestOdysseyRestoreTrn_00000000000002.trn" };
			readonly string[] trnFilesAfterBak = new[] { "TestOdysseyRestoreTrn_00000000000003.trn", "TestOdysseyRestoreTrn_00000000000004.trn" };
			const string testBackupFileName = "TestOdysseyRestoreTrn.bak";
			const string databaseNameRestored = "DbBackupAndRestoreTest_2113C7CFC24A434E902BFB8613EE0EDB";

			string testLocalBackupFilePath;
			TempDirectory tempDirectory;
			List<string> messages;
			DbRestoreManager restoreManager;
			DbFileInfoCollection dbFileInfos;

			protected override void SetUp()
			{
				base.SetUp();
				tempDirectory = new TempDirectory();
				testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDirectory.DirectoryName, testBackupFileName);

				foreach (var trnFile in trnFilesBeforeBak)
				{
					TestDataHelpers.CopyTestFileResource(tempDirectory.DirectoryName, trnFile);
				}

				foreach (var trnFile in trnFilesAfterBak)
				{
					TestDataHelpers.CopyTestFileResource(tempDirectory.DirectoryName, trnFile);
				}

				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, TestDatabaseName);
				}
				messages = new List<string>();
				restoreManager = new DbRestoreManager(new DbHeaderOnlyReader());

				restoreManager.OnShowInfoMessage += new InformationEvent((message) => messages.Add(message));
				restoreManager.OnTaskCompleted += new InformationEvent((message) => messages.Add(message));
				restoreManager.OnTaskFailed += new InformationEvent((message) => messages.Add(message));

				dbFileInfos = new DbFileInfoCollection
				{
					new DbFileInfo("Odyssey_Data", tempDirectory.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath),
					new DbFileInfo("Odyssey_Data02", tempDirectory.DirectoryName, DbFileInfo.FileTypeData, testLocalBackupFilePath),
					new DbFileInfo("Odyssey_Log", tempDirectory.DirectoryName, DbFileInfo.FileTypeLog, testLocalBackupFilePath),
				};
			}

			protected override void TearDown()
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, databaseNameRestored);
				}
				tempDirectory.Dispose();
			}

			public void TestReportsLockedFileWithUnlockedTrnBeforeBakAndLockedLastTrnAfterBak()
			{
				// Arrange
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(databaseNameRestored, dbFileInfos, DbRestoreOption.RestoreWithNoRecovery, restoreTransactionLogBackups: true);
				var lockedLastTrnAfterBak = trnFilesAfterBak[1];

				// Act
				using (var file = File.Open(Path.Combine(tempDirectory.DirectoryName, lockedLastTrnAfterBak), FileMode.Open, FileAccess.Read, FileShare.None))
				{
					restoreManager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				}

				// Assert
				CombineAssertions(string.Join(System.Environment.NewLine, messages), () =>
				{
					AssertCollectionContains(messages, message => message.StartsWith("Database restore with restore option RestoreWithNoRecovery completed with success", StringComparison.OrdinalIgnoreCase));
					AssertEquals(true, Utilities.DatabaseExists(Db.NewAdminConnection(), databaseNameRestored));
					AssertCollectionContains(messages, message => message.StartsWith($"Will not attempt to restore '{lockedLastTrnAfterBak}' as it cannot be read.", StringComparison.OrdinalIgnoreCase));
				});
			}

			public void TestReportsLockedFileAndAndRestoreErrorWithLockedFirstTrnAfterBak()
			{
				// Arrange
				var lockedFirstTrnAfterBak = trnFilesAfterBak[0];
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(databaseNameRestored, dbFileInfos, DbRestoreOption.RestoreWithNoRecovery, restoreTransactionLogBackups: true);

				// Act
				using (var file = File.Open(Path.Combine(tempDirectory.DirectoryName, lockedFirstTrnAfterBak), FileMode.Open, FileAccess.Read, FileShare.None))
				{
					restoreManager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				}

				// Assert
				CombineAssertions(string.Join(System.Environment.NewLine, messages), () =>
				{
					AssertCollectionContains(messages, message => message.StartsWith("Database restore with restore option RestoreWithNoRecovery completed with success", StringComparison.OrdinalIgnoreCase));
					AssertEquals(true, Utilities.DatabaseExists(Db.NewAdminConnection(), databaseNameRestored));
					AssertCollectionContains(messages, message => message.StartsWith($"Error restoring transaction log backup '{trnFilesAfterBak[1]}'", StringComparison.OrdinalIgnoreCase));
					AssertCollectionContains(messages, message => message.StartsWith($"Will not attempt to restore '{lockedFirstTrnAfterBak}' as it cannot be read.", StringComparison.OrdinalIgnoreCase));
				});
			}

			public void TestReportsLockedFileWithLockedTrnBeforeBakAndUnlockedTrnsAfterBak()
			{
				// Arrange
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(databaseNameRestored, dbFileInfos, DbRestoreOption.RestoreWithNoRecovery, restoreTransactionLogBackups: true);
				var lockedTrnBeforeBak = trnFilesBeforeBak[1];

				// Act
				using (var file = File.Open(Path.Combine(tempDirectory.DirectoryName, lockedTrnBeforeBak), FileMode.Open, FileAccess.Read, FileShare.None))
				{
					restoreManager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				}

				// Assert
				CombineAssertions(string.Join(System.Environment.NewLine, messages), () =>
				{
					AssertCollectionContains(messages, message => message.StartsWith("Database restore with restore option RestoreWithNoRecovery completed with success", StringComparison.OrdinalIgnoreCase));
					AssertEquals(true, Utilities.DatabaseExists(Db.NewAdminConnection(), databaseNameRestored));
					AssertCollectionContains(messages, message => message.StartsWith($"Will not attempt to restore '{lockedTrnBeforeBak}' as it cannot be read.", StringComparison.OrdinalIgnoreCase));
				});
			}

			public void TestReportsLockedFileWithLockedTrnBeforeBakAndNoTrnsAfterBak()
			{
				// Arrange
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(databaseNameRestored, dbFileInfos, DbRestoreOption.RestoreWithNoRecovery, restoreTransactionLogBackups: true);
				var lockedTrnBeforeBak = trnFilesBeforeBak[1];

				foreach (var trnFile in trnFilesAfterBak)
				{
					File.Delete(Path.Combine(tempDirectory.DirectoryName, trnFile));
				}

				// Act
				using (var file = File.Open(Path.Combine(tempDirectory.DirectoryName, lockedTrnBeforeBak), FileMode.Open, FileAccess.Read, FileShare.None))
				{
					restoreManager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				}

				// Assert
				CombineAssertions(string.Join(System.Environment.NewLine, messages), () =>
				{
					AssertCollectionContains(messages, message => message.StartsWith("Database restore with restore option RestoreWithNoRecovery completed with success", StringComparison.OrdinalIgnoreCase));
					AssertEquals(true, Utilities.DatabaseExists(Db.NewAdminConnection(), databaseNameRestored));
					AssertCollectionContains(messages, message => message.StartsWith($"Will not attempt to restore '{lockedTrnBeforeBak}' as it cannot be read.", StringComparison.OrdinalIgnoreCase));
				});
			}
		}

		[UseSnapshotProtection]
		public void TestRecoveryModelIsAdjustedForRestoredDatabases()
		{
			// Arrange
			const string mainDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			const string eDocsDbName = mainDbName + "_SD001";
			const string refDbName = mainDbName + "_RefDb_Trf_CA";

			using var tempDir = new TempDirectory();

			var testMainDbBackUpFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription.bak");
			_ = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription_SD001.bak");
			_ = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription_RefDb_Trf_CA.bak");

			var mockManager = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>()) { CallBase = true };
			var manager = mockManager.Object;

			mockManager
				.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()))
				.Callback(() => { });

			var dbFiles = manager.GetBackupDbFileInfoCollection(Db.ServerName, testMainDbBackUpFilePath, null, null, null, null);

			var mainServerConfig = new DbServerConfiguration(Db.ServerName, testMainDbBackUpFilePath, tempDir, tempDir);
			var auditServerConfig = AuditDbServerConfiguration.Empty;
			var edwServerConfig = EdwDbServerConfiguration.Empty;
			var dbRestoreSettings = new DbRestoreSettings(mainDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (AdoTestUtils.DropDbIfExistsDisposable(mainDbName))
			using (AdoTestUtils.DropDbIfExistsDisposable(eDocsDbName))
			using (AdoTestUtils.DropDbIfExistsDisposable(refDbName))
			{
				// Act
				manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				// Assert
				CombineAssertions(() =>
				{
					Assert("DatabaseExists(testDbName)", connection.DatabaseExists(mainDbName));
					Assert("DatabaseExists(testDbName_SD001)", connection.DatabaseExists(eDocsDbName));
					Assert("DatabaseExists(testDbName_RefDb_Trf_CA)", connection.DatabaseExists(refDbName));

					dbRecoverModelManagerMock.Verify(r => r.AdjustDatabase(It.IsAny<DbConnection>(), mainDbName, mainDbName, It.IsAny<string>()));
					dbRecoverModelManagerMock.Verify(r => r.AdjustDatabase(It.IsAny<DbConnection>(), mainDbName, eDocsDbName, It.IsAny<string>()));
					dbRecoverModelManagerMock.Verify(r => r.AdjustDatabase(It.IsAny<DbConnection>(), mainDbName, refDbName, It.IsAny<string>()));
				});
			}
		}

		[UseSnapshotProtection]
		public void TestRestoreDatabasesForMainDb()
		{
			CombineAssertions(() =>
			{
				AssertRestoreDatabasesForMainDb("chocolate");
				AssertRestoreDatabasesForMainDb("yoghurtAuditAuditAuditAuditAudit");
				AssertRestoreDatabasesForMainDb("OdysseyMainDb");
			});

			void AssertRestoreDatabasesForMainDb(string mainDbName)
			{
				// Arrange
				using var tempDir = new TempDirectory();

				var auditDbName = mainDbName + Db.AuditDatabaseSuffix;
				var edwDbName = mainDbName + Db.EdwDatabaseSuffix;
				var eDocsDbName = mainDbName + "_SD001";
				var refDbName = mainDbName + "_RefDb_Trf_CA";

				var mainDbBackupFilePath = CreateTemporaryDatabaseBackup(tempDir, "TestIsTargetDatabaseMainDb.bak");
				var auditDbBackupFilePath = CreateTemporaryDatabaseBackup(tempDir, "TestIsTargetDatabaseMainDb_Audit.bak");
				var edwDbBackupFilePath = CreateTemporaryDatabaseBackup(tempDir, "TestIsTargetDatabaseMainDb_EDW.bak");
				TestDataHelpers.CopyAndNameTestFileResource(tempDir, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription_SD001.bak", "TestIsTargetDatabaseMainDb_SD001.bak");
				TestDataHelpers.CopyAndNameTestFileResource(tempDir, "CW1Bkp_T-20230630-114910_S-SYDCO-LMZT-1_D-OdysseyNoDescription_RefDb_Trf_CA.bak", "TestIsTargetDatabaseMainDb_RefDb_Trf_CA.bak");

				var mockManager = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>()) { CallBase = true };
				var mockRestoreSubscriber = new Mock<IDbRestoreSubscriber>();
				var manager = mockManager.Object;

				mockManager
					.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()))
					.Callback(() => { });

				mockRestoreSubscriber
					.Setup(m => m.Run(It.IsAny<DbConnection>()))
					.Verifiable();

				var dbFiles = manager.GetBackupDbFileInfoCollection(Db.ServerName, mainDbBackupFilePath, Db.ServerName, auditDbBackupFilePath, Db.ServerName, edwDbBackupFilePath);
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, mainDbBackupFilePath, tempDir, tempDir);
				var auditServerConfig = new AuditDbServerConfiguration(Db.ServerName, auditDbBackupFilePath, tempDir, tempDir);
				var edwServerConfig = new EdwDbServerConfiguration(Db.ServerName, edwDbBackupFilePath, tempDir, tempDir);
				var dbRestoreSettings = new DbRestoreSettings(mainDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

				using (ObjectFactory.Substitute("DbRestoreSubscribers", new List<IDbRestoreSubscriber> { mockRestoreSubscriber.Object }))
				using (AdoTestUtils.DropDbIfExistsDisposable(mainDbName))
				using (AdoTestUtils.DropDbIfExistsDisposable(auditDbName))
				using (AdoTestUtils.DropDbIfExistsDisposable(edwDbName))
				using (AdoTestUtils.DropDbIfExistsDisposable(eDocsDbName))
				using (AdoTestUtils.DropDbIfExistsDisposable(refDbName))
				{
					// Act
					manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

					// Assert
					AssertEquals($"IsTargetDatabaseMainDb({mainDbName})", expected: true, manager.IsTargetDatabaseMainDb(mainDbName));
					AssertEquals($"IsTargetDatabaseMainDb({auditDbName})", expected: false, manager.IsTargetDatabaseMainDb(auditDbName));
					AssertEquals($"IsTargetDatabaseMainDb({edwDbName})", expected: false, manager.IsTargetDatabaseMainDb(edwDbName));
					AssertEquals($"IsTargetDatabaseMainDb({eDocsDbName})", expected: false, manager.IsTargetDatabaseMainDb(eDocsDbName));
					AssertEquals($"IsTargetDatabaseMainDb({refDbName})", expected: false, manager.IsTargetDatabaseMainDb(refDbName));

					mockRestoreSubscriber.Verify(m => m.Run(It.IsAny<DbConnection>()), Times.Once, "Restore subscribers should be called exactly once");
				}
			}
		}

		public void TestRestoreDatabasesForNotMainDb()
		{
			CombineAssertions(() =>
			{
				AssertRestoreDatabasesForNotMainDb("TestRestoreDatabasesForNotMainDb_Audit");
				AssertRestoreDatabasesForNotMainDb("TestRestoreDatabasesForNotMainDb_EDW");
				AssertRestoreDatabasesForNotMainDb("TestRestoreDatabasesForNotMainDb_SD001");
				AssertRestoreDatabasesForNotMainDb("TestRestoreDatabasesForNotMainDb_RefDb_Trf_CA");
				AssertRestoreDatabasesForNotMainDb("TestRestoreDatabasesForNotMainDb_UserRepository");
			});

			void AssertRestoreDatabasesForNotMainDb(string dbName)
			{
				// Arrange
				using var tempDir = new TempDirectory();
				var bakFilePath = CreateTemporaryDatabaseBackup(tempDir, dbName + ".bak");

				var mockManager = new Mock<DbRestoreManager>(Mock.Of<IDbHeaderOnlyReader>()) { CallBase = true };
				var mockRestoreSubscriber = new Mock<IDbRestoreSubscriber>();
				var manager = mockManager.Object;

				mockManager
					.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()))
					.Callback(() => { });

				mockRestoreSubscriber
					.Setup(m => m.Run(It.IsAny<DbConnection>()))
					.Returns(string.Empty)
					.Callback(() => Fail($"Restore subscribers should not have been called on {dbName} because it is not a main Odyssey database"));

				var dbFiles = manager.GetBackupDbFileInfoCollection(Db.ServerName, bakFilePath, string.Empty, string.Empty, string.Empty, string.Empty);
				var mainServerConfig = new DbServerConfiguration(Db.ServerName, null, tempDir, tempDir);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(dbName, dbFiles, DbRestoreOption.RestoreWithRecovery, restoreOperationalDatabases: false);

				using (ObjectFactory.Substitute("DbRestoreSubscribers", new List<IDbRestoreSubscriber> { mockRestoreSubscriber.Object }))
				using (AdoTestUtils.DropDbIfExistsDisposable(dbName))
				{
					// Act
					manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

					// Assert
					AssertEquals($"IsTargetDatabaseMainDb({dbName})", expected: false, manager.IsTargetDatabaseMainDb(dbName));
				}
			}
		}

		public void TestIsTargetDatabaseMainDbForMainDb()
		{
			CombineAssertions(() =>
			{
				AssertIsTargetDatabaseMainDb("OtherRandomOdysseyDatabase", true);
				AssertIsTargetDatabaseMainDb("PokemonCollectionOdyssey", true);
				AssertIsTargetDatabaseMainDb("ThisOneIsAlsoOdyssey", true);
				AssertIsTargetDatabaseMainDb("OdysseyEdw", true);
				AssertIsTargetDatabaseMainDb("OdysseyWISGLOSYD", true);
				AssertIsTargetDatabaseMainDb("AnotherOdysseyDb", true);
			});
		}

		public void TestIsTargetDatabaseMainDbForNotMainDb()
		{
			CombineAssertions(() =>
			{
				AssertIsTargetDatabaseMainDb("OtherRandomNonMainDatabase_EDW", false);
				AssertIsTargetDatabaseMainDb("PokemonCollection_SD001", false);
				AssertIsTargetDatabaseMainDb("NotSchemaAuditDb_Audit", false);
				AssertIsTargetDatabaseMainDb("OdysseyEdw_UserRepository", false);
				AssertIsTargetDatabaseMainDb("CW-RefDb-Cmr-AU-000015", false);
			});
		}

		public void TestIsTargetDatabaseMainDbForExistingOdysseyDatabases()
		{
			CombineAssertions(() =>
			{
				AssertIsTargetDatabaseMainDbForExistingOdysseyDatabases(Db.DatabaseName, true);
				AssertIsTargetDatabaseMainDbForExistingOdysseyDatabases(Db.AuditDatabaseName, false);
				AssertIsTargetDatabaseMainDbForExistingOdysseyDatabases(Db.EdwDatabaseName, false);
				AssertIsTargetDatabaseMainDbForExistingOdysseyDatabases(RefDbTableNameResolver.SingleRefDatabaseName, false);
			});

			static void AssertIsTargetDatabaseMainDbForExistingOdysseyDatabases(string existingDbName, bool expectedIsMainDb)
			{
				// Arrange
				var manager = new DbRestoreManager(Mock.Of<IDbHeaderOnlyReader>());

				// Act & Assert
				AssertEquals($"IsTargetDatabaseMainDb({existingDbName})", expectedIsMainDb, manager.IsTargetDatabaseMainDb(existingDbName));
			}
		}

		void AssertIsTargetDatabaseMainDb(string dbName, bool expectedIsMainDb)
		{
			// Arrange
			var manager = new DbRestoreManager(Mock.Of<IDbHeaderOnlyReader>());

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName, Db.DatabaseName))
			{
				// Act & Assert
				AssertEquals($"IsTargetDatabaseMainDb({dbName})", expectedIsMainDb, manager.IsTargetDatabaseMainDb(dbName));
			}
		}

		string CreateTemporaryDatabaseBackup(string backupFolder, string backupFileName)
		{
			var backupFilePath = Path.Combine(backupFolder, backupFileName);
			var dbName = "TempDb" + Guid.NewGuid().ToString("N");
			var dbNameSafe = dbName.QuoteName();

			var sqlToBackupAndDropDb = @$"
BACKUP DATABASE {dbNameSafe} TO DISK = '{backupFilePath}'
DROP DATABASE {dbNameSafe}
";

			using (var connection = Db.NewAdminConnection())
			{
				connection.CreateDatabase(dbName);
				connection.ExecuteNonQuery(sqlToBackupAndDropDb);
			}

			return backupFilePath;
		}

		public void TestRestore_CreatesLoopbackLinkedServer()
		{
			// Arrange
			const string testDbName = "TestTargetDb";
			const string testSDDbName = "TestTargetDb_SD001";
			var llsuMock = new Mock<ServerConfigurationUtils>();
			llsuMock.Setup(x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.Is(testDbName, StringComparer.OrdinalIgnoreCase)));

			var manager = new DbRestoreManagerForTesting(isTargetDbMainDb: true);
			manager.serverConfigurationUtils = llsuMock.Object;

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				using (var tempDir = new TempDirectory())
				{
					try
					{
						const string testBackUpFileName = "DatabaseRestoreTest.bak";
						const string testSDBackUpFileName = "DatabaseRestoreTest_SD001.bak";
						var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);
						TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testSDBackUpFileName);

						var dbFiles = TestDataHelpers.GetAllFilesFromBackupWithTargetDirectory(manager, testLocalBackupFilePath, tempDir.DirectoryName);
						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithRecovery);

						// Act
						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
						dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testSDDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}

			// Assert
			llsuMock.Verify(x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.Is(testDbName, StringComparer.OrdinalIgnoreCase)), Times.Once);
			llsuMock.Verify(x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.Is(testSDDbName, StringComparer.OrdinalIgnoreCase)), Times.Never);
		}

		public void TestRestore_DoesNotCreateLoopbackLinkedServerOnNonMainDb()
		{
			// Arrange
			const string testDbName = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504";
			var llsuMock = new Mock<ServerConfigurationUtils>();

			var manager = new DbRestoreManagerForTesting(isTargetDbMainDb: false);
			manager.serverConfigurationUtils = llsuMock.Object;

			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				using (var tempDir = new TempDirectory())
				{
					try
					{
						const string testBackUpFileName = "testRestore.bak";
						var testLocalBackupFilePath = TestDataHelpers.CopyTestFileResource(tempDir.DirectoryName, testBackUpFileName);

						var dbFiles = TestDataHelpers.GetAllFilesFromBackupWithTargetDirectory(manager, testLocalBackupFilePath, tempDir.DirectoryName);
						var mainServerConfig = new DbServerConfiguration(Db.ServerName, testLocalBackupFilePath);
						var auditServerConfig = AuditDbServerConfiguration.Empty;
						var edwServerConfig = EdwDbServerConfiguration.Empty;
						var dbRestoreSettings = new DbRestoreSettings(testDbName, dbFiles, DbRestoreOption.RestoreWithNoRecovery);

						// Act
						manager.RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
					}
					finally
					{
						var dropCommand = CopyProductionToTestScriptManager.GetDropTemporaryDbScript(testDbName);
						connection.ExecuteNonQuery(dropCommand);
					}
				}
			}

			// Assert
			llsuMock.Verify(
				x => x.ConfigureServerFromDatabase(It.IsAny<AdminConnection>(), It.IsAny<string>()),
				Times.Never, "ConfigureServerFromDatabase should not be called when isTargetDbMainDb is false."
			);
		}

		public void TestRestore_CopyProdToTest_NotHostedWithCargoWise_DoesNotInvokeAWS()
		{
			var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
			var restoreManagerMock = new Mock<DbRestoreManager>(dbHeaderOnlyReaderMock.Object) { CallBase = true };
			var restoreManager = restoreManagerMock.Object;
			restoreManagerMock
				.Setup(x => x.ApplyChangeAfterRestoreDatabase(It.IsAny<DbConnection>(), It.IsAny<string>()))
				.Callback((DbConnection connection, string dbName) =>
				{
					CreateColumnsIfNotExists(connection, dbName);
					TestDataHelpers.RecreateSysRegistrationKey((AdminConnection)connection, TestDatabaseName, Core.Constants.LicenceConstants.NotHostedWithCargoWise);
				});
			restoreManagerMock.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()));
			restoreManagerMock.Protected().Setup<bool>(
				"IsTargetDatabaseMainDb",
				ItExpr.IsAny<string>()).Returns(true);

			var awsHelperMock = new Mock<IAWSHelper>();
			restoreManager.AWSHelper = awsHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(connection, TestDatabaseName)))
			{
				var targetDataPath = tempDir.DirectoryName;
				var restoreFromBackupFilePath = TestDataHelpers.CopyTestFileResource(targetDataPath, TestBackupFile);
				var dbFileInfos = new DbFileInfoCollection
				{
					new DbFileInfo("TestProdDbCopy", targetDataPath, DbFileInfo.FileTypeData, restoreFromBackupFilePath),
					new DbFileInfo("TestProdDbCopy_log", targetDataPath, DbFileInfo.FileTypeLog, restoreFromBackupFilePath)
				};

				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, restoreFromBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFileInfos, DbRestoreOption.CopyProdToTest);

				// Arrange
				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				Assert($"{TestDatabaseName} has been restored once as existing test db", Utilities.DatabaseExists(Db.Connection, TestDatabaseName));

				var existRegItems = TestDataHelpers.GetStmData(connection, TestDatabaseName);
				TestDataHelpers.PreconditionRegistryItems(connection, TestDatabaseName, existRegItems.Keys);
				TestDataHelpers.InsertStmData(connection, TestDatabaseName, "DATABASE_SCHEMA_VERSION", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), true);
				TestDataHelpers.SetDatabaseAsForTest(connection, TestDatabaseName);
				Assert($"{TestDatabaseName} must be set IsTestOrTrainingDb", Utilities.IsTestOrTrainingDb(connection, TestDatabaseName));
				restoreManagerMock.Invocations.Clear();
				TestDataHelpers.InsertStmData(connection, TestDatabaseName, "EDocsStorageProvider", "S3", true);

				// Act
				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				// Assert
				awsHelperMock.Verify(m => m.GetReadOnlyAccessKeysWithAssumeRole(It.IsAny<string>()), Times.Never);
				AssertEquals("S3", GetStmData(connection, TestDatabaseName, "EDocsStorageProvider"));
			}
		}

		public void TestRestore_CopyProdToTest_NotUsingS3ForEDocs_DoesNotInvokeAWS()
		{
			var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
			var restoreManagerMock = new Mock<DbRestoreManager>(dbHeaderOnlyReaderMock.Object) { CallBase = true };
			var restoreManager = restoreManagerMock.Object;
			restoreManagerMock
				.Setup(x => x.ApplyChangeAfterRestoreDatabase(It.IsAny<DbConnection>(), It.IsAny<string>()))
				.Callback((DbConnection connection, string dbName) =>
				{
					CreateColumnsIfNotExists(connection, dbName);
					TestDataHelpers.InsertStmData(connection, TestDatabaseName, "EDocsStorageProvider", "DB");
				});
			restoreManagerMock.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()));
			restoreManagerMock.Protected().Setup<bool>(
				"IsTargetDatabaseMainDb",
				ItExpr.IsAny<string>()).Returns(true);

			var awsHelperMock = new Mock<IAWSHelper>();
			restoreManager.AWSHelper = awsHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(connection, TestDatabaseName)))
			{
				var targetDataPath = tempDir.DirectoryName;
				var restoreFromBackupFilePath = TestDataHelpers.CopyTestFileResource(targetDataPath, TestBackupFile);
				var dbFileInfos = new DbFileInfoCollection
				{
					new DbFileInfo("TestProdDbCopy", targetDataPath, DbFileInfo.FileTypeData, restoreFromBackupFilePath),
					new DbFileInfo("TestProdDbCopy_log", targetDataPath, DbFileInfo.FileTypeLog, restoreFromBackupFilePath)
				};

				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, restoreFromBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFileInfos, DbRestoreOption.CopyProdToTest);

				// Arrange
				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				Assert($"{TestDatabaseName} has been restored once as existing test db", Utilities.DatabaseExists(Db.Connection, TestDatabaseName));

				var existRegItems = TestDataHelpers.GetStmData(connection, TestDatabaseName);
				TestDataHelpers.PreconditionRegistryItems(connection, TestDatabaseName, existRegItems.Keys);
				TestDataHelpers.InsertStmData(connection, TestDatabaseName, "DATABASE_SCHEMA_VERSION", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), true);
				TestDataHelpers.SetDatabaseAsForTest(connection, TestDatabaseName);
				Assert($"{TestDatabaseName} must be set IsTestOrTrainingDb", Utilities.IsTestOrTrainingDb(connection, TestDatabaseName));
				restoreManagerMock.Invocations.Clear();

				// Act
				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				// Assert
				awsHelperMock.Verify(m => m.GetReadOnlyAccessKeysWithAssumeRole(It.IsAny<string>()), Times.Never);
			}
		}

		public void TestRestore_CopyProdToTest_HostedOnCargoWiseAndUsesS3ForEDocs_StoresNewAccessKeys()
		{
			// Arrange
			var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
			var restoreManagerMock = new Mock<DbRestoreManager>(dbHeaderOnlyReaderMock.Object) { CallBase = true };
			var restoreManager = restoreManagerMock.Object;
			restoreManagerMock
				.Setup(x => x.ApplyChangeAfterRestoreDatabase(It.IsAny<DbConnection>(), It.IsAny<string>()))
				.Callback((DbConnection connection, string dbName) =>
				{
					CreateColumnsIfNotExists(connection, dbName);
					TestDataHelpers.InsertStmData(connection, TestDatabaseName, "EDocsStorageProvider", "S3");
				});
			restoreManagerMock.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()));
			restoreManagerMock.Protected().Setup<bool>(
				"IsTargetDatabaseMainDb",
				ItExpr.IsAny<string>()).Returns(true);

			var awsHelperMock = new Mock<IAWSHelper>();
			awsHelperMock.Setup(m => m.GetReadOnlyAccessKeysWithAssumeRole(It.IsAny<string>())).Returns("KeyId=abcd;Secret=1234");
			restoreManager.AWSHelper = awsHelperMock.Object;

			using (var tempDir = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(connection, TestDatabaseName)))
			{
				var targetDataPath = tempDir.DirectoryName;
				var restoreFromBackupFilePath = TestDataHelpers.CopyTestFileResource(targetDataPath, TestBackupFile);
				var dbFileInfos = new DbFileInfoCollection
				{
					new DbFileInfo("TestProdDbCopy", targetDataPath, DbFileInfo.FileTypeData, restoreFromBackupFilePath),
					new DbFileInfo("TestProdDbCopy_log", targetDataPath, DbFileInfo.FileTypeLog, restoreFromBackupFilePath)
				};

				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, restoreFromBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFileInfos, DbRestoreOption.CopyProdToTest);

				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				Assert($"{TestDatabaseName} has been restored once as existing test db", Utilities.DatabaseExists(Db.Connection, TestDatabaseName));

				var existRegItems = TestDataHelpers.GetStmData(connection, TestDatabaseName);
				TestDataHelpers.PreconditionRegistryItems(connection, TestDatabaseName, existRegItems.Keys);
				TestDataHelpers.InsertStmData(connection, TestDatabaseName, "DATABASE_SCHEMA_VERSION", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), true);
				TestDataHelpers.SetDatabaseAsForTest(connection, TestDatabaseName);
				Assert($"{TestDatabaseName} must be set IsTestOrTrainingDb", Utilities.IsTestOrTrainingDb(connection, TestDatabaseName));
				awsHelperMock.Invocations.Clear();

				// Act
				restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);

				// Assert
				awsHelperMock.Verify(m => m.GetReadOnlyAccessKeysWithAssumeRole(It.IsAny<string>()), Times.Once);
				AssertEquals("KeyId=abcd;Secret=1234", Utilities.GetRegistryValue(connection, TestDatabaseName, "EDocsStorageAccess"));
			}
		}

		public void TestRestore_CopyProdToTest_HostedOnCargoWiseAndUsesS3ForEDocs_FailToCreateAccessKeys()
		{
			// Arrange
			var dbHeaderOnlyReaderMock = new Mock<IDbHeaderOnlyReader>();
			var restoreManagerMock = new Mock<DbRestoreManager>(dbHeaderOnlyReaderMock.Object) { CallBase = true };
			var restoreManager = restoreManagerMock.Object;
			restoreManagerMock
				.Setup(x => x.ApplyChangeAfterRestoreDatabase(It.IsAny<DbConnection>(), It.IsAny<string>()))
				.Callback((DbConnection connection, string dbName) =>
				{
					CreateColumnsIfNotExists(connection, dbName);
					TestDataHelpers.InsertStmData(connection, TestDatabaseName, "EDocsStorageProvider", "S3");
				});
			restoreManagerMock.Setup(m => m.SynchroniseSynonyms(It.IsAny<string>(), It.IsAny<string>()));
			restoreManagerMock.Protected().Setup<bool>(
				"IsTargetDatabaseMainDb",
				ItExpr.IsAny<string>()).Returns(true);

			var awsHelperMock = new Mock<IAWSHelper>();
			awsHelperMock.Setup(m => m.GetReadOnlyAccessKeysWithAssumeRole(It.IsAny<string>())).Throws<Exception>();
			restoreManager.AWSHelper = awsHelperMock.Object;

			var errorMessages = new List<string>();
			restoreManager.OnTaskFailed += message => errorMessages.Add(message);

			using (var tempDir = new TempDirectory())
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			using (new DisposableAction(() => AdoTestUtils.DropDbIfExists(connection, TestDatabaseName)))
			{
				var targetDataPath = tempDir.DirectoryName;
				var restoreFromBackupFilePath = TestDataHelpers.CopyTestFileResource(targetDataPath, TestBackupFile);
				var dbFileInfos = new DbFileInfoCollection
				{
					new DbFileInfo("TestProdDbCopy", targetDataPath, DbFileInfo.FileTypeData, restoreFromBackupFilePath),
					new DbFileInfo("TestProdDbCopy_log", targetDataPath, DbFileInfo.FileTypeLog, restoreFromBackupFilePath)
				};

				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
				dbFileInfos.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var mainServerConfig = new DbServerConfiguration(Db.ServerName, restoreFromBackupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(TestDatabaseName, dbFileInfos, DbRestoreOption.CopyProdToTest);

				AssertExceptionThrown<Exception>(() => restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));
				Assert($"{TestDatabaseName} has been restored once as existing test db", Utilities.DatabaseExists(Db.Connection, TestDatabaseName));

				var existRegItems = TestDataHelpers.GetStmData(connection, TestDatabaseName);
				TestDataHelpers.PreconditionRegistryItems(connection, TestDatabaseName, existRegItems.Keys);
				TestDataHelpers.InsertStmData(connection, TestDatabaseName, "DATABASE_SCHEMA_VERSION", DataRegistry.MinUpgradableDbMajorSchemaVersion.ToString(), true);
				TestDataHelpers.SetDatabaseAsForTest(connection, TestDatabaseName);
				Assert($"{TestDatabaseName} must be set IsTestOrTrainingDb", Utilities.IsTestOrTrainingDb(connection, TestDatabaseName));
				awsHelperMock.Invocations.Clear();

				// Act
				AssertExceptionThrown<Exception>(() => restoreManager.RestoreDatabasesUnsafe(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings));

				// Assert
				awsHelperMock.Verify(m => m.GetReadOnlyAccessKeysWithAssumeRole(It.IsAny<string>()), Times.Once);
				AssertEquals("DB", Utilities.GetRegistryValue(connection, TestDatabaseName, "EDocsStorageProvider"));
				AssertNullOrEmpty(Utilities.GetRegistryValue(connection, TestDatabaseName, "EDocsStorageServiceUrl"));
				AssertNullOrEmpty(Utilities.GetRegistryValue(connection, TestDatabaseName, "DocManagerStorageBucketName"));
				AssertNullOrEmpty(Utilities.GetRegistryValue(connection, TestDatabaseName, "EDocsStorageAccess"));
				AssertCollectionContains(errorMessages, message => message.Contains("Failed to create read-only access keys for S3."));
			}
		}

		internal static void CreateColumnsIfNotExists(DbConnection connection, string dbName)
		{
			DbObjectCreator.CreateColumnIfNotExists(connection, dbName, "GlbStaff", "GS_PasswordHash", "varbinary(20)", null);
			DbObjectCreator.CreateColumnIfNotExists(connection, dbName, "GlbStaff", "GS_PasswordHashIterations", "int", null);
			DbObjectCreator.CreateColumnIfNotExists(connection, dbName, "GlbStaff", "GS_PasswordSalt", "varbinary(16)", null);
			DbObjectCreator.CreateColumnIfNotExists(connection, dbName, "StmData", "SD_PreserveTestValue", "BIT NOT NULL", "0");
			DbObjectCreator.CreateColumnIfNotExists(connection, dbName, "StmData", "SD_IsCancelled", "bit", "0");
		}

		protected override void SetUp()
		{
			base.SetUp();

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, TestDatabaseName);
			}

			DropExistingDbLoginsForTestDatabase();
			dbRecoverModelManagerMock = new DbRecoveryModelManagerMock();
			dbRecoverModelManagerServiceSubstitute = ObjectFactory.Substitute(dbRecoverModelManagerMock.Object);
		}

		protected override void TearDown()
		{
			dbRecoverModelManagerServiceSubstitute?.Dispose();
			dbRecoverModelManagerServiceSubstitute = null;

			base.TearDown();
			DropExistingDbLoginsForTestDatabase();
		}

		void DropExistingDbLoginsForTestDatabase()
		{
			using (var adminConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var script = $@"
DECLARE @stmt nvarchar(max)
SELECT
	@stmt = ISNULL(@stmt + CHAR(13) + CHAR(10), N'')
		+ N'DROP LOGIN ' + QUOTENAME(name) + N';'
FROM
	sys.server_principals
WHERE
	name like '{TestDatabaseName}[_]%'
;
EXEC(@stmt)
";
				adminConnection.ExecuteNonQuery(script);
			}
		}

		IDisposable dbRecoverModelManagerServiceSubstitute;
		DbRecoveryModelManagerMock dbRecoverModelManagerMock;
		const string TestDatabaseName = nameof(DbRestoreManagerTest);
		const string TestBackupFile = "testProdCopy.bak";
		const string TestFilesPath = @"Enterprise\Product\Core\DataTools\DbBackupAndRestore\DbBackupAndRestore.Business\Restore\TestFiles";

		#region TestHelpers

		public static bool CheckLoginExistsWithActualDatabaseLoginName(string actualDbLoginName, AdminConnection conn)
		{
			string sqlText = string.Format(
				"SELECT default_database_name FROM sys.server_principals WHERE name = @userLogin COLLATE SQL_Latin1_General_CP1_CI_AS"
			);

			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@userLogin", SqlDbType.NVarChar, 128, actualDbLoginName);
				object userDb = cmd.ExecuteScalar();
				return (userDb != null && userDb.ToString() == Db.DatabaseName);
			}
		}

		void SQL_CreateLogin(DbConnection connection, string loginName, string password)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"CREATE LOGIN {0} WITH PASSWORD = {1}, DEFAULT_DATABASE = {2}, CHECK_POLICY = OFF, DEFAULT_LANGUAGE = us_english;"
				, loginName.QuoteName() //0
				, "'" + password + "'" // 1
				, Db.DatabaseName.QuoteName()          // 2
				);

			using (var cmd = connection.Command(sqlText))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void AssertStmDataValue(DbConnection connection, string dbName, string name, Guid owner, Guid departmentguid, string value)
		{
			var dbValue = GetStmData(connection, dbName, name, owner, departmentguid);
			AssertEquals(value, dbValue);

			var dbOwner = GetStmDataOwner(connection, dbName, name, owner, departmentguid);
			AssertEquals(owner, dbOwner);

			var dbDepartmentGuid = GetStmDataDepartmentGuid(connection, dbName, name, owner, departmentguid);
			AssertEquals(departmentguid, dbDepartmentGuid);
		}

		string GetMailDbStatus(DbConnection connection, string dbName)
		{
			const string selectMailDbStatus = "SELECT TOP 1 [MI_Status] FROM [{0}].[dbo].[MailDBItems]";
			var selectCommand = string.Format(selectMailDbStatus, dbName);
			return ((string)connection.ExecuteScalar(selectCommand)).Trim();
		}

		int GetTableRowCount(DbConnection connection, string dbName, string tableName)
		{
			var selectCommand = string.Format("SELECT COUNT(*) FROM [{0}].[dbo].[{1}]", dbName, tableName);
			return (int)connection.ExecuteScalar(selectCommand);
		}

		int GetStmPrintJobCount(DbConnection connection, string dbName)
		{
			return GetTableRowCount(connection, dbName, "StmPrintJob");
		}

		int GetStmPrintJobCopyRecipientCount(DbConnection connection, string dbName)
		{
			return GetTableRowCount(connection, dbName, "StmPrintJobCopyRecipient");
		}

		string GetStmDataWhereClause(string name, Guid owner, Guid departmentGuid)
		{
			var filter = string.Format(
				"WHERE SD_Name = '{0}' AND SD_Owner {1} AND SD_DepartmentGuid {2}",
				name,
				(owner == Guid.Empty) ? "is NULL" : "= '" + owner + "'",
				(departmentGuid == Guid.Empty) ? "is NULL" : "= '" + departmentGuid + "'");
			return filter;
		}

		string GetStmData(DbConnection connection, string dbName, string name, Guid owner, Guid departmentGuid)
		{
			var selectClause = @"
				SELECT TOP 1 convert(nvarchar(max), sd_binaryvalue) as textvalue
				FROM [{0}].[dbo].[StmData] {1}
				";
			var filter = GetStmDataWhereClause(name, owner, departmentGuid);
			var selectCommand = string.Format(selectClause, dbName, filter);

			var o = connection.ExecuteScalar(selectCommand);
			var result = o?.ToString().Trim();

			return result;
		}

		string GetStmData(DbConnection connection, string dbName, string name)
		{
			var selectStmData = string.Format(@"
				SELECT TOP 1 convert(nvarchar(max), SD_BinaryValue) as textvalue
				FROM [{0}].[dbo].[StmData]
				WHERE SD_Name = '{1}'",
				dbName, name);

			var o = connection.ExecuteScalar(selectStmData);
			var result = o?.ToString().Trim();

			return result;
		}

		string GetStmDataByPk(DbConnection connection, string dbName, string sd_pk)
		{
			var selectStmData = @"
				SELECT convert(nchar(200), convert(varbinary(8000), sd_binaryvalue)) as textvalue
				FROM [{0}].[dbo].[StmData]
				WHERE sd_pk = '{1}'";

			var selectCommand = string.Format(selectStmData, dbName, sd_pk.ToLower());

			var o = connection.ExecuteScalar(selectCommand);
			string result = null;
			if (o != null)
			{
				result = ((string)o).Trim();
			}

			return result;
		}

		Guid GetStmDataOwner(DbConnection connection, string dbName, string name, Guid owner, Guid departmentGuid)
		{
			var selectStmData = "SELECT sd_owner FROM [{0}].[dbo].[StmData] {1}";

			var selectCommand = string.Format(selectStmData, dbName, GetStmDataWhereClause(name, owner, departmentGuid));

			var o = connection.ExecuteScalar(selectCommand);
			var result = Guid.Empty;
			if (o != null)
			{
				if (o == DBNull.Value)
				{
					result = Guid.Empty;
				}
				else
				{
					result = (Guid)o;
				}
			}
			else
			{
				result = Guid.Empty;
			}
			return result;
		}

		Guid GetStmDataDepartmentGuid(DbConnection connection, string dbName, string name, Guid owner, Guid departmentGuid)
		{
			var selectStmData = "SELECT sd_departmentguid FROM [{0}].[dbo].[StmData] {1}";

			var selectCommand = string.Format(selectStmData, dbName, GetStmDataWhereClause(name, owner, departmentGuid));

			var o = connection.ExecuteScalar(selectCommand);
			var result = Guid.Empty;
			if (o != null)
			{
				if (o == DBNull.Value)
				{
					result = Guid.Empty;
				}
				else
				{
					result = (Guid)o;
				}
			}
			else
			{
				result = Guid.Empty;
			}
			return result;
		}

		string[] GetEK_DestinationFromEDICommunicationsMode(DbConnection connection, string dbName)
		{
			StringCollection values = new StringCollection();
			string[] result = Array.Empty<string>();

			var selectEDIComms = string.Format(@"
				SELECT TOP 3 EK_Destination
				FROM [{0}].[dbo].[EDICommunicationsMode]",
				dbName);

			DbCommand o = connection.Command(selectEDIComms);

			using (var reader = o.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add(reader.GetString(0));
				}
			}

			result = new string[values.Count];
			values.CopyTo(result, 0);

			return result;
		}

		string[] GetEK_LoginNameFromEDICommunicationsMode(DbConnection connection, string dbName)
		{
			StringCollection values = new StringCollection();
			string[] result = Array.Empty<string>();

			var selectEDIComms = string.Format(@"
				SELECT TOP 3 EK_LoginName
				FROM [{0}].[dbo].[EDICommunicationsMode]",
				dbName);

			DbCommand o = connection.Command(selectEDIComms);

			using (var reader = o.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add(reader.GetString(0));
				}
			}

			result = new string[values.Count];
			values.CopyTo(result, 0);

			return result;
		}

		string[] GetEK_PasswordFromEDICommunicationsMode(DbConnection connection, string dbName)
		{
			StringCollection values = new StringCollection();
			string[] result = Array.Empty<string>();

			var selectEDIComms = string.Format(@"
				SELECT TOP 3 EK_Password
				FROM [{0}].[dbo].[EDICommunicationsMode]",
				dbName);

			DbCommand o = connection.Command(selectEDIComms);

			using (var reader = o.ExecuteReader())
			{
				while (reader.Read())
				{
					values.Add(reader.GetString(0));
				}
			}

			result = new string[values.Count];
			values.CopyTo(result, 0);

			return result;
		}

		void InsertEDICommunicationsMode(DbConnection connection, string dbName, Guid pk, string module, string commsDirection, string communicationsTransport, string destination, string fileFormat, string loginName, string password, Guid parentId, string parentTableCode, string messagePurpose, string publishInternalMilestones)
		{
			var insertEDICommunicationMode = @"
				INSERT INTO [{0}].[dbo].[EDICommunicationsMode]  (EK_PK, EK_Module, EK_CommsDirection, EK_CommunicationsTransport, EK_Destination, EK_FileFormat, EK_LoginName, EK_Password, EK_ParentID, EK_ParentTableCode, EK_MessagePurpose, EK_PublishInternalMilestones)
				 VALUES  ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}')";

			var insertCommand = string.Format(insertEDICommunicationMode, dbName, pk, module, commsDirection, communicationsTransport, destination, fileFormat, loginName, password, parentId, parentTableCode, messagePurpose, publishInternalMilestones);
			connection.ExecuteNonQuery(insertCommand);
		}

		void UpdateEDICommunicationsMode(DbConnection connection, string dbName, Guid pk, string destination)
		{
			var updateEDICommunicationsMode = @"
				UPDATE [{0}].[dbo].[EDICommunicationsMode] SET EK_Destination = '{2}' WHERE EK_PK = '{1}'";

			var updateCommand = string.Format(updateEDICommunicationsMode, dbName, pk, destination);
			connection.ExecuteNonQuery(updateCommand);
		}

		void DeleteEDICommunicationsMode(DbConnection connection, string dbName, Guid pk)
		{
			var deleteEDICommunicationsMode = @"
				DELETE FROM [{0}].[dbo].[EDICommunicationsMode] WHERE EK_PK = '{1}'";

			var deleteCommand = string.Format(deleteEDICommunicationsMode, dbName, pk);
			connection.ExecuteNonQuery(deleteCommand);
		}

		void InsertStmUsageData(DbConnection connection, string dbName, Guid pk, string priceCode, string category, string schemaVersion, int isFailed, string billingdata)
		{
			var insertCommand = string.Format("INSERT INTO [{0}].[dbo].[StmUsageData] (SUD_PK, SUD_Category, SUD_Code, SUD_Schema, SUD_Fail, SUD_Data) VALUES ('{1}' , '{2}', '{3}', '{4}', {5}, '{6}')",
				dbName, pk, category, priceCode, schemaVersion, isFailed, billingdata);

			connection.ExecuteNonQuery(insertCommand);
		}

		void InsertStorageDocsToDeleteData(DbConnection connection, string dbName, Guid pk, Guid documentId)
		{
			var insertCommand = string.Format("INSERT INTO [{0}].[dbo].[StorageDocsToDelete] (SCD_PK, SCD_StorageDocIdentifier) VALUES ('{1}' , '{2}')",
				dbName, pk, documentId);

			connection.ExecuteNonQuery(insertCommand);
		}

		int GetGlbStaffCountWithNonEmptyActiveDirectoryDetails(DbConnection connection, string dbName)
		{
			var selectCommand = string.Format("SELECT COUNT(*) FROM [{0}].[dbo].[GlbStaff] WHERE GS_ActiveDirectoryObjectGUID IS NOT NULL OR GS_DomainName <> ''", dbName);
			return (int)connection.ExecuteScalar(selectCommand);
		}

		int GetGlbGroupCountWithNonEmptyActiveDirectoryDetails(DbConnection connection, string dbName)
		{
			var selectCommand = string.Format("SELECT COUNT(*) FROM [{0}].[dbo].[GlbGroup] WHERE GG_ActiveDirectoryObjectGUID IS NOT NULL OR GG_DomainName <> ''", dbName);
			return (int)connection.ExecuteScalar(selectCommand);
		}

		Guid GetEDIMessagePK(DbConnection connection, string dbName, string status)
		{
			var sql = $@"SELECT TOP(1) EM_PK FROM [{dbName}].[dbo].[EDIMessage] WHERE EM_ApplicationCode = 'CMR' AND EM_MessageType = 'CRS' AND EM_ReceiveTransmit = 'TRX' AND EM_Status = '{status}'";
			Guid.TryParse(connection.Command(sql).ExecuteScalar()?.ToString() ?? Guid.Empty.ToString(), out var result);

			return result;
		}

		Guid GetEDIInterchangePK(DbConnection connection, string dbName, string status)
		{
			var sql = $@"SELECT TOP(1) EI_PK FROM [{dbName}].[dbo].[EDIInterchange] WHERE EI_ApplicationCode = 'CMR' AND EI_InterchangeType = 'CMR' AND EI_ReceiveTransmit = 'TRX' AND EI_Status = '{status}'";
			Guid.TryParse(connection.Command(sql).ExecuteScalar()?.ToString() ?? Guid.Empty.ToString(), out var result);

			return result;
		}

		void AssertEDIMessage(DbConnection connection, string dbName, Guid pk, string message, string expectedStatus)
		{
			var sqlText = string.Format(@"
				SELECT EM_Status
				FROM [{0}]..EDIMessage
				WHERE EM_PK = '{1}'", dbName, pk);

			var actualStatus = (string)connection.Command(sqlText).ExecuteScalar();
			AssertEquals(message, expectedStatus, actualStatus);
		}

		void AssertEDIInterchange(DbConnection connection, string dbName, Guid pk, string message, string expectedStatus)
		{
			var sqlText = string.Format(@"
				SELECT EI_Status
				FROM [{0}]..EDIInterchange
				WHERE EI_PK = '{1}'", dbName, pk);

			var actualStatus = (string)connection.Command(sqlText).ExecuteScalar();
			AssertEquals(message, expectedStatus, actualStatus);
		}

		void InsertStmData(DbConnection connection, string dbName, string name, string value, bool preserveTestValue = false)
		{
			var insertStmData = @"
				INSERT INTO [{0}].[dbo].[StmData] (SD_Name, SD_BinaryValue, SD_PreserveTestValue)
				VALUES ('{1}',convert(varbinary(max), N'{2}'), @PreserveTestValue)";

			var insertCommand = string.Format(insertStmData, dbName, name, value);
			using (var command = connection.Command(insertCommand))
			{
				command.AddParameter("@PreserveTestValue", SqlDbType.Bit, preserveTestValue);
				command.ExecuteNonQuery();
			}
		}

		void UpdateStmData(DbConnection connection, string dbName, string name, string value)
		{
			var updateStmData = @"
				UPDATE [{0}].[dbo].[StmData]
				SET sd_binaryvalue = convert(varbinary(max), N'{1}')
				WHERE sd_name = '{2}'";

			var updateCommand = string.Format(updateStmData, dbName, value, name);
			connection.ExecuteNonQuery(updateCommand);
		}

		void UpdateStmDataByPk(DbConnection connection, string dbName, string pk, string value)
		{
			var updateStmData = @"
				UPDATE [{0}].[dbo].[StmData]
				SET sd_binaryvalue = convert(varbinary(max), N'{1}')
				WHERE sd_pk = '{2}'";

			var updateCommand = string.Format(updateStmData, dbName, value, pk);
			connection.ExecuteNonQuery(updateCommand);
		}

		void AssertStmServiceHost_HostName(DbConnection connection, string dbName, string pk, string expectedHostName)
		{
			AssertStmServiceHost_HostName(string.Empty, connection, dbName, pk, expectedHostName);
		}

		void AssertStmServiceHost_HostName(string message, DbConnection connection, string dbName, string pk, string expectedHostName)
		{
			var sqlText = string.Format(@"
				SELECT SH_HostName
				FROM [{0}]..StmServiceHost
				WHERE SH_PK = '{1}'", dbName, pk);
			var hostName = (string)connection.ExecuteScalar(sqlText);
			AssertEquals(message, expectedHostName, hostName);
		}

		void AssertStorageDocsToPreserved(string message, DbConnection connection, string dbName, string pk, string expectedStorageDocsIdentifier)
		{
			var sqlText = string.Format(@"
				SELECT SCD_StorageDocIdentifier
				FROM [{0}]..StorageDocsToDelete
				WHERE SCD_PK = '{1}'", dbName, pk);
			var storageDocsIdentifier = ((Guid)connection.ExecuteScalar(sqlText)).ToString();
			AssertEquals(message, expectedStorageDocsIdentifier, storageDocsIdentifier);
		}

		void InsertStmServiceHost(DbConnection connection, string dbName, string pk, string hostName)
		{
			var sqlText = string.Format(@"
				INSERT [{0}]..StmServiceHost (SH_PK, SH_HostName)
				VALUES ('{1}', '{2}')", dbName, pk, hostName);
			connection.ExecuteNonQuery(sqlText);
		}

		void UpdateStmServiceHost_HostName(DbConnection connection, string dbName, string pk, string hostName)
		{
			var sqlText = string.Format(@"
				UPDATE [{0}]..StmServiceHost
				SET SH_HostName = '{1}'
				WHERE SH_PK = '{2}'", dbName, hostName, pk);
			connection.ExecuteNonQuery(sqlText);
		}

		void AssertStmScheduleTask_ScheduleDescription(DbConnection connection, string dbName, string pk, string expectedScheduleDescription)
		{
			AssertStmScheduleTask_ScheduleDescription(string.Empty, connection, dbName, pk, expectedScheduleDescription);
		}

		void AssertStmScheduleTask_ScheduleDescription(string message, DbConnection connection, string dbName, string pk, string expectedScheduleDescription)
		{
			var sqlText = string.Format(@"
				SELECT S5_ScheduleDescription
				FROM [{0}]..StmScheduleTask
				WHERE S5_PK = '{1}'", dbName, pk);
			var scheduleDescription = (string)connection.ExecuteScalar(sqlText);
			AssertEquals(message, expectedScheduleDescription, scheduleDescription);
		}

		void UpdateStmScheduleTask_ScheduleDescription(DbConnection connection, string dbName, string pk, string scheduleDescription)
		{
			var sqlText = string.Format(@"
				UPDATE [{0}]..StmScheduleTask
				SET S5_ScheduleDescription = '{1}'
				WHERE S5_PK = '{2}'", dbName, scheduleDescription, pk);
			connection.ExecuteNonQuery(sqlText);
		}

		void CreateNewBranchAndUpdateStmScheduleTaskBranchFk(DbConnection connection, string dbName, string pk)
		{
			var sqlText = string.Format(@"
				DECLARE @BranchPk uniqueidentifier; SET @BranchPk = '5F7317CD-2E3F-41A1-B245-92A49696EC2C';
				INSERT [{0}]..GlbBranch (GB_PK, GB_Code) VALUES (@BranchPk, 'B2');
				UPDATE [{0}]..StmScheduleTask
					SET S5_GB = @BranchPk
					WHERE S5_PK = '{1}'",
				dbName, pk);
			connection.ExecuteNonQuery(sqlText);
		}

		void CreateTempTable()
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				var dataVault = "TestTargetDb_A9244977D81748AE8BFB5EA815F3D504-DataVault";
				var tableName = "TempStmDataTransformationVersion";

				var sqlText = string.Format(@"
					IF EXISTS(SELECT * FROM sys.databases WHERE name='{0}')
						DROP DATABASE [{0}]
					CREATE DATABASE [{0}]
					 ;", dataVault);
				connection.ExecuteNonQuery(sqlText);

				sqlText = string.Format(@"
					USE [{0}]
					IF EXISTS(SELECT * FROM [{0}].sys.tables WHERE name = '{1}')
						DROP TABLE [{0}]..{1}
					CREATE TABLE [{0}]..[{1}] (PK UNIQUEIDENTIFIER)
					 ;",
					dataVault, tableName);
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void AssertStmALog(DbConnection connection, string dbName, string operation, string schemaVersionBefore, string schemaVersionAfter)
		{
			var query = string.Format(@"SELECT SL_Reference FROM [{0}]..StmALog WHERE SL_SE_NKEvent = 'DRS'", dbName);

			using (var cmd = connection.Command(query))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var reference = reader.GetString(0);
					if (reference.Contains("Operation:" + operation) && reference.Contains(string.Format("DbSchemaVersion Before:{0}, After:{1}", schemaVersionBefore, schemaVersionAfter)))
					{
						Assert(true);
						return;
					}
				}
				Fail("No log found with database restore details");
			}
		}

		void AssertRegistry(DbConnection connection, string dbName, string operation, string schemaVersionBefore, string schemaVersionAfter)
		{
			var query = string.Format(@"SELECT SD_BinaryValue FROM [{0}]..StmData WHERE SD_Name = 'LastDatabaseRestore'", dbName);

			using (var cmd = connection.Command(query))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var binaryValue = reader["SD_BinaryValue"];
					if (binaryValue != DBNull.Value)
					{
						var value = Encoding.ASCII.GetString((byte[])binaryValue).Replace("\0", "");

						if (value.Contains(string.Format("<Operation>{0}</Operation>", operation)) && value.Contains(string.Format("<DbSchemaVersionBefore>{0}</DbSchemaVersionBefore><DbSchemaVersionAfter>{1}</DbSchemaVersionAfter>", schemaVersionBefore, schemaVersionAfter)))
						{
							Assert(true);
							return;
						}
					}
				}
				Fail("No registry found with database restore details");
			}
		}

		void AssertCdcEnabled(DbConnection connection, string dbName)
		{
			var query = string.Format(@"SELECT is_cdc_enabled FROM sys.databases WHERE name = '{0}'", dbName);
			AssertEquals("Database should be CDC enabled.", 1, Convert.ToInt32(connection.ExecuteScalar(query)));
		}

		void AssertCdcNotEnabled(DbConnection connection, string dbName, string errorMessage)
		{
			var query = string.Format(@"SELECT is_cdc_enabled FROM sys.databases WHERE name = '{0}'", dbName);
			AssertEquals($@"Database should not be CDC enabled. {errorMessage}", 0, Convert.ToInt32(connection.ExecuteScalar(query)));
		}

		void AssertResetCDCIsFlagged(DbConnection connection, string dbName)
		{
			var value = GetStmData(connection, dbName, "BiResetChangeDataCapture");
			AssertEquals("Database should be flagged for CDC reset", "True", value);
		}

		static Action CreateStorageToDeleteDocsTableIfMissing(DbConnection connection, string dbName)
		{
			return () => DbObjectCreator.CreateTableIfNotExists(connection, dbName, "StorageDocsToDelete", "CREATE TABLE [dbo].[StorageDocsToDelete]([SCD_PK] [uniqueidentifier] NOT NULL, [SCD_StorageDocIdentifier] [uniqueidentifier] NOT NULL) ON [PRIMARY]");
		}

		const string DeleteAllTransformationVersions = @"DELETE FROM [{0}].[dbo].[StmData] WHERE SD_Name in ('DatabaseMajorTransformationVersion','DatabaseMinorTransformationVersion','DATABASE_SCHEMA_VERSION')";

		const string FreightNotesHeaderLength = "FreightNotesHeaderLength";
		const string FreightNotesLengthNew = "FreightNotesLengthNew";
		const string PhysicalServerID = "PhysicalServerID";

		// Decrypted Company Licences for reference
		// const string CompanyLicenceXml1 = @"<LicenceKey><CheckPoints><COR Type=""PUR"" User=""12"" Expiry=""00010101"" /><MAN Type=""REN"" User=""3456"" Expiry=""00010101"" /></CheckPoints><Company CountryPK=""e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e"" PhysicalServerID=""PRD"" /><Branches/><InstallationDetails/></LicenceKey>";
		// const string CompanyLicenceXml2 = @"<LicenceKey><CheckPoints><DIM Type=""OTM"" User=""21"" Expiry=""00010101"" /><ACC Type=""OPN"" User=""321"" Expiry=""00010101"" /></CheckPoints><Company CountryPK=""e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e"" PhysicalServerID=""PRD"" /><Branches/><InstallationDetails/></LicenceKey>";
		// To Generate encrypted string:
		//		var encoder = LicenceBuilder.GetNewLicenceEncoder();
		// 		var encrypted = encoder.Encrypt(CompanyLicenceXml1);
		//
		// To update TestFiles backup:
		// update dbo.StmData
		// set SD_BinaryValue = convert(varbinary(max), convert(nvarchar(max), cast ('<encrypted licence key 1 goes here>' as varchar(max))))
		// where SD_name = 'FreightNotesLengthNew' and SD_Owner = 'D438C595-B8D2-4B56-B3A4-3B3CD37D9418'
		//
		// update dbo.StmData
		// set SD_BinaryValue = convert(varbinary(max), convert(nvarchar(max), cast ('<encrypted licence key 2 goes here>' as varchar(max))))
		// where SD_name = 'FreightNotesLengthNew' and SD_Owner = 'E7AC2090-BB53-43F9-BFA5-687A7BAE4467'

		// Company (SD_Owner) = D438C595-B8D2-4B56-B3A4-3B3CD37D9418
		const string CompanyLicenceFromBackup01 = "A0tboxrvZFKJPnUzPkrmtAhcDHioB2S8fqBY29JF7zXXkVLU2d89XjGMRIjH2j9vc1Djfn3tZzEZXIfuWBiACg8Z62RlThZ05jefm/VIJDXlg0XL1ooKf782Jn+/9gzsfQV9zw03fyTxIeFhvRVXIzPqFPzeRMJXOc1fZLepio2ZaBLvClz2hwsTlfZzn/Dm2xmlv2L3Jrx3hzmzK4J7Ppx6ybFLON4s+rpLHai0SRTvPOObC0a9WHjJwgSFSCuvjsiNqyr9ovc8mvsUi1r6GiOjTy5HPnSFkbmwBsGjSplY5G0hGNtIW+4SVW+i/k8cP2/QHcreKufVPDqEyT/lS8cLSkx5ymnjJXHU22s35KF0wK9P4rYSuPXuDpggyF6Kgd1EQe9AB2AObmvErPzlHYhKw+pLy85AQxwvR++N7O+BHYo4J6LbsCm0OrKqL/j4YQYw4bIihNUplLWYKHRfOJoFsApaXSPxj7V4xNriTW+S5tOB79wvVVzXfIhrYwdtpsPoVqd1OZvkwM4S04ae94zPECPhsxWdIeKuVlRSRKGHXz4nNjthsxEKboAufJPDXGBaYjzRifhs0DSXbj9XFsix89Zvh3TbgbBd/w8fgTKvZP8WAQ9x2ngOwSzNUhnbYpD13mLo23YykrhYKgnodWr5jBxpddurxb/wpuSrd73i5OTVV+Q570Pl/M86z1mP75LVq9j9IgXnYDmCGQkGIHLjrz1KNhitwqMEgxq773wbYaIL/sGgkyvoV+lBgcfVdbKxMcU7UEGdh2KXFkMOcGMtoZSO8NWyLsY33SINtEC9ZcNN60c0zLIvR0zii1el";
		// Company (SD_Owner) = E7AC2090-BB53-43F9-BFA5-687A7BAE4467
		const string CompanyLicenceFromBackup02 = "A0tboxrvZFKJPnUzPkrmtAhcDHioB2S8fqBY29JF7zXXkVLU2d89XjGMRIjH2j9v78SbaE49LvpBycxTBvKe00A3N08qwYF7NFe7LlSLvXvy1Dcg9n+zsJ5v9ihUWPzG225xQZnKYwt03cPHbM4RSkYt6lRqn1HyBZt2QQ3c+k+CLAnyniHr/mZ+GrgMGjHBqNYD5p8ZRNHCJYoeTjhxcWePBOCKJhiJRGfhwDzYAKrcO+7Pojs6HdcEt1eTnGQhrnL3ZKz/3SxGOjhEy9AsjfadSZZbY4VoIKl0D4UIqvIFMW4RB0XqVj8zILAYPe0JDUdyPUI911PAdwqyey88vhLCcMjq1kHVm8N8oPSpLbEnoFyNyewd4V/52OUJCbtxBRtkfTu2CYAttib97G16M+Q/WOAkHq3A48K7nY+MoSYfqDBRoBbUfbSlHUYj49WYUB5+5hdXxmQ5OTzlPOcnbRH+sL9+L5T1zXhfLSRrhP+MWqfq26XdguN9BmgOwcdtfhxly51o0h2xtvL6pfQWjWUcYXNa4keyGLRQ9q7cd+9kuDIn56wS1voNv3Xv9qNW/YK82TsNuAgv5wcmJzdDVFAVfP+iKWt37uLPI4dCMztt3dxeVGwpVo1eFSAKnVmNWOjoIlVFiY4TeYJHygVrLDdCYjZGGVIDOMPg8znvzsYkMDcgUVtM9B2IoVBoJ7+c";
		// Value of system registration key used to asset it is preserved when a test system is copied from production
		const string ChangedSysRegKey = "GAPLcP4y8YaMhFHYeaeNGKCfLexsSSDCx2Ylb3nUg4rc0NQbGAIJGcufyV1XoKPWW0F0Vh5qhmKsbOTxJq4cqZz9jGdkVUFUX2nLLVhvCqgwOOV8/g2MNUjgCRrmXl4AJasEcWKOEuN2/17gtipST/hvRYPIB/WNV/5CgLeBiL2kGZKSLbZjyRQK7ZLdCPVypFxNI1QI5o1GHrSik5fLeRJd/rNV0c241toy5m1V5eGCq9LQJrQ9Lp1s0wD6gOmUnTek5vzVVFV9RYyLTUHfQ89oRk9B3PLIxBtOWvNoKkmsHx9UHrCNBCi6AyFNWavrwCYYGQLUmyeZpnasnnXccy38AoE+A9DpfE/SPgMEubvtVdxrhSbG8yqRj1bkoNXzQVlfFrE21Wwx4FTlzUO0J/bY7TAs054oWSOrP/gPzXg7JzV5zi0LuRwjO07TPegenFLHL5m/JtCx2CaA/o/A2anwJplm8bT3n4fp4XHvLvbfyKe82+JxEjkOPdnpVrCoV37fZ5hv7T3LJVhgpV3M5RjSufTPjykStO7u8uMl6g5Ez0WYYDSXLZlV8kTawl4s25lanNHs0jYxz2VjEQDymZQFMArxN3pH3S1sCA+jhRMAC+UK9B7NJGpA5ollhjv6oiPerSzfa5mV6I9pdtSCelNyZTavkGFSL/MfzXsOKcf3bZ3wjAiksCJf6YyA6YDkpSY152Q5aWaPmPTn10+blc0eCk2x+aNO/s4KnhV3kBIvJAipWrhs2Cc4GTZ7J5cz3cc1qyzOoNCo/+Le8gsFxFckNMCF3H2pqG43m5cRRsQFTdrGfL+ZXMG/8wcw6GR1MN45Mc6D99ADc9cKqgvfezUWObxmc74W5F1afH4X6WSI7VgGDDelY5yQFNbgJuJbn5+DLO7Npr9xw+y6wAnPnQMzyUj7dUFEuEA6yXyhEhBu7prllJ1JminQZK4UhqlnTAYiJZGdcwPkV4TN+mNyE7UzvLnjIBGJO96IfEpMd1fmVexEjq3nPEwoYBJKmZXUGaemTwKS5+il3HDLdFVXbw==";

		internal class DbRestoreManagerForTesting : DbRestoreManager
		{
			public DbRestoreManagerForTesting(bool isTargetDbMainDb = true) : base(new Mock<DbHeaderOnlyReader>().Object)
			{
				this.isTargetDbMainDb = isTargetDbMainDb;
			}

			readonly bool isTargetDbMainDb;
			internal List<Action> SqlCommandAfterRestoreDatabaseActions => sqlCommandAfterRestoreDatabaseActions ?? (sqlCommandAfterRestoreDatabaseActions = new List<Action>());
			List<Action> sqlCommandAfterRestoreDatabaseActions;

			protected internal override bool IsTargetDatabaseMainDb(string targetDbName)
			{
				return isTargetDbMainDb;
			}

			public void DoRestore_Exposed(AdminConnection connection, DbServerConfiguration mainServerConfig, AuditDbServerConfiguration auditServerConfig, EdwDbServerConfiguration edwServerConfig, DbRestoreSettings dbRestoreSettings)
			{
				DoRestore(connection, mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
				ApplyChangeAfterRestoreDatabase(connection, dbRestoreSettings.TargetDbName);
			}

			public void AddExtendedProperties(string name, string value)
			{
				extendedProperties.Add(name, value);
			}

			public override void ApplyChangeAfterRestoreDatabase(DbConnection connection, string dbName)
			{
				if (SqlCommandAfterRestoreDatabaseActions != null)
				{
					foreach (var sqlCommandAction in SqlCommandAfterRestoreDatabaseActions)
					{
						sqlCommandAction();
					}
				}
			}

			public void AddPasswordHashColumnsIfMissing(DbConnection connection, string dBName)
			{
				// the test dbs' schema is outdated
				SqlCommandAfterRestoreDatabaseActions.Add(() => DbObjectCreator.CreateColumnIfNotExists(connection, dBName, "GlbStaff", "GS_PasswordHash", "varbinary(20)", null));
				SqlCommandAfterRestoreDatabaseActions.Add(() => DbObjectCreator.CreateColumnIfNotExists(connection, dBName, "GlbStaff", "GS_PasswordHashIterations", "int", null));
				SqlCommandAfterRestoreDatabaseActions.Add(() => DbObjectCreator.CreateColumnIfNotExists(connection, dBName, "GlbStaff", "GS_PasswordSalt", "varbinary(16)", null));
			}

			public void AddIsCancelledColumnIfMissing(DbConnection connection, string dBName)
			{
				SqlCommandAfterRestoreDatabaseActions.Add(() => DbObjectCreator.CreateColumnIfNotExists(connection, dBName, "StmData", "SD_IsCancelled", "bit", "0"));
			}

			public void AddPreserveTestValueColumnIfMissing(DbConnection connection, string dBName)
			{
				SqlCommandAfterRestoreDatabaseActions.Add(() => DbObjectCreator.CreateColumnIfNotExists(connection, dBName, "StmData", "SD_PreserveTestValue", "bit", "1"));
			}

			public void AddStorageDocsToDeleteTableIfMissing(DbConnection connection, string dBName)
			{
				// the test dbs' schema is outdated
				SqlCommandAfterRestoreDatabaseActions.Add(CreateStorageToDeleteDocsTableIfMissing(connection, dBName));
			}

			internal Regex GetRelatedDatabaseBackupFileRegex_Exposed(string backupFileName, FileValidity fileValidity, string relatedDbRegexPattern, bool isSharedDb = false)
			{
				return GetRelatedDatabaseBackupFileRegex(backupFileName, fileValidity, relatedDbRegexPattern, isSharedDb);
			}

			internal override void SynchroniseSynonyms(string dbServer, string targetDbName)
			{
			}
		}

		class TestLogger : IDisposable
		{
			public TestLogger()
			{
				outputMessage = new List<string>();
				originalLogger = Logger.Instance.logger;

				var loggerMock = new Mock<ILog>();
				loggerMock
					.Setup(x => x.Info(It.IsAny<object>()))
					.Callback<object>(x => outputMessage.Add(x.ToString()));

				Logger.Instance.logger = loggerMock.Object;
			}

			public string GetOutputMessage()
			{
				return string.Join("", outputMessage);
			}

			public void Dispose()
			{
				Logger.Instance.logger = originalLogger;
			}

			readonly List<string> outputMessage;
			readonly ILog originalLogger;
		}

		#endregion
	}
}
