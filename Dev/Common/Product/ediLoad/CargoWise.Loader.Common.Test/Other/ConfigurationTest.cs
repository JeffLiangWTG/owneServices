using System;
using System.IO;
using System.Reflection;
using CargoWise.BrandManager;
using CargoWise.IO;
using Microsoft.Win32;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class ConfigurationTest : TestCase
	{
		MockConfiguration configuration;

		public void TestAllArguments()
		{
			Configuration.Initialize(new string[]
			{
				"-Return",
				"0",
				"a b",
				Common.Configuration.NoUIArgument,
				"zz"
			});
			AssertEquals("AllArguments", "-Return 0 \"a b\" -NoUI zz", Configuration.AllArguments);
		}

		public void TestApplicationName()
		{
			AssertEquals("ApplicationName", BrandingFactory.Instance.ProductName, Configuration.ApplicationName);
		}

		public void TestAppManagerDirectoryName()
		{
			AssertEquals("AppManagerDirectoryName", "CargoWise Application Manager", Configuration.AppManagerDirectoryName);
		}

		public void TestHandledArgumentsAreNotPassedToChildren()
		{
			Configuration.Initialize(new string[]
			{
				"x",
				Common.Configuration.AutomatedArgument,
				Common.Configuration.NoUIArgument,
				Common.Configuration.RelaunchedElevatedArgument,
				"y"
			});
			AssertEquals("UnhandledArguments.Count", 2, Configuration.UnhandledArguments.Count);
			AssertEquals("UnhandledArguments[0]", "x", Configuration.UnhandledArguments[0]);
			AssertEquals("UnhandledArguments[1]", "y", Configuration.UnhandledArguments[1]);
		}

		public void TestNotifier()
		{
			AssertNotNull("Notifier", Configuration.Notifier);
		}

		public void TestRelaunchedElevated()
		{
			AssertEquals("RelaunchedElevated", false, Configuration.RelaunchedElevated);
			Configuration.Initialize(new string[] { Common.Configuration.RelaunchedElevatedArgument });
			AssertEquals("RelaunchedElevated", true, Configuration.RelaunchedElevated);
		}

		public void TestReturnCode()
		{
			AssertEquals("ReturnCode", ReturnCode.Success, Configuration.ReturnCode);
			Configuration.ReturnCode = ReturnCode.Failure;
			AssertEquals("ReturnCode", ReturnCode.Failure, Configuration.ReturnCode);
		}

		public void TestServiceContainer()
		{
			AssertEquals("Services.GetType()", typeof(ServiceContainer), Configuration.Services.GetType());
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ Configuration.Services = null; });
			var services = new MoqMockServiceContainer();
			Configuration.Services = services;
			AssertEquals("Services", services, Configuration.Services);
		}

		[TestRequiresAdministrativePrivileges("Registry write")]
		public void TestTargetPath()
		{
			var backupValue = Common.Configuration.ProgramFilesOverrideRegistry;
			RegistryInstallationLocation.SetRegistryInstallationLocationOverride(null);

			try
			{
				var mocker = new MockRepository(MockBehavior.Default);
				var services = new MoqMockServiceContainer(mocker);
				Configuration.Services = services;
				var envMock = new Mock<IEnvironmentProxy>();
				services.Environment = envMock.Object;
				using (var directory = new TempDirectory())
				{
					envMock.Setup(m => m.GetFolderPath(Environment.SpecialFolder.ProgramFiles)).Returns(directory);
					AssertEquals("TargetPath", Path.Combine(directory, @"WiseTech Global\CargoWise"), Configuration.TargetPath);

					Configuration.TargetDirectoryName = "Another Enterprise";
					Configuration.TargetPath = null;
					AssertEquals("TargetPath", Path.Combine(directory, @"WiseTech Global\Another Enterprise"), Configuration.TargetPath);

					envMock.SetupSequence(m => m.GetEnvironmentVariable(Common.Configuration.ProgramFilesOverrideEnvironmentVariable))
						.Returns(@"X:\Oink")
						.Returns(@"X:\Oink")
						.CallBase();
					Configuration.TargetPath = null;
					AssertEquals("TargetPath", @"X:\Oink\WiseTech Global\Another Enterprise", Configuration.TargetPath);
					AssertEquals("TargetPath", @"X:\Oink\WiseTech Global\Gah!", Configuration.GetTargetPath("Gah!"));

					envMock.Setup(m => m.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)).Returns(directory);
					RegistryInstallationLocation.SetRegistryInstallationLocationOverride(@"X:\Nostrils");
					Configuration.TargetPath = null;
					AssertEquals("TargetPath", @"X:\Nostrils\WiseTech Global\Another Enterprise", Configuration.TargetPath);
					AssertEquals("TargetPath", @"X:\Nostrils\WiseTech Global\Gah!", Configuration.GetTargetPath("Gah!"));
				}
			}
			finally
			{
				RegistryInstallationLocation.SetRegistryInstallationLocationOverride(backupValue);
			}
		}

		[GuiTest]
		public void TestUILevel()
		{
			AssertEquals("UILevel", UILevel.Normal, Configuration.UILevel);

			Configuration.Initialize(new string[] { Common.Configuration.AutomatedArgument });
			AssertEquals("UILevel", UILevel.AutomatedWithUI, Configuration.UILevel);

			Configuration.Initialize(new string[] { Common.Configuration.NoUIArgument, Common.Configuration.AutomatedArgument });
			AssertEquals("UILevel", UILevel.AutomatedWithNoUI, Configuration.UILevel);
		}

		public void TestStartupPathFromLocation()
		{
			AssertEquals(@"C:\", Configuration.StartupPathFromLocation(@"C:\test.exe"));
			AssertEquals(@"C:\Folder", Configuration.StartupPathFromLocation(@"C:\Folder\test.exe"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Testing production code that uses it")]
		public void TestStartupPath()
		{
			var location = Assembly.GetEntryAssembly().Location;

			AssertEquals(Path.GetDirectoryName(location), Configuration.StartupPath);
			AssertEquals("StartupPathFromLocation was called", location, Configuration.LastLocation);
		}

		public void TestStartBrandingType()
		{
			AssertEquals("Default BrandingType is CW1 Legacy", BrandingFactory.BrandingType.CW1LegacyBranding, Configuration.BrandingType);

			Configuration.Initialize(new string[] { "-StartBranding:CWNext" });
			AssertEquals("Set BrandingType as CargoWiseNext", BrandingFactory.BrandingType.CargoWiseNext, Configuration.BrandingType);
		}

		MockConfiguration Configuration
		{
			get { return configuration ?? (configuration = new MockConfiguration()); }
		}
	}

	public static class RegistryInstallationLocation
	{
		public static void SetRegistryInstallationLocationOverride(string location)
		{
			try
			{
				using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				{
					using (var key = hklm32.OpenSubKey(@"SOFTWARE\WiseTech Global", true))
					{
						if (location != null)
						{
							key.SetValue("InstallationLocation", location);
						}
						else
						{
							key.DeleteValue("InstallationLocation", false);
						}
					}
				}
			}
			catch (System.Security.SecurityException e)
			{
				throw new System.Security.SecurityException("This test must be run with administrative privileges.", e);
			}
		}
	}
}
