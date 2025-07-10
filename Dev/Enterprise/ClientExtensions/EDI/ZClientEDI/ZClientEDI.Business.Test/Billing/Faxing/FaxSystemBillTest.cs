using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Fax.Test
{
	[TestedType(typeof(FaxSystemBill))]
	internal class FaxSystemBillTest : TransactionalSystemBillTestCase<FaxSystemBill>
	{
		public void TestAddAmountLine()
		{
			ClientFaxPrice priceAUD = Factory.New<ClientFaxPrice>();
			priceAUD.CFP_RX_NKCurrencyCode = "AUD";
			priceAUD.CFP_PageRate = 0.17m;
			priceAUD.CFP_Month = 10;
			priceAUD.CFP_Year = 2010;
			ClientFaxPrice priceNZD = Factory.New<ClientFaxPrice>();
			priceNZD.CFP_RX_NKCurrencyCode = "NZD";
			priceNZD.CFP_PageRate = 0.10m;
			priceNZD.CFP_Month = 10;
			priceNZD.CFP_Year = 2010;

			FaxPriceCollection prices = new FaxPriceCollection();
			prices.Add(new FaxPrice() { Code = "AUD", Price = 0.20m });
			prices.Add(new FaxPrice() { Code = "NZD", Price = 0.12m });
			EDIDataRegistry.Instance.FaxPriceRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, prices);

			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(org);
			BillingTestHelper.SetInvoicing(org, Env.CurrentBranch.PK, "AUD");

			AccTaxRate rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, "PROGFAXES");
			Factory.Save();

			FaxUsage usage = new FaxUsage(Factory, user, new ZDateTime(2010, 10, 01));
			usage.PageCount = 71;

			FaxSystemBill bill = new FaxSystemBill(Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage });

			AssertEquals("Amount", 71 * 0.20m, bill.Amount);
			AssertEquals("Discount", 0m, bill.DiscountAmount);

			List<SystemBill.BillLine> lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals(1, lines.Count);
			AssertAmountLine(lines[0], 71 * 0.20m, "PROGFAXES",
				"Faxing Service\r\n71 @ AUD 0.20 per page");

			ClientLicencePriceHeader newPriceHeader = org.LicCompany.PriceHeaders.AddNew();
			newPriceHeader.L6_ValidFrom = new ZDateTime(2010, 10, 1);
			BillingTestHelper.AddPriceItem(newPriceHeader, "FAX", BillingConstants.FeeType.PerPage, "", 0);

			bill.PopulateFromSystemUsages(new SystemUsage[] { usage });
			AssertEquals("Amount", 71 * 0.17m, bill.Amount);
			AssertEquals("Discount", 0m, bill.DiscountAmount);

			lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals(1, lines.Count);
			AssertAmountLine(lines[0], 71 * 0.17m, "PROGFAXES",
				"Faxing Service\r\nUSD 0.16 is converted to your invoice currency at the xe.com rate on the 1st of the billing month\r\n71 @ AUD 0.17 per page");
		}

		void AssertAmountLine(SystemBill.BillLine invoiceLine, ZDecimal amount, ZString amountChargeCodeName, ZString description)
		{
			AssertEquals(amount, invoiceLine.Amount);
			AssertEquals(amountChargeCodeName, invoiceLine.ChargeCodeName);
			AssertEquals(description, invoiceLine.Description);
		}

		public void TestValidateAll()
		{
			FaxPriceCollection prices = new FaxPriceCollection();
			prices.Add(new FaxPrice() { Code = "AUD", Price = 0.20m });
			prices.Add(new FaxPrice() { Code = "NZD", Price = 0.12m });
			EDIDataRegistry.Instance.FaxPriceRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, prices);

			EDIOrgHeader org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(org);
			BillingTestHelper.SetInvoicing(org, Env.CurrentBranch.PK, "EUR");

			FaxUsage usage = new FaxUsage(Factory, user, new ZDateTime(2010, 10, 01));

			FaxSystemBill bill = new FaxSystemBill(Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage });

			AssertNoNotifications(bill);
			bill.ValidateAll(bill);
			AssertHasRowError(bill, FaxSystemBill.NoPrice);
			bill.ClearAllNotifications();

			BillingTestHelper.SetInvoiceCurrency(org, "AUD");
			bill.ValidateAll(bill);
			AssertNoNotifications(bill);
		}

		protected override FaxSystemBill GetNewSystemBill()
		{
			return new FaxSystemBill(Factory);
		}
	}
}
