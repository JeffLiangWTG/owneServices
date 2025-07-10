using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Async.AsyncTaskContext.Public;
using CargoWise.Common;
using CargoWise.Common.Testing;
using Moq;
using NUnit.Framework;

namespace CargoWise.Async.Test
{
	[GuiTest]
	class ThreadSentryTest : TestCase
	{
		public void TestEnsureCurrentThreadIsOwner_DoesNotProduceDeadLock()
		{
			InvokeThreadSentryFromMultipleThreads_WithPotentialDeadLocks(threadSentry => threadSentry.EnsureCurrentThreadIsOwner(), ThreadSentry.OnlyOwnerThreadCanAccessThisObjectMessage);
		}

		public void TestRelinquishThreadOwnership_DoesNotProduceDeadLock()
		{
			InvokeThreadSentryFromMultipleThreads_WithPotentialDeadLocks((threadSentry) => threadSentry.RelinquishThreadOwnership(), ThreadSentry.OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage);
		}

		public void TestForciblyRelinquishThreadOwnership_DoesNotProduceDeadLock()
		{
			InvokeThreadSentryFromMultipleThreads_WithPotentialDeadLocks((threadSentry) => threadSentry.ForciblyRelinquishThreadOwnership_ForTest(), ThreadSentry.OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage);
		}

		public void TestTakeThreadOwnership_DoesNotProduceDeadLock()
		{
			InvokeThreadSentryFromMultipleThreads_WithPotentialDeadLocks((threadSentry) => threadSentry.TakeThreadOwnership(), ThreadSentry.CanNotTakeThreadOwnershipOfAnOwnedObjectMessage);
		}

		static void InvokeThreadSentryFromMultipleThreads_WithPotentialDeadLocks(Action<ThreadSentry> threadSentryMethodThatWillCallReportThreadError, string expectedErrorMessage)
		{
			// Amnesty failures (see WI00850107) at times have reported undisposed
			// objects of type CargoWise.Common.DisposableAction.
			// Enabling stack trace will help find when/where these objects were created.
			// If the issue is not reported in another couple of months, we can remove
			// this code. Today's date: 22-01-2025
			DisposableLeakListener.Instance.StackTraceEnabled = true;

			// Arrange
			var lockObj = new object();
			var threadSentry = (ThreadSentry)ThreadSentryProvider.GetThreadSentry(true);
			var lockObjIsLockedEvent = new AutoResetEvent(false);
			var errorReporterMock = new Mock<IErrorReporter>();
			var threadErrorMessage = string.Empty;

			errorReporterMock
				.Setup(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback((string key, string message, Exception exception) =>
				{
					lockObjIsLockedEvent.WaitOne(); // lockObj was acquired by the other thread

					lock (lockObj) // deadlock point - wait for the lockObj to be released by the other thread
					{
						// if the other thread does NOT have to wait for the threadSentry lock
						// acquired by nameof(threadSentryMethodThatWillCallReportThreadError),
						// it would finish it's work and release the lockObj in no time.
					}

					threadErrorMessage = message;
				});

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				var threads = new[]
				{
					new Thread(() =>
					{
						threadSentryMethodThatWillCallReportThreadError.Invoke(threadSentry); // thread 0 acquired the lock to the thread sentry
					}),
					new Thread(() =>
					{
						lock (lockObj) // thread 1 acquired the lock to the lockObj
						{
							lockObjIsLockedEvent.Set();

							using (threadSentry.ForcefullyBorrowThreadOwnership()) // deadlock point - wait for thread sentry lock to be released by the other thread
							{
							}
						}
					}),
				};

				threads.ForEach(t => t.Start());
				threads.ForEach(t => t.Join());

				// Assert
				Assert("All threads have exited with no deadlock", threads.All(t => !t.IsAlive));
				Assert(threadErrorMessage, threadErrorMessage.StartsWith(expectedErrorMessage));
			}
		}

		public void TestPostAndRelinquish()
		{
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			var flagged = false;

			DoOnBackgroundThread(() => ts.Post(s => { flagged = true; }, null, nameof(TestPostAndRelinquish)));
			ts.RelinquishThreadOwnership();
			AssertContextAndDoEvents();
			Assert("Should not flag, because sentry was reliquished before post", !flagged);
		}

		public void TestPostAndRelinquishAndRejoin()
		{
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			var flagged = false;

			DoOnBackgroundThread(() => ts.Post(s => { flagged = true; }, null, nameof(TestPostAndRelinquishAndRejoin)));
			ts.RelinquishThreadOwnership();
			ts.TakeThreadOwnership();
			AssertContextAndDoEvents();
			Assert(flagged);
		}

