using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTradeGroupView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTradeGroupView.Testing
{
	[TestedType(typeof(trgCusRefTradeGroupView_Del))]
	class trgCusRefTradeGroupView_Del_Test : DbCreateScriptTest
	{
		public void TesttrgCusRefTradeGroupView_Del()
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
				VALUES (NEWID(), 'USER1', 'User TG1', '2020-07-01', '2020-07-16', 'IT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var deleteSql = "DELETE dbo.CusRefTradeGroupView WHERE ZZA_TradeGroup = 'ZZ1'";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(deleteSql));
			AssertContains("Cannot delete the system-defined TradeGroup record with ZZA_DataSet = 'Z'.", exceptionSql.Message);

			deleteSql = "DELETE dbo.CusRefTradeGroupView WHERE ZZA_TradeGroup = 'USER1'";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(deleteSql));
			});
		}
	}
}

