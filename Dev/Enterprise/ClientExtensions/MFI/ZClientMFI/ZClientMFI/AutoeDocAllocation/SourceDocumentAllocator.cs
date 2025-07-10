using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.MFI.AutoeDocAllocation
{
	class SourceDocumentAllocator : DocumentAllocator
	{
		protected override string DirectoryToProcess
		{
			get { return MFIDataRegistry.Instance.AutoeDocAllocationSourceDirectory; }
		}

		protected override void ProcessUnattachedFile(FileInfo documentToAttach, string readableFileName, INotifications notify)
		{
			base.ProcessUnattachedFile(documentToAttach, readableFileName, notify);

			notify.Notify(new InfoNotification(string.Format("Document {0} is not able to be attached.", readableFileName)));
			string movedFileName = readableFileName + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss");
			string newFileLocation = Path.Combine(MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory, movedFileName);
			documentToAttach.MoveTo(newFileLocation);
			LogCounters.FilesMoved++;
		}

		protected override void CommenceLogging(FileInfo[] filesFound, ZString directoryToProcess, INotifications notify)
		{
			notify.Notify(new InfoNotification(string.Format("Searching Source Directory {0} for new documents to allocate...", directoryToProcess)));

			base.CommenceLogging(filesFound, directoryToProcess, notify);
		}

		protected override void FinishLoggingForThisParse(INotifications notify)
		{
			if (LogCounters.FilesAttached + LogCounters.FilesMoved > 0)
			{
				if (LogCounters.FilesAttached > 0 && LogCounters.FilesMoved > 0)
				{
					notify.Notify(new InfoNotification(string.Format("{0}{1} files deleted and/or moved from the source directory", LogCounters.FilesAttached, LogCounters.FilesMoved)));
				}
				else if (LogCounters.FilesAttached > 0)
				{
					if (LogCounters.FilesAttached > 1)
					{
						notify.Notify(new InfoNotification(string.Format("{0} attached files deleted from the source directory", LogCounters.FilesAttached.ToString())));
					}
					else
					{
						notify.Notify(new InfoNotification("1 attached file deleted from the source directory"));
					}
				}
				else if (LogCounters.FilesMoved > 0)
				{
					if (LogCounters.FilesMoved > 0)
					{
						notify.Notify(new InfoNotification(string.Format("{0} unable to allocate files have been moved to the hold directory", LogCounters.FilesMoved.ToString())));
					}
					else
					{
						notify.Notify(new InfoNotification("1 unable to allocate file has been moved to the hold directory"));
					}
				}
			}

			base.FinishLoggingForThisParse(notify);

			notify.Notify(new InfoNotification("Attaching Documents completed"));
			notify.Notify(new InfoNotification(System.Environment.NewLine));
			notify.Notify(new InfoNotification(System.Environment.NewLine));
		}
	}
}
