using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(RawPLBSHALTAccDep))]
	class RawPLBSHALTAccDepTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestRawPLBSHALTAccDepWhenNotHitCaseWhenCondition()
		{
			PrepareData();
			Helper.InsertGLAggregate(100, 201001, 1);
			Helper.InsertGLAggregate(200, 200910, 2);
			Helper.InsertGLAggregate(-300, 200910, 3);
			Helper.InsertGLAggregate(400, 200910, 4);
			Helper.InsertGLAggregate(500, 200910, 5);

			AssertAmount(100m, 0m, 0m);
		}

		public void TestRawPLBSHALTAccDepWhenHitCaseWhenCondition()
		{
			PrepareData();
			Helper.InsertGLAggregate(100, 201001, 1);
			Helper.InsertGLAggregate(200, 200910, 2);
			Helper.InsertGLAggregate(300, 200910, 3);
			Helper.InsertGLAggregate(-400, 200910, 4);
			Helper.InsertGLAggregate(500, 200910, 5);

			AssertAmount(100m, 300m, -400m);
		}

		void PrepareData()
		{
			Helper.InsertGLAccount(1, "1000.00.00");
			Helper.InsertGLAccount(2, "2000.00.00", "BSH");
			Helper.InsertGLAccount(3, "3010.00.00", "BSH");
			Helper.InsertGLAccount(4, "4010.00.00", alternateAccountKey: 5);
			Helper.InsertGLAccount(5, "5010.00.00");
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertBASAccount(2);
			Helper.InsertPeriodForInputYear(2009);
			Helper.InsertStmData(new DateTime(2009, 01, 01));
		}

		void AssertAmount(decimal amount1, decimal amount2, decimal amount3)
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM {ScriptDbName}.dbo.RawPLBSHALTAccDep (0, 201000, 201009, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '')");
			AssertEquals("Row count == 3", 3, result.Rows.Count);
			AssertEquals("The amount of row 1", amount1, result.Rows[0]["Amount"]);
			AssertEquals("The amount of row 2", amount2, result.Rows[1]["Amount"]);
			AssertEquals("The amount of row 3", amount3, result.Rows[2]["Amount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

