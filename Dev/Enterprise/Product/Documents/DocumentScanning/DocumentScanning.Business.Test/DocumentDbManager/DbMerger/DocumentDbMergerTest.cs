using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class DocumentDbMergerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestQuantityMbsNecessaryToMove_MergeStopsIfLessThanOneWritableDbWithFreeSpace()
		{
			// Arrange
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentDbMerger = new DocumentDbMerger(factory);
			var internalDictionary = new Dictionary<int, DbMergeInfo>
			{
				{ 1, new DbMergeInfo(1, "1", 1024, false) },
				{ 2, new DbMergeInfo(2, "2", 1024, false) },
				{ 3, new DbMergeInfo(3, "3", 3, true) },
				{ 4, new DbMergeInfo(4, "4", 3072, false) }
			};
			var collection = new DbMergeInfoCollectionForTesting(internalDictionary);

			// Act
			using (SystemDataRegistry.Instance.DocManagerDataFileSizeThresholdGb.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var result = documentDbMerger.QuantityMbsNecessaryToMove(collection);

				// Assert
				// SD001 gets 1024 MB data from SD004
				// SD002 becomes the only writable database with free space
				// SD003 will not be merged as it is readonly.
				AssertEquals(1024, result);
			}
		}

		[UseSnapshotProtection]
		public void TestMergeDatabasesWaitUntilAllProviderSuccess()
		{
			// Arrange
			var storageDb2 = DocManagerDBHelper.GetDatabaseNameFromNumber(2);
			AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value = dbName => false;
			var failedBacklogResult = new AttemptInGettingBacklog(true, string.Empty,
				new BacklogResult()
				{
					BacklogSize = 100
				});
			var passedBacklogResult = new AttemptInGettingBacklog(true, string.Empty,
				new BacklogResult()
				{
					BacklogSize = 0
				});
			var logFullnessProviderMockForSourceDb = new Mock<LogFullnessProvider>();
			logFullnessProviderMockForSourceDb
				.SetupSequence(x => x.GetCurrentBacklog())
				.Returns(failedBacklogResult)
				.Returns(() =>
				{
					logFullnessProviderMockForSourceDb
						.Setup(x => x.GetCurrentBacklog())
						.Returns(passedBacklogResult);
					return passedBacklogResult;
				});
			var alwaysOnDelayProviderMockForSourceDb = new Mock<AlwaysOnDelayProvider>(string.Empty);
			alwaysOnDelayProviderMockForSourceDb
				.SetupSequence(x => x.GetCurrentBacklog())
				.Returns(failedBacklogResult)
				.Returns(() =>
				{
					alwaysOnDelayProviderMockForSourceDb
						.Setup(x => x.GetCurrentBacklog())
						.Returns(passedBacklogResult);
					return passedBacklogResult;
				});

			var lowPriorityProcessPauserFactoryMock = new Mock<ILowPriorityProcessPauserFactory>();
			lowPriorityProcessPauserFactoryMock
				.SetupSequence(x => x.Create(It.IsAny<IBacklogInfoProvider[]>()))
				.Returns(new LowPriorityProcessPauser(new IBacklogInfoProvider[] { logFullnessProviderMockForSourceDb.Object, alwaysOnDelayProviderMockForSourceDb.Object }))
				.Returns(() =>
				{
					var returnValue = new LowPriorityProcessPauser(new IBacklogInfoProvider[] { Mock.Of<LogFullnessProvider>(x => x.GetCurrentBacklog() == passedBacklogResult) });
					lowPriorityProcessPauserFactoryMock
						.Setup(x => x.Create(It.IsAny<IBacklogInfoProvider[]>()))
						.Returns(returnValue);
					return returnValue;
				});

			var factory = new DocumentFactoryProvider()
				.GetFactory(new BusinessObjectFactory());
			var documentDbMergerMock = new Mock<DocumentDbMerger>(factory, lowPriorityProcessPauserFactoryMock.Object)
			{
				CallBase = true
			};
			documentDbMergerMock
				.Protected()
				.Setup<bool>("IsDatabaseReadyToDrop", ItExpr.IsAny<DbConnection>(), ItExpr.IsAny<string>())
				.Returns(true);

			var runTaskCancellationTokenSource = new CancellationTokenSource();
			using (new DisposableAction(() => runTaskCancellationTokenSource.Cancel()))
			using (new DisposableAction(() => AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.ResetValue()))
			using (AdoTestUtils.CreateDbDropExistingDisposable(DocManagerDBHelper.GetDatabaseNameFromNumber(1)))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb2))
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				// Act
				var runTask = Task.Run(() => documentDbMergerMock.Object.Run(), runTaskCancellationTokenSource.Token);

				// Assert
				Assert("Fail to wait", runTask.Wait(30 * 1000));
				Assert("Db still exist", !Db.Connection.DatabaseExists(storageDb2));
				lowPriorityProcessPauserFactoryMock.Verify(x => x.Create(It.Is<IBacklogInfoProvider[]>(
					y => y.Length == 2
					&& y[0] is LogFullnessProvider
					&& y[1] is AlwaysOnDelayProvider)), Times.Exactly(2));
				lowPriorityProcessPauserFactoryMock.VerifyNoOtherCalls();
				logFullnessProviderMockForSourceDb.Verify(x => x.GetCurrentBacklog(), Times.Exactly(3));
			}
		}

		[UseSnapshotProtection]
		public void TestQuantityMbsNecessaryToMove()
		{
			// Arrange
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var documentDbMerger = new DocumentDbMerger(factory);
			var internalDictionary = new Dictionary<int, DbMergeInfo>
			{
				{ 1, new DbMergeInfo(1, "1", 1024, false) },
				{ 2, new DbMergeInfo(2, "2", 1024, false) },
				{ 3, new DbMergeInfo(3, "3", 3, true) },
				{ 4, new DbMergeInfo(4, "4", 3072, false) },
				{ 5, new DbMergeInfo(5, "5", 3071, false) },
			};
			var collection = new DbMergeInfoCollectionForTesting(internalDictionary);

			// Act
			using (SystemDataRegistry.Instance.DocManagerDataFileSizeThresholdGb.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var result = documentDbMerger.QuantityMbsNecessaryToMove(collection);

				// Assert
				// SD001 gets 1024 MB data from SD005
				// SD002 gets another 1024 MB data from SD005
				// SD003 will not be merged as it is readonly.
				AssertEquals(2048, result);
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestDoesNotRunWhenAlwaysOnResourceLockOccupied()
		{
			// Arrange
			var storageDb2 = DocManagerDBHelper.GetDatabaseNameFromNumber(2);
			var exclusiveResource = "AlwaysOnExclusiveLockKey";
			var notGetAlwaysOnResourceMessage = $"Failed to get AppLock for {storageDb2} with Lock result {LockedProcessResult.AlreadyBeingProcessed}. Please try again after other process finishes its task.";

			AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value = dbName => true;
			AlwaysOn.AlwaysOnDatabases_ForTest.Value = new List<string>() { storageDb2 };
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = Db.ServerName, AvailabilityMode = 1 },
			};

			var callBackEvent = new Mock<NotificationEvent>();
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var dbMerger = new DocumentDbMerger(factory);
			dbMerger.OnProcessFailed += callBackEvent.Object;

			using (new DisposableAction(() => AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.ResetValue()))
			using (new DisposableAction(() => AlwaysOn.AlwaysOnDatabases_ForTest.ResetValue()))
			using (new DisposableAction(() => AlwaysOn.ReplicaNames_ForTest.ResetValue()))
			using (AdoTestUtils.CreateDbDropExistingDisposable(DocManagerDBHelperTestClass.GetDatabaseNameFromNumber(1)))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb2))
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				extraConnection.RunLocked(
					exclusiveResource,
					_ =>
					{
						// Act
						dbMerger.Run();
					},
					max_tries: 1,
					dbName: Db.DatabaseName);

				// Assert
				callBackEvent.Verify(
					x => x.Invoke(It.Is<string>(y => y == $"Merge has stopped with an error, please fix it and try again.\r\nFailed to get AppLock for {storageDb2} with Lock result {LockedProcessResult.AlreadyBeingProcessed}. Please try again after other process finishes its task.")),
					Times.Once);
			}
		}

		[UseSnapshotProtection]
		public void TestNotStuckWhenReplicaDbDoesNotExist()
		{
			// Arrange
			var storageDb2 = DocManagerDBHelper.GetDatabaseNameFromNumber(2);

			AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value = dbName => true;
			AlwaysOn.AlwaysOnDatabases_ForTest.Value = new List<string>() { storageDb2 };
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = Db.ServerName, AvailabilityMode = 1 },
			};

			var callBackEvent = new Mock<NotificationEvent>();
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var dbMerger = new Mock<DocumentDbMerger>(factory, null) { CallBase = true };
			dbMerger
				.Protected()
				.Setup<bool>("IsDatabaseReadyToDrop", ItExpr.IsAny<DbConnection>(), ItExpr.IsAny<string>())
				.Callback(() =>
				{
					using (var dbConnection = Db.NewAdminConnection())
					{
						AdoTestUtils.DropDbIfExists(dbConnection, storageDb2);
					}
				})
				.CallBase();
			dbMerger.Object.OnProcessFailed += callBackEvent.Object;

			using (new DisposableAction(() => AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.ResetValue()))
			using (new DisposableAction(() => AlwaysOn.AlwaysOnDatabases_ForTest.ResetValue()))
			using (new DisposableAction(() => AlwaysOn.ReplicaNames_ForTest.ResetValue()))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb2))
			{
				var runningTask = Task.Run(() =>
				{
					// Act
					dbMerger.Object.Run();
				});

				// Assert
				Assert(runningTask.Wait(30 * 1000));
				callBackEvent.Verify(
					x => x.Invoke(It.Is<string>(y => y.Contains("Merge has stopped with an error, please fix it and try again."))),
					Times.Never);
			}
		}

		[UseSnapshotProtection, RequiresSoftware(RequiredSoftware.IsVM)]
		public void TestDropEmptyDatabaseOnSecondaryReplicaDoesNotConnectToMainDb()
		{
			// Arrange
			var storageDb2 = DocManagerDBHelper.GetDatabaseNameFromNumber(2);
			var storageDb3 = DocManagerDBHelper.GetDatabaseNameFromNumber(3);
			var alwaysOnDatabases = new List<string>(new[] { storageDb2, storageDb3 });
			AlwaysOn.AlwaysOnDatabases_ForTest.Value = alwaysOnDatabases;
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = Db.ServerName, AvailabilityMode = 1 },
			};

			AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value = dbName => true;

			using (new DisposableAction(() => AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.ResetValue()))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb2))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb3))
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var replicaDbName = string.Empty;
				var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				var documentDbMergerMock = new Mock<DocumentDbMerger>(factory, null) { CallBase = true };
				var documentDbMerger = documentDbMergerMock.Object;
				documentDbMergerMock.Protected()
					.Setup<bool>("IsDatabaseReadyToDrop", ItExpr.IsAny<DbConnection>(), ItExpr.IsAny<string>())
					.Callback((DbConnection connection, string dbName) =>
					{
						replicaDbName = connection.CurrentDatabase;
					})
					.Returns(true);

				// Act
				documentDbMerger.Run();

				// Assert
				CombineAssertions(() =>
				{
					AssertNotNullOrEmpty(replicaDbName);
					AssertNotEquals(Db.DatabaseName, replicaDbName);
					AssertEquals("master", replicaDbName);
				});
			}
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestGetDbMergeInfoCollectionAndRefreshUserFeedback_SqlExceptionWithInvalidObjectName()
		{
			var ep_StorageDocsMerge_AllDbSizes = $@"
ALTER PROCEDURE [dbo].[ep_StorageDocsMerge_AllDbSizes]
AS
BEGIN
	Select * from {Db.DatabaseName}_InvalidSD.sys.allocation_units
END";
			using (var adminCollection = Db.NewAdminConnection())
			{
				adminCollection.ExecuteNonQuery(ep_StorageDocsMerge_AllDbSizes);
			}

			var documentDbMerger = new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()));
			AssertNoExceptionThrown(() => documentDbMerger.GetDbMergeInfoCollectionAndRefreshUserFeedback());
		}

		#region TestMergeDatabases

		[UseSnapshotProtection]
		public void TestMergeDatabases()
		{
			mergerHelper = new DbMergerTestDataHelper(SetupDbCount, TestImageData);
			SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);

			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					mergerHelper.PrepareTestDbs(adminConnection, 2, 7);
					RunAndAssert(adminConnection);
				}
				finally
				{
					mergerHelper.CleanupTestData(adminConnection);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestMergeDatabases_ShouldDropEmptyDb()
		{
			TestMergeDatabases_InitialDbWithData(false);
		}

		[UseSnapshotProtection]
		public void TestMergeDatabases_ShouldKeepDbWithData()
		{
			TestMergeDatabases_InitialDbWithData(true);
		}

		void TestMergeDatabases_InitialDbWithData(bool sd2HasData)
		{
			SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);

			var factory = new BusinessObjectFactory();
			var masterFactory = new DbBackendDocumentFactory(factory);

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			using (Db.DisposableActionForDbConnection())
			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					mergerHelper = new DbMergerTestDataHelper(3, TestImageData);
					mergerHelper.PrepareTestDbs(adminConnection);
					mergerHelper.NewTestStorageMainAndDocs(adminConnection, 1, 5);
					if (sd2HasData)
					{
						mergerHelper.NewTestStorageMainAndDocs(adminConnection, 2, 1);
					}
					mergerHelper.NewTestStorageMainAndDocs(adminConnection, 3, 1);

					var logMessages = new List<string>();
					var testDbMerger = new DocumentDbMergerForTesting(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), 8);
					testDbMerger.OnShowMessage += logMessages.Add;
					testDbMerger.OnNoMergeRequired += logMessages.Add;
					testDbMerger.Run();

					var mergeInfoItems = DbMergeInfoCollection.New(adminConnection, 8);

					AssertEquals("Database count should be 2", 2, mergeInfoItems.Count);
					AssertEquals("DB 2 is writeable", 2, mergeInfoItems.FirstWritableDbWithFreeSpace.Number);
					AssertEquals("DB 1 is not writeable", 1, mergeInfoItems.CountWritableWithFreeSpace);

					AssertEquals("Merge has been completed successfully.", logMessages[4]);

					var expectedWarning = $@"Last writable database is {Db.DatabaseName.Trim()}_SD002.
