using CargoWise.Definitions;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Core.Test.Utilities;

namespace CargoWise.Main.Test.Startup.Tasks
{
	class ResourceStringsUpdaterTaskTest : AbstractApplicationStartupTaskTest<ResourceStringsUpdaterTask>
	{
		public override int DefaultErrorExitCode => ExitCodes.ResourceStringsUpdaterTaskError;
	}
}
