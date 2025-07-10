using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Renci.SshNet;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal class BISIUploaderTest : FtpUploaderTestCase
	{
		public void TestCanOverwriteTargetFile()
		{
			BISIUploaderForTest uploader = new BISIUploaderForTest(Notifications);
			AssertEquals(true, uploader.CanOverwriteTargetFile(0));
			AssertEquals(false, uploader.CanOverwriteTargetFile(100000));
			AssertEquals(true, uploader.CanOverwriteTargetFile(302));
		}

		public void TestWarningMessageWhenFileCannotBeOverwritten()
		{
			BISIUploaderForTest uploader = new BISIUploaderForTest(Notifications);
			AssertEquals("Pre-condition", 0, Notifications.Events.Length);
			uploader.WarnTargetFileCannotBeOverwritten();
			AssertEquals(1, Notifications.Events.Length);
			AssertEquals("\"File1.txt\" has not been processed by the BISI Mainframe", ((INotificationSubscriberNotification)Notifications.Events[0]).AdditionalInfo);
		}

		#region TestCanUpload
		public void TestCanUpload()
		{
			BISIUploaderForTest uploader = new BISIUploaderForTest(Notifications);
			uploader.GetFileSizeHandler += new BISIUploaderForTest.FileSizeEventHandler(GetFileSize);
			AssertEquals("Pre-condition", 0, Notifications.Events.Length);
			TestGetFileSize = 12;
			AssertEquals(false, uploader.CanUpload());
			AssertEquals(ExpectedLogGetTargetFileSize, uploader.TestFtpClient.Log);
			AssertEquals("There should be a warning", 1, Notifications.Events.Length);
			AssertEquals("\"File1.txt\" has not been processed by the BISI Mainframe", ((INotificationSubscriberNotification)Notifications.Events[0]).AdditionalInfo);
			Notifications.Clear();
			TestGetFileSize = 302;
			AssertEquals(true, uploader.CanUpload());
			AssertEquals("There should be no warnings", 0, Notifications.Events.Length);
			TestGetFileSize = 1;
			AssertEquals(false, uploader.CanUpload());
			AssertEquals("There should be a warning", 1, Notifications.Events.Length);
			AssertEquals("\"File1.txt\" has not been processed by the BISI Mainframe", ((INotificationSubscriberNotification)Notifications.Events[0]).AdditionalInfo);
			Notifications.Clear();
			TestGetFileSize = 0;
			AssertEquals(true, uploader.CanUpload());
			AssertEquals("There should be no warnings", 0, Notifications.Events.Length);
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
			return new BISIUploader(notifications);
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
			InitialServerAddress = UPEDataRegistry.Instance.BISISftpServerAddress;
			InitialServerPort = UPEDataRegistry.Instance.BISISftpServerPort;
			InitialUploadDirectory = UPEDataRegistry.Instance.BISIUploadDirectory;
			InitialUploadFilename = UPEDataRegistry.Instance.BISIUploadFilename;
			InitialUsername = UPEDataRegistry.Instance.BISISftpServerUsername;
			InitialPassword = UPEDataRegistry.Instance.BISISftpServerPassword;
			InitialArchiveDirectory = UPEDataRegistry.Instance.BISIUploadArchiveDirectory;
			UPEDataRegistry.Instance.BISISftpServerAddress = "BISIServer";
			UPEDataRegistry.Instance.BISISftpServerPort = 12;
			UPEDataRegistry.Instance.BISIUploadDirectory = "Dir1";
			UPEDataRegistry.Instance.BISIUploadFilename = "File1.txt";
			UPEDataRegistry.Instance.BISISftpServerUsername = "Username1";
			UPEDataRegistry.Instance.BISISftpServerPassword = "Password1";
			UPEDataRegistry.Instance.BISIUploadArchiveDirectory = "Archive";
		}

		void ClearRegistryItems()
		{
			UPEDataRegistry.Instance.BISISftpServerAddress = InitialServerAddress;
			UPEDataRegistry.Instance.BISISftpServerPort = InitialServerPort;
			UPEDataRegistry.Instance.BISIUploadDirectory = InitialUploadDirectory;
			UPEDataRegistry.Instance.BISIUploadFilename = InitialUploadFilename;
			UPEDataRegistry.Instance.BISISftpServerUsername = InitialUsername;
			UPEDataRegistry.Instance.BISISftpServerPassword = InitialPassword;
			UPEDataRegistry.Instance.BISIUploadArchiveDirectory = InitialArchiveDirectory;
		}

		ZString InitialServerAddress;
		ZInt InitialServerPort;
		ZString InitialUploadDirectory;
		ZString InitialUploadFilename;
		ZString InitialUsername;
		ZString InitialPassword;
		ZString InitialArchiveDirectory;
		long TestGetFileSize;
		#region BISIUploaderForTest
		class BISIUploaderForTest : BISIUploader
		{
			public BISIUploaderForTest(INotifications notifications) : base(notifications)
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

			FtpClientForTest fTestFtpClient;
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
