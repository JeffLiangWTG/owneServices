using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal class FtpUploaderTest_ForCoreFunctionality : TestCase
	{
		#region TestCanUpload
		public void TestCanUpload()
		{
			FtpUploaderForTest uploader = new FtpUploaderForTest(Notifications);
			uploader.GetFileSizeHandler += new FtpUploaderForTest.FileSizeEventHandler(GetFileSize);
			uploader.TestFtpClient.CheckExistsHandler += new FtpClientForTest.CheckExistsEventHandler(CheckFileExists);
			AssertEquals("Pre-condition", 0, Notifications.Events.Length);
			uploader.TestCanOverwriteTargetFile = false;
			AssertEquals(false, uploader.CanUpload());
			AssertLogEquals(ExpectedLogGetTargetFileSize, uploader.TestFtpClient);
			AssertEquals("There should be a warning", 1, Notifications.Events.Length);
			AssertEquals("Target file already exists and cannot be deleted", ((INotificationSubscriberNotification)Notifications.Events[0]).AdditionalInfo);
			Notifications.Clear();
			uploader.TestCanOverwriteTargetFile = true;
			AssertEquals(true, uploader.CanUpload());
			AssertEquals("There should be no warnings", 0, Notifications.Events.Length);
		}

		const string ExpectedLogGetTargetFileSize = "Connected\r\n" + "Current working directory is set to upld\r\n" + "File size of fln is 200";
		public void TestCanUpload_ErrorOnConnect()
		{
			FtpUploaderForTest uploader = new FtpUploaderForTest(Notifications);
			AssertEquals("Pre-condition", 0, Notifications.Events.Length);
			uploader.TestCanOverwriteTargetFile = true;
			AssertEquals(true, uploader.CanUpload());
			AssertEquals("There should be no error", 0, Notifications.Events.Length);
			uploader.TestFtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.WhenConnecting;
			uploader.TestCanOverwriteTargetFile = false;
			AssertEquals(false, uploader.CanUpload());
			Assert(Notifications.Events.ContainsNotificationContaining("Error: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond"));
		}

		#endregion
		public void TestUploadToFtpServerAndArchive_UsingTextReader()
		{
			StringReader reader = new StringReader("TheFtpUploadedFileContent");
			bool uploadSuccess = FtpUploader.UploadToFtpServerAndArchive(reader);
			AssertEquals("There should be no errors", true, uploadSuccess);
			AssertEquals("There should be no errors/warnings", 0, Notifications.Events.Length);
		}

		public void TestUploadToFtpServerAndArchive_UsingAFilename()
		{
			bool uploadSuccess = FtpUploader.UploadToFtpServerAndArchive(TestFileToUpload);
			AssertEquals("There should be no errors", true, uploadSuccess);
			AssertLogEquals(ExpectedLogComplete, FtpUploader.TestFtpClient);
			AssertEquals("There should be no errors/warnings", 0, Notifications.Events.Length);
		}

		public void TestUploadToFtpServerAndArchive_WhenTargetFileCannotBeOverwritten()
		{
			bool uploadSuccess = FtpUploader.UploadToFtpServerAndArchive(TestFileToUpload);
			AssertEquals("1st upload. Should be able to upload file", true, uploadSuccess);
			FtpUploader.TestFtpClient.ClearLog();
			using (File.Create(TestFileToUpload))
			{
			}

			uploadSuccess = FtpUploader.UploadToFtpServerAndArchive(TestFileToUpload);
			AssertEquals("Should not be able to upload file", false, uploadSuccess);
			AssertLogEquals(ExpectedLogCannotOverwriteFile, FtpUploader.TestFtpClient);
			AssertEquals("There should be a warning notification", true, Notifications.HasWarnings);
			AssertEquals("Target file already exists and cannot be deleted", ((WarningNotification)Notifications.LastOne).AdditionalInfo);
		}

		public void TestUploadToFtpServerAndArchive_WhenTargetFileCanBeOverwritten()
		{
			FtpUploader.TestCanOverwriteTargetFile = true;
			bool uploadSuccess = FtpUploader.UploadToFtpServerAndArchive(TestFileToUpload);
			AssertEquals("Should be able to upload file", true, uploadSuccess);
			AssertLogEquals(ExpectedLogComplete, FtpUploader.TestFtpClient);
			AssertEquals("There should be no errors/warnings", 0, Notifications.Events.Length);
		}

		public void TestUploadToFtpServerAndArchive_ArchivesTheLocalFile()
		{
			string tempFile = Env.GetTempFileName();
			bool uploadSuccess = FtpUploader.UploadToFtpServerAndArchive(tempFile);
			AssertEquals("Should be able to upload file", true, uploadSuccess);
			AssertEquals("There should be no errors/warnings", 0, Notifications.Events.Length);
			AssertEquals("Local file should be deleted", false, File.Exists(FtpUploader.UploadFilename));
			AssertEquals("Local file should be archived", 1, Directory.GetFiles(FtpUploader.ArchiveDirectory).Length);
		}

		public void TestUploadToFtpServerAndArchive_DoesntArchiveOnFail()
		{
			string tempFile = Env.GetTempFileName();
			FtpUploader.TestFtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.Always;
			bool uploadSuccess = FtpUploader.UploadToFtpServerAndArchive(tempFile);
			AssertEquals("Upload should have failed", false, uploadSuccess);
			AssertEquals("Local file should be deleted", false, File.Exists(FtpUploader.UploadFilename));
			AssertEquals("Local file should not be archived", 0, Directory.GetFiles(FtpUploader.ArchiveDirectory).Length);
		}

		public void TestWarnTargetFileCannotBeOverwritten()
		{
			AssertEquals("Pre-condition", 0, Notifications.Events.Length);
			FtpUploader.WarnTargetFileCannotBeOverwritten();
			AssertEquals(1, Notifications.Events.Length);
			AssertEquals("Target file already exists and cannot be deleted", ((INotificationSubscriberNotification)Notifications.LastOne).AdditionalInfo);
		}

		public void TestExceptionsReportedWhenUploadingToFtpServer_NoFileSpecified()
		{
			bool uploadSuccess = FtpUploader.UploadToFtpServerAndArchive(ZString.Empty);
			AssertEquals("Should be returning to false when there is error", false, uploadSuccess);
			AssertEquals(1, Notifications.Events.Length);
			AssertEquals("File name to be uploaded has to be specified.", ((INotificationSubscriberNotification)Notifications.LastOne).AdditionalInfo);
		}

		public void TestExceptionsReportedWhenUploadingToFtpServer_ErrorOnConnect()
		{
			AssertExceptionThrown(FtpClientForTest.ExceptionToBeThrown.WhenConnecting, ExpectedLogConnect, "A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond");
		}

		public void TestExceptionsReportedWhenUploadingToFtpServer_ErrorOnDirectoryChange()
		{
			AssertExceptionThrown(FtpClientForTest.ExceptionToBeThrown.WhenChangingDirectory, ExpectedLogChangeDir, "Permission denied");
		}

		public void TestExceptionsReportedWhenUploadingToFtpServer_ErrorOnFileExistenceCheck()
		{
			AssertExceptionThrown(FtpClientForTest.ExceptionToBeThrown.WhenCheckingIfExists, ExpectedLogCheckFileExists, "checkexists");
		}

		public void TestExceptionsReportedWhenUploadingToFtpServer_ErrorOnPutFile()
		{
			AssertExceptionThrown(FtpClientForTest.ExceptionToBeThrown.WhenPuttingFile, ExpectedLogPutFile, "put");
		}

		public void TestExceptionsReportedWhenUploadingToFtpServer_ErrorOnRename()
		{
			AssertExceptionThrown(FtpClientForTest.ExceptionToBeThrown.WhenRenaming, ExpectedLogRename, "rename");
		}

		public void TestDefaultCanOverwriteTargetFile()
		{
			AssertEquals("Default should be false", false, FtpUploader.BaseCanOverwriteTargetFile(0));
		}

		#region TestGetTargetFileSize
		public void TestGetTargetFileSize_FileDoesNotExist()
		{
			AssertEquals(0, FtpUploader.GetTargetFileSize());
			AssertLogEquals(ExpectedLogGetTargetFileSize_FileDoesNotExist, FtpUploader.TestFtpClient);
			AssertEquals("There should be no errors/warnings", 0, Notifications.Events.Length);
		}

		public void TestGetTargetFileSize_FileDoesNotExist_SimulateRaceCondition()
		{
			FtpUploader.GetFileSizeHandler += new FtpUploaderForTest.FileSizeEventHandler(GetFileSizeThrowException);
			AssertEquals(0, FtpUploader.GetTargetFileSize());
			AssertLogEquals(ExpectedLogGetTargetFileSize_FileDoesNotExist, FtpUploader.TestFtpClient);
			AssertEquals("There should be no errors/warnings", 0, Notifications.Events.Length);
		}

		public void TestGetTargetFileSize_FileExists()
		{
			FtpUploader.TestFtpClient.CheckExistsHandler += new FtpClientForTest.CheckExistsEventHandler(CheckFileExists);
			FtpUploader.GetFileSizeHandler += new FtpUploaderForTest.FileSizeEventHandler(GetFileSize);
			AssertEquals(200, FtpUploader.GetTargetFileSize());
			AssertLogEquals(ExpectedLogGetTargetFileSize_FileExists, FtpUploader.TestFtpClient);
			AssertEquals("There should be no errors/warnings", 0, Notifications.Events.Length);
		}

		long GetFileSize(string fileName)
		{
			return TestGetFileSize;
		}

		const long TestGetFileSize = 200;
		long GetFileSizeThrowException(string fileName)
		{
			throw new SshException("ErrorOnGettingFileSize");
		}

		bool CheckFileExists(string fileName)
		{
			return true;
		}

		#endregion
		#region ExpectedLogs
		const string ExpectedLogConnect = "";
		const string ExpectedLogChangeDir = "Connected";
		const string ExpectedLogCheckFileExists = "Connected\r\n" + "Current working directory is set to upld\r\n" + "Uploaded testremote from test.txt";
		const string ExpectedLogCannotOverwriteFile = "Connected\r\n" + "Uploaded testremote from test.txt\r\n" + "fln exists\r\n" + "File size of fln is 0\r\n" + "testremote exists\r\n" + "Deleted testremote\r\n";
		const string ExpectedLogPutFile = "Connected\r\n" + "Current working directory is set to upld\r\n";
		const string ExpectedLogRename = "Connected\r\n" + "Current working directory is set to upld\r\n" + "Uploaded testremote from test.txt\r\n" + "fln not exists\r\n" + "testremote exists\r\n" + "Deleted testremote\r\n";
		const string ExpectedLogComplete = "Connected\r\n" + "Current working directory is set to upld\r\n" + "Uploaded testremote from test.txt\r\n" + "fln not exists\r\n" + "Renamed testremote to fln\r\n" + "testremote not exists";
		const string ExpectedLogGetTargetFileSize_FileDoesNotExist = "Connected\r\n" + "Current working directory is set to upld\r\n" + "File size of fln is 0";
		const string ExpectedLogGetTargetFileSize_FileExists = "Connected\r\n" + "Current working directory is set to upld\r\n" + "File size of fln is 200";
		#endregion
		#region Implementation
		FtpUploaderForTest FtpUploader
		{
			get
			{
				if (fFtpUploader == null)
				{
					fFtpUploader = new FtpUploaderForTest(Notifications);
				}

				return fFtpUploader;
			}
		}

		FtpUploaderForTest fFtpUploader;
		NotificationBufferForTesting Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new NotificationBufferForTesting();
				}

				return fNotifications;
			}
		}

		NotificationBufferForTesting fNotifications;
		void AssertExceptionThrown(FtpClientForTest.ExceptionToBeThrown toBeThrown, string expectedLog, string expectedLastErrorMessage)
		{
			FtpUploader.TestFtpClient.ClearLog();
			FtpUploader.TestFtpClient.ToBeThrown = toBeThrown;
			AssertEquals("Should be returning false when there is error", false, FtpUploader.UploadToFtpServerAndArchive(TestFileToUpload));
			AssertLogEquals(expectedLog, FtpUploader.TestFtpClient);
			AssertEquals("Should try 3 times", 3, FtpUploader.DoUploadCallCounter);
			AssertEquals("There should be error notifications", true, Notifications.HasErrors);
			AssertEquals("There should be notifications indicating the retries", true, Notifications.AsString.IndexOf("Retrying...") != -1);
			AssertEquals(expectedLastErrorMessage, ((INotificationSubscriberNotification)Notifications.Events[0]).AdditionalInfo);
		}

		void AssertLogEquals(ZString expectedLog, FtpClientForTest ftpClient)
		{
			ZString actualLog = ftpClient.Log.Replace(TestFileToUpload, "test.txt");
			AssertEquals(true, actualLog.StartsWith(expectedLog));
		}

		ZString TestFileToUpload;
		protected override void SetUp()
		{
			base.SetUp();
			TestFileToUpload = Env.GetTempFileName();
			if (Directory.Exists(FtpUploader.ArchiveDirectory))
			{
				Directory.Delete(FtpUploader.ArchiveDirectory, true);
			}

			Directory.CreateDirectory(FtpUploader.ArchiveDirectory);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (Directory.Exists(FtpUploader.ArchiveDirectory))
			{
				Directory.Delete(FtpUploader.ArchiveDirectory, true);
			}

			if (File.Exists(TestFileToUpload))
			{
				File.Delete(TestFileToUpload);
			}
		}

		#region FtpUploaderForTest
		class FtpUploaderForTest : FtpUploader
		{
			public FtpUploaderForTest(INotifications notifications) : base(notifications, true)
			{
			}

			protected override ZString GetTempFileName()
			{
				return "testremote";
			}

			public override ZString Password
			{
				get
				{
					return "pwd";
				}
			}

			public override ZString ServerAddress
			{
				get
				{
					return "localhost";
				}
			}

			public override ZString ServerName
			{
				get
				{
					return "TestSomething";
				}
			}

			public override int ServerPort
			{
				get
				{
					return 568;
				}
			}

			public override ZString UploadDirectory
			{
				get
				{
					return "upld";
				}
			}

			public override ZString UploadFilename
			{
				get
				{
					return "fln";
				}
			}

			public override ZString ArchiveDirectory
			{
				get
				{
					return Path.Combine(Env.TempPath, "archive");
				}
			}

			public override ZString Username
			{
				get
				{
					return "testuser";
				}
			}

			protected override ISftpClient GetNewFtpClient()
			{
				return TestFtpClient;
			}

			public bool TestCanOverwriteTargetFile;
			protected override bool CanOverwriteTargetFile(long targetFileSize)
			{
				return TestCanOverwriteTargetFile;
			}

			public new void WarnTargetFileCannotBeOverwritten()
			{
				base.WarnTargetFileCannotBeOverwritten();
			}

			public new long GetTargetFileSize()
			{
				return base.GetTargetFileSize();
			}

			public bool BaseCanOverwriteTargetFile(long targetFileSize)
			{
				return base.CanOverwriteTargetFile(targetFileSize);
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
			public int DoUploadCallCounter;
			protected override bool DoUpload(string localFileName)
			{
				DoUploadCallCounter++;
				return base.DoUpload(localFileName);
			}

			public delegate long FileSizeEventHandler(string fileName);
			public FileSizeEventHandler GetFileSizeHandler;
			protected override long GetTargetFileSize(ISftpClient sftpClient)
			{
				long result;
				try
				{
					result = (GetFileSizeHandler != null) ? GetFileSizeHandler(UploadFilename) : base.GetTargetFileSize(sftpClient);
				}
				catch (Exception)
				{
					result = 0;
				}

				TestFtpClient.Logger.Add(string.Format("File size of {0} is {1}", UploadFilename, result));
				return result;
			}
		}
		#endregion
		#endregion
	}
}
