using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(vw_GetEnableTriggerScripts))]
	class vw_GetEnableTriggerScriptsTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			string sqlText = "SELECT count(*) FROM " + ScriptToTest.Name;
			int rowCount = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			Assert("Row count too small", rowCount > 0);

			sqlText = "SELECT Command FROM " + ScriptToTest.Name + " WHERE Command like '%[[]TG_CP_INS_StmALogQueue]'";
			string command = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("TG_CP_INS_StmALogQueue trigger", "ALTER TABLE [dbo].[StmALog] ENABLE TRIGGER [TG_CP_INS_StmALogQueue]", command);
		}
	}
}

