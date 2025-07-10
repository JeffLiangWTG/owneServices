using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(vw_FkReferences))]
	class vw_FkReferencesTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			string sqlText = "SELECT count(*) FROM " + ScriptToTest.Name;
			int allTableRefCount = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			Assert("Expected more than 100 FK references", allTableRefCount > 100);

			sqlText = "SELECT * FROM " + ScriptToTest.Name + " WHERE PkTable = 'StmALog'";
			DataTable stmalogRefs = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("StmALog reference count", 0, stmalogRefs.Rows.Count);

			sqlText = "SELECT * FROM " + ScriptToTest.Name + " WHERE PkTable = 'StmNums'";
			DataTable stmnumsRefs = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("StmNums reference count", 1, stmnumsRefs.Rows.Count);
			AssertEquals("Reference FK schema name", "dbo", stmnumsRefs.Rows[0]["FkSchema"].ToString());
			AssertEquals("Reference FK table name", "StmNumberCache", stmnumsRefs.Rows[0]["FkTable"].ToString());
			AssertEquals("Reference FK column name", "SG_SN", stmnumsRefs.Rows[0]["FkColumn"].ToString());
			AssertEquals("Reference FK name", "StmNumberCache_SG_SN_FK2_StmNums_CRR_120N", stmnumsRefs.Rows[0]["FkName"].ToString());
			AssertEquals("Reference FK column is nullable?", false, Convert.ToInt32(stmnumsRefs.Rows[0]["IsNullable"]) == 1);
			AssertEquals("Is self-reference?", false, Convert.ToInt32(stmnumsRefs.Rows[0]["IsSelfReference"]) == 1);
			AssertEquals("Is disabled?", false, Convert.ToInt32(stmnumsRefs.Rows[0]["IsDisabled"]) == 1);
		}
	}
}

