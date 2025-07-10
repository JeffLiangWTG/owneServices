using CargoWise.Main.Startup.DotNetVersionSwitch;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class LaunchDefaultDotNetVersionTask : IPostLoginTask
	{
		public string TaskDescription => Res.GetString("6ec8268a-4648-4e7e-bf75-272feb915061", "Checking running .NET version");

		readonly IDotNetVersionSwitchManager SwitchManager;

		public LaunchDefaultDotNetVersionTask() : this(new DotNetVersionSwitchManager(isRunningTest: Globals.IsTest))
		{
		}

		public LaunchDefaultDotNetVersionTask(IDotNetVersionSwitchManager switchManager)
		{
			SwitchManager = switchManager;
		}

		public bool ShouldExecute()
		{
			var arguments = CommandLineArguments.UsedToLaunchApplication;

			if (arguments[ApplicationArguments.OptionSkipDotNetVersionSwitch] is not null
				&& (bool)arguments[ApplicationArguments.OptionSkipDotNetVersionSwitch])
			{
				return false;
			}

			return SwitchManager.EnforcedNetCoreVersionForUser() || SwitchManager.IsNetVersionSwitchEnabled();
		}

		public void Execute()
		{
			var defaultConfiguratedVersion = SwitchManager.GetDefaultNetVersionItem();
			if (defaultConfiguratedVersion.IsVersionCurrentRunning())
			{
				return;
			}

			defaultConfiguratedVersion.LaunchCurrentVersion();
		}
	}
}
