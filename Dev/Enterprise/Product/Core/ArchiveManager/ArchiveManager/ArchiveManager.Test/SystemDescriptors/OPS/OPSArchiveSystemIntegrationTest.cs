using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusOutturn = Enterprise.Customs.Business.CusOutturn;
using CusOutturnHeader = Enterprise.Customs.Business.CusOutturnHeader;
using CusUnderbond = Enterprise.Customs.AU.Declaration.Business.CusUnderbond;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.OPS
{
	public sealed class OPSArchiveSystemIntegrationTest : ArchiveSystemIntegrationTestWithDbSetup, IArchiveSystemDescriptorIntegrationTest
	{
		public OPSArchiveSystemIntegrationTest()
		{
			TestConfig = new TestConfiguration()
			{
				ArchiveSystemCodeToTest = ArchiveManagerConstants.Codes.OPS,
				ListOfStageNames = ["Operational Jobs Archive"],
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
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLoggingOfAMUsageData()
		{
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.NeedWorkingStatusJobHeader = true;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);

			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 60))
			{
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				archiveSchedule.IsArchiveRecordsOnOrBeforeDate = true;
				archiveSchedule.ShouldArchiveDeclaration = true;
				archiveSchedule.ArchiveRecordsOnOrBeforeDate = ZDateTime.UtcNow;
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
				archiveSystemStageName: "Operational Jobs Archive",
				includeCustomsJobs: true,
				batchSize: 60,
				totalRecordsDeleted: 142,
				jobHeadersProcessed: 5,
				mainRecordsLoaded: 6,
				totalMissingDocumentsGenerated: 49,
				documentsDeleted: 0);
		}

		List<string> CreateAndRunArchiveConfigurationForBatchSizeAndReturnListOfMessages(ArchiveScheduleTask archiveSchedule, bool shouldIncludeDeclarations)
		{
			var config = new ArchiveConfiguration(ZDateTime.Now, 5, ZDateTime.UtcNow, TestConfig.IsVerboseLog, shouldIncludeDeclarations);
			var currentArchiveManager = new Engine.ArchiveManager(new ArchiveSystemDescriptorLoader());
			currentArchiveManager.Run(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule, new CancellationToken());
			return archiveLogger.ListOfMessages;
		}

		[UseSnapshotProtection]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestArchiveWhenAllJobHeadersWithBatchSizeAndTimeOut()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.NeedWorkingStatusJobHeader = true;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SystemDataRegistry.Instance.BatchSizeControl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			SetupTestData.ForCommonCase(TestConfig);
			var listOfMessages = CreateAndRunArchiveConfigurationForBatchSizeAndReturnListOfMessages(archiveSchedule, shouldIncludeDeclarations: true);
			var countOfHeaders = GetCountOfHeaders(listOfMessages);
			Assert("Count of Headers must be greater than the archiving batch size", countOfHeaders > SystemDataRegistry.Instance.BatchSizeControl.Value);
		}

		[UseSnapshotProtection]
		public void TestArchiveWithArchivingJobChargeAndItsRelatedTables()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				TestConfig.NeedTableRecordCounts = true;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				SetupTestData.ForCommonCase(TestConfig);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

				ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
				TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

				ArchiveManagerAssertions.AssertTableRecordCounts(JobChargeSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobConsolCostSchema.Constants.TableName, 1, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobPaymentBasisSchema.Constants.TableName, 1, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			}
		}

		[TestDate(2022, 02, 22)]
		[UseSnapshotProtection]
		public void TestArchiveWhenAllJobHeadersCanBeArchivedAndPeriodIsClosed()
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
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
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

				ArchiveManagerAssertions.AssertArchiveSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "OperationalJobsArchiveSystemReport_"), TestConfig.ArchiveSystemCodeToTest, archiveSystemHasMultipleArchiveStageDescriptors: false, archiveSystemHasDateParameterSelection: true);
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		void AssertStorageMainAndRef()
		{
			var storageMainBaseline = TestConfig.TableRecordCountsBaseline[StorageMainSchema.Constants.TableName].RowCount;
			var storageMainAfterRun = TestConfig.TableRecordCountsAfterRun[StorageMainSchema.Constants.TableName].RowCount;

			Assert("StorageMain count after archiving", storageMainBaseline < storageMainAfterRun);

			var storageReferenceBaseline = TestConfig.TableRecordCountsBaseline[StorageReferenceSchema.Constants.TableName].RowCount;
			var storageReferenceAfterRun = TestConfig.TableRecordCountsAfterRun[StorageReferenceSchema.Constants.TableName].RowCount;

			Assert("StorageReference count after archiving", storageReferenceBaseline < storageReferenceAfterRun);
			var archiveFactory = new DocumentFactoryProvider().GetFactory(Factory);
			AssertReferenceKeys(archiveFactory, "CON", "C00001000",
				@"CON: C00001000 : 
CTG: C00001000/I : CON: C00001000
SHP: S00001000 : CON: C00001000 JOB: S00001000 ORD: S00001000 DEC: S00001000
SHP: S00001003 : CON: C00001000 JOB: S00001003 ORD: S00001003 DEC: S00001003
SHP: S00001006 : CON: C00001000 ORD: S00001006 DEC: S00001006
");

			AssertReferenceKeys(archiveFactory, "SHP", "S00001003",
		@"CTG: S00001003/I : SHP: S00001003
DEC: S00001003 : SHP: S00001003 JOB: S00001003 ORD: S00001003
ORD: P000002 : SHP: S00001003
SHP: S00001003 : CON: C00001000 JOB: S00001003 ORD: S00001003 DEC: S00001003
SHP: S00001004 : SHP: S00001003
SHP: S00001005 : SHP: S00001003
");

			AssertReferenceKeys(archiveFactory, "DEC", "S00001003",
		@"DEC: S00001003 : SHP: S00001003 JOB: S00001003 ORD: S00001003
MSG: 20091215-152618|APP|TYP|SUB : DEC: S00001003 MRT: RCV MAR: AUC MNO: 234 MUS: E INT: S00001003
MSG: 20091215-152619|APP|TYP|SUB : DEC: S00001003 MRT: RCV MAR: AUC MNO: 345 MUS: E INT: S00001003
SHP: S00001003 : CON: C00001000 JOB: S00001003 ORD: S00001003 DEC: S00001003
");
		}

		void AssertReferenceKeys(DocumentFactory archiveFactory, string searchReferenceType, string searchReferenceValue, string expectedSearchResult)
		{
			var query = new ZDBOnlyQuery(typeof(ArchiveStorageMain));
			var subQuery = new ZDBOnlySubQuery(typeof(StorageReference), StorageReferenceSchema.SR_SM);

			_ = subQuery.AddToFilter(StorageReferenceSchema.SR_Reference, SQLComparisonOperator.Equal, searchReferenceValue);
			_ = subQuery.AddToFilter(StorageReferenceSchema.SR_TYPE, searchReferenceType);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var archivedRecordsFound = archiveFactory.Load<ArchiveStorageMain>(query);
			Array.Sort(archivedRecordsFound, (x, y) => x.MainReference.CompareTo(y.MainReference));

			var searchResultBuilder = new StringBuilder();
			foreach (var archiveStorageMain in archivedRecordsFound)
			{
				_ = searchResultBuilder.AppendLine(archiveStorageMain.MainReference + " : " + archiveStorageMain.AdditionalReferences);
			}

			AssertEquals("Reference search result", expectedSearchResult, searchResultBuilder.ToString());
		}

		void AssertLog(IArchiveSet archiveSet)
		{
			var index = 0;
			AssertEquals("Information|OPS|Loaded next batch of 3 JobHeader", archiveLogger.ListOfMessages[index++]);

			var maxNumberForGenDocs = 59;

			for (index = 1; index < maxNumberForGenDocs; index++)
			{
				AssertContains("Information|OPS|Generating missing", archiveLogger.ListOfMessages[index]);
			}

			var mainArchiveItemKey = archiveSet.MainArchiveItemNK ?? archiveSet.MainArchiveItem.PK.ToString();
			var archiveSetRelatedRecordsCount = archiveSet.Count - 1;

			AssertEquals($"Log {index}: ", $"Information|OPS|Deleting JobHeader '{mainArchiveItemKey}' and {archiveSetRelatedRecordsCount} related records", archiveLogger.ListOfMessages[index++]);
			AssertContains($"Log {index}", "JobShipments Per Hour: ", archiveLogger.ListOfMessages[index]);
			AssertContains($"Log {index}", "JobHeaders Per Hour: ", archiveLogger.ListOfMessages[index++]);
			AssertContains($"Log {index}", "Information|OPS|Archived Data Dated Between Earliest Possible Date and ", archiveLogger.ListOfMessages[index++]);
			AssertEquals($"Log {index}", "Information|OPS|Generated Archive Report and stored on eDocs tab of the Archive Schedule", archiveLogger.ListOfMessages[index]);
		}

		[UseSnapshotProtection]
		public void TestLoggingOfGeneratedMissingDocuments()
		{
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			SetupTestData.ForCommonCase(TestConfig);
			var messages = CreateAndRunArchiveConfigurationForBatchSizeAndReturnListOfMessages(archiveSchedule, shouldIncludeDeclarations: true);
			AssertEquals("Should say something about generating missing documents", expected: true, messages.Any(x => x.Contains("Time taken to generate 40 missing document(s)")));
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
				AssertCollectionContains("Configuration Parameter Log", $"Information|{ArchiveManagerConstants.Codes.OPS}|Configuration Parameters:", archiveLogger.ListOfMessages);
				AssertCollectionContains("Date Parameter Log", $"Information|{ArchiveManagerConstants.Codes.OPS}|Date Parameter: Job Close Date", archiveLogger.ListOfMessages);
				AssertCollectionContains("On or Before Log", $"Information|{ArchiveManagerConstants.Codes.OPS}|Archiving Records on or Before: 2-Feb-2020", archiveLogger.ListOfMessages);
				AssertCollectionContains("Max Run Duration Log", $"Information|{ArchiveManagerConstants.Codes.OPS}|Max Run Duration: 5 minutes", archiveLogger.ListOfMessages);
				AssertCollectionContains("Verbose Logging Log", $"Information|{ArchiveManagerConstants.Codes.OPS}|Verbose Logging: No", archiveLogger.ListOfMessages);
				AssertCollectionContains("Incl. Customs Log", $"Information|{ArchiveManagerConstants.Codes.OPS}|Incl. Customs: Yes", archiveLogger.ListOfMessages);
			});
		}

		[UseSnapshotProtection]
		public void TestDoesNotLoadSetWhenShouldNotIncludeDeclarations()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				TestConfig.NeedTableRecordCounts = false;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsVerboseLog = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				SetupTestData.ForCommonCase(TestConfig);

				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: false);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
				AssertNull("ArchiveSet should be null because shouldIncludeDeclarations=false, and there is a Job Declaration in the set.", set);
			}
		}

		[UseSnapshotProtection]
		public void TestJobDeclarationAttatchedToJobHeaderViaJobConsolAndNotIncludingCustomsDoesNotArchive()
		{
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

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("Set containing a JobDeclaration should be skipped as ShouldIncludeDeclarations=false", () =>
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
		public void TestArchivingCusCAeMHMaster_WhenshouldIncludeDeclarationsIsTrue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var caCustomsTestDataCreator = ObjectFactory.Get<ICACustomsTestDataCreator>();
				var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
				forwardingTestDataCreator.CreateConsolData(0, 0, out var consol1PK, isCancelled: false, out _, out _);

				var jobHeaderWithConsol = Factory.NewJobForTesting<JobHeader>();
				jobHeaderWithConsol.JH_GB = Env.CurrentBranch.PK;
				jobHeaderWithConsol.JH_GE = Env.CurrentDepartment.PK;
				jobHeaderWithConsol.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				jobHeaderWithConsol.JH_ParentID = consol1PK;
				jobHeaderWithConsol.JH_Status = JobHeaderStatus.Closed.Code;

				caCustomsTestDataCreator.CreateAttachedCusCAeMHMaster(consol1PK);

				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				Factory.Save();
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
				_ = set.Load(archiveLogger);

				AssertEquals("CusCAeMHMaster should be in the ArchiveSet because isArchiveDeclarations=true.", 1, set.GetArchiveItems().Where(i => i.PKColumn.TableName == "CusCAeMHMaster" && i.ParentPKColumn == JobConsolSchema.PK).Count());
			}
		}

		[UseSnapshotProtection]
		public void TestArchiveWithArchivingJobDeclarationAndCusEntryInstructionAndAllRelatedTablesForTW()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				TestConfig.NeedTableRecordCounts = true;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsVerboseLog = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				SetupTestData.ForCommonCase(TestConfig);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

				ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
				TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

				ArchiveManagerAssertions.AssertTableRecordCounts(CusTWControllingMessageHeaderSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(CusTWProductLabelRangeSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			}
		}

		[UseSnapshotProtection]
		public void TestArchiveWithRatingAndAllRelatedTables()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = true;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

			ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
			TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

			ArchiveManagerAssertions.AssertTableRecordCounts(RatingHeaderSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(RateOneOffShipmentSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(RateOneOffContainersSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
		}

		[UseSnapshotProtection]
		public void TestRatingHeaderExcludesRateAttachment_NoJobShipmentAttached()
		{
			var factory = new BusinessObjectFactory();

			SetupTestData.ForExcludedRatingData(false);

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals(1, factory.GetDatabaseCount(typeof(RatingHeader)));
			AssertEquals(1, factory.GetDatabaseCount(typeof(RateAttachment)));
		}

		[UseSnapshotProtection]
		public void TestRatingHeaderExcludesRateAttachment_JobShipmentAttached()
		{
			var factory = new BusinessObjectFactory();
			SetupTestData.ForExcludedRatingData(true);

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals(1, factory.GetDatabaseCount(typeof(RatingHeader)));
			AssertEquals(1, factory.GetDatabaseCount(typeof(RateAttachment)));
		}

		[UseSnapshotProtection]
		public void TestArchiveWithArchivingJobDeclarationAndRelatedTablesForBR()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				TestConfig.NeedTableRecordCounts = true;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsVerboseLog = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				SetupTestData.ForCommonCase(TestConfig);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

				ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
				TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();
			}
		}

		[UseSnapshotProtection]
		public void TestEcommerceDataIsCorrectlyArchived_WhenDeclarationsAreIncluded()
		{
			var factory = new BusinessObjectFactory();

			var testDataCreator = ObjectFactory.Get<IEcommerceTestDataCreator>();
			var testBizOs = testDataCreator.CreateEcommerceTestData(factory);
			var customsTestBizOs = testDataCreator.CreateEcommerceWithCustomsTestData(factory);

			ArchiveManagerAssertions.AssertEcommerceTestDataExists(factory, testBizOs);
			ArchiveManagerAssertions.AssertEcommerceCustomsTestDataExists(factory, customsTestBizOs);

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: true);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

			foreach (var archiveableItem in testBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, archiveableItem);
			}

			foreach (var bizO in customsTestBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, bizO);
			}
		}

		[UseSnapshotProtection]
		public void TestEcommerceDataIsCorrectlyArchived_WhenDeclarationsAreNotIncluded()
		{
			var factory = new BusinessObjectFactory();

			var testDataCreator = ObjectFactory.Get<IEcommerceTestDataCreator>();
			var testBizOs = testDataCreator.CreateEcommerceTestData(factory);
			var customsTestBizOs = testDataCreator.CreateEcommerceWithCustomsTestData(factory);

			ArchiveManagerAssertions.AssertEcommerceTestDataExists(factory, testBizOs);
			ArchiveManagerAssertions.AssertEcommerceCustomsTestDataExists(factory, customsTestBizOs);

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));

			foreach (var bizO in testBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, bizO);
			}

			foreach (var bizO in customsTestBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasNotArchived(factory, bizO);
			}
		}

		[UseSnapshotProtection]
		public void TestHVLVScanningSummaryIsExcludedFromArchiving()
		{
			var factory = new BusinessObjectFactory();

			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var shipmentHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			shipmentHeader.JH_Status = JobHeaderStatus.Closed.Code;
			shipmentHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			shipmentHeader.JH_JobNum = "T0001";
			shipmentHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipmentHeader.JH_ParentID = shipment.PK;

			factory.Save();

			var createHvlvScanningSummaryCommand = @"
				INSERT INTO dbo.HVLVScanningSummary (HSR_PK, HSR_ScanningSession, HSR_JS_Shipment, HSR_GS_NKUser, 
				HSR_SystemCreateTimeUtc, HSR_SystemCreateUser, HSR_SystemLastEditTimeUtc, HSR_SystemLastEditUser) 
				VALUES (NEWID(), NEWID(), @js_pk, 'KGG', GETUTCDATE(), 'KGG',  GETUTCDATE(), 'KGG')
			";

			using (var command = Db.Connection.Command(createHvlvScanningSummaryCommand))
			{
				command.AddParameter("@js_pk", SqlDbType.UniqueIdentifier, shipment.PK.ToGuid());

				_ = command.ExecuteNonQuery();
			}

			CombineAssertions("Precondition: Expected the following to exist.", () =>
			{
				AssertEquals("JobHeader", 1, factory.GetDatabaseCount(typeof(JobHeader), new ZQuery(JobHeaderSchema.PK, shipmentHeader.PK)));
				AssertEquals("ForwardingShipment", 1, factory.GetDatabaseCount(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.PK, shipment.PK)));
			});

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, logger, schedule);

			CombineAssertions("Postcondition: Expected the following to still exist after archiving.", () =>
			{
				AssertEquals("JobHeader", 1, factory.GetDatabaseCount(typeof(JobHeader), new ZQuery(JobHeaderSchema.PK, shipmentHeader.PK)));
				AssertEquals("ForwardingShipment", 1, factory.GetDatabaseCount(typeof(ForwardingShipment), new ZQuery(JobShipmentSchema.PK, shipment.PK)));
			});

			Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
		}

		[UseSnapshotProtection]
		public void TestArchiveWithArchivingJobDeclarationAndCusEntryInstructionAndAllRelatedTablesForCA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				TestConfig.NeedTableRecordCounts = true;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsVerboseLog = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				SetupTestData.ForCommonCase(TestConfig);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

				ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
				TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

				ArchiveManagerAssertions.AssertTableRecordCounts(CusRulingConfigSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobCADeclarationSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			}
		}

		[UseSnapshotProtection]
		public void TestArchiveWithArchivingJobDeclarationAndRelatedTablesForEU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				TestConfig.NeedTableRecordCounts = true;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsVerboseLog = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				SetupTestData.ForCommonCase(TestConfig);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

				ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
				TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

				ArchiveManagerAssertions.AssertTableRecordCounts(JobEUDeclarationSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			}
		}

		[UseSnapshotProtection]
		public void TestArchiveWithArchivingJobDeclarationAndCusEntryInstructionAndAllRelatedTablesForUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				TestConfig.NeedTableRecordCounts = true;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = true;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsVerboseLog = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				SetupTestData.ForCommonCase(TestConfig);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

				ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
				TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

				ArchiveManagerAssertions.AssertTableRecordCounts(JobUSDeclarationSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
				ArchiveManagerAssertions.AssertTableRecordCounts(JobUSComInvoiceLineSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			}
		}

		public void TestArchiveWithArchivingJobDeclarationAndCusEntryInstructionAndAllRelatedTables()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = true;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

			ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
			TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

			ArchiveManagerAssertions.AssertTableRecordCounts(JobDeclarationSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(CusHouseContPackInvoiceLinePivotSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(CusEntryInstructionSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(JobComInvoiceLineSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(CusEntryHeaderSchema.Constants.TableName, 6, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(CusEntrySnapshotSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(CusContainerEntryHeaderPivotSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(CusContainerSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(JobComInvoiceLineTaxSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(CusDecHouseContainerPackSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
		}

		public void TestArchiveWhenNOTAllJobHeadersCanBeArchivedAndPeriodIsClosed()
		{
			TestConfig.NeedTableRecordCounts = false;
			TestConfig.AllJobHeadersNeedToBeClosed = false;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			_ = set.Load(archiveLogger);
			AssertEquals("ArchiveSet should not load any records", 0, set.Count);
		}

		public void TestArchiveWhenAllJobHeadersCanBeArchivedAndPeriodIsOpen()
		{
			TestConfig.NeedTableRecordCounts = false;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = false;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: false);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			AssertNull("When period is open, archive should not find a set to return", set);
		}

		public void TestArchiveWhenAllJobHeadersCanBeArchivedAndPeriodIsClosedButHasOpenHotCheques()
		{
			TestConfig.NeedTableRecordCounts = false;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = false;
			TestConfig.IsHotChequeLinkedToAH = false;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: false);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			AssertNull("When hotcheque is open, archive should not find a set to return", set);
		}

		public void TestArchiveWhenAllJobHeadersCanBeArchivedAndPeriodIsClosedButHasHotChequesNotLinkedToAH()
		{
			TestConfig.NeedTableRecordCounts = false;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = false;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: false);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			AssertNull("When hotcheque is open, archive should not find a set to return", set);
		}

		public void TestArchivingStandAloneDeclarations()
		{
			var accountingTestDataCreator = ObjectFactory.Get<IAccountingTestDataCreator>();
			accountingTestDataCreator.CreatePeriods(200701, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 31), isClosed: true);

			var customsTestDataCreator = ObjectFactory.Get<ICustomsTestDataCreator>();
			var declarationPK = customsTestDataCreator.CreateStandAloneDeclarationData();

			var jobHeaderStandAloneDec = Factory.NewJobForTesting<JobHeader>();
			jobHeaderStandAloneDec.JH_JobNum = "8888";
			jobHeaderStandAloneDec.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			jobHeaderStandAloneDec.JH_ParentID = declarationPK;
			jobHeaderStandAloneDec.JH_A_JCL = new ZDateTime(2007, 1, 21);
			Factory.Save();

			var postDate = new ZDateTime(2007, 1, 14);
			accountingTestDataCreator.CreateAccountingData(jobHeaderStandAloneDec.PK, postDate, isHotChequeCancelled: true, isHotChequeLinkedToAH: true);
			jobHeaderStandAloneDec.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);

			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: false);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			AssertNull("set should be null as it's for a Declaration", set);

			stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			AssertNotNull("set now include Declaration", set);
		}

		public void TestArchivingClientSpecificTables()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.UPE))
			{
				TestConfig.NeedTableRecordCounts = false;
				TestConfig.AllJobHeadersNeedToBeClosed = true;
				TestConfig.NeedJobHeaderConsol = true;
				TestConfig.NeedShipmentDeclaration = false;
				TestConfig.NeedWorkingStatusJobHeader = false;
				TestConfig.IsVerboseLog = false;
				TestConfig.IsPeriodClosed = true;
				TestConfig.IsHotChequeCancelled = true;
				TestConfig.IsHotChequeLinkedToAH = true;
				SetupTestData.ForCommonCase(TestConfig);
				var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
				var clientSpecificTestDataCreator = ClientHookLoader.Instance.ClientHook.GetClientSpecificArchiveManagerHelper();
				clientSpecificTestDataCreator.CreateClientSpecificConfiguration();
				clientSpecificTestDataCreator.AssertNumberOfRecords("Beginning", Assert);
				foreach (var shipmentPK in TestConfig.ShipmentPKs)
				{
					clientSpecificTestDataCreator.CreateClientSpecificData(shipmentPK);
				}

				Factory.Save();

				clientSpecificTestDataCreator.AssertNumberOfRecords("TestRecords", Assert);
				var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
				var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
				var loadResult = set.Load(archiveLogger);
				loadResult = stage.ArchiveToImages(set);
				stage.EndRun();
				Assert("There were errors encountered.", loadResult.ErrorsEncountered.Count == 0);
				clientSpecificTestDataCreator.AssertNumberOfRecords("AfterPurge", Assert);
				clientSpecificTestDataCreator.DropClientSpecificConfiguration();
			}
		}

		[UseSnapshotProtection]
		public void TestArchivingJobCartage()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = false;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.NeedJobHeaderCartage = true;
			TestConfig.IsVerboseLog = true;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
			TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

			AssertEquals(0, TestConfig.TableRecordCountsBaseline[JobCartageSchema.Constants.TableName].RowCount);
			AssertEquals(4, TestConfig.TableRecordCountsWithTestRecords[JobCartageSchema.Constants.TableName].RowCount);
			AssertEquals(0, TestConfig.TableRecordCountsAfterRun[JobCartageSchema.Constants.TableName].RowCount);
		}

		[UseSnapshotProtection]
		public void TestArchivingJobPackLinesAndRelatedTables()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = false;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.NeedJobHeaderCartage = false;
			TestConfig.IsVerboseLog = true;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

			ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
			TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

			ArchiveManagerAssertions.AssertTableRecordCounts(JobPackLineHarmonisedCodeSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(JobPackLinePackageSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
			ArchiveManagerAssertions.AssertTableRecordCounts(JobPackLinePortMessagingSchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
		}

		[UseSnapshotProtection]
		public void TestArchivingJobShipmentGatewayAndRelatedTables()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = false;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedJobHeaderCartage = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

			ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
			TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

			ArchiveManagerAssertions.AssertTableRecordCounts(JobShipmentGatewaySchema.Constants.TableName, 3, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
		}

		public void TestArchivingAttachedSeaCargo()
			=> RunSeaAndAirTest(true);

		public void TestArchivingAttachedAirCargo()
			=> RunSeaAndAirTest(false);

		public void TestArchivingSeaCargoWithNoMessages()
		{
			TestConfig.NeedTableRecordCounts = false;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = false;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: false);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ArchiveMainItemQueue");

			AssertNotNull("set1 should load the consol jobheader, as the consol has no ocean bill", set);
			var customsTestDataCreator = ObjectFactory.Get<ICustomsTestDataCreator>();
			customsTestDataCreator.CreateOceanBillForConsol(TestConfig.Consol1PK);
			stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: false);
			set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ArchiveMainItemQueue");

			AssertNull("set1 should NOT load the consol jobheader, as the consol has ocean bill and not including customs", set);
			stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			AssertNotNull("set1 should load the consol jobheader, even though the consol has ocean bill", set);
			_ = set.Load(archiveLogger);
			Assert("Something should be loaded as we now allow Customs Jobs", set.Count > 0);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestArchiveWhenJobHeaderStatusForAllCompaniesNotClosed()
		{
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.IsVerboseLog = true;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SystemDataRegistry.Instance.BatchSizeControl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			SetupTestData.ForSkippedJobHeader(TestConfig);
			var listOfMessages = CreateAndRunArchiveConfigurationForBatchSizeAndReturnListOfMessages(archiveSchedule, shouldIncludeDeclarations: true);

			Assert(listOfMessages.Exists(log => log.Contains("all login Companies is closed [This item has been found under company code : SIN]")));
		}

		void RunSeaAndAirTest(bool isSeaCargo)
		{
			TestConfig.NeedTableRecordCounts = false;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var customsTestDataCreator = ObjectFactory.Get<ICustomsTestDataCreator>();
			if (isSeaCargo)
			{
				customsTestDataCreator.CreateAttachedSeaCargo(TestConfig.ShipmentPKs[0]);
			}
			else
			{
				customsTestDataCreator.CreateAttachedAirCargo(TestConfig.ShipmentPKs[0]);
			}

			Factory.Save();

			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: false);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			AssertNotNull("set1 should load the consol jobheader, even tho the shipment has customs job", set);
			_ = set.Load(archiveLogger);
			AssertEquals("Nothing should be loaded as shipment is attached to customs job", 0, set.Count);

			AssertEquals("2 items should be loaded into the queue", 2, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());
			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ArchiveMainItemQueue");

			stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			AssertNotNull("set1 should load the consol jobheader, even tho the shipment has customs job", set);
			_ = set.Load(archiveLogger);
			Assert("Something should be loaded as we now allow Customs Jobs", set.Count > 0);
		}

		public void TestArchiveToImagesWithJobHeaderCanBeArchived()
		{
			TestConfig.NeedTableRecordCounts = false;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = false;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			SetupTestData.ForCommonCase(TestConfig);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();
			var loadResult = set.Load(archiveLogger);
			var archiveResult = stage.ArchiveToImages(set);
			stage.EndRun();
			AssertEquals(0, loadResult.ErrorsEncountered.Count);
			AssertEquals(0, archiveResult.ErrorsEncountered.Count);
		}

		[UseSnapshotProtection]
		public void TestArchiveJobConsolWhenThereIsAContainerPenaltyOnJobContainer()
		{
			TestConfig.NeedTableRecordCounts = true;
			TestConfig.AllJobHeadersNeedToBeClosed = true;
			TestConfig.NeedJobHeaderConsol = true;
			TestConfig.NeedShipmentDeclaration = true;
			TestConfig.NeedWorkingStatusJobHeader = false;
			TestConfig.IsVerboseLog = false;
			TestConfig.IsPeriodClosed = true;
			TestConfig.IsHotChequeCancelled = true;
			TestConfig.IsHotChequeLinkedToAH = true;
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			SetupTestData.ForCommonCase(TestConfig);

			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

			ArchiveManagerAssertions.AssertArchiveSetDetails(stage, set, archiveLogger, TestConfig.TablesToIgnore, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords);
			TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

			ArchiveManagerAssertions.AssertTableRecordCounts(JobContainerPenaltySchema.Constants.TableName, 4, TestConfig.TableRecordCountsBaseline, TestConfig.TableRecordCountsWithTestRecords, TestConfig.TableRecordCountsAfterRun);
		}

		int GetCountOfHeaders(List<string> listOfMessages)
			=> listOfMessages.Count(m => m.Contains("Skipped JobHeader with Code") || m.Contains("Loaded JobHeader"));

		public void TestGeneratedSummaryReport()
		{
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			config.SetIsFilteringByJobOpenDate(true);
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, config, archiveLogger, archiveSchedule);

			Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			AssertEquals("Report for each stage should have been generated.", 1, TestHelpers.GetAllReportsFromSchedule(archiveSchedule).Count());
			ArchiveManagerAssertions.AssertArchiveSummaryReport(TestHelpers.GetArchiveReportFromSchedule(archiveSchedule, "OperationalJobsArchiveSystemReport_"), TestConfig.ArchiveSystemCodeToTest, archiveSystemHasMultipleArchiveStageDescriptors: false, archiveSystemHasDateParameterSelection: true);
		}

		public void TestEDIMessageGetsArchivedButOtherEDIMessagesStillExistOnEDIInterchange()
		{
			TestConfig.TableRecordCountsBaseline = TableRecordCountsHelper.GetTableRecordCounts();

			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			var shipmentPK = testDataCreator.CreateShipmentWithMessagesData();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;

			Factory.Save();

			var interchangeWeJustCreated = Factory.Load<EDIInterchange>(new ZQuery()).FirstOrDefault();
			var newMessageUnrelatedToJobHeader = Factory.NewWithValidTestData<EDIMessage>();
			newMessageUnrelatedToJobHeader.EM_EI = interchangeWeJustCreated.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			TestConfig.TableRecordCountsAfterRun = TableRecordCountsHelper.GetTableRecordCounts();

			Assert("Error found in logs", !archiveLogger.ListOfMessages.Any(m => m.Contains("Error")));
			AssertEquals("EDIMessage still has active children and should not be archived",
				1, TestConfig.TableRecordCountsAfterRun[EDIInterchangeSchema.Constants.TableName].RowCount);
		}

		public void TestArchiveSetContainsAsycudaBill_ButArchiveScheduleNotIncludeCustoms()
		{
			var jobShipment = Factory.New<ForwardingShipment>();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "Job123";
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = jobShipment.PK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.UtcNow;

			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_JS_Shipment = jobShipment.PK;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions(() =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error")));
			});
		}

		public void TestCusDecHouseBillIsAttachedToJobShipmentOnJobHeaderAndNotIncludingCustoms()
		{
			var jobShipment = Factory.New<ForwardingShipment>();
			var shipmentPK = jobShipment.PK;

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ClusterKey = 1;

			var bill = declaration.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.CU_JS = shipmentPK;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "Job123";
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.UtcNow;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions(() =>
			{
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("The DELETE statement conflicted with the REFERENCE constraint")));
				AssertNull(archiveLogger.ListOfMessages.Find(log => log.Contains("Error")));
			});
		}

		public void TestArchiveJobShipmentWithOutturnAndOutturnHeader()
		{
			var jobShipment = Factory.New<ForwardingShipment>();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "Job123";
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = jobShipment.PK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.UtcNow;

			const int numOfOutturnsToCreate = 3;

			var cusOutturnHeader = Factory.New<CusOutturnHeader>();
			cusOutturnHeader.C6_VoyageNum = "1";

			for (var i = 0; i < numOfOutturnsToCreate; i++)
			{
				var outturn = Factory.NewWithValidTestData<CusOutturn>();
				outturn.C5_ParentID = jobShipment.PK;
				outturn.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				outturn.C5_C6 = cusOutturnHeader.PK;

				var underbond = Factory.NewWithValidTestData<CusUnderbond>();
				underbond.C4_C6 = cusOutturnHeader.PK;

				var otherOutturn = Factory.NewWithValidTestData<CusOutturn>();
				otherOutturn.C5_C4_Underbond = underbond.PK;
			}

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: true);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			var newFactory = new BusinessObjectFactory();

			CombineAssertions("All records should have been deleted", () =>
			{
				AssertNull(newFactory.LoadTop1<ForwardingShipment>(new ZQuery()));
				AssertNull(newFactory.LoadTop1<JobHeader>(new ZQuery()));
				AssertNull(newFactory.LoadTop1<CusOutturnHeader>(new ZQuery()));
				AssertNull(newFactory.LoadTop1<CusOutturn>(new ZQuery()));
				AssertNull(newFactory.LoadTop1<CusUnderbond>(new ZQuery()));
			});
		}

		public void TestRunArchiveWhenJobShipmentHasOutturnButNotIncludingDeclarations()
		{
			var jobShipment = Factory.New<ForwardingShipment>();

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "Job123";
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = jobShipment.PK;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.UtcNow;

			var outturn = Factory.New<CusOutturn>();
			outturn.C5_ParentID = jobShipment.PK;
			outturn.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			var configuration = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			var newFactory = new BusinessObjectFactory();

			CombineAssertions("Nothing should have been deleted", () =>
			{
				AssertNotNull(newFactory.LoadTop1<ForwardingShipment>(new ZQuery()));
				AssertNotNull(newFactory.LoadTop1<JobHeader>(new ZQuery()));
				AssertNotNull(newFactory.LoadTop1<CusOutturn>(new ZQuery()));
			});
		}

		[DeveloperOnlyTest]
		[SnailTest]
		[UseSnapshotProtection]
		public void TestPerformanceOfLoadArchiveSetForOPS()
		{
			var numberOfChildrenToCreate = 10000;

			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			testDataCreator.CreateShipmentData(out var shipmentPK);

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipmentPK;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;

			for (var i = 0; i < numberOfChildrenToCreate; i++)
			{
				var dummyChildProcessTask = Factory.NewWithValidTestData<ProcessTask>();
				dummyChildProcessTask.P9_ParentID = shipmentPK;
				dummyChildProcessTask.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}

			Factory.Save();

			var stopwatch = new Stopwatch();
			var archiveSchedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			var stage = CreateAndRunArchiveConfiguration(archiveSchedule, shouldIncludeDeclarations: true);
			var set = stage.GetNextArchiveSet(null, archiveSchedule, stage).FirstOrDefault();

			stopwatch.Start();
			_ = set.Load(archiveLogger);
			stopwatch.Stop();

			CombineAssertions(() =>
			{
				AssertLessThan("Time Taken to Load Archive Set: ", stopwatch.ElapsedMilliseconds, 19659);
				AssertEquals("Archive Set should have been loaded.", 1, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());
			});
		}

		[UseSnapshotProtection]
		public void TestWatermarkUpdatesToJobOpenDate_WhenJobOpenDateIsSelectedFromDropDown()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			schedule.IsArchiveRecordsOnOrBeforeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = new DateTime(2017, 1, 18);
			schedule.ArchiveRecordsOnOrBeforeType = "Y";
			schedule.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
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
				Assert("Archiving Records on or Before: 18-Jan-17", archiveLogger.ListOfMessages.Exists(log => log.Contains("Archiving Records on or Before: 18-Jan-2017")));
				Assert("Date Parameter is Job Open Date", archiveLogger.ListOfMessages.Exists(log => log.Contains("Date Parameter: Job Open Date")));
				Assert("Watermark was updated to '18-Jan-17 00:00:00'", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was updated to '18-Jan-17 00:00:00'")));
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		[UseSnapshotProtection]
		public void TestWatermarkUpdatesToJobCloseDate_WhenJobCloseDateIsSelectedFromDropDown()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			schedule.IsArchiveRecordsOnOrBeforeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = new DateTime(2017, 1, 18);
			schedule.ArchiveRecordsOnOrBeforeType = "Y";
			schedule.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
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
				Assert("Archiving Records on or Before: 18-Jan-17", archiveLogger.ListOfMessages.Exists(log => log.Contains("Archiving Records on or Before: 18-Jan-2017")));
				Assert("Date Parameter is Job Close Date", archiveLogger.ListOfMessages.Exists(log => log.Contains("Date Parameter: Job Close Date")));
				Assert("Watermark was updated to '18-Jan-17 00:00:00'", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was updated to '18-Jan-17 00:00:00'")));
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		[UseSnapshotProtection]
		public void TestOPSArchivesRecordsCorrectly_WhenJobOpenDateIsSelectedFromDropDown()
		{
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, ArchiveManagerConstants.Codes.OPS);
			schedule.IsArchiveRecordsOnOrBeforeDate = true;
			schedule.ArchiveRecordsOnOrBeforeDate = new DateTime(2017, 1, 18);
			schedule.ArchiveRecordsOnOrBeforeType = "Y";
			schedule.S5_ScheduleType = ArchiveManagerConstants.Codes.OPS;
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
				Assert("Archiving Records on or Before: 18-Jan-17", archiveLogger.ListOfMessages.Exists(log => log.Contains("Archiving Records on or Before: 18-Jan-2017")));
				Assert("Date Parameter is Job Open Date", archiveLogger.ListOfMessages.Exists(log => log.Contains("Date Parameter: Job Open Date")));
				Assert("Watermark was reset to '01-Jan-00 00:00:00'", archiveLogger.ListOfMessages.Exists(log => log.Contains("Watermark was reset to '01-Jan-00 00:00:00'")));
				Assert("Logs shouldn't contain any errors", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
			});
		}

		[UseSnapshotProtection]
		public void TestJobConsolSelfRelationshipDoesNotCauseReferenceConstraintError()
		{
			var factory = new BusinessObjectFactory();
			var jobConsol = factory.New<ForwardingConsol>();

			var childJobConsol = factory.New<ForwardingConsol>();
			childJobConsol.JK_JK_MasterConsol = jobConsol.PK;

			var jobHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			jobHeader.JH_JobNum = "C0001";
			jobHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobHeader.JH_ParentID = jobConsol.PK;

			var jobConsolAWBSpecialHandling = jobConsol.AWBSpecialHandlingItems.AddNew();
			jobConsolAWBSpecialHandling.JKH_JK_Consol = childJobConsol.PK;
			jobConsolAWBSpecialHandling.JKH_Code = "ABC";

			factory.Save();

			CombineAssertions("Precondition: Test data was successfully created", () =>
			{
				AssertEquals("Job Consol (parent) should exist", 1, factory.GetDatabaseCount(jobConsol.GetType(), new ZQuery(jobConsol.PKSchemaColumn, jobConsol.PK)));
				AssertEquals("Job Header should exist", 1, factory.GetDatabaseCount(jobHeader.GetType(), new ZQuery(jobHeader.PKSchemaColumn, jobHeader.PK)));
				AssertEquals("Job Consol (child) should exist", 1, factory.GetDatabaseCount(childJobConsol.GetType(), new ZQuery(childJobConsol.PKSchemaColumn, childJobConsol.PK)));
				AssertEquals("Job Consol (child) should exist", 1, factory.GetDatabaseCount(jobConsolAWBSpecialHandling.GetType(), new ZQuery(jobConsolAWBSpecialHandling.PKSchemaColumn, jobConsolAWBSpecialHandling.PK)));
			});

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: true);
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);
			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, archiveLogger, schedule);

			CombineAssertions("Postcondition: Archivable data was successfully archived", () =>
			{
				Assert("Logs shouldn't contain any errors.", !archiveLogger.ListOfMessages.Exists(log => log.Contains("Error")));
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, jobConsol);
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, jobHeader);
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, childJobConsol);
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, jobConsolAWBSpecialHandling);
			});
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

		[UseSnapshotProtection]
		public void TestForwardingDataIsCorrectlyArchived()
		{
			var factory = new BusinessObjectFactory();

			var testDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			var archiveableBizOs = testDataCreator.CreateArchiveableForwardingData(factory);

			ArchiveManagerAssertions.AssertForwardingTestDataExists(factory, archiveableBizOs);

			var configuration = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: true, shouldIncludeDeclarations: true);
			var logger = new TestArchiveLogger();
			var schedule = TestHelpers.GetArchiveScheduleTask(Factory, TestConfig.ArchiveSystemCodeToTest);

			TestHelpers.RunArchiveSystem(TestConfig.ArchiveSystemCodeToTest, configuration, logger, schedule);

			Assert("Logs shouldn't contain any errors.", !logger.ListOfMessages.Exists(log => log.Contains("Error")));

			foreach (var archiveableItem in archiveableBizOs)
			{
				ArchiveManagerAssertions.AssertBusinessObjectWasArchived(factory, archiveableItem);
			}
		}
		
		public void TestArchiveLoggerContainsCorrectStageNames_InOrder()
			=> IntegrationTestHelper.RunArchiveLoggerContainsCorrectStageNames_InOrder(TestConfig, Factory, archiveLogger);
	}
}
