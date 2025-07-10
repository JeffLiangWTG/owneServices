using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Actions.ArchiveReport.ReportGenerators
{
	class ReportGeneratorHelperTest : TestCaseWithFactory
	{
		public void TestAddProcessedRecordsCountToReportDataSource()
		{
			var dummyBizoWithDependents = Factory.NewWithValidTestData<DummyWithDependentsBusinessObject>();
			_ = dummyBizoWithDependents.Dependents.AddNew();
			_ = dummyBizoWithDependents.Dependents.AddNew();

			Factory.Save();

			var topLevelDataSource = new ReportDataSourceNonPersistentBusinessObject(Factory);
			var processedRecordsCountPerTable = new Dictionary<string, ITableProcessingInfo>
			{
				{ DummyBizoSchema.Constants.TableName, new TableProcessingInfo(10) },
				{ DummyDependentBizoSchema.Constants.TableName, new TableProcessingInfo(22) },
			};

			ReportGeneratorHelper.AddProcessedRecordsCountToReportDataSource(topLevelDataSource, new ConcurrentDictionary<string, ITableProcessingInfo>(processedRecordsCountPerTable));

			CombineAssertions("Data source has been prepared correctly.", () =>
			{
				AssertEquals("Collection count", 2, topLevelDataSource.Collection.Count);

				var dummyDependentBizoCollection = topLevelDataSource.Collection.Where(bizo => bizo.TableNameCounted == DummyDependentBizoSchema.Constants.TableName);
				AssertEquals("There should be 1 DummyDependentBizo item in the collection.", 1, dummyDependentBizoCollection.Count());

				var dummyDependentBizo = dummyDependentBizoCollection.First();
				AssertEquals("dummyDependentBizo.DeletedRecordCount", 22, dummyDependentBizo.DeletedRecordCount);
				AssertEquals("dummyDependentBizo.RecordCountAfter", 2, dummyDependentBizo.RecordCountAfter);
				AssertEquals("dummyDependentBizo.SpaceUsedAfter", 0, dummyDependentBizo.SpaceUsedAfter);

				var dummyBizoCollection = topLevelDataSource.Collection.Where(bizo => bizo.TableNameCounted == DummyBizoSchema.Constants.TableName);
				AssertEquals("There should be 1 DummyBizo item in the collection.", 1, dummyBizoCollection.Count());

				var dummyBizo = dummyBizoCollection.First();
				AssertEquals("dummyBizo.DeletedRecordCount", 10, dummyBizo.DeletedRecordCount);
				AssertEquals("dummyBizo.RecordCountAfter", 1, dummyBizo.RecordCountAfter);
				AssertEquals("dummyBizo.SpaceUsedAfter", 0, dummyBizo.SpaceUsedAfter);
			});
		}

		public void TestAddProcessedRecordsCountToReportDataSource_WithStorageDocs()
		{
			SetupStorageDocsDB();

			var documentFactoryProvider = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			var numberedFactory = documentFactoryProvider.GetFactory(DbNumber);
			_ = numberedFactory.NewWithValidTestData<StorageDocs>();

			documentFactoryProvider.Save();

			var topLevelDataSource = new ReportDataSourceNonPersistentBusinessObject(Factory);
			var processedRecordsCountPerTable = new Dictionary<string, ITableProcessingInfo>
			{
				{ StorageMainSchema.Constants.TableName, new TableProcessingInfo(5) },
				{ StorageDocsSchema.Constants.TableName, new TableProcessingInfo(11) },
			};

			ReportGeneratorHelper.AddProcessedRecordsCountToReportDataSource(topLevelDataSource, new ConcurrentDictionary<string, ITableProcessingInfo>(processedRecordsCountPerTable));

			CombineAssertions("Data source has been prepared correctly.", () =>
			{
				AssertEquals("Collection count", 2, topLevelDataSource.Collection.Count);

				var storageMainCollection = topLevelDataSource.Collection.Where(bizo => bizo.TableNameCounted == StorageMainSchema.Constants.TableName);
				AssertEquals("There should be 1 StorageMain item in the collection.", 1, storageMainCollection.Count());

				var storageMain = storageMainCollection.First();
				AssertEquals("storageMain.DeletedRecordCount", 5, storageMain.DeletedRecordCount);
				AssertEquals("storageMain.RecordCountAfter", 1, storageMain.RecordCountAfter);
				AssertEquals("storageMain.SpaceUsedAfter", 0, storageMain.SpaceUsedAfter);

				var storageDocsCollection = topLevelDataSource.Collection.Where(bizo => bizo.TableNameCounted == StorageDocsSchema.Constants.TableName);
				AssertEquals("There should be 1 StorageDocs item in the collection.", 1, storageDocsCollection.Count());

				var storageDocs = storageDocsCollection.First();
				AssertEquals("storageDocs.DeletedRecordCount", 11, storageDocs.DeletedRecordCount);
				Assert("storageDocs.RecordCountAfter", storageDocs.RecordCountAfter >= 1);
				AssertEquals("storageDocs.SpaceUsedAfter", 0, storageDocs.SpaceUsedAfter);
			});
		}

		public void TestAddProcessedRecordsCountToReportDataSource_WhenRecordNotPurged()
		{
			var dummyDependentBizo = Factory.NewWithValidTestData<DummyWithDependentsBusinessObject>();
			_ = dummyDependentBizo.Dependents.AddNew();
			_ = dummyDependentBizo.Dependents.AddNew();

			Factory.Save();

			var topLevelDataSource = new ReportDataSourceNonPersistentBusinessObject(Factory);
			var processedRecordsCountPerTable = new Dictionary<string, ITableProcessingInfo>
			{
				{ DummyBizoSchema.Constants.TableName, new TableProcessingInfo(1, purged: false) },
				{ DummyDependentBizoSchema.Constants.TableName, new TableProcessingInfo(5) },
			};

			ReportGeneratorHelper.AddProcessedRecordsCountToReportDataSource(topLevelDataSource, new ConcurrentDictionary<string, ITableProcessingInfo>(processedRecordsCountPerTable));

			CombineAssertions("Data source has been prepared correctly.", () =>
			{
				AssertEquals("Collection count", 1, topLevelDataSource.Collection.Count);

				var dummyDependentBizo = topLevelDataSource.Collection[0];
				AssertEquals("dummyDependentBizo.DeletedRecordCount", 5, dummyDependentBizo.DeletedRecordCount);
				AssertEquals("dummyDependentBizo.RecordCountAfter", 2, dummyDependentBizo.RecordCountAfter);
				AssertEquals("dummyDependentBizo.SpaceUsedAfter", 0, dummyDependentBizo.SpaceUsedAfter);
			});
		}

		[TestDate(2022, 02, 22)]
		public void TestGetReportFileName()
		{
			var dummyArchiveSystem = new DummyArchiveSystem("DMS");
			var dummyArchiveStage = new DummyArchiveStage();
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var reportName = ReportGeneratorHelper.GetReportFileName(dummyArchiveSystem, dummyArchiveStage, config);

			AssertEquals("DummyArchiveSystemReport_202202220000.xls", reportName);
		}

		[TestDate(2022, 02, 22)]
		public void TestGetReportFileNameWithStageNames()
		{
			var systemDescriptor = new IPSArchiveSystemDescriptor();
			var stageDescriptor = new IPSInactiveShipmentArchiveStageDescriptor();
			var config = new ArchiveConfiguration(new ZDateTime(2020, 02, 02), 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var reportName = ReportGeneratorHelper.GetReportFileName(systemDescriptor, stageDescriptor, config);

			AssertEquals("InactiveOperationalJobsArchiveSystemReport_JobShipment_202202220000.xls", reportName);
		}

		public void TestGetReportFileDescription()
		{
			var reportName = ReportGeneratorHelper.GetReportFileDescription(new DummyArchiveSystem("DMS"));

			AssertEquals("Archive Report", reportName);
		}

		public void TestGetStorageDocsCountFromProcessedRecords()
		{
			var processedRecordsCountPerTable = new Dictionary<string, ITableProcessingInfo>();
			processedRecordsCountPerTable.Add(StorageDocsSchema.Constants.TableName, new TableProcessingInfo(100));

			var storageDocsCount = ReportGeneratorHelper.GetProcessedCountForTable(StorageDocsSchema.Constants.TableName, new ConcurrentDictionary<string, ITableProcessingInfo>(processedRecordsCountPerTable));

			AssertEquals("Precondition", 1, processedRecordsCountPerTable.Count);
			AssertEquals("StorageDocs count", 100, storageDocsCount);
		}

		public void TestGetStorageDocsCountFromProcessedRecords_WhenProcessedRecordsIsEmpty()
		{
			var processedRecordsCountPerTable = new Dictionary<string, ITableProcessingInfo>();

			var storageDocsCount = ReportGeneratorHelper.GetProcessedCountForTable(StorageDocsSchema.Constants.TableName, new ConcurrentDictionary<string, ITableProcessingInfo>(processedRecordsCountPerTable));

			AssertEquals("Precondition", 0, processedRecordsCountPerTable.Count);
			AssertEquals("StorageDocs count", 0, storageDocsCount);
		}

		public void TestGetStorageDocsCountFromProcessedRecords_WhenProcssedRecordsDoesntIncludeStorageDocs()
		{
			var processedRecordsCountPerTable = new Dictionary<string, ITableProcessingInfo>();
			processedRecordsCountPerTable.Add(DummyBizoSchema.Constants.TableName, new TableProcessingInfo(5));
			processedRecordsCountPerTable.Add(JobHeaderSchema.Constants.TableName, new TableProcessingInfo(20));

			var storageDocsCount = ReportGeneratorHelper.GetProcessedCountForTable(StorageDocsSchema.Constants.TableName, new ConcurrentDictionary<string, ITableProcessingInfo>(processedRecordsCountPerTable));

			AssertEquals("Precondition", 2, processedRecordsCountPerTable.Count);
			AssertEquals("StorageDocs count", 0, storageDocsCount);
		}

		public void TestSortReportDataSourceCollection()
		{
			var dataSource = new ReportDataSourceNonPersistentBusinessObject();

			var record1 = dataSource.Collection.AddNew();
			record1.TableNameCounted = DummyBizoSchema.Constants.TableName;
			record1.DeletedRecordCount = 40;
			record1.RecordCountAfter = 100;

			var record2 = dataSource.Collection.AddNew();
			record2.TableNameCounted = DummyDependentBizoSchema.Constants.TableName;
			record2.DeletedRecordCount = 13;
			record2.RecordCountAfter = 25;

			var record3 = dataSource.Collection.AddNew();
			record3.TableNameCounted = StorageMainSchema.Constants.TableName;
			record3.DeletedRecordCount = 4;
			record3.RecordCountAfter = 30;

			var record4 = dataSource.Collection.AddNew();
			record4.TableNameCounted = StorageDocsSchema.Constants.TableName;
			record4.DeletedRecordCount = 13;
			record4.RecordCountAfter = 56;

			ReportGeneratorHelper.SortReportDataSourceCollection(dataSource.Collection);

			var expected = new ReportDataSourceNonPersistentBusinessObject();
			expected.Collection.Add(record1);
			expected.Collection.Add(record4);
			expected.Collection.Add(record2);
			expected.Collection.Add(record3);

			AssertContainsExactElementsInExactOrder(expected.Collection.Select(r => r.TableNameCounted), dataSource.Collection.Select(r => r.TableNameCounted));
		}

		public void TestKBToMB_ForZeroKBValue()
			=> AssertEquals("Kilobytes should be correctly converted to Megabytes.", 0, ReportGeneratorHelper.KBToMB(0));

		public void TestKBToMB_ForSmallKBValue()
			=> AssertEquals("Kilobytes should be correctly converted to Megabytes.", 0, ReportGeneratorHelper.KBToMB(100));

		public void TestKBToMB()
			=> AssertEquals("Kilobytes should be correctly converted to Megabytes.", 1, ReportGeneratorHelper.KBToMB(1024));

		public void TestKBToMB_ForLargeKBValue()
			=> AssertEquals("Kilobytes should be correctly converted to Megabytes.", 3198099, ReportGeneratorHelper.KBToMB(3274854328));

		const int DbNumber = 333;
		readonly DocManagerDBHelperTestClass DbHelper = new();

		void SetupStorageDocsDB()
		{
			if (!DbHelper.DatabaseExists(DbNumber))
			{
				_ = DbHelper.CreateDatabase(DbNumber);
			}
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();
			if (DbHelper.DatabaseExists(DbNumber))
			{
				var dbName = DbHelper.GetDatabaseName(DbNumber);
				DbHelper.DropDatabase(dbName);
			}
		}
	}
}
