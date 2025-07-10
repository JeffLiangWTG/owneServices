using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Reports;
using CargoWise.Definitions;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Reports.Testing
{
	[TestedType(typeof(fn_NVPMGetGrossValueOfInvoices))]
	class fn_NVPMGetGrossValueOfInvoicesTest : DbCreateScriptTest
	{
		public void Testfn_NVPMGetGrossValueOfInvoices()
		{
			var insertSql = @"
DECLARE @SydneyCompanyPK uniqueidentifier = '10E94EF2-92E4-44BA-B0C8-856904EE7F81';
DECLARE @MelbourneCompanyPK uniqueidentifier = '3904D0C5-C5D9-493E-8D00-6823DFCEBF62';
DECLARE @BrisbaneCompanyPK uniqueidentifier = '7A55D481-2155-4FC9-87AA-328F6C2FEB1A';

DECLARE @SydneyOrgPK uniqueidentifier = '36EB6B6E-AE85-4B3C-B413-827496720280';
DECLARE @MelbourneOrgPK uniqueidentifier = 'EED00D6D-87F2-43D1-81CD-726163187C00';
DECLARE @GoldCoastOrgPK uniqueidentifier = '45A5C921-E11F-4FD9-96DB-12FC13549469';
DECLARE @BrisbaneOrgPK uniqueidentifier = '09DF9EF1-E4DC-4D11-BB74-19B9E3D6DC67';

DECLARE @SydneyBranchPK uniqueidentifier = '1B23B69D-0B67-47B8-8732-F153DD0877F3';
DECLARE @MelbourneBranchPK uniqueidentifier = 'C62A6C11-4959-4A14-B037-AA91F684DA47';
DECLARE @BrisbaneBranchPK uniqueidentifier = '1B4535F2-F187-4823-B8D9-EC77A7CE67A5';

DECLARE @DepartmentPK uniqueidentifier = '64DAD85E-4778-48D8-B4FE-5E1680405130';

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

INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPK);

INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_Ledger, AH_TransactionType, AH_GC, AH_GB, AH_GE, AH_OH, AH_DueDate, AH_InvoiceDate, AH_InvoiceAmount, AH_GSTAmount, AH_LocalTaxAmountOtherTaxes, AH_OutstandingAmount) 
VALUES 
	(newid(), '001', 'AR', 'INV', @SydneyCompanyPK, @SydneyBranchPK, @DepartmentPK, @MelbourneOrgPK, '2017-2-2', '2017-2-2', 100, 10, 1, 0),
	(newid(), '002', 'AR', 'INV', @SydneyCompanyPK, @SydneyBranchPK, @DepartmentPK, @GoldCoastOrgPK, '2017-2-1', '2017-2-2', 200, 20, 2, 0),
	(newid(), '003', 'AR', 'INV', @SydneyCompanyPK, @SydneyBranchPK,  @DepartmentPK, @BrisbaneOrgPK, '2017-2-10', '2017-2-2', 300, 30, 3, 0),

	(newid(), '004', 'AR', 'INV', @MelbourneCompanyPK, @MelbourneBranchPK, @DepartmentPK, @SydneyOrgPK, '2017-2-1', '2017-2-2', 400, 40, 4, 0),
	(newid(), '005', 'AR', 'INV', @MelbourneCompanyPK, @MelbourneBranchPK, @DepartmentPK, @GoldCoastOrgPK, '2017-2-1', '2017-2-2', 500, 50, 5, 0),
	(newid(), '006', 'AR', 'INV', @MelbourneCompanyPK, @MelbourneBranchPK, @DepartmentPK, @BrisbaneOrgPK, '2017-2-1', '2017-2-2', 600, 60, 6, 0),

	(newid(), '007', 'AR', 'INV', @BrisbaneCompanyPK, @BrisbaneBranchPK, @DepartmentPK, @SydneyOrgPK, '2017-2-1', '2017-2-2', 700, 70, 7, 0),
	(newid(), '008', 'AR', 'INV', @BrisbaneCompanyPK, @BrisbaneBranchPK, @DepartmentPK, @MelbourneOrgPK, '2017-2-1', '2017-2-2', 800, 80, 8, 0),
	(newid(), '009', 'AR', 'INV', @BrisbaneCompanyPK, @BrisbaneBranchPK, @DepartmentPK, @BrisbaneOrgPK, '2017-2-1', '2017-2-2', 900, 90, 9, 0),


	(newid(), '001', 'AP', 'INV', @SydneyCompanyPK, @SydneyBranchPK, @DepartmentPK, @MelbourneOrgPK, '2017-2-2', '2017-2-2', 100, 10, 1, 0),
	(newid(), '002', 'AP', 'INV', @SydneyCompanyPK, @SydneyBranchPK, @DepartmentPK, @GoldCoastOrgPK, '2017-2-1', '2017-2-2', 200, 20, 2, 0),
	(newid(), '003', 'AP', 'INV', @SydneyCompanyPK, @SydneyBranchPK,  @DepartmentPK, @BrisbaneOrgPK, '2017-2-10', '2017-2-2', 300, 30, 3, 0),

	(newid(), '004', 'AP', 'INV', @MelbourneCompanyPK, @MelbourneBranchPK, @DepartmentPK, @SydneyOrgPK, '2017-2-1', '2017-2-2', 400, 40, 4, 0),
	(newid(), '005', 'AP', 'INV', @MelbourneCompanyPK, @MelbourneBranchPK, @DepartmentPK, @GoldCoastOrgPK, '2017-2-1', '2017-2-2', 500, 50, 5, 0),
	(newid(), '006', 'AP', 'INV', @MelbourneCompanyPK, @MelbourneBranchPK, @DepartmentPK, @BrisbaneOrgPK, '2017-2-1', '2017-2-2', 600, 60, 6, 0),

	(newid(), '007', 'AP', 'INV', @BrisbaneCompanyPK, @BrisbaneBranchPK, @DepartmentPK, @SydneyOrgPK, '2017-2-1', '2017-2-2', 700, 70, 7, 0),
	(newid(), '008', 'AP', 'INV', @BrisbaneCompanyPK, @BrisbaneBranchPK, @DepartmentPK, @MelbourneOrgPK, '2017-2-1', '2017-2-2', 800, 80, 8, 0),
	(newid(), '009', 'AP', 'INV', @BrisbaneCompanyPK, @BrisbaneBranchPK, @DepartmentPK, @BrisbaneOrgPK, '2017-2-1', '2017-2-2', 900, 90, 9, 0)
";

			TestConnection.ExecuteNonQuery(insertSql);

			var sqlQuery = "SELECT * FROM fn_NVPMGetValueOfInvoices(@CompanyPKs, @Ledger, @StartDate, @EndDate, @NettingCompanyPK, @IsReciprocal)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPKs", System.Data.SqlDbType.VarChar, "10E94EF2-92E4-44BA-B0C8-856904EE7F81,3904D0C5-C5D9-493E-8D00-6823DFCEBF62,7A55D481-2155-4FC9-87AA-328F6C2FEB1A");
			command.AddParameter("@Ledger", System.Data.SqlDbType.Char, LedgerTypeCodes.AccountsReceivable);
			command.AddParameter("@StartDate", System.Data.SqlDbType.DateTime, "2017-2-1");
			command.AddParameter("@EndDate", System.Data.SqlDbType.DateTime, "2017-2-10");
			command.AddParameter("@NettingCompanyPK", System.Data.SqlDbType.UniqueIdentifier, new Guid("10E94EF2-92E4-44BA-B0C8-856904EE7F81"));
			command.AddParameter("@IsReciprocal", System.Data.SqlDbType.Bit, true);

			var result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(3, result.Rows.Count);

			var expectedResult = @"
