using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(vw_GetEnableFkScripts))]
	class vw_GetEnableFkScriptsTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			string sqlText = "SELECT count(*) FROM " + ScriptToTest.Name;
			int rowCount = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			Assert("Row count too small", rowCount > 100);

			sqlText = "SELECT TOP 1 Command FROM " + ScriptToTest.Name + " WHERE Command like '%[_]FK2[_]StmNums[_]%'";
			string command = TestConnection.ExecuteScalar(sqlText).ToString();
			AssertEquals("StmALog reference count", "ALTER TABLE [dbo].[StmNumberCache] WITH CHECK CHECK CONSTRAINT [StmNumberCache_SG_SN_FK2_StmNums_CRR_120N];", command);
		}
	}
}

