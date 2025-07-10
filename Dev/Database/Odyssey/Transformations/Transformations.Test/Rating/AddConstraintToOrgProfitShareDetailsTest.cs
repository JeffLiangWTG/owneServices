using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Rating;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations;

[TestedType(typeof(AddConstraintToOrgProfitShareDetails))]
public class AddConstraintToOrgProfitShareDetailsTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new AddConstraintToOrgProfitShareDetails();

	protected override void AssertTransformationResults()
	{
		AssertEquals("Rows with O4_SendingPortOrCountry len in (1, 3) should not exists",
			false, Db.Connection.Exists(" FROM dbo.OrgProfitShareDetails WHERE LEN(O4_SendingPortOrCountry) in (1, 3)"));
		AssertEquals("Rows with O4_ReceivingPortOrCountry len in (1, 3) should not exists",
			false, Db.Connection.Exists(" FROM dbo.OrgProfitShareDetails WHERE LEN(O4_ReceivingPortOrCountry) in (1, 3)"));

		AssertEquals("Rows with O4_SendingPortOrCountry len not in (1, 3) should not be deleted",
			true, Db.Connection.Exists(" FROM dbo.OrgProfitShareDetails WHERE LEN(O4_SendingPortOrCountry) not in (1, 3)"));
		AssertEquals("Rows with O4_ReceivingPortOrCountry len not in (1, 3) should not be deleted",
			true, Db.Connection.Exists(" FROM dbo.OrgProfitShareDetails WHERE LEN(O4_ReceivingPortOrCountry) not in (1, 3)"));
	}

	protected override void PrepareTestData()
	{
		DBTransformationTestHelper.DropConstraintIfExists("OrgProfitShareDetails", "Constraint_O4_SendingPortOrCountry");
		DBTransformationTestHelper.DropConstraintIfExists("OrgProfitShareDetails", "Constraint_O4_ReceivingPortOrCountry");

		var sql = @"
			DECLARE @OH_PK UNIQUEIDENTIFIER = NEWID();
			DECLARE @O3_PK UNIQUEIDENTIFIER = NEWID();

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
			VALUES (@OH_PK, 'TestOH1Name', GETDATE(), 'USR', GETDATE(), 'USR');

			INSERT INTO dbo.OrgAgentRelationship (O3_PK, O3_OH_SendingAgent, O3_OH_ReceivingAgent, O3_SystemCreateTimeUtc, O3_SystemCreateUser, O3_SystemLastEditTimeUtc, O3_SystemLastEditUser)
			VALUES (@O3_PK, @OH_PK, @OH_PK, GETDATE(), 'USR', GETDATE(), 'USR');
		
			INSERT INTO dbo.OrgProfitShareDetails (O4_PK, O4_O3_OrgProfitShareHeader, O4_SendingPortOrCountry, O4_ReceivingPortOrCountry, O4_SystemCreateTimeUtc, O4_SystemCreateUser, O4_SystemLastEditTimeUtc, O4_SystemLastEditUser)
			VALUES (NEWID(), @O3_PK, '', '', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, 'A', '', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, 'AA', '', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, 'AAA', '', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, 'AAAA', '', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, 'AAAAA', '', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, '', 'A', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, '', 'AA', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, '', 'AAA', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, '', 'AAAA', GETDATE(), 'USR', GETDATE(), 'USR'),
				   (NEWID(), @O3_PK, '', 'AAAAA', GETDATE(), 'USR', GETDATE(), 'USR');
		";

		TestConnection.ExecuteNonQuery(sql);
	}
}