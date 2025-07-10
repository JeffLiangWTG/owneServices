using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTradeGroupCountryView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTradeGroupCountryView.Testing
{
	[TestedType(typeof(trgCusRefTradeGroupCountryView_Upd))]
	class trgCusRefTradeGroupCountryView_Upd_Test : DbCreateScriptTest
	{
		public void TesttrgCusRefTradeGroupCountryView_Upd()
		{
			var createRecordsSql = @"
				DECLARE @parentDataGrouping UNIQUEIDENTIFIER = NEWID()
				DECLARE @tradeGroup UNIQUEIDENTIFIER = NEWID()
				DECLARE @tradeGroupZZ UNIQUEIDENTIFIER = NEWID()

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

				INSERT INTO dbo.CusRefTradeGroupView (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping, ZZA_SystemCreateTimeUtc, ZZA_SystemCreateUser, ZZA_SystemLastEditTimeUtc, ZZA_SystemLastEditUser)
				VALUES (@tradeGroup, 'USER1', 'USER TG1', '2020-07-01', '2020-07-16', 'IT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTradeGroupCountryView (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode, ZZB_SystemCreateTimeUtc, ZZB_SystemCreateUser, ZZB_SystemLastEditTimeUtc, ZZB_SystemLastEditUser)
				VALUES (NEWID(), @tradeGroup, '2020-07-01', '2020-07-16',  'ZZB DESC', 'AA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var updateSql = "UPDATE dbo.CusRefTradeGroupCountryView SET ZZB_DataSet = 'Z' WHERE ZZB_RN_NKTradeGroupCountryCode = 'AA'";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined TradeGroupCountry record with ZZB_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefTradeGroupCountryView SET ZZB_Description = 'Description Changed' WHERE ZZB_RN_NKTradeGroupCountryCode = 'ZZ'";
			exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined TradeGroupCountry record with ZZB_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefTradeGroupCountryView SET ZZB_DataSet = 'O' WHERE ZZB_RN_NKTradeGroupCountryCode = 'ZZ'";
			exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined TradeGroupCountry record with ZZB_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefTradeGroupCountryView SET ZZB_Description = 'Description Changed', ZZB_SystemLastEditTimeUtc='2020-07-03', ZZB_SystemLastEditUser='NEW' WHERE ZZB_RN_NKTradeGroupCountryCode = 'AA'";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(updateSql));
			});

			var selectSql = "SELECT * FROM dbo.CusRefTradeGroupCountryView WHERE ZZB_RN_NKTradeGroupCountryCode = 'AA'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($"{reader["ZZB_IsSystem"]}, {reader["ZZB_DataSet"]}, {reader["ZZB_Description"]}, {reader["ZZB_SystemLastEditTimeUtc"]}, {reader["ZZB_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(new[] { "False, O, Description Changed, 3/07/2020 12:00:00 AM, NEW" }, results);
		}
	}
}

