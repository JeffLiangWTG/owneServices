using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefRateUOMView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefRateUOMView.Testing
{
	[TestedType(typeof(trgCusRefRateUOMView_Ins))]
	class trgCusRefRateUOMView_Ins_Test : DbCreateScriptTest
	{
		public void TestCusRefRateUOMView_Ins()
		{
			var ratePK = Guid.NewGuid();

			var createRecordsSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID()
				DECLARE @tariffPK2 UNIQUEIDENTIFIER = NEWID()
				DECLARE @rateCodePK UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'ZA', 'South Africa', NULL)

				INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
				VALUES (@TariffTypePK, 'TTT', 'test', 'ZA')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES(@tariffPK2, 'TTT', 'test', 'test', '2020-07-02', '2079-07-02', 'TFF', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefRateCode (CR7_PK, CR7_RateCode, CR7_RateType, CR7_Description, CR7_RN_NKCountryCode, CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
				VALUES (@rateCodePK, 'test', 'ADD', 'test', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefRate (CR2_PK, CR2_CR1_TARIFF, CR2_CR7_RateCode, CR2_RateFormula, CR2_StartDate, CR2_SystemCreateTimeUtc, CR2_SystemCreateUser, CR2_SystemLastEditTimeUtc, CR2_SystemLastEditUser)
				VALUES ('{ratePK}', @tariffPK2, @rateCodePK, 'CusRate1', '2020-01-01 00:00:00.000', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var insertSql = $"INSERT INTO dbo.CusRefRateUOMView (ZXG_PK, ZXG_ZZ2_Rate, ZXG_UOM, ZXG_DataSet) VALUES(NEWID(), '{ratePK}', 'CusUOM2', 'Z')";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(insertSql));
			AssertContains("Cannot insert the system-defined Rate UOM record with ZXG_DataSet = 'Z'.", exceptionSql.Message);

			insertSql = $@"INSERT INTO dbo.CusRefRateUOMView (ZXG_PK, ZXG_ZZ2_Rate, ZXG_UOM, ZXG_DataSet, ZXG_SystemCreateTimeUtc, ZXG_SystemCreateUser, ZXG_SystemLastEditTimeUtc, ZXG_SystemLastEditUser)
VALUES(NEWID(), '{ratePK}', 'CusUOM2', 'O', '2019-1-1', '~E', '2020-07-03', '~F')";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(insertSql));
			});

			connection.ExecuteReader("SELECT * FROM dbo.CusRefRateUOMView",
				reader =>
				{
					AssertEquals(ratePK, (Guid)reader["ZXG_ZZ2_Rate"]);
					AssertEquals("CusUOM2", (string)reader["ZXG_UOM"]);
					AssertEquals("O", (string)reader["ZXG_DataSet"]);
					AssertEquals(false, (bool)reader["ZXG_IsSystem"]);
					AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZXG_SystemCreateTimeUtc"]);
					AssertEquals("~E", (string)reader["ZXG_SystemCreateUser"]);
					AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZXG_SystemLastEditTimeUtc"]);
					AssertEquals("~F", (string)reader["ZXG_SystemLastEditUser"]);
				});
		}
	}
}

