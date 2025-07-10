using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(vw_report_ContactBirthdayReport))]
	class vw_report_ContactBirthdayReportTest : DbCreateScriptTest
	{
		public void TestContactBirthdayViewReturnsExpectedResults()
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Top 1 * FROM dbo.vw_report_ContactBirthdayReport");

			AssertEquals(13, result.Columns.Count);
			AssertEquals("OrgActive", result.Columns[11].ToString());
			AssertEquals("Country", result.Columns[12].ToString());
		}
	}
}
