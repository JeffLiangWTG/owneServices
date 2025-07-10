using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PremiumServiceBill))]
	internal class PremiumServiceBillTest : SystemBillTestCase<PremiumServiceBill>
	{
		public void TestValidateUnitPrice()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var ldsPriceList1 = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList1.L6_PricelistVersion = "V1";
			ldsPriceList1.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList1.L6_RX_NKCurrency = "USD";
			ldsPriceList1.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var priceItem1 = BillingTestHelper.AddPriceItem(ldsPriceList1, "SCC", BillingConstants.FeeType.PerDevicePerMonth, "", 100m);
			priceItem1.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";

			var ldsPriceList2 = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList2.L6_PricelistVersion = "V2";
			ldsPriceList2.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList2.L6_RX_NKCurrency = "USD";
			ldsPriceList2.L6_ValidFrom = new ZDateTime(2019, 1, 1);

			var priceItem2 = BillingTestHelper.AddPriceItem(ldsPriceList2, "SCC", BillingConstants.FeeType.PerDevicePerMonth, "", 110m);
			priceItem2.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			var rate2 = priceItem2.CurrencyRates.AddNew();
			rate2.PIR_RX_NKCurrency = "AUD";
			rate2.PIR_Price = 145m;

			var org = BillingTestHelper.CreateOrganisation(Factory, "FOT");
			BillingTestHelper.SetInvoiceCurrency(org, "AUD");

			var mainPriceHeader = org.LicCompany.PriceHeaders.AddNew();
			mainPriceHeader.L6_RX_NKCurrency = "AUD";
			mainPriceHeader.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var service = Factory.New<ClientPremiumService>();
			service.CPS_Type = "SCC";
			service.CPS_Units = 10;
			service.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;
			service.CPS_StartDate = new ZDateTime(2018, 1, 1);

			var usage1 = new PremiumServiceUsage(Factory, new UsingParty(org), new ZDateTime(2018, 2, 1), new ClientPremiumService[] { service });
			var usage2 = new PremiumServiceUsage(Factory, new UsingParty(org), new ZDateTime(2019, 2, 1), new ClientPremiumService[] { service });
			var bill1 = new PremiumServiceBill(Factory);
			var bill2 = new PremiumServiceBill(Factory);
			bill1.PopulateFromSystemUsages(new[] { usage1 });
			bill2.PopulateFromSystemUsages(new[] { usage2 });
			bill1.ValidateAll(bill1);
			bill2.ValidateAll(bill2);
			AssertHasRowErrorContaining(bill1, "No global price defined for FOTSYD, price code SCC in currency AUD");
			AssertNoNotifications(bill2);
		}

		public void TestCreateInvoiceLines()
		{
			var charge1 = BillingTestHelper.CreateChargeCode(Factory, null, "PREM1");
			var charge2 = BillingTestHelper.CreateChargeCode(Factory, null, "PREM2");
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var org1 = lic1.Company.Header;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);
			var pricesHeader1 = BillingTestHelper.CreatePriceList(lic1.Company);
			var price1 = BillingTestHelper.AddPriceItem(pricesHeader1, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);
			price1.L7_ChargeCode = "PREM1";
			price1.L7_Description = "Premium Hosting Web Connection";
			var price2 = BillingTestHelper.AddPriceItem(pricesHeader1, "CCC", BillingConstants.FeeType.PerDevicePerMonth, "", 20m);
			price2.L7_ChargeCode = "PREM2";
			price2.L7_Description = "LDaaS rental";

			var db1 = lic1.Database;
			var service1 = BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 1, 1), ZDateTime.Empty);
			service1.CPS_Units = 7;
			var service2 = BillingTestHelper.CreatePremiumService(db1, "CCC", new ZDateTime(2013, 1, 1), ZDateTime.Empty);
			service2.CPS_Units = 11;
			Factory.Save();

			var bill = new PremiumServiceBill(Factory);
			PremiumServiceUsage usage1 = new PremiumServiceUsage(Factory, new UsingParty(lic1), new ZDateTime(2013, 1, 1), new ClientPremiumService[] { service1 });
			PremiumServiceUsage usage2 = new PremiumServiceUsage(Factory, new UsingParty(lic1), new ZDateTime(2013, 1, 1), new ClientPremiumService[] { service2 });

			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1, usage2 });

			AssertEquals("PREM1", bill.GetAmountChargeCodeName(usage1));
			AssertEquals("PREM2", bill.GetAmountChargeCodeName(usage2));
			AssertEquals("", bill.GetDiscountChargeCodeName(usage1));
			AssertEquals("", bill.GetDiscountChargeCodeName(usage2));

			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now, invoice);
			AssertEquals(2, lines.Count);
			var line1 = lines.Single(x => x.ChargeCodeName == "PREM1");
			var line2 = lines.Single(x => x.ChargeCodeName == "PREM2");
			AssertEquals(7 * 10m, line1.Amount);
			AssertEquals(11 * 20m, line2.Amount);
			AssertEquals("Premium Hosting Web Connection", line1.Description);
			AssertEquals("LDaaS rental", line2.Description);
		}

		public void TestCreateInvoiceLines_LDaaS()
		{
			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var freeTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0);

			var charge1 = BillingTestHelper.CreateChargeCode(Factory, null, "PREM1");
			var charge2 = BillingTestHelper.CreateChargeCode(Factory, null, "PREM2");
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");

			var org = licence.Company.Header;

			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoiceCurrency(org, "AUD");
			licence.Company.InvoiceDeliveries[0].L9_AT_TaxId = freeTaxRate.PK;

			var ldsInvoicingDelivery = licence.Company.InvoiceDeliveries.AddNew();
			ldsInvoicingDelivery.L9_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsInvoicingDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			ldsInvoicingDelivery.L9_RX_NKInvoiceCurrency = "AUD";
			ldsInvoicingDelivery.L9_AT_TaxId = gstTaxRate.PK;

			var mainPriceList = BillingTestHelper.CreatePriceList(licence.Company);
			var priceItem1 = BillingTestHelper.AddPriceItem(mainPriceList, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);
			priceItem1.L7_ChargeCode = "PREM1";
			priceItem1.L7_Description = "Premium Hosting Web Connection";

			LicenceCompany.ClearStandardPricesCompanyCache();

			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var ldsPriceList = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList.L6_RX_NKCurrency = "AUD";
			ldsPriceList.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var priceItem2 = BillingTestHelper.AddPriceItem(ldsPriceList, "SCC", BillingConstants.FeeType.PerDevicePerMonth, "", 15m);
			priceItem2.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			priceItem2.L7_ChargeCode = "PREM2";

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var db = licence.Database;
			var service1 = BillingTestHelper.CreatePremiumService(db, "BBB", new ZDateTime(2016, 10, 1), ZDateTime.Empty);
			service1.CPS_Units = 10;
			var service2 = BillingTestHelper.CreatePremiumService(db, "SCC", new ZDateTime(2016, 10, 1), ZDateTime.Empty);
			service2.CPS_Units = 15;
			service2.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;

			Factory.Save();

			var bill = new PremiumServiceBill(Factory);
			var usage1 = new PremiumServiceUsage(Factory, new UsingParty(licence), new ZDateTime(2016, 10, 1), new ClientPremiumService[] { service1 });
			var usage2 = new PremiumServiceUsage(Factory, new UsingParty(licence), new ZDateTime(2016, 10, 1), new ClientPremiumService[] { service2 });

			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1, usage2 });
			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now, invoice);
			AssertEquals(2, lines.Count);
			var line1 = lines.Single(x => x.ChargeCodeName == "PREM1");
			var line2 = lines.Single(x => x.ChargeCodeName == "PREM2");
			AssertEquals(10 * 10m, line1.Amount);
			AssertEquals(15 * 15m, line2.Amount);
			AssertEquals("FREEGST", line1.Tax.Code);
			AssertEquals("GST", line2.Tax.Code);
		}

		public void TestOnInvoiceFactorySaving_SetChargeableUsagesInvoiceCore()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var org1 = lic1.Company.Header;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);
			var prices1 = BillingTestHelper.CreatePriceList(lic1.Company);
			var price = BillingTestHelper.AddPriceItem(prices1, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);
			price.L7_ChargeCode = "PREM1";
			var db1 = lic1.Database;
			var service1 = BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 1, 1), ZDateTime.Empty);
			service1.CPS_Units = 7;
			Factory.Save();

			ZQuery chargeableUsageQuery = new ZQuery(ClientChargeableUsageSchema.U1_Parent, service1.PK);
			ClientChargeableUsage[] chargeableUsages = Factory.Load<ClientChargeableUsage>(chargeableUsageQuery);
			AssertEquals("Precondition: no chargeable usages", 0, chargeableUsages.Length);

			var bill = new PremiumServiceBill(Factory);
			PremiumServiceUsage usage = new PremiumServiceUsage(Factory, new UsingParty(lic1), new ZDateTime(2013, 1, 1), new ClientPremiumService[] { service1 });

			bill.PopulateFromSystemUsages(new SystemUsage[] { usage });

			AssertEquals("PREM1", bill.GetAmountChargeCodeName(usage));
			AssertEquals("", bill.GetDiscountChargeCodeName(usage));

			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();
			bill.OnInvoiceFactorySaving(invoice);
			invoiceFactory.Save();

			chargeableUsages = Factory.Load<ClientChargeableUsage>(chargeableUsageQuery);
			AssertEquals("Chargeable usages created", 1, chargeableUsages.Length);
			ClientChargeableUsage chargeableUsage = chargeableUsages[0];
			AssertEquals("SVC", chargeableUsage.U1_Code);
			AssertEquals("BBB", chargeableUsage.U1_SubCode);
			AssertEquals(usage.PeriodStart, chargeableUsage.U1_PeriodStart);
			AssertEquals(10m, chargeableUsage.U1_UnitPrice);
			AssertEquals(7m, chargeableUsage.U1_UnitCount);
			AssertEquals(invoice.PK, chargeableUsage.U1_AH_Invoice);
			AssertEquals(lic1.LA_LC, chargeableUsage.U1_LC);
			AssertEquals(service1.PK, chargeableUsage.U1_Parent);
		}

		public void TestCalculateMinimumFeeContribution()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "DDD", "BBB", "MEL");

			var user1 = new UsingParty(licence1);
			var user2 = new UsingParty(licence2);

			var prices1 = BillingTestHelper.CreatePriceList(licence1.Company);
			var price1 = BillingTestHelper.AddPriceItem(prices1, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);

			var prices2 = BillingTestHelper.CreatePriceList(licence2.Company);
			var price2 = BillingTestHelper.AddPriceItem(prices2, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);

			var service1 = BillingTestHelper.CreatePremiumService(licence1.Database, "BBB", new ZDateTime(2016, 1, 1), ZDateTime.Empty);
			service1.CPS_Units = 10;
			var service2 = BillingTestHelper.CreatePremiumService(licence2.Database, "BBB", new ZDateTime(2016, 1, 1), ZDateTime.Empty);
			service2.CPS_Units = 20;

			var usage1 = new PremiumServiceUsage(Factory, user1, new ZDateTime(2016, 5, 1), new ClientPremiumService[] { service1 });
			var usage2 = new PremiumServiceUsage(Factory, user2, new ZDateTime(2016, 5, 1), new ClientPremiumService[] { service2 });
			var usage3 = new PremiumServiceUsage(Factory, user1, new ZDateTime(2016, 6, 1), new ClientPremiumService[] { service1 });

			var bill = new PremiumServiceBill(Factory);
			bill.PopulateFromSystemUsages(new PremiumServiceUsage[] { usage1, usage2, usage3 });

			var minimumFeeContribution = bill.CalculateMinimumFeeContribution().ToArray();
			AssertEquals(3, minimumFeeContribution.Length);
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence2.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 6, 1)));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewSystemBill();
		}

		protected override PremiumServiceBill GetNewSystemBill()
		{
			return new PremiumServiceBill(Factory);
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
