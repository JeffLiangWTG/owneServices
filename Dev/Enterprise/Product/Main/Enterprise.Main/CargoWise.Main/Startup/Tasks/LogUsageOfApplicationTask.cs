using CargoWise.Data;
using CargoWise.Definitions;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Startup
{
	public class LogUsageOfApplicationTask : BackgroundApplicationStartupTask
	{
		public override int FailureExitCode => ExitCodes.LogUsageOfApplicationTaskError;

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Path names are exempt")]
		public override void DoExecute()
		{
			new ApplicationUsageLogFile().LogUsage(new ApplicationUsageLog(Db.ServerName, Db.DatabaseName));
		}
	}
}
