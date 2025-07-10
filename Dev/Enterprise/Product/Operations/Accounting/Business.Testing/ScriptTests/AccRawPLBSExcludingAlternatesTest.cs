


using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AccRawPLBSExcludingAlternatesTest : ScriptTest
	{
		[TestDate(2000, 06, 20, 15, 39, 42)]
		public void TestPLAppropriationAccount()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(1999, 1, 1));
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2000, 1, 1));

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.03.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.03.96", "TS", "Test PNL 2", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader3PNL = testObjectCreator.CreateAccGLHeader("3300.00.33", "TS", "Test PLApp", "P&L", Constants.DebitCredit.Debit);

			testObjectCreator.CreateAccGLAggregate(10m, 199906, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(20m, 199906, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(40m, 200006, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader3PNL.PK.ToGuid());

			Factory.Save();

			DataTable results = RunScript(glHeader3PNL.PK);
			AssertEquals("Count should be 2", 2, results.Rows.Count);

			AssertEquals("GLAccount should be : glHeader2PNL", glHeader2PNL.PK, results.Rows[0][AccGLHeader.Schema.PK]);
			AssertEquals("Aomunt should be : 40", 40m, results.Rows[0]["Amount"]);
			AssertEquals("GLAccount should be : glHeader3PNL", glHeader3PNL.PK, results.Rows[1][AccGLHeader.Schema.PK]);
			AssertEquals("Aomunt should be : 30", 30m, results.Rows[1]["Amount"]);
		}

		DataTable RunScript(ZGuid accGLPK)
		{
			ZString sql = string.Format(@"SELECT * FROM AccRawPLBSExcludingAlternates(0, 200001, 200012, '{0}') WHERE Amount<>0"
							, GlbCompany.CurrentCompany.PK);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}

