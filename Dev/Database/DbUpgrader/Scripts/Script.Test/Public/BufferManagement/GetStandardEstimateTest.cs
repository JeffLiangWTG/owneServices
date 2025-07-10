using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetStandardEstimate))]
	sealed class GetStandardEstimateTest : DbCreateScriptTest
	{
		public void TestReturnsCorrectValue()
		{
			var sql = "SELECT Value FROM dbo.GetStandardEstimate(10.0, 2)";

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					AssertEquals(reader.FieldCount, 1);
					var result = reader[0];
					AssertEquals(result, 15d);
				}
			}

			sql = "SELECT Value FROM dbo.GetStandardEstimate(-10.0, 2)";

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					AssertEquals(reader.FieldCount, 1);
					var result = reader[0];
					AssertEquals(result, -15d);
				}
			}

			sql = "SELECT Value FROM dbo.GetStandardEstimate(10.5, 3)";

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					AssertEquals(reader.FieldCount, 1);
					var result = reader[0];
					AssertEquals(result, 21d);
				}
			}
		}
	}
}

