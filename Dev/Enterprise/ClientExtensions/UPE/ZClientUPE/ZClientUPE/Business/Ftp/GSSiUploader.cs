using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.Ftp
{
	public class GSSiUploader : FtpUploader
	{
		public GSSiUploader(INotifications notifications, string uploadFileName)
			: base(notifications, false)
		{
			this.uploadFileName = uploadFileName;
		}
		readonly string uploadFileName;

		public override ZString ServerAddress => UPEDataRegistry.Instance.GSSiSftpServerAddress;

		public override int ServerPort => UPEDataRegistry.Instance.GSSiSftpServerPort;

		public override ZString ServerName => "GSSi Mainframe";

		public override ZString Username => UPEDataRegistry.Instance.GSSiSftpServerUsername;

		public override ZString Password => UPEDataRegistry.Instance.GSSiSftpServerPassword;

		public override ZString UploadDirectory => UPEDataRegistry.Instance.GSSiSftpServerUploadDirectory;

		public override ZString UploadFilename => uploadFileName;

		public override ZString ArchiveDirectory => "";

		protected override ZString WarningMessageWhenFileCannotBeOverwritten => "\"" + UploadFilename + "\" has not been processed by the " + ServerName;

		protected override bool CanOverwriteTargetFile(long targetFileSize) => targetFileSize == 0;
	}
}