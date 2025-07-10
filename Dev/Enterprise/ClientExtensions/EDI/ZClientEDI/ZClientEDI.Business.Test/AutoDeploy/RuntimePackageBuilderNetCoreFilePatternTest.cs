using System;
using NUnit.Framework;

namespace Enterprise.Client.EDI.AutoDeploy.Test;

public class NetCoreExcludeFilePatternsTests : TestCase
{
	public void TestMatchesNetCoreExcludeFilePatterns_TestExtension_ReturnsTrue()
	{
		Assert(@"*.Test file should be excluded", RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(
			@"C:\git\wtg\CargoWise\Dev\Bin\net8.0-windows\.msCoverageSourceRootsMapping_Enterprise.OutlookUtilities.Test"));
	}

	public void TestMatchesNetCoreExcludeFilePatterns_NUnitFiles_ReturnsTrue()
	{
		string[] nunitFiles = {
			@"C:\Project\bin\Debug\nunit.engine.dll",
			@"C:\Project\bin\Debug\nunit.framework.legacy.dll",
			@"C:\Project\bin\Debug\nunit.engine.api.dll",
			@"C:\Project\bin\Debug\nunit.framework.dll",
		};
		foreach (var fileName in nunitFiles)
		{
			Assert($@"{fileName} should be excluded", RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(fileName));
		}
	}

	public void TestMatchesNetCoreExcludeFilePatterns_TestRelatedFiles_ReturnsTrue()
	{
		string[] testRelatedFiles = {
			@"C:\Project\bin\Debug\testhost.dll",
			@"C:\Project\bin\Debug\testhost.exe",
			@"C:\Project\bin\Debug\testcentric.engine.dll",
			@"C:\Project\bin\Debug\testcentric.engine.metadata.dll",
			@"C:\Project\bin\Debug\test.dll",
			@"C:\Project\bin\Debug\abc.test.dll",
			@"C:\Project\bin\Debug\cdf.testing.dll",
		};
		foreach (var fileName in testRelatedFiles)
		{
			Assert($@"{fileName} should be excluded", RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(fileName));
		}
	}

	public void TestMatchesNetCoreExcludeFilePatterns_RegularProjectFiles_ReturnsFalse()
	{
		string[] regularProjectFiles = {
			@"C:\Project\bin\Debug\MyApp.dll",
			@"C:\Project\bin\Debug\ProductionCode.exe",
			@"C:\Project\bin\Debug\BusinessLogic.deps.json",
			@"C:\Project\bin\Debug\CoreApp.runtimeconfig.json",
			@"net8.0\AppDomainWrappers.Net8.dll",
			@"C:\git\wtg\CargoWise\Dev\Bin\net8.0\AppDomainWrappers.Net8.exe",
			@"C:\git\wtg\CargoWise\Dev\Bin\appserver\CargoWise.Glow.Model.CW.Resources.dll",
			@"C:\git\wtg\CargoWise\Dev\Bin\appserver\CargoWise.Organizations.CodeGeneration.dll",
			@"C:\git\wtg\CargoWise\Dev\Bin\appserver\Test.XyZ.dll",
			@"C:\git\wtg\CargoWise\Dev\Bin\appserver\Test.xxxx.yyyy.dll",
		};
		foreach (var fileName in regularProjectFiles)
		{
			Assert($@"{fileName} should not be excluded", !RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(fileName));
		}
	}

	public void TestMatchesNetCoreExcludeFilePatterns_CaseInsensitivity_ReturnsTrue()
	{
		string[] caseInsensitiveFiles = {
			@"C:\Project\bin\Debug\NUnit3.TestAdapter.dll",
			@"C:\Project\bin\Debug\NUnit3.TestAdapter.PDB",
		};
		foreach (var fileName in caseInsensitiveFiles)
		{
			Assert($@"{fileName} should be excluded", RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(fileName));
		}
	}

	public void TestMatchesNetCoreExcludeFilePatterns_EmptyFileName_ReturnsFalse()
	{
		AssertExceptionThrown<ArgumentNullException>(() =>
			RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(string.Empty));
	}

	public void TestMatchesNetCoreExcludeFilePatterns_NullFileName_ThrowsArgumentNullException()
	{
		AssertExceptionThrown<ArgumentNullException>(() =>
			RuntimePackageBuilder.MatchesNetCoreExcludeFilePatterns(null));
	}

	public void TestGetNoDeployFiles_ReturnsNoDeployFiles()
	{
		var buildXml = CargoWise.BuildTools.BuildXml.Instance;
		var noDeployFiles = RuntimePackageBuilder.GetNoDeployFiles(buildXml);

		Assert("Enterprise.Metadata.Business.Tests.deps.json should be in no deploy files",
			noDeployFiles.Contains("net8.0\\Enterprise.Metadata.Business.Tests.deps.json"));
		Assert("Enterprise.Metadata.Business.Tests.runtimeconfig.json should be in no deploy files",
			noDeployFiles.Contains("net8.0\\Enterprise.Metadata.Business.Tests.runtimeconfig.json"));
		Assert("Enterprise.Metadata.Business.Tests.xml should be in no deploy files",
			noDeployFiles.Contains("net8.0\\Enterprise.Metadata.Business.Tests.xml"));
	}
}
