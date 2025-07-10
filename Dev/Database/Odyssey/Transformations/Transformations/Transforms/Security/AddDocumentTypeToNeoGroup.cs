using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public abstract class AddDocumentTypeToNeoGroup : DataTransformation
	{
		protected abstract string DocumentGroup { get; }
		protected abstract string DocumentType { get; }
		protected abstract string DocumentDescription { get; }
		string GroupCode => $"DOC_{DocumentGroup}_{DocumentType}";
		string GroupDescription => $"Document {DocumentType} for {DocumentGroup} ({DocumentDescription})";
		protected string SecurityRight => $"RefDocType:{DocumentGroup}:{DocumentType}";
		public override string UserDescription => $"Add system security group for {GroupDescription}";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"))  //ediprod
			{
				return;
			}

			if (!DbObjectCreator.TableExists(Db.Connection, GlbGroupSchema.Constants.TableName)
				|| !DbObjectCreator.TableExists(Db.Connection, GlbGroupRoleSchema.Constants.TableName))
			{
				return;
			}

			Db.Connection.ExecuteNonQuery(GlbGroupTransformQuery);
			Db.Connection.ExecuteNonQuery(NeoAccessGlbSecurityTransformQuery);
			Db.Connection.ExecuteNonQuery(GlbSecurityTransformQuery);
		}

		string GlbGroupTransformQuery => $@"
DECLARE @GG_PK UNIQUEIDENTIFIER = NEWID();
DECLARE @CurrentUTC  SMALLDATETIME = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = '{GroupCode}' AND GG_Type = 'ORG')
BEGIN
	INSERT INTO dbo.GlbGroup
	(
		GG_PK
		,GG_Code
		,GG_Desc
		,GG_Type
		,GG_IsSystemDefined
		,GG_IsSecurityEnabled
		,GG_IsValid
		,GG_IsActive
		,GG_SystemCreateTimeUtc
		,GG_SystemCreateUser
		,GG_SystemLastEditTimeUtc
		,GG_SystemLastEditUser
	)
	SELECT GG_PK = @GG_PK
		,GG_Code = '{GroupCode}'
		,GG_Desc = '{GroupDescription}'
		,GG_Type = 'ORG'
		,GG_IsSystemDefined = 1
		,GG_IsSecurityEnabled = 0
		,GG_IsValid = 1
		,GG_IsActive = 1
		,GG_SystemCreateTimeUtc = @CurrentUTC
		,GG_SystemCreateUser = 'E'
		,GG_SystemLastEditTimeUtc = @CurrentUTC
		,GG_SystemLastEditUser = 'E';
END
";

		string NeoAccessGlbSecurityTransformQuery => $@"
DECLARE @CurrentUTC  SMALLDATETIME = GETUTCDATE();
DECLARE @NEODOCACCESS_PK UNIQUEIDENTIFIER;

SELECT @NEODOCACCESS_PK = GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'NEODOCACCESS' AND GG_Type = 'ORG';

IF @NEODOCACCESS_PK IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{SecurityRight}' AND GU_GG = @NEODOCACCESS_PK)
BEGIN
	INSERT INTO dbo.GlbSecurity
		(
		GU_PK
		,GU_GG
		,GU_IsValid
		,GU_SecurityItemIsAllowed
        ,GU_SystemCreateTimeUtc
        ,GU_SystemCreateUser
        ,GU_SystemLastEditTimeUtc
        ,GU_SystemLastEditUser
        ,GU_SecurityRight
	)
	SELECT GU_PK = NEWID()
        ,GU_GG = @NEODOCACCESS_PK
		,GU_IsValid = 1
        ,GU_SecurityItemIsAllowed = 1
        ,GU_SystemCreateTimeUtc = @CurrentUTC
        ,GU_SystemCreateUser = 'E'
        ,GU_SystemLastEditTimeUtc = @CurrentUTC
        ,GU_SystemLastEditUser = 'E'
        ,GU_SecurityRight = '{SecurityRight}';
END
";
		string GlbSecurityTransformQuery => $@"
DECLARE @GG_PK UNIQUEIDENTIFIER;
DECLARE @CurrentUTC  SMALLDATETIME = GETUTCDATE();

SELECT @GG_PK = GG_PK FROM dbo.GlbGroup WHERE GG_Code = '{GroupCode}' AND GG_Type = 'ORG';

IF NOT EXISTS(SELECT 1 FROM dbo.GlbSecurity WHERE GU_SecurityRight = '{SecurityRight}' AND GU_GG = @GG_PK)
BEGIN
	INSERT INTO dbo.GlbSecurity
	(
		GU_PK
        ,GU_GG
        ,GU_IsValid
        ,GU_SecurityItemIsAllowed
        ,GU_SystemCreateTimeUtc
        ,GU_SystemCreateUser
        ,GU_SystemLastEditTimeUtc
        ,GU_SystemLastEditUser
        ,GU_SecurityRight
	)
	SELECT GU_PK = NEWID()
        ,GU_GG = @GG_PK
		,GU_IsValid = 1
        ,GU_SecurityItemIsAllowed = 1
        ,GU_SystemCreateTimeUtc = @CurrentUTC
        ,GU_SystemCreateUser = 'E'
        ,GU_SystemLastEditTimeUtc = @CurrentUTC
        ,GU_SystemLastEditUser = 'E'
        ,GU_SecurityRight = '{SecurityRight}';
END
";
	}
}
