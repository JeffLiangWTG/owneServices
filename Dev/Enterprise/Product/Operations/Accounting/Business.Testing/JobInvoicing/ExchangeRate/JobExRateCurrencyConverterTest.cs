using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobExRateCurrencyConverterTest : TestCaseWithFactory
	{
		public void TestConvertExactWithDoNotRetainNewExchangeRateFalg()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("ALL", "ALL", "ALL", "BUY", "TDR");
			GlbCompany.CurrentCompany.Factory.Save();

			var exchangeRateCollection = new ExchangeRatesCollection(TestObjectCreator.Job1, Factory);

			var eurBuyRate = TestObjectCreator.EUR.ExchangeRates.AddNew();
			eurBuyRate.RE_StartDate = ZDate.Today;
			eurBuyRate.RE_ExpiryDate = ZDate.Today.AddDays(10);
			eurBuyRate.RE_ExRateType = "BUY";
			eurBuyRate.RE_SellRate = 0.6m;

			Factory.Save();

			var converter = new JobExRateCurrencyConverter(exchangeRateCollection, ZDateTime.Now, ExchangeRateType.Buy);
			var money = new Money(120m, TestObjectCreator.EUR);
			var exchangeRates = exchangeRateCollection.Cast<ExchangeRate>();

			Factory.SetContext(BusinessContext.ConvertingAmountsForExportAWBHeader);
			Assert("No exchange rates available", !exchangeRates.Any());
			Assert(Factory.HasContext(BusinessContext.ConvertingAmountsForExportAWBHeader));
			converter.ConvertExact(money, TestObjectCreator.AUD, TestObjectCreator.Debtor.PK, CostSell.Revenue);
			var eurExchangeRate = exchangeRates.FirstOrDefault(x => x.JF_RX_NKRateCurrency == Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertNotNull(eurExchangeRate);
			Assert("ShouldDeleteDuplicateExchangeRateBeforeSave must be true because EUR exchange rate is not in DB and factory has context.", eurExchangeRate.ShouldDeleteDuplicateExchangeRateBeforeSave);
			Factory.RemoveContext(BusinessContext.ConvertingAmountsForExportAWBHeader);

			eurExchangeRate.Delete();
			Factory.Save();

			Assert("No exchange rates available", !exchangeRates.Any());
			Assert(!Factory.HasContext(BusinessContext.ConvertingAmountsForExportAWBHeader));
			converter.ConvertExact(money, TestObjectCreator.AUD, TestObjectCreator.Debtor.PK, CostSell.Revenue);
			eurExchangeRate = exchangeRates.FirstOrDefault(x => x.JF_RX_NKRateCurrency == Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertNotNull(eurExchangeRate);
			Assert("ShouldDeleteDuplicateExchangeRateBeforeSave must be false because factory does not have context.", !eurExchangeRate.ShouldDeleteDuplicateExchangeRateBeforeSave);

			eurExchangeRate.Delete();
			Factory.Save();

			var exRate = exchangeRateCollection.AddNew();
			exRate.JF_RX_NKRateCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			exRate.JF_BaseRate = 1.75m;
			exRate.JF_OH_Org = TestObjectCreator.Debtor.PK;
			exRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			Factory.Save();

			Assert("EUR exchange rate is available and it is in database", exchangeRates.Any(x => x.JF_RX_NKRateCurrency == Core.Constants.CurrencyCodes.EuropeanUnion && x.IsInDatabase));

			Factory.SetContext(BusinessContext.ConvertingAmountsForExportAWBHeader);
			Assert(Factory.HasContext(BusinessContext.ConvertingAmountsForExportAWBHeader));
			converter.ConvertExact(money, TestObjectCreator.AUD, TestObjectCreator.Debtor.PK, CostSell.Revenue);
			eurExchangeRate = exchangeRates.FirstOrDefault(x => x.JF_RX_NKRateCurrency == Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertNotNull(eurExchangeRate);
			Assert("ShouldDeleteDuplicateExchangeRateBeforeSave must be false because EUR exchange rate is in DB.", !eurExchangeRate.ShouldDeleteDuplicateExchangeRateBeforeSave);
			Factory.RemoveContext(BusinessContext.ConvertingAmountsForExportAWBHeader);

			Assert(!Factory.HasContext(BusinessContext.ConvertingAmountsForExportAWBHeader));
			converter.ConvertExact(money, TestObjectCreator.AUD, TestObjectCreator.Debtor.PK, CostSell.Revenue);
			eurExchangeRate = exchangeRates.FirstOrDefault(x => x.JF_RX_NKRateCurrency == Core.Constants.CurrencyCodes.EuropeanUnion);
			AssertNotNull(eurExchangeRate);
			Assert("ShouldDeleteDuplicateExchangeRateBeforeSave must be false because EUR exchange rate is in DB.", !eurExchangeRate.ShouldDeleteDuplicateExchangeRateBeforeSave);
		}

		[ExpectNoExceptions()]
		public void TestCurrencyConverterFactoryConstructor()
		{
			new JobExRateCurrencyConverter(fExchangeRates, ZDateTime.Now, ExchangeRateType.Buy);
		}

		public void TestConvertExact()
		{
			ExchangeRate rate = fExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = DecoratedCurrency.RX_Code;
			rate.JF_BaseRate = 3;
			rate.JF_OH_Org = TestObjectCreator.Debtor.PK;
			rate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();

			rate = fExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = DecoratedCurrency.RX_Code;
			rate.JF_BaseRate = 4;
			rate.JF_OH_Org = TestObjectCreator.Creditor1.PK;
			rate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();

			var converter = new JobExRateCurrencyConverter(fExchangeRates, ZDateTime.Now, ExchangeRateType.Buy);
			var money = new Money(120m, DecoratedCurrency);
			AssertEquals("Amount conversion", 40m, converter.ConvertExact(money, TestObjectCreator.AUD, TestObjectCreator.Debtor.PK, CostSell.Revenue).Amount);
			AssertEquals("Amount conversion", 30m, converter.ConvertExact(money, TestObjectCreator.AUD, TestObjectCreator.Creditor1.PK, CostSell.Cost).Amount);
			AssertEquals("Amount conversion", 60m, converter.ConvertExact(money, TestObjectCreator.AUD, TestObjectCreator.Creditor1.PK, CostSell.Revenue).Amount);
			AssertEquals("Amount conversion", 60m, converter.ConvertExact(money, TestObjectCreator.AUD, TestObjectCreator.Debtor.PK, CostSell.Cost).Amount);
		}

		public void TestConvertExact_ForeignToLocalToForeignCurrencies()
		{
			ExchangeRate rate = fExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.EUR.Code;
			rate.JF_BaseRate = 0.5m;
			rate.JF_OH_Org = TestObjectCreator.Debtor.PK;
			rate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();

			rate = fExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rate.JF_BaseRate = 0.9m;
			rate.JF_OrgType = ExchangeRateOrgTypeEnum.None.ToCode();

			rate = fExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rate.JF_BaseRate = 0.7m;
			rate.JF_OH_Org = TestObjectCreator.Debtor.PK;
			rate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();

			var converter = new JobExRateCurrencyConverter(fExchangeRates, ZDateTime.Now, ExchangeRateType.Sell);
			var money = new Money(120m, TestObjectCreator.EUR);
			AssertEquals("Amount conversion happens with debtor specific exchange rate and not with generic exchange rate", 168m, converter.ConvertExact(money, TestObjectCreator.USD, TestObjectCreator.Debtor.PK, CostSell.Revenue).Amount);
		}

		public void TestGetExchangeRate()
		{
			JobExRateCurrencyConverter converter;

			converter = new JobExRateCurrencyConverter(fExchangeRates, ZDateTime.Now, ExchangeRateType.Buy);
			AssertEquals("Correct buy rate", 2m, converter.GetExchangeRate(DecoratedCurrency));

			converter = new JobExRateCurrencyConverter(fExchangeRates, ZDateTime.Now, ExchangeRateType.Sell);
			AssertEquals("Correct sell rate", 1m, converter.GetExchangeRate(DecoratedCurrency)); //cfx = 50m, non-reciprocal

			int nofCurrencies = converter.ExchangeRates.Count;
			AssertEquals("Local currency", 1m, converter.GetExchangeRate(GlbCompany.CurrentCompany.LocalCurrency));
			AssertEquals("LocalCurrency shouldn't be added to the list of currencies", nofCurrencies, converter.ExchangeRates.Count);
		}

		public void TestSellRateCalculatedFromBuyUsingCFX()
		{
			TestObjectCreator.Job1.JH_LocalChargesCFX = 0m;
			RefExchangeRate uSDSellRate = this.TestObjectCreator.USD.ExchangeRates.AddNew();
			uSDSellRate.RE_StartDate = ZDate.Today;
			uSDSellRate.RE_ExpiryDate = ZDate.Today.AddDays(10);
			uSDSellRate.RE_ExRateType = "SEL";
			uSDSellRate.RE_SellRate = 0.66m;

			RefExchangeRate uSDBuyRate = this.TestObjectCreator.USD.ExchangeRates.AddNew();
			uSDBuyRate.RE_StartDate = ZDate.Today;
			uSDBuyRate.RE_ExpiryDate = ZDate.Today.AddDays(10);
			uSDBuyRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			uSDBuyRate.RE_SellRate = 0.77m;

			Factory.Save();

			JobExRateCurrencyConverter converter = new JobExRateCurrencyConverter(TestObjectCreator.Job1.ExchangeRates, ZDateTime.Now, ExchangeRateType.Buy);
			AssertEquals("Should be buy rate of 0.77", 0.77m, converter.GetExchangeRate(TestObjectCreator.USD));

			TestObjectCreator.Job1.ExchangeRates.RemoveAndDeleteAll();
			converter = new JobExRateCurrencyConverter(TestObjectCreator.Job1.ExchangeRates, ZDateTime.Now, ExchangeRateType.Sell);
			AssertEquals("Should still be buy rate of 0.77", 0.77m, converter.GetExchangeRate(TestObjectCreator.USD));

			TestObjectCreator.Job1.ExchangeRates.RemoveAndDeleteAll();
			TestObjectCreator.Job1.LocalCharges.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 3m);
			converter = new JobExRateCurrencyConverter(TestObjectCreator.Job1.ExchangeRates, ZDateTime.Now, ExchangeRateType.Sell);
			//Here we create a new ex rate and there is nothing we can get CFX from
			AssertEquals("Should be buy rate adjusted for CFX of 0.7469", 0.77m, converter.GetExchangeRate(TestObjectCreator.USD));
		}

		public void TestGetExchangeRate_FallbackToNormalCurrencyConverter_WhenNoRateAvailable()
		{
			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate("ALL", "ALL", "ALL", "SEL", "TDR");
			GlbCompany.CurrentCompany.Factory.Save();

			Factory.Save(); //save DecoratedCurrency object

			var rate = 5m;
			TestObjectCreator.CreateExchangeRate(DecoratedCurrency, Core.Constants.ExchangeRateTypes.Code.BuyRate, rate, ZDateTime.Now.AddDays(3), ZDateTime.Now.AddDays(5));
			var newRate = 3.14m;
			TestObjectCreator.CreateExchangeRate(DecoratedCurrency, Core.Constants.ExchangeRateTypes.Code.SellRate, newRate, ZDateTime.Today, ZDateTime.Now.AddDays(2));

			var newFactory = new BusinessObjectFactory();
			var newObjectCreator = new TestObjectCreator(newFactory);
			var shipment = newObjectCreator.CreateShipment("S0001001");
			var job = newObjectCreator.CreateJob(shipment);
			newFactory.Save();

			var exchangeRates = new ExchangeRatesCollection(job, newFactory);
			var converter = new JobExRateCurrencyConverter(exchangeRates, ZDateTime.Now.AddDays(4), ExchangeRateType.Sell);
			var actualRate = converter.GetExchangeRate(DecoratedCurrency);
			AssertEquals("Actual rate should be 1", newRate, actualRate);
			AssertEquals("Actual rate should be 1", newRate, exchangeRates[0].JF_BaseRate);
		}

		#region Implementation

		protected ExchangeRatesCollection fExchangeRates;

		protected override void SetUp()
		{
			base.SetUp();
			var job = Factory.NewJobForTesting<Job>();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_JobNum = TestObjectCreator.GetRandomString(9);

			fExchangeRates = new ExchangeRatesCollection(job, Factory);
			ExchangeRate rate = fExchangeRates.AddNew();
			rate.JF_RX_NKRateCurrency = DecoratedCurrency.RX_Code;
			rate.JF_BaseRate = 2;
			rate.JF_CFXPercent = 50;
		}

		RefCurrency fDecoratedCurrency;
		protected RefCurrency DecoratedCurrency
		{
			get
			{
				if (fDecoratedCurrency == null)
				{
					fDecoratedCurrency = Factory.New<RefCurrency>();
					fDecoratedCurrency.RX_Code = "XXX";
					fDecoratedCurrency.RX_Desc = "Description";
					fDecoratedCurrency.RX_IsSystem = true;
					fDecoratedCurrency.RX_SubUnitName = "SubUnit";
					fDecoratedCurrency.RX_SubUnitRatio = 100;
					fDecoratedCurrency.RX_Symbol = "X";
					fDecoratedCurrency.RX_UnitName = "XXX";
				}
				return fDecoratedCurrency;
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
