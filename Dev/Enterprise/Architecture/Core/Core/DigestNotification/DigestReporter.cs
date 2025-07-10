using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core
{
	public delegate void ProcessDigestDelegate<TKey, TMessage>(TKey key, TMessage message, IEnumerable<DateTime> occurrences);
	/// <summary>
	/// Accumulates events and reports about them in specified time intervals.
	/// Each event is defined with a key.
	/// If reportFirst return true the first occurrence of an event will be reported immediately (in the same thread that invokes it). The other occurrences with the same key will be reported based on a timer (in the background thread). The timer period is set with AccumulateTimeGetter.
	/// If events with a particular key does not happen for a time period set with ReleaseTimeGetter, the next event with the same key will be reported immediately.
	/// If the number of event does not reach the threshold in a time period set with ReleaseTimeGetter all the occurrences with the same key will not be reported and the number of occurrences will set to zero
	/// AccumulateTimeGetter and ReleaseTimeGetter allow to change behavior dynamically and even switch accumulation on and off. When AccumulateTimeGetter returns zero time span, accumulation switches off and all reports start reporting immediately.
	/// The class is thread safe.
	/// </summary>
	public class DigestReporter<TKey, TMessage> : IDigestReporter, IDisposable
	{
		internal DigestReporter(
			ProcessDigestDelegate<TKey, TMessage> processDigestHandler,
			Func<TimeSpan> accumulateTimeGetter,
			Func<TimeSpan> releaseTimeGetter,
			ITimeProvider timeProvider,
			Func<bool> reportFirst,
			Func<int> threshold
			)
		{
			Argument.NotNull(processDigestHandler, nameof(processDigestHandler));
			Argument.NotNull(accumulateTimeGetter, nameof(accumulateTimeGetter));
			Argument.NotNull(releaseTimeGetter, nameof(releaseTimeGetter));
			Argument.NotNull(timeProvider, nameof(timeProvider));
			Argument.NotNull(reportFirst, nameof(reportFirst));
			Argument.NotNull(threshold, nameof(threshold));

			this.processDigestHandler = processDigestHandler;
			this.accumulateTimeGetter = accumulateTimeGetter;
			this.releaseTimeGetter = releaseTimeGetter;
			this.timeProvider = timeProvider;
			this.reportFirst = reportFirst;
			this.threshold = threshold;

			timer = timeProvider.GetTimer();
			timer.Elapsed += (object sender, ElapsedEventArgs e) => ProcessAllReportsInBackground();
			var accumulateTime = AccumulateTimeSpan;
			if (accumulateTime.TotalMilliseconds != 0)
			{
				timer.Interval = accumulateTime.TotalMilliseconds;
				timer.Start();
			}
		}

		public void Report(TKey key, TMessage message)
		{
			Argument.NotNull(key, nameof(key));

			DeferredReport[] reportsCopiesToReportImmediately = null;

			lock (deferredReportsAndTimerLocker)
			{
				UpdateTimer_Unsafe();

				if (!deferredReports.TryGetValue(key, out DeferredReport report))
				{
					report = CreateNewReport(key, message);
					RegisterNewReport_Unsafe(key, report);

					if (timer.Enabled && (reportFirst() || threshold() <= 1))
					{
						reportsCopiesToReportImmediately = new DeferredReport[] { new DeferredReport(report) };
						MarkAsReported_Unsafe(report);
					}
				}
				else if (!report.Occurrences.Any())
				{
					RegisterFirstOccurrence_Unsafe(report, message);
				}
				else
				{
					RegisterNewOccurrence_Unsafe(report);
				}

				if (!timer.Enabled)
				{
					reportsCopiesToReportImmediately = ProcessReportsAndGetCopiesToReportImmediately_Unsafe();
				}
			}

			if (reportsCopiesToReportImmediately != null)
			{
				ReportImmediately(reportsCopiesToReportImmediately);
			}
		}

		public void ClearReports()
		{
			lock (deferredReportsAndTimerLocker)
			{
				deferredReports.Clear();
			}
		}

		#region IDisposable Support

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					timer.Dispose();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Implementation

		readonly object deferredReportsAndTimerLocker = new object();

		readonly ProcessDigestDelegate<TKey, TMessage> processDigestHandler;
		readonly Func<TimeSpan> accumulateTimeGetter;
		readonly Func<TimeSpan> releaseTimeGetter;
		readonly Func<bool> reportFirst;
		readonly Func<int> threshold;
		TimeSpan AccumulateTimeSpan => accumulateTimeGetter.Invoke();
		TimeSpan ReleaseTimeSpan => releaseTimeGetter.Invoke();

		class DeferredReport
		{
			public TKey Key
			{
				get
				{
					return key;
				}
			}
			readonly TKey key;

			public TMessage Message;

			public List<DateTime> Occurrences
			{
				get
				{
					return occurrences;
				}
			}
			readonly List<DateTime> occurrences = new List<DateTime>();

			public DateTime ReleaseTime;

			readonly ITimeProvider timeProvider;

			readonly int threshold;

			DeferredReport(TKey key, TMessage message, ITimeProvider timeProvider, int threshold)
			{
				Argument.NotNull(key, nameof(key));
				Argument.NotNull(timeProvider, nameof(timeProvider));

				this.timeProvider = timeProvider;
				this.key = key;
				Message = message;
				this.threshold = threshold;
			}

			public DeferredReport(DeferredReport report)
				: this(report.key, report.Message, report.timeProvider, report.threshold)
			{
				Argument.NotNull(report, nameof(report));

				occurrences = new List<DateTime>(report.Occurrences);
				ReleaseTime = report.ReleaseTime;
			}

			public DeferredReport(TKey key, TMessage message, TimeSpan releaseTimeSpan, ITimeProvider timeProvider, int threshold)
				: this(key, message, timeProvider, threshold)
			{
				RegisterNewOccurrence(releaseTimeSpan);
			}

			public void RegisterNewOccurrence(TimeSpan releaseTimeSpan)
			{
				Occurrences.Add(timeProvider.GetCurrentUtcDateTime());
				ReleaseTime = timeProvider.GetCurrentLocalMachineDateTime() + releaseTimeSpan;
			}

			public void MarkAsReported()
			{
				Occurrences.Clear();
			}

			public bool NeedsReporting => Occurrences.Any() && Occurrences.Count >= threshold;

			public bool NeedsRelease => !NeedsReporting && ReleaseTime <= timeProvider.GetCurrentLocalMachineDateTime();
		}

		readonly Dictionary<TKey, DeferredReport> deferredReports = new Dictionary<TKey, DeferredReport>();

		readonly ITimerAdaptor timer;
		readonly ITimeProvider timeProvider;

#if DEBUG
		public int TimerInterval_ExposedForTesting => (int)timer.Interval;
#endif

		DeferredReport CreateNewReport(TKey key, TMessage message)
		{
			Argument.NotNull(key, nameof(key));

			return new DeferredReport(key, message, ReleaseTimeSpan, timeProvider, threshold());
		}

		void RegisterNewReport_Unsafe(TKey key, DeferredReport report)
		{
			Argument.NotNull(key, nameof(key));
			Argument.NotNull(report, nameof(report));

			deferredReports.Add(key, report);
		}

		void RegisterFirstOccurrence_Unsafe(DeferredReport report, TMessage message)
		{
			Argument.NotNull(report, nameof(report));

			report.Message = message;
			report.RegisterNewOccurrence(ReleaseTimeSpan);
		}

		void RegisterNewOccurrence_Unsafe(DeferredReport report)
		{
			Argument.NotNull(report, nameof(report));

			report.RegisterNewOccurrence(ReleaseTimeSpan);
		}

		void ProcessAllReportsInBackground()
		{
			Argument.NotNull(timer, nameof(timer));

			using (Db.DisposableActionForDbConnection())
			{
				DeferredReport[] reportsCopiesToReportImmediately;
				lock (deferredReportsAndTimerLocker)
				{
					UpdateTimer_Unsafe();
					reportsCopiesToReportImmediately = ProcessReportsAndGetCopiesToReportImmediately_Unsafe();
				}

				ReportImmediately(reportsCopiesToReportImmediately);
			}
		}

		DeferredReport[] ProcessReportsAndGetCopiesToReportImmediately_Unsafe()
		{
			ReleaseReports_Unsafe();
			DeferredReport[] reportsToReportImmediately = GetReportsToReportImmediately_Unsafe();
			var reportsCopies = reportsToReportImmediately.Select(r => new DeferredReport(r)).ToArray();
			MarkAsReported_Unsafe(reportsToReportImmediately);
			return reportsCopies;
		}

		void ReleaseReports_Unsafe()
		{
			foreach (var report in deferredReports.Values.Where(r => r.NeedsRelease).ToArray())
			{
				Argument.NotNull(report.Key, nameof(report.Key));
				deferredReports.Remove(report.Key);
			}
		}

		DeferredReport[] GetReportsToReportImmediately_Unsafe()
		{
			//we need to report all deferred reports if the timer has just been stopped
			return deferredReports.Values.Where(r => !timer.Enabled || r.NeedsReporting).ToArray();
		}

		void MarkAsReported_Unsafe(DeferredReport[] reports)
		{
			Argument.NotNull(reports, nameof(reports));

			foreach (var report in reports)
			{
				MarkAsReported_Unsafe(report);
			}
		}

		void MarkAsReported_Unsafe(DeferredReport report)
		{
			Argument.NotNull(report, nameof(report));

			report.MarkAsReported();
		}

		void ReportImmediately(DeferredReport[] reports)
		{
			Argument.NotNull(reports, nameof(reports));

			foreach (var report in reports)
			{
				ReportImmediately(report);
			}
		}

		void ReportImmediately(DeferredReport report)
		{
			Argument.NotNull(report, nameof(report));

			processDigestHandler(report.Key, report.Message, report.Occurrences);
		}

		void UpdateTimer_Unsafe()
		{
			var accumulateTime = AccumulateTimeSpan.TotalMilliseconds;

			if (accumulateTime != 0)
			{
				if (timer.Interval != accumulateTime || !timer.Enabled)
				{
					timer.Interval = accumulateTime;
					timer.Start();
				}
			}
			else
			{
				timer.Stop();
			}
		}

		#endregion
	}
}
