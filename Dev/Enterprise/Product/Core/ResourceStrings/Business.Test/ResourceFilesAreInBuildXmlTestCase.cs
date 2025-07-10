using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class ResourceFilesAreInBuildXmlTestCase : TestCase
	{
		public void TestIt()
		{
			IList<string> languagesInAssemblies = DataFile.GetAvailableLanguages();
			IList<string> languagesInBuildXml = new List<string>(GetLanguagesInBuildXml());

			AssertEquals("Language assemblies exist", true, languagesInAssemblies.Count > 0);
			foreach (string language in languagesInAssemblies)
			{
				if (language != Res.DefaultLanguage)
				{
					AssertEquals("Add file " + DataFileAssemblyPrefix + language + ".zrs to build.xml", true, languagesInBuildXml.Contains(language));
				}
			}
		}

		IEnumerable<string> GetLanguagesInBuildXml()
		{
			foreach (string assembly in BuildXml.Instance.GetAllAssembliesDeployedToClient(AssemblyLoader.GetBinPath()).Where(file => IsDataFile(file)))
			{
				yield return assembly.Substring(DataFileAssemblyPrefix.Length).Replace(".zrs", "");
			}
		}

		bool IsDataFile(string fileName)
		{
			return fileName.StartsWith(DataFileAssemblyPrefix, StringComparison.OrdinalIgnoreCase);
		}

		const string DataFileAssemblyPrefix = "Enterprise.ResourceStrings.DataFiles.";
	}
}
