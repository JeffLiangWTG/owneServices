using System;
using CargoWise.Loader.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Client.Testing
{
	class InstallationProgramFromFileTest : TestCase
	{
		public void TestInstall()
		{
			MockClientConfiguration configuration = new MockClientConfiguration();
			configuration.TargetPath = AppDomain.CurrentDomain.BaseDirectory;
			configuration.SetProgramArguments("-WaitTwoSecondsAndExit");
			configuration.SetProgramFileName("MockProgram.exe");

			bool taskDescriptionChanged = false;
			ClientInstallation installation = new ClientInstallation(configuration);
			installation.CurrentTaskDescriptionChanged += delegate
			{
				taskDescriptionChanged = true;
			};

			var program = new InstallationProgramFromFile(installation);
			var results = new InstallationResultCollection();
			configuration.UILevel = UILevel.Normal;
			program.WaitForExit = true;
			program.Install(results);
			AssertEquals($"The base implementation for InstallExcludingDependencies should be called.\r\nErrors Thrown:\r\n{results.GetErrorMessages()}", true, taskDescriptionChanged);
			AssertEquals("IsOK", true, results[0].IsOK);
		}

		public void TestProperties()
		{
			MockClientConfiguration configuration = new MockClientConfiguration();
			configuration.UILevel = UILevel.Normal;
			configuration.TargetPath = @"X:\Moo";
			configuration.SetProgramArguments("Argh!");
			configuration.SetProgramFileName("Oink");
			ClientInstallation installation = new ClientInstallation(configuration);
			InstallationProgramFromFile program = new InstallationProgramFromFile(installation);
			AssertEquals("Arguments", "Argh!", program.Arguments);
			AssertEquals("FullPathOfProgramToRun", @"X:\Moo\Oink", program.FullPathOfProgramToRun);
			AssertEquals("WaitForInputIdle", Environment.UserInteractive, program.WaitForInputIdle);
		}

		public void TestArgumentsAreCalculated()
		{
			MockClientConfiguration configuration = new MockClientConfiguration();
			ClientInstallation installation = new ClientInstallation(configuration);
			InstallationProgramFromFile program = new InstallationProgramFromFile(installation);
			AssertEquals("Arguments", "", program.Arguments);
			configuration.SetProgramArguments("IWasSetByRunningAnotherTask");
			AssertEquals("Arguments", "IWasSetByRunningAnotherTask", program.Arguments);
		}

		public void TestWaitForInputIdleDefaultToUserInteractiveMode()
		{
			var clientConfigurationMock = new Mock<ClientConfiguration>();
			clientConfigurationMock.Setup(x => x.UILevel).Returns(UILevel.AutomatedWithNoUI);
			ClientInstallation installation = new ClientInstallation(clientConfigurationMock.Object);
			InstallationProgramFromFile program = new InstallationProgramFromFile(installation);

			AssertEquals("WaitForInputIdle", false, program.WaitForInputIdle);
		}
	}
}
