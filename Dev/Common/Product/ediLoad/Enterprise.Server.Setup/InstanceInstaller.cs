using System;
using CargoWise.Common;
using CargoWise.Loader.Common;
using Enterprise.Client.Common;

namespace Enterprise.Server.Setup
{
	class InstanceInstaller : InstallationItem
	{
		public InstanceInstaller(Installation installation, InstallationSettings installationSettings)
			: base(installation)
		{
			this.installationSettings = installationSettings;
		}

		protected override bool NeedsToInstallCore()
		{
			return installationSettings.RegisterInstance;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			try
			{
				if (!Installation.Configuration.Services.CargoWiseOneInstanceClass.ExistsInCurrentSchema())
				{
					CargoWiseOneInstanceManager.CreateInCurrentSchemaWithLoginIfRequired(Installation.Configuration);
				}
				Installation.Configuration.Services.CargoWiseOneInstanceClass.AddNewInstance(installationSettings.InstanceName, installationSettings.ServerName, installationSettings.DbName);
				return InstallationResult.OK();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return InstallationResult.Warning("Instance was not registered on domain: " + ex.Message + @"\r\n\rError details:" + ex.ToString());
			}
		}

		readonly InstallationSettings installationSettings;
	}
}
