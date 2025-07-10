using CargoWise.Definitions;
using Enterprise.Client.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	public class RemoveOldInstallationsTask : BackgroundApplicationStartupTask
	{
		public override int FailureExitCode => ExitCodes.RemoveOldInstallationsTaskError;

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return ((bool)arguments[ApplicationArguments.OptionRunWithoutLoader] || (bool)arguments[ApplicationArguments.OptionScheduledDbUpgrader]) && !(bool)arguments[ApplicationArguments.OptionSkipVersionCheck];
		}

		public override void DoExecute()
		{
			OldVersionsRemover.Run(InstallationEnvironment.Instance.BaseInstallPath, ReleaseInfo.Instance.VersionNumber.ToVersion());
		}
	}
}
