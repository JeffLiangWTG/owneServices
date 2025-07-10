using System.IO;
using CargoWise.BuildTools.Testing;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Testing
{
	sealed class EventGeneratorTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestStmEventsWinPathExists()
		{
			var eventGenerator = new EventGenerator(outputDirectory);

			Assert(File.Exists(Path.Combine(BaseSourcePath, eventGenerator.StmEventsFilePath)));
		}

		protected override void SetUp()
		{
			base.SetUp();
			outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.DontSave);
			MockSourceControl.Setup();
		}

		protected override void TearDown()
		{
			base.TearDown();
			outputDirectory.Dispose();
			MockSourceControl.TearDown();
		}

		GeneratorOutputDirectory outputDirectory;
	}
}
