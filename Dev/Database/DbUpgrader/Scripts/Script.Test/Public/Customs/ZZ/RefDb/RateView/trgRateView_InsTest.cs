using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.RateView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.RateView.Testing
{
	[TestedType(typeof(trgRateView_Ins))]
	class trgRateView_Ins_Test : DbCreateScriptTest
	{
		public void TestRateView_Ins()
		{
			var tariffPK = Guid.NewGuid();
			var rateCodePK = Guid.NewGuid();

			var createRecordsSql = $@"
				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'ZA', 'South Africa', NULL)

				INSERT INTO [dbo].[CusRefTariff] ([CR1_PK], [CR1_ZZI_NKTariffType], [CR1_TariffCode], [CR1_Description], [CR1_StartDate], [CR1_EndDate], [CR1_ZZF_NKTaxOrFeeCode], [CR1_RN_NKCountryCode], CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES('{tariffPK}', 'test', 'test', 'test', '2020-07-02', '2079-07-02', 'TFF', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO [dbo].[CusRefRateCode] ([CR7_PK], [CR7_RateCode], [CR7_RateType], [CR7_Description], [CR7_RN_NKCountryCode], CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
				VALUES ('{rateCodePK}', 'test', 'ADD', 'test', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			var insertSql = $@"INSERT INTO dbo.RateView (ZZ2_PK, ZZ2_ZZ1_ParentTariffOrNationalCode, ZZ2_ZY1_RateCode, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_DataSet)
							VALUES(NEWID(), '{tariffPK}', '{rateCodePK}', 'ZA', 'CusRate', '2020-01-01 00:00:00.000', '2022-01-01 00:00:00.000','Z')";
			var exceptionSql = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(insertSql));
			AssertContains("Cannot insert the system-defined Rate record with ZZ2_DataSet = 'Z'.", exceptionSql.Message);

			insertSql = $@"INSERT INTO dbo.RateView (ZZ2_PK, ZZ2_ZZ1_ParentTariffOrNationalCode, ZZ2_ZY1_RateCode, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_DataSet, ZZ2_SystemCreateTimeUtc, ZZ2_SystemCreateUser, ZZ2_SystemLastEditTimeUtc, ZZ2_SystemLastEditUser)
							VALUES(NEWID(), '{tariffPK}', '{rateCodePK}', 'ZA', 'CusRate', '2020-01-01 00:00:00.000', '2022-01-01 00:00:00.000','O', '2019-1-1', '~E', '2020-07-03', '~F')";
			AssertNoExceptionThrown(() =>
			{
				AssertEquals(1, connection.ExecuteNonQuery(insertSql));
			});

			connection.ExecuteReader("SELECT * FROM dbo.RateView",
				reader =>
				{
					AssertEquals(tariffPK, (Guid)reader["ZZ2_ZZ1_ParentTariffOrNationalCode"]);
					AssertEquals(rateCodePK, (Guid)reader["ZZ2_ZY1_RateCode"]);
					AssertEquals("ZA", (string)reader["ZZ2_ZZZ_NKDataGrouping"]);
					AssertEquals("CusRate", (string)reader["ZZ2_RateFormula"]);
					AssertEquals(new DateTime(2020, 1, 1), (DateTime)reader["ZZ2_StartDate"]);
					AssertEquals(new DateTime(2022, 1, 1), (DateTime)reader["ZZ2_EndDate"]);
					AssertEquals("O", (string)reader["ZZ2_DataSet"]);
					AssertEquals(false, (bool)reader["ZZ2_IsSystem"]);
					AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZZ2_SystemCreateTimeUtc"]);
					AssertEquals("~E", (string)reader["ZZ2_SystemCreateUser"]);
					AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZZ2_SystemLastEditTimeUtc"]);
					AssertEquals("~F", (string)reader["ZZ2_SystemLastEditUser"]);
				});
		}
	}
}

