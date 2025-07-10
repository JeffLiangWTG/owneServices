

using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_OutstandingTransactionsByChargeCodeTest : ScriptTest
	{
		public void TestARSettlementGroupList()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			DataTable resultForOrg = RunScript(
				LedgerTypes.AccountsReceivable,
				new string[] { TestObjectCreator.ABIGAS.OH_Code, TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient2.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			DataTable resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code });

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			TestObjectCreator.ABIGAS.ARSettlementGroupPK = TestObjectCreator.Agent.PK;
			Factory.Save();

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsReceivable,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.ABIGAS.OH_Code });

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain ABIGAS", 0, resultForSettlementGroup.Select("OH_Code = 'ABIGAS'").Length);
		}

		public void TestAPSettlementGroupList()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.AALSHI.APSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.Creditor1.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.Creditor2.APSettlementGroupPK = TestObjectCreator.AALSHI.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			Factory.Save();

			DataTable resultForOrg = RunScript(
				LedgerTypes.AccountsPayable,
				new string[] { TestObjectCreator.AALSHI.OH_Code, TestObjectCreator.Creditor1.OH_Code, TestObjectCreator.Creditor2.OH_Code },
				System.Array.Empty<string>());

			AssertEquals("Result for Organisation list", 3, resultForOrg.Rows.Count);

			DataTable resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code });

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			AssertEquals("Results must contain the same amount of rows.", resultForSettlementGroup.Rows.Count, resultForOrg.Rows.Count);

			TestObjectCreator.AALSHI.APSettlementGroupPK = TestObjectCreator.Agent.PK;
			Factory.Save();

			resultForSettlementGroup = RunScript(
				LedgerTypes.AccountsPayable,
				System.Array.Empty<string>(),
				new string[] { TestObjectCreator.AALSHI.OH_Code });

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain AALSHI", 0, resultForSettlementGroup.Select("OH_Code = 'AALSHI'").Length);
		}

		public void TestARCountryList()
		{
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true, "USCHI");
			var invoice1 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice1.AH_OH = orgHeader1.PK;

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, true, "AUSYD");
			var invoice2 = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "Inv002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice2.AH_OH = orgHeader2.PK;

			Factory.Save();

			DataTable resultForUS = RunScript(
				LedgerTypes.AccountsReceivable,
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForUS.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForUS.Rows[0]["CountryCode"]);

			DataTable resultForAU = RunScript(
				LedgerTypes.AccountsReceivable,
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForAU.Rows[0]["CountryCode"]);
		}

		public void TestAPCountryList()
		{
			var orgHeader1 = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true, "USCHI");
			var invoice1 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice1.AH_OH = orgHeader1.PK;

			var orgHeader2 = TestObjectCreator.CreateOrgHeader("TSTORG2", false, true, "AUSYD");
			var invoice2 = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "Inv002", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M);
			invoice2.AH_OH = orgHeader2.PK;

			Factory.Save();

			DataTable resultForUS = RunScript(
				LedgerTypes.AccountsPayable,
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForUS.Rows.Count);
			AssertEquals("Country should be ", orgHeader1.CountryCode, resultForUS.Rows[0]["CountryCode"]);

			DataTable resultForAU = RunScript(
				LedgerTypes.AccountsPayable,
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				System.Array.Empty<string>(),
				new string[1] { orgHeader1.CountryCode });

			AssertEquals("Result for Country list", 1, resultForAU.Rows.Count);
			AssertEquals("Country should be ", orgHeader2.CountryCode, resultForAU.Rows[0]["CountryCode"]);
		}

		DataTable RunScript(string ledger, string[] orgList, string[] settlementGroupList, string[] includingCountryList = null, string[] excludedCountryList = null)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_OutstandingTransactionsByChargeCode(
'{0}',	--@company				
'{1}',	--@ledgerType				
'{2}',	--@orgList				
'', 	--@orgGroupList			
'', 	--@headerBranchList		
'', 	--@headerDeptList			
'', 	--@lineBranchList			
'', 	--@lineDeptList			
NULL,	--@accountsRelationship	
NULL,	--@consolidatedCategory	
'{3}',	--@settlementGroupList	
NULL,	--@creditRating			
NULL,	--@chargeGroupList		
'',		--@chargeCodeList
0,		--@period					
'',		--@agedByInvoiceDate		
'',		--@ageingOption			 
0,		--@day1					
0,		--@day2					
0,		--@day3					
0,		--@day4
'{4}',	--@CurrentDateTime,
'{5}',	--@CountryList,
'{6}'	--@ExCountryList					
) 
",
			GlbCompany.CurrentCompany.PK,
			ledger,
			new ZStringBuilder(orgList).ToStringWithDelimiterBetweenAppends(","),
			new ZStringBuilder(settlementGroupList).ToStringWithDelimiterBetweenAppends(","),
			ZDateTime.Now.ToISO8601String(),
			includingCountryList == null ? "" : new ZStringBuilder(includingCountryList).ToStringWithDelimiterBetweenAppends(","),
			excludedCountryList == null ? "" : new ZStringBuilder(excludedCountryList).ToStringWithDelimiterBetweenAppends(",")
			));
		}
	}
}


