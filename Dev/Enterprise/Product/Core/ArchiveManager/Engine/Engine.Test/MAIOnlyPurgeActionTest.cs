using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.Registry.Business;

namespace Enterprise.ArchiveManager.Engine.Test
{
	internal class MAIOnlyPurgeActionTest : TestCaseWithFactory
	{
		public void TestMAIOnlyPurgeActionLogMessage()
		{
			using (SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dummyBusinessObject = Factory.NewWithValidTestData<DummyBusinessObject>();

				var systemDescriptor = new DummyMAIOnlySystemDescriptor();
				var archiveItem = new ArchiveItem(dummyBusinessObject.PKSchemaColumn, dummyBusinessObject.PK.ToGuid(),
					null, System.Guid.Empty, false, dummyBusinessObject.TablePrefix);

				var set = new TestArchiveSet(systemDescriptor, "Dummy Test Name", Guid.NewGuid(), archiveItem,
					dummyBusinessObject.Z0_Code);
				var logger = new TestArchiveLogger();
				var purgeAction = new MAIOnlyPurgeAction(set, logger);

				purgeAction.Execute();
				var log = $"Information|DMA|Deleting 1 records from {AutoDummyBizo.Schema.TableName}";

				CombineAssertions(() =>
				{
					AssertEquals(1, logger.ListOfMessages.Count);
					AssertCollectionContains($"Archive Logs should contain the following log message, but didn't: ",
						log, logger.ListOfMessages);
				});
			}
		}

		public void TestMAIOnlyArchiveSetThatLoadedNothing()
		{
			using (SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var schedule = new TestArchiveSchedule();
				var systemDescriptor = new DummyMAIOnlySystemDescriptor();
				var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-1), 1, ZDateTime.Now, isVerboseLog: false,
					shouldIncludeDeclarations: false);
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
				var logger = new TestArchiveLogger();

				var dummyBusinessObject = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummyBusinessObject.Z0_Date = ZDateTime.Now;

				Factory.Save();

				var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
				archiveStage.BeginRun(config, schedule, logger);

				var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();

				archiveSet.Load(logger);

				AssertEquals("No items are loaded", 0, archiveSet.GetArchiveItems().Count);

				var purgeAction = new MAIOnlyPurgeAction(archiveSet, logger);

				purgeAction.Execute();

				AssertEquals("Nothing should be logged since an empty set.", 0, logger.ListOfMessages.Count);
			}
		}

		public void TestPurgeAction_MultipleItemsLoadedIntoSet()
		{
			using (SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var dummyBizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummyBizo1.Z0_Date = ZDateTime.Now.AddYears(-1);
				var dummyBizo2 = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummyBizo2.Z0_Date = ZDateTime.Now.AddYears(-2);
				var dummyBizo3 = Factory.NewWithValidTestData<DummyBusinessObject>();
				dummyBizo3.Z0_Date = ZDateTime.Now.AddYears(-3);

				Factory.Save();

				var archiveItem = new ArchiveItem(dummyBizo1.PKSchemaColumn, dummyBizo1.PK.ToGuid(),
					null, System.Guid.Empty, false, dummyBizo1.TablePrefix);

				var systemDescriptor = new DummyMAIOnlySystemDescriptor();
				var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false,
					shouldIncludeDeclarations: false);
				var logger = new TestArchiveLogger();
				var schedule = new TestArchiveSchedule();
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
				var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);

				archiveStage.BeginRun(config, schedule, logger);

				var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
				archiveSet.Load(logger);
				var archiveItems = archiveSet.GetArchiveItems();

				AssertEquals("All bizos should be loaded", 3, archiveItems.Count);

				var purgeAction = new MAIOnlyPurgeAction(archiveSet, logger);

				purgeAction.Execute();

				var log = $"Information|DMA|Deleting 3 records from {AutoDummyBizo.Schema.TableName}";

				AssertContainsExactElementsInAnyOrder([log], logger.ListOfMessages);
			}
		}
	}
}



