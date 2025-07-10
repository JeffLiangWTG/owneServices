using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	class BackgroundThreadActionQueueTest
	{
		[Test]
		public void TestWaitForEnqueueReturnsFalseOnTimeout()
		{
			var result = backgroundThreadActionQueue!.WaitForEnqueue(TimeSpan.FromSeconds(2), CancellationToken.None);
			Assert.That(result, Is.EqualTo(false));
		}

		[Test]
		public void TestWaitForEnqueueReturnsFalseOnCancellation()
		{
			using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
			var token = cancellationTokenSource.Token;
			var result = backgroundThreadActionQueue!.WaitForEnqueue(TimeSpan.FromMinutes(15), token);
			Assert.That(result, Is.EqualTo(false));
		}

		[Test]
		public void TestWaitForEnqueueReturnsTrue()
		{
			backgroundThreadActionQueue!.Enqueue(() => { });
			var result = backgroundThreadActionQueue.WaitForEnqueue(TimeSpan.FromMinutes(15), CancellationToken.None);
			Assert.That(result, Is.EqualTo(true));
		}

		[Test]
		public void TestOnlyOneInstanceInTheQueueAtATime()
		{
			var timerRuns = 0;
			using var timer = backgroundThreadActionQueue!.CreateTimer(() => ++timerRuns, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300));

			Thread.Sleep(TimeSpan.FromMilliseconds(500));
			backgroundThreadActionQueue.InvokeActions();
			Assert.That(timerRuns, Is.EqualTo(1));

			Thread.Sleep(TimeSpan.FromMilliseconds(300));
			backgroundThreadActionQueue.InvokeActions();
			Assert.That(timerRuns, Is.EqualTo(2));

			Thread.Sleep(TimeSpan.FromSeconds(1));
			backgroundThreadActionQueue.InvokeActions();
			Assert.That(timerRuns, Is.EqualTo(3));
		}

		[Test]
		public void TestDoesNotRunAfterDispose()
		{
			var timerRuns = 0;
			using (var timer = backgroundThreadActionQueue!.CreateTimer(() => ++timerRuns, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(300)))
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(500));
				backgroundThreadActionQueue.InvokeActions();
				Assert.That(timerRuns, Is.EqualTo(1));

				Thread.Sleep(TimeSpan.FromMilliseconds(300));
			}

			backgroundThreadActionQueue.InvokeActions();
			Assert.That(timerRuns, Is.EqualTo(1));
		}

		[Test]
		public void TestEnqueue_IgnoreWhenDisposed()
		{
			backgroundThreadActionQueue!.Dispose();
			bool actionWasRun = false;
			Assert.DoesNotThrow(() => backgroundThreadActionQueue.Enqueue(() => { actionWasRun = true; }), "Enqueue when disposed should not throw");
			Assert.That(actionWasRun, Is.EqualTo(false), "action should not be run");
		}

		[Test]
		public void TestWake_IgnoreWhenDisposed()
		{
			backgroundThreadActionQueue!.Dispose();
			backgroundThreadActionQueue.Wake();
		}

		[Test]
		public void TestCancellationWhenDelayEnqueueing()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			var delayProvider = new Mock<IAsyncDelayProvider>();
			delayProvider
				.Setup(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
				.Returns(Task.CompletedTask);

			using var actionQueue = new BackgroundThreadActionQueue(cancellationTokenSource.Token, delayProvider.Object);

			// Act
			actionQueue.Enqueue(TimeSpan.FromMilliseconds(10), () => { });

			// Assert
			delayProvider.Verify(x => x.DelayAsync(It.IsAny<TimeSpan>(), cancellationTokenSource.Token), Times.Once);
		}

		[SetUp]
		public void SetUp()
		{
			backgroundThreadActionQueue = new BackgroundThreadActionQueue(CancellationToken.None);
		}

		[TearDown]
		public void TearDown()
		{
			backgroundThreadActionQueue!.Dispose();
		}

		BackgroundThreadActionQueue? backgroundThreadActionQueue;

		public class InvokeActionsTest
		{
			[Test]
			public void TestInvokesAllActions()
			{
				// Arrange
				var count = 0;
				backgroundThreadActionQueue!.Enqueue(() => Interlocked.Increment(ref count));
				backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
				backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));

				// Act
				backgroundThreadActionQueue.InvokeActions();

				// Assert
				Assert.That(count, Is.EqualTo(3));
			}

			[Test]
			public void TestInvokesOnlyActionsQueuedBeforeExecution()
			{
				// Arrange
				var count = 0;
				backgroundThreadActionQueue!.Enqueue(() =>
				{
					Interlocked.Increment(ref count);
					backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
					backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
				});

				// Act
				backgroundThreadActionQueue.InvokeActions();

				// Assert
				Assert.That(count, Is.EqualTo(1));
			}

			[Test]
			public void TestInvokesActionsQueuedDuringExecution()
			{
				// Arrange
				var count = 0;
				backgroundThreadActionQueue!.Enqueue(() =>
				{
					backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
					backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
				});
				backgroundThreadActionQueue.InvokeActions();

				// Act
				backgroundThreadActionQueue.InvokeActions();

				// Assert
				Assert.That(count, Is.EqualTo(2));
			}

			[Test]
			public void TestChecksCurrentThread()
			{
				// Arrange
				BackgroundThreadActionQueue? queue = null;
				var thread = new Thread(() =>
				{
					queue = new BackgroundThreadActionQueue(CancellationToken.None);
				});
				thread.Start();
				thread.Join(TimeSpan.FromSeconds(5));

				// Act
				// Assert
				Assert.Throws<InvalidOperationException>(() => queue?.InvokeActions());
			}

			[Test]
			public void TestOverrideMainThreadId()
			{
				// Arrange
				BackgroundThreadActionQueue? queue = null;
				using var waitForQueueConstruction = new ManualResetEvent(false);
				var thread = new Thread(() =>
				{
					queue = new BackgroundThreadActionQueue(CancellationToken.None);
					waitForQueueConstruction.Set();
				});
				thread.Start();
				thread.Join(TimeSpan.FromSeconds(5));

				waitForQueueConstruction.WaitOne();

				// Act
				queue!.SetMainThreadId();

				// Assert
				Assert.DoesNotThrow(() => queue!.InvokeActions());
			}

			[Test]
			public void TestOverrideMainThreadIdStillFailsOtherThreads()
			{
				// Arrange
				var thread = new Thread(() =>
				{
					backgroundThreadActionQueue!.SetMainThreadId();
				});
				thread.Start();
				thread.Join(TimeSpan.FromSeconds(5));

				// Act
				// Assert
				Assert.Throws<InvalidOperationException>(() => backgroundThreadActionQueue!.InvokeActions());
			}

			[TestCase(5)]
			[TestCase(10)]
			public void TestRecursiveActionsWillNotCauseStackOverflowExceptions(int maxDepth)
			{
				// Arrange
				using var queue = new BackgroundThreadActionQueue(CancellationToken.None);
				var stackTraces = new List<string>();

				// Act
				queue.Enqueue(() => RecursiveAction(0, maxDepth));
				for (var i = 0; i <= maxDepth; i++)
				{
					queue.InvokeActions();
				}

				// Assert
				Assert.That(stackTraces.Count, Is.EqualTo(maxDepth));
				Assert.That(stackTraces.Distinct().Count(), Is.EqualTo(2), $"There should be 2 Distinct stack traces, 1 for the first call and another for the next {maxDepth - 1} recursive calls");

				void RecursiveAction(int depth, int maxRecursion)
				{
					if (depth < maxRecursion)
					{
						stackTraces.Add(new StackTrace().ToString());
						queue.Enqueue(() => RecursiveAction(++depth, maxRecursion));
					}
				}
			}

			[SetUp]
			public void SetUp()
			{
				backgroundThreadActionQueue = new BackgroundThreadActionQueue(CancellationToken.None);
			}

			[TearDown]
			public void TearDown()
			{
				backgroundThreadActionQueue!.Dispose();
			}

			BackgroundThreadActionQueue? backgroundThreadActionQueue;
		}

		public class InvokeActionsWhileWaitingTest
		{
			[Test]
			public void TestInvokesAllActions()
			{
				// Arrange
				var count = 0;
				backgroundThreadActionQueue!.Enqueue(() => Interlocked.Increment(ref count));
				backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
				backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));

				// Act
				backgroundThreadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(5), () => false);

				// Assert
				Assert.That(count, Is.EqualTo(3));
			}

			[Test]
			public void TestInvokesActionsQueuedDuringExecution()
			{
				// Arrange
				var count = 0;
				backgroundThreadActionQueue!.Enqueue(() =>
				{
					Interlocked.Increment(ref count);
					backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
					backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
				});

				// Act
				backgroundThreadActionQueue.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(5), () => false);

				// Assert
				Assert.That(count, Is.EqualTo(3));
			}

			[Test]
			public void TestWaitsForTimeoutBeforeExit()
			{
				// Arrange
				var timeout = TimeSpan.FromSeconds(5);
				var stopwatch = Stopwatch.StartNew();

				// Act
				backgroundThreadActionQueue!.InvokeActionsWhileWaiting(timeout, () => false);

				// Assert
				Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(timeout));
			}

			[Test]
			public void TestLeavesActionsIfTimedOut()
			{
				// Arrange
				var count = 0;
				var timeout = TimeSpan.FromSeconds(5);
				backgroundThreadActionQueue!.Enqueue(() =>
				{
					Interlocked.Increment(ref count);
					Thread.Sleep(timeout.Add(TimeSpan.FromSeconds(1)));
				});
				backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));
				backgroundThreadActionQueue.Enqueue(() => Interlocked.Increment(ref count));

				// Act
				backgroundThreadActionQueue.InvokeActionsWhileWaiting(timeout, () => false);

				// Assert
				Assert.That(count, Is.EqualTo(1));
			}

			[Test]
			public void TestBailsFromActionsIfBailoutConditionIsMet_1() => AssertBailsFromActionsIfBailoutConditionIsMet(5, 1);
			[Test]
			public void TestBailsFromActionsIfBailoutConditionIsMet_2() => AssertBailsFromActionsIfBailoutConditionIsMet(10, 2);
			[Test]
			public void TestBailsFromActionsIfBailoutConditionIsMet_3() => AssertBailsFromActionsIfBailoutConditionIsMet(3, 3);

			void AssertBailsFromActionsIfBailoutConditionIsMet(int numberOfActionsInQueue, int numberOfActionsToProcess)
			{
				// Arrange
				var numberOfActionsProcessed = 0;
				for (var i = 0; i < numberOfActionsInQueue; i++)
				{
					backgroundThreadActionQueue!.Enqueue(() => numberOfActionsProcessed++);
				}

				// Act
				backgroundThreadActionQueue!.InvokeActionsWhileWaiting(TimeSpan.FromSeconds(5), () => numberOfActionsProcessed == numberOfActionsToProcess);

				// Assert
				Assert.That(numberOfActionsToProcess, Is.EqualTo(numberOfActionsProcessed));
			}

			[Test]
			public void TestChecksCurrentThread()
			{
				// Arrange
				BackgroundThreadActionQueue? queue = null;
				var thread = new Thread(() =>
				{
					queue = new BackgroundThreadActionQueue(CancellationToken.None);
				});
				thread.Start();
				thread.Join(TimeSpan.FromSeconds(5));

				// Act
				// Assert
				Assert.Throws<InvalidOperationException>(() => queue?.InvokeActionsWhileWaiting(TimeSpan.Zero, () => false));
			}

			[SetUp]
			public void SetUp()
			{
				backgroundThreadActionQueue = new BackgroundThreadActionQueue(CancellationToken.None);
			}

			[TearDown]
			public void TearDown()
			{
				backgroundThreadActionQueue!.Dispose();
			}

			BackgroundThreadActionQueue? backgroundThreadActionQueue;
		}
	}
}
