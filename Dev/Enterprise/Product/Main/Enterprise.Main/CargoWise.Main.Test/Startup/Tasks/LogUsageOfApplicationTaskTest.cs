using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Startup.Testing
{
	sealed class LogUsageOfApplicationTaskTest : BackgroundApplicationStartupTaskTest<LogUsageOfApplicationTask>
	{
		public override int DefaultErrorExitCode => ExitCodes.LogUsageOfApplicationTaskError;
	}
}
