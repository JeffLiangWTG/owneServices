using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(AppendDateBetweenToWhere))]
	class AppendDateBetweenToWhereTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var result = DataUtils.GetDataTableFromQuery(TestConnection, "select RESULT = dbo.AppendDateBetweenToWhere ( 'AND', '_Field_Name_', '2016-10-19 00:00:00', '2016-10-20 00:00:00')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("Dates should be passed without any alterations using < comparison for @ToDateNonInclusive", " AND _Field_Name_ >= 'Oct 19 2016 12:00AM' AND _Field_Name_ < 'Oct 20 2016 12:00AM'", result.Rows[0][0].ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select RESULT = dbo.AppendDateBetweenToWhere ( 'OR', '_Field_Name_', '2016-10-19 08:30:00', '2016-10-20 08:30:00')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("Dates with time should be passed without any alterations using < comparison for @ToDateNonInclusive", " OR _Field_Name_ >= 'Oct 19 2016  8:30AM' AND _Field_Name_ < 'Oct 20 2016  8:30AM'", result.Rows[0][0].ToString());
		}
	}
}

