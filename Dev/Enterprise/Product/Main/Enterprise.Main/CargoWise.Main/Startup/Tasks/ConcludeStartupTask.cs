using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup.Tasks
{
	class ConcludeStartupTask : AbstractApplicationStartupTask
	{
		protected override bool DoExecute(CommandLineArguments arguments)
		{
			return true;
		}

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return true;
		}

		public override string TaskDescription => (NoResString)"All startup tasks have been successfully completed";

		public override int FailureExitCode => ExitCodes.Success;
	}
}
