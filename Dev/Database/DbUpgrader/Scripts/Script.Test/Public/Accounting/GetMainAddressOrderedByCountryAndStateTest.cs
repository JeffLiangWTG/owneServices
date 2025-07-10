using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetMainAddressOrderedByCountryAndState))]
	class GetMainAddressOrderedByCountryAndStateTest : DbCreateScriptTest
	{
		public void TestSimpleRun()
		{
			const string insertSql = @"
DECLARE @OrgPk UNIQUEIDENTIFIER = '357C88A8-9CF6-4DE4-8136-5AF3FC55802C'
DECLARE @OrgAddress1 UNIQUEIDENTIFIER = newid()
DECLARE @OrgAddress2 UNIQUEIDENTIFIER = newid()
DECLARE @OrgAddress3 UNIQUEIDENTIFIER = newid()
DECLARE @OrgAddress4 UNIQUEIDENTIFIER = newid()
DECLARE @OrgAddress5 UNIQUEIDENTIFIER = newid()
DECLARE @OrgAddress6 UNIQUEIDENTIFIER = newid()

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsGlobalAccount) VALUES (@OrgPk, 'TESTORG', 1)

INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_OH, OA_Address1, OA_RN_NKCountryCode, OA_State, OA_Language) VALUES (@OrgAddress1, 'Addr1', @OrgPk, 'Address 1', 'AU', 'NSW', 'EN')
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @OrgAddress1, 'OFC', 1)

INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_OH, OA_Address1, OA_RN_NKCountryCode, OA_State, OA_Language) VALUES (@OrgAddress2, 'Addr2', @OrgPk, 'Address 2', 'AU', 'ACT', 'EN')
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @OrgAddress2, 'OFC', 1)

INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_OH, OA_Address1, OA_RN_NKCountryCode, OA_State, OA_Language) VALUES (@OrgAddress3, 'Addr3', @OrgPk, 'Address 3', 'AU', '', 'EN')
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @OrgAddress3, 'OFC', 1)

INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_OH, OA_Address1, OA_RN_NKCountryCode, OA_State, OA_Language) VALUES (@OrgAddress4, 'Addr4', @OrgPk, 'Address 4', '', 'NSW', 'EN')
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @OrgAddress4, 'OFC', 1)

INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_OH, OA_Address1, OA_RN_NKCountryCode, OA_State, OA_Language) VALUES (@OrgAddress5, 'Addr5', @OrgPk, 'Address 5', '', '', 'EN')
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @OrgAddress5, 'OFC', 1)

INSERT INTO dbo.OrgAddress (OA_PK, OA_Code, OA_OH, OA_Address1, OA_RN_NKCountryCode, OA_State, OA_Language) VALUES (@OrgAddress6, 'Addr6', @OrgPk, 'Address 6', 'CN', 'SHA', 'ZH-CN')
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress) VALUES (newid(), @OrgAddress6, 'OFC', 1)
";

			const string querySql = @"
DECLARE @OrgPk UNIQUEIDENTIFIER = '357C88A8-9CF6-4DE4-8136-5AF3FC55802C'

SELECT * FROM GetMainAddressOrderedByCountryAndState('AU', @OrgPk)
";

			TestConnection.ExecuteNonQuery(insertSql);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, querySql);
			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertEquals("AU", result.Rows[0]["OA_RN_NKCountryCode"].ToString());
			AssertEquals("ACT", result.Rows[0]["OA_State"].ToString());
		}
	}
}

