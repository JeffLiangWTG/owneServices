

using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_TransactionsByUserTest : ScriptTest
	{
		public void TestSettlementGroup()
		{
			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript();

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);
			AssertEquals("All result has ABIGAS as settlemet code", 3, resultForSettlementGroup.Select(string.Format("OB_OH_ARSettlementGroup = '{0}'", TestObjectCreator.ABIGAS.PK)).Length);
		}

		public void TestAmounts()
		{
			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.USD, 2M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 200M, GlbBranch.CurrentBranch.PK);
			Factory.Save();

			DataTable result = RunScript();

			AssertEquals("Result Rows.Count", 1, result.Rows.Count);
			AssertEquals("AH_InvoiceAmount", 110M, result.Rows[0]["AH_InvoiceAmount"]);
			AssertEquals("AH_OSTotal", 220M, result.Rows[0]["AH_OSTotal"]);
		}

		public void TestDateRangeFilters()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV1", TestObjectCreator.USD, 2M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 2M, 200M, GlbBranch.CurrentBranch.PK);

			var postDate = ZDateTime.Today.AddDays(-5);
			var dueDate = ZDateTime.Today.AddDays(-3);
			var invoiceDate = ZDateTime.Today.AddDays(-2);

			invoice.AH_PostDate = postDate;
			invoice.AH_DueDate = dueDate;
			invoice.AH_InvoiceDate = invoiceDate;

			Factory.Save();

			AssertEquals(true, invoice.IsInDatabase);

			var result = RunScript(postDate.ToISO8601String(), postDate.ToISO8601String(), "", "", "", "");
			AssertEquals("Result Rows.Count", 1, result.Rows.Count);
			AssertEquals("AH_InvoiceAmount", 110M, result.Rows[0]["AH_InvoiceAmount"]);
			AssertEquals("AH_OSTotal", 220M, result.Rows[0]["AH_OSTotal"]);

			result = RunScript("","",dueDate.ToISO8601String(), dueDate.ToISO8601String(), "", "");
			AssertEquals("Result Rows.Count", 1, result.Rows.Count);
			AssertEquals("AH_InvoiceAmount", 110M, result.Rows[0]["AH_InvoiceAmount"]);
			AssertEquals("AH_OSTotal", 220M, result.Rows[0]["AH_OSTotal"]);

			result = RunScript("","", "","", invoiceDate.ToISO8601String(), invoiceDate.ToISO8601String());
			AssertEquals("Result Rows.Count", 1, result.Rows.Count);
			AssertEquals("AH_InvoiceAmount", 110M, result.Rows[0]["AH_InvoiceAmount"]);
			AssertEquals("AH_OSTotal", 220M, result.Rows[0]["AH_OSTotal"]);
		}

		public void TestBranchManagementCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "TBS";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();
			TestObjectCreator.ABIGAS.ARSettlementGroupPK = ZGuid.Empty;
			TestObjectCreator.LocalClient.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.LocalClient2.ARSettlementGroupPK = TestObjectCreator.ABIGAS.PK;
			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			invoice1.AH_GB = branch.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRE', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = '{0}' And GB_GC = '{1}'", GlbBranch.CurrentBranch.GB_Code, GlbCompany.CurrentCompany.PK));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRT', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = 'TBS' And GB_GC = '{0}'", GlbCompany.CurrentCompany.PK));

			var result = RunScript("BRT");
			AssertEquals("1 rows returned by report", 1, result.Rows.Count);
			AssertEquals("Should found the branch TBS", "TBS", result.Rows[0]["GB_Code"]);

			result = RunScript("BRE");
			AssertEquals("2 rows returned by report", 2, result.Rows.Count);
			AssertEquals("Should found the current branch", GlbBranch.CurrentBranch.GB_Code, result.Rows[0]["GB_Code"]);
			AssertEquals("Should found the current branch", GlbBranch.CurrentBranch.GB_Code, result.Rows[1]["GB_Code"]);
		}

		DataTable RunScript(string branchManagementCode = "")
		{
			return RunScript(ZDateTime.Today.AddDays(-1).ToISO8601String(),ZDateTime.Today.AddDays(1).ToISO8601String(),"","","","", branchManagementCode);
		}

		DataTable RunScript(string fromDate, string toDate, string dueDateFrom, string dueDateTo, string invoiceDateFrom, string invoiceDateTo, string branchManagementCode = "")
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_TransactionsByUser(
'{0}',	--@Company
'INV',	--@TransactionTypeList
'',		--@Organisations
'',		--@OrgGroups
'',		--@BranchPKList
'',		--@CountryPKList
'',		--@ExCountryPKList
'',		--@CurrencyList
'',		--@SettlementGroupList
'{1}',	--@FromDate
'{2}',	--@ToDate
'{3}',	--@DueDateFrom
'{4}',	--@DueDateTo
'{5}',	--@InvoiceDateFrom
'{6}',	--@InvoiceDateTo
'{7}' --@BranchManagementCode
)  
",
			GlbCompany.CurrentCompany.PK,
			fromDate,
			toDate,
			dueDateFrom,
			dueDateTo,
			invoiceDateFrom,
			invoiceDateTo,
			branchManagementCode
			));
		}
	}
}


