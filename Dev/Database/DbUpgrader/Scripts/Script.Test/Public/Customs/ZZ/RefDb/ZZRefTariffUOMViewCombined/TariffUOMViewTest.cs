using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefTariffUOMViewCombined;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefTariffUOMViewCombined
{
	[TestedType(typeof(TariffUOMView))]
	class TariffUOMView_Test : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestViewColumns()
		{
			var connection = Db.Connection;
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(
				connection,
				"TariffUOMView", "RefDatabase_TariffUOMView",
				new[]
				{
					new TestDbViewHelper.DbColumn("ZZ8_IsSystem", "bit", -1),
					new TestDbViewHelper.DbColumn("ZZ8_UOM", "nvarchar", 10),
					new TestDbViewHelper.DbColumn("ZZ8_UOM", "varchar", 10),
					new TestDbViewHelper.DbColumn("ZZ8_DataSet", "varchar", 1),
					new TestDbViewHelper.DbColumn("ZZ8_SystemCreateTimeUtc", "smalldatetime", -1),
					new TestDbViewHelper.DbColumn("ZZ8_SystemCreateUser", "varchar", 3),
					new TestDbViewHelper.DbColumn("ZZ8_SystemLastEditTimeUtc", "smalldatetime", -1),
					new TestDbViewHelper.DbColumn("ZZ8_SystemLastEditUser", "varchar", 3),
				}
			);
		}

		public void TestTariffUOMView()
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
				VALUES ('AD31CEC7-8A1A-44D7-BE99-FB6239984C8C', @tariffPK1, 'CU1', 'KG', 'CN')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES (@tariffPK2, 'TTX', '10000011', 'Tariff in CusDB', '2020-07-02', '2079-07-02', 'TFF', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTariffUom (CR3_PK,CR3_CR1_Tariff,CR3_Type,CR3_UnitOfMeasure, CR3_SystemCreateTimeUtc, CR3_SystemCreateUser, CR3_SystemLastEditTimeUtc, CR3_SystemLastEditUser)
				VALUES ('34AE7400-B363-4CC4-A7ED-E6070FBDD7E2', @tariffPK2, 'CU2', 'T', '2020-07-02', '~E', '2020-07-03', '~F')
			");

			var tariffUOMs = new List<(string pk, string type, string uom, string dataSet, bool isSystem, object systemCreateDate, string systemCreateUser, object systemLastEditDate, string systemLastEditUser)>
			{
				("AD31CEC7-8A1A-44D7-BE99-FB6239984C8C", "CU1","KG","Z",true, DBNull.Value, "~BP", DBNull.Value, "~BP"),
				("34AE7400-B363-4CC4-A7ED-E6070FBDD7E2", "CU2","T","O",false, new DateTime(2020, 7, 2), "~E", new DateTime(2020, 7, 3), "~F")
			};

			connection.ExecuteReader("SELECT * FROM dbo.TariffUOMView",
				reader =>
				{
					AssertCollectionContains((
						reader["ZZ8_PK"].ToString().ToUpper(),
						(string)reader["ZZ8_Type"],
						(string)reader["ZZ8_UOM"],
						(string)reader["ZZ8_DataSet"],
						(bool)reader["ZZ8_IsSystem"],

						reader["ZZ8_SystemCreateTimeUtc"],
						(string)reader["ZZ8_SystemCreateUser"],
						reader["ZZ8_SystemLastEditTimeUtc"],
						(string)reader["ZZ8_SystemLastEditUser"]
						),
						tariffUOMs);
				});
		}
	}
}

