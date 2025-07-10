using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Fax.Test
{
	[TestedType(typeof(FaxUsage))]
	internal class FaxUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			FaxUsage faxUsage = new FaxUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.Fax, faxUsage.SystemCode);
		}

		public void TestCurrencyCode()
		{
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(org, "AAA");
			BillingTestHelper.SetInvoiceCurrency(org, "NZD");

			FaxUsage faxUsage = new FaxUsage(Factory, user, EdiDateTest.MonthToday);
			AssertEquals("NZD", faxUsage.CurrencyCode);

			BillingTestHelper.SetInvoiceCurrency(org, "USD");
			faxUsage = new FaxUsage(Factory, user, EdiDateTest.MonthToday);
			AssertEquals("USD", faxUsage.CurrencyCode);

			EDIOrgHeader invoiceOrg = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			BillingTestHelper.SetInvoiceCurrency(invoiceOrg, "EUR");
			org.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = invoiceOrg.PK;
			faxUsage = new FaxUsage(Factory, user, EdiDateTest.MonthToday);
			AssertEquals("USD", faxUsage.CurrencyCode);
		}

		public void TestIsExchangeRatePricing()
		{
			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "SNK");
			var user = new UsingParty(org, "AAA");
			ClientLicencePriceHeader oldPriceHeader = org.LicCompany.PriceHeaders.AddNew();
			oldPriceHeader.L6_ValidFrom = new ZDateTime(2012, 6, 1);
			// Any feetype that isn't Per Page will use the old fax rates from the registry
			BillingTestHelper.AddPriceItem(oldPriceHeader, "FAX", BillingConstants.FeeType.OldWebModule, "", 0);

			ClientLicencePriceHeader newPriceHeader = org.LicCompany.PriceHeaders.AddNew();
			newPriceHeader.L6_ValidFrom = new ZDateTime(2013, 6, 1);
			BillingTestHelper.AddPriceItem(newPriceHeader, "FAX", BillingConstants.FeeType.PerPage, "", 0);

			FaxUsage oldFaxUsage = new FaxUsage(Factory, user, new ZDateTime(2012, 6, 1));
			FaxUsage newFaxUsage = new FaxUsage(Factory, user, new ZDateTime(2013, 6, 1));

			AssertEquals(false, oldFaxUsage.IsExchangeRatePricing);
			AssertEquals(true, newFaxUsage.IsExchangeRatePricing);
		}

		[TestDate(2012, 06, 01)]
		public void TestAmount()
		{
			// Fax Prices for 3 monthly periods
			ClientFaxPrice priceAUD1 = Factory.New<ClientFaxPrice>();
			priceAUD1.CFP_RX_NKCurrencyCode = "AUD";
			priceAUD1.CFP_PageRate = 0.08m;
			priceAUD1.CFP_Month = 6;
			priceAUD1.CFP_Year = 2012;
			ClientFaxPrice priceNZD1 = Factory.New<ClientFaxPrice>();
			priceNZD1.CFP_RX_NKCurrencyCode = "NZD";
			priceNZD1.CFP_PageRate = 0.12m;
			priceNZD1.CFP_Month = 6;
			priceNZD1.CFP_Year = 2012;

			ClientFaxPrice priceAUD2 = Factory.New<ClientFaxPrice>();
			priceAUD2.CFP_RX_NKCurrencyCode = "AUD";
			priceAUD2.CFP_PageRate = 0.11m;
			priceAUD2.CFP_Month = 7;
			priceAUD2.CFP_Year = 2012;
			ClientFaxPrice priceNZD2 = Factory.New<ClientFaxPrice>();
			priceNZD2.CFP_RX_NKCurrencyCode = "NZD";
			priceNZD2.CFP_PageRate = 0.15m;
			priceNZD2.CFP_Month = 7;
			priceNZD2.CFP_Year = 2012;

			ClientFaxPrice priceAUD3 = Factory.New<ClientFaxPrice>();
			priceAUD3.CFP_RX_NKCurrencyCode = "AUD";
			priceAUD3.CFP_PageRate = 0.14m;
			priceAUD3.CFP_Month = 7;
			priceAUD3.CFP_Year = 2013;
			ClientFaxPrice priceNZD3 = Factory.New<ClientFaxPrice>();
			priceNZD3.CFP_RX_NKCurrencyCode = "NZD";
			priceNZD3.CFP_PageRate = 0.18m;
			priceNZD3.CFP_Month = 7;
			priceNZD3.CFP_Year = 2013;

			Factory.Save();

			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(org, "AAA");
			FaxUsage faxUsage1 = new FaxUsage(Factory, user, new ZDateTime(2012, 6, 1));
			AssertEquals("Precondition", 0m, faxUsage1.Amount);
			faxUsage1.PageCount = 7;
			AssertEquals("Precondition", 0m, faxUsage1.Amount);
			FaxUsage faxUsage2 = new FaxUsage(Factory, user, new ZDateTime(2012, 7, 1)) { PageCount = 7 };
			FaxUsage faxUsage3 = new FaxUsage(Factory, user, new ZDateTime(2013, 7, 1)) { PageCount = 7 };

			ClientLicencePriceHeader priceHeader2012 = org.LicCompany.PriceHeaders.AddNew();
			priceHeader2012.L6_ValidFrom = TestDateAttribute.Date; // 2012/06/01
			BillingTestHelper.AddPriceItem(priceHeader2012, "FAX", BillingConstants.FeeType.PerPage, "", 0);

			BillingTestHelper.SetInvoiceCurrency(org, "AUD");
			AssertEquals("2012/06 AUD rate should be used", faxUsage1.PageCount * priceAUD1.CFP_PageRate, faxUsage1.Amount);
			AssertEquals("2012/07 AUD rate should be used", faxUsage2.PageCount * priceAUD2.CFP_PageRate, faxUsage2.Amount);
			AssertEquals("2013/07 AUD rate should be used", faxUsage3.PageCount * priceAUD3.CFP_PageRate, faxUsage3.Amount);
			BillingTestHelper.SetInvoiceCurrency(org, "NZD");
			AssertEquals("2012/06 NZD rate should be used", faxUsage1.PageCount * priceNZD1.CFP_PageRate, faxUsage1.Amount);
			AssertEquals("2012/07 NZD rate should be used", faxUsage2.PageCount * priceNZD2.CFP_PageRate, faxUsage2.Amount);
			AssertEquals("2013/07 NZD rate should be used", faxUsage3.PageCount * priceNZD3.CFP_PageRate, faxUsage3.Amount);

			TestDateAttribute.Date = TestDateAttribute.Date.AddYears(1);
			FaxUsage faxUsage4 = new FaxUsage(Factory, user, new ZDateTime(2013, 8, 1)) { PageCount = 10 };
			ClientLicencePriceHeader priceHeader2013 = org.LicCompany.PriceHeaders.AddNew();
			priceHeader2013.L6_ValidFrom = TestDateAttribute.Date; // 2013/06/01
																   // Any feetype that isn't Per Page will use the old fax rates from the registry
			BillingTestHelper.AddPriceItem(priceHeader2013, "FAX", BillingConstants.FeeType.OldWebModule, "", 0);

			FaxPriceCollection prices = new FaxPriceCollection();
			prices.Add(new FaxPrice() { Code = "AUD", Price = 0.20m });
			prices.Add(new FaxPrice() { Code = "NZD", Price = 0.12m });
			EDIDataRegistry.Instance.FaxPriceRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, prices);

			BillingTestHelper.SetInvoiceCurrency(org, "AUD");
			AssertEquals("Registry AUD rate should be used", faxUsage4.PageCount * 0.20m, faxUsage4.Amount);
			BillingTestHelper.SetInvoiceCurrency(org, "NZD");
			AssertEquals("Registry NZD rate should be used", faxUsage4.PageCount * 0.12m, faxUsage4.Amount);

			BillingTestHelper.SetInvoiceCurrency(org, "USD");
			AssertEquals("Currency (USD) not in fax price registry", 0m, faxUsage3.Amount);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FaxUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
		}

		#endregion
	}
}
