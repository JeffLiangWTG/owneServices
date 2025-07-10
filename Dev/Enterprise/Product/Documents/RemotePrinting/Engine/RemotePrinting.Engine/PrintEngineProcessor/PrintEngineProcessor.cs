using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.RemotePrinting.Types;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.RemotePrinting.Engine
{
	public class PrintEngineProcessor : IPrintEngineProcessor
	{
		public PrintEngineProcessor(IPrinterFactory printerFactory)
		{
			PrinterFactory = Argument.NotNull(printerFactory, nameof(printerFactory));
			PrintQueues = new Dictionary<string, SerialisablePrintQueue>(StringComparer.OrdinalIgnoreCase);
		}

		IPrinterFactory PrinterFactory { get; }
		Dictionary<string, SerialisablePrintQueue> PrintQueues { get; }

		public event EventHandler<LogEventArgs> Logged;

		#region Print Queues

		public void SetPrinter(SerialisablePrintQueue changedQueue)
		{
			PrintQueues[changedQueue.Name] = changedQueue;
		}

		public void UpdatePrinters(IEnumerable<string> installedPrinters)
		{
			PrintQueues.Clear();

			foreach (var queueName in installedPrinters)
			{
				PrintQueues.Add(queueName, new SerialisablePrintQueue { Name = queueName, StateChangedStamp = Guid.NewGuid() });
			}
		}

		public bool HasQueueChanged(string queueName, Guid queueStateChangedStamp)
		{
			return PrintQueues.TryGetValue(queueName, out var queue) && queue != null && queue.StateChangedStamp != queueStateChangedStamp;
		}

		#endregion Print Queues

		public async Task<PrintResult> PrintAsync(SerialisablePrintJob jobToPrint, Watermark watermark)
		{
			Argument.NotNull(jobToPrint, nameof(jobToPrint));

			return await Task.Run(() => Print(jobToPrint, watermark)).ConfigureAwait(false);
		}

		public PrintResult Print(SerialisablePrintJob jobToPrint, Watermark watermark)
		{
			Argument.NotNull(jobToPrint, nameof(jobToPrint));

			var printJob = ConvertToPrintEngineJob(jobToPrint, watermark, out var conversionResult);
			return conversionResult ?? PrintJob(printJob);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		PrintEngineJob ConvertToPrintEngineJob(SerialisablePrintJob jobToPrint, Watermark watermark, out PrintResult conversionResult)
		{
			Argument.NotNull(jobToPrint, nameof(jobToPrint));

			if (PrintQueues.TryGetValue(jobToPrint.QueueName, out var printQueue) && printQueue != null)
			{
				if (string.IsNullOrEmpty(jobToPrint.BlobType))
				{
					conversionResult = PrintResult.WithFailureReason(jobToPrint.JobPk, jobToPrint.QueueName, "File Type of Print Job unknown.");
				}
				else
				{
					if (string.IsNullOrEmpty(jobToPrint.EmailSubjectLine))
					{
						jobToPrint.EmailSubjectLine = "[NO SUBJECT]";
					}

					conversionResult = null;
					return new PrintEngineJob(jobToPrint, printQueue, (jobToPrint.HasWatermark ? watermark : null));
				}
			}
			else
			{
				conversionResult = PrintResult.WithFailureReason(jobToPrint.JobPk, jobToPrint.QueueName, "Printer not found.");
			}

			return null;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		PrintResult PrintJob(PrintEngineJob job)
		{
			Argument.NotNull(job, nameof(job));

			PrintResult result;

			if (string.IsNullOrEmpty(job.DocumentName))
			{
				result = PrintResult.Fail(job.JobPk);
			}
			else
			{
				try
				{
					Log(string.Format(CultureInfo.InvariantCulture, "Getting printer for job [{0}].", job.DocumentName));
					var printer = PrinterFactory.GetPrinter(job);
					if (printer == null)
					{
						Log(string.Format(CultureInfo.InvariantCulture, "The file type is {0} and cannot be printed.", job.BlobType));
						return PrintResult.Fail(job.JobPk);
					}

					try
					{
						if (EnableVerboseLogging)
						{
							Log(string.Format(CultureInfo.InvariantCulture, "Printer type: {0}.", printer.GetType().Name));
							printer.Logged += OnLogged;
						}

						Log(string.Format(CultureInfo.InvariantCulture, "Printing job [{0}].", job.DocumentName));

						printer.Print();

						Log(string.Format(CultureInfo.InvariantCulture, "Printed job [{0}].", job.DocumentName));
						result = PrintResult.Success(job.JobPk);
					}
					finally
					{
						((IDisposable)printer).Dispose();
						printer.Logged -= OnLogged;
					}
				}
				catch (InvalidPrinterException)
				{
					result = PrintResult.WithFailureReason(job.JobPk, job.PrinterName, "Printer selected is no longer valid or not currently available / on-line.");
				}
				catch (Exception ex)
				{
					var failureReason = ExceptionParser(ex);
					result = PrintResult.WithUnhandledException(job.JobPk, job.PrinterName, failureReason, ex);
				}
			}

			return result;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		string ExceptionParser(Exception exception)
		{
			if (string.IsNullOrEmpty(exception?.Message))
			{
				return "An unknown error happened.";
			}

			if (exception is Win32Exception)
			{
				return "Driver Communication Error sending job to selected Printer.";
			}

			if (exception is FlexCelXlsAdapterException { ErrorCode: XlsErr.ErrFileIsNotSupported })
			{
				return "The file is not on any on the formats supported by FlexCel.";
			}

			if (exception is ExternalException { Source: "System.Drawing", ErrorCode: -2147467259 })
			{
				return @"A generic error occurred in GDI+.

Microsoft GDI+ has had a generic failure rendering a document. 
This can be caused by a machine being low on memory, out of window handles, having a problem with a video driver, or some sort of memory corruption creating an unexpected un-handled machine state.

If these errors continue to occur, please try rebooting this machine, adding more memory to this machine, or moving this process to another machine.";
			}

			if (exception is IOException or UnauthorizedAccessException)
			{
				return "An IO error occurred. Please check user permissions, security settings, that the folder exists and that there are no network errors/mis-configurations.";
			}

			if (exception is FlexCelCoreException { ErrorCode: FlxErr.ErrFontNotSupported })
			{
				return @"A font was used in a template that is not installed or corrupted on the machine running your Printing Service Tasks.

Please either install this font on the machine running your Printing Service Tasks or remove it from the template.";
			}

			return exception.Message;
		}

		void Log(string log)
		{
			Logged?.Invoke(this, new LogEventArgs(log));
		}

		void OnLogged(object sender, LogEventArgs logEventArgs)
		{
			Log(logEventArgs.Message);
		}

		public bool EnableVerboseLogging { get; set; }
	}
}
