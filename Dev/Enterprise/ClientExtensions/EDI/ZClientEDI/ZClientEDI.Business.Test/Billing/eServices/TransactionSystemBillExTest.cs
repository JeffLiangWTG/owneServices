using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(TransactionSystemBillEx))]
	public class TransactionSystemBillExTest : TransactionalSystemBillTestCase<TransactionSystemBillEx>
	{
		public void TestGetGroupSummarySections()
		{
			BillingConstants.BillingSystemList.AddPairIfNotExist("DDD", "eServices System");

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "AAA");
			var user1 = new UsingParty(org1);
			var priceHeader1 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader1.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader1.L6_RX_NKCurrency = "EUR";
			BillingTestHelper.AddPriceItem(priceHeader1, "A01", BillingConstants.FeeType.Transactional, "", 0.10m);

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user1, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });

			var bill = new TransactionSystemBillEx("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());
			var sections = bill.GetGroupSummarySections();
			AssertEquals(1, sections.Length);
			var section = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("eServices System Group Summary", section.Header.MainDescription);
				AssertEquals("AAASYD (AAA-SYD-xxx)", section.Lines[0].MainDescription);
				AssertEquals("2.00", section.Lines[0].Amount);
			});

			var org2 = BillingTestHelper.CreateOrganisation(Factory, "BBB", "SYD", "BBB");
			var user2 = new UsingParty(org2);
			var priceHeader2 = org2.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader2.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader2.L6_RX_NKCurrency = "EUR";
			BillingTestHelper.AddPriceItem(priceHeader2, "A01", BillingConstants.FeeType.Transactional, "", 0.10m);

			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user2, new ZDateTime(2014, 10, 31)) { TransactionCount = 50 });
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user1, new ZDateTime(2014, 10, 31)) { TransactionCount = 10 });

			bill = new TransactionSystemBillEx("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());
			sections = bill.GetGroupSummarySections();
			AssertEquals(1, sections.Length);
			section = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("eServices System Group Summary", section.Header.MainDescription);
				AssertEquals("AAASYD (AAA-SYD-xxx) Oct 2014", section.Lines[0].MainDescription);
				AssertEquals("AAASYD (AAA-SYD-xxx) Nov 2014", section.Lines[1].MainDescription);
				AssertEquals("BBBSYD (BBB-SYD-xxx) Oct 2014", section.Lines[2].MainDescription);
				AssertEquals("1.00", section.Lines[0].Amount);
				AssertEquals("2.00", section.Lines[1].Amount);
				AssertEquals("5.00", section.Lines[2].Amount);
			});
		}

		public void TestValidateUnitPrice()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);
			BillingTestHelper.SetInvoicing(organisation, Env.CurrentBranch.PK, "AUD");

			ClientLicencePriceHeader priceHeader = organisation.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);

			var usage = new EServicesSystemUsage("DDD", "A01", Factory, user, new ZDateTime(2014, 11, 1));

			var bill = new TransactionSystemBillEx("DDD", Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage });
			bill.ValidateAll(bill);
			AssertHasRowErrorContaining(bill, "[A01]: No price found for " + organisation.OH_Code);

			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_Code = "A01";
			priceItem.L7_Price = 10m;
			priceItem.L7_FeeType = BillingConstants.FeeType.Transactional;
			bill.ClearAllNotifications();
			bill.ValidateAll(bill);
			AssertNoNotifications(bill);

			usage = new EServicesSystemUsage(BillingConstants.BillingSystem.OceanTracingLegacy, BillingConstants.BillingSystem.OceanTracingLegacy, Factory, user, new ZDateTime(2014, 11, 1));
			bill = new TransactionSystemBillEx(BillingConstants.BillingSystem.OceanTracingLegacy, Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage });
			bill.ValidateAll(bill);
			AssertNoNotifications(bill);
		}

		public void TestInvoiceDescrption()
		{
			BillingConstants.BillingSystemList.AddPairIfNotExist("DDD", "eServices System");

			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "AAA");
			var user = new UsingParty(org);
			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			var priceItem = BillingTestHelper.AddPriceItem(priceHeader, "A01", BillingConstants.FeeType.Transactional, "", 0.10m);
			priceItem.L7_RX_NKCurrency = "EUR";

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user, new ZDateTime(2014, 11, 30)) { TransactionCount = 20 });

			var bill = new TransactionSystemBillEx("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(1, lines.Count);
			AssertEquals("eServices System - 20 Transactions at EUR 0.10 per Transaction", lines[0].Description);
		}

		public void TestInvoice_MultipleTaxGroup()
		{
			BillingConstants.BillingSystemList.AddPairIfNotExist("DDD", "eServices System");

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "AAA");
			var user1 = new UsingParty(org1);
			var invoiceDelivery1 = org1.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery1.L9_AT_TaxId = taxRate1.PK;

			var org2 = BillingTestHelper.CreateDependentOrganisation(org1, "BBB");
			var user2 = new UsingParty(org2);
			var invoiceDelivery2 = org2.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery2.L9_OH_InvoiceTo = org1.PK;
			invoiceDelivery2.L9_AT_TaxId = taxRate2.PK;

			var priceHeader = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader, "DDD", BillingConstants.FeeType.Transactional, "", 0.10m);

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "DDD", Factory, user1, new ZDateTime(2016, 5, 1)) { TransactionCount = 46 });
			usageList.Add(new EServicesSystemUsage("DDD", "DDD", Factory, user2, new ZDateTime(2016, 5, 1)) { TransactionCount = 16 });

			var bill = new TransactionSystemBillEx("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(2, lines.Count);
			AssertEquals("eServices System - 46 Transactions at USD 0.10 per Transaction", lines[0].Description);
			AssertEquals(4.60m, lines[0].Amount);
			AssertEquals("eServices System - 16 Transactions at USD 0.10 per Transaction", lines[1].Description);
			AssertEquals(1.60m, lines[1].Amount);
		}

		public void TestInvoiceWithCommitmentDiscount()
		{
			BillingConstants.BillingSystemList.AddPairIfNotExist("DDD", "eServices System");

			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "AAA");
			var user = new UsingParty(org);
			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader, "DDD", BillingConstants.FeeType.Transactional, "", 0.10m);

			var discount1 = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = "DDD";
			discount1.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount1.L5_Type = BillingConstants.DiscountType.Commitment;
			discount1.L5_Units = 1000;
			discount1.L5_Discount = 20m;

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "DDD", Factory, user, new ZDateTime(2016, 5, 1)) { TransactionCount = 800 });

			var bill = new TransactionSystemBillEx("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(2, lines.Count);
			AssertEquals("eServices System - 1000 Transactions at USD 0.10 per Transaction", lines[0].Description);
			AssertEquals(100m, lines[0].Amount);
			AssertEquals("eServices System Discount", lines[1].Description);
			AssertEquals(-20m, lines[1].Amount);
		}

		public void TestInvoiceWithCommitmentDiscount_MultiplePrices()
		{
			BillingConstants.BillingSystemList.AddPairIfNotExist("DDD", "eServices System");

			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "AAA");
			var user = new UsingParty(org);
			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			var priceHeader1 = org.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader1.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader1.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader1, "DDD", BillingConstants.FeeType.Transactional, "", 0.10m);

			var priceHeader2 = org.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader2.L6_ValidFrom = new ZDateTime(2016, 5, 1);
			priceHeader2.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader2, "DDD", BillingConstants.FeeType.Transactional, "", 0.15m);

			var discount1 = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = "DDD";
			discount1.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount1.L5_Type = BillingConstants.DiscountType.Commitment;
			discount1.L5_Units = 1000;
			discount1.L5_Discount = 20m;

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "DDD", Factory, user, new ZDateTime(2016, 4, 1)) { TransactionCount = 900 });
			usageList.Add(new EServicesSystemUsage("DDD", "DDD", Factory, user, new ZDateTime(2016, 5, 1)) { TransactionCount = 800 });

			var bill = new TransactionSystemBillEx("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			CombineAssertions(() =>
			{
				AssertInvoiceLine(lines[0], "eServices System - 1000 Transactions at USD 0.10 per Transaction", 100m);
				AssertInvoiceLine(lines[1], "eServices System - 1000 Transactions at USD 0.15 per Transaction", 150m);
				AssertInvoiceLine(lines[2], "eServices System Discount", -50m);
			});
		}

		void AssertInvoiceLine(SystemBill.BillLine line, string description, decimal amount)
		{
			AssertEquals("Description", description, line.Description);
			AssertEquals("Amount", amount, line.Amount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewSystemBill();
		}

		protected override TransactionSystemBillEx GetNewSystemBill()
		{
			return new TransactionSystemBillEx("DDD", Factory);
		}
	}
}
