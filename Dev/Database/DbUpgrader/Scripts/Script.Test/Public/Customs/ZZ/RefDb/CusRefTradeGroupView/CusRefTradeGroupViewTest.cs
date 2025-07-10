using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTradeGroupView.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTradeGroupView.CusRefTradeGroupView))]
	class CusRefTradeGroupViewTest : DbCreateScriptTest
	{
		public void TestCusRefTradeGroupView()
		{
			var createRecordsSql = @"
				DECLARE @parentDataGrouping UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'EUN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (@parentDataGrouping, 'EUN', 'Europe Union', NULL)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'IT')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'IT', 'Italy', @parentDataGrouping)

				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES (NEWID(), 'ZZ1', 'ZZ TG1', '2020-07-01', '2020-07-16', 'IT')

				INSERT INTO dbo.CusRefTradeGroup (CR9_PK, CR9_TradeGroup, CR9_Description, CR9_StartDate, CR9_EndDate, CR9_RN_NKCountryCode, CR9_SystemCreateTimeUtc, CR9_SystemCreateUser, CR9_SystemLastEditTimeUtc, CR9_SystemLastEditUser)
				VALUES (NEWID(), 'USER1', 'User TG1', '2020-07-01', '2020-07-16', 'IT', '2020-07-01', '~E1', '2021-07-01', '~F1')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);
			AssertSelectResult("ZZ RefCusTradeGroup", connection, "ZZ1", new[] { "True, Z, ZZ TG1, , ~BP, , ~BP" });
			AssertSelectResult("Cus CusRefTradeGroup", connection, "USER1", new[] { "False, O, User TG1, 1/07/2020 12:00:00 AM, ~E1, 1/07/2021 12:00:00 AM, ~F1" });
		}

		public void TestViewColumnsMatchUnderlyingTable()
		{
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(Db.Connection, "CusRefTradeGroupView", "RefDatabase_RefCusTradeGroup", new[] {
				new TestDbViewHelper.DbColumn("ZZA_IsSystem", "bit", -1),
				new TestDbViewHelper.DbColumn("ZZA_DataSet", "varchar", 1),
				new TestDbViewHelper.DbColumn("ZZA_SystemCreateTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZZA_SystemCreateUser", "varchar", 3),
				new TestDbViewHelper.DbColumn("ZZA_SystemLastEditTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZZA_SystemLastEditUser", "varchar", 3),
			});
		}

		protected override bool RequiresSchemaBinding => false;

		void AssertSelectResult(string message, DbConnection connection, string tradeGroup, string[] expectedResults)
		{
			var selectSql = $"SELECT * FROM dbo.CusRefTradeGroupView WHERE ZZA_TradeGroup = '{tradeGroup}'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZZA_IsSystem"]}, {reader["ZZA_DataSet"]}, {reader["ZZA_Description"]}, {reader["ZZA_SystemCreateTimeUtc"]}, {reader["ZZA_SystemCreateUser"]}, {reader["ZZA_SystemLastEditTimeUtc"]}, {reader["ZZA_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(message, expectedResults, results);
		}
	}
}

