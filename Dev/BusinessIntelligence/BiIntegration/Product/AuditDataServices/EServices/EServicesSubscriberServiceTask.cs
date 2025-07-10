using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.EServices.EServicesSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.EServices.EServicesSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.EServices.EServicesSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.EServices
{
	public class EServicesSubscriberServiceTask : ClientSpecificAuditSubscriberTask
	{
		public const string Code = "EHS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "eServices subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.EServicesSubscriberCode;
		public override string AssemblyName => SubscriberLoader.ZClientEdiBusinessAssemblyName;
		public override string SubscriberNamespace => SubscriberLoader.EServicesNamespace;
	}
}
