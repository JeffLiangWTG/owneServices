using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Renci.SshNet;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	class BISIEntryPrintUploaderTest : FtpUploaderTestCase
	{
		public void TestCanOverwriteTargetFile()
		{
			BISIEntryPrintUploaderForTest uploader = new BISIEntryPrintUploaderForTest(Notifications);
			AssertEquals(true, uploader.CanOverwriteTargetFile(0));
			AssertEquals(false, uploader.CanOverwriteTargetFile(100000));
		}

		public void TestWarningMessageWhenFileCannotBeOverwritten()
		{
			BISIEntryPrintUploaderForTest uploader = new BISIEntryPrintUploaderForTest(Notifications);
			AssertEquals("Pre-condition", 0, Notifications.Events.Length);
			uploader.WarnTargetFileCannotBeOverwritten();
			AssertEquals(1, Notifications.Events.Length);
			AssertEquals("\"File1.txt\" has not been processed by the BISI Mainframe", ((INotificationSubscriberNotification)Notifications.Events[0]).AdditionalInfo);
		}

		#region TestCanUpload
		public void TestCanUpload()
		{
			BISIEntryPrintUploaderForTest uploader = new BISIEntryPrintUploaderForTest(Notifications);
			uploader.GetFileSizeHandler += new BISIEntryPrintUploaderForTest.FileSizeEventHandler(GetFileSize);
			AssertEquals("Pre-condition", 0, Notifications.Events.Length);
			TestGetFileSize = 12;
			AssertEquals(false, uploader.CanUpload());
			AssertEquals(ExpectedLogGetTargetFileSize, uploader.TestFtpClient.Log);
			AssertEquals("There should be a warning", 1, Notifications.Events.Length);
			AssertEquals("\"File1.txt\" has not been processed by the BISI Mainframe", ((INotificationSubscriberNotification)Notifications.Events[0]).AdditionalInfo);
			Notifications.Clear();
			TestGetFileSize = 0;
			AssertEquals(true, uploader.CanUpload());
			AssertEquals("There should be no warnings", 0, Notifications.Events.Length);
		}

		public void TestCanUpload_WhenNoServerAddressSpecified()
		{
			BISIEntryPrintUploaderForTest uploader = new BISIEntryPrintUploaderForTest(Notifications);
			AssertEquals("Should be able to upload when ServerAddress is populated", true, uploader.CanUpload());
			ZString oldEntryPrintFtpServerAddress = UPEDataRegistry.Instance.EntryPrintSftpServerAddress;
			try
			{
				UPEDataRegistry.Instance.EntryPrintSftpServerAddress = "";
				AssertEquals("Should not be able to upload when ServerAddress is empty", false, uploader.CanUpload());
			}
			finally
			{
				UPEDataRegistry.Instance.EntryPrintSftpServerAddress = oldEntryPrintFtpServerAddress;
			}
		}

		long GetFileSize(string fileName)
		{
			return TestGetFileSize;
		}

		const string ExpectedLogGetTargetFileSize = "Connected\r\n" + "Current working directory is set to Dir1\r\n" + "File size of File1.txt is 12";
		#endregion
		#region Implementation
		protected override string ExpectedPassword
		{
			get
			{
				return "Password1";
			}
		}

		protected override string ExpectedServerAddress
		{
			get
			{
				return "BISIServer";
			}
		}

		protected override string ExpectedServerName
		{
			get
			{
				return "BISI Mainframe";
			}
		}

		protected override int ExpectedServerPort
		{
			get
			{
				return 12;
			}
		}

		protected override string ExpectedUploadDirectory
		{
			get
			{
				return "Dir1";
			}
		}

		protected override string ExpectedUploadFilename
		{
			get
			{
				return "File1.txt";
			}
		}

		protected override string ExpectedArchiveDirectory
		{
			get
			{
				return "Archive";
			}
		}

		protected override string ExpectedUsername
		{
			get
			{
				return "Username1";
			}
		}

		protected override FtpUploader GetNewFtpUploader(INotifications notifications)
		{
			return new BISIEntryPrintUploader(notifications);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetRegistryItems();
		}

		protected override void TearDown()
		{
			ClearRegistryItems();
			base.TearDown();
		}

		void SetRegistryItems()
		{
			InitialServerAddress = UPEDataRegistry.Instance.EntryPrintSftpServerAddress;
			InitialServerPort = UPEDataRegistry.Instance.EntryPrintSftpServerPort;
			InitialServerUploadDirectory = UPEDataRegistry.Instance.EntryPrintSftpServerUploadDirectory;
			InitialServerUploadFilename = UPEDataRegistry.Instance.EntryPrintSftpServerUploadFilename;
			InitialUsername = UPEDataRegistry.Instance.EntryPrintSftpServerUsername;
			InitialPassword = UPEDataRegistry.Instance.EntryPrintSftpServerPassword;
			InitialArchiveDirectory = UPEDataRegistry.Instance.EntryPrintSftpArchiveDirectory;
			UPEDataRegistry.Instance.EntryPrintSftpServerAddress = "BISIServer";
			UPEDataRegistry.Instance.EntryPrintSftpServerPort = 12;
			UPEDataRegistry.Instance.EntryPrintSftpServerUploadDirectory = "Dir1";
			UPEDataRegistry.Instance.EntryPrintSftpServerUploadFilename = "File1.txt";
			UPEDataRegistry.Instance.EntryPrintSftpServerUsername = "Username1";
			UPEDataRegistry.Instance.EntryPrintSftpServerPassword = "Password1";
			UPEDataRegistry.Instance.EntryPrintSftpArchiveDirectory = "Archive";
		}

		void ClearRegistryItems()
		{
			UPEDataRegistry.Instance.EntryPrintSftpServerAddress = InitialServerAddress;
			UPEDataRegistry.Instance.EntryPrintSftpServerPort = InitialServerPort;
			UPEDataRegistry.Instance.EntryPrintSftpServerUploadDirectory = InitialServerUploadDirectory;
			UPEDataRegistry.Instance.EntryPrintSftpServerUploadFilename = InitialServerUploadFilename;
			UPEDataRegistry.Instance.EntryPrintSftpServerUsername = InitialUsername;
			UPEDataRegistry.Instance.EntryPrintSftpServerPassword = InitialPassword;
			UPEDataRegistry.Instance.EntryPrintSftpArchiveDirectory = InitialArchiveDirectory;
		}

		ZString InitialServerAddress;
		ZInt InitialServerPort;
		ZString InitialServerUploadDirectory;
		ZString InitialServerUploadFilename;
		ZString InitialUsername;
		ZString InitialPassword;
		ZString InitialArchiveDirectory;
		long TestGetFileSize;
		#region BISIEntryPrintUploaderForTest
		class BISIEntryPrintUploaderForTest : BISIEntryPrintUploader
		{
			public BISIEntryPrintUploaderForTest(INotifications notifications) : base(notifications)
			{
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

			public new bool CanOverwriteTargetFile(long targetFileSize)
			{
				return base.CanOverwriteTargetFile(targetFileSize);
			}

			public new void WarnTargetFileCannotBeOverwritten()
			{
				base.WarnTargetFileCannotBeOverwritten();
			}

			public delegate long FileSizeEventHandler(string fileName);
			public FileSizeEventHandler GetFileSizeHandler;
			protected override long GetTargetFileSize(ISftpClient sftpClient)
			{
				long result;
				try
				{
					result = (GetFileSizeHandler != null) ? GetFileSizeHandler(UploadFilename) : base.GetTargetFileSize(sftpClient);
					TestFtpClient.Logger.Add(string.Format("File size of {0} is {1}", UploadFilename, result));
				}
				catch (Exception)
				{
					result = 0;
				}

				return result;
			}

			FtpClientForTest fTestFtpClient;
		}
		#endregion
		#endregion
	}
}
