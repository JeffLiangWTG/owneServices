using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BuildTools;
using Enterprise.BusinessObjectGenerator.ModelView;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Test.ConsistencyTestRunner
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	public class GeneratorConsistencyTest : TestCase
	{
		// Test methods populated by GeneratorConsistencyTestGenerator.Program
		public void TestConsistencyTestsAreGenerated()
		{
			const int staticTests = 4; // We have 4 statically compiled tests in this file, and are fetching the count of dynamic + static tests

			var testCount = GetType().GetMethods().Count(method => method.Name.StartsWith("Test", StringComparison.OrdinalIgnoreCase));
			var bizosCount = BuildXml.Instance.AllBusinessObjects.Cast<BuildXmlBizOEntry>().Count();
			AssertEquals(bizosCount, testCount - staticTests);
		}

		public void TestAutoEnterpriseSchema()
		{
			Runner.GenerateAndAssert(outputDirectory =>
			{
				var schemaGenerator = new AutoSchemaGenerator(outputDirectory);
				schemaGenerator.Generate();
			});
		}

		public void TestSchemaForNonBusinessObjectTables()
		{
			Runner.GenerateAndAssert(outputDirectory =>
			{
				var schemaGenerator = new AutoSchemaGenerator(outputDirectory);
				schemaGenerator.GenerateSchemaForNonBusinessObjectTables();
			});
		}

		#region ModelView Consistency Test

		public void TestModelViewObjectGenerator()
		{
			var binariesFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
			var scriptDefAssPath = Path.Combine(binariesFolder, "CargoWise.DbUpgrader.Scripts.Definitions.dll");
			var scriptDefAss = Assembly.LoadFrom(scriptDefAssPath);
			var scriptDefAllResources = scriptDefAss.GetManifestResourceNames().Where(rn =>
					rn.StartsWith(ModelViewConstants.NamespacePrefix, StringComparison.Ordinal))
				.ToDictionary(rn => rn.Substring(ModelViewConstants.NamespacePrefix.Length + 1), rn => rn);
			var modelFileResources = scriptDefAllResources.Values.Where(d =>
				d.EndsWith(ModelViewConstants.ModelFileSuffix, StringComparison.InvariantCultureIgnoreCase));

			using var tempCWSharedPathDirectoryInfo = Runner.WithTempCWSharedPathForTest(Runner.CWSharedPathDirectoryInfo.FullName);
			using var tempCWSharedSubPath = Runner.WithTempCWSharedSubPath(ModelViewConstants.RelativeProjectRootPath + "\\");
			using var tempGetActualResource = Runner.WithTempGetActualResource(actualFilename =>
			{
				if (scriptDefAllResources.TryGetValue(actualFilename.Replace('\\', '.'), out var resourceName))
				{
					return scriptDefAss.GetManifestResourceStream(resourceName);
				}

				return null;
			});

			Runner.GenerateAndAssert(outputDirectory =>
			{
				Runner.SetupModelViewSources(outputDirectory, modelFileResources, scriptDefAss, binariesFolder, f => true);

				var schemaGenerator = new ModelViewObjectGenerator(outputDirectory);
				schemaGenerator.Generate();
			});
		}

		#endregion
	}
}
