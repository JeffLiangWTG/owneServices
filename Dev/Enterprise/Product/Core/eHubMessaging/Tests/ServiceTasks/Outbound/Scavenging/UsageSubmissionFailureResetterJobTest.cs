using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.Outbound.Scavenging
{
	class UsageSubmissionFailureResetterJobTest : TestCaseWithFactory
	{
		public void TestResetsFailedBillingRecords()
		{
			AssertResetsFailedRecords("STS");
		}

		public void TestResetsFailedUsageRecords()
		{
			AssertResetsFailedRecords("USG");
		}

		public void TestResetsFailedUsageSummaryRecords()
		{
			AssertResetsFailedRecords("USS");
		}

		void AssertResetsFailedRecords(string code)
		{
			var stmUsageData = Factory.New<StmUsageData>();
			stmUsageData.SUD_Code = code;
			stmUsageData.SUD_Fail = ZBool.True;
			stmUsageData.SUD_PostedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var notifier = new NotificationBuffer();
			var job = new UsageSubmissionFailureResetterJob(eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockServiceTaskSupport(ServiceTaskNames.UsageSubmissionFailureResetter).Object, notifier);
			job.Execute(new CancellationToken());
			Assert("execute did not return success when records were processed", job.NextExecuteIterationIsScheduled);
			Factory.ReloadAll<StmUsageData>();
			AssertEquals("Failure has not been reset", ZBool.False, stmUsageData.SUD_Fail);
			notifier.AssertNotificationContains("Reset SUD_Fail=0 for 1 usage record(s).");
		}

		public void TestDoesNotResetOldBillingRecords()
		{
			AssertDoesNotResetNewRecords("STS", ZDateTime.UtcNow.AddMonths(-4));
		}

		public void TestDoesNotResetOldUsageRecords()
		{
			AssertDoesNotResetNewRecords("USG", ZDateTime.UtcNow.AddDays(-8));
		}

		public void TestDoesNotResetOldSummaryRecords()
		{
			AssertDoesNotResetNewRecords("USS", ZDateTime.UtcNow.AddDays(-8));
		}

		public void AssertDoesNotResetNewRecords(string code, ZDateTime postedTimeUtc)
		{
			var stmUsageData = Factory.New<StmUsageData>();
			stmUsageData.SUD_Code = code;
			stmUsageData.SUD_Fail = ZBool.True;
			stmUsageData.SUD_PostedTimeUtc = postedTimeUtc;
			Factory.Save();

			var notifier = new NotificationBuffer();
			var job = new UsageSubmissionFailureResetterJob(eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockServiceTaskSupport(ServiceTaskNames.UsageSubmissionFailureResetter).Object, notifier);
			job.Execute(new CancellationToken());
			Assert("Execute did not return true when records deleted", job.NextExecuteIterationIsScheduled);
			var usageRecords = Factory.Load<StmUsageData>(new ZQuery());
			AssertEquals("All usage records should have been deleted", 0, usageRecords.Length);
			notifier.AssertNotificationContains("Deleted 1 expired usage record(s).");
		}

		public void TestBatches()
		{
			var stmUsageData1 = Factory.New<StmUsageData>();
			stmUsageData1.SUD_Fail = ZBool.True;
			stmUsageData1.SUD_PostedTimeUtc = ZDateTime.UtcNow;
			var stmUsageData2 = Factory.New<StmUsageData>();
			stmUsageData2.SUD_Fail = ZBool.True;
			stmUsageData2.SUD_PostedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			var notificationBuffer = new NotificationBuffer();
			var job = new UsageSubmissionFailureResetterJob(eHubServiceTaskTest<eHubServiceTask, ServiceTaskJob>.CreateMockServiceTaskSupport(ServiceTaskNames.UsageSubmissionFailureResetter).Object, notificationBuffer, batchSize: 1);

			job.Execute(new CancellationToken());
			Assert("execute did not return success when records were processed", job.NextExecuteIterationIsScheduled);
			Factory.ReloadAll<StmUsageData>();
			AssertEquals("Failure has not been reset", ZBool.False, stmUsageData1.SUD_Fail);
			AssertEquals("Failure has been reset before second batch is processed", ZBool.True, stmUsageData2.SUD_Fail);
			notificationBuffer.AssertNotificationContains("Reset SUD_Fail=0 for 1 usage record(s).");
			notificationBuffer.Clear();

			job.Execute(new CancellationToken());
			Assert("execute did not return success when records were processed", job.NextExecuteIterationIsScheduled);
			Factory.ReloadAll<StmUsageData>();
			AssertEquals("Failure has not been reset in second batch", ZBool.False, stmUsageData2.SUD_Fail);
			notificationBuffer.AssertNotificationContains("Reset SUD_Fail=0 for 1 usage record(s).");
			notificationBuffer.Clear();

			job.Execute(new CancellationToken());
			Assert("Execute did not return false when no records processed", !job.NextExecuteIterationIsScheduled);
			notificationBuffer.AssertNotificationContains("Reset SUD_Fail=0 for 0 usage record(s).");
			notificationBuffer.Clear();
		}
	}
}
