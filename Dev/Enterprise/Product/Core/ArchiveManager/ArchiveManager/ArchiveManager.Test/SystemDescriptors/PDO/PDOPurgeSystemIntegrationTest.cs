using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.ArchiveManager.Test.Actions;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.PDO
{
	public sealed class PDOPurgeSystemIntegrationTest : ArchiveSystemIntegrationTestWithDbSetup, IArchiveSystemDescriptorIntegrationTest
	{
		public PDOPurgeSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.PDO,
				ListOfStageNames = ["Purge Documents of Operational Records without Jobs - Job Cartage",
					"Purge Documents of Operational Records without Jobs - Job Consol",
					"Purge Documents of Operational Records without Jobs - Job Shipment",
					"Purge Documents of Operational Records without Jobs - Rating Header",
					"Purge Documents of Operational Records without Jobs - Job Declaration"
					]
			};
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2024, 05, 15)]
		public void TestLoggingOfAMUsageData()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK2);

			CreateJobHeaders(new List<ZGuid> { shipmentPK1 });
			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentPK1, shipmentPK2 }, documentFactory, documentFactoryForDB333, DbNumber);

			documentFactory.Save();
			Factory.Save();

			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				schedule.IsArchiveRecordsOnOrBeforeDate = true;
				schedule.ShouldArchiveDeclaration = true;
				schedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.UtcNow;
				schedule.MaxRunDurationInMinutes = 60;
				schedule.Run();
			}

			var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
			var messages = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManager);
			AssertEquals("There should be one and only one usage report", 1, messages.Length);

			var usageProperties = messages[0].UsageProperties;

			ArchiveManagerAssertions.AssertResultsForAMUsageCollector(
				usageProperties: usageProperties,
				archiveCode: TestConfig.ArchiveSystemCodeToTest,
				archiveSystemStageName: "Purge Documents of Operational Records",
				includeCustomsJobs: true,
				batchSize: 60,
				totalRecordsDeleted: 2,
				jobHeadersProcessed: 1,
				mainRecordsLoaded: 1,
				totalMissingDocumentsGenerated: 0,
				documentsDeleted: 1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPurgeDocumentsOfOperationalRecords()
		{
			try
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
				var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

				var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
				forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);
				forwardingTestDataCreator.CreateShipmentData(out var shipmentPK2);

				CreateJobHeaders(new List<ZGuid> { shipmentPK1 });
				TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentPK1, shipmentPK2 }, documentFactory, documentFactoryForDB333, DbNumber);

				documentFactory.Save();
				Factory.Save();

				AssertDatabaseCount("Precondition", jobHeaderCount: 1, shipmentCount: 2, storageMainCount: 2, storageDocsCount: 2, documentFactoryForDB333);

				TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
				AssertLogs(archiveLogger, configuration);
				AssertDatabaseCount("All operational records should remain in the database, but their related documents should have been deleted.", jobHeaderCount: 1, shipmentCount: 2, storageMainCount: 1, storageDocsCount: 1, documentFactoryForDB333);
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestLoggingOfTimeTakenAtEnd()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK2);

			CreateJobHeaders(new List<ZGuid> { shipmentPK1 });
			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentPK1, shipmentPK2 }, documentFactory, documentFactoryForDB333, DbNumber);

			documentFactory.Save();
			Factory.Save();

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			Assert("Logs should say something about documents deleted", archiveLogger.ListOfMessages.Any(x => x.Contains("Time taken to delete 1 document(s):")));
		}

		[UseSnapshotProtection]
		public void TestCannotRunTwoInstancesOfPDOAtOnce()
		{
			var purgeSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
			var lockResult = false;
			SqlApplicationLock pdoLock = null;

			using var conn = Db.NewExtraConnectionToMainDb();
			lockResult = conn.TryGetLock("RunPDOLock", TimeSpan.FromMilliseconds(0), out pdoLock);

			using (pdoLock)
			{
				Assert("Lock should have been obtained", lockResult);
				Assert("Lock should be held", pdoLock.IsHoldingLock());

				var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
				var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
				TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

				Assert("Should not have run PDO", archiveLogger.ListOfMessages.Any(s => s.Contains("PDO is already running in another instance of ARC.")));
			}
		}

		[UseSnapshotProtection]
		public void TestLoggingOfTimeTakenAtEndWithStorageMainButNoStorageDocs()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK2);

			CreateJobHeaders(new List<ZGuid> { shipmentPK1 });

			foreach (var pk in new List<ZGuid> { shipmentPK1, shipmentPK2 })
			{
				var storageMain = documentFactory.New<StorageMain>();
				storageMain.SM_DB = DbNumber;
				storageMain.SM_ParentFK = pk;
			}

			documentFactory.Save();
			Factory.Save();

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			AssertEquals("Even with no StorageDocs, it should take nonzero time to delete our StorageMains", true, archiveLogger.ListOfMessages.Any(m => Regex.Match(m, "Time taken to delete 0 document\\(s\\): [1-9]+\\d*ms").Success));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDOLogsOnStorageOwner()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			CreateJobHeaders(new List<ZGuid> { shipmentPK1 });
			var storageMain = CreateRelatedDocumentsAndReturnStorageMains(new List<ZGuid> { shipmentPK1 }, documentFactory, documentFactoryForDB333)[0];
			var shipment = Factory.Load<ForwardingShipment>(shipmentPK1);

			var ddpTrigger = shipment.WorkflowItems.Triggers.AddNew();
			ddpTrigger.TriggerConditions.TriggerEventCode = AutoEvents.DocumentDeletedPermanentlyCode;
			ddpTrigger.TriggerConditions.TriggerFiredCountdown = 100;

			documentFactory.Save();
			Factory.Save();

			AssertDatabaseCount("Precondition", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 1, storageDocsCount: 1, documentFactoryForDB333);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			var documentOwner = new BusinessObjectFactory().Load<ForwardingShipment>(storageMain.DocumentOwner.PK);
			var logs = documentOwner.GetLogs().GetAllLogs().Cast<StmALog>().ToList();

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			Assert("Storage owner should have deletion logs",
				logs.Exists(l =>
					l.SL_Reference.Contains("Document Deleted Permanently") &&
					l.SL_Reference.Contains("by PDO Archive Schedule Task")));
			AssertDatabaseCount("All operational records should remain in the database, but their related documents should have been deleted.", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);

			AssertEquals("DDP trigger should not be fired during archiving", (ZShort)100, ddpTrigger.P9_TriggerFiredCountdown);

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				_ = MasterFilesTestHelper.RunLogWalker();
			}

			ddpTrigger = new BusinessObjectFactory().Load<ProcessTask>(ddpTrigger.PK);
			AssertEquals("Trigger does not fire when suppress firing workflow is true", (ZShort)100, ddpTrigger.P9_TriggerFiredCountdown);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDOLogsToStmALogWithoutCompanyAndBranch()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, true, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			CreateJobHeaders(new List<ZGuid> { shipmentPK1 });
			var storageMain = CreateRelatedDocumentsAndReturnStorageMains(new List<ZGuid> { shipmentPK1 }, documentFactory, documentFactoryForDB333)[0];
			storageMain.eDocs[0].SC_Desc = "test description";

			documentFactory.Save();
			Factory.Save();

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			var documentOwner = new BusinessObjectFactory().Load<ForwardingShipment>(storageMain.DocumentOwner.PK);
			var stmALogs = documentOwner.GetLogs().GetAllLogs().Cast<StmALog>().ToList();

			Assert("StmALog contains log in the correct format i.e. without company/branch but with SC_Desc", stmALogs.Exists(l => l.SL_Reference.Contains("eDoc 'test description - sample.tif'")));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDODoesNotApplyWorkflowTemplates()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
			
			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);

			CreateJobHeaders(new List<ZGuid> { shipmentPK1 });
			var storageMain = CreateRelatedDocumentsAndReturnStorageMains(new List<ZGuid> { shipmentPK1 }, documentFactory, documentFactoryForDB333)[0];
			var shipment = Factory.Load<ForwardingShipment>(shipmentPK1);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			var triggerTask = template.WorkflowItems.Triggers.AddNew();
			triggerTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			triggerTask.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			triggerTask.TriggerConditions.TriggerEventCode = "CLR";
			triggerTask.TemplateConditions.TemplateCondition1 = "";
			triggerTask.P9_Description = "Test";

			documentFactory.Save();
			Factory.Save();

			AssertDatabaseCount("Precondition", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 1, storageDocsCount: 1, documentFactoryForDB333);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			Assert("Workflow template shouldn't be applied", !storageMain.DocumentOwner.GetLogs().GetAllLogs().ToList().Exists(l => (l as StmALog).SL_SE_NKEvent == AutoEvents.WorkflowTemplateAppliedCode));
			AssertDatabaseCount("All operational records should remain in the database, but their related documents should have been deleted.", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDO_WorksCorrectlyWithMultipleArchiveSets()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK2);
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK3);
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK4);

			CreateJobHeaders(new List<ZGuid>() { shipmentPK1, shipmentPK2, shipmentPK3, shipmentPK4 });
			TestHelpers.CreateRelatedDocuments(new List<ZGuid>() { shipmentPK1, shipmentPK2 }, documentFactory, documentFactoryForDB333, DbNumber);

			documentFactory.Save();
			Factory.Save();

			AssertDatabaseCount("Precondition", jobHeaderCount: 4, shipmentCount: 4, storageMainCount: 2, storageDocsCount: 2, documentFactoryForDB333);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertDatabaseCount("Jobs and their related records should remain in the database, but their related documents should have been deleted.", jobHeaderCount: 4, shipmentCount: 4, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDO_WorksCorrectlyWithMultipleBatches()
		{
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
				var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

				var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
				var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

				var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
				forwardingTestDataCreator.CreateShipmentData(out var shipmentPK1);
				forwardingTestDataCreator.CreateShipmentData(out var shipmentPK2);
				forwardingTestDataCreator.CreateShipmentData(out var shipmentPK3);
				forwardingTestDataCreator.CreateShipmentData(out var shipmentPK4);

				CreateJobHeaders(new List<ZGuid> { shipmentPK1, shipmentPK2, shipmentPK3, shipmentPK4 });
				TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentPK1, shipmentPK2, shipmentPK3 }, documentFactory, documentFactoryForDB333, DbNumber);

				documentFactory.Save();
				Factory.Save();

				Assert("Precondition: ", SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.Value < Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertDatabaseCount("Precondition", jobHeaderCount: 4, shipmentCount: 4, storageMainCount: 3, storageDocsCount: 3, documentFactoryForDB333);

				TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
				AssertDatabaseCount("Jobs and their related records should remain in the database, but their related documents should have been deleted.", jobHeaderCount: 4, shipmentCount: 4, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);
			}
		}

		public void TestPDO_CanIncludeDeclarations()
			=> Assert("PDO should be allowed to include declarations.", new PDOPurgeSystemDescriptor().AllowShouldArchiveDeclaration);

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDO_WhenIncludingCustomsJobs()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var customsTestDataCreator = ObjectFactory.Get<ICustomsTestDataCreator>();
			var declarationPK = customsTestDataCreator.CreateAttachedDeclarationData(shipmentPK, false);

			CreateJobHeaders(new List<ZGuid> { shipmentPK });
			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentPK, declarationPK }, documentFactory, documentFactoryForDB333, DbNumber);

			documentFactory.Save();
			Factory.Save();

			AssertDatabaseCount("Precondition", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 2, storageDocsCount: 2, documentFactoryForDB333);
			AssertEquals($"Table: {JobDeclarationSchema.Constants.TableName}.", 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertDatabaseCount("Run 1: Nothing should have been purged as IncludeCustomsJobs was not checked.", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 2, storageDocsCount: 2, documentFactoryForDB333);
			AssertEquals($"Table: {JobDeclarationSchema.Constants.TableName}.", 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));

			var configurationIncludingCustomsJobs = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, true);
			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configurationIncludingCustomsJobs, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertDatabaseCount("Run 2: Configuration has IncludeCustomsJobs checked and should purge documents.", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);
			AssertEquals($"Table: {JobDeclarationSchema.Constants.TableName}.", 1, Factory.GetDatabaseCount(typeof(BaseJobDeclaration)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDOWithoutJobsAndWithCustoms()
		{
			TestPDOWithoutJobsHelper(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDOWithoutJobsAndWithoutCustoms()
		{
			TestPDOWithoutJobsHelper(false);
		}

		public void TestPDOWithoutJobsHelper(bool includeCustoms)
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, includeCustoms, shouldIncludeRecordsWithoutJobs: true);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);
			forwardingTestDataCreator.CreateShipmentData(out var shipmentWithJobHeaderPK);
			forwardingTestDataCreator.CreateConsolData(0, 0, out var consolPK, isCancelled: false, out _, out _);
			forwardingTestDataCreator.CreateConsolData(0, 0, out var consolWithJobHeaderPK, isCancelled: false, out _, out _);
			forwardingTestDataCreator.CreateCartageData(ZGuid.NewZGuid(), out var cartagePK);
			forwardingTestDataCreator.CreateCartageDataSafeDuplicate(ZGuid.NewZGuid(), out var cartageWithJobHeaderPK);

			var ratingTestDataCreator = ObjectFactory.Get<IRatingTestDataCreator>();
			var ratingHeaderPK = ratingTestDataCreator.CreateAttachedRatingData(0.ToString(), isCancelled: false);
			var ratingHeaderWithJobHeaderPK = ratingTestDataCreator.CreateAttachedRatingData(1.ToString(), isCancelled: false);

			var pksToCreateDocsFor = new List<ZGuid> { shipmentPK, consolPK, cartagePK, ratingHeaderPK };
			var tablesAndPKsToCreateJobHeadersFor = new List<(ZGuid, string)>
			{
				(shipmentWithJobHeaderPK, JobShipmentSchema.Constants.Prefix),
				(consolWithJobHeaderPK, JobConsolSchema.Constants.Prefix),
				(cartageWithJobHeaderPK, JobConsolSchema.Constants.Prefix),
				(ratingHeaderWithJobHeaderPK, JobConsolSchema.Constants.Prefix)
			};

			if (includeCustoms)
			{
				var customsTestDataCreator = ObjectFactory.Get<ICustomsTestDataCreator>();
				var declarationPK = customsTestDataCreator.CreateAttachedDeclarationData(shipmentPK, isCancelled: false);
				var declarationWitHJobHeaderPK = customsTestDataCreator.CreateAttachedDeclarationData(shipmentWithJobHeaderPK, isCancelled: false);
				pksToCreateDocsFor.Add(declarationPK);
				tablesAndPKsToCreateJobHeadersFor.Add((declarationWitHJobHeaderPK, JobDeclarationSchema.Constants.Prefix));
			}

			TestHelpers.CreateRelatedDocuments(pksToCreateDocsFor, documentFactory, documentFactoryForDB333, DbNumber);
			TestHelpers.CreateRelatedDocuments(tablesAndPKsToCreateJobHeadersFor.Select(x => x.Item1).ToList(), documentFactory, documentFactoryForDB333, DbNumber);
			CreateJobHeaders(tablesAndPKsToCreateJobHeadersFor);

			documentFactory.Save();
			Factory.Save();

			var tableRecordCounts = TableRecordCountsHelper.GetTableRecordCounts();

			var databaseCounts = new List<(string, int)>
			{
				(JobShipmentSchema.Constants.TableName, 2),
				(JobConsolSchema.Constants.TableName, 2),
				(JobCartageSchema.Constants.TableName, 4),
				(RatingHeaderSchema.Constants.TableName, 2)
			};

			if (includeCustoms)
			{
				databaseCounts.Add((JobDeclarationSchema.Constants.TableName, 2));
			}

			AssertDatabaseCount(includeCustoms ? 10 : 8, includeCustoms ? 10 : 8, tableRecordCounts, documentFactoryForDB333, databaseCounts.ToArray());

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertDatabaseCount(includeCustoms ? 5 : 4, includeCustoms ? 5 : 4, tableRecordCounts, documentFactoryForDB333, databaseCounts.ToArray());
		}

		[UseSnapshotProtection]
		public void TestPDO_WithNoRelatedDocuments()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			testDataCreator.CreateShipmentData(out var shipmentPK);
			testDataCreator.CreateShipmentData(out _);

			CreateJobHeaders(new List<ZGuid> { shipmentPK });

			documentFactory.Save();
			Factory.Save();

			AssertDatabaseCount("Precondition", jobHeaderCount: 1, shipmentCount: 2, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			AssertDatabaseCount("Jobs and their related records should remain in the database.", jobHeaderCount: 1, shipmentCount: 2, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDO_WithNoJobs()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			testDataCreator.CreateShipmentData(out var shipmentWithoutJobHeaderPK1);
			testDataCreator.CreateShipmentData(out var shipmentWithoutJobHeaderPK2);

			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentWithoutJobHeaderPK1, shipmentWithoutJobHeaderPK2 }, documentFactory, documentFactoryForDB333, DbNumber);

			documentFactory.Save();
			Factory.Save();

			AssertDatabaseCount("Precondition", jobHeaderCount: 0, shipmentCount: 2, storageMainCount: 2, storageDocsCount: 2, documentFactoryForDB333);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			AssertDatabaseCount("Purge Operational Records Documents should not remove any documents if they are not related to Jobs.", jobHeaderCount: 0, shipmentCount: 2, storageMainCount: 2, storageDocsCount: 2, documentFactoryForDB333);
		}

		[UseSnapshotProtection]
		public void TestPDO_WithNothingToPurge()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

			documentFactory.Save();
			Factory.Save();

			AssertDatabaseCount("Precondition", jobHeaderCount: 0, shipmentCount: 0, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			AssertDatabaseCount("Nothing should have been removed or added from the database.", jobHeaderCount: 0, shipmentCount: 0, storageMainCount: 0, storageDocsCount: 0, documentFactoryForDB333);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDO_WorksCorrectlyWithAnyJobStatus()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var factoryForDB333 = documentFactory.GetFactory(DbNumber);

			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();

			var jobHeaderStatusList = new JobHeaderStatusList().GetAllCodes();

			foreach (var jobStatus in jobHeaderStatusList)
			{
				_ = TestHelpers.CreateJobHeaderWithRelatedDocuments(jobStatus, documentFactory, Factory, factoryForDB333, testDataCreator, DbNumber);
			}

			documentFactory.Save();
			Factory.Save();

			AssertDatabaseCount("Precondition", jobHeaderCount: 13, shipmentCount: 13, storageMainCount: 13, storageDocsCount: 13, factoryForDB333);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			AssertDatabaseCount("Purge Operational Records Documents should work for JobHeaders any JobStatus.", jobHeaderCount: 13, shipmentCount: 13, storageMainCount: 0, storageDocsCount: 0, factoryForDB333);
		}

		[UseSnapshotProtection]
		public void TestPDO_DoesNotCheckDetailsInOtherCompaniesWhenLogging()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.Now.AddYears(-10), 1, ZDateTime.Now, true, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);

			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var pk1);
			forwardingTestDataCreator.CreateShipmentData(out var pk2);
			CreateJobHeaders(new List<ZGuid>() { pk1, pk2 });

			var headers = Factory.Load<JobHeader>(new ZQuery());
			var header1 = headers[0];
			var header2 = headers[1];
			header2.JH_JH_ParentJob = header1.PK;
			header2.JH_Status = "WRK";
			header2.JH_SystemCreateTimeUtc = ZDateTime.Now;
			header1.JH_SystemCreateTimeUtc = ZDateTime.Now.AddYears(-11);

			Factory.Save();

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.PDO, configuration, archiveLogger, schedule);

			Assert("Logs should not say anything about open JobHeader in other company", !archiveLogger.ListOfMessages.Any(x => x.Contains("[This item has been found under company code :")));
		}

		[UseSnapshotProtection]
		public void TestArchiveLoggerContainsArchiveConfigurationParameters()
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(ArchiveManagerConstants.Codes.PDO);
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 5, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, archiveSchedule, new CancellationToken());

			CombineAssertions("Configuration logs did not contain the correct messages", () =>
			{
				AssertCollectionContains("Configuration Parameter Log", $"Information|{ArchiveManagerConstants.Codes.PDO}|Configuration Parameters:", archiveLogger.ListOfMessages);
				AssertCollectionContains("On or Before Log", $"Information|{ArchiveManagerConstants.Codes.PDO}|Purging Records on or Before: 2-Feb-2020", archiveLogger.ListOfMessages);
				AssertCollectionContains("Max Run Duration Log", $"Information|{ArchiveManagerConstants.Codes.PDO}|Max Run Duration: 5 minutes", archiveLogger.ListOfMessages);
				AssertCollectionContains("Verbose Logging Log", $"Information|{ArchiveManagerConstants.Codes.PDO}|Verbose Logging: No", archiveLogger.ListOfMessages);
				AssertCollectionContains("Incl. Customs Log", $"Information|{ArchiveManagerConstants.Codes.PDO}|Incl. Customs: No", archiveLogger.ListOfMessages);
			});
		}

		public void TestGeneratedSummaryReport()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
			ArchiveManagerAssertions.AssertPurgeSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "PurgeDocumentsofOperationalRecordsReport_"), TestConfig.ArchiveSystemCodeToTest, archiveSystemHasMultipleArchiveStageDescriptors: false);
		}

		[UseSnapshotProtection]
		public void TestOnArchiveSetProcessed()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateConsolData(noOfShipmentsToCreate: 2, inputIndex: 0, out var consolPK, isCancelled: false, out var shipmentPKs, out _);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobHeader.JH_ParentID = consolPK;
			jobHeader.JH_SystemCreateTimeUtc = DateTime.UtcNow.AddYears(-5);

			Factory.Save();

			var systemDescriptor = new PDOPurgeSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, archiveLogger);

			var archiveSets = archiveStage.GetNextArchiveSet(null, schedule, archiveStage);
			var archiveSet = archiveSets.FirstOrDefault();
			_ = archiveSet.Load(archiveLogger);
			var mainArchiveItem = archiveSet.GetArchiveItem(jobHeader.PK.ToGuid());
			mainArchiveItem.TotalDocumentsDeleted = 5;
			var consolArchiveItem = archiveSet.GetArchiveItem(consolPK.ToGuid());
			consolArchiveItem.TotalDocumentsDeleted = 3;
			var shipmentArchiveItem = archiveSet.GetArchiveItem(shipmentPKs[0].ToGuid());
			shipmentArchiveItem.TotalDocumentsDeleted = 7;

			for (var i = 0; i < 5; i++)
			{
				stageDescriptor.OnArchiveSetProcessed(archiveSet);
			}

			var commonStageDescriptor = (CommonArchiveStageDescriptor)stageDescriptor;
			var processedRecordsCountPerTable = commonStageDescriptor.ProcessingInfoPerTable;

			Assert(processedRecordsCountPerTable.Count > 0);

			AssertCollectionContains("StorageMain should be included in the collection.", StorageMainSchema.Constants.TableName, processedRecordsCountPerTable.Keys);
			AssertCollectionContains("StorageDocs should be included in the collection.", StorageDocsSchema.Constants.TableName, processedRecordsCountPerTable.Keys);
			AssertCollectionContains("JobHeaders should be included in the collection.", JobHeaderSchema.Constants.TableName, processedRecordsCountPerTable.Keys);

			Assert("StorageMains should have Purgeable=true", processedRecordsCountPerTable[StorageMainSchema.Constants.TableName].Purged);
			Assert("StorageDocs should have Purgeable=true", processedRecordsCountPerTable[StorageDocsSchema.Constants.TableName].Purged);
			Assert("JobHeaders should have Purgeable=false", !processedRecordsCountPerTable[JobHeaderSchema.Constants.TableName].Purged);

			AssertEquals("When all StorageDocs are deleted from an ArchiveItem, then the associated StorageMain should also be deleted.", 15, processedRecordsCountPerTable[StorageMainSchema.Constants.TableName].Count);
			AssertEquals("StorageDocs should contain the total documents deleted from all archive items.", 75, processedRecordsCountPerTable[StorageDocsSchema.Constants.TableName].Count);
		}

		[UseSnapshotProtection]
		public void TestOnArchiveSetProcessed_WhenNoDocumentsWereDeleted()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK;
			jobHeader.JH_SystemCreateTimeUtc = DateTime.UtcNow.AddYears(-5);

			Factory.Save();

			var systemDescriptor = new PDOPurgeSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();

			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, archiveLogger);
			var archiveSets = archiveStage.GetNextArchiveSet(null, schedule, archiveStage);
			var archiveSet = archiveSets.FirstOrDefault();
			_ = archiveSet.Load(archiveLogger);

			Assert("Precondition", archiveSet.Count > 0);

			stageDescriptor.OnArchiveSetProcessed(archiveSet);

			var commonStageDescriptor = (CommonArchiveStageDescriptor)stageDescriptor;
			var processedRecordsCountPerTable = commonStageDescriptor.ProcessingInfoPerTable;

			CombineAssertions(() =>
			{
				Assert("There should be some ArchiveItem metadata.", processedRecordsCountPerTable.Count > 0);
				AssertEquals("None of the ArchiveItems should have been purgeable.", false, processedRecordsCountPerTable.Values.Any(v => v.Purged));
				AssertEquals("No metadata about StorageDocs or StorageMains should exist.", false, processedRecordsCountPerTable.Keys.Any(k => k == StorageDocsSchema.Constants.TableName || k == StorageMainSchema.Constants.TableName));
			});
		}

		public void TestPDOUsesCorrectDescriptors()
		{
			var systemDescriptor = new PDOPurgeSystemDescriptor();
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 1, ZDateTime.UtcNow, false, false);

			var stageDescriptors = systemDescriptor.GetArchiveStageDescriptors(config);
			AssertEquals("Should contain just the default PDO descriptor", 1, stageDescriptors.Count());
			AssertEquals("Should contain the correct type of descriptor", typeof(PDOPurgeStageDescriptor), stageDescriptors.ToArray()[0].GetType());

			config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 1, ZDateTime.UtcNow, false, false, shouldIncludeRecordsWithoutJobs: true);

			stageDescriptors = systemDescriptor.GetArchiveStageDescriptors(config);
			AssertEquals("Should contain one PDO descriptor for each table we're archiving, but not declarations", 4, stageDescriptors.Count());

			config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 1, ZDateTime.UtcNow, false, true, shouldIncludeRecordsWithoutJobs: true);

			stageDescriptors = systemDescriptor.GetArchiveStageDescriptors(config);
			AssertEquals("Should contain one PDO descriptor for each table we're archiving", 5, stageDescriptors.Count());
		}

		[UseSnapshotProtection]
		public void TestPDOIgnoresRecordsWithActiveAccountingDataWithCustoms()
		{
			TestPDOIgnoresRecordsWithActiveAccountingDataHelper(true);
		}

		[UseSnapshotProtection]
		public void TestEcommerceDataIsExcludedFromPurge()
		{
			var factory = new BusinessObjectFactory();
			var testDataCreator = ObjectFactory.Get<IEcommerceTestDataCreator>();
			var eCommerceBizOs = testDataCreator.CreateEcommerceTestData(factory);

			ArchiveManagerAssertions.AssertEcommerceTestDataExists(factory, eCommerceBizOs);

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, logger, schedule);

			Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));

			foreach (var eCommerceItem in eCommerceBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasNotArchived(factory, eCommerceItem);
			}
		}

		[UseSnapshotProtection]
		public void TestPDOIgnoresRecordsWithActiveAccountingDataWithoutCustoms()
		{
			TestPDOIgnoresRecordsWithActiveAccountingDataHelper(false);
		}

		public void TestPDOIgnoresRecordsWithActiveAccountingDataHelper(bool includeDeclarations)
		{
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var systemDescriptor = new PDOPurgeSystemDescriptor();
				var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, includeDeclarations);
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).First();
				var stage = new PDOPurgeStage(stageDescriptor, systemDescriptor);
				var schedule = new TestArchiveSchedule();

				var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
				var numberOfRecords = 2;
				var pks = new List<ZGuid>();
				for (var i = 0; i < numberOfRecords; i++)
				{
					forwardingTestDataCreator.CreateShipmentData(out var pk);
					pks.Add(pk);
				}

				CreateJobHeaders(pks);
				var jobHeaders = Factory.Load<JobHeader>(new ZQuery());

				var testCompany = Factory.NewWithValidTestData<GlbCompany>();
				var accTransactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
				var accPeriodManagement = Factory.NewWithValidTestData<AccPeriodManagement>();
				accTransactionLine.AL_JH = jobHeaders[0].PK;
				accTransactionLine.AL_GC = testCompany.PK;
				accPeriodManagement.AM_GC_Company = testCompany.PK;
				accPeriodManagement.AM_StartDate = ZDateTime.Now.AddDays(-10);
				accTransactionLine.AL_PostDate = ZDateTime.Now.AddDays(-5);
				accPeriodManagement.AM_EndDate = ZDateTime.Now.AddDays(-1);
				accPeriodManagement.AM_IsSubLedgerClosed = false;

				var accHotCheque = Factory.NewWithValidTestData<AccHotCheque>();
				accHotCheque.AQ_JH = jobHeaders[1].PK;
				accHotCheque.AQ_Cancelled = false;

				Factory.Save();

				stage.BeginRun(config, schedule, archiveLogger);

				AssertEquals("There should be no valid sets as we have accounting data", 0, stage.GetNextArchiveSet(null, schedule, stage).Count());
			}
		}

		[UseSnapshotProtection]
		[TestDate(2020, 2, 2)]
		public void TestPDODoesNotSkipOverRecordsWithJobsAndWithCustoms()
		{
			TestPDODoesNotSkipOverRecordsWithJobsHelper(true);
		}

		[UseSnapshotProtection]
		[TestDate(2020, 2, 2)]
		public void TestPDODoesNotSkipOverRecordsWithJobsAndWithoutCustoms()
		{
			TestPDODoesNotSkipOverRecordsWithJobsHelper(false);
		}

		void TestPDODoesNotSkipOverRecordsWithJobsHelper(bool includeDeclarations)
		{
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var systemDescriptor = new PDOPurgeSystemDescriptor();
				var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, includeDeclarations);
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).First();
				var stage = new PDOPurgeStage(stageDescriptor, systemDescriptor);
				var schedule = new TestArchiveSchedule();

				var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
				var numberOfRecords = 12;
				var pks = new List<ZGuid>();
				for (var i = 0; i < numberOfRecords; i++)
				{
					forwardingTestDataCreator.CreateShipmentData(out var pk);
					pks.Add(pk);
				}

				CreateJobHeaders(pks);
				var jobHeaders = Factory.Load<JobHeader>(new ZQuery());

				for (var i = 0; i < jobHeaders.Length; i++)
				{
					jobHeaders[i].JH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-i / 2);
				}

				Factory.Save();

				stage.BeginRun(config, schedule, archiveLogger);

				for (var i = 0; i < numberOfRecords; i++)
				{
					AssertEquals("Our ordering should be correct so no archive set should be empty", 1,
						stage.GetNextArchiveSet(null, schedule, stage).Count());
				}

				AssertEquals("There should be no trailing archive sets", 0, stage.GetNextArchiveSet(null, schedule, stage).Count());
			}
		}

		[UseSnapshotProtection]
		[TestDate(2020, 2, 2)]
		public void TestPDODoesNotSkipOverRecordsWithoutJobsAndWithCustoms()
		{
			TestPDODoesNotSkipOverRecordsWithoutJobsHelper(true);
		}

		[UseSnapshotProtection]
		[TestDate(2020, 2, 2)]
		public void TestPDODoesNotSkipOverRecordsWithoutJobsAndWithoutCustoms()
		{
			TestPDODoesNotSkipOverRecordsWithoutJobsHelper(false);
		}

		void TestPDODoesNotSkipOverRecordsWithoutJobsHelper(bool includeDeclarations)
		{
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var systemDescriptor = new PDOPurgeSystemDescriptor();
				var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.UtcNow, false, includeDeclarations, shouldIncludeRecordsWithoutJobs: true);
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault(x => x.Name == "Purge Documents of Operational Records without Jobs - Job Shipment");
				var stage = new PDOPurgeStage(stageDescriptor, systemDescriptor);
				var schedule = new TestArchiveSchedule();

				var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
				var numberOfRecords = 12;

				for (var i = 0; i < numberOfRecords; i++)
				{
					forwardingTestDataCreator.CreateShipmentData(out _);
				}

				var shipments = Factory.Load<IForwardingShipment>(new ZQuery());

				for (var i = 0; i < shipments.Length; i++)
				{
					shipments[shipments.Length - i - 1].JS_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-i / 2);
				}

				Factory.Save();

				stage.BeginRun(config, schedule, archiveLogger);

				for (var i = 0; i < numberOfRecords; i++)
				{
					AssertEquals("Our ordering should be correct so no archive set should be empty", 1,
						stage.GetNextArchiveSet(null, schedule, stage).Count());
				}

				AssertEquals("There should be no trailing archive sets", 0, stage.GetNextArchiveSet(null, schedule, stage).Count());
			}
		}

		[UseSnapshotProtection]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestPDODoesNotSkipOverRecordsWithTheSameJobNum()
		{
			using (SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var systemDescriptor = new PDOPurgeSystemDescriptor();
				var archiveOnOrBeforeDate = new ZDateTime(2020, 02, 02);
				var config = new ArchiveConfiguration(archiveOnOrBeforeDate, 1, ZDateTime.UtcNow, false, false);
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).First();
				var stage = new PDOPurgeStage(stageDescriptor, systemDescriptor);
				var schedule = new TestArchiveSchedule();

				var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
				forwardingTestDataCreator.CreateShipmentData(out var pk1);
				forwardingTestDataCreator.CreateShipmentData(out var pk2);
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeader.JH_ParentID = pk1;
				jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
				jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;
				jobHeader.JH_SystemCreateTimeUtc = archiveOnOrBeforeDate.AddDays(-1);

				var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
				var jobHeaderInADifferentCompany = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeaderInADifferentCompany.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeaderInADifferentCompany.JH_ParentID = pk1;
				jobHeaderInADifferentCompany.JH_GC = otherCompany.PK;
				jobHeaderInADifferentCompany.JH_Status = JobHeaderStatus.Codes.Closed;
				jobHeaderInADifferentCompany.JH_SystemCreateTimeUtc = archiveOnOrBeforeDate.AddDays(-1);

				var differentJobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				differentJobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				differentJobHeader.JH_ParentID = pk2;
				differentJobHeader.JH_GC = otherCompany.PK;
				differentJobHeader.JH_Status = JobHeaderStatus.Codes.Closed;
				differentJobHeader.JH_SystemCreateTimeUtc = archiveOnOrBeforeDate.AddDays(-1);

				Factory.Save();

				stage.BeginRun(config, schedule, archiveLogger);
				_ = stage.GetNextArchiveSet(null, schedule, stage);
				Assert("2nd archive set should not be empty: we should not skip over the same job header in a different company", stage.GetNextArchiveSet(null, schedule, stage).Count() == 1);
				Assert("3rd archive set should move onto a different jobheader", stage.GetNextArchiveSet(null, schedule, stage).FirstOrDefault().MainArchiveItemNK == differentJobHeader.JH_JobNum);
			}
		}

		[UseSnapshotProtection]
		public void TestPDOIgnoresCertainChildrenOfRatingHeader()
		{
			var systemDescriptor = new PDOPurgeSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.UtcNow, false, false, shouldIncludeRecordsWithoutJobs: true);
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).Last();
			var stage = new PDOPurgeStage(stageDescriptor, systemDescriptor);
			var schedule = new TestArchiveSchedule();

			var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
			ratingHeader.TH_QuoteDate = ZDate.Today;
			ratingHeader.TH_QuoteEndDate = ratingHeader.DefaultQuoteEndDate;
			ratingHeader.TH_QuoteNumber = ZString.Empty;
			ratingHeader.TH_OneTimeQuote = true;
			ratingHeader.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var rateEntry = ratingHeader.AddRateEntry("AIR");
			rateEntry.TI_TH = ratingHeader.PK;
			var rateLine = rateEntry.AddRateLine(chargeCode);
			var rateLineItem = rateLine.RateLineItems.AddNew();

			Factory.Save();

			stage.BeginRun(config, schedule, archiveLogger);
			var sets = stage.GetNextArchiveSet(null, schedule, stage);

			AssertEquals("There is only 1 archive set", 1, sets.Count());

			_ = sets.First().Load(archiveLogger);

			CombineAssertions("The archive set should contain only the rating header", () =>
			{
				AssertEquals("Archive set has only the root item", 1, sets.First().Count);
				AssertEquals("Archive set contains the right item", ratingHeader.PK, sets.First().MainArchiveItem.PK);
			});
		}

		[UseSnapshotProtection]
		public void TestRatingHeaderWhenArchivingWithoutJobsRatingHeaderStage()
		{
			var systemDescriptor = new PDOPurgeSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.UtcNow, false, false, shouldIncludeRecordsWithoutJobs: true);
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault(descriptor => descriptor.MainArchivePKColumn == RatingHeaderSchema.PK);
			var stage = new PDOPurgeStage(stageDescriptor, systemDescriptor);
			var schedule = new TestArchiveSchedule();

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var oneOffQuote = Factory.NewWithValidTestData<RatingHeader>();
			oneOffQuote.TH_QuoteDate = ZDate.Today;
			oneOffQuote.TH_QuoteEndDate = oneOffQuote.DefaultQuoteEndDate;
			oneOffQuote.TH_QuoteNumber = "111";
			oneOffQuote.TH_OneTimeQuote = true;
			oneOffQuote.TH_GC = company1.PK;
			oneOffQuote.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			oneOffQuote.TH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			var nonOneOffQuote = Factory.NewWithValidTestData<RatingHeader>();
			nonOneOffQuote.TH_QuoteDate = ZDate.Today;
			nonOneOffQuote.TH_QuoteEndDate = nonOneOffQuote.DefaultQuoteEndDate;
			nonOneOffQuote.TH_QuoteNumber = "222";
			nonOneOffQuote.TH_OneTimeQuote = false;
			nonOneOffQuote.TH_GC = company2.PK;
			nonOneOffQuote.TH_RateType = RatingConstants.RatingHeaderTypes.Tariff;
			nonOneOffQuote.TH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			stage.BeginRun(config, schedule, archiveLogger);
			var sets = stage.GetNextArchiveSet(null, schedule, stage);

			AssertEquals("There is only one archive set", 1, sets.Count());

			sets.FirstOrDefault().Load(archiveLogger);

			CombineAssertions("Only the RatingHeader which is a one off quote is included", () =>
			{
				AssertEquals("The ArchiveSet contains only the RatingHeader", 1, sets.First().Count);
				AssertEquals("The top-level record is correct", oneOffQuote.PK, sets.First().MainArchiveItem.PK);
			});
		}

		[UseSnapshotProtection]
		public void TestRateAttachmentOnJobHeaderDoesNotCauseHeaderToBeSkipped()
		{
			var factory = new BusinessObjectFactory();

			SetupTestData.ForExcludedRatingData(false);

			var systemDescriptor = new PDOPurgeSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.UtcNow, false, false, shouldIncludeRecordsWithoutJobs: false);
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var stage = new PDOPurgeStage(stageDescriptor, systemDescriptor);
			var schedule = new TestArchiveSchedule();

			stage.BeginRun(config, schedule, archiveLogger);
			var sets = stage.GetNextArchiveSet(null, schedule, stage);

			var jobHeader = factory.LoadTop1<JobHeader>(new ZQuery());
			var ratingHeader = factory.LoadTop1<RatingHeader>(new ZQuery());

			AssertEquals("There is only one archive set", 1, sets.Count());

			sets.FirstOrDefault().Load(archiveLogger);

			CombineAssertions("The JobHeader and RatingHeader are included, but RateAttachment is not (it doesn't support eDocs anyway)", () =>
			{
				AssertNotNull("JobHeader is present", sets.First().GetArchiveItem(jobHeader.PK.ToGuid()));
				AssertNotNull("RatingHeader is present", sets.First().GetArchiveItem(ratingHeader.PK.ToGuid()));
				AssertNull("RateAttachment is not included", sets.First().GetArchiveItems().FirstOrDefault(i => i.TableCode == RateAttachmentSchema.PK.ColumnPrefix));
			});
		}

		[UseSnapshotProtection]
		public void TestRateAttachmentOnJobShipmentOnJobHeaderDoesNotCauseHeaderToBeSkipped()
		{
			var factory = new BusinessObjectFactory();

			SetupTestData.ForExcludedRatingData(true);

			var systemDescriptor = new PDOPurgeSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.UtcNow, false, false, shouldIncludeRecordsWithoutJobs: false);
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var stage = new PDOPurgeStage(stageDescriptor, systemDescriptor);
			var schedule = new TestArchiveSchedule();

			stage.BeginRun(config, schedule, archiveLogger);
			var sets = stage.GetNextArchiveSet(null, schedule, stage);

			var jobHeader = factory.LoadTop1<JobHeader>(new ZQuery());
			var ratingHeader = factory.LoadTop1<RatingHeader>(new ZQuery());
			var jobShipment = factory.LoadTop1<ForwardingShipment>(new ZQuery());

			AssertEquals("There is only one archive set", 1, sets.Count());

			sets.FirstOrDefault().Load(archiveLogger);

			CombineAssertions("The JobHeader, JobShipment and RatingHeader are included, but RateAttachment is not (it doesn't support eDocs anyway)", () =>
			{
				AssertNotNull("JobShipment is present", sets.First().GetArchiveItem(jobShipment.PK.ToGuid()));
				AssertNotNull("JobHeader is present", sets.First().GetArchiveItem(jobHeader.PK.ToGuid()));
				AssertNotNull("RatingHeader is present", sets.First().GetArchiveItem(ratingHeader.PK.ToGuid()));
				AssertNull("RateAttachment is not included", sets.First().GetArchiveItems().FirstOrDefault(i => i.TableCode == RateAttachmentSchema.PK.ColumnPrefix));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDOWatermarkCorrectlyUpdates()
		{
			var stageDescriptorName = new PDOPurgeStageDescriptor().Name;

			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
			schedule.IsArchiveRecordsOnOrBeforeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = new ZDateTime(2018, 1, 18);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var factoryForDB333 = documentFactory.GetFactory(DbNumber);

			var jobHeader = TestHelpers.CreateJobHeaderWithRelatedDocuments(JobHeaderStatus.Working.Code, documentFactory, Factory, factoryForDB333, ObjectFactory.Get<IForwardingTestDataCreator>(), DbNumber);
			jobHeader.JH_SystemCreateTimeUtc = schedule.ArchiveRecordsOnOrBeforeDate.AddDays(-1);

			Factory.Save();
			documentFactory.Save();

			AssertDatabaseCount("JobHeaders and their related records and documents should exist", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 1, storageDocsCount: 1, factoryForDB333);

			CombineAssertions("Preconditions", () =>
			{
				AssertNull("Watermark should not be set", schedule.GetWatermark(stageDescriptorName));
				Assert("ArchiveScheduleTask.UseOnOrBeforeDateWhenWatermarkReset should be false", !schedule.UseOnOrBeforeDateWhenWatermarkReset);
			});

			schedule.Run(archiveLogger, CancellationToken.None);

			AssertDatabaseCount("eDocs should have been correctly deleted from JobShipment.", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 0, storageDocsCount: 0, factoryForDB333);

			CombineAssertions("Run #1: Watermark should be correctly updated, and records correctly processed", () =>
			{
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

				Assert("Logs should contain log about purging records", archiveLogger.ListOfMessages.Exists(log => log.Contains("Purging Records on or Before: 18-Jan-2018")));
				Assert("Logs should contain log about UseOnOrBeforeDateWhenWatermarkReset", archiveLogger.ListOfMessages.Exists(log => log.Contains("Use 'On Or Before' Date When Watermark Is Reset: No")));
				Assert("Logs should contain log about watermark being updated", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was updated to '17-Jan-18 00:00:00'")));
				Assert("Logs should contain log about loading JobHeader", archiveLogger.ListOfMessages.Exists(log => log.Contains("Loaded JobHeader 'S00001000'")));
				Assert("Logs should contain log about watermark being reset", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was reset to '01-Jan-00 00:00:00'")));

				AssertNull("Watermark should not be set", schedule.GetWatermark(stageDescriptorName));
				Assert("ArchiveScheduleTask.UseOnOrBeforeDateWhenWatermarkReset should be false", !schedule.UseOnOrBeforeDateWhenWatermarkReset);
			});

			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ArchiveMainItemQueue"); // Manually cleanup ArchiveMainItemQueue.
			archiveLogger = new TestArchiveLogger();

			schedule.Run(archiveLogger, CancellationToken.None);

			AssertDatabaseCount("Database counts should be correct", jobHeaderCount: 1, shipmentCount: 1, storageMainCount: 0, storageDocsCount: 0, factoryForDB333);

			CombineAssertions("Run #2: Records should have been processed again, even though there is now no more related eDocs", () =>
			{
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

				Assert("Logs should contain log about purging records", archiveLogger.ListOfMessages.Exists(log => log.Contains("Purging Records on or Before: 18-Jan-2018")));
				Assert("Logs should contain log about UseOnOrBeforeDateWhenWatermarkReset", archiveLogger.ListOfMessages.Exists(log => log.Contains("Use 'On Or Before' Date When Watermark Is Reset: No")));
				Assert("Logs should contain log about watermark being updated", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was updated to '17-Jan-18 00:00:00'")));
				Assert("Logs should contain log about loading JobHeader", archiveLogger.ListOfMessages.Exists(log => log.Contains("Loaded JobHeader 'S00001000'")));
				Assert("Logs should contain log about watermark being reset", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was reset to '01-Jan-00 00:00:00'")));

				AssertNull("Watermark should not be set", schedule.GetWatermark(stageDescriptorName));
				Assert("ArchiveScheduleTask.UseOnOrBeforeDateWhenWatermarkReset should be false", !schedule.UseOnOrBeforeDateWhenWatermarkReset);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestPDOWatermarkCorrectlyUpdates_WhenUseOnOrBeforeDateWhenWatermarkReset()
		{
			var stageDescriptorName = new PDOPurgeStageDescriptor().Name;

			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.PDO);
			schedule.IsArchiveRecordsOnOrBeforeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = new ZDateTime(2018, 1, 18);
			schedule.UseOnOrBeforeDateWhenWatermarkReset = true;

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var factoryForDB333 = documentFactory.GetFactory(DbNumber);

			var jobHeader1 = TestHelpers.CreateJobHeaderWithRelatedDocuments(JobHeaderStatus.Working.Code, documentFactory, Factory, factoryForDB333, ObjectFactory.Get<IForwardingTestDataCreator>(), DbNumber);
			jobHeader1.JH_SystemCreateTimeUtc = schedule.ArchiveRecordsOnOrBeforeDate.AddDays(-1);

			var jobHeader2 = TestHelpers.CreateJobHeaderWithRelatedDocuments(JobHeaderStatus.Working.Code, documentFactory, Factory, factoryForDB333, ObjectFactory.Get<IForwardingTestDataCreator>(), DbNumber);
			jobHeader2.JH_SystemCreateTimeUtc = schedule.ArchiveRecordsOnOrBeforeDate.AddDays(1);

			Factory.Save();
			documentFactory.Save();

			AssertDatabaseCount("JobHeaders and their related records and documents should exist", jobHeaderCount: 2, shipmentCount: 2, storageMainCount: 2, storageDocsCount: 2, factoryForDB333);

			CombineAssertions("Preconditions", () =>
			{
				AssertNull("Watermark should not be set", schedule.GetWatermark(stageDescriptorName));
				Assert("ArchiveScheduleTask.UseOnOrBeforeDateWhenWatermarkReset should be true", schedule.UseOnOrBeforeDateWhenWatermarkReset);
			});

			schedule.Run(archiveLogger, CancellationToken.None);

			AssertDatabaseCount("eDocs should have been correctly deleted", jobHeaderCount: 2, shipmentCount: 2, storageMainCount: 1, storageDocsCount: 1, factoryForDB333);

			CombineAssertions("Run #1: Watermark should be correctly updated, and records correctly processed", () =>
			{
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

				Assert("Logs should contain log about purging records", archiveLogger.ListOfMessages.Exists(log => log.Contains("Purging Records on or Before: 18-Jan-2018")));
				Assert("Logs should contain log about UseOnOrBeforeDateWhenWatermarkReset", archiveLogger.ListOfMessages.Exists(log => log.Contains("Use 'On Or Before' Date When Watermark Is Reset: Yes")));
				Assert("Logs should contain log about watermark being updated", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was updated to '17-Jan-18 00:00:00'")));
				Assert("Logs should contain log about loading JobHeader", archiveLogger.ListOfMessages.Exists(log => log.Contains("Loaded JobHeader 'S00001000'")));
				Assert("Logs should NOT contain log about watermark being reset", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was reset to '01-Jan-00 00:00:00'")));

				AssertNotNull("Watermark should be set from previous run", schedule.GetWatermark(stageDescriptorName));
				AssertEquals("Watermark date should be correct", new ZDateTime(2018, 1, 18), schedule.GetWatermark(stageDescriptorName).WatermarkDate);
			});

			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ArchiveMainItemQueue"); // Manually cleanup ArchiveMainItemQueue.
			archiveLogger = new TestArchiveLogger();

			// Since we're using an absolute date, we need to bump the ArchiveRecordsOrOrrBeforeDate by some amount of time (e.g., one day) so that we have a time interval of watermark (old On-Or-Before date) to new On-Or-Before date.
			// In the real world, this UseOnOrBeforeDateWhenWatermarkReset option will best be used with a relative On-Or-Before date where new records will become available for processing as time passes.
			schedule.ArchiveRecordsOnOrBeforeDate = new ZDateTime(2018, 1, 20);

			schedule.Run(archiveLogger, CancellationToken.None);

			AssertDatabaseCount("eDocs should have been correctly deleted", jobHeaderCount: 2, shipmentCount: 2, storageMainCount: 0, storageDocsCount: 0, factoryForDB333);

			CombineAssertions("Run #2: Watermark should be correctly updated, and records correctly processed", () =>
			{
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

				Assert("Logs should contain log about purging records", archiveLogger.ListOfMessages.Exists(log => log.Contains("Purging Records on or Before: 20-Jan-2018")));
				Assert("Logs should contain log about UseOnOrBeforeDateWhenWatermarkReset", archiveLogger.ListOfMessages.Exists(log => log.Contains("Use 'On Or Before' Date When Watermark Is Reset: Yes")));
				Assert("Logs should contain log about watermark already existing", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark date is '18-Jan-18 00:00:00'")));
				Assert("Logs should contain log about watermark being updated", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was updated to '19-Jan-18 00:00:00'")));
				Assert("Logs should contain log about loading JobHeader", archiveLogger.ListOfMessages.Exists(log => log.Contains("Loaded JobHeader 'S00001001'")));
				Assert("Logs should NOT contain log about watermark being reset", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was reset to '01-Jan-00 00:00:00'")));

				AssertNotNull("Watermark should be set from previous run", schedule.GetWatermark(stageDescriptorName));
				AssertEquals("Watermark date should be correct", new ZDateTime(2018, 1, 20), schedule.GetWatermark(stageDescriptorName).WatermarkDate);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestJobDeclarationAttachedToJobHeaderViaJobConsolAndNotIncludingCustomsDoesNotArchive()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

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

			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentPK, declaration.PK, consol.PK }, documentFactory, documentFactoryForDB333, DbNumber);

			documentFactory.Save();
			Factory.Save();

			var query = new ZDBOnlyQuery(typeof(JobComInvoiceHeader));
			_ = query.AddToFilter(JobComInvoiceHeaderSchema.JZ_JE, declaration.PK);
			var comInvoice = Factory.LoadTop1<JobComInvoiceGroupHeader>(query);
			comInvoice.Delete();

			Factory.Save();

			var stage = CreateAndRunArchiveConfiguration(schedule, shouldIncludeDeclarations: false);
			var set = stage.GetNextArchiveSet(null, schedule, stage).FirstOrDefault();
			AssertNull("ArchiveSet should be null as shouldIncludeDeclarations=false, and hence Job Declaration should not get archived", set);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestJobDeclarationAttachedToJobHeaderViaJobConsolAndIncludingCustomsDoesArchive()
		{
			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, isVerboseLog: true, shouldIncludeDeclarations: true);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			var documentFactory = new DocumentFactoryProvider().GetFactory(Factory);
			var documentFactoryForDB333 = documentFactory.GetFactory(DbNumber);

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

			TestHelpers.CreateRelatedDocuments(new List<ZGuid> { shipmentPK, declaration.PK, consol.PK }, documentFactory, documentFactoryForDB333, DbNumber);

			documentFactory.Save();
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
				AssertEquals(3, documentFactoryForDB333.GetDatabaseCount(typeof(StorageDocs), new ZQuery(StorageDocsSchema.SC_Desc, SQLComparisonOperator.NotEqual, "Purge Report")));
			});

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("Records should still exist and only their doccuments are purged", () =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error")));
				Assert("Logs should say something about documents deleted", archiveLogger.ListOfMessages.Any(x => x.Contains("Time taken to delete 3 document(s):")));
				AssertEquals(0, documentFactoryForDB333.GetDatabaseCount(typeof(StorageDocs), new ZQuery(StorageDocsSchema.SC_Desc, SQLComparisonOperator.NotEqual, "Purge Report")));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobConShipLink)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration)));
			});
		}

		[UseSnapshotProtection]
		public void TestForwardingeDataIsExcludedFromPurge()
		{
			var factory = new BusinessObjectFactory();
			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			var eCommerceBizOs = testDataCreator.CreateArchiveableForwardingData(factory);

			ArchiveManagerAssertions.AssertForwardingTestDataExists(factory, eCommerceBizOs);

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, logger, schedule);

			Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));

			foreach (var eCommerceItem in eCommerceBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasNotArchived(factory, eCommerceItem);
			}
		}

		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
			=> IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);

		void AssertLogs(TestArchiveLogger logger, ArchiveConfiguration config)
		{
			var count = 0;

			CombineAssertions(() =>
			{
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Registry Settings:", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|On or Before Minimum: 10", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Set Batch Size: 50", logger.ListOfMessages[count++]);

				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Configuration Parameters:", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Purging Records on or Before: {config.ArchiveJobsOnOrBeforeThisDate.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture)}", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Max Run Duration: 1 minutes", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Verbose Logging: No", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Incl. Customs: No", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Use 'On Or Before' Date When Watermark Is Reset: No", logger.ListOfMessages[count++]);

				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Executing: Purge Documents of Operational Records", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Loaded next batch of 1 JobHeader", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Loaded JobHeader 'S00001000'", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Deleted all documents related to Shipment S00001000", logger.ListOfMessages[count++]);

				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Loaded next batch of 0 JobHeader", logger.ListOfMessages[count++]);
				AssertContains($"Information|{ArchiveManagerConstants.Codes.PDO}|JobHeaders Per Hour: ", logger.ListOfMessages[count++]);
				AssertContains($"Information|{ArchiveManagerConstants.Codes.PDO}|Purged Data Dated Between Earliest Possible Date and ", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Generated Purge Report and stored on eDocs tab of the Purge Schedule", logger.ListOfMessages[count++]);
				AssertEquals($"Information|{ArchiveManagerConstants.Codes.PDO}|Completed purging records", logger.ListOfMessages[count++]);
			});
		}

		void AssertDatabaseCount(string message, int jobHeaderCount, int shipmentCount, int storageMainCount, int storageDocsCount, NumberedBusinessObjectFactory documentFactoryForDB333)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals($"Table: {JobHeaderSchema.Constants.TableName}.", jobHeaderCount, Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertEquals($"Table: {JobShipmentSchema.Constants.TableName}.", shipmentCount, Factory.GetDatabaseCount(typeof(CommonShipment)));
				AssertEquals($"Table: {StorageMainSchema.Constants.TableName}.", storageMainCount, Factory.GetDatabaseCount(typeof(StorageMain), new ZQuery(StorageMainSchema.SM_Type, SQLComparisonOperator.NotEqual, Core.Constants.DocManagerCodes.ArchiveSchedule)));
				AssertEquals($"Table: {StorageDocsSchema.Constants.TableName}.", storageDocsCount, documentFactoryForDB333.GetDatabaseCount(typeof(StorageDocs), new ZQuery(StorageDocsSchema.SC_Desc, SQLComparisonOperator.NotEqual, "Purge Report")));
			});
		}

		void AssertDatabaseCount(int storageMainCount, int storageDocsCount, Dictionary<string, TableInfo> tableRecordCounts, NumberedBusinessObjectFactory documentFactoryForDB333, params (string, int)[] databaseCounts)
		{
			foreach (var countTuple in databaseCounts)
			{
				AssertEquals($"Table: {countTuple.Item1}.", countTuple.Item2, tableRecordCounts[countTuple.Item1].RowCount);
			}

			AssertEquals($"Table: {StorageMainSchema.Constants.TableName}.", storageMainCount, Factory.GetDatabaseCount(typeof(StorageMain), new ZQuery(StorageMainSchema.SM_Type, SQLComparisonOperator.NotEqual, Core.Constants.DocManagerCodes.ArchiveSchedule)));
			AssertEquals($"Table: {StorageDocsSchema.Constants.TableName}.", storageDocsCount, documentFactoryForDB333.GetDatabaseCount(typeof(StorageDocs), new ZQuery(StorageDocsSchema.SC_Desc, SQLComparisonOperator.NotEqual, "Purge Report")));
		}

		List<StorageMain> CreateRelatedDocumentsAndReturnStorageMains(List<ZGuid> parentPKs, DocumentFactory documentFactory, NumberedBusinessObjectFactory documentFactoryForDB333)
		{
			var result = new List<StorageMain>();

			foreach (var pk in parentPKs)
			{
				var storageMain = documentFactory.New<StorageMain>();
				storageMain.SM_DB = DbNumber;
				storageMain.SM_ParentFK = pk;
				storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

				var storageDoc = documentFactoryForDB333.New<StorageDocs>();
				storageDoc.SC_SM = storageMain.PK;
				storageDoc.SC_FileName = "sample";
				storageDoc.SC_ImageData = TestFileHelper.SamplePDF;

				result.Add(storageMain);
			}

			return result;
		}

		void CreateJobHeaders(List<ZGuid> parentPKs)
		{
			foreach (var pk in parentPKs)
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeader.JH_ParentID = pk;
				jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;
			}
		}

		void CreateJobHeaders(List<(ZGuid, string)> pkAndTablePrefixTuples)
		{
			foreach (var pkAndTable in pkAndTablePrefixTuples)
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.JH_ParentTableCode = pkAndTable.Item2;
				jobHeader.JH_ParentID = pkAndTable.Item1;
				jobHeader.JH_Status = JobHeaderStatus.Codes.Closed;
			}
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

		public void TestArchiveStagesSortByMainDateFilterColumn()
			=> IntegrationTestHelper.RunArchiveStagesSortByMainDateFilterColumn(TestConfig);
	}
}
