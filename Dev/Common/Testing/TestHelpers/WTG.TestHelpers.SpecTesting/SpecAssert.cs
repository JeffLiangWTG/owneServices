using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace WTG.TestHelpers.SpecTesting
{
	public static class SpecAssert
	{
		public static void AssertAllSpecMatches(Assembly testAssembly, IEnumerable<SpecFile> specFiles, string namespacePrefix, string regenExecutable)
		{
			// For this, we just assume that all "namespacePrefix" values would contain a ".SpecTesting" somewhere inside.
			// Just makes life easier for quality of life test error messages with less boilerplate.
			var namespacePostSpecTest = namespacePrefix.Split(new[] { ".SpecTesting" }, StringSplitOptions.None)[1];
			var pathPrefix = namespacePostSpecTest.Replace('.', '/');

			var failMessageSuffix = $@"

If this test has failed, then you have broken the spec for this spec test.
If you intended to change the spec, then please run Bin/{regenExecutable} to regenerate the spec files.
For more info, please see https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/14017/Spec-Testing";

			foreach (var file in specFiles)
			{
				var filePath = $"{pathPrefix}/{file.FileName}";

				// Get existing spec
				var resourceStream = testAssembly.GetManifestResourceStream($"{namespacePrefix}.{file.FileName}")
					?? throw new Exception($"Missing spec info for file {filePath}" + failMessageSuffix);

				var stringFromResource = new StreamReader(resourceStream!).ReadToEnd();

				// Ensure they match
				Assert.That(
					file.NormalizedContents(),
					Is.EqualTo(SpecText.NormalizeSpecText(stringFromResource)).NoClip,
					$"Spec filed for file {filePath}" + failMessageSuffix
				);
			}
		}

		public static void AssertAllSpecMatches(Assembly testAssembly, ISpecProvider specProvider)
		{
			AssertAllSpecMatches(testAssembly, specProvider.GetSpecFiles(), specProvider.SpecNamespacePrefix, specProvider.RegenExecutableName);
		}
	}
}
