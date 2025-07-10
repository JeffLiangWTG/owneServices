using CargoWise.Definitions;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core.Test.Utilities;

namespace CargoWise.Main.Test.Startup.Tasks
{
	class ExitApplicationTaskTest : AbstractApplicationStartupTaskTest<ExitApplicationTask>
	{
		public override int DefaultErrorExitCode => ExitCodes.Success;
	}
}
