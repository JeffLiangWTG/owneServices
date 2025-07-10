using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Engine.Test
{
	public class ArchiveSetTestTransactionless : TestCase
	{
		[UseSnapshotProtection]
		public void TestLogsSessionsWithLocksWhenLockRequestTimeoutPeriodExceededException()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.NewWithValidTestData<DummyBusinessObject>();
			dummyBizo.Z0_Date = ZDateTime.Now.AddYears(-1);
			dummyBizo.Z0_Code = "SHREK";
			factory.Save();

			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: false);
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			var watermark = new ArchiveWatermark { WatermarkDate = ZDateTime.Now.AddYears(-2) };

			archiveStage.BeginRun(config, schedule, logger);
			var set = archiveStage.GetNextArchiveSet(watermark, schedule, archiveStage).Single();

			var queryBlockingLoadArchiveSet = "SELECT TOP 1 * FROM dbo.ArchiveMainItemQueue WITH (TABLOCKX, HOLDLOCK)";
			var queryHoldingLocksForUnrelatedTables = "DELETE FROM dbo.HVLVItem";

			using (var blockingConnection = Db.NewAdminConnection())
			using (var blockingConnectionTwo = Db.NewAdminConnection())
			using (var blockingTransactionManager = blockingConnection.BeginTransactionWithManager())
			using (var blockingTransactionManagerTwo = blockingConnectionTwo.BeginTransactionWithManager())
			{
				blockingConnection.ExecuteScalar(queryBlockingLoadArchiveSet);
				blockingConnectionTwo.ExecuteScalar(queryHoldingLocksForUnrelatedTables);

				using (var blockedConnection = Db.NewAdminConnection())
				using (blockedConnection.TemporarySetLockTimeout(500))
				using (new DisposableAction(() => Db.ConnectionOverrideForTest = null))
				{
					Db.ConnectionOverrideForTest = blockedConnection;
					set.Load(logger);
				}

				AssertEquals("ArchiveManager.ArchiveSetLockRequestTimeoutPeriodExceeded", ErrorReporter.LastKeyReported);
				AssertEquals("Lock request time out period exceeded.", ErrorReporter.LastExceptionReported.Message);

				AssertContains("Archive Manager has encountered lock contention with an unknown process, where one or more of the queries below may be the culprit", ErrorReporter.LastMessageReported);
				AssertContains($"[{blockingConnection.SPID}] ArchiveMainItemQueue - X\n{queryBlockingLoadArchiveSet}", ErrorReporter.LastMessageReported);
				AssertContains($"[{blockingConnectionTwo.SPID}] HVLVItem - IX, HVLVUsage - IX\n{queryHoldingLocksForUnrelatedTables}", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}
	}
}
