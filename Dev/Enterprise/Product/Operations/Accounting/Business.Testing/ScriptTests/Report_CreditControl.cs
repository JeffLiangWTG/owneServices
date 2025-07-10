
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_CreditControl : ScriptTest
	{
		public void TestCreditLimitFields()
		{
			// This test doesn't use a TestDate attribute because we are testing date logic within SQL Server for temporary credit limit, and
			// setting that attribute doesn't change the date in SQL Server.

			var org = TestObjectCreator.ABIGAS;
			CreditTemporaryIncreaseAuthorisationSettingsHelper.QuickSetupTemporaryCreditLimitOnOrg(org.CompanyData, 2000m, 200m);
			Factory.Save();

			var result = RunScript(org.OH_Code);
			AssertEquals("Result Rows", 1, result.Rows.Count);

			var headers = new[] { "CreditLimit", "TemporaryCreditLimitIncrease", "TemporaryCreditLimitIncreaseExpiry", "AdjustedCreditLimit" };

			AssertEquals("exportedRows.Count", 1, result.Rows.Count);
			AssertDataRow(result.Rows[0], headers, new object[] { 2000M, 200M, org.CompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry, 2200M });
		}

		DataTable RunScript(ZString orgList)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
select * from Report_CreditControl(
'{0}',	--@Company
'', --@EventCode
'', --@EventDateFrom
'', --@EventDateTo
'{1}', --@OrgList
'', --@OrgGroupList VARCHAR(8000),
'', --@CountryCode
'', --@SalesRep
'', --@CreditController 
'', --@CustomerServiceRep
'All',	--@CurrentlyApproved
'All'	--@CCurrentlyUnHold
)",
			GlbCompany.CurrentCompany.PK,
			orgList
			));
		}
	}
}


