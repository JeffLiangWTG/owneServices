using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public abstract class eServicesHealthCheckServiceTask : ServiceProviderImpl
	{
		public abstract IEnumerable<IEHubServiceTaskHealthCheckJob> GetJobs();

		public override void RunTask(CancellationToken token)
		{
			var earliestNextRuntimeFinalLoop = ZDateTime.Empty;
			var nextExecuteIterationIsRequired = true;
			var exceptions = new List<Exception>();

			while (nextExecuteIterationIsRequired)
			{
				var earliestNextRuntimeThisLoop = ZDateTime.Empty;
				nextExecuteIterationIsRequired = false;

				foreach (var job in GetJobs())
				{
					token.ThrowIfCancellationRequested();
					try
					{
						if (job.Execute())
						{
							nextExecuteIterationIsRequired = true;
						}

						if (earliestNextRuntimeThisLoop.IsEmpty || job.ServiceTaskNextRunTimeUtc < earliestNextRuntimeThisLoop)
						{
							earliestNextRuntimeThisLoop = job.ServiceTaskNextRunTimeUtc;
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						exceptions.Add(ex);
					}
				}

				if (exceptions.Count > 0)
				{
					throw new AggregateException(exceptions);
				}

				earliestNextRuntimeFinalLoop = earliestNextRuntimeThisLoop;
			}

			if (earliestNextRuntimeFinalLoop != ZDateTime.Empty)
			{
				UpdateTaskSchedule(earliestNextRuntimeFinalLoop);
			}
		}

		void UpdateTaskSchedule(ZDateTime serviceTaskNextRuntimeUtc)
		{
			var taskExists = ObjectFactory.Get<IServiceManagerQuerier>()
				.TryGetServiceTaskNextRunTime(ServiceTaskCode, out var originalNextRunTimeUtc);

			if (!taskExists ||
				(originalNextRunTimeUtc ?? DateTimeOffset.MinValue) == (serviceTaskNextRuntimeUtc.UtcToDateTimeOffset().ToDateTimeOffsetSafe() ?? DateTimeOffset.MinValue))
			{
				return;
			}

			ObjectFactory
				.Get<IServiceManagerGovernor>()
				.SetServiceTaskNextRuntime(ServiceTaskCode, serviceTaskNextRuntimeUtc.UtcToDateTimeOffset().ToDateTimeOffsetSafe());

			ServiceLogger.Log(Integration.LogType.Information,
				string.Format(CultureInfo.InvariantCulture,
					"Changed Service Task next runtime from {0} to {1} to match minimum of health check jobs.",
					new ZDateTime(originalNextRunTimeUtc?.DateTime), serviceTaskNextRuntimeUtc));
		}

		public virtual INotifications Notifier
		{
			get => notifier ?? (notifier = ServiceLogger.GetTaskNotificationSubscriber());
			set => notifier = value;
		}

		public abstract string ServiceTaskCode { get; }

		INotifications notifier;
	}
}