Code       Participant1                            Participant2                            Participant3                            Participant4                            Participant5                            Participant6                            Participant7                            Participant8                            Participant9                            Participant10                           Participant11                           Participant12                           Participant13                           Participant14                           Participant15                           Participant16                           Participant17                           Participant18                           Participant19                           Participant20                           Participant21                           Participant22                           Participant23                           Participant24                           Participant25
---------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- ---------------------------------------
BNE        NULL                                    888.00                                  777.00                                  NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL
MEL        1221.00                                 NULL                                    444.00                                  NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL
SYD        555.00                                  111.00                                  NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL
";
			var testHelper = new TestDbHelperBase(TestConnection);
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("fn_NVPMGetValueOfInvoices", result, expectedResult, new List<string>() { });

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPKs", System.Data.SqlDbType.VarChar, "10E94EF2-92E4-44BA-B0C8-856904EE7F81,3904D0C5-C5D9-493E-8D00-6823DFCEBF62,7A55D481-2155-4FC9-87AA-328F6C2FEB1A");
			command.AddParameter("@Ledger", System.Data.SqlDbType.Char, LedgerTypeCodes.AccountsReceivable);
			command.AddParameter("@StartDate", System.Data.SqlDbType.DateTime, "2017-2-11");
			command.AddParameter("@EndDate", System.Data.SqlDbType.DateTime, "2017-2-20");
			command.AddParameter("@NettingCompanyPK", System.Data.SqlDbType.UniqueIdentifier, new Guid("10E94EF2-92E4-44BA-B0C8-856904EE7F81"));
			command.AddParameter("@IsReciprocal", System.Data.SqlDbType.Bit, true);

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(0, result.Rows.Count);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPKs", System.Data.SqlDbType.VarChar, "10E94EF2-92E4-44BA-B0C8-856904EE7F81,3904D0C5-C5D9-493E-8D00-6823DFCEBF62,7A55D481-2155-4FC9-87AA-328F6C2FEB1A");
			command.AddParameter("@Ledger", System.Data.SqlDbType.Char, LedgerTypeCodes.AccountsPayable);
			command.AddParameter("@StartDate", System.Data.SqlDbType.DateTime, "2017-2-1");
			command.AddParameter("@EndDate", System.Data.SqlDbType.DateTime, "2017-2-10");
			command.AddParameter("@NettingCompanyPK", System.Data.SqlDbType.UniqueIdentifier, new Guid("10E94EF2-92E4-44BA-B0C8-856904EE7F81"));
			command.AddParameter("@IsReciprocal", System.Data.SqlDbType.Bit, true);

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(3, result.Rows.Count);

			expectedResult = @"
