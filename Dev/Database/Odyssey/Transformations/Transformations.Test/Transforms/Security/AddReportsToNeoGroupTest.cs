using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	public abstract class AddReportsToNeoGroupTest : DataTransformationTestCase
	{
		protected abstract string ReportType { get; }
		protected abstract string ReportName { get; }
		protected abstract string GroupCode { get; }

		string GroupDescription => $"{ReportType}: {ReportName}";

		protected override void AssertTransformationResults()
		{
			if (DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage"))
			{
				Assert(true);
				return;
			}

			AssertEquals($"Security group {GroupCode} exists",
				expected: true,
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
			var stmMenuItemPK = TestConnection.ExecuteScalar<Guid>($" SELECT SU_PK FROM dbo.StmMenuItem WHERE SU_MenuType = 'WEB' AND SU_BusinessContext = '{ReportType}' AND SU_MenuName = '{ReportName}';");

			AssertGlbSecurity(GroupCode, stmMenuItemPK);
		}

		void AssertGlbSecurity(string groupCode, Guid itemGUID)
		{
			AssertEquals($"Security {itemGUID} exists",
				expected: true,
				Db.Connection.Exists($@"
FROM dbo.GlbSecurity
JOIN dbo.GlbGroup ON GU_GG = GG_PK
WHERE  GG_Code = '{groupCode}'
AND GU_IsValid = 1
AND GU_SecurityItemIsAllowed = 1
AND GU_ItemGUID = '{itemGUID}'
"));
		}

		protected override void PrepareTestData()
		{
			var deleteGroupAndSecurity = $@"
DECLARE @GG_PK UNIQUEIDENTIFIER;
DECLARE @SU_PK UNIQUEIDENTIFIER;

SELECT @GG_PK = GG_PK 
FROM dbo.GlbGroup 
WHERE GG_Code = '{GroupCode}' AND GG_Type = 'ORG';

IF @GG_PK IS NOT NULL
BEGIN
    DELETE FROM dbo.GlbGroup WHERE GG_PK = @GG_PK;

    SELECT @SU_PK = SU_PK 
    FROM dbo.StmMenuItem
    WHERE SU_MenuType = 'WEB' AND SU_BusinessContext = '{ReportType}' AND SU_MenuName = '{ReportName}';

    IF @SU_PK IS NOT NULL
    BEGIN
        DELETE FROM dbo.GlbSecurity WHERE GU_ItemGUID = @SU_PK AND GU_GG = @GG_PK;
    END
END
";
			Db.Connection.ExecuteNonQuery(deleteGroupAndSecurity);
		}
	}
}
