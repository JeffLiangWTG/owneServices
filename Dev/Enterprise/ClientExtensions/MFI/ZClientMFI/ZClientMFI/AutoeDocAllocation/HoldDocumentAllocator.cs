using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.MFI.AutoeDocAllocation
{
	class HoldDocumentAllocator : DocumentAllocator
	{
		protected override string DirectoryToProcess
		{
			get { return MFIDataRegistry.Instance.AutoeDocAllocationHoldDirectory; }
		}

		protected override void ProcessUnattachedFile(FileInfo documentToAttach, string readableFileName, INotifications notify)
		{
			base.ProcessUnattachedFile(documentToAttach, readableFileName, notify);

			if (HoldPeriodHasExpired(documentToAttach.Name))
			{
				notify.Notify(new InfoNotification(string.Format("File {0} has been rejected as the period of time to hold it has expired.", readableFileName)));
				SendExpiredEmail(notify, documentToAttach);
				documentToAttach.Delete();
				LogCounters.FilesRejected++;
			}
			else
			{
				notify.Notify(new InfoNotification(string.Format("Document {0} is still unable to be attached.", readableFileName)));
			}
		}

		protected override void CommenceLogging(FileInfo[] filesFound, ZString directoryToProcess, INotifications notify)
		{
			notify.Notify(new InfoNotification(string.Format("Searching Hold Directory {0} for previously held documents...", directoryToProcess)));

			base.CommenceLogging(filesFound, directoryToProcess, notify);
		}

		protected override void FinishLoggingForThisParse(INotifications notify)
		{
			if (LogCounters.FilesAttached > 0)
			{
				if (LogCounters.FilesAttached > 1)
				{
					notify.Notify(new InfoNotification(string.Format("{0} files have now been attached - deleted from the hold directory", LogCounters.FilesAttached.ToString())));
				}
				else
				{
					notify.Notify(new InfoNotification("1 file has now been attached - deleted from the hold directory"));
				}
			}

			base.FinishLoggingForThisParse(notify);
			notify.Notify(new InfoNotification(System.Environment.NewLine));
		}

		bool HoldPeriodHasExpired(string fileNameWithDateStamp)
		{
			bool periodHasExpired = false;
			var heldSince = GetTimeStampFromFileName(fileNameWithDateStamp);
			if (heldSince.AddDays(MFIDataRegistry.Instance.AutoeDocAllocationHoldPeriod) < ZDateTime.Now)
			{
				periodHasExpired = true;
			}

			return periodHasExpired;
		}

		protected override string GetReadableFileName(string fileName)
		{
			string readableFileName = fileName;

			int fileHoldDateIndex = fileName.IndexOf("_" + ZDateTime.Now.Year.ToString());
			if (fileHoldDateIndex < 0)
			{
				fileHoldDateIndex = fileName.IndexOf("_" + (ZDateTime.Now.Year - 1).ToString());
			}

			if (fileHoldDateIndex > 0)
			{
				readableFileName = fileName.Remove(fileHoldDateIndex);
			}

			return readableFileName;
		}

		int GetTimeStampIndex(string fileName)
		{
			int timeStampIndex = fileName.IndexOf("_" + ZDateTime.Now.Year.ToString());
			if (timeStampIndex < 0)
			{
				timeStampIndex = fileName.IndexOf("_" + (ZDateTime.Now.Year - 1).ToString());
			}
			return timeStampIndex;
		}

		ZDateTime GetTimeStampFromFileName(string fileName)
		{
			var fileHeldTimeStamp = ZDateTime.Now;
			int timeStampIndex = GetTimeStampIndex(fileName);

			if (timeStampIndex > 0)
			{
				try
				{
					string dateStampString = fileName.Remove(0, timeStampIndex + 1);
					int dSYear = Convert.ToInt32(dateStampString.Substring(0, 4));
					int dSMonth = Convert.ToInt32(dateStampString.Substring(4, 2));
					int dSDay = Convert.ToInt32(dateStampString.Substring(6, 2));
					int dSHour = Convert.ToInt32(dateStampString.Substring(8, 2));
					int dSMin = Convert.ToInt32(dateStampString.Substring(10, 2));
					int dSSec = Convert.ToInt32(dateStampString.Substring(12, 2));
					fileHeldTimeStamp = new ZDateTime(dSYear, dSMonth, dSDay, dSHour, dSMin, dSSec);
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}

			return fileHeldTimeStamp;
		}
	}
}
