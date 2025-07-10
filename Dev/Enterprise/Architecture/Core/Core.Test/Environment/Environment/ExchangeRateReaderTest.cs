using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ExchangeRateReaderTest : TransactionedTestCase
	{
		public ExchangeRateReaderTest() : base() { }

		public void TestGetRateWithInvalidRateTypeReturnsZero()
		{
			var reader = new ExchangeRateReader();
			var invalidRate = reader.GetRate(CompanyPK, "ANY", CurrencyCode, SomeDate);
			AssertEquals(0M, invalidRate);
		}

		public void TestGetCustomsRate()
		{
			ExchangeRateReader reader = new ExchangeRateReader();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.SellRate, SomeDateRate);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, CurrencyCode, SomeDate);
			AssertEquals(0M, actualRate);

			reader.ClearCache();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.CustomsRate, SomeDateRate);
			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, CurrencyCode, SomeDate);

			AssertEquals(SomeDateRate, actualRate);

			reader.ClearCache();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.CustomsRateSecondary, SomeDateRate);
			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRateSecondary, CurrencyCode, SomeDate);

			AssertEquals(SomeDateRate, actualRate);
		}

		public void TestGetCustomsRateIncludingExpired()
		{
			ExchangeRateReader reader = new ExchangeRateReader();
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "CUS", 0.4m);
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "BUY", 0.5m);
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "CUE", 0.6m);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, CurrencyCode, DateTime.Now.AddDays(-5), true);
			AssertEquals(0.4M, actualRate);

			reader.ClearCache();
			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRateSecondary, CurrencyCode, DateTime.Now.AddDays(-5), true);
			AssertEquals(0.6M, actualRate);
		}

		public void TestGetBuyRate()
		{
			ExchangeRateReader reader = new ExchangeRateReader();

			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.CustomsRate, SomeDateRate);
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.SellRate, SomeDateRate);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, CurrencyCode, SomeDate);
			AssertEquals(0M, actualRate);

			reader.ClearCache();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);
			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, CurrencyCode, SomeDate, false);
			AssertEquals(SomeDateRate, actualRate);
		}

		public void TestGetRateForInvalidCurrency()
		{
			var reader = new ExchangeRateReader();
			AssertEquals(0M, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, "'", SomeDate));
		}

		public void TestGetRateForEmptyCurrency()
		{
			var reader = new ExchangeRateReader();
			int dbCountBefore = Db.Connection.ExecutedCommandCount;
			AssertEquals(0m, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, "", SomeDate));
			AssertEquals(dbCountBefore, Db.Connection.ExecutedCommandCount);
		}

		public void TestGetBuyRateIncludingExpired()
		{
			ExchangeRateReader reader = new ExchangeRateReader();
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "BUY", 0.4m);
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "CUS", 0.5m);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, CurrencyCode, DateTime.Now.AddDays(-5), true);
			AssertEquals(0.4M, actualRate);
		}

		public void TestGetSellRate()
		{
			ExchangeRateReader reader = new ExchangeRateReader();

			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.CustomsRate, SomeDateRate);
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.SellRate, CurrencyCode, SomeDate);
			AssertEquals(0M, actualRate);

			reader.ClearCache();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.SellRate, SomeDateRate);
			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.SellRate, CurrencyCode, SomeDate);
			AssertEquals(SomeDateRate, actualRate);
		}

		public void TestGetSellRateIncludingExpired()
		{
			ExchangeRateReader reader = new ExchangeRateReader();
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "SEL", 0.4m);
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "BUY", 0.5m);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.SellRate, CurrencyCode, DateTime.Now.AddDays(-5), true);
			AssertEquals(0.4M, actualRate);
		}

		public void TestGetPeriodEndRate()
		{
			var reader = new ExchangeRateReader();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.SellRate, SomeDateRate);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.PeriodEndRate, CurrencyCode, SomeDate);
			AssertEquals(0M, actualRate);

			reader.ClearCache();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.PeriodEndRate, SomeDateRate);
			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.PeriodEndRate, CurrencyCode, SomeDate);
			AssertEquals(SomeDateRate, actualRate);
		}

		public void TestGetPeriodEndRateIncludingExpired()
		{
			DateTime today = ZDateTime.Today.ToDateTime();
			DateTime day_1 = today.AddDays(-5);
			DateTime day_2 = today.AddDays(-6);
			DateTime day_3 = today.AddDays(-10);

			var reader = new ExchangeRateReader();
			InsertExpiringExchangeRateRow(day_3, day_2, Constants.ExchangeRateTypes.Code.PeriodEndRate, 0.2m);
			InsertExpiringExchangeRateRow(day_3, day_2, Constants.ExchangeRateTypes.Code.SellRate, 0.4m);
			InsertExpiringExchangeRateRow(day_3, day_2, Constants.ExchangeRateTypes.Code.BuyRate, 0.5m);

			AssertEquals(0.2m, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.PeriodEndRate, CurrencyCode, day_1, true));
		}

		public void TestGetIATARate()
		{
			var reader = new ExchangeRateReader();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.SellRate, SomeDateRate);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.IATARate, CurrencyCode, SomeDate);
			AssertEquals(0M, actualRate);

			reader.ClearCache();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.IATARate, SomeDateRate);
			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.SellRate, CurrencyCode, SomeDate);
			AssertEquals(SomeDateRate, actualRate);
		}

		public void TestGetIATARateIncludingExpired()
		{
			DateTime today = ZDateTime.Today.ToDateTime();
			DateTime day_1 = today.AddDays(-5);
			DateTime day_2 = today.AddDays(-6);
			DateTime day_3 = today.AddDays(-10);

			var reader = new ExchangeRateReader();
			InsertExpiringExchangeRateRow(day_3, day_2, Constants.ExchangeRateTypes.Code.IATARate, 0.2m);
			InsertExpiringExchangeRateRow(day_3, day_2, Constants.ExchangeRateTypes.Code.SellRate, 0.4m);
			InsertExpiringExchangeRateRow(day_3, day_2, Constants.ExchangeRateTypes.Code.BuyRate, 0.5m);

			AssertEquals(0.2m, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.IATARate, CurrencyCode, day_1, true));
		}

		public void TestExpiredRate()
		{
			ExchangeRateReader reader = new ExchangeRateReader();
			DateTime expiryDate = SomeDate.AddDays(7);

			InsertExpiringExchangeRateRow(SomeDate, expiryDate, Constants.ExchangeRateTypes.Code.CustomsRate, SomeDateRate);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, CurrencyCode, expiryDate.AddDays(1));
			AssertEquals("Rate when requested date is the day after the expiry date", 0M, actualRate);

			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, CurrencyCode, expiryDate);
			AssertEquals("Rate that expires on requested date", SomeDateRate, actualRate);

			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, CurrencyCode, expiryDate.AddHours(23));
			AssertEquals("Rate that expires on earlier in the day on the requested date", SomeDateRate, actualRate);
		}

		public void TestFindsPreviousExchangeRate()
		{
			ExchangeRateReader reader = new ExchangeRateReader();

			DateTime previousDate = SomeDate.AddDays(-1);
			decimal previousDateRate = SomeDateRate - 0.1M;

			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.CustomsRate, SomeDateRate);
			InsertExchangeRateRow(previousDate, Constants.ExchangeRateTypes.Code.CustomsRate, previousDateRate);

			AssertEquals("For PreviousDate", previousDateRate, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, CurrencyCode, previousDate));
			AssertEquals("For SomeDate", SomeDateRate, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, CurrencyCode, SomeDate));
		}

		public void TestForNonExistentCurrency()
		{
			ExchangeRateReader reader = new ExchangeRateReader();
			AssertEquals(0M, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsRate, "XXX", SomeDate));
		}

		class ExchangeTestReader : ExchangeRateReader
		{
			protected override DateTime GetCurrentDateTime()
			{
				return CurrentDateTime;
			}

			public DateTime CurrentDateTime;
		}

		public void TestCacheHasFiveMinuteDuration()
		{
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);

			ExchangeTestReader reader = new ExchangeTestReader();
			reader.CurrentDateTime = DateTime.Now;
			AssertEquals("GetBuyRate", SomeDateRate, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, CurrencyCode, SomeDate));

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.RefExchangeRate");

			decimal newRate = SomeDateRate + 0.05M;
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, newRate);

			reader.CurrentDateTime = DateTime.Now.AddMinutes(5); //simulate 5 minutes later.
			AssertEquals("GetBuyRate after exchangerate updated", newRate, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, CurrencyCode, SomeDate));
		}

		public void TestClearCache()
		{
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);

			ExchangeRateReader reader = new ExchangeRateReader();
			AssertEquals("GetBuyRate", SomeDateRate, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, CurrencyCode, SomeDate));

			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.RefExchangeRate");

			decimal newRate = SomeDateRate + 0.05M;
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, newRate);

			reader.ClearCache();
			AssertEquals("GetBuyRate after exchangerate updated", newRate, reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.BuyRate, CurrencyCode, SomeDate));
		}

		public void TestCustomRates()
		{
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C01Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C01Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C02Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C02Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C03Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C03Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C04Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C04Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C05Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C05Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C06Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C06Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C07Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C07Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C08Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C08Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C09Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C09Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C10Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C10Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C11Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C11Rate);
			TestGetCustomRate(Constants.ExchangeRateTypes.Code.C12Rate);
			TestGetCustomRateIncludingExpired(Constants.ExchangeRateTypes.Code.C12Rate);
		}

		void TestGetCustomRate(string exchangeRateType)
		{
			var reader = new ExchangeRateReader();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.SellRate, SomeDateRate);

			var actualRate = reader.GetRate(CompanyPK, exchangeRateType, CurrencyCode, SomeDate);
			AssertEquals(0M, actualRate);

			reader.ClearCache();
			InsertExchangeRateRow(SomeDate, exchangeRateType, SomeDateRate);
			actualRate = reader.GetRate(CompanyPK, exchangeRateType, CurrencyCode, SomeDate);

			AssertEquals(SomeDateRate, actualRate);
		}

		void TestGetCustomRateIncludingExpired(string exchangeRateType)
		{
			var reader = new ExchangeRateReader();
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), exchangeRateType, 0.4m);
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "BUY", 0.5m);

			var actualRate = reader.GetRate(CompanyPK, exchangeRateType, CurrencyCode, DateTime.Now.AddDays(-5), true);
			AssertEquals(0.4M, actualRate);
		}

		public void TestGetCustomsMeasureEURExRate()
		{
			ExchangeRateReader reader = new ExchangeRateReader();

			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.CustomsRate, SomeDateRate);
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.BuyRate, SomeDateRate);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate, CurrencyCode, SomeDate);
			AssertEquals(0M, actualRate);

			reader.ClearCache();
			InsertExchangeRateRow(SomeDate, Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate, SomeDateRate);
			actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate, CurrencyCode, SomeDate);
			AssertEquals(SomeDateRate, actualRate);
		}

		public void TestGetCustomsMeasureEURExRateIncludingExpired()
		{
			ExchangeRateReader reader = new ExchangeRateReader();
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "CUD", 0.7m);
			InsertExpiringExchangeRateRow(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-6), "BUY", 0.5m);

			var actualRate = reader.GetRate(CompanyPK, Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate, CurrencyCode, DateTime.Now.AddDays(-5), true);
			AssertEquals(0.7M, actualRate);
		}

		#region Implementation

		DateTime SomeDate;
		const decimal SomeDateRate = 2.1234M;
		Guid CompanyPK;
		string CurrencyCode;

		protected override void SetUp()
		{
			base.SetUp();
			CompanyPK = EnvProxy.Instance.CurrentCompany.PK;
			CurrencyCode = EnvProxy.Instance.CurrentCompany.LocalCurrency.Code;
			SomeDate = new DateTime(2003, 4, 1);
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.RefExchangeRate");
		}

		void InsertExchangeRateRow(DateTime startDate, string rateType, decimal rate)
		{
			InsertExpiringExchangeRateRow(startDate, DateTime.MinValue, rateType, rate);
		}

		void InsertExpiringExchangeRateRow(DateTime startDate, DateTime expiryDate, string rateType, decimal rate)
		{
			string insertSQL =
				"Insert into " + RefExchangeRateSchema.Constants.SqlSchemaName + "." + RefExchangeRateSchema.Constants.TableName + " ("
				+ RefExchangeRateSchema.Constants.PK + ", "
				+ RefExchangeRateSchema.Constants.RE_StartDate + ", "
				+ RefExchangeRateSchema.Constants.RE_ExpiryDate + ", "
				+ RefExchangeRateSchema.Constants.RE_SellRate + ", "
				+ RefExchangeRateSchema.Constants.RE_ExRateType + ", "
				+ RefExchangeRateSchema.Constants.RE_RX_NKExCurrency + ", "
				+ RefExchangeRateSchema.Constants.RE_GC + ", "
				+ RefExchangeRateSchema.Constants.RE_AsPublished + ") VALUES ("
				+ "'" + Guid.NewGuid() + "', '"
				+ SqlFormatInfo.ToSqlDateString(startDate) + "', "
				+ (expiryDate == DateTime.MinValue ? "NULL" : "'" + SqlFormatInfo.ToSqlDateString(expiryDate) + "'") + ", "
				+ rate.ToString(Culture.Invariant) + ", "
				+ "'" + rateType + "', "
				+ "'" + CurrencyCode + "', "
				+ "'" + CompanyPK + "', "
				+ "'" + string.Empty + "')";

			Db.Connection.ExecuteNonQuery(insertSQL);
		}

		#endregion
	}
}
