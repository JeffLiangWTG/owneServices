using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.MDM.MDMSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.MDM.MDMSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.MDM.MDMSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.MDM
{
	public class MDMSubscriberServiceTask : AuditSubscriberTask
	{
		public const string Code = "PTM";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "Pattern Matching subscriber service task";
		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.MdmSubscriberCode;
		public override string AssemblyName => base.AssemblyName + "MDM";
	}
}
