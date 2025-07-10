using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Async.AsyncTaskContext.Public;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker
{
	[Serializable]
	public abstract class LogSubscriber : ILogSubscriber, IDisposable
	{
		protected LogSubscriber()
		{
		}

		public abstract string Name { get; }

		public abstract string[] EventTypes { get; }

		public abstract string[] TableNames { get; }

		public virtual string FriendlyName => Name;

		public virtual bool IsRequired => true;

		/// <summary>
		/// System Log Subscribers are persisted in the NewsPublisherEventMapping table.
		/// Set HasDynamicProperties equals true if IsRequired, EventTypes or TableNames are not constant and the subscriber is registered as a SystemLogSubscriber.
		/// </summary>
		public virtual bool HasDynamicProperties => false;

		public virtual bool IsClientSpecificSubscriber => false;

		public string FactoryName => FormattableString.Invariant($"Subscriber: {GetType().Name}");

#if DEBUG
		public virtual bool EnableFactorySaveAlerterInTesting
		{
			get { return false; }
		}
#endif

		internal LogSubscriberResult ProcessLogs(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs.Length > 0)
			{
				SubscribeWorkflowTemplateApplicationScope(queuedLogs);
				var result = ProcessBatch(queuedLogs);
				CleanupLogQueueItems(queuedLogs);
				return result;
			}
			else
			{
				return new LogSubscriberResult(queuedLogs, queuedLogs);
			}
		}

		#region Pretty Printer

		public virtual PrettyPrinter GetPrettyPrinter() => new PrettyPrinter(Name);

		public class PrettyPrinter
		{
			protected readonly string subsciberName;

			public PrettyPrinter(string subscriberName)
			{
				this.subsciberName = subscriberName;
			}

			public virtual string PrettyPrint(IQueuedLog log)
			{
				return string.Format(CultureInfo.InvariantCulture, "Log (Parent = {0} {1}, Evnt = {2}, Ref = {3})", log.SJ_ParentTableCode, log.SJ_ParentID, log.SJ_SE_NKEvent, log.SJ_Reference);
			}

			public virtual string PrettyPrintSubscriberErrorInfo(IQueuedLog log)
			{
				var creator = ObjectFactory.Get<ITriggerActionDiagnostics>();
				var triggerActionDiagnostics = creator.GetUnsavedFiredTriggersInformation(log.Factory);

				var result =
$@"Subscriber Name: {subsciberName},
Parent: {log.SJ_ParentID} Table: {log.SJ_ParentTableCode}";
				if (!string.IsNullOrEmpty(triggerActionDiagnostics))
				{
					result += $@"
{triggerActionDiagnostics}";
				}
				return result;
			}
		}

		#endregion

		#region Logs Grouping

		protected virtual ILogBatcher GetLogBatcher() => DefaultLogBatcher.Instance;
		internal LogsGroupContext SetContextForLogsGroup(object key, IEnumerable<IQueuedLog> queuedLogs) => GetLogBatcher().SetContextForLogsGroup(key, queuedLogs);

		protected internal LogBatchCollection GroupLogs(IList<AppLockedItem<IQueuedLog>> queuedLogs)
		{
			var batcher = GetLogBatcher();
			batcher.AddGroupingFetchHints(queuedLogs.Select(l => l.Item));
			var groups = queuedLogs.GroupBy(
				GetGroupKey(batcher),
				(key, logs) =>
				{
					var newFactory = GetFactoryForProcessing(enableDataRefresh: false);
					var result = new List<AppLockedItem<IQueuedLog>>();
					foreach (var pair in logs)
					{
						var newLog = pair.Item is BusinessObject bizo ? (IQueuedLog)newFactory.ImportFromAnotherFactory(bizo) : pair.Item;
						result.Add(new AppLockedItem<IQueuedLog>(newLog, pair.Lock));
					}

					result.Sort(Comparer<AppLockedItem<IQueuedLog>>.Create((c1, c2) => c1.Item.SJ_PostedTimeUtc.CompareTo(c2.Item.SJ_PostedTimeUtc)));
					return new LogBatch(key, result);
				})
				.OrderByDescending(g => g.Key.Retries)
				.ThenBy(g => g.First().Item.SJ_PostedTimeUtc);

			return new LogBatchCollection(groups);
		}

		protected internal BusinessObjectFactory GetFactoryForProcessing(bool enableDataRefresh)
		{
			return new BusinessObjectFactory { RefreshEnabled = enableDataRefresh, NameForDebugging = FactoryName };
		}

		static Func<AppLockedItem<IQueuedLog>, LogBatchKey> GetGroupKey(ILogBatcher batcher)
		{
			return applock => new LogBatchKey(applock.Item.SJ_RetryCount, batcher.GetGroupLogKey(applock.Item));
		}

		#endregion

		#region Log Processing

		protected virtual void SubscribeWorkflowTemplateApplicationScope(IQueuedLog[] queuedLogs)
		{
			var workflowTemplateRestrictionHook = ObjectFactory.Get<IMessageTemplateApplicationScopeEnforcerProvider>();
			foreach (var log in queuedLogs)
			{
				workflowTemplateRestrictionHook.Track(log.Factory, log.SJ_ParentID);
			}
		}

		/// <summary>
		/// You should override either this, or "ProcessBatch", but not both.
		///
		/// This is the old API, which assumes you processed all of your logs.
		/// </summary>
		protected virtual void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			throw new NotImplementedException("Either ProcessLogQueueItems or ProcessBatch should be overriden.");
		}

		/// <summary>
		/// You should override either this, or "ProcessLogQueueItems", but not both.
		///
		/// This is the new API, which supports yielding processing half-way through a batch.
		/// </summary>
		protected virtual LogSubscriberResult ProcessBatch(IQueuedLog[] queuedLogs)
		{
			ProcessLogQueueItems(queuedLogs);
			return new LogSubscriberResult(queuedLogs, Array.Empty<IQueuedLog>());
		}

		static void CleanupLogQueueItems(IQueuedLog[] queuedLogs)
		{
			queuedLogs.Select(log => log.Factory).Distinct().ForEach(f => f.DeactivateActiveCollectionsAndCaches());
		}

		protected internal virtual ZQuery GetFeedbackLogFilter(IQueuedLog[] queuedLogs)
		{
			ZQuery feedbackLogFilter = new ZQuery();

			if (EventTypes.Length > 0)
			{
				feedbackLogFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, EventTypes);
			}

			if (TableNames.Length > 0)
			{
				feedbackLogFilter.AddToFilter(StmALogSchema.SL_Table, TableNames);
			}

			return feedbackLogFilter;
		}

		#endregion

		#region Logs

		public ILogger DefaultLogger
		{
			get
			{
				if (logger == null)
				{
					logger = new NullLogger();
				}
				return logger;
			}
		}

		public void SetDefaultLogger(ILogger loggerToSet)
		{
			this.logger = SetDefaultLoggerCore(loggerToSet);
		}

		protected internal virtual ILogger SetDefaultLoggerCore(ILogger loggerToSet)
		{
			return new NewsTransmitterNotifier(loggerToSet, this);
		}

		ILogger logger;

		#endregion

		#region BackgroundWorker

		public ITaskRunner BackgroundWorker
		{
			get
			{
				return backgroundWorkerRunner = backgroundWorkerRunner ?? backgroundWorkerRunnerFactory();
			}
		}

		internal void SetBackgroundWorkerFactory(Func<ITaskRunner> createbackgroundWorkerFunc)
		{
			this.backgroundWorkerRunnerFactory = createbackgroundWorkerFunc;
		}

		ITaskRunner backgroundWorkerRunner;
		Func<ITaskRunner> backgroundWorkerRunnerFactory;

		#endregion

		#region INotifications Wrapper

		/// <summary>
		/// HACK: This wraps an INotifications around ILogger.
		/// 
		/// Please use Logger instead.
		/// 
		/// This hack is here because implementations of LogSubscriber call through to IProcessor.Process which requires this.
		/// IProcessor.Processor should have never used INotifications in the first place, but here we are.
		/// Consolidating the implementations of ILogger->INotifications into one place is better.
		/// </summary>
		public INotifications GetINotificationsWrapperAroundILogger()
		{
			return new NotificationsWrapper(DefaultLogger);
		}

		class NotificationsWrapper : INotifications
		{
			public NotificationsWrapper(ILogger logger)
			{
				Argument.NotNull(logger, nameof(logger));
				this.logger = logger;
			}
			readonly ILogger logger;

			public void Add(INotification notification) => logger.Log(notification.Type.ToLogType(), notification.Message);
		}

		#endregion

		#region Exception Handling

		public enum ExceptionHandlingResult
		{
			Unhandled,
			SaveAsSuccessfulInNewFactory
		}

		internal ExceptionHandlingResult TryHandleException(Exception ex, IEnumerable<IQueuedLog> logs, int retryCount)
		{
			return TryHandleExceptionCore(ex, logs, retryCount);
		}

		protected virtual ExceptionHandlingResult TryHandleExceptionCore(Exception e, IEnumerable<IQueuedLog> logs, int retryCount)
		{
			if (e is ZSaveException)
			{
				try
				{
					var notifier = new NullNotificationHandler();
					ZExceptionReporting.HandleSaveException(e, notifier, true);
					if (!notifier.Message.IsEmpty)
					{
						DefaultLogger.Log(LogType.Warning, notifier.Message);
					}
				}
				catch (Exception ex) when (ex.Find<ZSaveException>() != null)
				{
					// Handle by retrying
				}
			}
			return ExceptionHandlingResult.Unhandled;
		}

		class NullNotificationHandler : INotificationHandler
		{
			ZString message;

			public ZString Message { get => message; }

			public void ReportInformation(string message, string caption)
			{
				// Continue handling exception
			}

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				this.message = message + "\n" + caption;
				// Continue handling exception
			}
		}

		internal void FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory(BusinessObjectFactory newFactory, Exception ex)
		{
			FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactoryCore(newFactory, ex);
		}

		protected virtual void FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactoryCore(BusinessObjectFactory newFactory, Exception ex) { }

		#endregion

		#region IDisposable Support

		bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		class NewsTransmitterNotifier : ILogger
		{
			internal NewsTransmitterNotifier(ILogger notifier, LogSubscriber subscriber)
			{
				this.notifier = Argument.NotNull(notifier, nameof(notifier));
				this.subscriber = subscriber;
			}

			readonly ILogger notifier;
			readonly LogSubscriber subscriber;

			public void Log(LogType type, string message) => Log(type, message, null);

			public void Log(LogType notificationType, string message, Exception ex)
			{
#if DEBUG
				if (!Globals.IsTest)
				{
					message += FormattableString.Invariant($" (PID = {Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture)})");
				}
#endif

				notifier.Log(notificationType, FormattableString.Invariant($"[{subscriber.FriendlyName}] {message}"), ex);
			}
		}

		#region Current Subscriber Tracking

		internal IDisposable SetCurrentSubscriber() => CurrentLogSubscriberTracker.SetCurrentCode(this);

		#endregion
	}

	public class NullLogger : ILogger
	{
		public void Log(LogType type, string message) { }
		public void Log(LogType type, string message, Exception ex) { }
	}
}
