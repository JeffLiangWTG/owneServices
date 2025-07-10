using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.CA;
using static Enterprise.Integration.Forwarding;
using ContainerPenalty = Enterprise.Freight.Business.ContainerPenalty;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.IPS
{
	public sealed class IPSArchiveSystemIntegrationTest : ArchiveSystemIntegrationTest, IArchiveSystemDescriptorIntegrationTest
	{
		public IPSArchiveSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.IPS,
				ListOfStageNames = [
					"Inactive Shipment Archive",
					"Inactive Consol Archive",
					"Inactive Rating Header Archive",
					"Inactive Job Cartage Archive",
					"Inactive Job Declaration Archive",
				]
			};
		}

		void CreateAndRunArchiveConfiguration(ArchiveScheduleTask archiveSchedule, bool shouldIncludeDeclarations)
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations);
			var algorithmEnumerator = archiveSystem.GetArchiveStages(config).GetEnumerator();
			_ = algorithmEnumerator.MoveNext();
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());
			currentArchiveManager.Run(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule, new CancellationToken());
			TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();
		}

		public void CreateData()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			var customsTestDataCreator = ObjectFactory.Get<ICustomsTestDataCreator>();

			if (TestConfig.NeedTableRecordCounts)
			{
				TestConfig.TableRecordCountsBaseline = TableRecordCountsHelper.GetTableRecordCounts();
			}

			forwardingTestDataCreator.CreateConsolData(3, 0, out var consol1PK, isCancelled: true, out var shipmentPKs, out _);//3
			TestConfig.Consol1PK = consol1PK;
			TestConfig.ShipmentPKs = shipmentPKs;
			Factory.Save();

			if (TestConfig.NeedShipmentDeclaration)
			{
				foreach (var shipmentPK in TestConfig.ShipmentPKs)
				{
					_ = customsTestDataCreator.CreateAttachedDeclarationData(shipmentPK, isCancelled: true);
				}
			}

			if (TestConfig.NeedTableRecordCounts)
			{
				TestConfig.TableRecordCountsWithTestRecords = TableRecordCountsHelper.GetTableRecordCounts();
			}
		}

		[TestDate(2024, 05, 15)]
		public void TestLoggingOfAMUsageData()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.NeedShipmentDeclaration = true;
			CreateData();

			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);
				archiveSchedule.IsArchiveRecordsOnOrBeforeRelativeDate = true;
				archiveSchedule.ArchiveRecordsOnOrBeforeNumber = 0;
				archiveSchedule.ArchiveRecordsOnOrBeforeType = "Y";
				archiveSchedule.ShouldArchiveDeclaration = true;
				archiveSchedule.MaxRunDurationInMinutes = 60;
				archiveSchedule.Run();
			}

			var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
			var messages = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManager);
			AssertEquals("There should be five usage reports, one for each stage", 5, messages.Length);

			foreach (var message in messages)
			{
				var usageProperties = message.UsageProperties;

				CombineAssertions("These assertions should apply for every stage.", () =>
				{
					AssertEquals("Database Name", Db.DatabaseName, usageProperties.Value<string>(UsageProperties.DatabaseName));

					AssertEquals("Archive Schedule Start Date/Time", ZDateTime.UtcNow.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture), usageProperties.Value<string>(UsageProperties.ArchiveScheduleStartTime));
					Assert("Archive Schedule Total Runtime Existst", usageProperties.Value<int>(UsageProperties.ArchiveScheduleTotalRuntime) > 0);
					AssertEquals("Archive System Name", ArchiveManagerConstants.Codes.IPS, usageProperties.Value<string>(UsageProperties.ArchiveSystemName));
					AssertEquals("Archive System Date Parameter", $"0 Year(s) ago", usageProperties.Value<string>(UsageProperties.ArchiveSystemDateParameters));
					Assert("Include Customs Jobs", usageProperties.Value<bool>(UsageProperties.IncludeCustomsJobsInArchiving));
					AssertEquals("Archiving Batch Size", 60, usageProperties.Value<int>(UsageProperties.ArchivingBatchSize));
				});
			}

			CombineAssertions("The total number of deleted/processed records across all stages should be correct.", () =>
			{
				AssertEquals("Total Records Deleted", 173, messages.Sum(x => x.UsageProperties.Value<int>(UsageProperties.TotalRecordsDeletedFromAllTablesDuringArchiving)));
				AssertEquals("Total JobHeaders Processed", 0, messages.Sum(x => x.UsageProperties.Value<int>(UsageProperties.TotalJobHeadersProcessedDuringArchiving)));
				AssertEquals("Total Main Records Loaded", 4, messages.Sum(x => x.UsageProperties.Value<int>(UsageProperties.TotalMainRecordLoaded)));
				AssertEquals("Total Missing Documents Generated", 58, messages.Sum(x => x.UsageProperties.Value<int>(UsageProperties.TotalMissingDocumentsGeneratedDuringArchiving)));
				AssertEquals("Total Documents Deleted", 0, messages.Sum(x => x.UsageProperties.Value<int>(UsageProperties.TotalDocumentsDeletedDuringArchiving)));
			});
		}

		public void TestDeleteInactiveRecords()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.NeedShipmentDeclaration = true;
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);
			CreateData();
			CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);

			CombineAssertions("TableRecordCounts", () =>
			{
				ArchiveManagerAssertions.AssertTableRecordCounts(JobShipmentSchema.Constants.TableName, 9, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobConsolSchema.Constants.TableName, 1, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobCartageSchema.Constants.TableName, 4, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(RatingHeaderSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobDeclarationSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			});
		}

		public void TestLoggingOfMissingDocuments()
		{
			TestConfig.NeedShipmentDeclaration = true;
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);
			CreateData();
			CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			Assert("Should say something about number of documents generated"
				, archiveLogger.ListOfMessages.Any(x => x.Contains("Time taken to generate 58 missing document(s):")));
			AssertEquals(4, archiveLogger.ListOfMessages.Count(x => x.Contains("Time taken to generate 0 missing document(s):")));
		}

		[UseSnapshotProtection]
		public void TestArchiveManagerThrowsNoExceptionWhenWeAddWaterMark()
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 5, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());
			archiveSchedule.SetWatermark("Inactive Shipment Archive", new ArchiveWatermark { WatermarkDate = new ZDateTime(2016, 11, 1) });
			AssertNoExceptionThrown(() => currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, archiveSchedule, new CancellationToken()));
		}

		[UseSnapshotProtection]
		public void TestArchiveLoggerContainsArchiveConfigurationParameters()
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 5, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, archiveSchedule, new CancellationToken());

			CombineAssertions("Configuration logs did not contain the correct messages", () =>
			{
				AssertCollectionContains("Configuration Parameter Log", $"Information|{archiveSystem.Descriptor.Code}|Configuration Parameters:", archiveLogger.ListOfMessages);
				AssertCollectionContains("On or Before Log", $"Information|{archiveSystem.Descriptor.Code}|Archiving Records on or Before: 2-Feb-2020", archiveLogger.ListOfMessages);
				AssertCollectionContains("Max Run Duration Log", $"Information|{archiveSystem.Descriptor.Code}|Max Run Duration: 5 minutes", archiveLogger.ListOfMessages);
				AssertCollectionContains("Verbose Logging Log", $"Information|{archiveSystem.Descriptor.Code}|Verbose Logging: No", archiveLogger.ListOfMessages);
				AssertCollectionContains("Incl. Customs Log", $"Information|{archiveSystem.Descriptor.Code}|Incl. Customs: Yes", archiveLogger.ListOfMessages);
			});
		}

		public void TestDeleteInactiveRecordsWithAccountingData()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.NeedShipmentDeclaration = true;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsForwardingDataCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);
			SetupTestData.ForCommonCase(TestConfig);
			CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);

			CombineAssertions("TableRecordCounts", () =>
			{
				ArchiveManagerAssertions.AssertTableRecordCounts(JobShipmentSchema.Constants.TableName, 9, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobConsolSchema.Constants.TableName, 1, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobCartageSchema.Constants.TableName, 4, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(RatingHeaderSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobDeclarationSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			});
		}

		public void TestGeneratedSummaryReport()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals("Report for each stage should have been generated.", 5, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
			ArchiveManagerAssertions.AssertArchiveSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "InactiveOperationalJobsArchiveSystemReport_JobShipment_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: JobShipmentSchema.Constants.TableName);
		}

		public void TestIPSInactiveShipmentArchiveStageDescriptor_WhenNotIncludingDeclarations()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			_ = jobDeclaration.Invoices.AddNew();
			jobDeclaration.JE_JS = shipmentPK;
			jobDeclaration.IsCancelled = true;

			Factory.Save();

			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new IPSInactiveShipmentArchiveStageDescriptor(), systemDescriptor);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			CombineAssertions(() =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error|IPS|")));
			});
		}

		public void TestIPSInactiveShipmentArchiveStageDescriptorWhenNotIncludingDeclarationsWithCusSCAHouseAttached()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK, isCancelled: true);

			var cusSCAHouse = Factory.NewWithValidTestData<BaseCusSCAHouse>();
			cusSCAHouse.CA_JS = shipmentPK;

			Factory.Save();
			RunARCWithoutDeclarationsAndCheckNoErrorOccurs(new IPSInactiveShipmentArchiveStageDescriptor());
		}

		public void TestIPSInactiveShipmentArchiveStageDescriptorWithOceanBillOnAttachedJobConsol()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipment1PK, isCancelled: true);
			forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: false, out _, out _);

			var cusSCAOceanBill = Factory.NewWithValidTestData<BaseCusSCAOceanBill>();
			cusSCAOceanBill.CB_ParentTableCode = "JK";
			cusSCAOceanBill.CB_ParentId = consol1PK;

			var jobConShipLinkWithOceanBill = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLinkWithOceanBill.JN_JK = consol1PK;
			jobConShipLinkWithOceanBill.JN_JS = shipment1PK;

			Factory.Save();
			RunARCWithoutDeclarationsAndCheckNoErrorOccurs(new IPSInactiveShipmentArchiveStageDescriptor());

			var jobShipmentThatShouldNotBeDeleted = new BusinessObjectFactory().Load<ICommonShipment>(shipment1PK);
			AssertNotNull("Shipment had a customs ocean bill on an attached job consol; it should not be deleted", jobShipmentThatShouldNotBeDeleted);
		}

		public void TestIPSInactiveConsolArchiveStageDescriptorWithOceanBillOnConsol()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: true, out _, out _);

			var cusSCAOceanBill = Factory.NewWithValidTestData<BaseCusSCAOceanBill>();
			cusSCAOceanBill.CB_ParentTableCode = "JK";
			cusSCAOceanBill.CB_ParentId = consol1PK;

			Factory.Save();
			RunARCWithoutDeclarationsAndCheckNoErrorOccurs(new IPSInactiveConsolArchiveStageDescriptor());

			var jobConsolThatShouldNotBeDeleted = new BusinessObjectFactory().Load<ICommonConsol>(consol1PK);
			AssertNotNull("Consol had a customs ocean bill; it should not be deleted", jobConsolThatShouldNotBeDeleted);
		}

		public void TestIPSInactiveShipmentArchiveStageDescriptorWithCusCAeMHMasterOnAttachedJobConsol()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipment1PK, isCancelled: true);
			forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: false, out _, out _);

			var cusCAeMHMaster = Factory.New<ICusCAeMHMaster>();
			cusCAeMHMaster.BP_ParentTableCode = "JK";
			cusCAeMHMaster.BP_ParentID = consol1PK;

			var jobConShipLinkWithCusCAeMHMasterl = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLinkWithCusCAeMHMasterl.JN_JK = consol1PK;
			jobConShipLinkWithCusCAeMHMasterl.JN_JS = shipment1PK;

			Factory.Save();
			RunARCWithoutDeclarationsAndCheckNoErrorOccurs(new IPSInactiveShipmentArchiveStageDescriptor());

			var jobShipmentThatShouldNotBeDeleted = new BusinessObjectFactory().Load<ICommonShipment>(shipment1PK);
			AssertNotNull("Shipment had a Canadian master bill on an attached job consol; it should not be deleted", jobShipmentThatShouldNotBeDeleted);
		}

		public void TestIPSInactiveConsolArchiveStageDescriptorWithCusCAeMHMasterOnJobConsol()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: true, out _, out _);

			var cusCAeMHMaster = Factory.New<ICusCAeMHMaster>();
			cusCAeMHMaster.BP_ParentTableCode = "JK";
			cusCAeMHMaster.BP_ParentID = consol1PK;

			Factory.Save();
			RunARCWithoutDeclarationsAndCheckNoErrorOccurs(new IPSInactiveConsolArchiveStageDescriptor());

			var jobConsolThatShouldNotBeDeleted = new BusinessObjectFactory().Load<ICommonConsol>(consol1PK);
			AssertNotNull("Consol had a Canadian master bill; it should not be deleted", jobConsolThatShouldNotBeDeleted);
		}

		public void TestIPSInactiveShipmentArchiveStageDescriptorWithAsycudaManifestHeaderOnAttachedJobConsol()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipment1PK, isCancelled: true);
			forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: false, out _, out _);

			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_ParentTableCode = "JK";
			asycudaManifestHeader.AMA_ParentId = consol1PK;

			var jobConShipLinkWithCusCAeMHMasterl = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLinkWithCusCAeMHMasterl.JN_JK = consol1PK;
			jobConShipLinkWithCusCAeMHMasterl.JN_JS = shipment1PK;

			Factory.Save();
			RunARCWithoutDeclarationsAndCheckNoErrorOccurs(new IPSInactiveShipmentArchiveStageDescriptor());

			var jobShipmentThatShouldNotBeDeleted = new BusinessObjectFactory().Load<ICommonShipment>(shipment1PK);
			AssertNotNull("Shipment had an ASYCUDA manifest header on an attached job consol; it should not be deleted", jobShipmentThatShouldNotBeDeleted);
		}

		public void TestIPSInactiveConsolArchiveStageDescriptorWithAsycudaManifestHeaderOnJobConsol()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: true, out _, out _);

			var asycudaManifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			asycudaManifestHeader.AMA_ParentTableCode = "JK";
			asycudaManifestHeader.AMA_ParentId = consol1PK;

			Factory.Save();
			RunARCWithoutDeclarationsAndCheckNoErrorOccurs(new IPSInactiveConsolArchiveStageDescriptor());

			var jobConsolThatShouldNotBeDeleted = new BusinessObjectFactory().Load<ICommonConsol>(consol1PK);
			AssertNotNull("Consol had an ASYCUDA manifest header; it should not be deleted", jobConsolThatShouldNotBeDeleted);
		}

		public void TestIPSInactiveShipmentArchiveStageDescriptor_WhenNotIncludingDeclarationsButThereIsRelatedCusHAWB()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK, isCancelled: true);

			var cusHAWB = Factory.NewWithValidTestData<CusHAWB>();
			cusHAWB.CS_JS = shipmentPK;

			RunARCWithoutDeclarationsAndCheckNoErrorOccurs(new IPSInactiveShipmentArchiveStageDescriptor());
		}

		void RunARCWithoutDeclarationsAndCheckNoErrorOccurs(IArchiveStageDescriptor stageDescriptor)
		{
			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			CombineAssertions(() =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error|IPS|")));
			});
		}

		public void TestCusSCADepotHouseIsAttachedToJobShipment()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var cusSCADepotHouse = Factory.NewWithValidTestData<Customs.AU.Declaration.Business.CusSCADepotHouse>();
			cusSCADepotHouse.CX_JS = shipmentPK;

			var shipment = Factory.Load<IForwardingShipment>(shipmentPK);
			shipment.JS_IsCancelled = true;

			Factory.Save();

			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new IPSInactiveShipmentArchiveStageDescriptor(), systemDescriptor);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			CombineAssertions(() =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error|IPS|")));
			});
		}

		public void TestCusOutturnIsLinkedToJobShipment()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var cusOutturn = Factory.NewWithValidTestData<CusOutturn>();
			cusOutturn.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cusOutturn.C5_ParentID = shipmentPK;

			var shipment = Factory.Load<IForwardingShipment>(shipmentPK);
			shipment.JS_IsCancelled = true;

			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new IPSInactiveShipmentArchiveStageDescriptor(), systemDescriptor);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);

			Factory.Save();

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			var newFactory = new BusinessObjectFactory();

			CombineAssertions("Nothing should have been deleted", () =>
			{
				AssertNotNull(newFactory.Load<IForwardingShipment>(shipmentPK));
				AssertNotNull(newFactory.LoadTop1<CusOutturn>(new ZQuery()));
			});

			config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: true);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			newFactory = new BusinessObjectFactory();

			CombineAssertions("Records should now be deleted", () =>
			{
				AssertNull(newFactory.Load<IForwardingShipment>(shipmentPK));
				AssertNull(newFactory.LoadTop1<CusOutturn>(new ZQuery()));
			});
		}

		public void TestRunArchiveSystem_WhenJobConsolLinkedToJobDeclaration()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateConsolDataWithShipment(out _, out var shipmentPK);

			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobDeclaration.JE_JS = shipmentPK;
			jobDeclaration.IsCancelled = true;

			Factory.Save();

			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new IPSInactiveConsolArchiveStageDescriptor(), systemDescriptor);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			CombineAssertions(() =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error|IPS|")));
			});
		}

		public void TestCusDecHouseBillIsAttachedToJobShipment()
		{
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			forwardingTestDataCreator.CreateShipmentData(out var shipmentPK);

			var shipment = Factory.Load<IForwardingShipment>(shipmentPK);
			shipment.JS_IsCancelled = true;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ClusterKey = 1;

			var bill = declaration.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.CU_JS = shipmentPK;

			Factory.Save();

			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new IPSInactiveShipmentArchiveStageDescriptor(), systemDescriptor);
			var config = new ArchiveConfiguration(ZDateTime.Now, 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.IPS);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			CombineAssertions(() =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error|IPS|")));
			});
		}

		[UseSnapshotProtection]
		public void TestHVLVShipmentsAreExcludedFromArchiving()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_IsCancelled = true;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			jobHeader.JH_JobNum = "T00001";
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipment.PK;

			Factory.Save();

			CombineAssertions("Precondition: Rows successfully created", () =>
			{
				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(HVLVConsignmentHeader)));
			});

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("HVLV-type Shipment is successfully skipped", () =>
			{
				Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

				AssertEquals(1, Factory.GetDatabaseCount(typeof(JobHeader)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(HVLVConsignmentHeader)));
			});
		}

		[UseSnapshotProtection]
		public void TestJobContainerPenaltyAttatchedToJobShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001";
			shipment.JS_IsCancelled = true;

			var jobContainerPenalty = Factory.NewWithValidTestData<ContainerPenalty>();
			jobContainerPenalty.CPY_JS_Shipment = shipment.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Expected Job Shipmment to exist.", 1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals("Expected Job Container Penalty to exist.", 1, Factory.GetDatabaseCount(typeof(ContainerPenalty)));
			});

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertEquals("Expected  Job Shipmment to be deleted.", 0, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals("Expected Job Container Penalty to be deleted.", 0, Factory.GetDatabaseCount(typeof(ContainerPenalty)));
			});

			CombineAssertions("Expect no errors or reference contraints", () =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error|IPS|")));
			});
		}

		[UseSnapshotProtection]
		public void TestArchiveRatingHeaderWithRateAttachment()
		{
			var ratingHeader = Factory.NewWithValidTestData<RatingHeader>();
			var glbCompany = Factory.New<GlbCompany>();
			ratingHeader.TH_GC = glbCompany.PK;
			ratingHeader.TH_RateType = "GLB";
			ratingHeader.TH_QuoteDate = ZDate.Today.AddMonths(-1);
			ratingHeader.TH_IsCancelled = true;

			var attachmentSet = Factory.New<RateAttachmentSet>();
			attachmentSet.TS_AttachmentName = "Test Document";

			var rateAttachment = Factory.New<RateAttachment>();
			rateAttachment.TA_TH = ratingHeader.PK;
			rateAttachment.TA_TS = attachmentSet.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("The RatingHeader is successfully skipped without an error", () =>
			{
				Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

				AssertEquals(1, Factory.GetDatabaseCount(typeof(RatingHeader)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(RateAttachment)));
			});
		}

		[UseSnapshotProtection]
		public void TestForwardingDataIsExcludedFromArchiving()
		{
			var factory = new BusinessObjectFactory();
			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			var eCommerceBizOs = testDataCreator.CreateArchiveableForwardingData(factory, isActiveProcess: false);

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

		[UseSnapshotProtection]
		public void TestArchiveJobShipmentWithRateAttachmentAttachedToOneOffQuote_WithDeclarations()
			=> ArchiveJobShipmentWithRateAttachmentAttachedToOneOffQuote(shouldIncludeDeclarations: true);

		[UseSnapshotProtection]
		public void TestArchiveJobShipmentWithRateAttachmentAttachedToOneOffQuote_WithoutDeclarations()
			=> ArchiveJobShipmentWithRateAttachmentAttachedToOneOffQuote(shouldIncludeDeclarations: false);

		void ArchiveJobShipmentWithRateAttachmentAttachedToOneOffQuote(bool shouldIncludeDeclarations)
		{
			var ratingHeader = Factory.New<RatingHeader>();
			ratingHeader.TH_RateType = RatingConstants.RatingHeaderTypes.Quote;
			ratingHeader.TH_QuoteDate = ZDate.Today;

			var jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobShipment.JS_TH_OneTimeQuote = ratingHeader.PK;
			jobShipment.IsCancelled = true;

			var attachmentSet = Factory.New<RateAttachmentSet>();
			attachmentSet.TS_AttachmentName = "Test";

			var rateAttachment = Factory.New<RateAttachment>();
			rateAttachment.TA_TH = ratingHeader.PK;
			rateAttachment.TA_TS = attachmentSet.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("Records should be successfully skipped without an error", () =>
			{
				Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingShipment)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(RatingHeader)));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(RateAttachment)));
			});
		}

		public void TestArchiveStagesSortByMainDateFilterColumn()
			=> IntegrationTestHelper.RunArchiveStagesSortByMainDateFilterColumn(TestConfig);

		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
			=> IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);

		[UseSnapshotProtection]
		public void TestEcommerceDataIsExcludedFromArchiving()
		{
			var factory = new BusinessObjectFactory();
			var testDataCreator = ObjectFactory.Get<IEcommerceTestDataCreator>();
			var eCommerceBizOs = testDataCreator.CreateEcommerceTestData(factory, isActiveProcess: false);

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
	}
}
