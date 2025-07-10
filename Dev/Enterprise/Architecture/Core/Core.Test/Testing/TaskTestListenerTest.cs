using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TaskTestListenerTest : TestCase
	{
		public void TestTaskInAwaitingStateReported()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = Task.Delay(TimeSpan.FromMilliseconds(10));
			try
			{
				AssertExceptionThrownContaining(listener, "Status = WaitingForActivation");
			}
			finally
			{
				task.Wait();
			}
		}

		public void TestTaskInRunningStateReported()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = Task.Run(() => Thread.Sleep(TimeSpan.FromMilliseconds(10)));
			try
			{
				AssertExceptionThrown<AssertionFailedError>(() => listener.AfterEachTest(DateTime.UtcNow));
			}
			finally
			{
				task.Wait();
			}
		}

		public void TestCompletedTaskNotReported()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = Task.Delay(TimeSpan.FromMilliseconds(1));
			task.Wait();
			AssertNoExceptionThrown(() => listener.AfterEachTest(DateTime.UtcNow));
		}

		public void TestSameTaskStillRunningInSecondTestNotReported()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = Task.Delay(TimeSpan.FromMilliseconds(100));
			try
			{
				AssertExceptionThrown<AssertionFailedError>(() => listener.AfterEachTest(DateTime.UtcNow));
				AssertNoExceptionThrown(() => listener.AfterEachTest(DateTime.UtcNow));
			}
			finally
			{
				task.Wait();
			}
		}

		public void TestSecondLeakedTaskReported()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = Task.Delay(TimeSpan.FromMilliseconds(10));
			try
			{
				AssertExceptionThrown<AssertionFailedError>(() => listener.AfterEachTest(DateTime.UtcNow));
			}
			finally
			{
				task.Wait();
			}

			task = Task.Delay(TimeSpan.FromMilliseconds(10));
			try
			{
				AssertExceptionThrown<AssertionFailedError>(() => listener.AfterEachTest(DateTime.UtcNow));
			}
			finally
			{
				task.Wait();
			}
		}

		#region Task Descriptions

		public void TestDescription_WhenTaskWasNotRegistered()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = Task.Delay(TimeSpan.FromMilliseconds(10));
			try
			{
				AssertExceptionThrownContaining(listener, "Description = The task with the provided ID was not registered by TaskRegistryForTest. Try using AsyncHelper for creating tasks.");
			}
			finally
			{
				task.Wait();
			}
		}

		public void TestDescription_WhenTaskWasRegistered()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = AsyncHelper.RunTask(() =>
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}, "Task description");

			try
			{
				AssertExceptionThrownContaining(listener, "Description = Task description");
			}
			finally
			{
				task.Wait();
			}
		}

		public void TestDescription_WhenTaskWasRegisteredWithNullDescription()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = AsyncHelper.RunTask(() =>
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}, description: null);

			try
			{
				AssertExceptionThrownContaining(listener, "Description = ");
			}
			finally
			{
				task.Wait();
			}
		}

		public void TestDescription_DoAsync()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = DefaultAsyncStrategy.Get().DoAsync(() =>
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}, threadName: "Task description");

			try
			{
				AssertExceptionThrownContaining(listener, "Description = Task description");
			}
			finally
			{
				task.Wait();
			}
		}

		public void TestDescription_GetAsync()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = DefaultAsyncStrategy.Get().GetAsync(() =>
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(10));
				return true;
			}, threadName: "Task description");

			try
			{
				AssertExceptionThrownContaining(listener, "Description = Task description");
			}
			finally
			{
				task.Wait();
			}
		}

		public void TestDescription_DoAsyncAsThread()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = DefaultAsyncStrategy.Get().DoAsyncAsThread(() =>
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}, threadName: "Task description");

			try
			{
				AssertNoExceptionThrown(() => listener.AfterEachTest(DateTime.UtcNow));
			}
			finally
			{
				task.Join();
			}
		}

		public void TestShouldResetTaskRegistryAfterEachTest()
		{
			var listener = new TaskTestListener();
			listener.StartAllTests(DateTime.UtcNow);
			var task = AsyncHelper.RunTask(() =>
			{
				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}, "Task description");
			try
			{
				Assert(TaskRegistryForTest.TryGetTaskDescriptionById(task.Id, out string description));
				AssertEquals("Task description", description);

				AssertExceptionThrown<AssertionFailedError>(() => listener.AfterEachTest(DateTime.UtcNow));

				Assert(!TaskRegistryForTest.TryGetTaskDescriptionById(task.Id, out description));
			}
			finally
			{
				task.Wait();
			}
		}

		#endregion

		void AssertExceptionThrownContaining(TaskTestListener listener, string expectedMessage)
		{
			var ex = AssertExceptionThrown<AssertionFailedError>(() => listener.AfterEachTest(DateTime.UtcNow));
			var message = ex.Message.Replace("&nbsp;", " ");
			AssertContains($@"Expected message:
{expectedMessage}

Actual message:
{message}", expectedMessage, message);
		}
	}
}
