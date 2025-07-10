using System;
using System.IO;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	public class TerminalServerInstallModeTest : TestCase
	{
		Installation installation;
		TerminalServerInstallMode installMode;
		MockRepository mocker;
		Mock<IFileProxy> mockFileProxy;

		protected override void SetUp()
		{
			base.SetUp();
			mocker = new MockRepository(MockBehavior.Default);
			var configuration = new Configuration();
			var mockContainer = new MoqMockServiceContainer(mocker);
			configuration.Services = mockContainer;
			mockFileProxy = mockContainer.FileForTest;
			installation = new Installation(configuration);
		}

		TerminalServerInstallMode InstallMode
		{
			get { return installMode ?? (installMode = new TerminalServerInstallMode(installation)); }
		}

		protected override void TearDown()
		{
			mocker.VerifyAll();
			base.TearDown();
		}

		public static void AssertDependsOnInstallMode(InstallationItem item)
		{
			AssertEquals("Should include TerminalServerInstallMode as a dependency.", 1, item.FindItems(new InstallationItemFilter(IsTerminalServerInstallModeItem)).Count);
		}

		public void TestBothFilesExist()
		{
			mockFileProxy.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);
			AssertEquals("Should only have one dependency.", 1, InstallMode.Dependencies.Count);
			var changeProgram = InstallMode.Dependencies[0] as InstallationProgram;
			Assert("Should prefer Windows over Citrix.", changeProgram.FullPathOfProgramToRun.EndsWith("\\CHANGE.EXE"));
			AssertEquals("Should prefer Windows over Citrix.", "USER /INSTALL", changeProgram.Arguments);
			Assert("Should wait for exit.", changeProgram.WaitForExit);
		}

		public void TestNeedsToInstall()
		{
			mockFileProxy.SetupSequence(m => m.Exists(It.IsAny<string>())).Returns(true).Returns(true).CallBase();
			Assert("Installer helper always needs to run.", InstallMode.NeedsToInstall());
		}

		public void TestNeitherFileExists()
		{
			mockFileProxy.SetupSequence(m => m.Exists(It.IsAny<string>())).Returns(false).Returns(false).CallBase();
			AssertEquals("Should not try to change mode because this is not a terminal server.", 0, InstallMode.Dependencies.Count);
		}

		public void TestOnlyCitrixExists()
		{
			mockFileProxy.SetupSequence(m => m.Exists(GetSystemFilePath("CHANGE.EXE"))).Returns(false).CallBase();
			mockFileProxy.SetupSequence(m => m.Exists(GetSystemFilePath("CHGUSR.EXE"))).Returns(true).CallBase();
			AssertEquals("Should only have one dependency.", 1, InstallMode.Dependencies.Count);
			var changeProgram = InstallMode.Dependencies[0] as InstallationProgram;
			Assert("Should use Citrix.", changeProgram.FullPathOfProgramToRun.EndsWith("\\CHGUSR.EXE"));
			AssertEquals("Should use Citrix command line.", "/INSTALL", changeProgram.Arguments);
			Assert("Should wait for exit.", changeProgram.WaitForExit);
		}

		public void TestOnlyWindowsExists()
		{
			mockFileProxy.SetupSequence(m => m.Exists(GetSystemFilePath("CHANGE.EXE"))).Returns(true).CallBase();
			AssertEquals("Should only have one dependency.", 1, InstallMode.Dependencies.Count);
			var changeProgram = InstallMode.Dependencies[0] as InstallationProgram;
			Assert("Should prefer Windows over Citrix.", changeProgram.FullPathOfProgramToRun.EndsWith("\\CHANGE.EXE"));
			AssertEquals("Should prefer Windows over Citrix.", "USER /INSTALL", changeProgram.Arguments);
			Assert("Should wait for exit.", changeProgram.WaitForExit);
		}

		static string GetSystemFilePath(string fileName)
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), fileName);
		}

		static bool IsTerminalServerInstallModeItem(InstallationItem item)
		{
			return item is TerminalServerInstallMode;
		}
	}
}
