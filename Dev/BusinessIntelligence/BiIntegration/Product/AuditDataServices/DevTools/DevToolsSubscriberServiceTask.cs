using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.DevTools.DevToolsSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.DevTools.DevToolsSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.DevTools.DevToolsSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = false,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.DevTools
{
	public class DevToolsSubscriberServiceTask : ClientSpecificAuditSubscriberTask
	{
		public const string Code = "DTX";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "DevTools subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.DevToolsSubscriberCode;
		public override string AssemblyName => SubscriberLoader.ZClientEdiBusinessAssemblyName;
		public override string SubscriberNamespace => SubscriberLoader.DevToolsNamespace;
	}
}
