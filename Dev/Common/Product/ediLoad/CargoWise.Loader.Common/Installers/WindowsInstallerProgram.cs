using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public class WindowsInstallerProgram : InstallationProgram
	{
		public const string ErrorMessageWasMessage = "The error message was: ";
		const int ErrorSuccessRebootRequired = 3010;
		readonly InstallationItem parentInstallationItem;
		bool silentlyIgnoreRebootRequired;

		public WindowsInstallerProgram(InstallationItem parentInstallationItem, string nameOfComponent) : this(parentInstallationItem, nameOfComponent, true)
		{ }

		public WindowsInstallerProgram(InstallationItem parentInstallationItem, string nameOfComponent, bool waitForExit)
			: base(parentInstallationItem.Installation)
		{
			Argument.NotNull(parentInstallationItem, nameof(parentInstallationItem));

			this.parentInstallationItem = parentInstallationItem;
			this.WaitForExit = waitForExit;
			this.nameOfComponent = nameOfComponent;
		}

		readonly string nameOfComponent;

		public bool SilentlyIgnoreRebootRequired
		{
			get { return silentlyIgnoreRebootRequired; }
			set { silentlyIgnoreRebootRequired = value; }
		}

		protected override bool NeedsToInstallCore()
		{
			return parentInstallationItem.NeedsToInstall();
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			ChangeCurrentTaskDescription("Installing " + nameOfComponent);
			return base.InstallExcludingDependencies();
		}

		protected override InstallationResult ResultForExitCode(int exitCode)
		{
			switch (exitCode)
			{
				case 0:
					return InstallationResult.OK();

				// http://support.microsoft.com/default.aspx?scid=kb;en-us;255582
				case 1305:
				case 2755:
					return InstallationResult.Error(UnableToInstallMessage + MappedNetworkDriveMessage);

				case ErrorSuccessRebootRequired:
					return SilentlyIgnoreRebootRequired ?
						InstallationResult.OK() :
						InstallationResult.Warning("A new software component was installed, but it will not take effect until you reboot your computer.");

				default:
					return InstallationResult.Error(UnableToInstallMessage + ErrorMessageWasMessage + new Win32Exception(exitCode).Message + ".");
			}
		}

		public string MappedNetworkDriveMessage
		{
			get
			{
				return string.Format(
					"This error can occur if you are running {0} from a mapped network drive. You should run {0} using a UNC path (e.g. \\\\server_name\\{0}) instead of a mapped network drive.",
					Installation.Configuration.ApplicationName);
			}
		}

		public string UnableToInstallMessage
		{
			get
			{
				return Installation.Configuration.ApplicationName + " was unable to install " + nameOfComponent + ". ";
			}
		}
	}
}
