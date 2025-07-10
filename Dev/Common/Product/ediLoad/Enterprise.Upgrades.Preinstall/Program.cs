using System;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;

namespace Enterprise.Upgrades.Preinstall
{
	/// <summary>
	/// Tasks to run after an upgrade has been extracted to a temp directory but before copied to the Program Files directory
	/// </summary>
	class Program : StartupDirector
	{
		protected override Configuration GetNewConfiguration()
		{
			return new EnterpriseUpgradePreinstallConfiguration();
		}

		protected override bool InitializeInstallationItems()
		{
			Installation installation = new Installation(Configuration);
			TopLevelItem = new InstallationItemContainer(installation);
			AddDependencies();
			return true;
		}

		protected internal virtual void AddDependencies()
		{
			TopLevelItem.AddDependency(new AppManagerInstaller(TopLevelItem.Installation));
		}

		[STAThread]
		static int Main(string[] args)
		{
			Program program = new Program();
			program.StartApplication(args);
			return program.Configuration.ReturnCode == ReturnCode.Success ? 0 : -1;
		}
	}
}
