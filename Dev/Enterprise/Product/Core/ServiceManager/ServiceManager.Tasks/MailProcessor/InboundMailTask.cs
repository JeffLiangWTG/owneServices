using System.Diagnostics;
using System.Security.Authentication;
using System.Threading;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.ServiceManager.Tasks.MailProcessor;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"IMS",
	"Inbound Mail Service",
	"MAI",
	typeof(InboundMailTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	MaximumPeriod = "1hour",
	DefaultScheduleRunEvery = "3minutes",
	ActiveByDefault = true)
]

namespace Enterprise.ServiceManager.Tasks.MailProcessor
{
	public class InboundMailTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			if (string.IsNullOrEmpty(Env.Instance.Registry.MailServer) && !Env.Instance.Registry.UseGraphApiForIncoming)
			{
				var nextRunTimeUtc = ZDateTime.UtcNow.AddDays(7);
				ServiceLogger.Log(LogType.Error, $@"As a result of incomplete mail configurations in the Registry, the Inbound Mail Service Task has been paused and has been rescheduled to run again on {nextRunTimeUtc}.
Please verify the mail configuration settings in the Registry at Registry -> {RawDataRegistry.Instance.MailServer.GetLocation()}.
Once the mail configuration has been set, the Service Task will automatically resume running after {RegistryRefresh.FrequencyInSeconds} seconds.");

				MailProcessorHelper.RescheduleMailTask("IMS", nextRunTimeUtc);
				return;
			}

			cancellationToken = token;
			DownloadEmail();
		}

		#region Implementation

		void DownloadEmail()
		{
			using var downloader = GetMailDownloader();
			downloader.LogMessage += Downloader_LogMessage;
			downloader.EmailDownloaded += Downloader_EmailDownloaded;

			try
			{
				var saver = GetMailSaver(downloader);
				saver.Retrieve();
			}
			catch (InvalidCredentialException ex)
			{
				ServiceLogger.Log(LogType.Error, ex.Message);
			}
			finally
			{
				downloader.EmailDownloaded -= Downloader_EmailDownloaded;
				downloader.LogMessage -= Downloader_LogMessage;
			}
		}

		protected virtual MailSaver GetMailSaver(IMailDownloader downloader)
		{
			return new MailSaver(downloader, ServiceLogger);
		}

		protected virtual IMailDownloader GetMailDownloader()
		{
			if (Env.Instance.Registry.UseGraphApiForIncoming)
			{
				return new GraphMailDownloader();
			}

			return new MailDownloader();
		}

		void Downloader_LogMessage(TraceEventType eventType, string message)
		{
			ServiceLogger.Log(GetLogTypeFromTraceEventType(eventType), message);
		}

		void Downloader_EmailDownloaded(string uniqueId, ref string email, ref bool continueDownloading)
		{
			if (ShouldStop())
			{
				continueDownloading = false;
			}
		}

		static LogType GetLogTypeFromTraceEventType(TraceEventType eventType)
		{
			LogType logType;
			switch (eventType)
			{
				case TraceEventType.Critical:
				case TraceEventType.Error:
					logType = LogType.Error;
					break;

				case TraceEventType.Warning:
					logType = LogType.Warning;
					break;

				case TraceEventType.Verbose:
					logType = LogType.Debug;
					break;

				default:
					logType = LogType.Information;
					break;
			}

			return logType;
		}

		#endregion

		#region IInteruptibleServiceTask Members

		bool ShouldStop()
		{
			if (cancellationToken.IsCancellationRequested)
			{
				Thread.MemoryBarrier();
				return true;
			}

			return false;
		}

		CancellationToken cancellationToken;

		#endregion
	}
}
