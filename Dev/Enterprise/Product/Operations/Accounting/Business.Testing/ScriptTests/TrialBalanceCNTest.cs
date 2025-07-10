

using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class TrialBalanceCNTest : ScriptTest
	{
		[TestDate(2018, 01, 01)]
		public void TestTotal()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.PostPeriodsForEntireYear(2008);
			testHelper.PostPeriodsForEntireYear(2009);
			testHelper.PostPeriodsForEntireYear(2010);

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader glHeader0PNL = testObjectCreator.CreateAccGLHeader("1100.90.00", "TS", "Test PNL 0", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.93.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.93.96", "TS", "Test PNL 2", "P&L", Constants.DebitCredit.Debit);

			testObjectCreator.CreateAccountDescriptor(glHeader0PNL, "6100.000", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "6100.095", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(glHeader2PNL, "6100.096", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);

			testObjectCreator.CreateAccGLAggregate(1001m, 201011, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(2001m, 201011, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			AccGLHeader glHeader3PNL = testObjectCreator.CreateAccGLHeader("1200.03.97", "TS", "Test PNL 3", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4PNL = testObjectCreator.CreateAccGLHeader("1200.03.98", "TS", "Test PNL 4", "P&L", Constants.DebitCredit.Credit);

			testObjectCreator.CreateAccountDescriptor(glHeader3PNL, "6100.097", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(glHeader4PNL, "5300.098", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);

			testObjectCreator.CreateAccGLAggregate(3001m, 201011, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-3001m, 201011, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(1101m, 201001, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1101m, 201001, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(101m, 200911, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(201m, 200911, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(301m, 200911, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-301m, 200911, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			AccGLHeader glHeader0 = testObjectCreator.CreateAccGLHeader("5300.90.00", "AS", "Test BSH 0", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.93.95", "AS", "Test BSH 1", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.93.96", "AS", "Test BSH 2", "BSH", Constants.DebitCredit.Debit);

			testObjectCreator.CreateAccountDescriptor(glHeader0, "5300.000", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(glHeader1, "5300.095", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(glHeader2, "5300.096", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);

			testObjectCreator.CreateAccGLAggregate(1000m, 201012, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(2000m, 201012, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.97", "AS", "Test BSH 3", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4 = testObjectCreator.CreateAccGLHeader("5300.03.98", "AS", "Test BSH 4", "BSH", Constants.DebitCredit.Credit);

			testObjectCreator.CreateAccountDescriptor(glHeader3, "5300.097", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(glHeader4, "5300.098", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);

			testObjectCreator.CreateAccGLAggregate(3000m, 201012, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-3000m, 201012, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(1100m, 201001, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1100m, 201001, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(100m, 200912, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(200m, 200912, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(300m, 200912, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-300m, 200912, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();

			DataTable results = RunScript(200910, 200910);

			AssertEquals("Should not return row", 0, results.Rows.Count);

			results = RunScript(200910, 200911);
			DataRow row = results.Rows.Find("5300");
			AssertNotNull("Should find row", row);
			AssertEquals("CurrentDebit", 0m, row["CurrentDebit"]);
			AssertEquals("CurrentCredit", 301m, row["CurrentCredit"]);
			AssertEquals("MovementDebit", 0m, row["MovementDebit"]);
			AssertEquals("MovementCredit", 301m, row["MovementCredit"]);
			AssertEquals("LastDebit", 0m, row["LastDebit"]);
			AssertEquals("LastCredit", 0m, row["LastCredit"]);

			row = results.Rows.Find("6100");
			AssertNotNull("Should find row", row);
			AssertEquals("CurrentDebit", 603m, row["CurrentDebit"]);
			AssertEquals("CurrentCredit", 0m, row["CurrentCredit"]);
			AssertEquals("MovementDebit", 603m, row["MovementDebit"]);
			AssertEquals("MovementCredit", 0m, row["MovementCredit"]);
			AssertEquals("LastDebit", 0m, row["LastDebit"]);
			AssertEquals("LastCredit", 0m, row["LastCredit"]);

			results = RunScript(200901, 200912);
			row = results.Rows.Find("5300");
			AssertNotNull("Should find row", row);
			AssertEquals("CurrentDebit", 300m, row["CurrentDebit"]);
			AssertEquals("CurrentCredit", 301m, row["CurrentCredit"]);
			AssertEquals("MovementDebit", 0m, row["MovementDebit"]);
			AssertEquals("MovementCredit", 1m, row["MovementCredit"]);
			AssertEquals("LastDebit", 0m, row["LastDebit"]);
			AssertEquals("LastCredit", 0m, row["LastCredit"]);

			row = results.Rows.Find("6100");
			AssertNotNull("Should find row", row);
			AssertEquals("CurrentDebit", 603m, row["CurrentDebit"]);
			AssertEquals("CurrentCredit", 0m, row["CurrentCredit"]);
			AssertEquals("MovementDebit", 603m, row["MovementDebit"]);
			AssertEquals("MovementCredit", 0m, row["MovementCredit"]);
			AssertEquals("LastDebit", 0m, row["LastDebit"]);
			AssertEquals("LastCredit", 0m, row["LastCredit"]);

			results = RunScript(201001, 201001);
			row = results.Rows.Find("5300");
			AssertNotNull("Should find row", row);
			AssertEquals("CurrentDebit", 300m, row["CurrentDebit"]);
			AssertEquals("CurrentCredit", 1101m, row["CurrentCredit"]);
			AssertEquals("MovementDebit", 0m, row["MovementDebit"]);
			AssertEquals("MovementCredit", 1101m, row["MovementCredit"]);
			AssertEquals("LastDebit", 300m, row["LastDebit"]);
			AssertEquals("LastCredit", 0m, row["LastCredit"]);

			row = results.Rows.Find("6100");
			AssertNotNull("Should find row", row);
			AssertEquals("CurrentDebit", 1101m, row["CurrentDebit"]);
			AssertEquals("CurrentCredit", 0m, row["CurrentCredit"]);
			AssertEquals("MovementDebit", 1101m, row["MovementDebit"]);
			AssertEquals("MovementCredit", 0m, row["MovementCredit"]);
			AssertEquals("LastDebit", 0m, row["LastDebit"]);
			AssertEquals("LastCredit", 0m, row["LastCredit"]);

			results = RunScript(201001, 201012);
			row = results.Rows.Find("5300");
			AssertNotNull("Should find row", row);
			AssertEquals("CurrentDebit", 3300m, row["CurrentDebit"]);
			AssertEquals("CurrentCredit", 4102m, row["CurrentCredit"]);
			AssertEquals("MovementDebit", 0m, row["MovementDebit"]);
			AssertEquals("MovementCredit", 1102m, row["MovementCredit"]);
			AssertEquals("LastDebit", 300m, row["LastDebit"]);
			AssertEquals("LastCredit", 0m, row["LastCredit"]);

			row = results.Rows.Find("6100");
			AssertNotNull("Should find row", row);
			AssertEquals("CurrentDebit", 7104m, row["CurrentDebit"]);
			AssertEquals("CurrentCredit", 0m, row["CurrentCredit"]);
			AssertEquals("MovementDebit", 7104m, row["MovementDebit"]);
			AssertEquals("MovementCredit", 0m, row["MovementCredit"]);
			AssertEquals("LastDebit", 0m, row["LastDebit"]);
			AssertEquals("LastCredit", 0m, row["LastCredit"]);
		}

		DataTable RunScript(int period, int period1)
		{
			DataTable table = DataUtils.GetDataTableFromQuery(Db.Connection,
									string.Format(@"EXEC TrialBalanceCN {0},{1},'{2}','','{3}','','ZH-CN', 'CN', 'N'",
													period,
													period1,
													GlbCompany.CurrentCompany.PK,
													GlbBranch.CurrentBranch.GB_Code
													)
								);

			table.PrimaryKey = new[] { table.Columns["AccountNumber"] };

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

