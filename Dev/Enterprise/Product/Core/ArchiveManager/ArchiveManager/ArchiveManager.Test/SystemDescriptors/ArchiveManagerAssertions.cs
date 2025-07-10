using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Billing.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.eManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors
{
	public static class ArchiveManagerAssertions
	{
		public static void AssertArchiveSummaryReport(IeDoc archiveReport, string archiveCode, bool archiveSystemHasMultipleArchiveStageDescriptors = true, string mainArchiveTableName = JobHeaderSchema.Constants.TableName, bool archiveSystemHasDateParameterSelection = false)
		{
			using var stream = archiveReport.GetImageDataReader();
			using var excel = new ExcelInterface();
			excel.LoadExcelFile(stream);
			var rowCount = 0;
			var worksheet = excel.WorkSheets[0];
			var stageDescriptorConstant = typeof(ArchiveManagerConstants.Names).GetField(archiveCode).GetRawConstantValue();

			Assertion.AssertEquals(Core.Constants.FileFormats.XLSX, excel.GetExtensionForExcelFromFile());

			Assertion.AssertEquals($"{stageDescriptorConstant} Summary Report", worksheet[rowCount, 1].ToString());

			Assertion.AssertEquals("Database Server", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Database Name", worksheet[++rowCount, 1].ToString());
			if (archiveSystemHasMultipleArchiveStageDescriptors)
			{
				Assertion.AssertEquals("Stage Name", worksheet[++rowCount, 1].ToString());
			}
			Assertion.AssertEquals("Start Time", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("End Time", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Duration", worksheet[++rowCount, 1].ToString());
			if (mainArchiveTableName == JobHeaderSchema.Constants.TableName)
			{
				Assertion.AssertEquals("JobShipments Per Hour", worksheet[++rowCount, 1].ToString());
			}
			Assertion.AssertEquals($"{mainArchiveTableName}s Per Hour", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

			Assertion.AssertEquals("Registry Settings:", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Batch Size", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("On Or Before Minimum", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

			Assertion.AssertEquals("Archive Schedule Parameters:", worksheet[++rowCount, 1].ToString());
			if (archiveSystemHasDateParameterSelection)
			{
				Assertion.AssertEquals("Date Parameter", worksheet[++rowCount, 1].ToString());
			}
			Assertion.AssertEquals("On Or Before Date", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Max. Run Duration (Minutes)", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

			Assertion.AssertContains("Archived Data Dated Between", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Table Name", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Deleted Record Count", worksheet[rowCount, 2].ToString());
			Assertion.AssertEquals("Record Count", worksheet[rowCount, 4].ToString());
			Assertion.AssertEquals("Approximate Space Used (MB)", worksheet[rowCount, 6].ToString());
		}

		public static void AssertPurgeSummaryReport(IeDoc archiveReport, string purgeCode, string mainArchiveTableName = JobHeaderSchema.Constants.TableName, bool archiveSystemHasMultipleArchiveStageDescriptors = true, bool archiveSystemHasDateParameterSelection = false)
		{
			using var stream = archiveReport.GetImageDataReader();
			using var excel = new ExcelInterface();
			excel.LoadExcelFile(stream);
			var rowCount = 0;
			var worksheet = excel.WorkSheets[0];
			var stageDescriptorConstant = typeof(ArchiveManagerConstants.Names).GetField(purgeCode).GetRawConstantValue();

			Assertion.AssertEquals(Core.Constants.FileFormats.XLSX, excel.GetExtensionForExcelFromFile());

			Assertion.AssertEquals($"{stageDescriptorConstant} Summary Report", worksheet[rowCount, 1].ToString());

			Assertion.AssertEquals("Database Server", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Database Name", worksheet[++rowCount, 1].ToString());
			if (archiveSystemHasMultipleArchiveStageDescriptors)
			{
				Assertion.AssertEquals("Stage Name", worksheet[++rowCount, 1].ToString());
			}
			Assertion.AssertEquals("Start Time", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("End Time", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Duration", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Total eDocs Purged", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("eDocs Purged Per Hour", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals($"{mainArchiveTableName}s Per Hour", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

			Assertion.AssertEquals("Registry Settings:", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Batch Size", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("On Or Before Minimum", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

			Assertion.AssertEquals("Archive Schedule Parameters:", worksheet[++rowCount, 1].ToString());
			if (archiveSystemHasDateParameterSelection)
			{
				Assertion.AssertEquals("Date Parameter", worksheet[++rowCount, 1].ToString());
			}
			Assertion.AssertEquals("On Or Before Date", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Max. Run Duration (Minutes)", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals(string.Empty, worksheet[++rowCount, 1].ToString());

			Assertion.AssertContains("Purged Data Dated Between", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Table Name", worksheet[++rowCount, 1].ToString());
			Assertion.AssertEquals("Deleted Record Count", worksheet[rowCount, 5].ToString());
			Assertion.AssertEquals("Record Count", worksheet[rowCount, 7].ToString());
			Assertion.AssertEquals("Approximate Space Used (MB)", worksheet[rowCount, 9].ToString());
		}

		public static void AssertAccManagementPeriod(BusinessObjectFactory factory)
		{
			var period = factory.Load<AccPeriodManagement>(new ZQuery(AccPeriodManagementSchema.AM_Period, 200701));
			Assertion.Assert("ArchiveCommenced", period[0].AM_ArchiveCommenced);
		}

		public static void AssertResultsForAMUsageCollector(JObject usageProperties, string archiveCode, string archiveSystemStageName, bool includeCustomsJobs, int batchSize, int totalRecordsDeleted, int jobHeadersProcessed, int mainRecordsLoaded, int totalMissingDocumentsGenerated, int documentsDeleted)
		{
			Assertion.CombineAssertions("Reported results should be correct.", () =>
			{
				Assertion.AssertEquals("Database Name", Db.DatabaseName, usageProperties.Value<string>(UsageProperties.DatabaseName));

				Assertion.AssertEquals("Archive Schedule Start Date/Time", ZDateTime.Now.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture), usageProperties.Value<string>(UsageProperties.ArchiveScheduleStartTime));
				Assertion.Assert("Archive Schedule Total Runtime Exists", usageProperties.Value<int>(UsageProperties.ArchiveScheduleTotalRuntime) > 0);
				Assertion.AssertEquals("Archive System Name", archiveCode, usageProperties.Value<string>(UsageProperties.ArchiveSystemName));
				Assertion.AssertEquals("Archive Stage Name", archiveSystemStageName, usageProperties.Value<string>(UsageProperties.ArchiveStageName));
				Assertion.AssertEquals("Archive System Date Parameter", $"On or Before {ZDateTime.UtcNow.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture)}", usageProperties.Value<string>(UsageProperties.ArchiveSystemDateParameters));
				Assertion.AssertEquals("Include Customs Jobs", includeCustomsJobs, usageProperties.Value<bool>(UsageProperties.IncludeCustomsJobsInArchiving));
				Assertion.AssertEquals("Archiving Batch Size", batchSize, usageProperties.Value<int>(UsageProperties.ArchivingBatchSize));

				Assertion.AssertEquals("Total Records Deleted", totalRecordsDeleted, usageProperties.Value<int>(UsageProperties.TotalRecordsDeletedFromAllTablesDuringArchiving));
				Assertion.AssertEquals("Total JobHeaders Processed", jobHeadersProcessed, usageProperties.Value<int>(UsageProperties.TotalJobHeadersProcessedDuringArchiving));
				Assertion.AssertEquals("Total Main Records Loaded", mainRecordsLoaded, usageProperties.Value<int>(UsageProperties.TotalMainRecordLoaded));
				Assertion.AssertEquals("Total Missing Documents Generated", totalMissingDocumentsGenerated, usageProperties.Value<int>(UsageProperties.TotalMissingDocumentsGeneratedDuringArchiving));
				Assertion.AssertEquals("Total Documents Deleted", documentsDeleted, usageProperties.Value<int>(UsageProperties.TotalDocumentsDeletedDuringArchiving));
			});
		}

		public static void AssertAllStageDescriptorsForSystemSortProperlyByMainDateFilterColumn(IArchiveSystem system, IArchiveConfiguration config)
		{
			var stageDescriptors = system.Descriptor.GetArchiveStageDescriptors(config);
			foreach (var descriptor in stageDescriptors)
			{
				var archiveStage = new ArchiveStage(descriptor, system.Descriptor);
				_ = descriptor.GetMainArchiveableFilter(config);
				Assertion.AssertEquals("Main archiveable query should contain date filter column or watermarking will fail",
					descriptor.MainDateFilterColumn.Name, archiveStage.GetMainBusinessObjectFilter(config).OrderBy.Split(',')[0]);
			}
		}

		public static void AssertTableRecordCounts(string tableName, int testRecordsCount, Dictionary<string, TableInfo> tableRecordCountsBaseline, Dictionary<string, TableInfo> tableRecordCountsWithTestRecords, Dictionary<string, TableInfo> tableRecordCountsAfterRun)
		{
			Assertion.AssertEquals("TableRecordCountsBaseline", 0, tableRecordCountsBaseline[tableName].RowCount);
			Assertion.AssertEquals("TableRecordCountsWithTestRecords", testRecordsCount, tableRecordCountsWithTestRecords[tableName].RowCount);
			Assertion.AssertEquals("TableRecordCountsAfterRun", 0, tableRecordCountsAfterRun[tableName].RowCount);
		}

		public static void AssertAfterPurge(List<string> tablesToIgnore, Dictionary<string, TableInfo> tableRecordCountsBaseline, Dictionary<string, TableInfo> tableRecordCountsWithTestRecords, Dictionary<string, TableInfo> tableRecordCountsAfterRun)
		{
			tablesToIgnore.Add(StorageMainSchema.Constants.TableName);
			tablesToIgnore.Add(StorageReferenceSchema.Constants.TableName);
			tablesToIgnore.Add(EDIMessageSchema.Constants.TableName);
			tablesToIgnore.Add("ArchiveMainItemQueue");
			tablesToIgnore.Add("ArchiveRelatedItemQueue");

			var diff = tableRecordCountsAfterRun.ToDictionary(tableCounts => tableCounts.Key, tableCounts => tableCounts.Value.RowCount)
				.Except(tableRecordCountsBaseline.ToDictionary(tableCounts => tableCounts.Key, tableCounts => tableCounts.Value.RowCount))
				.Where(tableCounts => !tablesToIgnore.Contains(tableCounts.Key) && !tableCounts.Key.Contains("ArchiveRelationship"));

			Assertion.Assert("All new test records created should be purged", !diff.Any());
			//Removed AM helper/queue tables
			Assertion.AssertEquals(tableRecordCountsAfterRun.Keys.Where(r => !tableRecordCountsWithTestRecords.ContainsKey(r)).Count(), 1);
			Assertion.AssertEquals("TableRecordCounts after purge should contain all test data records added", tableRecordCountsAfterRun.Count - 1, tableRecordCountsWithTestRecords.Count);

			var stmALogBaseline = tableRecordCountsAfterRun[StmALogSchema.Constants.TableName].RowCount;
			var stmALogfterRun = tableRecordCountsAfterRun[StmALogSchema.Constants.TableName].RowCount;
			Assertion.Assert("No. of StmALog", stmALogfterRun >= stmALogBaseline);
		}

		public static void AssertArchiveSetDetails(IArchiveStage stage, IArchiveSet set, TestArchiveLogger archiveLogger, List<string> tablesToIgnore, Dictionary<string, TableInfo> tableRecordCountsBaseline, Dictionary<string, TableInfo> tableRecordCountsWithTestRecords)
		{
			var recordsAdded = tableRecordCountsWithTestRecords.Except(tableRecordCountsBaseline).ToDictionary(tableCounts => tableCounts.Key, tableCounts => tableCounts.Value.RowCount);

			var archiveItems = new Dictionary<string, long>();
			_ = set.Load(archiveLogger);
			tablesToIgnore.Add("StmNumberCache");
			tablesToIgnore.Add(ViewStmNumsSchema.Constants.TableName);
			tablesToIgnore.Add(StmDataSchema.Constants.TableName); //docmanager saves a registry to cache the assembly data
			tablesToIgnore.Add(EDIInterchangeSchema.Constants.TableName);
			// These accounting records are not archived by OPS, but Headers and Lines's JH FK should be nullafied.
			tablesToIgnore.Add(AccHotChequeSchema.Constants.TableName);
			tablesToIgnore.Add(AccPaymentApprovalSchema.Constants.TableName);
			tablesToIgnore.Add(AccPaymentApprovalItemSchema.Constants.TableName);
			tablesToIgnore.Add(AccQueryClaimSchema.Constants.TableName);
			tablesToIgnore.Add(AccTransactionHeaderSchema.Constants.TableName);
			tablesToIgnore.Add(AccTransactionLinesSchema.Constants.TableName);
			tablesToIgnore.Add(OrgOpportunitySchema.Constants.TableName);
			tablesToIgnore.Add(OrgOpportunityValueSchema.Constants.TableName);
			tablesToIgnore.Add(AccTransactionMatchLinkSchema.Constants.TableName);

			tablesToIgnore.Add(StmActivityLogSchema.Constants.TableName);
			tablesToIgnore.Add(StmServiceHeartBeatSchema.Constants.TableName);
			tablesToIgnore.Add(StmServiceSemaphoreSchema.Constants.TableName);
			tablesToIgnore.Add(JobRequiredDocumentSchema.Constants.TableName); // Caused by change to enter correct doc ref type in storage main record. Need a way to deal with this in another archive system.

			tablesToIgnore.Add(JobContainerSchema.Constants.TableName);
			tablesToIgnore.Add(ProcessTasksSchema.Constants.TableName);
			tablesToIgnore.Add(ProcessTaskIterationLinkSchema.Constants.TableName);
			tablesToIgnore.Add(ProcessTaskIterationLinkPivotSchema.Constants.TableName);
			tablesToIgnore.Add(AccGLHeaderSchema.Constants.TableName);
			tablesToIgnore.Add(JobChargeRevRecognitionSchema.Constants.TableName);
			tablesToIgnore.Add("StmALogQueue");
			tablesToIgnore.Add("StmALogQueueWTE");
			tablesToIgnore.Add(StmALogSchema.Constants.TableName);
			tablesToIgnore.Add("StmNums");
			tablesToIgnore.Add("OrgPatternMatch");
			tablesToIgnore.Add("OrgAddress");
			tablesToIgnore.Add("CusEntryNum");
			tablesToIgnore.Add("ZZRefCusRuling");
			tablesToIgnore.Add(StmNoteSchema.Constants.TableName); //there is an orphan StmNote record in OdysseyDat database, which is being purged and gives difference -1. Let's ignore it.
			tablesToIgnore.Add(RateAttachmentSetSchema.Constants.TableName);
			tablesToIgnore.Add(DummyBizoSchema.Constants.TableName);
			tablesToIgnore.Add(PkgPackageJobSchema.Constants.TableName);
			tablesToIgnore.Add(PkgPackageSchema.Constants.TableName);
			tablesToIgnore.Add(CusCodeDataSchema.Constants.TableName);

			foreach (var archiveItem in set.GetArchiveItems())
			{
				if (archiveItems.TryGetValue(archiveItem.PKColumn.TableName, out var value))
				{
					archiveItems[archiveItem.PKColumn.TableName] = ++value;
				}
				else
				{
					archiveItems.Add(archiveItem.PKColumn.TableName, 1);
				}
			}

			var loadResult = stage.ArchiveToImages(set);
			((ArchiveStage)stage).OnArchiveSetProcessed(set);

			stage.EndRun();

			var diff = archiveItems.Except(recordsAdded).Where(result => !tablesToIgnore.Contains(result.Key));
			Assertion.CombineAssertions(() =>
			{
				Assertion.Assert("Count of records loaded in first archive set by archive system.", set.Count > 0);
				Assertion.Assert("Archive set should contain all test data records added.", !diff.Any());

				foreach (var errorMessage in loadResult.ErrorsEncountered)
				{
					Assertion.Assert($"Error: stageName={stage.Name} message={errorMessage}", condition: false);
				}
			});
		}

		public static void AssertEcommerceTestDataExists(BusinessObjectFactory factory, List<BusinessObject> archiveablePKs)
		{
			Assertion.AssertEquals("Correct number of eCommerce BizOs were created", 11, archiveablePKs.Count);

			Assertion.Assert("Precondition: Expected JobShipment to exist.", JobShipmentExistsInBusinessFactory_IgnoreActiveFilter(factory, archiveablePKs[0]));

			Assertion.AssertEquals("Precondition: Expected JobConsol to exist.", 1, factory.GetDatabaseCount(typeof(ForwardingConsol), new ZQuery(JobConsolSchema.PK, archiveablePKs[1].PK)));

			Assertion.AssertEquals("Precondition: Expected JobHeader (child of JobShipment) to exist.", 1, factory.GetDatabaseCount(typeof(JobHeader), new ZQuery(JobHeaderSchema.PK, archiveablePKs[2].PK)));
			Assertion.AssertEquals("Precondition: Expected JobHeader (child of JobConsol) to exist.", 1, factory.GetDatabaseCount(typeof(JobHeader), new ZQuery(JobHeaderSchema.PK, archiveablePKs[3].PK)));

			Assertion.AssertEquals("Precondition: Expected HVLVOuterPackage to exist.", 1, factory.GetDatabaseCount(typeof(HVLVOuterPackage), new ZQuery(HVLVOuterPackageSchema.PK, archiveablePKs[4].PK)));

			Assertion.AssertEquals("Precondition: Expected HVLVOuterPackage to exist.", 1, factory.GetDatabaseCount(typeof(HVLVOuterPackage), new ZQuery(HVLVOuterPackageSchema.PK, archiveablePKs[5].PK)));
			Assertion.AssertEquals("Precondition: Expected HVLVItem to exist.", 1, factory.GetDatabaseCount(typeof(HVLVItem), new ZQuery(HVLVItemSchema.PK, archiveablePKs[6].PK)));
			Assertion.AssertEquals("Precondition: Expected JobPackLines to exist.", 1, factory.GetDatabaseCount(typeof(ForwardingPackLine), new ZQuery(JobPackLinesSchema.PK, archiveablePKs[7].PK)));
			Assertion.AssertEquals("Precondition: Expected ELoadList to exist.", 1, factory.GetDatabaseCount(typeof(ELoadList), new ZQuery(ELoadListSchema.PK, archiveablePKs[8].PK)));
			Assertion.AssertEquals("Precondition: Expected SupplierBookingLine to exist.", 1, factory.GetDatabaseCount(typeof(SupplierBookingLine), new ZQuery(SupplierBookingLineSchema.PK, archiveablePKs[9].PK)));
			Assertion.AssertEquals("Precondition: Expected HVLVItem to exist.", 1, factory.GetDatabaseCount(typeof(HVLVItem), new ZQuery(HVLVItemSchema.PK, archiveablePKs[10].PK)));
		}

		public static void AssertEcommerceCustomsTestDataExists(BusinessObjectFactory factory, List<BusinessObject> archiveablePKs)
		{
			Assertion.AssertEquals("Correct number of eCommerce BizOs were created", 12, archiveablePKs.Count);

			Assertion.Assert("Precondition: Expected JobShipment to exist.", JobShipmentExistsInBusinessFactory_IgnoreActiveFilter(factory, archiveablePKs[0]));

			Assertion.AssertEquals("Precondition: Expected JobHeader (child of JobShipment - customs) to exist.", 1, factory.GetDatabaseCount(typeof(JobHeader), new ZQuery(JobHeaderSchema.PK, archiveablePKs[1].PK)));

			Assertion.AssertEquals("Precondition: Expected HVLVConsignmentHeader to exist.", 1, factory.GetDatabaseCount(typeof(HVLVConsignmentHeader), new ZQuery(HVLVConsignmentHeaderSchema.PK, archiveablePKs[2].PK)));
			Assertion.AssertEquals("Precondition: Expected HVLVConsignment to exist.", 1, factory.GetDatabaseCount(typeof(HVLVConsignment), new ZQuery(HVLVConsignmentSchema.PK, archiveablePKs[3].PK)));
			Assertion.AssertEquals("Precondition: Expected HVLVReturnPivot to exist.", 1, factory.GetDatabaseCount(typeof(HVLVReturnPivot), new ZQuery(HVLVReturnPivotSchema.PK, archiveablePKs[4].PK)));
			Assertion.AssertEquals("Precondition: Expected HVLVReturnPivot to exist.", 1, factory.GetDatabaseCount(typeof(HVLVReturnPivot), new ZQuery(HVLVReturnPivotSchema.PK, archiveablePKs[5].PK)));
			Assertion.AssertEquals("Precondition: Expected HVLVItem to exist.", 1, factory.GetDatabaseCount(typeof(HVLVItem), new ZQuery(HVLVItemSchema.PK, archiveablePKs[6].PK)));
			Assertion.AssertEquals("Precondition: Expected HVLVItemLine to exist.", 1, factory.GetDatabaseCount(typeof(HVLVItemLine), new ZQuery(HVLVItemLineSchema.PK, archiveablePKs[7].PK)));
			Assertion.AssertEquals("Precondition: Expected CusUSLVClearance to exist.", 1, factory.GetDatabaseCount(typeof(CusUSLVClearance), new ZQuery(CusUSLVClearanceSchema.PK, archiveablePKs[8].PK)));
			Assertion.AssertEquals("Precondition: Expected CusUSLVConsignment to exist.", 1, factory.GetDatabaseCount(typeof(CusUSLVConsignment), new ZQuery(CusUSLVConsignmentSchema.PK, archiveablePKs[9].PK)));
			Assertion.AssertEquals("Precondition: Expected CusUSLVItem to exist.", 1, factory.GetDatabaseCount(typeof(CusUSLVItem), new ZQuery(CusUSLVItemSchema.PK, archiveablePKs[10].PK)));
			Assertion.AssertEquals("Precondition: Expected CusUSLVItemPGA to exist.", 1, factory.GetDatabaseCount(typeof(CusUSLVItemPGA), new ZQuery(CusUSLVItemPGASchema.PK, archiveablePKs[11].PK)));
		}

		public static void AssertForwardingTestDataExists(BusinessObjectFactory factory, List<BusinessObject> archiveableBusinessObjects)
		{
			Assertion.AssertEquals("Correct number of eCommerce BizOs were created", 18, archiveableBusinessObjects.Count);

			Assertion.Assert("Precondition: Expected JobShipment to exist.", JobShipmentExistsInBusinessFactory_IgnoreActiveFilter(factory, archiveableBusinessObjects[0]));

			Assertion.AssertEquals("Precondition: Expected JobConsol to exist.", 1, factory.GetDatabaseCount(typeof(ForwardingConsol), new ZQuery(JobConsolSchema.PK, archiveableBusinessObjects[1].PK)));
			Assertion.AssertEquals("Precondition: Expected JobHeader (child of JobShipment) to exist.", 1, factory.GetDatabaseCount(typeof(JobHeader), new ZQuery(JobHeaderSchema.PK, archiveableBusinessObjects[2].PK)));
			Assertion.AssertEquals("Precondition: Expected JobHeader (child of JobConsol) to exist.", 1, factory.GetDatabaseCount(typeof(JobHeader), new ZQuery(JobHeaderSchema.PK, archiveableBusinessObjects[3].PK)));

			Assertion.AssertEquals("Precondition: Expected SupplierBookingLine to exist.", 1, factory.GetDatabaseCount(typeof(SupplierBookingLine), new ZQuery(SupplierBookingLineSchema.PK, archiveableBusinessObjects[4].PK)));
			Assertion.AssertEquals("Precondition: Expected ShipmentProfitShares to exist.", 1, factory.GetDatabaseCount(typeof(ShipmentProfitShares), new ZQuery(ShipmentProfitSharesSchema.PK, archiveableBusinessObjects[5].PK)));

			Assertion.AssertEquals("Precondition: Expected JobConsol to exist.", 1, factory.GetDatabaseCount(typeof(ForwardingConsol), new ZQuery(JobConsolSchema.PK, archiveableBusinessObjects[6].PK)));
			Assertion.AssertEquals("Precondition: Expected JobConShipLink to exist.", 1, factory.GetDatabaseCount(typeof(JobConShipLink), new ZQuery(JobConShipLinkSchema.PK, archiveableBusinessObjects[7].PK)));
			Assertion.AssertEquals("Precondition: Expected ConsolDGRestrictions to exist.", 1, factory.GetDatabaseCount(typeof(ConsolDGRestrictions), new ZQuery(JobConsolDGRestrictionsSchema.PK, archiveableBusinessObjects[8].PK)));
			Assertion.AssertEquals("Precondition: Expected HVLVOuterPackage to exist.", 1, factory.GetDatabaseCount(typeof(HVLVOuterPackage), new ZQuery(HVLVOuterPackageSchema.PK, archiveableBusinessObjects[9].PK)));
			Assertion.AssertEquals("Precondition: Expected CusMAWB to exist.", 1, factory.GetDatabaseCount(typeof(CusMAWB), new ZQuery(CusMAWBSchema.PK, archiveableBusinessObjects[10].PK)));
			Assertion.AssertEquals("Precondition: Expected JobContainer to exist.", 1, factory.GetDatabaseCount(typeof(ForwardingContainer), new ZQuery(JobContainerSchema.PK, archiveableBusinessObjects[11].PK)));
			Assertion.AssertEquals("Precondition: Expected JobConsolAWBSpecialHandling to exist.", 1, factory.GetDatabaseCount(typeof(JobConsolAWBSpecialHandling), new ZQuery(JobConsolAWBSpecialHandlingSchema.PK, archiveableBusinessObjects[12].PK)));
			Assertion.AssertEquals("Precondition: Expected ConsolidationProfitShare to exist.", 1, factory.GetDatabaseCount(typeof(ConsolidationProfitShare), new ZQuery(ConsolidationProfitShareSchema.PK, archiveableBusinessObjects[13].PK)));
			Assertion.AssertEquals("Precondition: Expected ProfitShareRedistribution to exist.", 1, factory.GetDatabaseCount(typeof(ProfitShareRedistribution), new ZQuery(ProfitShareRedistributionSchema.PK, archiveableBusinessObjects[14].PK)));
			Assertion.AssertEquals("Precondition: Expected ShipmentProfitShares to exist.", 1, factory.GetDatabaseCount(typeof(ShipmentProfitShares), new ZQuery(ShipmentProfitSharesSchema.PK, archiveableBusinessObjects[15].PK)));

			Assertion.AssertEquals("Precondition: Expected ConsolidationProfitShare to exist.", 1, factory.GetDatabaseCount(typeof(ConsolidationProfitShare), new ZQuery(ConsolidationProfitShareSchema.PK, archiveableBusinessObjects[16].PK)));
			Assertion.AssertEquals("Precondition: Expected ShipmentProfitShares to exist.", 1, factory.GetDatabaseCount(typeof(ShipmentProfitShares), new ZQuery(ShipmentProfitSharesSchema.PK, archiveableBusinessObjects[17].PK)));
		}
		public static void AssertBusinessObjectWasArchived(BusinessObjectFactory factory, BusinessObject businessObject)
		{
			factory.ClearQueryCache();

			if (businessObject.TableName == JobShipmentSchema.Constants.TableName)
			{
				Assertion.Assert($"Postcondition: Expected {businessObject.GetType()} of PK {businessObject.PK} to be successfully archived.", !JobShipmentExistsInBusinessFactory_IgnoreActiveFilter(factory, businessObject));
			}
			else
			{
				Assertion.AssertEquals($"Postcondition: Expected {businessObject.GetType()} of PK {businessObject.PK} to be successfully archived.", 0, factory.GetDatabaseCount(businessObject.GetType(), new ZQuery(businessObject.PKSchemaColumn, businessObject.PK)));
			}
		}

		public static void AssertBusinessObjectWasNotArchived(BusinessObjectFactory factory, BusinessObject businessObject)
		{
			factory.ClearQueryCache();

			if (businessObject.TableName == JobShipmentSchema.Constants.TableName)
			{
				Assertion.Assert($"Postcondition: Expected {businessObject.GetType()} of PK {businessObject.PK} to not have been archived.", JobShipmentExistsInBusinessFactory_IgnoreActiveFilter(factory, businessObject));
			}
			else
			{
				Assertion.AssertEquals($"Postcondition: Expected {businessObject.GetType()} of PK {businessObject.PK} to not have been archived.", 1, factory.GetDatabaseCount(businessObject.GetType(), new ZQuery(businessObject.PKSchemaColumn, businessObject.PK)));
			}
		}

		public static bool JobShipmentExistsInBusinessFactory_IgnoreActiveFilter(BusinessObjectFactory factory, BusinessObject jobShipment)
		{
			var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
			_ = query.AddToFilter(JobShipmentSchema.PK, jobShipment.PK);
			query.IgnoreActiveFilter = true;

			return factory.ExistsInDatabase(JobShipmentSchema.Constants.TableName, query);
		}
	}
}
