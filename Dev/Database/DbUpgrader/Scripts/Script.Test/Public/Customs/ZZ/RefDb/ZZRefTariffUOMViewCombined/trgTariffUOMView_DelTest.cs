using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefTariffUOMViewCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefTariffUOMViewCombined
{
	[TestedType(typeof(trgTariffUOMView_Del))]
	class trgTariffUOMView_Del_Test : DbCreateScriptTest
	{
		public void TestDelete()
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
				VALUES ('23634B43-5E74-4516-A00F-0AECE6BF9F69', @tariffPK1, 'CU1', 'KG', 'CN')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES (@tariffPK2, 'TTX', '10000011', 'Tariff in CusDB', '2020-07-02', '2079-07-02', 'TFF', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTariffUom (CR3_PK,CR3_CR1_Tariff,CR3_Type,CR3_UnitOfMeasure, CR3_SystemCreateTimeUtc, CR3_SystemCreateUser, CR3_SystemLastEditTimeUtc, CR3_SystemLastEditUser)
				VALUES ('B023AE1F-5573-482C-9E67-FC6E3F271DCE', @tariffPK2, 'CU2', 'T', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var delExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("DELETE FROM dbo.TariffUOMView WHERE ZZ8_PK='23634B43-5E74-4516-A00F-0AECE6BF9F69'"));
			AssertEquals("Should have thrown a correct exception with correct message", "Cannot delete a system-defined Tariff UOM.", delExp.Message);

			AssertNoExceptionThrown("Should be able to delete item from CusDB.", () => connection.ExecuteNonQuery("DELETE FROM dbo.TariffUOMView WHERE ZZ8_PK='B023AE1F-5573-482C-9E67-FC6E3F271DCE'"));
			connection.ExecuteReader("SELECT ZZ8_PK FROM dbo.TariffUOMView",
			reader =>
			{
				AssertEquals("Should only be RefDatabase_RefCusTariffUOM item left.", "23634B43-5E74-4516-A00F-0AECE6BF9F69", reader["ZZ8_PK"].ToString().ToUpper());
			});
		}
	}
}

