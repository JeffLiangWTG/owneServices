using CargoWise.ComponentModel;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal abstract class FtpUploaderTestCase : FtpTransferTestCase
	{
		public void TestFtpUploaderProperties()
		{
			AssertEquals(ExpectedUploadDirectory, FtpUploader.UploadDirectory);
			AssertEquals(ExpectedUploadFilename, FtpUploader.UploadFilename);
			AssertEquals(ExpectedArchiveDirectory, FtpUploader.ArchiveDirectory);
		}

		protected override FtpTransfer GetNewFtpTransfer(INotifications notifications)
		{
			return GetNewFtpUploader(notifications);
		}

		FtpUploader FtpUploader
		{
			get
			{
				return (FtpUploader)FtpTransfer;
			}
		}

		#region Abstract
		protected abstract string ExpectedUploadDirectory { get; }

		protected abstract string ExpectedUploadFilename { get; }

		protected abstract string ExpectedArchiveDirectory { get; }

		protected abstract FtpUploader GetNewFtpUploader(INotifications notifications);
		#endregion
	}
}
