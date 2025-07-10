using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.eTail.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.STA
{
	public sealed class STAArchiveSystemIntegrationTest : ArchiveSystemIntegrationTest, IArchiveSystemDescriptorIntegrationTest
	{
		public STAArchiveSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.STA,
				ListOfStageNames = ["Standalone Canadian Customs eManifest Master Archive",
					"Standalone Canadian Customs eManifest House Archive",
					"Standalone Customs Temporary Storage Job Header Archive",
					"Standalone Customs Temporary Storage Register Header Archive",
					"Standalone Customs Exit Header Archive",
					"Standalone Asycuda Manifest Header Archive",
					"Standalone Customs In-Bond Header Archive",
					"Standalone Customs Sea Cargo Ocean Bill Archive",
					"Standalone Customs Underbond Archive",
					"Standalone Customs Intrastat Group Archive",
					"Standalone Customs Intrastat Header Archive",
					"Standalone HVLV Booking Header Archive",
					"Standalone HVLV Origin Load List Archive"
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
			var bPPK1 = Guid.NewGuid();
			var bPPK2 = Guid.NewGuid();
			var bPPK3 = Guid.NewGuid();
			var bPPK4 = Guid.NewGuid();
			var bWPK1 = Guid.NewGuid();
			var bWPK2 = Guid.NewGuid();
			var bWPK3 = Guid.NewGuid();
			var bWPK4 = Guid.NewGuid();
			var cAGC = Guid.NewGuid();
			var cAGB = Guid.NewGuid();

			if (TestConfig.NeedTableRecordCounts)
			{
				TestConfig.TableRecordCountsBaseline = TableRecordCountsHelper.GetTableRecordCounts();
			}

			var script = $@"
INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) VALUES ('{cAGC}', 'CA', 'AUD', 'AAA', 'CA company');
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES ('{cAGB}', '{cAGC}', 'CCC');

INSERT INTO dbo.CusCAeMHMaster(BP_PK, BP_GB_Branch, BP_SystemCreateTimeUtc, BP_SystemCreateUser, BP_SystemLastEditTimeUtc, BP_SystemLastEditUser, BP_ParentID) VALUES('{bPPK1}', '{cAGB}', '2021-12-01 12:00:00', '~BP', '2021-12-01 12:00:00', '~BP',Null)
INSERT INTO dbo.CusCAeMHMaster(BP_PK, BP_GB_Branch, BP_SystemCreateTimeUtc, BP_SystemCreateUser, BP_SystemLastEditTimeUtc, BP_SystemLastEditUser, BP_D4MessageStatus, BP_ParentID) VALUES('{bPPK2}', '{cAGB}', '2021-12-01 12:00:00', '~BP', '2021-12-01 12:00:00', '~BP', '8000',Null)
INSERT INTO dbo.CusCAeMHMaster(BP_PK, BP_GB_Branch, BP_SystemCreateTimeUtc, BP_SystemCreateUser, BP_SystemLastEditTimeUtc, BP_SystemLastEditUser, BP_RNSProcessingDate, BP_ParentID) VALUES('{bPPK3}', '{cAGB}', '2021-12-01 12:00:00', '~BP', '2021-12-01 12:00:00', '~BP', '2021-12-01 10:00:00',Null)
INSERT INTO dbo.CusCAeMHMaster(BP_PK, BP_GB_Branch, BP_SystemCreateTimeUtc, BP_SystemCreateUser, BP_SystemLastEditTimeUtc, BP_SystemLastEditUser, BP_D4MessageStatus, BP_RNSProcessingDate, BP_ParentID) VALUES('{bPPK4}', '{cAGB}', '2021-12-01 12:00:00', '~BP', '2021-12-01 12:00:00', '~BP', '8000', '2021-12-01 10:00:00',Null)

INSERT INTO dbo.CusCAeMHHouse(BW_PK, BW_BP_Master, BW_MessageReference, BW_SystemCreateTimeUtc, BW_SystemCreateUser, BW_SystemLastEditTimeUtc, BW_SystemLastEditUser) VALUES('{bWPK1}', '{bPPK1}', '1', '2021-12-01 12:00:00', '~BP', '2021-12-01 12:00:00', '~BP')
INSERT INTO dbo.CusCAeMHHouse(BW_PK, BW_BP_Master, BW_MessageReference, BW_SystemCreateTimeUtc, BW_SystemCreateUser, BW_SystemLastEditTimeUtc, BW_SystemLastEditUser, BW_D4MessageStatus) VALUES('{bWPK2}', '{bPPK2}', '2', '2021-12-01 12:00:00', '~BP', '2021-12-01 12:00:00', '~BP', '8000')
INSERT INTO dbo.CusCAeMHHouse(BW_PK, BW_BP_Master, BW_MessageReference, BW_SystemCreateTimeUtc, BW_SystemCreateUser, BW_SystemLastEditTimeUtc, BW_SystemLastEditUser, BW_RNSProcessingDate) VALUES('{bWPK3}', '{bPPK3}', '3', '2021-12-01 12:00:00', '~BP', '2021-12-01 12:00:00', '~BP', '2021-12-01 10:00:00')
INSERT INTO dbo.CusCAeMHHouse(BW_PK, BW_BP_Master, BW_MessageReference, BW_SystemCreateTimeUtc, BW_SystemCreateUser, BW_SystemLastEditTimeUtc, BW_SystemLastEditUser, BW_D4MessageStatus, BW_RNSProcessingDate) VALUES('{bWPK4}', '{bPPK4}', '4', '2021-12-01 12:00:00', '~BP', '2021-12-01 12:00:00', '~BP', '8000', '2021-12-01 10:00:00')
";
			_ = Db.Connection.Command(script).ExecuteNonQuery();

			if (TestConfig.NeedTableRecordCounts)
			{
				TestConfig.TableRecordCountsWithTestRecords = TableRecordCountsHelper.GetTableRecordCounts();
			}
		}

		public void TestDeleteStandaloneRecords()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.NeedShipmentDeclaration = true;
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			CreateData();
			CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);

			AssertEquals(0, TestConfig.TableRecordCountsAfterRun[CusCAeMHMasterSchema.Constants.TableName].RowCount);
			AssertEquals(0, TestConfig.TableRecordCountsAfterRun[CusCAeMHHouseSchema.Constants.TableName].RowCount);
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
				AssertCollectionContains("Configuration Parameter Log", $"Information|{ArchiveManagerConstants.Codes.STA}|Configuration Parameters:", archiveLogger.ListOfMessages);
				AssertCollectionContains("On or Before Log", $"Information|{ArchiveManagerConstants.Codes.STA}|Archiving Records on or Before: 2-Feb-2020", archiveLogger.ListOfMessages);
				AssertCollectionContains("Max Run Duration Log", $"Information|{ArchiveManagerConstants.Codes.STA}|Max Run Duration: 5 minutes", archiveLogger.ListOfMessages);
				AssertCollectionContains("Verbose Logging Log", $"Information|{ArchiveManagerConstants.Codes.STA}|Verbose Logging: No", archiveLogger.ListOfMessages);
				AssertCollectionContains("Incl. Customs Log", $"Information|{ArchiveManagerConstants.Codes.STA}|Incl. Customs: Yes", archiveLogger.ListOfMessages);
			});
		}

		[TestDate(2024, 05, 15)]
		public void TestLoggingOfAMUsageData()
		{
			TestConfig.NeedShipmentDeclaration = true;
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			CreateData();

			using (SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				archiveSchedule.IsArchiveRecordsOnOrBeforeRelativeDate = true;
				archiveSchedule.ArchiveRecordsOnOrBeforeNumber = 0;
				archiveSchedule.ArchiveRecordsOnOrBeforeType = "M";
				archiveSchedule.ShouldArchiveDeclaration = true;
				archiveSchedule.MaxRunDurationInMinutes = 60;
				archiveSchedule.Run();
			}

			var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
			var messages = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManager);
			AssertEquals("There should be thirteen usage reports, one for each stage", 13, messages.Length);

			foreach (var message in messages)
			{
				var usageProperties = message.UsageProperties;

				CombineAssertions("These assertions should apply for every stage.", () =>
				{
					AssertEquals("Database Name", Db.DatabaseName, usageProperties.Value<string>(UsageProperties.DatabaseName));

					AssertEquals("Archive Schedule Start Date/Time", ZDateTime.UtcNow.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture), usageProperties.Value<string>(UsageProperties.ArchiveScheduleStartTime));
					Assert("Archive Schedule Total Runtime Exists", usageProperties.Value<int>(UsageProperties.ArchiveScheduleTotalRuntime) > 0);
					AssertEquals("Archive System Name", ArchiveManagerConstants.Codes.STA, usageProperties.Value<string>(UsageProperties.ArchiveSystemName));
					AssertEquals("Archive System Date Parameter", $"0 Month(s) ago", usageProperties.Value<string>(UsageProperties.ArchiveSystemDateParameters));
					Assert("Include Customs Jobs", usageProperties.Value<bool>(UsageProperties.IncludeCustomsJobsInArchiving));
					AssertEquals("Archiving Batch Size", 60, usageProperties.Value<int>(UsageProperties.ArchivingBatchSize));
				});
			}

			CombineAssertions("The total number of deleted/processed records across all stages should be correct.", () =>
			{
				AssertEquals("Total Records Deleted", 8, messages.Select(x => x.UsageProperties.Value<int>(UsageProperties.TotalRecordsDeletedFromAllTablesDuringArchiving)).Sum());
				AssertEquals("Total JobHeaders Processed", 0, messages.Select(x => x.UsageProperties.Value<int>(UsageProperties.TotalJobHeadersProcessedDuringArchiving)).Sum());
				AssertEquals("Total Main Records Loaded", 4, messages.Select(x => x.UsageProperties.Value<int>(UsageProperties.TotalMainRecordLoaded)).Sum());
				AssertEquals("Total Missing Documents Generated", 0, messages.Select(x => x.UsageProperties.Value<int>(UsageProperties.TotalMissingDocumentsGeneratedDuringArchiving)).Sum());
				AssertEquals("Total Documents Deleted", 0, messages.Select(x => x.UsageProperties.Value<int>(UsageProperties.TotalDocumentsDeletedDuringArchiving)).Sum());
			});
		}

		public void TestGetArchiveStageDescriptors()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stageDescriptors = new STAArchiveSystemDescriptor().GetArchiveStageDescriptors(config).ToList();

			AssertEquals("All non-customs standalone archive stages should be in the list.", 2, stageDescriptors.Count);
		}

		public void TestGetArchiveStageDescriptors_ShouldIncludeDeclarations()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: true);
			var stageDescriptors = new STAArchiveSystemDescriptor().GetArchiveStageDescriptors(config).ToList();

			AssertEquals("All standalone customs archive stages should be in the list.", 13, stageDescriptors.Count);
		}

		[UseSnapshotProtection]
		public void TestGetArchiveStageDescriptors_LogIsCorrect()
		{
			var archiveSystem = TestHelpers.GetArchiveSystem(TestConfig.ArchiveSystemCodeToTest);
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());

			currentArchiveManager.Run(archiveSystem.Descriptor.Code, config, archiveLogger, archiveSchedule, new CancellationToken());

			CombineAssertions("Logs did not contain the correct messages", () =>
			{
				AssertCollectionContains("HVLV Booking Header Archive Log", $"Information|{ArchiveManagerConstants.Codes.STA}|Executing: Standalone HVLV Booking Header Archive", archiveLogger.ListOfMessages);
				AssertCollectionContains("HVLV Origin Load List Archive Log", $"Information|{ArchiveManagerConstants.Codes.STA}|Executing: Standalone HVLV Origin Load List Archive", archiveLogger.ListOfMessages);
			});
		}

		public void TestRunArchiveSystem_ArchivesHVLVBookingHeaderCorrectly()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			var returnPivotWithFormerFK = Factory.NewWithValidTestData<HVLVReturnPivot>();
			returnPivotWithFormerFK.HVP_HVC_Former = consignment.PK;
			var returnPivotWithReturnFK = Factory.NewWithValidTestData<HVLVReturnPivot>();
			returnPivotWithReturnFK.HVP_HVC_Return = consignment.PK;

			var entryNum = Factory.NewWithValidTestData<CusEntryNumber>();

			entryNum.CE_ParentID = consignment.PK;
			entryNum.CE_ParentTable = HVLVConsignmentSchema.Constants.TableName;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertType("Expected HVLV Booking Header not to be deleted.", typeof(HVLVBookingHeader), bookingHeader);
				AssertType("Expected HVLV Consignment not to be deleted.", typeof(HVLVConsignment), consignment);
				AssertType("Expected HVLV Return Pivot Former not to be deleted.", typeof(HVLVReturnPivot), returnPivotWithFormerFK);
				AssertType("Expected HVLV Return Pivot Return not to be deleted.", typeof(HVLVReturnPivot), returnPivotWithReturnFK);
				AssertType("Expected Custom Entry Number not to be deleted.", typeof(CusEntryNumber), entryNum);
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, schedule);

			var newFactory = new BusinessObjectFactory();
			bookingHeader = newFactory.Load<HVLVBookingHeader>(bookingHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			returnPivotWithFormerFK = newFactory.Load<HVLVReturnPivot>(returnPivotWithFormerFK.PK);
			returnPivotWithReturnFK = newFactory.Load<HVLVReturnPivot>(returnPivotWithReturnFK.PK);

			entryNum = newFactory.Load<CusEntryNumber>(entryNum.PK);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertNull("Expected HVLV Booking Header is deleted.", bookingHeader);
				AssertNull("Expected HVLV Consignment is deleted.", consignment);
				AssertNull("Expected HVLV Return Pivot Former is deleted.", returnPivotWithFormerFK);
				AssertNull("Expected HVLV Return Pivot Return is deleted.", returnPivotWithReturnFK);
				AssertNull("Expected Custom Entry Number is deleted.", entryNum);
			});
		}

		public void TestRunArchiveSystem_ArchivesHVLVOriginLoadListCorrectly()
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertType("Expected HVLV Origin Load List not to be deleted.", typeof(HVLVOriginLoadList), originLoadList);
				AssertType("Expected HVLV Outer Package not to be deleted.", typeof(HVLVOuterPackage), outerPackage);
			});

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, schedule);

			var newFactory = new BusinessObjectFactory();
			originLoadList = newFactory.Load<HVLVOriginLoadList>(originLoadList.PK);
			outerPackage = newFactory.Load<HVLVOuterPackage>(outerPackage.PK);

			CombineAssertions("Expected objects are deleted correctly", () =>
			{
				AssertNull("Expected HVLV Origin Load List is deleted", originLoadList);
				AssertNull("Expected HVLV Outer Package is deleted", outerPackage);
			});
		}

		public void TestSTAHVLVOriginLoadListFilter()
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_HVL_LoadList = originLoadList.PK;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var systemDescriptor = new STAArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new STAHVLVOriginLoadListArchiveStageDescriptor(), systemDescriptor);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			var newFactory = new BusinessObjectFactory();
			originLoadList = newFactory.Load<HVLVOriginLoadList>(originLoadList.PK);
			outerPackage = newFactory.Load<HVLVOuterPackage>(outerPackage.PK);
			item = newFactory.Load<HVLVItem>(item.PK);

			CombineAssertions("Expected objects are not deleted due to MainArchiveableFilter", () =>
			{
				AssertType("Expected HVLV Origin Load List not to be deleted.", typeof(HVLVOriginLoadList), originLoadList);
				AssertType("Expected HVLV Outer Package not to be deleted.", typeof(HVLVOuterPackage), outerPackage);
				AssertType("Expected HVLV Item not to be deleted.", typeof(HVLVItem), item);
			});
		}

		public void TestSTAHVLVOriginLoadListIsNotArchivedWhenGrandchildHVLVItemExists()
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_HVL_LoadList = originLoadList.PK;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVO_OuterPackage = outerPackage.PK;

			Factory.Save();

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var systemDescriptor = new STAArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new STAHVLVOriginLoadListArchiveStageDescriptor(), systemDescriptor);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			Assert("No error should have occurred", !archiveLogger.ListOfMessages.Any(l => l.Contains("Error")));

			var newFactory = new BusinessObjectFactory();
			originLoadList = newFactory.Load<HVLVOriginLoadList>(originLoadList.PK);
			outerPackage = newFactory.Load<HVLVOuterPackage>(outerPackage.PK);
			item = newFactory.Load<HVLVItem>(item.PK);

			CombineAssertions("Expected objects are not deleted", () =>
			{
				AssertType("Expected HVLV Origin Load List not to be deleted.", typeof(HVLVOriginLoadList), originLoadList);
				AssertType("Expected HVLV Outer Package not to be deleted.", typeof(HVLVOuterPackage), outerPackage);
				AssertType("Expected HVLV Item not to be deleted.", typeof(HVLVItem), item);
			});
		}

		public void TestSTAHVLVOriginLoadListIsNotArchivedWhenChildHVLVItemExists()
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var systemDescriptor = new STAArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new STAHVLVOriginLoadListArchiveStageDescriptor(), systemDescriptor);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			Assert("No error should have occurred", !archiveLogger.ListOfMessages.Any(l => l.Contains("Error")));

			var newFactory = new BusinessObjectFactory();
			originLoadList = newFactory.Load<HVLVOriginLoadList>(originLoadList.PK);
			item = newFactory.Load<HVLVItem>(item.PK);

			CombineAssertions("Expected objects are not deleted", () =>
			{
				AssertType("Expected HVLV Origin Load List not to be deleted.", typeof(HVLVOriginLoadList), originLoadList);
				AssertType("Expected HVLV Item not to be deleted.", typeof(HVLVItem), item);
			});

			item.HVI_HVL_LoadList = ZGuid.Empty;
			newFactory.Save();

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			newFactory = new BusinessObjectFactory();
			originLoadList = newFactory.Load<HVLVOriginLoadList>(originLoadList.PK);
			item = newFactory.Load<HVLVItem>(item.PK);

			CombineAssertions("Load list is now archived", () =>
			{
				AssertNull("Load list is now deleted", originLoadList);
				AssertNotNull("HVLV item is not deleted", item);
			});
		}

		public void TestSTAHVLVBookingHeaderArchiveItemsOfConsignments()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = Factory.NewWithValidTestData<HVLVItem>();

			item.HVI_HVC_Consignment = consignment.PK;
			consignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			Factory.Save();

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var systemDescriptor = new STAArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new STAHVLVBookingHeaderArchiveStageDescriptor(), systemDescriptor);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			var newFactory = new BusinessObjectFactory();
			bookingHeader = newFactory.Load<HVLVBookingHeader>(bookingHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			item = newFactory.Load<HVLVItem>(item.PK);

			CombineAssertions("Objects are deleted", () =>
			{
				AssertNull("Booking header should be deleted", bookingHeader);
				AssertNull("Consignment should be deleted", consignment);
				AssertNull("Item should be deleted", item);
			});
		}

		public void TestSTAHVLVBookingHeaderDoesNotArchiveWhenChildConsignmentHasConsignmentHeader()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			Factory.Save();

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();

			consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			Factory.Save();

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var systemDescriptor = new STAArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new STAHVLVBookingHeaderArchiveStageDescriptor(), systemDescriptor);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			var newFactory = new BusinessObjectFactory();
			bookingHeader = newFactory.Load<HVLVBookingHeader>(bookingHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);

			CombineAssertions("Nothing is deleted", () =>
			{
				AssertNotNull("Booking header should not be deleted", bookingHeader);
				AssertNotNull("Consignment should not be deleted", consignment);
				AssertNotNull("Consignment header should not be deleted", consignmentHeader);
			});

			consignment.HVC_HCH_Header = ZGuid.Empty;

			newFactory.Save();

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			newFactory = new BusinessObjectFactory();
			bookingHeader = newFactory.Load<HVLVBookingHeader>(bookingHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			consignmentHeader = newFactory.Load<HVLVConsignmentHeader>(consignmentHeader.PK);

			CombineAssertions("BookingHeader and consignment should now be deleted, but consignment header should remain", () =>
			{
				AssertNull("Booking header should be deleted", bookingHeader);
				AssertNull("Consignment should be deleted", consignment);
				AssertNotNull("Consignment header should not be deleted", consignmentHeader);
			});
		}

		public void TestHVLVBookingHeaderIsNotArchivedWhenCusUSLVConsignmentExistsOnConsignment()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var cusUSLVConsignment = Factory.NewWithValidTestData<CusUSLVConsignment>();

			consignment.HVC_HVH_BookingHeader = bookingHeader.PK;
			cusUSLVConsignment.ULB_HVC_Consignment = consignment.PK;

			Factory.Save();

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 10, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var systemDescriptor = new STAArchiveSystemDescriptor();
			var archiveStage = new ArchiveStage(new STAHVLVBookingHeaderArchiveStageDescriptor(), systemDescriptor);

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			var newFactory = new BusinessObjectFactory();
			bookingHeader = newFactory.Load<HVLVBookingHeader>(bookingHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			cusUSLVConsignment = newFactory.Load<CusUSLVConsignment>(cusUSLVConsignment.PK);

			CombineAssertions("Nothing is deleted", () =>
			{
				AssertNotNull("Booking header should not be deleted", bookingHeader);
				AssertNotNull("Consignment should not be deleted", consignment);
				AssertNotNull("USLV Consignment should not be deleted", cusUSLVConsignment);
			});

			cusUSLVConsignment.ULB_HVC_Consignment = Guid.Empty;

			newFactory.Save();

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, archiveLogger, schedule, new CancellationToken());

			newFactory = new BusinessObjectFactory();
			bookingHeader = newFactory.Load<HVLVBookingHeader>(bookingHeader.PK);
			consignment = newFactory.Load<HVLVConsignment>(consignment.PK);
			cusUSLVConsignment = newFactory.Load<CusUSLVConsignment>(cusUSLVConsignment.PK);

			CombineAssertions("Everything except cusUSLVConsignment is now deleted", () =>
			{
				AssertNull("Booking header should be deleted", bookingHeader);
				AssertNull("Consignment should be deleted", consignment);
				AssertNotNull("USLV Consignment should not be deleted", cusUSLVConsignment);
			});
		}

		public void TestGeneratedSummaryReport()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

			AssertEquals("Report for each stage should have been generated.", 13, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
			ArchiveManagerAssertions.AssertArchiveSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "StandaloneRecordsArchiveSystemReport_AsycudaManifestHeader_"), TestConfig.ArchiveSystemCodeToTest, mainArchiveTableName: AsycudaManifestHeaderSchema.Constants.TableName);
		}

		public void TestArchiveStagesSortByMainDateFilterColumn()
			=> IntegrationTestHelper.RunArchiveStagesSortByMainDateFilterColumn(TestConfig);

		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
			=> IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);
	}
}
