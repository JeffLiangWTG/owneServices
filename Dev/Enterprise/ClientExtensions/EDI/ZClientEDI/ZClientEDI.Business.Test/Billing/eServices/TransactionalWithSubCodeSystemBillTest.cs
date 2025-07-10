using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(TransactionalWithSubCodeSystemBill))]
	public class TransactionalWithSubCodeSystemBillTest : TransactionalSystemBillTestCase<TransactionalWithSubCodeSystemBill>
	{
		public void TestInvoiceWithCommitmentDiscount()
		{
			BillingConstants.BillingSystemList.AddPairIfNotExist("DDD", "eServices System");

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "AAA");
			var user1 = new UsingParty(org1);
			var invoiceDelivery1 = org1.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			var org2 = BillingTestHelper.CreateOrganisation(Factory, "AAA", "MEL", "AAA");
			var user2 = new UsingParty(org2);
			var invoiceDelivery2 = org2.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery2.L9_OH_InvoiceTo = org1.PK;

			var priceHeader = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader, "A01", BillingConstants.FeeType.Transactional, "", 0.10m);
			BillingTestHelper.AddPriceItem(priceHeader, "A02", BillingConstants.FeeType.Transactional, "", 0.20m);

			var discount1 = org1.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = "DDD";
			discount1.L5_SubCode = "A01";
			discount1.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount1.L5_Type = BillingConstants.DiscountType.Commitment;
			discount1.L5_Units = 1000;
			discount1.L5_Discount = 20m;

			var discount2 = org1.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount2.L5_SystemCode = "DDD";
			discount2.L5_SubCode = "A02";
			discount2.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount2.L5_Type = BillingConstants.DiscountType.Commitment;
			discount2.L5_Units = 2000;
			discount2.L5_Discount = 15m;

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user1, new ZDateTime(2016, 5, 1)) { TransactionCount = 800 });
			usageList.Add(new EServicesSystemUsage("DDD", "A02", Factory, user2, new ZDateTime(2016, 5, 1)) { TransactionCount = 500 });

			var bill = new TransactionalWithSubCodeSystemBill("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertInvoiceLine(lines[0], "eServices System - 1000 Transactions at USD 0.10 per Transaction", 100m);
			AssertInvoiceLine(lines[1], "eServices System - 2000 Transactions at USD 0.20 per Transaction", 400m);
			AssertInvoiceLine(lines[2], "eServices System Discount", -80m);
		}

		public void TestInvoiceWithCommitmentDiscount_MultiplePrices()
		{
			BillingConstants.BillingSystemList.AddPairIfNotExist("DDD", "eServices System");

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "AAA", "SYD", "AAA");
			var user1 = new UsingParty(org1);
			var invoiceDelivery1 = org1.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery1.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			var org2 = BillingTestHelper.CreateOrganisation(Factory, "AAA", "MEL", "AAA");
			var user2 = new UsingParty(org2);
			var invoiceDelivery2 = org2.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery2.L9_OH_InvoiceTo = org1.PK;

			var priceHeader1 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader1.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader1.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader1.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader1, "A01", BillingConstants.FeeType.Transactional, "", 0.10m);
			BillingTestHelper.AddPriceItem(priceHeader1, "A02", BillingConstants.FeeType.Transactional, "", 0.20m);

			var priceHeader2 = org1.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.Other;
			priceHeader2.L6_ValidFrom = new ZDateTime(2016, 5, 1);
			priceHeader2.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader2, "A01", BillingConstants.FeeType.Transactional, "", 0.15m);
			BillingTestHelper.AddPriceItem(priceHeader2, "A02", BillingConstants.FeeType.Transactional, "", 0.25m);

			var discount1 = org1.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = "DDD";
			discount1.L5_SubCode = "A01";
			discount1.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount1.L5_Type = BillingConstants.DiscountType.Commitment;
			discount1.L5_Units = 1000;
			discount1.L5_Discount = 20m;

			var discount2 = org1.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount2.L5_SystemCode = "DDD";
			discount2.L5_SubCode = "A02";
			discount2.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount2.L5_Type = BillingConstants.DiscountType.Commitment;
			discount2.L5_Units = 2000;
			discount2.L5_Discount = 15m;

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user1, new ZDateTime(2016, 4, 1)) { TransactionCount = 800 });
			usageList.Add(new EServicesSystemUsage("DDD", "A02", Factory, user2, new ZDateTime(2016, 4, 1)) { TransactionCount = 500 });
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user1, new ZDateTime(2016, 5, 1)) { TransactionCount = 800 });
			usageList.Add(new EServicesSystemUsage("DDD", "A02", Factory, user2, new ZDateTime(2016, 5, 1)) { TransactionCount = 500 });

			var bill = new TransactionalWithSubCodeSystemBill("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			CombineAssertions(() =>
			{
				AssertInvoiceLine(lines[0], "eServices System - 1000 Transactions at USD 0.10 per Transaction", 100m);
				AssertInvoiceLine(lines[1], "eServices System - 1000 Transactions at USD 0.15 per Transaction", 150m);
				AssertInvoiceLine(lines[2], "eServices System - 2000 Transactions at USD 0.20 per Transaction", 400m);
				AssertInvoiceLine(lines[3], "eServices System - 2000 Transactions at USD 0.25 per Transaction", 500m);
				AssertInvoiceLine(lines[4], "eServices System Discount", -185m);
			});
		}

		void AssertInvoiceLine(SystemBill.BillLine line, string description, decimal amount)
		{
			AssertEquals("Description", description, line.Description);
			AssertEquals("Amount", amount, line.Amount);
		}

		public void TestCalculateMinimumFeeContribution()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "DDD", "BBB", "MEL");

			var user1 = new UsingParty(licence1);
			var user2 = new UsingParty(licence2);

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user1, new ZDateTime(2016, 5, 1)) { TransactionCount = 800 });
			usageList.Add(new EServicesSystemUsage("DDD", "A02", Factory, user2, new ZDateTime(2016, 5, 1)) { TransactionCount = 500 });
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user1, new ZDateTime(2016, 6, 1)) { TransactionCount = 800 });

			var bill = new TransactionalWithSubCodeSystemBill("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var minimumFeeContribution = bill.CalculateMinimumFeeContribution().ToArray();
			AssertEquals(3, minimumFeeContribution.Length);
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence2.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 6, 1)));
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
			BillingTestHelper.AddPriceItem(priceHeader, "A01", BillingConstants.FeeType.Transactional, "", 0.10m);
			BillingTestHelper.AddPriceItem(priceHeader, "A02", BillingConstants.FeeType.Transactional, "", 0.20m);

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user1, new ZDateTime(2016, 5, 1)) { TransactionCount = 46 });
			usageList.Add(new EServicesSystemUsage("DDD", "A01", Factory, user2, new ZDateTime(2016, 5, 1)) { TransactionCount = 30 });
			usageList.Add(new EServicesSystemUsage("DDD", "A02", Factory, user2, new ZDateTime(2016, 5, 1)) { TransactionCount = 16 });

			var bill = new TransactionalWithSubCodeSystemBill("DDD", Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(3, lines.Count);
			AssertEquals("eServices System - 46 Transactions at USD 0.10 per Transaction", lines[0].Description);
			AssertEquals(4.60m, lines[0].Amount);
			AssertEquals("eServices System - 30 Transactions at USD 0.10 per Transaction", lines[1].Description);
			AssertEquals(3.00m, lines[1].Amount);
			AssertEquals("eServices System - 16 Transactions at USD 0.20 per Transaction", lines[2].Description);
			AssertEquals(3.20m, lines[2].Amount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewSystemBill();
		}

		protected override TransactionalWithSubCodeSystemBill GetNewSystemBill()
		{
			return new TransactionalWithSubCodeSystemBill("DDD", Factory);
		}
	}
}
