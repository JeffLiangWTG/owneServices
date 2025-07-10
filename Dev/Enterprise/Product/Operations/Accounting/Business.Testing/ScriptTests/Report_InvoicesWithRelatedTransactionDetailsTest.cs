using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_InvoicesWithRelatedTransactionDetailsTest : ScriptTest
	{
		public void TestPrimaryOrgList()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, primaryOrgList: new[] { TestObjectCreator.AALSHI.PK, TestObjectCreator.Creditor1.PK, TestObjectCreator.Creditor2.PK });

			AssertEquals("Rows.Count", 3, result.Rows.Count);

			result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, primaryOrgList: new[] { TestObjectCreator.Creditor1.PK, TestObjectCreator.Creditor2.PK });

			AssertEquals("Rows.Count", 2, result.Rows.Count);
			AssertEquals("Result don't contain AALSHI", 0, result.Select("Primary_OH_Code_Filter = 'AALSHI'").Length);
		}

		public void TestDepartmentList()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var department1 = TestObjectCreator.NonCurrentDepartment;
			var department2 = GlbDepartment.CurrentDepartment;

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			invoice1.AH_GE = department2.PK;

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			invoice2.AH_GE = department1.PK;
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			invoice3.AH_GE = department1.PK;

			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, departmentList: new[] { department1.PK });
			AssertEquals("Rows.Count", 2, result.Rows.Count);
			var actualCreditorsForDepartment1 = result.Rows.Cast<DataRow>().Select(x => x["Primary_OH_Code_Filter"].ToString().Trim());
			AssertContainsExactElementsInAnyOrder(new[] { "ZCreditor1", "ZCreditor2" }, actualCreditorsForDepartment1);

			result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, departmentList: new[] { department2.PK });
			AssertEquals("Rows.Count", 1, result.Rows.Count);
			var actualCreditorsForDepartment2 = result.Rows.Cast<DataRow>().Select(x => x["Primary_OH_Code_Filter"].ToString().Trim());
			AssertContainsExactElementsInAnyOrder(new[] { "AALSHI" }, actualCreditorsForDepartment2);

			result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, departmentList: new[] { department1.PK, department2.PK });
			AssertEquals("Rows.Count", 3, result.Rows.Count);
			var actualCreditorsForBothDepartments = result.Rows.Cast<DataRow>().Select(x => x["Primary_OH_Code_Filter"].ToString().Trim());
			AssertContainsExactElementsInAnyOrder(new[] { "AALSHI", "ZCreditor1", "ZCreditor2" }, actualCreditorsForBothDepartments);
		}

		public void TestBranchList()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var branch1 = TestObjectCreator.NonCurrentBranch;
			var branch2 = GlbBranch.CurrentBranch;

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			invoice1.AH_GB = branch2.PK;

			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			invoice2.AH_GB = branch1.PK;
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			invoice3.AH_GB = branch1.PK;

			Factory.Save();

			var result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, branchList: new[] { branch1.PK } );
			AssertEquals("Rows.Count", 2, result.Rows.Count);
			var actualCreditorsForBranch1 = result.Rows.Cast<DataRow>().Select(x => x["Primary_OH_Code_Filter"].ToString().Trim());
			AssertContainsExactElementsInAnyOrder(new[] { "ZCreditor1", "ZCreditor2" }, actualCreditorsForBranch1);

			result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, branchList: new[] { branch2.PK });
			AssertEquals("Rows.Count", 1, result.Rows.Count);
			var actualCreditorsForBranch2 = result.Rows.Cast<DataRow>().Select(x => x["Primary_OH_Code_Filter"].ToString().Trim());
			AssertContainsExactElementsInAnyOrder(new[] { "AALSHI", }, actualCreditorsForBranch2);

			result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, branchList: new[] { branch1.PK, branch2.PK });
			AssertEquals("Rows.Count", 3, result.Rows.Count);
			var actualCreditorsForBothBranches = result.Rows.Cast<DataRow>().Select(x => x["Primary_OH_Code_Filter"].ToString().Trim());
			AssertContainsExactElementsInAnyOrder(new[] { "AALSHI", "ZCreditor1", "ZCreditor2" }, actualCreditorsForBothBranches);
		}

		public void TestCreditorGroupList()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var testCreditorGroup1 = Factory.NewWithValidTestData<OrgCreditorGroup>();
			TestObjectCreator.AALSHI.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup1.PK;
			TestObjectCreator.Creditor1.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup1.PK;
			TestObjectCreator.Creditor2.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup1.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			Factory.Save();

			var creditorGroupList = new[] { testCreditorGroup1.PK };
			var result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, creditorGroupList: creditorGroupList);

			AssertEquals("Rows.Count", 3, result.Rows.Count);

			var testCreditorGroup2 = Factory.NewWithValidTestData<OrgCreditorGroup>();
			TestObjectCreator.AALSHI.CompanyData.OB_OG_APCreditorGroup = testCreditorGroup2.PK;
			Factory.Save();

			result = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, creditorGroupList: creditorGroupList);

			AssertEquals("Rows.Count", 2, result.Rows.Count);
			AssertEquals("Result don't contain AALSHI", 0, result.Select("Primary_OH_Code_Filter = 'AALSHI'").Length);
		}

		public void TestDebtorGroupList()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var testDebtorGroup1 = Factory.NewWithValidTestData<OrgDebtorGroup>();
			TestObjectCreator.ABIGAS.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup1.PK;
			TestObjectCreator.LocalClient.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup1.PK;
			TestObjectCreator.LocalClient2.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup1.PK;
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.ABIGAS, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV2", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.LocalClient, glAccount.PK);
			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV3", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.LocalClient2, glAccount.PK);
			Factory.Save();

			var debtorGroupList = new[] { testDebtorGroup1.PK };
			DataTable result = RunScript(LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable, debtorGroupList: debtorGroupList);

			AssertEquals("Rows.Count", 3, result.Rows.Count);

			var testDebtorGroup2 = Factory.NewWithValidTestData<OrgDebtorGroup>();
			TestObjectCreator.ABIGAS.CompanyData.OB_OJ_ARDebtorGroup = testDebtorGroup2.PK;
			Factory.Save();

			result = RunScript(LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable, debtorGroupList: debtorGroupList);

			AssertEquals("Rows.Count", 2, result.Rows.Count);
			AssertEquals("Result don't contain ABIGAS", 0, result.Select("Primary_OH_Code_Filter = 'ABIGAS'").Length);
		}

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

			DataTable resultForSettlementGroup = RunScript(LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable, settlementGroupList: new[] { TestObjectCreator.ABIGAS.PK });

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			TestObjectCreator.ABIGAS.ARSettlementGroupPK = TestObjectCreator.Agent.PK;
			Factory.Save();

			resultForSettlementGroup = RunScript(LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable, settlementGroupList: new[] { TestObjectCreator.ABIGAS.PK });

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain ABIGAS", 0, resultForSettlementGroup.Select("Primary_OH_Code_Filter = 'ABIGAS'").Length);
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

			DataTable resultForSettlementGroup = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, settlementGroupList: new[] { TestObjectCreator.AALSHI.PK });

			AssertEquals("Result for SettlementGroup list", 3, resultForSettlementGroup.Rows.Count);

			TestObjectCreator.AALSHI.APSettlementGroupPK = TestObjectCreator.Agent.PK;
			Factory.Save();

			resultForSettlementGroup = RunScript(LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable, settlementGroupList: new[] { TestObjectCreator.AALSHI.PK });

			AssertEquals("Result for SettlementGroup list", 2, resultForSettlementGroup.Rows.Count);
			AssertEquals("Result don't contain AALSHI", 0, resultForSettlementGroup.Select("Primary_OH_Code_Filter = 'AALSHI'").Length);
		}

		public void TestWhenFlagARTransactionType()
		{
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, TestObjectCreator.AALSHI, glAccount.PK);
			var aRCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD", TestObjectCreator.AUD, 1M, 20M, 0M, 20M, 0M, TestObjectCreator.Creditor1, glAccount.PK);
			var aRAdjustmentNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "ADJ", TestObjectCreator.AUD, 1M, 30M, 0M, 30M, 0M, TestObjectCreator.Creditor2, glAccount.PK);
			Factory.Save();

			var result0 = RunScript(LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable);
			AssertEquals("when flag none, should been report all entry", 3, result0.Rows.Count);

			string flagString = "'INV, CRD, ADJ'";
			var result1 = RunScript(LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable, transactionType: flagString);
			AssertEquals("when flag all, should been report all entry", 3, result1.Rows.Count);

			flagString = "'CRD, ADJ'";
			var result2 = RunScript(LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable, transactionType: flagString);
			AssertEquals("when flag 2, should been report 2 entry", 2, result2.Rows.Count);
		}

		DataTable RunScript(string primaryLedger, string relatedLedger, ZGuid[] creditorGroupList = null, ZGuid[] debtorGroupList = null, ZGuid[] settlementGroupList = null, ZGuid[] branchList = null, ZGuid[] departmentList = null, ZGuid[] primaryOrgList = null,string transactionType = null)
		{
			var sql = string.Format(@"
SELECT * 
FROM Report_InvoicesWithRelatedTransactionDetails
(
'{0}',								--@Company                      uniqueidentifier,
'{1}',								--@PrimaryLedger                char(2),
'{2}',								--@RelatedLedger                char(2),
'OPEN',								--@PrimaryTransactionPaidStatus char(4),
'BOTH',								--@RelatedTransactionPaidStatus char(4),
@CreditorGroupList,												  --dbo.TVP_uniqueidentifier readonly,
@CreditorGroupListIsEmpty,										  --bit,
@DebtorGroupList,												  --dbo.TVP_uniqueidentifier readonly,
@DebtorGroupListIsEmpty,										  --bit,
NULL,								--@AccountsRelationship         varchar(3),
@SettlementGroupList,											  --dbo.TVP_uniqueidentifier readonly,
@SettlementGroupListIsEmpty,									  --bit,
@BranchList,													  --dbo.TVP_uniqueidentifier readonly,
@BranchListIsEmpty,												  --bit,
@DepartmentList,												  --dbo.TVP_uniqueidentifier readonly,
@DepartmentListIsEmpty,											  --bit,
@PrimaryOrgList,												  --dbo.TVP_uniqueidentifier readonly,
@PrimaryOrgListIsEmpty,											  --bit,
NULL,								--@Primary_PostDate_From        smalldatetime = NULL,
NULL,								--@Primary_PostDate_To          smalldatetime = NULL,
NULL,								--@Primary_DueDate_From         smalldatetime = NULL,
NULL,								--@Primary_DueDate_To           smalldatetime = NULL,
NULL,								--@Primary_InvoiceDate_From     smalldatetime = NULL,
NULL,								--@Primary_InvoiceDate_To       smalldatetime = NULL,
{3},			                    --@Primary_TransactionType	    varchar(100) = '',
NULL,								--@Related_PostDate_From        smalldatetime = NULL,
NULL,								--@Related_PostDate_To          smalldatetime = NULL,
NULL,								--@Related_DueDate_From         smalldatetime = NULL,
NULL,								--@Related_DueDate_To           smalldatetime = NULL,
NULL,								--@Related_InvoiceDate_From     smalldatetime = NULL,
NULL								--@Related_InvoiceDate_To       smalldatetime = NULL
)  
",
			GlbCompany.CurrentCompany.PK,
			primaryLedger,
			relatedLedger,
			transactionType ?? "''"
			);

			var command = Db.Connection.Command(sql);

			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@CreditorGroupList", creditorGroupList);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@DebtorGroupList", debtorGroupList);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@SettlementGroupList", settlementGroupList);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@BranchList", branchList);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@DepartmentList", departmentList);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@PrimaryOrgList", primaryOrgList);

			return DataUtils.GetDataTableFromCommand(command);
		}

		void AddTVP_uniqueidentifierAndIsEmptyParameters(DbCommand command, string paramName, ZGuid[] values)
		{
			var table = new DataTable();
			table.Columns.Add("Value", typeof(Guid));

			if (values != null)
			{
				foreach (var value in values)
				{
					table.Rows.Add(value.ToGuid());
				}
			}

			command.AddTableValuedParameter(paramName, "dbo.TVP_uniqueidentifier", table);
			command.AddParameter(paramName + "IsEmpty", SqlDbType.Bit, table.Rows.Count == 0);
		}
	}
}


