using System.Reflection;
using CargoWise.BuildTools;
using CargoWise.StaticAnalysis;

[assembly: AssemblyTitle("Resource Strings SpellCheck Test Generator")]

namespace ResourceStringsSpellCheckTestGenerator
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				args = new[] { string.Empty };
			}

			var spellCheckAssemblies = BuildXml.Instance.GetAllAssembliesToBuild(false);

			Emit.GenerateTestMethods(args[0], "ResourceStrings.SpellCheck.TestRunner.SpellCheckEnglishTest",
				"ResourceStrings.SpellCheck.TestRunner.Runner", "CheckEnglishSpelling", spellCheckAssemblies);
			Emit.GenerateTestMethods(args[0], "ResourceStrings.SpellCheck.TestRunner.SpellCheckBritishEnglishTest",
				"ResourceStrings.SpellCheck.TestRunner.Runner", "CheckBritishEnglishSpelling", spellCheckAssemblies);
		}
	}
}
