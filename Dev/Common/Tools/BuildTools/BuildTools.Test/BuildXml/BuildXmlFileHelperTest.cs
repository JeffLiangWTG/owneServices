using System.IO;
using CargoWise.IO;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace CargoWise.BuildTools.Testing;

sealed class BuildXmlFileHelperTest : TestCase
{
	public void TestGetRootBuildXmlFilePathForRootPath()
	{
		AssertEquals(rootPath, BuildXmlFileHelper.GetRootBuildXmlFilePath(rootPath));
	}

	public void TestGetRootBuildXmlFilePathForSubmodulePath()
	{
		var path = Path.Combine(temp.DirectoryName, "Root", "Intermediate", "Submodule");
		AssertEquals(rootPath, BuildXmlFileHelper.GetRootBuildXmlFilePath(path));
	}

	public void TestGetRootBuildXmlFilePathForPathWithinRoot()
	{
		var path = Path.Combine(temp.DirectoryName, "Root", "Intermediate");
		AssertEquals(rootPath, BuildXmlFileHelper.GetRootBuildXmlFilePath(path));
	}

	public void TestGetRootBuildXmlFilePathForPathWithinSubmodule()
	{
		var path = Path.Combine(temp.DirectoryName, "Root", "Intermediate", "Submodule", "Deep");
		AssertEquals(rootPath, BuildXmlFileHelper.GetRootBuildXmlFilePath(path));
	}

	public void TestGetRootBuildXmlFilePathForPathAboveRoot()
	{
		AssertEquals(null, BuildXmlFileHelper.GetRootBuildXmlFilePath(temp.DirectoryName));
	}

	protected override void SetUp()
	{
		base.SetUp();
		temp = new TempDirectory();
		rootPath = Path.Combine(temp.DirectoryName, "Root");
		var submodulePath = Path.Combine(rootPath, "Intermediate", "Submodule");
		Directory.CreateDirectory(submodulePath);
		Directory.CreateDirectory(Path.Combine(submodulePath, "Deep"));
		File.WriteAllText(Path.Combine(temp.DirectoryName, "Root", BuildXmlFile.FileName), "Whatever");
		File.WriteAllText(Path.Combine(temp.DirectoryName, "Root", "Intermediate", "Submodule", BuildXmlFile.FileName), "Whatever");
	}

	protected override void TearDown()
	{
		base.TearDown();
		temp?.Dispose();
		temp = null;
	}

	string rootPath;
	TempDirectory temp;
}
