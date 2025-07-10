using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.Telematics.TelematicsSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.Telematics.TelematicsSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.Telematics.TelematicsSubscriberServiceTask),
		IsMandatory = false,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.Telematics
{
	public class TelematicsSubscriberServiceTask : AuditSubscriberTask
	{
		public const string Code = "TSS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "Telematics subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.TelematicsSubscriberCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Assembly name")]
		public override string AssemblyName => base.AssemblyName + "Telematics";
	}
}
