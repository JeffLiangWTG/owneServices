using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefTariffViewCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefTariffViewCombined
{
	[TestedType(typeof(trgTariffView_Del))]
	class trgTariffView_Del_Test : DbCreateScriptTest
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

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES (@tariffPK2, 'TTX', '10000011', 'Tariff in CusDB', '2020-07-02', '2079-07-02', 'TFF', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var delExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("DELETE FROM dbo.TariffView WHERE ZZ1_PK='C1070EC7-F2E2-407D-B298-D6A3D7083D9B'"));
			AssertEquals("Should have thrown a correct exception with correct message", "Cannot delete a system-defined Tariff.", delExp.Message);
			AssertNoExceptionThrown("Should be able to delete item from CusDB.", () => connection.ExecuteNonQuery("DELETE FROM dbo.TariffView WHERE ZZ1_PK='0B55193D-ACEA-4EB1-AE7A-383725F4FCD9'"));
			connection.ExecuteReader("SELECT ZZ1_PK FROM dbo.TariffView",
			reader =>
			{
				AssertEquals("Should only be RefDatabase_RefCusTariff item left.", "C1070EC7-F2E2-407D-B298-D6A3D7083D9B", reader["ZZ1_PK"].ToString().ToUpper());
			});
		}
	}
}

