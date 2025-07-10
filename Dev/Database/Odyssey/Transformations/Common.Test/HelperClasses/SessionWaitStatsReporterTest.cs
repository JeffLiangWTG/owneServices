using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	class SessionWaitStatsReporterTest : TransactionedTestCase
	{
		public void TestGetSessionWaits()
		{
			// Arrange
			var reporter = new SessionWaitStatsReporter("#TempSessionsForTest");
			reporter.StartRecording();
			Db.Connection.ExecuteNonQuery("WAITFOR DELAY '00:00:02'");

			// Act
			var waits = reporter.GetSessionWaits();

			// Assert
			AssertCollectionContains("Contains WAITFOR", waits, w => w is { WaitType: "WAITFOR", WaitTimeMs: >= 2000, WaitingTaskCount: 1 });
		}

		public void TestCallingGetSessionWaitsWithoutStartRecording()
		{
			// Arrange
			var reporter = new SessionWaitStatsReporter("#TempSessionsForTest");

			// Act
			var ex = AssertExceptionThrown<InvalidOperationException>(() => reporter.GetSessionWaits());

			// Assert
			AssertContains(nameof(ex.Message), "call StartRecording method first" ,ex.Message);
		}

		protected override void TearDown()
		{
			Db.Connection.ExecuteNonQuery("DROP TABLE IF EXISTS #TempSessionsForTest");
			base.TearDown();
		}
	}
}