		public void TestPostAndRelinquish_AndRejoin()
		{
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			var postsFired = 0;

			DoOnBackgroundThread(() => ts.Post(s => { postsFired++; }, null, nameof(TestPostAndRelinquish_AndRejoin)));
			ts.RelinquishThreadOwnership();
			AssertContextAndDoEvents();
			AssertEquals("Can't consume post queue.", 0, postsFired);

			ts.TakeThreadOwnership();
			AssertContextAndDoEvents();
			AssertEquals("Doesn't consume post queue. Note: This is not necessarily desired behaviour. Just a side effect of the implementation I chose.", 0, postsFired);

			DoOnBackgroundThread(() => ts.Post(s => { /*Do nothing*/ }, null, nameof(TestPostAndRelinquish_AndRejoin)));
			AssertContextAndDoEvents();
			AssertEquals("Completes post queue", 1, postsFired);
		}

		public void TestPostOnRelinquished_AndPostOnRejoined()
		{
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			var postsFired = 0;

			ts.RelinquishThreadOwnership();
			DoOnBackgroundThread(() => ts.Post(s => { postsFired++; }, null, nameof(TestPostOnRelinquished_AndPostOnRejoined)));
			AssertContextAndDoEvents();
			AssertEquals("Can't consume post queue.", 0, postsFired);

			ts.TakeThreadOwnership();
			AssertContextAndDoEvents();
			AssertEquals("Is not prompted to consume post queue", 0, postsFired);

			DoOnBackgroundThread(() => ts.Post(s => { postsFired++; }, null, nameof(TestPostOnRelinquished_AndPostOnRejoined)));
			AssertContextAndDoEvents();
			AssertEquals("Only completes the post that occured when was postable.", 1, postsFired);
		}

		public void TestIsPostable()
		{
#if NET
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
#endif
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			AssertEquals(true, ts.IsPostable);
			ts.RelinquishThreadOwnership();
			AssertEquals(false, ts.IsPostable);
			ts.TakeThreadOwnership();
			AssertEquals(true, ts.IsPostable);
		}

		public void TestPostOnSameThread()
		{
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			var flagged = false;
			ts.Post(s => { flagged = true; }, null, nameof(TestPostOnSameThread));
			Assert(flagged);
		}

		public void TestPostOnNoSyncContext()
		{
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			ts.RelinquishThreadOwnership();
			var flagged = false;
			ts.Post(s => { flagged = true; }, null, nameof(TestPostOnNoSyncContext));
			Assert(!flagged);
		}

		public void TestPostFromBackgroundThreadToGui()
		{
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			var flagged = false;
			var thread = new Thread(() =>
			{
				ts.Post(s => { flagged = true; }, null, nameof(TestPostFromBackgroundThreadToGui));
			});
			thread.Start();
			thread.Join();

			AssertContextAndDoEvents();

			Assert(flagged);
		}

		public void TestPostOnNonGuiThread()
		{
			IThreadSentry ts = null;
			var flagged = false;
			var thread = new Thread(() =>
			{
				ts = ThreadSentryProvider.GetThreadSentry(true);
			});
			thread.Start();
			thread.Join();
			ts.Post(s => { flagged = true; }, null, nameof(TestPostOnNonGuiThread));
			Assert(!flagged);
		}

