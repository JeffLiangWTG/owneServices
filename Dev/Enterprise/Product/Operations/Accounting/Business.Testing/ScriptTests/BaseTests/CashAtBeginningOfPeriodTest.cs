using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests
{
	[UseSnapshotProtection(new[] { DatabaseType.Main })]
	class CashAtBeginningOfPeriodTest : ScriptTest
	{
		public void TestCashAtBeginningOfPeriod()
		{
			using (Connection = Db.NewAdminConnection())
			{
				Prepare_CashAtBeginningOfPeriodTest();

				var result = Execute(CompanyPkStr, 201909, BranchPkStr);
				AssertEquals(1000m, result.Rows[0]["Value"]);

				result = Execute(CompanyPkStr, 201909, BranchPkStr, 'Y');
				AssertEquals(0m, result.Rows[0]["Value"]);
			}
		}

		string CompanyPkStr { get; set; }
		string BranchPkStr { get; set; }
		protected AdminConnection Connection;
		protected virtual string ScriptDbName => Db.DatabaseName;

		protected TestDbHelper Helper => helper ?? (helper = new TestDbHelper(Connection));

		TestDbHelper helper;

		protected virtual void Prepare_CashAtBeginningOfPeriodTest()
		{
			PrepareData();
		}

		void PrepareData()
		{
			Helper.InsertGLAccount("4444.22.22", "TestGLAccount 1", "P&L");
			var glAccountPK1 = Helper.InsertGLAccount("3333.22.22", "TestGLAccount 1", "P&L", cashFlowType: "CSH");
			Helper.InsertGLAccount("4900.22.22", "TestGLAccount 1", "P&L");

			var plAppropriationAccountPK = new Guid(Connection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			var desc1 = TestObjectCreator.CreateAccountDesriptorLight("4444.22.22", "COA", "ZH-CN", "CN");
			var desc2 = TestObjectCreator.CreateAccountDesriptorLight("4900.22.22", "COA", "ZH-CN", "CN");
			TestObjectCreator.CreateGLDescriptorPivotLight(desc1, Factory.Load<AccGLHeader>(glAccountPK1));
			TestObjectCreator.CreateGLDescriptorPivotLight(desc2, Factory.Load<AccGLHeader>(plAppropriationAccountPK));
			var companyPK = Helper.InsertCompany("ABC", "ABC Compay", "CNY", "CN", true, true);
			CompanyPkStr = companyPK.ToString();
			var branch2PK = Helper.InsertBranch("BR2", companyPK, "Branch 2");
			BranchPkStr = branch2PK.ToString();
			var departmentPK = Helper.InsertDepartment("DP2", "Department 2");

			Helper.InsertAccPeriod(2019, 05, companyPK);
			Helper.InsertAccPeriod(2019, 06, companyPK);
			Helper.InsertAccPeriod(2019, 07, companyPK, new DateTime(2020, 01, 01));
			Helper.InsertAccPeriod(2019, 08, companyPK, new DateTime(2020, 02, 01));
			Helper.InsertAccPeriod(2019, 09, companyPK, new DateTime(2020, 03, 01));
			Helper.InsertAccPeriod(2019, 10, companyPK, new DateTime(2020, 04, 01));

			Helper.InsertGLAggregate(66, "", 201907, plAppropriationAccountPK, branch2PK, departmentPK, companyPK);
			Helper.InsertGLAggregate(100, "", 201905, glAccountPK1, branch2PK, departmentPK, companyPK);
			Helper.InsertGLAggregate(200, "", 201906, glAccountPK1, branch2PK, departmentPK, companyPK);
			Helper.InsertGLAggregate(300, "", 201907, glAccountPK1, branch2PK, departmentPK, companyPK);
			Helper.InsertGLAggregate(400, "", 201908, glAccountPK1, branch2PK, departmentPK, companyPK);
			Helper.InsertGLAggregate(500, "", 201909, glAccountPK1, branch2PK, departmentPK, companyPK);
			Helper.InsertGLAggregate(600, "", 201910, glAccountPK1, branch2PK, departmentPK, companyPK);

			Connection.ExecuteNonQuery($@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue) VALUES(NEWID(), 'JournalEntriesLastProcessedDate', '{CompanyPkStr}', NULL, 'DT', convert(varbinary(8000), N'2019-06-01 00:00:00.000'), NULL)");
		}

		protected DataTable Execute(string companyPK, int period, string branchID, char isYearToDate = 'N')
		{
			return DataUtils.GetDataTableFromQuery(Connection, string.Format($"SELECT * FROM [{ScriptDbName}].[dbo].CashAtBeginningOfPeriod({period}, '{companyPK}', '{branchID}', '{isYearToDate}')"));
		}
	}
}

