using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.HVLV.HVLVSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.HVLV.HVLVSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.HVLV.HVLVSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.HVLV
{
	public class HVLVSubscriberServiceTask : AuditSubscriberTask
	{
		public const string Code = "HVV";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "HVLV subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.HvlvSubscriberCode;
		public override string AssemblyName => base.AssemblyName + "HVLV";
	}
}
