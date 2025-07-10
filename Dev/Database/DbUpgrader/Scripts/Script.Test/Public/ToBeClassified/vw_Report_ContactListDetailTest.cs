using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(vw_Report_ContactListDetail))]
	class vw_Report_ContactListDetailTest : DbCreateScriptTest
	{
		public void TestContactListDetailViewReturnsExpectedResults()
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Top 1 * FROM dbo.vw_Report_ContactListDetail");

			AssertEquals(28, result.Columns.Count);
			AssertEquals("Phone", result.Columns[8].ToString());
			AssertEquals("Mobile", result.Columns[9].ToString());
			AssertEquals("HomePhone", result.Columns[10].ToString());
			AssertEquals("OtherPhone", result.Columns[11].ToString());
		}
	}
}

