using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefTariffViewCombined;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefTariffViewCombined
{
	[TestedType(typeof(trgTariffView_Ins))]
	class trgTariffView_Ins_Test : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var connection = Db.Connection;
			connection.ExecuteNonQuery(@"

				DECLARE @TariffTypePK UNIQUEIDENTIFIER = CAST('D4550B6A-DF4F-4DDE-B3DA-9CCD9B4CA85C' AS UNIQUEIDENTIFIER)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'CN', 'China', NULL)

				INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
				VALUES (@TariffTypePK, 'TTX', 'Test Tariff Type', 'CN')

				INSERT INTO dbo.CusRefTariffVersion (CRT_PK, CRT_Version, CRT_Description, CRT_EffectiveDate, CRT_RN_NKCountryCode, CRT_SystemCreateTimeUtc, CRT_SystemCreateUser, CRT_SystemLastEditTimeUtc, CRT_SystemLastEditUser)
				VALUES (NEWID(), '000001', 'VERSION DESC', '2019-1-1', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var insExc = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.TariffView (ZZ1_PK,ZZ1_DataSet,ZZ1_ZZI_NKTariffType,ZZ1_TariffCode,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping)
VALUES('54C7EA59-0C6D-42F7-B450-8E9E9F101265','Z','CUS','10000010','Customs Tariff','2019-1-1','2079-1-1','','CN')
"));
			AssertEquals("Ins Exc Message", "Cannot insert a system-defined Tariff.", insExc.Message);

			insExc = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.TariffView (ZZ1_PK,ZZ1_DataSet,ZZ1_ZZI_NKTariffType,ZZ1_TariffCode,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping)
VALUES(NEWID(),'O','CUS','10000020','Customs Tariff','2019-1-1','2079-1-1','','EUN')
"));
			AssertContains("Ins Exc Message", "Country code cannot be longer than 2 characters.", insExc.Message);

			AssertNoExceptionThrown(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.TariffView (ZZ1_PK,ZZ1_DataSet,ZZ1_ZZI_NKTariffType,ZZ1_TariffCode,ZZ1_Description,ZZ1_StartDate,ZZ1_EndDate,ZZ1_ZZF_NKTaxOrFeeCode,ZZ1_ZZZ_NKDataGrouping,ZZ1_CRT_NKTariffVersion, ZZ1_SystemCreateTimeUtc, ZZ1_SystemCreateUser, ZZ1_SystemLastEditTimeUtc, ZZ1_SystemLastEditUser)
VALUES(NEWID(),'O','CUS','10000030','Customs Tariff 3','2019-1-1','2079-1-1','F','CN','000001', '2019-1-1', '~E', '2020-07-03', '~F')
"));
			connection.ExecuteReader("SELECT * FROM dbo.TariffView",
			reader =>
			{
				AssertEquals("CUS", (string)reader["ZZ1_ZZI_NKTariffType"]);
				AssertEquals("10000030", (string)reader["ZZ1_TariffCode"]);
				AssertEquals("Customs Tariff 3", (string)reader["ZZ1_Description"]);
				AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZZ1_StartDate"]);
				AssertEquals(new DateTime(2079, 1, 1), (DateTime)reader["ZZ1_EndDate"]);
				AssertEquals("F", (string)reader["ZZ1_ZZF_NKTaxOrFeeCode"]);
				AssertEquals("CN", (string)reader["ZZ1_ZZZ_NKDataGrouping"]);
				AssertEquals("000001", (string)reader["ZZ1_CRT_NKTariffVersion"]);
				AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZZ1_SystemCreateTimeUtc"]);
				AssertEquals("~E", (string)reader["ZZ1_SystemCreateUser"]);
				AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZZ1_SystemLastEditTimeUtc"]);
				AssertEquals("~F", (string)reader["ZZ1_SystemLastEditUser"]);
			});
		}
	}
}

