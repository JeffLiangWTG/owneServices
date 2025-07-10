using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ExchangeRateCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2017, 01, 01)]
		public void TestUpdateConsolChargesExchangeRates_ARAPInvoicePostingExchangeRateOption_InvoiceDate()
		{
			AssertUpdateConsolChargesExchangeRates(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		[TestDate(2017, 01, 01)]
		public void TestUpdateConsolChargesExchangeRates_ARAPInvoicePostingExchangeRateOption_PostDate()
		{
			AssertUpdateConsolChargesExchangeRates(ExRateOption.ExchangeRateBasedOnPostDate.Code);
		}

		[TestDate(2017, 01, 01)]
		public void TestUpdateConsolChargesExchangeRates_ARAPInvoicePostingExchangeRateOption_EarliestInvoicePostDate()
		{
			AssertUpdateConsolChargesExchangeRates(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
		}

		[TestDate(2017, 01, 01)]
		public void TestUpdateConsolChargesExchangeRates_ARAPInvoicePostingExchangeRateOption_Today()
		{
			AssertUpdateConsolChargesExchangeRates(ExRateOption.TodayExchangeRate.Code);
		}

		[TestDate(2017, 01, 01)]
		public void TestUpdateConsolChargesExchangeRates_ARAPInvoicePostingExchangeRateOption_Default()
		{
			AssertUpdateConsolChargesExchangeRates(ExRateOption.Default.Code);
		}

		void AssertUpdateConsolChargesExchangeRates(string registryOption)
		{
			var rateForPostDate = 1.8m;
			var rateForInvoiceDate = 2.5m;
			var rateForTaxDate = 2.8m;
			var rateForToday = 3.7m;
			var manualRateOnCost = 4.6m;
			var postDate = ZDateTime.Today.AddDays(-2);
			var invoiceDate = ZDateTime.Today.AddDays(-5);
			var taxDate = ZDate.Today.AddDays(-6);
			var expectedRate = (registryOption == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code ? rateForInvoiceDate :
								(registryOption == ExRateOption.ExchangeRateBasedOnPostDate.Code ? rateForPostDate :
								(registryOption == ExRateOption.EarliestOfInvoiceOrTaxDate.Code ? rateForTaxDate :
								(registryOption == ExRateOption.TodayExchangeRate.Code ? rateForToday : manualRateOnCost))));

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(JobInvoicingConsumerTypes.ForwardingConsol.Code, Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All, Constants.ExchangeRateTypes.Code.SellRate, Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			GlbCompany.CurrentCompany.Factory.Save();

			TestObjectCreator.CreateTestPeriods(new ZDateTime(2017, 01, 01));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, rateForPostDate, postDate, postDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, rateForInvoiceDate, invoiceDate, invoiceDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, rateForTaxDate, taxDate, taxDate);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.SellRate, rateForToday, ZDateTime.Today, ZDateTime.Today);
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol();
			var shipment1 = TestObjectCreator.CreateShipment("S1000", consol);
			var shipment2 = TestObjectCreator.CreateShipment("S2000", consol);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			consolCost.E6_InvoiceNum = "INV123";
			consolCost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			consolCost.E6_InvoiceDate = invoiceDate;
			consolCost.E6_TaxDate = taxDate;
			consolCost.E6_ExchangeRate = manualRateOnCost;
			Factory.Save();

			AssertEquals(manualRateOnCost, consolCost.E6_ExchangeRate);
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			AssertEquals(manualRateOnCost, consolCost.ApportionmentCharges[0].JR_OSCostExRate);
			AssertEquals(manualRateOnCost, consolCost.ApportionmentCharges[1].JR_OSCostExRate);

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryOption))
			{
				var chargesToUpdate = Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, consolCost.ApportionmentCharges.GetPKs()));
				ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsPayable, chargesToUpdate, postDate);
				AssertEquals(expectedRate, consolCost.E6_ExchangeRate);
				AssertEquals(expectedRate, consolCost.ApportionmentCharges[0].JR_OSCostExRate);
				AssertEquals(expectedRate, consolCost.ApportionmentCharges[1].JR_OSCostExRate);
			}
		}

		[TestDate(2022, 5, 20)]
		public void TestUpdateChargesExchangeRates_ConsolChargesApplyRateViaCurrency()
		{
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ExRateOption.TodayExchangeRate.Code);
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);
			var masterTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(Factory);

			var config_AllCur = masterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, "AP", JobInvoicingConsumerTypes.ForwardingConsol.Code, "ALL", "ALL", currencyCodes: null);
			config_AllCur.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.C01Rate;
			var config_USD = masterTestObjectCreator.CreateAccExchangeRateConfiguration(AccExRateConfigurationLevelEnum.System, ZGuid.Empty, ZString.Empty, JobInvoicingConsumerTypes.ForwardingConsol.Code, "ALL", "ALL", currencyCodes: new string[] { Constants.CurrencyCodes.UnitedStates });
			config_USD.GetCurrencyConfig(Constants.CurrencyCodes.UnitedStates, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.C02Rate;

			const decimal rateForEUR = 5m;
			const decimal rateForUSD = 4m;
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, Constants.ExchangeRateTypes.Code.C01Rate, rateForEUR, new ZDateTime(2022, 5, 16), new ZDateTime(2022, 5, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.C02Rate, rateForUSD, new ZDateTime(2022, 5, 16), new ZDateTime(2022, 5, 31));
			Factory.Save();

			var consol = TestObjectCreator.CreateConsol();
			TestObjectCreator.CreateShipment("S1000", consol);
			TestObjectCreator.CreateShipment("S2000", consol);

			const decimal defaultExchangeRate = 3.14159m;
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			consolCost.E6_ExchangeRate = defaultExchangeRate;
			Factory.Save();

			AssertEquals(defaultExchangeRate, consolCost.E6_ExchangeRate);
			AssertEquals(2, consolCost.ApportionmentCharges.Count);

			var chargesToUpdate = Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, consolCost.ApportionmentCharges.GetPKs()));
			ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsPayable, chargesToUpdate, ZDateTime.Today);
			AssertEquals(rateForUSD, consolCost.E6_ExchangeRate);
			AssertEquals(rateForUSD, consolCost.ApportionmentCharges[0].JR_OSCostExRate);
			AssertEquals(rateForUSD, consolCost.ApportionmentCharges[1].JR_OSCostExRate);

			consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.EuropeanUnion;
			ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsPayable, chargesToUpdate, ZDateTime.Today);
			AssertEquals(rateForEUR, consolCost.E6_ExchangeRate);
			AssertEquals(rateForEUR, consolCost.ApportionmentCharges[0].JR_OSCostExRate);
			AssertEquals(rateForEUR, consolCost.ApportionmentCharges[1].JR_OSCostExRate);
		}

		public void TestUpdateChargesExchangeRates_WhenTaxDateIsEmptyAndUseEarliestOfInvoiceOrTaxDate()
		{
			var consol = TestObjectCreator.CreateConsol();
			TestObjectCreator.CreateShipment("S1000", consol);
			TestObjectCreator.CreateShipment("S2000", consol);

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			consolCost.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			consolCost.E6_InvoiceNum = "INV123";
			consolCost.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			consolCost.E6_InvoiceDate = ZDateTime.Today.AddDays(-5);
			consolCost.E6_ExchangeRate = 4.6m;

			Factory.Save();

			using (PostingExRateRegistryAP.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ExRateOption.EarliestOfInvoiceOrTaxDate.Code))
			{
				var chargesToUpdate = Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, consolCost.ApportionmentCharges.GetPKs()));
				ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsPayable, chargesToUpdate, ZDateTime.Today.AddDays(-2));
				Assert(chargesToUpdate.All(x => x.JR_CostTaxDate == ZDate.Today));
				Assert(consolCost.E6_TaxDate == ZDate.Today);
			}
		}

		public void TestGetExchangeRateDate_EarliestOfInvoiceOrTaxDate()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var today = ZDateTime.Today;
			var otherDate = ZDateTime.Today.AddDays(-2);

			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExRateOption.EarliestOfInvoiceOrTaxDate.Code))
			{
				AssertEquals(yesterday, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, true, GlbCompany.CurrentCompany.PK, yesterday, otherDate, yesterday));
				AssertEquals(yesterday, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, true, GlbCompany.CurrentCompany.PK, today, otherDate, yesterday));
				AssertEquals(yesterday, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, true, GlbCompany.CurrentCompany.PK, yesterday, otherDate, today));
				AssertEquals(yesterday, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, true, GlbCompany.CurrentCompany.PK, yesterday, otherDate, ZDateTime.Invalid));
			}
		}

		[TestDate(2022, 05, 15)]
		public void TestGetExchangeRateDate()
		{
			var today = ZDateTime.Today;
			var invoiceDate = new ZDateTime(2022, 05, 14);
			var postDate = new ZDateTime(2022, 05, 13);
			var taxDate = new ZDateTime(2022, 05, 12);

			AssertGetExchangeRateDate(today, ExRateOption.Default.Code);
			AssertGetExchangeRateDate(today, ExRateOption.TodayExchangeRate.Code);
			AssertGetExchangeRateDate(invoiceDate, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			AssertGetExchangeRateDate(postDate, ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertGetExchangeRateDate(taxDate, ExRateOption.EarliestOfInvoiceOrTaxDate.Code);

			void AssertGetExchangeRateDate(ZDateTime expectedDate, string registryValue)
			{
				AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, true, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate, registryValue));
				AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, false, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate, registryValue));
				AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AR, true, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate, registryValue));
				AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AR, false, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate, registryValue));

				using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				{
					AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, true, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate));
					AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, false, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate));
				}
				using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				{
					AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AR, true, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate));
					AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AR, false, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate));
				}
			}
		}

		[TestDate(2022, 05, 15)]
		public void TestGetExchangeRateDate_Invoice()
		{
			var today = ZDateTime.Today;
			var invoiceDate = new ZDateTime(2022, 05, 14);
			var postDate = new ZDateTime(2022, 05, 13);
			var taxDate = new ZDateTime(2022, 05, 12);

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD, null, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 100m, TestObjectCreator.USD);

			invoice.AH_InvoiceDate = new ZDateTime(2022, 05, 14);
			invoice.AH_PostDate = new ZDateTime(2022, 05, 13);
			line.AL_TaxDate = new ZDate(2022, 05, 12);

			AssertEquals(new ZDate(2022, 05, 12), invoice.InvoiceTaxDate);

			AssertGetExchangeRateDate(today, ExRateOption.Default.Code);
			AssertGetExchangeRateDate(today, ExRateOption.TodayExchangeRate.Code);
			AssertGetExchangeRateDate(invoiceDate, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			AssertGetExchangeRateDate(postDate, ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertGetExchangeRateDate(taxDate, ExRateOption.EarliestOfInvoiceOrTaxDate.Code);

			void AssertGetExchangeRateDate(ZDateTime expectedDate, string registryValue)
			{
				using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				{
					AssertEquals(expectedDate, ExchangeRateCalculator.GetExchangeRateDate(invoice));
				}
			}
		}

		[TestDate(2022, 05, 15)]
		public void TestGetExchangeRateDate_InvoicePostingOffSet()
		{
			var today = ZDateTime.Today;
			var invoiceDate = new ZDateTime(2022, 05, 14);
			var postDate = new ZDateTime(2022, 05, 13);
			var taxDate = new ZDateTime(2022, 05, 12);

			var collectionAP = new InvoicePostingExRateOptionCollection();
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, 2));
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, ExRateOption.ExchangeRateBasedOnPostDate.Code, 0));
			var collectionAR = new InvoicePostingExRateOptionCollection();
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, -3));
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, ExRateOption.TodayExchangeRate.Code, 1));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAP);

			AssertEquals(invoiceDate.AddDays(2), ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, false, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate));
			AssertEquals(postDate, ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AP, true, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate));
			AssertEquals(taxDate.AddDays(-3), ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AR, false, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate));
			AssertEquals(today.AddDays(1), ExchangeRateCalculator.GetExchangeRateDate(ExchangeRateValidLedgerEnum.AR, true, GlbCompany.CurrentCompany.PK, invoiceDate, postDate, taxDate));
		}

		[TestDate(2022, 10, 12)]
		public void TestGetOverrideExchangeRateOffSet()
		{
			var companyAU = GlbCompany.CurrentCompany;
			var companyUS = TestObjectCreator.CreateNewCompany("CUS");
			companyUS.GC_RX_NKLocalCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals("PreCond: companyAU use currency AUD", TestObjectCreator.AUD.RX_Code, companyAU.GC_RX_NKLocalCurrency);
			AssertEquals("PreCond: companyUS use currency USD", TestObjectCreator.USD.RX_Code, companyUS.GC_RX_NKLocalCurrency);

			var dateToUse = ZDateTime.Today;
			var dateNotToBeUsed = new ZDateTime(2022, 10, 20);
			AssertEquals("PreCond: Today's date is 12/10/2022", new ZDateTime(2022, 10, 12), ZDateTime.Today);

			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, dateToUse.AddDays(-2), dateToUse.AddDays(-2));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 3m, dateToUse.AddDays(-1), dateToUse.AddDays(-1));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 4m, dateToUse, dateToUse);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 5m, dateToUse.AddDays(1), dateToUse.AddDays(1));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 6m, dateToUse.AddDays(2), dateToUse.AddDays(2));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 7m, dateToUse.AddDays(3), dateToUse.AddDays(3));

			SetRegistryExRateOptionRegistry(ExRateOption.TodayExchangeRate.Code);
			AssertGetOverrideExchangeRateResult(dateNotToBeUsed, dateNotToBeUsed, dateNotToBeUsed);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			AssertEquals("PreCond: Today's date is 13/10/2022", new ZDateTime(2022, 10, 13), ZDateTime.Today);

			SetRegistryExRateOptionRegistry(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			AssertGetOverrideExchangeRateResult(dateToUse, dateNotToBeUsed, dateNotToBeUsed);

			SetRegistryExRateOptionRegistry(ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertGetOverrideExchangeRateResult(dateNotToBeUsed, dateToUse, dateNotToBeUsed);

			//EIT - invoice date is before tax date
			SetRegistryExRateOptionRegistry(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertGetOverrideExchangeRateResult(dateToUse, dateNotToBeUsed, dateNotToBeUsed);

			//EIT - tax date is before invoice date
			SetRegistryExRateOptionRegistry(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertGetOverrideExchangeRateResult(dateNotToBeUsed, dateNotToBeUsed, dateToUse);

			//EIT - tax date equals invoice date
			SetRegistryExRateOptionRegistry(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertGetOverrideExchangeRateResult(dateToUse, dateNotToBeUsed, dateToUse);

			void AssertGetOverrideExchangeRateResult(ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate)
			{
				AssertEquals("OffSet is zero", 4m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", true, companyUS.PK, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
				AssertEquals("OffSet is 1", 5m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyAU.PK, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
				AssertEquals("OffSet is 2", 6m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyAU.PK, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
				AssertEquals("OffSet is -1", 3m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", true, companyUS.PK, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
			}
		}

		[TestDate(2022, 10, 12)]
		public void TestGetOverrideExchangeRateOffSetwithInvalidDate()
		{
			var invalidDate = ZDateTime.Invalid;
			var emptyDate = ZDateTime.Empty;
			var companyAU = GlbCompany.CurrentCompany;

			SetRegistryExRateOptionRegistry(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			AssertEmptyExchangeRatewithInvalidDate(emptyDate, invalidDate, invalidDate);

			SetRegistryExRateOptionRegistry(ExRateOption.ExchangeRateBasedOnPostDate.Code);
			AssertEmptyExchangeRatewithInvalidDate(emptyDate, invalidDate, invalidDate);

			SetRegistryExRateOptionRegistry(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertEmptyExchangeRatewithInvalidDate(emptyDate, invalidDate, invalidDate);

			SetRegistryExRateOptionRegistry(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
			AssertEmptyExchangeRatewithInvalidDate(invalidDate, invalidDate, emptyDate);

			void AssertEmptyExchangeRatewithInvalidDate(ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate)
			{
				CombineAssertions("When invoiceDate or postDate is invalid and it's used as a based date to caculate exchange rate, exchange rate will be 0m no matter what offset is", () =>
				{
					AssertEquals("OffSet is zero", 0m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", true, companyAU.PK, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
					AssertEquals("OffSet is 1", 0m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyAU.PK, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AR, invoiceDate, postDate, taxDate));
					AssertEquals("OffSet is 2", 0m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyAU.PK, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
					AssertEquals("OffSet is -1", 0m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", true, companyAU.PK, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AP, invoiceDate, postDate, taxDate));
				});
			}
		}

		void SetRegistryExRateOptionRegistry(string exRateOptionValue)
		{
			var collectionAR = new InvoicePostingExRateOptionCollection();
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, exRateOptionValue, 1));
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, exRateOptionValue, 0));
			var collectionAP = new InvoicePostingExRateOptionCollection();
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, exRateOptionValue, 2));
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, exRateOptionValue, -1));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAP);
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);
		}

		public void TestIsExRateOptionApplicable()
		{
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AP, false, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AP, true, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AR, false, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AR, true, false);

			AssertIsExRateOptionApplicable(ExRateOption.TodayExchangeRate.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AP, false, true);
			AssertIsExRateOptionApplicable(ExRateOption.TodayExchangeRate.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AP, true, false);
			AssertIsExRateOptionApplicable(ExRateOption.TodayExchangeRate.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AR, false, false);
			AssertIsExRateOptionApplicable(ExRateOption.TodayExchangeRate.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AR, true, false);

			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AP, false, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AP, true, true);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AR, false, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AR, true, false);

			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AP, false, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AP, true, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AR, false, true);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code, ExRateOption.Default.Code, ExchangeRateValidLedgerEnum.AR, true, false);

			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExchangeRateValidLedgerEnum.AP, false, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExchangeRateValidLedgerEnum.AP, true, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExchangeRateValidLedgerEnum.AR, false, false);
			AssertIsExRateOptionApplicable(ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.Default.Code, ExRateOption.EarliestOfInvoiceOrTaxDate.Code, ExchangeRateValidLedgerEnum.AR, true, true);
		}

		void AssertIsExRateOptionApplicable(string apForeignOption, string apLocalOption, string arForeignOption, string arLocalOption, ExchangeRateValidLedgerEnum ledger, bool isLocalCurrency, bool expectedValue)
		{
			var companyPK = GlbCompany.CurrentCompany.PK;

			var collectionAP = new InvoicePostingExRateOptionCollection();
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, apForeignOption, 0));
			collectionAP.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, apLocalOption, 0));

			var collectionAR = new InvoicePostingExRateOptionCollection();
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, arForeignOption, 0));
			collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, arLocalOption, 0));

			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAP);
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);

			AssertEquals(expectedValue, ExchangeRateCalculator.IsExRateOptionApplicable(ledger, isLocalCurrency, companyPK));

			if (expectedValue)
			{
				var expectedExchangeRateOption = ledger == ExchangeRateValidLedgerEnum.AP
					? isLocalCurrency ? apLocalOption : apForeignOption
					: isLocalCurrency ? arLocalOption : arForeignOption;

				var notExpectedOption = new[] { ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, ExRateOption.ExchangeRateBasedOnPostDate.Code }.First(x => x != expectedExchangeRateOption);

				AssertEquals(true, ExchangeRateCalculator.IsExRateOptionApplicable(ledger, isLocalCurrency, companyPK, expectedExchangeRateOption));
				AssertEquals(false, ExchangeRateCalculator.IsExRateOptionApplicable(ledger, isLocalCurrency, companyPK, notExpectedOption));
			}
		}

		public void TestIsInLocalInvoiceCurrencyForPosting()
		{
			var localInvoiceTypes = new[] { InvoiceTypesList.Codes.FinalInvoice, InvoiceTypesList.Codes.FinalInvoice_Batching,
				InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTypesList.Codes.DisbursementInvoice_Batching, InvoiceTypesList.Codes.InvoicePerTaxCode,
				InvoiceTypesList.Codes.InvoicePerTaxCode_Batching, InvoiceTypesList.Codes.DestinationChargesInvoice, InvoiceTypesList.Codes.DestinationChargesInvoice_Batching,
				AgencyInvoiceTypesList.Codes.LocalCollect, AgencyInvoiceTypesList.Codes.LocalCollect_Batching,
				AgencyInvoiceTypesList.Codes.LocalPrePaid, AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching,
				AgencyInvoiceTypesList.Codes.Misc, AgencyInvoiceTypesList.Codes.Misc_Batching };

			var foreignInvoiceTypes = new[] { InvoiceTypesList.Codes.ForeignCurrencyInvoice, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching,
				InvoiceTypesList.Codes.FreightInvoice, InvoiceTypesList.Codes.FreightInvoice_Batching, InvoiceTypesList.Codes.DisbursementInForeignCurrency,
				InvoiceTypesList.Codes.DisbursementInForeignCurrency_Batching, AgencyInvoiceTypesList.Codes.ForeignCollect, AgencyInvoiceTypesList.Codes.ForeignCollect_Batching,
				AgencyInvoiceTypesList.Codes.ForeignPrePaid, AgencyInvoiceTypesList.Codes.ForeignPrePaid_Batching };

			var charge = Factory.NewWithValidTestData<Charge>();
			var localCurrency = charge.Company.GC_RX_NKLocalCurrency;
			var foreignCurrency = TestObjectCreator.USD.RX_Code;
			AssertNotEquals(localCurrency, foreignCurrency);

			charge.JR_InvoiceType = foreignInvoiceTypes.First();
			charge.JR_RX_NKSellCurrency = foreignCurrency;
			charge.JR_RX_NKSellInvoiceCurrency = localCurrency;
			Assert("when sell invoice currency is not empty, we compare it to the company local currency", charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));

			charge.JR_InvoiceType = localInvoiceTypes.First();
			charge.JR_RX_NKSellCurrency = localCurrency;
			charge.JR_RX_NKSellInvoiceCurrency = foreignCurrency;
			Assert("when sell invoice currency is not empty, we compare it to the company local currency", !charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));

			charge.JR_RX_NKSellInvoiceCurrency = string.Empty;
			charge.JR_RX_NKSellCurrency = localCurrency;
			foreach (var foreignInvoiceType in foreignInvoiceTypes)
			{
				charge.JR_InvoiceType = foreignInvoiceType;
				Assert("when sell invoice currency is empty, we check the charge invoice type", !charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			}

			charge.JR_RX_NKSellCurrency = foreignCurrency;
			foreach (var localInvoiceType in localInvoiceTypes)
			{
				charge.JR_InvoiceType = localInvoiceType;
				Assert("when sell invoice currency is empty, we check the charge invoice type", charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			}

			//when sell invoice currency is empty and invoice type is self billing, then we compare the sell currency to the local currency
			foreach (var sellBillingInvoiceType in new[] { InvoiceTypesList.Codes.SelfBillingInvoice, InvoiceTypesList.Codes.SelfBillingInvoice_Batching })
			{
				charge.JR_RX_NKSellCurrency = foreignCurrency;
				charge.JR_InvoiceType = sellBillingInvoiceType;
				Assert(!charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
				charge.JR_RX_NKSellCurrency = localCurrency;
				charge.JR_InvoiceType = sellBillingInvoiceType;
				Assert(charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			}

			charge.JR_RX_NKCostCurrency = localCurrency;
			Assert(charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AP));
			charge.JR_RX_NKCostCurrency = foreignCurrency;
			Assert(!charge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AP));
		}

		public void TestGetRate()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, new DateTime(2000, 01, 01), new DateTime(2015, 01, 01));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 3m, new DateTime(2000, 01, 01), new DateTime(2015, 01, 31));

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 100m, new DateTime(2016, 01, 01), new DateTime(2016, 01, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 100m, new DateTime(2016, 01, 01), new DateTime(2016, 01, 31));

			var dateNotExpired = new DateTime(2015, 1, 1);
			var dateExpired = new DateTime(2015, 2, 1);

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals(2m, ExchangeRateCalculator.GetRate("USD", ExchangeRateType.Buy, dateNotExpired));
			AssertEquals(0m, ExchangeRateCalculator.GetRate("USD", ExchangeRateType.Buy, dateExpired));
			AssertEquals(3m, ExchangeRateCalculator.GetRate("USD", ExchangeRateType.Sell, dateNotExpired));
			AssertEquals(0m, ExchangeRateCalculator.GetRate("USD", ExchangeRateType.Sell, dateExpired));

			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals(2m, ExchangeRateCalculator.GetRate("USD", ExchangeRateType.Buy, dateNotExpired));
			AssertEquals("fall back exchange rate will not apply future date", 2m, ExchangeRateCalculator.GetRate("USD", ExchangeRateType.Buy, dateExpired));
			AssertEquals(3m, ExchangeRateCalculator.GetRate("USD", ExchangeRateType.Sell, dateNotExpired));
			AssertEquals("fall back exchange rate will not apply future date", 3m, ExchangeRateCalculator.GetRate("USD", ExchangeRateType.Sell, dateExpired));

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 01, 01)]
		public void TestGetOverrideExchangeRateAR() => AssertGetOverrideExchangeRate(ExchangeRateValidLedgerEnum.AR);
		[TestDate(2015, 01, 01)]
		public void TestGetOverrideExchangeRateAP() => AssertGetOverrideExchangeRate(ExchangeRateValidLedgerEnum.AP);
		void AssertGetOverrideExchangeRate(ExchangeRateValidLedgerEnum ledger)
		{
			var postingExRateRegistry = GetInvoicePostingExchangeRateOptionRegistryItem(ledger);
			var companyPK = GlbCompany.CurrentCompany.PK;

			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, new DateTime(2000, 01, 01), new DateTime(2014, 12, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 3m, new DateTime(2000, 01, 01), new DateTime(2014, 12, 31));

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 4m, new DateTime(2015, 01, 01), new DateTime(2015, 01, 01));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 5m, new DateTime(2015, 01, 01), new DateTime(2015, 01, 01));

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 6m, new DateTime(2015, 01, 02), new DateTime(2015, 12, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 7m, new DateTime(2015, 01, 02), new DateTime(2015, 12, 31));

			AssertEquals(false, ExchangeRateCalculator.IsExRateOptionApplicable(ledger, true, companyPK));
			AssertEquals(false, ExchangeRateCalculator.IsExRateOptionApplicable(ledger, false, companyPK));

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			AssertEquals(true, ExchangeRateCalculator.IsExRateOptionApplicable(ledger, true, companyPK));
			AssertEquals(true, ExchangeRateCalculator.IsExRateOptionApplicable(ledger, false, companyPK));

			var postDate = new ZDateTime(2014, 1, 1);
			var invoiceDate = new ZDateTime(2015, 1, 2);
			var taxDate = new ZDateTime(2016, 1, 1);

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			AssertEquals("DEF use today's date", 4m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyPK, ExchangeRateType.Buy, ledger, invoiceDate, postDate, taxDate));
			AssertEquals("DEF use today's date", 5m, ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyPK, ExchangeRateType.Sell, ledger, invoiceDate, postDate, taxDate));

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			var rate = ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyPK, ExchangeRateType.Buy, ledger, invoiceDate, postDate, taxDate);
			AssertEquals("PST rate", 2m, rate);
			rate = ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyPK, ExchangeRateType.Sell, ledger, invoiceDate, postDate, taxDate);
			AssertEquals("PST rate", 3m, rate);

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			rate = ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyPK, ExchangeRateType.Buy, ledger, invoiceDate, postDate, taxDate);
			AssertEquals("TOD rate", 4m, rate);
			rate = ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyPK, ExchangeRateType.Sell, ledger, invoiceDate, postDate, taxDate);
			AssertEquals("TOD rate", 5m, rate);

			postingExRateRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			rate = ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyPK, ExchangeRateType.Buy, ledger, invoiceDate, postDate, taxDate);
			AssertEquals("INV rate", 6m, rate);
			rate = ExchangeRateCalculator.GetOverrideExchangeRate("USD", false, companyPK, ExchangeRateType.Sell, ledger, invoiceDate, postDate, taxDate);
			AssertEquals("INV rate", 7m, rate);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void TestCheckExchangeRateErrorMessageShowsExpectedRateAndInputRateAP() => AssertCheckExchangeRateErrorMessageShowsExpectedRateAndInputRate(ExchangeRateValidLedgerEnum.AP);
		public void TestCheckExchangeRateErrorMessageShowsExpectedRateAndInputRateAR() => AssertCheckExchangeRateErrorMessageShowsExpectedRateAndInputRate(ExchangeRateValidLedgerEnum.AR);
		public void AssertCheckExchangeRateErrorMessageShowsExpectedRateAndInputRate(ExchangeRateValidLedgerEnum ledger)
		{
			var ledgerStr = ledger == ExchangeRateValidLedgerEnum.AP ? "AP" : "AR";
			var postingExRateRegistry = GetInvoicePostingExchangeRateOptionRegistryItem(ledger);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2.7777m, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			postingExRateRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			var result = ExchangeRateCalculator.CheckExchangeRate("USD", false, GlbCompany.CurrentCompany, ExchangeRateType.Buy, ledger, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today, 1.2345);
			AssertNotNull(result);
			AssertEquals("Notification type", ExchangeRateCalculator.OverridenExchangeRate, result.Type);
			AssertEquals($@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 2.777700 rate but 1.234500 was entered.", result.Message);
		}

		[TestDate(2018, 6, 10)]
		public void TestCheckExchangeRateForMissingExchangeRateAR() => AssertCheckExchangeRateForMissingExchangeRate(ExchangeRateValidLedgerEnum.AR);

		[TestDate(2018, 6, 10)]
		public void TestCheckExchangeRateForMissingExchangeRateAP() => AssertCheckExchangeRateForMissingExchangeRate(ExchangeRateValidLedgerEnum.AP);

		void AssertCheckExchangeRateForMissingExchangeRate(ExchangeRateValidLedgerEnum ledger)
		{
			var ledgerStr = ledger == ExchangeRateValidLedgerEnum.AP ? "AP" : "AR";
			var expectedDate = new ZDateTime(2018, 6, 10);
			var otherDate = new ZDateTime(2018, 6, 15);

			AssertEquals("PreCond: Today's date is expectedDate", expectedDate, ZDateTime.Today);
			AssertGetZeroExchangeRateErrorMessage(ExRateOption.TodayExchangeRate.Code, otherDate, otherDate, otherDate, GetErrorMessage("Today Exchange Rate"));

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			AssertEquals("PreCond: Today's date is different from expectedDate", new ZDateTime(2018, 6, 11), ZDateTime.Today);

			AssertGetZeroExchangeRateErrorMessage(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, expectedDate, otherDate, otherDate, GetErrorMessage("Exchange Rate based on Invoice Date"));
			AssertGetZeroExchangeRateErrorMessage(ExRateOption.ExchangeRateBasedOnPostDate.Code, otherDate, expectedDate, otherDate, GetErrorMessage("Exchange Rate based on Post Date"));
			AssertGetZeroExchangeRateErrorMessage(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, expectedDate, otherDate, otherDate, GetErrorMessage("Earliest of Invoice Date and Tax Date"));
			AssertGetZeroExchangeRateErrorMessage(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, otherDate, otherDate, expectedDate, GetErrorMessage("Earliest of Invoice Date and Tax Date"));
			AssertGetZeroExchangeRateErrorMessage(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, expectedDate, otherDate, expectedDate, GetErrorMessage("Earliest of Invoice Date and Tax Date"));

			void AssertGetZeroExchangeRateErrorMessage(string exRateOption, ZDateTime invoiceDate, ZDateTime postDate, ZDateTime taxDate, string expectedError)
			{
				GetInvoicePostingExchangeRateOptionRegistryItem(ledger).SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, exRateOption);
				var result = ExchangeRateCalculator.CheckExchangeRate("USD", false, GlbCompany.CurrentCompany, ExchangeRateType.Buy, ledger, invoiceDate, postDate, taxDate, 1.2345);
				AssertNotNull(result);
				AssertEquals("Notification type", ExchangeRateCalculator.MissingExchangeRate, result.Type);
				AssertEquals(expectedError, result.Message);
			}

			string GetErrorMessage(string exRateOptionDescription) => $@"The ""{ledgerStr} Invoice Posting Exchange Rate Option"" has been set to ""{exRateOptionDescription}"".
But the USD exchange rate is not set for the date 10-Jun-18. Please check your data and try again.";
		}

		InvoicePostingExRateOptionRegistryItem GetInvoicePostingExchangeRateOptionRegistryItem(ExchangeRateValidLedgerEnum ledger)
			=> ledger == ExchangeRateValidLedgerEnum.AR
				? AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR
				: AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;

		[TestDate(2020, 11, 18)]
		public void TestCheckInvoiceExchangeRate()
		{
			void setupConfig(AccExchangeRateConfigurationCollection configCol, string exRateType)
			{
				var config = configCol.AddNew();
				config.JCE_JobType = "ALL";
				config.JCE_ServiceDirection = "ALL";
				config.JCE_TransportMode = "ALL";
				config.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = exRateType;
			}

			ExchangeRateReader.GetReaderInstance().ClearCache();
			var rateFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(rateFactory);
			creator.CreateExchangeRate(creator.USD, Constants.ExchangeRateTypes.Code.C15Rate, 0.15m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			rateFactory.Save();

			setupConfig(creator.AALSHI.CompanyData.AccARExchangeRateConfigurations, Constants.ExchangeRateTypes.Code.C15Rate);

			var shipment = creator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = creator.CreateJob(shipment, false);
			job.PlugInData = shipment;

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.USD, null, creator.AALSHI);
			invoice.AH_PostDate = new ZDateTime(2020, 11, 17);
			invoice.AH_InvoiceDate = new ZDateTime(2020, 11, 17);
			invoice.AH_JH = job.PK;

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AC = creator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_AT = creator.GSTFREE1.PK;
			line.AL_OSExTaxAmount = 100m;

			creator.CreateCharge(line, job, creator.CC1, creator.USD);
			creator.Factory.Save();

			using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			{
				var result = ExchangeRateCalculator.CheckInvoiceExchangeRate(invoice);
				AssertEquals("Expect no error in result", string.Empty, result);
			}
		}

		[TestDate(2020, 6, 19)]
		public void TestCheckExchangeRateErrorMessage_ExchangeRateConfiguration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var orgWithARExchangeRateConfigurations = TestObjectCreator.CreateOrgHeader("TEST1", true, true);
				var orgWithoutARExchangeRateConfigurations = TestObjectCreator.CreateOrgHeader("TEST2", true, true);

				orgWithARExchangeRateConfigurations.CompanyData.AccARExchangeRateConfigurations.RemoveAndDeleteAll();
				orgWithoutARExchangeRateConfigurations.CompanyData.AccARExchangeRateConfigurations.RemoveAndDeleteAll();
				orgWithARExchangeRateConfigurations.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", "ALL", "ALL", "ALL", "SEL", "TDR", 0, false);

				var orgWithAPExchangeRateConfigurations = TestObjectCreator.CreateOrgHeader("TEST3", true, true);
				var orgWithoutAPExchangeRateConfigurations = TestObjectCreator.CreateOrgHeader("TEST4", true, true);

				orgWithAPExchangeRateConfigurations.CompanyData.AccAPExchangeRateConfigurations.RemoveAndDeleteAll();
				orgWithoutAPExchangeRateConfigurations.CompanyData.AccAPExchangeRateConfigurations.RemoveAndDeleteAll();
				orgWithAPExchangeRateConfigurations.CompanyData.AccAPExchangeRateConfigurations.SetExRate("AP", "ALL", "ALL", "ALL", "SEL", "TDR", 0, false);

				AssertEquals("Pre-condition", 1, orgWithARExchangeRateConfigurations.CompanyData.AccARExchangeRateConfigurations.Count);
				AssertEquals("Pre-condition", 0, orgWithoutARExchangeRateConfigurations.CompanyData.AccARExchangeRateConfigurations.Count);
				AssertEquals("Pre-condition", 1, orgWithAPExchangeRateConfigurations.CompanyData.AccAPExchangeRateConfigurations.Count);
				AssertEquals("Pre-condition", 0, orgWithoutAPExchangeRateConfigurations.CompanyData.AccAPExchangeRateConfigurations.Count);

				var shipment = TestObjectCreator.CreateShipment("S1");
				var job = TestObjectCreator.CreateJob(shipment, false);
				var invoiceDate = ZDateTime.Today.AddDays(-1);
				var postDate = ZDateTime.Today.AddDays(1);
				var taxDate = ZDateTime.Today.AddDays(-2);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 6.7777m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, Constants.ExchangeRateTypes.Code.BuyRate, 6.1111m, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

				foreach (var exRateOption in new CodeDescriptionPair[] {
					ExRateOption.Default,
					ExRateOption.TodayExchangeRate,
					ExRateOption.ExchangeRateBasedOnInvoiceDate,
					ExRateOption.EarliestOfInvoiceOrTaxDate,
					ExRateOption.ExchangeRateBasedOnPostDate })
				{
					var expectedDate = exRateOption.Code == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code
						? invoiceDate : exRateOption.Code == ExRateOption.ExchangeRateBasedOnPostDate.Code
						? postDate : exRateOption.Code == ExRateOption.EarliestOfInvoiceOrTaxDate.Code
						? taxDate : ZDateTime.Today;

					using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, exRateOption.Code))
					{
						var expectedARMessage = exRateOption.Code == ExRateOption.Default.Code ? "" : $@"The ""AR Invoice Posting Exchange Rate Option"" has been set to ""{exRateOption.Description}"".
But the USD exchange rate is not set for the date {expectedDate.ToShortDateString()}. Please check your data and try again.";

						var result = ExchangeRateCalculator.CheckExchangeRate(job, "USD", false, ExchangeRateValidLedgerEnum.AR, orgWithARExchangeRateConfigurations.PK, invoiceDate, postDate, taxDate);
						AssertEquals("Check foreign currency exchange rate with the exchange rate configuration", expectedARMessage, result);

						result = ExchangeRateCalculator.CheckExchangeRate(job, "USD", false, ExchangeRateValidLedgerEnum.AR, orgWithoutARExchangeRateConfigurations.PK, invoiceDate, postDate, taxDate);
						AssertEquals("Check foreign currency exchange rate without the exchange rate configuration", "", result);

						result = ExchangeRateCalculator.CheckExchangeRate(job, "CNY", true, ExchangeRateValidLedgerEnum.AR, orgWithARExchangeRateConfigurations.PK, invoiceDate, postDate, taxDate);
						AssertEquals("Check local currency exchange rate without the exchange rate configuration", "", result);

						result = ExchangeRateCalculator.CheckExchangeRate(job, "CNY", true, ExchangeRateValidLedgerEnum.AR, orgWithoutARExchangeRateConfigurations.PK, invoiceDate, postDate, taxDate);
						AssertEquals("Check local currency exchange rate without the exchange rate configuration", "", result);
					}

					using (AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, exRateOption.Code))
					{
						var expectedAPMessage = exRateOption.Code == ExRateOption.Default.Code ? "" : $@"The ""AP Invoice Posting Exchange Rate Option"" has been set to ""{exRateOption.Description}"".
But the EUR exchange rate is not set for the date {expectedDate.ToShortDateString()}. Please check your data and try again.";

						var result = ExchangeRateCalculator.CheckExchangeRate(job, "EUR", false, ExchangeRateValidLedgerEnum.AP, orgWithAPExchangeRateConfigurations.PK, invoiceDate, postDate, taxDate);
						AssertEquals("Check foreign currency exchange rate with the exchange rate configuration", expectedAPMessage, result);

						result = ExchangeRateCalculator.CheckExchangeRate(job, "EUR", false, ExchangeRateValidLedgerEnum.AP, orgWithoutAPExchangeRateConfigurations.PK, invoiceDate, postDate, taxDate);
						AssertEquals("Check foreign currency exchange rate without the exchange rate configuration", "", result);

						result = ExchangeRateCalculator.CheckExchangeRate(job, "CNY", true, ExchangeRateValidLedgerEnum.AP, orgWithAPExchangeRateConfigurations.PK, invoiceDate, postDate, taxDate);
						AssertEquals("Check local currency exchange rate without the exchange rate configuration", "", result);

						result = ExchangeRateCalculator.CheckExchangeRate(job, "CNY", true, ExchangeRateValidLedgerEnum.AP, orgWithoutAPExchangeRateConfigurations.PK, invoiceDate, postDate, taxDate);
						AssertEquals("Check local currency exchange rate without the exchange rate configuration", "", result);
					}
				}
			}
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_PostDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.ExchangeRateBasedOnPostDate.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_InvoiceDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_EarliestInvoicePostDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_TodayOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.TodayExchangeRate.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_DefaultOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.Default.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_PostDateOption_RateType()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.ExchangeRateBasedOnPostDate.Code, "EUR", useExRateConfig: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_InvoiceDateOption_RateType()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, "EUR", useExRateConfig: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_EarliestInvoicePostDateOption_RateType()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, "EUR", useExRateConfig: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_TodayOption_RateType()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.TodayExchangeRate.Code, "EUR", useExRateConfig: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency_DefaultOption_RateType()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ExRateOption.Default.Code, "EUR", useExRateConfig: true);
		}

		void AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrency(ZString exRateOption, string sellInvoiceCurrencyCode, bool useExRateConfig = false)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			CreateExchangeRates(TestObjectCreator.USD);

			var sellInvoiceCurrency = TestObjectCreator.GetCurrency(sellInvoiceCurrencyCode);
			CreateExchangeRates(sellInvoiceCurrency, 1);

			if (useExRateConfig)
			{
				TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(ledgerCode: "AR", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.SellRate);
				TestObjectCreator.ABIGAS.CompanyData.AccAPExchangeRateConfigurations.SetExRate(ledgerCode: "AP", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.SellRate);
			}
			Factory.Save();

			var isApplicableOption = exRateOption == ExRateOption.ExchangeRateBasedOnPostDate.Code
				|| exRateOption == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code
				|| exRateOption == ExRateOption.EarliestOfInvoiceOrTaxDate.Code;
			var invoiceAmount = 120m;
			var invoiceExRate = useExRateConfig && !isApplicableOption ? 8m : 6m;
			var sellExRate = 5m;
			var todayExRate = useExRateConfig ? 8m : 7m;
			var expectedSellExRate = useExRateConfig ? 5m : 4m;
			var expectedSellInvExRate = useExRateConfig ? 8m : 7m;
			var expectedFinalInvoiceExRate = useExRateConfig ? 6m : 5m;

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var exRateCred = job.ExchangeRates.AddNew();
			exRateCred.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			exRateCred.OrgType = ExchangeRateOrgTypeEnum.Creditor;
			exRateCred.JF_OH_Org = TestObjectCreator.ABIGAS.PK;
			exRateCred.JF_BaseRate = sellExRate;

			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			exRate.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			exRate.JF_OH_Org = TestObjectCreator.ABIGAS.PK;
			exRate.JF_BaseRate = sellExRate;

			Factory.Save();
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var expectedRateForNonApplicableOption = exRateOption == ExRateOption.Default.Code ? exRate.JF_BaseRate : ExchangeRateCalculator.GetRate(TestObjectCreator.USD.Code, useExRateConfig ? ExchangeRateType.Sell : ExchangeRateType.Buy, ZDateTime.Today.ToDateTime());

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", sellInvoiceCurrency, sellExRate, TestObjectCreator.ABIGAS);
			invoice1.AH_PostDate = new ZDateTime(2016, 4, 11);
			invoice1.AH_InvoiceDate = new ZDateTime(2016, 4, 11);
			invoice1.AH_PostedToEFT = useExRateConfig;

			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, invoiceAmount, sellInvoiceCurrency);
			line1.AL_JH = job.PK;
			line1.AL_TaxDate = ZDate.Today;
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: TestObjectCreator.USD, osSellAmt: 100m, debtor: invoice1.Header);
			job.Charges.Load();

			invoice1.AH_ExchangeRate = invoiceExRate;

			AssertEquals("Job Charge Sell Ex Rate should be defaulted from the latest Ex Rate", 5m, charge1.JR_OSSellExRate);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_RX_NKSellInvoiceCurrency = sellInvoiceCurrency.RX_Code;
			Assert("BillInInvoiceCurrency", charge1.BillInInvoiceCurrency);

			var sellInvoiceCurrencyExRate = charge1.SellInvoiceExchangeRate;
			AssertNotNull("Sell Invoice Currency Ex Rate on Job", sellInvoiceCurrencyExRate);
			AssertEquals("Sell Invoice Currency Ex Rate on Job should be defaulted from the today's Ex Rate", todayExRate, sellInvoiceCurrencyExRate.Rate);
			charge1.JR_AL_ARLine = line1.PK;
			AssertEquals("Job Charge Sell Ex Rate should be same as Line Ex Rate", line1.AL_ExchangeRate, charge1.JR_OSSellInvoiceExRate);

			if (isApplicableOption)
			{
				var expectedChargeSellExRate = exRateOption == ExRateOption.ExchangeRateBasedOnPostDate.Code
					? (useExRateConfig ? 5m : 4m)
					: (useExRateConfig ? 7m : 6m);  // PostDate option will give Post Date Ex Rate, otherwise it is Today's Rate

				ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsReceivable, new Charge[] { Factory.Load<Charge>(charge1.PK) }, invoice1.AH_PostDate);
				AssertEquals("Job Ex Rate should not be updated to the one on Invoice Date", sellExRate, exRate.JF_BaseRate);
				AssertEquals("Sell Invoice Currency Ex Rate on Job should be defaulted from the today's Ex Rate", expectedSellInvExRate, sellInvoiceCurrencyExRate.Rate);
				AssertEquals("Job Charge Sell Invoice Ex Rate should be updated as Charge is Linked to Line", expectedChargeSellExRate, charge1.JR_OSSellExRate);
				AssertEquals("Job Charge Sell Invoice Currency Ex Rate should be same as Line Ex Rate", line1.AL_ExchangeRate, charge1.JR_OSSellInvoiceExRate);
				AssertEquals("Line Ex Rate should be same as Invoice Ex Rate", invoiceExRate, line1.AL_ExchangeRate);
				AssertEquals("Line OS Amount should not be updated yet", 120m, line1.AL_OSAmount);
				AssertEquals("Line Amount should not be updated yet", 20m, line1.AL_LineAmount);

				ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice1, Factory);

				AssertEquals("Job Ex Rate should not be updated to the one on Invoice Date", sellExRate, exRate.JF_BaseRate);
				AssertEquals("Sell Invoice Currency Ex Rate on Job should be defaulted from the today's Ex Rate", expectedSellInvExRate, sellInvoiceCurrencyExRate.Rate);
				AssertEquals("Job Charge Sell Ex Rate should be updated as this Charge will be posted by Invoice", expectedSellExRate, charge1.JR_OSSellExRate);
				AssertEquals("Job Charge Sell Invoice Currency Ex Rate should be same as Line Ex Rate", line1.AL_ExchangeRate, charge1.JR_OSSellInvoiceExRate);
				AssertEquals("Line Ex Rate should be same as Job Charge Sell Invoice Currency Ex Rate", expectedFinalInvoiceExRate, line1.AL_ExchangeRate);
				AssertEquals("Line OS Amount should be updated", useExRateConfig ? 120m : 125m, line1.AL_OSAmount);
				AssertEquals("Line Amount should be updated", useExRateConfig ? 20m : 25m, line1.AL_LineAmount);
			}

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice1);

			if (isApplicableOption)
			{
				AssertEquals("Job Ex Rate should be updated to the one on Invoice Date", expectedSellExRate, exRate.JF_BaseRate);
				AssertEquals("Sell Invoice Currency Ex Rate on Job should be updated to the Invoice Date Ex Rate", expectedFinalInvoiceExRate, sellInvoiceCurrencyExRate.Rate);
				AssertEquals("Job Charge Sell Ex Rate", expectedSellExRate, charge1.JR_OSSellExRate);
				AssertEquals("Job Charge Sell Invoice Currency Ex Rate should be same as Line Ex Rate", line1.AL_ExchangeRate, charge1.JR_OSSellInvoiceExRate);
				AssertEquals("Line Ex Rate should be same as Job Charge Sell Invoice Currency Ex Rate", expectedFinalInvoiceExRate, line1.AL_ExchangeRate);
			}
			else
			{
				AssertEquals("Job Ex Rate should not be updated or set to today's rate for non applicable option", expectedRateForNonApplicableOption, exRate.JF_BaseRate);
				AssertEquals("Sell Invoice Currency Ex Rate on Job should be defaulted from the today's Ex Rate", todayExRate, sellInvoiceCurrencyExRate.Rate);
				AssertEquals("Job Charge Sell Ex Rate should not be updated", sellExRate, charge1.JR_OSSellExRate);
				AssertEquals("Job Charge Sell Invoice Currency Ex Rate should be same as Line Ex Rate", line1.AL_ExchangeRate, charge1.JR_OSSellInvoiceExRate);
				AssertEquals("Line Ex Rate should be same as Invoice Ex Rate", invoiceExRate, line1.AL_ExchangeRate);
			}
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency_PostDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.ExchangeRateBasedOnPostDate.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrencyAndCFX_PostDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.ExchangeRateBasedOnPostDate.Code, "EUR", withCFX: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency_InvoiceDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrencyAndCFX_InvoiceDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, "EUR", withCFX: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency_EarliestInvoicePostDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrencyAndCFX_EarliestInvoicePostDateOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, "EUR", withCFX: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency_TodayOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.TodayExchangeRate.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrencyAndCFX_TodayOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.TodayExchangeRate.Code, "EUR", withCFX: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency_DefaultOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.Default.Code, "EUR");
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrencyAndCFX_DefaultOption()
		{
			AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ExRateOption.Default.Code, "EUR", withCFX: true);
		}

		void AssertUpdateChargesAndLinesExchangeRateForBackDating_SellInvoiceCurrencyWithLocalSellCurrency(ZString exRateOption, string sellInvoiceCurrencyCode, bool withCFX = false)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var sellInvoiceCurrency = TestObjectCreator.GetCurrency(sellInvoiceCurrencyCode);
			CreateExchangeRates(sellInvoiceCurrency, 1);

			var invoiceAmount = 120m;
			var invoiceExRate = 6m;
			var sellExRate = 4m;

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var sellInvoiceCurrencyExRate = job.ExchangeRates.AddNew();
			sellInvoiceCurrencyExRate.JF_RX_NKRateCurrency = sellInvoiceCurrencyCode;
			sellInvoiceCurrencyExRate.JF_BaseRate = sellExRate;

			AssertEquals(true, sellInvoiceCurrencyExRate.JF_IsTransformed);

			Factory.Save();

			bool isApplicableOption = exRateOption == ExRateOption.ExchangeRateBasedOnPostDate.Code
				|| exRateOption == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code
				|| exRateOption == ExRateOption.EarliestOfInvoiceOrTaxDate.Code;

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var expectedRateForNonApplicableOption = exRateOption == ExRateOption.Default.Code ? sellInvoiceCurrencyExRate.JF_BaseRate : ExchangeRateCalculator.GetRate(sellInvoiceCurrencyCode, ExchangeRateType.Buy, ZDateTime.Today.ToDateTime());

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", sellInvoiceCurrency);
			invoice1.AH_PostDate = new ZDateTime(2016, 4, 11);
			invoice1.AH_InvoiceDate = new ZDateTime(2016, 4, 11);

			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, invoiceAmount, sellInvoiceCurrency);
			line1.AL_JH = job.PK;
			line1.AL_AT = TestObjectCreator.GST1.PK;

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, sellCurrency: GlbCompany.CurrentCompany.LocalCurrency, osSellAmt: 20m, debtor: invoice1.Header);
			charge1.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			job.Charges.Load();

			invoice1.AH_ExchangeRate = invoiceExRate;

			AssertEquals("Job Charge Sell Ex Rate should be 1", 1m, charge1.JR_OSSellExRate);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge1.JR_RX_NKSellInvoiceCurrency = sellInvoiceCurrencyCode;
			Assert("BillInInvoiceCurrency", charge1.BillInInvoiceCurrencyWithLocalSellCurrency);
			if (withCFX)
			{
				charge1.JR_LineCFX = 10m;
			}
			charge1.JR_AL_ARLine = line1.PK;
			AssertEquals("JR_LocalSellInvoiceAmt should be taken from the line as it is linked to the charge", line1.AL_LineAmount, charge1.JR_LocalSellInvoiceAmt);

			if (isApplicableOption)
			{
				ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsReceivable, new Charge[] { Factory.Load<Charge>(charge1.PK) }, invoice1.AH_PostDate);
				AssertEquals("Job Ex Rate should not be updated to the one on Invoice Date", sellExRate, sellInvoiceCurrencyExRate.JF_BaseRate);
				AssertEquals("Job Charge Sell Invoice Ex Rate should still be 1", 1m, charge1.JR_OSSellExRate);
				AssertEquals("Job Charge Sell Invoice Currency Ex Rate should be same as Line Ex Rate", invoiceExRate, charge1.JR_OSSellInvoiceExRate);
				AssertEquals("Line Ex Rate should be same as Job Charge Sell Ex Rate", invoiceExRate, line1.AL_ExchangeRate);
				AssertEquals("Line OS Total Amount should not be updated yet", 132m, line1.AL_OSAmount);
				AssertEquals("Line OS Ex Tax Amount should not be updated yet", 120m, line1.AL_OSExTaxAmount);
				AssertEquals("Line OS Tax Amount should not be updated yet", 12m, line1.AL_OSTaxAmount);
				AssertEquals("Line Amount should not be updated yet", 20m, line1.AL_LineAmount);
				AssertEquals("Line Tax Amount should not be updated yet", 2m, line1.AL_GSTVAT);

				ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice1, Factory);
				AssertEquals("Job Ex Rate should not be updated to the one on Invoice Date", sellExRate, sellInvoiceCurrencyExRate.JF_BaseRate);
				AssertEquals("Job Charge Sell Invoice Ex Rate should still be 1", 1m, charge1.JR_OSSellExRate);
				AssertEquals("Job Charge Sell Invoice Currency Ex Rate should be same as Line Ex Rate", 5m, charge1.JR_OSSellInvoiceExRate);
				AssertEquals("Line Ex Rate should be same as Job Charge Sell Invoice Currency Ex Rate", 5m, line1.AL_ExchangeRate);
				AssertEquals("Line OS Total Amount should be updated", withCFX ? 121m : 110m, line1.AL_OSAmount);
				AssertEquals("Line OS Ex Tax Amount should be updated", withCFX ? 110m : 100m, line1.AL_OSExTaxAmount);
				AssertEquals("Line OS Tax Amount should be updated", withCFX ? 11m : 10m, line1.AL_OSTaxAmount);
				AssertEquals("Line Amount", withCFX ? 22m : 20m, line1.AL_LineAmount);
				AssertEquals("Line Tax Amount", withCFX ? 2.2m : 2m, line1.AL_GSTVAT);
			}

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice1);
			Factory.Save();

			if (isApplicableOption)
			{
				AssertEquals("Sell Invoice Currency Ex Rate on Job should be updated to the Invoice Date Ex Rate", 5m, sellInvoiceCurrencyExRate.JF_BaseRate);
				AssertEquals("Job Charge Sell Invoice Ex Rate should still be 1", 1m, charge1.JR_OSSellExRate);
				AssertEquals("Job Charge Sell Invoice Currency Ex Rate should be same as Line Ex Rate", 5m, charge1.JR_OSSellInvoiceExRate);
				AssertEquals("Line Ex Rate should be same as Job Charge Sell Invoice Currency Ex Rate", 5m, line1.AL_ExchangeRate);
			}
			else
			{
				AssertEquals("Job Ex Rate should not be updated or set to today's rate for non applicable option", expectedRateForNonApplicableOption, sellInvoiceCurrencyExRate.JF_BaseRate);
				AssertEquals("Job Charge Sell Invoice Ex Rate should still be 1", 1m, charge1.JR_OSSellExRate);
				AssertEquals("Job Charge Sell Invoice Currency Ex Rate should be same as Line Ex Rate", invoiceExRate, charge1.JR_OSSellInvoiceExRate);
				AssertEquals("Line Ex Rate should be same as Job Charge Sell Invoice Currency Ex Rate", invoiceExRate, line1.AL_ExchangeRate);
			}
		}

		[TestDate(2022, 5, 20)]
		public void TestUpdateShipmentChargesExchangeRatesWithDifferentTaxDatesAndEIT()
		{
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ExRateOption.EarliestOfInvoiceOrTaxDate.Code);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 2m, new ZDateTime(2022, 4, 1), new ZDateTime(2022, 4, 30));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 3m, new ZDateTime(2022, 5, 1), new ZDateTime(2022, 5, 15));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 4m, new ZDateTime(2022, 5, 16), new ZDateTime(2022, 5, 31));
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 0m);
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge1.JR_APInvoiceNum = "INV01";
			charge1.JR_APInvoiceDate = ZDateTime.Today;
			charge1.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			charge1.JR_CostTaxDate = new ZDate(2022, 4, 15);

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 200m, 0m);
			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge2.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge2.JR_APInvoiceNum = "INV01";
			charge2.JR_APInvoiceDate = ZDateTime.Today;
			charge2.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			charge2.JR_CostTaxDate = new ZDate(2022, 5, 10);

			ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsPayable, new[] { charge1, charge2 }, ZDateTime.Now);
			AssertEquals("Charges should have the same exchange rate", 2m, charge1.JR_OSCostExRate);
			AssertEquals("Charges should have the same exchange rate", 2m, charge2.JR_OSCostExRate);
		}

		[TestDate(2022, 5, 20)]
		public void TestUpdateConsolChargesExchangeRatesWithDifferentTaxDatesAndEIT()
		{
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ExRateOption.EarliestOfInvoiceOrTaxDate.Code);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 2m, new ZDateTime(2022, 4, 1), new ZDateTime(2022, 4, 30));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 3m, new ZDateTime(2022, 5, 1), new ZDateTime(2022, 5, 15));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 4m, new ZDateTime(2022, 5, 16), new ZDateTime(2022, 5, 31));
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 0m);
			charge1.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge1.JR_APInvoiceNum = "INV01";
			charge1.JR_APInvoiceDate = ZDateTime.Today;
			charge1.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			charge1.JR_CostTaxDate = new ZDate(2022, 4, 15);

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 200m, 0m);
			charge2.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;
			charge2.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge2.JR_APInvoiceNum = "INV01";
			charge2.JR_APInvoiceDate = ZDateTime.Today;
			charge2.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			charge2.JR_CostTaxDate = new ZDate(2022, 5, 10);

			var consol = TestObjectCreator.CreateConsol();
			TestObjectCreator.CreateShipment("S1000", consol);
			TestObjectCreator.CreateShipment("S2000", consol);

			var consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100m);
			consolCost1.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			consolCost1.E6_InvoiceNum = "INV123";
			consolCost1.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			consolCost1.E6_InvoiceDate = ZDateTime.Today;
			consolCost1.E6_TaxDate = new ZDate(2022, 4, 15);
			Factory.Save();

			var consolCost2 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 200m);
			consolCost2.E6_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
			consolCost2.E6_InvoiceNum = "INV123";
			consolCost2.E6_OH_Creditor = TestObjectCreator.Creditor1.PK;
			consolCost2.E6_InvoiceDate = ZDateTime.Today;
			consolCost2.E6_TaxDate = new ZDate(2022, 5, 10);
			Factory.Save();

			AssertEquals(2, consolCost1.ApportionmentCharges.Count);
			AssertEquals(2, consolCost2.ApportionmentCharges.Count);

			var chargesPKs = consolCost1.ApportionmentCharges.GetPKs();
			chargesPKs.AddRange(consolCost2.ApportionmentCharges.GetPKs());
			var charges = Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, chargesPKs));
			ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsPayable, charges, ZDateTime.Now);
			AssertEquals(2m, consolCost1.E6_ExchangeRate);
			AssertEquals(2m, consolCost2.E6_ExchangeRate);
			Assert("Charges should have the same exchange rate", charges.All(x => x.JR_OSCostExRate == 2m));
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig_PostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig(ExRateOption.ExchangeRateBasedOnPostDate.Code, false);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig_InvoiceDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, false);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig_EarliestInvoicePostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, false);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig_PostDateOption_UseJobExRate()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig(ExRateOption.ExchangeRateBasedOnPostDate.Code, true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig_InvoiceDateOption_UseJobExRate()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig_EarliestInvoicePostDateOption_UseJobExRate()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, true);
		}

		void AssertUpdateJobsExchangeRatesToTheLatestExRates_UpdatesGenericRatesWithCompanyExRateConfig(ZString exRateOption, bool useJobExRateTicked)
		{
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var buyRate = 5m;
			var genericRate = 10m;
			var arRate = 15m;
			var apRate = 20m;

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, buyRate, new DateTime(2016, 4, 1), new DateTime(2016, 4, 10));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.C01Rate, genericRate, new DateTime(2016, 4, 1), new DateTime(2016, 4, 10));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.C02Rate, arRate, new DateTime(2016, 4, 1), new DateTime(2016, 4, 10));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Constants.ExchangeRateTypes.Code.C03Rate, apRate, new DateTime(2016, 4, 1), new DateTime(2016, 4, 10));
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(ledgerCode: "", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.C01Rate);
			TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(ledgerCode: "AR", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.C02Rate);
			TestObjectCreator.ABIGAS.CompanyData.AccAPExchangeRateConfigurations.SetExRate(ledgerCode: "AP", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.C03Rate);
			GlbCompany.CurrentCompany.Factory.Save();
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var exRateDEB = job.ExchangeRates.AddRate(TestObjectCreator.USD, 1, TestObjectCreator.ABIGAS.PK, ExchangeRateOrgTypeEnum.Debtor);
			var exRateCRD = job.ExchangeRates.AddRate(TestObjectCreator.USD, 1, TestObjectCreator.ABIGAS.PK, ExchangeRateOrgTypeEnum.Creditor);
			var exRateGeneric = job.ExchangeRates.AddRate(TestObjectCreator.USD, 1, Guid.Empty, ExchangeRateOrgTypeEnum.None);
			Factory.Save();

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD, 1, TestObjectCreator.ABIGAS);
			invoice1.AH_PostDate = new ZDateTime(2016, 4, 1);
			invoice1.AH_InvoiceDate = new ZDateTime(2016, 4, 1);
			invoice1.UseJobExchangeRate = useJobExRateTicked;
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, 100m, TestObjectCreator.USD);
			line1.AL_JH = job.PK;
			var charge1 = TestObjectCreator.CreateJobCharge(line1, job, TestObjectCreator.CC1);
			job.Charges.Load();

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice1);

			AssertEquals("AR Rate should be updated to C02 rate as it is updated with the organisation, which has its own ex rate config grid", arRate, exRateDEB.JF_BaseRate);
			if (useJobExRateTicked)
			{
				AssertEquals("When Use Job Exchange Rate is ticked, Generic Rate should be updated to C01 rate as it is updated without an organisation, and so falls back to company level", genericRate, exRateGeneric.JF_BaseRate);
			}
			else
			{
				AssertEquals("Generic rate is updated to BUY rate as since it doesn't have a ledger it does not use job ex rate config grid if it is AR", buyRate, exRateGeneric.JF_BaseRate);
			}

			AssertEquals("AP should not be updated as nothing was posted for it", 1m, exRateCRD.JF_BaseRate);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "I0001", TestObjectCreator.USD, 1, TestObjectCreator.ABIGAS);
			invoice2.AH_PostDate = new ZDateTime(2016, 4, 1);
			invoice2.AH_InvoiceDate = new ZDateTime(2016, 4, 1);
			invoice2.AH_PostedToEFT = useJobExRateTicked;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice2, 100m, TestObjectCreator.USD);
			line2.AL_JH = job.PK;
			var charge2 = TestObjectCreator.CreateJobCharge(line2, job, TestObjectCreator.CC1);
			job.Charges.Load();

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice2);

			if (useJobExRateTicked)
			{
				AssertEquals("AP Rate should be updated to C02 rate as it is updated with the organisation, which has its own ex rate config grid", apRate, exRateCRD.JF_BaseRate);
				AssertEquals("Generic Rate should be updated to C01 rate is it is updated without an organisation, and so falls back to company level", genericRate, exRateGeneric.JF_BaseRate);
			}
			else
			{
				AssertEquals("AP rate is updated to BUY rate when Use Job Exchange Rate is not ticked", buyRate, exRateCRD.JF_BaseRate);
				AssertEquals("Generic rate is updated to BUY rate when Use Job Exchange Rate is not ticked", buyRate, exRateGeneric.JF_BaseRate);
			}
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_PostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.ExchangeRateBasedOnPostDate.Code);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_InvoiceDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_EarliestInvoicePostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_TodayOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.TodayExchangeRate.Code);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_DefaultOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.Default.Code);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_PostDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.ExchangeRateBasedOnPostDate.Code, useExRateConfig: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_InvoiceDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, useExRateConfig: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_EarliestInvoicePostDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, useExRateConfig: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_TodayOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.TodayExchangeRate.Code, useExRateConfig: true);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRates_DefaultOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRates(ExRateOption.Default.Code, useExRateConfig: true);
		}

		void AssertUpdateJobsExchangeRatesToTheLatestExRates(ZString exRateOption, bool useExRateConfig = false)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			CreateExchangeRates(TestObjectCreator.USD);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);
			Factory.Save();

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(ledgerCode: "", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.SellRate);
			GlbCompany.CurrentCompany.Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			ZDecimal startingRate = 1;

			var exRateDEBAbigas = job.ExchangeRates.AddRate(TestObjectCreator.USD, startingRate, TestObjectCreator.ABIGAS.PK, ExchangeRateOrgTypeEnum.Debtor);
			var exRateCRDAbigas = job.ExchangeRates.AddRate(TestObjectCreator.USD, startingRate, TestObjectCreator.ABIGAS.PK, ExchangeRateOrgTypeEnum.Creditor);

			var exRateDEBAalshi = job.ExchangeRates.AddRate(TestObjectCreator.USD, startingRate, TestObjectCreator.AALSHI.PK, ExchangeRateOrgTypeEnum.Debtor);
			var exRateCRDAalshi = job.ExchangeRates.AddRate(TestObjectCreator.USD, startingRate, TestObjectCreator.AALSHI.PK, ExchangeRateOrgTypeEnum.Creditor);

			var exRateGeneric = job.ExchangeRates.AddRate(TestObjectCreator.USD, startingRate, ZGuid.Empty, ExchangeRateOrgTypeEnum.None);
			var exRateGenericCAD = job.ExchangeRates.AddRate(RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.Canada), startingRate, ZGuid.Empty, ExchangeRateOrgTypeEnum.None);

			Factory.Save();

			bool isApplicableOption = exRateOption == ExRateOption.ExchangeRateBasedOnPostDate.Code
				|| exRateOption == ExRateOption.ExchangeRateBasedOnInvoiceDate.Code
				|| exRateOption == ExRateOption.EarliestOfInvoiceOrTaxDate.Code;

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var expectedRateForNonApplicableOptionSELL = exRateOption == ExRateOption.Default.Code ? startingRate : ExchangeRateCalculator.GetRate(TestObjectCreator.USD.Code, ExchangeRateType.Sell, ZDateTime.Today.ToDateTime());
			var expectedRateForNonApplicableOptionBUY = exRateOption == ExRateOption.Default.Code ? startingRate : ExchangeRateCalculator.GetRate(TestObjectCreator.USD.Code, ExchangeRateType.Buy, ZDateTime.Today.ToDateTime());

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD, startingRate, TestObjectCreator.ABIGAS);
			invoice1.AH_PostDate = new ZDateTime(2016, 4, 11);
			invoice1.AH_InvoiceDate = new ZDateTime(2016, 4, 11);
			invoice1.UseJobExchangeRate = useExRateConfig;
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, 100m, TestObjectCreator.USD);
			line1.AL_JH = job.PK;
			var charge1 = TestObjectCreator.CreateJobCharge(line1, job, TestObjectCreator.CC1);
			job.Charges.Load();

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice1);
			Factory.Save();

			var expectedSellExRate = 5m;
			var expectedBuyExRate = 4m;

			if (isApplicableOption)
			{
				AssertEquals("Ex Rate should only be updated for rates matching Abigas Debtor, USD Currency as there are no other charges posted", expectedSellExRate, exRateDEBAbigas.JF_BaseRate);
				AssertEquals(@"Ex Rate should only be updated for rates matching Abigas Debtor, USD Currency as there are no other charges posted,
generic rate should not use sell if UseJobExchangeRate is unticked as it does not count as AR in this case", useExRateConfig ? expectedSellExRate : expectedBuyExRate, exRateGeneric.JF_BaseRate);

				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Debtor, USD Currency", startingRate, exRateCRDAbigas.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Debtor, USD Currency", startingRate, exRateDEBAalshi.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Debtor, USD Currency", startingRate, exRateCRDAalshi.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Debtor, USD Currency", startingRate, exRateGenericCAD.JF_BaseRate);
			}
			else
			{
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", expectedRateForNonApplicableOptionSELL, exRateDEBAbigas.JF_BaseRate);
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", expectedRateForNonApplicableOptionSELL, exRateDEBAalshi.JF_BaseRate);
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", expectedRateForNonApplicableOptionSELL, exRateGeneric.JF_BaseRate);

				AssertEquals("Different Ledger Job Ex Rates should not be updated", startingRate, exRateCRDAbigas.JF_BaseRate);
				AssertEquals("Different Ledger Job Ex Rates should not be updated", startingRate, exRateCRDAalshi.JF_BaseRate);
				AssertEquals("Different Ledger Job Ex Rates should not be updated", startingRate, exRateGenericCAD.JF_BaseRate);
			}

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "I0002", TestObjectCreator.USD, startingRate, TestObjectCreator.ABIGAS);
			invoice2.AH_PostDate = new ZDateTime(2016, 5, 1);
			invoice2.AH_InvoiceDate = new ZDateTime(2016, 5, 1);
			invoice2.UseJobExchangeRate = useExRateConfig;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice2, 200m, TestObjectCreator.USD);
			line2.AL_JH = job.PK;
			var charge2 = TestObjectCreator.CreateJobCharge(line2, job, TestObjectCreator.CC2);
			job.Charges.Load();
			charge2.JR_APInvoiceDate = invoice2.AH_InvoiceDate;

			var expectedCostExRate = useExRateConfig ? 7m : 6m;

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice2);
			Factory.Save();

			if (isApplicableOption)
			{
				AssertEquals("Ex rate should be updated for rates matching Abigas Creditor, USD Currency as there are no other cost lines posted", expectedCostExRate, exRateCRDAbigas.JF_BaseRate);
				AssertEquals("Ex Rate should be updated for generic USD rate since the invoice/post date is greater than the posted line", expectedCostExRate, exRateGeneric.JF_BaseRate);

				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", expectedSellExRate, exRateDEBAbigas.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateDEBAalshi.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateCRDAalshi.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateGenericCAD.JF_BaseRate);
			}
			else
			{
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", useExRateConfig ? expectedRateForNonApplicableOptionSELL : expectedRateForNonApplicableOptionBUY, exRateCRDAbigas.JF_BaseRate);
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", useExRateConfig ? expectedRateForNonApplicableOptionSELL : expectedRateForNonApplicableOptionBUY, exRateCRDAalshi.JF_BaseRate);
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", useExRateConfig ? expectedRateForNonApplicableOptionSELL : expectedRateForNonApplicableOptionBUY, exRateGeneric.JF_BaseRate);

				AssertEquals("Different Ledger Job Ex Rates should not be updated", expectedRateForNonApplicableOptionSELL, exRateDEBAbigas.JF_BaseRate);
				AssertEquals("Different Ledger Job Ex Rates should not be updated", expectedRateForNonApplicableOptionSELL, exRateDEBAalshi.JF_BaseRate);
				AssertEquals("Different Ledger Job Ex Rates should not be updated", startingRate, exRateGenericCAD.JF_BaseRate);
			}

			var creditNote1 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "N0001", TestObjectCreator.USD, startingRate, TestObjectCreator.ABIGAS);
			creditNote1.AH_PostDate = new ZDateTime(2016, 4, 1);
			creditNote1.AH_InvoiceDate = new ZDateTime(2016, 4, 1);
			creditNote1.UseJobExchangeRate = useExRateConfig;
			var line3 = TestObjectCreator.CreateInvoiceLine(creditNote1, 300m, TestObjectCreator.USD);
			line3.AL_JH = job.PK;
			var charge3 = TestObjectCreator.CreateJobCharge(line3, job, TestObjectCreator.CC3);
			job.Charges.Load();
			charge3.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(creditNote1);
			Factory.Save();

			if (isApplicableOption)
			{
				AssertEquals("Ex rate should not be updated for rates matching Abigas Debtor, USD Currency as there exists a line with greater invoice/post date", expectedSellExRate, exRateDEBAbigas.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for generic USD rate since the invoice/post date is lesser than the posted lines", expectedCostExRate, exRateGeneric.JF_BaseRate);

				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", expectedCostExRate, exRateCRDAbigas.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateDEBAalshi.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateCRDAalshi.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateGenericCAD.JF_BaseRate);
			}
			else
			{
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", expectedRateForNonApplicableOptionSELL, exRateDEBAbigas.JF_BaseRate);
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", expectedRateForNonApplicableOptionSELL, exRateDEBAalshi.JF_BaseRate);
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", expectedRateForNonApplicableOptionSELL, exRateGeneric.JF_BaseRate);

				AssertEquals("Different Ledger Job Ex Rates should not be updated", useExRateConfig ? expectedRateForNonApplicableOptionSELL : expectedRateForNonApplicableOptionBUY, exRateCRDAbigas.JF_BaseRate);
				AssertEquals("Different Ledger Job Ex Rates should not be updated", useExRateConfig ? expectedRateForNonApplicableOptionSELL : expectedRateForNonApplicableOptionBUY, exRateCRDAalshi.JF_BaseRate);
				AssertEquals("Different Ledger Job Ex Rates should not be updated", startingRate, exRateGenericCAD.JF_BaseRate);
			}

			var lastRate = TestObjectCreator.USD.ExchangeRates.First(x => x.RE_StartDate == new DateTime(2016, 5, 1) && x.RE_ExRateType == Constants.ExchangeRateTypes.Code.SellRate);
			lastRate.RE_SellRate = 7.5m;
			lastRate = TestObjectCreator.USD.ExchangeRates.First(x => x.RE_StartDate == new DateTime(2016, 5, 1) && x.RE_ExRateType == Constants.ExchangeRateTypes.Code.BuyRate);
			lastRate.RE_SellRate = 6.5m;
			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			var updatedExpectedRateForNonApplicableOptionSELL = exRateOption == ExRateOption.Default.Code ? startingRate : ExchangeRateCalculator.GetRate(TestObjectCreator.USD.Code, ExchangeRateType.Sell, ZDateTime.Today.ToDateTime());
			var updatedExpectedRateForNonApplicableOptionBUY = exRateOption == ExRateOption.Default.Code ? startingRate : ExchangeRateCalculator.GetRate(TestObjectCreator.USD.Code, ExchangeRateType.Buy, ZDateTime.Today.ToDateTime());

			var creditNote2 = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "N0002", TestObjectCreator.USD, 6m, TestObjectCreator.ABIGAS);
			creditNote2.AH_PostDate = new ZDateTime(2016, 5, 1);
			creditNote2.AH_InvoiceDate = new ZDateTime(2016, 5, 1);
			creditNote2.UseJobExchangeRate = useExRateConfig;
			var line4 = TestObjectCreator.CreateInvoiceLine(creditNote2, 400m, TestObjectCreator.USD);
			line4.AL_JH = job.PK;
			var charge4 = TestObjectCreator.CreateJobCharge(line4, job, TestObjectCreator.CC4);
			job.Charges.Load();
			charge4.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
			charge4.JR_APInvoiceDate = creditNote2.AH_InvoiceDate;

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(creditNote2);
			Factory.Save();
			expectedCostExRate = useExRateConfig ? 7.5m : 6.5m;
			if (isApplicableOption)
			{
				AssertEquals("Ex rate should be updated for rates matching Abigas Creditor, USD Currency as it is the same invoice/post date as the latest posted line", expectedCostExRate, exRateCRDAbigas.JF_BaseRate);
				AssertEquals("Ex Rate should be updated for generic USD rate since the invoice/post date is the same as the latest posted line", expectedCostExRate, exRateGeneric.JF_BaseRate);

				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", expectedSellExRate, exRateDEBAbigas.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateDEBAalshi.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateCRDAalshi.JF_BaseRate);
				AssertEquals("Ex Rate should not be updated for rates not matching Abigas Creditor, USD Currency", startingRate, exRateGenericCAD.JF_BaseRate);
			}
			else
			{
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", useExRateConfig ? updatedExpectedRateForNonApplicableOptionSELL : updatedExpectedRateForNonApplicableOptionBUY, exRateCRDAbigas.JF_BaseRate);
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", useExRateConfig ? updatedExpectedRateForNonApplicableOptionSELL : updatedExpectedRateForNonApplicableOptionBUY, exRateCRDAalshi.JF_BaseRate);
				AssertEquals("Same Ledger Job Ex Rates should either not be updated or set to today's rate", useExRateConfig ? updatedExpectedRateForNonApplicableOptionSELL : updatedExpectedRateForNonApplicableOptionBUY, exRateGeneric.JF_BaseRate);

				AssertEquals("Different Ledger Job Ex Rates should not be updated", expectedRateForNonApplicableOptionSELL, exRateDEBAbigas.JF_BaseRate);
				AssertEquals("Different Ledger Job Ex Rates should not be updated", expectedRateForNonApplicableOptionSELL, exRateDEBAalshi.JF_BaseRate);
				AssertEquals("Different Ledger Job Ex Rates should not be updated", startingRate, exRateGenericCAD.JF_BaseRate);
			}
		}

		[TestDate(2022, 05, 15)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesUseRegistryOffset()
		{
			var today = ZDateTime.Today;
			ExchangeRateReader.GetReaderInstance().ClearCache();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, today.AddDays(-1), today.AddDays(-1));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2.5m, today.AddDays(-2), today.AddDays(-2));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 3m, today, today);

			TestObjectCreator.CreateTestPeriodsForEntireYear(today.Year);

			var collection = new InvoicePostingExRateOptionCollection
			{
				new InvoicePostingExRateOption(Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ExRateOption.TodayExchangeRate.Code, -1),
				new InvoicePostingExRateOption(Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, ExRateOption.TodayExchangeRate.Code, -2)
			};
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD, 1.5m, TestObjectCreator.ABIGAS);
			invoice.AH_PostDate = today;
			invoice.AH_InvoiceDate = today;
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, 100m, TestObjectCreator.USD);
			line1.AL_JH = job.PK;
			TestObjectCreator.CreateJobCharge(line1, job, TestObjectCreator.CC1);
			job.Charges.Load();

			AssertEquals(3m, job.ExchangeRates.Cast<ExchangeRate>().First(x => x.OrgType == ExchangeRateOrgTypeEnum.Debtor).JF_BaseRate);
			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice);
			AssertEquals(2m, job.ExchangeRates.Cast<ExchangeRate>().First(x => x.OrgType == ExchangeRateOrgTypeEnum.Debtor).JF_BaseRate);

			Factory.Save();
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignSellInvoiceCurrency_PostDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.ExchangeRateBasedOnPostDate.Code, CreateARInvoiceWithForeignCurrencyButLocalCharge);
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignSellInvoiceCurrency_InvoiceDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, CreateARInvoiceWithForeignCurrencyButLocalCharge);
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignSellInvoiceCurrency_EarliestInvoicePostDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, CreateARInvoiceWithForeignCurrencyButLocalCharge);
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignSellCurrency_PostDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.ExchangeRateBasedOnPostDate.Code, CreateARInvoiceWithLocalCurrencyButForeignCharge);
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignSellCurrency_InvoiceDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, CreateARInvoiceWithLocalCurrencyButForeignCharge);
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignSellCurrency_EarliestInvoicePostDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, CreateARInvoiceWithLocalCurrencyButForeignCharge);
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignCostCurrency_PostDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.ExchangeRateBasedOnPostDate.Code, CreateAPInvoiceWithLocalCurrencyButForeignCharge);
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignCostCurrency_InvoiceDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, CreateAPInvoiceWithLocalCurrencyButForeignCharge);
		}

		[TestDate(2016, 5, 10)]
		public void TestGetsOriginalChargeCurrencyFromDB_ForeignCostCurrency_EarliestInvoicePostDateOption()
		{
			AssertGetsOriginalChargeCurrencyFromDB(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, CreateAPInvoiceWithLocalCurrencyButForeignCharge);
		}

		InvoicingBase CreateARInvoiceWithForeignCurrencyButLocalCharge(string invoiceNum, Job job)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceNum, TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 400m, TestObjectCreator.AUD);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC4);
			job.Charges.Load();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.Code;
			charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.USD.Code;
			return invoice;
		}

		InvoicingBase CreateARInvoiceWithLocalCurrencyButForeignCharge(string invoiceNum, Job job)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), invoiceNum, TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 400m, TestObjectCreator.AUD);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC4);
			job.Charges.Load();
			charge.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
			return invoice;
		}

		InvoicingBase CreateAPInvoiceWithLocalCurrencyButForeignCharge(string invoiceNum, Job job)
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), invoiceNum, TestObjectCreator.AUD, 1m, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 400m, TestObjectCreator.AUD);
			line.AL_JH = job.PK;
			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC4);
			job.Charges.Load();
			charge.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;
			return invoice;
		}

		void AssertGetsOriginalChargeCurrencyFromDB(ZString exRateOption, Func<string, Job, InvoicingBase> createInvoice)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			CreateExchangeRates(TestObjectCreator.USD);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);
			Factory.Save();

			GlbCompany.CurrentCompany.AccExchangeRateConfigurations.SetExRate(ledgerCode: "", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.BuyRate);
			GlbCompany.CurrentCompany.Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			ZDecimal startingRate = 1;

			var exRate = job.ExchangeRates.AddRate(TestObjectCreator.USD, startingRate, ZGuid.Empty, ExchangeRateOrgTypeEnum.None);

			Factory.Save();

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var invoice1 = createInvoice("I001", job);
			invoice1.AH_PostDate = new ZDateTime(2016, 4, 11);
			invoice1.AH_InvoiceDate = new ZDateTime(2016, 4, 11);
			invoice1.Lines.Cast<InvoicingLineBase>().ForEach(x => x.AL_TaxDate = new ZDate(2016, 4, 11));

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice1);
			Factory.Save();

			var expectedBuyExRate = 4m;
			AssertEquals("Rate should be updated with posted charge's exchange rate", expectedBuyExRate, exRate.JF_BaseRate);

			var invoice2 = createInvoice("I002", job);
			invoice2.AH_PostDate = new ZDateTime(2016, 4, 1);
			invoice2.AH_InvoiceDate = new ZDateTime(2016, 4, 1);
			invoice2.Lines.Cast<InvoicingLineBase>().ForEach(x => x.AL_TaxDate = new ZDate(2016, 4, 1));

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice2);
			Factory.Save();

			AssertEquals("Rate should not be updated as the invoice/post date is before an existing posted charge's", expectedBuyExRate, exRate.JF_BaseRate);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesIgnoresReversedTransactions_PostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesIgnoresReversedTransactions(ExRateOption.ExchangeRateBasedOnPostDate.Code);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesIgnoresReversedTransactions_InvoiceDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesIgnoresReversedTransactions(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		[TestDate(2016, 5, 10)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesIgnoresReversedTransactions_EarliestInvoicePostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesIgnoresReversedTransactions(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
		}

		void AssertUpdateJobsExchangeRatesToTheLatestExRatesIgnoresReversedTransactions(ZString exRateOption)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			CreateExchangeRates(TestObjectCreator.USD);

			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			exRate.JF_BaseRate = 5m;

			Factory.Save();

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD);
			invoice1.AH_PostDate = new ZDateTime(2016, 4, 11);
			invoice1.AH_InvoiceDate = new ZDateTime(2016, 4, 11);

			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, 100m, TestObjectCreator.USD);
			line1.AL_JH = job.PK;
			var charge1 = TestObjectCreator.CreateJobCharge(line1, job, TestObjectCreator.CC1);
			job.Charges.Load();
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice1);
			Factory.Save();

			AssertEquals("Job Ex Rate should be updated to the one on Invoice Date", 4m, exRate.JF_BaseRate);

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "I0002", TestObjectCreator.USD);
			invoice2.AH_PostDate = new ZDateTime(2016, 4, 1);
			invoice2.AH_InvoiceDate = new ZDateTime(2016, 4, 1);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice2, 200m, TestObjectCreator.USD);
			line2.AL_JH = job.PK;
			var charge2 = TestObjectCreator.CreateJobCharge(line2, job, TestObjectCreator.CC2);
			job.Charges.Load();
			charge2.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;
			charge2.JR_APInvoiceDate = invoice2.AH_InvoiceDate;

			ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsPayable, new Charge[] { Factory.Load<Charge>(charge2.PK) }, invoice2.AH_PostDate);
			AssertEquals("Job Ex Rate should not be updated to the one on Invoice Date", 4m, exRate.JF_BaseRate);
			AssertEquals("Job Charge Cost Ex Rate", 2m, charge2.JR_OSCostExRate);
			AssertEquals("Job Charge Sell Ex Rate", 4m, charge2.JR_OSSellExRate); //2m
			ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice2, Factory);
			AssertEquals("Job Ex Rate should not be updated to the one on Invoice Date", 4m, exRate.JF_BaseRate);
			AssertEquals("Job Charge Cost Ex Rate", 2m, charge2.JR_OSCostExRate);
			AssertEquals("Job Charge Sell Ex Rate", 4m, charge2.JR_OSSellExRate); //2m
			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice2);
			Factory.Save();

			AssertEquals("Job Ex Rate should not be updated as this Invoice Date is earlier than the previous Invoice", 4m, exRate.JF_BaseRate);
			AssertEquals("Job Charge Cost Ex Rate", 2m, charge2.JR_OSCostExRate);
			AssertEquals("Job Charge Sell Ex Rate", 4m, charge2.JR_OSSellExRate);

			string message;
			var reversalCreditNote = TestObjectCreator.ReverseTransaction(invoice1, out message);
			AssertEquals("Reversal is posted today", ZDateTime.Today, reversalCreditNote.TransactionDate.Date);
			AssertEquals("Reversal is posted today", ZDateTime.Today, reversalCreditNote.PostDate);
			Factory.Save();

			AssertEquals("Job Ex Rate should not be updated as we only reversed the latest date Invoice", 4m, exRate.JF_BaseRate);

			var creditNote1 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "N0001", TestObjectCreator.USD);
			creditNote1.AH_PostDate = new ZDateTime(2016, 4, 10);
			creditNote1.AH_InvoiceDate = new ZDateTime(2016, 4, 10);
			var line3 = TestObjectCreator.CreateInvoiceLine(creditNote1, 300m, TestObjectCreator.USD);
			line3.AL_JH = job.PK;
			var charge3 = TestObjectCreator.CreateJobCharge(line3, job, TestObjectCreator.CC3);
			job.Charges.Load();
			charge3.JR_RX_NKCostCurrency = TestObjectCreator.USD.Code;

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(creditNote1);
			Factory.Save();

			AssertEquals("Job Ex Rate should be updated as this Invoice Date is later than any previous not reversed Invoice", 2m, exRate.JF_BaseRate);

			var creditNote2 = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "N0002", TestObjectCreator.USD);
			creditNote2.AH_PostDate = new ZDateTime(2016, 5, 10);
			creditNote2.AH_InvoiceDate = new ZDateTime(2016, 5, 10);
			var line4 = TestObjectCreator.CreateInvoiceLine(creditNote2, 400m, TestObjectCreator.USD);
			line4.AL_JH = job.PK;
			var charge4 = TestObjectCreator.CreateJobCharge(line4, job, TestObjectCreator.CC4);
			job.Charges.Load();
			charge4.JR_RX_NKSellCurrency = TestObjectCreator.USD.Code;

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(creditNote2);
			Factory.Save();

			AssertEquals("Job Ex Rate should be updated as this Invoice Date is later than previous Invoice", 6m, exRate.JF_BaseRate);
			AssertEquals("Job Charge Cost Ex Rate", 6m, charge4.JR_OSCostExRate);
			AssertEquals("Job Charge Sell Ex Rate", 6m, charge4.JR_OSSellExRate);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_PostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.ExchangeRateBasedOnPostDate.Code);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_InvoiceDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_EarliestInvoicePostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_SellInvoiceCurrency_PostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.ExchangeRateBasedOnPostDate.Code, "EUR");
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_SellInvoiceCurrency_InvoiceDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, "EUR");
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_SellInvoiceCurrency_EarliestInvoicePostDateOption()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, "EUR");
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_PostDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.ExchangeRateBasedOnPostDate.Code);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_InvoiceDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_EarliestInvoicePostDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.EarliestOfInvoiceOrTaxDate.Code);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_SellInvoiceCurrency_PostDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.ExchangeRateBasedOnPostDate.Code, "EUR", useExRateConfig: true);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_SellInvoiceCurrency_InvoiceDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, "EUR", useExRateConfig: true);
		}

		[TestDate(2016, 6, 2)]
		public void TestUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice_SellInvoiceCurrency_EarliestInvoicePostDateOption_RateType()
		{
			AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ExRateOption.EarliestOfInvoiceOrTaxDate.Code, "EUR", useExRateConfig: true);
		}

		void AssertUpdateJobsExchangeRatesToTheLatestExRatesOnARFINInvoice(ZString exRateOption, string sellInvoiceCurrencyCode = null, bool useExRateConfig = false)
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			CreateExchangeRates(TestObjectCreator.USD);

			RefCurrency sellInvoiceCurrency = null;
			IExchangeRateJobBilling sellInvoiceCurrencyExRate = null;
			if (!string.IsNullOrEmpty(sellInvoiceCurrencyCode))
			{
				sellInvoiceCurrency = TestObjectCreator.GetCurrency(sellInvoiceCurrencyCode);
				CreateExchangeRates(sellInvoiceCurrency, 1);
			}

			if (useExRateConfig)
			{
				TestObjectCreator.ABIGAS.CompanyData.AccARExchangeRateConfigurations.SetExRate(ledgerCode: "AR", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.SellRate);
				TestObjectCreator.Creditor1.CompanyData.AccAPExchangeRateConfigurations.SetExRate(ledgerCode: "AP", jobType: "SHP", transportMode: "ALL", serviceDirection: "ALL", exRateType: Constants.ExchangeRateTypes.Code.SellRate);
			}

			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);

			var exRateCrd = job.ExchangeRates.AddNew();
			exRateCrd.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			exRateCrd.OrgType = ExchangeRateOrgTypeEnum.Creditor;
			exRateCrd.JF_OH_Org = TestObjectCreator.Creditor1.PK;
			exRateCrd.JF_BaseRate = 5m;

			var exRateDeb = job.ExchangeRates.AddNew();
			exRateDeb.JF_RX_NKRateCurrency = TestObjectCreator.USD.Code;
			exRateDeb.OrgType = ExchangeRateOrgTypeEnum.Debtor;
			exRateDeb.JF_OH_Org = TestObjectCreator.ABIGAS.PK;
			exRateDeb.JF_BaseRate = 5m;

			Factory.Save();

			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Test Charge", TestObjectCreator.USD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.USD, 125m, TestObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			AssertEquals("Job Charge Cost Ex Rate", 5m, charge1.JR_OSCostExRate);
			AssertEquals("Job Charge Sell Ex Rate", 5m, charge1.JR_OSSellExRate);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);
			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, exRateOption);

			var localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			var invoiceCurrency = localCurrency;
			var invoiceAmount = 25m;
			var invoiceExRate = 1m;
			var todayExRate = useExRateConfig ? 8m : 7m;
			var expectedSellRate = useExRateConfig ? 5m : 4m;
			var expectedSellInvoiceRate = useExRateConfig ? 6m : 5m;

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "FIN001", invoiceCurrency, null, TestObjectCreator.ABIGAS);
			invoice1.AH_PostDate = new ZDateTime(2016, 4, 11);
			invoice1.AH_InvoiceDate = new ZDateTime(2016, 4, 11);
			invoice1.AH_PostedToEFT = useExRateConfig;

			if (sellInvoiceCurrency != null)
			{
				invoiceCurrency = sellInvoiceCurrency;
				invoiceAmount = 175m;
				invoiceExRate = 7m;
				invoice1.AH_ExchangeRate = invoiceExRate;

				charge1.JR_RX_NKSellInvoiceCurrency = sellInvoiceCurrency.RX_Code;
				Assert("BillInInvoiceCurrency", charge1.BillInInvoiceCurrency);
				sellInvoiceCurrencyExRate = charge1.SellInvoiceExchangeRate;
				AssertNotNull("Sell Invoice Currency Ex Rate on Job", sellInvoiceCurrencyExRate);
				AssertEquals("Sell Invoice Currency Ex Rate on Job should be defaulted from the today's Ex Rate", todayExRate, sellInvoiceCurrencyExRate.Rate);
			}

			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, invoiceAmount, invoiceCurrency, invoiceExRate);
			line1.AL_JH = job.PK;
			charge1.JR_AL_ARLine = line1.PK;

			AssertEquals("Line Currency", invoiceCurrency.Code, line1.AL_RX_NKTransactionCurrency);
			AssertEquals("Job Charge Sell Currency", TestObjectCreator.USD.Code, charge1.JR_RX_NKSellCurrency);
			AssertEquals("Job Charge Sell Invoice Currency", string.IsNullOrEmpty(sellInvoiceCurrencyCode) ? string.Empty : sellInvoiceCurrency.Code, charge1.JR_RX_NKSellInvoiceCurrency);

			ExchangeRateCalculator.UpdateJobsExchangeRatesToTheLatestExRates(invoice1);

			AssertEquals("Job Ex Rate should not be updated for Creditor Ex Rate", 5m, exRateCrd.JF_BaseRate);
			AssertEquals("Job Charge Cost Ex Rate should not be updated", 5m, charge1.JR_OSCostExRate);
			AssertEquals("Job Ex Rate should be updated to the one on Invoice Date for Debtor Ex Rate", expectedSellRate, exRateDeb.JF_BaseRate);
			AssertEquals("Job Charge Sell Ex Rate should not be updated as it is linked to AR Invoice Line", 5m, charge1.JR_OSSellExRate);

			if (sellInvoiceCurrency != null)
			{
				AssertEquals("Sell Invoice Currency Ex Rate on Job should be updated to the one on Invoice Date", expectedSellInvoiceRate, sellInvoiceCurrencyExRate.Rate);
			}
		}

		public void TestUpdateSellInvoiceExchangeRatesWithPostingExchangeRateConfiguration()
		{
			AsserttUpdateSellInvoiceExchangeRatesDuringPosting(true);
		}

		public void TestUpdateSellInvoiceExchangeRatesWithoutPostingExchangeRateConfiguration()
		{
			AsserttUpdateSellInvoiceExchangeRatesDuringPosting(false);
		}

		void AsserttUpdateSellInvoiceExchangeRatesDuringPosting(bool withPostingExchangeRateConfiguration)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.SetCurrentCompanyReciprocal(true);
			var postingExchangeRateOption = withPostingExchangeRateConfiguration
				? AccountingConstants.InvoicePostingExchangeRateOption.TodayExchangeRate.Code
				: AccountingConstants.InvoicePostingExchangeRateOption.Default.Code;
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, postingExchangeRateOption);

			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 0.6m);
			testObjectCreator.CreateExchangeRate(testObjectCreator.EUR, Constants.ExchangeRateTypes.Code.BuyRate, 1.1m);
			Factory.Save();

			var shipment = testObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), saveIt: true);
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);
			job.PlugInData = shipment;
			testObjectCreator.CreateExchangeRate(job, testObjectCreator.USD, 0.7m);
			testObjectCreator.CreateExchangeRate(job, testObjectCreator.EUR, 1.2m);

			var localCharge = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, "Revenue Transaction Test 1", testObjectCreator.AUD, 0m, null, testObjectCreator.AUD, 1M, testObjectCreator.LocalClient);
			var foreignChargeWithSameSellInvoiceCurrency = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, "Revenue Transaction Test 2", testObjectCreator.USD, 0m, null, testObjectCreator.USD, 1M, testObjectCreator.LocalClient);
			var foreignChargeWithDifferentSellInvoiceCurrency = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, "Revenue Transaction Test 2", testObjectCreator.EUR, 0m, null, testObjectCreator.EUR, 1M, testObjectCreator.LocalClient);
			localCharge.JR_RX_NKSellInvoiceCurrency = testObjectCreator.USD.RX_Code;
			foreignChargeWithSameSellInvoiceCurrency.JR_RX_NKSellInvoiceCurrency = testObjectCreator.USD.RX_Code;
			foreignChargeWithDifferentSellInvoiceCurrency.JR_RX_NKSellInvoiceCurrency = testObjectCreator.USD.RX_Code;

			AssertEquals(0.7m, localCharge.JR_OSSellInvoiceExRate);
			AssertEquals(0.7m, foreignChargeWithSameSellInvoiceCurrency.JR_OSSellInvoiceExRate);
			AssertEquals(0.7m, foreignChargeWithDifferentSellInvoiceCurrency.JR_OSSellInvoiceExRate);

			var charges = new Charge[] { localCharge, foreignChargeWithSameSellInvoiceCurrency, foreignChargeWithDifferentSellInvoiceCurrency };
			ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsReceivable, charges, ZDateTime.Now);

			if (withPostingExchangeRateConfiguration)
			{
				Assert("posting context is false", charges.Any(x => !x.HasContext(BusinessContext.PostingReceivableCharges)));

				AssertEquals("JR_OSSellInvoiceExRate will use the job exchange rate", 0.7m, localCharge.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceExRate equals JR_OSSellExRate if BIllInInvoiceCurrencySameAsSellCurrency is true", foreignChargeWithSameSellInvoiceCurrency.JR_OSSellExRate, foreignChargeWithSameSellInvoiceCurrency.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceExRate will use the job exchange rate", 0.7m, foreignChargeWithDifferentSellInvoiceCurrency.JR_OSSellInvoiceExRate);

				charges.ForEach(x => x.SetContext(BusinessContext.PostingReceivableCharges));

				Assert("posting context is true", charges.Any(x => x.HasContext(BusinessContext.PostingReceivableCharges)));

				AssertEquals("JR_OSSellInvoiceExRate will use the job today's exchange rate", 0.6m, localCharge.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceExRate equals JR_OSSellExRate if BIllInInvoiceCurrencySameAsSellCurrency is true", foreignChargeWithSameSellInvoiceCurrency.JR_OSSellExRate, foreignChargeWithSameSellInvoiceCurrency.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceExRate will use the job today's exchange rate", 0.6m, foreignChargeWithDifferentSellInvoiceCurrency.JR_OSSellInvoiceExRate);
			}
			else
			{
				Assert("posting context is false", charges.Any(x => !x.HasContext(BusinessContext.PostingReceivableCharges)));

				AssertEquals("JR_OSSellInvoiceExRate was not updated because registry posting option is default", 0.7m, localCharge.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceExRate was not updated because registry posting option is default", 0.7m, foreignChargeWithSameSellInvoiceCurrency.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceExRate was not updated because registry posting option is default", 0.7m, foreignChargeWithDifferentSellInvoiceCurrency.JR_OSSellInvoiceExRate);

				charges.ForEach(x => x.SetContext(BusinessContext.PostingReceivableCharges));

				Assert("posting context is true", charges.Any(x => x.HasContext(BusinessContext.PostingReceivableCharges)));

				AssertEquals("JR_OSSellInvoiceExRate was not updated because registry posting option is default", 0.7m, localCharge.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceExRate was not updated because registry posting option is default", 0.7m, foreignChargeWithSameSellInvoiceCurrency.JR_OSSellInvoiceExRate);
				AssertEquals("JR_OSSellInvoiceExRate was not updated because registry posting option is default", 0.7m, foreignChargeWithDifferentSellInvoiceCurrency.JR_OSSellInvoiceExRate);
			}
		}

		public void TestUpdateSellInvoiceExchangeRatesWhereCustomRateShouldBeUseWhenItSetForDebtorWithPostingExchangeRateConfiguration()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.SetCurrentCompanyReciprocal(true);

			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, Constants.ExchangeRateTypes.Code.BuyRate, 0.6m);
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, Constants.ExchangeRateTypes.Code.C01Rate, 0.45m);

			var exRateARConfig = testObjectCreator.LocalClient.CompanyData.AccARExchangeRateConfigurations.AddNew();
			exRateARConfig.JCE_JobType = "ALL";
			exRateARConfig.JCE_ServiceDirection = "ALL";
			exRateARConfig.JCE_TransportMode = "ALL";
			exRateARConfig.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = Constants.ExchangeRateTypes.Code.C01Rate;

			Factory.Save();

			var shipment = testObjectCreator.CreateShipment("S0004", saveIt: true);
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);
			job.PlugInData = shipment;

			var localCharge = testObjectCreator.CreateCharge(job, testObjectCreator.FRT, "Revenue Transaction Test 1", testObjectCreator.AUD, 0m, null, testObjectCreator.AUD, 1M, testObjectCreator.LocalClient);
			localCharge.JR_RX_NKSellInvoiceCurrency = testObjectCreator.USD.RX_Code;
			localCharge.SetContext(BusinessContext.PostingReceivableCharges);

			AssertEquals("JR_OSSellInvoiceExRate will use the custom rate", 0.45m, localCharge.JR_OSSellInvoiceExRate);

			foreach (CodeDescriptionPair option in AccountingConstants.InvoicePostingExchangeRateOption.CodeList)
			{
				PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option.Code);
				ExchangeRateCalculator.UpdateChargesExchangeRates(LedgerTypes.AccountsReceivable, new Charge[] { localCharge }, ZDateTime.Now);
				AssertEquals("after running UpdateSellInvoiceExchangeRates, JR_OSSellInvoiceExRate will still use the custom rate", 0.45m, localCharge.JR_OSSellInvoiceExRate);
			}
		}

		[TestDate(2019, 08, 02)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_RevLineTaxAmoutAndChargeTaxAmout()
		{
			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			var testObjectCreator = new TestObjectCreator(Factory);

			var taxRate = testObjectCreator.CreateTaxRate("GST2", string.Empty, 7);
			testObjectCreator.CC1.AC_AT_GSTRate = taxRate.PK;

			var shipment = testObjectCreator.CreateShipment("S0001");
			var job = testObjectCreator.CreateJob(shipment);
			testObjectCreator.CreateExchangeRate(job, testObjectCreator.USD, 4M);

			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", testObjectCreator.AUD);
			invoice.AH_PostDate = new ZDateTime(2019, 08, 02);
			invoice.AH_InvoiceDate = new ZDateTime(2019, 08, 02);

			var line = testObjectCreator.CreateInvoiceLine(invoice, 25m, testObjectCreator.AUD);
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "test", testObjectCreator.USD, 0M, null, testObjectCreator.USD, 100M, invoice.Header);
			charge.JR_AT_SellGSTRate = taxRate.PK;
			job.Charges.Load();

			AssertEquals("Precondition", 100m, charge.JR_OSSellAmt);
			AssertEquals("Precondition", 7m, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Precondition", 4m, charge.JR_OSSellExRate);
			AssertEquals("Precondition", 25m, charge.JR_LocalSellAmt);
			AssertEquals("Precondition", "REV", line.AL_LineType);
			AssertEquals("Precondition", 1.75m, line.AL_OSTaxAmount);
			AssertEquals("Precondition", 25m, line.AL_OSExTaxAmount);

			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "BUY", 5.0m, invoice.AH_PostDate, invoice.AH_PostDate.AddDays(1));
			charge.JR_AL_ARLine = line.PK;
			ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice, Factory);

			AssertEquals(5m, charge.JR_OSSellExRate);
			AssertEquals(20m, charge.JR_LocalSellAmt);
			AssertEquals("Exchange rate change, OS Amount should not change", 100m, charge.JR_OSSellAmt);
			AssertEquals("Tax amount should not change when exchange rates change", 7m, charge.JR_OSSellGSTAmt_Calc);
			AssertEquals("The currency of REV Line is local currency, so when the charge change exchange rate . OSTaxAmount of REV Line should be changed.", 1.4m, line.AL_OSTaxAmount);
			AssertEquals("The currency of REV Line is local currency, so when the charge change exchange rate . OSExTaxAmount of REV Line should be changed. ", 20m, line.AL_OSExTaxAmount);
		}

		[TestDate(2019, 08, 02)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_SyncExchangeRateInUpdateChargesAndLinesExchangeRateForBackDating()
		{
			PostingExRateRegistryAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			var testObjectCreator = new TestObjectCreator(Factory);

			var taxRate = testObjectCreator.CreateTaxRate("GST2", string.Empty, 7);
			testObjectCreator.CC1.AC_AT_GSTRate = taxRate.PK;

			var shipment = testObjectCreator.CreateShipment("S0001");
			var job = testObjectCreator.CreateJob(shipment);
			testObjectCreator.CreateExchangeRate(job, testObjectCreator.USD, 4M);

			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", testObjectCreator.USD);
			invoice.AH_PostDate = new ZDateTime(2019, 08, 02);
			invoice.AH_InvoiceDate = new ZDateTime(2019, 08, 02);

			var line = testObjectCreator.CreateInvoiceLine(invoice, 25m, testObjectCreator.USD);
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			line.AL_LineAmount = 21m;
			line.AL_OSAmount = 4m;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "test", testObjectCreator.USD, 0M, null, testObjectCreator.USD, 100M, invoice.Header);
			charge.JR_AT_SellGSTRate = taxRate.PK;
			job.Charges.Load();

			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "BUY", 5.0m, invoice.AH_PostDate, invoice.AH_PostDate.AddDays(1));
			charge.JR_AL_ARLine = line.PK;
			ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice, Factory);

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.SyncExchangeRateInUpdateChargesAndLinesExchangeRateForBackDating);
			AssertContains("SyncExchangeRateInUpdateChargesAndLinesExchangeRateForBackDating: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", collectedInfo);

			line.AL_ExchangeRate = 2m;
			ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice, Factory);

			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.SyncExchangeRateInUpdateChargesAndLinesExchangeRateForBackDating);
			AssertContains("SyncExchangeRateInUpdateChargesAndLinesExchangeRateForBackDating:\r\nChargeCurrency = USD, Line Currency = USD, BillInInvoiceCurrencyWithLocalSellCurrency = False, Charge Exchange Rate = 5.000000, Line Exchange Rate = 2", collectedInfo);
		}

		[TestDate(2019, 08, 02)]
		public void TestUpdateChargesAndLinesExchangeRateForBackDating_WhenUnlinkedChargesAndLinesEmpty()
		{
			var postingExRateOptions = new InvoicePostingExRateOptionCollection();
			postingExRateOptions.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, -1));
			postingExRateOptions.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, ExRateOption.ExchangeRateBasedOnInvoiceDate.Code, -1));
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, postingExRateOptions);

			PostingExRateRegistryAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code);
			var taxRate = TestObjectCreator.CreateTaxRate("GST2", string.Empty, 7);
			TestObjectCreator.CC1.AC_AT_GSTRate = taxRate.PK;

			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 4m);

			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "I0001", TestObjectCreator.USD);
			invoice.AH_PostDate = new ZDateTime(2019, 08, 02);
			invoice.AH_InvoiceDate = new ZDateTime(2019, 08, 02);

			var line = TestObjectCreator.CreateInvoiceLine(invoice, 25m, TestObjectCreator.USD);
			line.AL_JH = job.PK;
			line.AL_AT = taxRate.PK;
			line.AL_LineAmount = 21m;
			line.AL_OSAmount = 4m;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "test", TestObjectCreator.AUD, 0M, null, TestObjectCreator.AUD, 100M, invoice.Header);
			charge.JR_AT_SellGSTRate = taxRate.PK;
			job.Charges.Load();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "BUY", 5.0m, invoice.AH_PostDate, invoice.AH_PostDate.AddDays(1));
			charge.JR_AL_ARLine = line.PK;

			ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice, Factory);
			var collectedInfo1 = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.UpdateChargesAndLinesExchangeRateForBackDatingWithUnlinkedChargesAndLinesEmpty);
			AssertContains("UpdateChargesAndLinesExchangeRateForBackDatingWithUnlinkedChargesAndLinesEmpty: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.", collectedInfo1);

			ExchangeRateCalculator.UpdateChargesAndLinesExchangeRateForBackDating(invoice, Factory);

			var collectedInfo2 = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.UpdateChargesAndLinesExchangeRateForBackDatingWithUnlinkedChargesAndLinesEmpty);
			AssertContains(@"
UpdateChargesAndLinesExchangeRateForBackDatingWithUnlinkedChargesAndLinesEmpty:
LocalCurrency = AUD, Charge BillInInvoiceCurrencyWithLocalSellCurrency = False, JR_RX_NKSellInvoiceCurrency = , JR_RX_NKSellCurrency = AUD, ChargeCompanyLocalCurrency = AUD, JR_InvoiceType = ", collectedInfo2);
		}

		void CreateExchangeRates(RefCurrency currency, decimal rateBase = 0m)
		{
			TestObjectCreator.CreateExchangeRate(currency, "BUY", rateBase + 2m, new DateTime(2016, 4, 1), new DateTime(2016, 4, 10));
			TestObjectCreator.CreateExchangeRate(currency, "SEL", rateBase + 3m, new DateTime(2016, 4, 1), new DateTime(2016, 4, 10));

			TestObjectCreator.CreateExchangeRate(currency, "BUY", rateBase + 4m, new DateTime(2016, 4, 11), new DateTime(2016, 4, 30));
			TestObjectCreator.CreateExchangeRate(currency, "SEL", rateBase + 5m, new DateTime(2016, 4, 11), new DateTime(2016, 4, 30));

			TestObjectCreator.CreateExchangeRate(currency, "BUY", rateBase + 6m, new DateTime(2016, 5, 1), new DateTime(2016, 12, 31));
			TestObjectCreator.CreateExchangeRate(currency, "SEL", rateBase + 7m, new DateTime(2016, 5, 1), new DateTime(2016, 12, 31));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
	}
}
