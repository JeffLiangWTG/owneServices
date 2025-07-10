using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.AuditDataServices.Subscription
{
	public abstract class ClientSpecificAuditSubscriberTask : AuditSubscriberTask
	{
		[HostedServiceRequirement]
		public static string IsEdiClient()
		{
			var enterpriseCode = ObjectFactory.Get<IClientHookLoader>()?.ClientHook?.UniqueId;
			return enterpriseCode == "EDI" ? string.Empty : $"This is not an EDI system";
		}

		public override void NudgeAuditSubscriberTask(ILogger serviceLogger)
		{
			if (string.IsNullOrEmpty(IsEdiClient()))
			{
				base.NudgeAuditSubscriberTask(serviceLogger);
			}
		}
	}
}
