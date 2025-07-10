using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Async.Test
{
	class AsyncStrategyTest : TestCase
	{
		public void TestDoAsync_ShouldUseCallerMethodName()
		{
			var methodName = string.Empty;

			var task = strategy.DoAsync(() => methodName = Thread.CurrentThread.Name);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("DoEvents in WaitForThreadError should not thrown Exception", () => WaitTask(task));
				AssertEquals("TestDoAsync_ShouldUseCallerMethodName", methodName);
			});
		}

		[GuiTest]
		public void TestDoAsync_ExceptionHandling()
		{
			var task = strategy.DoAsync(() =>
			{
				throw new Exception("I should be rethrown on the thread the test started on (TestDoAsync_ExceptionHandling)");
			});

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("DoEvents in WaitForThreadError should not thrown Exception", () => WaitTask(task));
				AssertNotNull("LastExceptionReported should not be null.", ErrorReporter.LastExceptionReported);
				AssertNotNull("LastExceptionReported's Inner Exception should not be null." + ErrorReporter.LastExceptionReported.ToString(), ErrorReporter.LastExceptionReported.InnerException);
				AssertEquals("I should be rethrown on the thread the test started on (TestDoAsync_ExceptionHandling)", GetInnerMostException(ErrorReporter.LastExceptionReported).Message);
			});

			ErrorReporter.Clear();
		}

		[GuiTest]
		public void TestDoAsyncAsThread_ExceptionHandling()
		{
			var task = strategy.DoAsyncAsThread(() =>
			{
				throw new Exception("I should be rethrown on the thread the test started on");
			}, ApartmentState.STA);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("DoEvents in WaitForThreadError should not thrown Exception", () => task.Join());
				Application.DoEvents();
				AssertNotNull("LastExceptionReported should not be null.", ErrorReporter.LastExceptionReported);
				AssertStartsWith("We expect this with the thread ID", "An exception occurred on another thread", ErrorReporter.LastExceptionReported.Message);
				AssertEquals("I should be rethrown on the thread the test started on", GetInnerMostException(ErrorReporter.LastExceptionReported).Message);
			});

			GC.Collect();
			GC.WaitForPendingFinalizers();
			ErrorReporter.Clear();
		}

		[GuiTest]
		public void TestDoAsync_ExceptionHandling_CriticalException()
		{
			var task = strategy.DoAsync(() =>
			{
				new Action(() => throw new TestCriticalException("I am a banana")).DynamicInvoke();
			});
			while (!task.IsCompleted)
			{
				Thread.Sleep(1); // spin
			}

			AssertNull(ErrorReporter.LastExceptionReported);
		}

		public void TestDoAsyncAsThread_ShouldAddThreadToActiveSTAThreads()
		{
			AssertContainsExactElementsInAnyOrder("The dictionary should be empty at the start because we haven't created any STA threads, and yet...", Array.Empty<string>(), DefaultAsyncStrategy.GetActiveSTAThreads().Select(x => x.Name));
			IEnumerable<string> staThreadsWhileThreadWasRunning = Array.Empty<string>();

			var task = strategy.DoAsyncAsThread(() =>
			{
				staThreadsWhileThreadWasRunning = DefaultAsyncStrategy.GetActiveSTAThreads().Select(x => x.Name);
			}, ApartmentState.STA, null, "Don't squarsh ma thread!");
			task.Join();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("DoEvents in WaitForThreadError should not thrown Exception", () => task.Join());
				AssertContainsExactElementsInAnyOrder("The new thread should be in the dictionary while that thread was running, and yet...", new[] { "Don't squarsh ma thread!" }, staThreadsWhileThreadWasRunning);
				AssertContainsExactElementsInAnyOrder("The dictionary should be empty again because the new thread has finished, and yet...", Array.Empty<string>(), DefaultAsyncStrategy.GetActiveSTAThreads().Select(x => x.Name));
			});
		}

		public void TestDoAsync_ShouldNotAddThreadToActiveSTAThreads()
		{
			AssertContainsExactElementsInAnyOrder("The dictionary should be empty at the start because we haven't created any STA threads, and yet...", Array.Empty<string>(), DefaultAsyncStrategy.GetActiveSTAThreads().Select(x => x.Name));
			IEnumerable<string> staThreadsWhileThreadWasRunning = Array.Empty<string>();

			var task = strategy.DoAsync(() =>
			{
				staThreadsWhileThreadWasRunning = DefaultAsyncStrategy.GetActiveSTAThreads().Select(x => x.Name);
			}, null, "Don't squarsh ma thread!");

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("DoEvents in WaitForThreadError should not thrown Exception", () => WaitTask(task));
				AssertContainsExactElementsInAnyOrder("The new thread should not be in the dictionary while it was running because it wasn't STA, and yet...", Array.Empty<string>(), staThreadsWhileThreadWasRunning);
				AssertContainsExactElementsInAnyOrder("The dictionary should still be empty because no STA threads are running, and yet...", Array.Empty<string>(), DefaultAsyncStrategy.GetActiveSTAThreads().Select(x => x.Name));
			});
		}

		[GuiTest]
		public void TestGetAsync_ExceptionHandling()
		{
			var testFunc = new Func<int>(() =>
			{
				throw new InvalidOperationException("I should be rethrown on the thread the test started on");
			});

			var task = strategy.GetAsync(() => testFunc());

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("DoEvents in WaitForThreadError should not thrown Exception", () => WaitTask(task));
				AssertStartsWith("We expect this with the thread ID", "An exception occurred on another thread", ErrorReporter.LastExceptionReported.Message);
				AssertEquals("I should be rethrown on the thread the test started on", GetInnerMostException(ErrorReporter.LastExceptionReported).Message);
			});

			ErrorReporter.Clear();
		}

		[GuiTest]
		public void TestGetAsync_ExceptionHandling_CancellationToken()
		{
			var testFunc = new Func<int>(() =>
			{
				var cts = new CancellationTokenSource();
				cts.Dispose();
				cts.Token.ThrowIfCancellationRequested();
				return -1;
			});

			var task = strategy.GetAsync(() => testFunc());
			_ = AssertExceptionThrown<ObjectDisposedException>(() => WaitTask(task));
			AssertNull(ErrorReporter.LastExceptionReported);
		}

		static Exception GetInnerMostException(Exception exception)
		{
			var currentException = exception;

			while (currentException.InnerException != null)
			{
				currentException = currentException.InnerException;
			}

			return currentException;
		}

		static void WaitTask(Task task)
		{
			try
			{
				var awaiter = task.ConfigureAwait(false).GetAwaiter();

				//TO Avoid:  A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread.
				//https://stackoverflow.com/questions/7883052/a-tasks-exceptions-were-not-observed-either-by-waiting-on-the-task-or-accessi
				ExceptionDispatchInfo innerException = null;
				_ = task.ContinueWith(t =>
				{
					var aggException = t.Exception.Flatten();
					foreach (var exception in aggException.InnerExceptions)
					{
						if (exception.Message != "I should be rethrown on the thread the test started on")
						{
							innerException = ExceptionDispatchInfo.Capture(exception);
							break;
						}
					}
				}, TaskContinuationOptions.OnlyOnFaulted);

				awaiter.GetResult();
				Application.DoEvents();
				innerException?.Throw();
			}
			finally
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
		}

		IAsyncStrategy strategy;

		protected override void SetUp()
		{
			base.SetUp();
			strategy = DefaultAsyncStrategy.Get();
			dispatcher = ApplicationDispatcher.Current;
		}
		SynchronizationContext dispatcher;

		protected override void TearDown()
		{
			base.TearDown();
			ApplicationDispatcher.Current = dispatcher;
		}

		[Serializable]
		class TestCriticalException : Exception, ICriticalException
		{
			public TestCriticalException(string content)
				: base(content)
			{
			}

#if NETFRAMEWORK
			protected TestCriticalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
			public bool IsCriticalException => true;
		}
	}
}
