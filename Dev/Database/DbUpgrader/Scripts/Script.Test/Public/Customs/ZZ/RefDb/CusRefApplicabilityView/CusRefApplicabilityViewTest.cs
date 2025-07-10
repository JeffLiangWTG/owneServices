using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefApplicabilityView
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefApplicabilityView.CusRefApplicabilityView))]
	class CusRefApplicabilityView_Test : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestViewColumns()
		{
			var connection = Db.Connection;
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(
				connection,
				"CusRefApplicabilityView", "RefDatabase_RefCusApplicability",
				new[]
				{
					new TestDbViewHelper.DbColumn("ZZT_IsSystem", "bit", -1),
					new TestDbViewHelper.DbColumn("ZZT_DataSet", "varchar", 1),
					new TestDbViewHelper.DbColumn("ZZT_SystemCreateTimeUtc", "smalldatetime", -1),
					new TestDbViewHelper.DbColumn("ZZT_SystemCreateUser", "varchar", 3),
					new TestDbViewHelper.DbColumn("ZZT_SystemLastEditTimeUtc", "smalldatetime", -1),
					new TestDbViewHelper.DbColumn("ZZT_SystemLastEditUser", "varchar", 3),
				}
			);
		}

		public void TestLoad()
		{
			var connection = Db.Connection;
			connection.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'CN')
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
VALUES (NewID(), 'CN', 'China', NULL)

INSERT INTO RefDatabase_RefCusTariffType (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping)
VALUES ('FF78CB63-A110-4295-9CC7-7650EEF773D2', 'TTX', 'Test Tariff Type', '', 'CN')

