using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefApplicabilityView;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefApplicabilityView
{
	[TestedType(typeof(trgCusRefApplicabilityView_Ins))]
	class trgCusRefApplicabilityView_Ins_Test : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var connection = Db.Connection;
			PrepareData(connection);

			var insExc = AssertExceptionThrown<SqlException>(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.CusRefApplicabilityView (ZZT_PK,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber,ZZT_DataSet,ZZT_IsSystem)
VALUES(NEWID(),'2019-1-1','2079-1-1','ADD','1','Z',CONVERT(BIT, 1))
"));
			AssertEquals("Should have thrown a correct exception with correct message", "Cannot insert a system-defined Applicability.", insExc.Message);

			AssertNoExceptionThrown(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.CusRefApplicabilityView (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber,ZZT_DataSet,ZZT_IsSystem, ZZT_SystemCreateTimeUtc, ZZT_SystemCreateUser, ZZT_SystemLastEditTimeUtc, ZZT_SystemLastEditUser)
VALUES('B5E7E1E0-AF67-4469-8961-3E68E54F6ADA','F22BDDC4-069B-440B-A4A5-6CC6FAAA6193','2019-1-1','2079-1-1','ADD','1','O',CONVERT(BIT, 0), '2019-1-1', '~E', '2020-07-03', '~F')
"));

			connection.ExecuteReader("SELECT * FROM dbo.CusRefApplicabilityView",
			reader =>
			{
				AssertEquals("Should only have a non-system item.", "B5E7E1E0-AF67-4469-8961-3E68E54F6ADA", reader["ZZT_PK"].ToString().ToUpper());
				AssertEquals("O", (string)reader["ZZT_DataSet"]);
				AssertEquals(false, (bool)reader["ZZT_IsSystem"]);
				AssertEquals(new DateTime(2019, 1, 1), (DateTime)reader["ZZT_SystemCreateTimeUtc"]);
				AssertEquals("~E", (string)reader["ZZT_SystemCreateUser"]);
				AssertEquals(new DateTime(2020, 7, 3), (DateTime)reader["ZZT_SystemLastEditTimeUtc"]);
				AssertEquals("~F", (string)reader["ZZT_SystemLastEditUser"]);
			});
		}

		public void TestInsert_MultipleData()
		{
			var connection = Db.Connection;
			PrepareData(connection);

			AssertNoExceptionThrown(() => connection.ExecuteNonQuery(@"
INSERT INTO dbo.CusRefApplicabilityView (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber,ZZT_DataSet,ZZT_IsSystem, ZZT_SystemCreateTimeUtc, ZZT_SystemCreateUser, ZZT_SystemLastEditTimeUtc, ZZT_SystemLastEditUser)
VALUES
(NEWID(),'F22BDDC4-069B-440B-A4A5-6CC6FAAA6193','2019-1-1','2079-1-1','ADD','1','O',CONVERT(BIT, 0), GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(),'F22BDDC4-069B-440B-A4A5-6CC6FAAA6193','2019-2-1','2079-1-1','ADD','1','O',CONVERT(BIT, 0), GetUtcDate(), '~BP', GetUtcDate(), '~BP')
"));
		}

		void PrepareData(DbConnection connection)
		{
			connection.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CN')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NEWID(), 'CN', 'China', NULL)

INSERT INTO RefDatabase_RefCusTariffType (
	ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping)
VALUES (
	'FF78CB63-A110-4295-9CC7-7650EEF773D2', 'TTX', 'Test Tariff Type', '', 'CN'
)

INSERT INTO dbo.CusRefTariff (
	CR1_PK,CR1_ZZI_NKTariffType,CR1_TariffCode,CR1_Description,CR1_StartDate,CR1_EndDate,CR1_ZZF_NKTaxOrFeeCode,CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser
) VALUES (
	'A5125DEF-4B3C-4C92-8B29-3C1A31E3B5E3','TTX','10000010','Test TTX Tariff','1900-01-01','2079-06-06','','CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP'
)

INSERT INTO dbo.CusRefPreference (CR8_PK,CR8_Preference,CR8_Description,CR8_RN_NKCountryCode, CR8_SystemCreateTimeUtc, CR8_SystemCreateUser, CR8_SystemLastEditTimeUtc, CR8_SystemLastEditUser)
VALUES ('74D764FF-CF88-4AE9-B22E-7BDEA6D7DEBF','PR1','Preference 1 in Cus DB','CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.CusRefRateCode (CR7_PK,CR7_RateCode,CR7_RateType,CR7_Description,CR7_RN_NKCountryCode, CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
VALUES ('6B7EE720-04EE-430C-B46F-FB2CD4C8D71B','RC','OTH','Test Rate Code','CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.CusRefRate (CR2_PK,CR2_CR1_Tariff,CR2_CR7_RateCode,CR2_CR8_Preference,CR2_StartDate,CR2_EndDate,CR2_RateFormula, CR2_SystemCreateTimeUtc, CR2_SystemCreateUser, CR2_SystemLastEditTimeUtc, CR2_SystemLastEditUser)
VALUES ('F22BDDC4-069B-440B-A4A5-6CC6FAAA6193','A5125DEF-4B3C-4C92-8B29-3C1A31E3B5E3','6B7EE720-04EE-430C-B46F-FB2CD4C8D71B','74D764FF-CF88-4AE9-B22E-7BDEA6D7DEBF','1900-01-01','2079-06-06','VFD*0.12', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode)
VALUES ('BDC232E2-5A19-4D43-B353-BB2A4C7177CF', 'FF78CB63-A110-4295-9CC7-7650EEF773D2', '10000011', 'Valid Tariff', 'CN', '1900-01-01','2079-06-06', '')

INSERT INTO RefDatabase_RefCusPreference (ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
VALUES ('D06EB447-A7BF-4F3B-BA16-360FB33C2A1A', 'PPP', 'test', 'CN')

INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula)
VALUES ('4C9D1634-332A-4296-B22C-5C275481B8DB', 'BDC232E2-5A19-4D43-B353-BB2A4C7177CF', 'D06EB447-A7BF-4F3B-BA16-360FB33C2A1A', 'CN', 'ZZRate')"
);
		}
	}
}

