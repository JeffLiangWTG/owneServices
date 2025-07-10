using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal class BISIDownloaderTest_CoreFunctionality : TransactionedTestCase
	{
		public void TestDownloadFile()
		{
			AssertEquals("Should return true when there is no error", true, BISIDownloader.DownloadFile("BISI\\PROD\\8691cod\\8691cod.zip"));
			AssertEquals("Should be empty when there is no error", 0, Buffer.Events.Length);
			AssertEquals("should be TempPath + BISITemp\\DownloaderTempFile.txt", Path.Combine(Env.TempPath, "BISITemp\\DownloaderTempFile.txt"), BISIDownloader.DownloadedFileName);
			AssertEquals(ExpectedDownloadLogComplete, BISIDownloader.TestFtpClient.Log);
		}

		public void TestDownloadFile_DeleteRemoteFileWhenFinished()
		{
			AssertEquals("There should be no errors", true, BISIDownloader.DownloadFile("BISI\\PROD\\8691cod\\8691cod.zip", true));
			AssertEquals("should be TempPath + BISITemp\\DownloaderTempFile.txt", Path.Combine(Env.TempPath, "BISITemp\\DownloaderTempFile.txt"), BISIDownloader.DownloadedFileName);
			AssertEquals(ExpectedDownloadLogComplete_RemoteFileDeleted, BISIDownloader.TestFtpClient.Log);
		}

		public void TestExceptionsReportedWhenDownloadingFile()
		{
			BISIDownloader.TestFtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.WhenConnecting;
			AssertEquals("Should be returning false when there is error", false, BISIDownloader.DownloadFile("BISI\\PROD\\8691cod\\8691cod.zip"));
			AssertEquals("", BISIDownloader.DownloadedFileName);
			AssertEquals(ExpectedLogConnect, BISIDownloader.TestFtpClient.Log);
			AssertEquals(1, Buffer.Events.Length);
			AssertEquals("A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond", ((INotificationSubscriberNotification)Buffer.LastOne).AdditionalInfo);
			BISIDownloader.TestFtpClient.ClearLog();
			BISIDownloader.TestFtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.WhenChangingDirectory;
			AssertEquals("Should be returning false when there is error", false, BISIDownloader.DownloadFile("BISI\\PROD\\8691cod\\8691cod.zip"));
			AssertEquals("", BISIDownloader.DownloadedFileName);
			AssertEquals(ExpectedDownloadLogChangeDir, BISIDownloader.TestFtpClient.Log);
			AssertEquals(2, Buffer.Events.Length);
			AssertEquals("Permission denied", ((INotificationSubscriberNotification)Buffer.LastOne).AdditionalInfo);
			BISIDownloader.TestFtpClient.ClearLog();
			BISIDownloader.TestFtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.WhenGettingFile;
			AssertEquals("Should be returning false when there is error", false, BISIDownloader.DownloadFile("BISI\\PROD\\8691cod\\8691cod.zip"));
			AssertEquals("", BISIDownloader.DownloadedFileName);
			AssertEquals(ExpectedDownloadLogGetFile, BISIDownloader.TestFtpClient.Log);
			AssertEquals(3, Buffer.Events.Length);
			AssertEquals("get", ((INotificationSubscriberNotification)Buffer.LastOne).AdditionalInfo);
		}

		public void TestListDirectory()
		{
			var sftpClientMock = new Mock<SftpClientWrapper>(new PasswordConnectionInfo("dummy.com", "dummy", "com"));

			sftpClientMock.Setup(m => m.BaseListDirectory(It.IsAny<string>())).Returns(new List<ISftpFile>());

			AssertNotNull(sftpClientMock.Object.ListDirectory(string.Empty));
		}

		public void TestListZipFilesWithOutOfMemoryException()
		{
			AssertEquals(0, Buffer.Events.Length);
			BISIDownloader.ThrowOutOfMemoryException = true;
			var zipFileList = BISIDownloader.ListZipFiles();
			AssertEquals(1, Buffer.Events.Length);
			AssertEquals(true, Buffer.HasWarnings);
			AssertEquals(true, Buffer.LastOne.Message.StartsWith("Out of memory! Please reduce the amount of files or directories. The number of zip files is"));
		}

		#region ExpectedLogs
		const string ExpectedLogConnect = "";
		const string ExpectedDownloadLogChangeDir = "Connected";
		const string ExpectedDownloadLogGetFile = "Connected\r\n" + "Current working directory is set to BISI\\PROD\\8691cod";
		readonly string ExpectedDownloadLogComplete = "Connected\r\n" + "Current working directory is set to BISI\\PROD\\8691cod\r\n" + "Downloaded 8691cod.zip to " + Env.TempPath + "BISITemp\\DownloaderTempFile.txt";
		readonly string ExpectedDownloadLogComplete_RemoteFileDeleted = "Connected\r\n" + "Current working directory is set to BISI\\PROD\\8691cod\r\n" + "Downloaded 8691cod.zip to " + Env.TempPath + "BISITemp\\DownloaderTempFile.txt\r\n" + "Deleted 8691cod.zip";
		#endregion
		#region Implementation
		BISIDownloaderForTest BISIDownloader
		{
			get
			{
				if (fBISIDownloader == null)
				{
					fBISIDownloader = new BISIDownloaderForTest(Buffer);
				}

				return fBISIDownloader;
			}
		}

		NotificationBufferForTesting Buffer
		{
			get
			{
				if (fBuffer == null)
				{
					fBuffer = new NotificationBufferForTesting();
				}

				return fBuffer;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetRegistryItems();
		}

		protected override void TearDown()
		{
			ClearRegistryItems();
			if (Directory.Exists(Env.TempPath + "BISITemp"))
			{
				Directory.Delete(Env.TempPath + "BISITemp", true);
			}

			if (Directory.Exists(Env.TempPath + "BISIUnzippedFiles"))
			{
				Directory.Delete(Env.TempPath + "BISIUnzippedFiles", true);
			}

			base.TearDown();
		}

		void SetRegistryItems()
		{
			InitialServerAddress = UPEDataRegistry.Instance.BISISftpServerAddress;
			InitialServerPort = UPEDataRegistry.Instance.BISISftpServerPort;
			InitialUsername = UPEDataRegistry.Instance.BISISftpServerUsername;
			InitialPassword = UPEDataRegistry.Instance.BISISftpServerPassword;
			UPEDataRegistry.Instance.BISISftpServerAddress = "BISIServer";
			UPEDataRegistry.Instance.BISISftpServerPort = 12;
			UPEDataRegistry.Instance.BISISftpServerUsername = "Username1";
			UPEDataRegistry.Instance.BISISftpServerPassword = "Password1";
			UPEDataRegistry.Instance.CODFilesParentFolder = "BISI/CODFiles";
		}

		void ClearRegistryItems()
		{
			UPEDataRegistry.Instance.BISISftpServerAddress = InitialServerAddress;
			UPEDataRegistry.Instance.BISISftpServerPort = InitialServerPort;
			UPEDataRegistry.Instance.BISISftpServerUsername = InitialUsername;
			UPEDataRegistry.Instance.BISISftpServerPassword = InitialPassword;
		}

		ZString InitialServerAddress;
		ZInt InitialServerPort;
		ZString InitialUsername;
		ZString InitialPassword;
		BISIDownloaderForTest fBISIDownloader;
		NotificationBufferForTesting fBuffer;
		#region BISIDownloaderForTest
		class BISIDownloaderForTest : BISIDownloader
		{
			public BISIDownloaderForTest(INotifications notifications) : base(notifications)
			{
			}

			protected override ZString GetTempFileName()
			{
				return "DownloaderTempFile.txt";
			}

			protected override ISftpClient GetNewFtpClient()
			{
				return TestFtpClient;
			}

			public FtpClientForTest TestFtpClient
			{
				get
				{
					if (fTestFtpClient == null)
					{
						fTestFtpClient = new FtpClientForTest(new PasswordConnectionInfo(ServerAddress, ServerPort, Username, Password));
					}

					return fTestFtpClient;
				}
			}

			FtpClientForTest fTestFtpClient;
			public bool ThrowOutOfMemoryException { get; set; }

			protected override void ListZipFilesInDirectory(ISftpClient ftpClient, string directoryName, ref List<string> zipFileList)
			{
				if (ThrowOutOfMemoryException)
				{
					throw new OutOfMemoryException();
				}
				else
				{
					base.ListZipFilesInDirectory(ftpClient, directoryName, ref zipFileList);
				}
			}
		}
		#endregion
		#endregion
	}
}
