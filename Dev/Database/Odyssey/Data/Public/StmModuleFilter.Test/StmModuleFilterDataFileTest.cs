using System.IO;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class StmModuleFilterDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new StmModuleFilterDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestStmModuleFilterDataFile()
		{
			DataHelpers.ClearTable("TagRule");
			DataHelpers.ClearTable("StmModuleFilter");

			string insertSql = @"
				INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_FilterName, S9_IsSystem, S9_FilterData) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC3', '1', 'Test 000', 1, cast('blah' as varbinary(max)))
				INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_FilterName, S9_IsSystem, S9_FilterData) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC4', '2', 'Test 111', 0, cast('blah' as varbinary(max)))
				INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_FilterName, S9_IsSystem, S9_FilterData) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC5', '_CT', 'Test 222', 1, cast('blah' as varbinary(max)))
				INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_FilterName, S9_IsSystem, S9_FilterData) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC6', 'TrackingSomething', 'Test 333', 1, cast('blah' as varbinary(max)))
				INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_FilterName, S9_IsSystem, S9_FilterData) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC7', 'LinerAndAgencySomethings', 'Test 444', 1, cast('blah' as varbinary(max)))
				INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_FilterName, S9_IsSystem, S9_FilterData) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC8', 'WhsSomething', 'Test 555', 1, cast('blah' as varbinary(max)))
				INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_FilterName, S9_IsSystem, S9_FilterData) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC9', 'CommissionApprovalRequest', 'Test 666', 1, cast('blah' as varbinary(max)))
				INSERT dbo.StmModuleFilterUserData (S0_PK, S0_RelatedEntityTableCode, S0_RelatedEntityID, S0_S9) VALUES ('015981DD-9248-47a3-AE9F-BF135039A3F3', 'GS', '70EFA270-3F0F-479C-9AED-0009455622E2', '07C017B7-0817-4714-8285-3F3B165F9DC3')
				INSERT dbo.StmModuleFilterUserData (S0_PK, S0_RelatedEntityTableCode, S0_RelatedEntityID, S0_S9) VALUES ('86060C6E-1526-4948-AE8C-507020A9771B', 'GS', '70EFA270-3F0F-479C-9AED-0009455622E2', '07C017B7-0817-4714-8285-3F3B165F9DC4')
				INSERT dbo.StmModuleFilterUserData (S0_PK, S0_RelatedEntityTableCode, S0_RelatedEntityID, S0_S9) VALUES ('86FA7854-ADC0-44b9-8B9B-F53AC1073A16', 'GS', '70EFA270-3F0F-479C-9AED-0009455622E2', '07C017B7-0817-4714-8285-3F3B165F9DC5')
				INSERT dbo.StmModuleFilterUserData (S0_PK, S0_RelatedEntityTableCode, S0_RelatedEntityID, S0_S9) VALUES ('86FA7854-ADC0-44b9-8B9B-F53AC1073A17', 'GS', '70EFA270-3F0F-479C-9AED-0009455622E2', '07C017B7-0817-4714-8285-3F3B165F9DC7')
				INSERT dbo.StmModuleFilterUserData (S0_PK, S0_RelatedEntityTableCode, S0_RelatedEntityID, S0_S9) VALUES ('86FA7854-ADC0-44b9-8B9B-F53AC1073A18', 'GS', '70EFA270-3F0F-479C-9AED-0009455622E2', '07C017B7-0817-4714-8285-3F3B165F9DC8')
				INSERT dbo.StmModuleFilterUserData (S0_PK, S0_RelatedEntityTableCode, S0_RelatedEntityID, S0_S9) VALUES ('86FA7854-ADC0-44b9-8B9B-F53AC1073A19', 'GS', '70EFA270-3F0F-479C-9AED-0009455622E2', '07C017B7-0817-4714-8285-3F3B165F9DC9')
				";

			Db.Connection.ExecuteNonQuery(insertSql);

			StmModuleFilterDataFile file = new StmModuleFilterDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 2, data.Tables.Count);
			AssertEquals("Row count", 6, data.Tables["StmModuleFilter"].Rows.Count);
			AssertEquals("Row 0", "1", data.Tables["StmModuleFilter"].Rows[0]["S9_ModuleID"].ToString());
			AssertEquals("Row 1", "_CT", data.Tables["StmModuleFilter"].Rows[1]["S9_ModuleID"].ToString());
			AssertEquals("Row 2", "TrackingSomething", data.Tables["StmModuleFilter"].Rows[2]["S9_ModuleID"].ToString());
			AssertEquals("Row 3", "LinerAndAgencySomethings", data.Tables["StmModuleFilter"].Rows[3]["S9_ModuleID"].ToString());
			AssertEquals("Row 4", "WhsSomething", data.Tables["StmModuleFilter"].Rows[4]["S9_ModuleID"].ToString());
			AssertEquals("Row 5", "CommissionApprovalRequest", data.Tables["StmModuleFilter"].Rows[5]["S9_ModuleID"].ToString());

			AssertEquals("Row count", 4, data.Tables["StmModuleFilterUserData"].Rows.Count);
			AssertEquals("Row 0", "86FA7854-ADC0-44B9-8B9B-F53AC1073A16", data.Tables["StmModuleFilterUserData"].Rows[0]["S0_PK"].ToString().ToUpper());
			AssertEquals("Row 1", "86FA7854-ADC0-44B9-8B9B-F53AC1073A17", data.Tables["StmModuleFilterUserData"].Rows[1]["S0_PK"].ToString().ToUpper());
			AssertEquals("Row 2", "86FA7854-ADC0-44B9-8B9B-F53AC1073A18", data.Tables["StmModuleFilterUserData"].Rows[2]["S0_PK"].ToString().ToUpper());
			AssertEquals("Row 3", "86FA7854-ADC0-44B9-8B9B-F53AC1073A19", data.Tables["StmModuleFilterUserData"].Rows[3]["S0_PK"].ToString().ToUpper());
		}

		[ExpectNoExceptions()]
		public void TestParentlessUserDataIsRemoved()
		{
			DataHelpers.ClearTable("TagRule");
			DataHelpers.ClearTable("StmModuleFilter");
			DataHelpers.ClearTable("StmModuleFilterUserData");

			string insertSql = @"
				INSERT dbo.StmModuleFilter (S9_PK, S9_ModuleID, S9_FilterName, S9_IsSystem, S9_FilterData) VALUES ('F6BC5CE3-A2A7-4331-8EDF-0DDF8A6615C7', '1', 'Test 000', 1, cast('blah' as varbinary(max)))
				INSERT dbo.StmModuleFilterUserData (S0_PK, S0_RelatedEntityTableCode, S0_RelatedEntityID, S0_S9) VALUES ('E14EE184-AB20-4b80-AF2B-7E3B4279E5EA', 'GS', '74E5F761-388E-4414-84ED-113D48B07375', 'F6BC5CE3-A2A7-4331-8EDF-0DDF8A6615C7')
				";

			Db.Connection.ExecuteNonQuery(insertSql);

			StmModuleFilterUpgradeTask task = new StmModuleFilterUpgradeTask(new StmModuleFilterDataFile());
			task.Run();

			string sqlText = string.Format("SELECT count(*) FROM dbo.StmModuleFilter WHERE S9_PK = 'F6BC5CE3-A2A7-4331-8EDF-0DDF8A6615C7'");

			AssertEquals("Row count", 0, Db.Connection.ExecuteScalar(sqlText));

			sqlText = string.Format("SELECT count(*) FROM dbo.StmModuleFilterUserData WHERE S0_S9 = 'F6BC5CE3-A2A7-4331-8EDF-0DDF8A6615C7'");
			AssertEquals("Row count", 0, Db.Connection.ExecuteScalar(sqlText));
		}
	}
}
