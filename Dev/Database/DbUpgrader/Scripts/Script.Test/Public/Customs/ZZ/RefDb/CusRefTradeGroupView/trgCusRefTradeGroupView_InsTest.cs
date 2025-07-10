using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTradeGroupView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTradeGroupView.Testing
{
	[TestedType(typeof(trgCusRefTradeGroupView_Ins))]
	class trgCusRefTradeGroupView_Ins_Test : DbCreateScriptTest
	{
		public void TesttrgCusRefTradeGroupView_Ins()
		{
			var createRecordsSql = @"
				DECLARE @parentDataGrouping UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'EUN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (@parentDataGrouping, 'EUN', 'Europe Union', NULL)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'IT')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'IT', 'Italy', @parentDataGrouping)
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var insertSql = @"INSERT INTO dbo.CusRefTradeGroupView(ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping, ZZA_DataSet)
				VALUES(NEWID(), 'USER1', 'USER TG1', '2020-07-01', '2020-07-16', 'IT', 'Z')";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(insertSql));
			AssertContains("Cannot insert the system-defined TradeGroup record with ZZA_DataSet = 'Z'.", exceptionSql.Message);

			insertSql = @"INSERT INTO dbo.CusRefTradeGroupView(ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping, ZZA_DataSet, ZZA_SystemCreateTimeUtc, ZZA_SystemCreateUser, ZZA_SystemLastEditTimeUtc, ZZA_SystemLastEditUser)
				VALUES(NEWID(), 'USER1', 'USER TG1', '2020-07-01', '2020-07-16', 'IT', 'O', '2020-07-01', '~E1', '2021-07-01', '~F1')";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(insertSql));
			});

			var selectSql = "SELECT * FROM dbo.CusRefTradeGroupView WHERE ZZA_TradeGroup = 'USER1'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZZA_IsSystem"]}, {reader["ZZA_DataSet"]}, {reader["ZZA_Description"]}, {reader["ZZA_SystemCreateTimeUtc"]}, {reader["ZZA_SystemCreateUser"]}, {reader["ZZA_SystemLastEditTimeUtc"]}, {reader["ZZA_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(new[] { "False, O, USER TG1, 1/07/2020 12:00:00 AM, ~E1, 1/07/2021 12:00:00 AM, ~F1" }, results);
		}
	}
}

