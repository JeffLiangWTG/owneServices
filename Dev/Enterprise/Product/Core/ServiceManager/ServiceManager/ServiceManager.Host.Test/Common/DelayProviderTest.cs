using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ServiceManager.Host;
using NUnit.Framework;

namespace Enterprise.ServiceManager.HostsController.Test
{
	abstract class DelayProviderTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			delayProvider = new DelayProvider();
		}

		public void TestWaitsForTimeout()
		{
			void Test(TimeSpan timeout)
			{
				// Arrange
				var stopwatch = Stopwatch.StartNew();

				// Act
				DelayFunction(timeout);

				// Assert
				stopwatch.Stop();
				AssertCloseEnough((long)stopwatch.Elapsed.TotalMilliseconds, (long)timeout.TotalMilliseconds, 100);
			}

			NUnit.Framework.Assert.Multiple(() =>
			{
				Test(TimeSpan.FromSeconds(1));
				Test(TimeSpan.FromSeconds(5));
				Test(TimeSpan.FromSeconds(7));
			});
		}

		public void TestCancellationWhenAsynchronouslyDelay()
		{
			// Arrange
			var cancelTime = TimeSpan.FromMilliseconds(100);
			using var cancellationTokenSource = new CancellationTokenSource(cancelTime);

			// Act
			var cancelledTask = delayProvider.DelayAsync(TimeSpan.FromSeconds(10), cancellationTokenSource.Token);

			// Assert
			Thread.Sleep(cancelTime);
			var exception = AssertExceptionThrown<AggregateException>(() => cancelledTask.Wait());
			AssertType<TaskCanceledException>(exception.InnerException);
		}

		public void TestWrongParamsCall()
		{
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => DelayFunction(TimeSpan.FromSeconds(-1)));
		}

		public class DelayTest : DelayProviderTest
		{
			protected override void DelayFunction(TimeSpan timeout)
			{
				delayProvider.Delay(timeout);
			}
		}

		public class DelayAsyncTest : DelayProviderTest
		{
			protected override void DelayFunction(TimeSpan timeout)
			{
				delayProvider.DelayAsync(timeout, CancellationToken.None).Wait();
			}
		}

		public class DelayWithTokenTest : DelayProviderTest
		{
			protected override void DelayFunction(TimeSpan timeout)
			{
				delayProvider.Delay(timeout, CancellationToken.None);
			}

			public void TestCancelsOnToken()
			{
				// Arrange
				using (var cancellationTokenSource = new CancellationTokenSource())
				using (var delayStartedEvent = new ManualResetEventSlim())
				{
					var requestedDelay = TimeSpan.FromMinutes(5);
					var task = Task.Run(() =>
					{
						delayStartedEvent.Set();
						delayProvider.Delay(requestedDelay, cancellationTokenSource.Token);
					});
					var stopwatch = Stopwatch.StartNew();
					Thread.Yield();
					delayStartedEvent.Wait();

					// Act
					cancellationTokenSource.Cancel();

					// Assert
					AssertExceptionThrown<OperationCanceledException>(() => task.GetAwaiter().GetResult());
					stopwatch.Stop();
					AssertLessThan(stopwatch.Elapsed, requestedDelay);
				}
			}

			public void TestCancelledToken()
			{
				// Arrange
				var cancellationToken = new CancellationToken(true);

				// Act
				AnonymousMethod result = () => delayProvider.Delay(TimeSpan.FromMinutes(5), cancellationToken);

				// Assert
				AssertExceptionThrown<OperationCanceledException>(result);
			}
		}

		protected abstract void DelayFunction(TimeSpan timeout);

		DelayProvider delayProvider;
	}
}
