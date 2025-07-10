using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Engine.Test
{
	[UseSnapshotProtection]
	public class ArchiveSetTest : TestCaseWithFactory
	{
		public class ArchiveSetThatThrowsExceptionOnLoad : ArchiveSet
		{
			public ArchiveSetThatThrowsExceptionOnLoad(IArchiveSystemDescriptor systemDescriptor, string stageName, Guid scheduleIdentifier, ArchiveItem mainItem, ArchiveableType mainArchiveableType, ZQuery mainArchiveableTypeFilter, string mainArchiveItemNK)
				: base(systemDescriptor, stageName, scheduleIdentifier, mainItem, mainArchiveableType, mainArchiveableTypeFilter, mainArchiveItemNK)
			{
			}

			public Exception ExToThrow { get; set; }

			public int NumberOfExceptionsToThrowWhenLoadingForTest { get; set; }

			public override void LoadUnsafe(string suffix)
			{
				if (NumberOfExceptionsToThrowWhenLoadingForTest > 0)
				{
					NumberOfExceptionsToThrowWhenLoadingForTest--;
					throw ExToThrow ?? SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded");
				}
				else
				{
					base.LoadUnsafe(suffix);
				}
			}
		}

		public class ArchiveSetThatFailsToFindMainRecordInItsTable : ArchiveSet
		{
			public ArchiveSetThatFailsToFindMainRecordInItsTable(IArchiveSystemDescriptor systemDescriptor, string stageName, Guid scheduleIdentifier, ArchiveItem mainItem, ArchiveableType mainArchiveableType, ZQuery mainArchiveableTypeFilter, string mainArchiveItemNK)
				: base(systemDescriptor, stageName, scheduleIdentifier, mainItem, mainArchiveableType, mainArchiveableTypeFilter, mainArchiveItemNK)
			{
			}

			protected override bool QueryMainTableToFindRecord()
			{
				throw new Exception("Test exception");
			}
		}

		public void TestLoadArchiveSetTimeout()
		{
			AssertEquals(SystemDataRegistry.Instance.LoadArchiveSetTimeout.Value, ArchiveSet.loadArchiveSetTimeout);

			SystemDataRegistry.Instance.LoadArchiveSetTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals(50, ArchiveSet.loadArchiveSetTimeout);
		}

		public void TestGetTotalDocumentsDeletedFromArchiveItems()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var dummyChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			dummyChildBizo.Z0_Date = dummyBizo.Z0_Date;

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault();
			archiveSet.Load(logger);
			var mainArchiveItem = archiveSet.GetArchiveItem(dummyBizo.PK.ToGuid());
			mainArchiveItem.TotalDocumentsDeleted = 2;
			var childArchiveItem = archiveSet.GetArchiveItem(dummyChildBizo.PK.ToGuid());
			childArchiveItem.TotalDocumentsDeleted = 10;

			var documentsDeleted = archiveSet.GetTotalDocumentsDeletedFromArchiveItems();

			AssertEquals("There should be two items in the collection.", 2, documentsDeleted.Count);

			AssertCollectionContains("StorageMain should be included in the collection.", StorageMainSchema.Constants.TableName, documentsDeleted.Keys);
			AssertCollectionContains("StorageDocs should be included in the collection.", StorageDocsSchema.Constants.TableName, documentsDeleted.Keys);

			AssertEquals("Total StorageMains from the ArchiveSet.", 2, documentsDeleted[StorageMainSchema.Constants.TableName].Count);
			AssertEquals("Total StorageDocs deleted from the ArchiveSet.", 12, documentsDeleted[StorageDocsSchema.Constants.TableName].Count);
		}

		public void TestDoesNotThrowAndDoesNotMultiLoadWhenRetryIsNotExceeded()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var dummyChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			dummyChildBizo.Z0_Date = dummyBizo.Z0_Date;

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage
				.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) }, schedule, archiveStage)
				.FirstOrDefault() as ArchiveSet;
			var testArchiveSet = 
				new ArchiveSetThatThrowsExceptionOnLoad(systemDescriptor, archiveStage.StageNameForQueries, schedule.SchedulePK, archiveSet.MainArchiveItem as ArchiveItem, 
				archiveSet.mainArchiveableType, archiveSet.MainArchiveableTypeFilter, archiveSet.MainArchiveItemNK);
			testArchiveSet.NumberOfExceptionsToThrowWhenLoadingForTest = 2;
			_ = testArchiveSet.Load(logger);
			AssertEquals("No error was reported", 0, ErrorReporter.TotalErrorCount);
			AssertEquals("Only the 1 related record is loaded into the related item queue", 1,
				ArchiveTableHelper.GetNumberOfItemsInRelatedRecordsArchiveQueue());
		}

		public void TestThrowsWhenRetryThresholdIsExceeded()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var dummyChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			dummyChildBizo.Z0_Date = dummyBizo.Z0_Date;

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault() as ArchiveSet;
			var testArchiveSet =
				new ArchiveSetThatThrowsExceptionOnLoad(systemDescriptor, archiveStage.StageNameForQueries, schedule.SchedulePK, archiveSet.MainArchiveItem as ArchiveItem,
				archiveSet.mainArchiveableType, archiveSet.MainArchiveableTypeFilter, archiveSet.MainArchiveItemNK);
			testArchiveSet.NumberOfExceptionsToThrowWhenLoadingForTest = 5;
			_ = testArchiveSet.Load(logger);
			AssertEquals("Error should have been reported", "ArchiveManager.ArchiveSetLoadException", ErrorReporter.LastKeyReported);
			AssertEquals("Nothing is loaded into the related item queue", 0,
				ArchiveTableHelper.GetNumberOfItemsInRelatedRecordsArchiveQueue());
		}

		public void TestFetchMainItemFK()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var dummyChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			dummyChildBizo.Z0_Date = dummyBizo.Z0_Date;

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault() as ArchiveSet;
			var mainItemFK = archiveSet.MainItemFK;

			AssertNotEquals("As we haven't deleted from the main queue, the main item FK shouldn't be empty", Guid.Empty, mainItemFK);
		}

		public void TestFetchMainItemFKWhenMainQueueRecordHasVanished()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault() as ArchiveSet;
			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ArchiveMainItemQueue");
			Guid mainItemFK;

			try
			{
				mainItemFK = archiveSet.MainItemFK;
				Fail("When archiveSet.MainItemFK does not exist, an exception should be thrown, but no exception was found");
			}
			catch (Exception ex)
			{
				var exMessage = ex.Message;

				CombineAssertions("Exception message contains the correct parts", () =>
				{
					AssertContains("Contains opening line", "The expected corresponding record in ArchiveMainItemQueue was not found.", exMessage);
					AssertContains("Main archive item PK", $"MainArchiveItem.PK was: '{dummyBizo.PK}'", exMessage);
					AssertContains("Main archive item table name", $"MainArchiveItem Table was: '{dummyBizo.TableName}'", exMessage);
					AssertContains("Main archive item still exists", $"MainArchiveItem still exists in its table: {true}", exMessage);
					AssertContains("Stage name", "StageName: 'SimpleStageWithoutDeclarations'.", exMessage);
					AssertContains("Schedule PK", "SchedulePK: '00000000-0000-0000-0000-000000000000'.", exMessage);
					AssertContains("Schedule PK string", "SchedulePKString: AIM_S5_ParentSchedule IS NULL.", exMessage);
				});
			}
		}

		public void TestExceptionWhenFindingMainRecordInItsTableIsHandled()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var dummyChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			dummyChildBizo.Z0_Date = dummyBizo.Z0_Date;

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var originalSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault() as ArchiveSet;
			var testArchiveSet = new ArchiveSetThatFailsToFindMainRecordInItsTable(systemDescriptor, archiveStage.Name, originalSet.ScheduleIdentifier,
				originalSet.MainArchiveItem as ArchiveItem, originalSet.mainArchiveableType, originalSet.MainArchiveableTypeFilter, originalSet.MainArchiveItemNK);
			var stringDescribingIfMainRecordStillExists = testArchiveSet.DoesMainRecordStillExistInItsActualTable();

			AssertEquals("Should mention exception", "An exception was encountered while trying to determine if the top-level record being archived still exists: 'Test exception'.", stringDescribingIfMainRecordStillExists);
		}

		public void TestMainRecordNoLongerExists()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var dummyChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			dummyChildBizo.Z0_Date = dummyBizo.Z0_Date;

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault() as ArchiveSet;

			AssertEquals("Precondition: records initially exist", 2, Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.DummyBizo"));

			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.DummyBizo");

			AssertEquals("Should correctly recognise top-level record no longer exists", false.ToString(), archiveSet.DoesMainRecordStillExistInItsActualTable());
		}

		public void TestLoggingOfPartiallyLoadedItems()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);
			dummyBizo.Z0_Code = "0";

			var dummyChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			dummyChildBizo.Z0_Date = dummyBizo.Z0_Date;
			dummyChildBizo.Z0_Code = dummyBizo.Z0_Code + "C1";

			var otherChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			otherChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			otherChildBizo.Z0_Date = dummyBizo.Z0_Date;
			otherChildBizo.Z0_Code = dummyBizo.Z0_Code + "C2";

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var originalSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault() as ArchiveSet;
			var testSet =
				new ArchiveSetThatThrowsExceptionOnLoad(systemDescriptor, archiveStage.StageNameForQueries, schedule.SchedulePK, originalSet.MainArchiveItem as ArchiveItem,
				originalSet.mainArchiveableType, originalSet.MainArchiveableTypeFilter, originalSet.MainArchiveItemNK);
			testSet.ExToThrow = SqlExceptionBuilder
				.CreateSqlException(-2, "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.");
			testSet.NumberOfExceptionsToThrowWhenLoadingForTest = 4;
			_ = testSet.Load(logger);

			AssertEquals("Nothing is left behind in related item queue", 0, ArchiveTableHelper.GetNumberOfItemsInRelatedRecordsArchiveQueue());
			AssertEquals("Error is reported to IM", "ArchiveManager.ArchiveSetLoadTimeout", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestLoadReportsMainRecordNotFoundException()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);
			dummyBizo.Z0_Code = "0";

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var originalSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault() as ArchiveSet;
			var testSet =
				new ArchiveSetThatThrowsExceptionOnLoad(systemDescriptor, archiveStage.StageNameForQueries, schedule.SchedulePK, originalSet.MainArchiveItem as ArchiveItem,
				originalSet.mainArchiveableType, originalSet.MainArchiveableTypeFilter, originalSet.MainArchiveItemNK);
			testSet.ExToThrow = SqlExceptionBuilder
				.CreateSqlException(-2, "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.");
			_ = testSet.Load(logger);

			_ = Db.Connection.ExecuteNonQuery("DELETE FROM dbo.ArchiveMainItemQueue");
			testSet =
				new ArchiveSetThatThrowsExceptionOnLoad(systemDescriptor, archiveStage.StageNameForQueries, schedule.SchedulePK, originalSet.MainArchiveItem as ArchiveItem,
				originalSet.mainArchiveableType, originalSet.MainArchiveableTypeFilter, originalSet.MainArchiveItemNK);
			_ = testSet.Load(logger);

			AssertEquals("Nothing is left behind in related item queue", 0, ArchiveTableHelper.GetNumberOfItemsInRelatedRecordsArchiveQueue());
			AssertEquals("Error is reported to IM", "ArchiveManager.ArchiveQueueMainRecordNotFound", ErrorReporter.LastKeyReported);

			ErrorReporter.Clear();
		}

		public void TestExaminePartiallyLoadedRecords()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);
			dummyBizo.Z0_Code = "0";

			var dummyChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			dummyChildBizo.Z0_Date = dummyBizo.Z0_Date;
			dummyChildBizo.Z0_Code = dummyBizo.Z0_Code + "C1";

			var otherChildBizo = Factory.NewWithValidTestData<DummyChildBusinessObject>();
			otherChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
			otherChildBizo.Z0_Date = dummyBizo.Z0_Date;
			otherChildBizo.Z0_Code = dummyBizo.Z0_Code + "C2";

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-10) },
				schedule, archiveStage).FirstOrDefault() as ArchiveSet;
			_ = archiveSet.Load(logger);
			var relatedLoadedRecordsString = archiveSet.GetSummaryOfAllItemsRetrievedBeforeTimeout();

			AssertEquals("Precondition: There are two items in the related item queue", 2, ArchiveTableHelper.GetNumberOfItemsInRelatedRecordsArchiveQueue());
			AssertContains("Related loaded records", "DummyChild: 2", relatedLoadedRecordsString);
		}

		public void TestArchiveWatermarkDateIsZDateTime()
		{
			var watermark = new ArchiveWatermark();
			AssertEquals("WatermarkDate is ZDateTime", watermark.WatermarkDate.GetType(), typeof(ZDateTime));
		}
	}
}
