using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ChinaGLAccountBalanceTest : ScriptTest
	{
		public void TestReport_ChinaGLAccountBalanceForDifferentBranchWithBalanced()
		{
			PrepareDataForTest(true);

			var result = RunScript(null);
			AssertEquals("The result should be 4 rows.", 4, result.Rows.Count);

			var expectRows = result.Select("GLAccount = '11002233'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11002233 should be 500m.", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11002233 should be 8000m.", 8000m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33004455'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33004455 should be 900m.", 900m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33004455 should be 900m.", 900m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '11000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11000000 should be 500m.", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11000000 should be 500m.", 500m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33000000 should be 900m.", 900m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33000000 should be 900m.", 900m, expectRows[0]["OsOpeningBalance"]);

			result = RunScript(new ZGuid[] { branch1.PK, branch2.PK, branch3.PK });
			AssertEquals("The result should be 4 rows.", 4, result.Rows.Count);

			expectRows = result.Select("GLAccount = '11002233'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11002233 should be 500m.", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11002233 should be 8000m.", 8000m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33004455'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33004455 should be 900m.", 900m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33004455 should be 900m.", 900m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '11000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11000000 should be 500m.", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11000000 should be 500m.", 500m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33000000 should be 900m.", 900m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33000000 should be 900m.", 900m, expectRows[0]["OsOpeningBalance"]);

			result = RunScript(new ZGuid[] { branch1.PK, branch3.PK });
			AssertEquals("The result should be 3 rows, because GLHeader1 balance sum is ZERO and is excluded.", 3, result.Rows.Count);

			AssertEquals("The result should not contain account 11002233", 0, result.Select("GLAccount = '11002233'").Length);

			expectRows = result.Select("GLAccount = '33004455'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33004455 should be 500m (branch 3 : GLHeader2).", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33004455 should be 500m (branch 3 : GLHeader2).", 500m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '11000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11000000 should be 0m as didn't contain GLHeader1.", 0m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11000000 should be 0m as didn't contain GLHeader1.", 0m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33000000 should be 500m (branch 3 : GLHeader2).", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33000000 should be 500m (branch 3 : GLHeader2).", 500m, expectRows[0]["OsOpeningBalance"]);

			result = RunScript(new ZGuid[] { branch1.PK, branch2.PK });
			AssertEquals("The result should be 4 rows.", 4, result.Rows.Count);

			expectRows = result.Select("GLAccount = '11002233'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11002233 should be 500m (branch 1 : GLHeader1 and branch 2 : GLHeader1).", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11002233 should be 8000m (AB_OpeningBalance).", 8000m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33004455'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33004455 should be 400m, because accounting amount relative to GL-header-2 and branch3 was excluded.", 400m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33004455 should be 400m, because accounting amount relative to GL-header-2 and branch3 was excluded.", 400m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '11000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11000000 should be 500m (GLHeader 1).", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11000000 should be 500m (GLHeader 1).", 500m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33000000 should be 400m (GLHeader 2).", 400m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33000000 should be 400m (GLHeader 2).", 400m, expectRows[0]["OsOpeningBalance"]);
		}

		public void TestReport_ChinaGLAccountBalanceForDifferentBranchWithoutBalanced()
		{
			PrepareDataForTest(false);

			var result = RunScript(null);
			AssertEquals("The result should be 4 rows.", 4, result.Rows.Count);

			var expectRows = result.Select("GLAccount = '11002233'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11002233 should be 600m.", 600m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11002233 should be 8000m.", 8000m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33004455'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33004455 should be 900m.", 900m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33004455 should be 900m.", 900m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '11000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11000000 should be 600m (GLHeader 1).", 600m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11000000 should be 600m (GLHeader 1).", 600m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33000000 should be 900m (GLHeader 2).", 900m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33000000 should be 900m (GLHeader 2).", 900m, expectRows[0]["OsOpeningBalance"]);

			result = RunScript(new ZGuid[] { branch1.PK, branch2.PK, branch3.PK });
			AssertEquals("The result should be 4 rows.", 4, result.Rows.Count);

			expectRows = result.Select("GLAccount = '11002233'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11002233 should be 600m.", 600m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11002233 should be 8000m.", 8000m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33004455'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33004455 should be 900m.", 900m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33004455 should be 900m.", 900m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '11000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11000000 should be 600m (GLHeader 1).", 600m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11000000 should be 600m (GLHeader 1).", 600m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33000000 should be 900m (GLHeader 2).", 900m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33000000 should be 900m (GLHeader 2).", 900m, expectRows[0]["OsOpeningBalance"]);

			result = RunScript(new ZGuid[] { branch1.PK, branch3.PK });
			AssertEquals("The result should be 4 rows, , because GLHeader1 balance sum is not ZERO and is included.", 4, result.Rows.Count);

			expectRows = result.Select("GLAccount = '11002233'");
			AssertEquals("The opening balance of 11002233 should be 100m.", 100m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11002233 should be 0m as GLHeader1 not belong to branch1 and the balance sum is not ZERO in branch1.", 0m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33004455'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33004455 should be 500m.", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33004455 should be 500m.", 500m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '11000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11000000 should be 100m (GLHeader 1).", 100m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11000000 should be 100m (GLHeader 1).", 100m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33000000 should be 500m (GLHeader 2).", 500m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33000000 should be 500m (GLHeader 2).", 500m, expectRows[0]["OsOpeningBalance"]);

			result = RunScript(new ZGuid[] { branch1.PK, branch2.PK });
			AssertEquals("The result should be 4 rows.", 4, result.Rows.Count);

			expectRows = result.Select("GLAccount = '11002233'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11002233 should be 600m (Branch1 : GLHeader1 and Brach2 : GLHeader1).", 600m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11002233 should be 8000m (AB_OpeningBalance).", 8000m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33004455'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33004455 should be 400m, because accounting amount relative to GL-header-2 and branch3 was excluded.", 400m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33004455 should be 400m, because accounting amount relative to GL-header-2 and branch3 was excluded.", 400m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '11000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 11000000 should be 600m (GLHeader 1).", 600m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 11000000 should be 600m (GLHeader 1).", 600m, expectRows[0]["OsOpeningBalance"]);

			expectRows = result.Select("GLAccount = '33000000'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
			AssertEquals("The opening balance of 33000000 should be 400m (GLHeader 2).", 400m, expectRows[0]["OpeningBalance"]);
			AssertEquals("The opening balance of 33000000 should be 400m (GLHeader 2).", 400m, expectRows[0]["OsOpeningBalance"]);
		}

		public void TestReport_ChinaGLAccountBalance_MultipleAccountTypeForSameLocalGLAccount()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			var list = new GLLocalNumberFormatCollection();
			var localNumberFormat = list.AddNew();
			localNumberFormat.NumberFormat = "4-2-2";
			localNumberFormat.CountryCode = Core.Constants.CountryCodes.China;
			localNumberFormat.Language = Core.Constants.Languages.ChineseSimplified;
			localNumberFormat.IsFixedLength = true;
			AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var glHeader1 = TestObjectCreator.CreateAccGLHeader("1100.22.33", "AS", "BSH Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			var glHeader2 = TestObjectCreator.CreateAccGLHeader("3300.44.55", "AS", "P&L Account", Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.DebitCredit.Debit);

			var glHeader1Desc = CreateGLAccountDescriptorWihtGLDescriptorPivotForChina("", glHeader1.PK, "BalanceSheetAccount");
			CreateGLAccountDescriptorWihtGLDescriptorPivotForChina("60010020", glHeader2.PK, "ProfitAndLossAccount");
			Factory.Save();

			var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var bankAccount = TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, glHeader1);
			bankAccount.AB_GB = branch.PK;
			bankAccount.AB_OpenOSBalance = 8000m;
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			Factory.Save();

			TestObjectCreator.CreateAccGLAggregate(100m, 202012, glHeader1.PK, branch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			TestObjectCreator.CreateAccGLAggregate(200m, 202012, glHeader2.PK, branch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Factory.Save();

			DataTable result = null;

			glHeader1Desc.AJ_LocalAccountNumber = "60010010";
			Factory.Save();
			var exForTier3 = AssertExceptionThrown<System.Data.Common.DbException>(() => result = RunScript(null));
			AssertEquals(ErrorMessageForInvalidMixedTiers, exForTier3.Message);
			AssertNull(result);

			glHeader1Desc.AJ_LocalAccountNumber = "60010100";
			Factory.Save();
			var exForTier2 = AssertExceptionThrown<System.Data.Common.DbException>(() => result = RunScript(null));
			AssertEquals(ErrorMessageForInvalidMixedTiers, exForTier2.Message);
			AssertNull(result);

			glHeader1Desc.AJ_LocalAccountNumber = "60010000";
			Factory.Save();
			result = RunScript(null);
			AssertNotNull(result);
		}

		public void TestChinaGLAccountBalance_LengthValidation()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			var list = new GLLocalNumberFormatCollection();
			var localNumberFormat = list.AddNew();
			localNumberFormat.NumberFormat = "9-2-2";
			localNumberFormat.CountryCode = Core.Constants.CountryCodes.China;
			localNumberFormat.Language = Core.Constants.Languages.ChineseSimplified;
			localNumberFormat.IsFixedLength = true;
			AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var glHeader = TestObjectCreator.CreateAccGLHeader("4444.44.44", "AS", "BSH Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			var localGLAccountNum = "1234567890123";
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountNum, glHeader.PK, "BalanceSheetAccount");
			Factory.Save();

			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "CNYINV1", TestObjectCreator.CNY, 1M, 30, 0M, 30, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now, ZDateTime.Now.AddMonths(-1), ZDateTime.Now, true);
			var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var bankAccount = TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, glHeader);
			bankAccount.AB_GB = branch.PK;
			bankAccount.AB_OpenOSBalance = 8000m;
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			Factory.Save();

			TestObjectCreator.CreateAccGLAggregate(100m, 202210, glHeader.PK, branch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Factory.Save();

			var result = RunScript(null, "202210");
			AssertEquals("The result should be 2 rows.", 2, result.Rows.Count);

			var expectRows = result.Select($"GLAccount = '{localGLAccountNum}'");
			AssertEquals("expectRows should contain 1 row.", 1, expectRows.Length);
		}

		[TestDate(2020,12,12,13,14,15)]
		public void TestAmountAndOpeningBalanceWithSameAccountAndDifferentCurrency()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var localGLAccountNum = "62101000";
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountNum, AccountingConfigurationRegistry.Instance.ARControlAccount.Value, "ARControlAccount");
				Factory.Save();

				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AUDINV0", TestObjectCreator.AUD, 2M, 400, 0M, 200, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now.AddYears(-1), ZDateTime.Empty, ZDateTime.Now.AddYears(-1), true);
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AUDINV1", TestObjectCreator.AUD, 2M, 20, 0M, 10, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, true);
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "CNYINV0", TestObjectCreator.CNY, 1M, 600, 0M, 600, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now.AddYears(-1), ZDateTime.Empty, ZDateTime.Now.AddYears(-1), true);
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "CNYINV1", TestObjectCreator.CNY, 1M, 30, 0M, 30, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Now, ZDateTime.Empty, ZDateTime.Now, true);

				Factory.Save();

				var result = RunScript(null);

				var filtedResult = result.Select($"GLAccount = '{localGLAccountNum}' AND Account = '{TestObjectCreator.ABIGAS.OH_Code}' AND Period = 202012");
				AssertEquals("Should contain 2 rows.", 2, filtedResult.Length);

				foreach (var row in filtedResult)
				{
					if (row["Currency"].ToString() == TestObjectCreator.AUD.Code)
					{
						AssertEquals(20m, row["Amount"]);
						AssertEquals(200m, row["OpeningBalance"]);
						AssertEquals(400m, row["OsOpeningBalance"]);
						AssertEquals(40m, row["OsAmount"]);
					}
					else
					{
						AssertEquals(TestObjectCreator.CNY.Code, row["Currency"]);
						AssertEquals(60m, row["Amount"]);
						AssertEquals(600m, row["OpeningBalance"]);
						AssertEquals(600m, row["OsOpeningBalance"]);
						AssertEquals(60m, row["OsAmount"]);
					}
				}
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountIsTopLevel()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

				var localGLAccountNumAppropriationAccount = "40100000";
				CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountNumAppropriationAccount, (Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value, "PLAppropriationAccount");
				Factory.Save();

				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
				var glHeader1 = TestObjectCreator.CreateAccGLHeader("1100.22.33", "AS", "BSH Account 1", Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.DebitCredit.Debit);
				TestObjectCreator.CreateAccGLAggregate(1000m, 201911, glHeader1.PK, branch1.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				TestObjectCreator.CreateAccGLAggregate(100m, 201912, glHeader1.PK, branch1.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

				Factory.Save();

				var result = RunScript(null);

				var filtedResult = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals("PreCondition", 1, filtedResult.Length);

				var row = filtedResult[0];
				AssertEquals(TestObjectCreator.CNY.Code, row["Currency"]);
				AssertEquals(0m, row["Amount"]);
				AssertEquals(1100m, row["OpeningBalance"]);
				AssertEquals(1100m, row["OsOpeningBalance"]);
				AssertEquals(0m, row["OsAmount"]);
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountSetLevel3()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountLevel1 = "40100000";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel1, ZGuid.Empty);

				var localGLAccountLevel2 = "40100100";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel2, ZGuid.Empty);

				var localGLAccountNumAppropriationAccount = "40100101";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount);
				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowLocalGLAccountNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountNumAppropriationAccount["Currency"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel2 = result.Select($"GLAccount = '{localGLAccountLevel2}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel2.Length);
				var rowLocalGLAccountLevel2 = resultLocalGLAccountLevel2[0];
				CombineAssertions("check amount is aggregated to level2", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountLevel2["Currency"]);
					AssertEquals(20m, rowLocalGLAccountLevel2["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountLevel2["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountLevel2["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountLevel2["OsAmount"]);
				});

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '{localGLAccountLevel1}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel1.Length);
				var rowLocalGLAccountLevel1 = resultLocalGLAccountLevel1[0];
				CombineAssertions("check amount is aggregated to level1", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountLevel1["Currency"]);
					AssertEquals(20m, rowLocalGLAccountLevel1["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountLevel1["OsAmount"]);
				});
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountSetLevel3WithoutLevel2Number()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountLevel1 = "40100000";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel1, ZGuid.Empty);

				var localGLAccountNumAppropriationAccount = "40100101";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount);
				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowLocalGLAccountNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountNumAppropriationAccount["Currency"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel2 = result.Select($"GLAccount = '40100100' AND Period = 202012");
				AssertEquals(0, resultLocalGLAccountLevel2.Length);

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '{localGLAccountLevel1}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel1.Length);
				var rowLocalGLAccountLevel1 = resultLocalGLAccountLevel1[0];
				CombineAssertions("check amount is aggregated to level1", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountLevel1["Currency"]);
					AssertEquals(20m, rowLocalGLAccountLevel1["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountLevel1["OsAmount"]);
				});
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountSetLevel3WithoutLevel2Number_OnlyLastYearPlJournal()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountLevel1 = "40100000";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel1, ZGuid.Empty);

				var localGLAccountNumAppropriationAccount = "40100101";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount, haveCurrentYearJournal: false);
				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowLocalGLAccountNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountNumAppropriationAccount["Currency"]);
					AssertEquals(0m, rowLocalGLAccountNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(0m, rowLocalGLAccountNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel2 = result.Select($"GLAccount = '40100100' AND Period = 202012");
				AssertEquals(0, resultLocalGLAccountLevel2.Length);

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '{localGLAccountLevel1}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel1.Length);
				var rowLocalGLAccountLevel1 = resultLocalGLAccountLevel1[0];
				CombineAssertions("check amount is aggregated to level1", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountLevel1["Currency"]);
					AssertEquals(0m, rowLocalGLAccountLevel1["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OsOpeningBalance"]);
					AssertEquals(0m, rowLocalGLAccountLevel1["OsAmount"]);
				});
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountSetLevel3WithLevel1Aslevel2()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountLevel1 = "40100000";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel1, ZGuid.Empty);

				var localGLAccountNumAppropriationAccount = "40100101";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount);
				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowLocalGLAccountNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountNumAppropriationAccount["Currency"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '{localGLAccountLevel1}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel1.Length);
				var rowLocalGLAccountLevel1 = resultLocalGLAccountLevel1[0];
				CombineAssertions("check amount is aggregated to level1", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountLevel1["Currency"]);
					AssertEquals(20m, rowLocalGLAccountLevel1["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountLevel1["OsAmount"]);
				});
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountSetLevel3WithoutLevel1Number()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountLevel2 = "40100100";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel2, ZGuid.Empty);

				var localGLAccountNumAppropriationAccount = "40100101";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount);

				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowLocalGLAccountNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountNumAppropriationAccount["Currency"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel2 = result.Select($"GLAccount = '{localGLAccountLevel2}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel2.Length);
				var rowLocalGLAccountLevel2 = resultLocalGLAccountLevel2[0];
				CombineAssertions("check amount is aggregated to level2", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountLevel2["Currency"]);
					AssertEquals(20m, rowLocalGLAccountLevel2["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountLevel2["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountLevel2["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountLevel2["OsAmount"]);
				});

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '40100000' AND Period = 202012");
				AssertEquals(0, resultLocalGLAccountLevel1.Length);
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountSetLevel2()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountLevel1 = "40100000";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel1, ZGuid.Empty);

				var localGLAccountNumAppropriationAccount = "40100100";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount);
				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowLocalGLAccountNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountNumAppropriationAccount["Currency"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '{localGLAccountLevel1}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel1.Length);
				var rowLocalGLAccountLevel1 = resultLocalGLAccountLevel1[0];
				CombineAssertions("check amount is aggregated to level1", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountLevel1["Currency"]);
					AssertEquals(20m, rowLocalGLAccountLevel1["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountLevel1["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountLevel1["OsAmount"]);
				});
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountSetLevel2WithoutLevel1Number()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountNumAppropriationAccount = "40100100";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount);
				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowLocalGLAccountNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowLocalGLAccountNumAppropriationAccount["Currency"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowLocalGLAccountNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(20m, rowLocalGLAccountNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '40100000' AND Period = 202012");
				AssertEquals(0, resultLocalGLAccountLevel1.Length);
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountAmountAggregateWhenLevel3()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountLevel1 = "40100000";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel1, ZGuid.Empty);

				var localGLAccountLevel2 = "40100100";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel2, ZGuid.Empty);

				var localGLAccountLevel3 = "40100102";
				var glHeaderLocalGLAccountLevel3 = TestObjectCreator.CreateAccGLHeader("1100.99.88", "AS", "BSH Account 1", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
				TestObjectCreator.CreateAccGLAggregate(10m, 202012, glHeaderLocalGLAccountLevel3.PK, branch1.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountLevel3, glHeaderLocalGLAccountLevel3.PK, "BSH Account 1");

				var localGLAccountNumAppropriationAccount = "40100101";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount);
				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowlocalGLAccountLevelNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowlocalGLAccountLevelNumAppropriationAccount["Currency"]);
					AssertEquals(20m, rowlocalGLAccountLevelNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowlocalGLAccountLevelNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowlocalGLAccountLevelNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(20m, rowlocalGLAccountLevelNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel3 = result.Select($"GLAccount = '{localGLAccountLevel3}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel3.Length);
				var rowlocalGLAccountLevel3 = resultLocalGLAccountLevel3[0];
				CombineAssertions("basic testing, Level3 detail", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowlocalGLAccountLevel3["Currency"]);
					AssertEquals(10m, rowlocalGLAccountLevel3["Amount"]);
					AssertEquals(0m, rowlocalGLAccountLevel3["OpeningBalance"]);
					AssertEquals(0m, rowlocalGLAccountLevel3["OsOpeningBalance"]);
					AssertEquals(10m, rowlocalGLAccountLevel3["OsAmount"]);
				});

				var resultLocalGLAccountLevel2 = result.Select($"GLAccount = '{localGLAccountLevel2}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel2.Length);
				var rowlocalGLAccountLevel2 = resultLocalGLAccountLevel2[0];
				CombineAssertions("check amount is aggregated to level2", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowlocalGLAccountLevel2["Currency"]);
					AssertEquals(30m, rowlocalGLAccountLevel2["Amount"]);
					AssertEquals(1100m, rowlocalGLAccountLevel2["OpeningBalance"]);
					AssertEquals(1100m, rowlocalGLAccountLevel2["OsOpeningBalance"]);
					AssertEquals(30m, rowlocalGLAccountLevel2["OsAmount"]);
				});

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '{localGLAccountLevel1}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel1.Length);
				var rowlocalGLAccountLevel1 = resultLocalGLAccountLevel1[0];
				CombineAssertions("check amount is aggregated to level1", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowlocalGLAccountLevel1["Currency"]);
					AssertEquals(30m, rowlocalGLAccountLevel1["Amount"]);
					AssertEquals(1100m, rowlocalGLAccountLevel1["OpeningBalance"]);
					AssertEquals(1100m, rowlocalGLAccountLevel1["OsOpeningBalance"]);
					AssertEquals(30m, rowlocalGLAccountLevel1["OsAmount"]);
				});
			}
		}

		[TestDate(2020, 12, 12, 13, 14, 15)]
		public void TestProfitLossAppropriationLocalGLAccountAmountAggregateWhenLevel2()
		{
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

				var localGLAccountLevel1 = "40100000";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel1, ZGuid.Empty);

				var localGLAccountLevel2 = "40100100";
				CreateChinaProfitLossGLAccountDescriptorHeader(localGLAccountLevel2, ZGuid.Empty);

				var localGLAccountLevel3 = "40100102";
				var glHeaderLocalGLAccountLevel3 = TestObjectCreator.CreateAccGLHeader("1100.99.88", "AS", "BSH Account 1", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
				TestObjectCreator.CreateAccGLAggregate(10m, 202012, glHeaderLocalGLAccountLevel3.PK, branch1.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountLevel3, glHeaderLocalGLAccountLevel3.PK, "BSH Account 1");

				var localGLAccountNumAppropriationAccount = "40100200";
				AddJournalForPLAccount(branch1, localGLAccountNumAppropriationAccount);
				Factory.Save();

				var result = RunScript(null);

				var resultLocalGLAccountNumAppropriationAccount = result.Select($"GLAccount = '{localGLAccountNumAppropriationAccount}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountNumAppropriationAccount.Length);
				var rowlocalGLAccountLevelNumAppropriationAccount = resultLocalGLAccountNumAppropriationAccount[0];
				CombineAssertions("basic testing, ProfitLossAppropriation", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowlocalGLAccountLevelNumAppropriationAccount["Currency"]);
					AssertEquals(20m, rowlocalGLAccountLevelNumAppropriationAccount["Amount"]);
					AssertEquals(1100m, rowlocalGLAccountLevelNumAppropriationAccount["OpeningBalance"]);
					AssertEquals(1100m, rowlocalGLAccountLevelNumAppropriationAccount["OsOpeningBalance"]);
					AssertEquals(20m, rowlocalGLAccountLevelNumAppropriationAccount["OsAmount"]);
				});

				var resultLocalGLAccountLevel3 = result.Select($"GLAccount = '{localGLAccountLevel3}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel3.Length);
				var rowlocalGLAccountLevel3 = resultLocalGLAccountLevel3[0];
				CombineAssertions("basic testing, Level3 detail", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowlocalGLAccountLevel3["Currency"]);
					AssertEquals(10m, rowlocalGLAccountLevel3["Amount"]);
					AssertEquals(0m, rowlocalGLAccountLevel3["OpeningBalance"]);
					AssertEquals(0m, rowlocalGLAccountLevel3["OsOpeningBalance"]);
					AssertEquals(10m, rowlocalGLAccountLevel3["OsAmount"]);
				});

				var resultLocalGLAccountLevel2 = result.Select($"GLAccount = '{localGLAccountLevel2}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel2.Length);
				var rowlocalGLAccountLevel2 = resultLocalGLAccountLevel2[0];
				CombineAssertions("check amount is aggregated to level2", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowlocalGLAccountLevel2["Currency"]);
					AssertEquals(10m, rowlocalGLAccountLevel2["Amount"]);
					AssertEquals(0m, rowlocalGLAccountLevel2["OpeningBalance"]);
					AssertEquals(0m, rowlocalGLAccountLevel2["OsOpeningBalance"]);
					AssertEquals(10m, rowlocalGLAccountLevel2["OsAmount"]);
				});

				var resultLocalGLAccountLevel1 = result.Select($"GLAccount = '{localGLAccountLevel1}' AND Period = 202012");
				AssertEquals(1, resultLocalGLAccountLevel1.Length);
				var rowlocalGLAccountLevel1 = resultLocalGLAccountLevel1[0];
				CombineAssertions("check amount is aggregated to level1", () => {
					AssertEquals(TestObjectCreator.CNY.Code, rowlocalGLAccountLevel1["Currency"]);
					AssertEquals(30m, rowlocalGLAccountLevel1["Amount"]);
					AssertEquals(1100m, rowlocalGLAccountLevel1["OpeningBalance"]);
					AssertEquals(1100m, rowlocalGLAccountLevel1["OsOpeningBalance"]);
					AssertEquals(30m, rowlocalGLAccountLevel1["OsAmount"]);
				});
			}
		}

		void AddJournalForPLAccount(GlbBranch branch, string localGLAccountNumAppropriationAccount, bool haveCurrentYearJournal = true)
		{
			if (haveCurrentYearJournal)
			{
				TestObjectCreator.CreateAccGLAggregate(20m, 202012, (Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value, branch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			}

			CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountNumAppropriationAccount, (Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value, "PLAppropriationAccount");
			var glHeaderLocalGLAccountNumAppropriationAccount = TestObjectCreator.CreateAccGLHeader("1100.22.33", "AS", "BSH Account 1", Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.DebitCredit.Debit);
			TestObjectCreator.CreateAccGLAggregate(1000m, 201911, glHeaderLocalGLAccountNumAppropriationAccount.PK, branch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			TestObjectCreator.CreateAccGLAggregate(100m, 201912, glHeaderLocalGLAccountNumAppropriationAccount.PK, branch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
		}

		public void TestTransactionHeaderMatchedResult_ExtremeAR()
		{
			const decimal exchangeRateTolerance = 0.1m;
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();

				var localGLAccountNum = "62101000";
				CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountNum, AccountingConfigurationRegistry.Instance.ARControlAccount.Value, "ARControlAccount");
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

				var invoice1 = CreateHeader<ARInvoice>(new ZDate(2019, 04, 01), 1000m, TestObjectCreator.Debtor, true);
				Factory.Save();
				PayInvoice(invoice1, new ZDate(2019, 07, 01), 100m, 100m, true, exchangeRateTolerance, "M0001A");
				Factory.Save();

				PayInvoice(invoice1, new ZDate(2019, 07, 02), 200m, 200m, false, exchangeRateTolerance, "M0001B");
				Factory.Save();

				var invoice2 = CreateHeader<ARInvoice>(new ZDate(2019, 04, 01), 1000m, TestObjectCreator.Debtor, false);
				Factory.Save();
				PayInvoice(invoice2, new ZDate(2019, 07, 01), 300m, 900m, false, exchangeRateTolerance, "M0002A");
				Factory.Save();
				PayInvoice(invoice2, new ZDate(2019, 07, 02), 400m, 1200m, false, exchangeRateTolerance, "M0002B");
				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertForNewOsOutstandingAmountFeatureEnabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					AssertForNewOsOutstandingAmountFeatureEnabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertForNewOsOutstandingAmountFeatureDisabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					AssertForNewOsOutstandingAmountFeatureDisabled(localGLAccountNum);
				}
			}

			void AssertForNewOsOutstandingAmountFeatureEnabled(string localGLAccountNum)
			{
				var result201907 = RunScript(null, "201907");
				var filtedResult201907 = result201907.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Debtor.OH_Code}'");
				AssertEquals(1, filtedResult201907.Length);
				var rowResult201907 = filtedResult201907[0];
				CombineAssertions("201907 should not contain match links result.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201907["Currency"]);
					AssertEquals("Amount, Receipt Local Amount(100+200+900+1200) / 2", -1200.0000m, rowResult201907["Amount"]);
					AssertEquals("OpeningBalance, Invoice Local Amount", 1000.0000m, rowResult201907["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Invoice OS Amount", 2000.0000m, rowResult201907["OsOpeningBalance"]);
					AssertEquals("OsAmount, Receipt OS Amount(100+200+900+1200)", -2400.0000m, rowResult201907["OsAmount"]);
				});

				var result201908 = RunScript(null, "201908");
				var filtedResult201908 = result201908.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Debtor.OH_Code}'");
				AssertEquals(1, filtedResult201908.Length);
				var rowResult201908 = filtedResult201908[0];
				CombineAssertions("201908 should contain match links result, invoice2 do not use AP_OSAmount due to AH_IsOSOutstandingAmountApplicable is false.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201908["Currency"]);
					AssertEquals("Amount", 0m, rowResult201908["Amount"]);
					AssertEquals("OpeningBalance, Remaining Receipt Local Amount", -200.0000m, rowResult201908["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Remaining Receipt OS Amount, the 0.1m is exchange rate tolerance saved at AP_OSAmount, missing 200m due to M0001B is dirty data.", -200.1000m, rowResult201908["OsOpeningBalance"]);
					AssertEquals("OsAmount", 0m, rowResult201908["OsAmount"]);
				});
			}

			void AssertForNewOsOutstandingAmountFeatureDisabled(string localGLAccountNum)
			{
				var result201907 = RunScript(null, "201907");
				var filtedResult201907 = result201907.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Debtor.OH_Code}'");
				AssertEquals(1, filtedResult201907.Length);
				var rowResult201907 = filtedResult201907[0];
				CombineAssertions("201907 should not contain match links result.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201907["Currency"]);
					AssertEquals("Amount, Receipt Local Amount(100+200+900+1200) / 2", -1200.0000m, rowResult201907["Amount"]);
					AssertEquals("OpeningBalance, Invoice Local Amount", 1000.0000m, rowResult201907["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Invoice OS Amount", 2000.0000m, rowResult201907["OsOpeningBalance"]);
					AssertEquals("OsAmount, Receipt OS Amount(100+200+900+1200)", -2400.0000m, rowResult201907["OsAmount"]);
				});

				var result201908 = RunScript(null, "201908");
				var filtedResult201908 = result201908.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Debtor.OH_Code}'");
				AssertEquals(1, filtedResult201908.Length);
				var rowResult201908 = filtedResult201908[0];
				CombineAssertions("201908 should contain match links result, but will not use AP_OSAmount due to Registry is disabled.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201908["Currency"]);
					AssertEquals("Amount", 0m, rowResult201908["Amount"]);
					AssertEquals("OpeningBalance, Remaining Receipt Local Amount", -200.0000m, rowResult201908["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Remaining Receipt OS Amount.", -400.0000m, rowResult201908["OsOpeningBalance"]);
					AssertEquals("OsAmount", 0m, rowResult201908["OsAmount"]);
				});
			}
		}

		public void TestTransactionHeaderMatchedResult_StandardAR()
		{
			const decimal exchangeRateTolerance = 0.1m;
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();

				var localGLAccountNum = "62101000";
				CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountNum, AccountingConfigurationRegistry.Instance.ARControlAccount.Value, "ARControlAccount");
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

				var invoice1 = CreateHeader<ARInvoice>(new ZDate(2019, 04, 01), 1000m, TestObjectCreator.Debtor, true);
				Factory.Save();
				PayInvoice(invoice1, new ZDate(2019, 07, 01), 100m, 100m, true, exchangeRateTolerance, "M0001A");
				Factory.Save();

				PayInvoice(invoice1, new ZDate(2019, 07, 02), 200m, 200m, true, exchangeRateTolerance, "M0001B");
				Factory.Save();

				var invoice2 = CreateHeader<ARInvoice>(new ZDate(2019, 04, 01), 1000m, TestObjectCreator.Debtor, false);
				Factory.Save();
				PayInvoice(invoice2, new ZDate(2019, 07, 01), 300m, 900m, false, exchangeRateTolerance, "M0002A");
				Factory.Save();
				PayInvoice(invoice2, new ZDate(2019, 07, 02), 400m, 1200m, false, exchangeRateTolerance, "M0002B");
				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertForNewOsOutstandingAmountFeatureEnabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					AssertForNewOsOutstandingAmountFeatureEnabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertForNewOsOutstandingAmountFeatureDisabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					AssertForNewOsOutstandingAmountFeatureDisabled(localGLAccountNum);
				}
			}

			void AssertForNewOsOutstandingAmountFeatureEnabled(string localGLAccountNum)
			{
				var result201907 = RunScript(null, "201907");
				var filtedResult201907 = result201907.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Debtor.OH_Code}'");
				AssertEquals(1, filtedResult201907.Length);
				var rowResult201907 = filtedResult201907[0];
				CombineAssertions("201907 should not contain match links result.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201907["Currency"]);
					AssertEquals("Amount, Receipt Local Amount(100+200+900+1200) / 2", -1200.0000m, rowResult201907["Amount"]);
					AssertEquals("OpeningBalance, Invoice Local Amount", 1000.0000m, rowResult201907["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Invoice OS Amount", 2000.0000m, rowResult201907["OsOpeningBalance"]);
					AssertEquals("OsAmount, Receipt OS Amount(100+200+900+1200)", -2400.0000m, rowResult201907["OsAmount"]);
				});

				var result201908 = RunScript(null, "201908");
				var filtedResult201908 = result201908.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Debtor.OH_Code}'");
				AssertEquals(1, filtedResult201908.Length);
				var rowResult201908 = filtedResult201908[0];
				CombineAssertions("201908 should contain match links result, invoice2 do not use AP_OSAmount due to AH_IsOSOutstandingAmountApplicable is false.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201908["Currency"]);
					AssertEquals("Amount", 0m, rowResult201908["Amount"]);
					AssertEquals("OpeningBalance, Remaining Receipt Local Amount", -200.0000m, rowResult201908["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Remaining Receipt OS Amount, the 0.2m is exchange rate tolerance saved at AP_OSAmount(2 matchlinks)", -400.2000m, rowResult201908["OsOpeningBalance"]);
					AssertEquals("OsAmount", 0m, rowResult201908["OsAmount"]);
				});
			}

			void AssertForNewOsOutstandingAmountFeatureDisabled(string localGLAccountNum)
			{
				var result201907 = RunScript(null, "201907");
				var filtedResult201907 = result201907.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Debtor.OH_Code}'");
				AssertEquals(1, filtedResult201907.Length);
				var rowResult201907 = filtedResult201907[0];
				CombineAssertions("201907 should not contain match links result.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201907["Currency"]);
					AssertEquals("Amount, Receipt Local Amount(100+200+900+1200) / 2", -1200.0000m, rowResult201907["Amount"]);
					AssertEquals("OpeningBalance, Invoice Local Amount", 1000.0000m, rowResult201907["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Invoice OS Amount", 2000.0000m, rowResult201907["OsOpeningBalance"]);
					AssertEquals("OsAmount, Receipt OS Amount(100+200+900+1200)", -2400.0000m, rowResult201907["OsAmount"]);
				});

				var result201908 = RunScript(null, "201908");
				var filtedResult201908 = result201908.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Debtor.OH_Code}'");
				AssertEquals(1, filtedResult201908.Length);
				var rowResult201908 = filtedResult201908[0];
				CombineAssertions("201908 should contain match links result, but will not use AP_OSAmount due to Registry is disabled.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201908["Currency"]);
					AssertEquals("Amount", 0m, rowResult201908["Amount"]);
					AssertEquals("OpeningBalance, Remaining Receipt Local Amount", -200.0000m, rowResult201908["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Remaining Receipt OS Amount.", -400.0000m, rowResult201908["OsOpeningBalance"]);
					AssertEquals("OsAmount", 0m, rowResult201908["OsAmount"]);
				});
			}
		}

		public void TestTransactionHeaderMatchedResult_ExtremeAP()
		{
			const decimal exchangeRateTolerance = 0.1m;
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();

				var localGLAccountNum = "62102000";
				CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountNum, AccountingConfigurationRegistry.Instance.APControlAccount.Value, "APControlAccount");
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

				var invoice1 = CreateHeader<APInvoice>(new ZDate(2019, 04, 01), 1000m, TestObjectCreator.Creditor1, true);
				Factory.Save();
				PayInvoice(invoice1, new ZDate(2019, 07, 01), 100m, 100m, true, exchangeRateTolerance, "M0001A");
				Factory.Save();

				PayInvoice(invoice1, new ZDate(2019, 07, 02), 200m, 200m, false, exchangeRateTolerance, "M0001B");
				Factory.Save();

				var invoice2 = CreateHeader<APInvoice>(new ZDate(2019, 04, 01), 1000m, TestObjectCreator.Creditor1, false);
				Factory.Save();
				PayInvoice(invoice2, new ZDate(2019, 07, 01), 300m, 900m, false, exchangeRateTolerance, "M0002A");
				Factory.Save();
				PayInvoice(invoice2, new ZDate(2019, 07, 02), 400m, 1200m, false, exchangeRateTolerance, "M0002B");
				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertForNewOsOutstandingAmountFeatureEnabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					AssertForNewOsOutstandingAmountFeatureEnabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertForNewOsOutstandingAmountFeatureDisabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					AssertForNewOsOutstandingAmountFeatureDisabled(localGLAccountNum);
				}
			}

			void AssertForNewOsOutstandingAmountFeatureEnabled(string localGLAccountNum)
			{
				var result201907 = RunScript(null, "201907");
				var filtedResult201907 = result201907.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Creditor1.OH_Code}'");
				AssertEquals(1, filtedResult201907.Length);
				var rowResult201907 = filtedResult201907[0];
				CombineAssertions("201907 should not contain match links result.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201907["Currency"]);
					AssertEquals("Amount, Payment Local Amount(100+200+900+1200) / 2", 1200.0000m, rowResult201907["Amount"]);
					AssertEquals("OpeningBalance, Invoice Local Amount", -1000.0000m, rowResult201907["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Invoice OS Amount", -2000.0000m, rowResult201907["OsOpeningBalance"]);
					AssertEquals("OsAmount, Payment OS Amount(100+200+900+1200)", 2400.0000m, rowResult201907["OsAmount"]);
				});

				var result201908 = RunScript(null, "201908");
				var filtedResult201908 = result201908.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Creditor1.OH_Code}'");
				AssertEquals(1, filtedResult201908.Length);
				var rowResult201908 = filtedResult201908[0];
				CombineAssertions("201908 should contain match links result, invoice2 do not use AP_OSAmount due to AH_IsOSOutstandingAmountApplicable is false.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201908["Currency"]);
					AssertEquals("Amount", 0m, rowResult201908["Amount"]);
					AssertEquals("OpeningBalance, Remaining Payment Local Amount", 200.0000m, rowResult201908["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Remaining Payment OS Amount, the 0.1m is exchange rate tolerance saved at AP_OSAmount, missing 200m due to M0001B is dirty data.", 200.1m, rowResult201908["OsOpeningBalance"]);
					AssertEquals("OsAmount", 0m, rowResult201908["OsAmount"]);
				});
			}

			void AssertForNewOsOutstandingAmountFeatureDisabled(string localGLAccountNum)
			{
				var result201907 = RunScript(null, "201907");
				var filtedResult201907 = result201907.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Creditor1.OH_Code}'");
				AssertEquals(1, filtedResult201907.Length);
				var rowResult201907 = filtedResult201907[0];
				CombineAssertions("201907 should not contain match links result.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201907["Currency"]);
					AssertEquals("Amount, Payment Local Amount(100+200+900+1200) / 2", 1200.0000m, rowResult201907["Amount"]);
					AssertEquals("OpeningBalance, Invoice Local Amount", -1000.0000m, rowResult201907["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Invoice OS Amount", -2000.0000m, rowResult201907["OsOpeningBalance"]);
					AssertEquals("OsAmount, Payment OS Amount(100+200+900+1200)", 2400.0000m, rowResult201907["OsAmount"]);
				});

				var result201908 = RunScript(null, "201908");
				var filtedResult201908 = result201908.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Creditor1.OH_Code}'");
				AssertEquals(1, filtedResult201908.Length);
				var rowResult201908 = filtedResult201908[0];
				CombineAssertions("201908 should contain match links result, but will not use AP_OSAmount due to Registry is disabled.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201908["Currency"]);
					AssertEquals("Amount", 0m, rowResult201908["Amount"]);
					AssertEquals("OpeningBalance, Remaining Payment Local Amount", 200.0000m, rowResult201908["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Remaining Payment OS Amount.", 400.0000m, rowResult201908["OsOpeningBalance"]);
					AssertEquals("OsAmount", 0m, rowResult201908["OsAmount"]);
				});
			}
		}

		public void TestTransactionHeaderMatchedResult_StandardAP()
		{
			const decimal exchangeRateTolerance = 0.1m;
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.DataType.SuspendValidation())
			using (AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				Factory.Save();

				var localGLAccountNum = "62102000";
				CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(localGLAccountNum, AccountingConfigurationRegistry.Instance.APControlAccount.Value, "APControlAccount");
				TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

				var invoice1 = CreateHeader<APInvoice>(new ZDate(2019, 04, 01), 1000m, TestObjectCreator.Creditor1, true);
				Factory.Save();
				PayInvoice(invoice1, new ZDate(2019, 07, 01), 100m, 100m, true, exchangeRateTolerance, "M0001A");
				Factory.Save();

				PayInvoice(invoice1, new ZDate(2019, 07, 02), 200m, 200m, true, exchangeRateTolerance, "M0001B");
				Factory.Save();

				var invoice2 = CreateHeader<APInvoice>(new ZDate(2019, 04, 01), 1000m, TestObjectCreator.Creditor1, false);
				Factory.Save();
				PayInvoice(invoice2, new ZDate(2019, 07, 01), 300m, 900m, false, exchangeRateTolerance, "M0002A");
				Factory.Save();
				PayInvoice(invoice2, new ZDate(2019, 07, 02), 400m, 1200m, false, exchangeRateTolerance, "M0002B");
				Factory.Save();

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertForNewOsOutstandingAmountFeatureEnabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					AssertForNewOsOutstandingAmountFeatureEnabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertForNewOsOutstandingAmountFeatureDisabled(localGLAccountNum);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					AssertForNewOsOutstandingAmountFeatureDisabled(localGLAccountNum);
				}
			}

			void AssertForNewOsOutstandingAmountFeatureEnabled(string localGLAccountNum)
			{
				var result201907 = RunScript(null, "201907");
				var filtedResult201907 = result201907.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Creditor1.OH_Code}'");
				AssertEquals(1, filtedResult201907.Length);
				var rowResult201907 = filtedResult201907[0];
				CombineAssertions("201907 should not contain match links result.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201907["Currency"]);
					AssertEquals("Amount, Payment Local Amount(100+200+900+1200) / 2", 1200.0000m, rowResult201907["Amount"]);
					AssertEquals("OpeningBalance, Invoice Local Amount", -1000.0000m, rowResult201907["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Invoice OS Amount", -2000.0000m, rowResult201907["OsOpeningBalance"]);
					AssertEquals("OsAmount, Payment OS Amount(100+200+900+1200)", 2400.0000m, rowResult201907["OsAmount"]);
				});

				var result201908 = RunScript(null, "201908");
				var filtedResult201908 = result201908.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Creditor1.OH_Code}'");
				AssertEquals(1, filtedResult201908.Length);
				var rowResult201908 = filtedResult201908[0];
				CombineAssertions("201908 should contain match links result, invoice2 do not use AP_OSAmount due to AH_IsOSOutstandingAmountApplicable is false.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201908["Currency"]);
					AssertEquals("Amount", 0m, rowResult201908["Amount"]);
					AssertEquals("OpeningBalance, Remaining Payment Local Amount", 200.0000m, rowResult201908["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Remaining Payment OS Amount, the 0.2m is exchange rate tolerance saved at AP_OSAmount(2 matchlinks)", 400.2000m, rowResult201908["OsOpeningBalance"]);
					AssertEquals("OsAmount", 0m, rowResult201908["OsAmount"]);
				});
			}

			void AssertForNewOsOutstandingAmountFeatureDisabled(string localGLAccountNum)
			{
				var result201907 = RunScript(null, "201907");
				var filtedResult201907 = result201907.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Creditor1.OH_Code}'");
				AssertEquals(1, filtedResult201907.Length);
				var rowResult201907 = filtedResult201907[0];
				CombineAssertions("201907 should not contain match links result.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201907["Currency"]);
					AssertEquals("Amount, Payment Local Amount(100+200+900+1200) / 2", 1200.0000m, rowResult201907["Amount"]);
					AssertEquals("OpeningBalance, Invoice Local Amount", -1000.0000m, rowResult201907["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Invoice OS Amount", -2000.0000m, rowResult201907["OsOpeningBalance"]);
					AssertEquals("OsAmount, Payment OS Amount(100+200+900+1200)", 2400.0000m, rowResult201907["OsAmount"]);
				});

				var result201908 = RunScript(null, "201908");
				var filtedResult201908 = result201908.Select($"GLAccount = '{localGLAccountNum}' AND  Account = '{TestObjectCreator.Creditor1.OH_Code}'");
				AssertEquals(1, filtedResult201908.Length);
				var rowResult201908 = filtedResult201908[0];
				CombineAssertions("201908 should contain match links result, but will not use AP_OSAmount due to Registry is disabled.", () => {
					AssertEquals("Currency", TestObjectCreator.USD.Code, rowResult201908["Currency"]);
					AssertEquals("Amount", 0m, rowResult201908["Amount"]);
					AssertEquals("OpeningBalance, Remaining Payment Local Amount", 200.0000m, rowResult201908["OpeningBalance"]);
					AssertEquals("OsOpeningBalance, Remaining Payment OS Amount.", 400.0000m, rowResult201908["OsOpeningBalance"]);
					AssertEquals("OsAmount", 0m, rowResult201908["OsAmount"]);
				});
			}
		}

		InvoicingBase CreateHeader<T>(ZDate invoicePostDate, decimal invoiceAmount, OrgHeader organisation, bool isEnableNewOSOutstandingAmountFeature) where T : InvoicingBase
		{
			var exchangeRate = 0.5m;

			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature))
			{
				var invoice = TestObjectCreator.CreateInvoice(typeof(T), currency: TestObjectCreator.USD, exchangeRate, organisation, invoiceDate: invoicePostDate);
				TestObjectCreator.CreateInvoiceLine(invoice, invoiceAmount, currency: TestObjectCreator.USD, exchangeRate: exchangeRate, setTaxes: false);
				invoice.AH_PostToGL = "Y";
				invoice.AH_PostDate = invoicePostDate;
				invoice.AH_IsOSOutstandingAmountApplicable = isEnableNewOSOutstandingAmountFeature;
				invoice.AH_OSOutstandingAmount = isEnableNewOSOutstandingAmountFeature
					? invoice.AH_OSTotal
					: 0;
				return invoice;
			}
		}

		void PayInvoice(InvoicingBase invoice, ZDate paidDate, decimal paidAmount, decimal receiptPaymentAmount, bool isEnableNewOSOutstandingAmountFeature, decimal partialPaidExchangeTolerance, string matchGroupNum)
		{
			ReceiptPaymentBase receiptPayment;
			using (AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableNewOSOutstandingAmountFeature))
			{
				switch (invoice.AH_Ledger)
				{
					case LedgerTypes.AccountsReceivable:
						receiptPayment = TestObjectCreator.CreateARReceipt(invoice.ExchangeRate.Rate, receiptPaymentAmount, paidDate, paidDate, invoice.AH_OH, TestObjectCreator.USDBankAccount.PK);
						receiptPayment.AH_PostToGL = "Y";
						MatchInvoice(invoice, receiptPayment, paidDate, paidAmount, matchGroupNum, invoice.AH_ExchangeRate, partialPaidExchangeTolerance);

						break;
					case LedgerTypes.AccountsPayable:
						receiptPayment = TestObjectCreator.CreateAPPayment(invoice.ExchangeRate.Rate, receiptPaymentAmount, paidDate, paidDate, invoice.AH_OH, TestObjectCreator.USDBankAccount.PK);
						receiptPayment.AH_PostToGL = "Y";
						MatchInvoice(invoice, receiptPayment, paidDate, paidAmount, matchGroupNum, invoice.AH_ExchangeRate, partialPaidExchangeTolerance);

						break;
					default:
						Assert(false);
						break;
				}

				if (invoice.AH_IsOSOutstandingAmountApplicable)
				{
					invoice.AH_OSOutstandingAmount -= paidAmount;
				}
			}
		}

		void MatchInvoice(InvoicingBase invoice, ReceiptPaymentBase receiptPaymentBase, ZDateTime matchingDate, decimal partPaidAmount, string matchGroupNumber, decimal exchangeRate, decimal exchangeRateTolerance)
		{
			var payInFull = partPaidAmount == invoice.AH_Calc_OSOutstandingAmount;
			var localMatchingDate = matchingDate;
			var invoiceAsIMatching = (IMatching)invoice;
			invoiceAsIMatching.CurrentMatchGroup.RemoveAll();
			invoiceAsIMatching.OSPartialPaymentAmount = invoiceAsIMatching.OSOutstandingAmount;
			if (payInFull)
			{
				invoiceAsIMatching.FullyPay(localMatchingDate);
			}
			else
			{
				invoiceAsIMatching.OSPartialPaymentAmount = partPaidAmount * invoice.Multiplier_ForTestOnly;
				invoiceAsIMatching.PartiallyPay();
			}
			invoiceAsIMatching.GenerateMatchLinks();
			UpdateMatchingLink(invoice);

			var receiptPaymentBaseAsIMatching = (IMatching)receiptPaymentBase;
			var payInFullReceipt = partPaidAmount == receiptPaymentBase.AH_Calc_OSOutstandingAmount;
			if (payInFullReceipt)
			{
				receiptPaymentBaseAsIMatching.FullyPay(localMatchingDate);
			}
			else
			{
				receiptPaymentBaseAsIMatching.OSPartialPaymentAmount = partPaidAmount * receiptPaymentBase.Multiplier_ForTestOnly;
				receiptPaymentBaseAsIMatching.PartiallyPay();
			}
			receiptPaymentBaseAsIMatching.GenerateMatchLinks();
			UpdateMatchingLink(receiptPaymentBase);

			invoiceAsIMatching.CurrentMatchGroup.AddRange(receiptPaymentBaseAsIMatching.CurrentMatchGroup);
			receiptPaymentBaseAsIMatching.CurrentMatchGroup.RemoveAll();
			invoiceAsIMatching.CurrentMatchGroup.SetMatchGroupNumberAndMatchDate(matchGroupNumber, localMatchingDate);

			void UpdateMatchingLink(TransactionHeader accTransactionHeader)
			{
				var matchLink = Factory.Load<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, accTransactionHeader.PK))
					.OrderByDescending(x => x.AP_MatchDate)
					.FirstOrDefault(x => !x.IsInDatabase);
				matchLink.AP_OSAmount = AccountingMasterFilesRegistry.Instance.EnableNewOSOutstandingAmountFeature.Value
					? (partPaidAmount + exchangeRateTolerance) * accTransactionHeader.Multiplier_ForTestOnly
					: 0;
			}
		}

		void PrepareDataForTest(bool isBalanced)
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Core.Constants.CountryCodes.China;
			gNF.Language = Core.Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;
			AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var glHeader1 = TestObjectCreator.CreateAccGLHeader("1100.22.33", "AS", "BSH Account 1", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			var glHeader2 = TestObjectCreator.CreateAccGLHeader("3300.44.55", "AS", "BSH Account 2", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);

			CreateGLAccountDescriptorWihtGLDescriptorPivotForChina("11002233", glHeader1.PK, "BSH Account 1");
			CreateGLAccountDescriptorWihtGLDescriptorPivotForChina("33004455", glHeader2.PK, "BSH Account 2");
			CreateGLAccountDescriptorWihtGLDescriptorPivotForChina("49004901", new ZGuid("176e946d-3483-4b2b-aa32-e348cbd94dfd"), "Dummy Local Account");

			Factory.Save();

			branch1 = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			branch2 = TestObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);
			branch3 = TestObjectCreator.CreateBranch("CCC", GlbCompany.CurrentCompany);

			var bankAccount = TestObjectCreator.CreateBankAccount("BANK", "Bank account", TestObjectCreator.AUD, glHeader1);
			bankAccount.AB_GB = branch2.PK;
			bankAccount.AB_OpenOSBalance = 8000m;

			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

			Factory.Save();

			TestObjectCreator.CreateAccGLAggregate(100m, 202011, glHeader1.PK, branch1.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			if (isBalanced)
			{
				TestObjectCreator.CreateAccGLAggregate(-100m, 202011, glHeader1.PK, branch1.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			}
			TestObjectCreator.CreateAccGLAggregate(200m, 202011, glHeader1.PK, branch2.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			TestObjectCreator.CreateAccGLAggregate(300m, 202011, glHeader1.PK, branch2.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			TestObjectCreator.CreateAccGLAggregate(400m, 202011, glHeader2.PK, branch2.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			TestObjectCreator.CreateAccGLAggregate(500m, 202011, glHeader2.PK, branch3.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();
		}

		AccGLAccountDescriptor CreateGLAccountDescriptorWihtGLDescriptorPivotForChina(string localAccountNum, ZGuid parentGLHeaderPK, string accountDescription)
		{
			var descriptor = CreateGLAccountDescriptorHeader(localAccountNum, parentGLHeaderPK, accountDescription);

			var pivot = Factory.New<AccGLDescriptorPivot>();
			pivot.YJ_AG = parentGLHeaderPK;
			pivot.YJ_AJ = descriptor.PK;

			return descriptor;
		}

		AccGLAccountDescriptor CreateChinaProfitLossGLAccountDescriptorHeader(string localAccountNum, ZGuid parentGLHeaderPK)
			=> CreateGLAccountDescriptorHeader(localAccountNum, parentGLHeaderPK, "China PL Account Header");

		AccGLAccountDescriptor CreateGLAccountDescriptorHeader(string localAccountNum, ZGuid parentGLHeaderPK, string accountDescription)
		{
			var descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			descriptor.AJ_Language = Core.Constants.Languages.ChineseSimplified;
			descriptor.ParentGLHeaderPK = parentGLHeaderPK;
			descriptor.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			descriptor.AJ_LocalAccountNumber = localAccountNum;
			descriptor.AJ_ReportCategory = Core.Constants.AccountType.Header;
			descriptor.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			descriptor.AJ_AccountDescription = accountDescription;
			return descriptor;
		}

		GlbBranch branch1, branch2, branch3;

		DataTable RunScript(ZGuid[] branchPKList , string endPeriod = "202012")
		{
			var branchPKListStringBuilder = new ZStringBuilder();
			branchPKList?.ForEach(x => branchPKListStringBuilder.Append(x.ToString()).Append(","));

			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
EXEC ChinaGLAccountBalance
'{0}',	--@CompanyPK
'{1}',	--@EndPeriod
'{2}',	--@BranchPKList
''	--@IncludePeriodEndCLosing
",
			GlbCompany.CurrentCompany.PK,
			endPeriod,
			branchPKListStringBuilder.ToString().TrimEnd(',')
			));
		}

		internal const string ErrorMessageForInvalidMixedTiers = "China local tier 2 and tier 3 GL Accounts with the same tier 1 account number must have the same account type. Please check your local GL accounts and its sub GL accounts, to ensure that they have the same account type.";
	}
}
