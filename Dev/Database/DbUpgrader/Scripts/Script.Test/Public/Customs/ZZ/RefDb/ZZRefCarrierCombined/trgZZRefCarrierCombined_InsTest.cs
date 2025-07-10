using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCarrierCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCarrierCombined
{
	[TestedType(typeof(trgZZRefCarrierCombined_Ins))]
	class trgZZRefCarrierCombined_Ins_Test : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var insert = @"INSERT INTO dbo.ZZRefCarrierCombined (ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_CountryOrGrouping, ZZ4_IsAir, ZZ4_IsRail, ZZ4_IsRoad, ZZ4_IsSea) VALUES ('e1d48fb9-c002-4329-98d8-fbdc7d0d068d', 'BOB', 'Bob the Builder', 'US', 1, 0, 0, 0)";
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insert));

			var select = @"SELECT ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_CountryOrGrouping, ZZ4_IsAir, ZZ4_IsRail, ZZ4_IsRoad, ZZ4_IsSea FROM dbo.ZZRefCarrierCombined";
			var cmd = TestConnection.Command(select);
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					AssertEquals(new Guid("e1d48fb9-c002-4329-98d8-fbdc7d0d068d"), reader["ZZ4_PK"]);
					AssertEquals("BOB", reader["ZZ4_Code"]);
					AssertEquals("Bob the Builder", reader["ZZ4_Description"]);
					AssertEquals("US", reader["ZZ4_CountryOrGrouping"]);
					AssertEquals(true, reader["ZZ4_IsAir"]);
					AssertEquals(false, reader["ZZ4_IsRail"]);
					AssertEquals(false, reader["ZZ4_IsRoad"]);
					AssertEquals(false, reader["ZZ4_IsSea"]);
				}
			}
		}
	}
}

