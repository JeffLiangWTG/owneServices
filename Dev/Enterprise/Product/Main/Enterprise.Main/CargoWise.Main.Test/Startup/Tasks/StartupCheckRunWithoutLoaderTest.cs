using CargoWise.Definitions;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core.Test.Utilities;

namespace CargoWise.Main.Test.Startup.Tasks
{
	class StartupCheckRunWithoutLoaderTest : AbstractApplicationStartupTaskTest<StartupCheckRunWithoutLoader>
	{
		public override int DefaultErrorExitCode => ExitCodes.StartupCheckRunWithoutLoaderError;
	}
}
