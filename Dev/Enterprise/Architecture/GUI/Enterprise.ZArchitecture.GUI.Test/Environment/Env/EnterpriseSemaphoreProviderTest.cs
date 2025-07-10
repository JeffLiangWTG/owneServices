using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Semaphores.Common;
using Enterprise.Semaphores.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Environment.Semaphore
{
	sealed class EnterpriseSemaphoreProviderTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestUsesMainConnectionToCreateSemaphoreInTheDatabaseInWinFormsEnvironmt()
		{
			ISemaphoreProvider provider = new EnterpriseSemaphoreProvider();
			ISemaphoreType testSemaphore = new SemaphoreForTesting();

			using (provider.CreateSemaphoreHandle(testSemaphore))
			{
				int mainConnCmdStartCount = Db.Connection.ExecutedCommandCount;
				int allCmdStartCount = Db.Connection.ExecutedCommandCountForAllConnections;

				using (provider.CreateSemaphoreHandle(testSemaphore)) { }

				int mainConnCmdCountDelta = Db.Connection.ExecutedCommandCount - mainConnCmdStartCount;
				int allCmdCountDelta = Db.Connection.ExecutedCommandCountForAllConnections - allCmdStartCount;

				AssertEquals("Should use main DB connection. Expected = Main Connection cmd count, Actual = Overall cmd count.", mainConnCmdCountDelta, allCmdCountDelta);
			}
		}

		public void TestHeartbeatDurationIsSufficientForFatTests()
		{
			var provider = new EnterpriseSemaphoreProviderWithExposedPropertiesForTesting();
			var heartbeatDuration = provider.HeartbeatDuration_Exposed;
			AssertGreaterThanOrEqualTo("Heartbeat duration should be greater than the max time for FAT tests plus the refresh threshold", heartbeatDuration, TimeSpan.FromMinutes(30) + SemaphoreDbManager.GetStopwatchThreshold(heartbeatDuration));
		}

		class EnterpriseSemaphoreProviderWithExposedPropertiesForTesting : EnterpriseSemaphoreProvider
		{
			public IHeartbeatInfoFactory HeartbeatSessionInfoFactory_Exposed
			{
				get { return HeartbeatSessionInfoFactory; }
			}

			public TimeSpan HeartbeatDuration_Exposed
			{
				get { return HeartbeatDuration; }
			}
		}
	}
}
