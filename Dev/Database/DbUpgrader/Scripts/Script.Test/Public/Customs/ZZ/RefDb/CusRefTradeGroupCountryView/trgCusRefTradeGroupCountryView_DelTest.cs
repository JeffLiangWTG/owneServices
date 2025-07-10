using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTradeGroupCountryView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTradeGroupCountryView.Testing
{
	[TestedType(typeof(trgCusRefTradeGroupCountryView_Del))]
	class trgCusRefTradeGroupCountryView_Del_Test : DbCreateScriptTest
	{
		public void TesttrgCusRefTradeGroupCountryView_Del()
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
				VALUES (@tradeGroup, 'USER1', 'USER TG1', '2020-07-01', '2020-07-16', 'IT', '2020-07-01', '~E1', '2021-07-01', '~F1')

				INSERT INTO dbo.CusRefTradeGroupCountryView (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode, ZZB_SystemCreateTimeUtc, ZZB_SystemCreateUser, ZZB_SystemLastEditTimeUtc, ZZB_SystemLastEditUser)
				VALUES (NEWID(), @tradeGroup, '2020-07-01', '2020-07-16',  'ZZB DESC', 'AA', '2020-07-01', '~E1', '2021-07-01', '~F1')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var deleteSql = "DELETE dbo.CusRefTradeGroupCountryView WHERE ZZB_RN_NKTradeGroupCountryCode = 'ZZ'";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(deleteSql));
			AssertContains("Cannot delete the system-defined TradeGroupCountry record with ZZB_DataSet = 'Z'.", exceptionSql.Message);

			deleteSql = "DELETE dbo.CusRefTradeGroupCountryView WHERE ZZB_RN_NKTradeGroupCountryCode = 'AA'";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(deleteSql));
			});
		}
	}
}

