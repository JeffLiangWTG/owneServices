using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	class SlowTransformReporterTest : TransactionedTestCase
	{
		public void TestCertainWaitCategoriesAreExcluded()
		{
			// Arrange
			SetRegistry(1);
			var waitReporterMock = new Mock<ISessionWaitStatsReporter>();
			waitReporterMock
				.Setup(r => r.GetSessionWaits())
				.Returns(new[] { new SessionWaitStats("SOS_SCHEDULER_YIELD", waitTimeMs: 1000, waitingTaskCount: 1, maxWaitTimeMs: 1500, signalWaitTimeMs: 500) });

			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePostUpgrade, waitReporterMock.Object))
			{
				Thread.Sleep(TimeSpan.FromSeconds(3));
			}

			Assert(ErrorReporter.HasBeenReported("SlowTransform: Example Transform"));
			Assert(ErrorReporter.LastMessageReported.Contains("Transformation Example Transform has taken 00:00:02"));
			Assert(ErrorReporter.LastMessageReported.Contains("SOS_SCHEDULER_YIELD (wait time is already subtracted from transform execution time):"));
			ErrorReporter.Clear();
		}

		public void TestSlowTransformReporterReportsSlowOfflinePostUpgrade()
		{
			SetRegistry(1);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePostUpgrade))
			{
				Db.Connection.ExecuteNonQuery("WAITFOR DELAY '00:00:02'");
			}

			Assert(ErrorReporter.HasBeenReported("SlowTransform: Example Transform"));
			ErrorReporter.Clear();
		}

		public void TestSlowTransformReporterReportsSlowOfflinePreUpgrade()
		{
			SetRegistry(1);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePreUpgrade))
			{
				Db.Connection.ExecuteNonQuery("WAITFOR DELAY '00:00:02'");
			}

			Assert(ErrorReporter.HasBeenReported("SlowTransform: Example Transform"));
			ErrorReporter.Clear();
		}

		public void TestSlowTransformReporterReportsCategoryWithAccurateTime()
		{
			SetRegistry(1);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePreUpgrade))
			{
				Db.Connection.ExecuteNonQuery("WAITFOR DELAY '00:00:02'");
			}

			Assert(ErrorReporter.HasBeenReported("SlowTransform: Example Transform"));
			Assert(ErrorReporter.LastMessageReported.Contains("CATEGORY: Wait Stats"));
			Assert(ErrorReporter.LastMessageReported.Contains("WAITFOR:"));
			Assert(ErrorReporter.LastMessageReported.Contains("wait_time_ms: 2")); //Typically slightly more than 2000 ms
			ErrorReporter.Clear();
		}

		public void TestSlowTransformReporterReportsWithCorrectFormatting()
		{
			SetRegistry(1);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePreUpgrade))
			{
				Db.Connection.ExecuteNonQuery("WAITFOR DELAY '00:00:02'");
			}

			Assert(ErrorReporter.HasBeenReported("SlowTransform: Example Transform"));
			Assert(ErrorReporter.LastMessageReported.Contains("CATEGORY: Wait Stats"));
			Assert(ErrorReporter.LastMessageReported.Contains("WAITFOR:"));
			Assert(!ErrorReporter.LastMessageReported.Contains(", wait_time_ms: "));
			Assert(ErrorReporter.LastMessageReported.Contains(", waiting_tasks_count: "));
			Assert(ErrorReporter.LastMessageReported.Contains(", signal_wait_time_ms: 0"));
			ErrorReporter.Clear();
		}

		public void TestSlowTransformReporterDoesNotReportUnusedCategory()
		{
			SetRegistry(1);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePreUpgrade))
			{
				Thread.Sleep(2000);
			}

			Assert(ErrorReporter.HasBeenReported("SlowTransform: Example Transform"));
			Assert(!ErrorReporter.LastMessageReported.Contains("CATEGORY: WAITFOR"));
			ErrorReporter.Clear();
		}

		public void TestSlowTransformReporterDoesNotReportZeroValueCategory()
		{
			SetRegistry(1);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePreUpgrade))
			{
				Thread.Sleep(2000);
			}

			Assert(ErrorReporter.HasBeenReported("SlowTransform: Example Transform"));
			Assert("Slow transform reporter should not report categories where all values are zero", !ErrorReporter.LastMessageReported.Contains("wait_time_ms:\r\n0\r\nwaiting_tasks_count:\r\n0\r\nmax_wait_time_ms:\r\n0\r\nsignal_wait_time_ms:\r\n0"));
			ErrorReporter.Clear();
		}

		public void TestSlowTransformReporterReportsAllWhenRegistryIsZero()
		{
			SetRegistry(0);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePostUpgrade))
			{
			}

			Assert(ErrorReporter.HasBeenReported("SlowTransform: Example Transform"));
			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestSlowTransformReporterDoesNotReportFast()
		{
			SetRegistry(2);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePostUpgrade))
			{
			}
		}

		[ExpectNoExceptions]
		public void TestSlowTransformReporterDoesNotReportOnlinePostUpgrade()
		{
			SetRegistry(1);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OnlinePostUpgrade))
			{
				Thread.Sleep(2000);
			}
		}

		[ExpectNoExceptions]
		public void TestSlowTransformReporterDoesNotReportOnlinePreUpgrade()
		{
			SetRegistry(1);
			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OnlinePreUpgrade))
			{
				Thread.Sleep(2000);
			}
		}

		[ExpectNoExceptions]
		public void TestSlowTransformReporterHandlesUnsetRegistry()
		{
			Db.Connection.ExecuteNonQuery($"delete from dbo.StmData where SD_Name='{DbRegistry.ReportSlowUpgradeTransformsSeconds.ItemName}'");
			SlowTransformReporter.ResetLazyRegistry();

			using (new SlowTransformReporter("Example Transform", DataModification.TransformationSection.OfflinePostUpgrade))
			{
			}
		}

		void SetRegistry(int value)
		{
			SlowTransformReporter.ResetLazyRegistry();
			DbRegistry.ReportSlowUpgradeTransformsSeconds.SaveValue(value, Db.Connection);
		}
	}
}
