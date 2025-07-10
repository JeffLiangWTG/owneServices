using System;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Client.EDI.Telematics.ServiceTasks;
using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(TelematicsRimEnrolmentReportServiceTask.Code,
	"Telematics Rim Enrolment Reporter",
	"TEL",
	typeof(TelematicsRimEnrolmentReportServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1month",
	MaximumPeriod = "1month",
	DefaultScheduleRunEvery = "1month",
	DefaultScheduleDayOfMonth = 1
	)]

namespace Enterprise.Client.EDI.Telematics.ServiceTasks
{
	class TelematicsRimEnrolmentReportServiceTask : ServiceProviderImpl
	{
		public TelematicsRimEnrolmentReportServiceTask()
		{
			rimEnrollmentReportSender = new Lazy<RimEnrolmentReportSender>(() => new RimEnrolmentReportSender(ServiceLogger));
		}

		public override void RunTask(CancellationToken token)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				rimEnrollmentReportSender.Value.Run(token, DateTimeOffset.UtcNow);
			}
		}

		public const string Code = "RIM";
		readonly Lazy<RimEnrolmentReportSender> rimEnrollmentReportSender;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
