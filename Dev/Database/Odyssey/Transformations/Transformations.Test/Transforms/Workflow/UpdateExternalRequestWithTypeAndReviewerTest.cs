using System;
using System.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Workflow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Workflow
{
	[TestedType(typeof(UpdateExternalRequestWithTypeAndReviewer))]
	public class UpdateExternalRequestWithTypeAndReviewerTest : DataTransformationTestCase
	{
		const string GenericExternalRequestTypePK = "007e8627-31fd-470c-a604-8d440213031b";

		const string StaffLoginCode = "STY";

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateExternalRequestWithTypeAndReviewer();

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();
			AssignedOrgPK = Guid.NewGuid();
			StaffOrgPK = Guid.NewGuid();
			var staffPKWithNoBranch = Guid.NewGuid();
			helper.CreateOrgOnly(AssignedOrgPK, "AssignedOrgPK", "Organization AssignedOrg");
			helper.CreateStaff(staffPKWithNoBranch, "Staff No Branch", "STN");
			CreateStaffLinkedToOrg(helper);

			DropTableColumns();

			var createPreparedData = @"
INSERT INTO dbo.ExternalRequest (REQ_PK, REQ_RequestID, REQ_ParentID, REQ_ParentTableCode, REQ_Description, REQ_OH_AssignedOrganization, REQ_SystemCreateTimeUtc, REQ_SystemCreateUser, REQ_SystemLastEditTimeUtc, REQ_SystemLastEditUser)
VALUES (NEWID(), 'REQ00001', NEWID(), 'JSB', 'desc', @AssignedOrgPK, '20240101', 'ZZ', '20240101', 'ZZ');
INSERT INTO dbo.ExternalRequest (REQ_PK, REQ_RequestID, REQ_ParentID, REQ_ParentTableCode, REQ_Description, REQ_OH_AssignedOrganization, REQ_SystemCreateTimeUtc, REQ_SystemCreateUser, REQ_SystemLastEditTimeUtc, REQ_SystemLastEditUser)
VALUES (NEWID(), 'REQ00002', NEWID(), 'JSB', 'desc', @AssignedOrgPK, '20240101', 'STN', '20240101', 'ZZ');
INSERT INTO dbo.ExternalRequest (REQ_PK, REQ_RequestID, REQ_ParentID, REQ_ParentTableCode, REQ_Description, REQ_OH_AssignedOrganization, REQ_SystemCreateTimeUtc, REQ_SystemCreateUser, REQ_SystemLastEditTimeUtc, REQ_SystemLastEditUser)
VALUES (NEWID(), 'REQ00003', NEWID(), 'JSB', 'desc', @AssignedOrgPK, '20240101', @Staff, '20240101', 'ZZ');";

			using var cmd = TestConnection.Command(createPreparedData);
			cmd.AddParameter("@AssignedOrgPK", SqlDbType.UniqueIdentifier, AssignedOrgPK);
			cmd.AddParameter("@Staff", SqlDbType.VarChar, StaffLoginCode);
			cmd.ExecuteNonQuery();
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(3, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequest WHERE REQ_RQT_Type = '{GenericExternalRequestTypePK}'"));
			AssertEquals(2, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequest WHERE REQ_OH_ReviewerOrganization = '{AssignedOrgPK}'"));
			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequest WHERE REQ_OH_ReviewerOrganization = '{StaffOrgPK}'"));
		}

		public void TestWhenExternalRequestTableNotExists()
		{
			DBTransformationTestHelper.DropTableIfExists(ExternalRequestSchema.Constants.TableName, TestConnection);
			AssertNoExceptionThrown(() => RunTransformation());
		}

		public void TestWhenExternalRequestTypeTableNotExists()
		{
			DBTransformationTestHelper.DropTableIfExists(ExternalRequestSchema.Constants.TableName, TestConnection);
			DBTransformationTestHelper.DropTableIfExists(ProcessTemplateValidationActionSchema.Constants.TableName, TestConnection);
			DBTransformationTestHelper.DropTableIfExists(ProcessTemplateValidationSchema.Constants.TableName, TestConnection);
			DBTransformationTestHelper.DropTableIfExists(ExternalRequestTypeSchema.Constants.TableName, TestConnection);
			AssertNoExceptionThrown(() => RunTransformation());
		}

		public void TestWhenExternalRequestHasTypeOrReviewer()
		{
			var helper = new TransformationTestDataCreator();
			var reviewerOrgPK = Guid.NewGuid();
			var requestTypePK = Guid.NewGuid();
			helper.CreateOrgOnly(reviewerOrgPK, "ReviewerOrgPK", "Organization ReviewerOrg");
			CreateStaffLinkedToOrg(helper);

			TestConnection.ExecuteNonQuery(@$"
INSERT INTO dbo.ExternalRequestType (RQT_PK, RQT_Code, RQT_Description, RQT_IsSystem, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES ('{requestTypePK}', 'QTY', 'New Quantity Request', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')");

			AlterTableColumns();
			var createPreparedData = $@"
INSERT INTO dbo.ExternalRequest (REQ_PK, REQ_RequestID, REQ_ParentID, REQ_ParentTableCode, REQ_Description, REQ_OH_AssignedOrganization, REQ_OH_ReviewerOrganization, REQ_SystemCreateTimeUtc, REQ_SystemCreateUser, REQ_SystemLastEditTimeUtc, REQ_SystemLastEditUser)
VALUES (NEWID(), 'REQ00011', NEWID(), 'JSB', 'desc', '{reviewerOrgPK}', '{reviewerOrgPK}', '20240101', 'ZZ', '20240101', 'ZZ');
INSERT INTO dbo.ExternalRequest (REQ_PK, REQ_RequestID, REQ_ParentID, REQ_ParentTableCode, REQ_Description, REQ_OH_AssignedOrganization, REQ_RQT_Type, REQ_SystemCreateTimeUtc, REQ_SystemCreateUser, REQ_SystemLastEditTimeUtc, REQ_SystemLastEditUser)
VALUES (NEWID(), 'REQ00012', NEWID(), 'JSB', 'desc', '{reviewerOrgPK}', '{requestTypePK}', '20240101', '{StaffLoginCode}', '20240101', 'ZZ');
INSERT INTO dbo.ExternalRequest (REQ_PK, REQ_RequestID, REQ_ParentID, REQ_ParentTableCode, REQ_Description, REQ_OH_AssignedOrganization, REQ_OH_ReviewerOrganization, REQ_RQT_Type, REQ_SystemCreateTimeUtc, REQ_SystemCreateUser, REQ_SystemLastEditTimeUtc, REQ_SystemLastEditUser)
VALUES (NEWID(), 'REQ00013', NEWID(), 'JSB', 'desc', '{reviewerOrgPK}', '{reviewerOrgPK}', '{requestTypePK}', '20240101', '{StaffLoginCode}', '20240101', 'ZZ');";
			TestConnection.ExecuteNonQuery(createPreparedData);

			AssertNoExceptionThrown(() => RunTransformation());
			AssertEquals(2, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequest WHERE REQ_OH_ReviewerOrganization = '{reviewerOrgPK}'"));
			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequest WHERE REQ_OH_ReviewerOrganization = '{StaffOrgPK}'"));
			AssertEquals(2, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequest WHERE REQ_OH_ReviewerOrganization = '{reviewerOrgPK}'"));
			AssertEquals(2, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequest WHERE REQ_RQT_Type = '{requestTypePK}'"));
			AssertEquals(1, TestConnection.ExecuteScalar($"SELECT count(1) FROM dbo.ExternalRequest WHERE REQ_RQT_Type = '{GenericExternalRequestTypePK}'"));
		}

		public void TestWhenExternalRequestTypeTableHasData()
		{
			TestConnection.ExecuteNonQuery(@$"
INSERT INTO dbo.ExternalRequestType (RQT_PK, RQT_Code, RQT_Description, RQT_IsSystem, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES (NEWID(), 'QTY', 'New Quantity Request', 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
IF NOT EXISTS (SELECT null FROM dbo.ExternalRequestType WHERE RQT_PK = '{GenericExternalRequestTypePK}')
BEGIN
INSERT INTO dbo.ExternalRequestType (RQT_PK, RQT_Code, RQT_Description, RQT_IsSystem, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES ('{GenericExternalRequestTypePK}', 'GEN', 'Generic', 1, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
END");

			AssertNoExceptionThrown(() => RunTransformation());
			AssertEquals(2, TestConnection.ExecuteScalar("SELECT count(1) FROM dbo.ExternalRequestType"));
		}

		void CreateStaffLinkedToOrg(TransformationTestDataCreator helper)
		{
			helper.CreateOrgOnly(StaffOrgPK, "StaffOrgPK", "Organization StaffOrg");
			var companyPK = helper.CreateCompany("CON", "CN");
			var branckPK = Guid.NewGuid();
			helper.CreateBranch(branckPK, "BRN", "Port", companyPK, StaffOrgPK);
			var pk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_GB_HomeBranch, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@pk, @code, @login, @branch, GetUtcDate(), '~BP', GetUtcDate(), '~BP')", parameters =>
			{
				parameters.AddParameterBasedOnDbColumn("@pk", pk, GlbStaffSchema.PK);
				parameters.AddParameterBasedOnDbColumn("@code", "STY", GlbStaffSchema.GS_Code);
				parameters.AddParameterBasedOnDbColumn("@login", "Staff Has Branch", GlbStaffSchema.GS_LoginName);
				parameters.AddParameterBasedOnDbColumn("@branch", branckPK, GlbStaffSchema.GS_GB_HomeBranch);
			});
		}

		void DropTableColumns()
		{
			DBTransformationTestHelper.DropIndexIfExists(ExternalRequestSchema.Constants.TableName, "FK_RX__REQ_OH_ReviewerOrganization");
			DBTransformationTestHelper.DropIndexIfExists(ExternalRequestSchema.Constants.TableName, "FK_RX__REQ_RQT_Type");
			DBTransformationTestHelper.DropConstraintIfExists(ExternalRequestSchema.Constants.TableName, "ExternalRequest_REQ_RQT_Type_FK2_ExternalRequestType_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(ExternalRequestSchema.Constants.TableName, "ExternalRequest_REQ_OH_ReviewerOrganization_FK2_OrgHeader_RRR_120N");
			DBTransformationTestHelper.DropColumnIfExists(ExternalRequestSchema.Constants.TableName, ExternalRequestSchema.Constants.REQ_OH_ReviewerOrganization, TestConnection);
			DBTransformationTestHelper.DropColumnIfExists(ExternalRequestSchema.Constants.TableName, ExternalRequestSchema.Constants.REQ_RQT_Type, TestConnection);
		}

		void AlterTableColumns()
		{
			TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.ExternalRequest ALTER COLUMN REQ_RQT_Type UNIQUEIDENTIFIER NULL;");
		}

		Guid AssignedOrgPK;
		Guid StaffOrgPK;
	}
}
