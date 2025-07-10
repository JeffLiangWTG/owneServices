using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(vw_Report_Contact))]
	class vw_Report_ContactTest : DbCreateScriptTest
	{
		public void TestContactViewReturnsExpectedResults()
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Top 1 * FROM dbo.vw_Report_Contact");

			AssertEquals(25, result.Columns.Count);
			AssertEquals("Phone", result.Columns[7].ToString());
		}
	}
}

