using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	public class PrintManager
	{
		public PrintManager()
			: this(new PrintEngineProcessor(new PrinterFactory()))
		{
		}

		public PrintManager(IPrintEngineProcessor processor)
		{
			Processor = Argument.NotNull(processor, nameof(processor));
			Processor.Logged += Processor_Logged;
		}

		void Processor_Logged(object sender, LogEventArgs e)
		{
			LogInfo(e.Message);
		}

		public event EventHandler<LogEventArgs> LoggedInfo;

		public event EventHandler<LogEventArgs> LoggedError;

		void LogInfo(string log)
		{
			LoggedInfo?.Invoke(this, new LogEventArgs(log));
		}
		void LogError(string log)
		{
			LoggedError?.Invoke(this, new LogEventArgs(log));
		}

		IPrintEngineProcessor Processor { get; }

		public TimeSpan JobPrintingTimeout { get; set; }

		/// <summary>
		/// Populates print queue list.
		/// Note:
		///		StateChangedStamp is initialised with a NEW GUID so that the queue attributes
		///		will be updated with the server values when the first jobs are downloaded.
		/// </summary>
		public IList<PrinterInfo> InitialisePrintQueuesAndReturnQueueList()
		{
			var installedPrinters = GetInstalledPrinters().ToList();
			Processor.UpdatePrinters(installedPrinters.Select(printerInfo => printerInfo.Name));

			return installedPrinters;
		}

		protected virtual IEnumerable<string> GetInstalledPrinterNames() => SafeInstalledPrinters.InstalledLocalPrinterNames;

		protected virtual IEnumerable<PrinterInfo> GetInstalledPrinters() => SafeInstalledPrinters.InstalledLocalPrinters;

		public IEnumerable<PrintResult> Print(SerialisablePrintJob[] printJobs, Watermark watermark)
		{
			if (printJobs.Length == 0)
			{
				return Enumerable.Empty<PrintResult>();
			}

			var printJobsByPrinter = printJobs.ToLookup(j => j.QueueName, StringComparer.OrdinalIgnoreCase);

			if (printJobsByPrinter.Count == 1)
			{
				// All jobs to single printer - print them synchronously
				return PrintJobs(printJobs, watermark);
			}
			else
			{
				try
				{
					return Task.WhenAll(
							printJobsByPrinter.Select(
								async jobs => await PrintJobsAsync(jobs, watermark).ConfigureAwait(false))
						)
						.Result
						.SelectMany(r => r).ToList();
				}
				catch (AggregateException ex)
				{
					LogError($"Error when printing jobs asynchronously: {ErrorReporter.GetExceptionMessage(ex)}");
				}
			}

			return Enumerable.Empty<PrintResult>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "CultureInfo")]
		IEnumerable<PrintResult> PrintJobs(IEnumerable<SerialisablePrintJob> printJobs, Watermark watermark)
		{
			var results = new List<PrintResult>();

			foreach (var job in printJobs)
			{
				var documentName = job?.EmailSubjectLine;

				LogInfo(string.Format(CultureInfo.InvariantCulture, "Begin printing job [{0}]", documentName));

				var result = JobPrintingTimeout.TotalSeconds > 1
					? PrintJobAsyncWithTimeout(job, watermark).Result
					: PrintJob(job, watermark);

				LogInfo(string.Format(CultureInfo.InvariantCulture, "End printing job [{0}]", documentName));

				results.Add(result);

				if (result.HadUnhandledException)
				{
					break;
				}
			}

			return results;
		}

		async Task<IEnumerable<PrintResult>> PrintJobsAsync(IEnumerable<SerialisablePrintJob> printJobs, Watermark watermark)
		{
			return await Task.Run(() => PrintJobs(printJobs, watermark)).ConfigureAwait(false);
		}

		public PrintResult PrintJob(SerialisablePrintJob printJob, Watermark watermark) => Processor.Print(printJob, watermark);

		public Task<PrintResult> PrintJobAsync(SerialisablePrintJob printJob, Watermark watermark)
		{
			if (JobPrintingTimeout.TotalSeconds > 1)
			{
				return PrintJobAsyncWithTimeout(printJob, watermark);
			}
			else
			{
				return Processor.PrintAsync(printJob, watermark);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Printing message")]
		public Task<PrintResult> PrintJobAsyncWithTimeout(SerialisablePrintJob printJob, Watermark watermark)
		{
			return Task.Run(() =>
			{
				var printTask = Processor.PrintAsync(printJob, watermark);
				printTask.Wait(JobPrintingTimeout);
				if (printTask.IsCompleted)
				{
					return printTask.Result;
				}
				else
				{
					return PrintResult.WithFailureReason(printJob.JobPk, printJob.QueueName, "Printing did not finish after timeout of " + (int)JobPrintingTimeout.TotalMinutes + " minutes.");
				}
			});
		}

		public void UpdatePrintQueues(SerialisablePrintQueue[] changedQueues)
		{
			foreach (var changedQueue in changedQueues)
			{
				Processor.SetPrinter(changedQueue);
			}
		}

		public bool HasQueueChanged(string queueName, Guid queueStateChangedStamp) => Processor.HasQueueChanged(queueName, queueStateChangedStamp);

		public bool EnableVerboseLogging
		{
			get => Processor.EnableVerboseLogging;
			set => Processor.EnableVerboseLogging = value;
		}
	}
}
