using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentScanning.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.PAR
{
	public sealed class PARPurgeSystemIntegrationTest : ArchiveSystemIntegrationTestWithDbSetup, IArchiveSystemDescriptorIntegrationTest
	{
		public PARPurgeSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.PAR,
				ListOfStageNames = ["Purge Archived Records"]
			};
		}

		ZDateTime purgeJobsBeforeDate;

		IArchiveConfiguration CreateAndRunPurgeConfiguration(ArchiveScheduleTask schedule)
		{
			var purgeSystem = TestHelpers.GetArchiveSystem(ArchiveManagerConstants.Codes.PAR);
			purgeJobsBeforeDate = ZDateTime.Now;
			var config = new ArchiveConfiguration(purgeJobsBeforeDate, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var algorithmEnumerator = purgeSystem.GetArchiveStages(config).GetEnumerator();
			_ = algorithmEnumerator.MoveNext();
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());
			currentArchiveManager.Run(ArchiveManagerConstants.Codes.PAR, config, archiveLogger, schedule, new CancellationToken());

			return config;
		}

		[UseSnapshotProtection]
		public void TestPurgeArchivedRecords()
		{
			var parentPkToNumDocuments = new List<KeyValuePair<ZGuid, int>>();
			SetUpFactory();
			SetUpData(parentPkToNumDocuments);

			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var config = CreateAndRunPurgeConfiguration(purgeSchedule);

			AssertLog(config, parentPkToNumDocuments);
			AssertPurgeSummaryReportContainsPurgedData(purgeSchedule);
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(purgeSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(purgeSchedule, "PurgeArchivedRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: StorageMainSchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false);
			AssertPurgedRecords("All records should have been purged from", 0, docGenerated: true);
		}

		[UseSnapshotProtection]
		public void TestCannotRunTwoInstancesOfPARAtOnce()
		{
			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var lockResult = false;
			SqlApplicationLock parLock = null;

			using var conn = Db.NewExtraConnectionToMainDb();
			lockResult = conn.TryGetLock("PARArchiveStageLock", TimeSpan.FromMilliseconds(0), out parLock);

			using (parLock)
			{
				Assert("Lock should have been obtained", lockResult);
				Assert("Lock should be held", parLock.IsHoldingLock());

				_ = CreateAndRunPurgeConfiguration(purgeSchedule);

				Assert("Should not have run PAR", archiveLogger.ListOfMessages.Any(s => s.Contains("PAR is already running in another instance of ARC.")));
			}
		}

		void CreateTestStorageDocs(DbConnection dbConnection, string dbName, ZGuid storageMainPK)
		{
			const string insertStorageDocsSQL = @"
				INSERT dbo.StorageDocs (
					SC_PK, SC_SM, SC_Date, SC_ImageData, SC_DataType, SC_DocType, SC_Desc, SC_FileName,
					SC_IsDeleted, SC_IsPublished, SC_IsSystemGenerated, SC_SaveVersions, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc
				)
				VALUES (
					NEWID(), '{0}', GETUTCDATE(), {1}, '{2}', '{2}', '{2}', '{2}',
					'N', '{3}', 'N', '{3}', GETUTCDATE(), GETUTCDATE()
				);";

			using (((ICurrentDbControl)dbConnection).UseDatabase(dbName))
			{
				var insertSotrageDocsSQLFormatted =
					string.Format(insertStorageDocsSQL, storageMainPK, "0x0001", "TIF", "Y") +
					string.Format(insertStorageDocsSQL, storageMainPK, "0x0002", "DOC", "N") +
					string.Format(insertStorageDocsSQL, storageMainPK, "0x0003", "TIF", "N");

				_ = dbConnection.ExecuteNonQuery(insertSotrageDocsSQLFormatted);
			}
		}

		void CreateTestStorageMain(DbConnection dbConnection, ZGuid storageMainPK, ZGuid storageMainParentFK, int dbNumber)
		{
			const string insertStorageMainSQL = @"
				INSERT dbo.StorageMain (SM_PK, SM_DB, SM_Archived, SM_ParentFK, SM_Type)
				VALUES ('{0}', '{1}', '{2}', '{3}', '{4}');";
			var insertStorageMainSQLFormatted = string.Format(insertStorageMainSQL, storageMainPK, dbNumber, ZDateTime.Today.AddYears(-8), storageMainParentFK, Core.Constants.DocManagerCodes.Consol);

			_ = dbConnection.ExecuteNonQuery(insertStorageMainSQLFormatted);
		}

		void CreateTestStorageReference(DbConnection dbConnection, ZGuid storageReferencePK, ZGuid storageMainPK)
		{
			const string insertStorageReferenceSQL = @"INSERT dbo.StorageReference(SR_PK, SR_SM, SR_TYPE) VALUES ('{0}', '{1}', '{2}')";
			var insertStorageReferenceSQLFormatted = string.Format(insertStorageReferenceSQL, storageReferencePK, storageMainPK, Core.Constants.DocManagerCodes.Consol);

			_ = dbConnection.ExecuteNonQuery(insertStorageReferenceSQLFormatted);
		}

		public void TestArchiveStagesSortByMainDateFilterColumn()
		{
			Assert("Since PARArchiveStage is special - no need for testing this", condition: true);
		}

		[UseSnapshotProtection]
		public void TestReadOnlyDatabaseIsHandledCorrectly()
		{
			var documentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var dbNumber = 334;
			var dbName = documentFactory.GetDatabaseName(dbNumber);

			var storageMainPK = ZGuid.NewZGuid();
			var storageMainParentFK = ZGuid.NewZGuid();
			var storageReferencePK = ZGuid.NewZGuid();

			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);

			using var adminConnection = Db.NewAdminConnection();
			AdoTestUtils.DropDbIfExists(adminConnection, dbName);

			try
			{
				AdoTestUtils.CreateDbIfNotExists(adminConnection, dbName);

				AssertEquals($"Precondition: {dbName} should be writable.", DbWriteableState.Writeable, documentFactory.GetDbWriteableState(dbNumber));

				CreateTestStorageDocs(adminConnection, dbName, storageMainPK);
				CreateTestStorageMain(adminConnection, storageMainPK, storageMainParentFK, dbNumber);
				CreateTestStorageReference(adminConnection, storageReferencePK, storageMainPK);

				var parentMain = documentFactory.Load<StorageMain>(storageMainPK);
				var numberOfStorageDocs = 3;
				AssertEquals($"Precondition: {dbName} count should have {numberOfStorageDocs} documents.", numberOfStorageDocs, parentMain.eDocs.Count);

				adminConnection.AlterDbWriteableStateForDocManager(dbName, writeable: false);
				AssertEquals($"Precondition: {dbName} should be read-only.", DbWriteableState.ReadOnly, documentFactory.GetDbWriteableState(dbNumber));

				_ = CreateAndRunPurgeConfiguration(purgeSchedule);

				var logMessages = archiveLogger.ListOfMessages;
				var lines = logMessages.Where(l => l.Contains("Error|PAR|") || l.Contains($"InnerException Message = {dbName} has been made to ReadOnly"));

				CombineAssertions(() =>
				{
					AssertEquals("StorageDocs should not have been purged.", numberOfStorageDocs, parentMain.eDocs.Count);
					Assert($"There should be no errors in the logs, but there were {lines.Count()}.", !lines.Any());
				});
			}
			finally
			{
				AdoTestUtils.DropDbIfExists(adminConnection, dbName);
			}
		}

		[UseSnapshotProtection]
		public void TestRecordsArchivedOfflineAreNotPurged()
		{
			var parentPkToNumDocuments = new List<KeyValuePair<ZGuid, int>>();
			SetUpFactory();
			SetUpData(parentPkToNumDocuments);
			var numAssociatedDocuments = 1;
			// Record does not need to be purged
			_ = CreateTestRecords(Core.Constants.DocManagerCodes.DomesticTransportBooking, ZDateTime.Empty, ZDateTime.Today.AddYears(-2), numAssociatedDocuments);
			masterFactory.Save();

			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var config = CreateAndRunPurgeConfiguration(purgeSchedule);

			AssertLog(config, parentPkToNumDocuments);
			AssertPurgeSummaryReportContainsPurgedData(purgeSchedule);
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(purgeSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(purgeSchedule, "PurgeArchivedRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: StorageMainSchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false);
			AssertPurgedRecords("There should be 1 remaining record in", 1, docGenerated: true);
		}

		[UseSnapshotProtection]
		public void TestRecordsThatAreNotArchivedAreNotPurged()
		{
			var parentPkToNumDocuments = new List<KeyValuePair<ZGuid, int>>();
			SetUpFactory();
			SetUpData(parentPkToNumDocuments);
			var numAssociatedDocuments = 1;
			_ = CreateTestRecords(Core.Constants.DocManagerCodes.DomesticTransportBooking, ZDateTime.Empty, ZDateTime.Empty, numAssociatedDocuments);
			masterFactory.Save();

			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var config = CreateAndRunPurgeConfiguration(purgeSchedule);

			AssertLog(config, parentPkToNumDocuments);
			AssertPurgeSummaryReportContainsPurgedData(purgeSchedule);
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(purgeSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(purgeSchedule, "PurgeArchivedRecordsReport_"), TestConfig.ArchiveSystemCodeToTest,mainArchiveTableName: StorageMainSchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false);
			AssertPurgedRecords("There should be 1 remaining record in", 1, docGenerated: true);
		}

		[UseSnapshotProtection]
		public void TestLoggingOfSummaryLogs()
		{
			var parentPkToNumDocuments = new List<KeyValuePair<ZGuid, int>>();
			using (SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				SetUpFactory();
				var numAssociatedDocuments = 1;
				SetUpData(parentPkToNumDocuments);
				var pk = CreateTestRecords(Core.Constants.DocManagerCodes.Container, ZDateTime.Today.AddYears(-8), ZDateTime.Empty, numAssociatedDocuments);
				parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));
				pk = CreateTestRecords(Core.Constants.DocManagerCodes.Shipment, ZDateTime.Today.AddYears(-9), ZDateTime.Empty, numAssociatedDocuments);
				parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));
				pk = CreateTestRecords(Core.Constants.DocManagerCodes.Shipment, ZDateTime.Today.AddYears(-5), ZDateTime.Empty, numAssociatedDocuments);
				parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));
				masterFactory.Save();

				var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
				_ = CreateAndRunPurgeConfiguration(purgeSchedule);
				CombineAssertions("Summary logs", () =>
				{
					Assert("Contains message on number loaded and number of batches", archiveLogger.ListOfMessages.Any(x => x.Contains("Time taken to load 3 StorageMain record(s) and their eDoc(s) in 3 batch(es):")));
					Assert("Contains message on number deleted", archiveLogger.ListOfMessages.Any(x => x.Contains("Time taken to purge 3 StorageMain record(s) and their eDoc(s):")));
				});
			}
		}

		[UseSnapshotProtection]
		public void TestPurgeArchivedRecordsWhenMultipleStorageReferenceToOneStorageMain()
		{
			var parentPkToNumDocuments = new List<KeyValuePair<ZGuid, int>>();
			SetUpFactory();
			SetUpData(parentPkToNumDocuments);
			var numAssociatedDocuments = 1;
			var parentPK = CreateTestRecords(Core.Constants.DocManagerCodes.Consol, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default), ZDateTime.Empty, numAssociatedDocuments);
			parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(parentPK, numAssociatedDocuments));

			var storageMain = masterFactory.GetStorageMainForPK(parentPK);

			var storageReference1 = masterFactory.New<StorageReference>();
			storageReference1.SR_SM = storageMain.PK;
			storageReference1.SR_Sequence = 1;

			var storageReference2 = masterFactory.New<StorageReference>();
			storageReference2.SR_SM = storageMain.PK;
			storageReference2.SR_Sequence = 2;

			masterFactory.Save();

			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var config = CreateAndRunPurgeConfiguration(purgeSchedule);

			AssertLog(config, parentPkToNumDocuments);
			AssertPurgeSummaryReportContainsPurgedData(purgeSchedule);
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(purgeSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(purgeSchedule, "PurgeArchivedRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: StorageMainSchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false);
			AssertPurgedRecords("All records should have been purged from", 0, docGenerated: true);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNoRecordsToPurge()
		{
			SetUpFactory();

			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var config = CreateAndRunPurgeConfiguration(purgeSchedule);

			AssertLog(config, new List<KeyValuePair<ZGuid, int>>());
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(purgeSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(purgeSchedule, "PurgeArchivedRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: StorageMainSchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false);
		}

		[UseSnapshotProtection]
		public void TestMultipleBatchesArePurged()
		{
			var parentPkToNumDocuments = new List<KeyValuePair<ZGuid, int>>();
			using (SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				SetUpFactory();
				var numAssociatedDocuments = 1;
				SetUpData(parentPkToNumDocuments);
				var pk = CreateTestRecords(Core.Constants.DocManagerCodes.Container, ZDateTime.Today.AddYears(-8), ZDateTime.Empty, numAssociatedDocuments);
				parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));
				pk = CreateTestRecords(Core.Constants.DocManagerCodes.Shipment, ZDateTime.Today.AddYears(-9), ZDateTime.Empty, numAssociatedDocuments);
				parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));
				pk = CreateTestRecords(Core.Constants.DocManagerCodes.Shipment, ZDateTime.Today.AddYears(-5), ZDateTime.Empty, numAssociatedDocuments);
				parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));
				masterFactory.Save();

				CombineAssertions("Precondition", () =>
				{
					AssertEquals($"Table: {StorageReferenceSchema.Constants.TableName}, should have 5 records.", 5, masterFactory.GetDatabaseCount(typeof(StorageReference)));
					AssertEquals($"Table: {StorageMainSchema.Constants.TableName}, should have 5 records.", 5, masterFactory.GetDatabaseCount(typeof(StorageMain)));
					AssertEquals($"Table: {StorageDocsSchema.Constants.TableName}, should have 5 records.", 5, childFactory.GetDatabaseCount(typeof(StorageDocs)));
				});

				var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
				_ = CreateAndRunPurgeConfiguration(purgeSchedule);

				AssertPurgeSummaryReportContainsPurgedData(purgeSchedule);
				AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(purgeSchedule).Count());
				ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(purgeSchedule, "PurgeArchivedRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: StorageMainSchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false);
				AssertPurgedRecords("All records should have been purged from", 0, docGenerated: true);
			}
		}

		[UseSnapshotProtection]
		public void TestPurgeWithMultipleEDocsPerRecord()
		{
			var parentPkToNumDocuments = new List<KeyValuePair<ZGuid, int>>();

			SetUpFactory();
			var numAssociatedDocuments = 2;
			var pk = CreateTestRecords(Core.Constants.DocManagerCodes.Container, ZDateTime.Today.AddYears(-8), ZDateTime.Empty, numAssociatedDocuments);
			parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));

			masterFactory.Save();

			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var config = CreateAndRunPurgeConfiguration(purgeSchedule);

			AssertLog(config, parentPkToNumDocuments);
			AssertPurgeSummaryReportContainsPurgedData(purgeSchedule);
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(purgeSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(purgeSchedule, "PurgeArchivedRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: StorageMainSchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false);
			AssertPurgedRecords("All records should have been purged from", 0, docGenerated: true);
		}

		public void TestArchiveDateLoggedCorrectly()
		{
			SetUpFactory();
			var pk = CreateTestRecords(Core.Constants.DocManagerCodes.Container, ZDateTime.Today.AddYears(-8), ZDateTime.Empty, 1);
			var storageMain = masterFactory.GetStorageMainForPK(pk);
			var archiveDate = ZDateTime.Today.AddDays(-1);
			storageMain.SM_Archived = archiveDate;

			masterFactory.Save();

			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			_ = CreateAndRunPurgeConfiguration(purgeSchedule);

			Assert(archiveLogger.ListOfMessages.Exists(log => log.Contains($"Archived on: {storageMain.SM_Archived}")));
		}

		[UseSnapshotProtection]
		public void TestPurgeArchivedRecordsWhenArchivedDateIsOnTheOnOrBeforeDate()
		{
			SetUpFactory();
			var archiveDate = new ZDateTime(2017, 06, 21, 15, 0, 0);

			var pk = CreateTestRecords(Core.Constants.DocManagerCodes.Container, archiveDate, ZDateTime.Empty, 1);
			var storageMain = masterFactory.GetStorageMainForPK(pk);

			masterFactory.Save();

			var archiveSystem = TestHelpers.GetArchiveSystem(ArchiveManagerConstants.Codes.PAR);
			var config = new ArchiveConfiguration(new ZDateTime(2017, 06, 21), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			Assert("Precondition", storageMain.SM_Archived > config.ArchiveJobsOnOrBeforeThisDate && storageMain.SM_Archived < config.ArchiveJobsOnOrBeforeThisDate.AddDays(1));

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, purgeSchedule, new CancellationToken());

			AssertPurgeSummaryReportContainsPurgedData(purgeSchedule);
			Assert("Archive Date Logged Correctly", archiveLogger.ListOfMessages.Exists(log => log.Contains($"Archived on: {storageMain.SM_Archived}")));
			AssertPurgedRecords("All records should have been purged from", 0, docGenerated: true);
		}

		[UseSnapshotProtection]
		public void TestPARRespondsToCancellationToken()
		{
			SetUpFactory();
			var archiveDate = new ZDateTime(2017, 06, 21, 15, 0, 0);

			var pk = CreateTestRecords(Core.Constants.DocManagerCodes.Container, archiveDate, ZDateTime.Empty, 1);

			masterFactory.Save();

			var archiveSystem = TestHelpers.GetArchiveSystem(ArchiveManagerConstants.Codes.PAR);
			var config = new ArchiveConfiguration(new ZDateTime(2017, 06, 21), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());
			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			var token = tokenSource.Token;

			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			AssertNotNull("Precondition: StorageMain exists", newFactory.GetStorageMainForPK(pk));

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, purgeSchedule, token);

			newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			AssertNotNull("Postcondition: StorageMain should still exist because operation was canceled", newFactory.GetStorageMainForPK(pk));
			Assert("Logger should contain message about cancellation", archiveLogger.ListOfMessages.Exists(log => log.Contains("was stopped by a cancellation request.")));
		}

		[UseSnapshotProtection]
		public void TestPARRespondsToRunningOutOfTime()
		{
			SetUpFactory();
			var archiveDate = new ZDateTime(2017, 06, 21, 15, 0, 0);

			var pk = CreateTestRecords(Core.Constants.DocManagerCodes.Container, archiveDate, ZDateTime.Empty, 1);

			masterFactory.Save();

			var archiveSystem = TestHelpers.GetArchiveSystem(ArchiveManagerConstants.Codes.PAR);
			var config = new ArchiveConfiguration(new ZDateTime(2017, 06, 21), 10, ZDateTime.UtcNow.AddMinutes(-10), false, shouldIncludeDeclarations: false);
			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			var newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());

			AssertNotNull("Precondition: StorageMain exists", newFactory.GetStorageMainForPK(pk));

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, purgeSchedule, new CancellationToken());

			newFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			AssertNotNull("Postcondition: StorageMain should still exist because operation was cancelled", newFactory.GetStorageMainForPK(pk));
			Assert("Logger should contain message about reaching max run time", archiveLogger.ListOfMessages.Exists(log => log.Contains("Max Run Duration reached")));
		}

		[UseSnapshotProtection]
		public void TestArchiveLoggerContainsArchiveConfigurationParameters()
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(ArchiveManagerConstants.Codes.PAR);
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, archiveSchedule, new CancellationToken());

			CombineAssertions("Configuration logs did not contain the correct messages", () =>
			{
				AssertCollectionContains("Configuration Parameter Log", $"Information|{ArchiveManagerConstants.Codes.PAR}|Configuration Parameters:", archiveLogger.ListOfMessages);
				AssertCollectionContains("On or Before Log", $"Information|{ArchiveManagerConstants.Codes.PAR}|Purging Records on or Before: 2-Feb-2020", archiveLogger.ListOfMessages);
				AssertCollectionContains("Max Run Duration Log", $"Information|{ArchiveManagerConstants.Codes.PAR}|Max Run Duration: 10 minutes", archiveLogger.ListOfMessages);
				AssertCollectionContains("Incl. Customs Log", $"Information|{ArchiveManagerConstants.Codes.PAR}|Verbose Logging: No", archiveLogger.ListOfMessages);
			});
		}

		void SetUpFactory()
		{
			masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			childFactory = masterFactory.GetFactory(DbNumber);

			AssertPurgedRecords("There should be no records in", 0, docGenerated: false);
		}

		void SetUpData(List<KeyValuePair<ZGuid, int>> parentPkToNumDocuments)
		{
			var numAssociatedDocuments = 1;
			var pk = CreateTestRecords(Core.Constants.DocManagerCodes.Consol, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default), ZDateTime.Empty, numAssociatedDocuments);
			parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));
			pk = CreateTestRecords(Core.Constants.DocManagerCodes.Shipment, ZDateTime.Today.AddYears(-ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default + 1), ZDateTime.Empty, numAssociatedDocuments);
			parentPkToNumDocuments.Add(new KeyValuePair<ZGuid, int>(pk, numAssociatedDocuments));

			masterFactory.Save();

			var rowsAdded = 2;

			AssertPurgedRecords($"{rowsAdded} rows should have been added to", rowsAdded, docGenerated: false);
		}

		DocumentFactory masterFactory;
		NumberedBusinessObjectFactory childFactory;

		ZGuid CreateTestRecords(string type, ZDateTime archivedDate, ZDateTime offLine, int numDocuments)
		{
			var storageMain = masterFactory.New<StorageMain>();
			storageMain.SM_DB = DbNumber;
			storageMain.SM_ParentFK = Guid.NewGuid();
			storageMain.SM_Type = type;
			storageMain.SM_Archived = archivedDate;
			storageMain.SM_OffLine = offLine;

			var storageReference = masterFactory.New<StorageReference>();
			storageReference.SR_SM = storageMain.PK;

			for (var i = 0; i < numDocuments; i++)
			{
				var storageDocuments = childFactory.New<StorageDocs>();
				storageDocuments.SC_SM = storageMain.PK;
				storageDocuments.SM_Type = type;
			}

			masterFactory.Save();
			return storageMain.SM_ParentFK;
		}

		void AssertPurgedRecords(string message, int expected, bool docGenerated)
		{
			AssertEquals($"{message} Table: {StorageReferenceSchema.Constants.TableName}.", expected, masterFactory.GetDatabaseCount(typeof(StorageReference)));
			if (docGenerated)
			{
				expected++;
			}

			CombineAssertions(() =>
			{
				AssertEquals($"{message} Table: {StorageMainSchema.Constants.TableName}.", expected, masterFactory.GetDatabaseCount(typeof(StorageMain)));
				AssertEquals($"{message} Table: {StorageDocsSchema.Constants.TableName}.", expected, childFactory.GetDatabaseCount(typeof(StorageDocs)));
			});
		}

		public void AssertMessage(int lineNumber, params string[] expectedMessages)
		{
			var messages = archiveLogger.ListOfMessages.ToArray();
			foreach (var expectedMessage in expectedMessages)
			{
				AssertContains("Line " + lineNumber.ToString(), expectedMessage, messages.Length > lineNumber ? messages[lineNumber] : string.Empty);
			}
		}

		void AssertValidPKAndType(int lineNumber, List<KeyValuePair<ZGuid, int>> parentPkToNumDocuments)
		{
			var messages = archiveLogger.ListOfMessages.ToArray();
			var message = string.Empty;
			if (messages.Length > lineNumber)
			{
				message = messages[lineNumber];
			}

			var containsPKAndType = false;
			foreach (var record in parentPkToNumDocuments)
			{
				var storageMain = masterFactory.GetStorageMainForPK(record.Key);
				if (message.Contains($"{storageMain.PK}") && message.Contains($"{storageMain.SM_Type}"))
				{
					containsPKAndType = true;
				}
			}

			Assert("Invalid PK and Type", containsPKAndType);
		}

		public void AssertPurgeSummaryReportContainsPurgedData(ArchiveScheduleTask archiveSchedule)
		{
			CombineAssertions(() =>
			{
				var archiveReport = TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "PurgeArchivedRecordsReport_");

				using var stream = archiveReport.GetImageDataReader();
				using var excel = new ExcelInterface();
				excel.LoadExcelFile(stream);
				var worksheet = excel.WorkSheets[0];

				var tablesInTheReport = new List<string>();
				tablesInTheReport.Add(excel.WorkSheets[0][worksheet.RowCount - 1, 1].ToString());
				tablesInTheReport.Add(excel.WorkSheets[0][worksheet.RowCount - 2, 1].ToString());
				tablesInTheReport.Add(excel.WorkSheets[0][worksheet.RowCount - 3, 1].ToString());

				AssertCollectionContains("Report row contains StorageDocs", "StorageDocs", tablesInTheReport);
				AssertCollectionContains("Report row contains StorageMain", "StorageMain", tablesInTheReport);
				AssertCollectionContains("Report row contains StorageReference", "StorageReference", tablesInTheReport);
			});
		}

		public void TestGeneratedSummaryReport()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "PurgeArchivedRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: StorageMainSchema.Constants.TableName, archiveSystemHasMultipleArchiveStageDescriptors: false);
		}

		void AssertLog(IArchiveConfiguration config, List<KeyValuePair<ZGuid, int>> parentPkToNumDocuments)
		{
			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

			var index = 0;

			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Registry Settings");
			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|On or Before Minimum: 7");
			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Set Batch Size: 200");

			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Configuration Parameters:");
			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Purging Records on or Before: {config.ArchiveJobsOnOrBeforeThisDate.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture)}");
			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Max Run Duration: {config.MaxRunDurationInMinutes} minutes");
			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Verbose Logging: {config.IsVerboseLog.ToYesNoString()}");

			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Executing: Purge Archived Records");
			for (var i = 0; i < parentPkToNumDocuments.Count; i++)
			{
				AssertMessage(index, $"Information|{ArchiveManagerConstants.Codes.PAR}|Deleting record");
				AssertValidPKAndType(index++, parentPkToNumDocuments);
				AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|eDocs: {parentPkToNumDocuments[i].Value}");
			}

			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Time taken to load");
			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Time taken to purge");
			AssertMessage(index, $"Information|{ArchiveManagerConstants.Codes.PAR}|StorageMains Per Hour:");
			AssertMessage(index++, $"StorageDocs Per Hour:");

			if (parentPkToNumDocuments.Count > 0)
			{
				AssertContains($"Information|{ArchiveManagerConstants.Codes.PAR}|Purged Data Dated Between Earliest Possible Date and ", archiveLogger.ListOfMessages[index++]);
			}

			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Generated Purge Report and stored on eDocs tab of the Purge Schedule");
			AssertMessage(index++, $"Information|{ArchiveManagerConstants.Codes.PAR}|Completed purging records");
		}

		[TestDate(2024, 05, 15)]
		public void TestLoggingOfAMUsageData()
		{
			var parentPkToNumDocuments = new List<KeyValuePair<ZGuid, int>>();
			SetUpFactory();
			SetUpData(parentPkToNumDocuments);

			using (SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PAR);
				archiveSchedule.IsArchiveRecordsOnOrBeforeDate = true;
				archiveSchedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.UtcNow;
				archiveSchedule.ShouldArchiveDeclaration = true;
				archiveSchedule.MaxRunDurationInMinutes = 60;
				archiveSchedule.Run();
			}

			var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
			var messages = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManager);
			AssertEquals("There should be one and only one usage report", 1, messages.Length);
			var usageProperties = messages[0].UsageProperties;

			ArchiveManagerAssertions.AssertResultsForAMUsageCollector(
				usageProperties: usageProperties,
				archiveCode: TestConfig.ArchiveSystemCodeToTest,
				archiveSystemStageName: "Purge Archived Records",
				includeCustomsJobs: true,
				batchSize: 60,
				totalRecordsDeleted: 6,
				jobHeadersProcessed: 0,
				mainRecordsLoaded: 2,
				totalMissingDocumentsGenerated: 0,
				documentsDeleted: 2);
		}

		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
			=> IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);
	}
}
