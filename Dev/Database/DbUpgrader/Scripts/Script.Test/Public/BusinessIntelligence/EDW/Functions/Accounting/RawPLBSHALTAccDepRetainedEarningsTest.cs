using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(RawPLBSHALTAccDepRetainedEarnings))]
	class RawPLBSHALTAccDepRetainedEarningsTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestRetainedEarnings()
		{
			Helper.InsertPeriodForInputYear(2009);
			Helper.InsertStmData(new DateTime(2009, 01, 01));
			Helper.InsertGLAccount(1, "4900.00.00", "BSH");
			Helper.InsertGLAccount(2, "2010.00.00");
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertBASAccount(1);
			Helper.InsertGLAggregate(100, 201008, 1);
			Helper.InsertGLAggregate(200, 201009, 1);
			Helper.InsertGLAggregate(300, 200910, 1);
			Helper.InsertGLAggregate(400, 201008, 2);
			Helper.InsertGLAggregate(500, 201009, 2);
			Helper.InsertGLAggregate(600, 200910, 2);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM {ScriptDbName}.dbo.RawPLBSHALTAccDepRetainedEarnings (201009, 201009, 201009, 1, '', '')");
			AssertEquals("Row count == 1", 1, result.Rows.Count);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("singe category", 200m, result.Rows[0]["Amount"]);

			DataTable result1 = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM {ScriptDbName}.dbo.RawPLBSHALTAccDepRetainedEarnings (0, 201000, 201009, 1, '', '')");
			AssertEquals("Row count == 1", 1, result1.Rows.Count);
			AssertEquals("Result should have one row", 1, result1.Rows.Count);
			AssertEquals("singe category", 1200m, result1.Rows[0]["Amount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

