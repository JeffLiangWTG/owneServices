using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.Testing
{
	[TestedType(typeof(ThreadMonitor))]
	sealed class ThreadMonitorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIntervalMillisecondSafe()
		{
			var monitor = new ThreadMonitor(Thread.CurrentThread);
			AssertEquals("Should default to 1 second", 1000, monitor.IntervalMillisecondSafe);

			monitor.IntervalType = IntervalTypeConstant.Minute;
			AssertEquals(60000, monitor.IntervalMillisecondSafe);

			monitor.IntervalNumber = 80;
			AssertEquals("Should not greater than 1 hour", monitor.MaxMilliseconds, monitor.IntervalMillisecondSafe);
		}

#if !WINZOR
		public void TestLogCallStack_ShouldLogSelectedThread()
		{
			var monitor = new ThreadMonitor(Thread.CurrentThread);
			var callStack = string.Empty;

			DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
			{
				monitor.LogCallStack();
				callStack = monitor.CallStack;
			}, null, "Don't squarsh ma thread!").Join();

			WaitForSingleThreadInList(monitor);
			AssertContains("The main thread should have been monitored because it should be selected by default, and yet...", "at Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)", callStack);

			DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
			{
				monitor.SelectedThreadId = System.Environment.CurrentManagedThreadId;
				monitor.LogCallStack();
				callStack = monitor.CallStack;
			}, null, "Don't squarsh ma thread!").Join();

			WaitForSingleThreadInList(monitor);
			AssertContains("The new thread should have been monitored because it was selected, and yet...", "at System.Threading.ThreadHelper.ThreadStart()", callStack);
		}
#endif

		[RequiresSTA]
		public
#if WINZOR
		async
#endif
		void TestThreadList()
		{
			var monitor = new ThreadMonitor(Thread.CurrentThread);
			var mainThreadId = System.Environment.CurrentManagedThreadId;
			AssertContainsExactElementsInAnyOrder("Only the main thread should be included because no other STA threads have been started, and yet...", new[] { mainThreadId + ": Main Thread" }, monitor.Threads.ToArray().Select(x => x.Code + ": " + x.Description));

			var threadsWhileStaThreadIsRunning = new List<string>();
			var newThreadId = 0;
#if WINZOR
			await DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
			{
				foreach (CodeDescriptionPair pair in monitor.Threads)
				{
					threadsWhileStaThreadIsRunning.Add(pair.Code + ": " + pair.Description);
				}

				newThreadId = System.Environment.CurrentManagedThreadId;
			}, null, "Don't squarsh ma thread!");
#else
			DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(() =>
			{
				foreach (CodeDescriptionPair pair in monitor.Threads)
				{
					threadsWhileStaThreadIsRunning.Add(pair.Code + ": " + pair.Description);
				}

				newThreadId = System.Environment.CurrentManagedThreadId;
			}, null, "Don't squarsh ma thread!").Join();
#endif
			AssertNotEquals(0, newThreadId);
			AssertContainsExactElementsInAnyOrder("The new STA thread should have been in the list while that thread was running, and yet...", new[] { mainThreadId + ": Main Thread", newThreadId + ": Don't squarsh ma thread!" }, threadsWhileStaThreadIsRunning);
			WaitForSingleThreadInList(monitor);
			AssertContainsExactElementsInAnyOrder("The STA thread finished, so it should no longer be in the list, and yet...", new[] { mainThreadId + ": Main Thread" }, monitor.Threads.ToArray().Select(x => x.Code + ": " + x.Description));

			threadsWhileStaThreadIsRunning.Clear();
			DefaultAsyncStrategy.Get().DoAsync(() =>
			{
				foreach (CodeDescriptionPair pair in monitor.Threads)
				{
					threadsWhileStaThreadIsRunning.Add(pair.Code + ": " + pair.Description);
				}
			}, null, "You wouldn't understand. This thread is a secret.").Wait();

			AssertContainsExactElementsInAnyOrder("The new thread was not STA so should not have appeared in the list, and yet...", new[] { mainThreadId + ": Main Thread" }, threadsWhileStaThreadIsRunning);
			WaitForSingleThreadInList(monitor);
			AssertContainsExactElementsInAnyOrder("There are still no other STA threads running so the list should only have the main thread, and yet...", new[] { mainThreadId + ": Main Thread" }, monitor.Threads.ToArray().Select(x => x.Code + ": " + x.Description));
		}

		static void WaitForSingleThreadInList(ThreadMonitor monitor)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			while (monitor.Threads.Count > 1 && stopwatch.Elapsed < TimeSpan.FromSeconds(2))
			{
				Thread.Sleep(100);
			}

			stopwatch.Stop();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ThreadMonitor(Thread.CurrentThread);
		}
	}
}
