using System;
using System.Text;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCarrierCombined
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCarrierCombined.ZZRefCarrierCombined))]
	class ZZRefCarrierCombined_Test : DbCreateScriptTest
	{
		public void TestTransportModes()
		{
			TestConnection.ExecuteNonQuery($@"
				IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CA')
				INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES 
				(NEWID(), 'CA', 'Canada', NULL)
				
				IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
				INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES 
				(NEWID(), 'ZA', 'South Africa', NULL)");

			var carrier1PK = Guid.NewGuid();
			var carrier2PK = Guid.NewGuid();
			var carrier3PK = Guid.NewGuid();
			var carrier4PK = Guid.NewGuid();
			var carrier5PK = Guid.NewGuid();
			var carrier6PK = Guid.NewGuid();

			AddCarrierWithTransportMode(carrier1PK, "1234", "CA", 1, 1, 1, 1);
			AssertTransportModes(carrier1PK, true, true, true, true);
			AddCarrierWithTransportMode(carrier2PK, "1235", "CA", 1, 0, 1, 0);
			AssertTransportModes(carrier2PK, true, false, true, false);

			AddCarrierWithTransportModeAttribute(carrier3PK, "1236", "CA", new string[] { "AIR" });
			AssertTransportModes(carrier3PK, false, false, true, false);

			AddCarrierWithTransportModeAttribute(carrier4PK, "1237", "CA", new string[] { "AIR", "SEA", "RAI", "ROA" });
			AssertTransportModes(carrier4PK, true, true, true, true);

			AddCarrierWithTransportModeAttribute(carrier5PK, "1238", "ZA", new string[] { "AIR", "SEA", "RAI", "ROA" });
			AssertTransportModes(carrier5PK, false, false, true, true);

			AddCarrierWithTransportMode(carrier6PK, "1239", "ZA", 1, 1, 1, 1);
			AssertTransportModes(carrier6PK, true, true, true, true);
		}

		void AddCarrierWithTransportMode(Guid carrierPK, string code, string country, int isRail, int isRoad, int isAir, int isSea)
		{
			TestConnection.ExecuteNonQuery($@"INSERT INTO RefDatabase_RefCarrierCode (ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_ZZZ_NKDataGrouping, ZZ4_IsRail, ZZ4_IsRoad,  ZZ4_IsAir, ZZ4_IsSea) VALUES ('{carrierPK}', '{code}', '{code} Description', '{country}', {isRail}, {isRoad}, {isAir}, {isSea})");
		}

		void AddCarrierWithTransportModeAttribute(Guid carrierPK, string code, string country, string[] attributes)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append($@"INSERT INTO RefDatabase_RefCarrierCode (ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_ZZZ_NKDataGrouping) VALUES ('{carrierPK}', '{code}', '{code} Description', '{country}')");
			foreach (var attribute in attributes)
			{
				stringBuilder.Append($@"insert into RefDatabase_RefCarrierCodeAttribute (ZZG_PK ,ZZG_ZZ4_CarrierCode ,ZZG_Name ,ZZG_Value) values (newid(), '{carrierPK}', '{attribute}','{attribute}')");
			}
			TestConnection.ExecuteNonQuery(stringBuilder.ToString());
		}

		void AssertTransportModes(Guid carrierPK, bool isRail, bool isRoad, bool isAir, bool isSea)
		{
			using (var reader = TestConnection.Command($@"SELECT ZZ4_IsRail, ZZ4_IsRoad, ZZ4_IsAir, ZZ4_IsSea FROM dbo.ZZRefCarrierCombined WHERE ZZ4_PK='{carrierPK}'").ExecuteReader())
			{
				reader.Read();

				AssertEquals("ZZ4_IsRail", isRail, reader.GetBoolean(0));
				AssertEquals("ZZ4_IsRoad", isRoad, reader.GetBoolean(1));
				AssertEquals("ZZ4_IsAir", isAir, reader.GetBoolean(2));
				AssertEquals("ZZ4_IsSea", isSea, reader.GetBoolean(3));
			}
		}
	}
}

