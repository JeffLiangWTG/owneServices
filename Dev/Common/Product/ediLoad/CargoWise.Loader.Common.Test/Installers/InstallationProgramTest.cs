using NUnit.Framework;

namespace CargoWise.Loader.Common
{
	class InstallationProgramTest : TestCase
	{
		public void TestProperties()
		{
			InstallationProgram program = new InstallationProgram(null);
			program.SetFullPathOfProgramToRun("foo bar");
			AssertEquals("foo bar", program.FullPathOfProgramToRun);
		}
	}
}