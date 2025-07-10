using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(DeleteAuditDuplicateDataForMasterStateAndAlterColumns))]
	class DeleteAuditDuplicateDataForMasterStateAndAlterColumnsTest : AuditDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() =>
			new DeleteAuditDuplicateDataForMasterStateAndAlterColumns();

		protected override void PrepareTestData()
		{
			var deleteConstraintSqlText = @"
IF EXISTS (
    SELECT *
    FROM sys.key_constraints
    WHERE type = 'UQ'
    AND parent_object_id = OBJECT_ID('biadmin.MasterState')
    AND name = 'UX_ParamName'
)
BEGIN
    ALTER TABLE biadmin.MasterState
    DROP CONSTRAINT UX_ParamName;
END";
			var sqlInsert = @"INSERT INTO biadmin.MasterState (ParamName, ParamValue)
				VALUES ('Test_Duplicate_Param', 'Test_Duplicate_Param')";
			TestConnection.ExecuteNonQuery(deleteConstraintSqlText);
			TestConnection.ExecuteNonQuery(sqlInsert);
			TestConnection.ExecuteNonQuery(sqlInsert);
		}

		protected override void AssertTransformationResults()
		{
			var sqlSelect = "SELECT COUNT(*) FROM biadmin.MasterState WHERE ParamName = 'Test_Duplicate_Param'";
			var actualValue = TestConnection.ExecuteScalar(sqlSelect);

			AssertEquals("MasterState count:", 1, actualValue);
		}

		DbConnection testConnection;
		protected override DbConnection TestConnection
		{
			get
			{
				return testConnection ?? (testConnection = Db.NewAdminConnection(Db.AuditDatabaseName));
			}
		}
	}
}
