using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ARAPTransactionsSummaryByOrganizationAndCountry))]
	class Report_ARAPTransactionsSummaryByOrganizationAndCountryTest : DbCreateScriptTest
	{
		public void TestSimpleCall()
		{
			var result = DataUtils.GetDataTableFromQuery(TestConnection, @"SELECT * FROM Report_ARAPTransactionsSummaryByOrganizationAndCountry('56D49995-9742-491F-8C7E-B244C7E30FFD', null, '2015-1-1', '2015-12-31', 'AR&AP', 'INV', '', '', '', '', '')");

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("OrganizationCode should be TESTORG", "TESTORG", result.Rows[0][0].ToString());
		}

		public void TestCanGetCountryCodeWhenOA_RL_NKRelatedPortCodeIsEmpty()
		{
			var sql = @"Update dbo.OrgHeader Set OH_RL_NKClosestPort = 'AUMEL' Where OH_PK = '127D0DCD-D392-4364-8642-4D64A0FAB5DF'";
			using (var command = TestConnection.Command(sql))
			{
				command.ExecuteNonQuery();
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection, @"SELECT * FROM Report_ARAPTransactionsSummaryByOrganizationAndCountry('56D49995-9742-491F-8C7E-B244C7E30FFD', null, '2015-1-1', '2015-12-31', 'AR&AP', 'INV', '', '', '', '', '')");

			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("OrganizationCode should be TESTORG", "TESTORG", result.Rows[0][0].ToString());
			AssertEquals("CountryCode should be AU", "AU", result.Rows[0][7].ToString());
		}

		protected override void SetUp()
		{
			const string insertSql = @"
DECLARE @BranchPk UNIQUEIDENTIFIER = '9C60AE3F-44F9-4E97-B404-040FA896626E'
DECLARE @CompanyPk UNIQUEIDENTIFIER = '56D49995-9742-491F-8C7E-B244C7E30FFD'
DECLARE @DepartmentPk UNIQUEIDENTIFIER = 'DD3E23A1-7C44-4DAE-8F38-ACE05FDBA455'
DECLARE @OrgPk UNIQUEIDENTIFIER = '127D0DCD-D392-4364-8642-4D64A0FAB5DF'
DECLARE @StartDate SMALLDATETIME = '2015-1-1'
DECLARE @EndDate SMALLDATETIME = '2015-12-31'
DECLARE @Ledger CHAR(5) = 'AR&AP'
DECLARE @TransactionTypeList varchar(8000) = 'INV'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'RAU', 'AU company', 'AU', 'AUD')

INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OrgPk, 'TESTORG')

DECLARE @InvoicePk UNIQUEIDENTIFIER = '28083B6E-95DC-4925-9B82-370A7D7C939D';
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_GC, AH_GB, AH_GE, AH_InvoiceDate, AH_PostDate, AH_TransactionType, AH_OH, AH_Ledger) 
VALUES (@InvoicePk, @CompanyPk, @BranchPk, @DepartmentPk, '2015-5-5', '2015-5-5', 'INV', @OrgPk, 'AR')

INSERT INTO dbo.RefExchangeRate (RE_PK, RE_GC, RE_RX_NKExCurrency, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_SellRate, RE_AsPublished) VALUES
	(newid(), @CompanyPk, 'USD', 'SEL', '2015-1-1', '2015-12-31', 0.8, '')
";

			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}

			base.SetUp();
		}
	}
}

