using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ArchiveManager.Engine.Test
{
	public class MAIOnlyArchiveSetTest : TestCaseWithFactory
	{
		public void TestLoad_SingleArchiveableItemIntoMAI()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			archiveSet.Load(logger);
			var mainArchiveItem = archiveSet.GetArchiveItem(dummyBizo.PK.ToGuid());

			AssertNotNull("GetArchiveItem from Archive Set", mainArchiveItem);
			AssertEquals(dummyBizo.PK, mainArchiveItem.PK);
		}

		public void TestRetrieveWatermarkParamater()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			archiveSet.Load(logger);

			var watermark = schedule.GetWatermark(archiveSet.StageName);
			AssertWatermark(dummyBizo, watermark);
		}

		public void TestLoad_MainArchiveableItemsIntoMAI()
		{
			var dummyBizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo1.Z0_Date = ZDateTime.Now.AddYears(-1);
			var dummyBizo2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo2.Z0_Date = ZDateTime.Now.AddYears(-2);
			var dummyBizo3 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo3.Z0_Date = ZDateTime.Now.AddYears(-3);

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			archiveSet.Load(logger);
			var archiveItems = archiveSet.GetArchiveItems();

			CombineAssertions("All bizos should be loaded", () =>
			{
				AssertEquals(3, archiveItems.Count);
				AssertEquals(dummyBizo3.PK, archiveItems[0].PK);
				AssertEquals(dummyBizo2.PK, archiveItems[1].PK);
				AssertEquals(dummyBizo1.PK, archiveItems[2].PK);
			});
		}

		public void TestLoad_MainArchiveableItemsIntoMAI_SameDateAndSameNK()
		{
			var dummyBizo1 = Factory.New<DummyBusinessObject>();
			dummyBizo1.Z0_Date = new ZDate(2009, 1, 15);
			dummyBizo1.Z0_Code = "AA";

			var dummyBizo2 = Factory.New<DummyBusinessObject>();
			dummyBizo2.Z0_Date = new ZDate(2009, 1, 15);
			dummyBizo1.Z0_Code = "AA";

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			archiveSet.Load(logger);

			var archiveItems = archiveSet.GetArchiveItems();
			AssertEquals("All bizos should be loaded", 2, archiveItems.Count);
			AssertWatermark(dummyBizo2, schedule.GetWatermark(archiveSet.StageName));
		}

		public void TestLoad_MainArchiveableItemsIntoMAI_SameDate()
		{
			var dummyBizo1 = Factory.New<DummyBusinessObject>();
			dummyBizo1.Z0_Code = "1111";
			dummyBizo1.Z0_Date = new ZDate(2009, 1,  1);

			var dummyBizo2 = Factory.New<DummyBusinessObject>();
			dummyBizo2.Z0_Code = "2222";
			dummyBizo2.Z0_Date = new ZDate(2009, 1, 1);

			var dummyBizo3 = Factory.New<DummyBusinessObject>();
			dummyBizo3.Z0_Code = "3333";
			dummyBizo3.Z0_Date = new ZDate(2009, 1, 1);

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			archiveSet.Load(logger);

			var archiveItems = archiveSet.GetArchiveItems();
			AssertEquals("All bizos should be loaded", 3, archiveItems.Count);
			AssertWatermark(dummyBizo3, schedule.GetWatermark(archiveSet.StageName));
		}

		public void TestLoad_MainArchiveableItemsIntoMAI_SameDateDifferentTime()
		{
			var notLoaded = Factory.New<DummyBusinessObject>();
			notLoaded.Z0_Date = ZDateTime.UtcNow.AddHours(-2);

			var loaded = Factory.New<DummyBusinessObject>();
			loaded.Z0_Date = ZDateTime.UtcNow;

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.UtcNow, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			schedule.SetWatermark(archiveSet.StageName, new ArchiveWatermark { WatermarkDate = ZDateTime.UtcNow.AddHours(-1) });
			archiveSet.Load(logger);

			var archiveItems = archiveSet.GetArchiveItems();
			AssertEquals("All bizos should be loaded", 1, archiveItems.Count);
			AssertWatermark(loaded, schedule.GetWatermark(archiveSet.StageName));
		}

		public void TestLoad_MainArchiveableItemsIntoMAI_TestWatermark()
		{
			var loaded1 = Factory.New<DummyBusinessObject>();
			loaded1.Z0_Date = new ZDate(2009, 1, 1);

			var loaded2 = Factory.New<DummyBusinessObject>();
			loaded2.Z0_Date = new ZDate(2008, 1, 1);

			var notLoaded = Factory.New<DummyBusinessObject>();
			notLoaded.Z0_Date = new ZDate(2007, 1, 1);

			Factory.Save();

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			schedule.SetWatermark(archiveSet.StageName, new ArchiveWatermark { WatermarkDate = new ZDateTime(2008, 1, 1) });
			archiveSet.Load(logger);

			var archiveItems = archiveSet.GetArchiveItems();
			var watermark = schedule.GetWatermark(archiveSet.StageName);
			AssertEquals("Bizos after this watermark date should be loaded ", 2, archiveItems.Count);
			AssertWatermark(loaded1, watermark);
		}

		public void TestLoad_MultipleMainArchiveableItemsIntoMAI()
		{
			var notloadedBizo1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			notloadedBizo1.Z0_Date = ZDateTime.Now.AddYears(-1);
			var loadedBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			loadedBizo.Z0_Date = ZDateTime.Now.AddYears(-2);
			var notloadedBizo2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			notloadedBizo2.Z0_Date = ZDateTime.Now.AddYears(-4);

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now.AddYears(-2), 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			schedule.SetWatermark(archiveSet.StageName, new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-3) });
			archiveSet.Load(logger);

			var archiveItems = archiveSet.GetArchiveItems();
			AssertEquals("All bizos should be loaded", 1, archiveItems.Count);
			AssertWatermark(loadedBizo, schedule.GetWatermark(archiveSet.StageName));
		}

		public void TestLoad_NoInvalidItemsAddedToMAI()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(1);

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault();
			archiveSet.Load(logger);

			var archiveItems = archiveSet.GetArchiveItems();
			Assert("No items should be loaded", !archiveItems.Any());
		}

		public void TestMAIArchiveStageIsCancellableByToken()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			Factory.Save();

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			var token = tokenSource.Token;
			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, logger, schedule, token);

			Assert("Cancellation log should be present", logger.ListOfMessages.Any(log => log.Contains($"{archiveStage.Name} was stopped by a cancellation request.")));
			AssertNotNull("Nothing should have been archived", new BusinessObjectFactory().LoadTop1<DummyBusinessObject>(new ZQuery()));
		}

		public void TestMAIArchiveStageStopsWhenOutOfTime()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			Factory.Save();

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 2, ZDateTime.UtcNow.AddMinutes(-10), isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, logger, schedule, new CancellationToken());

			Assert("Out of time log should be present", logger.ListOfMessages.Any(log => log.Contains($"Max Run Duration reached for {systemDescriptor.Name}")));
			AssertNotNull("Nothing should have been archived", new BusinessObjectFactory().LoadTop1<DummyBusinessObject>(new ZQuery()));
		}

		public void TestDelete_FromMAIArchiveSet_ItemNoLongerExists()
		{
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);

			Factory.Save();

			AssertEquals("Precondition: records initially exist", 1, Factory.GetDatabaseCount(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.PK, dummyBizo.PK)));

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, logger, schedule, new CancellationToken());

			AssertEquals("records should no longer exist", 0, Factory.GetDatabaseCount(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.PK, dummyBizo.PK)));
		}

		public void TestUpdateWatermarkForSkippedMAIOnlyArchiveSet_CorrectlyUpdatesWatermark()
		{
			ErrorReporter.Clear();
			var dummyBizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = new ZDate(2009, 1, 13);

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			archiveStage.BeginRun(config, schedule, logger);

			Factory.Save();

			var archiveSet = ((MAIOnlyArchiveSet)archiveStage.GetNextArchiveSet(schedule, archiveStage).FirstOrDefault());
			archiveSet.Factory = GetFactoryMock();
			archiveSet.Load(logger);

			Assert("ErrorReporter should have reported a lock request timeout error", ErrorReporter.LastExceptionReported.Message.Contains("Lock request timeout exceeded"));
			AssertWatermark(dummyBizo, archiveSet.DateRangeEndWatermark);
			ErrorReporter.Clear();
		}

		void AssertWatermark(DummyBusinessObject dummyBizo, IArchiveWatermark actualWatermark)
		{
			CombineAssertions("Watermark should be updated, but was not", () =>
			{
				AssertEquals("Watermark date is incorrect", dummyBizo.Z0_Date, actualWatermark.WatermarkDate);
				AssertEquals("Watermark PK is incorrect", dummyBizo.PK, actualWatermark.WatermarkPK);
				AssertEquals("Watermark NK is incorrect", dummyBizo.Z0_Code, actualWatermark.WatermarkNK);
			});
		}

		BusinessObjectFactory GetFactoryMock()
		{
			var factoryMock = new Mock<BusinessObjectFactory>();

			factoryMock
				.Setup(f => f.Load(It.IsAny<Type>(), It.IsAny<ZQuery>()))
				.Callback((Type type, ZQuery query) =>
				{
					throw SqlExceptionBuilder.CreateSqlException(1222, "Lock request timeout exceeded");
				});
			return factoryMock.Object;
		}
	}
}