Asterisk(*) indicate read-only database.
Databases that are read-only or larger than or equal to the threshold size set in the Registry ( System -> DocManager -> DocManager Database Size Threshold ) will not be merged.";
					AssertEquals(string.Empty, logMessages[5]);
					AssertEquals(expectedWarning, logMessages[6]);
				}
				finally
				{
					mergerHelper.CleanupTestData(adminConnection);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestMergeDatabases_WithDuplication()
		{
			mergerHelper = new DbMergerTestDataHelper(2, TestImageData);
			SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);

			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					mergerHelper.PrepareTestDbs(adminConnection);
					mergerHelper.AddDuplicationToStorageDocs(adminConnection, 2, 1);

					var mergeInfoItems = DbMergeInfoCollection.New(adminConnection, 80);
					AssertGreaterThanOrEqualTo("Before merge, SD DB Count", mergeInfoItems.Count, 2);
					AssertEquals("Before merge, StorageMain with SM_DB = 1", 0, mergerHelper.CountStorageMain(adminConnection, 1));
					AssertEquals("Before merge, StorageMain with SM_DB = 2", 1, mergerHelper.CountStorageMain(adminConnection, 2));
					AssertEquals("Before merge, SD001 record count", 1, mergerHelper.CountStorageDocs(adminConnection, 1));
					AssertEquals("Before merge, SD002 record count", 1, mergerHelper.CountStorageDocs(adminConnection, 2));

					var testDbMerger = new DocumentDbMergerForTesting(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), 80);
					testDbMerger.OnProcessFailed += (_) => Assert("Merge failed", false);
					testDbMerger.Run();

					mergeInfoItems = DbMergeInfoCollection.New(adminConnection, 80);
					AssertEquals("After merge, SD DB Count", 1, mergeInfoItems.Count);
					AssertEquals("After merge, StorageMain with SM_DB = 1", 1, mergerHelper.CountStorageMain(adminConnection, 1));
					AssertEquals("After merge, SD001 record count", 1, mergerHelper.CountStorageDocs(adminConnection, 1));
				}
				finally
				{
					mergerHelper.CleanupTestData(adminConnection);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestMergeDatabases_WithEDocsUploading()
		{
			SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);
			SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath);

			var factory = new BusinessObjectFactory();
			var masterFactory = new DbBackendDocumentFactory(factory);
			var eDocsAdded = false;

			using (Db.DisposableActionForDbConnection())
			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					mergerHelper = new DbMergerTestDataHelper(SetupDbCount, TestImageData);
					mergerHelper.PrepareTestDbs(adminConnection);

					var logMessages = new List<string>();
					var documentDbMergerMock = new Mock<DocumentDbMerger>(masterFactory, null) { CallBase = true };
					var documentDbMerger = documentDbMergerMock.Object;
					documentDbMergerMock.Protected()
						.Setup("DropDbWithLock", ItExpr.IsAny<DbConnection>(), ItExpr.IsAny<int>())
						.Callback((DbConnection connection, int dbNumber) =>
						{
							if (dbNumber == 6 && !eDocsAdded)
							{
								try
								{
									AddEDocs(dbNumber);
								}
								catch { }
								finally
								{
									eDocsAdded = true;
								}
							}
						})
						.CallBase();
					documentDbMerger.OnShowMessage += logMessages.Add;

					// Act
					documentDbMerger.Run();

					var log = string.Join(",", logMessages);
					AssertNotEquals($"Should not be empty", "", log);
					AssertNotContains($"Should not contain 'Cannot drop database'", "Cannot drop database [6]", log, true);
					AssertNotContains($"Should not contain 'it is not empty'", "it is not empty", log, true);
				}
				finally
				{
					mergerHelper.CleanupTestData(adminConnection);
				}
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestMergeDatabases_ResetReadOnlyWhenException()
		{
			// Arrange
			var storageDb1 = DocManagerDBHelper.GetDatabaseNameFromNumber(1);
			var storageDb2 = DocManagerDBHelper.GetDatabaseNameFromNumber(2);
			var exclusiveResource = "AlwaysOnExclusiveLockKey";
			var notGetAlwaysOnResourceMessage = $"Failed to get AppLock for {storageDb2} with Lock result {LockedProcessResult.AlreadyBeingProcessed}. Please try again after other process finishes its task.";

			AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.Value = dbName => true;
			AlwaysOn.AlwaysOnDatabases_ForTest.Value = new List<string>() { storageDb2 };
			AlwaysOn.ReplicaNames_ForTest.Value = new List<AlwaysOnReplicaInfo>
			{
				new AlwaysOnReplicaInfo { ReplicaServerName = Db.ServerName, AvailabilityMode = 1 },
			};

			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var helper = new DocManagerDBHelper();

			using (new DisposableAction(() => AlwaysOn.OverridableRemoveDatabaseFromAlwaysOnSetupForTest.ResetValue()))
			using (new DisposableAction(() => AlwaysOn.AlwaysOnDatabases_ForTest.ResetValue()))
			using (new DisposableAction(() => AlwaysOn.ReplicaNames_ForTest.ResetValue()))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb1))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb2))
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var dbMerger = new DocumentDbMerger(factory);
				dbMerger.OnProcessFailed += (msg) =>
				{
					AssertEquals("SD_001 should be writeable", DbWriteableState.Writeable, helper.GetDbWriteableState(1));
					AssertEquals("SD_002 should be writeable", DbWriteableState.Writeable, helper.GetDbWriteableState(2));
				};

				extraConnection.RunLocked(
					exclusiveResource,
					_ =>
					{
						// Act
						dbMerger.Run();
					},
					max_tries: 1,
					dbName: Db.DatabaseName);
			}
		}

		void AddEDocs(int dbNumber)
		{
			using (Db.DisposableActionForDbConnection())
			{
				using var dbConnection = Db.NewExtraConnectionToMainDb();
				var factory = new BusinessObjectFactory(dbConnection);
				var masterFactory = new DbBackendDocumentFactory(factory);

				var shipment = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));
				shipment["JS_UniqueConsignRef"] = $"EBM22Q33TU475BXH3P06";

				var storageMain = masterFactory.New<StorageMain>();
				storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				storageMain.SM_ParentFK = shipment.PK;
				storageMain.SM_DB = masterFactory.LastWriteableDatabaseWithFreeSpace();
				factory.Save();
				masterFactory.Save();

				var file = storageMain.eDocs.Factory.New<StorageDocs>();
				file.SC_SM = storageMain.PK;
				file.SC_DocType = Core.Constants.RefDocTypes.PackingList;
				file.SC_FileName = "test.txt";
				file.SC_Desc = "test desc";
				file.SC_ImageData = new byte[] { 1, 2, 3 };

				factory.Save();
				masterFactory.Save();
			}
		}

		void RunAndAssert(AdminConnection conn)
		{
			var mergeInfoItems = DbMergeInfoCollection.New(conn, 0);
			AssertEquals("[PRE-CONDITION] Total DB count", SetupDbCount, mergeInfoItems.Count);

			var db1Count = 4;
			var db2Count = 0;
			var db3Count = 3;
			var db4Count = 6;
			var db5Count = 2;
			var db6Count = 4;
			var db7Count = 0;
			var totalCount = db1Count + db2Count + db3Count + db4Count + db5Count + db6Count + db7Count;

			mergerHelper.NewTestStorageMainAndDocs(conn, 1, db1Count);
			mergerHelper.NewTestStorageMainAndDocs(conn, 3, db3Count);
			mergerHelper.NewTestStorageMainAndDocs(conn, 4, db4Count);
			mergerHelper.NewTestStorageMainAndDocs(conn, 5, db5Count);
			mergerHelper.NewTestStorageMainAndDocs(conn, 6, db6Count);

			AssertEquals("DB 1 initial record count", db1Count, mergerHelper.CountStorageDocs(conn, 1));
			AssertEquals("DB 2 initial record count", db2Count, mergerHelper.CountStorageDocs(conn, 2));
			AssertEquals("DB 3 initial record count", db3Count, mergerHelper.CountStorageDocs(conn, 3));
			AssertEquals("DB 4 initial record count", db4Count, mergerHelper.CountStorageDocs(conn, 4));
			AssertEquals("DB 5 initial record count", db5Count, mergerHelper.CountStorageDocs(conn, 5));
			AssertEquals("DB 6 initial record count", db6Count, mergerHelper.CountStorageDocs(conn, 6));
			AssertEquals("DB 7 initial record count", db7Count, mergerHelper.CountStorageDocs(conn, 7));

			var totalCountBeforeMerge = AssertCountPerDbAndGetTotalCount(conn, mergeInfoItems.Count);
			AssertEquals("Total Record Count", totalCount, totalCountBeforeMerge);
			AssertEquals("Total Record Count", true, totalCount > 0);

			// RUN MERGE
			var testDbMerger = new DocumentDbMergerForTesting(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), 8);
			testDbMerger.Run();

			mergeInfoItems = DbMergeInfoCollection.New(conn, 8);

			AssertEquals("Some DBs were dropped", true, mergeInfoItems.Count < SetupDbCount);

			if (mergeInfoItems.FirstWritableDbWithFreeSpace != null)
			{
				AssertEquals("DB 1 is full", true, mergeInfoItems.FirstWritableDbWithFreeSpace.Number != 1);
				AssertEquals("Only 1 writable DB has free space", 1, mergeInfoItems.CountWritableWithFreeSpace);
				AssertEquals("1st writable DB with free space (= Last writable DB)",
					mergeInfoItems.LastWritableDatabase.Number, mergeInfoItems.FirstWritableDbWithFreeSpace.Number);
			}

			var totalCountAfterMerge = AssertCountPerDbAndGetTotalCount(conn, SetupDbCount);
			AssertEquals("Total Record Count", totalCount, totalCountAfterMerge);
		}

		int AssertCountPerDbAndGetTotalCount(DbConnection conn, int dbCount)
		{
			var result = 0;

			for (var i = 1; i <= dbCount; i++)
			{
				if (dbHelper.DatabaseExists(i))
				{
					var countInDb = mergerHelper.CountStorageDocs(conn, i);
					AssertEquals("Count in DB = count in StorageMain" + i, countInDb, mergerHelper.CountStorageMain(conn, i));

					result += countInDb;
				}
			}

			return result;
		}

		#endregion

		#region TestLogging

		[UseSnapshotProtection]
		public void TestLogging()
		{
			using (SystemDataRegistry.Instance.DocManagerDBDataFilePath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath))
			using (SystemDataRegistry.Instance.DocManagerDBLogFilePath.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Temp.TempPath))
			{
				mergerHelper = new DbMergerTestDataHelper(5, TestImageData);
				try
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						mergerHelper.PrepareTestDbs(adminConnection);
					}
					mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 1, 8);
					mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 2, 0);
					mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 3, 0);
					mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 4, 0);
					mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 5, 10);

					var logMessages = new List<string>();
					var logErrorMessages = new List<string>();
					var testDbMerger = new DocumentDbMergerForTesting(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), 8);
					testDbMerger.OnShowMessage += logMessages.Add;
					testDbMerger.OnNoMergeRequired += logMessages.Add;
					testDbMerger.OnProcessFailed += logErrorMessages.Add;
					testDbMerger.Run();

					if (logErrorMessages.Count > 0)
					{
						var message = string.Join("\r\n", logErrorMessages.ToArray());
						Assert(message, false);
					}

					AssertEquals("DocManager database merge started.", logMessages[0]);
					AssertEquals("Moving documents from database [5] to database [2]", logMessages[1]);
					AssertEquals("Moving documents from database [5] to database [3]", logMessages[2]);
					AssertEquals($"Empty database [5] on server [{Db.Connection.ServerName}] has been dropped", logMessages[4]);
					AssertEquals($"Empty database [4] on server [{Db.Connection.ServerName}] has been dropped", logMessages[6]);
					AssertEquals("Merge has been completed successfully.", logMessages[7]);
				}
				finally
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						mergerHelper.CleanupTestData(adminConnection);
					}
				}
			}
		}

		#endregion

		#region TestDropDb

		public void TestDropDbOnSecondaryReplica()
		{
			// Arrange
			var storageDb1 = DocManagerDBHelper.GetDatabaseNameFromNumber(1);
			var storageDb2 = DocManagerDBHelper.GetDatabaseNameFromNumber(2);
			var storageDb3 = DocManagerDBHelper.GetDatabaseNameFromNumber(3);

			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb1))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb2))
			using (AdoTestUtils.CreateDbDropExistingDisposable(storageDb3))
			{
				var logMessages = new List<string>();
				var documentDbMerger = new DocumentDbMergerForTesting(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), 8);
				documentDbMerger.OnShowMessage += logMessages.Add;

				// Act
				using (var connection = Db.NewAdminConnection(Db.ServerName, Db.SqlMsdb))
				{
					documentDbMerger.DropDbOnSecondaryReplica_Exposed(connection, 1);
					documentDbMerger.DropDbOnSecondaryReplica_Exposed(connection, 2);
					documentDbMerger.DropDbOnSecondaryReplica_Exposed(connection, 3);
					documentDbMerger.DropDbOnSecondaryReplica_Exposed(connection, 4);
				}

				// Assert
				AssertEquals($"Empty database [1] on server [{Db.Connection.ServerName}] has been dropped", logMessages[0]);
				AssertEquals($"Empty database [2] on server [{Db.Connection.ServerName}] has been dropped", logMessages[1]);
				AssertEquals($"Empty database [3] on server [{Db.Connection.ServerName}] has been dropped", logMessages[2]);
				AssertEquals($"Cannot drop database [4] on server [{Db.Connection.ServerName}] - it does not exist", logMessages[3]);
			}
		}

		public void TestDropDb()
		{
			mergerHelper = new DbMergerTestDataHelper(4, TestImageData);
			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					mergerHelper.PrepareTestDbs(adminConnection);
				}
				mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 1, 10);
				mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 2, 0);
				mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 3, 0);
				mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 4, 10);

				var logMessages = new List<string>();
				var testDbMerger = new DocumentDbMergerForTesting(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), 8);
				testDbMerger.OnShowMessage += logMessages.Add;

				using (var connection = Db.NewAdminConnection())
				{
					testDbMerger.DropDbWithLock_Exposed(connection, 1);
					testDbMerger.DropDbWithLock_Exposed(connection, 2);
					testDbMerger.DropDbWithLock_Exposed(connection, 2);
					testDbMerger.DropDbWithLock_Exposed(connection, 3);
					testDbMerger.DropDbWithLock_Exposed(connection, 4);
					testDbMerger.DropDbWithLock_Exposed(connection, 5);
				}

				AssertEquals($"Cannot drop database [1] on server [{Db.Connection.ServerName}] - it is not empty", logMessages[0]);
				AssertEquals($"Empty database [2] on server [{Db.Connection.ServerName}] has been dropped", logMessages[1]);
				AssertEquals($"Cannot drop database [2] on server [{Db.Connection.ServerName}] - it does not exist", logMessages[2]);
				AssertEquals($"Empty database [3] on server [{Db.Connection.ServerName}] has been dropped", logMessages[3]);
				AssertEquals($"Cannot drop database [4] on server [{Db.Connection.ServerName}] - it is not empty", logMessages[4]);
				AssertEquals($"Cannot drop database [5] on server [{Db.Connection.ServerName}] - it does not exist", logMessages[5]);
			}
			finally
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					mergerHelper.CleanupTestData(adminConnection);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDropDbWithLockTakesLockOnStorageMainTable()
		{
			mergerHelper = new DbMergerTestDataHelper(1, TestImageData);
			try
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					mergerHelper.PrepareTestDbs(adminConnection);
				}
				mergerHelper.NewTestStorageMainAndDocs(Db.Connection, 1, 1);

				var testDbMerger = new DocumentDbMergerForDropMutexTesting(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()));

				using (var adminConnection = Db.NewAdminConnection())
				{
					testDbMerger.DropDbWithLock_Exposed(adminConnection, 1);
				}

				Fail("Should have thrown an exception");
			}
			catch (SqlException ex)
			{
				var error = new DbErrorMatch(ex);
				AssertEquals($"Expected error:", DbErrorType.LockTimeoutExpired, error.ExceptionType);
			}
			finally
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					mergerHelper.CleanupTestData(adminConnection);
				}
			}
		}
		#endregion

		#region Test Initialise on Single User DB Mode

		public void TestInitialiseOnSingleUserDBMode()
		{
			var singleUserDBId = 3;
			var oldDBUserMode = string.Empty;
			var logs = new List<string>();
			var exceptionErrors = new List<string>();

			using (var mainConn = Db.NewAdminConnection())
			{
				try
				{
					mergerHelper = new DbMergerTestDataHelper(4, TestImageData);
					var sqlText = $"SELECT user_access_desc FROM sys.databases WHERE [name]='{dbHelper.GetDatabaseName(singleUserDBId)}';";
					oldDBUserMode = Convert.ToString(mainConn.ExecuteScalar(sqlText));

					mergerHelper.PrepareTestDbs(mainConn);
					if (string.IsNullOrEmpty(oldDBUserMode) || !string.Equals(oldDBUserMode, "SINGLE_USER"))
					{
						mergerHelper.SetDatabaseUserAccess(mainConn, singleUserDBId, "SINGLE_USER");
					}

					mainConn.CloseConnection();

					using (var singleUserDBConn = Db.NewAdminConnection(dbHelper.GetDatabaseName(singleUserDBId)))
					{
						singleUserDBConn.EnsureIsOpen();
						var testDbMerger = new DocumentDbMergerForTesting(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()), 8);
						testDbMerger.OnProcessFailed += logs.Add;
						testDbMerger.Initialise(null);
					}
				}
				catch (Exception ex)
				{
					exceptionErrors.Add(ex.Message);
				}
				finally
				{
					mergerHelper.CleanupTestData(mainConn);
					if (!string.IsNullOrEmpty(oldDBUserMode))
					{
						mergerHelper.SetDatabaseUserAccess(mainConn, singleUserDBId, oldDBUserMode);
					}
				}

				var expectedError = $@"{Res.GetString("5dda2334-0f20-4352-b184-972764a75c13", "Please correct any errors and try again.")}
Database '{dbHelper.GetDatabaseName(singleUserDBId)}' is already open and can only have one user at a time.";

				CombineAssertions(() =>
				{
					AssertMultilineASCIIEquals("Initialise must not throw any exceptions (have to be shown as logs)", "", string.Join("\n", exceptionErrors.ToArray()));
					AssertEquals("Message must contain exception", expectedError, logs.Count > 0 ? logs[0] : string.Empty);
				});
			}
		}

		#endregion

		#region Implementation

		const int SetupDbCount = 7;
		DbMergerTestDataHelper mergerHelper;
		readonly DocManagerDBHelperTestClass dbHelper = new DocManagerDBHelperTestClass();

		protected override void TearDown()
		{
			DbCommitTracker.Ignore(DbMergerTestDataHelper.TestDataTag);
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] TestImageData => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.1MB.dat");

		#endregion
	}

	[TestedType(typeof(DocumentDbMerger))]
	class DocumentDbMergerRequiredTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentDbMerger(new DocumentFactoryProvider().GetFactory(Factory));
		}
	}

	#region Test Helper Classes

	abstract class BaseDocumentDbMergerForTesting : DocumentDbMerger
	{
		protected BaseDocumentDbMergerForTesting(DocumentFactory factory)
			: base(factory)
		{
		}

		public void DropDbWithLock_Exposed(AdminConnection connection, int dbNumber)
		{
			DropDbWithLock(connection, dbNumber);
		}

		public void DropDbOnSecondaryReplica_Exposed(DbConnection connection, int dbNumber)
		{
			DropDbOnSecondaryReplica(connection, dbNumber);
		}
	}

	class DocumentDbMergerForTesting : BaseDocumentDbMergerForTesting
	{
		public DocumentDbMergerForTesting(DocumentFactory factory, int testMaxDbSizeMb)
			: base(factory)
		{
			MaxDbSizeMb = testMaxDbSizeMb;
		}

		public override int MaxDbSizeMb { get; }
	}

	class DocumentDbMergerForDropMutexTesting : BaseDocumentDbMergerForTesting
	{
		public DocumentDbMergerForDropMutexTesting(DocumentFactory factory)
			: base(factory)
		{
		}

		protected override void DropDb(DbConnection connection, int dbNumber)
		{
			using (var conn = Db.NewExtraConnectionToMainDb())
			using (conn.BeginTransactionWithManager())
			{
				var sqlScript = $@"
SET lock_timeout 3;
INSERT INTO dbo.StorageMain (SM_PK, SM_CD1, SM_CD2, SM_DB, SM_ParentFK, SM_Type, SM_PhysicalLocation)
	VALUES (NEWID(), 0, 0, {dbNumber}, NEWID(), 'XXX', '')
";
				conn.ExecuteNonQuery(sqlScript);
			}
		}
	}

	class DbMergerTestDataHelper
	{
		public DbMergerTestDataHelper(int testDbCount, byte[] testImageData)
		{
			this.testDbCount = testDbCount;
			this.testImageData = testImageData;
		}

		public void NewTestStorageMainAndDocs(DbConnection conn, int dbNumber, int numberOfDocuments)
		{
			for (var i = 0; i < numberOfDocuments; i++)
			{
				var storageMainPk = NewStorageMain(conn, dbNumber);
				NewTestStorageDocs(conn, dbNumber, storageMainPk);
			}
		}

		public void AddDuplicationToStorageDocs(DbConnection conn, int dbNumberCurrent, int dbNumberDuplicated)
		{
			var storageMainPk = NewStorageMain(conn, dbNumberCurrent);
			var storageDocsPk = Guid.NewGuid();
			NewTestStorageDocs(conn, dbNumberCurrent, storageMainPk, storageDocsPk);
			NewTestStorageDocs(conn, dbNumberDuplicated, storageMainPk, storageDocsPk);
		}

		Guid NewStorageMain(DbConnection conn, int dbNumber)
		{
			var storageMainPk = Guid.NewGuid();
			var insertStorageMainSql = string.Format(
				"INSERT dbo.StorageMain (SM_PK, SM_ParentFK, SM_DB, SM_PhysicalLocation) VALUES ('{0}', '{0}', {1}, '{2}');",
				storageMainPk.ToString(), dbNumber, TestDataTag);
			conn.ExecuteNonQuery(insertStorageMainSql);

			return storageMainPk;
		}

		void NewTestStorageDocs(DbConnection conn, int dbNumber, Guid storageMainPk)
		{
			var insertStorageDocsSql = $"INSERT [{dbHelper.GetDatabaseName(dbNumber)}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData, SC_FileName) VALUES (newid(), '{storageMainPk.ToString()}', sysutcdatetime(), sysutcdatetime(), sysutcdatetime(), @BlobData, '{TestDataTag}');";

			using (var cmd = conn.Command(insertStorageDocsSql))
			{
				cmd.AddParameter("@BlobData", System.Data.SqlDbType.VarBinary, testImageData);
				cmd.ExecuteNonQuery();
			}
		}

		void NewTestStorageDocs(DbConnection conn, int dbNumber, Guid storageMainPk, Guid sc_pk)
		{
			var insertStorageDocsSql = $"INSERT [{dbHelper.GetDatabaseName(dbNumber)}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData, SC_FileName) VALUES ('{sc_pk}', '{storageMainPk}', sysutcdatetime(), sysutcdatetime(), sysutcdatetime(), @BlobData, '{TestDataTag}');";

			using (var cmd = conn.Command(insertStorageDocsSql))
			{
				cmd.AddParameter("@BlobData", System.Data.SqlDbType.VarBinary, testImageData);
				cmd.ExecuteNonQuery();
			}
		}

		public int CountStorageDocs(DbConnection conn, int dbNumber)
		{
			var sqlText = $"SELECT count(*) FROM [{dbHelper.GetDatabaseName(dbNumber)}]..StorageDocs;";
			var result = Convert.ToInt32(conn.ExecuteScalar(sqlText));
			return result;
		}

		public int CountStorageMain(DbConnection conn, int dbNumber)
		{
			var sqlText = $"SELECT count(*) FROM dbo.StorageMain WHERE SM_DB = {dbNumber} AND SM_PhysicalLocation = '{TestDataTag}';";
			var result = Convert.ToInt32(conn.ExecuteScalar(sqlText));
			return result;
		}

		public void PrepareTestDbs(AdminConnection conn, params int[] readOnlyDbs)
		{
			CleanupTestData(conn);

			for (var i = 1; i <= testDbCount; i++)
			{
				if (!dbHelper.DatabaseExists(i))
				{
					dbHelper.CreateDatabase(i);
				}
			}

			foreach (var readOnlyDbNumber in readOnlyDbs)
			{
				MakeDatabaseReadOnly(conn, readOnlyDbNumber);
			}
		}

		public void CleanupTestData(AdminConnection conn)
		{
			ClearStorageMain(conn);
			ClearStorageDocs(conn, 1);
			DropTestDatabases(conn);
		}

		void ClearStorageDocs(AdminConnection conn, int dbNumber)
		{
			var dbName = dbHelper.GetDatabaseName(dbNumber);

			if (dbHelper.DatabaseExists(dbNumber))
			{
				var sqlText = string.Format("IF (EXISTS(SELECT null FROM sys.databases WHERE name = '{0}')) TRUNCATE TABLE {0}..StorageDocs", dbName);
				conn.ExecuteNonQuery(sqlText);
			}
		}

		void ClearStorageMain(AdminConnection conn)
		{
			var sqlText = $"DELETE dbo.StorageMain WHERE SM_PhysicalLocation = '{TestDataTag}'";
			conn.ExecuteNonQuery(sqlText);
		}

		void DropTestDatabases(AdminConnection conn)
		{
			for (var i = 1; i <= testDbCount; i++)
			{
				var dbName = dbHelper.GetDatabaseName(i);
				var sqlText = string.Format("IF exists(SELECT null FROM sys.databases WHERE name = '{0}') DROP DATABASE [{0}];", dbName);
				conn.ExecuteNonQuery(sqlText);
			}
		}

		void MakeDatabaseReadOnly(AdminConnection conn, int dbNumber)
		{
			var dbName = dbHelper.GetDatabaseName(dbNumber);
			conn.AlterDbWriteableStateForDocManager(dbName, false);
		}

		public void SetDatabaseUserAccess(AdminConnection conn, int dbNumber, string userAccessMode)
		{
			var dbName = dbHelper.GetDatabaseName(dbNumber);
			var sqlText = $"ALTER DATABASE [{dbName}] SET {userAccessMode} WITH ROLLBACK IMMEDIATE;";
			conn.ExecuteNonQuery(sqlText);
		}

		readonly DocManagerDBHelperTestClass dbHelper = new DocManagerDBHelperTestClass();
		readonly int testDbCount;
		readonly byte[] testImageData;
		internal const string TestDataTag = "DbMergerTestDataHelper_D48DE0036E81451D934B8814BE9A03F3";
	}

	#endregion
}
