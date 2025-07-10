using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.MailManager.FileDownload;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionInfo;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class UpgradeDownloaderMessageProcessorTest : TestCaseWithFactory
	{
		public void TestDoUpgradeDownload()
		{
			var oldItem = GetVersionInfoWithUrlItem(TimeSpan.FromMinutes(-10), 100);
			var item = GetVersionInfoWithUrlItem(TimeSpan.FromMinutes(-1), 111);
			Factory.Save();

			AssertEquals("Precondition: No download in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Precondition: Download queued", EDIMessageStatusList.Codes.Queued, oldItem.EM_Status);
			AssertEquals("Precondition: Download queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.DoUpgradeDownload();
			Assert("Download in progress", UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);
			var logContent = $@"Checking a package to download...
Downloading package {TestFileName}...";
			AssertEquals("Log Records", logContent, UpgradeDownloader.LogRecords);

			item.Reload();
			AssertEquals("Download still queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);
			oldItem.Reload();
			AssertEquals("Old Download failed", EDIMessageStatusList.Codes.Failed, oldItem.EM_Status);

			UpgradeDownloader.DoUpgradeDownload();
			Assert("Download in progress", UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);
			AssertEquals("Log Records", $"Downloading package {TestFileName}, downloaded 80000 bytes out of 100000. Status: Unknown", UpgradeDownloader.LogRecords);

			string oldTestFileName = TestFileName;
			var item1 = GetVersionInfoWithUrlItem(TimeSpan.Zero, 200);
			Factory.Save();

			AssertEquals("Second Download queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.DoUpgradeDownload();
			string expectingLog = "Downloading package " + oldTestFileName + ", downloaded 80000 bytes out of 100000. Status: Unknown\r\n" +
				Res.GetString("c3a22eaa-24b2-408c-b203-0c073512c5f8", "Download of {0} was canceled", oldTestFileName) + "\r\n" +
				$"Downloading package {TestFileName}...";
			Assert("Download in progress", UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);
			AssertEquals("Log Records", expectingLog, UpgradeDownloader.LogRecords);

			item.Reload();
			AssertEquals("First Download failed", EDIMessageStatusList.Codes.Failed, item.EM_Status);
			item1.Reload();
			AssertEquals("Second Download still queued", EDIMessageStatusList.Codes.Queued, item1.EM_Status);

			UpgradeDownloader.DownloadCompleted = true;

			UpgradeDownloader.DoUpgradeDownload();
			expectingLog = $@"Downloading package {TestFileName}, downloaded 80000 bytes out of 100000. Status: Unknown
Package downloaded, importing package to the database...
Upgrade {TestFileName} was imported and is ready to be applied";
			AssertEquals("Download not in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloaded file path", Path.Combine(TestPath, TestFileName), UpgradeDownloader.DownloadedFilePath_Exposed);
			AssertEquals("Log Records", expectingLog, UpgradeDownloader.LogRecords);

			item1.Reload();
			AssertEquals("Second Download processed", EDIMessageStatusList.Codes.ProcessedOK, item1.EM_Status);
		}

		public void TestDoUpgradeDownloadCannotOverwriteCurrentVersion()
		{
			var upgrade = Factory.New<StmUpgrade>();
			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;
			upgrade.SZ_MajorVersion = 1;
			upgrade.SZ_MinorVersion = 2;
			upgrade.SZ_Release = 3;
			upgrade.SZ_Patch = 4;

			GetVersionInfoWithUrlItem(TimeSpan.FromMinutes(-1), 111);
			Factory.Save();

			try
			{
				if (!Directory.Exists(TestPath))
				{
					Directory.CreateDirectory(TestPath);
				}

				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var sourceFile = resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Test.testing.edp");
					var destinationFile = Path.Combine(TestPath, TestFileName);

					File.Copy(sourceFile, destinationFile, true);
					File.SetAttributes(destinationFile, FileAttributes.Normal);

					UpgradeDownloader.DoUpgradeDownload();
					AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);
					var logContent = $@"Checking a package to download...
Downloading package {TestFileName}...";
					AssertEquals("Log Records", logContent, UpgradeDownloader.LogRecords);

					UpgradeDownloader.DownloadCompleted = true;
					UpgradeDownloader.ImportedSuccesfully = false;

					const string expectedError = "Could not import downloaded package. Enterprise.Upgrades.CurrentVersionException: You cannot overwrite the current version upgrade package.";

					UpgradeDownloader.DoUpgradeDownload();
					AssertContains("Log Records", expectedError, UpgradeDownloader.LogRecords);

					File.Copy(sourceFile, destinationFile, true);
					File.SetAttributes(destinationFile, FileAttributes.Normal);

					UpgradeDownloader.DoUpgradeDownload();
					AssertNotContains("Log Records", expectedError, UpgradeDownloader.LogRecords);
				}
			}
			finally
			{
				if (Directory.Exists(TestPath))
				{
					Directory.Delete(TestPath, true);
				}
			}
		}

		[TestDate(2016, 10, 13)]
		public void TestDoUpgradeDownloadExpired()
		{
			var item = GetVersionInfoWithUrlItem(TimeSpan.Zero, 111);
			Factory.Save();

			AssertEquals("Precondition: No download in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Precondition: Download queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.DoUpgradeDownload();
			Assert("Download in progress", UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);
			var logContent = $@"Checking a package to download...
Downloading package {TestFileName}...";
			AssertEquals("Log Records", logContent, UpgradeDownloader.LogRecords);

			item.Reload();
			AssertEquals("Download still queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.OverrideDownloadInProcess = true;
			UpgradeDownloader.DownloadInProcess = false;
			UpgradeDownloader.DoUpgradeDownload();
			AssertEquals("Download not in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);

			string expectingLog = Res.GetString("352c627f-ed2c-44ab-bdf0-3194436f5877", "Downloading of the {0} failed. Status: {1}", TestFileName, "Unknown") + "\r\n" +
				Res.GetString("c3a22eaa-24b2-408c-b203-0c073512c5f8", "Download of {0} was canceled", TestFileName) + "\r\n" +
				$"Downloading package {TestFileName}...";

			AssertEquals("Log Records", expectingLog, UpgradeDownloader.LogRecords);

			TestDateAttribute.Date = item.EM_SystemCreateTimeUtc.AddDays(UpgradeDownloaderMessageProcessor.DownloadExpireDays).AddMinutes(10).ToDateTime();
			Factory.Save();
			UpgradeDownloader.DoUpgradeDownload();
			AssertEquals("Download not in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);
			expectingLog = string.Format("Downloading of the upgrade package {0} was unsuccessful during the last {1} days and is canceled now", TestFileName, UpgradeDownloaderMessageProcessor.DownloadExpireDays);
			AssertEquals("Log Records", expectingLog, UpgradeDownloader.LogRecords);

			item.Reload();
			AssertEquals("Download failed", EDIMessageStatusList.Codes.Failed, item.EM_Status);
		}

		public void TestCancelUpgradeDownload()
		{
			var item = GetVersionInfoWithUrlItem(TimeSpan.FromMinutes(-1), 111);
			Factory.Save();

			AssertEquals("No download in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Download queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.DoUpgradeDownload();
			Assert("Download in progress", UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);
			var logContent = $@"Checking a package to download...
Downloading package {TestFileName}...";
			AssertEquals("Log Records", logContent, UpgradeDownloader.LogRecords);

			item.Reload();
			AssertEquals("Download still queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.CancelDownload();

			AssertNull("No Downloader", UpgradeDownloader.UpgradeDownloader_Exposed);
		}

		public void TestImportOfDownloadedPackageFailed()
		{
			var item = GetVersionInfoWithUrlItem(TimeSpan.FromMinutes(-1), 111);
			Factory.Save();

			AssertEquals("No download in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Download queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.DoUpgradeDownload();
			Assert("Download in progress", UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Downloading file name", TestFileName, UpgradeDownloader.DownloadingFileName_Exposed);
			var logContent = $@"Checking a package to download...
Downloading package {TestFileName}...";
			AssertEquals("Log Records", logContent, UpgradeDownloader.LogRecords);

			item.Reload();
			AssertEquals("Download still queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.OverrideDownloadInProcess = true;
			UpgradeDownloader.DownloadInProcess = false;
			UpgradeDownloader.DownloadCompleted = true;
			UpgradeDownloader.ImportedSuccesfully = false;

			UpgradeDownloader.DoUpgradeDownload();

			AssertEquals("Download not in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertNull("No Downloader", UpgradeDownloader.UpgradeDownloader_Exposed);

			string expectingLog = "Could not import downloaded package.";
			Assert("Log Records contain Could not import", UpgradeDownloader.LogRecords.Contains(expectingLog));
			expectingLog = "Error while unpacking update package";
			AssertContains("Log Records contain stack trace", expectingLog, UpgradeDownloader.LogRecords);

			item.Reload();
			AssertEquals("Download still queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);
		}

		public void TestVersionInfoMessageWithInvalidBodyIsMarkedAsFailed()
		{
			var item = GetVersionInfoWithUrlItem(TimeSpan.Zero, 0);
			item.EM_MessageText = "invalid body";
			Factory.Save();

			AssertEquals("No download in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Download queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			UpgradeDownloader.DoUpgradeDownload();
			item.Reload();

			AssertEquals("No download in progress", false, UpgradeDownloader.UpgradeDownloadInProcess_Exposed);
			AssertEquals("Download Failed", EDIMessageStatusList.Codes.Failed, item.EM_Status);
		}

		[Obsolete("SYSLIB0014: WebRequest, HttpWebRequest, ServicePoint, WebClient are obsolete. Use HttpClient instead.")]
		public void TestUpgradeTimeOut()
		{
			// Arrange
			var port = new Random().Next(49152, 65535);
			var item = GetVersionInfoWithHostUrlItem(TimeSpan.FromMinutes(-1), 111, port);
			Factory.Save();

			var timeOutMilliseconds = 2000;
			WebFileDownloader downloader = null;
			var processMock = new Mock<UpgradeDownloaderMessageProcessor>(null) { CallBase = true };
			processMock
				.Protected()
				.Setup<WebFileDownloader>("CreateNewDownloader", ItExpr.IsAny<string>())
				.Returns((string url) =>
			{
				var webRequest = WebRequest.Create(url);
				webRequest.Timeout = timeOutMilliseconds;
				var downLoaderMock = new Mock<WebFileDownloader>(url) { CallBase = true };
				downLoaderMock.Protected().Setup<WebRequest>("GetRequest").Returns(webRequest);
				downloader = downLoaderMock.Object;
				return downloader;
			});
			var upgradeDownloaderWithTimeOut = processMock.Object;

			using (new DisposableAction(() => upgradeDownloaderWithTimeOut.CancelDownload()))
			{
				using (var socket = CreateTimeOutSocketAndStartListen())
				{
					var notifications = new NotificationBuffer();
					upgradeDownloaderWithTimeOut.Process(notifications);
					notifications.Clear();

					downloader.FinishedWaitHandle.WaitOne();

					// Act
					upgradeDownloaderWithTimeOut.Process(notifications);
					downloader.FinishedWaitHandle.WaitOne();

					// Assert
					var notifiedText = notifications.AsString.TrimEnd();

					var timeoutLogContent = $@"Downloading of the {TestFileName} failed. Status: The operation has timed out.
Download of {TestFileName} was canceled
Downloading package {TestFileName}...";

					var abortLogContent = $@"Downloading of the {TestFileName} failed. Status: Request Aborted
Download of {TestFileName} was canceled
Downloading package {TestFileName}...";

					string[] allowedLogsContent = [timeoutLogContent, abortLogContent];
					AssertCollectionContains("Download should failed and try again", notifiedText, allowedLogsContent);

					item.Reload();
					AssertEquals("Msaages status still queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);
				}
			}

			Socket CreateTimeOutSocketAndStartListen()
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
				socket.Listen((int)SocketOptionName.MaxConnections);
				return socket;
			}
		}

		public void TestSkipDownloadingThenImportUpgradeInfoWhenThePackageURLOfTheEdiMessageIsEmpty()
		{
			// Arrange
			var item = GetVersionInfoWithoutUrlItem(TimeSpan.FromMinutes(-1), 111);
			Factory.Save();

			AssertEquals("Precondition: Download queued", EDIMessageStatusList.Codes.Queued, item.EM_Status);

			// Act
			UpgradeDownloader.DoUpgradeDownload();

			item.Reload();

			// Assert
			var packageDownloadInfo = new PackageDownloadInfo(item.EM_MessageText);
			var logContent = $@"Checking a package to download...
Downloading of the upgrade package is skipped, because {packageDownloadInfo.Comment}
Information of upgrade {packageDownloadInfo.PackageFileName} was imported
No package to download.";
			AssertEquals("Log Records", logContent, UpgradeDownloader.LogRecords);
			AssertEquals("Second Download processed", EDIMessageStatusList.Codes.ProcessedOK, item.EM_Status);
		}

		#region Implementation

		#region TestHelper classes

		sealed class UpgradeDownloaderTestHelper : UpgradeDownloaderMessageProcessor
		{
			public UpgradeDownloaderTestHelper() : base(null) { }

			public void DisposeDownloaderForTest()
			{
				DisposeDownloader();
			}

			NotificationBuffer Notifications;

			public void DoUpgradeDownload()
			{
				Notifications = new NotificationBuffer();
				Process(Notifications);
			}

			protected override void DisposeDownloader()
			{
				if (UpgradeDownloader != null)
				{
					UpgradeDownloader.Dispose();
					UpgradeDownloader = null;
				}
			}

			protected override void StartDownload()
			{
				TestFileName = Path.GetFileName(UpgradeDownloader.Url);
			}

			public override void CancelDownload()
			{
				(UpgradeDownloader.FinishedWaitHandle as ManualResetEvent).Set();
				base.CancelDownload();
			}

			protected override string ImportDownloadedPackage(string packagePath, PackageVersionInfo packageVersionInfo = null)
			{
				if (ImportedSuccesfully)
				{
					return EDIMessageStatusList.Codes.ProcessedOK;
				}
				else
				{
					return base.ImportDownloadedPackage(packagePath);
				}
			}

			protected override string DownloadedFilePath
			{
				get { return Path.Combine(TestPath, DownloadingFileName); }
			}

			internal string DownloadingFileName_Exposed { get { return DownloadingFileName; } }

			protected override string DownloadingFileName
			{
				get { return TestFileName; }
			}

			protected override long DownloadingFileSize
			{
				get { return 100000; }
			}

			protected override long TotalDownloaded
			{
				get { return 80000; }
			}

			protected override bool UpgradeDownloadCompleted
			{
				get { return DownloadCompleted; }
			}

			public bool UpgradeDownloadInProcess_Exposed
			{
				get { return UpgradeDownloadInProcess; }
			}

			protected override bool UpgradeDownloadInProcess
			{
				get { return OverrideDownloadInProcess ? DownloadInProcess : base.UpgradeDownloadInProcess; }
			}

			public WebFileDownloader UpgradeDownloader_Exposed
			{
				get { return UpgradeDownloader; }
			}

			public string DownloadedFilePath_Exposed
			{
				get { return DownloadedFilePath; }
			}

			public string LogRecords
			{
				get
				{
					return Notifications.AsString.TrimEnd();
				}
			}

			public string TestPath;
			public string TestFileName;
			public bool DownloadCompleted;
			public bool OverrideDownloadInProcess;
			public bool DownloadInProcess;
			public bool ImportedSuccesfully;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			UpgradeDownloader = new UpgradeDownloaderTestHelper();
			UpgradeDownloader.TestPath = TestPath;
			UpgradeDownloader.TestFileName = "";

			UpgradeDownloader.ImportedSuccesfully = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			UpgradeDownloader.DisposeDownloaderForTest();
		}

		EDIMessage GetVersionInfoWithoutUrlItem(TimeSpan timeSpan, int patch)
		{
			return GetEDIMessage(ZDateTime.Now.Add(timeSpan), patch);
		}

		EDIMessage GetEDIMessage(ZDateTime zDateTime, int patch, string packageURL = null)
		{
			var xml =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<UpgradeDownload>" + System.Environment.NewLine +
				"  <ExeVersionDate>" + zDateTime.ToLongTimeString().ToUpper() + "</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>" + patch + "</Patch>" + System.Environment.NewLine +
				"  <Comment>Comment</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				(packageURL == null ? "  <PackageURL />" : $"  <PackageURL>{packageURL}</PackageURL>") + System.Environment.NewLine +
				"</UpgradeDownload>";

			var interchange = SystemMessage.CreateInterchange(Factory, xml);
			var result = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			result.EM_SystemCreateTimeUtc = zDateTime;
			result.EM_MessageSubType = SystemMessageList.Codes.UpgradeDownload;
			return result;
		}

		EDIMessage GetEDIMessageWithURL(ZDateTime zDateTime, int patch, string rootPath = null)
		{
			if (!rootPath.EndsWith("/"))
			{
				throw new ArgumentException($"{nameof(rootPath)} must end with '/'");
			}

			var versionInfo = new PackageVersionInfo(zDateTime, 1, 2, 3, patch);
			TestFileName = versionInfo.PackageFileName;

			return GetEDIMessage(zDateTime, patch, rootPath + TestFileName);
		}

		EDIMessage GetVersionInfoWithUrlItem(TimeSpan timeSpan, int patch)
		{
			return GetEDIMessageWithURL(ZDateTime.Now.Add(timeSpan), patch, "http://www.cargowise.com/ftpmirror/");
		}

		EDIMessage GetVersionInfoWithHostUrlItem(TimeSpan timeSpan, int patch, int port)
		{
			return GetEDIMessageWithURL(ZDateTime.Now.Add(timeSpan), patch, $"http://localhost:{port}/ftpmirror/");
		}

		string TestPath
		{
			get { return testPath ?? (testPath = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString())); }
		}

		string testPath;
		string TestFileName;
		UpgradeDownloaderTestHelper UpgradeDownloader;

		#endregion

	}
}
