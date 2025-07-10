using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class ExchangeRateTest : TransactionedTestCase
	{
		public ExchangeRateTest() : base() { }

		[TestDate(2007, 1, 1)]
		public void TestTodaysRate()
		{
			DateTime now = EnvProxy.Instance.Time.CurrentLocalDateTime;
			DateTime nowPlusHundredDays = now.AddDays(100);

			CreateExchageRate(ExchangeRateType.Buy, now, nowPlusHundredDays, 0.4m);
			CreateExchageRate(ExchangeRateType.Sell, now, nowPlusHundredDays, 0.6m);
			CreateExchageRate(ExchangeRateType.CustomsMeasureEURExRate, now, nowPlusHundredDays, 0.7m);
			CreateExchageRate(ExchangeRateType.Customs, now, nowPlusHundredDays, 0.8m);
			CreateExchageRate(ExchangeRateType.CustomsSecondary, now, nowPlusHundredDays, 0.9m);
			CreateExchageRate(ExchangeRateType.PeriodEnd, now, nowPlusHundredDays, 1.0m);
			CreateExchageRate(ExchangeRateType.GlobalCreditControl, now, nowPlusHundredDays, 1.1m);
			CreateExchageRate(ExchangeRateType.IATA, now, nowPlusHundredDays, 1.2m);

			var amount = 1.4m;
			for (int i = 1; i <= 99; i++)
			{
				var rateType = "C" + i.ToString().PadLeft(2, '0');
				CreateExchageRate((ExchangeRateType)Enum.Parse(typeof(ExchangeRateType), rateType), now, nowPlusHundredDays, amount);
				amount += 0.2m;
			}

			AssertEquals("Buy", 0.4M, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, ExchangeRateType.Buy));
			AssertEquals("Sell", 0.6M, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, ExchangeRateType.Sell));
			AssertEquals("CustomsMeasureEURExRate", 0.7M, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, ExchangeRateType.CustomsMeasureEURExRate));
			AssertEquals("Customs", 0.8M, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, ExchangeRateType.Customs));
			AssertEquals("CustomsSecondary", 0.9M, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, ExchangeRateType.CustomsSecondary));
			AssertEquals("PeriodEnd", 1.0M, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, ExchangeRateType.PeriodEnd));
			AssertEquals("GlobalCreditControl", 1.1M, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, ExchangeRateType.GlobalCreditControl));
			AssertEquals("IATA", 1.2M, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, ExchangeRateType.IATA));

			amount = 1.4m;
			for (int i = 1; i <= 99; i++)
			{
				var rateType = "C" + i.ToString().PadLeft(2, '0');
				AssertEquals(rateType, amount, NonReciprocalRate.TodaysRate(ForeignCurrencyCode, (ExchangeRateType)Enum.Parse(typeof(ExchangeRateType), rateType)));
				amount += 0.2m;
			}
		}

		public void TestForeignToForeign()
		{
			AssertEquals("Non-Reciprocal", 7.636M, NonReciprocalRate.ForeignToForeign(2M, 0.55555M, 2.12121M, ForeignCurrencyCode));
			AssertEquals("Reciprocal", 0.524M, ReciprocalRate.ForeignToForeign(2M, 0.55555M, 2.12121M, ForeignCurrencyCode));
		}

		public void TestForeignToForeignWithoutRounding()
		{
			AssertEquals("Non-Reciprocal", (2M / 0.55555M) * 1.12121M, NonReciprocalRate.ForeignToForeignWithoutRounding(2M, 0.55555M, 1.12121M, ForeignCurrencyCode));
			AssertEquals("Reciprocal", (2M * 0.55555M) / 1.12121M, ReciprocalRate.ForeignToForeignWithoutRounding(2M, 0.55555M, 1.12121M, ForeignCurrencyCode));
		}

		public void TestForeignToForeignWithRateEqualsZero()
		{
			AssertEquals("Non-Reciprocal", 0M, NonReciprocalRate.ForeignToForeign(2M, 0M, 1.1M, ForeignCurrencyCode));
			AssertEquals("Reciprocal", 0M, ReciprocalRate.ForeignToForeign(2M, 0.5M, 0M, ForeignCurrencyCode));
		}

		public void TestLocalToForeign()
		{
			AssertEquals("Non-Reciprocal", 1.225M, NonReciprocalRate.LocalToForeign(2M, 0.6123M, ForeignCurrencyCode));
			AssertEquals("Reciprocal", 3.333M, ReciprocalRate.LocalToForeign(2M, 0.6M, ForeignCurrencyCode));
		}

		public void TestLocalToForeignWithoutRounding()
		{
			AssertEquals("Non-Reciprocal", 1.2246M, NonReciprocalRate.LocalToForeignWithoutRounding(2M, 0.6123M, ForeignCurrencyCode));
			AssertEquals("Reciprocal", 2m / 0.6m, ReciprocalRate.LocalToForeignWithoutRounding(2M, 0.6M, ForeignCurrencyCode));
		}

		public void TestLocalToForeignWithRateEqualsZero()
		{
			AssertEquals("Non-Reciprocal", 0M, NonReciprocalRate.LocalToForeign(2M, 0M, ForeignCurrencyCode));
			AssertEquals("Reciprocal", 0M, ReciprocalRate.LocalToForeign(2M, 0M, ForeignCurrencyCode));
		}

		public void TestForeignToLocalWithoutRounding()
		{
			AssertEquals("Non-Reciprocal", 1m / 0.6m, NonReciprocalRate.ForeignToLocalWithoutRounding(1M, 0.6M));
			AssertEquals("Reciprocal", 0.6123M, ReciprocalRate.ForeignToLocalWithoutRounding(1M, 0.6123M));
		}

		public void TestForeignToLocal()
		{
			AssertEquals("Non-Reciprocal", 1.67M, NonReciprocalRate.ForeignToLocal(1M, 0.6M));
			AssertEquals("Reciprocal", 0.61M, ReciprocalRate.ForeignToLocal(1M, 0.6123M));
		}

		public void TestForeignToLocalWithRateEqualsZero()
		{
			AssertEquals("Non-Reciprocal", 0M, NonReciprocalRate.ForeignToLocal(1M, 0M));
			AssertEquals("Reciprocal", 0M, ReciprocalRate.ForeignToLocal(1M, 0M));
		}

		public void TestGetRate()
		{
			AssertEquals("Non-Reciprocal", 0.612345M, NonReciprocalRate.GetRate(1M, 0.612345M));
			AssertEquals("Reciprocal", 1.666667M, ReciprocalRate.GetRate(1M, 0.6M));

			AssertEquals("Non-Reciprocal (Without Rounding)", 0.612345679M, NonReciprocalRate.GetRate(1M, 0.6123456789M, 9));
			AssertEquals("Reciprocal (Without Rounding)", 1.666666667M, ReciprocalRate.GetRate(1M, 0.6M, 9));
		}

		public void TestTodaysRateIncludingExpired()
		{
			var now = DateTime.Now;
			CreateExchageRate(ExchangeRateType.Buy, now.AddDays(-10), now.AddDays(-6), 0.4m);
			CreateExchageRate(ExchangeRateType.Sell, now.AddDays(-10), now.AddDays(-6), 0.6m);
			CreateExchageRate(ExchangeRateType.CustomsMeasureEURExRate, now.AddDays(-10), now.AddDays(-6), 0.7m);
			CreateExchageRate(ExchangeRateType.Customs, now.AddDays(-10), now.AddDays(-6), 0.8m);
			CreateExchageRate(ExchangeRateType.CustomsSecondary, now.AddDays(-10), now.AddDays(-6), 0.9m);
			CreateExchageRate(ExchangeRateType.PeriodEnd, now.AddDays(-10), now.AddDays(-6), 1.0m);
			CreateExchageRate(ExchangeRateType.GlobalCreditControl, now.AddDays(-10), now.AddDays(-6), 1.1m);
			CreateExchageRate(ExchangeRateType.IATA, now.AddDays(-10), now.AddDays(-6), 1.2m);

			var amount1 = 1.4m;
			for (int i = 1; i <= 99; i++)
			{
				var rateType = "C" + i.ToString().PadLeft(2, '0');
				var rateTypeEnum = (ExchangeRateType)Enum.Parse(typeof(ExchangeRateType), rateType);
				CreateExchageRate(rateTypeEnum, now.AddDays(-10), now.AddDays(-6), amount1);
				amount1 += 0.2m;
			}

			AssertEquals("Get Rate should not find a rate as date is in expired period", 0.4M, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.Buy));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0.6M, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.Sell));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0.7M, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.CustomsMeasureEURExRate));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0.8M, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.Customs));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0.9M, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.CustomsSecondary));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 1.0M, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.PeriodEnd));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 1.1M, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.GlobalCreditControl));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 1.2M, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.IATA));

			var amount = 1.4m;
			for (int i = 1; i <= 99; i++)
			{
				var rateType = "C" + i.ToString().PadLeft(2, '0');
				var rateTypeEnum = (ExchangeRateType)Enum.Parse(typeof(ExchangeRateType), rateType);
				AssertEquals("Get Rate should not find a rate as date is in expired period", amount, NonReciprocalRate.TodaysRateIncludingExpired(ForeignCurrencyCode, rateTypeEnum));
				amount += 0.2m;
			}
		}

		public void TestGetRateIncludingExpired()
		{
			var now = DateTime.Now;
			CreateExchageRate(ExchangeRateType.Buy, now.AddDays(-10), now.AddDays(-6), 0.4m);
			CreateExchageRate(ExchangeRateType.Buy, now.AddDays(-4), now, 0.5m);
			CreateExchageRate(ExchangeRateType.Sell, now.AddDays(-10), now.AddDays(-6), 0.6m);
			CreateExchageRate(ExchangeRateType.Sell, now.AddDays(-4), now, 0.7m);
			CreateExchageRate(ExchangeRateType.CustomsMeasureEURExRate, now.AddDays(-10), now.AddDays(-6), 0.7m);
			CreateExchageRate(ExchangeRateType.CustomsMeasureEURExRate, now.AddDays(-4), now, 0.75m);
			CreateExchageRate(ExchangeRateType.Customs, now.AddDays(-10), now.AddDays(-6), 0.8m);
			CreateExchageRate(ExchangeRateType.Customs, now.AddDays(-4), now, 0.9m);
			CreateExchageRate(ExchangeRateType.CustomsSecondary, now.AddDays(-10), now.AddDays(-6), 0.85m);
			CreateExchageRate(ExchangeRateType.CustomsSecondary, now.AddDays(-4), now, 0.95m);
			CreateExchageRate(ExchangeRateType.PeriodEnd, now.AddDays(-10), now.AddDays(-6), 1.0m);
			CreateExchageRate(ExchangeRateType.PeriodEnd, now.AddDays(-4), now, 1.1m);
			CreateExchageRate(ExchangeRateType.GlobalCreditControl, now.AddDays(-10), now.AddDays(-6), 1.05m);
			CreateExchageRate(ExchangeRateType.GlobalCreditControl, now.AddDays(-4), now, 1.15m);
			CreateExchageRate(ExchangeRateType.IATA, now.AddDays(-10), now.AddDays(-6), 1.2m);
			CreateExchageRate(ExchangeRateType.IATA, now.AddDays(-4), now, 1.3m);

			var amount1 = 1.4m;
			var amount2 = 1.5m;
			for (int i = 1; i <= 99; i++)
			{
				var rateType = "C" + i.ToString().PadLeft(2, '0');
				var rateTypeEnum = (ExchangeRateType)Enum.Parse(typeof(ExchangeRateType), rateType);
				CreateExchageRate(rateTypeEnum, now.AddDays(-10), now.AddDays(-6), amount1);
				CreateExchageRate(rateTypeEnum, now.AddDays(-4), now, amount2);
				amount1 += 0.2m;
				amount2 += 0.2m;
			}

			DateTime dateWhereNoRateDefined = now.AddDays(-5);

			AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, ExchangeRateType.Buy, dateWhereNoRateDefined));
			AssertEquals("Get Rate Including expired should find a rate", 0.4M, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.Buy, dateWhereNoRateDefined));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, ExchangeRateType.Sell, dateWhereNoRateDefined));
			AssertEquals("Get Rate Including expired should find a rate", 0.6M, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.Sell, dateWhereNoRateDefined));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, ExchangeRateType.CustomsMeasureEURExRate, dateWhereNoRateDefined));
			AssertEquals("Get Rate Including expired should find a rate", 0.7M, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.CustomsMeasureEURExRate, dateWhereNoRateDefined));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, ExchangeRateType.Customs, dateWhereNoRateDefined));
			AssertEquals("Get Rate Including expired should find a rate", 0.8M, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.Customs, dateWhereNoRateDefined));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, ExchangeRateType.CustomsSecondary, dateWhereNoRateDefined));
			AssertEquals("Get Rate Including expired should find a rate", 0.85M, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.CustomsSecondary, dateWhereNoRateDefined));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, ExchangeRateType.PeriodEnd, dateWhereNoRateDefined));
			AssertEquals("Get Rate Including expired should find a rate", 1.0M, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.PeriodEnd, dateWhereNoRateDefined));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, ExchangeRateType.GlobalCreditControl, dateWhereNoRateDefined));
			AssertEquals("Get Rate Including expired should find a rate", 1.05M, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.GlobalCreditControl, dateWhereNoRateDefined));
			AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, ExchangeRateType.IATA, dateWhereNoRateDefined));
			AssertEquals("Get Rate Including expired should find a rate", 1.2M, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, ExchangeRateType.IATA, dateWhereNoRateDefined));

			var amount = 1.4m;
			for (int i = 1; i <= 99; i++)
			{
				var rateType = "C" + i.ToString().PadLeft(2, '0');
				var rateTypeEnum = (ExchangeRateType)Enum.Parse(typeof(ExchangeRateType), rateType);
				AssertEquals("Get Rate should not find a rate as date is in expired period", 0M, NonReciprocalRate.GetRate(ForeignCurrencyCode, rateTypeEnum, dateWhereNoRateDefined));
				AssertEquals("Get Rate Including expired should find a rate", amount, NonReciprocalRate.GetRateIncludingExpired(ForeignCurrencyCode, rateTypeEnum, dateWhereNoRateDefined));
				amount += 0.2m;
			}
		}

		void CreateExchageRate(ExchangeRateType type, DateTime from, DateTime to, decimal amount)
		{
			var fi = type.GetType().GetField(type.ToString());
			var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			var typeDescription = (attributes?.Length > 0) ? attributes[0].Description : type.ToString();

			string sqlText = string.Format(@"
				INSERT INTO dbo.REFEXCHANGERATE (
					RE_PK,
					RE_RX_NKExCurrency,
					RE_ExRateType,
					RE_StartDate,
					RE_ExpiryDate,
					RE_SellRate,
					RE_GC,
					RE_AsPublished
				) VALUES (
					newid(),
					'JOD',
					'{0}',
					'{1}',
					'{2}',
					{3},
					'{4}',
					'{5}'
				)",
							typeDescription.Substring(0, 3),
							SqlFormatInfo.ToSqlDateTimeString(from),
							SqlFormatInfo.ToSqlDateTimeString(to),
							amount,
							EnvProxy.Instance.CurrentCompany.PK.ToString(),
							string.Empty);

			Db.Connection.ExecuteNonQuery(sqlText);
		}

		public void TestGetRateWithBothAmountsZero()
		{
			AssertEquals("Non-Reciprocal", 1M, NonReciprocalRate.GetRate(0M, 0M));
			AssertEquals("Reciprocal", 1M, NonReciprocalRate.GetRate(0M, 0M));

			AssertEquals("Non-Reciprocal (Rounded to 9 Decimals)", 1M, NonReciprocalRate.GetRate(0M, 0M, 9));
			AssertEquals("Reciprocal (Rounded to 9 Decimals)", 1M, NonReciprocalRate.GetRate(0M, 0M, 9));
		}

		public void TestGetRateWithLocalAmountZero()
		{
			AssertEquals("Non-Reciprocal", 0M, NonReciprocalRate.GetRate(0M, 1M));
			AssertEquals("Reciprocal", 0M, ReciprocalRate.GetRate(0M, 1M));

			AssertEquals("Non-Reciprocal (Rounded to 9 Decimals)", 0M, NonReciprocalRate.GetRate(0M, 1M, 9));
			AssertEquals("Reciprocal (Rounded to 9 Decimals)", 0M, ReciprocalRate.GetRate(0M, 1M, 9));
		}

		public void TestGetRateWithForeignAmountZero()
		{
			AssertEquals("Non-Reciprocal", 0M, NonReciprocalRate.GetRate(1M, 0M));
			AssertEquals("Reciprocal", 0M, ReciprocalRate.GetRate(1M, 0M));

			AssertEquals("Non-Reciprocal (Rounded to 9 Decimals)", 0M, NonReciprocalRate.GetRate(1M, 0M, 9));
			AssertEquals("Reciprocal (Rounded to 9 Decimals)", 0M, ReciprocalRate.GetRate(1M, 0M, 0));
		}

		public void TestByCrossCheckingNonReciprocalCalculations()
		{
			AssertCalculationsAreCorrectByCrossCheckingVariousAmounts(NonReciprocalRate);
		}

		public void TestByCrossCheckingReciprocalCalculations()
		{
			AssertCalculationsAreCorrectByCrossCheckingVariousAmounts(ReciprocalRate);
		}

		public void TestRoundingError()
		{
			decimal local = 9999;
			decimal foreign = 1;
			decimal rate = NonReciprocalRate.GetRate(local, foreign);
			AssertEquals("Foreign Non-Reciprocal", foreign, NonReciprocalRate.LocalToForeign(local, rate, ForeignCurrencyCode));
			AssertEquals("Local Non-Reciprocal expected rounding error", 1 + local, NonReciprocalRate.ForeignToLocal(foreign, rate));

			rate = ReciprocalRate.GetRate(local, foreign);
			AssertEquals("Foreign Reciprocal", foreign, ReciprocalRate.LocalToForeign(local, rate, ForeignCurrencyCode));
			AssertEquals("Local Reciprocal", local, ReciprocalRate.ForeignToLocal(foreign, rate));
		}

		public void TestTodaysRateWIthGuidEmpty()
		{
			ExchangeRate anExchangeRate = new ExchangeRate(false, 2, EnvProxy.Instance.CurrentCompany.PK);
			decimal result = anExchangeRate.TodaysRate("", ExchangeRateType.Buy);
			AssertEquals("Guid.Empty as an exchange rate", 0.0m, result);
		}

		public void TestGetExchangeRateType()
		{
			AssertEquals("ExchangeRateType.All", ExchangeRateType.All, ExchangeRate.GetExchangeRateType("XYZ"));
			AssertEquals("ExchangeRateType.Buy", ExchangeRateType.Buy, ExchangeRate.GetExchangeRateType("BUY"));
			AssertEquals("ExchangeRateType.Sell", ExchangeRateType.Sell, ExchangeRate.GetExchangeRateType("SEL"));
			AssertEquals("ExchangeRateType.CustomsMeasureEURExRate", ExchangeRateType.CustomsMeasureEURExRate, ExchangeRate.GetExchangeRateType("CUD"));
			AssertEquals("ExchangeRateType.Customs", ExchangeRateType.Customs, ExchangeRate.GetExchangeRateType("CUS"));
			AssertEquals("ExchangeRateType.CustomsSecondary", ExchangeRateType.CustomsSecondary, ExchangeRate.GetExchangeRateType("CUE"));
			AssertEquals("ExchangeRateType.PeriodEnd", ExchangeRateType.PeriodEnd, ExchangeRate.GetExchangeRateType("PER"));
			AssertEquals("ExchangeRateType.GlobalCreditControl", ExchangeRateType.GlobalCreditControl, ExchangeRate.GetExchangeRateType("GCB"));
			AssertEquals("ExchangeRateType.IATA", ExchangeRateType.IATA, ExchangeRate.GetExchangeRateType("IAT"));

			for (int i = 1; i <= 99; i++)
			{
				var rateType = "C" + i.ToString().PadLeft(2, '0');
				var rateTypeEnum = (ExchangeRateType)Enum.Parse(typeof(ExchangeRateType), rateType);
				AssertEquals("ExchangeRateType" + rateType, rateTypeEnum, ExchangeRate.GetExchangeRateType(rateType));
			}
		}

		public void TestConvertedAmount_Overflow()
		{
			decimal amount = 9999999999999999999999999999m;
			decimal exchangerate = 0.01m;
			decimal d = 0;
			ExchangeRate anExchangeRate = new ExchangeRate(false, 2, EnvProxy.Instance.CurrentCompany.PK);
			AssertExceptionThrown<OverflowException>(() => { d = amount / exchangerate; });
			AssertNoExceptionThrown(() => { d = anExchangeRate.ForeignToLocal(amount, exchangerate); });
			AssertEquals(-1, d, 0.000001m);
		}

		ExchangeRate NonReciprocalRate;
		ExchangeRate ReciprocalRate;
		readonly string ForeignCurrencyCode = "JOD"; //Jordan Dinars with 3 decimal places

		protected override void SetUp()
		{
			base.SetUp();
			NonReciprocalRate = new ExchangeRate(false, 2, EnvProxy.Instance.CurrentCompany.PK);
			ReciprocalRate = new ExchangeRate(true, 2, EnvProxy.Instance.CurrentCompany.PK);
		}

		void AssertCalculationsAreCorrectByCrossCheckingVariousAmounts(ExchangeRate exchangeRate)
		{
			AssertCalculationsAreCorrectByCrossChecking(exchangeRate, 0, 0);
			AssertCalculationsAreCorrectByCrossChecking(exchangeRate, 1, 1);
			AssertCalculationsAreCorrectByCrossChecking(exchangeRate, 3, 1);
			AssertCalculationsAreCorrectByCrossChecking(exchangeRate, 1, 3);
			AssertCalculationsAreCorrectByCrossChecking(exchangeRate, 100, 0.01M);
			AssertCalculationsAreCorrectByCrossChecking(exchangeRate, 0.01M, 100);
			AssertCalculationsAreCorrectByCrossChecking(exchangeRate, 5000, 1);
			AssertCalculationsAreCorrectByCrossChecking(exchangeRate, 1, 5000);
		}

		void AssertCalculationsAreCorrectByCrossChecking(ExchangeRate exchangeRate, decimal local, decimal foreign)
		{
			decimal rate = exchangeRate.GetRate(local, foreign);
			AssertEquals("Foreign", foreign, exchangeRate.LocalToForeign(local, rate, ForeignCurrencyCode));
			AssertEquals("Local", local, exchangeRate.ForeignToLocal(foreign, rate));

			rate = exchangeRate.GetRate(local, foreign, 9);
			AssertEquals("Foreign", foreign, exchangeRate.LocalToForeign(local, rate, ForeignCurrencyCode));
			AssertEquals("Local", local, exchangeRate.ForeignToLocal(foreign, rate));
		}
	}
}