INSERT INTO RefDatabase_RefCusTradeGroup(ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
VALUES('979C37D5-76E4-4261-BFF6-E8BDCD4ADB7A', 'TESTTRADEGROUP', 'Test trade group', '1900-01-01','2079-06-06', 'CN')

INSERT INTO RefDatabase_RefCusTradeGroup(ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
VALUES('3AE9CD22-CF65-4F8F-A461-26AD6EFA86A1', 'SECONDTESTTRADEGROUP', 'Second test trade group', '1900-01-01','2079-06-06', 'CN')

INSERT INTO dbo.CusRefTariff (CR1_PK,CR1_ZZI_NKTariffType,CR1_TariffCode,CR1_Description,CR1_StartDate,CR1_EndDate,CR1_ZZF_NKTaxOrFeeCode,CR1_RN_NKCountryCode, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
VALUES ('A5125DEF-4B3C-4C92-8B29-3C1A31E3B5E3','TTX','10000010','Test TTX Tariff','1900-01-01','2079-06-06','','CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.CusRefPreference (CR8_PK,CR8_Preference,CR8_Description,CR8_RN_NKCountryCode, CR8_SystemCreateTimeUtc, CR8_SystemCreateUser, CR8_SystemLastEditTimeUtc, CR8_SystemLastEditUser)
VALUES ('74D764FF-CF88-4AE9-B22E-7BDEA6D7DEBF','PR1','Preference 1 in Cus DB','CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.CusRefRateCode (CR7_PK,CR7_RateCode,CR7_RateType,CR7_Description,CR7_RN_NKCountryCode, CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
VALUES ('6B7EE720-04EE-430C-B46F-FB2CD4C8D71B','RC','OTH','Test Rate Code','CN', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.CusRefRate (CR2_PK,CR2_CR1_Tariff,CR2_CR7_RateCode,CR2_CR8_Preference,CR2_StartDate,CR2_EndDate,CR2_RateFormula, CR2_SystemCreateTimeUtc, CR2_SystemCreateUser, CR2_SystemLastEditTimeUtc, CR2_SystemLastEditUser)
VALUES ('F22BDDC4-069B-440B-A4A5-6CC6FAAA6193','A5125DEF-4B3C-4C92-8B29-3C1A31E3B5E3','6B7EE720-04EE-430C-B46F-FB2CD4C8D71B','74D764FF-CF88-4AE9-B22E-7BDEA6D7DEBF','1900-01-01','2079-06-06','VFD*0.12', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.CusRefApplicability (CR4_PK,CR4_CR2_Rate,CR4_CR9_TradeGroup,CR4_StartDate,CR4_EndDate,CR4_OrderNumber, CR4_SystemCreateTimeUtc, CR4_SystemCreateUser, CR4_SystemLastEditTimeUtc, CR4_SystemLastEditUser)
VALUES ('C9B3A235-E397-486B-9B18-DD705176D1B0','F22BDDC4-069B-440B-A4A5-6CC6FAAA6193',NULL,'1900-01-01','2079-06-06','1', '2020-07-02', '~E', '2020-07-03', '~F')

INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode)
VALUES ('BDC232E2-5A19-4D43-B353-BB2A4C7177CF', 'FF78CB63-A110-4295-9CC7-7650EEF773D2', '10000011', 'Valid Tariff', 'CN', '1900-01-01','2079-06-06', '')

INSERT INTO RefDatabase_RefCusPreference (ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
VALUES ('D06EB447-A7BF-4F3B-BA16-360FB33C2A1A', 'PPP', 'test', 'CN')

INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula)
VALUES ('4C9D1634-332A-4296-B22C-5C275481B8DB', 'BDC232E2-5A19-4D43-B353-BB2A4C7177CF', 'D06EB447-A7BF-4F3B-BA16-360FB33C2A1A', 'CN', 'ZZRate')

INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup)
VALUES ('CEB001F0-2BB6-4A27-AFB8-88237E7519BE', '4C9D1634-332A-4296-B22C-5C275481B8DB', '1900-01-01', '2079-06-06', '979C37D5-76E4-4261-BFF6-E8BDCD4ADB7A', 'ADD', '2','3AE9CD22-CF65-4F8F-A461-26AD6EFA86A1')"
			);

			var expectedResults = new List<(
				string pk, string zz2_rate, string zx1_conditions, DateTime startDate, DateTime endDate,
				object zza_tradeGroup, string additionalCode, string orderNumber, object zy2_additionalCode, string dataSet, bool isSystem, object zza_secondTradeGroup,
				object systemCreateDate, string systemCreateUser, object systemLastEditDate, string systemLastEditUser
			)>
			{
				("C9B3A235-E397-486B-9B18-DD705176D1B0","F22BDDC4-069B-440B-A4A5-6CC6FAAA6193","",new DateTime(1900,1,1),new DateTime(2079,6,6),"","","1",DBNull.Value,"O",false,"", new DateTime(2020, 7, 2), "~E", new DateTime(2020, 7, 3), "~F"),
				("CEB001F0-2BB6-4A27-AFB8-88237E7519BE","4C9D1634-332A-4296-B22C-5C275481B8DB","",new DateTime(1900,1,1),new DateTime(2079,6,6),"979C37D5-76E4-4261-BFF6-E8BDCD4ADB7A","ADD","2",DBNull.Value,"Z",true,"3AE9CD22-CF65-4F8F-A461-26AD6EFA86A1", DBNull.Value, "~BP", DBNull.Value, "~BP")
			};

			connection.ExecuteReader("SELECT * FROM dbo.CusRefApplicabilityView", reader =>
				AssertCollectionContains((
					reader["ZZT_PK"].ToString().ToUpper(),
					reader["ZZT_ZZ2_Rate"].ToString().ToUpper(),
					reader["ZZT_ZX1_Conditions"].ToString().ToUpper(),
					(DateTime)reader["ZZT_StartDate"],
					(DateTime)reader["ZZT_EndDate"],
					(object)reader["ZZT_ZZA_TradeGroup"].ToString().ToUpper(),
					(string)reader["ZZT_AdditionalCode"],
					(string)reader["ZZT_OrderNumber"],
					reader["ZZT_ZY2_AdditionalCode"],
					(string)reader["ZZT_DataSet"],
					(bool)reader["ZZT_IsSystem"],
					(object)reader["ZZT_ZZA_SecondTradeGroup"].ToString().ToUpper(),

					reader["ZZT_SystemCreateTimeUtc"],
					(string)reader["ZZT_SystemCreateUser"],
					reader["ZZT_SystemLastEditTimeUtc"],
					(string)reader["ZZT_SystemLastEditUser"]
				), expectedResults)
			);
		}
	}
}

