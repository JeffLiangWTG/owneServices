using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities
{
	[TestedType(typeof(GetCalculatedEconomicGrouping))]
	class GetCalculatedEconomicGroupingTest : DbCreateScriptTest
	{
		public void TestCalculatedEconomicGrouping()
		{
			var transactionPostDate = "2019-03-29 23:59:29";
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'OUT')");
			AssertEquals("Calculated Economic Grouping", "OUT", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);

			transactionPostDate = "2019-03-30 00:00:00";
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'OUT')");
			AssertEquals("Calculated Economic Grouping", "OUT", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);

			transactionPostDate = "2019-03-30 00:00:01";
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'OUT')");
			AssertEquals("Calculated Economic Grouping", "OUT", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);

			TestConnection.ExecuteNonQuery($@"
IF NOT EXISTS (SELECT null FROM RefDatabase_RefDataGrouping WHERE ZZZ_DataGrouping = 'ZZ')
INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), 'ZZ', 'TEST', NULL)
INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping) VALUES (NEWID(), '_XX_', 'JUST FOR TEST', 1, 'ZZ')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
					VALUES (NEWID(), '_XX_', 'ACCBREXIT', '_X_1', '2019-03-30 00:00:00', '2079-06-06 23:59:00', 'ZZ')"); // To ensure ZZD_ZZK_NKCodeType = FUNC is used

			transactionPostDate = "2019-03-29 23:59:29";
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'OUT')");
			AssertEquals("Calculated Economic Grouping", "OUT", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);

			TestConnection.ExecuteNonQuery($@"
INSERT INTO RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), '_X', 'TEST', NULL)
INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping) VALUES (NEWID(), 'FUNC', 'JUST FOR TEST', 1, '_X')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
					VALUES (NEWID(), 'FUNC', 'ACCBREXIT', '_X_1', '2019-03-30 00:00:00', '2079-06-06 23:59:00', '_X')"); // To ensure IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZK_NKCodeType_ZZD_Code_ZZD_StartDate is used

			transactionPostDate = "2019-03-29 23:59:29";
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'OUT')");
			AssertEquals("Calculated Economic Grouping", "OUT", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);

			TestConnection.ExecuteNonQuery($@"
INSERT INTO RefDatabase_RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadOnly, ZZK_ZZZ_NKDataGrouping) VALUES (NEWID(), 'FUNC', 'JUST FOR TEST', 1, 'ZZ')
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
					VALUES (NEWID(), 'FUNC', 'ACCBREXIT', '_X_1', '2019-03-30 00:00:00', '2079-06-06 23:59:00', 'ZZ')");

			transactionPostDate = "2019-03-29 23:59:29";
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'OUT')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', '')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);

			transactionPostDate = "2019-03-30 00:00:00";
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'OUT')");
			AssertEquals("Calculated Economic Grouping", "OUT", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);

			transactionPostDate = "2019-03-30 00:00:01";
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'OUT')");
			AssertEquals("Calculated Economic Grouping", "OUT", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'GB', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', 'EUN')");
			AssertEquals("Calculated Economic Grouping", "EUN", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM dbo.GetCalculatedEconomicGrouping('{transactionPostDate}', 'IT', '')");
			AssertEquals("Calculated Economic Grouping", "", dataTable.Rows[0]["CalculatedEconomicGrouping"]);
		}
	}
}

