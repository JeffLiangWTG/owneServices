using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;

namespace Enterprise.Loader
{
	public class VersionInfoInitializer : InstallationItem
	{
		public VersionInfoInitializer(Installation installation)
			: this(installation, new DatabaseConnectionInitializer(installation))
		{
		}

		internal VersionInfoInitializer(Installation installation, DatabaseConnectionInitializer databaseConnectionInitializer)
			: base(installation)
		{
			if (databaseConnectionInitializer != null)
			{
				AddDependency(databaseConnectionInitializer);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		protected override InstallationResult InstallExcludingDependencies()
		{
			if (ranOnce)
			{
				return InstallationResult.OK();
			}

			ChangeCurrentTaskDescription("Checking current version");
			EnterpriseConfiguration configuration = (EnterpriseConfiguration)Installation.Configuration;
			UpgradeManager upgradeManager = configuration.NewUpgradeManager();
			CurrentVersion = upgradeManager.QueryCurrentVersion();
			if (CurrentVersion != null && CurrentVersion.Version < UpgradeManager.MinimumRunnableVersion)
			{
				CurrentVersion = null;
				if (!configuration.SelectCurrentVersion)
				{
					MessageBox.Show("The current version marked in the database is too old to be run from a " + BrandingFactory.Instance.ProductName + " startup shortcut. You must upgrade to a newer version to continue.", BrandingFactory.Instance.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
			ranOnce = true;
			return InstallationResult.OK();
		}

		protected override bool NeedsToInstallCore()
		{
			return !ranOnce;
		}

		static bool ranOnce;
		public static UpgradeInfo CurrentVersion { get; private set; }
	}
}
