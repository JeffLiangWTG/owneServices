using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.ArchiveManager.DeleteOrphanSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.ArchiveManager.DeleteOrphanSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.ArchiveManager.DeleteOrphanSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "60minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.ArchiveManager
{
	public class DeleteOrphanSubscriberServiceTask : AuditSubscriberTask
	{
		public const string Code = "DOT";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "DOP subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.DopSubscriberCode;
		public override string AssemblyName => "Enterprise.AuditDataServices.ArchiveManager";
	}
}
