using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefCusMapCombined
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefCusMapCombined.ZZRefCusMapCombined))]
	class ZZRefCusMapCombined_Test : DbCreateScriptTest
	{
		[TestDate(2016, 09, 01)]
		public void TestCombinedData()
		{
			InsertRefCusMapType(TestConnection, "I", "INW", "Customs To CW1");
			InsertRefCusMapType(TestConnection, "O", "OUT", "CW1 To Customs");
			InsertRefCusMapType(TestConnection, "B", "BTH", "Both Direction");
			InsertRefCusMapType(TestConnection, "A", "ANY", "Any Combination");

			var today = DateTime.Today;
			var maxSmallDateTimeValue = new DateTime(2079, 6, 6, 23, 59, 29);
			var refCusMap1 = InsertRefCusMap(TestConnection, "ZA", "I", "1", "CLR", today, maxSmallDateTimeValue);
			var refCusMap2 = InsertRefCusMap(TestConnection, "ZA", "I", "2", "CLR", today, maxSmallDateTimeValue);
			var refCusMap3 = InsertRefCusMap(TestConnection, "ZA", "I", "3", "CLR", today, maxSmallDateTimeValue);
			var refCusMap4 = InsertRefCusMap(TestConnection, "ZA", "O", "YYY", "A", today, maxSmallDateTimeValue);
			var refCusMap5 = InsertRefCusMap(TestConnection, "ZA", "O", "YYY", "B", today, maxSmallDateTimeValue);
			var refCusMap6 = InsertRefCusMap(TestConnection, "ZA", "O", "YYY", "C", today, maxSmallDateTimeValue);
			var refCusMap7 = InsertRefCusMap(TestConnection, "ZA", "B", "E", "E", today, maxSmallDateTimeValue);
			var refCusMap8 = InsertRefCusMap(TestConnection, "ZA", "B", "R", "R", today, maxSmallDateTimeValue);
			var refCusMap9 = InsertRefCusMap(TestConnection, "ZA", "B", "E", "R", today, maxSmallDateTimeValue);
			var refCusMapA = InsertRefCusMap(TestConnection, "ZA", "B", "R", "E", today, maxSmallDateTimeValue);
			var refCusMapB = InsertRefCusMap(TestConnection, "ZA", "A", "X", "Y", today, maxSmallDateTimeValue);
			var refCusMapC = InsertRefCusMap(TestConnection, "ZA", "A", "Y", "Z", today, maxSmallDateTimeValue);

			var zzRefCusMap1 = InsertZZRefCusMap(TestConnection, "ZA", "I", "2", "XXX");
			var zzRefCusMap2 = InsertZZRefCusMap(TestConnection, "ZA", "I", "4", "CLR");
			var zzRefCusMap3 = InsertZZRefCusMap(TestConnection, "ZA", "O", "YYY", "D");
			var zzRefCusMap4 = InsertZZRefCusMap(TestConnection, "ZA", "O", "ZZZ", "A");
			var zzRefCusMap5 = InsertZZRefCusMap(TestConnection, "ZA", "B", "E", "E");
			var zzRefCusMap6 = InsertZZRefCusMap(TestConnection, "ZA", "A", "X", "Z");
			var zzRefCusMap7 = InsertZZRefCusMap(TestConnection, "ZA", "A", "Y", "A");
			var zzRefCusMap8 = InsertZZRefCusMap(TestConnection, "ZA", "A", "B", "Z");
			var zzRefCusMap9 = InsertZZRefCusMap(TestConnection, "ZA", "A", "Y", "Z");

			AssertZZRefCusMapCombined(TestConnection, refCusMap1, true);
			AssertZZRefCusMapCombined(TestConnection, refCusMap2, false);
			AssertZZRefCusMapCombined(TestConnection, refCusMap3, true);
			AssertZZRefCusMapCombined(TestConnection, refCusMap4, false);
			AssertZZRefCusMapCombined(TestConnection, refCusMap5, true);
			AssertZZRefCusMapCombined(TestConnection, refCusMap6, true);
			AssertZZRefCusMapCombined(TestConnection, refCusMap7, false);
			AssertZZRefCusMapCombined(TestConnection, refCusMap8, true);
			AssertZZRefCusMapCombined(TestConnection, refCusMap9, false);
			AssertZZRefCusMapCombined(TestConnection, refCusMapA, false);
			AssertZZRefCusMapCombined(TestConnection, refCusMapB, true);
			AssertZZRefCusMapCombined(TestConnection, refCusMapC, false);

			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap1, true);
			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap2, true);
			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap3, true);
			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap4, true);
			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap5, true);
			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap6, true);
			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap7, true);
			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap8, true);
			AssertZZRefCusMapCombined(TestConnection, zzRefCusMap9, true);
		}

		public static Guid InsertRefCusMapType(DbConnection connection, string mapType, string direction, string description, bool isReadonly = false)
		{
			var pk = Guid.NewGuid();
			using (var command = connection.Command(@"INSERT INTO RefDatabase_RefCusMapType(ZZP_PK, ZZP_MapType, ZZP_Direction, ZZP_Description, ZZP_IsReadonly)
				VALUES (@pk, @mapType, @direction, @description, @isReadonly)"))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, RefCusMapTypeSchema.PK);
				command.AddParameterBasedOnDbColumn("@mapType", mapType, RefCusMapTypeSchema.ZZP_MapType);
				command.AddParameterBasedOnDbColumn("@direction", direction, RefCusMapTypeSchema.ZZP_Direction);
				command.AddParameterBasedOnDbColumn("@description", description, RefCusMapTypeSchema.ZZP_Description);
				command.AddParameterBasedOnDbColumn("@isReadonly", @isReadonly, RefCusMapTypeSchema.ZZP_IsReadonly);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid InsertRefCusMap(DbConnection connection, string country, string mapType, string customsValue, string commercialValue, DateTime startDate, DateTime endDate)
		{
			var pk = Guid.NewGuid();
			using (var command = connection.Command(@"

IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = @country)
	INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES(NEWID(), @country, @country, NULL)

INSERT INTO RefDatabase_RefCusMap(ZZM_PK, ZZM_ZZZ_NKDataGrouping, ZZM_ZZP_NKMapType, ZZM_CustomsValue, ZZM_CW1OrCommercialValue, ZZM_StartDate, ZZM_EndDate)
	VALUES (@pk, @country, @mapType, @customsValue, @commercialValue, @startDate, @endDate)"))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, RefCusMapSchema.PK);
				command.AddParameter("@country", System.Data.SqlDbType.VarChar, country);//ZZM_ZZZ_NKDataGrouping
				command.AddParameterBasedOnDbColumn("@mapType", mapType, RefCusMapSchema.ZZM_ZZP_NKMapType);
				command.AddParameterBasedOnDbColumn("@customsValue", customsValue, RefCusMapSchema.ZZM_CustomsValue);
				command.AddParameterBasedOnDbColumn("@commercialValue", commercialValue, RefCusMapSchema.ZZM_CW1orCommercialValue);
				command.AddParameterBasedOnDbColumn("@startDate", startDate, RefCusMapSchema.ZZM_StartDate);
				command.AddParameterBasedOnDbColumn("@endDate", endDate, RefCusMapSchema.ZZM_EndDate);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid InsertZZRefCusMap(DbConnection connection, string country, string mapType, string customsValue, string commercialValue)
		{
			var pk = Guid.NewGuid();
			using (var command = connection.Command(@"INSERT INTO dbo.ZZRefCusMap(ZZM_PK, ZZM_CountryOrGrouping, ZZM_ZZP_NKMapType, ZZM_CustomsValue, ZZM_CW1OrCommercialValue, ZZM_StartDate, ZZM_EndDate)
				VALUES (@pk, @country, @mapType, @customsValue, @commercialValue, '1900-01-01', '2079-06-06 23:59:00')"))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, ZZRefCusMapSchema.PK);
				command.AddParameterBasedOnDbColumn("@country", country, ZZRefCusMapSchema.ZZM_CountryOrGrouping);
				command.AddParameterBasedOnDbColumn("@mapType", mapType, ZZRefCusMapSchema.ZZM_ZZP_NKMapType);
				command.AddParameterBasedOnDbColumn("@customsValue", customsValue, ZZRefCusMapSchema.ZZM_CustomsValue);
				command.AddParameterBasedOnDbColumn("@commercialValue", commercialValue, ZZRefCusMapSchema.ZZM_CW1OrCommercialValue);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static void AssertZZRefCusMapCombined(DbConnection connection, Guid pk, bool exists)
		{
			var count = connection.ExecuteScalar<int>(string.Format("SELECT COUNT(*) FROM dbo.ZZRefCusMapCombined WHERE ZZM_PK='{0}'", pk));
			if (exists)
			{
				Assert("ZZRefCusMapCombined should exists", count == 1);
			}
			else
			{
				Assert("ZZRefCusMapCombined should NOT exists", count == 0);
			}
		}
	}
}

