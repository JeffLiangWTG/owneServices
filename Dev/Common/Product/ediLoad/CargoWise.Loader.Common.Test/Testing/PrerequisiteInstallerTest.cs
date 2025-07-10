using System.IO;
using NUnit.Framework;
using NUnit.Framework.Dat;

namespace CargoWise.Loader.Common.Testing
{
	public abstract class PrerequisiteInstallerTest : TestCase
	{
		public string TemplateCDPath = Path.Combine(DatServerConnection.DatFileSharePath, @"ServerInstallCD\DvdBuilderFiles\v1");

		protected abstract InstallationItem GetItemToTest(Installation installation);

		protected InstallationItem ItemToTest { get; private set; }

		protected override void SetUp()
		{
			Installation installation = new Installation(new MockConfiguration(TemplateCDPath));
			ItemToTest = GetItemToTest(installation);
		}

		public virtual void TestDependsOnTerminalServerInstallMode()
		{
			TerminalServerInstallModeTest.AssertDependsOnInstallMode(ItemToTest);
		}

		[ExpectNoExceptions]
		public virtual void TestInstallationProgramsAreWindowsInstallerPrograms()
		{
			foreach (InstallationItem item in ItemToTest.GetDepthFirstEnumerable())
			{
				InstallationProgram program = item as InstallationProgram;
				if (program != null)
				{
					const string Message = "Setup programs should use WindowsInstallerProgram so they will show user-friendly Windows Installer error messages if installation fails.";
					Assert(Message, program is WindowsInstallerProgram || program.FullPathOfProgramToRun.EndsWith("CHANGE.EXE") /* Win2003 terminal server install mode is the exception to this rule */);
				}
			}
		}

		[ExpectNoExceptions]
		public virtual void TestRequiredFilesExistInServerInstall()
		{
			foreach (InstallationItem item in ItemToTest.GetDepthFirstEnumerable())
			{
				InstallationProgram installProgram = item as InstallationProgram;
				string sourcePath = (installProgram == null) ? null : installProgram.FullPathOfProgramToRun;
				AssertImplies("File should exist: " + sourcePath, sourcePath != null, File.Exists(sourcePath));
			}
		}
	}
}