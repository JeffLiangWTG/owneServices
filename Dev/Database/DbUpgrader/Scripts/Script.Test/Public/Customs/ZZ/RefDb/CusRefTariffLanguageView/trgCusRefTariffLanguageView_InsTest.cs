using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTariffLanguageView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTariffLanguageView.Testing
{
	[TestedType(typeof(trgCusRefTariffLanguageView_Ins))]
	class trgCusRefTariffLanguageView_Ins_Test : DbCreateScriptTest
	{
		public void TestCusRefTariffLanguageViewInsert()
		{
			var tariffPK = Guid.NewGuid();

			var createRecordsSql = $@"
				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'ZA', 'South Africa', NULL)

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES('{tariffPK}', 'test', 'test', 'test', '2020-07-02', '2079-07-02', 'TFF', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var insertSql = $"INSERT INTO dbo.CusRefTariffLanguageView (ZX7_PK, ZX7_ZX6_NKLanguage, ZX7_ZZ1_Tariff, ZX7_Description, ZX7_DataSet) VALUES(NEWID(), 'TT2', '{tariffPK}', 'Test2', 'Z')";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(insertSql));
			AssertContains("Cannot insert the system-defined TariffLanguage record with ZX7_DataSet = 'Z'.", exceptionSql.Message);

			insertSql = $@"INSERT INTO dbo.CusRefTariffLanguageView (ZX7_PK, ZX7_ZX6_NKLanguage, ZX7_ZZ1_Tariff, ZX7_Description, ZX7_DataSet, ZX7_SystemCreateTimeUtc, ZX7_SystemCreateUser, ZX7_SystemLastEditTimeUtc, ZX7_SystemLastEditUser)
VALUES(NEWID(), 'TT1', '{tariffPK}', 'Test1', 'O', '2019-1-1', '~E', '2020-07-03', '~F')";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(insertSql));
			});

			connection.ExecuteReader("SELECT * FROM dbo.CusRefTariffLanguageView",
				reader =>
				{
					AssertEquals(tariffPK, (Guid)reader["ZX7_ZZ1_Tariff"]);
					AssertEquals("TT1", (string)reader["ZX7_ZX6_NKLanguage"]);
					AssertEquals("Test1", (string)reader["ZX7_Description"]);
					AssertEquals("O", (string)reader["ZX7_DataSet"]);
					AssertEquals(false, (bool)reader["ZX7_IsSystem"]);
					AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZX7_SystemCreateTimeUtc"]);
					AssertEquals("~E", (string)reader["ZX7_SystemCreateUser"]);
					AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZX7_SystemLastEditTimeUtc"]);
					AssertEquals("~F", (string)reader["ZX7_SystemLastEditUser"]);
				});
		}
	}
}

