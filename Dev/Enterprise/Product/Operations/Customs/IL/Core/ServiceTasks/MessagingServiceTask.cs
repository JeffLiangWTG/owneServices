using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.IL.ServiceTasks
{
	public abstract class MessagingServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string MessageServiceTaskCategory = "ILC";

		[HostedServiceRequirement]
		public static string IsRequired() => string.Empty; //TODO: To be changed if certificate is implemented.
	}
}
