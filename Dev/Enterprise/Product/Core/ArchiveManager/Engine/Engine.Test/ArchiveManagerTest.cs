using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Engine.Test
{
	public class ArchiveManagerTest : TestCaseWithFactory
	{
		public void TestStageNameIsUniqueWithinEachArchiveSystem()
		{
			var loader = new ArchiveSystemDescriptorLoader();
			var result = loader.Load();
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);

			foreach (var system in result.ToList())
			{
				var stageNameList = new List<string>();
				foreach (var stage in system.GetArchiveStageDescriptors(config))
				{
					if (stageNameList.Contains(stage.Name))
					{
						Fail("stage name: '" + stage.Name + "' should be unique in " + system.Name);
					}
					else
					{
						stageNameList.Add(stage.Name);
					}
				}
			}

			Assert(true);
		}

		public void TestArchiveSystemWithErrorInLoad()
		{
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();

			var dummyToArchive1 = Factory.New<DummyBusinessObject>();
			dummyToArchive1.Z0_Code = "1111";
			dummyToArchive1.Z0_Date = new ZDate(2009, 1, 13);

			var dummyToArchive2 = Factory.New<DummyBusinessObject>();
			dummyToArchive2.Z0_Code = "2222";
			dummyToArchive2.Z0_Date = new ZDate(2009, 1, 14);

			var dummyToArchive3 = Factory.New<DummyBusinessObject>();
			dummyToArchive3.Z0_Code = "3333";
			dummyToArchive3.Z0_Date = new ZDate(2009, 1, 15);

			Factory.Save();

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 15), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);

			manager.Run(TestArchiveManagerConstants.Codes.DME, config, logger, schedule, new CancellationToken());

			AssertContains("Error should be reported to ErrorReporter", "Error encountered during archiving. Run aborted.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSystemSpecificSuffix()
			=> AssertEquals($"{ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode}{ObjectFactory.Get<IProductRegistration>().Key.ServerCode}", ArchiveManager.SystemSpecificSuffix);

		public void TestRun_WhenNoArchiveStages()
		{
			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(ZDateTime.UtcNow, 1, ZDateTime.UtcNow, false, false);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();

			manager.Run(TestArchiveManagerConstants.Codes.DNS, config, logger, schedule, new CancellationToken());

			AssertCollectionContains("No Archive Stages Log", $"Information|{TestArchiveManagerConstants.Codes.DNS}|There are no archive stages to execute with the current configuration.", logger.ListOfMessages);
		}

		protected override void SetUp()
		{
			base.SetUp();

			DummyArchiveStorage.ArchiveDirectory = Path.Combine(Env.TempPath, "DummyArchive");

			DeleteArchiveDirectory();
			_ = Directory.CreateDirectory(DummyArchiveStorage.ArchiveDirectory);
			TestCaseHelper.ClearTable(DummyDependantBusinessObject.Schema.TableName);
			TestCaseHelper.ClearTable(DummyBusinessObject.Schema.TableName);

			DummyComplexArchiveSystemWithErrorDescriptor.DummyComplexArchiveStageDescriptorWithError.RaiseErrorEnabled = true;
		}

		protected override void TearDown()
		{
			base.TearDown();

			DummyComplexArchiveSystemWithErrorDescriptor.DummyComplexArchiveStageDescriptorWithError.RaiseErrorEnabled = false;
			DeleteArchiveDirectory();
		}

		void DeleteArchiveDirectory()
		{
			if (Directory.Exists(DummyArchiveStorage.ArchiveDirectory))
			{
				Directory.Delete(DummyArchiveStorage.ArchiveDirectory, true);
			}
		}
	}

	[UseSnapshotProtection]
	public class ArchiveManagerTestNonTransactional : TestCase
	{
		readonly BusinessObjectFactory Factory = new BusinessObjectFactory();

		public void TestArchivingSimpleDummyRecords()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			var schedule = new TestArchiveSchedule();

			var dummyToArchive1 = Factory.New<DummyBusinessObject>();
			dummyToArchive1.Z0_Code = "1111";
			dummyToArchive1.Z0_Date = new ZDate(2009, 1, 13);

			var dummyToArchive2 = Factory.New<DummyBusinessObject>();
			dummyToArchive2.Z0_Code = "2222";
			dummyToArchive2.Z0_Date = new ZDate(2009, 1, 14);

			var dummyToArchive3 = Factory.New<DummyBusinessObject>();
			dummyToArchive3.Z0_Code = "3333";
			dummyToArchive3.Z0_Date = new ZDate(2009, 1, 15);

			Factory.Save();

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);

			manager.Run(TestArchiveManagerConstants.Codes.DMS, config, logger, schedule, new CancellationToken());

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Configuration Parameters:");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Archiving Records on or Before: 14-Jan-2009");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Max Run Duration: 10 minutes");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Verbose Logging: No");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Incl. Customs: No");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Executing: Simple Stage");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded next batch of 2 DummyBizo Time taken:");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded DummyBizo '1111 ' and 0 related records Time taken:");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Generating detailed job reports to archive");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Deleting DummyBizo '1111 ' and 0 related records Time taken:");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded DummyBizo '2222 ' and 0 related records Time taken:");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Generating detailed job reports to archive");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Deleting DummyBizo '2222 ' and 0 related records Time taken:");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded next batch of 0 DummyBizo Time taken:");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Time taken to load 2 Archive Set(s) of DummyBizo record(s) in 1 batch(es):");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Time taken to archive 2 DummyBizo record(s) and their related record(s):");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Clean up completed");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Completed archiving records");

			AssertEquals("Message Count", 21, logger.ListOfMessages.Count);

			AssertNotExistsInDb(dummyToArchive1, "dummyToArchive1");
			AssertNotExistsInDb(dummyToArchive2, "dummyToArchive2");
			AssertExistsInDb(dummyToArchive3, "dummyToArchive3");

			Assert("LastArchiveDirectory: " + DummyArchiveStorage.ArchiveDirectory + " should exist", Directory.Exists(DummyArchiveStorage.ArchiveDirectory));

			var indexFile = Path.Combine(DummyArchiveStorage.ArchiveDirectory, "index.txt");
			Assert("index.txt should exist", File.Exists(indexFile));

			var indexLines = File.ReadAllLines(indexFile);
			AssertMessage(indexLines, "File: ");
			AssertMessage(indexLines, "Code: 1111");
			AssertMessage(indexLines, "");
			AssertMessage(indexLines, "File: ");
			AssertMessage(indexLines, "Code: 2222");

			AssertEquals("indexFile line count", 5, indexLines.Length);
			AssertFileExists(indexLines);
		}

		// A.K:This test intermittently can get deadlocks if parallelism at ArchiveSet level is activated
		// I keep the test for regression testing, but disable parallelisation
		// I'm adding similar new test with parallelisation ON - TestArchivingSimpleInterrelatedSystemsWithParallealismEnabled()
		//
		// The deadloack happen on SQL level when two transactions block each other
		// http://blog.sqlgrease.com/do-you-have-locks-being-caused-by-unindexed-foreign-keys/
		//
		// The deadlock can be recreated by running two transactions with UPDATE (NullifyFKAction) and DELETE (PurgeAction)
		//
		//--Tran 1

		//begin tran
		//UPDATE dbo.DummyDependentBizo SET
		//	ZD1_Z0 = NULL
		//FROM
		//	DummyDependentBizo WITH(INDEX([PK_UX__ZD1_PK]), UPDLOCK)
		//WHERE 1=1
		//	AND ZD1_PK = 'BF31D5AD-CD81-4B12-9A14-6B9EC4B0926B'
		//	AND ZD1_Code = 'TRIG1'
		//	AND ZD1_Number = 800
		//	AND ZD1_NumberUnit is NULL
		//	AND ZD1_NumberUnitCode is NULL
		//	AND(ZD1_Z0 = 'F49FF21D-4B80-48E6-803E-2ED3F67A47AB' OR ZD1_Z0 is NULL);
		//		delete dbo.DummyBizo where Z0_PK = 'F49FF21D-4B80-48E6-803E-2ED3F67A47AB'

		//rollback
		public void TestArchivingSimpleInterrelatedSystems()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			var schedule = new TestArchiveSchedule();

			var dummyToArchive1 = Factory.New<DummyBusinessObject>();
			dummyToArchive1.Z0_Code = "1111";
			dummyToArchive1.Z0_Date = new ZDate(2009, 1, 13);

			var dummyToArchive2 = Factory.New<DummyBusinessObject>();
			dummyToArchive2.Z0_Code = "2222";
			dummyToArchive2.Z0_Date = new ZDate(2009, 1, 14);

			var dummyToArchive3 = Factory.New<DummyBusinessObject>();
			dummyToArchive3.Z0_Code = "3333";
			dummyToArchive3.Z0_Date = new ZDate(2009, 1, 15);

			var dummyToArchive4 = Factory.New<DummyBusinessObject>();
			dummyToArchive4.Z0_Code = "4444";
			dummyToArchive4.Z0_Date = new ZDate(2009, 1, 15);

			var dummyToArchive5 = Factory.New<DummyBusinessObject>();
			dummyToArchive5.Z0_Code = "5555";
			dummyToArchive5.Z0_Date = new ZDate(2009, 1, 16);

			var dummyDependent1 = Factory.New<DummyDependantBusinessObject>();
			dummyDependent1.ZD1_Number = 800;
			dummyDependent1.ZD1_Code = DummyAdditionalActionArchiveSystemDescriptor.DummyAdditionalArchiveStageDescriptor.TriggerCode + "1"; // special code to trigger related action
			dummyDependent1.ZD1_Z0 = dummyToArchive1.PK;

			var dummyDependent2 = Factory.New<DummyDependantBusinessObject>();
			dummyDependent2.ZD1_Number = 1200;
			dummyDependent2.ZD1_Code = DummyAdditionalActionArchiveSystemDescriptor.DummyAdditionalArchiveStageDescriptor.TriggerCode + "2"; // special code to trigger related action

			var dummyDependent3 = Factory.New<DummyDependantBusinessObject>();
			dummyDependent3.ZD1_Number = 1500;
			dummyDependent3.ZD1_Z0 = dummyToArchive3.PK;
			dummyDependent3.ZD1_Code = DummyAdditionalActionArchiveSystemDescriptor.DummyAdditionalArchiveStageDescriptor.TriggerCode + "3"; // special code to trigger related action

			var dummyDependent4 = Factory.New<DummyDependantBusinessObject>();
			dummyDependent4.ZD1_Number = 2000;
			dummyDependent4.ZD1_Z0 = dummyToArchive4.PK;
			dummyDependent4.ZD1_Code = DummyAdditionalActionArchiveSystemDescriptor.DummyAdditionalArchiveStageDescriptor.TriggerCode + "4"; // special code to trigger related action

			var dummyDependent41 = Factory.New<DummyDependantBusinessObject>();
			dummyDependent41.ZD1_Number = 666; //special one to cause a rollback
			dummyDependent41.ZD1_Z0 = dummyToArchive4.PK;
			dummyDependent41.ZD1_Code = DummyAdditionalActionArchiveSystemDescriptor.DummyAdditionalArchiveStageDescriptor.TriggerCode + "5"; // special code to trigger related action

			Factory.Save();

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());

			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 15), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);

			manager.Run(TestArchiveManagerConstants.Codes.DMR, config, logger, schedule, new CancellationToken());

			AssertNotExistsInDb(dummyToArchive1, "dummyToArchive1");
			AssertNotExistsInDb(dummyToArchive2, "dummyToArchive2");
			AssertNotExistsInDb(dummyToArchive3, "dummyToArchive3");

			AssertExistsInDb(dummyDependent1, "dummyDependent1");
			AssertExistsInDb(dummyDependent2, "dummyDependent2");
			AssertExistsInDb(dummyDependent3, "dummyDependent3");

			AssertExistsInDb(dummyToArchive4, "dummyToArchive4");
			AssertExistsInDb(dummyDependent4, "dummyDependent4");
			AssertExistsInDb(dummyDependent41, "dummyDependent41");
			var factory2 = new BusinessObjectFactory();
			var dummyDependent4Reloaded = factory2.Load<DummyDependantBusinessObject>(dummyDependent4.PK);
			AssertEquals(DummyDependentBizoSchema.ZD1_Z0.Name + " should not be changed as its rolled back.", dummyToArchive4.PK, dummyDependent4Reloaded.ZD1_Z0);

			AssertExistsInDb(dummyToArchive5, "dummyToArchive4");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Configuration Parameters:");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Archiving Records on or Before: 15-Jan-2009");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Max Run Duration: 10 minutes");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Verbose Logging: No");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Incl. Customs: No");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Executing: Simple Stage");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Loaded DummyBizo '1111 ' and 0 related records");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Generating detailed job reports to archive");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Nullifying ZD1_Z0 Foreign Key on DummyDependentBusinessObject");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Deleting DummyBizo '1111 ' and 0 related records");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Loaded DummyBizo '2222 ' and 0 related records");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Generating detailed job reports to archive");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Deleting DummyBizo '2222 ' and 0 related records");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Loaded DummyBizo '3333 ' and 0 related records");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Generating detailed job reports to archive");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Nullifying ZD1_Z0 Foreign Key on DummyDependentBusinessObject");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Deleting DummyBizo '3333 ' and 0 related records");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Loaded DummyBizo '4444 ' and 0 related records");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Generating detailed job reports to archive");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Nullifying ZD1_Z0 Foreign Key on DummyDependentBusinessObject");
			AssertMessage($"Error|{TestArchiveManagerConstants.Codes.DMR}|Failed to archive this set.");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Clean up completed");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMR}|Completed archiving records");
		}

		public void TestArchivingWithMultipleDeadlocksEnabled()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			var schedule = new TestArchiveSchedule();

			var dummyToArchive1 = Factory.New<DummyBusinessObject>();
			dummyToArchive1.Z0_Code = "1111";
			dummyToArchive1.Z0_Date = new ZDate(2009, 1, 13);

			Factory.Save();

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());

			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 15), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);

			manager.Run(TestArchiveManagerConstants.Codes.DMD, config, logger, schedule, new CancellationToken());

			var usageCollectorTestHelper = new UsageCollectorTestHelper(Factory);
			var deadlockReports = usageCollectorTestHelper.LoadUsageMessages(UsageFeatures.Codes.ArchiveManagerDeadlock);
			Assert("A deadlock should have been reported", deadlockReports.Length > 0);
		}

		public void TestHandlingPreparationActionFailure()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			var schedule = new TestArchiveSchedule();

			var dummyToArchive1 = Factory.New<DummyBusinessObject>();
			dummyToArchive1.Z0_Code = "1111";
			dummyToArchive1.Z0_Date = new ZDate(2009, 1, 13);

			var dummyToArchive2 = Factory.New<DummyBusinessObject>();
			dummyToArchive2.Z0_Code = "2222";
			dummyToArchive2.Z0_Date = new ZDate(2009, 1, 13);

			DeleteArchiveDirectory(); // cause of preparation failure.

			Factory.Save();

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());

			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);

			manager.Run(TestArchiveManagerConstants.Codes.DMS, config, logger, schedule, new CancellationToken());

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Configuration Parameters:");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Archiving Records on or Before: 14-Jan-2009");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Max Run Duration: 10 minutes");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Verbose Logging: No");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Incl. Customs: No");

			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Executing: Simple Stage");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded DummyBizo '1111 ' and 0 related records");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Generating detailed job reports to archive");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded DummyBizo '2222 ' and 0 related records ");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Generating detailed job reports to archive");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Clean up completed");
			AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Completed archiving records");

			Assert("LastArchiveDirectory: " + DummyArchiveStorage.ArchiveDirectory + " should NOT exist", !Directory.Exists(DummyArchiveStorage.ArchiveDirectory));

			AssertEquals(2, ErrorReporter.TotalErrorCount);
			AssertEquals("ArchiveManager.ArchiveStage.ArchiveToImagesException", ErrorReporter.LastKeyReported);
			AssertContains("Failed to archive this set containing MainArchiveItem=ArchiveItem(", ErrorReporter.LastMessageReported);
			AssertContains("The preparation action 'ArchiveToImageAction' failed.", ErrorReporter.LastMessageReported);
		}

		public void TestOPSWatermarkRemainsConsistent()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			const string errorMessage = "Archive watermark name should remain constant otherwise multiple watermarks will exist for a single schedule. If changing the watermark name, then a transformation to update or delete old watermarks will need to be done";

			var dummyToArchive = Factory.New<DummyBusinessObject>();
			dummyToArchive.Z0_Code = "1111";
			dummyToArchive.Z0_Date = new ZDate(2009, 1, 13);

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 13), 10, ZDateTime.UtcNow, true, shouldIncludeDeclarations: false);
			var schedule = Factory.New<ArchiveScheduleTask>();

			Factory.Save();

			manager.Run(TestArchiveManagerConstants.Codes.DMO, config, logger, schedule, new CancellationToken());

			var watermarks = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Owner, schedule.PK));
			AssertEquals(3, watermarks.Length);

			var expectedList = new List<ZString>()
			{
				"ArchiveWatermark|Operational Jobs Archive",
				"ArchiveWatermarkNK|Operational Jobs Archive",
				"ArchiveWatermarkPK|Operational Jobs Archive"
			};

			AssertContainsExactElementsInAnyOrder(errorMessage, expectedList, watermarks.Select(wm => wm.SD_Name));

			AssertNotExistsInDb(dummyToArchive, "dummyToArchive");
		}

		public void TestPDRWatermarkRemainsConsistent()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			const string errorMessage = "Archive watermark name should remain constant otherwise multiple watermarks will exist for a single schedule. If changing the watermark name, then a transformation to update or delete old watermarks will need to be done";

			var dummyToPurge = Factory.New<DummyBusinessObject>();
			dummyToPurge.Z0_Code = "1111";
			dummyToPurge.Z0_Date = new ZDate(2009, 1, 13);

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 13), 10, ZDateTime.UtcNow, true, shouldIncludeDeclarations: false);
			var schedule = Factory.New<ArchiveScheduleTask>();

			Factory.Save();

			manager.Run(TestArchiveManagerConstants.Codes.DMP, config, logger, schedule, new CancellationToken());
			var watermarks = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Owner, schedule.PK));

			AssertEquals(3, watermarks.Length);

			var expectedList = new List<ZString>()
			{
				"ArchiveWatermark|Purge Documents and Records",
				"ArchiveWatermarkNK|Purge Documents and Records",
				"ArchiveWatermarkPK|Purge Documents and Records"
			};

			AssertContainsExactElementsInAnyOrder(errorMessage, expectedList, watermarks.Select(wm => wm.SD_Name));
			AssertNotExistsInDb(dummyToPurge, "dummyToPurge");
		}

		public void TestRunStartTimeConfiguration()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			var schedule = new TestArchiveSchedule();

			var dummyToArchive1 = Factory.New<DummyBusinessObject>();
			dummyToArchive1.Z0_Code = "1111";
			dummyToArchive1.Z0_Date = new ZDate(2009, 1, 13);

			var dummyToArchive2 = Factory.New<DummyBusinessObject>();
			dummyToArchive2.Z0_Code = "2222";
			dummyToArchive2.Z0_Date = new ZDate(2009, 1, 14);

			var dummyToArchive3 = Factory.New<DummyBusinessObject>();
			dummyToArchive3.Z0_Code = "3333";
			dummyToArchive3.Z0_Date = new ZDate(2009, 1, 15);

			Factory.Save();

			DummySimpleArchiveSystemDescriptor.DummySimpleArchiveStageDescriptor.StallActionForSeconds = 6;
			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());

			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 0, ZDateTime.UtcNow.AddSeconds(5), false, shouldIncludeDeclarations: false);

			manager.Run(TestArchiveManagerConstants.Codes.DMS, config, logger, schedule, new CancellationToken());

			CombineAssertions("Regardless of delay, everything the driver loaded should be archived", delegate
			{
				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Configuration Parameters:");
				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Archiving Records on or Before: 14-Jan-2009");
				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Max Run Duration: 0 minutes");
				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Verbose Logging: No");
				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Incl. Customs: No");

				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded DummyBizo '1111 ' and 0 related records");
				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Generating detailed job reports to archive");
				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Deleting DummyBizo '1111 ' and 0 related records");

				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Executing: Simple Stage");

				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Max Run Duration reached for Dummy Simple Archive System");

				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Clean up completed");
				AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMS}|Completed archiving records");

				AssertNotExistsInDb(dummyToArchive1, "dummyToArchive1");
				AssertNotExistsInDb(dummyToArchive2, "dummyToArchive2");
				AssertExistsInDb(dummyToArchive3, "dummyToArchive3");

				Assert("LastArchiveDirectory: " + DummyArchiveStorage.ArchiveDirectory + " should exist", Directory.Exists(DummyArchiveStorage.ArchiveDirectory));

				var indexFile = Path.Combine(DummyArchiveStorage.ArchiveDirectory, "index.txt");
				Assert("index.txt should exist", File.Exists(indexFile));

				var indexLines = File.ReadAllLines(indexFile);
				AssertMessage(indexLines, 0, "File: ");
				AssertMessage(indexLines, 1, "Code: 1111");
				AssertMessage(indexLines, 2, "");
				AssertMessage(indexLines, 3, "File: ");
				AssertMessage(indexLines, 4, "Code: 2222");

				AssertEquals("indexFile line count", 5, indexLines.Length);
				AssertFileExists(indexLines);

				var watermark = schedule.GetWatermark("Simple Stage");

				AssertEquals("Watermark date", dummyToArchive2.Z0_Date, watermark.WatermarkDate);
				AssertEquals("Watermark NK", dummyToArchive2.Z0_Code.ToString(), watermark.WatermarkNK.TrimEnd(' '));
				AssertEquals("Watermark PK", dummyToArchive2.PK.ToGuid(), watermark.WatermarkPK);
			});
		}

		// A.K: This Test is not multi thread friendly
		// reason being - behaviour becomes non deterministic with multithreading
		// If 2222 is picked first - it'll load 3333 and archive it as a part of 2222 ArchiveSet
		// If 3333 is picked first - then it'll be archived by itself (and 2222 is being ignored for the current run)
		//
		// One would ask - why loading 3333 would load 2222, but not vice versa?
		// The answer is: there is relationships defined for Z0_AnotherNumber -> Z0_Number, but not vice versa
		//
		// relevant code is in C:\git\wtg\CargoWise\Dev\Enterprise\Product\Core\ArchiveManager\Engine\Engine.Test\DummyComplexArchiveSystem.cs : 74
		//
		// systemSetup.AddRelationship("DummyChild", DummyBizoSchema.Z0_AnotherNumber, DummyBizoSchema.Z0_Number.TableName, DummyBizoSchema.Z0_Number, true);

		// In single threaded environment if 3333 is archived before 2222, 2222 is archived in the same run
		// I'll keep the test for regression testing the way it is and turn the ArchiveSet parallelism off.
		// New test, which is multithreaded sibling of this one is addded below

		public void TestArchivingComplexDummyRecordsAndIsVerboseLogTrue()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			var schedule = new TestArchiveSchedule();

			try
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				CreateComplexSet('0', new ZDate(2009, 1, 13), out var dummy0, out var dummy0Child1, out var dummy0Child2, out var dummy0Child2Grandchild1, out var dummy0Child2Grandchild2, out var dummy0WithDependent1, out var dummy0Dependent1, out var dummy0Dependent2, out var dummy0Notes);
				CreateComplexSet('1', new ZDate(2009, 1, 14), out var dummy1, out var dummy1Child1, out var dummy1Child2, out var dummy1Child2Grandchild1, out var dummy1Child2Grandchild2, out var dummy1WithDependent1, out var dummy1Dependent1, out var dummy1Dependent2, out var dummy1Notes);
				CreateComplexSet('2', new ZDate(2009, 1, 15), out var dummy2, out var dummy2Child1, out var dummy2Child2, out var dummy2Child2Grandchild1, out var dummy2Child2Grandchild2, out var dummy2WithDependent1, out var dummy2Dependent1, out var dummy2Dependent2, out var dummy2Notes);
				CreateComplexSet('3', new ZDate(2009, 1, 15), out var dummy3, out var dummy3Child1, out var dummy3Child2, out var dummy3Child2Grandchild1, out var dummy3Child2Grandchild2, out var dummy3WithDependent1, out var dummy3Dependent1, out var dummy3Dependent2, out var dummy3Notes);

				dummy2Child1.Z0_AnotherNumber = dummy3.Z0_Number; // testing a recursive relationship that meets date criteria, so will be archived together

				CreateComplexSet('4', new ZDate(2009, 1, 16), out var dummy4, out var dummy4Child1, out var dummy4Child2, out var dummy4Child2Grandchild1, out var dummy4Child2Grandchild2, out var dummy4WithDependent1, out var dummy4Dependent1, out var dummy4Dependent2, out var dummy4Notes);

				dummy0Child1.Z0_AnotherNumber = dummy4.Z0_Number; // testing a recursive relationship that DOES NOT meet date criteria, so will all be ignored

				Factory.Save();

				var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());

				var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 15), 10, ZDateTime.UtcNow, true, shouldIncludeDeclarations: false);

				manager.Run(TestArchiveManagerConstants.Codes.DMC, config, logger, schedule, new CancellationToken());

				CombineAssertions(delegate
				{
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Registry Settings:");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Configuration Parameters:");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Archiving Records on or Before: 15-Jan-2009");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Max Run Duration: 10 minutes");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Verbose Logging: Yes");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Incl. Customs: No");

					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Executing: Complex Stage");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Loaded next batch of 4 DummyBizo");

					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Skipped DummyBizo with Code '00000', because one of its related records did not meet archiving criteria.");

					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Loaded DummyBizo '11111' and 15 related records\r\n" +
		"DummyBizo: 6\r\n" +
		"11111 - Desc 11111\r\n" +
		"1C1 - 11111 Child 1\r\n" +
		"1C2 - 11111 Child 2\r\n" +
		"D1 - Default\r\n" +
		"1C2G1 - 11111 Child 2 Grandchild 1\r\n" +
		"1C2G2 - 11111 Child 2 Grandchild 2\r\n" +
		"StmNote: 8\r\n" +
		"DummyDependentBizo: 2\r\n");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Generating detailed job reports to archive");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Deleting DummyBizo '11111' and 15 related records");

					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Loaded DummyBizo '22222' and 31 related records\r\n" +
		"DummyBizo: 12\r\n" +
		"22222 - Desc 22222\r\n" +
		"2C1 - 22222 Child 1\r\n" +
		"2C2 - 22222 Child 2\r\n" +
		"D2 - Default\r\n" +
		"2C2G1 - 22222 Child 2 Grandchild 1\r\n" +
		"2C2G2 - 22222 Child 2 Grandchild 2\r\n" +
		"33333 - Desc 33333\r\n" +
		"3C1 - 33333 Child 1\r\n" +
		"3C2 - 33333 Child 2\r\n" +
		"D3 - Default\r\n" +
		"3C2G1 - 33333 Child 2 Grandchild 1\r\n" +
		"3C2G2 - 33333 Child 2 Grandchild 2\r\n" +
		"StmNote: 16\r\n" +
		"DummyDependentBizo: 4\r\n");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Generating detailed job reports to archive");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Deleting DummyBizo '22222' and 31 related records");
					// previously, when we were loading ArchiveSet-s one by one, we would archive '33333' and '22222' together during '22222' archiving
					// and '33333' would be added to #IgnoredPKCache table when the set of related records for '22222' is loaded by StoredProcedure [dbo].[LoadArchiveSet]
					// so the record '33333' would not be loaded separately after '22222' is processed
					// now, when we are loading in batches, we already have '33333' in the enumerable and therefore will try to archive it, but silently exit the archiving
					// Reason being - it's already archived and LoadArchiveableBusinessObject(IArchiveItem item, BusinessObjectFactory factory) will return null.
					// I did consider double checking #IgnoredPKCache before trying to archive '33333', but it means a certain DB hit for every record in the batch, while cases when two records of the same type are child and parent are rare, I think
					// Therefore I prefer to load in batches AND if some of the records in the batch was already archived - we try to archive it, but since it's not there, we move on.
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Skipped DummyBizo with Code '33333', because it is already loaded as part of another archive set.");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Generating detailed job reports to archive");
					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Loaded next batch of 0 DummyBizo");

					AssertMessage($"Information|{TestArchiveManagerConstants.Codes.DMC}|Completed archiving records");

					AssertEquals("Info Count", 18, logger.ListOfMessages.Count);

					AssertExistsInDb(dummy0, "dummy0");
					AssertExistsInDb(dummy0Child1, "dummy0Child1");
					AssertExistsInDb(dummy0Child2, "dummy0Child2");
					AssertExistsInDb(dummy0Child2Grandchild1, "dummy0Child2Grandchild1");
					AssertExistsInDb(dummy0Child2Grandchild2, "dummy0Child2Grandchild2");
					AssertExistsInDb(dummy0WithDependent1, "dummy0WithDependent1");
					AssertExistsInDb(dummy0Dependent1, "dummy0Dependent1");
					AssertExistsInDb(dummy0Dependent2, "dummy0Dependent2");

					foreach (var note in dummy0Notes)
					{
						AssertExistsInDb(note, "dummy0 notes");
					}

					AssertNotExistsInDb(dummy1, "dummy1");
					AssertNotExistsInDb(dummy1Child1, "dummy1Child1");
					AssertNotExistsInDb(dummy1Child2, "dummy1Child2");
					AssertNotExistsInDb(dummy1Child2Grandchild1, "dummy1Child2Grandchild1");
					AssertNotExistsInDb(dummy1Child2Grandchild2, "dummy1Child2Grandchild2");
					AssertNotExistsInDb(dummy1WithDependent1, "dummy1WithDependent1");
					AssertNotExistsInDb(dummy1Dependent1, "dummy1Dependent1");
					AssertNotExistsInDb(dummy1Dependent2, "dummy1Dependent2");

					foreach (var note in dummy1Notes)
					{
						AssertNotExistsInDb(note, "dummy1 notes");
					}

					AssertNotExistsInDb(dummy2, "dummy2");
					AssertNotExistsInDb(dummy2Child1, "dummy2Child1");
					AssertNotExistsInDb(dummy2Child2, "dummy2Child2");
					AssertNotExistsInDb(dummy2Child2Grandchild1, "dummy2Child2Grandchild1");
					AssertNotExistsInDb(dummy2Child2Grandchild2, "dummy2Child2Grandchild2");
					AssertNotExistsInDb(dummy2WithDependent1, "dummy2WithDependent1");
					AssertNotExistsInDb(dummy2Dependent1, "dummy2Dependent1");
					AssertNotExistsInDb(dummy2Dependent2, "dummy2Dependent2");

					foreach (var note in dummy2Notes)
					{
						AssertNotExistsInDb(note, "dummy2 notes");
					}

					AssertNotExistsInDb(dummy3, "dummy3");
					AssertNotExistsInDb(dummy3Child1, "dummy3Child1");
					AssertNotExistsInDb(dummy3Child2, "dummy3Child2");
					AssertNotExistsInDb(dummy3Child2Grandchild1, "dummy3Child2Grandchild1");
					AssertNotExistsInDb(dummy3Child2Grandchild2, "dummy3Child2Grandchild2");
					AssertNotExistsInDb(dummy3WithDependent1, "dummy3WithDependent1");
					AssertNotExistsInDb(dummy3Dependent1, "dummy3Dependent1");
					AssertNotExistsInDb(dummy3Dependent2, "dummy3Dependent2");

					foreach (var note in dummy3Notes)
					{
						AssertNotExistsInDb(note, "dummy3 notes");
					}

					AssertExistsInDb(dummy4, "dummy4");
					AssertExistsInDb(dummy4Child1, "dummy4Child1");
					AssertExistsInDb(dummy4Child2, "dummy4Child2");
					AssertExistsInDb(dummy4Child2Grandchild1, "dummy4Child2Grandchild1");
					AssertExistsInDb(dummy4Child2Grandchild2, "dummy4Child2Grandchild2");
					AssertExistsInDb(dummy4WithDependent1, "dummy4WithDependent1");
					AssertExistsInDb(dummy4Dependent1, "dummy4Dependent1");
					AssertExistsInDb(dummy4Dependent2, "dummy4Dependent2");

					foreach (var note in dummy4Notes)
					{
						AssertExistsInDb(note, "dummy4 notes");
					}

					Assert("LastArchiveDirectory: " + DummyArchiveStorage.ArchiveDirectory + " should exist", Directory.Exists(DummyArchiveStorage.ArchiveDirectory));

					var indexFile = Path.Combine(DummyArchiveStorage.ArchiveDirectory, "index.txt");
					Assert("index.txt should exist", File.Exists(indexFile));

					var indexLines = File.ReadAllLines(indexFile);
					int index = 0;
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 11111");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 1C1");
					AssertMessage(indexLines, index++, "Parent Code: 11111");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 1C2");
					AssertMessage(indexLines, index++, "Parent Code: 11111");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D1");
					AssertMessage(indexLines, index++, "Parent Code: 11111");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D1C1");
					AssertMessage(indexLines, index++, "Parent Code: 11111");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D1C2");
					AssertMessage(indexLines, index++, "Parent Code: 11111");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 1C2G1");
					AssertMessage(indexLines, index++, "Parent Code: 11111");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 1C2G2");
					AssertMessage(indexLines, index++, "Parent Code: 11111");
					AssertMessage(indexLines, index++, "");

					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 2C1");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 2C2");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D2");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D2C1");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D2C2");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 2C2G1");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 2C2G2");
					AssertMessage(indexLines, index++, "Parent Code: 22222");

					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 33333");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "IsReversed: True");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 3C1");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 3C2");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D3");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D3C1");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: D3C2");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 3C2G1");
					AssertMessage(indexLines, index++, "Parent Code: 22222");
					AssertMessage(indexLines, index++, "File: ");
					AssertMessage(indexLines, index++, "Code: 3C2G2");
					AssertMessage(indexLines, index++, "Parent Code: 22222");

					AssertEquals("indexFile line count", index, indexLines.Length);

					AssertFileExists(indexLines);

					AssertNull("Watermark should be null", schedule.GetWatermark("Complex Stage"));
				});

				//rerun with water mark
				var watermark = new ArchiveWatermark { WatermarkDate = new ZDateTime(2009, 1, 15) };
				schedule.SetWatermark("Complex Stage", watermark);
				logger.ListOfMessages.Clear();

				manager.Run(TestArchiveManagerConstants.Codes.DMC, config, logger, schedule, new CancellationToken());

				CombineAssertions(delegate
				{
					int msgIndex = 0;
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Registry Settings:");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Configuration Parameters:");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Archiving Records on or Before: 15-Jan-2009");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Max Run Duration: 10 minutes");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Verbose Logging: Yes");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Incl. Customs: No");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Watermark date is '{watermark.WatermarkDate}'");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Executing: Complex Stage");
					// should not see "skipped" messages, as the watermark should cause the complete skip without loading each one to decide to skip.
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Loaded next batch of 0 DummyBizo");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Completed archiving records");
				});

				//rerun with water mark but use NK as tiebreaker
				watermark = new ArchiveWatermark { WatermarkDate = new ZDateTime(2009, 1, 15), WatermarkNK = "ZZ" };
				schedule.SetWatermark("Complex Stage", watermark);
				logger.ListOfMessages.Clear();

				manager.Run(TestArchiveManagerConstants.Codes.DMC, config, logger, schedule, new CancellationToken());

				CombineAssertions(delegate
				{
					int msgIndex = 0;
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Registry Settings:");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Configuration Parameters:");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Archiving Records on or Before: 15-Jan-2009");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Max Run Duration: 10 minutes");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Verbose Logging: Yes");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Incl. Customs: No");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Watermark date is '{watermark.WatermarkDate}'");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Executing: Complex Stage");
					// should not see "skipped" messages, as the watermark should cause the complete skip without loading each one to decide to skip.
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Loaded next batch of 0 DummyBizo");
					AssertMessage(msgIndex++, $"Information|{TestArchiveManagerConstants.Codes.DMC}|Completed archiving records");
				});
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		// A.K this test is not multi thread friendly, so I keep it for regression testing
		// but turn parallelisation off and to test the scenario when parallelism is ON - I'm adding its sibling below that is parallel safe
		public void TestErrorHandlingDuringDeleteFailure()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			var schedule = new TestArchiveSchedule();

			try
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				var dummyToArchive1 = Factory.New<DummyBusinessObject>();
				dummyToArchive1.Z0_Code = "1111";
				dummyToArchive1.Z0_Date = new ZDate(2009, 1, 13);

				var dummyToArchive2 = Factory.New<DummyBusinessObject>();
				dummyToArchive2.Z0_Code = "2222";
				dummyToArchive2.Z0_Date = new ZDate(2009, 1, 14);

				var dummyDependent1 = Factory.New<DummyDependantBusinessObject>();
				dummyDependent1.ZD1_Code = "D1";
				dummyDependent1.ZD1_Z0 = dummyToArchive1.PK; // to cause a delete problem

				Factory.Save();

				var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());

				var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);

				manager.Run(TestArchiveManagerConstants.Codes.DMS, config, logger, schedule, new CancellationToken()); // simple archive will only archive DummyBusinessObject, not DummyDependant, simulating cases where developer forgets to include some relationships

				int index = 3;
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Configuration Parameters:");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Archiving Records on or Before: 14-Jan-2009");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Max Run Duration: 10 minutes");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Verbose Logging: No");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Incl. Customs: No");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Executing: Simple Stage");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded next batch of 2 DummyBizo");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded DummyBizo", "and 0 related records");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Generating detailed job reports to archive");
				//AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Deleting DummyBizo '1111 ' and 0 related records");
