using System;
using System.Collections.Generic;
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
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.ArchiveManager.Engine.Test.ArchiveStages
{
	[UseSnapshotProtection]
	public class MAIOnlyArchiveStageTest : TestCaseWithFactory
	{
		TestArchiveLogger logger;
		TestArchiveLogger Logger => logger ?? (logger = new TestArchiveLogger());

		public void TestLogLoadedArchiveSetDetails_AddsCorrectLogMessage()
		{
			var dummyBizo = Factory.New<DummyBusinessObject>();
			dummyBizo.Z0_Code = "D8669";
			dummyBizo.Z0_Date = new ZDate(2009, 1, 13);

			Factory.Save();

			var systemDescriptor = new DummyMAIOnlySystemDescriptor();
			var config = new ArchiveConfiguration(new ZDateTime(2009, 1, 14), 2, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
			var schedule = new TestArchiveSchedule();

			_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, Logger, schedule, new CancellationToken());

			AssertLog([$"Information|{systemDescriptor.Code}|Loaded batch of 1 {dummyBizo.TableName}"]);
			AssertLog([$"Information|{systemDescriptor.Code}|Time taken to load 1 Archive Set(s) of {dummyBizo.TableName} record(s) in 1 batch(es):"]);
		}

		public void TestMAIOnlyArchiveSetIsSkipped_AndNextSetIsArchivedCorrectly_WhenArchiveToImagesThrowsException()
		{
			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var dummyBizos = CreateTestData(6);

				var dummyDependentBizo = Factory.New<DummyDependantBusinessObject>();
				dummyDependentBizo.ZD1_Z0 = dummyBizos[2].PK;
				dummyDependentBizo.ZD1_Code = "D8675";

				Factory.Save();

				CombineAssertions("Pre-conditions", () =>
				{
					AssertEquals(6, Factory.GetDatabaseCount(typeof(DummyBusinessObject)));
					AssertEquals(1, Factory.GetDatabaseCount(typeof(DummyDependantBusinessObject)));
				});

				var systemDescriptor = new DummyMAIOnlySystemDescriptor();
				var config = new ArchiveConfiguration(ZDateTime.UtcNow.AddDays(-2), 2, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
				var archiveStage = new MAIOnlyArchiveStage(stageDescriptor, systemDescriptor);
				var schedule = new TestArchiveSchedule();

				_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, Logger, schedule, new CancellationToken());

				CombineAssertions("Post-conditions", () =>
				{
					AssertEquals("Second batch should've been skipped, but other 2 batches should've still been archived", 2, Factory.GetDatabaseCount(typeof(DummyBusinessObject)));
					AssertEquals("The dummyDependentBizo should've remained in the database", 1, Factory.GetDatabaseCount(typeof(DummyDependantBusinessObject)));
					AssertLog([$"Information|{TestArchiveManagerConstants.Codes.DMA}|Loaded batch of 2 DummyBizo", $"DummyBizo: 2"], 3);
					AssertLog([$"Error|{TestArchiveManagerConstants.Codes.DMA}|Failed to archive this set.", "The DELETE statement conflicted with the REFERENCE constraint", "DummyDependentBizo_ZD1_Z0_FK2_DummyBizo_RRR_120N"]);
					AssertLog([$"Information|{TestArchiveManagerConstants.Codes.DMA}|Skipped records dated between '{dummyBizos[1].Z0_Date.ToShortDateString()}' and '{dummyBizos[3].Z0_Date.ToShortDateString()}'"]);
				});
			}
		}

		public void TestMAIOnlyArchiveSetIsSkipped_AndNextSetIsArchivedCorrectly_WhenLoadThrowsException()
		{
			using (SystemDataRegistry.Instance.BatchSizeControl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				ErrorReporter.Clear();
				var dummyBizos = CreateTestData(4);

				Factory.Save();

				AssertEquals("Pre-condition", 4, Factory.GetDatabaseCount(typeof(DummyBusinessObject)));

				var systemDescriptor = new DummyMAIOnlySystemDescriptor();
				var config = new ArchiveConfiguration(ZDateTime.UtcNow.AddDays(-2), 2, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
				var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
				var archiveStage = new MAIOnlyArchiveStageThatThrowsExceptionOnLoad(stageDescriptor, systemDescriptor);
				var schedule = new TestArchiveSchedule();

				_ = archiveStage.ExecuteStage(new ArchiveSystem(systemDescriptor), archiveStage, config, Logger, schedule, new CancellationToken());

				CombineAssertions("Post-conditions", () =>
				{
					AssertEquals("First batch should've been skipped and second batch should've still been archived", 2, Factory.GetDatabaseCount(typeof(DummyBusinessObject)));
					AssertLog([$"Information|{TestArchiveManagerConstants.Codes.DMA}|Loaded batch of 2 DummyBizo", $"DummyBizo: 2"]);
					Assert("ErrorReporter should have reported a lock request timeout error", ErrorReporter.LastExceptionReported.Message.Contains("Lock request timeout exceeded"));
					AssertLog([$"Information|{TestArchiveManagerConstants.Codes.DMA}|Skipped records dated between 'Earliest Possible Date' and '{dummyBizos[1].Z0_Date.ToShortDateString()}'"]);
				});

				ErrorReporter.Clear();
			}
		}

		public class MAIOnlyArchiveStageThatThrowsExceptionOnLoad : MAIOnlyArchiveStage
		{
			bool HasLoadedArchiveSet { get; set; }

			public MAIOnlyArchiveStageThatThrowsExceptionOnLoad(IArchiveStageDescriptor stageDescriptor, IArchiveSystemDescriptor systemDescriptor)
				: base(stageDescriptor, systemDescriptor)
			{
			}

			public override IEnumerable<IArchiveSet> GetNextArchiveSet(IArchiveSchedule schedule, IArchiveStage stage)
			{
				var archiveSets = base.GetNextArchiveSet(schedule, stage);
				if (!HasLoadedArchiveSet)
				{
					((MAIOnlyArchiveSet)archiveSets.FirstOrDefault()).Factory = GetFactoryMock();
					HasLoadedArchiveSet = true;
				}
				return archiveSets;
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

		List<DummyBusinessObject> CreateTestData(int numberOfBizos)
		{
			var dummyBizos = new List<DummyBusinessObject>();
			for (var i = 0; i < numberOfBizos; i++)
			{
				var dummyBizo = Factory.New<DummyBusinessObject>();
				dummyBizo.Z0_Code = "D86" + (i + 1).ToString("D2");
				dummyBizo.Z0_Date = new ZDate(2009, 1, 13 + i);
				dummyBizos.Add(dummyBizo);
			}
			return dummyBizos;
		}

		void AssertLog(string[] expectedMessages, int count = 1)
			=> Assert($"A log containing the following messages should've occurred {count} time(s):\n" + expectedMessages.Aggregate((s1, s2) => s1 + "\n" + s2), Logger.ListOfMessages.Count(log => expectedMessages.All(expected => log.Contains(expected))) == count);
	}
}
