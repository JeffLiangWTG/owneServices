using System;
using System.IO;
using CargoWise.Loader.Common;

namespace Enterprise.Upgrades.Postinstall
{
	/// <summary>
	/// Tasks to run after an upgrade is copied to the Program Files directory
	/// </summary>
	class Program : StartupDirector
	{
		protected override Configuration GetNewConfiguration()
		{
			return new EnterpriseUpgradePostinstallConfiguration();
		}

		protected override bool InitializeInstallationItems()
		{
			Installation installation = new Installation(Configuration);
			TopLevelItem = new InstallationItemContainer(installation);
			AddDependencies();
			return true;
		}

		internal protected virtual void AddDependencies()
		{
			if (!TopLevelItem.Installation.Configuration.BaseTargetPath.StartsWith(UpgradeManager.OldBaseInstallationPath))
			{
				TopLevelItem.AddDependency(new NGenInstaller(TopLevelItem.Installation,
					TopLevelItem.Installation.Configuration.CurrentPackage, Path.Combine(TopLevelItem.Installation.Configuration.CurrentPackage, NGenInstaller.NGenRootsDllName), NGenInstaller.Action.Install));
				TopLevelItem.AddDependency(new EdiUrlRegistration(TopLevelItem.Installation));
				TopLevelItem.AddDependency(new ClientMsiInstaller(TopLevelItem.Installation));
			}
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
