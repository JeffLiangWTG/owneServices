using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common.Async;
using NUnit.Framework;

namespace CargoWise.Common.Testing.Async
{
	internal class ActionObserverTest : TestCase
	{
		public void TestActionIsPerformedOnSameThreadThatRunObservableTaskIsRunOn()
		{
			int runnerTaskThreadId = 0;

			using (CancellationTokenSource runnerCts = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() => { runnerTaskThreadId = Thread.CurrentThread.ManagedThreadId; }, () =>
				{
					return false;
				}, TimeSpan.FromSeconds(1), runnerCts);
			}

			AssertEquals(Thread.CurrentThread.ManagedThreadId, runnerTaskThreadId);
		}

		public void TestObserverThreadIsNamedCorrectly()
		{
			string expectedThreadName = "Action Observer";
			string threadName = "";

			using (CancellationTokenSource runnerCts = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() =>
				{
					runnerCts.Token.WaitHandle.WaitOne(210);
				},
					() =>
					{
						threadName = Thread.CurrentThread.Name;
						return false;
					}, TimeSpan.FromMilliseconds(100), runnerCts);
			}

			Assert($"Thread name should be: {expectedThreadName}. Thread name is actually {threadName}.", threadName.Equals(expectedThreadName));
		}

		public void TestCanceledTaskDoesNotRunUntilCompletion()
		{
			bool ranToCompletion = false;

			using (CancellationTokenSource runnerCts = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(
					() =>
					{
						int i = 0;
						while (i < 5)
						{
							runnerCts.Token.WaitHandle.WaitOne(2000);
							runnerCts.Token.ThrowIfCancellationRequested();
							i++;
						}
						ranToCompletion = true;
					}, () =>
					{
						return true;
					},
					TimeSpan.FromSeconds(1),
					runnerCts);
			}

			Assert(!ranToCompletion);
		}

		public void TestCancellationCallbackIsCalledOnce()
		{
			int callCount = 0;

			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() =>
				{
					int i = 0;
					while (i < 5)
					{
						runnerCTS.Token.WaitHandle.WaitOne(100);
						runnerCTS.Token.ThrowIfCancellationRequested();
						i++;
					}
				}, () =>
				{
					callCount++;
					return true;
				}, TimeSpan.FromMilliseconds(100), runnerCTS);
			}

			Assert(callCount == 1);
		}

		public void TestCancellationCallbackIsNotCalledImmediately()
		{
			var observerInterval = TimeSpan.FromMilliseconds(500);
			var stopWatch = Stopwatch.StartNew();

			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() =>
				{
					while (true)
					{
						runnerCTS.Token.WaitHandle.WaitOne(1000);
						runnerCTS.Token.ThrowIfCancellationRequested();
					}
				}, () =>
				{
					return true;
				}, observerInterval, runnerCTS);
			}

			Assert($"The task is cancelled after {stopWatch.Elapsed}. It should be after {observerInterval}.", stopWatch.Elapsed.Milliseconds >= observerInterval.Milliseconds - 16); // 1000ms/64 ~= 16ms is the accuracy of windows low precision timers
		}

		public void TestRunnerActionIsCalled()
		{
			bool isCalled = false;
			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() =>
				{
					isCalled = true;
				}, () => { return true; }, TimeSpan.FromSeconds(2), runnerCTS);
			}

			Assert(isCalled);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestExceptionThrownWhenRunnerActionNotProvided()
		{
			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(null, () =>
				{
					return true;
				}, TimeSpan.FromSeconds(1), runnerCTS);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestExceptionThrownWhenObserverFuncNotProvided()
		{
			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() => { }, null, TimeSpan.FromSeconds(1), runnerCTS);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestExceptionThrownWhenRunnerCancellationTokenNotProvided()
		{
			ActionObserver.RunObservableTask(() => { }, () => { return true; }, TimeSpan.FromSeconds(1), null);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestNegativeObserverInterval()
		{
			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() => { }, () => { return true; }, TimeSpan.MinValue, runnerCTS);
			}
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestZeroObserverInterval()
		{
			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() => { }, () => { return true; }, TimeSpan.Zero, runnerCTS);
			}
		}

		public void TestRunnerCancellationTokenSourceCancelsRunner()
		{
			bool ranTillCompletion = false;

			using (var runnerCTS = new CancellationTokenSource())
			{
				runnerCTS.CancelAfter(TimeSpan.FromMilliseconds(500));
				ActionObserver.RunObservableTask(() =>
				{
					int i = 0;
					while (i < 5)
					{
						runnerCTS.Token.WaitHandle.WaitOne(500);
						runnerCTS.Token.ThrowIfCancellationRequested();
						i++;
					}
					ranTillCompletion = true;
				}, () => { return false; }, TimeSpan.FromMilliseconds(500), runnerCTS);
			}

			Assert(!ranTillCompletion);
		}

		public void TestRunnerCancellationTokenSourceCancelsObserver()
		{
			bool ranTillCompletion = false;

			using (var runnerCTS = new CancellationTokenSource())
			{
				runnerCTS.CancelAfter(TimeSpan.FromMilliseconds(500));
				ActionObserver.RunObservableTask(() =>
				{
					runnerCTS.Token.WaitHandle.WaitOne(3000);
				}, () =>
				{
					ranTillCompletion = true;
					return false;
				}, TimeSpan.FromMilliseconds(1000), runnerCTS);
			}

			Assert(!ranTillCompletion);
		}

		public void TestObserverCancelsImmediatelyAfterRunnerFinished()
		{
			bool runnerRanTillCompletion = false;
			bool observerRanTillCompletion = false;

			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() =>
				{
					runnerCTS.Token.WaitHandle.WaitOne(100);
					runnerCTS.Token.ThrowIfCancellationRequested();

					runnerRanTillCompletion = true;
				}, () =>
				{
					runnerCTS.Token.WaitHandle.WaitOne(1000);
					runnerCTS.Token.ThrowIfCancellationRequested();

					observerRanTillCompletion = true;
					return false;
				}, TimeSpan.FromMilliseconds(500), runnerCTS);
			}

			Assert(runnerRanTillCompletion);
			Assert(!observerRanTillCompletion);
		}

		[ExpectNoExceptions]
		public void TestObserverDoesNotThrowIfRunnerFinishesFirst()
		{
			using (var runnerCTS = new CancellationTokenSource())
			{
				ActionObserver.RunObservableTask(() =>
				{
					runnerCTS.Token.WaitHandle.WaitOne(100);
				}, () =>
				{
					return true;
				}, TimeSpan.FromMilliseconds(500), runnerCTS);
			}
		}

		public void TestObserverDoesNotThrowOnCancellation()
		{
			bool taskIsCancelled = false;

			using (var runnerCTS = new CancellationTokenSource())
			{
				taskIsCancelled = ActionObserver.RunObservableTask(() =>
				{
					runnerCTS.Token.WaitHandle.WaitOne(2000);
					runnerCTS.Token.ThrowIfCancellationRequested();
				}, () =>
				{
					return true;
				}, TimeSpan.FromMilliseconds(500), runnerCTS);
			}

			Assert(taskIsCancelled);
		}
	}
}
