using System;
using System.Linq;
using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Host.Queue;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	sealed class QueueMonitorTask : IServiceManagerTask, IQueueMonitorInitializer, IDisposable
	{
		public QueueMonitorTask(IQueueMonitorLogger queueMonitorLogger, IQueueStatusProviderFactory queueStatusProviderFactory, IHostLogger hostLogger, ISqlMutexLockProvider lockProvider, ISharedRegistrySettings sharedRegistry)
		{
			configuredEvent = new ManualResetEvent(false);

			this.queueMonitorLogger = queueMonitorLogger ?? throw new ArgumentNullException(nameof(queueMonitorLogger));
			this.queueStatusProviderFactory = queueStatusProviderFactory ?? throw new ArgumentNullException(nameof(queueStatusProviderFactory));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.lockProvider = lockProvider ?? throw new ArgumentNullException(nameof(lockProvider));
			this.sharedRegistry = sharedRegistry ?? throw new ArgumentNullException(nameof(sharedRegistry));
		}

		public string Name => nameof(QueueMonitorTask);
		public TimeSpan RunDelay => sharedRegistry.ProcessControllerQueueMonitoringFrequency < minDelay ? minDelay : sharedRegistry.ProcessControllerQueueMonitoringFrequency;
		public TimeSpan ErrorDelay => minDelay;

		public void Initialise(CancellationToken cancellationToken)
		{
			WaitHandle.WaitAny(new[] { configuredEvent, cancellationToken.WaitHandle });
		}

		public void ConfigureQueueMonitor(ITaskStatusProvider taskStatusProvider)
		{
			taskStatusProvider = taskStatusProvider ?? throw new ArgumentNullException(nameof(taskStatusProvider));

			statusProvider = queueStatusProviderFactory.Create(taskStatusProvider);

			configuredEvent.Set();
		}

		public void Run(CancellationToken cancellationToken)
		{
			if (!configuredEvent.WaitOne(TimeSpan.Zero))
			{
				throw new QueueuMonitorInitializationException();
			}

			if (!sharedRegistry.ProcessControllerQueueMonitoringEnabled
				|| cancellationToken.IsCancellationRequested
				|| !TryApplyLock())
			{
				return;
			}

			var queueStatuses = statusProvider.GetQueueStatus().QueueList;
			if (!queueStatuses.Any())
			{
				hostLogger.Log(LogLevel.Warning, "Queue status is empty");
				return;
			}

			foreach (var queueStatus in queueStatuses)
			{
				queueMonitorLogger.Log(queueStatus);
			}

			bool TryApplyLock()
			{
				return lockProvider.GetUnobservedLock(
					LockInfo,
					Category,
					sharedRegistry.ProcessControllerQueueMonitoringFrequency,
					((GlbStaff)Env.CurrentUser)?.GS_Code ?? User.ServiceUserCode);
			}
		}

		public void Dispose()
		{
			configuredEvent.Dispose();
		}

		const string Category = "PRC";
		const string LockInfo = "PRC-QMT";

		IQueueStatusProvider statusProvider;

		readonly TimeSpan minDelay = TimeSpan.FromMinutes(1);
		readonly IQueueMonitorLogger queueMonitorLogger;
		readonly IHostLogger hostLogger;
		readonly ISqlMutexLockProvider lockProvider;
		readonly IQueueStatusProviderFactory queueStatusProviderFactory;
		readonly ManualResetEvent configuredEvent;
		readonly ISharedRegistrySettings sharedRegistry;
	}
}
