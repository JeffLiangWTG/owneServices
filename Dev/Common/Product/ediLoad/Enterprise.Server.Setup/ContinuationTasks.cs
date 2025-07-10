using CargoWise.Loader.Common;

namespace Enterprise.Server.Setup
{
	class WriteContinuationFile : InstallationItem
	{
		public WriteContinuationFile(Installation installation, InstallationSettings installationSettings)
			: base(installation)
		{
			this.installationSettings = installationSettings;
		}

		readonly InstallationSettings installationSettings;

		protected override bool NeedsToInstallCore()
		{
			return Installation.Configuration.UILevel != UILevel.AutomatedWithNoUI;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			installationSettings.WriteContinuationFile();
			return InstallationResult.OK();
		}
	}

	class DeleteContinuationFile : InstallationItem
	{
		public DeleteContinuationFile(Installation installation, InstallationSettings installationSettings)
			: base(installation)
		{
			this.installationSettings = installationSettings;
		}

		readonly InstallationSettings installationSettings;

		protected override bool NeedsToInstallCore()
		{
			return Installation.Configuration.UILevel != UILevel.AutomatedWithNoUI;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			installationSettings.DeleteContinuationFile();
			return InstallationResult.OK();
		}
	}
}
