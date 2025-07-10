using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(UniversalPriceSystemUsage))]
	internal class UniversalPriceSystemUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new UniversalPriceSystemUsage(Factory, "AAA", "PPP", new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals("System code set in construction", "AAA", usage.SystemCode);

			usage = new UniversalPriceSystemUsage(Factory, "BBB", "PP2", new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals("System code set in construction", "BBB", usage.SystemCode);
		}

		public void TestPriceHeader()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceHeader = BillingTestHelper.CreatePriceHeader(stdLicCompany, BillingConstants.PriceHeaderType.ABMCustoms, "V1", "AUD", new ZDateTime(2014, 1, 1));
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);
			Factory.Save();

			var usage1 = new UniversalPriceSystemUsage(Factory, "AAA", BillingConstants.PriceHeaderType.ABMCustoms, new UsingParty(), EdiDateTest.MonthToday);
			var usage2 = new UniversalPriceSystemUsage(Factory, "AAA", BillingConstants.PriceHeaderType.GlobalContainerTracking, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(priceHeader.PK, usage1.PriceHeader.PK);
			AssertNull(usage2.PriceHeader);
		}

		public void TestPriceAndCurrency()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var pricesWithOneCurrencyOnHeader = BillingTestHelper.CreatePriceHeader(stdLicCompany, BillingConstants.PriceHeaderType.ABMCustoms, "V1", "USD", new ZDateTime(2014, 1, 1));
			var itemWithCurrencyOnHeader = BillingTestHelper.AddPriceItem(pricesWithOneCurrencyOnHeader, "ZZZ", "", "", 1.1m);

			var pricesWithOneCurrencyOnItem = BillingTestHelper.CreatePriceHeader(stdLicCompany, BillingConstants.PriceHeaderType.ABMCustoms, "V2", "USD", new ZDateTime(2015, 1, 1));
			var itemWithCurrencyOnItem = BillingTestHelper.AddPriceItem(pricesWithOneCurrencyOnItem, "ZZZ", "", "", 2.2m);
			itemWithCurrencyOnItem.L7_RX_NKCurrency = "USD";

			var pricesWithMultipleCurrency = BillingTestHelper.CreatePriceHeader(stdLicCompany, BillingConstants.PriceHeaderType.ABMCustoms, "V3", "USD", new ZDateTime(2016, 1, 1));
			var itemWithMultiCurrency = BillingTestHelper.AddPriceItem(pricesWithMultipleCurrency, "ZZZ", "", "", 3.31m);
			BillingTestHelper.AddPriceItemRate(itemWithMultiCurrency, "USD", 3.3m);
			BillingTestHelper.AddPriceItemRate(itemWithMultiCurrency, "AUD", 4.4m);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var usageCurrencyOnHeader = new UniversalPriceSystemUsage(Factory, "AAA", BillingConstants.PriceHeaderType.ABMCustoms, new UsingParty(), new ZDateTime(2014, 1, 1), "ZZZ", 5, "", "");
			var usageCurrencyOnItem = new UniversalPriceSystemUsage(Factory, "AAA", BillingConstants.PriceHeaderType.ABMCustoms, new UsingParty(), new ZDateTime(2015, 1, 1), "ZZZ", 5, "", "");
			var usageMultiCurrencyNoCurrencyInCtor = new UniversalPriceSystemUsage(Factory, "AAA", BillingConstants.PriceHeaderType.ABMCustoms, new UsingParty(), new ZDateTime(2016, 1, 1), "ZZZ", 5, "", "");
			var usageMultiCurrencyWithCurrencyInCtor = new UniversalPriceSystemUsage(Factory, "AAA", BillingConstants.PriceHeaderType.ABMCustoms, new UsingParty(), new ZDateTime(2016, 1, 1), "ZZZ", 5, "", "AUD");
			var usageMultiCurrencyWithUnknownCurrencyInCtor = new UniversalPriceSystemUsage(Factory, "AAA", BillingConstants.PriceHeaderType.ABMCustoms, new UsingParty(), new ZDateTime(2016, 1, 1), "ZZZ", 5, "", "NZD");

			AssertEquals(1.1m, usageCurrencyOnHeader.UnitPrice);
			AssertEquals("USD", usageCurrencyOnHeader.CurrencyCode);

			AssertEquals(2.2m, usageCurrencyOnItem.UnitPrice);
			AssertEquals("USD", usageCurrencyOnItem.CurrencyCode);

			AssertEquals(0m, usageMultiCurrencyNoCurrencyInCtor.UnitPrice);
			AssertEquals("", usageMultiCurrencyNoCurrencyInCtor.CurrencyCode);

			AssertEquals(4.4m, usageMultiCurrencyWithCurrencyInCtor.UnitPrice);
			AssertEquals("AUD", usageMultiCurrencyWithCurrencyInCtor.CurrencyCode);

			AssertEquals(0m, usageMultiCurrencyWithUnknownCurrencyInCtor.UnitPrice);
			AssertEquals("NZD", usageMultiCurrencyWithUnknownCurrencyInCtor.CurrencyCode);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UniversalPriceSystemUsage(Factory, "DUM", "DUM", new UsingParty(), EdiDateTest.MonthToday);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion
	}
}
