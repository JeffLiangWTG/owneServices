using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.ZZRefTariffViewCombined;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.ZZRefTariffViewCombined
{
	[TestedType(typeof(TariffView))]
	class TariffView_Test : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestViewColumns()
		{
			var connection = Db.Connection;
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(
				connection,
				"TariffView", "RefDatabase_TariffView",
				new[]
				{
					new TestDbViewHelper.DbColumn("ZZ1_IsSystem", "bit", -1),
					new TestDbViewHelper.DbColumn("ZZ1_ZZI_NKTariffType","varchar", 5),
					new TestDbViewHelper.DbColumn("ZZ1_DataSet", "varchar", 1),
					new TestDbViewHelper.DbColumn("ZZ1_CRT_NKTariffVersion", "varchar", 6),
					new TestDbViewHelper.DbColumn("ZZ1_SystemCreateTimeUtc", "smalldatetime", -1),
					new TestDbViewHelper.DbColumn("ZZ1_SystemCreateUser", "varchar", 3),
					new TestDbViewHelper.DbColumn("ZZ1_SystemLastEditTimeUtc", "smalldatetime", -1),
					new TestDbViewHelper.DbColumn("ZZ1_SystemLastEditUser", "varchar", 3),
				}
			);
		}

		public void TestLoad()
		{
			var connection = Db.Connection;
			connection.ExecuteNonQuery(@"

				DECLARE @TariffTypePK UNIQUEIDENTIFIER = CAST('E808FCBD-C1CF-470F-A87C-1F4ED2DFBA1C' AS UNIQUEIDENTIFIER)
				DECLARE @tariffPK1 UNIQUEIDENTIFIER = CAST('C1070EC7-F2E2-407D-B298-D6A3D7083D9B' AS UNIQUEIDENTIFIER)
				DECLARE @tariffPK2 UNIQUEIDENTIFIER = CAST('0B55193D-ACEA-4EB1-AE7A-383725F4FCD9' AS UNIQUEIDENTIFIER)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (NEWID(), 'CN', 'China', NULL)

				INSERT RefDatabase_RefCusTariffType(ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) 
				VALUES (@TariffTypePK, 'TTX', 'Test Tariff Type', 'CN')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_PublishedDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES (@tariffPK1, @TariffTypePK, '10000010', 'Tariff in RefDbEntZZ', 'CN', '2020-01-01', '2079-06-06', '2020-01-01', '')

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_CRT_NKTariffVersion, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES (@tariffPK2, 'TTX', '10000011', 'Tariff in CusDB', '2020-07-02', '2079-07-02', 'TFF', 'CN', '000001', '2020-07-02', '~E', '2020-07-03', '~F')

				INSERT INTO dbo.CusRefTariffVersion (CRT_PK, CRT_Version, CRT_Description, CRT_EffectiveDate, CRT_RN_NKCountryCode, CRT_SystemCreateTimeUtc, CRT_SystemCreateUser, CRT_SystemLastEditTimeUtc, CRT_SystemLastEditUser)
				VALUES (NEWID(), '000001', 'VERSION DESC', '2020-07-02', 'CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			");

			var expectedTariffs = new List<(string pk, string typePk, string nkType, string tariffCode, string description, string taxOrFeeCode, string grouping, DateTime startDate, DateTime endDate, string dataSet, bool isSystem, string tariffVersion,
				DateTime systemCreateDate, string systemCreateUser, DateTime systemLastEditDate, string systemLastEditUser)>
			{
				("C1070EC7-F2E2-407D-B298-D6A3D7083D9B", "E808FCBD-C1CF-470F-A87C-1F4ED2DFBA1C","TTX", "10000010", "Tariff in RefDbEntZZ","","CN",new DateTime(2020,1,1), new DateTime(2079,6,6), "Z", true, "", new DateTime(2020, 1, 1), "~BP", new DateTime(2020, 1, 1), "~BP"),
				("0B55193D-ACEA-4EB1-AE7A-383725F4FCD9", "","TTX", "10000011", "Tariff in CusDB","TFF","CN",new DateTime(2020,7,2), new DateTime(2079,7,2),"O", false, "000001", new DateTime(2020, 7, 2), "~E", new DateTime(2020, 7, 3), "~F")
			};

			connection.ExecuteReader("SELECT * FROM dbo.TariffView",
				reader =>
				{
					AssertCollectionContains((
						reader["ZZ1_PK"].ToString().ToUpper(),
						reader["ZZ1_ZZI_TariffType"].ToString().ToUpper(),
						(string)reader["ZZ1_ZZI_NKTariffType"],
						(string)reader["ZZ1_TariffCode"],
						(string)reader["ZZ1_Description"],

						(string)reader["ZZ1_ZZF_NKTaxOrFeeCode"],
						(string)reader["ZZ1_ZZZ_NKDataGrouping"],
						(DateTime)reader["ZZ1_StartDate"],
						(DateTime)reader["ZZ1_EndDate"],
						(string)reader["ZZ1_DataSet"],

						(bool)reader["ZZ1_IsSystem"],
						(string)reader["ZZ1_CRT_NKTariffVersion"],

						(DateTime)reader["ZZ1_SystemCreateTimeUtc"],
						(string)reader["ZZ1_SystemCreateUser"],
						(DateTime)reader["ZZ1_SystemLastEditTimeUtc"],
						(string)reader["ZZ1_SystemLastEditUser"]
						),
					expectedTariffs);
				});
		}
	}
}

