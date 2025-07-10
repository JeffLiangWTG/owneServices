using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common.Async.Testing
{
	/// <summary>
	/// This class allows you to run snippets of code synchronously on alternating threads. It will switch between the main thread and an 'other' thread for each piece
	/// added through ThenOnOther/ThenOnMain. Under normal circumstances this is completely useless, because you want threads to run independantly, however it's 
	/// useful for testing.
	/// 
	/// Some unit tests require multiple threads to reproduce the original issue, usually due to some threadstatic data that needs initialisation 
	/// or some kind of complex race condition. For these tests you usually need snippets of code to run on alternating threads, however thanks 
	/// to initialisation, error handling and deadlocking it can be a fairly complex task.
	/// 
	/// This will handle switching, deadlocking and exceptions. See the tests for sample snippets.
	/// </summary>
	public class ThreadSwapper
	{
		readonly Queue<Action> methods = new Queue<Action>();
		readonly Func<IDisposable> setup;

		Exception exceptionThatOccured;

		ThreadSwapper(Func<IDisposable> setup)
			=> this.setup = setup;

		public static ThreadSwapper SetupOtherThread(Func<IDisposable> otherThreadSetup = null)
			=> new ThreadSwapper(otherThreadSetup);

		public ThreadSwapper ThenOnOther(Action a)
		{
			if (methods.Count % 2 == 0)
			{
				throw new Exception("Your previous call was on other. Please call in main/other/main/other order for readability's sake");
			}

			methods.Enqueue(a);
			return this;
		}

		public ThreadSwapper ThenOnMain(Action a)
		{
			if (methods.Count % 2 == 1)
			{
				throw new Exception("Your previous call was on main. Please call in main/other/main/other order for readability's sake");
			}

			methods.Enqueue(a);
			return this;
		}

		public void Go()
		{
			if (methods.Count < 2)
			{
				throw new InvalidOperationException("You need to add at least two methods so something can run on the other thread");
			}

			using (var mainEv = new AutoResetEvent(false))
			using (var otherEv = new AutoResetEvent(false))
			{
				var t = new Thread(() =>
				{
					using (setup?.Invoke())
					{
						otherEv.WaitOne();
						RunForThread(otherEv, mainEv);
					}
				});

				t.Start();

				RunForThread(mainEv, otherEv);
			}

			if (exceptionThatOccured != null)
			{
				throw new Exception("Exception occured", exceptionThatOccured);
			}
		}

		void RunForThread(EventWaitHandle myHandle, EventWaitHandle otherHandle)
		{
			try
			{
				while (methods.Count > 0)
				{
					methods.Dequeue()();

					otherHandle.Set();
					if (!WaitForSignalOrDeath(myHandle))
					{
						break;
					}
				}
			}
			catch (Exception ex)
			{
				exceptionThatOccured = ex;
				methods.Clear();
			}
		}

		bool WaitForSignalOrDeath(EventWaitHandle handle)
		{
			while (!handle.WaitOne(1000))
			{
				if (exceptionThatOccured != null || methods.Count == 0)
				{
					return false;
				}
			}

			return true;
		}
	}
}

namespace CargoWise.Common.Async.Testing
{
	public class TestThreadSwapper : TestCase
	{
		public void TestSwapsThreads()
		{
			var mainThreadId = Thread.CurrentThread.ManagedThreadId;
			ThreadSwapper
				.SetupOtherThread()
				.ThenOnMain(() => AssertEquals(mainThreadId, Thread.CurrentThread.ManagedThreadId))
				.ThenOnOther(() => AssertNotEquals(mainThreadId, Thread.CurrentThread.ManagedThreadId))
				.ThenOnMain(() => AssertEquals(mainThreadId, Thread.CurrentThread.ManagedThreadId))
				.ThenOnOther(() => AssertNotEquals(mainThreadId, Thread.CurrentThread.ManagedThreadId))
				.Go();
		}

		public void TestIsNotConcurrent()
		{
			var i = 0;
			ThreadSwapper
				.SetupOtherThread()
				.ThenOnMain(() => AssertEquals(1, ++i))
				.ThenOnOther(() => AssertEquals(2, ++i))
				.ThenOnMain(() => AssertEquals(3, ++i))
				.ThenOnOther(() => AssertEquals(4, ++i))
				.ThenOnMain(() => AssertEquals(5, ++i))
				.ThenOnOther(() => AssertEquals(6, ++i))
				.Go();
		}

		public void TestHandlesExceptionsOnOther()
		{
			var swapper = ThreadSwapper
				.SetupOtherThread()
				.ThenOnMain(() => { })
				.ThenOnOther(() => throw new Exception("Boom"));

			try
			{
				swapper.Go();
			}
			catch (Exception ex)
			{
				AssertContains("Boom", ex.ToString());
			}
		}

		public void TestHandlesExceptionsOnMain()
		{
			var swapper = ThreadSwapper
				.SetupOtherThread()
				.ThenOnMain(() => { })
				.ThenOnOther(() => { })
				.ThenOnMain(() => throw new Exception("Boom"));

			try
			{
				swapper.Go();
			}
			catch (Exception ex)
			{
				AssertContains("Boom", ex.ToString());
			}
		}

		public void TestExceptionThrownOnOthers()
		{
			try
			{
				var swapper = ThreadSwapper
	.SetupOtherThread()
	.ThenOnMain(() => { })
	.ThenOnOther(() => ErrorReporter.ReportOnce("Blah blah", "more blah", new Exception("Boom on other thread mate")))
	.ThenOnMain(() => ErrorReporter.ReportOnce("Blah blah and blah", "Hey mate there are more blah",  new Exception("Boom")));

				swapper.Go();
				var expectedFirstExceptionMessage = @"Type :System.Exception
Message :Boom on other thread mate
Stacktrace :";
				var expectedSecondExceptionMessage = @"Type :Enterprise.ZArchitecture.Environment.DeveloperNotificationException
Message :more blah
Stacktrace :";

				Assert(ErrorReporter.ExceptionsThrown.Select(x => x.Contains(expectedFirstExceptionMessage)).Any());
				Assert(ErrorReporter.ExceptionsThrown.Select(x => x.Contains(expectedSecondExceptionMessage)).Any());
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions] // This will timeout if it fails
		public void TestDoesntHangMainThread()
		{
			RunTestInAnotherThreadWithTimeout(delegate
			{
				ThreadSwapper
					.SetupOtherThread()
					.ThenOnMain(() => { })
					.ThenOnOther(() => { })
					.ThenOnMain(() => { })
					.Go();
			}, TimeSpan.FromSeconds(5));
		}
	}
}
