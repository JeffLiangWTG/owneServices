using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.PrintProcessing
{
	class FtpJobProcessor : MergedPrintGroupProcessor
	{
		public FtpJobProcessor(StmPrintJobMergedCollection mergedPrintGroup, PrintJobManager.ProgressDelegate logProgress, PrintJobManager.NotificationDelegate runNotification, PrintJobManager jobManager = null)
			: base(mergedPrintGroup, jobManager)
		{
			this.logProgress = logProgress;
			this.runNotification = runNotification;
		}

		readonly PrintJobManager.ProgressDelegate logProgress;

#if DEBUG
		internal
#endif
		readonly PrintJobManager.NotificationDelegate runNotification;

		const string ftpUrlStarter = "ftp://";
		const string sftpUrlStarter = "sftp://";

		string FixUrlCasing(string url)
		{
			if (url.StartsWith(ftpUrlStarter, StringComparison.OrdinalIgnoreCase))
			{
				return ftpUrlStarter + url.Substring(ftpUrlStarter.Length);
			}
			else if (url.StartsWith(sftpUrlStarter, StringComparison.OrdinalIgnoreCase))
			{
				return sftpUrlStarter + url.Substring(sftpUrlStarter.Length);
			}
			else
			{
				return url;
			}
		}

		void ExtractUsernameAndPassword(string s, out string user, out string password)
		{
			user = password = string.Empty;

			int splitterIndex = s.IndexOf("\0", StringComparison.Ordinal);
			if (splitterIndex >= 0)
			{
				user = s.Substring(0, splitterIndex);
				password = s.Substring(splitterIndex + 1);
			}
		}

		bool TryUploadFile(IFtpProcessor ftpProcessor, StmPrintJob printJob, string remoteFileName)
		{
			try
			{
				logProgress(TraceEventType.Information, Res.GetString("03ce47d5-5130-4614-8755-011eb111e3ce", "Uploading file '{0}'", remoteFileName));
				ftpProcessor.UploadFileSeveralAttempts(printJob.StoredAttachmentFilename, remoteFileName, 2, 1);

#if DEBUG
				IsPartialFileCreated = true;
#endif
				return true;
			}
			catch (FtpException ex)
			{
				notifyMessage = ex.ToString();

				logProgress(TraceEventType.Warning,
					Res.GetString("988e76fa-f107-407a-9048-dd59fa05fc0c",
						"Could not upload file '{0}' to FTP location {1}. Please check that location, user name, password are correct, and specified user has security rights to upload files to the FTP.",
						remoteFileName, ftpProcessor.ServerName) +
					"\r\n\r\n" + ex);

				return false;
			}
		}

		string GetNameWithoutCollision(IFtpProcessor ftpProcessor, string remoteFile)
		{
			var existingNames = new HashSet<string>(ftpProcessor.ListDirectory(string.Empty));
			var dotIndex = remoteFile.LastIndexOf('.');

			var nameToUse = remoteFile;
			for (var j = 1; existingNames.Contains(nameToUse); j++)
			{
				nameToUse = remoteFile.Insert(dotIndex, " (" + j + ")");
			}

			return nameToUse;
		}

		void RenameFileAndRemoveOriginal(IFtpProcessor ftpProcessor, string oldName, string newName)
		{
			try
			{
				logProgress(TraceEventType.Information, Res.GetString("ebac2b70-b4ab-4624-8397-169824c23066", "Renaming file '{0}' to '{1}'", oldName, newName));
				ThrowIfSupposedToWhenRenamingFile();

				ftpProcessor.RenameRemoteFileSeveralAttempts(oldName, newName, 2, 1);

				uploadSuccessfully = true;
#if DEBUG
				IsRemoteFileRenamed = true;
#endif
			}
			catch (FtpException ex)
			{
				notifyMessage = ex.ToString();

				logProgress(TraceEventType.Warning,
					Res.GetString("df6a6ae7-2841-446c-9cc5-20c6ebf8c03d",
						"Could not rename file '{0}' to '{1}' at FTP location {2}. Please check that specified user has security rights to rename files on the FTP, and there is no other file with same name. Partial file will be deleted.",
						oldName, newName, ftpProcessor.ServerName) +
					"\r\n\r\n" + ex);

				if (!TryRenameWithCollisionCheck(ftpProcessor, oldName, newName))
				{
					DeleteFile(ftpProcessor, oldName);
				}
			}
		}

		[Conditional("DEBUG")]
		void ThrowIfSupposedToWhenRenamingFile()
		{
#if DEBUG
			if (ThrowExceptionWhenRenamingRemoteFile)
			{
				throw new FtpException(FtpException.FtpExceptionType.RenameFile, "Could not rename file after 2 tries.", new WebException("Internal exception message"));
			}
#endif
		}

		bool TryRenameWithCollisionCheck(IFtpProcessor ftpProcessor, string oldName, string newName)
		{
			var alternateName = GetNameWithoutCollision(ftpProcessor, newName);
			if (alternateName != newName)
			{
				try
				{
					logProgress(TraceEventType.Warning, Res.GetString("106f26c5-e0ff-41af-bf1b-f66fdb4323e9", "File already existed. Renaming file '{0}' to '{1}' instead", oldName, newName));
					ThrowIfSupposedToWhenRenamingFile();

					ftpProcessor.RenameRemoteFileSeveralAttempts(oldName, alternateName, 2, 1);
					return true;
				}
				catch (FtpException ex)
				{
					notifyMessage = ex.ToString();

					logProgress(TraceEventType.Warning,
					Res.GetString("106d1c7d-42b5-46bc-b3cf-7177b6b1cba3",
						"Could not rename file '{0}' to '{1}' at FTP location {2}. Please check that specified user has security rights to rename files on the FTP, and there is no other file with same name. ",
						oldName, newName, ftpProcessor.ServerName) +
					"\r\n\r\n" + ex);
				}
			}

			return false;
		}

		void DeleteFile(IFtpProcessor ftpProcessor, string fileName)
		{
			try
			{
				ftpProcessor.DeleteRemoteFile(fileName);

#if DEBUG
				IsRemoteFileDeleted = true;
#endif
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifyMessage = ex.ToString();

				logProgress(TraceEventType.Warning,
					Res.GetString("b606df7d-1521-4032-aacd-adb1eaeb4161",
						"Could not delete partial file '{0}' from FTP location {1}. Please check that specified user has security rights to delete files on the FTP.",
						fileName, ftpProcessor.ServerName) +
					"\r\n\r\n" + ex);
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")] // "need to make sure that 'ftp' is always in lower case"
		protected override void ProcessIndividualItemCore(StmPrintJob printJob)
		{
#if DEBUG
			IsPartialFileCreated = IsRemoteFileRenamed = IsRemoteFileDeleted = false;
			if (Globals.IsTest && !UploadToFtpInUnitTests)
			{
				if (printJob.SP_Destination == "TestRunTask_WithNotificationFailure")
				{
					logProgress(TraceEventType.Error, "invalid ftp server");
					runNotification(false, "invalid ftp server");
				}
				else if (printJob.SP_Destination == "TestRunTask_WithNotificationSuccess")
				{
					runNotification(true, "Upload successfully");
				}
				return;
			}
#endif
			var url = FixUrlCasing(printJob.SP_FaxDestination);

			var fileName = Path.GetFileName(MakeFilenameSafe.MakeSafe(printJob.SP_DocumentName, '_') + " (" + ZDateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss") + ")." + printJob.SP_EmailAttachmentFormat);
			var partialFileName = fileName + ".partial";

			string user, password;
			ExtractUsernameAndPassword(printJob.SP_EmailFromAddress, out user, out password);

			var ftpProcessor = GetProcessor(url, user, password);

			if (TryUploadFile(ftpProcessor, printJob, partialFileName))
			{
				RenameFileAndRemoveOriginal(ftpProcessor, partialFileName, fileName);
			}

			runNotification?.Invoke(uploadSuccessfully, notifyMessage);
		}

		bool uploadSuccessfully;

		string notifyMessage;

		protected virtual IFtpProcessor GetProcessor(string url, string user, string password)
		{
			var processor = CreateProcessor(url, user, password);
			if (processor is SftpProcessor sFtpProcessor)
			{
				sFtpProcessor.Connect();
				return sFtpProcessor;
			}
			return processor;
		}

		public IFtpProcessor CreateProcessor(string url, string user, string password)
		{
			if (url.StartsWith(sftpUrlStarter))
			{
				url = url.Substring(sftpUrlStarter.Length);
				return new SftpProcessor(url, user, password, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
			}
			return new FtpProcessor(url, user, password, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0));
		}

#if DEBUG
		internal bool UploadToFtpInUnitTests { get; set; }
		internal bool IsPartialFileCreated { get; private set; }
		internal bool IsRemoteFileRenamed { get; private set; }
		internal bool IsRemoteFileDeleted { get; private set; }
		internal bool ThrowExceptionWhenRenamingRemoteFile { get; set; }
#endif
	}
}
