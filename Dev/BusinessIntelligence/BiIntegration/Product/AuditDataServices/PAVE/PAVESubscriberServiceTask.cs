using System.Threading;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.PAVE.PAVESubscriberServiceTask.Code,
		Enterprise.AuditDataServices.PAVE.PAVESubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.PAVE.PAVESubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.PAVE
{
	public class PAVESubscriberServiceTask : AuditSubscriberTask
	{
		public const string Code = "PVE";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "PAVE subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.PaveSubscriberCode;
		public override string AssemblyName => base.AssemblyName + "PAVE";

		public override void RunTask(CancellationToken token)
		{
			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				base.RunTask(token);
			}
		}
	}
}
