using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business.Ftp
{
	public class BISIEntryPrintUploader : FtpUploader
	{
		public BISIEntryPrintUploader(INotifications notifications)
			: base(notifications)
		{
		}

		public override bool CanUpload()
		{
			// temporary until entry print is usable and we want to ftp it every time
			return !UPEDataRegistry.Instance.EntryPrintSftpServerAddress.IsEmpty && base.CanUpload();
		}

		public override ZString ServerAddress
		{
			get { return UPEDataRegistry.Instance.EntryPrintSftpServerAddress; }
		}

		public override int ServerPort
		{
			get { return UPEDataRegistry.Instance.EntryPrintSftpServerPort; }
		}

		public override ZString ServerName
		{
			get { return "BISI Mainframe"; }
		}

		public override ZString Username
		{
			get { return UPEDataRegistry.Instance.EntryPrintSftpServerUsername; }
		}

		public override ZString Password
		{
			get { return UPEDataRegistry.Instance.EntryPrintSftpServerPassword; }
		}

		public override ZString UploadDirectory
		{
			get { return UPEDataRegistry.Instance.EntryPrintSftpServerUploadDirectory; }
		}

		public override ZString UploadFilename
		{
			get { return UPEDataRegistry.Instance.EntryPrintSftpServerUploadFilename; }
		}

		public override ZString ArchiveDirectory
		{
			get { return UPEDataRegistry.Instance.EntryPrintSftpArchiveDirectory; }
		}

		protected override ZString WarningMessageWhenFileCannotBeOverwritten
		{
			get { return "\"" + UploadFilename + "\" has not been processed by the " + ServerName; }
		}

		protected override bool CanOverwriteTargetFile(long targetFileSize)
		{
			return targetFileSize == 0;
		}
	}
}