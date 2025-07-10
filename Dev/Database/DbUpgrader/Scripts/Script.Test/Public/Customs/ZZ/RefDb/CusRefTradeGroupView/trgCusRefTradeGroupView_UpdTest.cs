using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTradeGroupView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTradeGroupView.Testing
{
	[TestedType(typeof(trgCusRefTradeGroupView_Upd))]
	class trgCusRefTradeGroupView_Upd_Test : DbCreateScriptTest
	{
		public void TesttrgCusRefTradeGroupView_Upd()
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

				INSERT INTO dbo.CusRefTradeGroupView (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping, ZZA_SystemCreateTimeUtc, ZZA_SystemCreateUser, ZZA_SystemLastEditTimeUtc, ZZA_SystemLastEditUser)
				VALUES (NEWID(), 'USER1', 'USER TG1', '2020-07-01', '2020-07-16', 'IT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var updateSql = "UPDATE dbo.CusRefTradeGroupView SET ZZA_DataSet = 'Z' WHERE ZZA_TradeGroup = 'USER1'";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined TradeGroup record with ZZA_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefTradeGroupView SET ZZA_DataSet = 'O' WHERE ZZA_TradeGroup = 'ZZ1'";
			exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined TradeGroup record with ZZA_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefTradeGroupView SET ZZA_Description = 'Description Changed', ZZA_SystemLastEditTimeUtc='2020-07-03', ZZA_SystemLastEditUser='NEW' WHERE ZZA_TradeGroup = 'USER1'";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(updateSql));
			});

			var selectSql = "SELECT * FROM dbo.CusRefTradeGroupView WHERE ZZA_TradeGroup = 'USER1'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($"{reader["ZZA_IsSystem"]}, {reader["ZZA_DataSet"]}, {reader["ZZA_Description"]}, {reader["ZZA_SystemLastEditTimeUtc"]}, {reader["ZZA_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(new[] { "False, O, Description Changed, 3/07/2020 12:00:00 AM, NEW" }, results);
		}
	}
}

