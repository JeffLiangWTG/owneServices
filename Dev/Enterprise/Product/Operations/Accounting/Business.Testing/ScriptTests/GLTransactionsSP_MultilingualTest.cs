

using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GLTransactionsSP_MultilingualTest : ScriptTest
	{
		[TestDate(2006, 06, 20, 15, 39, 42)]
		public void TestLocalGLTransactionsSP_Multilingual()
		{
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			ZDateTime dueDate = new ZDateTime(2006, 6, 1);
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2005, 7, 1));

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.03.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.03.96", "TS", "Test PNL 2", "P&L", Constants.DebitCredit.Debit);

			testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "61000.95", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc PNL1", "CN", Constants.DebitCredit.Debit);
			AccGLAccountDescriptor testAccGLAccountDescriptor1 = testObjectCreator.CreateAccountDescriptor(glHeader2PNL, "61000.96", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc PNL2", "CN", Constants.DebitCredit.Debit);

			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "Test BSH 1", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "Test BSH 2", "BSH", Constants.DebitCredit.Debit);

			AccGLAccountDescriptor testAccGLAccountDescriptor0 = testObjectCreator.CreateAccountDescriptor(glHeader1, "53000.95", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc BSH1", "CN", Constants.DebitCredit.Debit);

			TestObjectCreator.InsertTransaction("JNL", "AR", dueDate, true, TestObjectCreator.ABIGAS.PK, glHeader1PNL.PK, TestObjectCreator.USD.RX_Code, 1.3m, 39m, 30m);
			TestObjectCreator.InsertTransaction("JNL", "AP", dueDate, true, TestObjectCreator.ABIGAS.PK, glHeader2PNL.PK, TestObjectCreator.USD.RX_Code, 1.4m, -56m, -40m);

			TestObjectCreator.InsertTransaction("EXX", "AP", dueDate, true, TestObjectCreator.ABIGAS.PK, glHeader1.PK);
			TestObjectCreator.InsertTransaction("EXX", "AR", dueDate, true, TestObjectCreator.ABIGAS.PK, glHeader2.PK);

			testObjectCreator.CreateAccGLAggregate(30m, 201012, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-30m, 201012, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(30m, 201012, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-30m, 201012, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();
			DataTable results = RunScript(dueDate, testAccGLAccountDescriptor0.PK, testAccGLAccountDescriptor1.PK);
			AssertEquals("Missing GL Mapping, Count of invoice should be return 0", 0, results.Rows.Count);

			testObjectCreator.CreateAccountDescriptor(glHeader2, "53000.96", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc BSH2", "CN", Constants.DebitCredit.Debit);

			Factory.Save();

			results = RunScript(dueDate, testAccGLAccountDescriptor0.PK, testAccGLAccountDescriptor1.PK);
			AssertEquals("Count of invoice should be 4", 4, results.Rows.Count);

			AssertEquals("GLAccount should be : 5300.03.95", "5300.03.95", results.Rows[0]["GLAccount"].ToString());
			AssertEquals("GLAccount should be : 1100.03.96", "1100.03.96", results.Rows[1]["GLAccount"].ToString());
			AssertEquals("GLAccount should be : 5300.03.96", "5300.03.96", results.Rows[2]["GLAccount"].ToString());
			AssertEquals("GLAccount should be : 1100.03.95", "1100.03.95", results.Rows[3]["GLAccount"].ToString());

			AssertEquals("LocalGLAccount should be : 53000.95", "53000.95", results.Rows[0]["LocalGLAccount"].ToString());
			AssertEquals("LocalGLAccount should be : 61000.96", "61000.96", results.Rows[1]["LocalGLAccount"].ToString());
			AssertEquals("LocalGLAccount should be : 53000.96", "53000.96", results.Rows[2]["LocalGLAccount"].ToString());
			AssertEquals("LocalGLAccount should be : 61000.95", "61000.95", results.Rows[3]["LocalGLAccount"].ToString());

			AssertEquals("OsAmount should be DBNull", DBNull.Value, results.Rows[0]["OsAmount"]);
			AssertEquals("OsAmount should be : 56", 56m, results.Rows[1]["OsAmount"]);
			AssertEquals("OsAmount should be DBNull", DBNull.Value, results.Rows[2]["OsAmount"]);
			AssertEquals("OsAmount should be : -39", -39m, results.Rows[3]["OsAmount"]);

			AssertEquals("ExRate should be DBNull", DBNull.Value, results.Rows[0]["ExRate"]);
			AssertEquals("ExRate should be : 1.4", 1.4m, results.Rows[1]["ExRate"]);
			AssertEquals("ExRate should be DBNull", DBNull.Value, results.Rows[2]["ExRate"]);
			AssertEquals("ExRate should be : 1.3", 1.3m, results.Rows[3]["ExRate"]);

			AssertEquals("OsDebit should be DBNull", DBNull.Value, results.Rows[0]["OsDebit"]);
			AssertEquals("OsDebit should be 56", 56m, results.Rows[1]["OsDebit"]);
			AssertEquals("OsDebit should be DBNull", DBNull.Value, results.Rows[2]["OsDebit"]);
			AssertEquals("OsDebit should be DBNull", DBNull.Value, results.Rows[3]["OsDebit"]);

			AssertEquals("OsCredit should be DBNull", DBNull.Value, results.Rows[0]["OsCredit"]);
			AssertEquals("OsCredit should be DBNull", DBNull.Value, results.Rows[1]["OsCredit"]);
			AssertEquals("OsCredit should be DBNull", DBNull.Value, results.Rows[2]["OsCredit"]);
			AssertEquals("OsCredit should be 39", 39m, results.Rows[3]["OsCredit"]);

			AssertEquals("Currency should be AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, results.Rows[0]["Currency"]);
			AssertEquals("Currency should be USD", TestObjectCreator.USD.RX_Code, results.Rows[1]["Currency"]);
			AssertEquals("Currency should be AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, results.Rows[2]["Currency"]);
			AssertEquals("Currency should be USD", TestObjectCreator.USD.RX_Code, results.Rows[3]["Currency"]);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		[TestDate(2020, 06, 20, 15, 39, 42)]
		public void TestMultiSubAccountTypeCode()
		{
			var apSuspenseControl = TestObjectCreator.CreateAPSuspenseControlAccount();
			var arSuspenseControl = TestObjectCreator.CreateARSuspenseControlAccount();
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseControl.PK.ToGuid());

			ZDateTime dueDate = new ZDateTime(2020, 7, 1);
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 1, 1));
			TestObjectCreator.GLHeader1.AG_AccountNum = "GLHeader1";
			AccGLHeader glAccount = TestObjectCreator.GLHeader1;
			var salesGroup = TestObjectCreator.CreateSalesGroup("SG1");
			var staffGroup = TestObjectCreator.CreateStaffGroup("STF");
			var staff = TestObjectCreator.CreateStaff("STF");
			Factory.Save();
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccGLHeaderSubAccountSchema.Constants.TableName, glAccount.PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.LocalClient.PK);

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "ARINV1", TestObjectCreator.AUD, 1, TestObjectCreator.ABIGAS);
			arInvoice.AH_PostToGL = "Y";
			var line = TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.AUD, 1, 10, 0, 0, glAccount.PK);
			line.AL_ReverseToGL = "Y";
			Factory.Save();
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, line.PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.LocalClient.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, line.PK, AccGroupsSchema.Constants.Prefix, salesGroup.PK);

			var apCreditNote = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "APCRD1", TestObjectCreator.AUD, 1, TestObjectCreator.ABIGAS);
			apCreditNote.AH_PostToGL = "Y";
			line = TestObjectCreator.CreateInvoiceLine(apCreditNote, TestObjectCreator.AUD, 1, 10, 0, 0, glAccount.PK);
			line.AL_ReverseToGL = "Y";
			Factory.Save();
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, line.PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.LocalClient.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, line.PK, AccGroupsSchema.Constants.Prefix, salesGroup.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, line.PK, GlbStaffSchema.Constants.Prefix, staff.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, line.PK, GlbGroupSchema.Constants.Prefix, staffGroup.PK);

			DataTable resultForSettlementGroup = RunScript(dueDate, Guid.Empty, Guid.Empty);
			AssertEquals("Count of invoice should be 10", 10, resultForSettlementGroup.Rows.Count);

			var selectedRows = resultForSettlementGroup.Select(string.Format("GLAccount = 'GLHeader1'"));
			AssertEquals(2, selectedRows.Length);

			foreach (DataRow row in selectedRows)
			{
				if (row["Ledger"].ToString().Trim() == "AR" && row["TransactionType"].ToString().Trim() == "INV")
				{
					AssertEquals($"ORG: {TestObjectCreator.LocalClient.OH_Code}, SEG: {salesGroup.AR_Code}", row["MultiSubAccountTypeCode"].ToString());
					AssertEquals(TestObjectCreator.LocalClient.OH_Code, row["OrganisationSubAccount"].ToString());
					AssertEquals(salesGroup.AR_Code, row["SalesExpenseGroupsSubAccount"].ToString());
					AssertEquals("", row["StaffAndResourcesSubAccount"].ToString());
					AssertEquals("", row["StaffGroupSubAccount"].ToString());
					continue;
				}
				if (row["Ledger"].ToString().Trim() == "AP" && row["TransactionType"].ToString().Trim() == "CRD")
				{
					AssertEquals($"ORG: {TestObjectCreator.LocalClient.OH_Code}, SEG: {salesGroup.AR_Code}, STR: {staff.GS_Code}, SGP: {staffGroup.GG_Code}", row["MultiSubAccountTypeCode"].ToString());
					AssertEquals(TestObjectCreator.LocalClient.OH_Code, row["OrganisationSubAccount"].ToString());
					AssertEquals(salesGroup.AR_Code, row["SalesExpenseGroupsSubAccount"].ToString());
					AssertEquals(staff.GS_Code, row["StaffAndResourcesSubAccount"].ToString());
					AssertEquals(staffGroup.GG_Code, row["StaffGroupSubAccount"].ToString());
					continue;
				}
				Fail("Should not go here!");
			}
		}

		DataTable RunScript(ZDateTime startDate, ZGuid startLocalAccount, ZGuid endLocalAccount)
		{
			ZString sql = string.Format(@"
							EXEC GLTransactionsSP_Multilingual
							'{0}',	--@Company
							NULL,
							NULL,
							'{1}',	--@FromDate
							'{2}',	--@ToDate
							'{3}',	--@StartGLAccountPK
							'{4}',	--@EndGLAccountPK
							'{5}',	--@BranchPK
							'{6}',	--@DepartmentPK
							'',		--@DisplayDescription
							'',		--@TransactionCategory
							null,	--@BatchNumberToGet
							null,	--@BatchNumberToSet 
							'',		--@IncludeZeroBalance
							'ZH-CN',	--@Language
							'CN',
							'Y'",
						GlbCompany.CurrentCompany.PK,
						startDate.AddDays(-100).ToISO8601String(),
						startDate.AddDays(100).ToISO8601String(),
						startLocalAccount,
						endLocalAccount,
						GlbBranch.CurrentBranch.PK,
						GlbDepartment.CurrentDepartment.PK);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}

