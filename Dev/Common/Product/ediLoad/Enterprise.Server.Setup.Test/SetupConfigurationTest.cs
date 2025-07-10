using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using CargoWise.Loader.Common;
using Enterprise.Loader.Testing;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	[GuiTest] // Configuration behaviour changes based on Environment.UserInteractive
	class SetupConfigurationTest : TestCase
	{
		IDisposable adminCheckDisposable;
		SetupConfiguration configuration;
		MockRepository mocker;
		MockServiceContainer services;

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			Application.ConfigureApplicationServices();
		}

		protected override void SetUp()
		{
			base.SetUp();
			adminCheckDisposable = AdministratorChecker.OverrideForTest(true);
			mocker = new MockRepository(MockBehavior.Loose);
			services = new MockServiceContainer(mocker);
			services.PopulateWithRealServices();
			services.EventLog = mocker.Create<IEventLogProxy>(MockBehavior.Strict).Object;
			services.MessageBox = mocker.Create<IMessageBoxProxy>(MockBehavior.Strict).Object;
			ResetConfiguration();
		}

		protected override void TearDown()
		{
			adminCheckDisposable.Dispose();
			base.TearDown();
		}

		public void TestAutomatedModeInvokesValidation()
		{
			configuration.Initialize(new string[] { "-LicenseCode:0" });
			mocker.VerifyAll();
			AssertEquals("HasErrors", false, configuration.HasErrors);

			ResetConfiguration();
			ExpectError("Please enter a valid Product Key", "Error");
			configuration.Initialize(new string[] { "-LicenseCode:0", "-Automated" });
			mocker.VerifyAll();
			AssertEquals("HasErrors", true, configuration.HasErrors);
		}

		public void TestDataPathArgument()
		{
			configuration.Initialize(new string[] { "-DataPath:dp" });
			AssertEquals("DataPath", "dp", configuration.InstallationSettings.DataPath);
		}

		public void TestDbNameArgument()
		{
			configuration.Initialize(new string[] { "-DbName:dbn" });
			AssertEquals("DbName", "dbn", configuration.InstallationSettings.DbName);
			AssertEquals("UseDefaultDbName", false, configuration.InstallationSettings.UseDefaultDbName);
		}

		public void TestDuplicateArgument()
		{
			ExpectError("The argument 'DbName' has been specified more than once.", "Duplicate Argument");
			configuration.Initialize(new string[] { "-DbName:moo", "-Automated", "-DbName:oink" });
			mocker.VerifyAll();
			AssertEquals("HasErrors", true, configuration.HasErrors);
		}

		public void TestInvalidPropertyArgument()
		{
			TestHandleInvalidArgument(new string[] { "-DbName:moo", "-Goober:oink" });
		}

		public void TestInvalidSwitchArgument()
		{
			TestHandleInvalidArgument(new string[] { "-DbName:moo", "/Curry" });
		}

		public void TestLogPathArgument()
		{
			configuration.Initialize(new string[] { "-LogPath:lp" });
			AssertEquals("LogPath", "lp", configuration.InstallationSettings.LogPath);
		}

		public void TestSqlInstanceArgumentValid()
		{
			PopulateTestDatabaseList();
			configuration.Initialize(new string[] { "-SqlInstance:sql2" });
			AssertEquals("HasErrors", false, configuration.HasErrors);
			AssertEquals("SelectedDatabase", configuration.InstallationSettings.DatabaseList[1], configuration.InstallationSettings.SelectedDatabase);
		}

		public void TestSqlInstanceArgumentInvalid()
		{
			PopulateTestDatabaseList();
			configuration.Initialize(new string[] { "-SqlInstance:sql3" });
			mocker.VerifyAll();
			AssertEquals("HasErrors", false, configuration.HasErrors);
			AssertEquals("SelectedDatabase", configuration.InstallationSettings.DatabaseList[0], configuration.InstallationSettings.SelectedDatabase);

			ResetConfiguration();
			ExpectError("The specified SQL Server instance does not exist.", "Error");
			configuration.Initialize(new string[] { "-Automated", "-SqlInstance:sql3" });
			mocker.VerifyAll();
			AssertEquals("HasErrors", true, configuration.HasErrors);
			AssertEquals("SelectedDatabase", configuration.InstallationSettings.DatabaseList[0], configuration.InstallationSettings.SelectedDatabase);
		}

		public void TestTargetPath()
		{
			using (TempDirectory tempDirectory = new TempDirectory())
			{
				services.Environment = mocker.Create<IEnvironmentProxy>(MockBehavior.Loose).Object;
				Mock.Get(services.Environment).Setup(m => m.GetFolderPath(Environment.SpecialFolder.ProgramFiles)).Returns(tempDirectory);
				AssertEquals("TargetPath", Path.Combine(tempDirectory, @"WiseTech Global\CargoWise"), configuration.TargetPath);
				mocker.VerifyAll();
			}
		}

		public void TestUserIsNotAnAdmin()
		{
			adminCheckDisposable.Dispose();
			using (AdministratorChecker.OverrideForTest(false))
			{
				ExpectError("You must be an administrator on this computer to install CargoWise.", "Error");
				configuration.Initialize(new string[] { "-RelaunchedElevated" });
				mocker.VerifyAll();
				AssertEquals("HasErrors", true, configuration.HasErrors);
			}
		}

		[TestRequiresAdministrativePrivileges("Registry access")]
		public void TestRegisterRunOnReboot()
		{
			configuration.RegisterRunOnReboot();
			try
			{
				using (var runOnceKey = Registry.CurrentUser.OpenSubKey(SetupConfiguration.RunOnceRegistryKey))
				{
					string value = (string)runOnceKey.GetValue(configuration.ApplicationName);
					AssertNotNull(value);
					Assert("value.Contains(\"setup.exe\")", value.Contains("setup.exe"));
				}
			}
			finally
			{
				configuration.UnRegisterRunOnReboot();
			}

			using (var runOnceKey = Registry.CurrentUser.OpenSubKey(SetupConfiguration.RunOnceRegistryKey))
			{
				AssertNull(runOnceKey.GetValue(configuration.ApplicationName));
			}
		}

		public void TestContinuationFile()
		{
			configuration.InstallationSettings.UseDefaultDbName = false;
			configuration.InstallationSettings.DbName = "MYDB";
			configuration.InstallationSettings.WriteContinuationFile();
			try
			{
				configuration.InstallationSettings.UseDefaultDbName = true;
				configuration.InstallationSettings.DbName = "";

				AssertEquals(true, configuration.LoadContinuationFile());
				AssertEquals(false, configuration.InstallationSettings.UseDefaultDbName);
				AssertEquals("MYDB", configuration.InstallationSettings.DbName);
			}
			finally
			{
				configuration.InstallationSettings.DeleteContinuationFile();
			}
			AssertEquals(false, configuration.LoadContinuationFile());
		}

		public void TestSkipEmptyPropertiesWrittenToContinuationFile()
		{
			configuration.InstallationSettings.InstanceName = "";
			configuration.InstallationSettings.WriteContinuationFile();
			try
			{
				AssertEquals(true, configuration.LoadContinuationFile());
				AssertEquals("HasErrors", false, configuration.HasErrors);
				AssertEquals("", configuration.InstallationSettings.InstanceName);
				AssertEquals("SelectedDatabase", configuration.InstallationSettings.DatabaseList[0], configuration.InstallationSettings.SelectedDatabase);
			}
			finally
			{
				configuration.InstallationSettings.DeleteContinuationFile();
			}
			AssertEquals(false, configuration.LoadContinuationFile());
		}

		void ExpectError(string text, string caption)
		{
			Mock.Get(services.MessageBox).Setup(m => m.Show(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error)).Returns(DialogResult.None);
		}

		void PopulateTestDatabaseList()
		{
			configuration.InstallationSettings.DatabaseList.Clear();
			configuration.InstallationSettings.DatabaseList.Add(new DatabaseChoice("sql1"));
			configuration.InstallationSettings.DatabaseList.Add(new DatabaseChoice("sql2"));
		}

		void ResetConfiguration()
		{
			configuration = new SetupConfiguration { Services = services };
		}

		void TestHandleInvalidArgument(string[] args)
		{
			ExpectError(configuration.GetHelpMessage(true), "Invalid Arguments");
			configuration.Initialize(args);
			mocker.VerifyAll();
			AssertEquals("HasErrors", true, configuration.HasErrors);
		}
	}
}
