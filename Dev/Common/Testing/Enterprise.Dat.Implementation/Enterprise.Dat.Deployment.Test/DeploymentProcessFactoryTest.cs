using NUnit.Framework;
using WTG.DeploymentUtils.FileSystem;

namespace Enterprise.Dat.Implementation.Testing
{
	public class DeploymentProcessFactoryTest : TestCase
	{
		public void TestDatBackupFileGenerator()
		{
			var configuration = DeploymentConfiguration.FromConfigurationString("DatBackupFileUpdate", FakeBinPath, FakeSourcePath, null);
			var factory = new DeploymentProcessFactory();
			var process = factory.Create(configuration);

			Assert(process is DatBackupFileGenerator);
		}

		const string FakeBinPath = @"C:\AAA";
		const string FakeSourcePath = @"C:\BBB";
	}
}
