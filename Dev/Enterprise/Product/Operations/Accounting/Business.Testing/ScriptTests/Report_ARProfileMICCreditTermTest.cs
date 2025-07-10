
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ARProfileMICCreditTermTest : ScriptTest
	{
		public void TestARProfileMICCreditTerm()
		{
			var org1 = TestObjectCreator.ABIGAS;
			var org2 = TestObjectCreator.LocalClient;
			var debtorGroupPK = TestObjectCreator.CreateDebtorGroup().PK;

			org1.OH_IsActive = true;
			org1.OH_IsTempAccount = false;
			org1.CompanyData.OB_OJ_ARDebtorGroup = debtorGroupPK;
			org1.CompanyData.OB_ARCategory = "STD";
			org1.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";

			org2.OH_IsActive = true;
			org2.OH_IsTempAccount = true;
			org2.CompanyData.OB_OJ_ARDebtorGroup = debtorGroupPK;
			org2.CompanyData.OB_ARCategory = "KEY";
			org2.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";

			OrgARTerms arTerm1 = org1.CompanyData.LoadARTermForAllInvoiceTypes();
			arTerm1.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTerm1.PY_InvoiceDays = 2;

			OrgARTerms arTerm2 = org2.CompanyData.LoadARTermForAllInvoiceTypes();
			arTerm2.PY_InvoiceClass = "DSB";
			arTerm2.PY_InvoiceTerm = Constants.InvoiceTerms.MonthsFromInvoiceCycleDate;
			arTerm2.PY_InvoiceDays = 2;

			OrgARTermsCycle termCycle1 = arTerm1.ARTermsCycles.AddNew();
			OrgARTermsCycle termCycle2 = arTerm2.ARTermsCycles.AddNew();

			termCycle1.P5_ToDay = 10;
			termCycle2.P5_ToDay = 20;

			termCycle1.P5_PaymentDay = 5;
			termCycle2.P5_PaymentDay = 15;

			Factory.Save();

			DataTable resultOrgs = RunScript(debtorGroupPK);
			AssertEquals("Result Rows", 2, resultOrgs.Rows.Count);

			var headers = new[] { "AccountCode", "AccountName", "AccountPK", "IsTemporary", "AccountsRelationship", "ConsolidationCategory", "InvoiceType", "P5_ToDay", "P5_PaymentDay" };

			DataRow[] exportedRows = resultOrgs.Select("AccountCode = 'ABIGAS      '");
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] { "ABIGAS      ", TestObjectCreator.ABIGAS.OH_FullName, TestObjectCreator.ABIGAS.PK, "N", "STD", "UNR", "STD", (ZByte)10, (ZByte)5 });

			exportedRows = resultOrgs.Select(string.Format("AccountCode = '{0}'", TestObjectCreator.LocalClient.OH_Code));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] { TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient.OH_FullName, TestObjectCreator.LocalClient.PK, "Y", "KEY", "WHO", "DSB", (ZByte)20, (ZByte)15 });
		}

		DataTable RunScript(ZGuid debtorGroupPK)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
EXEC Report_ARProfileMICCreditTerm
'{0}',	--@Company
'{1}',		--@AccountTypeActive
'',		--@IsTemporary
'',		--@Debtors
'{2}',	--@DebtorGroupPK
NULL,		--@BranchPK
NULL,	--@UNLOCO
NULL,	--@CountryCode
NULL,	--@SalesID
''		--@StaffRole 
",
			GlbCompany.CurrentCompany.PK,
			"Active",
			debtorGroupPK
			));
		}
	}
}


