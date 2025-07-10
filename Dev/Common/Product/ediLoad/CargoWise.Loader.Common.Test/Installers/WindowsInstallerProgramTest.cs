using System;
using System.IO;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class WindowsInstallerProgramTest : TestCase
	{
		class WindowsInstallerProgramForTest : WindowsInstallerProgram
		{
			public WindowsInstallerProgramForTest(InstallationItem parentInstallationItem, string nameOfComponent)
				: base(parentInstallationItem, nameOfComponent)
			{
			}

			public new InstallationResult InstallExcludingDependencies()
			{
				return base.InstallExcludingDependencies();
			}
		}

		WindowsInstallerProgramForTest testProgram;

		Mock<InstallationItem> moqParentInstallationItem;

		Mock<InstallationItem> MoqParentInstallationItem
		{
			get
			{
				if (moqParentInstallationItem == null)
				{
					moqParentInstallationItem = new Mock<InstallationItem>(new object[] { new Installation(new Configuration()) });
					moqParentInstallationItem.CallBase = true;
				}
				return moqParentInstallationItem;
			}
		}

		WindowsInstallerProgramForTest TestProgram
		{
			get
			{
				if (testProgram == null)
				{
					testProgram = new WindowsInstallerProgramForTest(MoqParentInstallationItem.Object, "Mock Program");
					testProgram.SetFullPathOfProgramToRun(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockProgram.exe"));
				}
				return testProgram;
			}
		}

		public void TestIgnoreRebootRequired()
		{
			TestProgram.Arguments = "-Return 3010";
			TestProgram.SilentlyIgnoreRebootRequired = true;
			InstallationResult result = TestProgram.InstallExcludingDependencies();
			AssertEquals("Should be a warning.", true, result.IsOK);
		}

		public void TestNeedsToInstall()
		{
			MoqParentInstallationItem.Protected()
				.Setup<bool>("NeedsToInstallCore")
				.Returns(false);
			AssertEquals("NeedsToInstall()", false, TestProgram.NeedsToInstall());
			MoqParentInstallationItem.Protected()
				.Setup<bool>("NeedsToInstallCore")
				.Returns(true);
			AssertEquals("NeedsToInstall()", true, TestProgram.NeedsToInstall());
		}

		public void TestRebootRequired()
		{
			TestProgram.Arguments = "-Return 3010";
			InstallationResult result = TestProgram.InstallExcludingDependencies();
			Assert("Should be a warning.", result.IsWarning);
			Assert("Message should say something about a reboot.", result.Message.IndexOf("reboot") >= 0);
		}

		public void TestReturnCodeOK()
		{
			TestProgram.Arguments = "-Return 0";
			Assert(TestProgram.InstallExcludingDependencies().IsOK);
		}

		void TestReturnCodeMappedDrive()
		{
			InstallationResult result = TestProgram.InstallExcludingDependencies();
			Assert("Should be an error.", TestProgram.InstallExcludingDependencies().IsError);
			AssertEquals("Message", TestProgram.UnableToInstallMessage + TestProgram.MappedNetworkDriveMessage, result.Message);
		}

		public void TestReturnCodeMappedDrive1305()
		{
			TestProgram.Arguments = "-Return 1305";
			TestReturnCodeMappedDrive();
		}

		public void TestReturnCodeMappedDrive2755()
		{
			TestProgram.Arguments = "-Return 2755";
			TestReturnCodeMappedDrive();
		}

		public void TestReturnCodeSystemErrorCode()
		{
			TestProgram.Arguments = "-Return 1601";
			InstallationResult result = TestProgram.InstallExcludingDependencies();
			Assert("Should be an error.", result.IsError);

			string errorMessage = TestingState.IsVista ?
				"The Windows Installer Service could not be accessed. This can occur if the Windows Installer is not correctly installed. Contact your support personnel for assistance." :
				"The Windows Installer Service could not be accessed. This can occur if you are running Windows in safe mode, or if the Windows Installer is not correctly installed. Contact your support personnel for assistance.";

			AssertEquals("Message", TestProgram.UnableToInstallMessage + WindowsInstallerProgram.ErrorMessageWasMessage + errorMessage, result.Message);
		}

		public void TestWaitForExitByDefault()
		{
			Assert("Should wait for exit.", TestProgram.WaitForExit);
		}
	}
}
