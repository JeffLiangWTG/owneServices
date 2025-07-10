using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.MasterFiles
{
	[TestedType(typeof(UpdateExcelPasswordToExcelPasswordForModifyingInSdName))]
	class UpdateExcelPasswordToExcelPasswordForModifyingInSdNameTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Db.Connection.ExecuteScalar("select count(*) from StmData where SD_Name='ExcelPassword'"));
			AssertEquals(1, Db.Connection.ExecuteScalar("select count(*) from StmData where SD_Name='ExcelPasswordForModifying'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateExcelPasswordToExcelPasswordForModifyingInSdName();
		}

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();
			helper.CreateStmData();
		}
	}
}
