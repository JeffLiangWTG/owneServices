using System.IO;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	sealed class InstallationEnvironmentTest : TestCase
	{
		public void TestWithUnversionedDirectory()
		{
			var path = @"C:\Program Files\CargoWise edi\ediEnterprise";
			InstallationEnvironment.OverridableGetBinPath.Value = (() => { return path; });

			AssertEquals(path, InstallationEnvironment.Instance.BaseInstallPath);
			AssertEquals(Path.Combine(path, "1.2.3456.7"), InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber("1.2.3456.7")));
		}

		public void TestWithVersionedDirectory()
		{
			var path = @"C:\Program Files\CargoWise edi\ediEnterprise";
			InstallationEnvironment.OverridableGetBinPath.Value = (() => { return Path.Combine(path, "1.4.3585.0"); });

			AssertEquals(path, InstallationEnvironment.Instance.BaseInstallPath);
			AssertEquals(Path.Combine(path, "1.2.3456.7"), InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber("1.2.3456.7")));
		}

		public void TestWithDeepersionedDirectory()
		{
			var path = @"C:\Program Files\CargoWise edi\ediEnterprise\Test\AnotherDirectory";
			InstallationEnvironment.OverridableGetBinPath.Value = (() => { return Path.Combine(path, "1.4.3585.0"); });

			AssertEquals(path, InstallationEnvironment.Instance.BaseInstallPath);
			AssertEquals(Path.Combine(path, "1.2.3456.7"), InstallationEnvironment.Instance.GetTargetInstallPath(new VersionNumber("1.2.3456.7")));
		}
	}
}
