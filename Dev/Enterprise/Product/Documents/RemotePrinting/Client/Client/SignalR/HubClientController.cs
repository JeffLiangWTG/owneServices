using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	public class HubClientController : IRemoteClient
	{
		readonly PrintManager printManager;
		readonly string localPrintServer;
		readonly Func<IClientUpdate, IUpdateProcessor> getUpdateProcessor;
		readonly INudgeable nudgeable;
		readonly WebClient webClient;

		public event EventHandler<LogEventArgs> Logged;
		public event EventHandler<EventArgs> Updated;
		public event EventHandler<EventArgs> CNSWClientSettingStaled;

		Watermark watermark;
		IRemoteServer connectedHub;

		public HubClientController(string localPrintServer, PrintManager printManager, Func<IClientUpdate, IUpdateProcessor> getUpdateProcessor, INudgeable nudgeable, WebClient webClient)
		{
			this.localPrintServer = localPrintServer;
			this.printManager = printManager;
			this.getUpdateProcessor = getUpdateProcessor;
			this.nudgeable = nudgeable;
			this.webClient = webClient;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "InitialisePrintQueues")]
		public void Start(IRemoteServer hub)
		{
			connectedHub = hub;

			InitialisePrintQueues("Configuring connection to hub");
		}

		void InitialisePrintQueues(string message)
		{
			var queueNames = printManager.InitialisePrintQueuesAndReturnQueueList().Select(printerInfo => printerInfo.Name).ToArray();
			var version = UpdateProcessor.GetInstalledVersion();

			Log(message);
			connectedHub.Initialise(localPrintServer, version, queueNames);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "InvariantCulture")]
		void IRemoteClient.Print(SerialisablePrintJob job)
		{
			Log(string.Format(CultureInfo.InvariantCulture, "Print job received ({0} bytes), sending job to {1}", job.Contents.Length, job.QueueName));

			PrintResult result = null;
			try
			{
				result = printManager.PrintJobAsync(job, watermark).Result;
			}
			catch (AggregateException ex)
			{
				Log($"Error when print document: {ErrorReporter.GetExceptionMessage(ex)}");
			}

			if (result != null)
			{
				var message = new StringBuilder();
				if (!result.IsSuccess)
				{
					message.AppendLine("The document failed to print.")
						.Append(" Reason provided: ");
					if (result.FailureReason is null)
					{
						message.AppendLine("None");
					}
					else
					{
						message.AppendLine(result.FailureReason);
					}
					message.AppendLine("The server will attempt to notify the sender of the documents that have failed three times.");
				}

				if (result.IsSuccess)
				{
					message.AppendLine("Returning success to server");
				}
				else
				{
					message.AppendLine("Returning failure to server");
				}

				Log(message.ToString());
				connectedHub.SetPrintStatus(job.JobPk, result.IsSuccess ? ProcessedStatus.Processed : ProcessedStatus.Failed, result.FailureReason ?? string.Empty);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Watermark update")]
		void IRemoteClient.SetWatermark(SerialisableWatermark watermarkInfo)
		{
			Log("Watermark update received.");
			watermark = WatermarkFactory.GetWatermark(watermarkInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log")]
		void IRemoteClient.Update(string versionNumber, string link)
		{
			Log("Update received.");

			var update = new ClientUpdate { Version = versionNumber, Link = link };
			var processor = getUpdateProcessor(new ClientUpdateWrapper(update));
			try
			{
				if (!processor.IsUpdateRequired())
				{
					Log(string.Format(CultureInfo.InvariantCulture, "Was requested to update to {0}, but is already on {1}. Update will be ignored.",
						versionNumber, UpdateProcessor.GetInstalledVersion()));
					return;
				}

				processor.Updated += OnUpdated;
				processor.Process();
			}
			finally
			{
				processor.Updated -= OnUpdated;
				(processor as IDisposable)?.Dispose();
			}
		}

		void OnUpdated(object sender, EventArgs args)
		{
			Updated?.Invoke(sender, args);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log")]
		void IRemoteClient.Nudge()
		{
			Log("Nudge received.");
			nudgeable?.Nudge();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "IremoteClient")]
		void IRemoteClient.RegisterClientForReconnecting()
		{
			Log("RegisterClientForReconnecting received.");

			InitialisePrintQueues("Register client for reconnecting.");
		}

		void Log(string log)
			=> Logged?.Invoke(this, new LogEventArgs(log));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Log")]
		void IRemoteClient.RefreshCNSWClientSetting()
		{
			Log("CNSW client application setting updated on server.");
			CNSWClientSettingStaled?.Invoke(this, EventArgs.Empty);
		}

		void IRemoteClient.RetrieveLogsAndPostToServer(string recipientEmail, DateTime startDate, DateTime endDate, LogTypes logType)
		{
			RetrieveLogsAndPostToServer(recipientEmail, startDate, endDate, logType, LogWriter.GetOutputDirectory().FullName);
		}

		void RetrieveLogsAndPostToServer(string recipientEmail, DateTime startDate, DateTime endDate, LogTypes logTypes, string logFilesPath)
		{
			RetrieveLogsAndPostToServer(recipientEmail, startDate, endDate, logTypes, new LogCollector(logFilesPath));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "WebPrint Client has no access to CW1 constants")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "WebPrint Client has no access to ZDateTime")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "bodyText Append")]
		public void RetrieveLogsAndPostToServer(string recipientEmail, DateTime startDate, DateTime endDate, LogTypes logTypes, LogCollector logCollector)
		{
			var bodyText = new StringBuilder();
			bodyText.Append("Archive with log files");
			bodyText.Append(" for Print Server '").Append(localPrintServer).Append("'");
			bodyText.Append(" from ").Append(startDate.ToString("dd-MMM-yyyy"));
			bodyText.Append(" to ").Append(endDate.AddMinutes(-1).ToString("dd-MMM-yyyy"));

			var bytes = logCollector.GetCompressedLogData(logTypes, startDate, endDate);
			var fileName = "Logs-" + localPrintServer + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip";

			webClient.UpdateClientLogs(recipientEmail, fileName, bytes, bodyText.ToString());
		}
	}
}
