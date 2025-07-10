using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefTariffUOMViewCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefTariffUOMViewCombined
{
	[TestedType(typeof(trgTariffUOMView_Upd))]
	class trgTariffUOMView_Upd_Test : DbCreateScriptTest
	{
		public void TestUpdate()
		{
			var connection = Db.Connection;
			connection.ExecuteNonQuery(@"

				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID()
				DECLARE @tariffPK1 UNIQUEIDENTIFIER = CAST('C1070EC7-F2E2-407D-B298-D6A3D7083D9B' AS UNIQUEIDENTIFIER)
				DECLARE @tariffPK2 UNIQUEIDENTIFIER = CAST('0B55193D-ACEA-4EB1-AE7A-383725F4FCD9' AS UNIQUEIDENTIFIER)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'CN', 'China', NULL)

				INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
				VALUES (@TariffTypePK, 'TTX', 'Test Tariff Type', 'CN')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@tariffPK1, @TariffTypePK, '10000010', 'Tariff in RefDbEntZZ', 'CN', '2020-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '')

				INSERT INTO RefDatabase_RefCusTariffUOM (ZZ8_PK,ZZ8_ZZ1_Tariff,ZZ8_Type,ZZ8_UOM,ZZ8_ZZZ_NKDataGrouping)
				VALUES ('C4F07A5E-5027-40D3-99B1-1CBD5943C061', @tariffPK1, 'CU1', 'KG', 'CN')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES (@tariffPK2, 'TTX', '10000011', 'Tariff in CusDB', '2020-07-02', '2079-07-02', 'TFF', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTariffUom (CR3_PK,CR3_CR1_Tariff,CR3_Type,CR3_UnitOfMeasure, CR3_SystemCreateTimeUtc, CR3_SystemCreateUser, CR3_SystemLastEditTimeUtc, CR3_SystemLastEditUser)
				VALUES ('FE2DD9A7-1BF0-4BCD-AA0C-20E275CC7D5D', @tariffPK2, 'CU2', 'T', '2019-1-1', '~E', '2020-07-01', '~F')
			");

			var updExc = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(@"UPDATE dbo.TariffUOMView SET ZZ8_DataSet='O' WHERE ZZ8_PK='C4F07A5E-5027-40D3-99B1-1CBD5943C061'"));
			AssertEquals("Upd Exception", "Cannot update a system-defined Tariff UOM.", updExc.Message);

			AssertNoExceptionThrown(() => connection.ExecuteNonQuery(@"UPDATE dbo.TariffUOMView SET ZZ8_Type='CU3',ZZ8_UOM='KG', ZZ8_SystemLastEditTimeUtc='2020-07-03', ZZ8_SystemLastEditUser='NEW' WHERE ZZ8_PK='FE2DD9A7-1BF0-4BCD-AA0C-20E275CC7D5D'"));
			connection.ExecuteReader("SELECT * FROM dbo.TariffUOMView WHERE ZZ8_PK='FE2DD9A7-1BF0-4BCD-AA0C-20E275CC7D5D'",
			reader =>
			{
				AssertEquals("FE2DD9A7-1BF0-4BCD-AA0C-20E275CC7D5D", reader["ZZ8_PK"].ToString().ToUpper());
				AssertEquals("0B55193D-ACEA-4EB1-AE7A-383725F4FCD9", reader["ZZ8_ZZ1_ParentTariffOrNationalCode"].ToString().ToUpper());
				AssertEquals("CU3", (string)reader["ZZ8_Type"]);
				AssertEquals("KG", (string)reader["ZZ8_UOM"]);
				AssertEquals("O", (string)reader["ZZ8_DataSet"]);
				AssertEquals(false, (bool)reader["ZZ8_IsSystem"]);
				AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZZ8_SystemLastEditTimeUtc"]);
				AssertEquals("NEW", (string)reader["ZZ8_SystemLastEditUser"]);
			});
		}
	}
}

