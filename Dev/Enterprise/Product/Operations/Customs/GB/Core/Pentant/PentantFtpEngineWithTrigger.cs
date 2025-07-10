using System.IO;
using Enterprise.Customs.GB.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.GB.Pentant
{
	class PentantFtpEngineWithTrigger : FtpEngineWithTrigger
	{
		public PentantFtpEngineWithTrigger(IProviderOfTriggerFtpOptions optionsProvider, ILogger logger) : base(logger, optionsProvider)
		{
		}

		public void Upload()
		{
			if (Initialise())
			{
				PushLocalFilesToTarget();
			}
		}

		public void Download()
		{
			if (Initialise())
			{
				PullRemoteFilesFromSource();
			}
		}

		protected override void DeleteFileSetCore(FileInfo localTriggerFile, string fullName)
		{
			base.DeleteFileSetCore(localTriggerFile, fullName);
			foreach (var fi in localEnterpriseSharedFolder.GetFiles(Path.GetFileNameWithoutExtension(localTriggerFile.Name) + ".*"))
			{
				serviceLogger.Log(LogType.Information, "Deleting local file " + fi.Name);
				fi.Delete();
			}
		}

		public override string[] TriggerFileExtensionsIncludingDots_Pull => new[] { PentantConstants.TriggerFileExtension, PentantConstants.CargoMessageFileExtension };

		public override string TriggerFileExtensionIncludingDots_Push => PentantConstants.TriggerFileExtension;
	}
}
