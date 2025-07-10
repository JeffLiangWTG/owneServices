using CargoWise.Definitions;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core.Test.Utilities;

namespace CargoWise.Main.Test.Startup.Tasks
{
	class StartupInitAutoTestingTest : AbstractApplicationStartupTaskTest<StartupInitAutoTesting>
	{
		public override int DefaultErrorExitCode => ExitCodes.StartupInitAutoTestingError;
	}
}
