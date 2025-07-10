using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Workflow
{
	public class UpdateExternalRequestWithTypeAndReviewer : DataTransformation
	{
		const string GenericExternalRequestTypePK = "007e8627-31fd-470c-a604-8d440213031b";

		public override string UserDescription => "Populate default ExternalRequestType and reviewer organisation for ExternalRequest";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();
			const string requestTableName = ExternalRequestSchema.Constants.TableName;
			if (!DbObjectCreator.TableExists(Db.Connection, requestTableName)
				|| !DbObjectCreator.CreateColumnIfNotExists(Db.Connection, requestTableName, ExternalRequestSchema.Constants.REQ_RQT_Type, nameof(SqlDbType.UniqueIdentifier))
				|| !DbObjectCreator.CreateColumnIfNotExists(Db.Connection, requestTableName, ExternalRequestSchema.Constants.REQ_OH_ReviewerOrganization, nameof(SqlDbType.UniqueIdentifier)))
			{
				return;
			}

			CreateExternalRequestTypeAndAddDefaultValue();

			var updateExistingRequest = $@"
BEGIN TRY
UPDATE dbo.ExternalRequest
SET
	REQ_RQT_Type = '{GenericExternalRequestTypePK}',
	REQ_SystemLastEditTimeUtc = GETUTCDATE(),
	REQ_SystemLastEditUser = '~BP'
WHERE REQ_RQT_Type IS NULL;

UPDATE dbo.ExternalRequest
SET
	REQ_OH_ReviewerOrganization = IIF(branch.GB_OH_OrgProxy IS NULL, externalRequest.REQ_OH_AssignedOrganization, branch.GB_OH_OrgProxy),
	REQ_SystemLastEditTimeUtc = GETUTCDATE(),
	REQ_SystemLastEditUser = '~BP'
FROM 
	dbo.ExternalRequest externalRequest
	LEFT JOIN dbo.GlbStaff staff on staff.GS_Code = externalRequest.REQ_SystemCreateUser
	LEFT JOIN dbo.GlbBranch branch on branch.GB_PK = staff.GS_GB_HomeBranch
WHERE REQ_OH_ReviewerOrganization IS NULL;
END TRY
BEGIN CATCH
	THROW
END CATCH
";

			Db.Connection.ExecuteNonQuery(updateExistingRequest);
		}

		void CreateExternalRequestTypeAndAddDefaultValue()
		{
			var createExternalRequestTypeSql = @"
CREATE TABLE dbo.ExternalRequestType
(
	RQT_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UX__RQT_PK PRIMARY KEY NONCLUSTERED,
	RQT_RowVersion ROWVERSION NOT NULL,
	RQT_IsSystem BIT NOT NULL DEFAULT 0,
	RQT_IsActive BIT NOT NULL DEFAULT 1,
	RQT_Code VARCHAR(3) NOT NULL DEFAULT '',
	RQT_Description NVARCHAR(100) NOT NULL DEFAULT '',
	RQT_JobType VARCHAR(3) NOT NULL DEFAULT '',
	RQT_SystemCreateTimeUtc SMALLDATETIME NOT NULL,
	RQT_SystemCreateUser VARCHAR(3) NOT NULL DEFAULT '',
	RQT_SystemLastEditTimeUtc SMALLDATETIME NOT NULL,
	RQT_SystemLastEditUser VARCHAR(3) NOT NULL DEFAULT ''
)";
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, ExternalRequestTypeSchema.Constants.TableName, createExternalRequestTypeSql);

			var addDefaultRequestType = $@"
IF NOT EXISTS(SELECT NULL FROM dbo.ExternalRequestType WHERE RQT_PK = '{GenericExternalRequestTypePK}')
BEGIN
INSERT INTO dbo.ExternalRequestType (RQT_PK, RQT_Code, RQT_Description, RQT_IsSystem, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES ('{GenericExternalRequestTypePK}', 'GEN', 'Generic', 1, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
END";
			Db.Connection.ExecuteNonQuery(addDefaultRequestType);
		}
	}
}