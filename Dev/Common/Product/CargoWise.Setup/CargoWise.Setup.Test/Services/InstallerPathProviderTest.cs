using System.Reflection;
using CargoWise.Setup.Services;
using NUnit.Framework;

namespace CargoWise.Setup.Test.Services;

internal class InstallerPathProviderTest
{
	[TestCase]
	public void TestGetInstallBasePath()
	{
		// Arrange
		var provider = new InstallPathProvider();
		var expectedPath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "")?.FullName;

		// Act & Assert
		Assert.That(provider.GetInstallPath(), Is.EqualTo(expectedPath));
	}
}
