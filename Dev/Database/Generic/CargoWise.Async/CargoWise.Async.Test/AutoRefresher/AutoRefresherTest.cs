using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Async.Test
{
	class AutoRefresherTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestAutoRefresher_WhatHappensIfItCrashesInUpdate()
		{
			Task MainAutoRefresh(CancellationToken token)
			{
				throw new InvalidOperationException("This is an useless string");
			}

			var reporter = new CustomErrorReporter(Thread.CurrentThread.ManagedThreadId);
			ErrorReporter.Instance = reporter;

			using (var refresher = new AutoRefresher(MainAutoRefresh, TimeSpan.FromSeconds(1)))
			{
				refresher.Start();
				var watch = new Stopwatch();
				watch.Start();

				while (watch.Elapsed < TimeSpan.FromSeconds(10))
				{
					Application.DoEvents();
					Thread.Sleep(1000);
				}
			}

			Application.DoEvents();

			AssertEquals(false, reporter.PostedOnBackgroundThread);
			AssertEquals(true, reporter.PostedOnForegroundThread);
		}

		class CustomErrorReporter : IErrorReporter
		{
			public CustomErrorReporter(int threadID)
			{
				this.id = threadID;
			}

			readonly int id;

			public bool PostedOnBackgroundThread { get; private set; }
			public bool PostedOnForegroundThread { get; private set; }

			public void Clear() { }

			public void Report(string key, string message, Exception exception)
			{
				if (id != Thread.CurrentThread.ManagedThreadId)
				{
					PostedOnBackgroundThread = true;
				}
				else
				{
					PostedOnForegroundThread = true;
				}
			}

			public void ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception ex)
			{
				throw new NotImplementedException();
			}
		}

		public void TestAutoRefresher_TimeToRefresh()
		{
			using (var refresher = new AutoRefresher(PerformAutoRefreshAction, TimeSpan.FromSeconds(120)))
			using (var are = new AutoResetEvent(false))
			{
				var testPassed = false;
				refresher.Start();

				while (!are.WaitOne(1000))
				{
					if (refresher.TimeUntilRefresh == TimeSpan.FromSeconds(119) || refresher.TimeUntilRefresh == TimeSpan.FromSeconds(118))
					{
						are.Set();
						testPassed = true;
					}
					else if (refresher.TimeUntilRefresh == TimeSpan.FromSeconds(117))
					{
						Assert("The timer should not reach 117 in under 2 seconds, and yet...", false);
					}
				}

				Assert(string.Format("The timer should reach 1:59 in 1 second, and yet it was {0} after 1000 milliseconds.", refresher.TimeUntilRefresh), testPassed);
			}
		}

		public void TestAutoRefresher_BoardRefreshRestartsCountdownTimer()
		{
			using (var refresher = new AutoRefresher(PerformAutoRefreshAction, TimeSpan.FromSeconds(120)))
			{
				refresher.Start();
				Thread.Sleep(1000);

				refresher.TimeUntilRefresh = TimeSpan.FromSeconds(59);
				AssertLessThan(refresher.TimeUntilRefresh, TimeSpan.FromSeconds(60));

				refresher.Refresh();
				refresher.WaitForNextPulse_ForTest();

				AssertGreaterThan("The timer should have reset the TimeUntilRefresh value when Refresh() was called. SAD!", refresher.TimeUntilRefresh, TimeSpan.FromSeconds(100));
			}
		}

		[SnailTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public void TestAutoRefresher_PauseContinueTest()
		{
			IAsyncStrategy strategy = DefaultAsyncStrategy.Get();
			var task = strategy.DoAsync(() =>
			{
				using (var refresher = new AutoRefresher(PerformAutoRefreshAction, TimeSpan.FromSeconds(5)))
				{
					var recorder = new AutoRefresherEventRecorder(refresher);

					void AssertSecondsUntilRefreshEquals(int expectedSecondsUntilRefresh)
					{
						var message = "AutoRefresher events:" + Environment.NewLine + recorder.ToString();
						AssertEquals(message, TimeSpan.FromSeconds(expectedSecondsUntilRefresh), refresher.TimeUntilRefresh);
					}

					refresher.Start();
					Task.Delay(1050).Wait();
					recorder.RecordEvent("Should be at 1050 ms");
					AssertSecondsUntilRefreshEquals(4);

					refresher.IsPaused = true;
					Task.Delay(3000).Wait();
					recorder.RecordEvent("Should be at 4050 ms");
					AssertSecondsUntilRefreshEquals(4);

					refresher.IsPaused = false;
					Task.Delay(1050).Wait();
					recorder.RecordEvent("Should be at 5100 ms");
					AssertSecondsUntilRefreshEquals(3);
				}
			});

			AsyncHelper.WaitAllActiveTasksForTest();
		}

		sealed class AutoRefresherEventRecorder
		{
			struct AutoRefresherEvent
			{
				public long MillisecondsSinceTestStart { get; }
				public string Description { get; }

				public AutoRefresherEvent(long millisecondsSinceTestStart, string description)
				{
					MillisecondsSinceTestStart = millisecondsSinceTestStart;
					Description = description;
				}
			}

			readonly List<AutoRefresherEvent> events = new List<AutoRefresherEvent>();
			readonly Stopwatch stopwatch = Stopwatch.StartNew();
			readonly AutoRefresher Refresher;

			public AutoRefresherEventRecorder(AutoRefresher refresher)
			{
				Refresher = refresher;

				Refresher.PropertyChanged += (s, e) =>
				{
					if (e.PropertyName == nameof(AutoRefresher.TimeUntilRefresh))
					{
						RecordEvent("Pulse");
					}
				};
			}

			public void RecordEvent(string description)
			{
				lock (events)
				{
					events.Add(new AutoRefresherEvent(stopwatch.ElapsedMilliseconds, $"{description}, TimeUntilRefresh = {Refresher.TimeUntilRefresh.TotalSeconds} s"));
				}
			}

			public override string ToString()
			{
				lock (events)
				{
					return string.Join(Environment.NewLine, events.Select(e => $"{e.MillisecondsSinceTestStart} ms since test start: {e.Description}"));
				}
			}
		}

		public void TestAutoRefresher_ShouldWaitForShouldRefresh()
		{
			refreshCount = 0;
			shouldRefresh = true;

			using (AutoRefresher refresher = new AutoRefresher(PerformAutoRefreshAction, TimeSpan.FromSeconds(1), ShouldRefresh))
			{
				refresher.Start();
				AssertEquals(true, autoRefreshEvent.WaitOne(TimeSpan.FromSeconds(2)));
				AssertEquals(1, refreshCount);

				shouldRefresh = false;
				AssertEquals(false, autoRefreshEvent.WaitOne(TimeSpan.FromSeconds(2)));
				AssertEquals(1, refreshCount);

				shouldRefresh = true;
				AssertEquals(true, autoRefreshEvent.WaitOne(TimeSpan.FromSeconds(2)));
				AssertEquals(2, refreshCount);
			}
		}

		Task PerformAutoRefreshAction(CancellationToken token)
		{
			refreshCount++;
			autoRefreshEvent.Set();
			return Task.FromResult(false);
		}

		int refreshCount;

		bool ShouldRefresh()
		{
			return shouldRefresh;
		}

		bool shouldRefresh;

		readonly AutoResetEvent autoRefreshEvent = new AutoResetEvent(false);
	}
}
