using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	class ExitApplicationTask : AbstractApplicationStartupTask
	{
		protected override bool DoExecute(CommandLineArguments arguments) => false;

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;

		public override string TaskDescription => (NoResString)"Exiting";

		public override int FailureExitCode => ExitCodes.Success;
	}
}
