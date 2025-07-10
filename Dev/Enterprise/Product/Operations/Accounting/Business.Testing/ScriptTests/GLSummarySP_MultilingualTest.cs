


using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GLSummarySP_MultilingualTest : ScriptTest
	{
		[TestDate(2000, 06, 20, 15, 39, 42)]
		public void TestLocalGLSummarySP()
		{
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			ZDateTime dueDate = new ZDateTime(2000, 6, 1);
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2000, 1, 1));

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader glHeader0PNL = testObjectCreator.CreateAccGLHeader("1100.90.00", "TS", "Test PNL 0", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.03.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.03.96", "TS", "Test PNL 2", "P&L", Constants.DebitCredit.Debit);

			testObjectCreator.CreateAccountDescriptor(glHeader0PNL, "6100.000", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc PNL0", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "6100.095", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc PNL1", "CN", Constants.DebitCredit.Debit);
			AccGLAccountDescriptor testAccGLAccountDescriptor1 = testObjectCreator.CreateAccountDescriptor(glHeader2PNL, "6100.096", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc PNL2", "CN", Constants.DebitCredit.Debit);

			AccGLHeader glHeader0 = testObjectCreator.CreateAccGLHeader("2200.90.00", "AS", "Test BSH 0", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("2200.03.95", "AS", "Test BSH 1", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("2200.03.96", "AS", "Test BSH 2", "BSH", Constants.DebitCredit.Debit);

			AccGLAccountDescriptor testAccGLAccountDescriptor0 = testObjectCreator.CreateAccountDescriptor(glHeader0, "5300.000", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc BSH0", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(glHeader1, "5300.095", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc BSH1", "CN", Constants.DebitCredit.Debit);

			OrgHeader fInActiveOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			OrgHeader fInActiveOrg1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			TestObjectCreator.InsertTransaction("JNL", "AR", dueDate, true, fInActiveOrg.PK, glHeader1PNL.PK, TestObjectCreator.USD.RX_Code, 1.3m, 39m, 30m);
			TestObjectCreator.InsertTransaction("JNL", "AP", dueDate, true, fInActiveOrg.PK, glHeader2PNL.PK, TestObjectCreator.USD.RX_Code, 1.4m, -56m, -40m);

			TestObjectCreator.InsertTransaction("JNL", "AR", dueDate, true, fInActiveOrg1.PK, glHeader1.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 1m, 0m, 50m);
			TestObjectCreator.InsertTransaction("JNL", "AP", dueDate, true, fInActiveOrg1.PK, glHeader2.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 1m, 0m, 60m);

			testObjectCreator.CreateAccGLAggregate(30m, 200006, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-30m, 200006, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(30m, 200006, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-30m, 200006, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();
			DataTable results = RunScript(testAccGLAccountDescriptor0.PK, testAccGLAccountDescriptor1.PK);
			AssertEquals("Missing GL Mapping, Count of invoice should be return 0", 0, results.Rows.Count);

			testObjectCreator.CreateAccountDescriptor(glHeader2, "5300.096", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "Local Desc BSH2", "CN", Constants.DebitCredit.Debit);

			Factory.Save();

			results = RunScript(testAccGLAccountDescriptor0.PK, testAccGLAccountDescriptor1.PK);
			AssertEquals("Count of invoice should be 2", 2, results.Rows.Count);

			AssertEquals("LocalGLAccount should be : 5300", "5300", results.Rows[0]["AccountNumber"].ToString());
			AssertEquals("LocalGLAccount should be : 6100", "6100", results.Rows[1]["AccountNumber"].ToString());

			AssertEquals("OpeningBalance should be : 0", 0m, results.Rows[0]["OpeningBalance"]);
			AssertEquals("OpeningBalance should be : 0", 0m, results.Rows[1]["OpeningBalance"]);

			AssertEquals("PeriodDebit should be : 0", 0m, results.Rows[0]["PeriodDebit"]);
			AssertEquals("PeriodDebit should be : 40", 40m, results.Rows[1]["PeriodDebit"]);

			AssertEquals("PeriodCredit should be : 110", 110m, results.Rows[0]["PeriodCredit"]);
			AssertEquals("PeriodCredit should be : 30", 30m, results.Rows[1]["PeriodCredit"]);

			AssertEquals("YTDDebit should be : 0m", 0m, results.Rows[0]["YTDDebit"]);
			AssertEquals("YTDDebit should be : 40m", 40m, results.Rows[1]["YTDDebit"]);

			AssertEquals("YTDCredit should be : 110", 110m, results.Rows[0]["YTDCredit"]);
			AssertEquals("YTDCredit should be : 30", 30m, results.Rows[1]["YTDCredit"]);

			AssertEquals("CurrentBalance should be -110", -110m, results.Rows[0]["CurrentBalance"]);
			AssertEquals("CurrentBalance should be 10", 10m, results.Rows[1]["CurrentBalance"]);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		DataTable RunScript(ZGuid startLocalAccount, ZGuid endLocalAccount)
		{
			ZString sql = string.Format(@"
							EXEC GLSummarySP_Multilingual
							'{0}',	--@Company
							'{1}',	--@StartPeriod
							'{2}',	--@EndPeriod
							'{3}',	--@StartGLAccountPK
							'{4}',	--@EndGLAccountPK
							'ZH-CN',	--@Language
							'CN'",
						GlbCompany.CurrentCompany.PK,
						200001,
						200012,
						startLocalAccount,
						endLocalAccount);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}

