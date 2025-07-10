using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Engine.Test.ArchiveStages
{
	[UseSnapshotProtection]
	public class ArchiveStageTest : TestCaseWithFactory
	{
		public void TestArchiveStageBatchSize()
		{
			var system = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, true);
			var stageDescriptor = system.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, null);

			AssertEquals(SystemDataRegistry.Instance.BatchSizeControl.Value, archiveStage.BatchSize);

			SystemDataRegistry.Instance.BatchSizeControl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals(50, archiveStage.BatchSize);
		}

		public void TestJobShipmentGatewayIsHandledByCanBeHandledNextRun()
		{
			var ex = new Exception("The DELETE statement conflicted with the REFERENCE constraint bla bla bla JobShipmentGateway_JSG_JS_Shipment_FK2_JobShipment_RRR_120N");

			Assert("If JobShipmentGateway BizO is created during missing eDocs generation stage - it will be handled during next run", ArchiveStage.CanBeHandledNextRun(ex));
		}

		public void TestContainerPenaltyIsHandledByCanBeHandledNextRun()
		{
			var ex = new Exception("The DELETE statement conflicted with the REFERENCE constraint bla bla bla JobContainerPenalty_CPY_JC_Container_FK2_JobContainer_RRR_120N");

			Assert("If JobContainerPenalty BizO is created during missing eDocs generation stage - it will be handled during next run", ArchiveStage.CanBeHandledNextRun(ex));
		}

		public void TestJobPickupDeliveryConfirmIsHandledByCanBeHandledNextRun()
		{
			var ex = new Exception("The DELETE statement conflicted with the REFERENCE constraint bla bla bla JobPickupDeliveryConfirm_EU_JC_FK2_JobContainer_RRR_120N");

			Assert("If JobPickupDeliveryConfirm BizO is created during missing eDocs generation stage - it will be handled during next run", ArchiveStage.CanBeHandledNextRun(ex));
		}

		public void TestDeadLockIsHandledByCanBeHandledNextRun()
		{
			var ex = new Exception("Transaction (Process ID 73) was deadlocked on lock resources with another process and has been chosen as the deadlock victim");

			Assert("If Dead Lock found during purging stage - it will be handled during next run", ArchiveStage.DeadlockedFoundCanBeHandledNextRun(ex));
		}

		public void TestLoadArchiveSetBatchTimeout()
		{
			AssertEquals(SystemDataRegistry.Instance.LoadArchiveSetBatchTimeout.Value, ArchiveStage.LoadArchiveSetBatchTimeout);

			SystemDataRegistry.Instance.LoadArchiveSetBatchTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 50);
			AssertEquals(50, ArchiveStage.LoadArchiveSetBatchTimeout);
		}

		public void TestLogMessageContainsExceptionDataInfo()
		{
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var mainArchiveItem = new ArchiveItem(DummyBizoSchema.PK, Guid.NewGuid(), null, Guid.Empty, false, DummyBizoSchema.PK.ColumnPrefix);

			var archiveStage = new ArchiveStage(new DummySimpleArchiveStageDescriptorWithPreparationActionException(), new DummySimpleArchiveSystemDescriptor());
			archiveStage.BeginRun(config, schedule, logger);
			var archiveSet = new TestArchiveSet(new DummySimpleArchiveSystemDescriptor(), archiveStage.Name, schedule.SchedulePK, mainArchiveItem);
			archiveStage.ArchiveToImages(archiveSet);

			AssertArrayEqualsByElements("Error is not logged to logger", [], logger.ListOfMessages.ToArray());
			AssertContains($"Error should be reported to ErrorReporter", "The preparation action 'DummyArchivePreparationActionWithException' failed. Info about the ArchiveItem", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGetArchiveItemCodeWithFallback()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo.Z0_Code = "D1";
			factory.Save();

			var schedule = new TestArchiveSchedule();
			var mainArchiveItem = new ArchiveItem(DummyBizoSchema.PK, dummyBizo.PK.ToGuid(), null, Guid.Empty, false, DummyBizoSchema.PK.ColumnPrefix);
			var archiveStage = new ArchiveStage(new DummySimpleArchiveStageDescriptorWithPreparationActionException(), new DummySimpleArchiveSystemDescriptor());
			var testSet = new TestArchiveSet(new DummySimpleArchiveSystemDescriptor(), archiveStage.Name, schedule.SchedulePK, mainArchiveItem);

			testSet.MainArchiveItemNK = null;
			var getArchiveItemCode_WhenNkIsNull = archiveStage.GetArchiveItemCodeWithFallback(testSet);

			testSet.MainArchiveItemNK = "";
			var getArchiveItemCode_WhenNkIsEmptyString = archiveStage.GetArchiveItemCodeWithFallback(testSet);

			CombineAssertions("Postcondition", () =>
			{
				AssertEquals("ArchiveItemCode is PK when NK is null", dummyBizo.PK.ToString(), getArchiveItemCode_WhenNkIsNull);
				AssertEquals("ArchiveItemCode is PK when NK is empty string ('')", dummyBizo.PK.ToString(), getArchiveItemCode_WhenNkIsEmptyString);
			});
		}
	}

	[UseSnapshotProtection]
	public class ArchiveStageTestNonTransactional : TestCase
	{
		public void TestLogLoadedArchiveSetDetails_AddsCorrectLogMessage()
		{
			SetUpArchiveDirectory();
			var factory = new BusinessObjectFactory();

			var dummyBusinessObject = factory.New<DummyBusinessObject>();
			dummyBusinessObject.Z0_Code = "D8669";
			dummyBusinessObject.Z0_Date = new ZDate(2009, 1, 13);

			factory.Save();

			var system = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 2, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stage = system.GetArchiveStages(config).FirstOrDefault();
			var logger = new TestArchiveLogger();
			var schedule = factory.New<ArchiveScheduleTask>();

			factory.Save();

			_ = stage.ExecuteStage(new ArchiveSystem(system), stage, config, logger, schedule, new CancellationToken());

			var logMessage = $"Information|{system.Code}|Loaded {dummyBusinessObject.TableName} '{dummyBusinessObject.Z0_Code}' and 0 related records";
			Assert($"ArchiveLogger should contain the following message, but did not: '{logMessage}'", logger.ListOfMessages.Any(m => m.Contains(logMessage)));
		}

		public void TestDeadlock_ReportsCorrectMessageToKibanaAndLogsWarning()
		{
			SetUpArchiveDirectory();
			var factory = new BusinessObjectFactory();

			var dummyBusinessObject = factory.New<DummyBusinessObject>();
			dummyBusinessObject.Z0_Code = "D8669";
			dummyBusinessObject.Z0_Date = new ZDate(2009, 1, 13);

			var logger = new TestArchiveLogger();
			var schedule = factory.New<ArchiveScheduleTask>();
			var system = new DummySimpleDeadlockArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 2, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stage = system.GetArchiveStages(config).FirstOrDefault();
			factory.Save();

			_ = stage.ExecuteStage(new ArchiveSystem(system), stage, config, logger, schedule, new CancellationToken());

			var usageCollectorTestHelper = new UsageCollectorTestHelper(factory);
			var deadlockReports = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManagerDeadlock);
			AssertEquals("Only one deadlock should have been reported.", 1, deadlockReports.Length);

			var deadlockReport = deadlockReports[0];
			var databaseName = deadlockReport.UsageProperties.Value<string>(UsageProperties.DatabaseName);
			var archiveSystem = deadlockReport.UsageProperties.Value<string>(UsageProperties.ArchiveSystemName);
			var archiveStageName = deadlockReport.UsageProperties.Value<string>(UsageProperties.ArchiveStageName);
			var includeDeclarations = deadlockReport.UsageProperties.Value<bool>(UsageProperties.IncludeCustomsJobsInArchiving);
			var archiveActionName = deadlockReport.UsageProperties.Value<string>(UsageProperties.ArchiveActionName);
			var archiveNK = deadlockReport.UsageProperties.Value<string>(UsageProperties.ArchiveItemNK);

			CombineAssertions("The contents of the deadlock report should be correct", () =>
			{
				AssertEquals("Database Name", Db.DatabaseName, databaseName);
				AssertEquals("Archive System Name", system.Name, archiveSystem);
				AssertEquals("Archive Stage Name", stage.Name, archiveStageName);
				AssertEquals("Include Declarations", config.ShouldIncludeDeclarations, includeDeclarations);
				AssertEquals("Archive Action Name", "DeadlockingArchiveAction", archiveActionName);
				AssertEquals("Archive NK", dummyBusinessObject.Z0_Code, archiveNK);
			});

			Assert("Deadlocks are correctly reported as a warning", logger.ListOfMessages.Any(l => l.Contains("Warning|DMD|This archive set will not be removed from the database during this run as there is a Deadlock.")));
		}

		public void TestLoggingForArchiveSetThatLoadedNothing()
		{
			SetUpArchiveDirectory();
			var factory = new BusinessObjectFactory();

			var alreadyLoadedItem = factory.NewWithValidTestData<DummyBusinessObject>();

			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Code = "D1";
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);

			var dummyChildBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyChildBizo.Z0_Code = "D2";
			dummyChildBizo.Z0_Date = ZDateTime.Now;
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;

			var systemDescriptor = new DummySimpleCyclicArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var mainArchiveableType = new ArchiveableType(stageDescriptor.MainArchivePKColumn, stageDescriptor.MainArchiveNKColumn);
			var mainArchiveableTypeFilter = stageDescriptor.GetMainArchiveableFilter(config);
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			factory.Save();

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, logger, schedule, new CancellationToken());

			var logMessage = $"because one of its related records did not meet archiving criteria.";

			Assert($"ArchiveLogger should contain the following message, but did not: '{logMessage}'", logger.ListOfMessages.Select(m => m.Contains(logMessage)).Any(b => b));
		}

		public void TestLoggingForArchiveSetWhenItemIsAlreadyLoadedParallel()
		{
			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 50))
			{
				SetUpArchiveDirectory();
				var factory = new BusinessObjectFactory();

				var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
				dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
				dummyBizo.Z0_Code = "D1";

				var dummyChildBizo = factory.NewWithValidTestData<DummyBusinessObject>();
				dummyChildBizo.Z0_Code = "D2";
				dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;
				dummyChildBizo.Z0_Date = ZDateTime.Now.AddYears(-10);

				dummyBizo.Z0_FK_Code = dummyChildBizo.Z0_Code;

				var systemDescriptor = new DummySimpleCyclicArchiveSystemDescriptor();
				var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
				var logger = new TestArchiveLogger();
				var schedule = new TestArchiveSchedule();
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
				var mainArchiveableType = new ArchiveableType(stageDescriptor.MainArchivePKColumn, stageDescriptor.MainArchiveNKColumn);
				var mainArchiveableTypeFilter = stageDescriptor.GetMainArchiveableFilter(config);
				var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
				archiveStage.BeginRun(config, schedule, logger);

				factory.Save();

				_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, logger, schedule, new CancellationToken());

				Assert($"ArchiveLogger should say something about an item being loaded as part of another archive set, but did not.",
					logger.ListOfMessages.Select(m => m.Contains("because it is already loaded as part of another archive set.")).Any(b => b));
			}
		}

		public void TestNoMoreThanOneArchiveDriverCanRunSimultaneously()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo.Z0_Code = "D1";

			var stageAndSchedule = CreateArchiveStageAndBeginRun();
			var archiveStage = stageAndSchedule.Item2;
			var archiveSchedule = stageAndSchedule.Item1;

			factory.Save();

			var threads = new List<Thread>();
			var numberOfThreads = 10;
			var allLoadedArchiveSets = new ConcurrentBag<IArchiveSet>();

			for (var i = 0; i < numberOfThreads; i++)
			{
				threads.Add(new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var result = archiveStage.GetNextArchiveSet(null, archiveSchedule, archiveStage);

						foreach (var set in result)
						{
							allLoadedArchiveSets.Add(set);
						}
					}
				}));
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			AssertEquals("Exactly 1 record should have been loaded", 1, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());
			AssertEquals("Despite multiple threads running simultaneously, the threads shouldn't have loaded anything in common", 1,
				allLoadedArchiveSets.Count);
			CombineAssertions("The archive set correctly loaded PK and NK", () =>
			{
				AssertEquals("PK", dummyBizo.PK, allLoadedArchiveSets.FirstOrDefault().MainArchiveItem.PK);
				AssertEquals("NK", dummyBizo.Z0_Code, allLoadedArchiveSets.FirstOrDefault().MainArchiveItemNK.Trim());
			});
		}

		public void TestDoesNotLoadRecordsTwice()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo1 = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo1.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo1.Z0_Code = "D1";
			var dummyBizo2 = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo2.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo2.Z0_Code = "D2";

			var stageAndSchedule = CreateArchiveStageAndBeginRun();
			var archiveStage = stageAndSchedule.Item2;
			var archiveSchedule = stageAndSchedule.Item1;

			factory.Save();

			_ = archiveStage.GetNextArchiveSet(null, archiveSchedule, archiveStage);

			AssertEquals("Prerequisite: loads into the queue correctly", 2, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());

			archiveStage.GetNextArchiveSet(null, archiveSchedule, archiveStage);

			AssertEquals("Does not load any more records", 2, ArchiveTableHelper.GetNumberOfItemsInMainArchiveQueue());
		}

		public void TestTwoArchiveStagesDoNotLoadTheSameArchiveSets()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo1 = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo1.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo1.Z0_Code = "D1";
			var dummyBizo2 = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo2.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo2.Z0_Code = "D2";

			factory.Save();

			var stageAndSchedule1 = CreateArchiveStageAndBeginRun();
			var schedule1 = stageAndSchedule1.Item1;
			var stage1 = stageAndSchedule1.Item2;
			var stageAndSchedule2 = CreateArchiveStageAndBeginRun();
			var schedule2 = stageAndSchedule2.Item1;
			var stage2 = stageAndSchedule2.Item2;

			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var sets1 = stage1.GetNextArchiveSet(null, schedule1, stage1);
				var sets2 = stage2.GetNextArchiveSet(null, schedule2, stage2);

				AssertNotNull("First stage has loaded something", sets1.FirstOrDefault());
				AssertNotNull("Second stage has loaded something", sets2.FirstOrDefault());
				AssertNotEquals("Does not load the same pk", sets1.FirstOrDefault().MainArchiveItem.PK, sets2.FirstOrDefault().MainArchiveItem.PK);
			}
		}

		public void TestGetNextArchiveSet_WhenConfigHasNegativeRunDuration()
		{
			var factory = new BusinessObjectFactory();

			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo.Z0_Code = "D1";

			factory.Save();

			var config = new ArchiveConfiguration(ZDateTime.UtcNow, -1, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = factory.New<ArchiveScheduleTask>();
			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var stage = new ArchiveStage(stageDescriptor, systemDescriptor);

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Max Run Duration must be set to -1", -1, config.MaxRunDurationInMinutes);
				AssertEquals("Dummy Bizo should exist in the database", 1, factory.GetDatabaseCount(typeof(DummyBusinessObject)));
			});

			stage.BeginRun(config, schedule, logger);
			stage.ExecuteStage(new ArchiveSystem(systemDescriptor), stage, config, logger, schedule, new CancellationToken());

			CombineAssertions("Results after calling ExecuteStage()", () =>
			{
				AssertEquals("Dummy Bizo should remain in the database", 1, factory.GetDatabaseCount(typeof(DummyBusinessObject)));
				Assert("A log about max run duration being reached should exist",logger.ListOfMessages.Any(log => log.Contains("Max Run Duration reached for")));
			});
		}

		public void TestArchiveRelationshipsTableIsCreatedForBothCustomsAndWithoutCustoms()
		{
			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var configWithCustoms = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: true);
			var configWithoutCustoms = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(configWithCustoms).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			var factory = new BusinessObjectFactory();

			factory.Save();

			archiveStage.BeginRun(configWithCustoms, schedule, logger);
			archiveStage.BeginRun(configWithoutCustoms, schedule, logger);

			var relationshipNameWithCustoms = $"ArchiveRelationship{ArchiveManager.SystemSpecificSuffix}{archiveStage.Name + "WithDeclarations"}".Replace(" ", "");
			var relationshipNameWithoutCustoms = $"ArchiveRelationship{ArchiveManager.SystemSpecificSuffix}{archiveStage.Name + "WithoutDeclarations"}".Replace(" ", "");

			CombineAssertions("Relationships table is created for both with and without customs", () =>
			{
				AssertEquals(1, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM SYS.TABLES WHERE NAME = '{relationshipNameWithCustoms}'"));
				AssertEquals(1, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM SYS.TABLES WHERE NAME = '{relationshipNameWithoutCustoms}'"));
			});
		}

		[UseSnapshotProtection]
		public void TestDoesNotTouchRowsFromAnotherArchiveSchedule()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo1 = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo1.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo1.Z0_Code = "D1";

			factory.Save();

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule1 = factory.New<ArchiveScheduleTask>();
			schedule1.MaxRunDurationInMinutes = 1;
			schedule1.S5_ScheduleType = TestArchiveManagerConstants.Codes.DMS;
			schedule1.S5_ScheduleDescription = "Description";

			factory.Save();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage1 = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage1.BeginRun(config, schedule1, logger);
			var archiveSet1 = archiveStage1.GetNextArchiveSet(null, schedule1, archiveStage1).FirstOrDefault();

			AssertNotNull("First set loads successfully", archiveSet1);

			var archiveStage2 = new ArchiveStage(stageDescriptor, systemDescriptor);
			var schedule2 = factory.New<ArchiveScheduleTask>();
			schedule2.MaxRunDurationInMinutes = 1;
			schedule2.S5_ScheduleType = TestArchiveManagerConstants.Codes.DMS;
			schedule2.S5_ScheduleDescription = "Description";

			factory.Save();

			archiveStage2.BeginRun(config, schedule2, logger);
			var archiveSet2 = archiveStage2.GetNextArchiveSet(null, schedule2, archiveStage2).FirstOrDefault();

			AssertNull("Not loaded if already loaded/loading elsewhere", archiveSet2);
		}

		[TestDate(2020, 10 ,10 ,0, 0, 0)]
		public void TestLoadArchiveSet()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo.Z0_Code = "D1";

			var dummyChildBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyChildBizo.Z0_Code = "D2";
			dummyChildBizo.Z0_Date = ZDateTime.Now;
			dummyChildBizo.Z0_FK_Code = dummyBizo.Z0_Code;

			var stageAndSchedule = CreateArchiveStageAndBeginRun();
			var stage = stageAndSchedule.Item2;
			var schedule = stageAndSchedule.Item1;

			factory.Save();

			var set = stage.GetNextArchiveSet(null, schedule, stage);
			(set.FirstOrDefault() as ArchiveSet).Load(new TestArchiveLogger());

			AssertEquals("Count of records loaded is correct", 1, ArchiveTableHelper.GetNumberOfItemsInRelatedRecordsArchiveQueue());

			var getPKOfItemInMainArchiveQueueSQL = "select top 1 AIM_PK from dbo.ArchiveMainItemQueue";
			var guidOfItemInMainArchiveQueue = Guid.Empty;

			using (var reader = Db.Connection.Command(getPKOfItemInMainArchiveQueueSQL).ExecuteReader())
			{
				while (reader.Read())
				{
					guidOfItemInMainArchiveQueue = reader.GetGuid(0);
				}
			} 

			var queryRelatedQueueSQL = "select * from dbo.ArchiveRelatedItemQueue";

			using (var reader = Db.Connection.Command(queryRelatedQueueSQL).ExecuteReader())
			{
				while (reader.Read())
				{
					var loadedID = reader.GetGuid(1);
					var loadedTableCode = reader.GetString(2);
					var parentID = reader.GetGuid(3);
					var parentTableCode = reader.GetString(4);
					var typeName = reader.GetString(5);
					var parentTypeName = reader.GetString(6);
					var isReversed = reader.GetBoolean(7);
					var aim_mainItemFK = reader.GetGuid(8);
					var systemCreateUser = reader.GetString(10);
					var systemLastEditUser = reader.GetString(12);

					CombineAssertions("All values are correct", () =>
					{
						AssertEquals("LoadedID", dummyChildBizo.PK, new ZGuid(loadedID));
						AssertEquals("LoadedTableCode", DummyBizoSchema.Constants.Prefix, loadedTableCode);
						AssertEquals("ParentID", dummyBizo.PK, parentID);
						AssertEquals("ParentTableCode", DummyBizoSchema.Constants.Prefix, parentTableCode);
						AssertEquals("TypeName", "DummyChild", typeName);
						AssertEquals("ParentTypeName", DummyBizoSchema.Constants.TableName, parentTypeName);
						Assert("IsReversed", !isReversed);
						AssertEquals("AIM_MainItemFK", guidOfItemInMainArchiveQueue, aim_mainItemFK);
						AssertEquals("SystemCreateUser", "~BP", systemCreateUser);
						AssertEquals("SystemLastEditUser", "~BP", systemLastEditUser);
					});
				}
			}
		}

		public void TestDoesNotHangForeverIfCheckForOverlapLockIsTaken()
		{
			var factory = new BusinessObjectFactory();
			var dummyBusinessObject = factory.New<DummyBusinessObject>();
			dummyBusinessObject.Z0_Code = "D8669";
			dummyBusinessObject.Z0_Date = new ZDate(2009, 1, 13);

			var system = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 2, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stage = system.GetArchiveStages(config).FirstOrDefault();
			var logger = new TestArchiveLogger();
			var schedule = factory.New<ArchiveScheduleTask>();
			(stage as ArchiveStage).TotalWaitingTimeSpanForOverlapLock = TimeSpan.FromSeconds(5);

			factory.Save();

			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var getLockResult = conn.TryGetLock("ArchiveManagerCheckForOverlapLock", out var sqlLock);
				CombineAssertions("Lock should be taken", () =>
				{
					Assert("Get lock result should be true", getLockResult);
					AssertNotNull("Lock should not be null", sqlLock);
				});

				using (sqlLock)
				{
					_ = stage.ExecuteStage(new ArchiveSystem(system), stage, config, logger, schedule, new CancellationToken());
					Assert("Should contain message that we timed out", logger.ListOfMessages.
						Any(log => log.Contains("Archive Manager has detected heavy lock contention")));
				}
			}
		}

		public void TestCancellationTokenIsRespondedTo()
		{
			SetUpArchiveDirectory();
			var factory = new BusinessObjectFactory();
			var dummyBusinessObject = factory.New<DummyBusinessObject>();
			dummyBusinessObject.Z0_Code = "D8669";
			dummyBusinessObject.Z0_Date = new ZDate(2009, 1, 13);

			var system = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 2, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var stage = system.GetArchiveStages(config).FirstOrDefault();
			var logger = new TestArchiveLogger();
			var schedule = factory.New<ArchiveScheduleTask>();

			factory.Save();

			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			var token = tokenSource.Token;
			_ = stage.ExecuteStage(new ArchiveSystem(system), stage, config, logger, schedule, token);
			Assert("Cancellation log should be present", logger.ListOfMessages.Any(log => log.Contains($"{stage.Name} was stopped by a cancellation request.")));
			AssertNotNull("Dummy business object should not be deleted", new BusinessObjectFactory().LoadTop1<DummyBusinessObject>(new ZQuery()));
		}

		public void TestRunOutOfTime()
		{
			SetUpArchiveDirectory();
			var factory = new BusinessObjectFactory();
			var dummyBusinessObject = factory.New<DummyBusinessObject>();
			dummyBusinessObject.Z0_Code = "D8669";
			dummyBusinessObject.Z0_Date = new ZDate(2009, 1, 13);

			var system = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 2, ZDateTime.UtcNow.AddMinutes(-10), false, shouldIncludeDeclarations: false);
			var stage = system.GetArchiveStages(config).FirstOrDefault();
			var logger = new TestArchiveLogger();
			var schedule = factory.New<ArchiveScheduleTask>();

			factory.Save();

			_ = stage.ExecuteStage(new ArchiveSystem(system), stage, config, logger, schedule, new CancellationToken());
			Assert("Out of time log should be present", logger.ListOfMessages.Any(log => log.Contains($"Max Run Duration reached for {system.Name}")));
			AssertNotNull("Dummy business object should not be deleted", new BusinessObjectFactory().LoadTop1<DummyBusinessObject>(new ZQuery()));
		}

		public void TestGetNextWaitingTimeForOverlapLock()
		{
			var veryShortTimeSpan = TimeSpan.FromMilliseconds(1);
			var firstJumpTimeSpan = TimeSpan.FromSeconds(1);
			var firstPieceTimeSpan = TimeSpan.FromSeconds(5);
			var secondJumpTimeSpan = TimeSpan.FromSeconds(10);
			var secondPieceTimeSpan = TimeSpan.FromSeconds(30);
			var thirdJumpTimeSpan = TimeSpan.FromSeconds(45);
			var veryLongTimeSpan = TimeSpan.FromSeconds(50);

			CombineAssertions("Piecewise function generates the correct output", () =>
			{
				AssertEquals(TimeSpan.FromMilliseconds(50), ArchiveStage.GetTimeToWaitForOverlapLock(veryShortTimeSpan));
				AssertEquals(TimeSpan.FromMilliseconds(300), ArchiveStage.GetTimeToWaitForOverlapLock(firstJumpTimeSpan));
				AssertEquals(TimeSpan.FromMilliseconds(300), ArchiveStage.GetTimeToWaitForOverlapLock(firstPieceTimeSpan));
				AssertEquals(TimeSpan.FromMilliseconds(1500), ArchiveStage.GetTimeToWaitForOverlapLock(secondJumpTimeSpan));
				AssertEquals(TimeSpan.FromMilliseconds(1500), ArchiveStage.GetTimeToWaitForOverlapLock(secondPieceTimeSpan));
				AssertEquals(TimeSpan.FromMilliseconds(200), ArchiveStage.GetTimeToWaitForOverlapLock(thirdJumpTimeSpan));
				AssertEquals(TimeSpan.FromMilliseconds(200), ArchiveStage.GetTimeToWaitForOverlapLock(veryLongTimeSpan));
			});
		}

		(IArchiveSchedule, ArchiveStage) CreateArchiveStageAndBeginRun()
		{
			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			return (schedule, archiveStage);
		}

		[TestDate(2020, 10, 10, 0, 0, 0)]
		public void TestLoadArchiveSet_WhenMainArchivableFilterIsGreaterThan4000Characters()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001";

			var jobHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			jobHeader.JH_JobNum = "T00001";
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_ParentID = shipment.PK;

			var systemDescriptor = new OPSArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			factory.Save();

			var set = archiveStage.GetNextArchiveSet(null, schedule, archiveStage);
			var sql = "1=1";

			while (sql.Length <= 4000 - set.FirstOrDefault().MainArchiveableTypeFilter.ToString().Length)
			{
				sql += " AND 1=1";
			}

			_ = set.FirstOrDefault().MainArchiveableTypeFilter.AddFilterAndZSQLParameterCollection(sql, null);

			AssertGreaterThan("Precondition: MainArchiveableFilter length is greater than 4000 characters", set.FirstOrDefault().MainArchiveableTypeFilter.ParameterisedText.LiteralTextSql.Length, 4000);

			_ = set.FirstOrDefault().Load(logger); 

			CombineAssertions(() =>
			{
				Assert("Logs shouldn't contain any errors", !logger.ListOfMessages.Exists(log => log.Contains("Error")));
				AssertEquals("Count of records loaded is correct", 2, ArchiveTableHelper.GetNumberOfItemsInRelatedRecordsArchiveQueue());
			});
		}

		public void TestExecuteStage_WhenDatabaseUpgradedException_DoesNotOpenConnection()
		{
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false);
			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var stageDescriptor = new DummySimpleArchiveSystemDescriptor.DummySimpleArchiveStageDescriptor();
			AssertEquals("PRE: IsStageUsingTempTables means EndRun will hit the db to drop the temp table", true, stageDescriptor.IsStageUsingTempTables);
			var stage = new ArchiveStageForTest(stageDescriptor, systemDescriptor);
			stage.NextArchiveSetAction = () =>
			{
				RegistryItemDictionary.Instance.PurgeAll();
				Db.Connection.CloseConnection();
				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;
				throw new DatabaseUpgradedException();
			};
			AssertExceptionThrown<DatabaseUpgradedException>(() => stage.ExecuteStage(new ArchiveSystem(systemDescriptor), stage, config, logger, schedule, new CancellationToken()));
			AssertEquals(ConnectionState.Closed, Db.Connection.State);
			AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestDoesNotHangForeverIfRunDriverLockIsTaken()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-10);
			dummyBizo.Z0_Code = "D1";

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-7), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var stage = new ArchiveStage(stageDescriptor, systemDescriptor);
			stage.BeginRun(config, schedule, logger);
			stage.TotalWaitingTimeSpanForDriverLock = TimeSpan.FromSeconds(5);

			factory.Save();

			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				var obtainLockResult = conn.TryGetLock("ArchiveManagerRunDriverLock",
					TimeSpan.FromMilliseconds(0), out var sqlLock);
				using (sqlLock)
				{
					Assert("Lock was obtained", obtainLockResult);
					var sets = stage.GetNextArchiveSet(null, schedule, stage);

					AssertEquals("Nothing was loaded because we timed out", 0, sets.Count());
					Assert("Message was logged correctly",
						logger.ListOfMessages.Exists(s => s.Contains("Archive Manager has detected heavy lock contention")));
				}
			}
		}

		public void TestTimeToWaitForDriverLockTest()
		{
			var veryShortTimeSpan = TimeSpan.FromMilliseconds(1);
			var firstJumpTimeSpan = TimeSpan.FromSeconds(1);
			var firstPieceTimeSpan = TimeSpan.FromSeconds(5);
			var secondJumpTimeSpan = TimeSpan.FromMinutes(1);
			var secondPieceTimeSpan = TimeSpan.FromMinutes(2);
			var thirdJumpTimeSpan = TimeSpan.FromMinutes(5);
			var veryLongTimeSpan = TimeSpan.FromMinutes(6);

			CombineAssertions("Piecewise function generates the correct output", () =>
			{
				AssertEquals(TimeSpan.FromMilliseconds(100), ArchiveStage.GetTimeToWaitForDriverLock(veryShortTimeSpan));
				AssertEquals(TimeSpan.FromSeconds(3), ArchiveStage.GetTimeToWaitForDriverLock(firstJumpTimeSpan));
				AssertEquals(TimeSpan.FromSeconds(3), ArchiveStage.GetTimeToWaitForDriverLock(firstPieceTimeSpan));
				AssertEquals(TimeSpan.FromSeconds(10), ArchiveStage.GetTimeToWaitForDriverLock(secondJumpTimeSpan));
				AssertEquals(TimeSpan.FromSeconds(10), ArchiveStage.GetTimeToWaitForDriverLock(secondPieceTimeSpan));
				AssertEquals(TimeSpan.FromSeconds(1), ArchiveStage.GetTimeToWaitForDriverLock(thirdJumpTimeSpan));
				AssertEquals(TimeSpan.FromSeconds(1), ArchiveStage.GetTimeToWaitForDriverLock(veryLongTimeSpan));
			});
		}

		public void TestRecordInMainQueueNotFoundDoesNotStopProcessingOfFurtherSets()
		{
			SetUpArchiveDirectory();
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-3);

			var otherDummy = factory.NewWithValidTestData<DummyBusinessObject>();
			otherDummy.Z0_Date = ZDateTime.Now.AddYears(-1);

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			factory.Save();

			var archiveSets = archiveStage.GetNextArchiveSet(null, schedule, archiveStage);

			using (var cmd = Db.Connection.Command("DELETE FROM dbo.ArchiveMainItemQueue WHERE AIM_ParentID = @DummyPK"))
			{
				_ = cmd.AddParameter("@DummyPK", SqlDbType.UniqueIdentifier, dummyBizo.PK.ToGuid());
				_ = cmd.ExecuteNonQuery();
			}

			foreach (var set in archiveSets)
			{
				archiveStage.ExecuteForArchiveSet(new ArchiveSystem(systemDescriptor), archiveStage, config, logger, set);
			}

			AssertEquals("Error should have been reported", "ArchiveManager.ArchiveQueueMainRecordNotFound", ErrorReporter.LastKeyReported);
			Assert("Nothing was logged as skipped", !logger.ListOfMessages.Any(l => l.Contains("Skipped")));

			var newFactory = new BusinessObjectFactory();

			CombineAssertions("Appropriate items are deleted", () =>
			{
				AssertNull("Second bizo is successfully deleted", newFactory.Load<DummyBusinessObject>(otherDummy.PK));
				AssertNotNull("First bizo is not deleted", newFactory.Load<DummyBusinessObject>(dummyBizo.PK));
			});

			ErrorReporter.Clear();
		}

		public void TestThrowExceptionOnlyOnceInGetNextArchiveSet()
		{
			SetUpArchiveDirectory();

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStageThatThrowsExceptionForTest(stageDescriptor, systemDescriptor);
			archiveStage.NumberOfTimesToThrow = 1;
			archiveStage.BeginRun(config, schedule, logger);
			AssertNoExceptionThrown(() => { _ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, logger, schedule, new CancellationToken()); });
		}

		public void TestThrowExceptionManyTimesInGetNextArchiveSet()
		{
			SetUpArchiveDirectory();

			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStageThatThrowsExceptionForTest(stageDescriptor, systemDescriptor);
			archiveStage.NumberOfTimesToThrow = 4;
			archiveStage.BeginRun(config, schedule, logger);
			archiveStage.SleepSpanBeforeGetNextArchiveSetRetry = TimeSpan.FromSeconds(1);
			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, logger, schedule, new CancellationToken());
			AssertContains("Expected Exception was rethrown and reported", "Test Exception", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		class ArchiveStageForTest : ArchiveStage
		{
			public ArchiveStageForTest(IArchiveStageDescriptor descriptor, IArchiveSystemDescriptor systemDescriptor)
				: base(descriptor, systemDescriptor)
			{
			}

			public Action NextArchiveSetAction { get; set; }

			public override IEnumerable<IArchiveSet> GetNextArchiveSet(IArchiveWatermark watermark, IArchiveSchedule schedule, IArchiveStage stage)
			{
				NextArchiveSetAction?.Invoke();
				return base.GetNextArchiveSet(watermark, schedule, stage);
			}
		}

		void SetUpArchiveDirectory()
		{
			DeleteArchiveDirectory();
			DummyArchiveStorage.ArchiveDirectory = Path.Combine(Env.TempPath, "DummyArchive");
			_ = Directory.CreateDirectory(DummyArchiveStorage.ArchiveDirectory);
		}

		void DeleteArchiveDirectory()
		{
			if (Directory.Exists(DummyArchiveStorage.ArchiveDirectory))
			{
				Directory.Delete(DummyArchiveStorage.ArchiveDirectory, true);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteArchiveDirectory();
		}
	}
}
