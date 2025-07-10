using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ChinaVoucher;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ChinaVoucher
{
	[TestedType(typeof(RemoveTrailingZero))]
	class RemoveTrailingZeroTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var expectedResults = new[]
			{
				new { valueToTrim = "2340.000", expected = (object)"2340" },
				new { valueToTrim = "2340.020", expected = (object)"2340.020" },
				new { valueToTrim = "2340.XXX", expected = (object)"2340.XXX" },
			};

			using (var cmd = TestConnection.Command("SELECT [Value] FROM [dbo].[RemoveTrailingZero](@ValueToTrim)"))
			{
				cmd.AddParameter("@ValueToTrim", SqlDbType.VarChar, 50, "");

				foreach (var r in expectedResults)
				{
					cmd.SetParameterValue("@ValueToTrim", r.valueToTrim);
					AssertEquals($"(valueToTrim = {r.valueToTrim})", r.expected, cmd.ExecuteScalar());
				}
			}
		}
	}
}

