using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZZ.RefDb.CusRefRateCodeView.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZZ.RefDb.CusRefRateCodeView.CusRefRateCodeView))]
	class CusRefRateCodeViewTest : DbCreateScriptTest
	{
		public void TestCusRefRateCodeView()
		{
			var createRecordsSql = @"
				DECLARE @dataGrouping UNIQUEIDENTIFIER = NEWID()
				DECLARE @rateTypePK1 UNIQUEIDENTIFIER = NEWID()
				DECLARE @rateTypePK2 UNIQUEIDENTIFIER = NEWID()

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'EUN')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (@dataGrouping, 'EUN', 'Europe Uinion', NULL)

				IF NOT EXISTS (SELECT TOP 1 1 FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'IT')
				INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) 
				VALUES (newid(), 'IT', 'Italy', @dataGrouping)

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'DTY' and ZZR_ZZZ_NKDataGrouping = 'EUN')
				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) 
				VALUES(@rateTypePK1, 'DTY', 'Duty', 1, 'EUN', 'CV')

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateType where ZZR_RateType = 'DTY' and ZZR_ZZZ_NKDataGrouping = 'IT')
				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) 
				VALUES(@rateTypePK2, 'DTY', 'Other Duty', 1, 'IT', '11')

				IF NOT EXISTS(Select 1 from RefDatabase_RefCusRateCode where ZY1_RateCode = 'ZZ1')
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES(newid(), 'ZZ1', @rateTypePK1, 'ZZ RateCode')

				INSERT INTO [dbo].[CusRefRateCode] ([CR7_PK], [CR7_RateCode], [CR7_RateType], [CR7_Description], [CR7_RN_NKCountryCode], CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
				VALUES (newid(), 'US1', 'DTY', 'User RateCode1', 'IT', '2020-07-01', '~E1', '2021-07-01', '~F1')

				INSERT INTO [dbo].[CusRefRateCode] ([CR7_PK], [CR7_RateCode], [CR7_RateType], [CR7_Description], [CR7_RN_NKCountryCode], CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
				VALUES (newid(), 'US2', 'OTH', 'User RateCode2', 'IT', '2020-07-02', '~E2', '2021-07-02', '~F2')
			";

			var connection = Db.Connection;
			connection.ExecuteNonQuery(createRecordsSql);

			AssertSelectResult("ZZ RefCusRateCode", connection, "ZZ1", new[] { "True, Z, DTY, ZZ RateCode, EUN, , ~BP, , ~BP" });
			AssertSelectResult("Cus CusRefRateCode: US1", connection, "US1", new[] { "False, O, DTY, User RateCode1, IT, 1/07/2020 12:00:00 AM, ~E1, 1/07/2021 12:00:00 AM, ~F1" });
			AssertSelectResult("Cus CusRefRateCode: US2", connection, "US2", new[] { "False, O, OTH, User RateCode2, IT, 2/07/2020 12:00:00 AM, ~E2, 2/07/2021 12:00:00 AM, ~F2" });
		}

		public void TestViewColumnsMatchUnderlyingTable()
		{
			TestDbViewHelper.AssertViewColumnsMatchUnderlyingTable(Db.Connection, "CusRefRateCodeView", "RefDatabase_RefCusRateCode", new[] {
				new TestDbViewHelper.DbColumn("ZY1_IsSystem", "bit", -1),
				new TestDbViewHelper.DbColumn("ZY1_DataSet", "varchar", 1),
				new TestDbViewHelper.DbColumn("ZY1_RateType", "varchar", 3),
				new TestDbViewHelper.DbColumn("ZY1_ZZR_RateType", "uniqueidentifier", -1),
				new TestDbViewHelper.DbColumn("ZY1_IsExport", "bit", -1),
				new TestDbViewHelper.DbColumn("ZY1_SystemCreateTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZY1_SystemCreateUser", "varchar", 3),
				new TestDbViewHelper.DbColumn("ZY1_SystemLastEditTimeUtc", "smalldatetime", -1),
				new TestDbViewHelper.DbColumn("ZY1_SystemLastEditUser", "varchar", 3),
			});
		}

		protected override bool RequiresSchemaBinding => false;

		void AssertSelectResult(string message, DbConnection connection, string rateCode, string[] expectedResults)
		{
			var selectSql = $"SELECT * FROM dbo.CusRefRateCodeView WHERE ZY1_RateCode = '{rateCode}'";
			var results = new List<string>();
			connection.ExecuteReader(selectSql, reader =>
			{
				results.Add($@"{reader["ZY1_IsSystem"]}, {reader["ZY1_DataSet"]}, {reader["ZY1_RateType"]}, {reader["ZY1_Description"]}, {reader["ZY1_ZZZ_NKDataGrouping"]}, {reader["ZY1_SystemCreateTimeUtc"]}, {reader["ZY1_SystemCreateUser"]}, {reader["ZY1_SystemLastEditTimeUtc"]}, {reader["ZY1_SystemLastEditUser"]}");
			});
			AssertContainsExactElementsInAnyOrder(message, expectedResults, results);
		}
	}
}

