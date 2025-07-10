using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Loader.Common;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public class SessionKillerWarning : InstallationItem
	{
		readonly string pluginHostingProcessName;
		readonly string pluginProductName;

		public SessionKillerWarning(Installation installation, string pluginHostingProcessName, string pluginProductName)
			: base(installation)
		{
			this.pluginHostingProcessName = pluginHostingProcessName;
			this.pluginProductName = pluginProductName;
		}

		protected override bool NeedsToInstallCore()
		{
			return AssemblyResolver.IsApplicationManagerInstalled();
		}

		[SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override InstallationResult InstallExcludingDependencies()
		{
			const int accessDeniedErrorCode = 5;
			try
			{
				var result = InstallationResult.OK();
				var processes = Process.GetProcessesByName(pluginHostingProcessName).Where(p => !p.HasExited && !string.IsNullOrEmpty(p.MainWindowTitle));
				if (processes.Any())
				{
					if (Installation.Configuration.Services.MessageBox.Show($"The following applications will be automatically closed:\r\n" + string.Join("\r\n", processes.Select(p => "    " + p.MainWindowTitle)), pluginProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
					{
						result = InstallationResult.Error("Installation cancelled");
					}
				}

				return result;
			}
			catch (Win32Exception e) when (e.NativeErrorCode == accessDeniedErrorCode)
			{
				var errorMessage = $"{pluginProductName} could not be updated because other user sessions are currently using it. CargoWise will open but you will not have the latest {pluginProductName} features available to you. Please contact your system administrator to organise the update to occur in a maintenance window.";
				Installation.Configuration.Services.MessageBox.Show(errorMessage, pluginProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return InstallationResult.Error(errorMessage);
			}
		}

		protected override bool IsRemoteExclusive
		{
			get { return true; }
		}
	}
}
