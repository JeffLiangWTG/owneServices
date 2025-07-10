using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.HAR
{
	public sealed class HARArchiveSystemNonTransactionedTest : ArchiveSystemIntegrationTest, IArchiveSystemDescriptorIntegrationTest
	{
		public HARArchiveSystemNonTransactionedTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.HAR,
				ListOfStageNames = ["HVLV Archive"],
			};
		}

		[TestDate(2024, 05, 15)]
		public void TestLoggingOfAMUsageData()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.HAR) + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.HAR) + 1));

			var consignmentHeader1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			_ = shipment2.GetOrCreateHVLVConsignmentHeader();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HCH_Header = consignmentHeader1.PK;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_JS_LoadedOnShipment = shipment1.PK;
			item.HVI_HVC_Consignment = consignment.PK;

			var itemLine = Factory.NewWithValidTestData<HVLVItemLine>();
			itemLine.HVS_HVI_HVLVItem = item.PK;
			itemLine.HVS_Quantity = 1;

			Factory.Save();

			using (SystemDataRegistry.Instance.HVLVArchiveSystemBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);
				archiveSchedule.IsArchiveRecordsOnOrBeforeDate = true;
				archiveSchedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.UtcNow;
				archiveSchedule.Run();
			}

			var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
			var messages = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManager);
			AssertEquals("There should be one and only one usage report", 1, messages.Length);

			var usageProperties = messages[0].UsageProperties;

			ArchiveManagerAssertions.AssertResultsForAMUsageCollector(
				usageProperties: usageProperties,
				archiveCode: TestConfig.ArchiveSystemCodeToTest,
				archiveSystemStageName: "HVLV Archive",
				includeCustomsJobs: false,
				batchSize: 60,
				totalRecordsDeleted: 3,
				jobHeadersProcessed: 0,
				mainRecordsLoaded: 2,
				totalMissingDocumentsGenerated: 0,
				documentsDeleted: 0);
		}

		[UseSnapshotProtection]
		public void TestForMultipleShipments()
		{
			var onOrBeforeMinimumValue = RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.HAR);

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(onOrBeforeMinimumValue + 1));
			shipment2.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(onOrBeforeMinimumValue + 1));

			var consignmentHeader1 = shipment1.GetOrCreateHVLVConsignmentHeader();
			var consignmentHeader2 = shipment2.GetOrCreateHVLVConsignmentHeader();
			var consignmentHeader3 = shipment3.GetOrCreateHVLVConsignmentHeader();
			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment3 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_HCH_Header = consignmentHeader1.PK;
			consignment2.HVC_HCH_Header = consignmentHeader2.PK;
			consignment3.HVC_HCH_Header = consignmentHeader3.PK;
			consignment1.HVC_OH_LastMileCarrier = OrgHeader.DefaultOrg.PK;

			var item1 = consignment1.Items.AddNew();
			var item2 = consignment2.Items.AddNew();
			var item3 = consignment3.Items.AddNew();
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;
			item2.HVI_JS_LoadedOnShipment = shipment2.PK;
			item3.HVI_JS_LoadedOnShipment = shipment3.PK;

			var itemLine1 = item1.Lines.AddNew();
			var itemLine2 = item2.Lines.AddNew();
			var itemLine3 = item3.Lines.AddNew();
			itemLine1.HVS_Quantity = 1;
			itemLine2.HVS_Quantity = 1;
			itemLine3.HVS_Quantity = 1;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertNotArchived(consignmentHeader1, consignment1, item1, itemLine1);
				AssertNotArchived(consignmentHeader2, consignment2, item2, itemLine2);
				AssertNotArchived(consignmentHeader3, consignment3, item3, itemLine3);
			});

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow.AddMonths(-onOrBeforeMinimumValue), 10, ZDateTime.UtcNow, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			TestHelpers.RunArchiveSystem(ArchiveManagerConstants.Codes.HAR, configuration, archiveLogger, schedule);

			var newFactory = new BusinessObjectFactory();

			consignmentHeader1 = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader1.PK);
			consignmentHeader2 = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader2.PK);
			consignmentHeader3 = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader3.PK);

			consignment1 = newFactory.Load<HVLVConsignment>(consignment1.PK);
			consignment2 = newFactory.Load<HVLVConsignment>(consignment2.PK);
			consignment3 = newFactory.Load<HVLVConsignment>(consignment3.PK);

			item1 = newFactory.Load<HVLVItem>(item1.PK);
			item2 = newFactory.Load<HVLVItem>(item2.PK);
			item3 = newFactory.Load<HVLVItem>(item3.PK);

			itemLine1 = newFactory.Load<HVLVItemLine>(itemLine1.PK);
			itemLine2 = newFactory.Load<HVLVItemLine>(itemLine2.PK);
			itemLine3 = newFactory.Load<HVLVItemLine>(itemLine3.PK);

			CombineAssertions(() =>
			{
				AssertArchived(consignmentHeader1, consignment1, item1, itemLine1);
				AssertArchived(consignmentHeader2, consignment2, item2, itemLine2);
				AssertNotArchived(consignmentHeader3, consignment3, item3, itemLine3);

				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		void AssertArchived(HVLVConsignmentHeader consignmentHeader, HVLVConsignment consignment, HVLVItem item, HVLVItemLine itemLine)
		{
			Assert("Expected consignment header to be archived.", consignmentHeader.HCH_IsArchived);
			AssertNull("Expected consignment to be deleted.", consignment);
			AssertNull("Expected item to be deleted.", item);
			AssertNull("Expected line to be deleted.", itemLine);
		}

		void AssertNotArchived(HVLVConsignmentHeader consignmentHeader, HVLVConsignment consignment, HVLVItem item, HVLVItemLine itemLine)
		{
			Assert("Expected consignment header not to be archived.", !consignmentHeader.HCH_IsArchived);
			AssertNotNull("Expected consignment not to be deleted.", consignment);
			AssertNotNull("Expected item not to be deleted.", item);
			AssertNotNull("Expected line shoud not to be deleted.", itemLine);
		}

		[UseSnapshotProtection]
		public void TestNoReferenceConstraintOnConsignmentWhenHVLVItemIsLocked()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_E_ARV = ZDateTime.UtcNow.AddMonths(-(RegistryHelper.GetOnOrBeforeMinimumValue(ArchiveManagerConstants.Codes.HAR + 1)));
			shipment.JS_UniqueConsignRef = "SHP12345";

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			Factory.Save();

			CombineAssertions("Precondition", () => AssertNotArchived(consignmentHeader, consignment, item, itemLine));

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.HAR);

			var stage = CreateAndRunArchiveConfiguration(schedule, shouldIncludeDeclarations: false, logger);
			var set = stage.GetNextArchiveSet(null, schedule, stage).FirstOrDefault();
			_ = set.Load(logger);

			using (var blockingConnection = Db.NewAdminConnection())
			using (var blockingTransactionManager = blockingConnection.BeginTransactionWithManager())
			{
				blockingConnection.ExecuteScalar($"SELECT * FROM HVLVItem WITH (XLOCK, HOLDLOCK);");

				using (var blockedConnection = Db.NewAdminConnection())
				using (blockedConnection.TemporarySetLockTimeout(500))
				{
					Db.ConnectionOverrideForTest = blockedConnection;
					var loadResult = stage.ArchiveToImages(set);
					stage.EndRun();
					Db.ConnectionOverrideForTest = null;
				}
			}

			var newFactory = new BusinessObjectFactory();
			shipment.Reload();
			consignmentHeader.Reload();
			consignment.Reload();
			item.Reload();
			itemLine.Reload();

			CombineAssertions( () =>
			{
				Assert("Logs shouldn't contain any reference constraint errors.", !logger.ListOfMessages.Exists(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				Assert("Logs should contain a lock request timeout error", logger.ListOfMessages.Exists(log => log.Contains("Lock request time out period exceeded")));
				AssertNotNull("Expected consignment not to be deleted.", consignment);
				AssertNotNull("Expected item not to be deleted.", item);
				AssertNotNull("Expected line shoud not to be deleted.", itemLine);
			});
		}

		IArchiveStage CreateAndRunArchiveConfiguration(ArchiveScheduleTask archiveSchedule, bool shouldIncludeDeclarations, TestArchiveLogger logger)
		{
			var archiveSystem = TestHelpers.GetArchiveSystem("HAR");
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations);
			var stage = archiveSystem.GetArchiveStages(config).Single();
			stage.BeginRun(config, archiveSchedule, logger);
			return stage;
		}

		public void TestGeneratedSummaryReport()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
			ArchiveManagerAssertions.AssertArchiveSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "HVLVArchiveSystemReport_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: JobShipmentSchema.Constants.TableName,  archiveSystemHasMultipleArchiveStageDescriptors: false);
		}

		public void TestArchiveStagesSortByMainDateFilterColumn()
			=> IntegrationTestHelper.RunArchiveStagesSortByMainDateFilterColumn(TestConfig);

		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
			=> IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);
	}
}