		public void TestReport_OnlyOnce_WhenEnabled()
		{
			if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
			{
				Thread.CurrentThread.Name = "Current Test Thread";
			}

			var protectedObject = new SentryProtectedObject();

			int currentThreadId = -1;

			for (var i = 0; i < 2; i++)
			{
				var thread = new Thread(() =>
				{
					protectedObject.Property = 10;
				});
				thread.Name = "Test Thread " + i;

				if (currentThreadId < 0)
				{
					currentThreadId = thread.ManagedThreadId;
				}

				thread.Start();
				thread.Join();
			}

			AssertContains(ThreadSentry.OnlyOwnerThreadCanAccessThisObjectMessage, ErrorReporter.LastExceptionReported.Message);
#if NETFRAMEWORK
			AssertContains($"Creation and Owner Thread, Name: [Current Test Thread], ID: {Environment.CurrentManagedThreadId}, Stacktrace:", ErrorReporter.LastExceptionReported.Message);
#endif
#if Net
			AssertContains($"Creation and Owner Thread, Name: [.Net Long Running Task], ID: {Environment.CurrentManagedThreadId}, Stacktrace:", ErrorReporter.LastExceptionReported.Message);
#endif
			AssertContains($"Current Thread, Name: [Test Thread 0], ID: {currentThreadId}, Stacktrace:", ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestReport_OnlyOnce_WhenEnabled_STA()
		{
			var protectedObject = new SentryProtectedObject();

			for (var i = 0; i < 2; i++)
			{
				var thread = new Thread(() =>
				{
					protectedObject.Property = 1;
				});
				thread.SetApartmentState(ApartmentState.STA);

				thread.Start();
				thread.Join();
			}

			AssertContains(ThreadSentry.OnlyOwnerThreadCanAccessThisObjectMessage, ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestReportingDisabled()
		{
			var protectedObject = new SentryProtectedObject(false);

			var thread = new Thread(() =>
			{
				protectedObject.Property = 21;
			});

			thread.Start();
			thread.Join();

			AssertEquals(null, ErrorReporter.LastExceptionReported);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestReport_OnlyOnce_NoStack()
		{
			if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
			{
				Thread.CurrentThread.Name = "Current Test Thread";
			}

			var protectedObject = new SentryProtectedObject(stacktraceEnabled: false);

			for (var i = 0; i < 2; i++)
			{
				var thread = new Thread(() =>
				{
					protectedObject.Property = 10;
				});
				thread.Name = "Test Thread " + i;

				thread.Start();
				thread.Join();
			}

			AssertContains("Full stack disabled", ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestThreadSentryErrorReportingChanged()
		{
			var protectedObject = new SentryProtectedObject();

			var thread = new Thread(() =>
			{
				protectedObject.Property = 21;
			});

			thread.Start();
			thread.Join();

			AssertContains(ThreadSentry.OnlyOwnerThreadCanAccessThisObjectMessage, ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();

			ReportErrorWithDifferentStackTrace(protectedObject);

			AssertEquals(null, ErrorReporter.LastExceptionReported);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestRelinquishThreadOwnershipShouldReportWhenChangingOwnershipIsNotAllowed()
		{
			var ts = ThreadSentryProvider.GetThreadSentry(true, allowChangingThreadOwnership: false);
			ts.RelinquishThreadOwnership();

			AssertContains(ThreadSentry.RelinquishingOwnershipIsNotAllowed, ErrorReporter.LastExceptionReported.Message);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		public void TestRelinquishThreadOwnershipShouldNotReportWhenChangingOwnershipIsNotAllowed_ButRelinquishIsForced()
		{
			var ts = ThreadSentryProvider.GetThreadSentry(true, allowChangingThreadOwnership: false);
			ts.ForciblyRelinquishThreadOwnership_ForTest();

			AssertNull(ErrorReporter.LastExceptionReported);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestPostCallbackNullCallback()
		{
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			var flagged = false;
			ts.Post(s => { flagged = true; }, null, null, null, nameof(TestPostCallbackNullCallback));
			Assert(flagged);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestPostCallbackNullThreadSentry()
		{
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			ts.Post(s => { }, null, null, s => { }, nameof(TestPostCallbackNullThreadSentry));
		}

		public void TestPostCallbackSingleThreaded()
		{
			var ts = ThreadSentryProvider.GetThreadSentry(true);
			var postFlagged = false;
			var callbackFlagged = false;
			ts.Post(s => { postFlagged = true; }, null, ts, s => { callbackFlagged = true; }, nameof(TestPostCallbackSingleThreaded));
			Assert(postFlagged);
			Assert(callbackFlagged);
		}

		public void TestPostToCurrentThreadCallbackOnBackgroundThread()
		{
			PostToCurrentThreadCallbackOnBackgroundThreadCore();
		}

		public void TestPostToCurrentThreadCallbackOnBackgroundThread_ExceptionOnPost()
		{
			PostToCurrentThreadCallbackOnBackgroundThreadCore(() => throw new Exception());
			var matches = Regex.IsMatch(ErrorReporter.LastMessageReported, $"Error processing action dispatched to ThreadSentry. Calling Method: {nameof(ThreadSentryTest)}.{nameof(PostToCurrentThreadCallbackOnBackgroundThreadCore)}");
			AssertEquals($"Message regex mismatch. Original Message:\r\n{ErrorReporter.LastMessageReported}", true, matches);
			ErrorReporter.Clear();
		}

		static void PostToCurrentThreadCallbackOnBackgroundThreadCore(Action additionalAction = null)
		{
			var uiThreadSentry = ThreadSentryProvider.GetThreadSentry(true);
			IThreadSentry backgroundThreadSentry = null;

			var step = 1;
			var callbackFlagged = false;
			var thread = new Thread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
				backgroundThreadSentry = ThreadSentryProvider.GetThreadSentry(true);
				step = 2;

				SleepUntil(ref step, 3);

				Assert(!callbackFlagged);

				AssertContextAndDoEvents();
				step = 4;
			});
			thread.Start();

			SleepUntil(ref step, 2);

			var postFlagged = false;
			uiThreadSentry.Post(s => { postFlagged = true; }, null, backgroundThreadSentry,
				s => { callbackFlagged = true; additionalAction?.Invoke(); }, $"{nameof(ThreadSentryTest)}.{nameof(PostToCurrentThreadCallbackOnBackgroundThreadCore)}");
			Assert(postFlagged);
			step = 3;

			SleepUntil(ref step, 4);

			Assert($"Callback was not flagged. Step was at {step}.", callbackFlagged);

			thread.Join();
		}

		public void TestPostToBackgroundThreadCallbackOnCurrentThread()
		{
			var step = 1;
			IThreadSentry backgroundThreadSentry = null;
			var postFlagged = false;
			var thread = new Thread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
				backgroundThreadSentry = ThreadSentryProvider.GetThreadSentry(true);
				step = 2;

				SleepUntil(ref step, 3);

				AssertContextAndDoEvents();
				step = 4;
			});
			thread.Start();

			SleepUntil(ref step, 2);

			var callbackFlagged = false;
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
			var uiThreadSentry = ThreadSentryProvider.GetThreadSentry(true);
			backgroundThreadSentry.Post(s => { postFlagged = true; }, null, uiThreadSentry, s => { callbackFlagged = true; }, nameof(TestPostToBackgroundThreadCallbackOnCurrentThread));
			step = 3;

			SleepUntil(ref step, 4);

			Assert(postFlagged);
			Assert(!callbackFlagged);

			AssertContextAndDoEvents();
			Assert(callbackFlagged);

			thread.Join();
		}

		public void TestIsPostableProcess()
		{
#if NET
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
#endif
			var initialValue = ThreadSentry.IsPostableProcess;
			try
			{
				ThreadSentry.IsPostableProcess = true;
				AssertEquals(true, new ThreadSentry().IsPostable);
				ThreadSentry.IsPostableProcess = false;
				AssertEquals(false, new ThreadSentry().IsPostable);
			}
			finally
			{
				ThreadSentry.IsPostableProcess = initialValue;
			}
		}

		public void TestPostingDisabledForDefaultSynchronizationContext()
		{
			var intialSyncContext = SynchronizationContext.Current;
			try
			{
				SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
				AssertEquals(false, new ThreadSentry().IsPostable);
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(intialSyncContext);
			}
		}

		public void TestPostingDisabledForAsyncTaskSynchronizer()
		{
			var isPostable = true;
			AsyncTaskSynchronizer.Run(() =>
			{
				isPostable = new ThreadSentry().IsPostable;
				return Task.CompletedTask;
			});
			AssertEquals(false, isPostable);
		}

		public void TestPostingDisabledForBackgroundThreadTaskManager()
		{
			var tcs = new TaskCompletionSource<bool>();
			using (var taskContext = TaskContext.CreateThreadTaskContext())
			{
				taskContext.EnqueueTask(() => tcs.SetResult(new ThreadSentry().IsPostable));
				tcs.Task.Wait();
				AssertEquals(false, tcs.Task.Result);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Assert("Precondition: Environment must be user interactive for ThreadSentry.IsPostable to be true", Environment.UserInteractive);
		}

		static void AssertContextAndDoEvents()
		{
			AssertEquals("Precondition: Synchronization context must be WindowsFormsSynchronizationContext for Application.DoEvents()", "WindowsFormsSynchronizationContext", SynchronizationContext.Current?.GetType().Name);
			Application.DoEvents();
#if NET
			SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
#endif
		}

		static void ReportErrorWithDifferentStackTrace(SentryProtectedObject protectedObject)
		{
			var thread = new Thread(() =>
			{
				protectedObject.Property = 21;
			});

			thread.Start();
			thread.Join();
		}

		sealed class SentryProtectedObject
		{
			public SentryProtectedObject(bool reportingEnabled = true, bool stacktraceEnabled = true)
			{
				threadSentry = ThreadSentryProvider.GetThreadSentry(reportingEnabled, takeStackTraces: stacktraceEnabled);
			}

			readonly IThreadSentry threadSentry;

			internal int Property
			{
				set
				{
					threadSentry.EnsureCurrentThreadIsOwner();
				}
			}
		}

		#region Utils

		void DoOnBackgroundThread(Action action)
		{
			var thread = new Thread(() => action());
			thread.Start();
			thread.Join();
		}

		static void SleepUntil(ref int step, int stepCondition)
		{
			for (var i = 0; ((i < 500) && step != stepCondition); ++i)
			{
				Thread.Sleep(15);
			}
			AssertEquals(stepCondition, step);
		}

		#endregion
	}
}
