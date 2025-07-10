using System.Net;
using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("MUG", "Upgrade Downloader", "SYS", typeof(UpgradeDownloaderTask),
	MinimumPeriod = "15minutes",
	MaximumPeriod = "1day",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true)
]

[assembly: HostedServiceBusinessObjectBinding("MUG",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType  + "=" + SystemMessageList.Codes.UpgradeDownload
	},
	SystemMessageList.Descriptions.UpgradeDownload)]

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class UpgradeDownloaderTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				UpgradeDownloaderMessageProcessor upgradeDownloader = GetDownloader();
				bool keepGoing = true;
				do
				{
					token.ThrowIfCancellationRequested();
					upgradeDownloader.Process(ServiceLogger.GetTaskNotificationSubscriber(), token);

					if (token.IsCancellationRequested)
					{
						upgradeDownloader.CancelDownload();
						keepGoing = false;
					}
					else if (upgradeDownloader.UpgradeDownloader == null)
					{
						keepGoing = false;
					}
					else
					{
						Thread.Sleep(SleepMilliseconds);
					}
				} while (keepGoing);
			}
		}

		protected virtual UpgradeDownloaderMessageProcessor GetDownloader()
		{
			return new UpgradeDownloaderMessageProcessor(WebRequest.DefaultWebProxy);
		}

		protected virtual int SleepMilliseconds { get { return 10000; } }
	}
}
