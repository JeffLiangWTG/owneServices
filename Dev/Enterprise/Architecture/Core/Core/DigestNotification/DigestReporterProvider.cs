using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core
{
	public static class DigestReporterProvider
	{
		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "The reporter is supposed to be disposed by a caller")]
		public static DigestReporter<TKey, TMessage> GetDigestReporter<TKey, TMessage>(
			ProcessDigestDelegate<TKey, TMessage> processDigestHandler,
			Func<TimeSpan> accumulateTimeGetter,
			Func<TimeSpan> releaseTimeGetter = null,
			ITimeProvider timeProvider = null,
			Func<bool> reportFirst = null,
			Func<int> threshold = null
			)
		{
			if (releaseTimeGetter == null)
			{
				releaseTimeGetter = () => TimeSpan.Zero;
			}

			if (timeProvider == null)
			{
				timeProvider = new TimeProvider();
			}

			if (reportFirst == null)
			{
				reportFirst = () => true;
			}

			if (threshold == null)
			{
				threshold = () => 0;
			}

			var reporter = new DigestReporter<TKey, TMessage>(processDigestHandler, accumulateTimeGetter, releaseTimeGetter, timeProvider, reportFirst, threshold);
#if DEBUG
			lock (reporters)
			{
				reporters.Add(new WeakReference<IDigestReporter>(reporter));
			}
#endif
			return reporter;
		}

#if DEBUG
		public static void ResetAllDigestReportersAfterTest()
		{
			lock (reporters)
			{
				foreach (var link in reporters)
				{
					if (link.TryGetTarget(out IDigestReporter reporter))
					{
						reporter.ClearReports();
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is protected by the critical section.")]
		static readonly List<WeakReference<IDigestReporter>> reporters = new List<WeakReference<IDigestReporter>>();
#endif
	}
}
