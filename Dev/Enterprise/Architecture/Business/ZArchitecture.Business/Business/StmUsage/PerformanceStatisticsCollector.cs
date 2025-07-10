using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public interface IPerformanceStatisticsPersister
	{
		/// <summary>
		/// Persists records.
		/// </summary>
		/// <param name="elements">The data to persist</param>
		void Record(IEnumerable<IPerformanceStatisticCollectorToken> elements, bool forceWrite = false);
	}

	public partial class SpecificPerformanceStatisticsCollector : IPerformanceStatisticsCollector
	{
		public SpecificPerformanceStatisticsCollector()
		{
			recorder = ObjectFactory.Get<IPerformanceStatisticsPersister>();
			ClearLogs();
			statisticMode = CalculateStatisticMode();
			ThreadSentry = ThreadSentryProvider.GetThreadSentry(true);
		}

		EnabledState statisticMode;
		readonly IPerformanceStatisticsPersister recorder;

		readonly ConcurrentStack<PerformanceStatisticCollectorToken> incompleteTokens = new ConcurrentStack<PerformanceStatisticCollectorToken>();
		readonly ConcurrentStack<PerformanceStatisticCollectorToken> completeTokens = new ConcurrentStack<PerformanceStatisticCollectorToken>();

		public bool IsMonitoring { get; private set; }

		ZDateTime timeStarted;

		public IThreadSentry ThreadSentry { get; }

		public IDisposable StartMonitoring(string name, string subname = null)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				ErrorReporter.ReportOnce("Empty name parameter passed to StartMonitoring. This will cause performance statistics to fail when saving. Please pass a non-whitespace string instead.");
			}

			ThreadSentry.EnsureCurrentThreadIsOwner();
			IDisposable result = DisposableAction.NoAction;
			if (ShouldMonitor(name))
			{
				IsMonitoring = true;

				name = stringInterner.InternValue(name);
				subname = stringInterner.InternValue(subname);
				if (timeStarted.IsEmpty)
				{
					timeStarted = ZDateTime.UtcNow;
				}

				var token = new PerformanceStatisticCollectorToken(name, subname);
				var parentToken = CurrentToken;
				if (parentToken != null)
				{
					parentToken.AddChildToken(token);
					parentToken.IsChildTokenActive = true;
				}

				result = new DisposableAction(EndMonitoring);
				incompleteTokens.Push(token);
				return result;
			}
			return result;
		}

		public IDisposable Exclude()
		{
			return CurrentToken.Exclude();
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "The calls to complete tokens are required to check for parent")]
		void EndMonitoring()
		{
			IsMonitoring = false;

			ThreadSentry.EnsureCurrentThreadIsOwner();

			if (incompleteTokens.TryPop(out var token) && token != null)
			{
				var parentToken = CurrentToken;
				if (parentToken != null)
				{
					parentToken.IsChildTokenActive = false;
				}

				token.IsChildTokenActive = false;
				token.Stop();

				if (parentToken != null)
				{
					var sibling = parentToken.PreviousSibling(token);
					if (sibling != null && sibling.CanFold(token)) // nonverbose mode and the current element can be folded into the previous one
					{
						sibling.Incorporate(token);
						parentToken.RemoveChildToken(token);
					}
				}
				else if (token.ElapsedIncludingChildren > TimeSpan.Zero)
				// Only keep root tokens with non-zero time.
				{
					if (completeTokens.TryPeek(out var sibling) && sibling != null && sibling.CanFold(token)) // nonverbose mode and the current element can be folded into the previous one
					{
						sibling.Incorporate(token);
					}
					else
					{
						completeTokens.Push(token);
					}
				}
			}

			AttemptFlush();
		}

		public void AttemptFlush(bool force = false)
		{
			if (!isSuspended)
			{
				if (!incompleteTokens.Any() && HasLogs || force)
				{
					isSuspended = true;
					try
					{
						recorder.Record(completeTokens, force);
						ClearLogs();
					}
					finally
					{
						isSuspended = false;
						ResetStatisticMode();
					}
				}
			}
		}

		bool ShouldMonitor(string statisticName)
		{
			if ((StatisticMode != EnabledState.Disabled)
				&& (StatisticMode == EnabledState.Detailed
					|| ((incompleteTokens.Count == 0) && (StatisticMode != EnabledState.Simple || !PerformanceStatisticsCollector.CalculateIsMonitoring()))))
			{
				if (!isSuspended && !IsMonitoringStatistic(statisticName))
				{
					var currentToken = CurrentToken;
					if (currentToken != null)
					{
						//TODO: Perhaps oneday aggregate so it gets better data.
						// As opposed to just stopping recording once a magic-number-determined threshold is hit.
						// Not urgent because the total time taken is still recorded, so the most important data isn't lost
						// This may actually be possible now with 1337 hax etc. Cause we're making the tree structure implicit rather than explicit by using references
						return currentToken.Children.Count() < 500;
					}
					return true;
				}
			}
			return false;
		}

		PerformanceStatisticCollectorToken CurrentToken
		{
			get { return incompleteTokens.TryPeek(out var token) ? token : null; }
		}

		bool IsMonitoringStatistic(string statisticName)
		{
			return incompleteTokens.Any(x => x.Name == statisticName);
		}

		public EnabledState StatisticMode
		{
			get { return statisticMode; }
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Value for a switch statement, not client visible.")]
		EnabledState CalculateStatisticMode()
		{
			if (isSuspended)
			{
				return EnabledState.Disabled;
			}
			else if (EnvProxy.Instance == null || !EnvProxy.Instance.IsLoggedIn)
			{
				return EnabledState.NotLoggedIn;
			}
			else if (!Db.InstanceIsNull && IsDatabaseUpgradedExceptionHasBeenThrown)
			{
				return EnabledState.Disabled;
			}

			try
			{
				string registrySetting = ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled;
				switch (registrySetting)
				{
					case "Simple":
						return EnabledState.Simple;
					case "Detailed":
						return EnabledState.Detailed;
					default:
						return EnabledState.Disabled;
				}
			}
			catch (DatabaseUpgradeInProgressException)
			{
				return EnabledState.Disabled;
			}
		}

		public void ResetStatisticMode()
		{
			statisticMode = CalculateStatisticMode();
		}

		bool isSuspended;

		public bool HasLogs
		{
			get { return completeTokens.Any(); }
		}

		void ClearLogs()
		{
			stringInterner = new StringInterner();
			completeTokens.Clear();
			incompleteTokens.Clear();
			timeStarted = ZDateTime.Empty;
		}

		bool IsDatabaseUpgradedExceptionHasBeenThrown
		{
			get
			{
				using (Db.DisposableActionForDbConnection())
				{
					return Db.Connection.DatabaseUpgradedExceptionHasBeenThrown;
				}
			}
		}

		StringInterner stringInterner = new StringInterner();
	}
}

#if DEBUG

namespace Enterprise.ZArchitecture.Business
{
	using CargoWise.Common.Testing;

	public partial class SpecificPerformanceStatisticsCollector : IPerformanceStatisticsCollector, IPerformanceStatisticsTesting
	{
		public int PendingSaves
		{
			get
			{
				var persister = recorder as PerformanceStatisticsPersister;
				return (persister != null) ? persister.PendingSaves : 0;
			}
		}

		public int SuccessfullSaves
		{
			get
			{
				var persister = recorder as PerformanceStatisticsPersister;
				return (persister != null) ? persister.SuccessfullSaves : 0;
			}
		}

		public int FailedSaves
		{
			get
			{
				var persister = recorder as PerformanceStatisticsPersister;
				return (persister != null) ? persister.FailedSaves : 0;
			}
		}
	}
}

#endif
