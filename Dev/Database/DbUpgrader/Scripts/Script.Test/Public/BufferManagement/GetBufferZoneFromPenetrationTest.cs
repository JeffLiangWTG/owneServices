using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(GetBufferZoneFromPenetration))]
	sealed class GetBufferZoneFromPenetrationTest : DbCreateScriptTest
	{
		public void TestZoneFromBufferPenetration()
		{
			AssertZoneFromPenetration(0, 10);
			AssertZoneFromPenetration(3, 0);
			AssertZoneFromPenetration(3, 0.32m);
			AssertZoneFromPenetration(2, 0.34m);
			AssertZoneFromPenetration(2, 0.64m);
			AssertZoneFromPenetration(1, 0.67m);
			AssertZoneFromPenetration(1, 0.99m);
			AssertZoneFromPenetration(0, 1.0m);
			AssertZoneFromPenetration(0, 1.1m);
		}

		void AssertZoneFromPenetration(int expectedZone, decimal penetrationPercentProvided)
		{
			var sql = string.Format(@"SELECT a.Zone FROM dbo.GetBufferZoneFromPenetration({0}) a", penetrationPercentProvided);

			var result = TestConnection.ExecuteScalar(sql);

			AssertEquals(expectedZone, result);
		}
	}
}
