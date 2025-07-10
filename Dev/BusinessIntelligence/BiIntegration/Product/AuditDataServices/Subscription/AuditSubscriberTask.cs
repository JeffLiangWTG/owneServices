using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.AuditDataServices.Subscription
{
	public abstract class AuditSubscriberTask : ServiceProviderImpl
	{
		[HostedServiceRequirement]
		public static string CheckCdcIsEnabled() => CdcDatabase.CheckIsEnabled();

		[HostedServiceRequirement]
		public static string IsAuditEnabled() => BiServiceTaskHelpers.CheckAuditEnabledForHostedServiceRequirements();

		[HostedServiceRequirement]
		public static string CheckCdcShouldBeDisabled() => BiServiceTaskHelpers.IsCdcDisableFlagTrue();

		public abstract string ServiceTaskCode { get; }
		public abstract string ServiceTaskDescription { get; }
		public virtual string SubscriberCode => "";

		public virtual string AssemblyName => "Enterprise.AuditDataServices.";
		public virtual string SubscriberNamespace => AssemblyName + ".Subscribers";

		public virtual void NudgeAuditSubscriberTask(ILogger serviceLogger)
		{
			serviceLogger.Log(LogType.Debug, $"Nudging {ServiceTaskDescription}");
			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(ServiceTaskCode);
		}

		protected virtual IEnumerable<IAuditSubscriber> GetSubscribers()
		{
			return new SubscriberLoader().EnumerateSubscribersOfType(AssemblyName, SubscriberNamespace);
		}

		public override void RunTask(CancellationToken token)
		{
			var subscribers = GetSubscribers();
			var subManager = new SubscriberManager(ServiceLogger, ServiceTaskCode);
			subManager.Run(token, subscribers);
		}
	}
}
