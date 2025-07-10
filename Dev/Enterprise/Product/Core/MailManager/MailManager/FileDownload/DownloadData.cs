using System;
using System.IO;
using System.Net;
using System.Threading;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = MailManager.Res;

namespace Enterprise.MailManager.FileDownload
{
	public class DownloadData
	{
		public DownloadData(WebResponse response, string fileName, long size, long start, WaitHandle cancelEvent, ManualResetEvent finishedEvent)
		{
			fResponse = response;
			fFileName = fileName;
			fSize = size;
			fStart = start;
			fCancelEvent = cancelEvent;
			fTotalDownloaded = start;
			fFinishedEvent = finishedEvent;
		}

		public void DoDownload()
		{
			if (FinishIfUserCancelled())
			{
				return;
			}

			fStream = null;

			byte[] buffer = new byte[downloadBlockSize];

			int readCount;

			try
			{
				while ((readCount = DownloadStream.Read(buffer, 0, downloadBlockSize)) > 0)
				{
					if (HasUserCancelled())
					{
						NotifyWhenFinished();
						return;
					}

					SaveToFile(buffer, readCount);

					fTotalDownloaded += readCount;

					if (FinishIfUserCancelled())
					{
						return;
					}
				}

				bool done = (fSize == -1 || fTotalDownloaded == fSize);

				if (done)
				{
					FinaliseDownload();
					fCompleted = true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				CaughtException = ex;
				IOException ioException = ex as IOException;

				if (ioException != null && ioException.IsDiskFull())
				{
					SendNoSpaceMailToPostMasters();
				}
			}
			finally
			{
				if (fFile != null)
				{
					fFile.Close();
					fFile = null;
				}
			}

			NotifyWhenFinished();
		}

		protected void SendNoSpaceMailToPostMasters()
		{
			string driveName = Path.GetPathRoot(fFileName);
			string computerName = System.Environment.MachineName;
			string fileDescription = Res.GetString("e05bd816-770e-47ae-82bc-f2f224024df6", "file");
			if (Path.GetExtension(fFileName) == Enterprise.MasterFiles.Business.StmUpgrade.EDPFileExtension)
			{
				fileDescription = Res.GetString("45a823e7-bf02-4cfb-8175-a1ee37248267", "upgrade package file");
			}

			string messageBody = Res.GetString("33a846f6-7789-4a05-8b8f-8bd90436b5d0", "There was not enough disk space on disk {0} on {1} to download the {3} {2}. Please free up more space on disk {0} to allow the mail batch processor to complete downloading the file.", driveName, computerName, fFileName, fileDescription);

			EmailDef email = new EmailDef();
			email.Body = messageBody;
			email.Subject = Res.GetString("6329d851-6b4e-40bd-b421-1a0dc7357c63", "Mail Batch Processor Could Not Download {0}", fFileName);
			Env.OutgoingMailManager.CreateAndSave(email, Env.Registry.PostMasterGroup, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
		}

		public long TotalDownloaded
		{
			get { return fTotalDownloaded; }
		}

		public bool Completed
		{
			get { return fCompleted; }
		}

		public bool HasException
		{
			get { return fException != null; }
		}

		public Exception CaughtException
		{
			get { return fException; }
			set { fException = value; }
		}

		#region Implementation

		#region Private fields

		readonly WebResponse fResponse;
		readonly string fFileName;
		readonly long fSize;
		readonly long fStart;

		readonly WaitHandle fCancelEvent;
		readonly ManualResetEvent fFinishedEvent;
		Stream fStream;
		FileStream fFile;
		long fTotalDownloaded;
		bool fCompleted;
		Exception fException;

		#endregion

		#region Constants

		const int downloadBlockSize = 2048;

		#endregion

		protected
#if DEBUG
			virtual
#endif
		Stream DownloadStream
		{
			get
			{
				if (fStart == fSize)
				{
					return Stream.Null;
				}
				if (fStream == null)
				{
					fStream = fResponse.GetResponseStream();
				}
				return fStream;
			}
		}

		#region Private Methods

		void NotifyWhenFinished()
		{
			if (fFinishedEvent != null && !fFinishedEvent.SafeWaitHandle.IsClosed)
			{
				fFinishedEvent.Set();
			}
		}

		bool FinishIfUserCancelled()
		{
			bool result = false;

			if (HasUserCancelled())
			{
				NotifyWhenFinished();
				result = true;
			}

			return result;
		}

		bool HasUserCancelled()
		{
			return (fCancelEvent == null || fCancelEvent.SafeWaitHandle.IsClosed) || fCancelEvent.WaitOne(0, false);
		}

		void SaveToFile(byte[] buffer, int count)
		{
			if (fFile == null)
			{
				fFile = File.Open(fFileName, FileMode.Append, FileAccess.Write);
			}

			fFile.Write(buffer, 0, count);
		}

#if DEBUG
		protected virtual
#endif
		void FinaliseDownload()
		{
			if (fFile != null)
			{
				fFile.Close();
				fFile = null;
			}

			if (Path.GetExtension(this.fFileName) == WebFileDownloader.TempFileExtension)
			{
				string destinationFileName = Path.Combine(Path.GetDirectoryName(fFileName), Path.GetFileNameWithoutExtension((fFileName)));
				File.Move(fFileName, destinationFileName);
			}
		}

		#endregion

		#endregion
	}
}
