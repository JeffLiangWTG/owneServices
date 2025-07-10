using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefTariffViewCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefTariffViewCombined
{
	[TestedType(typeof(trgTariffView_Upd))]
	class trgTariffView_Upd_Test : DbCreateScriptTest
	{
		public void TestUpd()
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

				INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
				VALUES (NEWID(), 'TTY', 'Test Tariff Type 2', 'CN')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@tariffPK1, @TariffTypePK, '10000010', 'Tariff in RefDbEntZZ', 'CN', '2020-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_CRT_NKTariffVersion, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES (@tariffPK2, 'TTX', '10000011', 'Tariff in CusDB', '2020-07-02', '2079-07-02', 'TFF', 'CN', '000002', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTariffVersion (CRT_PK, CRT_Version, CRT_Description, CRT_EffectiveDate, CRT_RN_NKCountryCode, CRT_SystemCreateTimeUtc, CRT_SystemCreateUser, CRT_SystemLastEditTimeUtc, CRT_SystemLastEditUser)
				VALUES (NEWID(), '000002', 'VERSION DESC 2', '2020-07-02', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTariffVersion (CRT_PK, CRT_Version, CRT_Description, CRT_EffectiveDate, CRT_RN_NKCountryCode, CRT_SystemCreateTimeUtc, CRT_SystemCreateUser, CRT_SystemLastEditTimeUtc, CRT_SystemLastEditUser)
				VALUES (NEWID(), '000001', 'VERSION DESC', '2019-01-02', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var updExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("UPDATE dbo.TariffView SET ZZ1_TariffCode='10000030',ZZ1_DataSet='O' WHERE ZZ1_PK='C1070EC7-F2E2-407D-B298-D6A3D7083D9B'"));
			AssertEquals("Upd Exception", "Cannot update a system-defined Tariff.", updExp.Message);

			updExp = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery("UPDATE dbo.TariffView SET ZZ1_ZZZ_NKDataGrouping='EUN' WHERE ZZ1_PK='0B55193D-ACEA-4EB1-AE7A-383725F4FCD9'"));
			AssertContains("Upd Exception", "Country code cannot be longer than 2 characters.", updExp.Message);

			AssertNoExceptionThrown(() => connection.ExecuteNonQuery(@"
UPDATE dbo.TariffView SET ZZ1_ZZI_NKTariffType='TTY', ZZ1_Description='DESC CHANGED', ZZ1_ZZF_NKTaxOrFeeCode='F',ZZ1_StartDate='2019-1-2',ZZ1_EndDate='2079-6-5',ZZ1_CRT_NKTariffVersion='000001', ZZ1_SystemLastEditTimeUtc='2020-07-03', ZZ1_SystemLastEditUser='NEW' WHERE ZZ1_PK='0B55193D-ACEA-4EB1-AE7A-383725F4FCD9'
"));
			connection.ExecuteReader("SELECT * FROM dbo.TariffView WHERE ZZ1_PK='0B55193D-ACEA-4EB1-AE7A-383725F4FCD9'",
			reader =>
			{
				AssertEquals("TTY", (string)reader["ZZ1_ZZI_NKTariffType"]);
				AssertEquals("DESC CHANGED", (string)reader["ZZ1_Description"]);
				AssertEquals(new DateTime(2019, 1, 2), (DateTime)reader["ZZ1_StartDate"]);
				AssertEquals(new DateTime(2079, 6, 5), (DateTime)reader["ZZ1_EndDate"]);
				AssertEquals("F", (string)reader["ZZ1_ZZF_NKTaxOrFeeCode"]);
				AssertEquals("000001", (string)reader["ZZ1_CRT_NKTariffVersion"]);
				AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZZ1_SystemLastEditTimeUtc"]);
				AssertEquals("NEW", (string)reader["ZZ1_SystemLastEditUser"]);
			});
		}
	}
}

