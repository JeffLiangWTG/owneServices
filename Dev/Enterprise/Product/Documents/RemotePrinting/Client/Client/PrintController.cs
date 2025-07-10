using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using CargoWise.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	public class PrintController : Controller, INudgeable
	{
		public bool EnableSignalR => ConfigSetting.EnableSignalR;

		readonly TimeSpan intervalToClearOldFilesInError = TimeSpan.FromDays(1);
		protected DateTime lastPrintQueueRefresh = DateTime.MinValue;
		protected DateTime lastClearOldFilesInError = DateTime.MinValue;

		protected const int numberOfDaysToKeep = 7;
		protected int queuesCount;
		readonly string localPrintServer;

		protected override bool IsMainController => true;

		public Action<HubClientController> OnHubClientControllerCreated;

		public PrintController(string localPrintServer) : this(localPrintServer, null)
		{
		}

		public PrintController(string localPrintServer, ISynchronizeInvoke syncInvoke) : this(localPrintServer, new PrintManager(), syncInvoke)
		{
		}

		public PrintController(string localPrintServer, PrintManager printManager, ISynchronizeInvoke syncInvoke) : base(syncInvoke)
		{
			this.localPrintServer = localPrintServer;
			PrintJobManager = Argument.NotNull(printManager, nameof(printManager));
			PrintJobManager.LoggedInfo += OnShowInformation;

			SendNotificationEmailHelper.Instance.ReportError -= ReportError;
			SendNotificationEmailHelper.Instance.ReportError += ReportError;
			SendNotificationEmailHelper.Instance.SendNotificationEmail -= SendNotificationEmail;
			SendNotificationEmailHelper.Instance.SendNotificationEmail += SendNotificationEmail;
		}

		void ReportError(object sender, ReportErrorEventArgs e) => ErrorReporter.ReportOnce(e.Key, e.Message, e.Exception);

		void SendNotificationEmail(object sender, SendNotificationEmailEventArgs e) => WebServiceClient.SendNotificationEmail(e.Subject + " (" + localPrintServer + ")", e.Body);

		PrintManager PrintJobManager { get; }

		public override void Stop()
		{
			base.Stop();
			lastPrintQueueRefresh = new DateTime();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "CultureInfo")]
		protected override void InitialiseWebServiceClient()
		{
			ErrorReporter.Enable();
			if (ErrorReporter.Instance != null)
			{
				ErrorReporter.Instance.HandleError += ProcessUnhandledError;
				ErrorReporter.Instance.SendNotificationEmail += SendNotificationEmail;
				ErrorReporter.Instance.RestartApplication += OnRestartApplication;
			}

			base.InitialiseWebServiceClient();

			PrintJobManager.EnableVerboseLogging = ConfigSetting.EnableVerboseLogging;
			PrintJobManager.JobPrintingTimeout = TimeSpan.FromMinutes(ConfigSetting.JobPrintingTimeout);

			OnShowInformation(string.Format(CultureInfo.InvariantCulture, "Local Print Server: {0}", localPrintServer));
		}

		protected override void ResetWebServiceClient()
		{
			if (ErrorReporter.InstanceNoInitialize != null)
			{
				ErrorReporter.Instance.HandleError -= ProcessUnhandledError;
				ErrorReporter.Instance.SendNotificationEmail -= SendNotificationEmail;
				ErrorReporter.Instance.RestartApplication -= OnRestartApplication;
			}

			base.ResetWebServiceClient();
		}

#if DEBUG
		protected
