using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.PrintProcessing
{
	public class PrintQueueManager
	{
		/// <summary>
		/// Updates the list of available printers. 
		/// Any printers that are no longer available on this machine are marked as deleted. 
		/// If a printer that was previously removed is reinstalled, then it will reappear in the print queue.
		/// </summary>
		public bool MaintainStmPrintQueue()
		{
#if DEBUG
			CountOfCallsToMaintainStmPrintQueue++;
#endif

			ReadOnlyCollection<PrinterInfo> availablePrinters = null;
			try
			{
				availablePrinters = InstalledPrinters;
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				HandleException(exception);
			}

			bool updated = false;
			if (availablePrinters != null)
			{
				updated = UpdateListOfPrinters(availablePrinters);
			}

			return updated;
		}

		void HandleException(Exception ex)
		{
			string explanatoryMessage = null;
			if (ErrorHandler.RPC.IsRPCServerUnavailableException(ex))
			{
				explanatoryMessage = Res.GetString("aa3f9616-bdb0-454a-89a3-eda96352f080", @"{0} could not retrieve the list of available printers on {1} and did not update the available print queues for this server.

Please check 'Control Panel' > 'Printers and Faxes' for any printers that have errors, and that the 'Print Spooler' windows service is running.  If there are problems with the Remote Procedure Call (RPC) service on {1} it can also stop the list of printers being updated.

If you are getting this warning repeatedly, or are having trouble printing, please check (or get your IT provider to check)the Windows 'Event Viewer' under 'Administrative Tools' on that machine for possible causes.",
					Core.Constants.ProductName,
					System.Environment.MachineName);
			}
			else if (ex is Win32Exception && ex.IsHResult(-2147467259)) // The data area passed to a system call is too small
			{
				explanatoryMessage = Res.GetString("909fadd2-119a-4a2e-be8e-78a86d48fd0e", @"{0} could not retrieve the list of available printers on {1} and did not update the available print queues for this server.

While the service task was trying to get the list of available printers, the list was being changed so it could not be read properly. The service task will try and update the list of printers again the next time it is restarted.

If you are getting this warning repeatedly, or are having trouble printing, please check (or get your IT provider to check)the Windows 'Event Viewer' under 'Administrative Tools' on that machine for possible causes.",
					Core.Constants.ProductName,
					System.Environment.MachineName);
			}

			if (explanatoryMessage != null)
			{
				string caption = Res.GetString("cc9da509-dc3c-46a8-923c-3a8790cea253", "Print Queues not updated for {0}", System.Environment.MachineName);
				try
				{
					UnattendedUserNotification.Instance.ShowWarning(explanatoryMessage, caption, true);
				}
				catch (EmailHasNoRecipientsException emailHasNoRecipientsException)
				{
					throw new HostedServiceException(string.Format("{0}, and failed to send a notification e-mail with error message: {1}", caption, emailHasNoRecipientsException.Message), emailHasNoRecipientsException);
				}
			}
			else
			{
				ErrorReporter.ReportOnce("PrintQueueManager.MaintainStmPrintQueue", ex.Message, ex);
			}
		}

		bool UpdateListOfPrinters(ReadOnlyCollection<PrinterInfo> availablePrinters)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
			serverSubQuery.AddToFilter(StmPrintServerSchema.SPS_ServerName, System.Environment.MachineName);
			var queuesOnThisServerQuery = new ZDBOnlyQuery(typeof(StmPrintQueue));
			queuesOnThisServerQuery.AddSubQuery(serverSubQuery, JoinCondition.And);
			StmPrintQueueCollection printQueues = new StmPrintQueueCollection(factory, queuesOnThisServerQuery);
			printQueues.Load();

			DeactivateOrDeleteQueuesIfUnavailable(availablePrinters, printQueues);
			ReactivateOrCreateQueuesIfAvailable(availablePrinters, printQueues);

			bool queuesUpdated = printQueues.HasChanges;
			factory.Save();
			return queuesUpdated;
		}

		void DeactivateOrDeleteQueuesIfUnavailable(ReadOnlyCollection<PrinterInfo> availablePrinters, StmPrintQueueCollection printQueues)
		{
			for (int i = printQueues.Count - 1; i >= 0; i--)
			{
				StmPrintQueue printQueue = printQueues[i];

				bool printQueueExists = false;
				foreach (PrinterInfo printer in availablePrinters)
				{
					if (String.Equals(printQueue.SQ_QueueName, printer.Name, StringComparison.OrdinalIgnoreCase))
					{
						printQueueExists = printer.IsValid;
						break;
					}
				}

				if (!printQueueExists)
				{
					if (printQueue.SQ_QueueDeleted.IsEmpty)
					{
						printQueue.SQ_QueueDeleted = ZDateTime.Now;
					}
				}
			}
		}

		void ReactivateOrCreateQueuesIfAvailable(ReadOnlyCollection<PrinterInfo> availablePrinters, StmPrintQueueCollection printQueues)
		{
			foreach (PrinterInfo printer in availablePrinters)
			{
				if (!printer.IsValid)
				{
					continue;
				}

				StmPrintQueue printQueue = null;
				foreach (StmPrintQueue loadedPrintQueue in printQueues)
				{
					if (String.Equals(loadedPrintQueue.SQ_QueueName, printer.Name, StringComparison.OrdinalIgnoreCase))
					{
						loadedPrintQueue.SQ_QueueDeleted = ZDateTime.Empty;
						printQueue = loadedPrintQueue;
						break;
					}
				}

				if (printQueue == null)
				{
					printQueue = printQueues.AddNew();
					printQueue.SQ_QueueName = printer.Name;
					printQueue.SQ_AllowPrinting = !printer.IsSuspectedSurrogate;
					printQueue.SQ_ServerName = System.Environment.MachineName;
					printQueue.SQ_DisplayName = printQueue.GetDefaultDisplayName();
				}

				PaperSizeGetter paperSize = new PaperSizeGetter(printer.Name);
				if (paperSize.IsValid)
				{
					printQueue.SQ_PaperName = paperSize.PaperName.Left(printQueue.SQ_PaperNameInfo.MaxLength);
					printQueue.SQ_PaperHeight = paperSize.Height;
					printQueue.SQ_PaperWidth = paperSize.Width;
				}
				else
				{
					if (!string.IsNullOrEmpty(paperSize.ErrorMessage))
					{
						UnattendedUserNotification.Instance.ShowWarning(paperSize.ErrorMessage, Res.GetString("f8600e67-e2ba-4686-968b-1c25dd3f820c", "{0} could not get the paper size for the printer '{1}'.", Core.Constants.ProductName, printer.Name), true);
					}
				}
			}
		}

		/// <summary>
		/// This property has been split out to make testing easier
		/// </summary>
		protected internal virtual ReadOnlyCollection<PrinterInfo> InstalledPrinters
		{
			get { return SafeInstalledPrinters.AllPrinters; }
		}

#if DEBUG
		[ThreadStatic]
		public static int CountOfCallsToMaintainStmPrintQueue;

#endif
	}
}
