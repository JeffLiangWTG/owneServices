using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(UpdateBiAuditApiRegistryItem))]
	class UpdateBiAuditApiRegistryItemTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateBiAuditApiRegistryItem();
		}

		protected override void PrepareTestData()
		{
			var sqlDelete = "DELETE FROM dbo.StmData WHERE SD_Name = 'BiAuditAPI'";
			TestConnection.ExecuteNonQuery(sqlDelete);

			var sqlInsert = @"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue)
				VALUES (NEWID(), 'BiAuditAPI', CONVERT(VARBINARY(MAX), N'False'))";
			TestConnection.ExecuteNonQuery(sqlInsert);
		}

		protected override void AssertTransformationResults()
		{
			var sqlSelect = "SELECT CONVERT(NVARCHAR(MAX), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = 'BiAuditAPI'";
			var actualValue = TestConnection.ExecuteScalar(sqlSelect);

			AssertEquals("BiAuditAPI value:", "True", actualValue);
		}
	}
}
