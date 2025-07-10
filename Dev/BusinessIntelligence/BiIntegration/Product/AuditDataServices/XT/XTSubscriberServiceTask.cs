using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.XT.XTSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.XT.XTSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.XT.XTSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "15minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.XT
{
	public class XTSubscriberServiceTask : ClientSpecificAuditSubscriberTask
	{
		public const string Code = "XTU"; // Service task code
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "XT Credential Updates subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.XtSubscriberCode;
		public override string AssemblyName => SubscriberLoader.ZClientEdiAssemblyName;
		public override string SubscriberNamespace => SubscriberLoader.XtSubscriberNamespace;
	}
}
