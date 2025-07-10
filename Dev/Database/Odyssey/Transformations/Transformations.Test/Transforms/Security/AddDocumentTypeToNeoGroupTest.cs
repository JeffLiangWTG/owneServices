using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	public abstract class AddDocumentTypeToNeoGroupTest : DataTransformationTestCase
	{
		protected abstract string DocumentGroup { get; }
		protected abstract string DocumentType { get; }
		protected abstract string DocumentDescription { get; }

		string GroupCode => $"DOC_{DocumentGroup}_{DocumentType}";
		string GroupDescription => $"Document {DocumentType} for {DocumentGroup} ({DocumentDescription})";
		protected string SecurityRight => $"RefDocType:{DocumentGroup}:{DocumentType}";

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
		}

		public void TestNoExceptionThrownIfGlbGroupAlreadyExist()
		{
			var deleteGroup = $@"
			DELETE FROM dbo.GlbGroup
			WHERE GG_Type = 'ORG' AND GG_Code IN ('{GroupCode}', 'NEODOCACCESS');";

			var deleteSecurity = $@"
			DELETE FROM dbo.GlbSecurity
			WHERE GU_SecurityRight='{SecurityRight}';";

			var insertGroup = $@"INSERT INTO dbo.GlbGroup
	(	 GG_PK
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
	VALUES
	(	 NEWID()
		,'{GroupCode}'
		,'{GroupDescription}'
		,'ORG'
		,1
		,0
		,1
		,1
		,GETUTCDATE()
		,'E'
		,GETUTCDATE()
		,'E')
";
			Db.Connection.ExecuteNonQuery(deleteGroup);
			Db.Connection.ExecuteNonQuery(deleteSecurity);
			Db.Connection.ExecuteNonQuery(insertGroup);

			AssertNoExceptionThrown("No exception schould be thrown, if Group already exist", () =>
			{
				RunTransformation();
			});
		}

		public void TestNoExceptionThrownIfGlbSecurityAlreadyExist()
		{
			var deleteGroup = $@"
			DELETE FROM dbo.GlbGroup
			WHERE GG_Type = 'ORG' AND GG_Code IN ('{GroupCode}', 'NEODOCACCESS');";

			var deleteSecurity = $@"
			DELETE FROM dbo.GlbSecurity
			WHERE GU_SecurityRight='{SecurityRight}';";

			var insertGroupAndSecurity = $@"INSERT INTO dbo.GlbGroup
	(	 GG_PK
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
	VALUES
	(	 NEWID()
		,'{GroupCode}'
		,'{GroupDescription}'
		,'ORG'
		,1
		,0
		,1
		,1
		,GETUTCDATE()
		,'E'
		,GETUTCDATE()
		,'E')

	INSERT INTO dbo.GlbSecurity 
	(    GU_PK
		,GU_IsValid
		,GU_SecurityItemIsAllowed
		,GU_GG
		,GU_SecurityRight
		,GU_ItemGUID
		,GU_SystemCreateTimeUtc
		,GU_SystemCreateUser
		,GU_SystemLastEditTimeUtc
		,GU_SystemLastEditUser
	)
	VALUES
	(	 NEWID()
		,1
		,1
		,(SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = '{GroupCode}' AND GG_Type = 'ORG')
		,'{SecurityRight}'
		,NEWID()
		,GETUTCDATE()
		,'E'
		,GETUTCDATE()
		,'E')";

			Db.Connection.ExecuteNonQuery(deleteGroup);
			Db.Connection.ExecuteNonQuery(deleteSecurity);
			Db.Connection.ExecuteNonQuery(insertGroupAndSecurity);

			AssertNoExceptionThrown("No exception schould be thrown, if Group already exist", () =>
			{
				RunTransformation();
			});
		}

		protected override void PrepareTestData()
		{
			var deleteGroup = $@"
			DELETE FROM dbo.GlbGroup
			WHERE GG_Type = 'ORG' AND GG_Code IN ('{GroupCode}', 'NEODOCACCESS');
			";

			var deleteSecurity = $@"
			DELETE FROM dbo.GlbSecurity
			WHERE GU_SecurityRight='{SecurityRight}';
			";

			Db.Connection.ExecuteNonQuery(deleteGroup);
			Db.Connection.ExecuteNonQuery(deleteSecurity);
		}
	}
}
