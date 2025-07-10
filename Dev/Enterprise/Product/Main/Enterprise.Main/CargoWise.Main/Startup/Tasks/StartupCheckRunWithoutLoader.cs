using CargoWise.BrandManager;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class StartupCheckRunWithoutLoader : InitializingApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("29d02f45-f2f4-4b03-bdf2-d32f1f753d34", "Initializing");

		public override int FailureExitCode => ExitCodes.StartupCheckRunWithoutLoaderError;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			return IsAllowedToRunWithoutLoader((bool)arguments[ApplicationArguments.OptionRunWithoutLoader]);
		}

		bool IsAllowedToRunWithoutLoader(bool runEnterpriseWithoutLoader)
		{
			bool result = true;
			if (!Globals.IsDebugMode && !runEnterpriseWithoutLoader)
			{
				result = false;
				StartupNotification.ShowError(Res.GetString("5ACEA761-0CD3-4CF3-AA71-40D86EA79F66", "Application executable cannot be run directly.\r\nPlease run CargoWise.Start.exe instead.\r\n{0} will now close.", BrandingFactory.Instance.ProductName),
					Res.GetString("137a06dc-feee-4978-b943-1ecfbbc951d9", "Error Executing {0}", BrandingFactory.Instance.ProductName));
			}
			return result;
		}
	}
}
