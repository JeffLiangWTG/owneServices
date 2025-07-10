using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors
{
	public class OnArchiveSetProcessedHelpersTest : TestCaseWithFactory
	{
		public void TestAddOrUpdateDocumentsDeletedCount()
		{
			var systemDescriptor = new DummyArchiveSystem();
			var stageDescriptor = new DummyArchiveStage();
			var mainArchiveableType = new ArchiveableType(stageDescriptor.MainArchivePKColumn, stageDescriptor.MainArchiveNKColumn);

			var mainArchiveItem = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), null, Guid.Empty, isReversed: false, DummyBizoSchema.Constants.TableName);
			mainArchiveItem.TotalDocumentsDeleted = 3;

			var childArchiveItem1 = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), mainArchiveItem.PKColumn, mainArchiveItem.PK, isReversed: false, DummyDependentBizoSchema.Constants.TableName);
			var childArchiveItem2 = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), mainArchiveItem.PKColumn, mainArchiveItem.PK, isReversed: false, DummyDependentBizoSchema.Constants.TableName);
			childArchiveItem1.TotalDocumentsDeleted = 5;
			childArchiveItem2.TotalDocumentsDeleted = 1;

			var archiveSet = new TestArchiveSet(systemDescriptor, stageDescriptor.Name, Guid.Empty, mainArchiveItem, childItems: new List<IArchiveItem>() { childArchiveItem1, childArchiveItem2 });

			var processingInfoPerTable = new ConcurrentDictionary<string, ITableProcessingInfo>();

			AssertEquals("Precondition", 0, processingInfoPerTable.Count);

			OnArchiveSetProcessedHelpers.AddOrUpdateDocumentsDeletedCount(archiveSet, processingInfoPerTable);

			CombineAssertions("'processingInfoPerTable' should contain two key/value pairs: 'StorageMain' and 'StorageDocs'.", () =>
			{
				AssertEquals(2, processingInfoPerTable.Count);
				AssertCollectionContains("StorageMain", StorageMainSchema.Constants.TableName, processingInfoPerTable.Keys);
				AssertCollectionContains("StorageDocs", StorageDocsSchema.Constants.TableName, processingInfoPerTable.Keys);
			});

			CombineAssertions(() =>
			{
				AssertEquals("Value for 'StorageMain' should be Count=3", 3, processingInfoPerTable[StorageMainSchema.Constants.TableName].Count);
				Assert("Value for 'StorageMain' should be Purged=true", processingInfoPerTable[StorageMainSchema.Constants.TableName].Purged);

				AssertEquals("Value for 'StorageDocs' should be Count=9", 9, processingInfoPerTable[StorageDocsSchema.Constants.TableName].Count);
				Assert("Value for 'StorageDocs' should be Purged=true", processingInfoPerTable[StorageDocsSchema.Constants.TableName].Purged);
			});
		}

		public void TestAddOrUpdateRecordsProcessedCount()
		{
			var systemDescriptor = new DummyArchiveSystem();
			var stageDescriptor = new DummyArchiveStage();
			var mainArchiveableType = new ArchiveableType(stageDescriptor.MainArchivePKColumn, stageDescriptor.MainArchiveNKColumn);

			var mainArchiveItem = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), null, Guid.Empty, isReversed: false, DummyBizoSchema.Constants.Prefix);

			var childArchiveItem1 = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), mainArchiveItem.PKColumn, mainArchiveItem.PK, isReversed: false, DummyDependentBizoSchema.Constants.Prefix);
			var childArchiveItem2 = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), mainArchiveItem.PKColumn, mainArchiveItem.PK, isReversed: false, DummyDependentBizoSchema.Constants.Prefix);

			var archiveSet = new TestArchiveSet(systemDescriptor, stageDescriptor.Name, Guid.Empty, mainArchiveItem, childItems: new List<IArchiveItem>() { childArchiveItem1, childArchiveItem2 });

			var processingInfoPerTable = new ConcurrentDictionary<string, ITableProcessingInfo>();

			AssertEquals("Precondition", 0, processingInfoPerTable.Count);

			OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(archiveSet, processingInfoPerTable, recordsPurged: true);

			CombineAssertions("'processingInfoPerTable' should contain two key/value pairs: 'DummyBizo' and 'DummyDependentBizo'.", () =>
			{
				AssertEquals(2, processingInfoPerTable.Count);
				AssertCollectionContains("DummyBizo", DummyBizoSchema.Constants.TableName, processingInfoPerTable.Keys);
				AssertCollectionContains("DummyDependentBizo", DummyDependentBizoSchema.Constants.TableName, processingInfoPerTable.Keys);
			});

			CombineAssertions(() =>
			{
				AssertEquals("Value for 'DummyBizo' should be Count=1", 1, processingInfoPerTable[DummyBizoSchema.Constants.TableName].Count);
				Assert("Value for 'DummyBizo' should be Purged=true", processingInfoPerTable[DummyBizoSchema.Constants.TableName].Purged);

				AssertEquals("Value for 'DummyDependentBizo' should be Count=2", 2, processingInfoPerTable[DummyDependentBizoSchema.Constants.TableName].Count);
				Assert("Value for 'DummyDependentBizo' should be Purged=true", processingInfoPerTable[DummyDependentBizoSchema.Constants.TableName].Purged);
			});
		}

		public void TestAddOrUpdateRecordsProcessedCount_WhenArchiveSetsHaveDifferentPurgeableState_ThenPurgedValueDoesNotGetUpdated()
		{
			var systemDescriptor = new DummyArchiveSystem();
			var stageDescriptor = new DummyArchiveStage();
			var mainArchiveableType = new ArchiveableType(stageDescriptor.MainArchivePKColumn, stageDescriptor.MainArchiveNKColumn);
			var mainArchiveItem = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), null, Guid.Empty, isReversed: false, DummyBizoSchema.Constants.Prefix);
			var archiveSet = new TestArchiveSet(systemDescriptor, stageDescriptor.Name, Guid.Empty, mainArchiveItem);
			var processingInfoPerTable = new ConcurrentDictionary<string, ITableProcessingInfo>();

			AssertEquals("Precondition", 0, processingInfoPerTable.Count);

			OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(archiveSet, processingInfoPerTable, recordsPurged: true);

			CombineAssertions("Run 1: 'processingInfoPerTable' should contain one key/value pair: 'DummyBizo'", () =>
			{
				AssertEquals(1, processingInfoPerTable.Count);
				AssertCollectionContains("DummyBizo", DummyBizoSchema.Constants.TableName, processingInfoPerTable.Keys);
			});

			CombineAssertions("Run 1", () =>
			{
				AssertEquals("Value for 'DummyBizo' should be Count=1", 1, processingInfoPerTable[DummyBizoSchema.Constants.TableName].Count);
				Assert("Value for 'DummyBizo' should be Purged=true", processingInfoPerTable[DummyBizoSchema.Constants.TableName].Purged);
			});

			OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(archiveSet, processingInfoPerTable, recordsPurged: false);

			CombineAssertions("Run 2: 'processingInfoPerTable' should contain one key/value pair: 'DummyBizo'", () =>
			{
				AssertEquals(1, processingInfoPerTable.Count);
				AssertCollectionContains("DummyBizo", DummyBizoSchema.Constants.TableName, processingInfoPerTable.Keys);
			});

			CombineAssertions("Run 2", () =>
			{
				AssertEquals("Value for 'DummyBizo' should be Count=2", 2, processingInfoPerTable[DummyBizoSchema.Constants.TableName].Count);
				Assert("Value for 'DummyBizo' should be Purged=true", processingInfoPerTable[DummyBizoSchema.Constants.TableName].Purged);
			});
		}

		public void TestAddOrUpdateRecordsProcessedCount_WhenUsingArchiveItemPurgeableValue()
		{
			var systemDescriptor = new DummyArchiveSystem();
			var stageDescriptor = new DummyArchiveStage();
			var mainArchiveableType = new ArchiveableType(stageDescriptor.MainArchivePKColumn, stageDescriptor.MainArchiveNKColumn);

			var mainArchiveItem = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), null, Guid.Empty, isReversed: false, DummyBizoSchema.Constants.Prefix);
			mainArchiveItem.Purgeable = false;

			var childArchiveItem1 = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), mainArchiveItem.PKColumn, mainArchiveItem.PK, isReversed: false, DummyDependentBizoSchema.Constants.Prefix);
			var childArchiveItem2 = new ArchiveItem(mainArchiveableType.PKColumn, Guid.NewGuid(), mainArchiveItem.PKColumn, mainArchiveItem.PK, isReversed: false, DummyDependentBizoSchema.Constants.Prefix);
			childArchiveItem1.Purgeable = true;
			childArchiveItem2.Purgeable = false;

			var archiveSet = new TestArchiveSet(systemDescriptor, stageDescriptor.Name, Guid.Empty, mainArchiveItem, childItems: new List<IArchiveItem>() { childArchiveItem1, childArchiveItem2 });

			var processingInfoPerTable = new ConcurrentDictionary<string, ITableProcessingInfo>();

			AssertEquals("Precondition", 0, processingInfoPerTable.Count);

			OnArchiveSetProcessedHelpers.AddOrUpdateRecordsProcessedCount(archiveSet, processingInfoPerTable);

			CombineAssertions("'processingInfoPerTable' should contain two key/value pairs: 'DummyBizo' and 'DummyDependentBizo'.", () =>
			{
				AssertEquals(2, processingInfoPerTable.Count);
				AssertCollectionContains("DummyBizo", DummyBizoSchema.Constants.TableName, processingInfoPerTable.Keys);
				AssertCollectionContains("DummyDependentBizo", DummyDependentBizoSchema.Constants.TableName, processingInfoPerTable.Keys);
			});

			CombineAssertions(() =>
			{
				AssertEquals("Value for 'DummyBizo' should be Count=1", 1, processingInfoPerTable[DummyBizoSchema.Constants.TableName].Count);
				Assert("Value for 'DummyBizo' should be Purged=false", !processingInfoPerTable[DummyBizoSchema.Constants.TableName].Purged);

				AssertEquals("Value for 'DummyDependentBizo' should be Count=2", 2, processingInfoPerTable[DummyDependentBizoSchema.Constants.TableName].Count);
				Assert("Value for 'DummyDependentBizo' should be Purged=true", processingInfoPerTable[DummyDependentBizoSchema.Constants.TableName].Purged);
			});
		}
	}
}
