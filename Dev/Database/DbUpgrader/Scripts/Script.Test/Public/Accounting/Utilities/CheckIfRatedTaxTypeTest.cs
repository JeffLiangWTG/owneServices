using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(CheckIfRatedTaxType))]
	class CheckIfRatedTaxTypeTest : DbCreateScriptTest
	{
		public void TestCheckIfRatedTaxType()
		{
			var result = RunScript("CAP", "AU");
			AssertEquals("CAP - Not rated", 0, result.Rows[0]["IsRated"]);

			result = RunScript("RAT", "AU");
			AssertEquals("RAT - Is rated", 1, result.Rows[0]["IsRated"]);

			result = RunScript("SER", "AU");
			AssertEquals("SER - AU - Not rated", 0, result.Rows[0]["IsRated"]);

			result = RunScript("SER", "IN");
			AssertEquals("SER - AU - Is rated", 1, result.Rows[0]["IsRated"]);

			result = RunScript("CAPSER", "IN");
			AssertEquals("CAPSER - Not rated", 0, result.Rows[0]["IsRated"]);

			result = RunScript("INT", "IN");
			AssertEquals("INT - Is rated", 1, result.Rows[0]["IsRated"]);

			result = RunScript("EXT", "AU");
			AssertEquals("EXT - Not rated", 0, result.Rows[0]["IsRated"]);

			result = RunScript("INT", "AU");
			AssertEquals("INT - Is rated", 1, result.Rows[0]["IsRated"]);
		}

		DataTable RunScript(string taxRateType, string country)
		{
			var sql = string.Format(@"select * from CheckIfRatedTaxType('{0}', '{1}')", taxRateType, country);
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}
	}
}

