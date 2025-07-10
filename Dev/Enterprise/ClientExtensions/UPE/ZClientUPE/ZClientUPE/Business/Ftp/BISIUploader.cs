using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.Ftp
{
	public class BISIUploader : FtpUploader
	{
		public BISIUploader(INotifications notifications)
			: base(notifications)
		{
		}

		public override ZString ServerAddress
		{
			get { return UPEDataRegistry.Instance.BISISftpServerAddress; }
		}

		public override int ServerPort
		{
			get { return UPEDataRegistry.Instance.BISISftpServerPort; }
		}

		public override ZString ServerName
		{
			get { return "BISI Mainframe"; }
		}

		public override ZString Username
		{
			get { return UPEDataRegistry.Instance.BISISftpServerUsername; }
		}

		public override ZString Password
		{
			get { return UPEDataRegistry.Instance.BISISftpServerPassword; }
		}

		public override ZString UploadDirectory
		{
			get { return UPEDataRegistry.Instance.BISIUploadDirectory; }
		}

		public override ZString UploadFilename
		{
			get { return UPEDataRegistry.Instance.BISIUploadFilename; }
		}

		public override ZString ArchiveDirectory
		{
			get { return UPEDataRegistry.Instance.BISIUploadArchiveDirectory; }
		}

		protected override ZString WarningMessageWhenFileCannotBeOverwritten
		{
			get { return "\"" + UploadFilename + "\" has not been processed by the " + ServerName; }
		}

		protected override bool CanOverwriteTargetFile(long targetFileSize)
		{
			return (targetFileSize == 302 || targetFileSize == 0);
		}
	}
}
