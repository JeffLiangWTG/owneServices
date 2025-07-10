using CargoWise.BrandManager;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class StartupInitShowInSystray : InitializingApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("06c7eddf-e506-4299-b35b-b3aeebd9bfcc", "Show {0} in System Tray", BrandingFactory.Instance.ProductName);

		public override int FailureExitCode => ExitCodes.StartupInitShowInSystrayError;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			if ((bool)arguments[ApplicationArguments.OptionShowInSystemTray])
			{
				((WinFormsEnvironment)Env.Instance).ShowInSystemTray = true;
			}
			return true;
		}
	}
}
