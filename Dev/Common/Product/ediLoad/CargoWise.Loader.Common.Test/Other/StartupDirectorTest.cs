using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.BrandManager;
using CargoWise.IO;
using CargoWise.Loader.Common.Exceptions;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	[GuiTest]
	sealed class StartupDirectorTest : TestCase
	{
		MockStartupDirector startupDirector;

		public void TestBreakIntoDebuggerArgumentIsNotPassedToConfiguration()
		{
			StartupDirector.StartApplication(new string[] { Common.StartupDirector.BreakIntoDebuggerArgument });
			AssertEquals("Configuration.UnhandledArguments.Count", 0, StartupDirector.Configuration.UnhandledArguments.Count);
		}

		public void TestErrorsFromCheckAvailability()
		{
			InstallItemMoq.Reset();
			using (var form = new AutoClickOKInstallationResultsForm())
			using (InstallationResultsForm.OverrideFactoryForTest(form))
			{
				StartupDirector.TopLevelItem = InstallItemMoq.Object;
				var results = new AddResultsPredicate(3, 3);

				InstallItemMoq.Setup(m => m.CheckAvailability(It.Is<InstallationResultCollection>((s) => results.Eval(s))));
				StartupDirector.Initialize(null);
				StartupDirector.StartInstall();
				InstallItemMoq.VerifyAll();
				AssertEquals("LastLoadedResults", results.LastValue, form.LastLoadedResults);
			}
		}

		public void TestInstallWithReducedUILevel()
		{
			var startupDirector = new MockStartupDirector();
			startupDirector.StartApplication(new string[] { Configuration.NoUIArgument });
			AssertEquals("UserInterfaceCalls", 0, startupDirector.UserInterfaceCalls);

			startupDirector = new MockStartupDirector();
			startupDirector.StartApplication(new string[] { Configuration.AutomatedArgument });
			AssertEquals("UserInterfaceCalls", 1, startupDirector.UserInterfaceCalls);
		}

		public void TestStartApplicationGuiMode()
		{
			using (var temp = new TempDirectory())
			using (var form = CW1ProgressForm.New())
			using (CW1ProgressForm.OverrideFactoryForTest(form))
			{
				string formText = "";
				form.TextChanged += (object sender, EventArgs e) => formText = form.Text;
				StartupDirector.Initialize(Array.Empty<string>());
				StartupDirector.TopLevelItem = new SlowInstallationItem(new Installation(new MockConfiguration()));
				StartupDirector.Configuration.SetApplicationName("Goober");
				StartupDirector.StartActualUserInterface = true;
				StartupDirector.StartApplication(Array.Empty<string>());
				AssertEquals("CW1ProgressForm.Text", "Goober", formText);
			}
		}

		public void TestStartApplicationGuiModeAlwaysFinishes()
		{
			var thread = new Thread(() =>
				{
					using (CW1ProgressForm.OverrideFactoryForTest(new SlowProgressForm()))
					{
						StartupDirector.TopLevelItem = null;
						StartupDirector.Initialize(null);
						StartupDirector.Configuration.SetApplicationName("Test");
						StartupDirector.StartActualUserInterface = true;
						StartupDirector.StartApplication(Array.Empty<string>());
					}
				}
			);
			thread.Start();
			Assert(thread.Join(TimeSpan.FromSeconds(1)));
		}

		class SlowProgressForm : CW1ProgressForm
		{
			protected override void CreateHandle()
			{
				Thread.Sleep(500);
				base.CreateHandle();
			}
		}

		class SlowInstallationItem : InstallationItem
		{
			public SlowInstallationItem(Installation installation)
				: base(installation)
			{ }

			protected override bool NeedsToInstallCore()
			{
				return true;
			}

			public override void Install(InstallationResultCollection results)
			{
				Thread.Sleep(500);
			}
		}

		public void TestWarningsFromCheckAvailability()
		{
			InstallItemMoq.Reset();
			using (var form = new AutoClickOKInstallationResultsForm())
			using (InstallationResultsForm.OverrideFactoryForTest(form))
			{
				StartupDirector.TopLevelItem = InstallItemMoq.Object;
				var results = new AddResultsPredicate(3, 0);
				InstallItemMoq.Setup(m => m.CheckAvailability(It.Is<InstallationResultCollection>((s) => results.Eval(s))));
				InstallItemMoq.Setup(m => m.Install(It.IsNotNull<InstallationResultCollection>()));
				StartupDirector.Initialize(null);
				StartupDirector.StartInstall();
				InstallItemMoq.VerifyAll();
				AssertEquals("LastLoadedResults", results.LastValue, form.LastLoadedResults);
			}
		}

		public void TestWarningsOrErrorsFromInstall()
		{
			InstallItemMoq.Reset();
			using (var form = new AutoClickOKInstallationResultsForm())
			using (InstallationResultsForm.OverrideFactoryForTest(form))
			{
				StartupDirector.TopLevelItem = InstallItemMoq.Object;
				var results = new AddResultsPredicate(3, 0);
				InstallItemMoq.Setup(m => m.CheckAvailability(It.IsNotNull<InstallationResultCollection>()));
				InstallItemMoq.Setup(m => m.Install(It.Is<InstallationResultCollection>((s) => results.Eval(s))));
				StartupDirector.Initialize(null);
				StartupDirector.StartInstall();
				InstallItemMoq.VerifyAll();
				AssertEquals("LastLoadedResults", results.LastValue, form.LastLoadedResults);
			}
		}

		public void TestBranding_UsingPWParameter_ShouldUpdateBrandingCorrectly()
		{
			StartupDirector.StartApplication(Array.Empty<string>());
			AssertEquals("After starting the application without a PW argument, the instance of BrandingFactory should be CW1Legacy", true, BrandingFactory.Instance is CW1LegacyBranding);

			StartupDirector.StartApplication(new string[] { "-PW" });
			AssertEquals("After starting the application with a PW argument, the instance of BrandingFactory should be ProductivityWiseBranding", true, BrandingFactory.Instance is ProductivityWiseBranding);
		}

		public void TestWarningsOrErrorsWithNoUI()
		{
			InstallItemMoq.Reset();
			var mock = new MockRepository(MockBehavior.Default);
			var services = new MoqMockServiceContainer(mock);
			StartupDirector.TopLevelItem = InstallItemMoq.Object;
			StartupDirector.Initialize(null);
			StartupDirector.Configuration.Services = services;
			var results = new AddResultsPredicate(1, 2);
			InstallItemMoq.Setup(m => m.CheckAvailability(It.IsNotNull<InstallationResultCollection>()));
			InstallItemMoq.Setup(m => m.Install(It.Is<InstallationResultCollection>((s) => results.Eval(s))));
			services.EventLogForTest.Setup(m => m.WriteEntry("CargoWise One", "-NoUI\r\nError number 0\r\n\r\nError number 1", EventLogEntryType.Error));
			services.EventLogForTest.Setup(m => m.WriteEntry("CargoWise One", "-NoUI\r\nWarning number 0", EventLogEntryType.Warning));
			StartupDirector.StartApplication(new string[] { Configuration.NoUIArgument });
			mock.VerifyAll();
			Assert("Mocks verified successfully.", true);
		}

		public void TestAllCommandLineParamsAreIncludedToErrorLogs()
		{
			InstallItemMoq.Reset();
			var args = new[] { "OdysseyTestServer.www.sand", "OdysseyTestDb", "-NoUI", "-InstallOnly", "-InstallCurrentVersion" };
			var argsString = string.Join(" ", args);

			var mock = new MockRepository(MockBehavior.Default);
			var services = new MoqMockServiceContainer(mock);
			StartupDirector.TopLevelItem = InstallItemMoq.Object;
			StartupDirector.Initialize(null);
			StartupDirector.Configuration.Services = services;
			var results = new AddResultsPredicate(1, 2);
			InstallItemMoq.Setup(m => m.CheckAvailability(It.IsNotNull<InstallationResultCollection>()));
			InstallItemMoq.Setup(m => m.Install(It.Is<InstallationResultCollection>((s) => results.Eval(s))));
			services.EventLogForTest.Setup(m => m.WriteEntry("CargoWise One", $"{argsString}\r\nError number 0\r\n\r\nError number 1", EventLogEntryType.Error));
			services.EventLogForTest.Setup(m => m.WriteEntry("CargoWise One", $"{argsString}\r\nWarning number 0", EventLogEntryType.Warning));
			StartupDirector.StartApplication(args);
			mock.VerifyAll();
			Assert("Mocks verified successfully.", true);
		}

		public void TestShowHelpDoesNotThrowException()
		{
			//Arrange
			var args = new[] { "/?" };
			StartupDirector.Initialize(args);
			StartupDirector.Configuration.ShowHelp = true;
			//Act and Assert
			AssertNoExceptionThrown("Start Application should not throw exceptions when running the help menu",
				() => StartupDirector.StartApplication(args));
		}

		public void TestConfigurationWithoutInitialization()
		{
			AssertExceptionThrown<ConfigurationNotInitializedException>("Should have the Configuration initialized first.",
				() => { var value = StartupDirector.Configuration; });
			StartupDirector.Initialize(null);
			Assert("Configuration initialized.", StartupDirector.Configuration != null);
		}

		Mock<InstallationItem> installItemMoq;
		Mock<InstallationItem> InstallItemMoq => installItemMoq ?? (installItemMoq = new Mock<InstallationItem>(new Installation(new Configuration())) { CallBase = true });

		MockStartupDirector StartupDirector
		{
			get { return startupDirector ?? (startupDirector = new MockStartupDirector()); }
		}

		#region AddResultsPredicate

		class AddResultsPredicate
		{
			readonly int errorsToAdd;
			readonly int warningsToAdd;

			public AddResultsPredicate(int warningsToAdd, int errorsToAdd)
			{
				this.warningsToAdd = warningsToAdd;
				this.errorsToAdd = errorsToAdd;
			}

			public InstallationResultCollection LastValue { get; private set; }

			public bool Eval(InstallationResultCollection currentValue)
			{
				InstallationResultCollection results = currentValue;
				if (results == null)
				{
					return false;
				}
				else
				{
					for (int i = 0; i < warningsToAdd; i++)
					{
						results.Add(InstallationResult.Warning("Warning number " + i));
					}

					for (int i = 0; i < errorsToAdd; i++)
					{
						results.Add(InstallationResult.Error("Error number " + i));
					}
					LastValue = results;
					return true;
				}
			}
		}

		#endregion

		#region AutoClickOKInstallationResultsForm

		class AutoClickOKInstallationResultsForm : InstallationResultsForm
		{
			public InstallationResultCollection LastLoadedResults { get; private set; }

			public override void LoadInstallationResults(InstallationResultCollection results)
			{
				LastLoadedResults = results;
			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				OKButton.PerformClick();
			}
		}

		#endregion

		#region MockStartupDirector

		class MockStartupDirector : StartupDirector
		{
			public bool StartActualUserInterface { get; set; }
			public int UserInterfaceCalls { get; private set; }

			public new MockConfiguration Configuration
			{
				get { return (MockConfiguration)base.Configuration; }
			}

			public new InstallationItem TopLevelItem
			{
				get { return base.TopLevelItem; }
				set { base.TopLevelItem = value; }
			}

			protected override Configuration GetNewConfiguration()
			{
				return config ?? new MockConfiguration();
			}

			protected override bool InitializeInstallationItems()
			{
				if (TopLevelItem == null)
				{
					Installation installation = new Installation(Configuration);
					TopLevelItem = new DummyInstallationItem(installation);
				}
				return true;
			}

			public new void StartInstall()
			{
				base.StartInstall();
			}

			protected override void StartUserInterface()
			{
				UserInterfaceCalls++;
				if (StartActualUserInterface)
				{
					base.StartUserInterface();
				}
			}
		}

		#endregion
	}

	public abstract class StartupDirectorTestCase<T> : TestCase where T : StartupDirector, new()
	{
		T director;

		protected T Director
		{
			get { return director ?? (director = GetNewDirector()); }
		}

		protected virtual T GetNewDirector()
		{
			return new T();
		}

		protected int GetTopLevelItemCountForType(Type type)
		{
			InstallationItemCollection collection = Director.TopLevelItem.FindItems(delegate(InstallationItem item)
			{
				return type.IsAssignableFrom(item.GetType());
			});
			return collection.Count;
		}

		public virtual void TestTopLevelItemDependsOnEntireInstallation()
		{
			Director.Initialize(null);
			Assert(Director.TopLevelItem.Dependencies.Contains(Director.TopLevelItem.Installation));
		}
	}
}
