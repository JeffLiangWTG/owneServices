using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public abstract class SystemOrgSecurityGroupTransform : DataTransformation
	{
		public override string UserDescription => $"Add system security group for {GroupDescription}";

		protected abstract string GroupCode { get; }

		protected abstract string GroupDescription { get; }

		protected abstract string RoleName { get; }

		protected abstract bool IsNeoDefaultRole { get; }

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

			UpdateUserCreatedGroup(GroupCode);

			Db.Connection.ExecuteNonQuery(TransformQuery);
			if (IsNeoDefaultRole)
			{
				Db.Connection.ExecuteNonQuery(AddNeoDefaultTransformQuery);
			}
		}

		string TransformQuery => $@"
DECLARE @GG_PK UNIQUEIDENTIFIER;
DECLARE @CurrentUTC  SMALLDATETIME = GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = '{GroupCode}' AND GG_Type = 'ORG' AND GG_IsSystemDefined = 1)
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
	SELECT GG_PK = NEWID()
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

SELECT TOP 1 @GG_PK=GG_PK FROM dbo.GlbGroup WHERE GG_Code = '{GroupCode}' AND GG_Type = 'ORG' AND GG_IsSystemDefined = 1

IF NOT @GG_PK IS NULL AND NOT EXISTS (SELECT 1 FROM dbo.GlbGroupRole WHERE GGR_RoleName = '{RoleName}' AND GGR_GG_Group = @GG_PK)
BEGIN

	INSERT INTO dbo.GlbGroupRole
	(
		GGR_PK
		, GGR_RoleName
		, GGR_GG_Group
		, GGR_SystemCreateTimeUtc
		, GGR_SystemCreateUser
		, GGR_SystemLastEditTimeUtc
		, GGR_SystemLastEditUser
	)
	SELECT GGR_PK = NEWID()
		, GGR_RoleName = '{RoleName}'
		, GGR_GG_Group = @GG_PK
		, GGR_SystemCreateTimeUtc = @CurrentUTC
		, GGR_SystemCreateUser = 'E'
		, GGR_SystemLastEditTimeUtc = @CurrentUTC
		, GGR_SystemLastEditUser = 'E';
END
";
		string AddNeoDefaultTransformQuery => $@"
DECLARE @CurrentUTC  SMALLDATETIME = GETUTCDATE();
DECLARE @NEOROLES_PK UNIQUEIDENTIFIER;

SELECT @NEOROLES_PK = GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'NEOROLES' AND GG_Type = 'ORG' AND GG_IsSystemDefined = 1;

IF NOT @NEOROLES_PK IS NULL AND NOT EXISTS (SELECT 1 FROM dbo.GlbGroupRole WHERE GGR_RoleName = '{RoleName}' AND GGR_GG_Group = @NEOROLES_PK)
BEGIN
	INSERT INTO dbo.GlbGroupRole
	(
		GGR_PK
		, GGR_RoleName
		, GGR_GG_Group
		, GGR_SystemCreateTimeUtc
		, GGR_SystemCreateUser
		, GGR_SystemLastEditTimeUtc
		, GGR_SystemLastEditUser
	)
	SELECT GGR_PK = NEWID()
		, GGR_RoleName = '{RoleName}'
		, GGR_GG_Group = @NEOROLES_PK
		, GGR_SystemCreateTimeUtc = @CurrentUTC
		, GGR_SystemCreateUser = 'E'
		, GGR_SystemLastEditTimeUtc = @CurrentUTC
		, GGR_SystemLastEditUser = 'E';
END
";

		void UpdateUserCreatedGroup(string groupCode)
		{
			if (!Db.Connection.Exists($@"FROM dbo.GlbGroup WHERE GG_Code = @GroupCode AND GG_IsSystemDefined = 0",
											cmd => cmd.AddParameter("@GroupCode", SqlDbType.VarChar, groupCode)))
			{
				return;
			}

			var groupCodeSubstring = groupCode.PadRight(GlbGroupSchema.GG_Code.MaxLength, '_');
			groupCodeSubstring = groupCodeSubstring.Substring(0, groupCodeSubstring.Length - 3);

			for (var i = 1; i < 100; i++)
			{
				var newGroupCode = groupCodeSubstring + "_" + i.ToString("00");

				if (!Db.Connection.Exists($@"FROM dbo.GlbGroup WHERE GG_Code = @NewGroupCode",
									cmd => cmd.AddParameter("@NewGroupCode", SqlDbType.VarChar, newGroupCode)))
				{
					var sqlText = $"UPDATE dbo.GlbGroup SET GG_Code = @NewGroupCode, GG_SystemLastEditTimeUtc = GETUTCDATE(), GG_SystemLastEditUser = 'E' WHERE GG_Code = @GroupCode";
					using (var cmd = Db.Connection.Command(sqlText))
					{
						cmd.AddParameter("@NewGroupCode", SqlDbType.VarChar, newGroupCode);
						cmd.AddParameter("@GroupCode", SqlDbType.VarChar, groupCode);
						cmd.ExecuteNonQuery();
					}
					break;
				}
			}
		}
	}
}
