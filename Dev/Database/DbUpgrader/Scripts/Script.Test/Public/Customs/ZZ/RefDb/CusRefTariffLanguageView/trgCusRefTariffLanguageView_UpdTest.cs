using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefTariffLanguageView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefTariffLanguageView.Testing
{
	[TestedType(typeof(trgCusRefTariffLanguageView_Upd))]
	class trgCusRefTariffLanguageView_Upd_Test : DbCreateScriptTest
	{
		public void TestCusRefTariffLanguageViewUpdate()
		{
			var createRecordsSql = @"
				DECLARE @tariffPK UNIQUEIDENTIFIER = NEWID()
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

				INSERT INTO RefDatabase_RefCusTariffLanguage (ZX7_PK, ZX7_ZX6_NKLanguage, ZX7_ZZ1_Tariff, ZX7_Description)
				VALUES (NEWID(), 'EN', @tariffPK1, 'ZZ English')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES(@tariffPK, 'test', 'test', 'test', '2020-07-02', '2079-07-02', 'TFF', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTariffLanguageView (ZX7_PK, ZX7_ZX6_NKLanguage, ZX7_ZZ1_Tariff, ZX7_Description, ZX7_SystemCreateTimeUtc, ZX7_SystemCreateUser, ZX7_SystemLastEditTimeUtc, ZX7_SystemLastEditUser)
				VALUES (NEWID(), 'EN3', @tariffPK, 'User English', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var updateSql = "UPDATE dbo.CusRefTariffLanguageView SET ZX7_DataSet = 'Z' WHERE ZX7_ZX6_NKLanguage = 'EN3'";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined TariffLanguage record with ZX7_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefTariffLanguageView SET ZX7_Description = 'Z' WHERE ZX7_ZX6_NKLanguage = 'EN'";
			exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined TariffLanguage record with ZX7_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefTariffLanguageView SET ZX7_DataSet = 'O' WHERE ZX7_ZX6_NKLanguage = 'EN'";
			exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(updateSql));
			AssertContains("Cannot update the system-defined TariffLanguage record with ZX7_DataSet = 'Z'.", exceptionSql.Message);

			updateSql = "UPDATE dbo.CusRefTariffLanguageView SET ZX7_DataSet = 'O', ZX7_Description = 'Changed User English', ZX7_SystemLastEditTimeUtc='2020-07-03', ZX7_SystemLastEditUser='NEW' WHERE ZX7_ZX6_NKLanguage = 'EN3'";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(updateSql));
			});

			var selectSql = "SELECT * FROM dbo.CusRefTariffLanguageView WHERE ZX7_ZX6_NKLanguage = 'EN3'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($"{reader["ZX7_IsSystem"]}, {reader["ZX7_DataSet"]}, {reader["ZX7_Description"]}, {reader["ZX7_SystemLastEditTimeUtc"]}, {reader["ZX7_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(new[] { "False, O, Changed User English, 3/07/2020 12:00:00 AM, NEW" }, results);
		}
	}
}

