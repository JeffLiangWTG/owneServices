using System.Reflection;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal abstract class FtpTransferTestCase : TransactionedTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be initialised in the constructor", Notifications, FtpTransfer.Notifications);
		}

		public void TestFtpClient()
		{
			MethodInfo method = typeof(FtpTransfer).GetMethod("GetNewFtpClient", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertEquals(typeof(SftpClientWrapper), method.Invoke(FtpTransfer, null).GetType());
		}

		public void TestBaseProperties()
		{
			AssertEquals(ExpectedServerAddress, FtpTransfer.ServerAddress);
			AssertEquals(ExpectedServerName, FtpTransfer.ServerName);
			AssertEquals(ExpectedServerPort, FtpTransfer.ServerPort);
			AssertEquals(ExpectedUsername, FtpTransfer.Username);
			AssertEquals(ExpectedPassword, FtpTransfer.Password);
		}

		protected FtpTransfer FtpTransfer
		{
			get
			{
				if (fFtpTransfer == null)
				{
					fFtpTransfer = GetNewFtpTransfer(Notifications);
				}

				return fFtpTransfer;
			}
		}

		protected NotificationBuffer Notifications = new NotificationBuffer();
		#region Abstract
		protected abstract string ExpectedServerName { get; }

		protected abstract string ExpectedServerAddress { get; }

		protected abstract int ExpectedServerPort { get; }

		protected abstract string ExpectedUsername { get; }

		protected abstract string ExpectedPassword { get; }

		protected abstract FtpTransfer GetNewFtpTransfer(INotifications notificationSubscriber);
		#endregion
		FtpTransfer fFtpTransfer;
	}
}
