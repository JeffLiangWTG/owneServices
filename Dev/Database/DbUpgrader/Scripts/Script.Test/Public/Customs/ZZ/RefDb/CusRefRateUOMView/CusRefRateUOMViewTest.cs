using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefRateUOMView.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefRateUOMView.CusRefRateUOMView))]
	class CusRefRateUOMViewTest : DbCreateScriptTest
	{
		public void TestCusRefRateUOMView()
		{
			var createRecordsSql = @"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID()
				DECLARE @preferencePK UNIQUEIDENTIFIER = NEWID()
				DECLARE @tariffPK1 UNIQUEIDENTIFIER = NEWID()
				DECLARE @tariffPK2 UNIQUEIDENTIFIER = NEWID()
				DECLARE @rateCodePK UNIQUEIDENTIFIER = NEWID()
				DECLARE @ratePK1 UNIQUEIDENTIFIER = NEWID()
				DECLARE @ratePK2 UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'ZA', 'South Africa', NULL)

				INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
				VALUES (@TariffTypePK, 'TTT', 'test', 'ZA')

				INSERT INTO RefDatabase_RefCusPreference (ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
				VALUES (@preferencePK, 'PPP', 'test', 'ZA')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@tariffPK1, @TariffTypePK, 'TEST', 'Valid Tariff', 'ZA', '2020-01-01 00:00:00.000', '2079-06-06 23:59:00.000', '')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula)
				VALUES (@ratePK1, @tariffPK1, @preferencePK, 'ZA', 'ZZRate')

				INSERT INTO RefDatabase_RefCusRateUOM (ZXG_PK, ZXG_ZZ2_Rate, ZXG_UOM)
				VALUES (NEWID(), @ratePK1, 'ZZUOM')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES(@tariffPK2, 'TTT', 'test', 'test', '2020-07-02', '2079-07-02', 'TFF', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefRateCode (CR7_PK, CR7_RateCode, CR7_RateType, CR7_Description, CR7_RN_NKCountryCode, CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
				VALUES (@rateCodePK, 'test', 'ADD', 'test', 'ZA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefRate (CR2_PK, CR2_CR1_TARIFF, CR2_CR7_RateCode, CR2_RateFormula, CR2_StartDate, CR2_SystemCreateTimeUtc, CR2_SystemCreateUser, CR2_SystemLastEditTimeUtc, CR2_SystemLastEditUser)
				VALUES (@ratePK2, @tariffPK2, @rateCodePK, 'CusRate', '2020-01-01 00:00:00.000', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefRateUom (CR5_PK, CR5_CR2_Rate, CR5_UnitOfMeasure, CR5_SystemCreateTimeUtc, CR5_SystemCreateUser, CR5_SystemLastEditTimeUtc, CR5_SystemLastEditUser)
				VALUES (NEWID(), @ratePK2, 'CusUOM', '2020-07-01', '~E1', '2021-07-01', '~F1')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);
			AssertSelectResult("ZZ RefCusRateUOM", connection, "ZZUOM", new[] { "True, Z, , ~BP, , ~BP" });
			AssertSelectResult("Cus CusRefRateUom", connection, "CusUOM", new[] { "False, O, 1/07/2020 12:00:00 AM, ~E1, 1/07/2021 12:00:00 AM, ~F1" });
		}

		public void TestViewColumnsMatchUnderlyingTable()
		{
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(Db.Connection, "CusRefRateUOMView", "RefDatabase_RefCusRateUOM", new[] {
				new TestDbViewHelper.DbColumn("ZXG_IsSystem", "bit", -1),
				new TestDbViewHelper.DbColumn("ZXG_DataSet", "varchar", 1),
				new TestDbViewHelper.DbColumn("ZXG_SystemCreateTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZXG_SystemCreateUser", "varchar", 3),
				new TestDbViewHelper.DbColumn("ZXG_SystemLastEditTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZXG_SystemLastEditUser", "varchar", 3),
			});
		}

		protected override bool RequiresSchemaBinding => false;

		void AssertSelectResult(string message, DbConnection connection, string uom, string[] expectedResults)
		{
			var selectSql = $"SELECT * FROM dbo.CusRefRateUOMView WHERE ZXG_UOM = '{uom}'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZXG_IsSystem"]}, {reader["ZXG_DataSet"]}, {reader["ZXG_SystemCreateTimeUtc"]}, {reader["ZXG_SystemCreateUser"]}, {reader["ZXG_SystemLastEditTimeUtc"]}, {reader["ZXG_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(message, expectedResults, results);
		}
	}
}

