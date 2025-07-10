using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.PrintProcessing
{
	class HardCopyProcessor : MergedPrintGroupProcessor
	{
		public HardCopyProcessor(StmPrintJobMergedCollection mergedPrintGroup, IPrinterFactory printerFactory)
			: base(mergedPrintGroup)
		{
			PrinterFactory = Argument.NotNull(printerFactory, nameof(printerFactory));
		}

		readonly IPrinterFactory PrinterFactory;

		protected override void ProcessIndividualItemCore(StmPrintJob printJob)
		{
			switch ((PrintType)Enum.Parse(typeof(PrintType), printJob.SP_JobType, true))
			{
				case PrintType.PRN:
					PrintHardCopy(printJob);
					printJob.CreateLogOnParent(Events.DocumentSent);
					break;
				case PrintType.PRS:
					printJob.CreateLogOnParent(Events.DocumentSent);
					break;
				default:
					throw new ArgumentException("Invalid SP_JobType: " + printJob.SP_JobType);
			}
		}

		#region Printing

		void PrintHardCopy(StmPrintJob printJob)
		{
			if (printJob.PrintQueue == null)
			{
				throw new ApplicationException("Invalid print job - print queue cannot be null when job type is PRN");
			}

			string printQueueName = printJob.PrintQueue.SQ_QueueName;
			try
			{
				if (!IsValidPrinter(printQueueName))
				{
					GenerateAndSendPrinterNotAvailableMessage(printQueueName);
				}
				else
				{
					if (File.Exists(printJob.StoredAttachmentFilename))
					{
						using (BasePrinter printer = GetPrinterForPrintJob(printJob))
						{
							printer.Print();
						}
					}
					else
					{
						throw new ApplicationException("Filename " + printJob.StoredAttachmentFilename + " does not exist. Cannot print this job (PK: " + printJob.PK + ")");
					}
				}
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				if (ErrorHandler.RPC.IsRPCServerUnavailableException(exception))
				{
					GenerateAndSendRPCServerErrorUnavailableMessage(printQueueName);
				}
				else if (exception is Win32Exception)
				{
					var message = FormattableString.Invariant($"{exception.Message} - The printer name is {printQueueName}"); // Do not need to translate
					throw new Win32Exception(message, exception);
				}
				else
				{
					throw;
				}
			}
		}

		BasePrinter GetPrinterForPrintJob(StmPrintJob printJob)
		{
			var fileContents = File.ReadAllBytes(printJob.StoredAttachmentFilename);
			var printEngineJob = printJob.GetPrintEngineJob(fileContents);
			return PrinterFactory.GetPrinter(printEngineJob);
		}

		#endregion

		#region Validation

		protected virtual bool IsValidPrinter(string printQueueName)
		{
			return SafeInstalledPrinters.IsValidPrinter(printQueueName);
		}

		void GenerateAndSendPrinterNotAvailableMessage(string printQueueName)
		{
			string subject = Res.GetString("a12a81f5-1e9c-4422-94e3-2d48cd7b7634", "Printer {0} is unavailable on server {1}", printQueueName, System.Environment.MachineName);
			string message = Res.GetString("038750e1-b041-4583-a892-55562014207c", @"The printer {0} is unavailable on server {1}.
{2}
Please check 'Control Panel' > 'Printers and Faxes' to ensure that this printer is online.
If you are getting this warning repeatedly, or are having trouble printing in general, please get your IT provider to check the list of printers and the Windows Event Viewer on {1} for possible causes.",
				printQueueName, System.Environment.MachineName, GetPrintJobRelatedInfo());

			UnattendedUserNotification.Instance.ShowErrorOnceADay("HardCopyProcessor_PrinterNotValid:" + printQueueName, message, subject, true);
		}

		#region SuppressResourceStringsCheckRegion

		string GetPrintJobRelatedInfo()
		{
			var printJobInfo = new ZStringBuilder();

			MergedPrintGroup.OfType<StmPrintJob>().ForEach(job =>
			{
				printJobInfo.Append(FormattableString.Invariant($"	Job Name: {job.SP_EmailSubjectLine}"));
				printJobInfo.Append(FormattableString.Invariant($"	Job Type: {job.SP_JobType}"));
				printJobInfo.Append(FormattableString.Invariant($"	Printer Name: {job.PrintQueue?.SQ_DisplayName}"));
				printJobInfo.Append(FormattableString.Invariant($"	Server Name: {job.PrintQueue?.SQ_ServerName}\r\n"));
			});

			return FormattableString.Invariant($@"
	Service Task Code: {Env.Instance.ServiceTaskCode}

{printJobInfo.ToStringWithNewLineBetweenAppends()}");
		}

		#endregion

		void GenerateAndSendRPCServerErrorUnavailableMessage(string printQueueName)
		{
			string message = Res.GetString("8a2a3cd5-ef6c-4996-bcdb-15dd9a40572d", @"{0} was not able to print to {1} on server {2}. It cannot check that the printer exists.

If there are any other printers in the printers list with errors, OR the Remote Procedure Call (RPC) service on {2} is experiencing problems, then it can affect printing.

Please check 'Control Panel' > 'Printers and Faxes' for any printers that have errors. If you are getting this warning repeatedly, please get your IT provider to check the Windows 'Event Viewer' under 'Administrative Tools' on {2} for possible causes.",
					Core.Constants.ProductName,
					printQueueName,
					System.Environment.MachineName);

			string caption = Res.GetString("17f99ac9-8b19-4042-b8f7-8ef83808c329", "Cannot print to printer {0} on server {1}", printQueueName, System.Environment.MachineName);

			UnattendedUserNotification.Instance.ShowErrorOnceADay("HardCopyProcessor_RPCServerNotAvailable", message, caption, true);
		}

		#endregion
	}
}
