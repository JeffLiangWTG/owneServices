using Enterprise.AuditDataServices.Subscription;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.Glow.GlowSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.Glow.GlowSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.Glow.GlowSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "60minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.Glow
{
	public class GlowSubscriberServiceTask : AuditSubscriberTask
	{
		public const string Code = "GLW";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "GLOW subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.GlowSubscriberCode;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Assembly name")]
		public override string AssemblyName => base.AssemblyName + "Glow";
	}
}
