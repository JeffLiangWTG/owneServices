using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc.Test
{
	[TestedType(typeof(MaximumOfPair))]
	class GetMaximumValueTest : DbCreateScriptTest
	{
		public void TestMaximumOfPair()
		{
			AssertEquals(100m, MaximumOfPair(2, 100));
			AssertEquals(100m, MaximumOfPair(100, 2));
			AssertEquals(100m, MaximumOfPair(-2, 100));
			AssertEquals(2m, MaximumOfPair(2, 2));
			AssertEquals(-10000m, MaximumOfPair(-10000, -100000));
		}

		decimal MaximumOfPair(decimal val1, decimal val2)
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
