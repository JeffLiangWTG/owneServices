using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Loader.Common;

namespace Enterprise.Loader
{
	class RepairInstaller : InstallationItem
	{
		public RepairInstaller(Installation installation)
			: base(installation)
		{
			AddDependency(new VersionInfoInitializer(installation));
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			ChangeCurrentTaskDescription("Repairing installation");
			try
			{
				var upgradeManager = ((EnterpriseConfiguration)Installation.Configuration).NewUpgradeManager();
				upgradeManager.Repair(VersionInfoInitializer.CurrentVersion, Path.Combine(Installation.Configuration.BaseTargetPath, VersionInfoInitializer.CurrentVersion.Version.ToString()));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return InstallationResult.Error("Repair Failed:\r\n" + ex.ToString());
			}
			return InstallationResult.OK();
		}
	}
}
