using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.FTP
{
	internal class FtpServiceRunner
	{
		public FtpServiceRunner(ILogger serviceLogger)
		{
			this.ServiceLogger = serviceLogger;
		}

		public void DoEverything(CancellationToken token)
		{
			var configurations = FtpRegistry.Instance.Profiles.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			configurations.Sort(FtpProfile.Schema.FriendlyName);
			foreach (FtpProfile profile in configurations)
			{
				token.ThrowIfCancellationRequested();
				RunConfigProfileIfNeeded(profile);
			}
			if (atLeastOneProfileExecuted)
			{
				FtpRegistry.Instance.Profiles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configurations);
			}
		}

		void RunConfigProfileIfNeeded(FtpProfile profile)
		{
			if (profile.LastRunUtc.IsEmpty || (profile.LastRunUtc < ZDateTime.UtcNow.AddSeconds(-1 * profile.RunPeriodSeconds)))
			{
				RunOneProfile(profile);
				profile.LastRunUtc = ZDateTime.UtcNow;
				atLeastOneProfileExecuted = true;
			}
		}

		void RunOneProfile(FtpProfile profile)
		{
			if (CheckLocalValidity(profile))
			{
				try
				{
					switch (profile.PushOrPull)
					{
						#region Explode deliberately for TEST
#if DEBUG
						case "EXC":
							throw new Exception("Exception One", new Exception("Exception Two"));
#endif
						#endregion

						case FtpDirection.Codes.Push:
							Upload(profile);
							break;

						case FtpDirection.Codes.Pull:
							Download(profile);
							break;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var message = "CargoWise FTP Engine. Exception while running profile " + profile.FriendlyName;
					SendWarningEmail(ex, message, profile);
					ServiceLogger.Log(LogType.Error, message + "\r\nProfile details:" + profile.HumanReadableName, ex);
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Warning, "Profile environmental data not valid, ignoring profile " + profile.HumanReadableName);
			}
		}

		#region Remote file operations

		void Download(FtpProfile profile)
		{
			var remoteUri = new Uri(profile.RemoteLocation);
			var useSecure = false;
			if (remoteUri.Scheme == Uri.UriSchemeFtp + "s" || remoteUri.Scheme == Uri.UriSchemeFtp + "es")
			{
				useSecure = true;
			}
			var ftp = new FtpProcessor(remoteUri.Authority, profile.RemoteUsername, profile.RemotePassword, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0), useSecureConnection: useSecure);
			LogInfo("Enumerating files at " + remoteUri.Authority + " path " + remoteUri.PathAndQuery, profile);
			var remoteWaitingFiles = ftp.ListDirectory(remoteUri.PathAndQuery);
			foreach (var remoteFileName in remoteWaitingFiles)
			{
				if (FileNameMatchesMask(remoteFileName, profile))
				{
					LogInfo("Downloading " + remoteFileName, profile);
					var localTargetFileName = remoteFileName;
					var localTargetExists = new FileInfo(Path.Combine(profile.LocalFolder, remoteFileName)).Exists;
					switch (profile.ClobberOrMakeUnique)
					{
						case FtpClobberingOptions.Codes.Clobber:
							break;

						case FtpClobberingOptions.Codes.Fail:
							LogInfo("File already exists locally, ignoring " + remoteFileName, profile);
							return;

						case FtpClobberingOptions.Codes.UniqueCheckKeep:
							if (localTargetExists)
							{
								localTargetFileName = MakeFilenameUniqueKeepButOldFilename(localTargetFileName);
							}
							break;

						case FtpClobberingOptions.Codes.UniqueCheckNew:
							if (localTargetExists)
							{
								localTargetFileName = MakeFilenameUniqueMakeTotallyNew(localTargetFileName);
							}
							break;

						case FtpClobberingOptions.Codes.UniqueNocheckKeep:
							localTargetFileName = MakeFilenameUniqueKeepButOldFilename(localTargetFileName);
							break;

						case FtpClobberingOptions.Codes.UniqueNocheckNew:
							localTargetFileName = MakeFilenameUniqueMakeTotallyNew(localTargetFileName);
							break;
					}
					var fullLocalTargetFileName = Path.Combine(profile.LocalFolder, localTargetFileName);
					var fullRemoteSourceFile = remoteFileName;
					if (remoteUri.PathAndQuery != "/")
					{
						fullRemoteSourceFile = remoteUri.PathAndQuery + (remoteUri.PathAndQuery.EndsWith("/") ? fullRemoteSourceFile : "/" + fullRemoteSourceFile);
					}

					if (fullRemoteSourceFile.StartsWith("/"))
					{
						fullRemoteSourceFile = fullRemoteSourceFile.Remove(0, 1);
					}

					ftp.DownloadFile(fullLocalTargetFileName, fullRemoteSourceFile);
					LogInfo(string.Format("Downloaded '{0}' to {1}", fullRemoteSourceFile, fullLocalTargetFileName), profile);
					MaybeDeleteRemoteSourceOnSucccessfulDownload(fullRemoteSourceFile, ftp, profile);
				}
			}
		}

		void Upload(FtpProfile profile)
		{
			var remoteUri = new Uri(profile.RemoteLocation);
			var useSecure = false;
			if (remoteUri.Scheme == Uri.UriSchemeFtp + "s" || remoteUri.Scheme == Uri.UriSchemeFtp + "es")
			{
				useSecure = true;
			}
			var ftp = new FtpProcessor(remoteUri.Authority, profile.RemoteUsername, profile.RemotePassword, new TimeSpan(0, RawDataRegistry.Instance.FTPReadTimeout.Value, 0), new TimeSpan(0, RawDataRegistry.Instance.FTPConnectionTimeout.Value, 0), useSecureConnection: useSecure);
			var localWaitingFiles = new DirectoryInfo(profile.LocalFolder).GetFiles();

			foreach (var localFileInfo in localWaitingFiles)
			{
				if (FileNameMatchesMask(localFileInfo.Name, profile))
				{
					var remoteTargetFilename = localFileInfo.Name;
					if (remoteUri.PathAndQuery != "/")
					{
						remoteTargetFilename = remoteUri.PathAndQuery + (remoteUri.PathAndQuery.EndsWith("/") ? remoteTargetFilename : "/" + remoteTargetFilename);
					}

					if (remoteTargetFilename.StartsWith("/"))
					{
						remoteTargetFilename = remoteTargetFilename.Remove(0, 1);
					}

					switch (profile.ClobberOrMakeUnique)
					{
						case FtpClobberingOptions.Codes.Clobber:
							UploadToServerWithThisFilename(remoteTargetFilename, ftp, localFileInfo, profile);
							break;

						case FtpClobberingOptions.Codes.Fail:
							if (!FileExistsOnRemoteServer(ftp, remoteUri.PathAndQuery, localFileInfo.Name, profile))
							{
								UploadToServerWithThisFilename(remoteTargetFilename, ftp, localFileInfo, profile);
							}
							break;

						case FtpClobberingOptions.Codes.UniqueCheckKeep:
							if (FileExistsOnRemoteServer(ftp, remoteUri.PathAndQuery, localFileInfo.Name, profile))
							{
								UploadToServerWithThisFilename(MakeFilenameUniqueKeepButOldFilename(remoteTargetFilename), ftp, localFileInfo, profile);
							}
							break;

						case FtpClobberingOptions.Codes.UniqueCheckNew:
							if (FileExistsOnRemoteServer(ftp, remoteUri.PathAndQuery, localFileInfo.Name, profile))
							{
								UploadToServerUnique(localFileInfo, ftp, remoteUri.PathAndQuery, profile);
							}
							break;

						case FtpClobberingOptions.Codes.UniqueNocheckKeep:
							UploadToServerWithThisFilename(MakeFilenameUniqueKeepButOldFilename(remoteTargetFilename), ftp, localFileInfo, profile);
							break;

						case FtpClobberingOptions.Codes.UniqueNocheckNew:
							UploadToServerUnique(localFileInfo, ftp, remoteUri.PathAndQuery, profile);
							break;
					}

					DeleteOrMoveLocalSourceFileOnSuccessfulUpload(localFileInfo, profile);
				}
			}
		}

		void MaybeDeleteRemoteSourceOnSucccessfulDownload(string remoteFileName, FtpProcessor ftp, FtpProfile profile)
		{
			switch (profile.DeleteSourceOption)
			{
				case FtpArchiveOptions.Codes.Nothing:
					break;

				case FtpArchiveOptions.Codes.Delete:
				default:
					ftp.DeleteRemoteFile(remoteFileName);
					LogInfo("Deleted remote file " + remoteFileName, profile);
					break;
			}
		}

		void UploadToServerUnique(FileInfo localFileInfo, FtpProcessor ftp, string remoteDirectory, FtpProfile profile)
		{
			ftp.UploadFileUnique(localFileInfo.FullName, remoteDirectory);
			LogInfo(string.Format("Uploaded {0} to folder {1} uniquely", localFileInfo.Name, remoteDirectory), profile);
		}

		void UploadToServerWithThisFilename(string remoteTargetFilename, FtpProcessor ftp, FileInfo localFileInfo, FtpProfile profile)
		{
			ftp.UploadFileSeveralAttempts(localFileInfo.FullName, remoteTargetFilename, 5, 2);
			LogInfo(string.Format("Uploaded {0} as {1}", localFileInfo.Name, remoteTargetFilename), profile);
		}

		bool FileExistsOnRemoteServer(FtpProcessor ftp, string remotePath, string remoteTargetFilename, FtpProfile profile)
		{
			var upperCandidateFileName = remoteTargetFilename.ToUpper();
			var filesAlreadyOnServer = ftp.ListDirectory(remotePath);
			foreach (var oneRemoteFile in filesAlreadyOnServer)
			{
				if (oneRemoteFile.ToUpper().EndsWith(upperCandidateFileName))
				{
					LogInfo(string.Format("Checked remote path {0} for existing file {1}, found.", remotePath, remoteTargetFilename), profile);
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Local file operations and filename helpers

		void DeleteOrMoveLocalSourceFileOnSuccessfulUpload(FileInfo localFileInfo, FtpProfile profile)
		{
			switch (profile.DeleteSourceOption)
			{
				case FtpArchiveOptions.Codes.Move:
					var archiveDir = new DirectoryInfo(Path.Combine(localFileInfo.DirectoryName, "archive"));
					if (!archiveDir.Exists)
					{
						archiveDir.Create();
					}

					var archiveFileName = Path.Combine(archiveDir.FullName, localFileInfo.Name);
					if (File.Exists(archiveFileName))
					{
						archiveFileName = MakeFilenameUniqueKeepButOldFilename(archiveFileName);
					}

					localFileInfo.MoveTo(archiveFileName);
					LogInfo("Archived local file " + localFileInfo.Name, profile);
					break;

				case FtpArchiveOptions.Codes.Nothing:
					break;

				case FtpArchiveOptions.Codes.Delete:
				default:
					localFileInfo.Delete();
					LogInfo("Deleted local file " + localFileInfo.Name, profile);
					break;
			}
		}

		bool FileNameMatchesMask(string name, FtpProfile profile)
		{
			return profile.FindFileMask.IsEmpty || Regex.IsMatch(name, profile.FindFileMask, RegexOptions.IgnoreCase);
		}

		string MakeFilenameUniqueMakeTotallyNew(string fileName)
		{
			return "Transfer_" + Guid.NewGuid().ToString() + new FileInfo(fileName).Extension;
		}

		string MakeFilenameUniqueKeepButOldFilename(string fileName)
		{
			return fileName + "_" + Guid.NewGuid().ToString() + new FileInfo(fileName).Extension;
		}

		#endregion

		#region Internal stuff

		bool CheckLocalValidity(FtpProfile profile)
		{
			bool valid = false;
			try
			{
				valid = Directory.Exists(profile.LocalFolder);
				try
				{
					var uri = new Uri(profile.RemoteLocation);
					valid = valid && (uri.Scheme == Uri.UriSchemeFtp || uri.Scheme == Uri.UriSchemeFtp + "s" || uri.Scheme == Uri.UriSchemeFtp + "es");
				}
				catch (FormatException ex)
				{
					valid = false;
					ServiceLogger.Log(LogType.Information, "FTP(S) URI invalid", ex);
				}
			}
			catch (IOException ex)
			{
				valid = false;
				ServiceLogger.Log(LogType.Information, "Read/write error checking local folder.", ex);
			}

			return valid;
		}

		void LogInfo(string message, FtpProfile profile)
		{
			ServiceLogger.Log(LogType.Information, message + ". Profile " + profile.FriendlyName);
		}

		void SendWarningEmail(Exception ex, string subject, FtpProfile profile)
		{
			var email = profile.EmailAlertOnFailure;
			if (!email.IsEmpty)
			{
				var emailUtility = new EmailGroupUtility();
				var notificationSender = new HtmlNotificationEmailSender();
				var body = subject;
				while (ex != null)
				{
					body += System.Environment.NewLine + ex.Message;
					ex = ex.InnerException;
				}
				EmailDef notificationEmail = notificationSender.CreateSystemNotificationEmail(subject, body);
				notificationEmail.AddRecipientForUserCommunication(email);
				Env.OutgoingMailManager.CreateAndSave(notificationEmail);
			}
		}

		readonly ILogger ServiceLogger;
		bool atLeastOneProfileExecuted;

		#endregion
	}
}
