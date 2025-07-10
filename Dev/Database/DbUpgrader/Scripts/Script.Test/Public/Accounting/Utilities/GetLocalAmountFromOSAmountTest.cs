using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(GetLocalAmountFromOSAmount))]
	class GetLocalAmountFromOSAmountTest : DbCreateScriptTest
	{
		public void TestCalculation()
		{
			using (var cmd = Db.Connection.Command("SELECT LocalAmount FROM GetLocalAmountFromOSAmount(1, 0, 0.6, NULL, 100, NULL)"))
			{
				AssertEquals("Reciprocal = N. OS amount (1) divided by exchange rate (0.6), then rounded to 2 decimal places (sub unit ratio = 100)", 1.67m, cmd.ExecuteScalar());
			}

			using (var cmd = Db.Connection.Command("SELECT LocalAmount FROM GetLocalAmountFromOSAmount(1, 1, 0.6123, NULL, 100, NULL)"))
			{
				AssertEquals("Reciprocal = Y. OS amount (1) divided by exchange rate (0.6123), then rounded to 2 decimal places (sub unit ratio = 100)", 0.61M, cmd.ExecuteScalar());
			}
		}
	}
}

