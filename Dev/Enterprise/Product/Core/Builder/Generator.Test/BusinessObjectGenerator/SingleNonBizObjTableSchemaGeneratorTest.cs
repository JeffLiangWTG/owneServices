using CargoWise.BuildTools.Testing;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Builder.Generator
{
	sealed class SingleNonBizObjTableSchemaGeneratorTest : TestCase
	{
		public void TestListOfFilesToBeGenerated()
		{
			SingleNonBizObjTableSchemaGenerator testGenerator = new SingleNonBizObjTableSchemaGenerator(Db.DatabaseName, "", "StmNumberCache", outputDirectory);
			string[] fileList = testGenerator.ListOfFilesToBeGenerated;

			AssertEquals("File Count", 6, fileList.Length);
			AssertEquals("File 0 - No file to be generated", "No ", fileList[0].Substring(0, 3));
			AssertEquals("File 2 - No file to be generated", "No ", fileList[2].Substring(0, 3));
			AssertEquals("File 3 - No file to be generated", "No ", fileList[3].Substring(0, 3));
			AssertEquals("File 4 - No file to be generated", "No ", fileList[4].Substring(0, 3));
			AssertEquals("File 5 - No file to be generated", "No ", fileList[5].Substring(0, 3));
		}

		public void TestTableName()
		{
			var testGenerator = new SingleNonBizObjTableSchemaGeneratorForTesting(Db.DatabaseName, "", "StmNumberCache", outputDirectory);
			CombineAssertions(() =>
			{
				AssertEquals("Table Name", "StmNumberCache", testGenerator.TableName);
				AssertEquals("Table Name from DataTable", testGenerator.TableName, testGenerator.Table.TableName);
				AssertEquals("ActualDatabaseNameDuringRegen", Db.DatabaseName, testGenerator.ActualDatabaseNameDuringRegen_Exposed);
				AssertEquals("DatabaseNameForSchemaClasses", "", testGenerator.SubFolderNameForSchemaClasses_Exposed);
				AssertEquals("Full Table Name", Db.DatabaseName + ".dbo.StmNumberCache", testGenerator.FullTableName_Exposed);
			});
		}

		public void TestTableName_ForNonDboSchema()
		{
			var testGenerator = new SingleNonBizObjTableSchemaGeneratorForTesting(Db.DatabaseName, "", "HRCandidateEntitlement", outputDirectory);
			CombineAssertions(() =>
			{
				AssertEquals("Table Name", "HRCandidateEntitlement", testGenerator.TableName);
				AssertEquals("Table Name from DataTable", testGenerator.TableName, testGenerator.Table.TableName);
				AssertEquals("ActualDatabaseNameDuringRegen", Db.DatabaseName, testGenerator.ActualDatabaseNameDuringRegen_Exposed);
				AssertEquals("DatabaseNameForSchemaClasses", "", testGenerator.SubFolderNameForSchemaClasses_Exposed);
				AssertEquals("Full Table Name", Db.DatabaseName + ".hrm.HRCandidateEntitlement", testGenerator.FullTableName_Exposed);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut);
		}

		protected override void TearDown()
		{
			base.TearDown();
			outputDirectory.Dispose();
		}

		GeneratorOutputDirectory outputDirectory;
	}
}
