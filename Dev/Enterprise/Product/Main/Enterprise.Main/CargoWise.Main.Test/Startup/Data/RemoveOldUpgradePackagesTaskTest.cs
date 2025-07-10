using CargoWise.Definitions;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core.Testing;

namespace CargoWise.Main.Test.Startup.Data
{
	class RemoveOldUpgradePackagesTaskTest : BackgroundApplicationStartupTaskTest<RemoveOldUpgradePackagesTask>
	{
		public override int DefaultErrorExitCode => ExitCodes.RemoveOldUpgradePackagesTaskError;
	}
}
