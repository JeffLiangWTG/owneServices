using System;
using System.IO;
using CargoWise.IO;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Client.Testing
{
	class ClientConfigurationTest : TestCase
	{
		MockClientConfiguration configuration;

		public void TestTargetPath()
		{
			var mocker = new MockRepository(MockBehavior.Default);
			var services = new MoqMockServiceContainer(mocker);
			Configuration.Services = services;
			services.EnvironmentForTest = new Mock<IEnvironmentProxy>();
			using (TempDirectory directory = new TempDirectory())
			{
				string newPath = Path.Combine(directory, @"WiseTech Global\CargoWise");

				services.EnvironmentForTest.Setup(m => m.GetFolderPath(Environment.SpecialFolder.ProgramFiles)).Returns(directory);
				AssertEquals("TargetPath", newPath, Configuration.TargetPath);

				Directory.CreateDirectory(newPath);
				Configuration.TargetPath = null;
				AssertEquals("TargetPath", newPath, Configuration.TargetPath);
			}
		}

		MockClientConfiguration Configuration
		{
			get { return configuration ?? (configuration = new MockClientConfiguration()); }
		}
	}
}
