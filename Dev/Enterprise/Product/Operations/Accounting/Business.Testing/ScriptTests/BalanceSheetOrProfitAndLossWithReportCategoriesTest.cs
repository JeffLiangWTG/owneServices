
using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class BalanceSheetOrProfitAndLossWithReportCategoriesTest : ScriptTest
	{
		[TestDate(2018, 01, 01)]
		public void TestBalanceSheetOrProfitAndLoss()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.PostPeriodsForEntireYear(2008);
			testHelper.PostPeriodsForEntireYear(2009);
			testHelper.PostPeriodsForEntireYear(2010);

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.03.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.03.96", "TS", "Test PNL 2", "P&L", Constants.DebitCredit.Debit);
			AccGLAccountDescriptor accGLAccountDescriptor = testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "61000.95", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader1PNL, "P&L", "D11");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader1PNL, "PLM", "D11");

			accGLAccountDescriptor = testObjectCreator.CreateAccountDescriptor(glHeader2PNL, "61000.96", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader2PNL, "P&L", "D11");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader2PNL, "PLM", "D11");

			testObjectCreator.CreateAccGLAggregate(1001m, 201011, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(2001m, 201011, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			AccGLHeader glHeader3PNL = testObjectCreator.CreateAccGLHeader("1200.03.97", "TS", "Test PNL 3", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4PNL = testObjectCreator.CreateAccGLHeader("1200.03.98", "TS", "Test PNL 4", "P&L", Constants.DebitCredit.Credit);

			accGLAccountDescriptor = testObjectCreator.CreateAccountDescriptor(glHeader3PNL, "63000.97", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader3PNL, "P&L", "D12");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader3PNL, "PLM", "D12");

			AccGLAccountDescriptor testAccGLAccountDescriptor4PNL = testObjectCreator.CreateAccountDescriptor(glHeader4PNL, "53000.98", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);

			testObjectCreator.CreateGLDescriptorPivot(testAccGLAccountDescriptor4PNL, glHeader4PNL, "P&L", "D13");
			testObjectCreator.CreateGLDescriptorPivot(testAccGLAccountDescriptor4PNL, glHeader4PNL, "PLM", "D13");

			testObjectCreator.CreateAccGLAggregate(3001m, 201011, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-3001m, 201011, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(1101m, 201001, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1101m, 201001, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(101m, 200911, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(201m, 200911, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(301m, 200911, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-301m, 200911, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "Test BSH 1", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "Test BSH 2", "BSH", Constants.DebitCredit.Debit);

			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader1, "53000.95", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader1, "BSH", "D01");
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader2, "53000.96", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader2, "BSH", "D01");

			testObjectCreator.CreateAccGLAggregate(1000m, 201012, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(2000m, 201012, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.97", "AS", "Test BSH 3", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4 = testObjectCreator.CreateAccGLHeader("5300.03.98", "AS", "Test BSH 4", "BSH", Constants.DebitCredit.Credit);

			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader3, "53000.97", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader3, "BSH", "D02");
			AccGLAccountDescriptor testAccGLAccountDescriptor4 = testObjectCreator.CreateAccountDescriptor(glHeader4, "53000.98", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
			testObjectCreator.CreateGLDescriptorPivot(testAccGLAccountDescriptor4, glHeader4, "BSH", "D03");

			testObjectCreator.CreateAccGLAggregate(3000m, 201012, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-3000m, 201012, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(1100m, 201001, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1100m, 201001, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(100m, 200912, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(200m, 200912, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(300m, 200912, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-300m, 200912, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();

			DataTable results = RunScript(200912, "BSS");

			AssertEquals("Should not return row", 0, results.Rows.Count);

			results = RunScript(200912, "BSH");
			DataRow row = results.Rows.Find("200912");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", 300m, row["D01"]);
			AssertEquals("TOTAL D02", 300m, row["D02"]);
			AssertEquals("TOTAL D03", 300m, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12_YED"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13_YED"]);

			results = RunScript(201001, "BSH");
			row = results.Rows.Find("201001");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", 300m, row["D01"]);
			AssertEquals("TOTAL D02", 1400m, row["D02"]);
			AssertEquals("TOTAL D03", 1400m, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13"]);
			AssertEquals("TOTAL D01", 300m, row["D01_YED"]);
			AssertEquals("TOTAL D02", 300m, row["D02_YED"]);
			AssertEquals("TOTAL D03", 300m, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12_YED"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13_YED"]);

			results = RunScript(201012, "BSH");
			row = results.Rows.Find("201012");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", 3300m, row["D01"]);
			AssertEquals("TOTAL D02", 4400m, row["D02"]);
			AssertEquals("TOTAL D03", 4400m, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13"]);
			AssertEquals("TOTAL D01", 300m, row["D01_YED"]);
			AssertEquals("TOTAL D02", 300m, row["D02_YED"]);
			AssertEquals("TOTAL D03", 300m, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12_YED"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13_YED"]);

			results = RunScript(200912, "PNL");
			row = results.Rows.Find("200912");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03"]);
			AssertEquals("TOTAL D11", -302m, row["D11"]);
			AssertEquals("TOTAL D12", -301m, row["D12"]);
			AssertEquals("TOTAL D13", 301m, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12_YED"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13_YED"]);

			results = RunScript(201001, "PNL");
			row = results.Rows.Find("201001");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", -1101m, row["D12"]);
			AssertEquals("TOTAL D13", 1101m, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", -302m, row["D11_YED"]);
			AssertEquals("TOTAL D12", -301m, row["D12_YED"]);
			AssertEquals("TOTAL D13", 301m, row["D13_YED"]);

			results = RunScript(201012, "PNL");
			row = results.Rows.Find("201012");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03"]);
			AssertEquals("TOTAL D11", -3002m, row["D11"]);
			AssertEquals("TOTAL D12", -4102m, row["D12"]);
			AssertEquals("TOTAL D13", 4102m, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", -302m, row["D11_YED"]);
			AssertEquals("TOTAL D12", -301m, row["D12_YED"]);
			AssertEquals("TOTAL D13", 301m, row["D13_YED"]);

			results = RunScript(201001, "PLM");
			row = results.Rows.Find("201001");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", -1101m, row["D12"]);
			AssertEquals("TOTAL D13", 1101m, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", -1101m, row["D12_YED"]);
			AssertEquals("TOTAL D13", 1101m, row["D13_YED"]);
		}

		DataTable RunScript(int period, string repotType)
		{
			DataTable table = DataUtils.GetDataTableFromQuery(Db.Connection,
									string.Format(@"EXEC BalanceSheetOrProfitAndLossWithReportCategories '{0}',{1},'{2}','','{3}','D01, D02, D03, D11, D12, D13, D01_YED, D02_YED, D03_YED, D11_YED, D12_YED, D13_YED','ZH-CN','CN'",
													repotType,
													period,
													GlbCompany.CurrentCompany.PK,
													GlbBranch.CurrentBranch.GB_Code
													)
								);

			table.PrimaryKey = new[] { table.Columns["Period"] };

			return table;
		}

		#region Implementation

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			base.SetUp();
		}
		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			base.TearDown();
		}

		#endregion
	}
}


