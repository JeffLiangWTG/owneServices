using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using Enterprise.Client.Common;
using Enterprise.URLHandler;

namespace Enterprise.Loader
{
	class EnterpriseStartupDirector : StartupDirector
	{
		public EnterpriseStartupDirector()
		{
		}

		public new EnterpriseConfiguration Configuration
		{
			get { return (EnterpriseConfiguration)base.Configuration; }
		}

		protected override void Run()
		{
			if (Configuration.EdiEntUrl != null)
			{
				GetEnerpriseUrlHandlerClient().ExecuteUrlOnRunningEnterpriseProcessOrSpawnNew(Configuration.EdiEntUrl);
			}
			else
			{
				base.Run();
			}
		}

		/// <summary>
		/// Startup tasks are run every time Enterprise is launched
		/// </summary>
		static void AddStartupTasks(InstallationItem parentItem)
		{
			ClientInstallation installation = (ClientInstallation)parentItem.Installation;
			parentItem.AddDependency(new RemoveUserSpecificEdiUrlRegistration(installation));
		}

		protected override Configuration GetNewConfiguration()
		{
			return new EnterpriseConfiguration();
		}

		protected override bool InitializeInstallationItems()
		{
			var installation = new ClientInstallation(Configuration);
			TopLevelItem = InitializeTopLevel(Configuration, installation);
			return true;
		}

		static InstallationItem InitializeTopLevel(EnterpriseConfiguration configuration, ClientInstallation clientInstallation)
		{
			if (configuration.Uninstall)
			{
				return new Uninstaller(clientInstallation);
			}

			var mainInstallContainer = configuration.InstallOnly
				? new InstallationItemContainer(clientInstallation)
				: (InstallationItem)new InstallationProgramFromVersionedFile(clientInstallation) { WaitForExitInBackground = configuration.KeepStartRunning };

			AddDependencies(configuration, clientInstallation, mainInstallContainer);

			var topLevelItem = new InstallationItemContainer(clientInstallation);

			// In some cases (e.g. DAT build deployment), we expect OldVersionRemover runs prior to installation
			// so that we can release more disk space before installation.
			//
			// As the cleanup takes time and may clean already-installed but not-used-recently CW1 applications,
			// we don't expect the cleanup to run in front of installation when we only pass -RemoveOldVersions argument.
			// We have to specify -ForceRemoveOldVersionsInFrontOfInstallation argument additionally to make it work.
			if (configuration.RemoveOldVersions && configuration.ForceRemoveOldVersionsInFrontOfInstallation)
			{
				topLevelItem.AddDependency(new OldVersionsRemover(clientInstallation, silentlyContinueOnError: true));
			}

			topLevelItem.AddDependency(mainInstallContainer);

			if (configuration.UpdateUsageLog)
			{
				topLevelItem.AddDependency(new ApplicationUsageLogFileUpdater(clientInstallation, configuration.ServerName, configuration.DatabaseName));
			}
			if (configuration.RemoveOldVersions && !configuration.ForceRemoveOldVersionsInFrontOfInstallation)
			{
				topLevelItem.AddDependency(new OldVersionsRemover(clientInstallation, silentlyContinueOnError: true));
			}

			return topLevelItem;
		}

		static void AddDependencies(EnterpriseConfiguration configuration, ClientInstallation installation, InstallationItem parent)
		{
			parent.AddDependency(new ProgramLocationChecker(installation));
			parent.AddDependency(new LegacyMsiRemover(installation));
			parent.AddDependency(new ConfigurationChecker(installation));
			parent.AddDependency(installation);

			if (configuration.Repair)
			{
				parent.AddDependency(new RepairInstaller(installation));
			}
			else
			{
				parent.AddDependency(new CurrentVersionInstaller(installation, parent as InstallationProgramFromVersionedFile));
			}
			parent.AddDependency(new IconCreator(installation, typeof(IconCreationDialog)));
			parent.AddDependency(new WindowsRegistryInstaller(installation));
			parent.AddDependency(new EventSourceCreator(installation));

			AddStartupTasks(parent);
		}

		protected virtual bool ElevationRequired(UserAccountControl uac)
		{
			return uac.ElevationRequired;
		}

		protected virtual EnterpriseUrlHandlerClient GetEnerpriseUrlHandlerClient()
		{
			return new EnterpriseUrlHandlerClient();
		}
	}
}

