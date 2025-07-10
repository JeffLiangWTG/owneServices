using System;
using System.Windows.Forms;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	#region MockSetupStartupDirector
	class MockSetupStartupDirector : SetupStartupDirector
	{
		public MockSetupStartupDirector()
		{
		}

		public event EventHandler<ShowSettingsEventArgs> SettingsDialogShown;

		protected override Configuration GetNewConfiguration()
		{
			return new SetupConfiguration();
		}

		protected override DialogResult ShowSettingsDialog()
		{
			DialogResult result;
			EventHandler<ShowSettingsEventArgs> settingsDialogShown = SettingsDialogShown;
			if (settingsDialogShown == null)
			{
				result = DialogResult.OK;
			}
			else
			{
				ShowSettingsEventArgs e = new ShowSettingsEventArgs(Configuration.InstallationSettings);
				settingsDialogShown(this, e);
				result = e.Result;
			}
			return result;
		}

		#region ShowSettingsEventArgs

		public class ShowSettingsEventArgs : EventArgs
		{
			public ShowSettingsEventArgs(InstallationSettings settings)
			{
				Settings = settings;
			}

			public DialogResult Result { get; set; }
			public InstallationSettings Settings { get; private set; }
		}

		#endregion
	}

	#endregion

	class SetupStartupDirectorTest : StartupDirectorTestCase<MockSetupStartupDirector>
	{
		IDisposable adminCheckerOverride;

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			Application.ConfigureApplicationServices();
		}

		protected override void SetUp()
		{
			base.SetUp();
			adminCheckerOverride = AdministratorChecker.OverrideForTest(true);
		}

		protected override void TearDown()
		{
			adminCheckerOverride.Dispose();
			base.TearDown();
		}

		[GuiTest]
		public void TestInstallationItemsWithExistingSqlInstance()
		{
			Director.SettingsDialogShown += (object sender, MockSetupStartupDirector.ShowSettingsEventArgs e) =>
			{
				//AssertEquals("Settings.CDPath", installPath, e.Settings.CDPath);
				e.Settings.SelectedDatabase = new DatabaseChoice(null);
				e.Result = DialogResult.OK;
			};

			AssertEquals("Initialize()", true, Director.Initialize(Array.Empty<string>()));
			AssertEquals("TopLevelItem.GetType()", typeof(ServerInstallationTopLevelItem), Director.TopLevelItem.GetType());
			AssertArrayEqualsByElements(new Type[] {
				typeof(WriteContinuationFile),
				typeof(TerminalServerInstallMode),
				typeof(Installation),
				typeof(DownloadPackage),
				typeof(DatabaseInstaller),
				typeof(InstanceInstaller),
				typeof(UploadPackage),
				typeof(DeleteContinuationFile),
			},
			Array.ConvertAll(Director.TopLevelItem.Dependencies.ToArray(), dependency => dependency.GetType()));
		}

		public void TestSetupCanceled()
		{
			Director.SettingsDialogShown += (object sender, MockSetupStartupDirector.ShowSettingsEventArgs e) =>
			{
				e.Result = DialogResult.Cancel;
			};
			AssertEquals("Initialize()", false, Director.Initialize(null));
		}

		[GuiTest]
		public override void TestTopLevelItemDependsOnEntireInstallation()
		{
			Director.Initialize(Array.Empty<string>());
			Assert("configuration should not have error.", !Director.Configuration.HasErrors);
			AssertNotNull("toplevelitem should not be null.", Director.TopLevelItem);
			Assert(Director.TopLevelItem.Dependencies.Contains(Director.TopLevelItem.Installation));
		}
	}
}
