using System.Threading;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.BorderWise.BorderWiseSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.BorderWise.BorderWiseSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.BorderWise.BorderWiseSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "60minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.BorderWise
{
	public class BorderWiseSubscriberServiceTask : ClientSpecificAuditSubscriberTask
	{
		public const string Code = "BDW";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "BorderWise subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.BorderWiseSubscriberCode;
		public override string AssemblyName => base.AssemblyName + "BorderWise";

		[HostedServiceRequirement]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Check message")]
		public static string IsBorderwiseRegistryItemEnabled()
		{
			var result = string.Empty;
			if (!BorderWiseSyncConfig.Enabled)
			{
				result = "Borderwise is not enabled on this system";
			}
			return result;
		}

		public override void RunTask(CancellationToken token)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					base.RunTask(token);
				}
			}
		}
	}
}
