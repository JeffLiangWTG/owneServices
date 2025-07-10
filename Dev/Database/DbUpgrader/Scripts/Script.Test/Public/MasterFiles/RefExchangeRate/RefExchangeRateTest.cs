using System;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.RefExchangeRate
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.RefExchangeRate.RefExchangeRate))]
	class RefExchangeRateTest : DbCreateScriptTest
	{
		public void TestColumnTypes()
		{
			var numberOfOriginalColumns = TestConnection.ExecuteScalar(@"SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ZZRefExchangeRate')  AND [name] NOT IN ('RE_SystemCreateTimeUtc','RE_SystemCreateUser','RE_SystemLastEditTimeUtc','RE_SystemLastEditUser')");
			AssertEquals(numberOfOriginalColumns, TestConnection.ExecuteScalar(@"
SELECT COUNT(*) FROM sys.columns c1 
JOIN sys.columns c2 ON c1.name = c2.name AND (c1.system_type_id = c2.system_type_id OR (c1.system_type_id IN (167, 175) AND c2.system_type_id IN (167, 175))) -- char/varchar
WHERE c1.object_id = OBJECT_ID('dbo.ZZRefExchangeRate') AND c2.object_id = OBJECT_ID('dbo.RefExchangeRate')
"));
		}

		public void TestIsSystemColumnType()
		{
			AssertEquals(0, TestConnection.ExecuteScalar(@"
 SELECT COUNT(*) FROM sys.columns c JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.name LIKE '%IsSystem' AND t.name <> 'bit' AND c.object_id = OBJECT_ID('dbo.RefExchangeRate')
"));
		}

		public void TestUserExchangeRate_CombineData()
		{
			TestConnection.ExecuteNonQuery(@"DELETE FROM dbo.ZZRefExchangeRate");
			TestConnection.ExecuteNonQuery(@"DELETE FROM RefDatabase_RefExchangeRateZZ");

			var zaCompany1 = TestDataCreator.CreateCompany("ZA1", "ZA", "ZAR");
			var zaCompany2 = TestDataCreator.CreateCompany("ZA2", "ZA", "ZAR");
			var bwCompany = TestDataCreator.CreateCompany("BW1", "BW", "BWP");
			var usCompany = TestDataCreator.CreateCompany("US1", "US", "USD");

			var clientPK = TestDataCreator.CreateOrganisation("OH1", "Client");

			var exchangeRate1 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "SEL", string.Empty, new DateTime(2016, 1, 1), new DateTime(2016, 1, 31), 0.51m, Guid.Empty);
			var exchangeRate2 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "SEL", string.Empty, new DateTime(2016, 2, 10), new DateTime(2016, 2, 28), 0.52m, Guid.Empty);
			var exchangeRate3 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUS", string.Empty, new DateTime(2016, 1, 1), new DateTime(2016, 1, 31), 0.53m, Guid.Empty);
			var exchangeRate4 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUS", string.Empty, new DateTime(2016, 2, 1), new DateTime(2016, 2, 28), 0.54m, Guid.Empty);
			var exchangeRate5 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUS", string.Empty, new DateTime(2016, 3, 1), new DateTime(2016, 3, 31), 0.55m, Guid.Empty);
			var exchangeRate6 = InsertZZRefExchangeRate(bwCompany, "BW", "CAD", "SEL", string.Empty, new DateTime(2016, 1, 1), new DateTime(2016, 1, 31), 0.56m, clientPK);
			var exchangeRate7 = InsertZZRefExchangeRate(bwCompany, "BW", "CAD", "CUS", string.Empty, new DateTime(2016, 1, 1), new DateTime(2016, 1, 31), 0.57m, Guid.Empty);
			var exchangeRate8 = InsertZZRefExchangeRate(usCompany, "US", "CAD", "SEL", string.Empty, new DateTime(2016, 1, 1), new DateTime(2016, 1, 31), 0.58m, Guid.Empty);
			var exchangeRate9 = InsertZZRefExchangeRate(usCompany, "US", "CAD", "SEL", string.Empty, new DateTime(2016, 2, 1), new DateTime(2016, 2, 28), 0.59m, Guid.Empty);

			var exchangeRateZZ5 = InsertRefExchangeRateZZ("ZA", "CAD", "CUS", string.Empty, new DateTime(2015, 12, 1), new DateTime(2015, 12, 31), 0.65m);
			var exchangeRateZZ6 = InsertRefExchangeRateZZ("ZA", "CAD", "CUS", string.Empty, new DateTime(2016, 1, 1), new DateTime(2016, 1, 31), 0.66m);
			var exchangeRateZZ7 = InsertRefExchangeRateZZ("ZA", "CAD", "CUS", string.Empty, new DateTime(2016, 2, 1), new DateTime(2016, 2, 28), 0.67m);
			var exchangeRateZZ8 = InsertRefExchangeRateZZ("ZA", "CAD", "CUE", string.Empty, new DateTime(2015, 12, 1), new DateTime(2016, 2, 28), 0.67m);
			var exchangeRate10 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUE", string.Empty, new DateTime(2016, 1, 1), new DateTime(2016, 1, 31), 0.53m, Guid.Empty);
			var exchangeRate11 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUE", string.Empty, new DateTime(2016, 2, 1), new DateTime(2016, 2, 28), 0.54m, Guid.Empty);
			var exchangeRate12 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUE", string.Empty, new DateTime(2016, 3, 1), new DateTime(2016, 3, 31), 0.55m, Guid.Empty);

			var exchangeRate13 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "IAT", string.Empty, new DateTime(2016, 3, 1), new DateTime(2016, 3, 31), 0.55m, Guid.Empty);
			var exchangeRate14 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "IAT", string.Empty, new DateTime(2016, 2, 1), new DateTime(2016, 2, 28), 0.55m, Guid.Empty);
			var exchangeRateZZ9 = InsertRefExchangeRateZZ("ZA", "CAD", "IAT", string.Empty, new DateTime(2015, 12, 1), new DateTime(2015, 12, 31), 0.65m);
			var exchangeRateZZ10 = InsertRefExchangeRateZZ("ZA", "CAD", "IAT", string.Empty, new DateTime(2016, 2, 15), new DateTime(2016, 2, 28), 0.65m);

			AssertRefExchangeRate(exchangeRate1, true);
			AssertRefExchangeRate(exchangeRate2, true);
			AssertRefExchangeRate(exchangeRate3, false);
			AssertRefExchangeRate(exchangeRate4, false);
			AssertRefExchangeRate(exchangeRate5, true);
			AssertRefExchangeRate(exchangeRate6, true);
			AssertRefExchangeRate(exchangeRate7, true);
			AssertRefExchangeRate(exchangeRate8, true);
			AssertRefExchangeRate(exchangeRate9, true);
			AssertRefExchangeRate(exchangeRate10, false);
			AssertRefExchangeRate(exchangeRate11, false);
			AssertRefExchangeRate(exchangeRate12, true);

			exchangeRateZZ5.CompanyPK = zaCompany1;
			exchangeRateZZ6.CompanyPK = zaCompany1;
			exchangeRateZZ7.CompanyPK = zaCompany1;
			exchangeRateZZ8.CompanyPK = zaCompany1;
			AssertRefExchangeRate(exchangeRateZZ5, true);
			AssertRefExchangeRate(exchangeRateZZ6, true);
			AssertRefExchangeRate(exchangeRateZZ7, true);
			AssertRefExchangeRate(exchangeRateZZ8, true);

			exchangeRateZZ5.CompanyPK = zaCompany2;
			exchangeRateZZ6.CompanyPK = zaCompany2;
			exchangeRateZZ7.CompanyPK = zaCompany2;
			exchangeRateZZ8.CompanyPK = zaCompany2;
			AssertRefExchangeRate(exchangeRateZZ5, true);
			AssertRefExchangeRate(exchangeRateZZ6, true);
			AssertRefExchangeRate(exchangeRateZZ7, true);
			AssertRefExchangeRate(exchangeRateZZ8, true);

			exchangeRateZZ9.CompanyPK = zaCompany1;
			AssertRefExchangeRate(exchangeRate13, true);
			AssertRefExchangeRate(exchangeRate14, false);
			AssertRefExchangeRate(exchangeRateZZ9, true);
		}

		public void TestUserExchangeRate_NoIntersection()
		{
			TestConnection.ExecuteNonQuery(@"DELETE FROM dbo.ZZRefExchangeRate");
			TestConnection.ExecuteNonQuery(@"DELETE FROM RefDatabase_RefExchangeRateZZ");

			var zaCompany1 = TestDataCreator.CreateCompany("ZA1", "ZA", "ZAR");
			var clientPK = TestDataCreator.CreateOrganisation("OH1", "Client");

			var exchangeRateZZ1 = InsertRefExchangeRateZZ("ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 14), new DateTime(2023, 7, 14), 0.65m);
			var exchangeRateZZ2 = InsertRefExchangeRateZZ("ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 17), new DateTime(2023, 7, 17), 0.65m);

			var exchangeRate1 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 15), new DateTime(2023, 7, 15), 0.53m, Guid.Empty);
			var exchangeRate2 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 16), new DateTime(2023, 7, 16), 0.53m, Guid.Empty);

			exchangeRateZZ1.CompanyPK = zaCompany1;
			exchangeRateZZ2.CompanyPK = zaCompany1;

			AssertRefExchangeRate(exchangeRateZZ1, true);
			AssertRefExchangeRate(exchangeRateZZ2, true);
			AssertRefExchangeRate(exchangeRate1, true);
			AssertRefExchangeRate(exchangeRate2, true);
		}

		public void TestUserExchangeRate_Intersection()
		{
			TestConnection.ExecuteNonQuery(@"DELETE FROM dbo.ZZRefExchangeRate");
			TestConnection.ExecuteNonQuery(@"DELETE FROM RefDatabase_RefExchangeRateZZ");

			var zaCompany1 = TestDataCreator.CreateCompany("ZA1", "ZA", "ZAR");

			var exchangeRateZZ1 = InsertRefExchangeRateZZ("ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 14), new DateTime(2023, 7, 15), 0.65m);
			var exchangeRateZZ2 = InsertRefExchangeRateZZ("ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 16), new DateTime(2023, 7, 17), 0.65m);

			var exchangeRate1 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 13), new DateTime(2023, 7, 14), 0.53m, Guid.Empty);
			var exchangeRate2 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 15), new DateTime(2023, 7, 16), 0.53m, Guid.Empty);
			var exchangeRate3 = InsertZZRefExchangeRate(zaCompany1, "ZA", "CAD", "CUS", string.Empty, new DateTime(2023, 7, 17), new DateTime(2023, 7, 18), 0.53m, Guid.Empty);

			exchangeRateZZ1.CompanyPK = zaCompany1;
			exchangeRateZZ2.CompanyPK = zaCompany1;

			AssertRefExchangeRate(exchangeRateZZ1, true);
			AssertRefExchangeRate(exchangeRateZZ2, true);
			AssertRefExchangeRate(exchangeRate1, false);
			AssertRefExchangeRate(exchangeRate2, false);
			AssertRefExchangeRate(exchangeRate3, false);
		}

		public class RefExchangeRateBO
		{
			public Guid PK;
			public Guid CompanyPK;
			public string CountryCode;
			public string Currency;
			public string RateType;
			public string AsPublished;
			public DateTime StartDate;
			public DateTime ExpiryDate;
			public decimal Rate;
			public bool IsSystem;
		}

		public RefExchangeRateBO InsertZZRefExchangeRate(Guid companyPK, string countryCode, string currency, string rateType, string asPublished, DateTime startDate, DateTime expiryDate, decimal rate, Guid clientPK)
		{
			return InsertZZRefExchangeRate(TestConnection, Guid.NewGuid(), companyPK, countryCode, currency, rateType, asPublished, startDate, expiryDate, rate, clientPK);
		}

		public static RefExchangeRateBO InsertZZRefExchangeRate(DbConnection connection, Guid pk, Guid companyPK, string countryCode, string currency, string rateType, string asPublished, DateTime startDate, DateTime expiryDate, decimal rate, Guid clientPK)
		{
			using (var command = connection.Command(@"INSERT INTO dbo.ZZRefExchangeRate(RE_PK, RE_GC, RE_RX_NKExCurrency, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_SellRate, RE_OH_Client, RE_AsPublished)
				VALUES (@PK, @CompanyPk, @Currency, @RateType, @StartDate, @ExpiryDate, @Rate, @Client, @AsPublished)"))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, ZZRefExchangeRateSchema.PK);
				command.AddParameterBasedOnDbColumn("@CompanyPk", companyPK, ZZRefExchangeRateSchema.RE_GC);
				command.AddParameterBasedOnDbColumn("@Currency", currency, ZZRefExchangeRateSchema.RE_RX_NKExCurrency);
				command.AddParameterBasedOnDbColumn("@RateType", rateType, ZZRefExchangeRateSchema.RE_ExRateType);
				command.AddParameterBasedOnDbColumn("@StartDate", startDate, ZZRefExchangeRateSchema.RE_StartDate);
				command.AddParameterBasedOnDbColumn("@ExpiryDate", expiryDate, ZZRefExchangeRateSchema.RE_ExpiryDate);
				command.AddParameterBasedOnDbColumn("@Rate", rate, ZZRefExchangeRateSchema.RE_SellRate);
				command.AddParameterBasedOnDbColumn("@Client", clientPK == Guid.Empty ? DBNull.Value : clientPK, ZZRefExchangeRateSchema.RE_OH_Client);
				command.AddParameterBasedOnDbColumn("@AsPublished", asPublished, ZZRefExchangeRateSchema.RE_AsPublished);
				command.ExecuteNonQuery();
			}

			return new RefExchangeRateBO()
			{
				PK = pk,
				CompanyPK = companyPK,
				CountryCode = countryCode,
				Currency = currency,
				RateType = rateType,
				AsPublished = asPublished,
				StartDate = startDate,
				ExpiryDate = expiryDate,
				Rate = rate,
				IsSystem = false
			};
		}

		public RefExchangeRateBO InsertRefExchangeRateZZ(string countryCode, string currency, string rateType, string asPublished, DateTime startDate, DateTime expiryDate, decimal rate)
		{
			return InsertRefExchangeRateZZ(TestConnection, Guid.NewGuid(), countryCode, currency, rateType, asPublished, startDate, expiryDate, rate);
		}

		public static RefExchangeRateBO InsertRefExchangeRateZZ(DbConnection connection, Guid pk, string countryCode, string currency, string rateType, string asPublished, DateTime startDate, DateTime expiryDate, decimal rate)
		{
			using (var command = connection.Command(@"INSERT INTO RefDatabase_RefExchangeRateZZ(ZZN_PK, ZZN_RN_NKCountry, ZZN_RX_NKExCurrency, ZZN_ExRateType, ZZN_StartDate, ZZN_EndDate, ZZN_Rate, ZZN_AsPublished)
				VALUES (@PK, @CountryCode, @Currency, @RateType, @StartDate, @ExpiryDate, @Rate, @AsPublished)"))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, RefExchangeRateZZSchema.PK);
				command.AddParameterBasedOnDbColumn("@CountryCode", countryCode, RefExchangeRateZZSchema.ZZN_RN_NKCountry);
				command.AddParameterBasedOnDbColumn("@Currency", currency, RefExchangeRateZZSchema.ZZN_RX_NKExCurrency);
				command.AddParameterBasedOnDbColumn("@RateType", rateType, RefExchangeRateZZSchema.ZZN_ExRateType);
				command.AddParameterBasedOnDbColumn("@StartDate", startDate, RefExchangeRateZZSchema.ZZN_StartDate);
				command.AddParameterBasedOnDbColumn("@ExpiryDate", expiryDate, RefExchangeRateZZSchema.ZZN_EndDate);
				command.AddParameterBasedOnDbColumn("@Rate", rate, RefExchangeRateZZSchema.ZZN_Rate);
				command.AddParameterBasedOnDbColumn("@AsPublished", asPublished, RefExchangeRateZZSchema.ZZN_AsPublished);
				command.ExecuteNonQuery();
			}

			return new RefExchangeRateBO()
			{
				PK = pk,
				CountryCode = countryCode,
				Currency = currency,
				RateType = rateType,
				AsPublished = asPublished,
				StartDate = startDate,
				ExpiryDate = expiryDate,
				Rate = rate,
				IsSystem = true
			};
		}

		public void AssertRefExchangeRate(RefExchangeRateBO exchangeRate, bool inculde)
		{
			var sql = @"SELECT * FROM dbo.RefExchangeRate WHERE RE_GC = @CompanyPK 
AND RE_RX_NKExCurrency = @Currency AND RE_ExRateType = @RateType 
AND RE_StartDate = @StartDate AND RE_ExpiryDate = @ExpiryDate AND RE_SellRate = @Rate AND RE_IsSystem = @IsSystem
AND RE_AsPublished = @AsPublished";

			if (!exchangeRate.IsSystem)
			{
				sql += " AND RE_PK = @PK";
			}

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@CompanyPK", exchangeRate.CompanyPK, RefExchangeRateSchema.RE_GC);
				command.AddParameterBasedOnDbColumn("@Currency", exchangeRate.Currency, RefExchangeRateSchema.RE_RX_NKExCurrency);
				command.AddParameterBasedOnDbColumn("@RateType", exchangeRate.RateType, RefExchangeRateSchema.RE_ExRateType);
				command.AddParameterBasedOnDbColumn("@StartDate", exchangeRate.StartDate, RefExchangeRateSchema.RE_StartDate);
				command.AddParameterBasedOnDbColumn("@ExpiryDate", exchangeRate.ExpiryDate, RefExchangeRateSchema.RE_ExpiryDate);
				command.AddParameterBasedOnDbColumn("@Rate", exchangeRate.Rate, RefExchangeRateSchema.RE_SellRate);
				command.AddParameterBasedOnDbColumn("@IsSystem", exchangeRate.IsSystem, RefExchangeRateSchema.RE_IsSystem);
				command.AddParameterBasedOnDbColumn("@AsPublished", exchangeRate.AsPublished, RefExchangeRateSchema.RE_AsPublished);

				if (!exchangeRate.IsSystem)
				{
					command.AddParameterBasedOnDbColumn("@PK", exchangeRate.PK, RefExchangeRateSchema.PK);
				}

				using (var reader = command.ExecuteReader())
				{
					var message = string.Format("Exchange Rate {0}/{1}/{2}({3}-{4})", exchangeRate.CountryCode, exchangeRate.Currency, exchangeRate.RateType, exchangeRate.StartDate, exchangeRate.ExpiryDate);
					if (inculde)
					{
						Assert(message + " should be included", reader.Read());
					}
					else
					{
						Assert(message + " should NOT be included", !reader.Read());
					}
				}
			}
		}
	}
}

