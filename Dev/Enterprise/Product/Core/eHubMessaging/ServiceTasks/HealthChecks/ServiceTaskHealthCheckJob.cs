using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.Business.Interfaces;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract public class ServiceTaskHealthCheckJob : IEHubServiceTaskHealthCheckJob
	{
		readonly string serviceTaskName;
		readonly string serviceTaskCode;

		protected ServiceTaskHealthCheckJob(string serviceTaskName, string serviceTaskCode, INotifications notifier)
		{
			this.serviceTaskName = Argument.NotNullOrEmpty(serviceTaskName, "serviceTaskName");
			this.serviceTaskCode = Argument.NotNullOrEmpty(serviceTaskCode, "serviceTaskCode");
			Notifier = Argument.NotNull(notifier, "notifier");
		}

		#region Execution

		public bool Execute()
		{
			if (!ReadyToPerformCheck())
			{
				return false;
			}

			var result = Check();
			if (result.FoundError())
			{
				Notify(result);
			}
			return true;
		}

		public abstract bool ReadyToPerformCheck();
		public abstract IHealthCheckResult Check();
		public abstract void Notify(IHealthCheckResult companies);

		public abstract ZDateTime ServiceTaskNextRunTimeUtc { get; }
		public abstract int ServiceTaskPeriodInMinute { get; }

		public const int ServiceTaskPeriodInMinuteMin = 1;
		public const int ServiceTaskPeriodInMinuteMax = 60;

		protected INotifications Notifier { get; private set; }

		public string ServiceTaskName
		{
			get
			{
				return serviceTaskName;
			}
		}

		protected string ServiceTaskCode
		{
			get
			{
				return serviceTaskCode;
			}
		}

		protected abstract string HealthCheckName { get; }

		internal virtual void NotifyVerbose(string verboseMessage)
		{
			Notifier.Notify(new VerboseInfoNotification(getHealthCheckNotification(verboseMessage)));
		}

		protected virtual void AddInfo(string message)
		{
			Notifier.Notify(new InfoNotification(getHealthCheckNotification(message)));
		}

		protected virtual void AddWarning(string message)
		{
			Notifier.AddWarning(getHealthCheckNotification(message));
		}

		String getHealthCheckNotification(string message)
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", HealthCheckName, message);
		}

		#endregion
	}
}
