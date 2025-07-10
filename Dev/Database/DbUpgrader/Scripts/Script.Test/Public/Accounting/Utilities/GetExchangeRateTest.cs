using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(GetExchangeRate))]
	class GetExchangeRateTest : DbCreateScriptTest
	{
		public void TestCalculationOfNotReciprocalWithNoRounding()
		{
			using (var cmd = Db.Connection.Command("SELECT Rate FROM GetExchangeRate(1, 0.612345, 0, NULL)"))
			{
				AssertEquals("Non Reciprocal and unrounded exchange rate", 0.612345m, cmd.ExecuteScalar());
			}
		}

		public void TestCalculationOfReciprocalWithNoRounding()
		{
			using (var cmd = Db.Connection.Command("SELECT Rate FROM GetExchangeRate(1, 0.6, 1, NULL)"))
			{
				AssertEquals("Reciprocal and unrounded exchange rate", 1.66666666666666666666m, cmd.ExecuteScalar());
			}
		}

		public void TestCalculationOfNotReciprocalWithRounding()
		{
			using (var cmd = Db.Connection.Command("SELECT Rate FROM GetExchangeRate(1, 0.6123456789, 0, 9)"))
			{
				AssertEquals("Non Reciprocal and rounded exchange rate", 0.612345679m, cmd.ExecuteScalar());
			}
		}

		public void TestCalculationOfReciprocalWithRounding()
		{
			using (var cmd = Db.Connection.Command("SELECT Rate FROM GetExchangeRate(1, 0.6, 1, 9)"))
			{
				AssertEquals("Reciprocal and unrounded exchange rate", 1.666666667m, cmd.ExecuteScalar());
			}
		}
	}
}