Code       Participant1                            Participant2                            Participant3                            Participant4                            Participant5                            Participant6                            Participant7                            Participant8                            Participant9                            Participant10                           Participant11                           Participant12                           Participant13                           Participant14                           Participant15                           Participant16                           Participant17                           Participant18                           Participant19                           Participant20                           Participant21                           Participant22                           Participant23                           Participant24                           Participant25
---------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- --------------------------------------- ---------------------------------------
BNE        NULL                                    888.00                                  777.00                                  NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL
MEL        1221.00                                 NULL                                    444.00                                  NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL
SYD        555.00                                  111.00                                  NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL                                    NULL
";

			testHelper = new TestDbHelperBase(TestConnection);
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("AP transactions", result, expectedResult, new List<string>() { });

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPKs", System.Data.SqlDbType.VarChar, "10E94EF2-92E4-44BA-B0C8-856904EE7F81,3904D0C5-C5D9-493E-8D00-6823DFCEBF62,7A55D481-2155-4FC9-87AA-328F6C2FEB1A");
			command.AddParameter("@Ledger", System.Data.SqlDbType.Char, LedgerTypeCodes.AccountsPayable);
			command.AddParameter("@StartDate", System.Data.SqlDbType.DateTime, "2017-2-11");
			command.AddParameter("@EndDate", System.Data.SqlDbType.DateTime, "2017-2-20");
			command.AddParameter("@NettingCompanyPK", System.Data.SqlDbType.UniqueIdentifier, new Guid("10E94EF2-92E4-44BA-B0C8-856904EE7F81"));
			command.AddParameter("@IsReciprocal", System.Data.SqlDbType.Bit, true);

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(0, result.Rows.Count);
		}
	}
}
