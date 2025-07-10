using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public class ValidatePurgeDataRunStatus : AbstractApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("ad51c698-5ce7-46b0-b66f-eeda69dfef87", "Checking Purge Status");

		public override int FailureExitCode => ExitCodes.ValidatePurgeDataRunStatusError;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			if (Env.Registry.PurgeDataRunStatusFlag == Enterprise.Core.Constants.PurgeDataRunStatusFlagCodes.Completed)
			{
				return true;
			}
			if (Env.Registry.PurgeDataRunStatusFlag == Enterprise.Core.Constants.PurgeDataRunStatusFlagCodes.Error)
			{
				Globals.Message.ShowError(Res.GetString("71497f34-45f6-4b55-ac44-04c701bfeff2", "There was an error when purging data. Please restore the database from the latest backup."));
			}
			if (Env.Registry.PurgeDataRunStatusFlag == Enterprise.Core.Constants.PurgeDataRunStatusFlagCodes.Running)
			{
				Globals.Message.ShowError(Res.GetString("13f91afc-b5aa-4f0c-ba0f-7d5c337c1dd4", "A Data Purge is in progress. Please wait until the Data Purge is completed."));
			}

			return false;
		}
	}
}
