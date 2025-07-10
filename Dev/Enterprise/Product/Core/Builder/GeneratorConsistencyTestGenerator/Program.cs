using System.Linq;
using CargoWise.BuildTools;
using CargoWise.StaticAnalysis;

namespace Enterprise.Builder.GeneratorConsistencyTestGenerator
{
	public static class Program
	{
		public static void Main(string[] argv)
		{
			if (argv.Length == 0)
			{
				argv = new[] { string.Empty };
			}
			foreach(var group in BuildXml.Instance.AllBusinessObjects
				.Cast<BuildXmlBizOEntry>()
				.GroupBy(entry => entry.AddInfoEntries.Any()))
			{
				var tableNames = group.Select(entry => entry.TableName);

				Emit.GenerateTestMethods(argv[0], "Enterprise.Builder.Generator.Test.ConsistencyTestRunner.GeneratorConsistencyTest", "Enterprise.Builder.Generator.Test.ConsistencyTestRunner.Runner", group.Key ? "AssertBizoCreatedCorrectly_WithAddInfoEntries" : "AssertBizoCreatedCorrectly", tableNames);
			}
		}
	}
}
