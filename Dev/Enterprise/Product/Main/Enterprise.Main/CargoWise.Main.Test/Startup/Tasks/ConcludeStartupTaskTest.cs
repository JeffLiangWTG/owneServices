using CargoWise.Definitions;
using Enterprise.Startup.Tasks;
using Enterprise.ZArchitecture.Core.Test.Utilities;

namespace CargoWise.Main.Test.Startup.Tasks
{
	class ConcludeStartupTaskTest : AbstractApplicationStartupTaskTest<ConcludeStartupTask>
	{
		public override int DefaultErrorExitCode => ExitCodes.Success;
	}
}
