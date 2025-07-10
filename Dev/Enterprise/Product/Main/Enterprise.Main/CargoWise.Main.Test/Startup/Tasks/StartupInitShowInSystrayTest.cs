using CargoWise.Definitions;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core.Test.Utilities;

namespace CargoWise.Main.Test.Startup.Tasks
{
	class StartupInitShowInSystrayTest : AbstractApplicationStartupTaskTest<StartupInitShowInSystray>
	{
		public override int DefaultErrorExitCode => ExitCodes.StartupInitShowInSystrayError;
	}
}
