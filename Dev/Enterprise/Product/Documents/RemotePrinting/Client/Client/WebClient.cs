using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	public class WebClient
	{
		public WebClient(IRemotePrintingServiceAdaptor remotePrintingService)
		{
			//For debug only - uncomment to disable server certificate check (for self-signed certificates).
			//ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

			printingService = remotePrintingService;
		}

		readonly IRemotePrintingServiceAdaptor printingService;
		WebClientConfiguration webClientConfiguration;

		public IErrorResponseWebRequestProcessor ResponseProcessor => printingService.ResponseProcessor;

		public void SetWebServiceUrlAndCredentials(WebClientConfiguration config, Action<string> onShowInformation = null)
		{
			webClientConfiguration = config;
			if (config.KeepAliveEnabled)
			{
				if (config.KeepAliveTime > 0 && config.KeepAliveInterval > 0)
				{
					ServicePointManager.SetTcpKeepAlive(true, config.KeepAliveTime * 1_000, config.KeepAliveInterval * 1_000);
				}
			}

			ServicePointManager.Expect100Continue = config.Expect100Continue;
			printingService.SetWebServiceUrlAndCredentials(config, onShowInformation);
		}

		public virtual SerialisablePrintJob[] GetJobsCompressed(string printServer)
		{
			var serverPrintJobs = printingService.GetJobsCompressed(printServer);
			var printJobs = UnCompressPrintJobs(serverPrintJobs);
			return printJobs;
		}

		public virtual SerialisablePrintJob[] GetJobsCompressed2(string printServer)
		{
			var serverPrintJobs = printingService.GetJobsCompressed2(printServer);
			var printJobs = UnCompressPrintJobs(serverPrintJobs);
			return printJobs;
		}

		SerialisablePrintJob[] UnCompressPrintJobs(IReadOnlyList<ServerPrintJobEx> serverPrintJobs)
		{
			var printJobs = new SerialisablePrintJob[serverPrintJobs.Count];

			for (int i = 0; i < serverPrintJobs.Count; i++)
			{
				var serverPrintJob = serverPrintJobs[i];
				printJobs[i] = new SerialisablePrintJob
				{
					BlobType = serverPrintJob.BlobType,
					Contents = GetUncompressedByteArray(serverPrintJob.Contents),
					Copies = serverPrintJob.Copies,
					EmailSubjectLine = serverPrintJob.EmailSubjectLine,
					EscapeSequence = GetUncompressedByteArray(serverPrintJob.EscapeSequence),
					HasWatermark = serverPrintJob.HasWatermark,
					QueueName = serverPrintJob.QueueName,
					JobPk = serverPrintJob.JobPk,
					QueueStateChangedStamp = serverPrintJob.QueueStateChangedStamp
				};
			}

			return printJobs;
		}

		byte[] GetUncompressedByteArray(byte[] rawValue)
		{
			return Compressor.Uncompress(rawValue);
		}

		public virtual SerialisablePrintQueue[] GetChangedQueues(string printServer, string[] changedPrintQueueNames)
		{
			var serverPrintQueues = printingService.GetChangedQueues(printServer, changedPrintQueueNames);
			var printQueues = new SerialisablePrintQueue[serverPrintQueues.Length];

			for (int i = 0; i < serverPrintQueues.Length; i++)
			{
				var serverPrintQueue = serverPrintQueues[i];
				printQueues[i] = new SerialisablePrintQueue
				{
					ColumnScale = serverPrintQueue.ColumnScale,
					DisplayName = serverPrintQueue.DisplayName,
					LeftMargin = serverPrintQueue.LeftMargin,
					Name = serverPrintQueue.Name,
					PrintLanguage = serverPrintQueue.PrintLanguage,
					RowScale = serverPrintQueue.RowScale,
					Scale = serverPrintQueue.Scale,
					StateChangedStamp = serverPrintQueue.StateChangedStamp,
					SuppressLetterhead = serverPrintQueue.SuppressLetterhead,
					TopMargin = serverPrintQueue.TopMargin,
					XlsTemplate = serverPrintQueue.XlsTemplate,
					IsRollPaper = serverPrintQueue.IsRollPaper
				};
			}

			return printQueues;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log information")]
		public virtual void SetJobSuccess(Guid[] printJobPks, Action<string> log = null)
		{
			try
			{
				printingService.SetJobSuccess(printJobPks);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var jobDetails = printJobPks.Select(pk => new JobDetails(pk, "Success", ProcessedStatus.Processed));
				JobStatusFileWriter.Append(jobDetails, log);
				throw;
			}
		}

		public virtual void SetJobFailure(PrintJobFailed[] printJobs, Action<string> log = null)
		{
			try
			{
				printingService.SetJobFailure(printJobs);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var jobDetails = printJobs.Select(printJob => new JobDetails(printJob.JobPk, printJob.FailureReason, ProcessedStatus.Failed));
				JobStatusFileWriter.Append(jobDetails, log);
				throw;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Do not block other print jobs")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Invoke message")]
		public void SyncJobStatusSafe(Action<string> log, Action<Exception, string, bool, bool> handleServerException)
		{
			var unsyncedJobs = JobStatusFileWriter.ReadAll(log)?.ToList();
			if (unsyncedJobs == null)
			{
				return;
			}

			try
			{
				log?.Invoke("Synchronizing previously processed jobs statuses...");

				var processedJobs = unsyncedJobs.Where(j => j.Status == ProcessedStatus.Processed).Select(j => j.PK).ToArray();
				if (processedJobs.Length > 0)
				{
					var processedJobsPk = string.Join(",", processedJobs);
					log?.Invoke($"Start updating successful print jobs status. (Jobs: {processedJobsPk})");
					UpdateJobStatusWithRetry(log, processedJobs, printingService.SetJobSuccess, aJob => aJob);
					log?.Invoke("End updating successful print jobs status.");
				}

				var failedJobs = unsyncedJobs.Where(j => j.Status != ProcessedStatus.Processed)
					.Select(job => new PrintJobFailed { JobPk = job.PK, FailureReason = job.FailureReason }).ToArray();
				if (failedJobs.Length > 0)
				{
					var failedJobsPk = string.Join(",", failedJobs.Select(job => job.JobPk));
					log?.Invoke($"Start updating failed print jobs status. (Failed jobs: {failedJobsPk})");
					UpdateJobStatusWithRetry(log, failedJobs, printingService.SetJobFailure, aJob => aJob.JobPk);
					log?.Invoke("End updating failed print jobs status.");
				}

				JobStatusFileWriter.Delete();

				log?.Invoke("Finished synchronized previously processed jobs statuses.");
			}
			catch (Exception ex)
			{
				handleServerException(ex, "Sync Job Status Failed", true, false);
			}
		}

		/// <remarks>
		/// Catches and does not rethrow SoapException - either request massage is too long or some other format issue.
		/// Either way need to prevent endless unsuccessful attempts to update same print jobs statuses.
		/// </remarks>
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "onShowInformation")]
		void UpdateJobStatusWithRetry<T>(Action<string> onShowInformation, T[] jobs, Action<T[]> updateJobStatus, Func<T, Guid> getJobPk)
		{
			try
			{
				updateJobStatus(jobs);
			}
			catch (SoapException ex)
			{
				if (jobs.Length > 1)
				{
					onShowInformation?.Invoke("Error updating multiple print jobs statuses. Retrying with individual print jobs.");

					foreach (var aJob in jobs)
					{
						try
						{
							updateJobStatus(new[] { aJob });
						}
						catch (SoapException ex1)
						{
							onShowInformation?.Invoke("Failed to update status of print job " + getJobPk(aJob) + System.Environment.NewLine + ex1.Message);
						}
					}
				}
				else if (jobs.Length > 0)
				{
					onShowInformation?.Invoke("Failed to update status of print job " + getJobPk(jobs.First()) + System.Environment.NewLine + ex.Message);
				}
			}
		}

		public virtual void SetQueues(string printServer, string[] printQueueNames)
		{
			printingService.SetQueues(printServer, printQueueNames);
		}

		public virtual void SetQueuesEx(string printServer, PrinterInfo[] printQueues)
		{
			SetQueuesEx(
				printServer,
				printQueues.Select(queue => new PrintQueueInfo { Name = queue.Name, IsSuspectedSurrogate = queue.IsSuspectedSurrogate }).ToArray());
		}

		public virtual void SetQueuesEx(string printServer, PrintQueueInfo[] printQueues)
		{
			try
			{
				printingService.SetQueuesEx(printServer, printQueues);
			}
			catch (Exception ex) when (IsIncompatibleApiException(ex, nameof(RemotePrintingService.SetQueuesEx)))
			{
				SetQueues(printServer, printQueues.Select(queue => queue.Name).ToArray());
			}
		}

		public virtual void SendNotificationEmail(string subject, string body)
		{
			printingService.SendNotificationEmail(subject, body);
		}

		public virtual void UpdateClientLogs(string recipientEmail, string fileName, byte[] fileData, string comments)
		{
			printingService.UpdateClientLogs(recipientEmail, fileName, fileData, comments);
		}

		public virtual SerialisableWatermark GetWatermarkInfo()
		{
			var serverWatermark = printingService.GetWatermarkInfo();
			var watermark = new SerialisableWatermark
			{
				FontSize = serverWatermark.FontSize,
				HorizontalAlignment = serverWatermark.HorizontalAlignment,
				HorizontalOffset = serverWatermark.HorizontalOffset,
				ImageWatermark = serverWatermark.ImageWatermark,
				Opacity = serverWatermark.Opacity,
				Rotation = serverWatermark.Rotation,
				TextWatermark = serverWatermark.TextWatermark,
				UseTextWatermark = serverWatermark.UseTextWatermark,
				VerticalAlignment = serverWatermark.VerticalAlignment,
				VerticalOffset = serverWatermark.VerticalOffset
			};

			return watermark;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SendNotificationEmail")]
		public IClientUpdate CheckClientUpdate(Action<string> onShowInformation)
		{
			ClientUpdate clientUpdate;
			try
			{
				clientUpdate = printingService.CheckClientUpdate2();
			}
			catch (Exception ex) when (IsIncompatibleApiException(ex, nameof(RemotePrintingService.CheckClientUpdate2)))
			{
				clientUpdate = printingService.CheckClientUpdate();

				UpdateProcessor.SendNotificationWithClientHasNewerVersionThanServer(webClientConfiguration.UpdateConfiguration, new ClientUpdateWrapper(clientUpdate), webClientConfiguration.LocalMachineName, (message) => SendNotificationEmail("Remote Printing Client Update", message), onShowInformation);
			}
			return new ClientUpdateWrapper(clientUpdate);
		}

		public virtual ICNSWClientApplicationSetting GetCNSWClientApplicationSetting(string machineName)
		{
			return new CNSWClientApplicationSettingWrapper(printingService.GetCNSWClientApplicationSetting(machineName));
		}

		public virtual ITWNCATKClientApplicationSetting GetTWNCATKClientApplicationSetting(string machineName)
		{
			return new TWNCATKClientApplicationSettingWrapper(printingService.GetTWNCATKClientSetting(machineName));
		}

		public virtual ICLSMSClientApplicationSetting GetCLSMSClientApplicationSetting(string machineName)
		{
			return new CLSMSClientApplicationSettingWrapper(printingService.GetCLSMSClientSetting(machineName));
		}

		public virtual IJPNACCSClientApplicationSetting GetJPNACCSClientApplicationSetting(string machineName)
		{
			return new JPNACCSClientApplicationSettingWrapper(printingService.GetJPNACCSClientSetting(machineName), machineName);
		}

		static bool IsIncompatibleApiException(Exception ex, string apiName)
		{
			var fullApiName = "http://www.cargowise.com/" + apiName;

			while (ex != null)
			{
				if (ex is SoapException soapEx && soapEx.Message.Contains(fullApiName))
				{
					return true;
				}
				ex = ex.InnerException;
			}

			return false;
		}

		public bool ValidOperationExecutedSuccessfullyBefore(string operationName)
		{
			return !string.IsNullOrEmpty(operationName) && printingService.ExecutedSuccessfullyOperations.Contains(operationName);
		}

		public bool IsSupportUser => printingService.IsSupportUser;
	}
}
