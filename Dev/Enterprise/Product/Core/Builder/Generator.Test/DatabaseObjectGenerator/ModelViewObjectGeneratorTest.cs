using System.IO;
using CargoWise.BuildTools.Testing;
using CargoWise.IO;
using Enterprise.BusinessObjectGenerator.ModelView;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Test.DatabaseObjectGenerator
{
	sealed class ModelViewObjectGeneratorTest : TestCase
	{
		public void TestFilePathOfModelViews()
		{
			var generator = new ModelViewObjectGenerator(outputDirectory);
			AssertEquals(Path.Combine(outputDirectory.CWSharedSourceDirectory, @"CargoWise.DbUpgrader\src\Scripts\Scripts.Definitions\ModelViews.cs"), generator.FilePathOfModelViews);
		}

		public void TestGenerateAll()
		{
			var generator = new ModelViewObjectGenerator(outputDirectory);
			generator.Generate();

			Assert(File.Exists(Path.Combine(outputDirectory.CWSharedOutputDirectory, @"CargoWise.DbUpgrader\src\Scripts\Scripts.Definitions\ModelViews.cs")));
			Assert(File.Exists(Path.Combine(outputDirectory.CWSharedOutputDirectory, @"CargoWise.DbUpgrader\src\Scripts\Scripts.Definitions\CountryFolder\ZZDummyBizo.model.cs")));
			Assert(File.Exists(Path.Combine(outputDirectory.CWSharedOutputDirectory, @"CargoWise.DbUpgrader\src\Scripts\Scripts.Definitions\CountryFolder\ZZDummyBizo.model.sql")));
			Assert(File.Exists(Path.Combine(outputDirectory.CWSharedOutputDirectory, @"CargoWise.DbUpgrader\src\Scripts\Scripts.Definitions\CountryFolder\ZZDummyBizo.index.sql")));
		}

		#region Implementation

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
