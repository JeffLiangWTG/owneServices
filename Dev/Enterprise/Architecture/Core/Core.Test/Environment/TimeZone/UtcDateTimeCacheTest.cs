using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class UtcDateTimeCacheTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestNoExceptionReported_OnUsingDisposableActionForDbConnection()
		{
			// Arrange
			var utcDateTimeCacheMock = new Mock<UtcDateTimeCache> { CallBase = true };
			utcDateTimeCacheMock.Protected()
				.Setup<(DateTime, DateTime, TimeSpan)>("CreateDateTimeCacheCateringForClientServerLatency", ItExpr.IsAny<DbConnection>())
				.Callback((DbConnection connection) => { connection.EnsureIsOpen(); });

			var utcDateTimeCache = utcDateTimeCacheMock.Object;
			var errorReporterMock = new Mock<IErrorReporter>();
			errorReporterMock
				.Setup(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
				.Verifiable();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				ErrorReporter.Clear();

				// Act
				using (var adminConnection = Db.NewAdminConnection())
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);

					try
					{
						var backgroundThreadCompletedEvent = new ManualResetEvent(false);
						var thread = new Thread(obj =>
						{
							_ = utcDateTimeCache.TryReallyHardToGetValues();
							backgroundThreadCompletedEvent.Set();
						});

						thread.Start();
						backgroundThreadCompletedEvent.WaitOne();
						thread.Join();
					}
					finally
					{
						adminConnection.ResetLockout();
					}
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					errorReporterMock.Verify(reporter => reporter
						.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);

					utcDateTimeCacheMock.Protected().Verify<(DateTime, DateTime, TimeSpan)>(
						"CreateDateTimeCacheCateringForClientServerLatency",
						Times.AtLeastOnce(),
						ItExpr.IsAny<DbConnection>());
				});
			}
		}

		[UseSnapshotProtection]
		public void TestRefreshDateTimeDatabaseUpgradeExceptionsCaught()
		{
			ErrorReporter.Clear();
			var errorReporterMock = new Mock<IErrorReporter>();
			errorReporterMock
				.Setup(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
				.Verifiable();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (var extraConnection = Db.Connection)
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					try
					{
						AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => extraConnection.BeginTransaction());
						AssertNoExceptionThrown(() => new UtcDateTimeCache());
						AssertNoExceptionThrown(UtcDateTimeCacheFromSeparateThread);
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
					}
					finally
					{
						adminConnection.ResetLockout();
					}

					try
					{
						AssertNoExceptionThrown(() => new UtcDateTimeCache());
						AssertNoExceptionThrown(() => new UtcDateTimeCache());
						AssertNoExceptionThrown(UtcDateTimeCacheFromSeparateThread);
					}
					finally
					{
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) - 1, adminConnection);
					}
				}
			}

			AssertNoExceptionThrown(() =>
			{
				errorReporterMock.Verify(reporter => reporter
					.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			});

			void UtcDateTimeCacheFromSeparateThread()
			{
				var thread = new Thread(() => _ = new UtcDateTimeCache());

				try
				{
					thread.Start();
				}
				finally
				{
					thread.Join();
				}
			}
		}

		[SnailTest]
		public void TestGetCurrentUtcWithLongClientServerLatency()
		{
			var deltaMs = 200;
			var roundTripSeconds = 4;
			DateTime clientUtcTimeBefore = DateTime.UtcNow;
			System.Threading.Thread.Sleep(1);

			var testUtcCache = new UtcDateTimeCacheForLongLatencyTesting();

			var delta = DateTime.UtcNow - testUtcCache.ClientUtcTime;
			AssertLessThan("Last refresh time should be utc DateTime", delta.TotalMilliseconds, deltaMs);

			System.Threading.Thread.Sleep(1);
			DateTime clientUtcTimeAfter = DateTime.UtcNow;

			AssertEquals(string.Format("[Round Trip = {0}s] Time to refresh cache ({1}s) > round-trip ({0}s)?", roundTripSeconds.ToString(), clientUtcTimeAfter.Subtract(clientUtcTimeBefore).TotalSeconds.ToString()),
				true, clientUtcTimeAfter.Subtract(clientUtcTimeBefore).TotalSeconds > roundTripSeconds);
			AssertEquals(string.Format("[Round Trip = {0}s] CacheLastRefresh ({1}) > clientUtcTimeBefore ({2})?", roundTripSeconds.ToString(), testUtcCache.ClientUtcTime.ToString(), clientUtcTimeBefore.ToString()),
				true, testUtcCache.ClientUtcTime > clientUtcTimeBefore);
			AssertEquals(string.Format("[Round Trip = {0}s] CacheLastRefresh ({1}) < clientUtcTimeAfter ({2})?", roundTripSeconds.ToString(), testUtcCache.ClientUtcTime.ToString(), clientUtcTimeAfter.ToString()),
				true, testUtcCache.ClientUtcTime < clientUtcTimeAfter);
			// Even if the roundtrip is long, this should be virtually = zero. 500ms used just in case the test PC is too busy.
			AssertEquals(string.Format("[Round Trip = {0}s] Client-Server time difference ({1}ms) < 500ms?", roundTripSeconds.ToString(), Math.Abs(testUtcCache.ClientPcToDbTimeDelta.TotalMilliseconds).ToString()),
				true, Math.Abs(testUtcCache.ClientPcToDbTimeDelta.TotalMilliseconds) < 500);
		}

		public void TestHandlingDbInUse()
		{
			UtcDateTimeCacheForTesting testUtcCache = new UtcDateTimeCacheForTesting();
			using (var dummyReader = Db.Connection.Command("SELECT NULL").ExecuteReader())
			{
				AssertNoExceptionThrown(() => testUtcCache.GetCurrentUtc());
			}
		}

		public void TestDateTimeReturnsSqlDataBaseTimePrecision()
		{
			UtcDateTimeCacheForTesting testUtcCache = new UtcDateTimeCacheForTesting();

			for (int i = 0; i < 10; i++)
			{
				DateTime testDateTime = testUtcCache.GetCurrentUtc();
				int milliseconds = testDateTime.Millisecond;

				Assert(
					"Must end with either 0 or 3 or 7 milliseconds for SqlDatabase time precision, but was " + milliseconds.ToString(),
					(milliseconds % 10 == 0) || (milliseconds % 10 == 3) || (milliseconds % 10 == 7));

				Assert(
					"Precision must not be more accurate than the millisecond, but was ." + testDateTime.ToString("fffffff") + " seconds",
					testDateTime.ToString("fffffff").EndsWith("0000"));

				System.Threading.Thread.Sleep(1);
			}
		}

		public void TestShouldUseDbCorrectly()
		{
			var lastError = string.Empty;

			var thread = new Thread(() =>
			{
				ErrorReporter.Clear();

				using (var cache = new UtcDateTimeCacheForTesting())
				{
					cache.UtcNow_Override = DateTime.MinValue;
					cache.GetCurrentUtc();
				}

				lastError = ErrorReporter.LastMessageReported;
				ErrorReporter.Clear();
			});
			thread.Start();
			thread.Join(1000);

			// Should not be "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()"
			AssertEquals(string.Empty, lastError);
		}

		#region When PC clock goes nuts

		public void TestExposedClientPcToDbTimeDelta()
		{
			AssertNoExceptionThrown("Should not throw error, when get the invalid current date from cache", () => new UtcDateTimeCacheWithInvalidPcToDbTime());
		}
		class UtcDateTimeCacheWithInvalidPcToDbTime : UtcDateTimeCache
		{
			protected override (DateTime dbServerUtcOnLastCacheRefresh, DateTime cacheLastRefreshClientUtcTime, TimeSpan clientPcToDbUtcTimeDelta) CreateDateTimeCacheWithExceptionHandling()
			{
				return (DateTime.UtcNow, DateTime.UtcNow, TimeSpan.FromDays(-99999));
			}
		}

		public void TestCheckTimeSpanBetweenServerAndClient()
		{
			AssertNoExceptionThrown("When PC client always valid", () => new UtcDateTimeCacheWithInvalidPcTime(0).GetCurrentUtc());
			var utcDateTimeCacheWithInvalidPcTime = new UtcDateTimeCacheWithInvalidPcTime(1);
			AssertNoExceptionThrown("When PC client invalid the 1st time", () => utcDateTimeCacheWithInvalidPcTime.TryReallyHardToGetValues());
			utcDateTimeCacheWithInvalidPcTime = new UtcDateTimeCacheWithInvalidPcTime(2);
			AssertNoExceptionThrown("When PC client invalid the 1st 2 times", () => utcDateTimeCacheWithInvalidPcTime.TryReallyHardToGetValues());
			utcDateTimeCacheWithInvalidPcTime = new UtcDateTimeCacheWithInvalidPcTime(3);
			AssertNoExceptionThrown("When PC client invalid the 1st 3 times", () => utcDateTimeCacheWithInvalidPcTime.TryReallyHardToGetValues());
			utcDateTimeCacheWithInvalidPcTime = new UtcDateTimeCacheWithInvalidPcTime(4);
			AssertExceptionThrown(typeof(SqlTypeException), () => utcDateTimeCacheWithInvalidPcTime.TryReallyHardToGetValues());
		}

		class UtcDateTimeCacheWithInvalidPcTime : UtcDateTimeCache
		{
			public UtcDateTimeCacheWithInvalidPcTime(int numberOfTimeToReturnInvalidTime = 0) : base()
			{
				this.numberOfTimeToReturnInvalidTime = numberOfTimeToReturnInvalidTime;
			}

			protected override (DateTime dbServerUtcOnLastCacheRefresh, DateTime cacheLastRefreshClientUtcTime, TimeSpan clientPcToDbUtcTimeDelta) CreateDateTimeCacheWithExceptionHandling()
			{
				if (numberOfTimeToReturnInvalidTime <= 0)
				{
					return base.CreateDateTimeCacheWithExceptionHandling();
				}

				numberOfTimeToReturnInvalidTime--;
				throw new SqlTypeException();
			}

			int numberOfTimeToReturnInvalidTime;
		}

		#endregion
	}
}
