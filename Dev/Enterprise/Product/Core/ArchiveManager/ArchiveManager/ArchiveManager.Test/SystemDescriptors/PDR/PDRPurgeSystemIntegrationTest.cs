using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.ArchiveManager.Test.Actions;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.PDR
{
	public sealed class PDRPurgeSystemIntegrationTest : ArchiveSystemIntegrationTestWithDbSetup, IArchiveSystemDescriptorIntegrationTest
	{
		public PDRPurgeSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.PDR,
				ListOfStageNames = ["Purge Documents and Records"]
			};
		}

		IArchiveStage CreateAndRunArchiveConfiguration(ArchiveScheduleTask archiveSchedule, bool shouldIncludeDeclarations)
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, TestConfig.IsVerboseLog, shouldIncludeDeclarations);
			var algorithmEnumerator = archiveSystem.GetArchiveStages(config).GetEnumerator();
			_ = algorithmEnumerator.MoveNext();
			var stage = algorithmEnumerator.Current;
			stage.BeginRun(config, archiveSchedule, archiveLogger);
			return stage;
		}

		[TestDate(2024, 05, 15)]
		public void TestLoggingOfAMUsageData()
		{
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = true;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);

			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDR);
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
				archiveSystemStageName: "Purge Documents and Records",
				includeCustomsJobs: true,
				batchSize: 60,
				totalRecordsDeleted: 196,
				jobHeadersProcessed: 3,
				mainRecordsLoaded: 3,
				totalMissingDocumentsGenerated: 0,
				documentsDeleted: 0);
		}

		public void TestPurgeWhenAllJobHeadersCanBeArchivedAndPeriodIsClosed()
		{
			try
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				TestConfig.NeedTableRecordCounts = true;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsVerboseLog = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDR);
				SetupTestData.ForCommonCase(TestConfig);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
				ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
				TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();
				AssertLog(set);
				AssertStorageMainAndRef();
				ArchiveManagerAssertions.AssertAfterPurge(TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertAccManagementPeriod(Factory);

				AssertEquals(1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());

				ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "PurgeDocumentsandRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, archiveSystemHasMultipleArchiveStageDescriptors: false, archiveSystemHasDateParameterSelection: true);
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		void AssertStorageMainAndRef()
		{
			var storageMainAfterRun = TestConfig.TableRecordCountsAfterRun[StorageMainSchema.Constants.TableName].RowCount;
			AssertEquals("StorageMain count after archiving", storageMainAfterRun, 1);

			var storageReferenceAfterRun = TestConfig.TableRecordCountsAfterRun[StorageReferenceSchema.Constants.TableName].RowCount;
			AssertEquals("StorageReference count after archiving", storageReferenceAfterRun, 0);
		}

		void AssertLog(IArchiveSet archiveSet)
		{
			var index = 0;
			var mainArchiveItemKey = archiveSet.MainArchiveItemNK ?? archiveSet.MainArchiveItem.PK.ToString();
			var archiveSetRelatedRecordsCount = archiveSet.Count - 1;

			AssertEquals("Information|PDR|Loaded next batch of 3 JobHeader", archiveLogger.ListOfMessages[index++]);
			AssertEquals($"Information|PDR|Deleting JobHeader '{mainArchiveItemKey}' and {archiveSetRelatedRecordsCount} related records", archiveLogger.ListOfMessages[index++]);
			AssertContains("JobHeaders Per Hour: ", archiveLogger.ListOfMessages[index]);
			AssertContains("StorageDocs Per Hour: ", archiveLogger.ListOfMessages[index++]);
			AssertContains("Information|PDR|Purged Data Dated Between Earliest Possible Date and ", archiveLogger.ListOfMessages[index++]);
			AssertEquals("Information|PDR|Generated Purge Report and stored on eDocs tab of the Purge Schedule", archiveLogger.ListOfMessages[index]);
		}

		[UseSnapshotProtection]
		public void TestSummaryLogs()
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = true;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 5, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());
			SetupTestData.ForCommonCase(TestConfig);

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, archiveSchedule, new CancellationToken());

			CombineAssertions(() =>
			{
				AssertEquals("Should contain log on how much was loaded", true, archiveLogger.ListOfMessages.Any(x => x.Contains("Time taken to load 2 Archive Set(s) of JobHeader record(s) in 1 batch(es)")));
				AssertEquals("Should contain log on documents deleted", true, archiveLogger.ListOfMessages.Any(x => x.Contains("Time taken to delete 0 document(s) and 2 JobHeader record(s) with their related record(s): 0ms")));
			});
		}

		[UseSnapshotProtection]
		public void TestArchiveLoggerContainsArchiveConfigurationParameters()
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 5, ZDateTime.UtcNow, false, true);
			config.SetIsFilteringByJobOpenDate(true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, archiveSchedule, new CancellationToken());

			CombineAssertions("Configuration logs did not contain the correct messages", () =>
			{
				AssertCollectionContains("Configuration Parameter Log", $"Information|{ArchiveManagerConstants.Codes.PDR}|Configuration Parameters:", archiveLogger.ListOfMessages);
				AssertCollectionContains("Date Parameter Log", $"Information|{ArchiveManagerConstants.Codes.PDR}|Date Parameter: Job Open Date", archiveLogger.ListOfMessages);
				AssertCollectionContains("On or Before Log", $"Information|{ArchiveManagerConstants.Codes.PDR}|Purging Records on or Before: 2-Feb-2020", archiveLogger.ListOfMessages);
				AssertCollectionContains("Max Run Duration Log", $"Information|{ArchiveManagerConstants.Codes.PDR}|Max Run Duration: 5 minutes", archiveLogger.ListOfMessages);
				AssertCollectionContains("Verbose Logging Log", $"Information|{ArchiveManagerConstants.Codes.PDR}|Verbose Logging: No", archiveLogger.ListOfMessages);
				AssertCollectionContains("Incl. Customs Log", $"Information|{ArchiveManagerConstants.Codes.PDR}|Incl. Customs: Yes", archiveLogger.ListOfMessages);
			});
		}

		public void TestGeneratedSummaryReport()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			config.SetIsFilteringByJobOpenDate(true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "PurgeDocumentsandRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, archiveSystemHasMultipleArchiveStageDescriptors: false, archiveSystemHasDateParameterSelection: true);
		}

		public void TestGetArchiveSetProcessed()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK;
			jobHeader.JH_SystemCreateTimeUtc = DateTime.UtcNow.AddYears(-5);
			jobHeader.JH_A_JCL = DateTime.UtcNow.AddYears(-2);
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;

			var systemDescriptor = new PDRPurgeSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, archiveLogger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(null, schedule, archiveStage).FirstOrDefault();
			_ = archiveSet.Load(archiveLogger);
			var mainArchiveItem = archiveSet.GetArchiveItem(jobHeader.PK.ToGuid());
			mainArchiveItem.TotalDocumentsDeleted = 5;
			var shipmentArchiveItem = archiveSet.GetArchiveItem(shipmentPK.ToGuid());
			shipmentArchiveItem.TotalDocumentsDeleted = 7;

			stageDescriptor.OnArchiveSetProcessed(archiveSet);

			var commonStageDescriptor = (CommonArchiveStageDescriptor)stageDescriptor;
			var processedRecordsCountPerTable = commonStageDescriptor.ProcessingInfoPerTable;

			AssertEquals("There should be five items in the collection.", 5, processedRecordsCountPerTable.Count);

			AssertCollectionContains("StorageMain should be included in the collection.", StorageMainSchema.Constants.TableName, processedRecordsCountPerTable.Keys);
			AssertCollectionContains("StorageDocs should be included in the collection.", StorageDocsSchema.Constants.TableName, processedRecordsCountPerTable.Keys);
			AssertCollectionContains("JobHeader should be included in the collection.", JobHeaderSchema.Constants.TableName, processedRecordsCountPerTable.Keys);
			AssertCollectionContains("JobShipment should be included in the collection.", JobShipmentSchema.Constants.TableName, processedRecordsCountPerTable.Keys);
			AssertCollectionContains("JobDocsAndCartage should be included in the collection.", JobDocsAndCartageSchema.Constants.TableName, processedRecordsCountPerTable.Keys);

			AssertEquals("When all StorageDocs are deleted from an ArchiveItem, then the associated StorageMain should also be deleted.", 2, processedRecordsCountPerTable[StorageMainSchema.Constants.TableName].Count);
			AssertEquals("StorageDocs should contain the total documents deleted from all archive items.", 12, processedRecordsCountPerTable[StorageDocsSchema.Constants.TableName].Count);
			AssertEquals("Number of JobHeaders deleted should be included in the collection.", 1, processedRecordsCountPerTable[JobHeaderSchema.Constants.TableName].Count);
			AssertEquals("Number of JobShipments deleted should be included in the collection.", 1, processedRecordsCountPerTable[JobShipmentSchema.Constants.TableName].Count);
			AssertEquals("Number of JobDocsAndCartages deleted should be included in the collection.", 1, processedRecordsCountPerTable[JobDocsAndCartageSchema.Constants.TableName].Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestArchiveStorageMainWithStorageReferences()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK;
			jobHeader.JH_SystemCreateTimeUtc = DateTime.UtcNow.AddYears(-2);
			jobHeader.JH_A_JCL = DateTime.UtcNow.AddYears(-2);
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentPK }, documentFactory, documentFactoryForDB333, DbNumber);

			Factory.Save();
			documentFactory.Save();

			var storageMain = documentFactory.GetStorageMainForPK(shipmentPK);
			var storageReference = documentFactory.NewWithValidTestData<StorageReference>();
			storageReference.SR_SM = storageMain.PK;

			documentFactory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDR);

			AssertEquals(1, Factory.GetDatabaseCount(typeof(StorageReference)));

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDR, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StorageReference)));
		}

		[UseSnapshotProtection]
		public void TestWatermarkUpdatesToJobCloseDate_WhenJobCloseDateIsSelectedFromDropDown()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDR);
			schedule.IsArchiveRecordsOnOrBeforeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = new DateTime(2017, 1, 18);
			schedule.ArchiveRecordsOnOrBeforeType = "Y";
			schedule.S5_ScheduleType = ArchiveManagerConstants.Codes.PDR;
			schedule.MaxRunDurationInMinutes = 1;
			schedule.DateParameter = DateParameterStrings.GetCode(DateParameterType.JCL);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK1;
			jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;
			jobHeader.JH_SystemCreateTimeUtc = new DateTime(2016, 1, 18);
			jobHeader.JH_A_JOP = new DateTime(2018, 1, 18);
			jobHeader.JH_A_JCL = new DateTime(2017, 1, 18);

			Factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("Date Parameter is set to Job Close Date", "JCL", schedule.DateParameter);
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
			});

			schedule.Run(archiveLogger, CancellationToken.None);

			CombineAssertions("Logs are correct and record was archived", () =>
			{
				AssertEquals(0, Factory.GetDatabaseCount(typeof(JobHeader)));
				Assert("Purging Records on or Before: 18-Jan-17", archiveLogger.ListOfMessages.Exists(log => log.Contains("Purging Records on or Before: 18-Jan-2017")));
				Assert("Date Parameter is Job Close Date", archiveLogger.ListOfMessages.Exists(log => log.Contains("Date Parameter: Job Close Date")));
				Assert("Watermark was updated to '18-Jan-17 00:00:00'", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was updated to '18-Jan-17 00:00:00'")));
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		[UseSnapshotProtection]
		public void TestWatermarkUpdatesToJobOpenDate_WhenJobOpenDateIsSelectedFromDropDown()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDR);
			schedule.IsArchiveRecordsOnOrBeforeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = new DateTime(2017, 1, 18);
			schedule.ArchiveRecordsOnOrBeforeType = "Y";
			schedule.S5_ScheduleType = ArchiveManagerConstants.Codes.PDR;
			schedule.MaxRunDurationInMinutes = 1;
			schedule.DateParameter = DateParameterStrings.GetCode(DateParameterType.JOP);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK1;
			jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;
			jobHeader.JH_SystemCreateTimeUtc = new DateTime(2016, 1, 18);
			jobHeader.JH_A_JOP = new DateTime(2017, 1, 18);
			jobHeader.JH_A_JCL = new DateTime(2018, 1, 18);

			Factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("Date Parameter is set to Job Open Date", "JOP", schedule.DateParameter);
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
			});

			schedule.Run(archiveLogger, CancellationToken.None);

			CombineAssertions("Logs are correct and record was archived", () =>
			{
				AssertEquals(0, Factory.GetDatabaseCount(typeof(JobHeader)));
				Assert("Purging Records on or Before: 18-Jan-17", archiveLogger.ListOfMessages.Exists(log => log.Contains("Purging Records on or Before: 18-Jan-2017")));
				Assert("Date Parameter is Job Open Date", archiveLogger.ListOfMessages.Exists(log => log.Contains("Date Parameter: Job Open Date")));
				Assert("Watermark was updated to '18-Jan-17 00:00:00'", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was updated to '18-Jan-17 00:00:00'")));
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		[UseSnapshotProtection]
		public void TestPDRPurgesRecordsCorrectly_WhenJobOpenDateIsSelectedFromDropDown()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDR);
			schedule.IsArchiveRecordsOnOrBeforeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = new DateTime(2017, 1, 18);
			schedule.ArchiveRecordsOnOrBeforeType = "Y";
			schedule.S5_ScheduleType = ArchiveManagerConstants.Codes.PDR;
			schedule.MaxRunDurationInMinutes = 1;
			schedule.DateParameter = DateParameterStrings.GetCode(DateParameterType.JOP);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK1;
			jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;
			jobHeader.JH_SystemCreateTimeUtc = new DateTime(2016, 1, 18);
			jobHeader.JH_A_JOP = new DateTime(2017, 1, 18);
			jobHeader.JH_A_JCL = new DateTime(2018, 1, 18);

			Factory.Save();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("Date Parameter is set to Job Open Date", "JOP", schedule.DateParameter);
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
			});

			schedule.Run(archiveLogger, CancellationToken.None);

			CombineAssertions("Logs are correct and record was purged", () =>
			{
				AssertEquals(0, Factory.GetDatabaseCount(typeof(StorageReference)));
				Assert("Purging Records on or Before: 18-Jan-17", archiveLogger.ListOfMessages.Exists(log => log.Contains("Purging Records on or Before: 18-Jan-2017")));
				Assert("Date Parameter is Job Open Date", archiveLogger.ListOfMessages.Exists(log => log.Contains("Date Parameter: Job Open Date")));
				Assert("Watermark was reset to '01-Jan-00 00:00:00'", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was reset to '01-Jan-00 00:00:00'")));
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestEcommerceDataIsCorrectlyArchived_WhenDeclarationsAreIncluded()
		{
			var factory = new BusinessObjectFactory();

			var testDataCreator = ObjectFactory.Get<IEcommerceTestDataCreator>();
			var testBizOs = testDataCreator.CreateEcommerceTestData(factory);
			var customsTestBizOs = testDataCreator.CreateEcommerceWithCustomsTestData(factory);

			ArchiveManagerAssertions.AssertEcommerceTestDataExists(factory, testBizOs);
			ArchiveManagerAssertions.AssertEcommerceCustomsTestDataExists(factory, customsTestBizOs);

			var shipment = factory.Load<IForwardingShipment>(testBizOs[0].PK);
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipment.PK }, documentFactory, documentFactoryForDB333, DbNumber);
			factory.Save();
			documentFactory.Save();

			var storageMain = documentFactory.GetStorageMainForPK(shipment.PK);
			var storageReference = documentFactory.NewWithValidTestData<StorageReference>();
			storageReference.SR_SM = storageMain.PK;

			documentFactory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: true);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			AssertEquals(1, Factory.GetDatabaseCount(typeof(StorageReference)));
			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StorageReference)));

			foreach (var archiveableItem in testBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, archiveableItem);
			}

			foreach (var archiveableItem in customsTestBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, archiveableItem);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestEcommerceDataIsCorrectlyArchived_WhenDeclarationsAreNotIncluded()
		{
			var factory = new BusinessObjectFactory();

			var testDataCreator = ObjectFactory.Get<IEcommerceTestDataCreator>();
			var testBizOs = testDataCreator.CreateEcommerceTestData(factory);
			var customsTestBizOs = testDataCreator.CreateEcommerceWithCustomsTestData(factory);

			ArchiveManagerAssertions.AssertEcommerceTestDataExists(factory, testBizOs);
			ArchiveManagerAssertions.AssertEcommerceCustomsTestDataExists(factory, customsTestBizOs);

			var shipment = factory.Load<IForwardingShipment>(testBizOs[0].PK);
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipment.PK }, documentFactory, documentFactoryForDB333, DbNumber);
			factory.Save();
			documentFactory.Save();

			var storageMain = documentFactory.GetStorageMainForPK(shipment.PK);
			var storageReference = documentFactory.NewWithValidTestData<StorageReference>();
			storageReference.SR_SM = storageMain.PK;

			documentFactory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			AssertEquals(1, Factory.GetDatabaseCount(typeof(StorageReference)));
			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StorageReference)));

			foreach (var archiveableItem in testBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, archiveableItem);
			}

			foreach (var archiveableItem in customsTestBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasNotArchived(factory, archiveableItem);
			}
		}

		[UseSnapshotProtection]
		public void TestJobDeclarationAttatchedToJobHeaderViaJobConsolAndNotIncludingCustomsDoesNotArchive()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsCancelled = true;

			var jobShipment = consol.Shipments.AddNew();
			var shipmentPK = jobShipment.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipmentPK;
			declaration.JE_ClusterKey = 1;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "Job123";
			jobHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobHeader.JH_ParentID = consol.PK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.UtcNow;
			jobHeader.JH_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-11);

			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(JobComInvoiceHeader));
			_ = query.AddToFilter(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
			var comInvoice = Factory.LoadTop1<JobComInvoiceGroupHeader>(query);
			comInvoice.Delete();

			Factory.Save();

			CombineAssertions("Precondition: test data was successfully created", () =>
			{
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobConShipLink)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			});

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("Records should be succesfully skipped without an error as there is a declaration in the set", () =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error")));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobConShipLink)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			});
		}

		[UseSnapshotProtection]
		public void TestJobDeclarationAttatchedToJobHeaderViaJobConsolAndIncludingCustomsDoesArchive()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, isVerboseLog: true, shouldIncludeDeclarations: true);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_IsCancelled = true;

			var jobShipment = consol.Shipments.AddNew();
			var shipmentPK = jobShipment.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipmentPK;
			declaration.JE_ClusterKey = 1;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "Job123";
			jobHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobHeader.JH_ParentID = consol.PK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.UtcNow;
			jobHeader.JH_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-11);

			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(JobComInvoiceHeader));
			_ = query.AddToFilter(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
			var comInvoice = Factory.LoadTop1<JobComInvoiceGroupHeader>(query);
			comInvoice.Delete();

			Factory.Save();

			CombineAssertions("Precondition: test data was successfully created", () =>
			{
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobConShipLink)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			});

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("Records should be succesfully archived as we are including customs", () =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error")));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(JobConShipLink)));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestForwardingDataIsCorrectlyArchived()
		{
			var factory = new BusinessObjectFactory();

			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			var archiveableBizOs = testDataCreator.CreateArchiveableForwardingData(factory);

			ArchiveManagerAssertions.AssertForwardingTestDataExists(factory, archiveableBizOs);

			var shipment = factory.Load<IForwardingShipment>(archiveableBizOs[0].PK);
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);
			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipment.PK }, documentFactory, documentFactoryForDB333, DbNumber);
			factory.Save();
			documentFactory.Save();

			var storageMain = documentFactory.GetStorageMainForPK(shipment.PK);
			var storageReference = documentFactory.NewWithValidTestData<StorageReference>();
			storageReference.SR_SM = storageMain.PK;

			documentFactory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			AssertEquals(1, Factory.GetDatabaseCount(typeof(StorageReference)));
			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, logger, schedule);

			Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(StorageReference)));

			foreach (var archiveableItem in archiveableBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, archiveableItem);
			}
		}

		public void TestArchiveStagesSortByMainDateFilterColumn()
		{
			var system = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
			var archiveBeforeDate = ZDateTime.Now;
			var configWithoutCustoms = new ArchiveConfiguration(archiveBeforeDate, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var configWithCustoms = new ArchiveConfiguration(archiveBeforeDate, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: true);
			configWithCustoms.SetIsFilteringByJobOpenDate(false);
			configWithoutCustoms.SetIsFilteringByJobOpenDate(false);

			ArchiveManagerAssertions.AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(system, configWithCustoms);
			ArchiveManagerAssertions.AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(system, configWithoutCustoms);

			configWithCustoms.SetIsFilteringByJobOpenDate(true);
			configWithoutCustoms.SetIsFilteringByJobOpenDate(true);

			ArchiveManagerAssertions.AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(system, configWithCustoms);
			ArchiveManagerAssertions.AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(system, configWithoutCustoms);
		}

		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
			=> IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);
	}
}
