using System.Threading;
using Enterprise.ServiceManager.Tasks.FTP;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("FTP",
							"Generic FTP Pusher & Puller",
							"SYS",
							typeof(FtpGenericPusherPullerServiceTask),
							AllowsMultipleInstances = false,
							CanRunInAnyBranch = true,
							MinimumPeriod = "5minutes",
							DefaultScheduleRunEvery = "5minutes")]

namespace Enterprise.ServiceManager.Tasks.FTP
{
	public class FtpGenericPusherPullerServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			new FtpServiceRunner(ServiceLogger).DoEverything(token);
		}
	}
}
