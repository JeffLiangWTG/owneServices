using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(csfn_GetExceptionCodeDescriptionInline))]
	class csfn_GetExceptionCodeDescriptionInlineTest : DbCreateScriptTest
	{
		public void TestExceptionCodeDescriptionList()
		{
			AssertCodeDescription("010", "B3 lodged and accepted but not reported on DN");
			AssertCodeDescription("020", "B3 sent but no response");
			AssertCodeDescription("030", "Released but not B3 confirmed on due date");
			AssertCodeDescription("040", "ACROSS/IID sent but no response");
			AssertCodeDescription("050", "No timely release");
		}

		void AssertCodeDescription(string code, string description)
		{
			var sql = string.Format("select ExceptionDescription from dbo.csfn_GetExceptionCodeDescriptionInline('{0}')", code);
			var modesDataTable = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(1, modesDataTable.Rows.Count);
			AssertEquals(description, (string)modesDataTable.Rows[0]["ExceptionDescription"]);
		}
	}
}
