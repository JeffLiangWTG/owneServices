using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup
{
	public class RemoveOldUpgradePackagesTask : BackgroundApplicationStartupTask
	{
		public override int FailureExitCode => ExitCodes.RemoveOldUpgradePackagesTaskError;

		public override void DoExecute()
		{
			if (EnvProxy.Instance.Registry.RemoveOldUpgradePackages)
			{
				ObjectFactory.Get<DbUpgrader.Shared.IDbUpgraderRunner>().RemoveOldUpgradePackages();
			}
		}

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return (string)arguments[ApplicationArguments.OptionUpgrade] != null;
		}
	}
}
