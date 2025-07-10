using System.IO;
using CargoWise.BuildTools;
using CargoWise.BuildTools.Testing;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Testing;

class BizObjGeneratorTest : TestCase
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestConcreteClassesAreCreatedWhenMultipleBusinessObjectsAreGenerated()
	{
		try
		{
			MockSourceControl.Setup();

			var testInstance = new BuildXml(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "Builder", "Generator", "Testing", "TestBuild.xml"));
			BuildXml.SetInstanceForTesting(testInstance);

			using var outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut);
			var generator = new BizObjGenerator(outputDirectory);

			generator.GenerateSpecificFile("DummyPivot", exactMatch: true, excludeAutoEnterpriseSchema: true, excludeClientSpecific: true);
			generator.GenerateSpecificFile("DummyBizo", exactMatch: true, excludeAutoEnterpriseSchema: true, excludeClientSpecific: true);

			var paths = outputDirectory.GetAllFilePaths();
			var generatedFiles = paths.Values;
			AssertCollectionContains("Concrete validations should be generated.", Path.Combine(MockSourceControl.MockWorkspacePath, "BuildTools", "SourceSafeTestSolution", "DummyBizoValidation.cs"), generatedFiles);
		}
		finally
		{
			MockSourceControl.TearDown();
		}
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestXmlFilesAreGenerated()
	{
		try
		{
			MockSourceControl.Setup();

			var testInstance = new BuildXml(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "Builder", "Generator", "Testing", "TestBuild.xml"));
			BuildXml.SetInstanceForTesting(testInstance);

			using var outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut);
			var generator = new BizObjGenerator(outputDirectory);

			generator.GenerateSpecificFile("SGCusClassPartPivotAddInfoSchema.xml", exactMatch: true, excludeAutoEnterpriseSchema: true, excludeClientSpecific: true);

			var paths = outputDirectory.GetAllFilePaths();
			var generatedFiles = paths.Values;
			AssertGreaterThan("Files should be generated", generatedFiles.Count, 0);
		}
		finally
		{
			MockSourceControl.TearDown();
		}
	}
}
