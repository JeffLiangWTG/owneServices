using System;
using System.Collections.Generic;
using Enterprise.eHubMessaging.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskCodes.USF,
	ServiceTaskNames.UsageSubmissionFailureResetter,
	"ESV",
	typeof(Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.UsageSubmissionFailureResetterTask),
	IsMandatory = true,
	MinimumPeriod = "1day",
	IsScheduleReadOnly = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day"
	)]

namespace Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging
{
	class UsageSubmissionFailureResetterTask : eHubServiceTask
	{
		internal override DateTime OutageStartTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		internal override bool ReportKnownExceptionsAsIssues => false;
		public override string ServiceTaskName => ServiceTaskNames.UsageSubmissionFailureResetter;
		public override string DefaultServerAddress => throw new NotImplementedException();

		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new UsageSubmissionFailureResetterJob(this, Notifier);
		}
	}
}
