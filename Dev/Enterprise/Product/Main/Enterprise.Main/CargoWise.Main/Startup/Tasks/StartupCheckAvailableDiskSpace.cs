using CargoWise.BrandManager;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class StartupCheckAvailableDiskSpace : AbstractApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("801bc8df-3da2-482c-8c92-dc89f273d56f", "Checking available disk space");

		public override int FailureExitCode => ExitCodes.StartupCheckAvailableDiskSpaceError;

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			if (!ZSystemInformation.Instance.IsThereSufficientFreeDiskSpace(out var availableDiskSpace))
			{
				string message = Res.GetString("dd817dfb-2c59-4f41-a6b1-9e883b8043a9", "Your available disk space of {0}MB is not enough to run {1}. You should have at least {2}MB free on your PC.", availableDiskSpace.ToString(), BrandingFactory.Instance.ProductName, ZSystemInformation.MinimumRequirements.DiskFreeSpaceInMB);
				Globals.Message.ShowError(message);
			}
			return true;
		}
	}
}
