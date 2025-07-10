using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(AirlineMessagingBill))]
	public class AirlineMessagingBillTest : TransactionalSystemBillTestCase<AirlineMessagingBill>
	{
		#region Invoicing

		public void TestCreateInvoiceLines_ChargeableUsages()
		{
			var org = SetupAirlineMessagingPriceList();
			var user = new UsingParty(org);
			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("S1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("WEC", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("WRC", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(5, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("FWB", lines[0].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - BT - 20 transactions at USD 0.2000 per transaction", lines[0].Description);
				AssertEquals("FHL", lines[1].ChargeCodeName);
				AssertEquals("FHL Airline Messaging - BT - 30 transactions at USD 0.1000 per transaction", lines[1].Description);
				AssertEquals("FSU", lines[2].ChargeCodeName);
				AssertEquals("FSU Airline Messaging - BT - 10 transactions at USD 0.0500 per transaction", lines[2].Description);
				AssertEquals("FWB", lines[3].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - Traxon (EDP Service) - 10 transactions at USD 0.5000 per transaction", lines[3].Description);
				AssertEquals("FWB", lines[4].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - Traxon (RCF Service) - 10 transactions at USD 0.4000 per transaction", lines[4].Description);
			});
		}

		public void TestCreateInvoiceLines_RemitableUsages()
		{
			var org = SetupTraxonPriceList();
			var user = new UsingParty(org);
			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("WXP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("HXP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("SXP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("WEP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 50 });
			usageList.Add(new AirlineMessagingUsage("WRP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 40 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(5, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("DISCFWBFHL", lines[0].ChargeCodeName);
				AssertEquals("Remit: FWB Airline Messaging - Traxon (CX/LY/AI/5X/US) - 20 transactions at EUR 0.5000 per transaction", lines[0].Description);
				AssertEquals("DISCFWBFHL", lines[1].ChargeCodeName);
				AssertEquals("Remit: FHL Airline Messaging - Traxon (CX/LY/AI/5X/US) - 30 transactions at EUR 0.4000 per transaction", lines[1].Description);
				AssertEquals("DISCFWBFHL", lines[2].ChargeCodeName);
				AssertEquals("Remit: FSU Airline Messaging - Traxon (CX/LY/AI/5X/US) - 10 transactions at EUR 0.1000 per transaction", lines[2].Description);
				AssertEquals("DISCFWBFHL", lines[3].ChargeCodeName);
				AssertEquals("Remit: FWB Airline Messaging - Traxon (EDP Service) - 50 transactions at EUR 0.5000 per transaction", lines[3].Description);
				AssertEquals("DISCFWBFHL", lines[4].ChargeCodeName);
				AssertEquals("Remit: FWB Airline Messaging - Traxon (RCF Service) - 40 transactions at EUR 0.1500 per transaction", lines[4].Description);
			});
		}

		public void TestCreateInvoiceLines_Discount()
		{
			var org = SetupAirlineMessagingPriceList();
			var user = new UsingParty(org);
			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("S1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });

			var discount = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 10;
			discount.L5_StartDate = new ZDateTime(2014, 1, 1);

			var surcharge = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			surcharge.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge.L5_Discount = -10;
			surcharge.L5_Description = "Testing";

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(5, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("FWB", lines[0].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - BT - 20 transactions at USD 0.2000 per transaction", lines[0].Description);
				AssertEquals("FHL", lines[1].ChargeCodeName);
				AssertEquals("FHL Airline Messaging - BT - 30 transactions at USD 0.1000 per transaction", lines[1].Description);
				AssertEquals("FSU", lines[2].ChargeCodeName);
				AssertEquals("FSU Airline Messaging - BT - 10 transactions at USD 0.0500 per transaction", lines[2].Description);
				AssertEquals("DISCFWBFHL", lines[3].ChargeCodeName);
				AssertEquals("Airline Messaging Discount - BT", lines[3].Description);
				AssertEquals("SURCODPL", lines[4].ChargeCodeName);
				AssertEquals("Airline Messaging Surcharge - BT", lines[4].Description);
			});
		}

		public void TestCreateInvoiceLines_DiscountCurrencyConversion()
		{
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();

			var payingOrg = SetupAirlineMessagingPriceList("DDDYYYSYD");

			var org1 = SetupAirlineMessagingPriceList("DDDCCCMEL");
			org1.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = payingOrg.PK;
			org1.LicCompany.InvoiceDeliveries[0].L9_AT_TaxId = taxRate1.PK;

			var org2 = SetupAirlineMessagingPriceList("DDDEEEBRN");
			org2.LicCompany.InvoiceDeliveries[0].L9_OH_InvoiceTo = payingOrg.PK;
			org2.LicCompany.InvoiceDeliveries[0].L9_AT_TaxId = taxRate2.PK;

			var aud = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			var exchangeRate = aud.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exchangeRate.RE_SellRate = 0.6997m;
			exchangeRate.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);

			Factory.Save();

			var user1 = new UsingParty(org1);
			var user2 = new UsingParty(org2);
			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 138 });
			usageList.Add(new AirlineMessagingUsage("H2C", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 86 });
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, user2, new ZDateTime(2014, 11, 30)) { TransactionCount = 4105 });
			usageList.Add(new AirlineMessagingUsage("H2C", Factory, user2, new ZDateTime(2014, 11, 30)) { TransactionCount = 10862 });

			var discount = payingOrg.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_SubCode = "AD2";
			discount.L5_Discount = 100;
			discount.L5_StartDate = new ZDateTime(2014, 1, 1);

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var invoiceFactory = new BusinessObjectFactory();
			var invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranch.PK;

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now, invoice);

			CombineAssertions(() =>
			{
				AssertEquals("FWB Airline Messaging - Delta - 4105 transactions at USD 0.1000 per transaction", lines[0].Description);
				AssertEquals(410.50m, lines[0].Amount);
				AssertEquals(586.68m, lines[0].AmountInInvoiceCurrency);

				AssertEquals("FHL Airline Messaging - Delta - 10862 transactions at USD 0.1000 per transaction", lines[1].Description);
				AssertEquals(1086.20m, lines[1].Amount);
				AssertEquals(1552.38m, lines[1].AmountInInvoiceCurrency);

				AssertEquals("Airline Messaging Discount - Delta", lines[2].Description);
				AssertEquals(-1496.70m, lines[2].Amount);
				AssertEquals(-2139.06m, lines[2].AmountInInvoiceCurrency);

				AssertEquals("FWB Airline Messaging - Delta - 138 transactions at USD 0.1000 per transaction", lines[3].Description);
				AssertEquals(13.80m, lines[3].Amount);
				AssertEquals(19.72m, lines[3].AmountInInvoiceCurrency);

				AssertEquals("FHL Airline Messaging - Delta - 86 transactions at USD 0.1000 per transaction", lines[4].Description);
				AssertEquals(8.60m, lines[4].Amount);
				AssertEquals(12.29m, lines[4].AmountInInvoiceCurrency);

				AssertEquals("Airline Messaging Discount - Delta", lines[5].Description);
				AssertEquals(-22.40m, lines[5].Amount);
				AssertEquals(-32.01m, lines[5].AmountInInvoiceCurrency);
			});
		}

		public void TestCreateInvoiceLines_MergingAndGrouping()
		{
			var org1 = SetupAirlineMessagingPriceList("DDDMEOSYD");
			var org2 = SetupAirlineMessagingPriceList("DDDMEOMEL");
			var user1 = new UsingParty(org1);
			var user2 = new UsingParty(org2);

			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("S1C", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 50 });
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user2, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user2, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("S1C", Factory, user2, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());
			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(4, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("FWB", lines[0].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - BT - 40 transactions at USD 0.2000 per transaction", lines[0].Description);
				AssertEquals("FHL", lines[1].ChargeCodeName);
				AssertEquals("FHL Airline Messaging - BT - 60 transactions at USD 0.1000 per transaction", lines[1].Description);
				AssertEquals("FSU", lines[2].ChargeCodeName);
				AssertEquals("FSU Airline Messaging - BT - 20 transactions at USD 0.0500 per transaction", lines[2].Description);
				AssertEquals("FWB", lines[3].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - Delta - 50 transactions at USD 0.1000 per transaction", lines[3].Description);
			});

			var org3 = SetupAirlineMessagingPriceList("DDDMEOBNE");
			var user3 = new UsingParty(org3);
			var priceItems = org3.LicCompany.PriceHeaders[0].LocalOrStandardItems;
			priceItems.FindByCode("W1C").L7_Price = 0.30m;
			priceItems.FindByCode("H1C").L7_Price = 0.30m;

			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user3, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user3, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("S1C", Factory, user3, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, user3, new ZDateTime(2014, 11, 30)) { TransactionCount = 50 });

			bill.PopulateFromSystemUsages(usageList.ToArray());
			lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(6, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("FWB", lines[0].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - BT - 20 transactions at USD 0.3000 per transaction", lines[0].Description);
				AssertEquals(false, lines[0].RequireCommentLineAfter);

				AssertEquals("FHL", lines[1].ChargeCodeName);
				AssertEquals("FHL Airline Messaging - BT - 30 transactions at USD 0.3000 per transaction", lines[1].Description);
				AssertEquals(false, lines[1].RequireCommentLineAfter);

				AssertEquals("FSU", lines[2].ChargeCodeName);
				AssertEquals("FSU Airline Messaging - BT - 30 transactions at USD 0.0500 per transaction", lines[2].Description);
				AssertEquals(false, lines[2].RequireCommentLineAfter);

				AssertEquals("FWB", lines[3].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - BT - 40 transactions at USD 0.2000 per transaction", lines[3].Description);
				AssertEquals(false, lines[3].RequireCommentLineAfter);

				AssertEquals("FHL", lines[4].ChargeCodeName);
				AssertEquals("FHL Airline Messaging - BT - 60 transactions at USD 0.1000 per transaction", lines[4].Description);
				AssertEquals(true, lines[4].RequireCommentLineAfter);

				AssertEquals("FWB", lines[5].ChargeCodeName);
				AssertEquals("FWB Airline Messaging - Delta - 100 transactions at USD 0.1000 per transaction", lines[5].Description);
				AssertEquals(true, lines[5].RequireCommentLineAfter);
			});
		}

		public void TestCreateInvoiceLines_TaxGrouping()
		{
			var salesTax = BillingTestHelper.CreateChargeCode(Factory, null, "AAA");
			salesTax.AC_Desc = "Sales Tax 9.5%";

			var freeTax = BillingTestHelper.CreateChargeCode(Factory, null, "FRE");
			freeTax.AC_Desc = "Free Tax";

			Factory.Save();

			var org1 = SetupAirlineMessagingPriceList("DDDMEOSYD");
			var invoiceDelivery1 = org1.LicCompany.InvoiceDeliveries[0];
			invoiceDelivery1.L9_AC_SalesTaxChargeCode = salesTax.PK;
			invoiceDelivery1.L9_SystemCode = BillingConstants.BillingSystem.All;

			var discount1 = org1.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			discount1.L5_SubCode = "AD1";
			discount1.L5_Type = BillingConstants.DiscountType.Volume;
			discount1.L5_BreakAmount = 10;
			discount1.L5_Discount = 10;
			discount1.L5_StartDate = new ZDateTime(2014, 1, 1);

			var org2 = SetupAirlineMessagingPriceList("DDDMEOMEL");
			var invoiceDelivery2 = org2.LicCompany.InvoiceDeliveries[0];
			invoiceDelivery2.L9_OH_InvoiceTo = org1.PK;
			invoiceDelivery2.L9_SystemCode = BillingConstants.BillingSystem.All;
			invoiceDelivery2.L9_AC_SalesTaxChargeCode = freeTax.PK;

			var org3 = SetupAirlineMessagingPriceList("DDDMEOBNE");
			var invoiceDelivery3 = org3.LicCompany.InvoiceDeliveries[0];
			invoiceDelivery3.L9_OH_InvoiceTo = org1.PK;
			invoiceDelivery3.L9_SystemCode = BillingConstants.BillingSystem.All;
			invoiceDelivery3.L9_AC_SalesTaxChargeCode = freeTax.PK;

			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, new UsingParty(org1), new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, new UsingParty(org1), new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, new UsingParty(org2), new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, new UsingParty(org2), new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, new UsingParty(org2), new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, new UsingParty(org3), new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());
			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(7, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("FWB Airline Messaging - BT - 10 transactions at USD 0.2000 per transaction", lines[0].Description);
				AssertEquals(2m, lines[0].Amount);
				AssertEquals("FHL Airline Messaging - BT - 10 transactions at USD 0.1000 per transaction", lines[1].Description);
				AssertEquals(1m, lines[1].Amount);
				AssertEquals("Airline Messaging Discount - BT", lines[2].Description);
				AssertEquals(-0.3m, lines[2].Amount);
				AssertEquals(true, lines[2].RequireCommentLineAfter);

				AssertEquals("FWB Airline Messaging - BT - 20 transactions at USD 0.2000 per transaction", lines[3].Description);
				AssertEquals(4m, lines[3].Amount);
				AssertEquals("FHL Airline Messaging - BT - 20 transactions at USD 0.1000 per transaction", lines[4].Description);
				AssertEquals(2m, lines[4].Amount);
				AssertEquals("Airline Messaging Discount - BT", lines[5].Description);
				AssertEquals(-0.6m, lines[5].Amount);
				AssertEquals(true, lines[5].RequireCommentLineAfter);

				AssertEquals("FWB Airline Messaging - Delta - 50 transactions at USD 0.1000 per transaction", lines[6].Description);
				AssertEquals(5m, lines[6].Amount);
				AssertEquals(true, lines[6].RequireCommentLineAfter);

				AssertEquals(0, lines[0].Sequence);
				AssertEquals(1, lines[1].Sequence);
				AssertEquals(2, lines[2].Sequence);
				AssertEquals(3, lines[3].Sequence);
				AssertEquals(4, lines[4].Sequence);
				AssertEquals(5, lines[5].Sequence);
				AssertEquals(6, lines[6].Sequence);
			});
		}

		#endregion

		#region Summary Report

		public void TestGetDiscountSummarySections()
		{
			var org = SetupAirlineMessagingPriceList();
			var user = new UsingParty(org);

			var discount1 = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			discount1.L5_SubCode = "AD1";
			discount1.L5_Type = BillingConstants.DiscountType.Volume;
			discount1.L5_BreakAmount = 50;
			discount1.L5_Discount = 10;
			discount1.L5_StartDate = new ZDateTime(2014, 1, 1);

			var discount2 = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount2.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			discount2.L5_SubCode = "AD2";
			discount2.L5_Type = BillingConstants.DiscountType.Volume;
			discount2.L5_BreakAmount = 50;
			discount2.L5_Discount = 20;
			discount2.L5_StartDate = new ZDateTime(2014, 1, 1);

			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("S1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 50 });
			usageList.Add(new AirlineMessagingUsage("H2C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 50 });
			usageList.Add(new AirlineMessagingUsage("S2C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 50 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			ZDecimal expectedAmount = (0.20m * 20 + 0.10m * 30 + 0.05m * 10) + (0.10m * 50 + 0.10m * 50);
			AssertEquals("Amount", expectedAmount, bill.Amount);

			ZDecimal expectedDiscount = 0.1m * (0.20m * 20 + 0.10m * 30 + 0.05m * 10) + 0.2m * (0.10m * 50 + 0.10m * 50);
			AssertEquals("Discounts for each provider applied", expectedDiscount, bill.DiscountAmount);

			var sections = bill.GetDiscountSummarySections();
			AssertEquals(2, sections[0].Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Amount Calculations Applied", sections[0].Header.MainDescription);
				AssertEquals("BT - Volume Discount: -0.75 (-10% * 7.50)", sections[0].Lines[0].MainDescription);
				AssertEquals("Delta - Volume Discount: -2.00 (-20% * 10.00)", sections[0].Lines[1].MainDescription);
			});

			usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H2C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("S2C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });

			bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			expectedAmount = 0.10m * 20 + 0.10m * 20;
			AssertEquals("Amount", expectedAmount, bill.Amount);
			AssertEquals("No discount because S2C usage has zero price then billable usage count is 20 + 20 < 50", 0m, bill.DiscountAmount);
		}

		public void TestGetSurchargeSummarySections()
		{
			var org = SetupAirlineMessagingPriceList();
			var user = new UsingParty(org);

			var surcharge = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			surcharge.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			surcharge.L5_SubCode = "AD1";
			surcharge.L5_Type = BillingConstants.DiscountType.Surcharge;
			surcharge.L5_Discount = -10;
			surcharge.L5_Description = "Testing";

			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("S1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			ZDecimal expectedAmount = (0.20m * 20 + 0.10m * 30 + 0.05m * 10);
			AssertEquals("Amount", expectedAmount, bill.Amount);

			ZDecimal expectedSurcharge = 0.1m * (0.20m * 20 + 0.10m * 30 + 0.05m * 10);
			AssertEquals("Surcharge", expectedSurcharge, bill.SurchargeAmount);

			var sections = bill.GetSurchargeSummarySections();
			AssertEquals(1, sections[0].Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Amount Calculations Applied", sections[0].Header.MainDescription);
				AssertEquals("BT - Testing Surcharge: 0.75 (10% * 7.50)", sections[0].Lines[0].MainDescription);
			});
		}

		public void TestGetGeneralSummarySections()
		{
			var org = SetupAirlineMessagingPriceList();
			var user = new UsingParty(org);
			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("S1C", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("WEC", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("WRC", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);
			AssertEquals(2, sections.Length);
			var section1 = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Usage - BT", section1.Header.MainDescription);
				AssertEquals("FWB (net) - BT", section1.Lines[0].MainDescription);
				AssertEquals("FHL (net) - BT", section1.Lines[1].MainDescription);
				AssertEquals("FSU - BT", section1.Lines[2].MainDescription);
			});

			var section2 = sections[1];
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Usage - Traxon", section2.Header.MainDescription);
				AssertEquals("FWB - Traxon (EDP Service)", section2.Lines[0].MainDescription);
				AssertEquals("FWB - Traxon (RCF Service)", section2.Lines[1].MainDescription);
			});
		}

		public void TestGetGeneralSummarySections_Traxon()
		{
			var org = SetupTraxonPriceList();
			var user = new UsingParty(org);

			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("WAP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("HAP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("SAP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("WEP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("WRP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("WAN", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("HAN", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("SAN", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("WXP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("HXP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 30 });
			usageList.Add(new AirlineMessagingUsage("SXP", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);

			AssertEquals(3, sections.Length);
			var section1 = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Usage (Chargeable)", section1.Header.MainDescription);
				AssertEquals("FWB (net) - Traxon", section1.Lines[0].MainDescription);
				AssertEquals("FHL (net) - Traxon", section1.Lines[1].MainDescription);
			});

			var section2 = sections[1];
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Usage (Remitable)", section2.Header.MainDescription);
				AssertEquals("Remit: FWB - Traxon (CX/LY/AI/5X/US)", section2.Lines[0].MainDescription);
				AssertEquals("Remit: FHL - Traxon (CX/LY/AI/5X/US)", section2.Lines[1].MainDescription);
				AssertEquals("Remit: FSU - Traxon (CX/LY/AI/5X/US)", section2.Lines[2].MainDescription);
				AssertEquals("Remit: FWB - Traxon (EDP Service)", section2.Lines[3].MainDescription);
				AssertEquals("Remit: FWB - Traxon (RCF Service)", section2.Lines[4].MainDescription);
			});

			var section3 = sections[2];
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Usage (Non-Chargeable)", section3.Header.MainDescription);
				AssertEquals("FWB (net) - Traxon (Non-Chargeable)", section3.Lines[0].MainDescription);
				AssertEquals("FHL (net) - Traxon (Non-Chargeable)", section3.Lines[1].MainDescription);
				AssertEquals("FSU - Traxon (Non-Chargeable)", section3.Lines[2].MainDescription);
			});
		}

		public void TestGetGroupSummarySections()
		{
			var org1 = SetupAirlineMessagingPriceList("DDD111SYD");
			var org2 = SetupAirlineMessagingPriceList("DDD111MEL");
			var user1 = new UsingParty(org1, "PRD");
			var user2 = new UsingParty(org2, "SRV");

			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user1, new ZDateTime(2014, 10, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user2, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user2, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("W2C", Factory, user2, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGroupSummarySections();
			AssertEquals(2, sections.Length);

			var section1 = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Group Summary - BT", section1.Header.MainDescription);
				AssertEquals("DDD111MEL (DDD-MEL-SRV) Nov 2014", section1.Lines[0].MainDescription);
				AssertEquals("DDD111SYD (DDD-SYD-PRD) Oct 2014", section1.Lines[1].MainDescription);
				AssertEquals("DDD111SYD (DDD-SYD-PRD) Nov 2014", section1.Lines[2].MainDescription);
				AssertEquals("6.00", section1.Lines[0].Amount);
				AssertEquals("2.00", section1.Lines[1].Amount);
				AssertEquals("3.00", section1.Lines[2].Amount);
			});

			var section2 = sections[1];
			CombineAssertions(() =>
			{
				AssertEquals("Airline Messaging Group Summary - Delta", section2.Header.MainDescription);
				AssertEquals("DDD111MEL (DDD-MEL-SRV) Nov 2014", section2.Lines[0].MainDescription);
				AssertEquals("2.00", section2.Lines[0].Amount);
			});
		}

		#endregion

		public void TestCalculateMinimumFeeContribution()
		{
			var org1 = SetupAirlineMessagingPriceList("DDD111SYD");
			var org2 = SetupAirlineMessagingPriceList("DDD111MEL");

			var db1 = org1.LicCompany.LicEnterprise.Databases.AddNew();
			var licence1 = Factory.New<LicenceHeader>();
			licence1.LA_LD = db1.PK;
			licence1.LA_LC = org1.LicCompany.PK;

			var db2 = org1.LicCompany.LicEnterprise.Databases.AddNew();
			var licence2 = Factory.New<LicenceHeader>();
			licence2.LA_LD = db2.PK;
			licence2.LA_LC = org2.LicCompany.PK;

			var user1 = new UsingParty(licence1);
			var user2 = new UsingParty(licence2);

			var usageList = new List<AirlineMessagingUsage>();
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user1, new ZDateTime(2016, 5, 1)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user1, new ZDateTime(2016, 5, 1)) { TransactionCount = 10 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user1, new ZDateTime(2016, 5, 1)) { TransactionCount = 10 });

			usageList.Add(new AirlineMessagingUsage("W1C", Factory, user2, new ZDateTime(2016, 5, 1)) { TransactionCount = 20 });
			usageList.Add(new AirlineMessagingUsage("H1C", Factory, user2, new ZDateTime(2016, 5, 1)) { TransactionCount = 20 });

			usageList.Add(new AirlineMessagingUsage("W2C", Factory, user1, new ZDateTime(2016, 6, 1)) { TransactionCount = 20 });

			var bill = new AirlineMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var minimumFeeContribution = bill.CalculateMinimumFeeContribution().ToArray();
			AssertEquals(3, minimumFeeContribution.Length);
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence2.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 6, 1)));
		}

		EDIOrgHeader SetupAirlineMessagingPriceList(string orgCode = "DDDCOMSYD")
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = orgCode.Substring(0, 3);
			org.LicCompany.QuickAddOtherPriceList();
			org.LicCompany.PriceHeaders[0].L6_ValidFrom = new ZDateTime(2014, 1, 1);
			org.LicCompany.PriceHeaders[0].L6_RX_NKCurrency = "USD";
			var priceItems = org.LicCompany.PriceHeaders[0].LocalOrStandardItems;
			priceItems.FindByCode("W1C").L7_Price = 0.20m;
			priceItems.FindByCode("H1C").L7_Price = 0.10m;
			priceItems.FindByCode("S1C").L7_Price = 0.05m;
			priceItems.FindByCode("W2C").L7_Price = 0.10m;
			priceItems.FindByCode("H2C").L7_Price = 0.10m;
			priceItems.FindByCode("S2C").L7_Price = 0.00m;
			priceItems.FindByCode("WEC").L7_Price = 0.50m;
			priceItems.FindByCode("HEC").L7_Price = 0.50m;
			priceItems.FindByCode("WRC").L7_Price = 0.40m;
			priceItems.FindByCode("HRC").L7_Price = 0.40m;

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		EDIOrgHeader SetupTraxonPriceList()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDCOMSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.QuickAddOtherPriceList();
			org.LicCompany.PriceHeaders[0].L6_ValidFrom = new ZDateTime(2014, 1, 1);
			org.LicCompany.PriceHeaders[0].L6_RX_NKCurrency = "USD";
			var priceItems = org.LicCompany.PriceHeaders[0].LocalOrStandardItems;
			priceItems.FindByCode("WAP").L7_Price = 0.30m;
			priceItems.FindByCode("HAP").L7_Price = 0.20m;
			priceItems.FindByCode("SAP").L7_Price = 0m;
			priceItems.FindByCode("WAN").L7_Price = 0m;
			priceItems.FindByCode("HAN").L7_Price = 0m;
			priceItems.FindByCode("SAN").L7_Price = 0m;
			priceItems.FindByCode("WXP").L7_Price = -0.50m;
			priceItems.FindByCode("HXP").L7_Price = -0.40m;
			priceItems.FindByCode("SXP").L7_Price = -0.10m;
			priceItems.FindByCode("WEP").L7_Price = -0.50m;
			priceItems.FindByCode("WRP").L7_Price = -0.15m;

			priceItems.FindByCode("WAP").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("HAP").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("SAP").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("WAN").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("HAN").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("SAN").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("WXP").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("HXP").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("SXP").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("WEP").L7_RX_NKCurrency = "EUR";
			priceItems.FindByCode("WRP").L7_RX_NKCurrency = "EUR";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		#region Overrides

		protected override SystemUsage[] CreateValidSystemUsages()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);

			return new SystemUsage[]
			{
				new AirlineMessagingUsage("W1C", Factory, user, new ZDateTime(2010, 10, 01)),
				new AirlineMessagingUsage("H1C", Factory, user, new ZDateTime(2010, 11, 01))
			};
		}

		protected override AirlineMessagingBill GetNewSystemBill()
		{
			return new AirlineMessagingBill(Factory);
		}

		#endregion
	}
}
