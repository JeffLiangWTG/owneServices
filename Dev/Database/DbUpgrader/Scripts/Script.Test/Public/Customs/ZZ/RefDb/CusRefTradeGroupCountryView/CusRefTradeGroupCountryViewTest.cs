using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTradeGroupCountryView.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTradeGroupCountryView.CusRefTradeGroupCountryView))]
	class CusRefTradeGroupCountryViewTest : DbCreateScriptTest
	{
		public void TestCusRefTradeGroupCountryView()
		{
			var createRecordsSql = @"
				DECLARE @parentDataGrouping UNIQUEIDENTIFIER = NEWID()
				DECLARE @tradeGroupZZ UNIQUEIDENTIFIER = NEWID()
				DECLARE @tradeGroupCus UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'EUN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (@parentDataGrouping, 'EUN', 'Europe Union', NULL)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'IT')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'IT', 'Italy', @parentDataGrouping)

				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES (@tradeGroupZZ, 'USER1', 'USER TG1', '2020-07-01', '2020-07-16', 'IT')

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (NEWID(), @tradeGroupZZ, '2020-07-01', '2020-07-16',  'ZZB ZZ DESC', 'ZZ')

				INSERT INTO dbo.CusRefTradeGroup (CR9_PK, CR9_TradeGroup, CR9_Description, CR9_StartDate, CR9_EndDate, CR9_RN_NKCountryCode, CR9_SystemCreateTimeUtc, CR9_SystemCreateUser, CR9_SystemLastEditTimeUtc, CR9_SystemLastEditUser)
				VALUES (@tradeGroupCus, 'USER1', 'User TG1', '2020-07-01', '2020-07-16', 'IT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTradeGroupCountry (CRA_PK, CRA_CR9_TradeGroup, CRA_StartDate, CRA_EndDate, CRA_Description, CRA_RN_NKTradeGroupCountryCode, CRA_SystemCreateTimeUtc, CRA_SystemCreateUser, CRA_SystemLastEditTimeUtc, CRA_SystemLastEditUser)
				VALUES (NEWID(), @tradeGroupCus, '2020-07-01', '2020-07-16', 'ZZB DESC', 'AA', '2020-07-01', '~E1', '2021-07-01', '~F1')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);
			AssertSelectResult("ZZ RefCusTradeGroupCountry", connection, "ZZ", new[] { "True, Z, ZZB ZZ DESC, , ~BP, , ~BP" });
			AssertSelectResult("Cus CusRefTradeGroupCountry", connection, "AA", new[] { "False, O, ZZB DESC, 1/07/2020 12:00:00 AM, ~E1, 1/07/2021 12:00:00 AM, ~F1" });
		}

		public void TestViewColumnsMatchUnderlyingTable()
		{
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(Db.Connection, "CusRefTradeGroupCountryView", "RefDatabase_RefCusTradeGroupCountry", new[] {
				new TestDbViewHelper.DbColumn("ZZB_IsSystem", "bit", -1),
				new TestDbViewHelper.DbColumn("ZZB_DataSet", "varchar", 1),
				new TestDbViewHelper.DbColumn("ZZB_SystemCreateTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZZB_SystemCreateUser", "varchar", 3),
				new TestDbViewHelper.DbColumn("ZZB_SystemLastEditTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZZB_SystemLastEditUser", "varchar", 3),
			});
		}

		protected override bool RequiresSchemaBinding => false;

		void AssertSelectResult(string message, DbConnection connection, string tradeGroupCountryCode, string[] expectedResults)
		{
			var selectSql = $"SELECT * FROM dbo.CusRefTradeGroupCountryView WHERE ZZB_RN_NKTradeGroupCountryCode = '{tradeGroupCountryCode}'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZZB_IsSystem"]}, {reader["ZZB_DataSet"]}, {reader["ZZB_Description"]}, {reader["ZZB_SystemCreateTimeUtc"]}, {reader["ZZB_SystemCreateUser"]}, {reader["ZZB_SystemLastEditTimeUtc"]}, {reader["ZZB_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(message, expectedResults, results);
		}
	}
}

