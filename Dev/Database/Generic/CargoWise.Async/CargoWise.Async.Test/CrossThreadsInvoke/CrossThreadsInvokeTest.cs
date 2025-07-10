using System;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Async.Test.CrossThreadsInvoke
{
	class CrossThreadsInvokeTest : TestCase
	{
		static int savedThreadId;

		static int threadIdWhichHasDispatcher;
		static int threadIdWhichHasNoDispatcher;

		void SaveThreadId()
		{
			savedThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		public void TestActionByTheSameThread()
		{
			savedThreadId = -1;

			ThreadsInvoke.ActionByThread(Thread.CurrentThread.ManagedThreadId, SaveThreadId);

			AssertNotEquals(savedThreadId, -1);
			AssertEquals(savedThreadId, Thread.CurrentThread.ManagedThreadId);
		}

		public void TestActionByThreadWhichHasDispatcher()
		{
			using (var testSyncContext = SynchronizationContextForTest.Enable())
			{
				RegisterDispatcher.Register();

				savedThreadId = -1;
				threadIdWhichHasDispatcher = Thread.CurrentThread.ManagedThreadId;

				var thread = new Thread(ThreadFunc);
				thread.Start();

				Assert("Expect message to be posted", testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1)));

				thread.Join();

				AssertNotEquals(savedThreadId, -1);
				AssertEquals(savedThreadId, Thread.CurrentThread.ManagedThreadId);

				RegisterDispatcher.Deregister();
			}
		}

		void ThreadFunc()
		{
			ThreadsInvoke.ActionByThread(threadIdWhichHasDispatcher, SaveThreadId);
		}

		public void TestActionByThreadWhichHasNoDispatcher()
		{
			savedThreadId = -1;
			threadIdWhichHasNoDispatcher = -1;

			var thread = new Thread(ThreadFuncWhichHasNoDispatcher);
			thread.Start();

			for (int i = 0; i < 4; i++)
			{
				if (threadIdWhichHasNoDispatcher != -1)
				{
					break;
				}

				Thread.Sleep(5);
			}

			if (threadIdWhichHasNoDispatcher != -1)
			{
				ThreadsInvoke.ActionByThread(threadIdWhichHasNoDispatcher, SaveThreadId);
			}

			thread.Join();

			AssertEquals(savedThreadId, -1);
		}

		void ThreadFuncWhichHasNoDispatcher()
		{
			RegisterDispatcher.Register();
			threadIdWhichHasNoDispatcher = Thread.CurrentThread.ManagedThreadId;

			for (int i = 0; i < 20; i++)
			{
				Thread.Sleep(1);
			}

			RegisterDispatcher.Deregister();
		}
	}
}
