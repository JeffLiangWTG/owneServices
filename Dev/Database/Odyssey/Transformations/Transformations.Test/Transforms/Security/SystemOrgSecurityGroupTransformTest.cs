using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	public abstract class SystemOrgSecurityGroupTransformTest : DataTransformationTestCase
	{
		protected abstract bool IsSystemGroup { get; }
		protected abstract string GroupCode { get; }

		protected abstract string GroupDescription { get; }

		protected abstract string RoleName { get; }

		protected abstract bool IsNeoDefaultRole { get; }

		protected override void AssertTransformationResults()
		{
			if (DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"))
			{
				Assert(true);
				return;
			}

			AssertEquals($"User security group {GroupCode} does not exist",
				false,
				Db.Connection.Exists($@"
FROM
dbo.GlbGroup 
WHERE GG_Code = '{GroupCode}'
AND GG_Type = 'ORG'
AND GG_IsSystemDefined = 0
"));

			AssertEquals($"Security group {GroupCode} exists",
				true,
				Db.Connection.Exists($@"
FROM
dbo.GlbGroup 
WHERE GG_Code = '{GroupCode}'
AND GG_Desc = '{GroupDescription}'
AND GG_Type = 'ORG'
AND GG_IsSystemDefined = 1
AND GG_IsSecurityEnabled = 0
AND GG_IsValid = 1
AND GG_IsActive = 1
"));

			var renamedGroup = GroupCode.PadRight(GlbGroupSchema.GG_Code.MaxLength, '_');
			renamedGroup = $"{renamedGroup.Substring(0, renamedGroup.Length - 3)}_01";
			AssertEquals($"Security group {renamedGroup} exists",
	!IsSystemGroup,
	Db.Connection.Exists($@"
FROM
dbo.GlbGroup 
WHERE GG_Code = '{renamedGroup}'
AND GG_Desc = '{GroupDescription}'
AND GG_Type = 'ORG'
AND GG_IsSystemDefined = 0
AND GG_IsSecurityEnabled = 0
AND GG_IsValid = 1
AND GG_IsActive = 1
"));

			AssertEquals($"Group {GroupCode} contains role {RoleName}",
				true,
				Db.Connection.Exists($@"
FROM
dbo.GlbGroupRole
WHERE GGR_RoleName = '{RoleName}' AND GGR_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code = '{GroupCode}' AND GG_IsSystemDefined = 1 )
"));

			AssertEquals($"Group NEOROLES contains role {RoleName}",
				IsNeoDefaultRole,
				Db.Connection.Exists($@"
FROM
dbo.GlbGroupRole
WHERE GGR_RoleName = '{RoleName}' AND GGR_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code = 'NEOROLES' AND GG_IsSystemDefined = 1 )
"));
		}

		protected override void PrepareTestData()
		{
			Db.Connection.ExecuteNonQuery($@"
DELETE FROM dbo.GlbGroupRole
WHERE GGR_GG_Group IN
( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code IN ('{GroupCode}', 'NEOROLES') );

DELETE FROM dbo.GlbGroup
WHERE GG_Type = 'ORG' AND GG_Code = '{GroupCode}';

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
	,GG_IsSystemDefined = {(IsSystemGroup ? 1 : 0)}
	,GG_IsSecurityEnabled = 0
	,GG_IsValid = 1
	,GG_IsActive = 1
	,GG_SystemCreateTimeUtc = GETUTCDATE()
	,GG_SystemCreateUser = 'E'
	,GG_SystemLastEditTimeUtc = GETUTCDATE()
	,GG_SystemLastEditUser = 'E';
");
		}
	}

	public abstract class SystemOrgSecurityGroupTransformTestEDI : DataTransformationTestCase
	{
		protected abstract string GroupCode { get; }

		protected abstract string RoleName { get; }

		protected override void AssertTransformationResults()
		{
			AssertEquals(true, DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"));
			AssertEquals(false, Db.Connection.Exists($" FROM dbo.GlbGroup WHERE GG_Type = 'ORG' AND GG_Code = '{GroupCode}' AND GG_IsSystemDefined = 1 "));
			AssertEquals(false, Db.Connection.Exists($" FROM dbo.GlbGroupRole WHERE GGR_RoleName = '{RoleName}' "));
		}

		protected override void PrepareTestData()
		{
			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "EdiBilledUsage", "CREATE TABLE dbo.EdiBilledUsage (ID INT)");

			Db.Connection.ExecuteNonQuery($@"
DELETE FROM dbo.GlbGroupRole
WHERE GGR_RoleName = '{RoleName}';

DELETE FROM dbo.GlbGroup
WHERE GG_Type = 'ORG' AND GG_Code = '{GroupCode}';
");
		}
	}
}
