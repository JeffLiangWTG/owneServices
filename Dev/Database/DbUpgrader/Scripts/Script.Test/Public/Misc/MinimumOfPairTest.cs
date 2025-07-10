using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc.Test
{
	[TestedType(typeof(MinimumOfPair))]
	class GetMinimumValueTest : DbCreateScriptTest
	{
		public void TestMinimumOfPair()
		{
			AssertEquals(2m, MinimumOfPair(2, 100));
			AssertEquals(-2m, MinimumOfPair(-2, 100));
			AssertEquals(2m, MinimumOfPair(2, 2));
			AssertEquals(-100000m, MinimumOfPair(10000, -100000));
		}

		decimal MinimumOfPair(decimal val1, decimal val2)
		{
			decimal result;
			string sqlText = string.Format("SELECT * FROM dbo.{0}(@Value1, @Value2)", ScriptToTest.Name);

			using (DbCommand command = TestConnection.Command(sqlText))
			{
				command.AddParameter("@Value1", SqlDbType.Decimal, val1);
				command.AddParameter("@Value2", SqlDbType.Decimal, val2);
				result = (decimal)command.ExecuteScalar();
			}

			return result;
		}
	}
}
