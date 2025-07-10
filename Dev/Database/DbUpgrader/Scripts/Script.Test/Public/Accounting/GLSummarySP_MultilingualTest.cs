using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(GLSummarySP_Multilingual))]
	class GLSummarySP_MultilingualTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestTransactionDescPaymentReferenceNumberMaxLength()
		{
			GLTransactionsSPTest.SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength(TestConnection);

			var template = @"EXEC GLSummarySP_Multilingual
		@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
		@StartPeriod = 201504,
		@EndPeriod = 201504,
		@StartGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
		@EndGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
		@Language = 'ABC',
		@CountryCode = 'CN'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, template);
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
		}

		public void TestOpenBalanceForPNLAccountWithDifferentCompanies()
		{
			var helper = new TestDbHelper(TestConnection);

			var glAccountPK = helper.InsertGLAccount("4444.22.22", "TestGLAccount 1", "P&L");
			var glAccountPK1 = helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "P&L");
			var glAccountPK2 = helper.InsertGLAccount("4900.22.22", "TestGLAccount 1", "P&L");
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());
			TestHelper.InsertGLMappingAccount("", "4444.22.22", glAccountPK1, "Account mapping", "COA", "");
			TestHelper.InsertGLMappingAccount("", "4900.22.22", plAppropriationAccountPK, "Account mapping", "COA", "");

			var company2PK = helper.InsertCompany("ABC", "ABC Compay", "AUD", "AU", true, true);
			var branch2PK = helper.InsertBranch("BR2", company2PK, "Branch 2");
			var departmentPK = helper.InsertDepartment("DP2", "Department 2");

			helper.InsertAccPeriod(2019, 05, TestDbHelper.DefaultCompanyPK);
			helper.InsertAccPeriod(2019, 06, TestDbHelper.DefaultCompanyPK);

			helper.InsertAccPeriod(2019, 07, company2PK, new DateTime(2020, 01, 01));
			helper.InsertAccPeriod(2019, 08, company2PK, new DateTime(2020, 02, 01));
			helper.InsertAccPeriod(2019, 09, company2PK, new DateTime(2020, 03, 01));
			helper.InsertAccPeriod(2019, 10, company2PK, new DateTime(2020, 04, 01));

			var headerPK = helper.InsertTransactionHeader("GL", "GJL", "X0001", 100, new DateTime(2019, 05, 15), branch2PK, departmentPK, postToGL: true, companyPK: company2PK);
			helper.InsertTransactionLine(headerPK, null, null, glAccountPK1, branch2PK, departmentPK, null, 100, "CST", new DateTime(2019, 05, 15), null, 0, 1, "A", companyPK: company2PK);

			var headerPK1 = helper.InsertTransactionHeader("GL", "GJL", "X0002", 200, new DateTime(2019, 06, 15), branch2PK, departmentPK, postToGL: true, companyPK: company2PK);
			helper.InsertTransactionLine(headerPK1, null, null, glAccountPK1, branch2PK, departmentPK, null, 200, "CST", new DateTime(2019, 06, 15), null, 0, 1, "A", companyPK: company2PK);

			var headerPK2 = helper.InsertTransactionHeader("GL", "GJL", "X0003", 300, new DateTime(2020, 01, 15), branch2PK, departmentPK, postToGL: true, companyPK: company2PK);
			helper.InsertTransactionLine(headerPK2, null, null, glAccountPK1, branch2PK, departmentPK, null, 300, "CST", new DateTime(2020, 01, 15), null, 0, 1, "A", companyPK: company2PK);

			var headerPK3 = helper.InsertTransactionHeader("GL", "GJL", "X0004", 400, new DateTime(2020, 02, 15), branch2PK, departmentPK, postToGL: true, companyPK: company2PK);
			helper.InsertTransactionLine(headerPK3, null, null, glAccountPK1, branch2PK, departmentPK, null, 400, "CST", new DateTime(2020, 02, 15), null, 0, 1, "A", companyPK: company2PK);

			var headerPK4 = helper.InsertTransactionHeader("GL", "GJL", "X0005", 500, new DateTime(2020, 03, 15), branch2PK, departmentPK, postToGL: true, companyPK: company2PK);
			helper.InsertTransactionLine(headerPK4, null, null, glAccountPK1, branch2PK, departmentPK, null, 500, "CST", new DateTime(2020, 03, 15), null, 0, 1, "A", companyPK: company2PK);

			var headerPK5 = helper.InsertTransactionHeader("GL", "GJL", "X0006", 600, new DateTime(2020, 04, 15), branch2PK, departmentPK, postToGL: true, companyPK: company2PK);
			helper.InsertTransactionLine(headerPK5, null, null, glAccountPK1, branch2PK, departmentPK, null, 600, "CST", new DateTime(2020, 04, 15), null, 0, 1, "A", companyPK: company2PK);

			var template = $@"EXEC GLSummarySP_Multilingual
		@CompanyPK = '{company2PK}',
		@StartPeriod = 201909,
		@EndPeriod = 201909,
		@StartGLAccountPK = NULL,
		@EndGLAccountPK = NULL,
		@Language = '',
		@CountryCode = ''";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, template);

			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals("The balance of Account '4444' should be 700m", 700m, result.Select("AccountNumber ='4444'").FirstOrDefault().Field<decimal>("OpeningBalance"));
		}
	}
}
