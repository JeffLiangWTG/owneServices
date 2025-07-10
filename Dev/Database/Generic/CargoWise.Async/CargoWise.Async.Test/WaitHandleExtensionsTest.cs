using System;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Async.Test
{
	class WaitHandleExtensionsTest : TestCase
	{
		public void TestAlreadySignaled()
		{
			using (var handle = new ManualResetEvent(true))
			using (var task = handle.WaitOneAsync(1))
			{
				AssertEquals(true, task.GetAwaiter().GetResult());
			}
		}

		public void TestTimeout()
		{
			using (var handle = new ManualResetEvent(false))
			using (var task = handle.WaitOneAsync(1))
			{
				AssertEquals(false, task.GetAwaiter().GetResult());
			}
		}

		public void TestSignalAfterStarting()
		{
			using (var handle = new ManualResetEvent(false))
			using (var task = handle.WaitOneAsync(1000))
			{
				AssertEquals(false, task.IsCompleted);
				handle.Set();
				AssertEquals(true, task.GetAwaiter().GetResult());
			}
		}

		public void TestNoTimeout()
		{
			using (var handle = new ManualResetEvent(false))
			using (var task = handle.WaitOneAsync())
			{
				AssertEquals(false, task.IsCompleted);

				Thread.Sleep(100);
				AssertEquals(false, task.IsCompleted);

				handle.Set();

				task.GetAwaiter().GetResult();
				AssertEquals(true, task.IsCompleted);
			}
		}

		public void TestImmediateResult_True()
		{
			var signaled = true;
			using (var handle = new ManualResetEvent(signaled))
			using (var task = handle.WaitOneAsync(0))
			{
				var thread = Thread.CurrentThread;
				AssertEquals(true, task.IsCompleted);
				AssertEquals(signaled, task.GetAwaiter().GetResult());
			}
		}

		public void TestImmediateResult_False()
		{
			var signaled = false;
			using (var handle = new ManualResetEvent(signaled))
			using (var task = handle.WaitOneAsync(0))
			{
				var thread = Thread.CurrentThread;
				AssertEquals(true, task.IsCompleted);
				AssertEquals(signaled, task.GetAwaiter().GetResult());
			}
		}

		public void TestCancelAfterStarting()
		{
			using (var cts = new CancellationTokenSource())
			using (var handle = new ManualResetEvent(false))
			{
				var wr = DoStuff(handle, cts);

				FlushObjects();

				AssertEquals(false, wr.IsAlive);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference DoStuff(ManualResetEvent handle, CancellationTokenSource cts)
		{
			var task = handle.WaitOneAsync(cts.Token);
			AssertEquals(false, task.IsCanceled);

			cts.Cancel();
			Thread.Sleep(1000);
			AssertEquals(true, task.IsCanceled);

			var wr = new WeakReference(task);
			return wr;
		}

		public void TestAlreadyCancelled()
		{
			using (var cts = new CancellationTokenSource())
			using (var handle = new ManualResetEvent(false))
			{
				cts.Cancel();

				using (var task = handle.WaitOneAsync(cts.Token))
				{
					AssertEquals(true, task.IsCanceled);

					task.GetAwaiter();
				}
			}
		}

		public void TestKeepTaskReferencedWhileWaiting()
		{
			using (var handle = new ManualResetEvent(false))
			{
				var wr = GetWeakReference(handle);

				FlushObjects();

				AssertEquals(true, wr.IsAlive);

				handle.Set();
				Thread.Sleep(1000);

				FlushObjects();

				AssertEquals(false, wr.IsAlive);
			}
		}

		public void TestDontKeepAliveIfCancellableButNotCancelled()
		{
			// The passed in CancellationTokenSource will hold a reference to the internal TaskCompletionSource so
			// that it can be cancelled when required. This test is to ensure that the reference is broken when
			// the task completes so that we don't have CancellationTokenSource's accumulating completed tasks
			// over time.
			using (var cts = new CancellationTokenSource())
			using (var handle = new ManualResetEvent(false))
			{
				var wr = GetWeakReference(handle, cts.Token);

				FlushObjects();
				AssertEquals("Task should not have been collected.", true, wr.IsAlive);

				handle.Set();

				// Sleep to give the continuation tasks a chance to run and unregister everything.
				// (the continuation tasks will run outside the returned task)
				Thread.Sleep(100);

				FlushObjects();
				AssertEquals("Task should have been collected.", false, wr.IsAlive);
			}
		}

		#region Implementation

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference GetWeakReference(ManualResetEvent handle)
		{
			return new WeakReference(handle.WaitOneAsync());
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference GetWeakReference(ManualResetEvent handle, CancellationToken token)
		{
			return new WeakReference(handle.WaitOneAsync(token));
		}

		static void FlushObjects()
		{
			GC.Collect(); // This is a unit test
			GC.WaitForPendingFinalizers(); // This is a unit test
			GC.Collect(); // This is a unit test
			GC.WaitForPendingFinalizers(); // This is a unit test
		}

		protected override void TearDown()
		{
			try
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
			finally
			{
				base.TearDown();
			}
		}

		#endregion
	}
}
