using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Testing
{
	[TestedType(typeof(fn_GetAssociatedCompanyForOrgProxy))]
	class fn_GetAssociatedCompanyForOrgProxyTest : DbCreateScriptTest
	{
		public void Testfn_GetAssociatedCompanyForOrgProxy()
		{
			var sydneyOrgPK = new Guid("36EB6B6E-AE85-4B3C-B413-827496720280");
			var melbourneOrgPK = new Guid("EED00D6D-87F2-43D1-81CD-726163187C00");
			var goldCoastOrgPK = new Guid("45A5C921-E11F-4FD9-96DB-12FC13549469");
			var brisbaneOrgPK = new Guid("09DF9EF1-E4DC-4D11-BB74-19B9E3D6DC67");

			var insertSql = $@"
DECLARE @SydneyCompanyPK uniqueidentifier = '10E94EF2-92E4-44BA-B0C8-856904EE7F81';
DECLARE @MelbourneCompanyPK uniqueidentifier = '3904D0C5-C5D9-493E-8D00-6823DFCEBF62';
DECLARE @BrisbaneCompanyPK uniqueidentifier = '7A55D481-2155-4FC9-87AA-328F6C2FEB1A';

DECLARE @SydneyOrgPK uniqueidentifier = '{sydneyOrgPK.ToString()}';
DECLARE @MelbourneOrgPK uniqueidentifier = '{melbourneOrgPK.ToString()}';
DECLARE @GoldCoastOrgPK uniqueidentifier = '{goldCoastOrgPK.ToString()}';
DECLARE @BrisbaneOrgPK uniqueidentifier = '{brisbaneOrgPK.ToString()}';

DECLARE @SydneyBranchPK uniqueidentifier = '1B23B69D-0B67-47B8-8732-F153DD0877F3';
DECLARE @MelbourneBranchPK uniqueidentifier = 'C62A6C11-4959-4A14-B037-AA91F684DA47';
DECLARE @BrisbaneBranchPK uniqueidentifier = '1B4535F2-F187-4823-B8D9-EC77A7CE67A5';

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@SydneyOrgPK, 'SYDORG');
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@MelbourneOrgPK, 'MELORG');
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@GoldCoastOrgPK, 'GLDORG');
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@BrisbaneOrgPK, 'BNEORG');

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES (@SydneyCompanyPK, 'SYD', 'AU company1', 'AU', 'AUD', @SydneyOrgPK);
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES (@MelbourneCompanyPK, 'MEL', 'AU company2', 'AU', 'AUD', @MelbourneOrgPK);
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy) VALUES (@BrisbaneCompanyPK, 'BNE', 'AU company3', 'AU', 'AUD', @BrisbaneOrgPK);

INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@SydneyBranchPK, @SydneyCompanyPK, 'BSD', @SydneyOrgPK);
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@MelbourneBranchPK, @MelbourneCompanyPK, 'BML', @MelbourneOrgPK);
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_OH_OrgProxy) VALUES (@BrisbaneBranchPK, @BrisbaneCompanyPK, 'BGD', @GoldCoastOrgPK);
";

			TestConnection.ExecuteNonQuery(insertSql);

			var sqlQuery = "SELECT * FROM fn_GetAssociatedCompanyForOrgProxy(@OrgPK)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", System.Data.SqlDbType.UniqueIdentifier, sydneyOrgPK);

			var result = DataUtils.GetDataTableFromCommand(command);
			var expectedResult = @"
GC_Code           GC_Name
----------------- ---------------------------------------
SYD               AU company1
";

			var testHelper = new TestDbHelperBase(TestConnection);
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("Get associated company for Sydney org, Sydney org is the company org proxy for 'SYD'", result, expectedResult, new List<string>() { });

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", System.Data.SqlDbType.UniqueIdentifier, goldCoastOrgPK);

			result = DataUtils.GetDataTableFromCommand(command);
			expectedResult = @"
GC_Code           GC_Name
----------------- ---------------------------------------
BNE               AU company3
";
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("Get associated company for Gold Coast org, Gold Coast org is the branch org proxy for 'BNE'", result, expectedResult, new List<string>() { });

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", System.Data.SqlDbType.UniqueIdentifier, melbourneOrgPK);

			result = DataUtils.GetDataTableFromCommand(command);
			expectedResult = @"
GC_Code           GC_Name
----------------- ---------------------------------------
MEL               AU company2
";

			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("Get associated company for Melbourne org, Melbourne org is the company org proxy for 'MEL'", result, expectedResult, new List<string>() { });

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@OrgPK", System.Data.SqlDbType.UniqueIdentifier, brisbaneOrgPK);

			result = DataUtils.GetDataTableFromCommand(command);
			expectedResult = @"
GC_Code           GC_Name
----------------- ---------------------------------------
BNE               AU company3
";

			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("Get associated company for Brisbane org, Brisbane org is the company org proxy for 'BNE'", result, expectedResult, new List<string>() { });
		}
	}
}

