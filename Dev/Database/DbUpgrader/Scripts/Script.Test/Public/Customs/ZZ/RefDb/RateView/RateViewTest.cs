using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.RateView.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.RateView.RateView))]
	class RateViewTest : DbCreateScriptTest
	{
		public void TestRateView()
		{
			var tariffPK1 = Guid.NewGuid();
			var tariffPK2 = Guid.NewGuid();
			var ratePK1 = Guid.NewGuid();
			var ratePK2 = Guid.NewGuid();
			var rateCodePK1 = Guid.NewGuid();
			var rateCodePK2 = Guid.NewGuid();
			var preferencePK1 = Guid.NewGuid();
			var preferencePK2 = Guid.NewGuid();

			var createRecordsSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID()
				DECLARE @rateTypePK UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'ZA', 'South Africa', NULL)

				INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
				VALUES (@TariffTypePK, 'TTT', 'test', 'ZA')

				INSERT INTO RefDatabase_RefCusPreference (ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
				VALUES ('{preferencePK1}', 'PPP', 'test', 'ZA')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES ('{tariffPK1}', @TariffTypePK, 'TEST', 'Valid Tariff', 'ZA', '2020-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '')

				INSERT INTO RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula)
				VALUES(@rateTypePK, 'DTY', 'Ref Rate Type', 1, 'ZA', '')

				INSERT INTO RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) 
				VALUES ('{rateCodePK1}', 'RC1', @rateTypePK, 'RC1 Description')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate, ZZ2_ZY1_RateCode, ZZ2_RateFormulaDerivedFrom, ZZ2_RX_NKCurrencyOverride)
				VALUES ('{ratePK1}', '{tariffPK1}', '{preferencePK1}', 'ZA', 'ZZRate', '2020-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '{rateCodePK1}', 'FF', 'NAD')

				INSERT INTO [dbo].[CusRefTariff] ([CR1_PK], [CR1_ZZI_NKTariffType], [CR1_TariffCode], [CR1_Description], [CR1_StartDate], [CR1_EndDate], [CR1_ZZF_NKTaxOrFeeCode], [CR1_RN_NKCountryCode], CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES('{tariffPK2}', 'test', 'test', 'test', '2020-07-02', '2079-07-02', 'TFF', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO [dbo].[CusRefRateCode] ([CR7_PK], [CR7_RateCode], [CR7_RateType], [CR7_Description], [CR7_RN_NKCountryCode], CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
				VALUES ('{rateCodePK2}', 'test', 'ADD', 'RC2', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefPreference(CR8_PK,CR8_Preference,CR8_Description,CR8_RN_NKCountryCode, CR8_SystemCreateTimeUtc, CR8_SystemCreateUser, CR8_SystemLastEditTimeUtc, CR8_SystemLastEditUser)
				VALUES('{preferencePK2}', 'PR2', 'Cus Preference2', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefRate (CR2_PK, CR2_CR1_TARIFF, CR2_CR7_RateCode, CR2_RateFormula, CR2_StartDate, CR2_CR8_Preference, CR2_SystemCreateTimeUtc, CR2_SystemCreateUser, CR2_SystemLastEditTimeUtc, CR2_SystemLastEditUser)
				VALUES ('{ratePK2}', '{tariffPK2}', '{rateCodePK2}', 'CusRate', '2020-07-02', '{preferencePK2}', '2020-07-02', '~E', '2021-07-02', '~F')
";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			AssertSelectResult("ZZ Rate", connection, ratePK1, new[] { $@"{tariffPK1},
ZZ1,
1/01/2020 12:00:00 AM,
6/06/2079 11:59:00 PM,
{rateCodePK1},
ZZRate,
{preferencePK1},
FF,
ZA,
NAD,
Z,
True,
,
~BP,
,
~BP" });

			AssertSelectResult("Cus rRate", connection, ratePK2, new[] { $@"{tariffPK2},
CR2,
2/07/2020 12:00:00 AM,
31/12/9999 12:00:00 AM,
{rateCodePK2},
CusRate,
{preferencePK2},
,
ZA,
,
O,
False,
2/07/2020 12:00:00 AM,
~E,
2/07/2021 12:00:00 AM,
~F" });
		}

		void AssertSelectResult(string message, DbConnection connection, Guid ratePK, string[] expectedResults)
		{
			var selectSql = $"SELECT * FROM dbo.RateView WHERE ZZ2_PK = '{ratePK}'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZZ2_ZZ1_ParentTariffOrNationalCode"]},
{reader["ZZ2_ParentTableType"]},
{reader["ZZ2_StartDate"]},
{reader["ZZ2_EndDate"]},
{reader["ZZ2_ZY1_RateCode"]},
{reader["ZZ2_RateFormula"]},
{reader["ZZ2_ZZS_Preference"]},
{reader["ZZ2_RateFormulaDerivedFrom"]},
{reader["ZZ2_ZZZ_NKDataGrouping"]},
{reader["ZZ2_RX_NKCurrencyOverride"]},
{reader["ZZ2_DataSet"]},
{reader["ZZ2_IsSystem"]},
{reader["ZZ2_SystemCreateTimeUtc"]},
{reader["ZZ2_SystemCreateUser"]},
{reader["ZZ2_SystemLastEditTimeUtc"]},
{reader["ZZ2_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(message, expectedResults, results);
		}

		public void TestViewColumnsMatchUnderlyingTable()
		{
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(Db.Connection, "RateView", "RefDatabase_RateView", new[] {
				new TestDbViewHelper.DbColumn("ZZ2_IsSystem", "bit", -1),
				new TestDbViewHelper.DbColumn("ZZ2_DataSet", "varchar", 1),
				new TestDbViewHelper.DbColumn("ZZ2_SystemCreateTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZZ2_SystemCreateUser", "varchar", 3),
				new TestDbViewHelper.DbColumn("ZZ2_SystemLastEditTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZZ2_SystemLastEditUser", "varchar", 3),
			});
		}

		protected override bool RequiresSchemaBinding => false;
	}
}

