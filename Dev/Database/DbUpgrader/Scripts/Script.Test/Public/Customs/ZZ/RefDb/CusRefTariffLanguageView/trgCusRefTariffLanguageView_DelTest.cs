using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTariffLanguageView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTariffLanguageView.Testing
{
	[TestedType(typeof(trgCusRefTariffLanguageView_Del))]
	class trgCusRefTariffLanguageView_Del_Test : DbCreateScriptTest
	{
		public void TestCusRefTariffLanguageViewDelete()
		{
			var createRecordsSql = @"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID()
				DECLARE @tariffPK1 UNIQUEIDENTIFIER = NEWID()
				DECLARE @tariffPK2 UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'ZA', 'South Africa', NULL)

				INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
				VALUES (@TariffTypePK, 'TTT', 'test', 'ZA')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@tariffPK1, @TariffTypePK, 'TEST', 'Valid Tariff', 'ZA', '2020-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '')

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefLanguageType where ZX6_Language = 'EN')
				INSERT INTO RefDatabase_RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
				VALUES (NEWID() ,'EN','English')

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefLanguageType where ZX6_Language = 'FR')
				INSERT INTO RefDatabase_RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
				VALUES (NEWID() ,'FRN','French')

				INSERT INTO RefDatabase_RefCusTariffLanguage (ZX7_PK, ZX7_ZX6_NKLanguage, ZX7_ZZ1_Tariff, ZX7_Description)
				VALUES (NEWID(), 'EN', @tariffPK1, 'English')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES(@tariffPK2, 'test', 'test', 'test', '2020-07-02', '2079-07-02', 'TFF', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTariffLanguage (CR6_PK, CR6_ZX6_NKLanguage, CR6_CR1_Tariff, CR6_Description, CR6_SystemCreateTimeUtc, CR6_SystemCreateUser, CR6_SystemLastEditTimeUtc, CR6_SystemLastEditUser)
				VALUES (NEWID(), 'FRN', @tariffPK2, 'French', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var deleteSql = "DELETE dbo.CusRefTariffLanguageView WHERE ZX7_ZX6_NKLanguage = 'EN'";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(deleteSql));
			AssertContains("Cannot delete the system-defined TariffLanguage record with ZX7_DataSet = 'Z'.", exceptionSql.Message);

			deleteSql = "DELETE dbo.CusRefTariffLanguageView WHERE ZX7_ZX6_NKLanguage = 'FRN'";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(deleteSql));
			});
		}
	}
}

