using System.IO;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Loader.Common;

namespace Enterprise.Server.Setup
{
	class ServerInstallationTopLevelItem : InstallationProgramBase
	{
		public ServerInstallationTopLevelItem(Installation installation, InstallationSettings installationSettings)
			: base(installation)
		{
			this.installationSettings = installationSettings;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			this.Installation.Configuration.Services.MessageBox.Show(BrandingFactory.Instance.ProductName + " Server Setup has been completed. " + BrandingFactory.Instance.ProductName + " Client will now be launched.", BrandingFactory.Instance.ProductName + " Server Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return base.InstallExcludingDependencies();
		}

		public override string FullPathOfProgramToRun
		{
			get { return Path.Combine(installationSettings.CDPath, "CargoWise.Start.exe"); }
		}

		public override string Arguments
		{
			get
			{
				return $"{installationSettings.ServerName} {installationSettings.DbName}";
			}
			set
			{
			}
		}

		readonly InstallationSettings installationSettings;
	}
}
