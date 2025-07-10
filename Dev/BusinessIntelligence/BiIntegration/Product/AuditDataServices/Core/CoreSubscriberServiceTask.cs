using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.Core.CoreSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.Core.CoreSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.Core.CoreSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "1minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.Core
{
	public class CoreSubscriberServiceTask : AuditSubscriberTask
	{
		public const string Code = "CSS"; // Service task code
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]

		public const string Description = "Core Audit Subscriber Service Task";

		public override string ServiceTaskCode => Code;

		public override string ServiceTaskDescription => Description;

		public override string SubscriberCode => SubscriberLoader.CoreSubscriberCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Subscriber Assembly Name")]
		public override string AssemblyName => base.AssemblyName + "Core";
	}
}
