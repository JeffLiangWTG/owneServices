

using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ARProfileChargeGroupingRollUpTest : ScriptTest
	{
		public void TestARProfileChargeGroupingRollUp()
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

			TestObjectCreator.ABIGAS.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			TestObjectCreator.LocalClient.CompanyData.InvoiceRollupOrGroups.RemoveAndDeleteAll();
			OrgInvoiceRollupOrGroup group1 = org1.CompanyData.InvoiceRollupOrGroups.AddNew();
			group1.PG_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			group1.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.Import;
			group1.PG_TransportMode = "FCL";
			group1.PG_GroupOrSubTotal = "DEF";
			group1.PG_GroupOrSubtotalStyle = "NOG";
			group1.PG_InvoiceLineDisplayOption = "ALP";
			group1.PG_InvoicePostingStyle = "DFO";

			OrgInvoiceRollupOrGroup group2 = org2.CompanyData.InvoiceRollupOrGroups.AddNew();
			group2.PG_JobType = JobInvoicingConsumerTypes.Brokerage.Code;
			group2.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.CrossTrade;
			group2.PG_TransportMode = "LCL";
			group2.PG_GroupOrSubTotal = "DEF";
			group2.PG_GroupOrSubtotalStyle = "AEC";
			group2.PG_InvoiceLineDisplayOption = "ALX";
			group2.PG_InvoicePostingStyle = "DFI";

			Factory.Save();

			DataTable resultOrgs = RunScript(debtorGroupPK);
			AssertEquals("Result Rows", 2, resultOrgs.Rows.Count);

			var headers = new[] { "AccountCode", "AccountName", "AccountPK", "IsTemporary", "AccountsRelationship", "ConsolidationCategory",
									"SubtotalCharges", "InvoicingJobType", "Direction", "Mode", "Display", "GroupChargeSyle", "InvoiceLineDisplay", "InvoicePostingStyle"
						};

			DataRow[] exportedRows = resultOrgs.Select("AccountCode = 'ABIGAS      '");
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] {	"ABIGAS      ", TestObjectCreator.ABIGAS.OH_FullName, TestObjectCreator.ABIGAS.PK, "N", "STD", "UNR",
											"N", "SHP", "IMP", "FCL", "DEF", "NOG", "ALP", "DFO" });

			exportedRows = resultOrgs.Select(string.Format("AccountCode = '{0}'", TestObjectCreator.LocalClient.OH_Code));
			AssertEquals("exportedRows.Count", 1, exportedRows.Length);
			AssertDataRow(exportedRows[0], headers, new object[] {	TestObjectCreator.LocalClient.OH_Code, TestObjectCreator.LocalClient.OH_FullName, TestObjectCreator.LocalClient.PK, "Y", "KEY", "WHO",
											"N", "BRK", "CRO", "LCL", "DEF", "AEC", "ALX", "DFI" });
		}

		DataTable RunScript(ZGuid debtorGroupPK)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
EXEC Report_ARProfileChargeGroupingRollUp
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


