using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.StmNums;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums
{
	[TestedType(typeof(fn_StmNumsCalculatePrefixAndType))]
	class fn_StmNumsCalculatePrefixAndTypeTest : DbCreateScriptTest
	{
		public void TestcsfnStmNumsCalculatePrefixAndType()
		{
			AssertTypePrefix("", "", "");
			AssertTypePrefix("OrgOwned_TRF_1234567", "TRF", "1234567");
			AssertTypePrefix("short", "", "");
			AssertTypePrefix("OrgOwned_ABC_1234567890123456789012_VERYLONG", "ABC", "1234567890123456789012_VERYLONG");
			AssertTypePrefix("OrgOwned_   _111", "", "111");
			AssertTypePrefix("OrgOwned_A", "A", "");
			AssertTypePrefix("OrgOwned_ABCD", "ABC", "");
			AssertTypePrefix("NotOwned_TRF_1234567", "", "");
			AssertTypePrefix("C#IT-    _111", "", "111");
			AssertTypePrefix("C#IT-CEN _111", "CEN", "111");
			AssertTypePrefix("C#IT-CEND_111", "CEND", "111");
			AssertTypePrefix("C#IT- CEN_111", " CEN", "111");
		}

		void AssertTypePrefix(string name, string expectedType, string expectedPrefix)
		{
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"select * from dbo.fn_StmNumsCalculatePrefixAndType('{name}')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("SN_Type", expectedType, (string)result.Rows[0]["SN_Type"]);
			AssertEquals("SN_Prefix", expectedPrefix, (string)result.Rows[0]["SN_Prefix"]);
		}
	}
}

