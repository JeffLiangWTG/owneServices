using System.Data;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Testing
{
	[TestedType(typeof(csfn_GetRegistrationNumbers))]
	class csfn_GetRegistrationNumbersTest : DbCreateScriptTest
	{
		public void Testcsfn_GetRegistrationNumbers()
		{
			var insertSql = @"
			DECLARE @OrgPK1 UNIQUEIDENTIFIER = '4CD7C9F7-9EC9-4CDF-B595-21B90BB9C392';
			DECLARE @OrgPK2 UNIQUEIDENTIFIER = 'BEA9CDC0-C0CC-44EC-BFBE-D9B7DF04D9D6';

			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK1, 'TESTORG1');
			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPK2, 'TESTORG2');

			INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (newid(), 'ABN1234', 'ABN', 'AU', @OrgPK1)
			INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (newid(), 'GST1234', 'GST', 'AU', @OrgPK1)

			INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES (newid(), 'ABN5678', 'ABN', 'AU', @OrgPK2)
			";

			TestConnection.ExecuteNonQuery(insertSql);

			var sqlQuery = "SELECT * FROM csfn_GetRegistrationNumbers(@OrgCodes)";
			var command = Db.Connection.Command(sqlQuery);
			TestDbHelper.AddTVPParameters(command, "@OrgCodes", TVPHelper.TVP_nvarchar, new string[] { "TESTORG1", "TESTORG2" });

			DataTable result = DataUtils.GetDataTableFromCommand(command);

			AssertEquals("Result should have 3 rows", 3, result.Rows.Count);
			var rows = result.Select("OH_Code = 'TESTORG1'");
			AssertEquals("number of rows for org TESTORG1", 2, rows.Length);

			rows = result.Select("OH_Code = 'TESTORG2'");
			AssertEquals("number of rows for org TESTORG2", 1, rows.Length);
		}
	}
}