#endif
		void ProcessUnhandledError(object sender, ErrorHandlingArgs e)
		{
			// Handle all supported server exceptions, go through inner exceptions.
			e.Handled = HandleServerException(e.Exception, true, false);
		}

		void SendNotificationEmail(object sender, SendNotificationEmailForErrorReporterEventArgs e)
		{
			try
			{
				WebServiceClient.SendNotificationEmail(e.Subject, e.Body);
				e.Handled = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				e.Handled = false;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If SignalR fails (eg, because the server is down) we need to fall back to the aspx Print Client implementation. This is true regardless of the specific error.")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		(HubConnectionManager, HubClientController) RunSignalR()
		{
			if (!EnableSignalR)
			{
				OnShowInformation("SignalR is disabled. Skipping hub connection.");
				return (null, null);
			}

			try
			{
				var client = new HubClientController(localPrintServer, PrintJobManager, update => new UpdateProcessor(update, responseProcessor: WebServiceClient.ResponseProcessor, onShowInformation: OnShowInformation), this, WebServiceClient);
				client.Logged += OnShowInformation;
				client.Updated += OnUpdated;
				OnHubClientControllerCreated?.Invoke(client);

				var hubConnection = new HubConnectionManager(ConnectionRegistryManager, client, WebServiceClient.ResponseProcessor);
				hubConnection.LogInformation += OnShowInformation;
				hubConnection.LogErrorMessage += OnShowError;
				hubConnection.Start(ConfigName);

				return (hubConnection, client);
			}
			catch (Exception ex)
			{
				OnShowInformation("Failed to initialise SignalR. It has been disabled for this run.\r\nException Message=" + ex.ToString());
				return (null, null);
			}
		}

		HubClientController hubClientController;
		HubConnectionManager hubConnectionManager;

		bool SignalRIsRunning => hubConnectionManager != null;

		void DisposeSignalRClient()
		{
			if (hubClientController != null)
			{
				hubClientController.Logged -= OnShowInformation;
				hubClientController.Updated -= OnUpdated;
				hubClientController = null;
			}

			if (hubConnectionManager != null)
			{
				hubConnectionManager.LogInformation -= OnShowInformation;
				hubConnectionManager.Dispose();
				hubConnectionManager = null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		protected override void Process()
		{
			(hubConnectionManager, hubClientController) = RunSignalR();
			try
			{
				int requestPauseInSeconds = ConnectionRegistryManager.GetRemotePrintingRequestPauseInSeconds();
				OnShowInformation("Start Printing Cycle");
				ControlProcessLoop(requestPauseInSeconds);
			}
			finally
			{
				DisposeSignalRClient();
			}
		}

		void ControlProcessLoop(int requestPauseInSeconds)
		{
			while (!ShouldStop)
			{
				ExecutePrintIteration(requestPauseInSeconds);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		void ExecutePrintIteration(int requestPauseInSeconds)
		{
			int numberOfJobs = 0;

			try
			{
				var watermark = WatermarkFromRegistry;

				CleanOldLogFiles();

				if (!NudgeWasRaised && CheckForUpdateAndUpdateInstalled())
				{
					ShouldStop = true;
					return;
				}

				LogMemoryUsage();
				ClearOldFilesInError();

				if ((!NudgeWasRaised || queuesCount == 0) && IsPrintQueuesRefreshRequired())
				{
					queuesCount = UploadQueueListToServerAndReturnQueueCount();
				}

				NudgeWasRaised = false; // Clear before processing print jobs to allow receiving new nudges sooner

				if (queuesCount == 0)
				{
					OnShowInformation("There are no installed printers");
				}
				else
				{
					OnShowInformation("");
					numberOfJobs = DownloadJobsSendToPrinterAndReturnSuccessToServer(watermark, out var printJobsWithUnhandledException);
					foreach (var printJob in printJobsWithUnhandledException)
					{
						HandleExceptionWhenProcessing(printJob.GetUnhandledException());
					}
				}
			}
			catch (Exception ex)
			{
				HandleExceptionWhenProcessing(ex);
			}

			PauseJobWhenRequested(numberOfJobs, requestPauseInSeconds);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		Watermark WatermarkFromRegistry
		{
			get
			{
				if (watermarkFromRegistry == null)
				{
					OnShowInformation("Downloading watermark info from server");
					watermarkFromRegistry = WatermarkFactory.GetWatermark(WebServiceClient.GetWatermarkInfo());
				}
				return watermarkFromRegistry;
			}
		}
		Watermark watermarkFromRegistry;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		protected virtual void PauseJobWhenRequested(int numberOfJobs, int requestPauseInSeconds)
		{
			if (!ShouldStop && numberOfJobs == 0 && !NudgeWasRaised)
			{
				if (WaitHandle.WaitOne(requestPauseInSeconds * 1000))
				{
					OnShowInformation("Nudge received to check new print jobs");
				}
			}

			WaitHandle.Reset();
		}

		bool NudgeWasRaised { get; set; }

		EventWaitHandle WaitHandle
		{
			get
			{
				if (waitHandle == null)
				{
					lock (waitHandleLock)
					{
						if (waitHandle == null)
						{
							waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
						}
					}
				}

				return waitHandle;
			}
		}
		EventWaitHandle waitHandle;
		readonly object waitHandleLock = new object();

		public void Nudge()
		{
			if (!NudgeWasRaised && NudgeMutex.WaitOne(0))
			{
				try
				{
					if (!NudgeWasRaised)
					{
						NudgeWasRaised = true;
						WaitHandle.Set();
					}
				}
				finally
				{
					NudgeMutex.ReleaseMutex();
				}
			}
		}

		Mutex NudgeMutex
		{
			get
			{
				if (nudgeMutex == null)
				{
					lock (nudgeMutexLock)
					{
						if (nudgeMutex == null)
						{
							nudgeMutex = new Mutex();
						}
					}
				}
				return nudgeMutex;
			}
		}
		Mutex nudgeMutex;
		readonly object nudgeMutexLock = new object();

		void INudgeable.Nudge()
		{
			this.Nudge();
		}

		protected bool IsPrintQueuesRefreshRequired()
		{
			var intervalBetweenScanForNewPrinters = ConnectionRegistryManager.GetRefreshTimeForNewPrintersScanInSeconds();
			if (queuesCount == 0 || (DateTime.Now - lastPrintQueueRefresh) > TimeSpan.FromSeconds(intervalBetweenScanForNewPrinters))
			{
				lastPrintQueueRefresh = DateTime.Now;
				return true;
			}
			return false;
		}

		protected void ClearOldFilesInError()
		{
			if ((DateTime.Now - lastClearOldFilesInError) > intervalToClearOldFilesInError)
			{
				try
				{
					if (Directory.Exists(Engine.Constants.ErrorDir))
					{
						var preserveSpan = TimeSpan.FromDays(numberOfDaysToKeep);
						var files = new DirectoryInfo(Engine.Constants.ErrorDir).GetFiles().Where(file => DateTime.Now - file.CreationTime > preserveSpan);
						foreach (var file in files)
						{
							file.Delete();
						}
					}
				}
				catch (IOException)
				{
					//Couldn't clear old files, that's fine
				}
				catch (SecurityException)
				{
					//Couldn't clear old files, that's fine
				}
				catch (UnauthorizedAccessException)
				{
					//Couldn't clear old files, that's fine
				}
				finally
				{
					lastClearOldFilesInError = DateTime.Now;
				}
			}
		}

		void ForcePrintQueuesRefresh()
		{
			lastPrintQueueRefresh = DateTime.MinValue;
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Out parameter makes the most sense here")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		protected int DownloadJobsSendToPrinterAndReturnSuccessToServer(Watermark watermark, out PrintResult[] printJobsWithUnhandledException)
		{
			ForceGCCollectIfNeeded();

			WebServiceClient.SyncJobStatusSafe(OnShowInformation, HandleExceptionWhenProcessing);

			OnShowInformation("Downloading job(s) from server");
			SerialisablePrintJob[] jobs;
			if (SignalRIsRunning)
			{
				// Use new GetJobsCompressed2 with quick return if no jobs found
				jobs = WebServiceClient.GetJobsCompressed2(localPrintServer);
			}
			else
			{
				// Use original GetJobsCompressed with long poll on WebPrint server
				jobs = WebServiceClient.GetJobsCompressed(localPrintServer);
			}

			jobs = FilterUnsynchronizedProcessedJobs(jobs);

			var failedJobsCount = 0;
			printJobsWithUnhandledException = Array.Empty<PrintResult>();

			if (!ShouldStop)
			{
				if (jobs.Length > 0)
				{
					failedJobsCount = AttemptToPrintJobs(jobs, watermark, out printJobsWithUnhandledException);
				}
				else
				{
					OnShowInformation("No jobs found");
				}
			}

			return jobs.Length - failedJobsCount;
		}

		SerialisablePrintJob[] FilterUnsynchronizedProcessedJobs(SerialisablePrintJob[] jobs)
		{
			if (jobs.Length == 0)
			{
				ProcessedJobsPkCache.Clear();
			}
			else
			{
				var allJobs = jobs;

				jobs = jobs.Where(j => !ProcessedJobsPkCache.Contains(j.JobPk)).ToArray();

				foreach (var pk in allJobs.Select(j => j.JobPk).ToHashSet())
				{
					ProcessedJobsPkCache.Remove(pk);
				}
			}

			var unsyncedJobs = JobStatusFileWriter.ReadAll(OnShowInformation);
			if (unsyncedJobs != null)
			{
				var processedJobs = unsyncedJobs.Where(j => j.Status == ProcessedStatus.Processed).Select(j => j.PK).ToList();

				var filteredJobs = jobs.Where(j => !processedJobs.Contains(j.JobPk)).ToArray();

				var skippedJobsCount = jobs.Length - filteredJobs.Length;
				if (skippedJobsCount > 0)
				{
					OnShowInformation(FormattableString.Invariant($"Skipped {skippedJobsCount} previously processed job(s)."));
				}

				return filteredJobs;
			}

			return jobs;
		}

		protected HashSet<Guid> ProcessedJobsPkCache { get; } = new();

		int AttemptToPrintJobs(SerialisablePrintJob[] jobs, Watermark watermark, out PrintResult[] printJobsWithUnhandledException)
		{
			int result;

			var jobPksWithInvalidPrinter = AttemptToPrintJobsCore(jobs, watermark, out printJobsWithUnhandledException);
			if (printJobsWithUnhandledException.Length == 0 && jobPksWithInvalidPrinter.Length > 0)
			{
				DisplayInaccessiblePrintersToUsers(jobPksWithInvalidPrinter);
				result = jobPksWithInvalidPrinter.Length;
			}
			else
			{
				// if we have an unhandled exception we stop everything and act as if all jobs failed
				result = printJobsWithUnhandledException.Length > 0 ? jobs.Length : 0;
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "CultureInfo")]
		PrintResult[] AttemptToPrintJobsCore(SerialisablePrintJob[] printJobs, Watermark watermark, out PrintResult[] printJobsWithUnhandledException)
		{
			PrintResult[] jobPksWithInvalidPrinter;

			if (printJobs.Length > 0)
			{
				var printJobSize = printJobs.Sum(job => job.Contents.Length);
				OnShowInformation(string.Format(CultureInfo.InvariantCulture, "Got {0} print job(s), {1} bytes", printJobs.Length, printJobSize));

				SynchroniseChangedQueues(printJobs);

				OnShowInformation("Printing job(s)");

				var printResults = PrintJobManager.Print(printJobs, watermark).ToArray();
				var failedJobs = UpdateJobStatusAndReturnFailedJobs(printResults);

				printJobsWithUnhandledException = failedJobs.Where(job => job.HadUnhandledException).ToArray();
				jobPksWithInvalidPrinter = printJobsWithUnhandledException.Length == 0 ? failedJobs : Array.Empty<PrintResult>();
			}
			else
			{
				printJobsWithUnhandledException = Array.Empty<PrintResult>();
				jobPksWithInvalidPrinter = Array.Empty<PrintResult>();
			}

			return jobPksWithInvalidPrinter;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		void SynchroniseChangedQueues(SerialisablePrintJob[] printJobs)
		{
			var allPrintQueueNames = new HashSet<string>();
			var changedPrintQueueNames = new List<string>();

			OnShowInformation("Synchronising changed print queues");

			foreach (var printJob in printJobs)
			{
				if (allPrintQueueNames.Add(printJob.QueueName) && PrintJobManager.HasQueueChanged(printJob.QueueName, printJob.QueueStateChangedStamp))
				{
					changedPrintQueueNames.Add(printJob.QueueName);
				}
			}

			if (changedPrintQueueNames.Count > 0)
			{
				OnShowInformation("Changed print queues: " + string.Join(",", changedPrintQueueNames));
				var changedQueues = WebServiceClient.GetChangedQueues(localPrintServer, changedPrintQueueNames.ToArray());
				PrintJobManager.UpdatePrintQueues(changedQueues);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		PrintResult[] UpdateJobStatusAndReturnFailedJobs(PrintResult[] printResults)
		{
			const bool Success = true;

			var filteredJobs = printResults.ToLookup(r => r.IsSuccess);

			var successfulJobs = filteredJobs[Success].Select(r => r.JobPK).ToArray();
			if (successfulJobs.Length > 0)
			{
				OnShowInformation("Returning success to server");
				WebServiceClient.SetJobSuccess(successfulJobs, OnShowInformation);
			}
			foreach (var job in successfulJobs)
			{
				ProcessedJobsPkCache.Add(job);
			}

			var failedJobs = filteredJobs[!Success].ToArray();
			if (failedJobs.Length > 0)
			{
				var message = @"Returning failed jobs to the server
The server will attempt to notify the sender of the documents that have failed three times.";
				OnShowInformation(message);
				WebServiceClient.SetJobFailure(failedJobs.Select(failedJob => new PrintJobFailed { JobPk = failedJob.JobPK, FailureReason = failedJob.FailureReason }).ToArray(), OnShowInformation);
			}

			return failedJobs;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "CultureInfo")]
		void DisplayInaccessiblePrintersToUsers(PrintResult[] jobPksWithInvalidPrinter)
		{
			var failedJobsCount = jobPksWithInvalidPrinter.Length;
			ForcePrintQueuesRefresh();

			var inaccessiblePrintersMessages = new StringBuilder();
			var inaccessiblePrinters = jobPksWithInvalidPrinter.Select(j => j.PrinterName).Distinct().ToArray();
			inaccessiblePrintersMessages.AppendLine(string.Format(CultureInfo.InvariantCulture, "The following {0} no longer accessible:", inaccessiblePrinters.Length == 1 ? "printer is" : "printers are"));

			foreach (var printer in inaccessiblePrinters)
			{
				inaccessiblePrintersMessages.AppendLine("\t" + printer);
			}

			inaccessiblePrintersMessages.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0} print {1} still referencing {2} and will not be printed unless {2} {3} reinstalled.",
				failedJobsCount,
				failedJobsCount == 1 ? "job is" : "jobs are",
				inaccessiblePrinters.Length == 1 ? "this printer" : "these printers",
				inaccessiblePrinters.Length == 1 ? "is" : "are"));
			inaccessiblePrintersMessages.AppendLine("The printers list will be refreshed.");
			OnShowInformation(inaccessiblePrintersMessages.ToString());
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "OnShowInformation")]
		public int UploadQueueListToServerAndReturnQueueCount()
		{
			if (WebServiceClient.IsSupportUser)
			{
				OnShowInformation("CWSupport user cannot send print client queue list to server");
				return 0;
			}

			OnShowInformation("Sending print client queue list to server");

			var printQueues = PrintJobManager.InitialisePrintQueuesAndReturnQueueList();
			if (printQueues.Count > 0)
			{
				var printQueueNames = printQueues.Select(printerInfo => printerInfo.Name).ToArray();
				OnShowInformation(GetPrintQueueListMessage(printQueueNames));
				WebServiceClient.SetQueuesEx(localPrintServer, printQueues.ToArray());
				hubConnectionManager?.UpdatePrinters(localPrintServer, printQueueNames);
			}

			return printQueues.Count;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "printQueueNames")]
		static string GetPrintQueueListMessage(IEnumerable<string> printQueueNames)
		{
			return printQueueNames.Aggregate("Installed Printers:\r\n", (current, printerName) => current + ("\t" + printerName + "\r\n"));
		}
	}
}
