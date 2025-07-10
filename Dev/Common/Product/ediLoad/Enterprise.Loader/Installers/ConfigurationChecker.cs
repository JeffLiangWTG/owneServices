using CargoWise.Loader.Common;

namespace Enterprise.Loader
{
	class ConfigurationChecker : InstallationItem
	{
		public ConfigurationChecker(Installation installation)
			: base(installation)
		{ }

		protected override bool NeedsToInstallCore()
		{
			return string.IsNullOrEmpty(((EnterpriseConfiguration)Installation.Configuration).ServerName) || string.IsNullOrEmpty(((EnterpriseConfiguration)Installation.Configuration).DatabaseName);
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			var instanceName = ((EnterpriseConfiguration)Installation.Configuration).InstanceName;
			if (!string.IsNullOrEmpty(instanceName))
			{
				ChangeCurrentTaskDescription("Resolving instance");
				var instance = Installation.Configuration.Services.CargoWiseOneInstanceClass.FindInstance(instanceName);
				if (instance != null)
				{
					((EnterpriseConfiguration)Installation.Configuration).ServerName = instance.ServerName;
					((EnterpriseConfiguration)Installation.Configuration).DatabaseName = instance.DatabaseName;
					return InstallationResult.OK();
				}
				else
				{
					return InstallationResult.Error("The specified instance \"" + instanceName + "\" could not be found on the current domain.");
				}
			}
			else
			{
				if (Installation.Configuration.UILevel == UILevel.Normal)
				{
					using (var dialog = new ConfigurationDialog(Installation))
					{
						Installation.Configuration.Services.MessageBox.ShowDialog(dialog);
					}
				}
				return NeedsToInstallCore() ? InstallationResult.Error("An instance must be specified.") : InstallationResult.OK();
			}
		}
	}
}
