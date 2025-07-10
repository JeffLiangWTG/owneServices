using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefTariffUOMViewCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefTariffUOMViewCombined
{
	[TestedType(typeof(trgTariffUOMView_Ins))]
	class trgTariffUOMView_Ins_Test : DbCreateScriptTest
	{
		public void TestInsert()
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
				VALUES (NEWID(), @tariffPK1, 'CU1', 'KG', 'CN')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES (@tariffPK2, 'TTX', '10000011', 'Tariff in CusDB', '2020-07-02', '2079-07-02', 'TFF', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTariffUom (CR3_PK,CR3_CR1_Tariff,CR3_Type,CR3_UnitOfMeasure, CR3_SystemCreateTimeUtc, CR3_SystemCreateUser, CR3_SystemLastEditTimeUtc, CR3_SystemLastEditUser)
				VALUES (NEWID(), @tariffPK2, 'CU2', 'T', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var insExc = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.TariffUOMView (ZZ8_PK,ZZ8_ZZ1_ParentTariffOrNationalCode,ZZ8_ParentTableType,ZZ8_Type,ZZ8_UOM,ZZ8_ZZA_TradeGroup,ZZ8_DataSet,ZZ8_IsSystem)
VALUES('54C7EA59-0C6D-42F7-B450-8E9E9F101265','0B55193D-ACEA-4EB1-AE7A-383725F4FCD9','CR1','CU1','KG',NULL,'Z',CONVERT(BIT, 1))
"));
			AssertEquals("Ins Exc Message", "Cannot insert a system-defined Tariff UOM.", insExc.Message);

			AssertNoExceptionThrown(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.TariffUOMView (ZZ8_PK,ZZ8_ZZ1_ParentTariffOrNationalCode,ZZ8_ParentTableType,ZZ8_Type,ZZ8_UOM,ZZ8_ZZA_TradeGroup,ZZ8_DataSet,ZZ8_IsSystem, ZZ8_SystemCreateTimeUtc, ZZ8_SystemCreateUser, ZZ8_SystemLastEditTimeUtc, ZZ8_SystemLastEditUser)
VALUES('ED5ADC09-BAA7-439A-9908-6F64234E338F','0B55193D-ACEA-4EB1-AE7A-383725F4FCD9','CR1','CU1','KG',NULL,'O',CONVERT(BIT, 0), '2019-1-1', '~E', '2020-07-03', '~F')"));
			connection.ExecuteReader("SELECT * FROM dbo.TariffUOMView WHERE ZZ8_PK='ED5ADC09-BAA7-439A-9908-6F64234E338F'",
			reader =>
			{
				AssertEquals("Should have inserted a record.", "ED5ADC09-BAA7-439A-9908-6F64234E338F", reader["ZZ8_PK"].ToString().ToUpper());
				AssertEquals("0B55193D-ACEA-4EB1-AE7A-383725F4FCD9", reader["ZZ8_ZZ1_ParentTariffOrNationalCode"].ToString().ToUpper());
				AssertEquals("CU1", (string)reader["ZZ8_Type"]);
				AssertEquals("KG", (string)reader["ZZ8_UOM"]);
				AssertEquals("O", (string)reader["ZZ8_DataSet"]);
				AssertEquals(false, (bool)reader["ZZ8_IsSystem"]);
				AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZZ8_SystemCreateTimeUtc"]);
				AssertEquals("~E", (string)reader["ZZ8_SystemCreateUser"]);
				AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZZ8_SystemLastEditTimeUtc"]);
				AssertEquals("~F", (string)reader["ZZ8_SystemLastEditUser"]);
			});
		}
	}
}

