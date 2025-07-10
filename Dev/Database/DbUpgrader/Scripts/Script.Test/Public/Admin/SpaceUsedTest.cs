using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(SpaceUsed))]
	class SpaceUsedTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var allTables = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC " + ScriptToTest.Name);
			AssertEquals("Result should have rows", true, allTables.Rows.Count > 0);
			var filteredTables = allTables.Select("TableName IN ('[dbo].[StmEvent]', 'StmEvent')");
			AssertEquals($"Filtered Table count(StmEvent) [(For debug, {ScriptToTest.Name} returns {allTables.Rows.Count} row and the first TableName is: {allTables.Rows[0][0]}]", 1, filteredTables.Length);
			var stmEventRowcount = Convert.ToInt32(filteredTables[0]["Rows"].ToString().Trim());
			AssertEquals("StmEvent should contain rows", true, stmEventRowcount > 0);
		}
	}
}

