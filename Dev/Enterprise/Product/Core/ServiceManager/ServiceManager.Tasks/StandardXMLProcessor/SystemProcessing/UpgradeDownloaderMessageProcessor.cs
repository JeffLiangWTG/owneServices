using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.FileDownload;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionInfo;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class UpgradeDownloaderMessageProcessor : IProcessor
	{
		public UpgradeDownloaderMessageProcessor(IWebProxy proxy)
		{
			Proxy = proxy;
		}

		protected INotifications notificationList;

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			this.notificationList = Argument.NotNull(notifications, "notifications");

			if (!UpgradeDownloadExists)
			{
				AddLogRecord(Res.GetString("04b8113f-7301-4319-b755-e9f1d57d6241", "Checking a package to download..."));
			}
			else if (UpgradeDownloadInProcess)
			{
				AddLogRecord(Res.GetString("da1fa60c-3bc6-4198-975e-980c3230cb5e", "Downloading package {0}, downloaded {1} bytes out of {2}. Status: {3}", DownloadingFileName, TotalDownloaded, DownloadingFileSize, DownloadStatusMessage));
			}

			var messageBatch = new MessageBatch(GetEDIMessagePKs());
			ProcessMessageBatch(messageBatch, token);
			Factory.Save();
		}

		internal List<ZGuid> GetEDIMessagePKs()
		{
			return SystemXmlMessageProcessorBase.LoadEDIMessagePKs(Factory,
				new string[] { SystemMessageList.Codes.UpgradeDownload },
				batchSize: 10,
				mostRecentFirst: true);
		}

		public virtual void CancelDownload()
		{
			if (UpgradeDownloader != null)
			{
				UpgradeDownloader.Cancel();
				DisposeDownloader();
			}
		}

		public const int DownloadExpireDays = 2;

		#region Implementation

		#region Process

		internal void ProcessMessageBatch(MessageBatch messageBatch, CancellationToken token)
		{
			bool topDownloadItem = true;
			var downloadExistsBeforeProcessingMessageBatch = UpgradeDownloadExists;

			for (int i = 0; i < messageBatch.MessagePKs.Count; i++)
			{
				token.ThrowIfCancellationRequested();
				var message = Factory.Load<EDIMessage>(messageBatch.MessagePKs[i]);
				var info = SafeExtractInfo(message.EM_MessageText);
				if (info == null)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;
					continue;
				}

				if (!info.PackageURL.IsEmpty)
				{
					if (0 == string.Compare(DownloadingFileName, info.PackageFileName, StringComparison.OrdinalIgnoreCase))
					{
						if (UpgradeDownloadCompleted)
						{
							AddLogRecord(Res.GetString("3b0a043c-7eb6-4a7b-8a1e-0653c3c32152", "Package downloaded, importing package to the database..."));
							message.EM_Status = ImportDownloadedPackage(DownloadedFilePath);
							if (message.EM_Status == EDIMessageStatusList.Codes.ProcessedOK)
							{
								AddLogRecord(Res.GetString("9d4a00bb-3459-429f-bf61-fe03772d4588", "Upgrade {0} was imported and is ready to be applied", DownloadingFileName));
							}

							DisposeDownloader();

							if (File.Exists(DownloadedFilePath))
							{
								File.Delete(DownloadedFilePath);
							}
						}
						else if (!UpgradeDownloadInProcess)
						{
							if (message.EM_SystemCreateTimeUtc < ZDateTime.UtcNow.AddDays(-DownloadExpireDays))
							{
								DisposeDownloader();
								message.EM_Status = EDIMessageStatusList.Codes.Failed;
								AddLogRecord(Res.GetString("f829090e-3dc9-4e69-a26b-bf44c58e276f", "Downloading of the upgrade package {0} was unsuccessful during the last {1} days and is canceled now", DownloadingFileName, DownloadExpireDays));
							}
							else
							{
								AddLogRecord(Res.GetString("352c627f-ed2c-44ab-bdf0-3194436f5877", "Downloading of the {0} failed. Status: {1}", DownloadingFileName, UpgradeDownloader.StatusMessage));
								StartNewDownload(info);
								topDownloadItem = false;
							}
						}
					}
					else
					{
						if (topDownloadItem)
						{
							StartNewDownload(info);
							topDownloadItem = false;
						}
						else
						{
							message.EM_Status = EDIMessageStatusList.Codes.Failed;
						}
					}
				}
				else
				{
					AddLogRecord(Res.GetString("a3ec3fa7-b533-25a8-c5b2-80a3880b9207", "Downloading of the upgrade package is skipped, because {0}", info.Comment));
					message.EM_Status = ImportDownloadedPackage(null, info.VersionInfo);
					if (message.EM_Status == EDIMessageStatusList.Codes.ProcessedOK)
					{
						AddLogRecord(Res.GetString("3c7344f6-6f06-696a-e36c-12e9a6bc4b6d", "Information of upgrade {0} was imported", info.PackageFileName));
					}
				}
			}

			if (!downloadExistsBeforeProcessingMessageBatch && !UpgradeDownloadExists)
			{
				AddLogRecord(Res.GetString("a17e40c5-53cf-4303-bd3e-0da3e95e6d39", "No package to download."));
			}
		}

		PackageDownloadInfo SafeExtractInfo(ZString xml)
		{
			try
			{
				return new PackageDownloadInfo(xml);
			}
			catch (ArgumentException)
			{
				return null;
			}
		}

		#endregion

		#region Upgrade Downloader

		public WebFileDownloader UpgradeDownloader { get; set; }
		IWebProxy Proxy { get; set; }

		#endregion

		#region Protected Properties

		bool UpgradeDownloadExists
		{
			get { return UpgradeDownloader != null; }
		}

		protected virtual bool UpgradeDownloadCompleted
		{
			get { return UpgradeDownloader != null && UpgradeDownloader.HasDownloadCompleted; }
		}

		protected virtual bool UpgradeDownloadInProcess
		{
			get { return UpgradeDownloader != null && !UpgradeDownloader.HasFinished && !UpgradeDownloader.HasDownloadCompleted; }
		}

		protected virtual string DownloadingFileName
		{
			get
			{
				return (UpgradeDownloader != null)
					? new PackageVersionInfo(UpgradeDownloader.FileName).PackageFileName
					: "";
			}
		}

		protected virtual string DownloadedFilePath
		{
			get { return (UpgradeDownloader != null) ? UpgradeDownloader.TargetFilePath : ""; }
		}

		protected virtual long DownloadingFileSize
		{
			get { return (UpgradeDownloader != null) ? UpgradeDownloader.FileSize : -1; }
		}

		protected virtual long TotalDownloaded
		{
			get { return (UpgradeDownloader != null) ? UpgradeDownloader.TotalDownloaded : -1; }
		}

		protected virtual string DownloadStatusMessage
		{
			get { return (UpgradeDownloader != null) ? UpgradeDownloader.StatusMessage : Res.GetString("74bb720d-d11b-4c2f-b0d9-45fd4b94e94d", "Unknown"); }
		}

		#endregion

		#region Protected Methods

		protected void StartNewDownload(PackageDownloadInfo info)
		{
			if (UpgradeDownloader != null)
			{
				string fileName = DownloadingFileName;
				CancelDownload();
				AddLogRecord(Res.GetString("c3a22eaa-24b2-408c-b203-0c073512c5f8", "Download of {0} was canceled", fileName));
			}

			UpgradeDownloader = CreateNewDownloader(info.PackageURL);
			StartDownload();
			AddLogRecord(Res.GetString("9cb35cb1-8dbe-4127-8161-47664409c750", "Downloading package {0}...", DownloadingFileName));
		}

		protected virtual WebFileDownloader CreateNewDownloader(string source)
		{
			return new WebFileDownloader(source) { Proxy = this.Proxy };
		}

		protected virtual void StartDownload()
		{
			if (UpgradeDownloader != null)
			{
				UpgradeDownloader.StartFileDownload(Env.TempPath);
			}
		}

		protected virtual void DisposeDownloader()
		{
			if (UpgradeDownloader != null)
			{
				UpgradeDownloader.FinishedWaitHandle.WaitOne(30000, false);
				UpgradeDownloader.Dispose();
				UpgradeDownloader = null;
			}
		}

		protected virtual string ImportDownloadedPackage(string packagePath, PackageVersionInfo packageVersionInfo = null)
		{
			string result = EDIMessageStatusList.Codes.ProcessedOK;
			StmUpgradeImporter upgradeImporter = GetPackageImporter();
			try
			{
				if (packageVersionInfo == null)
				{
					upgradeImporter.ImportPackage(packagePath);
				}
				else
				{
					upgradeImporter.ImportPackage(null, packageVersionInfo);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				AddLogRecord(Res.GetString("7e026515-3923-4aa2-85cf-f957c33d55cf", "Could not import downloaded package. {0}", ex.ToString()));
				if (ex is CurrentVersionException)
				{
					result = EDIMessageStatusList.Codes.Failed;
				}
				else
				{
					result = EDIMessageStatusList.Codes.Queued;
				}
			}

			return result;
		}

		protected StmUpgradeImporter GetPackageImporter()
		{
			return new StmUpgradeImporter();
		}

		protected void AddLogRecord(ZString newRecord)
		{
			notificationList.Add(new InfoNotification(newRecord));
		}

		#endregion

		#endregion

		#region Factory

		protected BusinessObjectFactory Factory
		{
			get
			{
				factory = factory ?? new BusinessObjectFactory() { RefreshEnabled = false };
				return factory;
			}
		}
		BusinessObjectFactory factory;

		#endregion
	}
}