#if NETFRAMEWORK
				AssertMessage(index++, $"Error|{TestArchiveManagerConstants.Codes.DMS}|Failed to archive this set.", "System.Data.SqlClient.SqlException (0x80131904): The DELETE statement conflicted with the REFERENCE constraint");
#else
				AssertMessage(index++, $"Error|{TestArchiveManagerConstants.Codes.DMS}|Failed to archive this set.", "Microsoft.Data.SqlClient.SqlException (0x80131904): The DELETE statement conflicted with the REFERENCE constraint \"DummyDependentBizo_ZD1_Z0_FK2_DummyBizo_RRR_120N\". The conflict occurred in database \"OdysseyDat\", table \"dbo.DummyDependentBizo\", column 'ZD1_Z0'.");
#endif
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded DummyBizo", "and 0 related records");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Generating detailed job reports to archive");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Deleting DummyBizo '2222 ' and 0 related records");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Loaded next batch of 0 DummyBizo");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Clean up completed");
				AssertMessage(index++, $"Information|{TestArchiveManagerConstants.Codes.DMS}|Completed archiving records");

				AssertEquals("message count", index, logger.ListOfMessages.Count);

				Assert("LastArchiveDirectory: " + DummyArchiveStorage.ArchiveDirectory + " should exist", Directory.Exists(DummyArchiveStorage.ArchiveDirectory));

				var indexFile = Path.Combine(DummyArchiveStorage.ArchiveDirectory, "index.txt");
				Assert("index.txt should exist as archive to image preparation action should still be completed and committed", File.Exists(indexFile));

				var indexLines = File.ReadAllLines(indexFile);
				index = 0;
				AssertMessage(indexLines, index++, "File: ");
				AssertMessage(indexLines, index++, "Code: 1111");
				AssertMessage(indexLines, index++, "");
				AssertMessage(indexLines, index++, "File: ");
				AssertMessage(indexLines, index++, "Code: 2222");
				AssertEquals("indexFile line count", index, indexLines.Length);
			}
			finally
			{
				SystemDataRegistry.Instance.IncludeTimeTakenInTheARCLogs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		public void TestExcludeAnItemFromPurge()
		{
			var logger = new TestArchiveLogger();
			LogMessages = logger.ListOfMessages;

			var schedule = new TestArchiveSchedule();

			CreateComplexSet('0', new ZDate(2009, 1, 13), out var dummy0, out var dummy0Child1, out var dummy0Child2, out var dummy0Child2Grandchild1, out var dummy0Child2Grandchild2, out var dummy0WithDependent1, out var dummy0Dependent1, out var dummy0Dependent2, out _);

			Factory.Save();

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());

			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 16), 10, ZDateTime.UtcNow.ToDateTime(), false, shouldIncludeDeclarations: false);
			manager.Run(TestArchiveManagerConstants.Codes.DEP, config, logger, schedule, new CancellationToken());

			AssertNotExistsInDb(dummy0, "dummy0");
			AssertNotExistsInDb(dummy0Child1, "dummy0Child1");
			AssertNotExistsInDb(dummy0Child2, "dummy0Child2");
			AssertExistsInDb(dummy0Child2Grandchild1, "dummy0Child2Grandchild1"); // not purged
			AssertNotExistsInDb(dummy0Child2Grandchild2, "dummy0Child2Grandchild2");
			AssertNotExistsInDb(dummy0WithDependent1, "dummy0WithDependent1");
			AssertNotExistsInDb(dummy0Dependent1, "dummy0Dependent1");
			AssertNotExistsInDb(dummy0Dependent2, "dummy0Dependent2");
		}

		public void TestArchiveLoggerContainsArchiveConfigurationParameters()
		{
			var archiveManager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(new ZDateTime(2015, 6, 15), 10, ZDateTime.UtcNow.ToDateTime(), false, shouldIncludeDeclarations: false);
			var descriptor = new DummySimpleArchiveSystemDescriptor();
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();

			archiveManager.Run(descriptor.Code, config, logger, schedule, new CancellationToken());

			Assert(logger.ListOfMessages.Any());

			CombineAssertions("Configuration logs did not contain the correct messages", () =>
			{
				AssertCollectionContains("Configuration Parameter Log", $"Information|{descriptor.Code}|Configuration Parameters:", logger.ListOfMessages);
				AssertCollectionContains("On or Before Log", $"Information|{descriptor.Code}|Archiving Records on or Before: 15-Jun-2015", logger.ListOfMessages);
				AssertCollectionContains("Max Run Duration Log", $"Information|{descriptor.Code}|Max Run Duration: 10 minutes", logger.ListOfMessages);
				AssertCollectionContains("Verbose Logging Log", $"Information|{descriptor.Code}|Verbose Logging: No", logger.ListOfMessages);
				AssertCollectionContains("Incl. Customs Log", $"Information|{descriptor.Code}|Incl. Customs: No", logger.ListOfMessages);
			});
		}

		public void TestArchiveLoggerContainsRegistrySettings()
		{
			var archiveManager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(new ZDateTime(2015, 6, 15), 1, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var descriptor = new DummySimpleArchiveSystemDescriptor();
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();

			archiveManager.Run(descriptor.Code, config, logger, schedule, new CancellationToken());

			CombineAssertions("Registry settings should be in the ArchiveLogger", () =>
			{
				Assert(logger.ListOfMessages.Any());
				AssertEquals($"Information|{descriptor.Code}|Registry Settings:", logger.ListOfMessages[0]);
				AssertEquals($"Information|{descriptor.Code}|On or Before Minimum: 7", logger.ListOfMessages[1]);
				AssertEquals($"Information|{descriptor.Code}|Set Batch Size for Archiving and Purging Operational Jobs: 50", logger.ListOfMessages[2]);
			});
		}

		public void TestArchiveLoggerContainsCorrectWatermarkDate()
		{
			var archiveManager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(new ZDateTime(2015, 6, 15), 10, ZDateTime.UtcNow.ToDateTime(), false, shouldIncludeDeclarations: false);
			var descriptor = new DummyMultiStageArchiveSystemDescriptor();
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageNameList = descriptor.GetArchiveStageDescriptors(config).ToList();

			var dateTime1 = new ZDateTime(2016, 11, 1);
			var watermark1 = new ArchiveWatermark { WatermarkDate = dateTime1 };
			var dateTime2 = new ZDateTime(2017, 10, 2);
			var watermark2 = new ArchiveWatermark { WatermarkDate = dateTime2 };

			schedule.SetWatermark(stageNameList[0].Name, null);
			schedule.SetWatermark(stageNameList[1].Name, watermark1);
			schedule.SetWatermark(stageNameList[2].Name, watermark2);

			archiveManager.Run(descriptor.Code, config, logger, schedule, new CancellationToken());

			var logLinesWithWatermarkDateIs = logger.ListOfMessages.Where(line => line.StartsWith($"Information|{descriptor.Code}|Watermark date is '")).ToList();

			CombineAssertions("Correct watermark date logs should be in the ArchiveLogger", () =>
			{
				AssertEquals("Precondition", 2, logLinesWithWatermarkDateIs.Count);

				AssertEquals($"Information|{descriptor.Code}|Watermark date is '{dateTime1}'", logLinesWithWatermarkDateIs[0]);
				AssertEquals($"Information|{descriptor.Code}|Watermark date is '{dateTime2}'", logLinesWithWatermarkDateIs[1]);
			});
		}

		public void TestArchiveManagerRespondsToCancellationToken()
		{
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();

			var dummyToArchive1 = Factory.New<DummyBusinessObject>();
			dummyToArchive1.Z0_Code = "1111";
			dummyToArchive1.Z0_Date = new ZDate(2009, 1, 13);

			Factory.Save();

			var manager = new ArchiveManager(new ArchiveSystemDescriptorLoader());
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 10, ZDateTime.UtcNow, false, shouldIncludeDeclarations: false);
			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			var token = tokenSource.Token;

			AssertNotNull("Precondition: dummy exists", new BusinessObjectFactory().LoadTop1<DummyBusinessObject>(new ZQuery()));

			manager.Run(TestArchiveManagerConstants.Codes.DMS, config, logger, schedule, token);

			AssertNotNull("Since we called cancel, dummy should still exist", new BusinessObjectFactory().LoadTop1<DummyBusinessObject>(new ZQuery()));
		}

		StmNote NewNote(BusinessObject parentBizO)
		{
			var note = Factory.New<StmNote>();
			note.ST_ParentID = parentBizO.PK;
			note.ST_Table = parentBizO.TableName;
			note.ST_NoteText = "test";
			return note;
		}

		void CreateComplexSet(char parentCodeChar, ZDate parentDate, out DummyBusinessObject dummyParent, out DummyChildBusinessObject dummyChild1, out DummyChildBusinessObject dummyChild2, out DummyChildBusinessObject dummyChild2Grandchild1, out DummyChildBusinessObject dummyChild2Grandchild2, out DummyWithDependentsBusinessObject dummyWithDependent1, out DummyDependantBusinessObject dummyDependent1, out DummyDependantBusinessObject dummyDependent2, out List<StmNote> stmNoteList)
		{
			stmNoteList = new List<StmNote>();

			dummyParent = Factory.New<DummyBusinessObject>();
			dummyParent.Z0_Code = string.Empty.PadRight(5, parentCodeChar);
			dummyParent.Z0_Date = parentDate;
			dummyParent.Z0_Description = "Desc ".PadRight(10, parentCodeChar);
			dummyParent.Z0_Number = parentCodeChar;
			stmNoteList.Add(NewNote(dummyParent));

			// part 1 - dependent tree
			dummyChild1 = Factory.New<DummyChildBusinessObject>();
			dummyChild1.Z0_Code = parentCodeChar + "C1";
			dummyChild1.Z0_Description = dummyParent.Z0_Code + " Child 1";
			dummyChild1.Z0_FK_Code = dummyParent.Z0_Code;
			dummyChild1.Z0_Date = parentDate;
			dummyChild1.Z0_AnotherNumber = 9999;
			stmNoteList.Add(NewNote(dummyChild1));

			dummyChild2 = Factory.New<DummyChildBusinessObject>();
			dummyChild2.Z0_Code = parentCodeChar + "C2";
			dummyChild2.Z0_Description = dummyParent.Z0_Code + " Child 2";
			dummyChild2.Z0_FK_Code = dummyParent.Z0_Code;
			dummyChild2.Z0_Date = parentDate;
			dummyChild2.Z0_AnotherNumber = 9999;
			stmNoteList.Add(NewNote(dummyChild2));

			dummyChild2Grandchild1 = Factory.New<DummyChildBusinessObject>();
			dummyChild2Grandchild1.Z0_Code = parentCodeChar + "C2G1";
			dummyChild2Grandchild1.Z0_Description = dummyParent.Z0_Code + " Child 2 Grandchild 1";
			dummyChild2Grandchild1.Z0_FK_Code = dummyChild2.Z0_Code;
			dummyChild2Grandchild1.Z0_AnotherNumber = 9999;
			stmNoteList.Add(NewNote(dummyChild2Grandchild1));

			dummyChild2Grandchild2 = Factory.New<DummyChildBusinessObject>();
			dummyChild2Grandchild2.Z0_Code = parentCodeChar + "C2G2";
			dummyChild2Grandchild2.Z0_Description = dummyParent.Z0_Code + " Child 2 Grandchild 2";
			dummyChild2Grandchild2.Z0_FK_Code = dummyChild2.Z0_Code;
			dummyChild2Grandchild2.Z0_AnotherNumber = 9999;
			stmNoteList.Add(NewNote(dummyChild2Grandchild2));

			// part 2 - other types of dependent relationships
			var dummyWithDependentCode = "D" + parentCodeChar;
			dummyWithDependent1 = Factory.New<DummyWithDependentsBusinessObject>();
			dummyWithDependent1.Z0_Guid = dummyParent.PK;
			dummyWithDependent1.Z0_Code = dummyWithDependentCode;
			dummyWithDependent1.Z0_AnotherNumber = 9999;
			stmNoteList.Add(NewNote(dummyWithDependent1));

			dummyDependent1 = dummyWithDependent1.Dependents.AddNew();
			dummyDependent1.ZD1_Code = dummyWithDependentCode + "C1";
			stmNoteList.Add(NewNote(dummyDependent1));

			dummyDependent2 = dummyWithDependent1.Dependents.AddNew();
			dummyDependent2.ZD1_Code = dummyWithDependentCode + "C2";
			stmNoteList.Add(NewNote(dummyDependent2));
		}

		protected override void SetUp()
		{
			base.SetUp();

			DummyArchiveStorage.ArchiveDirectory = Path.Combine(Env.TempPath, "DummyArchive");

			DeleteArchiveDirectory();
			_ = Directory.CreateDirectory(DummyArchiveStorage.ArchiveDirectory);
			TestCaseHelper.ClearTable(DummyDependantBusinessObject.Schema.TableName);
			TestCaseHelper.ClearTable(DummyBusinessObject.Schema.TableName);

			DummyComplexArchiveSystemWithErrorDescriptor.DummyComplexArchiveStageDescriptorWithError.RaiseErrorEnabled = true;
		}

		protected override void TearDown()
		{
			base.TearDown();

			DummyComplexArchiveSystemWithErrorDescriptor.DummyComplexArchiveStageDescriptorWithError.RaiseErrorEnabled = false;
			DeleteArchiveDirectory();
		}

		void DeleteArchiveDirectory()
		{
			if (Directory.Exists(DummyArchiveStorage.ArchiveDirectory))
			{
				Directory.Delete(DummyArchiveStorage.ArchiveDirectory, true);
			}
		}

		void AssertFileExists(string[] indexLines)
		{
			foreach (var line in indexLines)
			{
				if (line.StartsWith("File: "))
				{
					var archiveFile = Path.Combine(DummyArchiveStorage.ArchiveDirectory, line.Replace("File: ", ""));
					Assert("File exists: " + archiveFile, File.Exists(archiveFile));
				}
			}
		}

		void AssertMessage(string expectedMessage)
			=> AssertMessage(LogMessages.ToArray(), expectedMessage);

		void AssertMessage(string[] messages, string expectedMessage)
		{
			foreach (var message in messages)
			{
				if (message.Contains(expectedMessage))
				{
					return;
				}
			}

			Assert("Expected in logs: " + expectedMessage + "\nmessages:\n" + string.Join("\n", messages) + "\nlogMessages:\n" + string.Join("\n", LogMessages), false);
		}

		void AssertMessage(string[] messages, int lineNumber, params string[] expectedMessages)
		{
			foreach (var expectedMessage in expectedMessages)
			{
				AssertContains("Line " + lineNumber.ToString(), expectedMessage, messages.Length > lineNumber ? messages[lineNumber] : string.Empty);
			}
		}

		void AssertMessage(int lineNumber, params string[] expectedMessages)
			=> AssertMessage(LogMessages.ToArray(), lineNumber, expectedMessages);

		void AssertNotExistsInDb(BusinessObject bizO, string description)
		{
			var factory = new BusinessObjectFactory();
			var bizOLoaded = factory.Load(bizO.GetType(), bizO.PK);
			AssertNull(description + " should NOT exist" + "\nlogMessages:\n" + string.Join("\n", LogMessages), bizOLoaded);
		}

		void AssertExistsInDb(BusinessObject bizO, string description)
		{
			var factory = new BusinessObjectFactory();
			var bizOLoaded = factory.Load(bizO.GetType(), bizO.PK);
			AssertNotNull(description + " should exist" + "\nlogMessages:\n" + string.Join("\n", LogMessages), bizOLoaded);
		}

		List<string> LogMessages { get; set; }
	}
}
