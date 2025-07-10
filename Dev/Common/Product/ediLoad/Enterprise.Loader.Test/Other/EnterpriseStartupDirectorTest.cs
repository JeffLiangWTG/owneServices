using System;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Loader.Client;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Enterprise.Client.Common;
using Enterprise.Upgrades;
using Enterprise.URLHandler;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Loader.Testing
{
	#region MockEnterpriseStartupDirector
	class MockEnterpriseStartupDirector : EnterpriseStartupDirector
	{
		public MockEnterpriseStartupDirector()
		{
		}

		public MockServiceContainer Services { get; set; }

		public int UserInterfaceCalls { get; private set; }

		protected override bool InitializeInstallationItems()
		{
			Configuration.Services = Services;
			return base.InitializeInstallationItems();
		}

		protected override void StartUserInterface()
		{
			UserInterfaceCalls++;
		}

		protected override bool ElevationRequired(UserAccountControl uac)
		{
			return false;
		}

		protected override Configuration GetNewConfiguration()
		{
			return new MockEnterpriseConfiguration();
		}

		protected override EnterpriseUrlHandlerClient GetEnerpriseUrlHandlerClient()
		{
			return EnterpriseUrlHandlerClient;
		}

		public EnterpriseUrlHandlerClient EnterpriseUrlHandlerClient;
	}

	#endregion

	[GuiTest]
	class EnterpriseStartupDirectorTest : StartupDirectorTestCase<MockEnterpriseStartupDirector>
	{
		MockRepository mocker;
		MockServiceContainer services;

		protected override MockEnterpriseStartupDirector GetNewDirector()
		{
			MockEnterpriseStartupDirector result = base.GetNewDirector();
			result.Services = Services;
			return result;
		}

		public void TestPrerequisitesNormal()
		{
			Director.Initialize(new[] { EnterpriseConfiguration.InstallCurrentVersionArgument });
			AssertEquals("Should have an icon creator.", 1, GetTopLevelItemCountForType(typeof(IconCreator)));
			AssertEquals("Should have an Edi Url handler registry item.", 1, GetTopLevelItemCountForType(typeof(RemoveUserSpecificEdiUrlRegistration)));
			AssertEquals("Should have WindowsRegistryInstaller.", 1, GetTopLevelItemCountForType(typeof(WindowsRegistryInstaller)));
		}

		public void TestTopLevelItemForEnterprise()
		{
			var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			var testPathCargoWiseWindowsDesktopExe = Path.Combine(tempDir, ExeFileNames.CargoWiseWindowsDesktopExe);

			Directory.CreateDirectory(tempDir);
			try
			{
				Director.Initialize(null);
				Director.Configuration.BaseTargetPath = tempDir;

				Assert("Top Level item should be a container", Director.TopLevelItem is InstallationItemContainer);
				Assert("Top Level item have only one dependencies", Director.TopLevelItem.Dependencies.Count == 1);

				InstallationProgramFromFile program = (InstallationProgramFromFile)Director.TopLevelItem.Dependencies[0];
				using (File.Create(testPathCargoWiseWindowsDesktopExe))
				{
					Assert(program.FullPathOfProgramToRun.EndsWith(ExeFileNames.CargoWiseWindowsDesktopExe));
				}
				Assert("Should wait for input idle.", program.WaitForInputIdle);
				AssertEquals(program.Arguments, ((EnterpriseConfiguration)program.Installation.Configuration).ProgramArguments);
			}
			finally
			{
				Directory.Delete(tempDir, recursive: true);
			}
		}

		public void TestDefaultInstallDependencies()
		{
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.InstallCurrentVersionArgument
			});

			Assert($"Top item should be type of {nameof(InstallationItemContainer)}", Director.TopLevelItem is InstallationItemContainer);

			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(InstallationProgramFromVersionedFile),
				});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestShouldContainRepairInstallerWhenWithRepairArgument()
		{
			//Arrange & Act
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.RepairArgument,
			});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(RepairInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestShouldContainCurrentVersionInstallerWhenWithoutRepairArgument()
		{
			//Arrange & Act
			Director.Initialize(null);

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestDefaultInstallDependenciesWithInstallOnly()
		{
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.InstallCurrentVersionArgument,
				EnterpriseConfiguration.InstallOnlyArgument,
			});

			Assert($"Top item should be type of {nameof(InstallationItemContainer)}", Director.TopLevelItem is InstallationItemContainer);

			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(InstallationItemContainer),
				});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestInstallWithUsageUpdatator()
		{
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.InstallCurrentVersionArgument,
				EnterpriseConfiguration.UpdateUsageLogArgument,
			});
			Assert($"Top item should be type of {nameof(InstallationItemContainer)}", Director.TopLevelItem is InstallationItemContainer);

			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(InstallationProgramFromVersionedFile),
					typeof(ApplicationUsageLogFileUpdater),
				});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestInstallWithOldVersionRemoval()
		{
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.InstallCurrentVersionArgument,
				EnterpriseConfiguration.RemoveOldVersionsArgument,
			});
			Assert($"Top item should be type of {nameof(InstallationItemContainer)}", Director.TopLevelItem is InstallationItemContainer);

			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(InstallationProgramFromVersionedFile),
					typeof(OldVersionsRemover),
				});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestInstallWithUsageUpdatatorAndOldVersionRemoval()
		{
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.InstallCurrentVersionArgument,
				EnterpriseConfiguration.UpdateUsageLogArgument,
				EnterpriseConfiguration.RemoveOldVersionsArgument,
			});
			Assert($"Top item should be type of {nameof(InstallationItemContainer)}", Director.TopLevelItem is InstallationItemContainer);

			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(InstallationProgramFromVersionedFile),
					typeof(ApplicationUsageLogFileUpdater),
					typeof(OldVersionsRemover),
				});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestInstallWithOldVersionRemovalWithInstallOnly()
		{
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.RemoveOldVersionsArgument,
				EnterpriseConfiguration.InstallOnlyArgument,
				EnterpriseConfiguration.InstallCurrentVersionArgument,
			});
			Assert($"Top item should be type of {nameof(InstallationItemContainer)}", Director.TopLevelItem is InstallationItemContainer);

			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[] {
					typeof(InstallationItemContainer),
					typeof(OldVersionsRemover),
				});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestInstallWithUsageUpdatatorAndOldVersionRemovalWithInstallOnly()
		{
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.RemoveOldVersionsArgument,
				EnterpriseConfiguration.InstallOnlyArgument,
				EnterpriseConfiguration.InstallCurrentVersionArgument,
				EnterpriseConfiguration.UpdateUsageLogArgument,
			});
			Assert($"Top item should be type of {nameof(InstallationItemContainer)}", Director.TopLevelItem is InstallationItemContainer);

			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(InstallationItemContainer),
					typeof(ApplicationUsageLogFileUpdater),
					typeof(OldVersionsRemover),
				});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		public void TestTypicalInstallForWinzorVersionBloker()
		{
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.UpdateUsageLogArgument,
				EnterpriseConfiguration.RemoveOldVersionsArgument,
				Configuration.NoUIArgument,
				Configuration.AutomatedArgument,
				EnterpriseConfiguration.InstallCurrentVersionArgument,
			});

			Assert($"Top item should be type of {nameof(InstallationItemContainer)}", Director.TopLevelItem is InstallationItemContainer);

			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(InstallationProgramFromVersionedFile),
					typeof(ApplicationUsageLogFileUpdater),
					typeof(OldVersionsRemover),
				});

			AssertTypesInDependencies(
				Director.TopLevelItem.Dependencies[0],
				new[]
				{
					typeof(ProgramLocationChecker),
					typeof(LegacyMsiRemover),
					typeof(ConfigurationChecker),
					typeof(ClientInstallation),
					typeof(CurrentVersionInstaller),
					typeof(IconCreator),
					typeof(WindowsRegistryInstaller),
					typeof(EventSourceCreator),
					typeof(RemoveUserSpecificEdiUrlRegistration),
				});
		}

		static void AssertTypesInDependencies(InstallationItem parentItem, Type[] expectedTypes)
		{
			var dependencies = parentItem.Dependencies
				.Cast<InstallationItem>()
				.ToList();
			for (var i = 0; i < dependencies.Count; i++)
			{
				Assert(
					$"{parentItem.GetType().Name}.Dependencies[{i}] should be {expectedTypes[i].Name}, but got {dependencies[i].GetType().Name}",
					dependencies[i].GetType() == expectedTypes[i]);
			}

			Assert(
				message: $"Expect dependencies for {parentItem.GetType().Name} :"
					+ string.Join(" ", expectedTypes.Select(t => "\r\n -" + t.Name))
					+ $"\r\n\r\nBut got:"
					+ string.Join(" ", dependencies.Select(t => "\r\n -" + t.GetType().Name)),
				condition: expectedTypes.Length == dependencies.Count);
		}

		public void TestFirstDependencyIsLocationChecker()
		{
			Director.Initialize(null);
			AssertType(typeof(ProgramLocationChecker), Director.TopLevelItem.Dependencies[0].Dependencies[0]);
		}

		public void TestUpdateFlagIgnoresMaintenanceMessage()
		{
			using (TempDirectory dir = new TempDirectory())
			{
				using (StreamWriter configFile = File.CreateText(Path.Combine(dir, ConfigFile.FileName)))
				{
					configFile.WriteLine("MaintenanceMessage=System is being upgraded.");
				}
				var args = new[] { "-update" };
				Director.StartApplication(args);
				Mocker.VerifyAll();
			}
			AssertEquals("Should have told the UI to start.", 1, Director.UserInterfaceCalls);
		}

		public void TestUninstall()
		{
			Director.Initialize(new[] { EnterpriseConfiguration.UninstallArgument });
			AssertType(typeof(Uninstaller), Director.TopLevelItem);
			AssertEquals("Top Level item have three dependencies", 2, Director.TopLevelItem.Dependencies.Count);
		}

		public void TestKeepStartRunningOption()
		{
			Director.Initialize(new[] { EnterpriseConfiguration.KeepStartRunningArgument });
			var program = (InstallationProgramFromFile)Director.TopLevelItem.Dependencies[0];
			AssertEquals("RunInBackground", true, program.WaitForExitInBackground);
		}

		public void TestEdiEntUrlIsHandled()
		{
			var director = GetNewDirector();
			var mockClient = new Mock<EnterpriseUrlHandlerClient>();
			director.EnterpriseUrlHandlerClient = mockClient.Object;
			var exceptionMessage = "asdfasdf";
			mockClient.Protected().Setup("ExecuteUrlOnPublishedEnterpriseProcess", ItExpr.IsAny<string>(), "edient:Command=DoSomething&Parameter=a9b3c019-d4f3-4f8b-bd10-b9a5f9045b03", ItExpr.Ref<bool>.IsAny).Throws(new Exception(exceptionMessage));
			AssertNoExceptionThrown(() => director.StartApplication(new[] { "edient:Command=DoSomething&Parameter=a9b3c019-d4f3-4f8b-bd10-b9a5f9045b03" }));
			mockClient.Protected().Verify("ShowMessage", Times.Once(), exceptionMessage);
		}

		public void TestInstallOnlyArgument()
		{
			Director.Initialize(new[] { EnterpriseConfiguration.InstallOnlyArgument });
			AssertType(typeof(InstallationItemContainer), Director.TopLevelItem);
		}

		public override void TestTopLevelItemDependsOnEntireInstallation()
		{
			Director.Initialize(null);
			Assert("Top Level item should be a container", Director.TopLevelItem is InstallationItemContainer);
			Assert("Top Level item have only one dependencies", Director.TopLevelItem.Dependencies.Count == 1);
			Assert(Director.TopLevelItem.Dependencies[0].Dependencies.Contains(Director.TopLevelItem.Installation));
		}

		public void TestOldVersionsRemoverIsRunAfterInstallationIfRemoveOldVersionsArgumentIsSpecified()
		{
			// Arrange
			// Act
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.RemoveOldVersionsArgument,
				EnterpriseConfiguration.InstallCurrentVersionArgument,
			});

			// Assert
			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(InstallationProgramFromVersionedFile),
					typeof(OldVersionsRemover), // after installation
				});
		}

		public void TestOldVersionsRemoverIsRunBeforeInstallationIfForceRemoveOldVersionsInFrontOfInstallationArgumentIsSpecified()
		{
			// Arrange
			// Act
			Director.Initialize(new[]
			{
				EnterpriseConfiguration.RemoveOldVersionsArgument,
				EnterpriseConfiguration.ForceRemoveOldVersionsInFrontOfInstallationArgument,
				EnterpriseConfiguration.InstallCurrentVersionArgument,
			});

			// Assert
			AssertTypesInDependencies(
				Director.TopLevelItem,
				new[]
				{
					typeof(OldVersionsRemover), // before installation
					typeof(InstallationProgramFromVersionedFile),
				});
		}

		MockRepository Mocker
		{
			get { return mocker ?? (mocker = new MockRepository(MockBehavior.Loose)); }
		}

		MockServiceContainer Services
		{
			get
			{
				if (services == null)
				{
					services = new MockServiceContainer(Mocker);
					services.PopulateWithRealServices();
					services.EventLog = mocker.Create<IEventLogProxy>(MockBehavior.Strict).Object;
					services.MessageBox = mocker.Create<IMessageBoxProxy>(MockBehavior.Strict).Object;
				}
				return services;
			}
		}
	}
}
