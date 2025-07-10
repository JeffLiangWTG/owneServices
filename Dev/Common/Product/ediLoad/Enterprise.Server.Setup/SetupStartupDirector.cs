using System;
using System.Windows.Forms;
using CargoWise.Loader.Common;
using Microsoft.Win32;

namespace Enterprise.Server.Setup
{
	class SetupStartupDirector : StartupDirector
	{
		public SetupStartupDirector()
		{
		}

		public new SetupConfiguration Configuration
		{
			get { return (SetupConfiguration)base.Configuration; }
		}

		protected override Configuration GetNewConfiguration()
		{
			return new SetupConfiguration();
		}

		protected override bool InitializeInstallationItems()
		{
			bool result = !Configuration.HasErrors;
			if (result)
			{
				if ((Configuration.UILevel != UILevel.Normal) || (result = (ShowSettingsDialog() == DialogResult.OK)))
				{
					InitializeSetup(Configuration.InstallationSettings);
				}
			}
			return result;
		}

		protected virtual DialogResult ShowSettingsDialog()
		{
			using (InstallationSettingsForm settingsForm = new InstallationSettingsForm(Configuration))
			{
				return settingsForm.ShowDialog();
			}
		}

		void InitializeSetup(InstallationSettings installationSettings)
		{
			SetupConfiguration configurationToInstall = new SetupConfiguration();
			Installation installation = new Installation(configurationToInstall);
			TopLevelItem = new ServerInstallationTopLevelItem(installation, installationSettings);
			TopLevelItem.AddDependency(new WriteContinuationFile(installation, installationSettings));
			TopLevelItem.AddDependency(new TerminalServerInstallMode(installation));
			TopLevelItem.AddDependency(installation);
			TopLevelItem.AddDependency(new DownloadPackage(installation, installationSettings));
			TopLevelItem.AddDependency(new DatabaseInstaller(installation, installationSettings));
			TopLevelItem.AddDependency(new InstanceInstaller(installation, installationSettings));
			TopLevelItem.AddDependency(new UploadPackage(installation, installationSettings));
			TopLevelItem.AddDependency(new DeleteContinuationFile(installation, installationSettings));
		}

		protected override void StartInstall()
		{
			SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
			try
			{
				base.StartInstall();
			}
			finally
			{
				SystemEvents.SessionEnding -= new SessionEndingEventHandler(SystemEvents_SessionEnding);
			}
		}

		void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
		{
			this.Configuration.RegisterRunOnReboot();
		}

		[STAThread]
		static int Main(string[] args)
		{
			AssemblyResolver.Initialize();
			Application.ConfigureApplicationServices();
			SetupStartupDirector director = new SetupStartupDirector();
			return director.StartApplication(args);
		}
	}
}

