using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.Business
{
	public abstract class FtpEngineWithTrigger
	{
		protected FtpEngineWithTrigger(ILogger serviceLogger, IProviderOfTriggerFtpOptions optionsProvider)
		{
			this.optionsProvider = optionsProvider;
			this.serviceLogger = serviceLogger;
		}

		public void DoEverything()
		{
			if (Initialise())
			{
				PushLocalFilesAndPullRemoteFiles();
			}
		}

		void PushLocalFilesAndPullRemoteFiles()
		{
			PushLocalFilesToTarget();
			PullRemoteFilesFromSource();
		}

		[SuppressMessage("Microsoft.Contracts", "Requires-13-67")]
		protected void PullRemoteFilesFromSource()
		{
			Log("Enumerating remote files at " + uriString, LogType.Debug);
			string[] allRemoteFilesWating = null;
			TryFtpActionFiveTimesOrFail(delegate
			{ allRemoteFilesWating = FTPProcessor.ListDirectory(remoteDirectory); });
			var allRemoteFilesWaitingIncludingIrrelevantOnesCount = allRemoteFilesWating.Length;
			Log(allRemoteFilesWaitingIncludingIrrelevantOnesCount + " remote objects (files and directories)", LogType.Debug);

			if (allRemoteFilesWaitingIncludingIrrelevantOnesCount > 200)
			{
				Log(TooManyRemoteFilesWarningText, LogType.Warning);
			}

			if (allRemoteFilesWaitingIncludingIrrelevantOnesCount > 0)
			{
				var fileSetsWating = new Dictionary<ZString, string[]>();
				var responseExtensionsToSeek = TriggerFileExtensionsIncludingDots_Pull;
				var triggerFileNames = (from string f in allRemoteFilesWating where responseExtensionsToSeek.Contains(Path.GetExtension(f)) select f);
				foreach (var triggerFileName in triggerFileNames.ToArray())
				{
					var filesInFileset = (from string f in allRemoteFilesWating where Path.GetFileNameWithoutExtension(f) == Path.GetFileNameWithoutExtension(triggerFileName) && f != triggerFileName select f);
					fileSetsWating.Add(triggerFileName, filesInFileset.ToArray());
				}

				Log(fileSetsWating.Count + " filesets to pull", LogType.Debug);

				foreach (var triggerName in fileSetsWating.Keys)
				{
					Log("Downloading for fileset " + triggerName, LogType.Information);
					foreach (var fileNotTrigger in fileSetsWating[triggerName])
					{
						long totalBytes = -1;
						Log("...... " + fileNotTrigger, LogType.Information);
						TryFtpActionFiveTimesOrFail(delegate
						{ totalBytes = FTPProcessor.DownloadFile(Path.Combine(localEnterpriseSharedFolder.FullName, fileNotTrigger), Path.Combine(remoteDirectory, fileNotTrigger)); });
						Log($"...... received {totalBytes} bytes for {fileNotTrigger}", LogType.Information);
					}
					Log("...... trigger " + triggerName, LogType.Information);
					TryFtpActionFiveTimesOrFail(delegate
					{ FTPProcessor.DownloadFile(Path.Combine(localEnterpriseSharedFolder.FullName, triggerName), Path.Combine(remoteDirectory, triggerName)); });
					foreach (var fileNotTrigger in fileSetsWating[triggerName])
					{
						Log("...... deleting " + fileNotTrigger, LogType.Information);
						TryFtpActionFiveTimesOrFail(delegate
						{ FTPProcessor.DeleteRemoteFile(Path.Combine(remoteDirectory, fileNotTrigger)); });
					}
					Log("...... deleting trigger " + triggerName, LogType.Information);
					TryFtpActionFiveTimesOrFail(delegate
					{ FTPProcessor.DeleteRemoteFile(Path.Combine(remoteDirectory, triggerName)); });
				}
			}
		}

		[SuppressMessage("Microsoft.Contracts", "Nonnull-6-0")]
		void TryFtpActionFiveTimesOrFail(Action unsafeAction)
		{
			var tryCount = 0;
			while (tryCount < 5)
			{
				try
				{
					unsafeAction();
					break;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					tryCount++;
					if (tryCount >= 5)
					{
						throw new HostedServiceException("Tried 5 times to perform FTP operation at " + uriString + ", suffered exception (follows).  Remote server is possibly down. Check it and your network connectivity. ", ex);
					}
					serviceLogger.Log(LogType.Warning, "Suffered exception while performing FTP operation.  Sleeping. If it happens " + (5 - tryCount) + " more times the attempt will be aborted and a new exception will be thrown and handled as usual.", ex);
					Thread.Sleep(sleepTimeInSeconds * 1000);
				}
			}
		}

		[SuppressMessage("Microsoft.Contracts", "MissingPrecondition-1-0")]
		[SuppressMessage("Microsoft.Contracts", "Nonnull-14-0")]
		[SuppressMessage("Microsoft.Contracts", "Nonnull-52-0")]
		[SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected void PushLocalFilesToTarget()
		{
			FileInfo[] localTriggerFiles = null;
			Log("Enumerating local files in " + localEnterpriseSharedFolder.FullName, LogType.Debug);
			try
			{
				localTriggerFiles = localEnterpriseSharedFolder.GetFiles("*" + TriggerFileExtensionIncludingDots_Push);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				serviceLogger.Log(LogType.Error, string.Format("Could not enumerate local files to upload. This is not a {1} error.  Check existence of and permissions on folder {0}", localEnterpriseSharedFolder.FullName, BrandingFactory.Instance.ProductName), ex);
			}

			foreach (var localTriggerFile in localTriggerFiles)
			{
				TryFtpActionFiveTimesOrFail(delegate
				{ UploadEntireFileSet(localTriggerFile); });
				DeleteFileset(localTriggerFile);
			}
		}

		[SuppressMessage("Microsoft.Contracts", "Nonnull-7-0")]
		[SuppressMessage("Microsoft.Contracts", "Nonnull-31-0")]
		[SuppressMessage("Microsoft.Contracts", "Nonnull-26-0")]
		[SuppressMessage("Microsoft.Contracts", "Nonnull-72-0")]
		void DeleteFileset(FileInfo localTriggerFile)
		{
			Log("Deleting local files for set " + localTriggerFile.FullName, LogType.Information);
			DeleteFileSetCore(localTriggerFile, localEnterpriseSharedFolder.FullName);
		}

		protected virtual void DeleteFileSetCore(FileInfo localTriggerFile, string fullName)
		{
		}

		void UploadEntireFileSet(FileInfo localTriggerFile)
		{
			var fileSetNumber = Path.GetFileNameWithoutExtension(localTriggerFile.Name);
			Log("Uploading fileset " + fileSetNumber, LogType.Information);
			var allFilesInFileset = localEnterpriseSharedFolder.GetFiles(fileSetNumber + ".*");
			foreach (var oneFile in allFilesInFileset)
			{
				if (oneFile.FullName != localTriggerFile.FullName)
				{
					Log("...... file " + oneFile.Name, LogType.Information);
					TryFtpActionFiveTimesOrFail(delegate
					{ FTPProcessor.UploadFile(oneFile.FullName, Path.Combine(remoteDirectory, oneFile.Name)); });
				}
			}
			Log("...... trigger file " + localTriggerFile.Name, LogType.Information);
			TryFtpActionFiveTimesOrFail(delegate
			{ FTPProcessor.UploadFile(localTriggerFile.FullName, Path.Combine(remoteDirectory, localTriggerFile.Name)); });
			Log("Done uploading", LogType.Information);
		}

		[SuppressMessage("Microsoft.Contracts", "Nonnull-7-0")]
		protected virtual bool Initialise()
		{
			ftpReadTimeout = optionsProvider.FtpReadTimeout;
			ftpConnectionTimeout = optionsProvider.FtpConnectionTimeout;
			sleepTimeInSeconds = optionsProvider.SleepTimeInSeconds;
			verboseLogging = optionsProvider.VerboseLogging;
			uriString = optionsProvider.UriString;
			username = optionsProvider.Username;
			password = optionsProvider.Password;
			ZString localEnterpriseSharedFolderName = optionsProvider.LocalEnterpriseSharedFolderName;

			if (uriString.IsEmpty || username.IsEmpty || password.IsEmpty || localEnterpriseSharedFolderName.IsEmpty)
			{
				Log(CannotInitialiseOptionsAreInsufficientMessage, LogType.Error);
				return false;
			}
			try
			{
				uri = new Uri(uriString);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log("The URI given in the options is not legal.  Ensure a correct FTP(S) URI is given." + uriString + " " + ex.Message, LogType.Error);
				return false;
			}
			if (uri.Scheme != Uri.UriSchemeFtp && uri.Scheme != Uri.UriSchemeFtp + "s" && uri.Scheme != Uri.UriSchemeFtp + "es")
			{
				Log("The URI given in the registry is not an FTP(S) URI.  Ensure a correct FTP(S) URI is given." + uriString, LogType.Error);
				return false;
			}
			localEnterpriseSharedFolder = new DirectoryInfo(localEnterpriseSharedFolderName);
			if (!localEnterpriseSharedFolder.Exists)
			{
				Log("The shared folder does not exist.  This process should not be run without it." + localEnterpriseSharedFolder, LogType.Error);
				return false;
			}
			remoteDirectory = uri.PathAndQuery;
			if (remoteDirectory.IsEmpty)
			{
				remoteDirectory = "/";
			}
			return true;
		}

		protected virtual string CannotInitialiseOptionsAreInsufficientMessage => "";

		IFtpProcessor FTPProcessor
		{
			get { return ftpProcessor ?? (ftpProcessor = CreateFTPProcessor()); }
		}
		IFtpProcessor ftpProcessor;

		protected virtual IFtpProcessor CreateFTPProcessor()
		{
			return new FtpProcessor(uri.Scheme + "://" + uri.Authority, username, password, ThrowHostedServiceExceptionOnDeleteError, new TimeSpan(0, ftpReadTimeout, 0), new TimeSpan(0, ftpConnectionTimeout, 0));
		}

		void Log(string p, LogType logLevel)
		{
			if (verboseLogging || logLevel <= LogType.Information)
			{
				serviceLogger.Log(LogType.Information, p);
			}
		}

		void ThrowHostedServiceExceptionOnDeleteError(string p)
		{
			throw new HostedServiceException("Error in FTP operation. " + p);
		}

		protected virtual int MaximumNumberOfRemoteFilesBeforeWarning => 200;
		protected virtual string TooManyRemoteFilesWarningText => "There are over " + MaximumNumberOfRemoteFilesBeforeWarning + " objects on the remote server.";

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public abstract string[] TriggerFileExtensionsIncludingDots_Pull { get; }
		public abstract string TriggerFileExtensionIncludingDots_Push { get; }

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int sleepTimeInSeconds;
		bool verboseLogging;
		Uri uri;
		ZString remoteDirectory;
		protected DirectoryInfo localEnterpriseSharedFolder;
		ZString uriString;
		ZString username;
		ZString password;
		protected ILogger serviceLogger;
		int ftpReadTimeout;
		int ftpConnectionTimeout;
		readonly IProviderOfTriggerFtpOptions optionsProvider;
	}

	[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
	public interface IProviderOfTriggerFtpOptions
	{
		bool VerboseLogging { get; }
		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int SleepTimeInSeconds { get; }
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		string UriString { get; }
		string Username { get; }
		string Password { get; }
		string LocalEnterpriseSharedFolderName { get; }
		int FtpReadTimeout { get; }
		int FtpConnectionTimeout { get; }
	}
}
