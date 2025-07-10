using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReflectionTest;

sealed class LanguageVersionTest : TestCaseWithFactory
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestNoOverrideLangVersion()
	{
		string[] filePatterns = ["*.csproj", "*.props", "*.targets"];
		var files = filePatterns.SelectMany(filePattern => Directory.GetFiles(BaseSourcePath, filePattern, SearchOption.AllDirectories));

		CombineAssertions("File should not have a LangVersion element, language version is configured at a root level. Please remove it from the file.", () =>
		{
			foreach (var file in files)
			{
				AssertNoLangVersion(file);
			}
		});
	}

	void AssertNoLangVersion(string file)
	{
		var fileContent = File.ReadAllText(file);
		var filePath = file.Substring(BaseSourcePath.Length).TrimStart('\\');

		if (filePath == "Directory.Build.props")
		{
			return;
		}

		AssertNotContains($"Path: {filePath}", "<LangVersion", fileContent);
	}
}
