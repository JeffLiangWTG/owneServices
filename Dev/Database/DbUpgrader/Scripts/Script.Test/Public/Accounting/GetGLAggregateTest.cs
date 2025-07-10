using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetAccTaxRate))]
	class GetGLAggregateTest : DbCreateScriptTest
	{
		public void TestGetGLAggregate()
		{
			PrepareTestData();
			var sql = string.Format(@"SELECT * FROM dbo.GetGLAggregate
														(
														'{0}',															  --@Company uniqueidentifier,
														@BranchList,													  --dbo.TVP_uniqueidentifier readonly,
														@BranchListIsEmpty,												  --bit,
														@DepartmentList,												  --dbo.TVP_uniqueidentifier readonly,
														@DepartmentListIsEmpty											  --bit,
														)", companyPK);

			var command = Db.Connection.Command(sql);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@BranchList", new[] { branch1 });
			AddTVP_uniqueidentifierAndIsEmptyParameters(command, "@DepartmentList", new[] { department2 });
			var result = DataUtils.GetDataTableFromCommand(command);

			AssertEquals(2, result.Rows.Count);

			var rows = result.Rows.Cast<DataRow>().ToList();
			
			var account9Row = rows.First(row => (Guid)row["GLAccountPK"] == account9);
			var account13Row = rows.First(row => (Guid)row["GLAccountPK"] == account13);

			AssertEquals(302m, (decimal)account9Row["Amount"]);
			AssertEquals(602m, (decimal)account13Row["Amount"]);

			var command1 = Db.Connection.Command(sql);
			AddTVP_uniqueidentifierAndIsEmptyParameters(command1, "@BranchList", Array.Empty<Guid>());
			AddTVP_uniqueidentifierAndIsEmptyParameters(command1, "@DepartmentList", Array.Empty<Guid>());
			var resultWithoutBranchAndDept = DataUtils.GetDataTableFromCommand(command1);
			AssertEquals("Should not include account3 in 202302 GLAggregate And account4 in 202301 GLD", 12, resultWithoutBranchAndDept.Rows.Count);
		}

		void PrepareTestData()
		{
			helper.InsertAccPeriod(2023, 1, companyPK);
			helper.InsertAccPeriod(2023, 2, companyPK);
			helper.InsertAccPeriod(2023, 3, companyPK);
			helper.SetRegistryGLDLastProcessedDate(companyPK, new DateTime(2023, 2, 1));

			helper.InsertGLAggregate(100, "", 201808, account1, companyPK: companyPK);
			helper.InsertGLAggregate(200, "", 202301, account2, companyPK: companyPK);
			helper.InsertGLAggregate(200, "", 202301, account2, companyPK: companyPK);
			helper.InsertGLAggregate(300, "", 202302, account3, companyPK: companyPK);

			helper.InsertGeneralLedgerData(202301, account4, new DateTime(2023, 1, 1), 400, companyPK: companyPK);
			helper.InsertGeneralLedgerData(202302, account5, new DateTime(2023, 2, 1), 500, companyPK: companyPK);
			helper.InsertGeneralLedgerData(202303, account6, new DateTime(2023, 3, 1), 600, companyPK: companyPK);

			helper.InsertGLAggregate(300, "", 201808, account7, companyPK: companyPK, branchPK: branch1, departmentPK: department1);
			helper.InsertGLAggregate(301, "", 201808, account8, companyPK: companyPK, branchPK: branch2, departmentPK: department2);
			helper.InsertGLAggregate(302, "", 201808, account9, companyPK: companyPK, branchPK: branch1, departmentPK: department2);
			helper.InsertGLAggregate(303, "", 201808, account10, companyPK: companyPK, branchPK: branch2, departmentPK: department1);

			helper.InsertGeneralLedgerData(202303, account11, new DateTime(2023, 3, 1), 600, companyPK: companyPK, branchPK: branch1, departmentPK: department1);
			helper.InsertGeneralLedgerData(202303, account12, new DateTime(2023, 3, 1), 601, companyPK: companyPK, branchPK: branch2, departmentPK: department2);
			helper.InsertGeneralLedgerData(202303, account13, new DateTime(2023, 3, 1), 602, companyPK: companyPK, branchPK: branch1, departmentPK: department2);
			helper.InsertGeneralLedgerData(202303, account14, new DateTime(2023, 3, 1), 603, companyPK: companyPK, branchPK: branch2, departmentPK: department1);
		}

		void AddTVP_uniqueidentifierAndIsEmptyParameters(DbCommand command, string paramName, Guid[] values)
		{
			var table = new DataTable();
			table.Columns.Add("Value", typeof(Guid));

			if (values != null)
			{
				foreach (var value in values)
				{
					table.Rows.Add(value);
				}
			}

			command.AddTableValuedParameter(paramName, "dbo.TVP_uniqueidentifier", table);
			command.AddParameter(paramName + "IsEmpty", SqlDbType.Bit, table.Rows.Count == 0);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new TestDbHelper(TestConnection);
			companyPK = helper.InsertCompany("TST", "DAU", "AUD", "AS", false, false);
			branch1 = helper.InsertBranch("SYN", companyPK);
			branch2 = helper.InsertBranch("JRO", companyPK);
			department1 = helper.InsertDepartment("AU1");
			department2 = helper.InsertDepartment("AU2");

			account1 = helper.InsertGLAccount("TestAcc1", "test1");
			account2 = helper.InsertGLAccount("TestAcc2", "test2");
			account3 = helper.InsertGLAccount("TestAcc3", "test3");
			account4 = helper.InsertGLAccount("TestAcc4", "test4");
			account5 = helper.InsertGLAccount("TestAcc5", "test5");
			account6 = helper.InsertGLAccount("TestAcc6", "test6");
			account7 = helper.InsertGLAccount("TestAcc7", "test7");
			account8 = helper.InsertGLAccount("TestAcc8", "test8");
			account9 = helper.InsertGLAccount("TestAcc9", "test9");
			account10 = helper.InsertGLAccount("TestAcc10", "test10");
			account11 = helper.InsertGLAccount("TestAcc11", "test11");
			account12 = helper.InsertGLAccount("TestAcc12", "test12");
			account13 = helper.InsertGLAccount("TestAcc13", "test13");
			account14 = helper.InsertGLAccount("TestAcc14", "test14");
		}

		TestDbHelper helper;

		Guid companyPK;
		Guid branch1;
		Guid branch2;
		Guid department1;
		Guid department2;

		Guid account1;
		Guid account2;
		Guid account3;
		Guid account4;
		Guid account5;
		Guid account6;
		Guid account7;
		Guid account8;
		Guid account9;
		Guid account10;
		Guid account11;
		Guid account12;
		Guid account13;
		Guid account14;
	}
}

