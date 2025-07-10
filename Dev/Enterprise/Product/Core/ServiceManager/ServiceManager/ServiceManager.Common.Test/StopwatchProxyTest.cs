using System;
using System.Threading;
using NUnit.Framework;
using ServiceManager.Common;

namespace Enterprise.ServiceManager.Shared.Testing
{
	sealed class StopwatchProxyTest
	{
		[SetUp]
		public void SetUp()
		{
			stopwatch = new StopwatchProxy();
		}

		StopwatchProxy? stopwatch;

		[Test]
		public void TestElapsedRemainsZeroWhenStartNotCalled()
		{
			// Arrange
			// Act
			var timeSpan = TimeSpan.FromMilliseconds(100);
			Thread.Sleep(timeSpan);

			// Assert
			Assert.That(stopwatch!.ElapsedMilliseconds, Is.EqualTo(0));
		}

		[Test]
		public void TestAfterStartElapsedShouldTrackTheTime()
		{
			// Arrange
			// Act
			stopwatch!.Start();
			var timeSpan = TimeSpan.FromMilliseconds(100);
			Thread.Sleep(timeSpan);

			// Assert
			Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(100));
		}

		[Test]
		public void TestRestartShouldResetTheTime()
		{
			// Arrange
			// Act
			stopwatch!.Start();
			var timeSpan = TimeSpan.FromMilliseconds(100);
			Thread.Sleep(timeSpan);
			stopwatch.Restart();
			Thread.Sleep(timeSpan);

			// Assert
			Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(150));
		}

		[Test]
		public void TestAfterRestartElapsedShouldTrackTheTime()
		{
			// Arrange
			// Act
			stopwatch!.Restart();
			var timeSpan = TimeSpan.FromMilliseconds(100);
			Thread.Sleep(timeSpan);

			// Assert
			Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(100));
		}

		[Test]
		public void TestAfterPauseElapsedShouldNotTrackTheTime()
		{
			// Arrange
			stopwatch!.Start();
			var timeSpan = TimeSpan.FromMilliseconds(100);

			// Act
			stopwatch.Stop();
			var firstRecord = stopwatch.ElapsedMilliseconds;
			Thread.Sleep(timeSpan);
			var secondRecord = stopwatch.ElapsedMilliseconds;

			// Assert
			Assert.That(firstRecord, Is.EqualTo(secondRecord));
		}

		[Test]
		public void TestIsRunningReturnsStopwatchState()
		{
			// Arrange
			var runningStopwatch = new StopwatchProxy();
			var restartedStopwatch = new StopwatchProxy();
			var stoppedStopwatch = new StopwatchProxy();

			// Act
			runningStopwatch.Start();
			restartedStopwatch.Restart();
			stoppedStopwatch.Start();
			stoppedStopwatch.Stop();

			//Assert
			Assert.That(runningStopwatch.IsRunning);
			Assert.That(restartedStopwatch.IsRunning);
			Assert.That(!stopwatch!.IsRunning);
			Assert.That(!stoppedStopwatch.IsRunning);
		}
	}
}
