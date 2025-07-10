using CargoWise.ComponentModel;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal class BISIDownloaderTest : FtpTransferTestCase
	{
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

		protected override string ExpectedUsername
		{
			get
			{
				return "Username1";
			}
		}

		protected override FtpTransfer GetNewFtpTransfer(INotifications notifications)
		{
			return new BISIDownloader(notifications);
		}

		protected override void SetUp()
		{
			base.SetUp();
			UPEDataRegistry.Instance.BISISftpServerAddress = "BISIServer";
			UPEDataRegistry.Instance.BISISftpServerPort = 12;
			UPEDataRegistry.Instance.BISIUploadDirectory = "Dir1";
			UPEDataRegistry.Instance.BISIUploadFilename = "File1.txt";
			UPEDataRegistry.Instance.BISISftpServerUsername = "Username1";
			UPEDataRegistry.Instance.BISISftpServerPassword = "Password1";
		}
	}
}
