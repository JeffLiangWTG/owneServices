using System;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(csfn_IsReciprocalInline))]
	class csfn_IsReciprocalInlineTest : DbCreateScriptTest
	{
		public void TestIsReciprocal()
		{
			var companyCheckList = new (Guid GC_PK, string GC_Code, string GC_RN_NKCountryCode, string GC_RX_NKLocalCurrency, int GC_IsReciprocal, int IsReciprocalExpected)[]
			{
				(Guid.NewGuid(), "US1", "US", "USD", 0, 1),
				(Guid.NewGuid(), "SG1", "SG", "SGD", 0, 1),
				(Guid.NewGuid(), "MY1", "MY", "MYR", 0, 1),
				(Guid.NewGuid(), "CN1", "CN", "CNY", 0, 1),
				(Guid.NewGuid(), "CA1", "CA", "CAD", 0, 1),
				(Guid.NewGuid(), "AE1", "AE", "AED", 0, 1),
				(Guid.NewGuid(), "KR1", "KR", "KRW", 0, 1),

				(Guid.NewGuid(), "AU1", "AU", "AUD", 1, 0),
				(Guid.NewGuid(), "NZ1", "NZ", "NZD", 1, 0),
				(Guid.NewGuid(), "GB1", "GB", "GBP", 1, 0),
				(Guid.NewGuid(), "ZA1", "ZA", "ZAR", 1, 0),
				(Guid.NewGuid(), "IT1", "IT", "EUR", 1, 0),
				(Guid.NewGuid(), "DE1", "DE", "DKK", 1, 0),
				(Guid.NewGuid(), "BW1", "BW", "BWP", 1, 0),
				(Guid.NewGuid(), "LS1", "LS", "LSL", 1, 0),
				(Guid.NewGuid(), "NA1", "NA", "NAD", 1, 0),
				(Guid.NewGuid(), "SZ1", "SZ", "SZL", 1, 0),

				(Guid.NewGuid(), "HK1", "HK", "HKD", 1, 1),
				(Guid.NewGuid(), "HK2", "HK", "HKD", 0, 0),
				(Guid.NewGuid(), "NA2", "NA", "NAD", 0, 1),
				(Guid.NewGuid(), "NA3", "NA", "NAD", 0, 0),
			};

			var sqlTextBuilder = new StringBuilder($@"DELETE dbo.GlbCompany WHERE GC_Code IN ('{companyCheckList[0]}'");
			var sqlTextInsertBuilder = new StringBuilder($@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_IsReciprocal, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{companyCheckList[0].GC_PK}', '{companyCheckList[0].GC_Code}', 'Company', {companyCheckList[0].GC_IsReciprocal}, '{companyCheckList[0].GC_RN_NKCountryCode}', '{companyCheckList[0].GC_RX_NKLocalCurrency}')");
			var sqlTextCheckBuilder = new StringBuilder($@"SELECT '{companyCheckList[0].GC_Code}' AS companyCode, {companyCheckList[0].IsReciprocalExpected} AS IsReciprocalExpected, IsReciprocal FROM csfn_IsReciprocalInline('{companyCheckList[0].GC_PK}')");

			for (int i = 1; i < companyCheckList.Length; i++)
			{
				var companyCheck = companyCheckList[i];
				sqlTextBuilder.Append($@", '{companyCheck.GC_Code}'");
				sqlTextInsertBuilder.Append($@", ('{companyCheck.GC_PK}', '{companyCheck.GC_Code}', 'Company', {companyCheck.GC_IsReciprocal}, '{companyCheck.GC_RN_NKCountryCode}', '{companyCheckList[0].GC_RX_NKLocalCurrency}')");
				sqlTextCheckBuilder.Append($@" UNION ALL SELECT '{companyCheck.GC_Code}' AS companyCode, {companyCheck.IsReciprocalExpected} AS IsReciprocalExpected, IsReciprocal FROM csfn_IsReciprocalInline('{companyCheck.GC_PK}')");
			}

			sqlTextBuilder.Append(")");
			sqlTextBuilder.AppendLine();
			sqlTextBuilder.Append(sqlTextInsertBuilder);
			sqlTextBuilder.AppendLine();
			sqlTextBuilder.AppendLine($@"INSERT INTO dbo.ZZRefCusConfiguration(ZZC_PK, ZZC_GC, ZZC_IsReciprocalExchangeRate) VALUES (NEWID(), '{companyCheckList.Single(x => x.GC_Code == "NA2").GC_PK}', 'Y');");
			sqlTextBuilder.AppendLine($@"INSERT INTO dbo.ZZRefCusConfiguration(ZZC_PK, ZZC_GC, ZZC_IsReciprocalExchangeRate) VALUES (NEWID(), '{companyCheckList.Single(x => x.GC_Code == "NA3").GC_PK}', 'N');");
			sqlTextBuilder.AppendLine();
			sqlTextBuilder.Append(sqlTextCheckBuilder);

			using (var cmd = Db.Connection.Command(sqlTextBuilder.ToString()))
			using (var reader = cmd.ExecuteReader())
			{
				CombineAssertions(() =>
				{
					var checkCount = 0;
					while (reader.Read())
					{
						var companyCode = reader.GetString(0);
						var isReciprocalExpected = reader.GetInt32(1);
						var isReciprocal = reader.GetInt32(2);
						AssertEquals($"IsReciprocal for {companyCode}", isReciprocalExpected, isReciprocal);
						checkCount++;
					}

					AssertEquals("should check all list items", companyCheckList.Length, checkCount);
				});
			}
		}
	}
}
