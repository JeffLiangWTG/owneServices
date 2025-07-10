using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using static Enterprise.Startup.StartupNotification;

namespace CargoWise.Main.Test.Startup.Tasks
{
	internal class InitializationTaskTest : AbstractApplicationStartupTaskTest<InitializationTask>
	{
		public override int DefaultErrorExitCode => ExitCodes.InitializationTaskError;
	}
}
