using System.Windows.Forms;
using CargoWise.Loader.Common;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public class InstallationDoneNotifier : InstallationItem
	{
		readonly Installation installation;
		readonly string pluginProductName;

		public InstallationDoneNotifier(Installation installation, string pluginProductName) : base(installation)
		{
			this.installation = installation;
			this.pluginProductName = pluginProductName;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			installation.Configuration.Services.MessageBox.Show(
				$"{pluginProductName} upgrade completed.",
				pluginProductName,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);

			return InstallationResult.OK();
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}
	}
}
