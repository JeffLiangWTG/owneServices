using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Renci.SshNet;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal class FtpTransferTest : TestCase
	{
		public void TestGetTempFileName()
		{
			string generated = FtpTransfer.GetTempFileName();
			AssertEquals("Has to be a valid guid", true, new ZGuid(generated.Replace(".txt", "")).IsValid);
		}

		[ExpectNoExceptions]
		public void TestDeleteRemoteFile_DoesNotBlowUpWhenNullBeingPassedIn()
		{
			FtpTransfer.DeleteRemoteFile(null);
		}

		public void TestDeleteRemoteFile()
		{
			FtpTransfer.DeleteRemoteFile(new List<string>()
			{ "/EBS/Prod/FileToDelete1", "/EBS/Prod2/FileToDelete2", "/EBS/Prod/FileToDelete3" });
			string expectedDeleteRemoteFilesLog = "Connected\r\n" + "Deleted /EBS/Prod/FileToDelete1\r\n" + "Deleted /EBS/Prod/FileToDelete3\r\n" + "Deleted /EBS/Prod2/FileToDelete2";
			AssertEquals(expectedDeleteRemoteFilesLog, FtpTransfer.TestFtpClient.Log);
		}

		#region Implementation
		FtpTransferForTest FtpTransfer
		{
			get
			{
				if (fFtpTransfer == null)
				{
					fFtpTransfer = new FtpTransferForTest(new NotificationBuffer());
				}

				return fFtpTransfer;
			}
		}

		FtpTransferForTest fFtpTransfer;
		#region FtpTransferForTest
		class FtpTransferForTest : FtpTransfer
		{
			public FtpTransferForTest(INotifications notificationSubscriber) : base(notificationSubscriber)
			{
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

			public new string GetTempFileName()
			{
				return base.GetTempFileName();
			}

			public override ZString ServerName
			{
				get
				{
					return "123";
				}
			}

			public override ZString Password
			{
				get
				{
					return "p";
				}
			}

			public override ZString ServerAddress
			{
				get
				{
					return "localhost";
				}
			}

			public override int ServerPort
			{
				get
				{
					return 0;
				}
			}

			public override ZString Username
			{
				get
				{
					return "u";
				}
			}

			protected override ISftpClient GetNewFtpClient()
			{
				return TestFtpClient;
			}

			FtpClientForTest fTestFtpClient;
		}
		#endregion
		#endregion
	}
}
