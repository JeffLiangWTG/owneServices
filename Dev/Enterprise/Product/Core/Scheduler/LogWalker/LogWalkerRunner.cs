using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.LogWalker
{
	public class LogWalkerRunner
	{
		#region Builders 

		public static LogWalkerRunner Default() => new LogWalkerRunner(new WorkerOperationsManager());

		public static LogWalkerRunner Master() => new LogWalkerRunner(new MasterOperationsManager());

		public static LogWalkerRunner Purge() => new LogWalkerRunner(new PurgeOperationsManager());

		internal LogWalkerRunner(OperationsManager manager)
		{
			Manager = manager;
		}

		#endregion

#if DEBUG
		public void Process(ILogger notifier, CancellationToken token)
		{
			Process(new LogWalkerCategoryLogger(notifier), token);
		}
#endif

		public void Process(ICategoryLogger<LogWalkerCategories> notifier, CancellationToken token)
		{
			try
			{
				using (new UserContextMonitor(notifier))
				{
					Manager.QueueAndProcessLogs(notifier.GetLogger(), token);
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
				notifier.Log(LogType.Error, ex.Message, ex);
			}
		}

		class UserContextMonitor : Disposable
		{
			public UserContextMonitor(ICategoryLogger<LogWalkerCategories> notifier)
			{
				this.logger = notifier;
				if (this.logger.ShouldLog(LogWalkerCategories.EnvironmentLogging))
				{
					Env.Instance.UserContextChanging += Instance_UserContextChanging;
				}
			}

			readonly ICategoryLogger<LogWalkerCategories> logger;

			void Instance_UserContextChanging(object sender, IUserContextChangingEventArgs e)
			{
				logger?.Log(LogWalkerCategories.EnvironmentLogging, LogType.Debug, string.Format(Culture.Invariant, "Environment {0} from {1} {2} to {3} {4}. On current thread: {5}. \r\n\r\nStackTrace: {6}",
					e.IsRevert ? "reverted" : "changed",
					e.OldUserContext?.Branch?.Code ?? "UNKNOWN",
					e.NewUserContext?.Branch?.Code ?? "UNKNOWN",
					e.OldUserContext?.User?.FullName ?? "UNKNOWN",
					e.NewUserContext?.User?.FullName ?? "UNKNOWN",
					e.SetCurrentThreadContext ? "Y" : "N",
					new StackTrace().ToString()));
			}

			protected override void Dispose(bool isDisposing)
			{
				try
				{
					Env.Instance.UserContextChanging -= Instance_UserContextChanging;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("No idea what has happened here...", e);
				}
			}
		}

		internal virtual OperationsManager Manager { get; }
	}
}
