using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Async.Test
{
	class RegisterDispatcherTest : TestCase
	{
		public void TestRegisterDispatcher()
		{
			RegisterDispatcher.Register();

			AssertNotNull(RegisterDispatcher.GetDispatcher(Thread.CurrentThread.ManagedThreadId));
			AssertEquals(SynchronizationContext.Current, RegisterDispatcher.GetDispatcher(Thread.CurrentThread.ManagedThreadId));

			Assert(condition: RegisterDispatcher.RegisteredThreadIds.Any());

			RegisterDispatcher.Deregister();
			AssertNull(RegisterDispatcher.GetDispatcher(Thread.CurrentThread.ManagedThreadId));

			var thread = new Thread(ThreadFunc);
			thread.Start();
			thread.Join();
		}

		void ThreadFunc()
		{
			RegisterDispatcher.Register();
			AssertNull(RegisterDispatcher.GetDispatcher(Thread.CurrentThread.ManagedThreadId));

			RegisterDispatcher.Deregister();
			AssertNull(RegisterDispatcher.GetDispatcher(Thread.CurrentThread.ManagedThreadId));
		}
	}
}
