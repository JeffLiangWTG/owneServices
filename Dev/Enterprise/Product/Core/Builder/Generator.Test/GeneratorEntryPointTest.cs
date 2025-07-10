using System.IO;
using CargoWise.BuildTools;
using CargoWise.BuildTools.Testing;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Testing
{
	sealed class GeneratorEntryPointTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateFilesForSolution()
		{
			BuildXml testInstance = new BuildXml(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "Builder", "Generator", "Testing", "TestBuild.xml"));
			BuildXml.SetInstanceForTesting(testInstance);

			var path = Path.Combine(cwshared.FullName, "CargoWise.DbUpgrader", "src", "Database", "CargoWise.Odyssey.Schema", "CargoWise.Odyssey.Schema", "AutoEnterpriseSchema.cs");
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, testContents);
			File.SetAttributes(path, FileAttributes.ReadOnly);
			SourceControl.EnterpriseDatabase.AddFile(path);

			string[] args = { Db.ServerName, Db.DatabaseName, CommandLineOptions.GenerateBizObjectsForSolution, "ZArchitecture" };

			GeneratorEntryPoint entryPoint = new GeneratorEntryPoint(outputDirectory);
			int result = entryPoint.Execute(GeneratorArguments.Parse(args));
			AssertEquals(0, result);
			string pathContent = File.ReadAllText(path);
			Assert("Was: " + testContents + " Now: " + pathContent, testContents != pathContent);
			Assert((File.GetAttributes(path) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly);
			Assert(File.Exists(Path.Combine(MockSourceControl.MockWorkspacePath, "ZArchitecture\\AutoStmALog.cs")));
			Assert(File.Exists(Path.Combine(MockSourceControl.MockWorkspacePath, "ZArchitecture\\AutoStmALogValidation.cs")));
			Assert(File.Exists(Path.Combine(MockSourceControl.MockWorkspacePath, "ZArchitecture\\StmALogValidation.cs")));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateSchemaList()
		{
			BuildXml testInstance = new BuildXml(Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "Builder", "Generator", "Testing", "TestBuild.xml"));
			BuildXml.SetInstanceForTesting(testInstance);

			var path = Path.Combine(cwshared.FullName, "CargoWise.DbUpgrader", "src", "Database", "CargoWise.Odyssey.Schema", "CargoWise.Odyssey.Schema", "AutoEnterpriseSchema.cs");
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, testContents);
			File.SetAttributes(path, FileAttributes.ReadOnly);
			SourceControl.EnterpriseDatabase.AddFile(path);

			string[] args = { Db.ServerName, Db.DatabaseName, CommandLineOptions.GenerateSchemaColumnList };

			GeneratorEntryPoint entryPoint = new GeneratorEntryPoint(outputDirectory);
			int result = entryPoint.Execute(GeneratorArguments.Parse(args));
			AssertEquals(0, result);
			Assert(testContents != File.ReadAllText(path));
			Assert((File.GetAttributes(path) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly);
		}

		#region Test Helpers

		protected override void SetUp()
		{
			MockSourceControl.Setup();
			base.SetUp();
			cwshared = Directory.CreateDirectory(Path.Combine(TempForTest.TempPath, "CWShared"));
			outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut, cwsharedSourceDirectory: cwshared.FullName);
		}

		protected override void TearDown()
		{
			MockSourceControl.TearDown();
			base.TearDown();
			outputDirectory.Dispose();
			cwshared?.Delete(recursive: true);
			DeleteIfExists(outputDirectory.CheckinLog);
		}

		const string testContents = "BLAH";

		GeneratorOutputDirectory outputDirectory;
		DirectoryInfo cwshared;

		#endregion
	}
}
