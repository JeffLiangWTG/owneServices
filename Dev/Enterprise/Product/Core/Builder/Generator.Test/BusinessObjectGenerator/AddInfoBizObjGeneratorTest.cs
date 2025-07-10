using System.IO;
using CargoWise.BuildTools.Testing;
using CargoWise.IO;
using Enterprise.BusinessObjectGenerator.ModelView;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Testing
{
	sealed class AddInfoBizObjGeneratorTest : TransactionedTestCase
	{
		public void TestGenerateAll()
		{
			var generator = new AddInfoBizObjGenerator(Path.Combine(MockSourceControl.MockWorkspacePath, AutoJobDeclaration), "ZAJobDeclaration", "JobDeclaration", outputDirectory);
			generator.GenerateAll(outputDirectory);
			AssertEquals(true, File.Exists(Path.Combine(outputDirectory.CWSharedOutputDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas\ZAJobDeclarationSchema.cs")));
			AssertEquals(true, File.Exists(Path.Combine(outputDirectory.DevOutputDirectory, @"Enterprise\Product\Operations\Customs\ZA\Business\Business\JobDeclaration\AutoJobDeclaration.cs")));
			AssertEquals(true, File.Exists(Path.Combine(outputDirectory.DevOutputDirectory, @"Enterprise\Product\Operations\Customs\ZA\Business\Business\JobDeclaration\AutoJobDeclarationValidation.cs")));
			AssertEquals(true, File.Exists(Path.Combine(outputDirectory.DevOutputDirectory, @"Enterprise\Product\Operations\Customs\ZA\Business\Business\JobDeclaration\AutoJobDeclarationLookups.cs")));
			AssertEquals(false, File.Exists(Path.Combine(outputDirectory.DevOutputDirectory, @"Enterprise\Product\Operations\Customs\ZA\Business\Business\JobDeclaration\JobDeclarationValidation.cs")));
			AssertEquals(false, File.Exists(Path.Combine(outputDirectory.DevOutputDirectory, @"Enterprise\Product\Operations\Customs\ZA\Business\Business\JobDeclaration\JobDeclarationLookups.cs")));
		}

		public void TestFileNameOfBusinessObjectSchema()
		{
			var generator = new AddInfoBizObjGenerator(Path.Combine(MockSourceControl.MockWorkspacePath, AutoJobDeclaration), "ZAJobDeclaration", "JobDeclaration", outputDirectory);
			AssertEquals(Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Database\CargoWise.Odyssey.Schema\CargoWise.Odyssey.Schema\Schemas\ZAJobDeclarationSchema.cs"), generator.FileNameOfBusinessObjectSchema);
		}

		#region Implementation

		const string AutoJobDeclaration =
			@"Enterprise\Product\Operations\Customs\ZA\Business\Business\JobDeclaration\AutoJobDeclaration.cs";

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			_ = embeddedResourceRetriever.SaveResourceToFile(sourceXml, "ZZDummyBizo.model.xml", Path.Combine("CargoWise.DbUpgrader\\src\\Scripts", ModelViewConstants.ScriptsDefinitionsNamespace, "CountryFolder"));

			MockSourceControl.Setup();
			outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut, cwsharedSourceDirectory: embeddedResourceRetriever.DirectoryPath);
		}

		protected override void TearDown()
		{
			embeddedResourceRetriever?.Dispose();
			embeddedResourceRetriever = null;
			MockSourceControl.TearDown();
			outputDirectory.Dispose();
			base.TearDown();
		}

		GeneratorOutputDirectory outputDirectory;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		const string sourceXml = "Enterprise.Builder.Generator.Test.DatabaseObjectGenerator.TestFiles.ZZDummyBizo.ZZDummyBizo.model.xml";

		#endregion
	}
}
