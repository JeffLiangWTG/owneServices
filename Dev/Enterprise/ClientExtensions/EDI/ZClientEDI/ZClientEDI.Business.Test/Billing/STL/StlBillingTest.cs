using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Client.EDI.Billing.Fee;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlBilling))]
	public sealed class StlBillingTest : UsageBillingTest
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_AmountWithDiscountRounding()
		{
			Factory.RefreshEnabled = false;
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "#HG");
			stlPrices.L6_DiscountCode = "STL1";
			stlPrices.L6_UseStandardDiscount = false;

			var discount = stlPrices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Special";
			discount.PHD_Percent = 5;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = stlPrices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount.PK;

			var tinyPrice = stlPrices.Items.FindByCode("#HG");
			tinyPrice.L7_Price = 0.01m;
			tinyPrice.L7_LicenceUnits = 0.1m;
			tinyPrice.L7_PGM_DiscountGroupCode = "Standard";

			var licCW1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			stlPrices.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices, periodStart);

			var usageUsr = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);
			var usageTinyPrice = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "#HG", periodStart, licCW1.ClientCompany, 101243);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill1", bill1);
				AssertEquals("bill1.InvoicePreDiscountTotal", 50 * 1.00m + 101243 * 0.01m, bill1.InvoicePreDiscountTotal);
				AssertEquals("bill1.InvoicePostDiscountTotal", 50 * 1.00m + Utilities.Round(101243 * 0.01m * 0.95m, 2), bill1.InvoicePostDiscountTotal);
			});
		}

		public void TestGenerateReport()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic.Company.PriceHeaders.AddNew();
			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			var rate1 = item1.CurrencyRates.AddNew();
			rate1.PIR_RX_NKCurrency = "AUD";
			rate1.PIR_Price = 17.5;

			var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(0, billing.Bills.Count);
			AssertEquals(0, billing.ValidationResults.Count);
		}

		public void TestGenerateReport_PremiumService()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();

			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var ldsPriceList1 = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList1.L6_PricelistVersion = "V1";
			ldsPriceList1.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList1.L6_RX_NKCurrency = "USD";
			ldsPriceList1.L6_ValidFrom = new ZDateTime(2015, 1, 1);

			var ldsPriceItem1 = BillingTestHelper.AddPriceItem(ldsPriceList1, "LS3", BillingConstants.FeeType.PerDevicePerMonth, "", 100m);
			ldsPriceItem1.L7_Order = 1;
			ldsPriceItem1.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			var ldsRate1 = ldsPriceItem1.CurrencyRates.AddNew();
			ldsRate1.PIR_RX_NKCurrency = "AUD";
			ldsRate1.PIR_Price = 125m;

			var goldenTaxPriceList = stdLicCompany.PriceHeaders.AddNew();
			goldenTaxPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.GoldenTax;
			goldenTaxPriceList.L6_RX_NKCurrency = "AUD";
			goldenTaxPriceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			var item2 = goldenTaxPriceList.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.Service;
			item2.L7_Code = "GTD";
			item2.L7_Price = 88m;
			item2.L7_Order = 2;

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic.Company.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_RX_NKCurrency = "AUD";
			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.Service;
			item1.L7_Code = "#HU";
			item1.L7_Price = 100;
			item1.L7_Order = 0;

			var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			var service1 = lic.Database.PremiumServices.AddNew();
			service1.CPS_Type = "#HU";
			service1.CPS_Units = 5;
			service1.CPS_StartDate = new ZDateTime(2015, 6, 1);
			service1.CPS_DisplayOrder = 0;

			var service2 = lic.Database.PremiumServices.AddNew();
			service2.CPS_Type = "LS3";
			service2.CPS_Units = 10;
			service2.CPS_StartDate = new ZDateTime(2015, 6, 1);
			service2.CPS_DisplayOrder = 1;
			service2.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;

			var service3 = lic.Database.PremiumServices.AddNew();
			service3.CPS_Type = "GTD";
			service3.CPS_Units = 7;
			service3.CPS_StartDate = new ZDateTime(2015, 6, 1);
			service3.CPS_DisplayOrder = 2;
			service3.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.GoldenTax;

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			var monthlyUsage = bill.MonthlyUsages.First();

			var usageLines = monthlyUsage.UsageLines.ToArray();
			var line1 = usageLines[0];
			AssertEquals("#HU", line1.PriceItemCode);
			AssertEquals(5m, line1.UnitCount);

			var line2 = usageLines[1];
			AssertEquals("LS3", line2.PriceItemCode);
			AssertEquals(10m, line2.UnitCount);
			AssertEquals(125m, line2.Price);

			var line3 = usageLines[2];
			AssertEquals("GTD", line3.PriceItemCode);
			AssertEquals(7m, line3.UnitCount);
			AssertEquals(88m, line3.Price);

			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		public void TestGenerateReport_InvoiceDeliveryValidation()
		{
			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var freeTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0);
			BillingTestHelper.CreateChargeCode(Factory, null, "STLUSAGE");

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK);
			licence.Company.InvoiceDeliveries[0].L9_RX_NKInvoiceCurrency = "AUD";
			licence.Company.InvoiceDeliveries[0].L9_AT_TaxId = freeTaxRate.PK;

			var ldsInvoicingDelivery = licence.Company.InvoiceDeliveries.AddNew();
			ldsInvoicingDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			ldsInvoicingDelivery.L9_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsInvoicingDelivery.L9_RX_NKInvoiceCurrency = "AUD";
			ldsInvoicingDelivery.L9_AT_TaxId = gstTaxRate.PK;

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_RX_NKCurrency = "AUD";
			var priceItem = prices.Items.AddNew();
			priceItem.L7_Category = BillingConstants.BillingSystem.STL;
			priceItem.L7_Code = "USR";
			priceItem.L7_Price = 100;
			priceItem.L7_ChargeCode = "STLUSAGE";

			var priceHeaderLink = licence.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			LicenceCompany.ClearStandardPricesCompanyCache();
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var ldsPriceList = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList.L6_PricelistVersion = "V1";
			ldsPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList.L6_RX_NKCurrency = "USD";
			ldsPriceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);

			var ldsPriceItem = BillingTestHelper.AddPriceItem(ldsPriceList, "LS3", BillingConstants.FeeType.PerDevicePerMonth, "", 100m);
			ldsPriceItem.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			ldsPriceItem.L7_ChargeCode = "STLUSAGE";
			var ldsRate1 = ldsPriceItem.CurrencyRates.AddNew();
			ldsRate1.PIR_RX_NKCurrency = "AUD";
			ldsRate1.PIR_Price = 125m;

			var service = licence.Database.PremiumServices.AddNew();
			service.CPS_Type = "LS3";
			service.CPS_Units = 10;
			service.CPS_StartDate = new ZDateTime(2015, 6, 1);
			service.CPS_DisplayOrder = 1;
			service.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), licence.ClientCompany, 10);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			var monthlyUsage = billing.Bills[0].MonthlyUsages.First();
			AssertEquals("Should allow different tax settings on STL and LDaaS", false, monthlyUsage.HasRowErrors);

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = "TTT";
			newBranch.GB_GC = Env.CurrentCompanyPK;
			ldsInvoicingDelivery.L9_GB_InvoicingBranch = newBranch.PK;
			billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			monthlyUsage = billing.Bills[0].MonthlyUsages.First();
			AssertEquals("Should not allow multiple invoicing branches on same database", true, monthlyUsage.HasRowErrors);

			ldsInvoicingDelivery.L9_GB_InvoicingBranch = Env.CurrentBranchPK;
			var invoiceOrg = Factory.NewWithValidTestData<OrgHeader>();
			ldsInvoicingDelivery.L9_OH_InvoiceTo = invoiceOrg.PK;
			billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			monthlyUsage = billing.Bills[0].MonthlyUsages.First();
			AssertEquals("Should not allow multiple invoice to orgs on same database", true, monthlyUsage.HasRowErrors);

			ldsInvoicingDelivery.L9_OH_InvoiceTo = ZGuid.Empty;
			ldsInvoicingDelivery.L9_RX_NKInvoiceCurrency = "HKD";
			billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			monthlyUsage = billing.Bills[0].MonthlyUsages.First();
			AssertEquals("Should not allow multiple invoicing currencies on same database", true, monthlyUsage.HasRowErrors);

			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		public void TestGenerateReport_InvoiceDeliveryGroupByDatabase()
		{
			BillingTestHelper.CreateChargeCode(Factory, null, "STLUSAGE");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "DB1");
			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB2");
			var delivery = BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			delivery.L9_GroupBy = ClientInvoiceDelivery.GroupBy.Db;
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			LicenceCompany.ClearStandardPricesCompanyCache();
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var prices = stdHeader.Company.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_RX_NKCurrency = "AUD";
			var priceItem = prices.Items.AddNew();
			priceItem.L7_Category = BillingConstants.BillingSystem.STL;
			priceItem.L7_Code = "USR";
			priceItem.L7_Price = 100;
			priceItem.L7_ChargeCode = "STLUSAGE";

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = prices.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			var priceHeaderLink2 = lic2.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink2.PHL_L6 = prices.PK;
			priceHeaderLink2.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink2.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 20);

			Factory.Save();

			var reportFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var billing = new StlBilling(reportFactory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(2, billing.Bills.Count);
			AssertNoErrors(billing.Bills[0]);
			AssertNoErrors(billing.Bills[1]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_Commitment()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;

			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Commitment";
			discount.PHD_Percent = 20;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = prices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 40;
			item1.L7_LicenceUnits = 400;
			item1.L7_PGM_DiscountGroupCode = "Standard";
			item1.L7_Order = 1;

			var item2 = prices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.STL;
			item2.L7_Code = "SHP";
			item2.L7_Price = 3;
			item2.L7_LicenceUnits = 30;
			item2.L7_PGM_DiscountGroupCode = "Standard";
			item2.L7_Order = 2;

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item2.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item2.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			var commitment = Factory.New<CommitmentLicenceSetting>();
			lic.Database.LicenceSettings.Add(commitment);
			commitment.LicenceUnits = (30 * 500 + 10 * 400) * 1.50; // 15000 + 4000 = 19000, 19000 * 1.50 = 28500

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", new ZDateTime(2015, 7, 1), lic.ClientCompany, 500);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			var monthlyUsage = bill.MonthlyUsages.First();
			var line1 = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "USR");
			var line2 = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "SHP");

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var billedUsages = invoice.Factory.Load<EdiBilledUsage>(new ZQuery());
			var usrRevenue = billedUsages.Single(x => x.BU9_PriceCode == "USR");
			var shpRevenue = billedUsages.Single(x => x.BU9_PriceCode == "SHP");

			// Discount factor = 0.80 (20%)
			// Commitment scaling = 1.50
			// Tax = 1.1 (10%)
			decimal preDiscountScaling = 1.50m;
			decimal postDiscountScaling = preDiscountScaling * 0.80m;

			CombineAssertions(() =>
			{
				AssertEquals("AH_LocalTotal", (10 * 40 + 500 * 3) * postDiscountScaling * 1.1m, invoice.AH_LocalTotal);

				AssertEquals("USR UnitCount", 10m, line1.UnitCount);
				AssertEquals("SHP UnitCount", 500m, line2.UnitCount);
				AssertEquals("commitment PK", commitment.PK, monthlyUsage.Commitment.PK);
				AssertEquals("TotalLicenceUnits", commitment.LicenceUnits, monthlyUsage.TotalLicenceUnits);

				AssertEquals("USR BU9_LocalAmountPostDiscount", 10 * 40 * postDiscountScaling, usrRevenue.BU9_LocalAmountPostDiscount);
				AssertEquals("USR BU9_LocalAmountPreDiscount", 10 * 40 * preDiscountScaling, usrRevenue.BU9_LocalAmountPreDiscount);

				AssertEquals("SHP BU9_LocalAmountPostDiscount", 3 * 500 * postDiscountScaling, shpRevenue.BU9_LocalAmountPostDiscount);
				AssertEquals("SHP BU9_LocalAmountPreDiscount", 3 * 500 * preDiscountScaling, shpRevenue.BU9_LocalAmountPreDiscount);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 4, 9)]
		public void TestGenerateReport_CommitmentShared_TwoMembers()
		{
			var periodStart = new ZDateTime(TestDateAttribute.Date.Year, TestDateAttribute.Date.Month, 1);
			BillingTestHelper.LoadClientSpecificDocuments();
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;
			var prices = BillingTestHelper.CreateStlPriceList(stdPriceCompany);

			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Commitment";
			discount.PHD_Percent = 20;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = prices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 40;
			item1.L7_LicenceUnits = 400;
			item1.L7_PGM_DiscountGroupCode = "Standard";
			item1.L7_Order = 1;

			var item2 = prices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.STL;
			item2.L7_Code = "SHP";
			item2.L7_Price = 3;
			item2.L7_LicenceUnits = 30;
			item2.L7_PGM_DiscountGroupCode = "Standard";
			item2.L7_Order = 2;

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item2.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item2.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreatePriceLink(lic1.Database, prices, periodStart, "AUD");
			// Make the commitment 50% more than the usage
			var commitmentUnits = (500 * 30m + 10 * 400m) * 1.50m; // 15000 + 4000 = 19000, 19000 * 1.50 = 28500
			var commitment1 = BillingTestHelper.CreateCommitment(lic1.Database, periodStart, periodStart.AddMonths(1).AddDays(-1), commitmentUnits, "SHARED");
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic1.ClientCompany, 350);

			var lic2 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO2", "DB2");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreatePriceLink(lic2.Database, prices, periodStart, "AUD");
			var commitment2 = BillingTestHelper.CreateCommitment(lic2.Database, periodStart, periodStart.AddMonths(1).AddDays(-1), commitmentUnits, "SHARED");
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic2.ClientCompany, 150);
			BillingTestHelper.SetInvoicingTo(lic2, lic1);

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			var monthlyUsage1 = bill.MonthlyUsages.Single(x => x.Database.LD_ServerCode == "DB1");
			var monthlyUsage2 = bill.MonthlyUsages.Single(x => x.Database.LD_ServerCode == "DB2");
			AssertEquals(7 * 400m + 350 * 30m, monthlyUsage1.TotalUsedLicenceUnits);
			AssertEquals(3 * 400m + 150 * 30m, monthlyUsage2.TotalUsedLicenceUnits);
			AssertEquals(monthlyUsage1.TotalUsedLicenceUnits + monthlyUsage2.TotalUsedLicenceUnits, monthlyUsage1.CommitmentGroup.TotalUsedLicenceUnits);
			AssertEquals(monthlyUsage1.TotalUsedLicenceUnits + monthlyUsage2.TotalUsedLicenceUnits, monthlyUsage2.CommitmentGroup.TotalUsedLicenceUnits);
			var totalAdjustment = commitmentUnits - monthlyUsage1.CommitmentGroup.TotalUsedLicenceUnits;
			AssertEquals(totalAdjustment / 2, monthlyUsage1.CommitmentGroup.Adjustment);
			AssertEquals(totalAdjustment / 2, monthlyUsage2.CommitmentGroup.Adjustment);

			AssertEquals("TotalLicenceUnits includes a share of the total adjustment", monthlyUsage1.TotalUsedLicenceUnits + monthlyUsage1.CommitmentGroup.Adjustment, monthlyUsage1.TotalLicenceUnits);
			AssertEquals("TotalLicenceUnits includes a share of the total adjustment", monthlyUsage2.TotalUsedLicenceUnits + monthlyUsage2.CommitmentGroup.Adjustment, monthlyUsage2.TotalLicenceUnits);

			var usrLine1 = monthlyUsage1.UsageLines.Single(x => x.PriceItemCode == "USR");
			var shpLine1 = monthlyUsage1.UsageLines.Single(x => x.PriceItemCode == "SHP");
			var usrLine2 = monthlyUsage2.UsageLines.Single(x => x.PriceItemCode == "USR");
			var shpLine2 = monthlyUsage2.UsageLines.Single(x => x.PriceItemCode == "SHP");

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var billedUsages = invoice.Factory.Load<EdiBilledUsage>(new ZQuery());
			var usrRevenue1 = billedUsages.Single(x => x.BU9_PriceCode == "USR" && x.BU9_LD == lic1.LA_LD);
			var shpRevenue1 = billedUsages.Single(x => x.BU9_PriceCode == "SHP" && x.BU9_LD == lic1.LA_LD);
			var usrRevenue2 = billedUsages.Single(x => x.BU9_PriceCode == "USR" && x.BU9_LD == lic2.LA_LD);
			var shpRevenue2 = billedUsages.Single(x => x.BU9_PriceCode == "SHP" && x.BU9_LD == lic2.LA_LD);

			// Discount factor = 0.80 (20%)
			// Commitment scaling = 1.50
			decimal preDiscountScaling = 1.50m;
			decimal postDiscountScaling = preDiscountScaling * 0.80m;

			// Tax = 1.1 (10%)
			AssertEquals("AH_LocalTotal", (10 * 40 + 500 * 3) * postDiscountScaling * 1.1m, invoice.AH_LocalTotal);

			decimal commitmentAdjustmentUnits = commitmentUnits - monthlyUsage1.CommitmentGroup.TotalUsedLicenceUnits;

			// DB1
			CombineAssertions(() =>
			{
				AssertEquals("USR UnitCount", 7m, usrLine1.UnitCount);
				AssertEquals("SHP UnitCount", 350m, shpLine1.UnitCount);
				AssertEquals("commitment PK", commitment1.PK, monthlyUsage1.Commitment.PK);
				AssertEquals("Commitment Adjustment", commitmentAdjustmentUnits / 2 * 0.1m, monthlyUsage1.CommitmentLines.Single().Price);
				AssertEquals("TotalLicenceUnits", 7 * 400m + 350 * 30m + commitmentAdjustmentUnits / 2, monthlyUsage1.TotalLicenceUnits);

				AssertEquals("USR BU9_LocalAmountPostDiscount", 7 * 40 * postDiscountScaling, usrRevenue1.BU9_LocalAmountPostDiscount);
				AssertEquals("USR BU9_LocalAmountPreDiscount", 7 * 40 * preDiscountScaling, usrRevenue1.BU9_LocalAmountPreDiscount);

				AssertEquals("SHP BU9_LocalAmountPostDiscount", 350 * 3 * postDiscountScaling, shpRevenue1.BU9_LocalAmountPostDiscount);
				AssertEquals("SHP BU9_LocalAmountPreDiscount", 350 * 3 * preDiscountScaling, shpRevenue1.BU9_LocalAmountPreDiscount);
			});

			// DB2
			CombineAssertions(() =>
			{
				AssertEquals("USR UnitCount", 3m, usrLine2.UnitCount);
				AssertEquals("SHP UnitCount", 150m, shpLine2.UnitCount);
				AssertEquals("commitment PK", commitment2.PK, monthlyUsage2.Commitment.PK);
				AssertEquals("Commitment Adjustment", commitmentAdjustmentUnits / 2 * 0.1m, monthlyUsage2.CommitmentLines.Single().Price);
				AssertEquals("TotalLicenceUnits", 3 * 400m + 150 * 30m + commitmentAdjustmentUnits / 2, monthlyUsage2.TotalLicenceUnits);

				AssertEquals("USR BU9_LocalAmountPostDiscount", 3 * 40 * postDiscountScaling, usrRevenue2.BU9_LocalAmountPostDiscount);
				AssertEquals("USR BU9_LocalAmountPreDiscount", 3 * 40 * preDiscountScaling, usrRevenue2.BU9_LocalAmountPreDiscount);

				AssertEquals("SHP BU9_LocalAmountPostDiscount", 150 * 3 * postDiscountScaling, shpRevenue2.BU9_LocalAmountPostDiscount);
				AssertEquals("SHP BU9_LocalAmountPreDiscount", 150 * 3 * preDiscountScaling, shpRevenue2.BU9_LocalAmountPreDiscount);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 4, 9)]
		public void TestGenerateReport_CommitmentShared_ThreeMembers()
		{
			var periodStart = new ZDateTime(TestDateAttribute.Date.Year, TestDateAttribute.Date.Month, 1);
			BillingTestHelper.LoadClientSpecificDocuments();
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;
			var prices = BillingTestHelper.CreateStlPriceList(stdPriceCompany);

			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Commitment";
			discount.PHD_Percent = 20;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = prices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 40;
			item1.L7_LicenceUnits = 400;
			item1.L7_PGM_DiscountGroupCode = "Standard";
			item1.L7_Order = 1;

			var item2 = prices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.STL;
			item2.L7_Code = "SHP";
			item2.L7_Price = 3;
			item2.L7_LicenceUnits = 30;
			item2.L7_PGM_DiscountGroupCode = "Standard";
			item2.L7_Order = 2;

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item2.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item2.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreatePriceLink(lic1.Database, prices, periodStart, "AUD");
			// Make the commitment 50% more than the usage
			var commitmentUnits = (500 * 30m + 10 * 400m) * 1.50m; // 15000 + 4000 = 19000, 19000 * 1.50 = 28500
			var commitment1 = BillingTestHelper.CreateCommitment(lic1.Database, periodStart, periodStart.AddMonths(1).AddDays(-1), commitmentUnits, "SHARED");
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic1.ClientCompany, 250);

			var lic2 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO2", "DB2");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreatePriceLink(lic2.Database, prices, periodStart, "AUD");
			var commitment2 = BillingTestHelper.CreateCommitment(lic2.Database, periodStart, periodStart.AddMonths(1).AddDays(-1), commitmentUnits, "SHARED");
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic2.ClientCompany, 150);
			BillingTestHelper.SetInvoicingTo(lic2, lic1);

			var lic3 = BillingTestHelper.CreateLicence(Factory, "ENT", "CO3", "DB3");
			BillingTestHelper.SetInvoicing(lic3, Env.CurrentBranch.PK, "AUD");
			lic3.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreatePriceLink(lic3.Database, prices, periodStart, "AUD");
			var commitment3 = BillingTestHelper.CreateCommitment(lic3.Database, periodStart, periodStart.AddMonths(1).AddDays(-1), commitmentUnits, "SHARED");
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic3.ClientCompany, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic3.ClientCompany, 100);
			BillingTestHelper.SetInvoicingTo(lic3, lic1);

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			var monthlyUsage1 = bill.MonthlyUsages.Single(x => x.Database.LD_ServerCode == "DB1");
			var monthlyUsage2 = bill.MonthlyUsages.Single(x => x.Database.LD_ServerCode == "DB2");
			var monthlyUsage3 = bill.MonthlyUsages.Single(x => x.Database.LD_ServerCode == "DB3");
			AssertEquals(5 * 400m + 250 * 30m, monthlyUsage1.TotalUsedLicenceUnits);
			AssertEquals(3 * 400m + 150 * 30m, monthlyUsage2.TotalUsedLicenceUnits);
			AssertEquals(2 * 400m + 100 * 30m, monthlyUsage3.TotalUsedLicenceUnits);
			AssertEquals(monthlyUsage1.TotalUsedLicenceUnits + monthlyUsage2.TotalUsedLicenceUnits + monthlyUsage3.TotalUsedLicenceUnits, monthlyUsage1.CommitmentGroup.TotalUsedLicenceUnits);
			AssertEquals(monthlyUsage1.TotalUsedLicenceUnits + monthlyUsage2.TotalUsedLicenceUnits + monthlyUsage3.TotalUsedLicenceUnits, monthlyUsage2.CommitmentGroup.TotalUsedLicenceUnits);
			AssertEquals(monthlyUsage1.TotalUsedLicenceUnits + monthlyUsage2.TotalUsedLicenceUnits + monthlyUsage3.TotalUsedLicenceUnits, monthlyUsage3.CommitmentGroup.TotalUsedLicenceUnits);
			var totalAdjustment = commitmentUnits - monthlyUsage1.CommitmentGroup.TotalUsedLicenceUnits;
			AssertEquals(totalAdjustment, monthlyUsage1.CommitmentGroup.Adjustment + monthlyUsage2.CommitmentGroup.Adjustment + monthlyUsage3.CommitmentGroup.Adjustment);
			AssertEquals(3166.7m, monthlyUsage1.CommitmentGroup.Adjustment);
			AssertEquals(3166.7m, monthlyUsage2.CommitmentGroup.Adjustment);
			AssertEquals(3166.6m, monthlyUsage3.CommitmentGroup.Adjustment);

			AssertEquals("TotalLicenceUnits includes a share of the total adjustment", monthlyUsage2.TotalUsedLicenceUnits + monthlyUsage2.CommitmentGroup.Adjustment, monthlyUsage2.TotalLicenceUnits);
			AssertEquals("TotalLicenceUnits includes a share of the total adjustment", monthlyUsage3.TotalUsedLicenceUnits + monthlyUsage3.CommitmentGroup.Adjustment, monthlyUsage3.TotalLicenceUnits);

			var usrLine1 = monthlyUsage1.UsageLines.Single(x => x.PriceItemCode == "USR");
			var shpLine1 = monthlyUsage1.UsageLines.Single(x => x.PriceItemCode == "SHP");
			var usrLine2 = monthlyUsage2.UsageLines.Single(x => x.PriceItemCode == "USR");
			var shpLine2 = monthlyUsage2.UsageLines.Single(x => x.PriceItemCode == "SHP");
			var usrLine3 = monthlyUsage3.UsageLines.Single(x => x.PriceItemCode == "USR");
			var shpLine3 = monthlyUsage3.UsageLines.Single(x => x.PriceItemCode == "SHP");

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var billedUsages = invoice.Factory.Load<EdiBilledUsage>(new ZQuery());
			var usrRevenue1 = billedUsages.Single(x => x.BU9_PriceCode == "USR" && x.BU9_LD == lic1.LA_LD);
			var shpRevenue1 = billedUsages.Single(x => x.BU9_PriceCode == "SHP" && x.BU9_LD == lic1.LA_LD);
			var usrRevenue2 = billedUsages.Single(x => x.BU9_PriceCode == "USR" && x.BU9_LD == lic2.LA_LD);
			var shpRevenue2 = billedUsages.Single(x => x.BU9_PriceCode == "SHP" && x.BU9_LD == lic2.LA_LD);
			var usrRevenue3 = billedUsages.Single(x => x.BU9_PriceCode == "USR" && x.BU9_LD == lic3.LA_LD);
			var shpRevenue3 = billedUsages.Single(x => x.BU9_PriceCode == "SHP" && x.BU9_LD == lic3.LA_LD);

			// Discount factor = 0.80 (20%)
			// Commitment scaling = 1.50
			decimal preDiscountScaling = 1.50m;
			decimal postDiscountScaling = preDiscountScaling * 0.80m;

			// Tax = 1.1 (10%)
			AssertEquals("AH_LocalTotal", (10 * 40 + 500 * 3) * postDiscountScaling * 1.1m, invoice.AH_LocalTotal);

			decimal commitmentAdjustmentUnits = commitmentUnits - monthlyUsage1.CommitmentGroup.TotalUsedLicenceUnits;

			// DB1
			CombineAssertions(() =>
			{
				AssertEquals("USR UnitCount", 5m, usrLine1.UnitCount);
				AssertEquals("SHP UnitCount", 250m, shpLine1.UnitCount);
				AssertEquals("commitment PK", commitment1.PK, monthlyUsage1.Commitment.PK);
				AssertEquals("Commitment Adjustment", 316.67m, monthlyUsage1.CommitmentLines.Single().Price);
				AssertEquals("TotalLicenceUnits includes a share of the total adjustment", monthlyUsage1.TotalUsedLicenceUnits + monthlyUsage1.CommitmentGroup.Adjustment, monthlyUsage1.TotalLicenceUnits);

				AssertEquals("USR BU9_LocalAmountPostDiscount", 5 * 40 * postDiscountScaling, usrRevenue1.BU9_LocalAmountPostDiscount);
				AssertEquals("USR BU9_LocalAmountPreDiscount", 5 * 40 * preDiscountScaling, usrRevenue1.BU9_LocalAmountPreDiscount);

				AssertEquals("SHP BU9_LocalAmountPostDiscount", 250 * 3 * postDiscountScaling, shpRevenue1.BU9_LocalAmountPostDiscount);
				AssertEquals("SHP BU9_LocalAmountPreDiscount", 250 * 3 * preDiscountScaling, shpRevenue1.BU9_LocalAmountPreDiscount);
			});

			// DB2
			CombineAssertions(() =>
			{
				AssertEquals("USR UnitCount", 3m, usrLine2.UnitCount);
				AssertEquals("SHP UnitCount", 150m, shpLine2.UnitCount);
				AssertEquals("commitment PK", commitment2.PK, monthlyUsage2.Commitment.PK);
				AssertEquals("Commitment Adjustment", 316.67m, monthlyUsage2.CommitmentLines.Single().Price);
				AssertEquals("TotalLicenceUnits includes a share of the total adjustment", monthlyUsage2.TotalUsedLicenceUnits + monthlyUsage2.CommitmentGroup.Adjustment, monthlyUsage2.TotalLicenceUnits);

				AssertEquals("USR BU9_LocalAmountPostDiscount", 3 * 40 * postDiscountScaling, usrRevenue2.BU9_LocalAmountPostDiscount);
				AssertEquals("USR BU9_LocalAmountPreDiscount", 3 * 40 * preDiscountScaling, usrRevenue2.BU9_LocalAmountPreDiscount);

				AssertEquals("SHP BU9_LocalAmountPostDiscount", 150 * 3 * postDiscountScaling, shpRevenue2.BU9_LocalAmountPostDiscount);
				AssertEquals("SHP BU9_LocalAmountPreDiscount", 150 * 3 * preDiscountScaling, shpRevenue2.BU9_LocalAmountPreDiscount);
			});

			// DB3
			CombineAssertions(() =>
			{
				AssertEquals("USR UnitCount", 2m, usrLine3.UnitCount);
				AssertEquals("SHP UnitCount", 100m, shpLine3.UnitCount);
				AssertEquals("commitment PK", commitment3.PK, monthlyUsage3.Commitment.PK);
				AssertEquals("Commitment Adjustment", 316.66m, monthlyUsage3.CommitmentLines.Single().Price);
				AssertEquals("TotalLicenceUnits includes a share of the total adjustment", monthlyUsage3.TotalUsedLicenceUnits + monthlyUsage3.CommitmentGroup.Adjustment, monthlyUsage3.TotalLicenceUnits);

				AssertEquals("USR BU9_LocalAmountPostDiscount", 2 * 40 * postDiscountScaling, usrRevenue3.BU9_LocalAmountPostDiscount);
				AssertEquals("USR BU9_LocalAmountPreDiscount", 2 * 40 * preDiscountScaling, usrRevenue3.BU9_LocalAmountPreDiscount);

				AssertEquals("SHP BU9_LocalAmountPostDiscount", 100 * 3 * postDiscountScaling, shpRevenue3.BU9_LocalAmountPostDiscount);
				AssertEquals("SHP BU9_LocalAmountPreDiscount", 100 * 3 * preDiscountScaling, shpRevenue3.BU9_LocalAmountPreDiscount);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_CommitmentWithMultipleCurrencies()
		{
			var periodStart = new ZDateTime(2018, 7, 1);
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.50); // 1 AUD = 0.50 USD

			var prices = lic.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;

			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Commitment";
			discount.PHD_Percent = 20;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = prices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 40;
			item1.L7_LicenceUnits = 400;
			item1.L7_PGM_DiscountGroupCode = "Standard";
			item1.L7_Order = 1;

			var item2 = prices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.STL;
			item2.L7_Code = "SHP";
			item2.L7_Price = 3;
			item2.L7_LicenceUnits = 30;
			item2.L7_PGM_DiscountGroupCode = "Standard";
			item2.L7_Order = 2;
			item2.L7_RX_NKCurrency = "USD";

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item2.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item2.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = periodStart;

			var commitment = Factory.New<CommitmentLicenceSetting>();
			lic.Database.LicenceSettings.Add(commitment);
			commitment.LicenceUnits = (500 * 30 + 10 * 400) * 1.50; // 15000 + 4000 = 19000, 19000 * 1.50 = 28500

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic.ClientCompany, 500);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			var monthlyUsage = bill.MonthlyUsages.First();
			var line1 = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "USR");
			var line2 = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "SHP");
			AssertEquals(1, monthlyUsage.CommitmentLines.Count());
			var commitmentLine = monthlyUsage.CommitmentLines.First();

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var billedUsages = invoice.Factory.Load<EdiBilledUsage>(new ZQuery());
			var usrRevenue = billedUsages.Single(x => x.BU9_PriceCode == "USR");
			var shpRevenue = billedUsages.Single(x => x.BU9_PriceCode == "SHP");

			// Discount factor = 0.80 (20%)
			// Commitment scaling = 1.50
			// Tax = 1.1 (10%)
			decimal preDiscountScaling = 1.50m;
			decimal postDiscountScaling = preDiscountScaling * 0.80m;

			CombineAssertions(() =>
			{
				AssertEquals("commitmentLine.PriceItem.L7_LicenceUnits", 9500m, commitmentLine.PriceItem.L7_LicenceUnits);
				AssertEquals("commitmentLine.TotalUnitCount", 1m, commitmentLine.TotalUnitCount);
				AssertEquals("commitmentLine.PriceCurrency", "AUD", commitmentLine.PriceCurrency);
				AssertEquals("commitmentLine.Price", 950m, commitmentLine.Price);
				AssertEquals("commitmentLine.DiscountedPrice", 950m * 0.8m, commitmentLine.DiscountedPrice);

				AssertEquals("AH_LocalTotal", (10 * 40 + 500 * 3 * 2.0m) * postDiscountScaling * 1.1m, invoice.AH_LocalTotal);

				AssertEquals("USR UnitCount", 10m, line1.UnitCount);
				AssertEquals("SHP UnitCount", 500m, line2.UnitCount);
				AssertEquals("commitment PK", commitment.PK, monthlyUsage.Commitment.PK);
				AssertEquals("TotalLicenceUnits", commitment.LicenceUnits, monthlyUsage.TotalLicenceUnits);

				AssertEquals("USR BU9_LocalAmountPostDiscount", 10 * 40 * postDiscountScaling, usrRevenue.BU9_LocalAmountPostDiscount);
				AssertEquals("USR BU9_LocalAmountPreDiscount", 10 * 40 * preDiscountScaling, usrRevenue.BU9_LocalAmountPreDiscount);

				AssertEquals("SHP BU9_LocalAmountPostDiscount", 3 * 500 * 2.0m * postDiscountScaling, shpRevenue.BU9_LocalAmountPostDiscount);
				AssertEquals("SHP BU9_LocalAmountPreDiscount", 3 * 500 * 2.0m * preDiscountScaling, shpRevenue.BU9_LocalAmountPreDiscount);
				AssertNoNotifications(bill);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_PerCompanyBilling()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			BillingTestHelper.CreateExchangeRate(Factory, "NZD", 1.25); // 1 AUD = 1.25 NZD

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;

			var lic3paidBy2 = BillingTestHelper.CreateDependentLicence(lic2, "CCC");

			var licOther = BillingTestHelper.CreateLicence(Factory, "TTT");

			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "NZD");
			BillingTestHelper.SetInvoicing(lic3paidBy2, Env.CurrentBranch.PK, "NZD");
			BillingTestHelper.SetInvoicing(licOther, Env.CurrentBranch.PK, "AUD");
			lic1.LA_RX_NKPriceCurrency = "AUD";
			lic2.LA_RX_NKPriceCurrency = "NZD";
			lic3paidBy2.LA_RX_NKPriceCurrency = "NZD";

			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic3paidBy2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			licOther.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.Database.LD_IsBilledPerCompany = true;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 4;
			item1.L7_LicenceUnits = 40;
			item1.L7_Order = 1;
			item1.L7_ParentCategory = BillingConstants.BillingSystem.STL;
			item1.L7_ParentCode = "#MF";
			var nzRate1 = item1.CurrencyRates.AddNew();
			nzRate1.PIR_Price = 5;
			nzRate1.PIR_RX_NKCurrency = "NZD";

			var minimumFeeItem = prices.Items.AddNew();
			minimumFeeItem.L7_Category = BillingConstants.BillingSystem.STL;
			minimumFeeItem.L7_Code = "#MF";
			minimumFeeItem.L7_Price = 150;
			minimumFeeItem.L7_LicenceUnits = 0;
			minimumFeeItem.L7_Order = 3;
			minimumFeeItem.L7_FeeType = BillingConstants.FeeType.MinimumFee;
			var nzRate2 = minimumFeeItem.CurrencyRates.AddNew();
			nzRate2.PIR_Price = 150 * 1.25;
			nzRate2.PIR_RX_NKCurrency = "NZD";

			var itemPerDatabase = prices.Items.AddNew();
			itemPerDatabase.L7_Category = BillingConstants.BillingSystem.ODM;
			itemPerDatabase.L7_Code = "GZH";
			itemPerDatabase.L7_Price = 300;
			itemPerDatabase.L7_LicenceUnits = 0;
			itemPerDatabase.L7_Order = 5;
			itemPerDatabase.L7_FeeType = BillingConstants.FeeType.Database;
			var nzRate3 = itemPerDatabase.CurrencyRates.AddNew();
			nzRate3.PIR_Price = 300 * 1.25;
			nzRate3.PIR_RX_NKCurrency = "NZD";

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			itemPerDatabase.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			minimumFeeItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			var priceHeaderLinkOther = licOther.Database.PriceHeaderLinks.AddNew();
			priceHeaderLinkOther.PHL_L6 = prices.PK;
			priceHeaderLinkOther.PHL_RX_NKCurrency = "AUD";
			priceHeaderLinkOther.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 4);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic3paidBy2.ClientCompany, 6);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), licOther.ClientCompany, 1000);

			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "GZH", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 55);
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "GZH", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 77);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(3, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			var bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
			var billOther = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licOther.Company.LC_OH);
			AssertNoErrors(bill1);
			AssertNoErrors(bill2);
			AssertNoErrors(billOther);
			AssertEquals("AUD", bill1.InvoiceCurrencyCode);
			AssertEquals("NZD", bill2.InvoiceCurrencyCode);
			AssertEquals("AUD", bill1.MonthlyUsages.First().PriceCurrencies.First());
			AssertEquals("NZD", bill2.MonthlyUsages.First().PriceCurrencies.First());

			AssertEquals((4 + 6) * 5m, bill2.InvoicePreDiscountTotal);
			AssertEquals("minimum fee and per-database item paid by database owner", 300 + 150 - bill2.InvoicePostDiscountTotal / 1.25m, bill1.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_PerCompanyBilling_WithNotBilledInvoiceInstruction()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;

			var delivery = BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			delivery.L9_IsBilled = false;

			lic1.LA_RX_NKPriceCurrency = "AUD";
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.Database.LD_IsBilledPerCompany = true;

			var prices = BillingTestHelper.CreateStlPriceList(lic1.Company);
			var item1 = BillingTestHelper.AddPriceItem(prices, "USR", "", "", 4m, "", 40m);
			item1.L7_Order = 1;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 5);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			AssertNoErrors(bill1);
			AssertEquals(false, bill1.IsBilled);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_NoInvoiceDelivery()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");

			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 4;
			item1.L7_LicenceUnits = 40;
			item1.L7_Order = 1;
			item1.L7_ParentCategory = BillingConstants.BillingSystem.STL;
			item1.L7_ParentCode = "#MF";

			var minimumFeeItem = prices.Items.AddNew();
			minimumFeeItem.L7_Category = BillingConstants.BillingSystem.STL;
			minimumFeeItem.L7_Code = "#MF";
			minimumFeeItem.L7_Price = 150;
			minimumFeeItem.L7_LicenceUnits = 0;
			minimumFeeItem.L7_Order = 3;
			minimumFeeItem.L7_FeeType = BillingConstants.FeeType.MinimumFee;

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			minimumFeeItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 1);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			AssertHasRowErrorContaining(bill1, BillRecipient.ValidationMessages.NoBranch);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_InactiveOrg()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 4;
			item1.L7_LicenceUnits = 40;
			item1.L7_Order = 1;
			item1.L7_ParentCategory = BillingConstants.BillingSystem.STL;
			item1.L7_ParentCode = "#MF";

			var minimumFeeItem = prices.Items.AddNew();
			minimumFeeItem.L7_Category = BillingConstants.BillingSystem.STL;
			minimumFeeItem.L7_Code = "#MF";
			minimumFeeItem.L7_Price = 150;
			minimumFeeItem.L7_LicenceUnits = 0;
			minimumFeeItem.L7_Order = 3;
			minimumFeeItem.L7_FeeType = BillingConstants.FeeType.MinimumFee;

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			minimumFeeItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 1);

			lic1.Company.Header.OH_IsActive = false;

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			AssertHasRowErrorContaining(bill1, "Organization is inactive.");
			AssertEquals(1, billing.ValidationResults.Count);
			AssertEquals(bill1.PK, billing.ValidationResults[0].StlBillPK);
			AssertEquals(bill1.OrganisationCode, billing.ValidationResults[0].OrgCode);
			AssertEquals("Error - STL Bill: Organization is inactive.", billing.ValidationResults[0].ValidationMessage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_PerCompanyBilling_MultiplePriceCurrencies()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			BillingTestHelper.CreateExchangeRate(Factory, "NZD", 1.25); // 1 AUD = 1.25 NZD

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateDependentLicence(lic1, "BBB");
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;

			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			lic1.LA_RX_NKPriceCurrency = "AUD";
			lic2.LA_RX_NKPriceCurrency = "NZD";

			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.Database.LD_IsBilledPerCompany = true;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 4;
			item1.L7_LicenceUnits = 40;
			item1.L7_Order = 1;
			item1.L7_ParentCategory = BillingConstants.BillingSystem.STL;
			item1.L7_ParentCode = "#MF";
			var nzRate1 = item1.CurrencyRates.AddNew();
			nzRate1.PIR_Price = 5;
			nzRate1.PIR_RX_NKCurrency = "NZD";

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 4);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			AssertHasRowErrorContaining(bill1, "multiple price currencies");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_PerCompanyBilling_MultipleTaxes()
		{
			var freeGst = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");

			var lic3paidBy2NoTax = BillingTestHelper.CreateDependentLicence(lic2, "CCC");

			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(lic3paidBy2NoTax, Env.CurrentBranch.PK, "AUD");
			lic1.LA_RX_NKPriceCurrency = "AUD";
			lic2.LA_RX_NKPriceCurrency = "AUD";
			lic3paidBy2NoTax.LA_RX_NKPriceCurrency = "AUD";

			lic3paidBy2NoTax.Company.InvoiceDeliveries[0].L9_AT_TaxId = freeGst.PK;

			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic3paidBy2NoTax.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.Database.LD_IsBilledPerCompany = true;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 40;
			item1.L7_LicenceUnits = 400;
			item1.L7_Order = 1;

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic3paidBy2NoTax.ClientCompany, 3);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(2, billing.Bills.Count);

			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			var bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
			AssertNoErrors(bill1);
			AssertHasRowErrorContaining(bill2, "multiple taxes");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_HUBonly()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Prepay";
			discount.PHD_Percent = 10;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = prices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Prepay Only";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			item1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			item1.L7_Ref4 = "Interface1";
			item1.L7_Price = 40;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_PGM_DiscountGroupCode = "Prepay Only";

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, item1.L7_Ref4, new ZDateTime(2015, 7, 1), lic.ClientCompany, 10);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic.Company.LC_OH);
			AssertNoErrors(bill1);
			AssertEquals(10 * 40m, bill1.InvoicePreDiscountTotal);
			AssertEquals(10 * 40m * 0.9m, bill1.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_HUBPriceOnSTLPriceList()
		{
			var periodStart = BillingTestHelper.MonthToday;

			LicenceCompany.ClearStandardPricesCompanyCache();
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");

			var legacyHubPrices = lic.Company.PriceHeaders.AddNew();
			legacyHubPrices.L6_RX_NKCurrency = "AUD";
			legacyHubPrices.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var legacyHubPriceItem = legacyHubPrices.Items.AddNew();
			legacyHubPriceItem.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			legacyHubPriceItem.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			legacyHubPriceItem.L7_Ref4 = "OldInterface1";
			legacyHubPriceItem.L7_Price = 40;
			legacyHubPriceItem.L7_Order = 1;
			legacyHubPriceItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			legacyHubPriceItem.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			Factory.Save();

			// Populate standard pricelist
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "CMP");
			stlPrices.L6_RX_NKCurrency = "USD";
			stlPrices.L6_HasExchangeRates = true;
			var audRate = stlPrices.ExchangeRates.AddNew();
			audRate.PHE_RX_NKCurrency = "AUD";
			audRate.PHE_Rate = 1.25m;
			audRate.PHE_GroupCode = "STL";
			var newHubPriceItem = stlPrices.Items.FindByCode("CMP");
			newHubPriceItem.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			newHubPriceItem.L7_Price = 35m;
			foreach (var item in stlPrices.Items)
			{
				item.L7_ExchangeRateGroupCode = "STL";
			}
			AssertNoErrors(newHubPriceItem);

			BillingTestHelper.CreatePriceLink(lic.Database, stlPrices, periodStart.AddYears(-1));

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, legacyHubPriceItem.L7_Ref4, periodStart, lic.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "NewInterface1", periodStart, lic.ClientCompany, 7);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic.Company.LC_OH);
			AssertNoErrors(bill1);
			AssertEquals(10 * 40m + 7 * 35m * 1.25m, bill1.InvoicePreDiscountTotal);
			var lines = bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.AdditionalDescription).ToList();
			AssertEquals(2, lines.Count);
			AssertEquals("", lines[0].AdditionalDescription);
			AssertEquals("NewInterface1", lines[1].AdditionalDescription);
			AssertEquals("AUD", lines[1].PriceCurrency);
			AssertEquals(35m * 1.25m, lines[1].Price);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_HubPriceNotFound()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var anotherItem = prices.Items.AddNew();
			anotherItem.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			anotherItem.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			anotherItem.L7_Ref4 = "Interface1";
			anotherItem.L7_Price = 40;
			anotherItem.L7_Order = 1;
			anotherItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			anotherItem.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			anotherItem.L7_PGM_DiscountGroupCode = "Prepay Only";

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, "New Interface Not On Pricelist", new ZDateTime(2015, 7, 1), lic.ClientCompany, 10);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var monthlyUsage = billing.Bills[0].MonthlyUsages.First();
			AssertHasRowError(monthlyUsage, "HUB [CMP.New Interface Not On Pricelist]: No price found. Price must be on the STL price list or, for older pricing, the HUB pricelist on the database usage owner (defaults to first live licence), or whoever their invoice is delivered to.");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_UnitBreakParentCode_TransactionalOneVolumeBreak()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(lic3, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic3.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";

			var itemBreakParent = prices.Items.AddNew();
			itemBreakParent.L7_Category = BillingConstants.BillingSystem.USCustoms;
			itemBreakParent.L7_Code = "AAA";
			itemBreakParent.L7_Price = 0;
			itemBreakParent.L7_Order = 0;
			itemBreakParent.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.USCustoms;
			item1.L7_Code = "ORD";
			item1.L7_Price = 5;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item1.L7_UnitBreakParentCode = "AAA";

			var item2 = prices.Items.AddNew();
			item2.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item2.L7_Category = BillingConstants.BillingSystem.USCustoms;
			item2.L7_Code = "ORD";
			item2.L7_Price = 4;
			item2.L7_Order = 2;
			item2.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item2.L7_UnitBreak = 100;
			item2.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item2.L7_UnitBreakParentCode = "AAA";

			var item3 = prices.Items.AddNew();
			item3.L7_Category = BillingConstants.BillingSystem.USCustoms;
			item3.L7_Code = "ORD";
			item3.L7_Price = 3;
			item3.L7_Order = 3;
			item3.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item3.L7_UnitBreak = 500;
			item3.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item3.L7_UnitBreakParentCode = "AAA";

			BillingTestHelper.CreatePriceLink(lic1.Database, prices, new ZDateTime(2015, 7, 1));
			BillingTestHelper.CreatePriceLink(lic2.Database, prices, new ZDateTime(2015, 7, 1));
			BillingTestHelper.CreatePriceLink(lic3.Database, prices, new ZDateTime(2015, 7, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "AAA", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 600);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "AAA", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 110);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "AAA", new ZDateTime(2015, 7, 1), lic3.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "AAA", new ZDateTime(2015, 6, 1), lic1.ClientCompany, 55);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "AAA", new ZDateTime(2015, 6, 1), lic2.ClientCompany, 66);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "AAA", new ZDateTime(2015, 6, 1), lic3.ClientCompany, 77);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "ORD", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 50);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "ORD", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 150);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.USCustoms, "ORD", new ZDateTime(2015, 7, 1), lic3.ClientCompany, 501);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(3, billing.Bills.Count);

			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			var bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
			var bill3 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic3.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill1", bill1);
				AssertNoErrors("bill2", bill2);
				AssertNoErrors("bill3", bill3);
				AssertEquals("bill1.InvoicePreDiscountTotal", 50 * 3m, bill1.InvoicePreDiscountTotal);
				AssertEquals("bill2.InvoicePreDiscountTotal", 150 * 4m, bill2.InvoicePreDiscountTotal);
				AssertEquals("bill3.InvoicePreDiscountTotal", 501 * 5m, bill3.InvoicePreDiscountTotal);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_Fees_NoDelivery()
		{
			BillingTestHelper.LoadClientSpecificDocuments();

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);

			var feeAUD = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc AUD", 200, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			feeAUD.L8_RX_NKCurrency = "AUD";

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertEquals("1 - Error - " + BillRecipient.ValidationMessages.NoBranch, bill1.Status);
				AssertHasRowError("bill1", bill1, BillRecipient.ValidationMessages.NoBranch);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_GlobalContainerTracking()
		{
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.75); // 1 AUD = 0.75 USD

			// Populate standard pricelist
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var ctrPriceList = stdLicCompany.PriceHeaders.AddNew();
			ctrPriceList.L6_PricelistVersion = "V1";
			ctrPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.GlobalContainerTracking;
			ctrPriceList.L6_RX_NKCurrency = "USD";
			ctrPriceList.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var item1 = ctrPriceList.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.GlobalContainerTracking;
			item1.L7_Code = "CTR";
			item1.L7_Price = 0.95;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = chargeCode;
			item1.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item1.L7_UnitBreak = 0;

			var item2 = ctrPriceList.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.GlobalContainerTracking;
			item2.L7_Code = "CTR";
			item2.L7_Price = 0.90;
			item2.L7_Order = 2;
			item2.L7_ChargeCode = chargeCode;
			item2.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item2.L7_UnitBreak = 250;

			var item3 = ctrPriceList.Items.AddNew();
			item3.L7_Category = BillingConstants.BillingSystem.GlobalContainerTracking;
			item3.L7_Code = "CTR";
			item3.L7_Price = 0.85;
			item3.L7_Order = 3;
			item3.L7_ChargeCode = chargeCode;
			item3.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item3.L7_UnitBreak = 1000;

			var item4 = ctrPriceList.Items.AddNew();
			item4.L7_Category = BillingConstants.BillingSystem.GlobalContainerTracking;
			item4.L7_Code = "CTR";
			item4.L7_Price = 0.80;
			item4.L7_Order = 4;
			item4.L7_ChargeCode = chargeCode;
			item4.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			item4.L7_UnitBreak = 2000;

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "USD";
			BillingTestHelper.CreatePriceLink(licence.Database, prices, new ZDateTime(2016, 3, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, "CTR", "", new ZDateTime(2016, 3, 1), licence.ClientCompany, 1200);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2016, 3, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal", 1200 * 0.85m / 0.75m, bill.InvoicePreDiscountTotal);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_ABMCustoms()
		{
			BillingTestHelper.CreateExchangeRate(Factory, "EUR", 0.50); // 1 AUD = 0.50 EUR

			// Populate standard pricelist
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var abmPriceList = stdLicCompany.PriceHeaders.AddNew();
			abmPriceList.L6_PricelistVersion = "V1";
			abmPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.ABMCustoms;
			abmPriceList.L6_RX_NKCurrency = "EUR";
			abmPriceList.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var item1 = abmPriceList.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.ABMCustoms;
			item1.L7_Code = "CTM";
			item1.L7_Price = 1.00m;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = chargeCode;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;

			var item2 = abmPriceList.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.ABMCustoms;
			item2.L7_Code = "POC";
			item2.L7_Price = 0.33m;
			item2.L7_Order = 2;
			item2.L7_ChargeCode = chargeCode;
			item2.L7_FeeType = BillingConstants.FeeType.Transactional;

			var item3 = abmPriceList.Items.AddNew();
			item3.L7_Category = BillingConstants.BillingSystem.ABMCustoms;
			item3.L7_Code = "FRP";
			item3.L7_Price = 1.00m;
			item3.L7_Order = 3;
			item3.L7_ChargeCode = chargeCode;
			item3.L7_FeeType = BillingConstants.FeeType.Transactional;

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "USD";
			BillingTestHelper.CreatePriceLink(licence.Database, prices, new ZDateTime(2016, 3, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, "ABM", "CTM", new ZDateTime(2016, 3, 1), licence.ClientCompany, 100);
			BillingTestHelper.CreateChargeableUsage(Factory, "ABM", "POC", new ZDateTime(2016, 3, 1), licence.ClientCompany, 200);
			BillingTestHelper.CreateChargeableUsage(Factory, "ABM", "FRP", new ZDateTime(2016, 3, 1), licence.ClientCompany, 300);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2016, 3, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal", (100 * 1.00m + 200 * 0.33m + 300 * 1.00m) / 0.50m, bill.InvoicePreDiscountTotal);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2016, 3, 16)]
		public void TestGenerateReport_PrepayCurrency_RateUp()
		{
			// Last month’s undiscounted amount was
			// USD 100,000.00
			// AUD 200,000.00 (at exchange rate 0.5000)

			// This month their balance was AUD 140,000 hence they were below the prepayment amount in AUD.
			// However this balance in USD is 112,000 (exchange rate is now 0.8) which is above the prepayment amount.

			decimal usdLastMonthUndiscounted = 100000;
			decimal audLastMonthUndiscounted = 200000;
			decimal audBalanceThisMonth = 140000m;

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);
			var period2 = period1.AddMonths(1);

			var rateForPeriod1 = BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.5); // 1 AUD = x USD
			var rateForPeriod2 = BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.8);
			rateForPeriod1.RE_StartDate = period1.AddDays(-1);
			rateForPeriod1.RE_ExpiryDate = period2.AddMonths(1).AddMinutes(-2);
			rateForPeriod2.RE_StartDate = period2.AddMonths(1);
			rateForPeriod2.RE_ExpiryDate = period2.AddMonths(2);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "USD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_UseStandardDiscount = false;

			var discount = priceHeader.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Prepayment;
			discount.PHD_Name = "Prepay";
			discount.PHD_Percent = 10;
			discount.PHD_IsDefaultEnabled = true;

			var itemDiscount = priceHeader.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Prepay Only";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.ImporterSecurityFiling;
			item1.L7_Code = "ISF";
			item1.L7_Price = 1;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			item1.L7_PGM_DiscountGroupCode = "Prepay Only";

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = priceHeader.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "USD";
			priceHeaderLink.PHL_ValidFrom = period1;

			var noPrepayForPeriod1 = Factory.New<DiscountLicenceSetting>();
			noPrepayForPeriod1.LS9_IsActive = false;
			noPrepayForPeriod1.LS9_LD = lic1.LA_LD;
			noPrepayForPeriod1.LS9_Name = "Prepay";
			noPrepayForPeriod1.LS9_ValidFrom = period1;
			noPrepayForPeriod1.LS9_ValidTo = period2.AddDays(-1);
			lic1.Database.LicenceSettings.Add(noPrepayForPeriod1);

			BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", period1, lic1, (int)usdLastMonthUndiscounted);
			BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", period2, lic1, 10);

			// deposit to cover the first invoice plus the undiscounted amount of the first invoice
			var deposit = Factory.New<EdiDepositAdjust>();
			deposit.DEA_Amount = audBalanceThisMonth + audLastMonthUndiscounted;
			deposit.DEA_ChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			deposit.DEA_GC = Env.CurrentCompany.PK;
			deposit.DEA_OH = lic1.Company.LC_OH;
			deposit.DEA_RX_NKCurrency = "AUD";
			deposit.DEA_Tax = deposit.DEA_Amount * 0.1m;

			Factory.Save();

			// need to set today's date since currency exchange is based on today's date
			TestDateAttribute.Date = period1.AddMonths(1).AddDays(1).ToDateTime();
			var billing = new StlBilling(Factory);
			billing.AccumulateMonths = 0;
			billing.DateTo = period1.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill1 = billing.Bills[0];
			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();

			TestDateAttribute.Date = period2.AddMonths(1).AddDays(1).ToDateTime();
			billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.AccumulateMonths = 0;
			billing.DateTo = period2.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill2 = billing.Bills[0];

			AssertNoErrors("bill1 " + bill1.Status, bill1);
			AssertEquals(usdLastMonthUndiscounted, bill1.InvoicePreDiscountTotal);
			AssertEquals(audLastMonthUndiscounted, bill1.AudInvoicePostDiscountTotal);

			AssertEquals(invoice1.PK, bill2.InvoicesForLastMonth.Single().PK);

			AssertEquals(audBalanceThisMonth * 1.1m, bill2.DepositAvailableLocal);
			AssertEquals(true, bill2.HasPrepaid);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2016, 3, 16)]
		public void TestGenerateReport_PrepayCurrency_RateDown()
		{
			// Last month’s undiscounted amount was
			// USD 100,000 (110,000 inc tax)
			// AUD 125,000 (at exchange rate 0.8)

			// This month their balance was 110,000 USD at last month's rate, i.e., AUD 137,500
			// which is USD 68,750 at this month's rate.
			// So they are above the required amount at last month's rate, and below at this month's rate.

			decimal usdLastMonthUndiscounted = 100000;

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);
			var period2 = period1.AddMonths(1);

			var rateForPeriod1 = BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.8); // 1 AUD = x USD
			var rateForPeriod2 = BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.5);
			rateForPeriod1.RE_StartDate = period1.AddDays(-1);
			rateForPeriod1.RE_ExpiryDate = period2.AddMonths(1).AddMinutes(-2);
			rateForPeriod2.RE_StartDate = period2.AddMonths(1);
			rateForPeriod2.RE_ExpiryDate = period2.AddMonths(2);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "USD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_UseStandardDiscount = false;

			var prepayDiscount = priceHeader.StlDiscounts.AddNew();
			prepayDiscount.PHD_Type = BillingConstants.DiscountCalculator.Prepayment;
			prepayDiscount.PHD_Name = "Prepay";
			prepayDiscount.PHD_Percent = 10;
			prepayDiscount.PHD_IsDefaultEnabled = true;

			var specialDiscount = priceHeader.StlDiscounts.AddNew();
			specialDiscount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			specialDiscount.PHD_Name = "Special";
			specialDiscount.PHD_Percent = 50;
			specialDiscount.PHD_IsDefaultEnabled = true;

			var prepayItemDiscount = priceHeader.StlItemDiscounts.AddNew();
			prepayItemDiscount.PGM_GroupCode = "Prepay and Special";
			prepayItemDiscount.PGM_PHD = prepayDiscount.PK;
			var specialItemDiscount = priceHeader.StlItemDiscounts.AddNew();
			specialItemDiscount.PGM_GroupCode = "Prepay and Special";
			specialItemDiscount.PGM_PHD = specialDiscount.PK;

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.ImporterSecurityFiling;
			item1.L7_Code = "ISF";
			item1.L7_Price = 1;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			item1.L7_PGM_DiscountGroupCode = prepayItemDiscount.PGM_GroupCode;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = priceHeader.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "USD";
			priceHeaderLink.PHL_ValidFrom = period1;

			var chargeForPeriod1 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", period1, lic1, (int)usdLastMonthUndiscounted);
			var chargeForPeriod2 = BillingTestHelper.CreateChargeableUsage(Factory, "ISF", "", period2, lic1, 10);

			var accTest = new TestObjectCreator(Factory);

			// receipt to cover the first invoice plus the undiscounted amount of the first invoice
			var usdInitialPayment = (usdLastMonthUndiscounted * 0.45m + usdLastMonthUndiscounted) * 1.1m;
			var receipt = accTest.CreateARReceipt(0.8, usdInitialPayment, period1, ZDateTime.Empty, lic1.Company.LC_OH, accTest.USDBankAccount.PK);
			receipt.AH_RX_NKTransactionCurrency = "USD";
			receipt.AH_OutstandingAmount = -usdInitialPayment / 0.8m;

			Factory.Save();

			AssertEquals(usdInitialPayment / 0.8m, ARBalance.GetOutstandingBalance(new[] { lic1.Company.LC_OH.ToGuid() }, Env.CurrentCompanyPK, period2.ToDateTime()).First().Value);

			// need to set today's date since currency exchange is based on today's date
			TestDateAttribute.Date = period1.AddMonths(1).AddDays(1).ToDateTime();
			var billing = new StlBilling(Factory);
			billing.AccumulateMonths = 0;
			billing.DateTo = period1.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill1 = billing.Bills[0];
			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();

			var lastUsageInvoice = Factory.Load<EdiUsageInvoice>(new ZQuery()).FirstOrDefault();
			AssertNotNull(lastUsageInvoice);
			AssertEquals(110000m, lastUsageInvoice.EUI_PrepayAmountIncTax);

			TestDateAttribute.Date = period2.AddMonths(1).AddDays(1).ToDateTime();
			billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.AccumulateMonths = 0;
			billing.DateTo = period2.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill2 = billing.Bills[0];

			var monthlyUsage = bill2.MonthlyUsages.First();

			CombineAssertions(() =>
			{
				AssertNoErrors("bill1 " + bill1.Status, bill1);
				AssertEquals("bill1.InvoicePreDiscountTotal", usdLastMonthUndiscounted, bill1.InvoicePreDiscountTotal);
				AssertEquals("bill1.InvoicePostDiscountTotal", usdLastMonthUndiscounted * 0.45m, bill1.InvoicePostDiscountTotal);
				AssertEquals("bill1.AudInvoicePostDiscountTotal", usdLastMonthUndiscounted * 0.45m / 0.8m, bill1.AudInvoicePostDiscountTotal);

				AssertEquals("bill2.InvoiceForLastMonth.PK", invoice1.PK, bill2.InvoicesForLastMonth.Single().PK);

				AssertNoErrors("bill2 " + bill2.Status, bill2);

				AssertEquals("OutstandingBalanceExDepositLocal", 137500m, bill2.OutstandingBalanceExDepositLocal);

				AssertEquals("PrepayBalanceRequiredIncTax", 110000m, bill2.PrepayBalanceRequiredIncTax);
				AssertEquals("PrepayBalanceActualIncTax", 110000m, bill2.PrepayBalanceActualIncTax);

				AssertEquals("HasPrepaid", true, bill2.HasPrepaid);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2016, 3, 16)]
		public void TestGenerateReport_FeePrepayDiscount()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var feeDiscounts = new CodeDescriptionPairList();
			feeDiscounts.AddPair(EDIDataRegistry.Instance.OdplUsageChargeCode.Value, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, feeDiscounts);

			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_UseStandardDiscount = false;

			var discount = priceHeader.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Prepayment;
			discount.PHD_Name = "Prepay";
			discount.PHD_Percent = 10;
			discount.PHD_IsDefaultEnabled = true;

			var itemDiscount = priceHeader.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Prepay Only";
			itemDiscount.PGM_PHD = discount.PK;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = priceHeader.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = period1;

			var feeWithDiscount = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc 1", 700, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, period1, ZDateTime.Empty);
			feeWithDiscount.L8_LD = lic1.LA_LD;
			feeWithDiscount.L8_IsDiscountable = true;

			var feeNoDiscount = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc 2", 300, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, period1, ZDateTime.Empty);
			feeNoDiscount.L8_LD = lic1.LA_LD;
			feeNoDiscount.L8_IsDiscountable = false;

			var creator = new TestObjectCreator(Factory);
			var receipt = creator.CreateARReceipt(1m, 100000m, period1, period1, lic1.Company.LC_OH, creator.AUDBankAccount.PK);

			var lastInvoice = BillingTestHelper.CreateInvoice(Factory, lic1, EDIDataRegistry.Instance.OdplDepositChargeCode.Value, 100m, 1, "", 0);

			var lastUsageInvoice = Factory.New<EdiUsageInvoice>();
			lastUsageInvoice.EUI_PeriodStart = period1.AddMonths(-1).Date;
			lastUsageInvoice.EUI_AH_Invoice = lastInvoice.PK;
			lastUsageInvoice.EUI_PrepayAmountIncTax = 220m;

			var lastUsage = Factory.New<EdiBilledUsage>();
			lastUsage.BU9_AH_Invoice = lastInvoice.PK;
			lastUsage.BU9_LD = lic1.LA_LD;
			lastUsage.BU9_PeriodStart = lastUsageInvoice.EUI_PeriodStart;

			Factory.Save();

			// With enough balance
			{
				var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
				billing.AccumulateMonths = 0;
				billing.DateTo = period1.AddMonths(1).AddDays(-1);
				billing.GenerateReport(null);
				var bill1 = billing.Bills[0];
				var invoice1 = bill1.CreateInvoicesWithoutSave().Single();

				AssertNoErrors(bill1);
				AssertEquals(true, bill1.HasPrepaid);
				AssertEquals(700m + 300m, bill1.InvoicePreDiscountTotal);
				AssertEquals(700m - 70m + 300m, bill1.InvoicePostDiscountTotal);

				var lines = invoice1.Lines.Cast<InvoicingLineBase>();
				var fee1Charge = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
				var fee1Discount = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
				var fee2Charge = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDepositChargeCode.Value);

				AssertEquals(700m, fee1Charge.AL_LineAmount);
				AssertEquals(-70m, fee1Discount.AL_LineAmount);
				AssertEquals(300m, fee2Charge.AL_LineAmount);
			}

			// Not enough balance
			{
				lastUsageInvoice.EUI_PrepayAmountIncTax = 150000;
				Factory.Save();

				var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
				billing.AccumulateMonths = 0;
				billing.DateTo = period1.AddMonths(1).AddDays(-1);
				billing.GenerateReport(null);
				var bill1 = billing.Bills[0];
				var invoice1 = bill1.CreateInvoicesWithoutSave().Single();

				AssertNoErrors(bill1);
				AssertEquals(false, bill1.HasPrepaid);
				AssertEquals(700m + 300m, bill1.InvoicePreDiscountTotal);
				AssertEquals(700m + 300m, bill1.InvoicePostDiscountTotal);

				var lines = invoice1.Lines.Cast<InvoicingLineBase>();
				var fee1Charge = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
				var fee1Discount = lines.FirstOrDefault(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
				var fee2Charge = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDepositChargeCode.Value);

				AssertEquals(700m, fee1Charge.AL_LineAmount);
				AssertNull(fee1Discount);
				AssertEquals(300m, fee2Charge.AL_LineAmount);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2016, 3, 16)]
		public void TestGenerateReport_MultipleDatabasesPerBill()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var feeDiscounts = new CodeDescriptionPairList();
			feeDiscounts.AddPair(EDIDataRegistry.Instance.OdplUsageChargeCode.Value, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			EDIDataRegistry.Instance.FeeBillingDiscountChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, feeDiscounts);

			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "BBB");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_UseStandardDiscount = false;

			var discount = priceHeader.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Prepayment;
			discount.PHD_Name = "Prepay";
			discount.PHD_Percent = 10;
			discount.PHD_IsDefaultEnabled = true;

			var itemDiscount = priceHeader.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Prepay Only";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 40;
			item1.L7_LicenceUnits = 400;
			item1.L7_Order = 1;

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = period1;

			var priceHeaderLink2 = lic2.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink2.PHL_L6 = priceHeader.PK;
			priceHeaderLink2.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink2.PHL_ValidFrom = period1;

			var fee1Db1 = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc 1a", 100, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, period1, ZDateTime.Empty);
			fee1Db1.L8_LD = lic1.LA_LD;
			fee1Db1.L8_IsDiscountable = false;

			var fee2Db1 = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc 2a", 200, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, period1, ZDateTime.Empty);
			fee2Db1.L8_LD = lic1.LA_LD;
			fee2Db1.L8_IsDiscountable = true;

			var fee1Db2 = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc 1b", 300, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, period1, ZDateTime.Empty);
			fee1Db2.L8_LD = lic2.LA_LD;
			fee1Db2.L8_IsDiscountable = false;

			var fee2Db2 = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc 2b", 400, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, period1, ZDateTime.Empty);
			fee2Db2.L8_LD = lic2.LA_LD;
			fee2Db2.L8_IsDiscountable = true;

			var fee1NoDb = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc 1", 500, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, period1, ZDateTime.Empty);
			fee1NoDb.L8_IsDiscountable = false;

			var fee2NoDb = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc 2", 600, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, period1, ZDateTime.Empty);
			fee2NoDb.L8_IsDiscountable = true;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic2.ClientCompany, 25);

			var creator = new TestObjectCreator(Factory);
			creator.CreateARReceipt(1m, 100000m, period1, period1, lic1.Company.LC_OH, creator.AUDBankAccount.PK);

			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.AccumulateMonths = 0;
			billing.DateTo = period1.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill1 = billing.Bills[0];
			AssertEquals(2, bill1.MonthlyUsages.Count());

			AssertNoErrors(bill1);
			AssertEquals(true, bill1.HasPrepaid);
			AssertEquals(100m + 200m + 300m + 400m + 500m + 600m + 15 * 40 + 25 * 40, bill1.InvoicePreDiscountTotal);
			AssertEquals(bill1.InvoicePreDiscountTotal - 20m - 40m - 60m, bill1.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2016, 7, 19)]
		public void TestGenerateReport_DateForExchangeRate()
		{
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var company1 = branch1.Company;
			var company2 = branch2.Company;
			Factory.Save();

			var currentMonth = new ZDateTime(TestDateAttribute.Date.Year, TestDateAttribute.Date.Month, 1);
			var lastMonth = currentMonth.AddMonths(-1);

			var company1CurrentPeriod = periodHelper.SetupSinglePeriod(201607, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company1.PK);
			var company1LastPeriod = periodHelper.SetupSinglePeriod(201606, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company1.PK);

			var company2CurrentPeriod = periodHelper.SetupSinglePeriod(201607, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company2.PK);
			var company2LastPeriod = periodHelper.SetupSinglePeriod(201606, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company2.PK);

			company1LastPeriod.AM_IsSubLedgerClosed = false;
			company2LastPeriod.AM_IsSubLedgerClosed = true;

			var periodStart = new ZDateTime(2016, 6, 1);

			BillingTestHelper.LoadClientSpecificDocuments();

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var organisation1 = lic1.Company.Header;
			BillingTestHelper.SetInvoicing(organisation1, branch1.PK);

			var organisation2 = lic2.Company.Header;
			BillingTestHelper.SetInvoicing(organisation2, branch2.PK);

			var prices = lic1.Company.PriceHeaders.AddNew();
			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			var rate1 = item1.CurrencyRates.AddNew();
			rate1.PIR_RX_NKCurrency = "AUD";
			rate1.PIR_Price = 40;

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = prices.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = periodStart;
			var priceHeaderLink2 = lic2.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink2.PHL_L6 = prices.PK;
			priceHeaderLink2.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink2.PHL_ValidFrom = periodStart;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, lic1, 5);
			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory());
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(true, billing.IsBackPostAvailable);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			AssertEquals(new ZDateTime(2016, 6, 30), bill1.DateForExchangeRate);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, lic2, 9);
			Factory.Save();

			billing.GenerateReport(null);
			bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			var bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
			AssertEquals(false, billing.IsBackPostAvailable);
			AssertEquals(new ZDateTime(2016, 7, 19), bill1.DateForExchangeRate);
			AssertEquals(new ZDateTime(2016, 7, 19), bill2.DateForExchangeRate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 5, 1)]
		public void TestGenerateReport_InvoiceSplit()
		{
			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			BillingTestHelper.LoadClientSpecificDocuments();

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_ChargeCode = chargeCode;
			item1.L7_RX_NKCurrency = "AUD";
			item1.L7_Price = 100;

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = prices.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = new ZDateTime(2022, 1, 1);

			var feeAUD = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc AUD", 200, chargeCode, new ZDateTime(2022, 1, 1), ZDateTime.Empty);
			feeAUD.L8_RX_NKCurrency = "AUD";
			feeAUD.L8_LD = lic1.Database.PK;
			Factory.Save();

			//2022-03
			var invoice03_1 = BillingTestHelper.CreateInvoice(Factory, lic1, chargeCode, 100, 1, "", 0);
			var invoice03_2 = BillingTestHelper.CreateInvoice(Factory, lic1, chargeCode, 200, 1, "", 0);
			invoice03_1.AH_PostDate = new DateTime(2022, 3, 31);
			invoice03_2.AH_PostDate = new DateTime(2022, 4, 29);

			var usage03_1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new DateTime(2022, 3, 1), lic1, 15);
			usage03_1.U1_AH_Invoice = invoice03_1.PK;
			var usage03_2 = FeeBill.CreateOrUpdateChargeableUsage(invoice03_2, feeAUD, ZGuid.Empty, new DateTime(2022, 3, 1), false);

			//2022-04
			var invoice04_1 = BillingTestHelper.CreateInvoice(Factory, lic1, chargeCode, 200, 1, "", 0);
			var invoice04_2 = BillingTestHelper.CreateInvoice(Factory, lic1, chargeCode, 300, 1, "", 0);
			invoice04_1.AH_PostDate = new DateTime(2022, 4, 30);
			invoice04_2.AH_PostDate = new DateTime(2022, 4, 30);

			var usage04_1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new DateTime(2022, 4, 1), lic1, 20);
			usage04_1.U1_AH_Invoice = invoice04_1.PK;
			var usage04_2 = FeeBill.CreateOrUpdateChargeableUsage(invoice04_2, feeAUD, ZGuid.Empty, new DateTime(2022, 4, 1), false);
			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2022, 4, 30);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill1 = billing.Bills.Cast<StlBill>().Single();
			AssertEquals("5 - Invoiced", bill1.Status);
			AssertEquals($"{invoice04_1.AH_TransactionNum}, {invoice04_2.AH_TransactionNum}", bill1.InvoiceNumbers);
			AssertContainsExactElementsInExactOrder(new[] { invoice04_1.PK, invoice04_2.PK }, bill1.InvoicePksForThisMonth);

			AssertEquals($"{invoice03_1.AH_TransactionNum}, {invoice03_2.AH_TransactionNum}", bill1.LastMonthInvoiceNumbers);
			AssertEquals(300m, bill1.LastMonthInvoiceAmount);
			AssertEquals($@"00001000-100-{invoice03_1.PK}
00001001-200-{invoice03_2.PK}", string.Join("\r\n", bill1.InvoicesForLastMonth.Select(x => $"{x.AH_TransactionNum}-{x.AH_OSExTaxAmount.ToZInt()}-{x.PK}").OrderBy(x => x)));
		}

		public void TestInvoicedOrgPkToIsDebtor()
		{
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.CompanyData.OB_IsDebtor = true;
			org2.CompanyData.OB_IsDebtor = true;
			org3.CompanyData.OB_IsDebtor = false;

			var data1 = org1.CompanyDataCollection.AddNew();
			var data2 = org2.CompanyDataCollection.AddNew();
			var data3 = org3.CompanyDataCollection.AddNew();
			data1.OB_GC = company2.PK;
			data2.OB_GC = company2.PK;
			data3.OB_GC = company2.PK;
			data1.OB_IsDebtor = false;
			data2.OB_IsDebtor = true;
			data3.OB_IsDebtor = true;
			Factory.Save();

			var orgPkToIsDebtor1 = StlBilling.InvoicedOrgPkToIsDebtor(new Guid[] { org1.PK.ToGuid(), org2.PK.ToGuid(), org3.PK.ToGuid() }, Env.CurrentCompanyPK);
			var orgPkToIsDebtor2 = StlBilling.InvoicedOrgPkToIsDebtor(new Guid[] { org3.PK.ToGuid(), org2.PK.ToGuid(), org1.PK.ToGuid() }, company2.PK.ToGuid());

			AssertEquals(true, orgPkToIsDebtor1[org1.PK.ToGuid()]);
			AssertEquals(true, orgPkToIsDebtor1[org2.PK.ToGuid()]);
			AssertEquals(false, orgPkToIsDebtor1[org3.PK.ToGuid()]);

			AssertEquals(false, orgPkToIsDebtor2[org1.PK.ToGuid()]);
			AssertEquals(true, orgPkToIsDebtor2[org2.PK.ToGuid()]);
			AssertEquals(true, orgPkToIsDebtor2[org3.PK.ToGuid()]);
		}

		[TestDate(2015, 7, 10)]
		public void TestCreateInvoice_BackPost()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic.Company.PriceHeaders.AddNew();
			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			var rate1 = item1.CurrencyRates.AddNew();
			rate1.PIR_RX_NKCurrency = "AUD";
			rate1.PIR_Price = 17.5;

			var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2016, 1, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2016, 6, 1), lic, 100);

			BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, null, EDIDataRegistry.Instance.CommentChargeCode.Value)
				.AC_ChargeType = Core.Constants.ChargeType.Comment;

			Factory.Save();

			StlBill bill = new StlBill(Factory, GlbBranch.CurrentBranch, lic.Company.Header, "AUD", new ZDateTime(2015, 6, 1), new ZDateTime(2015, 6, 1));
			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 6, 30);
			billing.IsBackPostAllowed = true;
			billing.SetIsBackPostAvailableForTest(true);

			var invoice = billing.CreateInvoices(bill).Single();
			AssertEquals("back posted", new ZDateTime(2015, 6, 30), invoice.AH_InvoiceDate);
			AssertEquals("back posted", new ZDateTime(2015, 6, 30), invoice.AH_PostDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2016, 7, 19)]
		public void TestCreateInvoice_ExchangeRateIsReciprocal()
		{
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code);
			var periodHelper = new AccountingPeriodTestHelper(Factory);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			var company1 = branch1.Company;
			var company2 = branch2.Company;
			var company3 = branch3.Company;
			company1.GC_RX_NKLocalCurrency = "ZAR";
			company1.GC_IsReciprocal = true;
			company2.GC_RX_NKLocalCurrency = "AUD";
			company2.GC_IsReciprocal = false;
			company3.GC_RX_NKLocalCurrency = "ZAR";
			company3.GC_IsReciprocal = true;
			company1.GC_Code = "AAA";
			company2.GC_Code = "BBB";
			company3.GC_Code = "CCC";
			Factory.Save();

			var currentMonth = new ZDateTime(TestDateAttribute.Date.Year, TestDateAttribute.Date.Month, 1);
			var lastMonth = currentMonth.AddMonths(-1);

			var company1CurrentPeriod = periodHelper.SetupSinglePeriod(201607, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company1.PK);
			var company1LastPeriod = periodHelper.SetupSinglePeriod(201606, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company1.PK);

			var company2CurrentPeriod = periodHelper.SetupSinglePeriod(201607, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company2.PK);
			var company2LastPeriod = periodHelper.SetupSinglePeriod(201606, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company2.PK);

			var company3CurrentPeriod = periodHelper.SetupSinglePeriod(201607, currentMonth, currentMonth.AddMonths(1).AddDays(-1), company3.PK);
			var company3LastPeriod = periodHelper.SetupSinglePeriod(201606, lastMonth, lastMonth.AddMonths(1).AddDays(-1), company3.PK);

			company1LastPeriod.AM_IsSubLedgerClosed = false;
			company2LastPeriod.AM_IsSubLedgerClosed = false;
			company3LastPeriod.AM_IsSubLedgerClosed = false;

			var periodStart = lastMonth;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic3.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			BillingTestHelper.CreateChargeCodesForBranch(Factory, branch1);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, branch2);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, branch3);

			BillingTestHelper.SetInvoicing(lic1, branch1.PK, "USD");
			BillingTestHelper.SetInvoicing(lic2, branch2.PK, "USD");
			BillingTestHelper.SetInvoicing(lic3, branch3.PK, "GBP");

			Factory.Save();

			using (branch1.SetAsTemporaryContext())
			{
				BusinessObjectFactory branchFactory = new BusinessObjectFactory();
				RefExchangeRate usdExchange = BillingTestHelper.CreateExchangeRate(branchFactory, "USD", 14.505m, currentMonth);
				lic1.Company.Header.CompanyData.OB_IsDebtor = true;
				branchFactory.Save();
			}

			using (branch2.SetAsTemporaryContext())
			{
				BusinessObjectFactory branchFactory = new BusinessObjectFactory();
				RefExchangeRate usdExchange = BillingTestHelper.CreateExchangeRate(branchFactory, "USD", 0.8m, currentMonth);
				lic2.Company.Header.CompanyData.OB_IsDebtor = true;
				branchFactory.Save();
			}

			using (branch3.SetAsTemporaryContext())
			{
				BusinessObjectFactory branchFactory = new BusinessObjectFactory();
				lic3.Company.Header.CompanyData.OB_IsDebtor = true;
				branchFactory.Save();
			}

			BillingTestHelper.LoadClientSpecificDocuments();

			var fee1 = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc USD", 500, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			fee1.L8_RX_NKCurrency = "USD";
			var fee2 = BillingTestHelper.CreateLicenceFee(lic2.Company, "ESV", "desc USD", 500, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			fee2.L8_RX_NKCurrency = "USD";
			var fee3 = BillingTestHelper.CreateLicenceFee(lic3.Company, "ESV", "desc GBP", 500, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			fee3.L8_RX_NKCurrency = "GBP";

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(true, billing.IsBackPostAvailable);

			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			var bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
			var bill3 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic3.Company.LC_OH);

			using (BillingInvoicingHelper.BranchContext(bill1.InvoicingBranch.PK))
			{
				var invoice1 = billing.CreateInvoices(bill1).Single();
				AssertEquals("back posted", new ZDateTime(2016, 6, 30), invoice1.AH_InvoiceDate);
				AssertEquals("back posted", new ZDateTime(2016, 6, 30), invoice1.AH_PostDate);

				AssertEquals(14.50537m, invoice1.AH_ExchangeRate);
				AssertNoErrors("AH_ExchangeRateInfo", invoice1.AH_ExchangeRateInfo);
			}

			using (BillingInvoicingHelper.BranchContext(bill2.InvoicingBranch.PK))
			{
				var invoice2 = billing.CreateInvoices(bill2).Single();
				AssertEquals(0.8m, invoice2.AH_ExchangeRate);
			}

			AssertHasRowError(bill3, "Login company CCC: exchange rate for 30-Jun-16 not found for ZAR to GBP");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_ContainerTrackingWithNoPriceLink()
		{
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.75); // 1 AUD = 0.75 USD

			// Populate standard pricelist
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var ctrPriceList = stdLicCompany.PriceHeaders.AddNew();
			ctrPriceList.L6_PricelistVersion = "V1";
			ctrPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.GlobalContainerTracking;
			ctrPriceList.L6_RX_NKCurrency = "USD";
			ctrPriceList.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var item1 = ctrPriceList.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.GlobalContainerTracking;
			item1.L7_Code = "CTR";
			item1.L7_Price = 0.95;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = chargeCode;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_UnitBreak = 0;

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "USD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			BillingTestHelper.CreateChargeableUsage(Factory, "CTR", "", new ZDateTime(2016, 3, 1), licence.ClientCompany, 1200);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2016, 3, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal", 1200 * 0.95m, bill.InvoicePreDiscountTotal);
			});
		}

		public void TestGenerateReport_DAZ()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);
			var period2 = period1.AddMonths(1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "CN";

			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			lic2.ClientCompany.LCC_RN_NKCountryCode = "DE";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "GZT";
			item1.L7_Price = 40;
			item1.L7_Order = 1;
			item1.L7_FeeType = BillingConstants.FeeType.DatabaseLanguageZ;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_Language = Core.SharedConstants.Languages.ChineseTraditional;

			var item2 = priceHeader.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.STL;
			item2.L7_Code = "GZH";
			item2.L7_Price = 50;
			item2.L7_Order = 1;
			item2.L7_FeeType = BillingConstants.FeeType.DatabaseLanguageZ;
			item2.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item2.L7_Language = Core.SharedConstants.Languages.ChineseSimplified;

			var item3 = priceHeader.Items.AddNew();
			item3.L7_Category = BillingConstants.BillingSystem.STL;
			item3.L7_Code = "GDE";
			item3.L7_Price = 60;
			item3.L7_Order = 1;
			item3.L7_FeeType = BillingConstants.FeeType.DatabaseLanguage;
			item3.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item3.L7_Language = Core.SharedConstants.Languages.German;

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = period1;

			// Period 1 - most users in China
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic2.ClientCompany, 14);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "GZT", period1, lic1.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "GZH", period1, lic1.ClientCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "GDE", period1, lic2.ClientCompany, 7);

			// Period 2 - most users not in China
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period2, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period2, lic2.ClientCompany, 16);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "GZT", period2, lic1.ClientCompany, 4);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "GZH", period2, lic1.ClientCompany, 6);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "GDE", period2, lic2.ClientCompany, 8);

			var creator = new TestObjectCreator(Factory);
			var receipt = creator.CreateARReceipt(1m, 100000m, period1, period1, lic1.Company.LC_OH, creator.AUDBankAccount.PK);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];

			var billing2 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing2.AccumulateMonths = 0;
			billing2.DateTo = period2.AddMonths(1).AddDays(-1);
			billing2.GenerateReport(null);
			var bill2 = billing2.Bills[0];

			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertNoErrors(bill1);
			AssertEquals(60m, bill1.InvoicePreDiscountTotal);

			AssertEquals(1, bill2.MonthlyUsages.Count());
			AssertNoErrors(bill2);
			AssertEquals(40m + 50m + 60m, bill2.InvoicePreDiscountTotal);
		}

		public void TestGenerateReport_DAL()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "EUR");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "FR";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "EUR";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "C01";
			item1.L7_Price = 40;
			item1.L7_Order = 1;
			item1.L7_FeeType = BillingConstants.FeeType.DatabaseLanguage;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_Language = Core.SharedConstants.Languages.French;

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "EUR";
			priceHeaderLink1.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C01", period1, lic1.ClientCompany, 3);

			var creator = new TestObjectCreator(Factory);
			creator.CreateARReceipt(1m, 100000m, period1, period1, lic1.Company.LC_OH, creator.AUDBankAccount.PK);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertNull(bill1);

			lic1.ClientCompany.LCC_RN_NKCountryCode = "US";
			Factory.Save();
			billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertEquals(40m, bill1.InvoicePreDiscountTotal);
		}

		public void TestGenerateReport_DAC()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "EUR");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "FR";

			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR2", "DB1");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "EUR");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.ClientCompany.LCC_RN_NKCountryCode = "FR";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "EUR";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "C03";
			item1.L7_Price = 40;
			item1.L7_Order = 1;
			item1.L7_FeeType = BillingConstants.FeeType.DatabaseCountry;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_Language = "";

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "EUR";
			priceHeaderLink1.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C03", period1, lic1.ClientCompany, 3);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic2.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C03", period1, lic2.ClientCompany, 3);

			var creator = new TestObjectCreator(Factory);
			creator.CreateARReceipt(1m, 100000m, period1, period1, lic1.Company.LC_OH, creator.AUDBankAccount.PK);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertNull(bill1);

			lic2.ClientCompany.LCC_RN_NKCountryCode = "US";
			Factory.Save();
			billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertEquals(40m, bill1.InvoicePreDiscountTotal);
		}

		public void TestGenerateReport_CountryUsage()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "EUR");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "FR";

			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN1", "GE1", "DB1");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "EUR");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.ClientCompany.LCC_RN_NKCountryCode = "GE";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "EUR";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "MCC";
			item1.L7_Price = 14.44m;
			item1.L7_Order = 1;
			item1.L7_FeeType = BillingConstants.FeeType.Country;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_Language = "";

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "EUR";
			priceHeaderLink1.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "MCC", period1, lic1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "MCC", period1, lic2.ClientCompany, 1);

			var creator = new TestObjectCreator(Factory);
			var receipt = creator.CreateARReceipt(1m, 100000m, period1, period1, lic1.Company.LC_OH, creator.AUDBankAccount.PK);

			Factory.Save();
			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertEquals(28.88m, bill1.InvoicePreDiscountTotal);
		}

		public void TestGenerateReport_UsersPerCountryVolumeBreak()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "EUR");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "FR";

			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN1", "GE1", "DB1");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "EUR");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.ClientCompany.LCC_RN_NKCountryCode = "GE";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "EUR";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var priceUsr = priceHeader.Items.AddNew();
			priceUsr.L7_Category = BillingConstants.BillingSystem.STL;
			priceUsr.L7_Code = "USR";
			priceUsr.L7_Price = 100;
			priceUsr.L7_ChargeCode = "STLUSAGE";

			Action<int, int> createPriceItem = (unitBreak, price) =>
			{
				var priceItem = priceHeader.Items.AddNew();
				priceItem.L7_Category = BillingConstants.BillingSystem.STL;
				priceItem.L7_Code = "GPC";
				priceItem.L7_Price = price;
				priceItem.L7_Order = 1;
				priceItem.L7_UnitBreak = unitBreak;
				priceItem.L7_FeeType = BillingConstants.FeeType.UsersPerCountryVolumeBreak;
				priceItem.L7_ChargeCode = "STLUSAGE";
				priceItem.L7_Language = "";
				priceItem.L7_Description = $"Greater than {unitBreak} users";
			};

			createPriceItem(0, 0);
			createPriceItem(5, 170);
			createPriceItem(25, 425);
			createPriceItem(100, 855);

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "EUR";
			priceHeaderLink1.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", period1, lic2.ClientCompany, 40);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "GPC", period1, lic1.ClientCompany, 2);

			Factory.Save();
			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			var lines = bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode).ToArray();
			AssertEquals("GPC", lines[0].PriceItemCode);
			AssertEquals(425m, lines[0].Price);
			AssertEquals(2m, lines[0].UnitCount);
			AssertEquals("Greater than 25 users", lines[0].PriceItemDesc);

			AssertEquals("USR", lines[1].PriceItemCode);
			AssertEquals(100m, lines[1].Price);
			AssertEquals(55m, lines[1].UnitCount);
		}

		public void TestGenerateReport_UsersPerCountryVolumeBreak_PerCompanyBilling()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR1", "DB1");
			lic1.Database.LD_IsBilledPerCompany = true;
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "EUR");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "GE1");
			lic2.ClientCompany.LCC_RN_NKCountryCode = "FR";
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "EUR");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.ClientCompany.LCC_RN_NKCountryCode = "GE";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "EUR";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var priceUsr = priceHeader.Items.AddNew();
			priceUsr.L7_Category = BillingConstants.BillingSystem.STL;
			priceUsr.L7_Code = "USR";
			priceUsr.L7_Price = 100;
			priceUsr.L7_ChargeCode = "STLUSAGE";

			Action<int, int> createPriceItem = (unitBreak, price) =>
			{
				var priceItem = priceHeader.Items.AddNew();
				priceItem.L7_Category = BillingConstants.BillingSystem.STL;
				priceItem.L7_Code = "GPC";
				priceItem.L7_Price = price;
				priceItem.L7_Order = 1;
				priceItem.L7_UnitBreak = unitBreak;
				priceItem.L7_FeeType = BillingConstants.FeeType.UsersPerCountryVolumeBreak;
				priceItem.L7_ChargeCode = "STLUSAGE";
				priceItem.L7_Language = "";
				priceItem.L7_Description = $"Greater than {unitBreak} users";
			};

			createPriceItem(0, 0);
			createPriceItem(5, 170);
			createPriceItem(25, 425);
			createPriceItem(100, 855);

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "EUR";
			priceHeaderLink1.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", period1, lic1.ClientCompany, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", period1, lic2.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "GPC", period1, lic1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "GPC", period1, lic2.ClientCompany, 1);

			Factory.Save();
			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			AssertEquals("one bill per company", 2, billing1.Bills.Count);
			var bill1 = billing1.Bills.Cast<StlBill>().Single(x => x.LicCompany.PK == lic1.LA_LC);
			var bill2 = billing1.Bills.Cast<StlBill>().Single(x => x.LicCompany.PK == lic2.LA_LC);

			AssertEquals(1, bill1.MonthlyUsages.Count());
			var lines1 = bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode).ToArray();
			AssertEquals(2, lines1.Length);
			AssertEquals("GPC", lines1[0].PriceItemCode);
			AssertEquals(170m, lines1[0].Price);
			AssertEquals(1m, lines1[0].UnitCount);
			AssertEquals("Greater than 5 users", lines1[0].PriceItemDesc);

			AssertEquals("USR", lines1[1].PriceItemCode);
			AssertEquals(100m, lines1[1].Price);
			AssertEquals(13m, lines1[1].UnitCount);

			var lines2 = bill2.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode).ToArray();
			AssertEquals("GPC", lines1[0].PriceItemCode);
			AssertEquals(170m, lines1[0].Price);
			AssertEquals(1m, lines1[0].UnitCount);
			AssertEquals("Greater than 5 users", lines1[0].PriceItemDesc);

			AssertEquals(2, lines2.Length);
			AssertEquals("USR", lines2[1].PriceItemCode);
			AssertEquals(100m, lines2[1].Price);
			AssertEquals(15m, lines2[1].UnitCount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2017, 7, 5)]
		public void TestApplyPrepaymentDiscount()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company, "ISF");

			var discount = priceHeader.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Prepayment;
			discount.PHD_Name = "Prepay";
			discount.PHD_Percent = 10;
			discount.PHD_IsDefaultEnabled = true;

			var itemDiscount = priceHeader.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Prepay Only";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = priceHeader.Items.FindByCode("ISF");
			item1.L7_Category = BillingConstants.BillingSystem.ImporterSecurityFiling;
			item1.L7_PGM_DiscountGroupCode = "Prepay Only";

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = priceHeader.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = period1;

			var chargeForPeriod1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ImporterSecurityFiling, "ISF", period1, lic1, 100);

			var deposit = Factory.New<EdiDepositAdjust>();
			deposit.DEA_Amount = 15;
			deposit.DEA_ChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			deposit.DEA_GC = Env.CurrentCompany.PK;
			deposit.DEA_OH = lic1.Company.LC_OH;
			deposit.DEA_RX_NKCurrency = "AUD";
			deposit.DEA_Tax = deposit.DEA_Amount * 0.1m;
			lic1.Company.SelfBilling.L4_PredeterminedPrepaidBalance = 0m;
			lic1.Company.SelfBilling.L4_FuturePredeterminedPrepaidBalance = 0m;

			Factory.Save();

			Action<bool, decimal[]> assertPrepaymentDiscount = (applied, prepayNext) =>
			{
				var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
				billing.AccumulateMonths = 0;
				billing.DateTo = period1.AddMonths(1).AddDays(-1);
				billing.GenerateReport(null);
				var bill1 = billing.Bills[0];
				var invoice1 = bill1.CreateInvoicesWithoutSave().Single();

				if (prepayNext == null)
				{
					AssertNull(bill1.PrepayNext);
				}
				else
				{
					CombineAssertions(() =>
					{
						AssertEquals("0 - PrepaymentBalanceRequired", prepayNext[0], bill1.PrepayNext.PrepaymentBalanceRequired);
						AssertEquals("1 - CurrentPrepaymentBalance", prepayNext[1], bill1.PrepayNext.CurrentPrepaymentBalance);
						AssertEquals("2 - CurrentInvoiceTotalAmount", prepayNext[2], bill1.PrepayNext.CurrentInvoiceTotalAmount);
						AssertEquals("3 - FuturePrepaymentBalanceRequired", prepayNext[3], bill1.PrepayNext.FuturePrepaymentBalanceRequired);
						AssertEquals("4 - CurrentInvoiceTotalTaxAmount", prepayNext[4], bill1.PrepayNext.CurrentInvoiceTotalTaxAmount);
					});
				}

				if (applied)
				{
					AssertEquals(true, bill1.HasPrepaid);
					AssertEquals(100m, bill1.InvoicePreDiscountTotal);
					AssertEquals(90m, bill1.InvoicePostDiscountTotal);

					var lines = invoice1.Lines.Cast<InvoicingLineBase>();
					var fee1Charge = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
					var fee1Discount = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
					var fee2Charge = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDepositChargeCode.Value);

					AssertEquals(100m, fee1Charge.AL_LineAmount);
					AssertEquals(-10m, fee1Discount.AL_LineAmount);
					AssertEquals(-15m, fee2Charge.AL_LineAmount);
				}
				else
				{
					AssertEquals(false, bill1.HasPrepaid);
					AssertEquals(100m, bill1.InvoicePreDiscountTotal);
					AssertEquals(100m, bill1.InvoicePostDiscountTotal);

					var lines = invoice1.Lines.Cast<InvoicingLineBase>();
					var fee1Charge = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
					var fee2Charge = lines.Single(x => x.ChargeCode != null && x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDepositChargeCode.Value);

					AssertEquals(100m, fee1Charge.AL_LineAmount);
					AssertEquals(-15m, fee2Charge.AL_LineAmount);
				}
			};

			//default
			assertPrepaymentDiscount(false, new[] { 0m, 0m, 93.50m, 0m, 8.50m });

			// ManualOverride
			var newFactory = new BusinessObjectFactory();
			var discountLicenceSetting = newFactory.New<DiscountLicenceSetting>();
			discountLicenceSetting.LS9_IsActive = true;
			discountLicenceSetting.LS9_LD = lic1.Database.PK;
			discountLicenceSetting.LS9_Name = "PREPAY";
			discountLicenceSetting.LS9_IsManualOverride = true;
			newFactory.Save();
			assertPrepaymentDiscount(true, new[] { 0m, 0m, 82.50m, 0m, 7.50m });
			discountLicenceSetting.LS9_IsManualOverride = false;
			newFactory.Save();
			assertPrepaymentDiscount(false, new[] { 0m, 0m, 93.50m, 0m, 8.50m });

			//prepaid balance >= predetermined prepaid balance
			var clientLicenceBilling = newFactory.Load<ClientLicenceBilling>(lic1.Company.SelfBilling.PK);
			clientLicenceBilling.L4_PredeterminedPrepaidBalance = 100m;
			clientLicenceBilling.L4_RX_NKPredeterminedPrepaidBalanceCurrency = "AUD";
			clientLicenceBilling.L4_FuturePredeterminedPrepaidBalance = 200m;
			clientLicenceBilling.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = "AUD";
			EDIDataRegistry.Instance.PrepaidBalanceChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STLDEPOSIT");
			var prepaidBalanceChargeCode = EDIDataRegistry.Instance.PrepaidBalanceChargeCode.Value;
			BillingTestHelper.SetDepositBalance(lic1.Company.LC_OH.ToGuid(), prepaidBalanceChargeCode, 99.99m, 0, "AUD");
			newFactory.Save();
			assertPrepaymentDiscount(false, new[] { 100m, 99.99m, 93.50m, 200m, 8.50m });
			BillingTestHelper.SetDepositBalance(lic1.Company.LC_OH.ToGuid(), prepaidBalanceChargeCode, 100m, 0, "AUD");
			assertPrepaymentDiscount(true, new[] { 100m, 100m, 82.50m, 200m, 7.50m });
			BillingTestHelper.SetDepositBalance(lic1.Company.LC_OH.ToGuid(), prepaidBalanceChargeCode, 0m, 0, "AUD");
			assertPrepaymentDiscount(false, new[] { 100m, 0m, 93.50m, 200m, 8.50m });
			BillingTestHelper.SetDepositBalance(lic1.Company.LC_OH.ToGuid(), prepaidBalanceChargeCode, 100.01m, 0, "AUD");
			assertPrepaymentDiscount(true, new[] { 100m, 100.01m, 82.50m, 200m, 7.50m });
		}

		public void TestGenerateReport_DatabaseNotBilled()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			var delivery = licence.Company.InvoiceDeliveries.AddNew();
			delivery.L9_IsBilled = false;
			licence.Company.Header.CompanyData.OB_IsDebtor = true;
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_RX_NKCurrency = "AUD";
			var priceItem = prices.Items.AddNew();
			priceItem.L7_Category = BillingConstants.BillingSystem.STL;
			priceItem.L7_Code = "USR";
			priceItem.L7_Price = 100;
			priceItem.L7_ChargeCode = "STLUSAGE";

			var priceHeaderLink = licence.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), licence.ClientCompany, 10);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			var bill = billing.Bills[0];
			AssertEquals("3 - Not Billable", bill.Status);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_BorderWise()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);
			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			var discount1a = AddPercentageDiscount(stlPrices1, "Discount 1", 10m);
			var discount1b = AddPercentageDiscount(stlPrices1, "Discount 2", 15m);

			var stlPrices2 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices2.L6_DiscountCode = "STL2";
			stlPrices2.L6_PricelistVersion = "STL v10";
			var discount2a = AddPercentageDiscount(stlPrices2, "Discount 1", 20m);
			var discount2b = AddPercentageDiscount(stlPrices2, "Discount 2", 25m);
			AddDiscountStructure(stlPrices1, "Standard", discount1a, discount1b);
			AddDiscountStructure(stlPrices1, "BorderWise", discount1b);
			AddDiscountStructure(stlPrices2, "Standard", discount2a, discount2b);
			AddDiscountStructure(stlPrices2, "BorderWise", discount2b);

			foreach (ClientLicencePriceItem priceItem in borderWisePrices.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "BorderWise";
			}
			foreach (ClientLicencePriceItem priceItem in stlPrices1.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "Standard";
			}
			foreach (ClientLicencePriceItem priceItem in stlPrices2.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "Standard";
			}

			var companyWithoutDatabase = BillingTestHelper.CreateLicenceCompany(Factory, "EN1", "CO1");
			BillingTestHelper.SetInvoicing(companyWithoutDatabase.Header, Env.CurrentBranch.PK, "AUD");

			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW1");
			var licBOR2 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN3", "CO3", "BW2");
			var licBORNoCW = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN4", "CO4", "BW4");
			var licCW1 = BillingTestHelper.CreateAnotherDatabase(licBOR1, "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var licCW2 = BillingTestHelper.CreateAnotherDatabase(licBOR2, "CW2");
			licCW2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(licCW2, Env.CurrentBranch.PK, "USD");
			BillingTestHelper.SetInvoicing(licBORNoCW, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices1, periodStart);
			BillingTestHelper.CreatePriceLink(licCW2.Database, stlPrices2, periodStart, "USD");
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 2);
			BillingTestHelper.AddPriceItemRates(borderWisePrices, "USD", 2);
			BillingTestHelper.AddPriceItemRates(stlPrices1, "USD", 2);
			BillingTestHelper.AddPriceItemRates(stlPrices2, "USD", 2);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW2.ClientCompany, 75);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.UserPriceCode, periodStart, licBOR1.LA_LC, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.ExtraMachinePriceCode, periodStart, licBOR1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.UserPriceCode, periodStart, licBOR2.LA_LC, 17);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.ExtraMachinePriceCode, periodStart, licBOR2.LA_LC, 11);

			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.UserPriceCode, periodStart, companyWithoutDatabase.PK, 23);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.UserPriceCode, periodStart, licBORNoCW.LA_LC, 19);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			AssertEquals("STL1", stlPrices1.L6_DiscountCode);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			var billing2 = new StlBilling(Factory);
			billing2.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing2.OrganisationPK = companyWithoutDatabase.LC_OH;
			billing2.GenerateReport(null);

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
				AssertNoErrors("billCW1", bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", 1, bill.MonthlyUsages.First().PriceCurrencies.Count());
				AssertEquals("PriceCurrency", "AUD", bill.MonthlyUsages.First().PriceCurrencies.First());
				AssertEquals("billCW1.InvoicePreDiscountTotal", (13 * 200m + 7 * 30m + 50 * 1.00m), bill.InvoicePreDiscountTotal);
				AssertEquals("billCW1.InvoicePostDiscountTotal", (13 * 200m + 7 * 30m) * 0.85m + (50 * 1.00m) * 0.9m * 0.85m, bill.InvoicePostDiscountTotal);
			});

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW2.Company.LC_OH);
				AssertNoErrors("billCW2", bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", 1, bill.MonthlyUsages.First().PriceCurrencies.Count());
				AssertEquals("PriceCurrency", "USD", bill.MonthlyUsages.First().PriceCurrencies.First());
				AssertEquals("billCW2.InvoicePreDiscountTotal", (17 * 200m + 11 * 30m + 75 * 1.00m) * 2m, bill.InvoicePreDiscountTotal);
				AssertEquals("billCW2.InvoicePostDiscountTotal", ((17 * 200m + 11 * 30m) * 0.75m + (75 * 1.00m) * 0.8m * 0.75m) * 2m, bill.InvoicePostDiscountTotal);
			});

			CombineAssertions(() =>
			{
				var billNoCW = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licBORNoCW.Company.LC_OH);
				AssertNoErrors("billBOR1", billNoCW);
				AssertEquals("billBOR1.InvoicePreDiscountTotal", (19 * 200m), billNoCW.InvoicePreDiscountTotal);
				AssertEquals("billBOR1.InvoicePostDiscountTotal", (19 * 200m), billNoCW.InvoicePostDiscountTotal);
			});

			AssertEquals(4, billing.Bills.Count);
			AssertEquals(1, billing2.Bills.Count);

			CombineAssertions(() =>
			{
				var billNoDb1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == companyWithoutDatabase.LC_OH);
				var billNoDb2 = billing2.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == companyWithoutDatabase.LC_OH);
				AssertEquals("", billNoDb1.PrepayCurrency);
				AssertHasRowError("billNoCW1", billNoDb1, companyWithoutDatabase.Header.OH_Code + " missing BorderWise Licence Database");
				AssertHasRowError("billNoCW2", billNoDb2, companyWithoutDatabase.Header.OH_Code + " missing BorderWise Licence Database");
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_BorderWisePurchased()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdLicCompany);
			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");

			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW1");
			licBOR1.Database.LD_OH_BillingParty = licBOR1.Company.LC_OH;
			var licBOR2 = BillingTestHelper.CreateAnotherLicence(licBOR1, "CO3", false);
			var licCW1 = BillingTestHelper.CreateAnotherDatabase(licBOR1, "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicingTo(licBOR2, licBOR1);
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices1, periodStart);

			var code1 = BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode;
			var code2 = BillingConstants.BorderWise.AUProPackPriceCode;
			var price1 = borderWisePrices.Items.FindByCode(code1).L7_Price;
			var price2 = borderWisePrices.Items.FindByCode(code2).L7_Price;

			var purchaseSetting1 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting1.LS9_ValidFrom = periodStart;
			purchaseSetting1.LS9_LD = licBOR1.LA_LD;
			purchaseSetting1.LicenceCount = 4;
			purchaseSetting1.PriceCode = code1;
			licBOR1.Database.LicenceSettings.Add(purchaseSetting1);

			var purchaseSetting2 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting2.LS9_ValidFrom = periodStart;
			purchaseSetting2.LS9_LD = licBOR1.LA_LD;
			purchaseSetting2.LicenceCount = 5;
			purchaseSetting2.PriceCode = code2;

			var u1a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code1, periodStart, licBOR1.LA_LC, 7);
			var u2a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code1, periodStart, licBOR2.LA_LC, 5);
			var u3a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code2, periodStart, licBOR1.LA_LC, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);

			// next month they had less users than purchased licences
			var u4a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code1, periodStart.AddMonths(1), licBOR1.LA_LC, 1);
			var u5a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code1, periodStart.AddMonths(1), licBOR2.LA_LC, 2);
			var u6a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code2, periodStart.AddMonths(1), licBOR1.LA_LC, 4);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart.AddMonths(1), licCW1.ClientCompany, 50);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			var billing2 = new StlBilling(Factory);
			billing2.DateTo = periodStart.AddMonths(2).AddDays(-1);
			billing2.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			AssertEquals(1, billing2.Bills.Count);

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
				AssertNoErrors("billCW1", bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", 1, bill.MonthlyUsages.First().PriceCurrencies.Count());
				AssertEquals("PriceCurrency", "AUD", bill.MonthlyUsages.First().PriceCurrencies.First());
				var usersLine1 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == code1);
				var usersLine2 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == code2);
				AssertEquals("billCW1.InvoicePreDiscountTotal " + price1 + " " + price2, ((7 + 5 - 4) * price1 + ((11 - 5) * price2) + 50 * 1.00m), bill.InvoicePreDiscountTotal);
				AssertEquals("billCW1.InvoicePostDiscountTotal", bill.InvoicePreDiscountTotal, bill.InvoicePostDiscountTotal);
				AssertEquals("usersLine1.TotalUnitCount", 7m + 5m, usersLine1.TotalUnitCount);
				AssertEquals("usersLine1.IncludedUnitCount", 4, usersLine1.IncludedUnitCount);
				AssertEquals("usersLine2.TotalUnitCount", 11m, usersLine2.TotalUnitCount);
				AssertEquals("usersLine2.IncludedUnitCount", 5, usersLine2.IncludedUnitCount);
			});

			CombineAssertions(() =>
			{
				var bill = billing2.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
				AssertNoErrors("billCW1", bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", 1, bill.MonthlyUsages.First().PriceCurrencies.Count());
				AssertEquals("PriceCurrency", "AUD", bill.MonthlyUsages.First().PriceCurrencies.First());
				var usersLine1 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == code1);
				var usersLine2 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == code2);
				AssertEquals("billCW1.InvoicePreDiscountTotal", 50 * 1.00m, bill.InvoicePreDiscountTotal);
				AssertEquals("billCW1.InvoicePostDiscountTotal", bill.InvoicePreDiscountTotal, bill.InvoicePostDiscountTotal);
				AssertEquals("usersLine1.TotalUnitCount", 1m + 2m, usersLine1.TotalUnitCount);
				AssertEquals("usersLine1.IncludedUnitCount", 4, usersLine1.IncludedUnitCount);
				AssertEquals("usersLine2.TotalUnitCount", 4m, usersLine2.TotalUnitCount);
				AssertEquals("usersLine2.IncludedUnitCount", 5, usersLine2.IncludedUnitCount);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_BorderWisePurchasedGroup()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var globalUserCode = BillingConstants.BorderWise.GlobalStandalonePriceCode;
			var globalProPackCode = BillingConstants.BorderWise.GlobalProPackPriceCode;
			var countryUserCode1 = BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode;
			var countryUserCode2 = BillingConstants.BorderWise.AUSingleWindowPartnerCW1PriceCode;
			var countryProPackCode = BillingConstants.BorderWise.AUProPackPriceCode;

			BillingTestHelper.CreateExchangeRate(Factory, "USD", 2);

			var priceCodeGroup = new CodeDescriptionPairList();
			priceCodeGroup.AddPair("GALL", string.Join(", ", globalUserCode, countryUserCode1, countryUserCode2));
			priceCodeGroup.AddPair("GPRO", string.Join(", ", globalProPackCode, countryProPackCode));
			EDIDataRegistry.Instance.BorderWisePurchasedGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, priceCodeGroup);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdLicCompany);
			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");

			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW1");
			licBOR1.Database.LD_OH_BillingParty = licBOR1.Company.LC_OH;
			var licBOR2 = BillingTestHelper.CreateAnotherLicence(licBOR1, "CO3", false);
			var licCW1 = BillingTestHelper.CreateAnotherDatabase(licBOR1, "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicingTo(licBOR2, licBOR1);
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices1, periodStart);

			var purchaseSetting1 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting1.LS9_ValidFrom = periodStart;
			purchaseSetting1.LS9_LD = licBOR1.LA_LD;
			purchaseSetting1.LicenceCount = 20;
			purchaseSetting1.PriceCode = "GALL";
			licBOR1.Database.LicenceSettings.Add(purchaseSetting1);

			var purchaseSetting2 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting2.LS9_ValidFrom = periodStart;
			purchaseSetting2.LS9_LD = licBOR1.LA_LD;
			purchaseSetting2.LicenceCount = 10;
			purchaseSetting2.PriceCode = "GPRO";

			var purchaseSetting3 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting3.LS9_ValidFrom = periodStart;
			purchaseSetting3.LS9_LD = licBOR1.LA_LD;
			purchaseSetting3.LicenceCount = 1;
			purchaseSetting3.PriceCode = countryUserCode2;

			// 12 global users, 6 country user code1, 4 country user code2
			// 7 global pro-packs, 5 country user pro packs
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalUserCode, periodStart, licBOR1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalUserCode, periodStart, licBOR2.LA_LC, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode1, periodStart, licBOR1.LA_LC, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode1, periodStart, licBOR2.LA_LC, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode2, periodStart, licBOR1.LA_LC, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode2, periodStart, licBOR2.LA_LC, 2);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalProPackCode, periodStart, licBOR1.LA_LC, 4);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalProPackCode, periodStart, licBOR2.LA_LC, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryProPackCode, periodStart, licBOR2.LA_LC, 5);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);

			// next month global users consume all licences
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalUserCode, periodStart.AddMonths(1), licBOR1.LA_LC, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalUserCode, periodStart.AddMonths(1), licBOR2.LA_LC, 6);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode2, periodStart.AddMonths(1), licBOR1.LA_LC, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart.AddMonths(1), licCW1.ClientCompany, 50);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			var billing2 = new StlBilling(Factory);
			billing2.DateTo = periodStart.AddMonths(2).AddDays(-1);
			billing2.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			AssertEquals(1, billing2.Bills.Count);

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
				AssertNoErrors("billCW1", bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", "AUD, USD", string.Join(", ", bill.MonthlyUsages.First().PriceCurrencies.OrderBy(x => x)));
				var usersLine1 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == globalUserCode);
				AssertEquals("globalUserCode IncludedUnitCount", 12, usersLine1.IncludedUnitCount);
				AssertEquals("globalUserCode UnitCount", 0, (int)usersLine1.UnitCount);

				var usersLine2 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == countryUserCode1);
				AssertEquals("countryUserCode1 IncludedUnitCount", 6, usersLine2.IncludedUnitCount);
				AssertEquals("countryUserCode1 UnitCount", 0, (int)usersLine2.UnitCount);

				var usersLine3 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == countryUserCode2);
				AssertEquals("countryUserCode2 IncludedUnitCount", 3, usersLine3.IncludedUnitCount);
				AssertEquals("countryUserCode2 UnitCount", 1, (int)usersLine3.UnitCount);

				var proPackLine1 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == globalProPackCode);
				AssertEquals("globalProPackCode IncludedUnitCount", 7, proPackLine1.IncludedUnitCount);
				AssertEquals("globalProPackCode UnitCount", 0, (int)proPackLine1.UnitCount);

				var proPackLine2 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == countryProPackCode);
				AssertEquals("countryProPackCode IncludedUnitCount", 3, proPackLine2.IncludedUnitCount);
				AssertEquals("countryProPackCode UnitCount", 2, (int)proPackLine2.UnitCount);
			});

			CombineAssertions(() =>
			{
				var bill = billing2.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
				AssertNoErrors("billCW1", bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", "AUD, USD", string.Join(", ", bill.MonthlyUsages.First().PriceCurrencies.OrderBy(x => x)));
				var usersLine1 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == globalUserCode);
				AssertEquals("globalUserCode IncludedUnitCount", 20, usersLine1.IncludedUnitCount);
				AssertEquals("globalUserCode UnitCount", 1, (int)usersLine1.UnitCount);

				var usersLine3 = bill.MonthlyUsages.First().UsageLines.First(x => x.PriceItemCode == countryUserCode2);
				AssertEquals("countryUserCode2 IncludedUnitCount", 1, usersLine3.IncludedUnitCount);
				AssertEquals("countryUserCode2 UnitCount", 1, (int)usersLine3.UnitCount);
			});
		}

		public void TestCreateInvoiceAndDetailedUsage_BorderWise()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdLicCompany);
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			var discount1 = stlPrices.StlDiscounts.AddNew();
			discount1.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount1.PHD_Name = "Discount 1";
			discount1.PHD_Percent = 10;
			discount1.PHD_IsDefaultEnabled = true;
			discount1.PHD_Version = "STL1";

			var itemDiscount = stlPrices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount1.PK;
			foreach (ClientLicencePriceItem priceItem in borderWisePrices.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "Standard";
			}
			foreach (ClientLicencePriceItem priceItem in stlPrices.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "Standard";
			}

			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1");
			var licBOR2 = BillingTestHelper.CreateAnotherLicence(licBOR1, "CO2", false);
			var licCW1 = BillingTestHelper.CreateAnotherDatabase(licBOR1, "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(licBOR1, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicing(licBOR2, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicingTo(licBOR2, licBOR1);
			stlPrices.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices, periodStart);

			var code1 = BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode;
			var code2 = BillingConstants.BorderWise.AUProPackPriceCode;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", code1, periodStart, licBOR1.LA_LC, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", code1, periodStart, licBOR2.LA_LC, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", code2, periodStart, licBOR1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", code2, periodStart, licBOR2.LA_LC, 3);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			AssertEquals("STL1", stlPrices.L6_DiscountCode);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			var priceItem1 = borderWisePrices.Items.FindByCode(code1);
			var priceItem2 = borderWisePrices.Items.FindByCode(code2);
			var price1 = priceItem1.L7_Price;
			var price2 = priceItem2.L7_Price;
			Assert(price1 != 0m);
			Assert(price2 != 0m);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill1", bill1);
				AssertEquals("bill1.InvoicePreDiscountTotal", ((13 + 11) * price1 + ((7 + 3) * price2) + 50 * 1.00m), bill1.InvoicePreDiscountTotal);
				AssertEquals("bill1.InvoicePostDiscountTotal", bill1.InvoicePreDiscountTotal * 0.9m, bill1.InvoicePostDiscountTotal);
			});

			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();

			var allBilledUsages = Factory.Load<EdiBilledUsage>(new ZQuery());
			AssertEquals(5, allBilledUsages.Length);
			CombineAssertions(() =>
			{
				var billedUsageBW11 = allBilledUsages.Single(x => x.BU9_PriceCode == code1 && x.BU9_LC == licBOR1.LA_LC);
				AssertEquals("BillingModel", "BOR", billedUsageBW11.BU9_BillingModel);
				AssertEquals("database", licBOR1.LA_LD, billedUsageBW11.BU9_LD);
				AssertEquals("UsageCode", "BOR", billedUsageBW11.BU9_UsageCode);
				AssertEquals("UsageSubCode", code1, billedUsageBW11.BU9_UsageSubCode);
				AssertEquals("UnitCount", 13m, billedUsageBW11.BU9_UnitCount);
				AssertEquals("BU9_L7", priceItem1.PK, billedUsageBW11.BU9_L7);
				AssertEquals("BU9_TransactionAmountPreDiscount", 13 * price1, billedUsageBW11.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_UnitPrice", price1, billedUsageBW11.BU9_UnitPrice);
			});

			CombineAssertions(() =>
			{
				var billedUsageBW12 = allBilledUsages.Single(x => x.BU9_PriceCode == code1 && x.BU9_LC == licBOR2.LA_LC);
				AssertEquals("BillingModel", "BOR", billedUsageBW12.BU9_BillingModel);
				AssertEquals("database", licBOR2.LA_LD, billedUsageBW12.BU9_LD);
				AssertEquals("UsageCode", "BOR", billedUsageBW12.BU9_UsageCode);
				AssertEquals("UsageSubCode", code1, billedUsageBW12.BU9_UsageSubCode);
				AssertEquals("UnitCount", 11m, billedUsageBW12.BU9_UnitCount);
				AssertEquals("BU9_L7", priceItem1.PK, billedUsageBW12.BU9_L7);
				AssertEquals("BU9_TransactionAmountPreDiscount", 11 * price1, billedUsageBW12.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_UnitPrice", price1, billedUsageBW12.BU9_UnitPrice);
			});

			CombineAssertions(() =>
			{
				var billedUsageBW21 = allBilledUsages.Single(x => x.BU9_PriceCode == code2 && x.BU9_LC == licBOR1.LA_LC);
				AssertEquals("BillingModel", "BOR", billedUsageBW21.BU9_BillingModel);
				AssertEquals("database", licBOR1.LA_LD, billedUsageBW21.BU9_LD);
				AssertEquals("UsageCode", "BOR", billedUsageBW21.BU9_UsageCode);
				AssertEquals("UsageSubCode", code2, billedUsageBW21.BU9_UsageSubCode);
				AssertEquals("UnitCount", 7m, billedUsageBW21.BU9_UnitCount);
				AssertEquals("BU9_L7", priceItem2.PK, billedUsageBW21.BU9_L7);
				AssertEquals("BU9_TransactionAmountPreDiscount", 7 * price2, billedUsageBW21.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_UnitPrice", price2, billedUsageBW21.BU9_UnitPrice);
			});

			CombineAssertions(() =>
			{
				var billedUsageBW22 = allBilledUsages.Single(x => x.BU9_PriceCode == code2 && x.BU9_LC == licBOR2.LA_LC);
				AssertEquals("BillingModel", "BOR", billedUsageBW22.BU9_BillingModel);
				AssertEquals("database", licBOR1.LA_LD, billedUsageBW22.BU9_LD);
				AssertEquals("UsageCode", "BOR", billedUsageBW22.BU9_UsageCode);
				AssertEquals("UsageSubCode", code2, billedUsageBW22.BU9_UsageSubCode);
				AssertEquals("UnitCount", 3m, billedUsageBW22.BU9_UnitCount);
				AssertEquals("BU9_L7", priceItem2.PK, billedUsageBW22.BU9_L7);
				AssertEquals("BU9_TransactionAmountPreDiscount", 3 * price2, billedUsageBW22.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_UnitPrice", price2, billedUsageBW22.BU9_UnitPrice);
			});

			CombineAssertions(() =>
			{
				var billedUsageUSR = allBilledUsages.Single(x => x.BU9_PriceCode == "USR");
				AssertEquals("BillingModel", "STL", billedUsageUSR.BU9_BillingModel);
				AssertEquals("database", licCW1.LA_LD, billedUsageUSR.BU9_LD);
			});

			var stlDetailLines = LoadStlUsageLinesForTest(licCW1.Company, periodStart);
			var odmDetailLines = LoadOdplUsageLinesForTest(BillingConstants.BillingSystemList, licCW1.Company.Header, periodStart);
			AssertEquals(1, stlDetailLines.Count);
			AssertEquals(2, odmDetailLines.Count);
			{
				var detailUSR = stlDetailLines.First();
				AssertEquals("CO1", detailUSR.CompanyCode);
				AssertEquals("CW1", detailUSR.DatabaseServerCode);
				AssertEquals("Item USR", detailUSR.PriceItemDescription);
				AssertEquals(licCW1.ClientCompany.PK, detailUSR.ClientCompanyPK);
			}
			{
				var detailBOR1 = odmDetailLines.Single(x => x.LicenceCompanyPK == licBOR1.LA_LC);
				AssertEquals("CO1", detailBOR1.CompanyCode);
				AssertEquals(ZGuid.Empty, detailBOR1.ClientCompanyPK);
				AssertEquals("", detailBOR1.DatabaseServerCode);
				AssertEquals("BOR", detailBOR1.SystemCode);
			}
			{
				var detailBOR2 = odmDetailLines.Single(x => x.LicenceCompanyPK == licBOR2.LA_LC);
				AssertEquals("CO2", detailBOR2.CompanyCode);
				AssertEquals(ZGuid.Empty, detailBOR2.ClientCompanyPK);
				AssertEquals("", detailBOR2.DatabaseServerCode);
				AssertEquals("BOR", detailBOR2.SystemCode);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_ClientChargeableUsageHasConcurrencyCheck()
		{
			Factory.RefreshEnabled = false;
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");

			var licCW1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "CW1");
			var licCW2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			stlPrices.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices, periodStart);

			var usage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill1", bill1);
				AssertEquals("bill1.InvoicePreDiscountTotal", 50 * 1.00m, bill1.InvoicePreDiscountTotal);
			});

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var anotherInvoice = BillingTestHelper.CreateInvoice(factory2, licCW2, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, 10, 1, "", 0);
			var usageInFactory2 = factory2.Load<ClientChargeableUsage>(usage.PK);
			usageInFactory2.U1_AH_Invoice = anotherInvoice.PK;
			factory2.Save();

			AssertExceptionThrown<ZSaveConcurrencyException>(() => bill1.CreateInvoices(ZDateTime.Empty));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_BillingSTLCurrencyWhenPriceZero_CurrencyFromHeader()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "P00", "P01");
			stlPrices1.L6_RX_NKCurrency = "USD";
			stlPrices1.Items[0].L7_Price = 1.1m;
			stlPrices1.Items[1].L7_Price = 0m;
			stlPrices1.Items[2].L7_Price = 1.5m;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "CW1");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices1, periodStart, "AUD");

			BillingTestHelper.AddPriceItemRate(stlPrices1.Items[0], "AUD", 2.1m);
			BillingTestHelper.AddPriceItemRate(stlPrices1.Items[2], "AUD", 2.5m);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P00", periodStart, lic1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P01", periodStart, lic1.ClientCompany, 1);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
				AssertNoErrors(bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", 1, bill.MonthlyUsages.First().PriceCurrencies.Count());
				AssertEquals("PriceCurrency", "AUD", bill.MonthlyUsages.First().PriceCurrencies.First());
				AssertEquals(2.1m + 2.5m, bill.InvoicePreDiscountTotal);

				var lines = bill.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode == "USR" ? 0 : (x.PriceItemCode == "P00" ? 1 : 2)).ToArray();
				AssertEquals(3, lines.Length);

				AssertEquals("AUD", lines[0].PriceCurrency);
				AssertEquals(2.1m, lines[0].Price);
				AssertEquals("USR", lines[0].PriceItemCode);

				AssertEquals("AUD", lines[1].PriceCurrency);
				AssertEquals(0m, lines[1].Price);
				AssertEquals("P00", lines[1].PriceItemCode);

				AssertEquals("AUD", lines[2].PriceCurrency);
				AssertEquals(2.5m, lines[2].Price);
				AssertEquals("P01", lines[2].PriceItemCode);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_BillingSTLCurrencyWhenPriceZero_CurrencyFromItem()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "P00");
			stlPrices1.L6_RX_NKCurrency = "USD";
			stlPrices1.Items[0].L7_Price = 1.1m;
			stlPrices1.Items[1].L7_Price = 0m;
			stlPrices1.Items[1].L7_RX_NKCurrency = "USD";
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 2);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "CW1");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices1, periodStart, "AUD");

			BillingTestHelper.AddPriceItemRate(stlPrices1.Items[0], "AUD", 2.1m);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P00", periodStart, lic1.ClientCompany, 13);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
				AssertNoErrors(bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", "AUD, USD", string.Join(", ", bill.MonthlyUsages.First().PriceCurrencies.OrderBy(x => x)));
				AssertEquals(7 * 2.1m, bill.InvoicePreDiscountTotal);

				var lines = bill.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode == "USR" ? 0 : (x.PriceItemCode == "P00" ? 1 : 2)).ToArray();
				AssertEquals(2, lines.Length);

				AssertEquals("AUD", lines[0].PriceCurrency);
				AssertEquals(2.1m, lines[0].Price);
				AssertEquals("USR", lines[0].PriceItemCode);

				AssertEquals("USD", lines[1].PriceCurrency);
				AssertEquals(0m, lines[1].Price);
				AssertEquals("P00", lines[1].PriceItemCode);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPriceHeaderExchangeRate()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var chargeCode = BillingTestHelper.CreateChargeCode(Factory, null, "STLUSAGE");
			var prices = lic.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "USD";
			prices.L6_PricelistVersion = "STL V100";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_HasExchangeRates = true;
			prices.L6_Rounding = "V1";
			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "P01";
			item1.L7_Price = 12.5m;
			item1.L7_LicenceUnits = 125m;
			item1.L7_ChargeCode = "STLUSAGE";
			var item2 = prices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.STL;
			item2.L7_Code = "USR";
			item2.L7_Price = 1.35m;
			item2.L7_LicenceUnits = 135m;
			item2.L7_ChargeCode = "STLUSAGE";

			var discount1 = prices.StlDiscounts.AddNew();
			discount1.PHD_Type = "VOL";
			var group1 = prices.StlItemDiscounts.AddNew();
			group1.PGM_GroupCode = "G1";
			group1.PGM_PHD = discount1.PK;
			item1.L7_PGM_DiscountGroupCode = item2.L7_PGM_DiscountGroupCode = "G1";

			var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "P01", new ZDateTime(2015, 7, 1), lic.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2015, 7, 1), lic.ClientCompany, 25);
			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory());
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var monthlyUsage = billing.Bills[0].MonthlyUsages.First();
			AssertHasRowError(monthlyUsage, "Price list STL V100 is missing exchange rate for AUD, group .");

			item1.L7_ExchangeRateGroupCode = "STL";
			item2.L7_ExchangeRateGroupCode = "STL";
			var rate = prices.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "AUD";
			rate.PHE_Rate = 1.3m;
			rate.PHE_UpliftPercent = 15m;
			priceHeaderLink.PHL_CorePackCode = "UP";
			priceHeaderLink.PHL_VolumeCode = "LV";
			priceHeaderLink.PHL_VolumePercent = 25m;
			Factory.Save();

			billing = new StlBilling(new BusinessObjectFactory());
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			monthlyUsage = billing.Bills[0].MonthlyUsages.First();
			AssertNoErrors(monthlyUsage);
			var lines = monthlyUsage.UsageLines.OrderBy(x => x.PriceItemCode).ToArray();
			AssertEquals(2, lines.Length);
			AssertEquals("P01", lines[0].PriceItemCode);
			AssertEquals(4.67m, lines[0].Price);
			AssertEquals("AUD", lines[0].PriceCurrency);
			AssertEquals(31.3m, lines[0].LicenceUnits);
			AssertEquals("USR", lines[1].PriceItemCode);
			AssertEquals(0.76m, lines[1].Price);
			AssertEquals("AUD", lines[1].PriceCurrency);
			AssertEquals(50.6m, lines[1].LicenceUnits);

			#region High Volume Feature

			var item3 = prices.Items.AddNew();
			item3.L7_Category = BillingConstants.BillingSystem.STL;
			item3.L7_Code = "HV0";
			item3.L7_Price = 11.25m;
			item3.L7_LicenceUnits = 112m;
			item3.L7_ChargeCode = "STLUSAGE";
			item3.L7_ExchangeRateGroupCode = "STL";
			item3.L7_IsVolumeAdjustmentEligible = false;
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "HV0", new ZDateTime(2015, 7, 1), lic.ClientCompany, 50);
			priceHeaderLink.PHL_VolumeCode = "HV";
			var hvfSetting = Factory.New<HighVolumeFeatureSetting>();
			hvfSetting.LS9_LD = lic.LA_LD;
			hvfSetting.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "HV0");
			lic.Database.LicenceSettings.Add(hvfSetting);

			Factory.Save();

			billing = new StlBilling(new BusinessObjectFactory());
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			monthlyUsage = billing.Bills[0].MonthlyUsages.First();
			AssertNoErrors(monthlyUsage);
			var linesAsText = string.Join("\r\n", monthlyUsage.UsageLines.OrderBy(x => x.PriceItemCode)
				.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.LicenceUnits}"));
			AssertEquals(
@"HV0 | 8.4 | AUD | 56.0
P01 | 18.7 | AUD | 125.0
USR | 3.03 | AUD | 202.5", linesAsText);

			#endregion
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_BillingFixedCurrencyFromItem()
		{
			var periodStart = BillingTestHelper.MonthToday;
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.50); // 1 AUD = 0.50 USD

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "3GT", "#NP", "STL");
			stlPrices1.L6_RX_NKCurrency = "USD";
			stlPrices1.L6_HasExchangeRates = true;
			stlPrices1.L6_TestDbPriceCode = "#NP";
			stlPrices1.L6_Rounding = "V1";
			stlPrices1.L6_ValidFrom = periodStart.AddMonths(-12);
			var rate = stlPrices1.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "AUD";
			rate.PHE_Rate = 1.0m;
			rate.PHE_UpliftPercent = 0m;
			stlPrices1.Items.FindByCode("#NP").L7_Category = BillingConstants.BillingSystem.Service;
			var itemAnyCurrency = stlPrices1.Items.FindByCode("USR");
			itemAnyCurrency.L7_ExchangeRateGroupCode = rate.PHE_GroupCode;
			var itemFixedCurrency = stlPrices1.Items.FindByCode("3GT");
			itemFixedCurrency.L7_Category = "3GT";
			itemFixedCurrency.L7_RX_NKCurrency = "USD";
			itemFixedCurrency.L7_Price = 3m;
			stlPrices1.Validation.ValidateAll();
			AssertNoErrors(stlPrices1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "CW1");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices1, periodStart, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "3GT", "3GT", periodStart, lic1.ClientCompany, 13);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
				AssertNoErrors(bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", "AUD, USD", string.Join(", ", bill.MonthlyUsages.First().PriceCurrencies.OrderBy(x => x)));
				AssertEquals("InvoicePreDiscountTotal", 7 * 1m + 13 * 3m * 2m, bill.InvoicePreDiscountTotal);

				var lines = bill.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode == "USR" ? 0 : (x.PriceItemCode == "3GT" ? 1 : 2)).ToArray();
				AssertEquals("lines.Length", 2, lines.Length);

				AssertEquals("lines[0].PriceCurrency", "AUD", lines[0].PriceCurrency);
				AssertEquals("lines[0].Price", 1m, lines[0].Price);
				AssertEquals("lines[0].PriceItemCode", "USR", lines[0].PriceItemCode);

				AssertEquals("lines[1].PriceCurrency", "USD", lines[1].PriceCurrency);
				AssertEquals("lines[1].Price", 3m, lines[1].Price);
				AssertEquals("lines[1].PriceItemCode", "3GT", lines[1].PriceItemCode);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_BillingFixedCurrencyForMinimumFeeItem()
		{
			var periodStart = BillingTestHelper.MonthToday;
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.50); // 1 AUD = 0.50 USD

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "#CS", "CES", "#NP");
			stlPrices1.L6_RX_NKCurrency = "USD";
			stlPrices1.L6_HasExchangeRates = true;
			stlPrices1.L6_TestDbPriceCode = "#NP";
			stlPrices1.L6_Rounding = "V1";
			stlPrices1.L6_ValidFrom = periodStart.AddMonths(-12);
			var rate = stlPrices1.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "AUD";
			rate.PHE_Rate = 1.0m;
			rate.PHE_UpliftPercent = 0m;
			stlPrices1.Items.FindByCode("#NP").L7_Category = BillingConstants.BillingSystem.Service;
			var itemAnyCurrency = stlPrices1.Items.FindByCode("USR");
			itemAnyCurrency.L7_ExchangeRateGroupCode = rate.PHE_GroupCode;
			var itemMinFeeFixedCurrency = stlPrices1.Items.FindByCode("#CS");
			itemMinFeeFixedCurrency.L7_RX_NKCurrency = "USD";
			itemMinFeeFixedCurrency.L7_Price = 100m;
			itemMinFeeFixedCurrency.L7_FeeType = BillingConstants.FeeType.MinimumFeePerReference;
			var itemFixedCurrency = stlPrices1.Items.FindByCode("CES");
			itemFixedCurrency.L7_RX_NKCurrency = "USD";
			itemFixedCurrency.L7_Price = 7m;
			itemFixedCurrency.L7_FeeType = BillingConstants.FeeType.Transactional;
			itemFixedCurrency.L7_ParentCode = "#CS";
			itemFixedCurrency.L7_ParentCategory = "STL";
			stlPrices1.Validation.ValidateAll();
			AssertNoErrors(stlPrices1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "CW1");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices1, periodStart, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 7);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "CES", periodStart, lic1.ClientCompany, 2);
			usage2.U1_Reference1 = "Ref 1";
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "CES", periodStart, lic1.ClientCompany, 3);
			usage2.U1_Reference1 = "Ref 2";

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
				AssertNoErrors(bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", "AUD, USD", string.Join(", ", bill.MonthlyUsages.First().PriceCurrencies.OrderBy(x => x)));
				AssertEquals("InvoicePreDiscountTotal", 7 * 1m + 100 * 2m, bill.InvoicePreDiscountTotal);

				var lines = bill.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode == "USR" ? 0 : (x.PriceItemCode == "#CS" ? 1 : 2)).ToArray();
				AssertEquals("lines.Length", 3, lines.Length);

				AssertEquals("lines[0].PriceCurrency", "AUD", lines[0].PriceCurrency);
				AssertEquals("lines[0].Price", 1m, lines[0].Price);
				AssertEquals("lines[0].PriceItemCode", "USR", lines[0].PriceItemCode);

				AssertEquals("lines[1].PriceCurrency", "USD", lines[1].PriceCurrency);
				AssertEquals("lines[1].Price", 65m, lines[1].Price);
				AssertEquals("lines[1].PriceItemCode", "#CS", lines[1].PriceItemCode);

				AssertEquals("lines[2].PriceCurrency", "USD", lines[2].PriceCurrency);
				AssertEquals("lines[2].Price", 7m, lines[2].Price);
				AssertEquals("lines[2].PriceItemCode", "CES", lines[2].PriceItemCode);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHighVolumeAdjustmentWithVolumeScale()
		{
			var periodStart = new ZDateTime(2019, 1, 1);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;

			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_RX_NKPriceCurrency = "AUD";
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_HasExchangeRates = true;
			prices.L6_Rounding = "V1";
			prices.L6_RX_NKCurrency = "USD";
			prices.L6_PricelistVersion = "STL V100";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;

			var rateAUD = prices.ExchangeRates.AddNew();
			rateAUD.PHE_GroupCode = "STL";
			rateAUD.PHE_RX_NKCurrency = "AUD";
			rateAUD.PHE_Rate = 1.4m;
			rateAUD.PHE_UpliftPercent = 0m;

			var item1 = prices.Items.AddNew();
			item1.L7_Code = "USR";
			item1.L7_Price = 4;
			item1.L7_LicenceUnits = 40;
			item1.L7_Order = 1;
			item1.L7_ExchangeRateGroupCode = "STL";

			var volumeItem1a = prices.Items.AddNew();
			volumeItem1a.L7_Code = "WOD";
			volumeItem1a.L7_ExchangeRateGroupCode = "STL";
			volumeItem1a.L7_Price = 1.5m;
			volumeItem1a.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var volumeItem1b = prices.Items.AddNew();
			volumeItem1b.L7_Code = "WOD";
			volumeItem1b.L7_UnitBreak = 1000;
			volumeItem1b.L7_ExchangeRateGroupCode = "STL";
			volumeItem1b.L7_Price = 1.2m;
			volumeItem1b.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;

			var volumeItem2a = prices.Items.AddNew();
			volumeItem2a.L7_Code = "ZZZ";
			volumeItem2a.L7_ExchangeRateGroupCode = "STL";
			volumeItem2a.L7_Price = 10m;
			volumeItem2a.L7_FeeType = BillingConstants.FeeType.Transactional;

			var volumeItem2b = prices.Items.AddNew();
			volumeItem2b.L7_Code = "ZZZ";
			volumeItem2b.L7_UnitBreak = 10;
			volumeItem2b.L7_ExchangeRateGroupCode = "STL";
			volumeItem2b.L7_Price = 8m;
			volumeItem2b.L7_FeeType = BillingConstants.FeeType.Transactional;

			var volumeItem2c = prices.Items.AddNew();
			volumeItem2c.L7_Code = "ZZZ";
			volumeItem2c.L7_UnitBreak = 50;
			volumeItem2c.L7_ExchangeRateGroupCode = "STL";
			volumeItem2c.L7_Price = 5m;
			volumeItem2c.L7_FeeType = BillingConstants.FeeType.Transactional;

			foreach (var item in prices.Items)
			{
				item.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
				item.L7_Category = BillingConstants.BillingSystem.STL;
			}

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);
			priceHeaderLink.PHL_CorePackCode = EdiPriceHeaderLinkCorePackCodeList.Codes.INC;
			priceHeaderLink.PHL_VolumeCode = EdiPriceHeaderLinkVolumeCodeList.Codes.HV;
			priceHeaderLink.PHL_VolumePercent = 50m;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "WOD", periodStart, lic1.ClientCompany, 4000);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "ZZZ", periodStart, lic1.ClientCompany, 70);

			var highVolSettingWOD = Factory.New<HighVolumeFeatureSetting>();
			highVolSettingWOD.LS9_LD = lic1.LA_LD;
			highVolSettingWOD.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "WOD");

			lic1.Database.LicenceSettings.Add(highVolSettingWOD);

			var highVolSettingZZZ = Factory.New<HighVolumeFeatureSetting>();
			highVolSettingZZZ.LS9_LD = lic1.LA_LD;
			highVolSettingZZZ.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "ZZZ");
			lic1.Database.LicenceSettings.Add(highVolSettingZZZ);

			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory());
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			AssertNoErrors(bill1);
			AssertEquals("AUD", bill1.InvoiceCurrencyCode);

			var linesAsText = string.Join("\r\n", bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode).ThenByDescending(x => x.Price)
				.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {(int)x.UnitCount}"));

			// WOD price = 1.2m * 1.4m * 0.5m = 0.85
			// ZZZ base price = 10m * 1.4m * 0.5m = 7
			AssertEquals(
@"USR | 5.6 | AUD | 5
WOD | 0.84 | AUD | 4000
ZZZ | 7.0 | AUD | 10
ZZZ | 5.6 | AUD | 40
ZZZ | 3.50 | AUD | 20", linesAsText);
		}

		public void TestSurcharge()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "C03";
			item1.L7_Price = 40;
			item1.L7_Order = 1;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_Language = "";

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C03", period1, lic1.ClientCompany, 3);

			lic1.Company.SelfBilling.L4_ProcessingFee = "MPF";
			lic1.Company.SelfBilling.L4_ProcessingFeePercent = 5m;

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertEquals(6m, bill1.InvoiceSurchargeTotal);
			AssertEquals(5m, bill1.SurchargePercent);
			AssertEquals("Manual Processing Fee", bill1.SurchargeDescription.ToString());
			AssertEquals(120m, bill1.InvoicePreDiscountTotal);
			AssertEquals(126m, bill1.InvoicePostDiscountTotal);
		}

		public void TestGenerateReport_CustomPriceSetting()
		{
			var period1 = BillingTestHelper.MonthToday;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company, "USR", "WOD", "WOD");
			BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, period1);

			var itemUSR = priceHeader.Items[0];
			itemUSR.L7_FeeType = BillingConstants.FeeType.Transactional;
			itemUSR.L7_Price = 40m;
			itemUSR.L7_LicenceUnits = 400m;

			var itemWOD1 = priceHeader.Items[1];
			itemWOD1.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			itemWOD1.L7_Price = 1m;
			itemWOD1.L7_LicenceUnits = 10m;
			var itemWOD2 = priceHeader.Items[2];
			itemWOD2.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			itemWOD2.L7_UnitBreak = 999;
			itemWOD2.L7_Price = 0.5m;
			itemWOD2.L7_LicenceUnits = 5m;

			var priceSettingUSR = Factory.New<PriceLicenceSetting>();
			priceSettingUSR.LS9_LD = lic1.LA_LD;
			priceSettingUSR.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "USR");
			priceSettingUSR.LS9_Price = 25m;
			priceSettingUSR.LS9_Units = 250m;
			priceSettingUSR.LS9_RX_NKPriceCurrency = "AUD";
			lic1.Database.LicenceSettings.Add(priceSettingUSR);

			var priceSettingWOD = Factory.New<PriceLicenceSetting>();
			priceSettingWOD.LS9_LD = lic1.LA_LD;
			priceSettingWOD.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "WOD");
			priceSettingWOD.LS9_Price = 0.75;
			priceSettingWOD.EnableUnits = false;
			priceSettingWOD.LS9_RX_NKPriceCurrency = "AUD";
			lic1.Database.LicenceSettings.Add(priceSettingWOD);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "WOD", period1, lic1.ClientCompany, 1500);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertNoNotifications(bill1);
			var monthlyUsage = bill1.MonthlyUsages.First();
			var usageWOD = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "WOD");
			var usageUSR = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "USR");

			AssertEquals(0.75m, usageWOD.Price);
			AssertEquals(10m, usageWOD.LicenceUnits);

			AssertEquals(25m, usageUSR.Price);
			AssertEquals(250m, usageUSR.LicenceUnits);

			AssertEquals(1500m * 0.75m + 15 * 25m, bill1.InvoicePreDiscountTotal);
			AssertEquals(1500m * 10m + 15 * 250, bill1.MonthlyUsages.First().TotalUsedLicenceUnits);
		}

		public void TestGenerateReport_PriceTierLicenceSetting()
		{
			var period1 = BillingTestHelper.MonthToday;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company, "USR", "WOD", "WOD", "WOL", "WOL");
			BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, period1);

			var itemUSR = priceHeader.Items[0];
			itemUSR.L7_FeeType = BillingConstants.FeeType.Transactional;
			itemUSR.L7_Price = 40m;
			itemUSR.L7_LicenceUnits = 400m;

			var itemWOD1 = priceHeader.Items[1];
			itemWOD1.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			itemWOD1.L7_Price = 1m;
			itemWOD1.L7_LicenceUnits = 10m;
			var itemWOD2 = priceHeader.Items[2];
			itemWOD2.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			itemWOD2.L7_UnitBreak = 999;
			itemWOD2.L7_Price = 0.5m;
			itemWOD2.L7_LicenceUnits = 5m;

			var itemWOL1 = priceHeader.Items[3];
			itemWOL1.L7_FeeType = BillingConstants.FeeType.Transactional;
			itemWOL1.L7_Price = 2m;
			itemWOL1.L7_LicenceUnits = 20m;
			var itemWOL2 = priceHeader.Items[4];
			itemWOL2.L7_FeeType = BillingConstants.FeeType.Transactional;
			itemWOL2.L7_UnitBreak = 100;
			itemWOL2.L7_Price = 1m;
			itemWOL2.L7_LicenceUnits = 10m;

			var priceSettingUSR = Factory.New<PriceLicenceSetting>();
			priceSettingUSR.LS9_LD = lic1.LA_LD;
			priceSettingUSR.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "USR");
			priceSettingUSR.LS9_Price = 25m;
			priceSettingUSR.LS9_Units = 250m;
			priceSettingUSR.LS9_RX_NKPriceCurrency = "AUD";
			lic1.Database.LicenceSettings.Add(priceSettingUSR);

			var priceSettingWOD = Factory.New<PriceTierLicenceSetting>();
			priceSettingWOD.LS9_LD = lic1.LA_LD;
			priceSettingWOD.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "WOD");
			priceSettingWOD.EnableUnits = true;
			priceSettingWOD.LS9_RX_NKPriceCurrency = "AUD";
			var line1WOD = priceSettingWOD.Lines.AddNew();
			line1WOD.UnitBreak = 999999;
			line1WOD.Price = 0.01m;
			line1WOD.Units = 0.1m;
			lic1.Database.LicenceSettings.Add(priceSettingWOD);

			var priceSettingWOL = Factory.New<PriceTierLicenceSetting>();
			priceSettingWOL.LS9_LD = lic1.LA_LD;
			priceSettingWOL.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "WOL");
			priceSettingWOL.EnableUnits = true;
			priceSettingWOL.LS9_RX_NKPriceCurrency = "AUD";
			lic1.Database.LicenceSettings.Add(priceSettingWOL);
			var line1WOL = priceSettingWOL.Lines.AddNew();
			line1WOL.UnitBreak = 0;
			line1WOL.Price = 0.9m;
			line1WOL.Units = 9m;
			var line2WOL = priceSettingWOL.Lines.AddNew();
			line2WOL.UnitBreak = 100;
			line2WOL.Price = 0.8m;
			line2WOL.Units = 8m;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "WOD", period1, lic1.ClientCompany, 1500);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "WOL", period1, lic1.ClientCompany, 210);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertHasRowError(bill1.MonthlyUsages.Single(), "Price (Tier) Setting is missing for Price WOD, Unit Break 999");

			var line2WOD = priceSettingWOD.Lines.AddNew();
			line2WOD.UnitBreak = 0;
			line2WOD.Price = 0.3m;
			line2WOD.Units = 3m;
			var line3WOD = priceSettingWOD.Lines.AddNew();
			line3WOD.UnitBreak = 999;
			line3WOD.Price = 0.2m;
			line3WOD.Units = 2m;
			Factory.Save();

			billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			bill1 = billing1.Bills[0];
			AssertNoNotifications(bill1);
			var monthlyUsage = bill1.MonthlyUsages.First();
			var usageWOD = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "WOD");
			var usagesWOL = monthlyUsage.UsageLines.Where(x => x.PriceItemCode == "WOL").OrderBy(x => x.Price).ToArray();
			var usageUSR = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "USR");

			AssertEquals(0.2m, usageWOD.Price);
			AssertEquals(2m, usageWOD.LicenceUnits);

			AssertEquals(2, usagesWOL.Length);
			AssertEquals(110m, usagesWOL[0].UnitCount);
			AssertEquals(0.8m, usagesWOL[0].Price);
			AssertEquals(8m, usagesWOL[0].LicenceUnits);
			AssertEquals(100m, usagesWOL[1].UnitCount);
			AssertEquals(0.9m, usagesWOL[1].Price);
			AssertEquals(9m, usagesWOL[1].LicenceUnits);

			AssertEquals(25m, usageUSR.Price);
			AssertEquals(250m, usageUSR.LicenceUnits);

			AssertEquals(1500m * 0.2m + 15 * 25m + 110m * 0.8m + 100m * 0.9m, bill1.InvoicePreDiscountTotal);
			AssertEquals(1500m * 2m + 15 * 250 + 110m * 8m + 100m * 9m, bill1.MonthlyUsages.First().TotalUsedLicenceUnits);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_CustomPriceSettingUniversalPricelistCurrency()
		{
			BillingTestHelper.CreateExchangeRate(Factory, "EUR", 0.50); // 1 AUD = 0.50 EUR

			// Populate standard pricelist
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var abmPriceList = stdLicCompany.PriceHeaders.AddNew();
			abmPriceList.L6_PricelistVersion = "V1";
			abmPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.ABMCustoms;
			abmPriceList.L6_RX_NKCurrency = "EUR";
			abmPriceList.L6_ValidFrom = new ZDateTime(2016, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var item1 = abmPriceList.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.ABMCustoms;
			item1.L7_Code = "CTM";
			item1.L7_Price = 1.00m;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = chargeCode;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_RX_NKCurrency = "EUR";

			var item2 = abmPriceList.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.ABMCustoms;
			item2.L7_Code = "POC";
			item2.L7_Price = 0.33m;
			item2.L7_Order = 2;
			item2.L7_ChargeCode = chargeCode;
			item2.L7_FeeType = BillingConstants.FeeType.Transactional;

			var item3 = abmPriceList.Items.AddNew();
			item3.L7_Category = BillingConstants.BillingSystem.ABMCustoms;
			item3.L7_Code = "FRP";
			item3.L7_Price = 1.00m;
			item3.L7_Order = 3;
			item3.L7_ChargeCode = chargeCode;
			item3.L7_FeeType = BillingConstants.FeeType.Transactional;

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "USD";
			BillingTestHelper.CreatePriceLink(licence.Database, prices, new ZDateTime(2016, 3, 1));

			BillingTestHelper.CreateChargeableUsage(Factory, "ABM", "CTM", new ZDateTime(2016, 3, 1), licence.ClientCompany, 100);
			BillingTestHelper.CreateChargeableUsage(Factory, "ABM", "POC", new ZDateTime(2016, 3, 1), licence.ClientCompany, 200);
			BillingTestHelper.CreateChargeableUsage(Factory, "ABM", "FRP", new ZDateTime(2016, 3, 1), licence.ClientCompany, 300);

			var priceSettingCTM = Factory.New<PriceLicenceSetting>();
			priceSettingCTM.LS9_LD = licence.LA_LD;
			priceSettingCTM.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.ABMCustoms, "CTM");
			priceSettingCTM.LS9_Price = 10m;
			licence.Database.LicenceSettings.Add(priceSettingCTM);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2016, 3, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			var usageLinesAsString = string.Join("\r\n", bill.MonthlyUsages.Single().UsageLines.Select(x => $"{x.PriceItemCategory}-{x.PriceItemCode}-{x.PriceCurrency}-{x.Price}")
				.OrderBy(x => x));
			AssertEquals(@"ABM-CTM-EUR-10
ABM-FRP-EUR-1.00
ABM-POC-EUR-0.33", usageLinesAsString);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal", (100 * 10.00m + 200 * 0.33m + 300 * 1.00m) / 0.50m, bill.InvoicePreDiscountTotal);
			});
		}

		public void TestGenerateReport_CustomPriceSetting_ApplyDiscounts()
		{
			var period1 = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "WOD");
			stlPrices.L6_DiscountCode = "STL1";
			stlPrices.L6_UseStandardDiscount = false;
			var discount = stlPrices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Special";
			discount.PHD_Percent = 50;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";
			var itemDiscount = stlPrices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount.PK;

			var itemUSR = stlPrices.Items[0];
			itemUSR.L7_FeeType = BillingConstants.FeeType.Transactional;
			itemUSR.L7_Price = 40m;
			itemUSR.L7_PGM_DiscountGroupCode = "Standard";

			var itemWOD1 = stlPrices.Items[1];
			itemWOD1.L7_FeeType = BillingConstants.FeeType.Transactional;
			itemWOD1.L7_Price = 100m;
			itemWOD1.L7_PGM_DiscountGroupCode = "Standard";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, period1);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceSettingUSR = Factory.New<PriceLicenceSetting>();
			priceSettingUSR.LS9_LD = lic1.LA_LD;
			priceSettingUSR.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "USR");
			priceSettingUSR.LS9_Price = 25m;
			priceSettingUSR.LS9_RX_NKPriceCurrency = "AUD";
			lic1.Database.LicenceSettings.Add(priceSettingUSR);

			var priceSettingWOD = Factory.New<PriceLicenceSetting>();
			priceSettingWOD.LS9_LD = lic1.LA_LD;
			priceSettingWOD.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "WOD");
			priceSettingWOD.LS9_Price = 50;
			priceSettingWOD.LS9_ApplyDiscounts = false;
			priceSettingWOD.LS9_RX_NKPriceCurrency = "AUD";
			lic1.Database.LicenceSettings.Add(priceSettingWOD);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "WOD", period1, lic1.ClientCompany, 20);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertNoNotifications(bill1);
			var monthlyUsage = bill1.MonthlyUsages.First();
			var usageWOD = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "WOD");
			var usageUSR = monthlyUsage.UsageLines.Single(x => x.PriceItemCode == "USR");

			AssertEquals(25m, usageUSR.Price);
			AssertEquals(10 * 25m, usageUSR.PreDiscountAmount);
			AssertEquals("discounts applied", 12.5m, usageUSR.DiscountedPrice);
			AssertEquals("discounts applied", 10 * 12.5m, usageUSR.PostDiscountAmount);

			AssertEquals(50m, usageWOD.Price);
			AssertEquals("discounts not applied", 50m, usageWOD.DiscountedPrice);
			AssertEquals("discounts not applied", usageWOD.PreDiscountAmount, usageWOD.PreDiscountAmount);

			AssertEquals(10m * 25m + 20 * 50m, bill1.InvoicePreDiscountTotal);
			AssertEquals(10m * 12.5m + 20 * 50m, bill1.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_GoldenTax()
		{
			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = BillingConstants.Category.GoldenTax;
			usageCodes1.PriceItemCode = "GTS";
			usageCodes1.PriceHeaderCode = BillingConstants.PriceHeaderType.GoldenTax;
			usageCodes1.KeyRefIndex1 = 1;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var goldenTaxPriceList = stdLicCompany.PriceHeaders.AddNew();
			goldenTaxPriceList.L6_PricelistVersion = "GoldenTax V1";
			goldenTaxPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.GoldenTax;
			goldenTaxPriceList.L6_RX_NKCurrency = "AUD";
			goldenTaxPriceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var item1 = goldenTaxPriceList.Items.AddNew();
			item1.L7_Category = BillingConstants.Category.GoldenTax;
			item1.L7_Code = "GTS";
			item1.L7_Price = 1.00m;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = chargeCode;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_UnitBreak = 500;

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.CreatePriceLink(licence.Database, prices, new ZDateTime(2018, 1, 1));

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "GTS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 800);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "GTS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 600);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, "ACC", "GTS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 300);

			usage1.U1_Reference1 = "Machine#1";
			usage2.U1_Reference1 = "Machine#2";
			usage3.U1_Reference1 = "Machine#3";

			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.DateTo = new ZDateTime(2018, 1, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal", (800m - 500m + 600m - 500m + 0m), bill.InvoicePreDiscountTotal);
			});

			// STL pricing
			var goldenTaxOnStlPriceList = BillingTestHelper.AddPriceItem(prices, new UsageCodeKey("ACC", "GTS"), "TRA", 0.5m);
			goldenTaxOnStlPriceList.L7_Description = "  New Per Additional Invoice";
			goldenTaxOnStlPriceList.L7_ChargeCode = chargeCode;
			goldenTaxOnStlPriceList.L7_UnitBreak = 500;
			Factory.Save();
			billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.DateTo = new ZDateTime(2018, 1, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal", (800m - 500m + 600m - 500m + 0m) * 0.5m, bill.InvoicePreDiscountTotal);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_LastMonthInvoiceAmount()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic1.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "USR";
			item1.L7_Price = 4;
			item1.L7_LicenceUnits = 40;
			item1.L7_Order = 1;
			item1.L7_ParentCategory = BillingConstants.BillingSystem.STL;
			item1.L7_ParentCode = "#MF";

			var minimumFeeItem = prices.Items.AddNew();
			minimumFeeItem.L7_Category = BillingConstants.BillingSystem.STL;
			minimumFeeItem.L7_Code = "#MF";
			minimumFeeItem.L7_Price = 150;
			minimumFeeItem.L7_LicenceUnits = 0;
			minimumFeeItem.L7_Order = 3;
			minimumFeeItem.L7_FeeType = BillingConstants.FeeType.MinimumFee;

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			minimumFeeItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2018, 1, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2018, 1, 1), lic1.ClientCompany, 100);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2018, 2, 1), lic1.ClientCompany, 200);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2018, 3, 1), lic1.ClientCompany, 300);
			Factory.Save();

			var billing1 = new StlBilling(Factory);
			billing1.DateTo = new ZDateTime(2018, 1, 31);
			billing1.GenerateReport(null);
			var invoice1 = billing1.Bills.OfType<StlBill>().Single().CreateInvoices(new ZDateTime(2018, 2, 1)).Single();
			AssertNotNull(invoice1);

			var billing2 = new StlBilling(Factory);
			billing2.DateTo = new ZDateTime(2018, 2, 28);
			billing2.GenerateReport(null);
			var invoice2 = billing2.Bills.OfType<StlBill>().Single().CreateInvoices(new ZDateTime(2018, 3, 1)).Single();
			AssertNotNull(invoice2);

			AssertEquals(true, invoice1.IsInDatabase);
			AssertEquals(true, invoice2.IsInDatabase);

			AssertEquals(400m, invoice1.AH_OSExTaxAmount);
			AssertEquals(440m, invoice1.AH_OSTotal);
			AssertEquals("00001000", invoice1.AH_TransactionNum);
			AssertEquals(800m, invoice2.AH_OSExTaxAmount);
			AssertEquals("00001001", invoice2.AH_TransactionNum);
			AssertEquals(880m, invoice2.AH_OSTotal);

			billing2 = new StlBilling(Factory);
			billing2.DateTo = new ZDateTime(2018, 2, 28);
			billing2.GenerateReport(null);
			var bill = billing2.Bills.OfType<StlBill>().Single();
			AssertEquals(invoice1.AH_OSExTaxAmount, bill.LastMonthInvoiceAmount);
			AssertEquals(invoice1.AH_TransactionNum, bill.LastMonthInvoiceNumbers);
			AssertEquals(invoice1.AH_OSExTaxAmount, bill.InvoicesForLastMonth.Single().AH_OSExTaxAmount);
			AssertEquals(invoice1.AH_TransactionNum, bill.InvoicesForLastMonth.Single().AH_TransactionNum);
			AssertEquals(invoice1.PK, bill.InvoicesForLastMonth.Single().PK);

			AssertEquals(bill.InvoiceNumbers, invoice2.AH_TransactionNum);
			AssertEquals(bill.InvoicePksForThisMonth.Single(), invoice2.PK);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_FlightStats_TRA()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var flightStatsPriceList = stdLicCompany.PriceHeaders.AddNew();
			flightStatsPriceList.L6_PricelistVersion = "FlightStats V1";
			flightStatsPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.FlightStats;
			flightStatsPriceList.L6_RX_NKCurrency = "AUD";
			flightStatsPriceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var item1 = flightStatsPriceList.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.FlightStats;
			item1.L7_Code = "FMS";
			item1.L7_Price = 1.00m;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = chargeCode;
			item1.L7_FeeType = BillingConstants.FeeType.Transactional;
			item1.L7_UnitBreak = 100;

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.CreatePriceLink(licence.Database, prices, new ZDateTime(2018, 1, 1));

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 800);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 600);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, "FMS", "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 300);
			usage1.U1_Reference1 = "Job#1";
			usage2.U1_Reference1 = "Job#2";
			usage3.U1_Reference1 = "Job#3";

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2018, 1, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal", (800m + 600m + 300m - 100m), bill.InvoicePreDiscountTotal);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_FlightStats_TRB()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var flightStatsPriceList = stdLicCompany.PriceHeaders.AddNew();
			flightStatsPriceList.L6_PricelistVersion = "FlightStats V1";
			flightStatsPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.FlightStats;
			flightStatsPriceList.L6_RX_NKCurrency = "AUD";
			flightStatsPriceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var chargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			for (var idx = 1; idx < 100; idx++)
			{
				var item = flightStatsPriceList.Items.AddNew();
				item.L7_Category = BillingConstants.BillingSystem.FlightStats;
				item.L7_Code = "FMS";
				item.L7_Price = 1.00m * idx;
				item.L7_Order = (ZShort)idx;
				item.L7_ChargeCode = chargeCode;
				item.L7_UnitBreak = 100 * idx;
				item.L7_FeeType = BillingConstants.FeeType.TransactionalOneVolumeBreak;
			}

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.CreatePriceLink(licence.Database, prices, new ZDateTime(2018, 1, 1));

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.FlightStats, "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 800);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.FlightStats, "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 600);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.FlightStats, "FMS", new ZDateTime(2018, 1, 1), licence.ClientCompany, 300);
			usage1.U1_Reference1 = "Job#1";
			usage2.U1_Reference1 = "Job#2";
			usage3.U1_Reference1 = "Job#3";

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2018, 1, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal", (800m + 600m + 300m) * 16m, bill.InvoicePreDiscountTotal);
			});
		}

		public void TestGenerateReport_HOS()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "DB2");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic2.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var itemHD = priceHeader.Items.AddNew();
			itemHD.L7_Category = BillingConstants.BillingSystem.HostingStorage;
			itemHD.L7_Code = "#HD";
			itemHD.L7_Price = 40;
			itemHD.L7_Order = 1;
			itemHD.L7_FeeType = BillingConstants.FeeType.Per10GBPerMonthMin1GB;
			itemHD.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			itemHD.L7_Language = "";

			var itemHE = priceHeader.Items.AddNew();
			itemHE.L7_Category = BillingConstants.BillingSystem.HostingStorage;
			itemHE.L7_Code = "#HE";
			itemHE.L7_Price = 50;
			itemHE.L7_Order = 2;
			itemHE.L7_FeeType = BillingConstants.FeeType.Per10GBPerMonthMin1GB;
			itemHE.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			itemHE.L7_Language = "";

			var itemHA = priceHeader.Items.AddNew();
			itemHA.L7_Category = BillingConstants.BillingSystem.HostingStorage;
			itemHA.L7_Code = "#HA";
			itemHA.L7_Price = 100;
			itemHA.L7_Order = 3;
			itemHA.L7_FeeType = BillingConstants.FeeType.Per10GBPerMonthMin1GB;
			itemHA.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			itemHA.L7_Language = "";

			var itemRG = priceHeader.Items.AddNew();
			itemRG.L7_Category = BillingConstants.BillingSystem.HostingStorage;
			itemRG.L7_Code = "#RG";
			itemRG.L7_Price = 25;
			itemRG.L7_Order = 4;
			itemRG.L7_FeeType = BillingConstants.FeeType.PerGBPerMonthMin1GB;
			itemRG.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			itemRG.L7_Language = "";

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = period1;

			var priceHeaderLink2 = lic2.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink2.PHL_L6 = priceHeader.PK;
			priceHeaderLink2.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink2.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, "#HD", period1, lic1.ClientCompany, 1024 * 100);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, "#HE", period1, lic1.ClientCompany, 1024 * 50);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, "#HA", period1, lic1.ClientCompany, 1024 * 17500);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, "#RG", period1, lic1.ClientCompany, 32);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, "#HA", period1, lic2.ClientCompany, 1024 * 102);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, "#RG", period1, lic2.ClientCompany, 1); //first 1gb is free here.

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills.OfType<StlBill>().Single();

			var logs = new List<string>();
			foreach (var stlMonthlyUsage in bill1.MonthlyUsages)
			{
				foreach (var usage in stlMonthlyUsage.UsageLines)
				{
					logs.Add($"LD:{stlMonthlyUsage.Database.LD_ServerCode} - PriceItemCode:{usage.PriceItemCode} - UnitCount:{usage.UnitCount} - Amount:{usage.LocalPostDiscountAmount}");
				}
			}

			var usagesAsText = @"LD:DB1 - PriceItemCode:#HA - UnitCount:2100 - Amount:210000.00
LD:DB1 - PriceItemCode:#HD - UnitCount:12 - Amount:480.00
LD:DB1 - PriceItemCode:#HE - UnitCount:6 - Amount:300.00
LD:DB1 - PriceItemCode:#RG - UnitCount:31 - Amount:775.00
LD:DB2 - PriceItemCode:#HA - UnitCount:13 - Amount:1300.00";
			AssertEquals(usagesAsText, string.Join("\r\n", logs.OrderBy(x => x)));
			AssertEquals(212855m, bill1.InvoicePreDiscountTotal);
		}

		public void TestGenerateReport_ProductivityWise()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			lic1.Database.LD_Product = ProductTypes.Codes.ProductivityWise;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = period1;
			priceHeader.L6_UseStandardDiscount = false;

			var item = priceHeader.Items.AddNew();
			item.L7_Category = BillingConstants.BillingSystem.STL;
			item.L7_Code = "C01";
			item.L7_Price = 40;
			item.L7_Order = 1;
			item.L7_FeeType = BillingConstants.FeeType.Transactional;
			item.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = period1;

			var usage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "C01", period1, lic1.ClientCompany, 50);

			Factory.Save();
			AssertEquals("PRW", usage.Database.LD_Product);

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];

			var usagesAsText = "C01-50-2000.00";
			AssertEquals(usagesAsText, string.Join("\r\n", bill1.MonthlyUsages.Single().UsageLines.Select(x => $"{x.PriceItemCode}-{x.UnitCount}-{x.LocalPostDiscountAmount}").OrderBy(x => x)));
			AssertEquals(2000.00m, bill1.InvoicePreDiscountTotal);
		}

		public void TestGenerateReport_MinimumFeePerReference_PerCompanyBilling()
		{
			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			const string TestUsageCategory = "CSC";
			usageCodes1.Category = TestUsageCategory;
			usageCodes1.PriceItemCode = "CES";
			usageCodes1.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			usageCodes1.KeyRefIndex1 = 1;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var stdLicence = BillingTestHelper.CreateLicence(Factory, "EDI", "EDI", "SYD", false);
			var stdPriceCompany = stdLicence.Company;

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "CEM", "CES", "CES", "CES", "CES");
			var cesPrices = stlPrices.Items.FindAllByCode("CES").ToList();
			foreach (var item in cesPrices)
			{
				item.L7_Category = TestUsageCategory;
				item.L7_ParentCategory = BillingConstants.BillingSystem.STL;
				item.L7_ParentCode = "CEM";
			}
			var upTo50Price = cesPrices[0];
			var upTo100Price = cesPrices[1];
			var upTo250Price = cesPrices[2];
			var over250Price = cesPrices[3];
			upTo50Price.L7_Price = 50;
			upTo100Price.L7_UnitBreak = 50;
			upTo100Price.L7_Price = 40;
			upTo250Price.L7_UnitBreak = 100;
			upTo250Price.L7_Price = 30;
			over250Price.L7_UnitBreak = 250;
			over250Price.L7_Price = 15;
			var minFeePrice = stlPrices.Items.FindByCode("CEM");
			minFeePrice.L7_Price = 100;
			minFeePrice.L7_FeeType = BillingConstants.FeeType.MinimumFee;

			var period1 = BillingTestHelper.MonthToday;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, period1.AddYears(-1));
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;
			lic1.Database.LD_IsBilledPerCompany = true;
			lic1.LA_RX_NKPriceCurrency = "AUD";

			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "CO2");
			lic2.ClientCompany.LCC_OH = lic2.Company.LC_OH;
			lic2.LA_RX_NKPriceCurrency = "AUD";
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranchPK, "AUD");

			// Lic1 has 120 x Carrier A,  90 x Carrier B
			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic1.ClientCompany, 120);
			usage1.U1_Reference1 = "Carrier A";
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic1.ClientCompany, 90);
			usage2.U1_Reference1 = "Carrier B";
			var usrUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", period1, lic1.ClientCompany, 10);

			// Lic2 has  40 x Carrier A, 110 x Carrier B
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic2.ClientCompany, 40);
			usage3.U1_Reference1 = "Carrier A";
			var usage4 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic2.ClientCompany, 110);
			usage4.U1_Reference1 = "Carrier B";

			Factory.Save();

			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			AssertEquals(2, billing1.Bills.Count);
			var bills = billing1.Bills.Cast<StlBill>().ToList();
			{
				var bill1 = bills.Single(x => x.Organisation.PK == lic1.Company.LC_OH);
				AssertNoErrors(bill1);
				AssertEquals(10m + (50 * 50m + 50 * 40m + 20 * 30m) + (50 * 50m + 40 * 40m), bill1.InvoicePreDiscountTotal);
			}
			{
				var bill2 = bills.Single(x => x.Organisation.PK == lic2.Company.LC_OH);
				AssertNoErrors(bill2);
				AssertEquals((40 * 50m) + (50 * 50m + 50 * 40m + 10 * 30m), bill2.InvoicePreDiscountTotal);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNonProductionServiceRebilledCorrectly()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var periodStart = new ZDateTime(2019, 3, 1);
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA", "CO1", "SYD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var lic2 = BillingTestHelper.CreateLicence(Factory, "AAA", "CO2", "MEL");
			lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company, "USR", "#NP");
			priceHeader.L6_TestDbPriceCode = "#NP";
			priceHeader.Items.FindByCode("#NP").L7_Category = BillingConstants.BillingSystem.Service;
			priceHeader.L6_HasExchangeRates = true;
			var rate = priceHeader.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "AUD";
			rate.PHE_Rate = 1.0m;
			rate.PHE_UpliftPercent = 0m;
			foreach (var priceItem in priceHeader.Items)
			{
				priceItem.L7_ExchangeRateGroupCode = "STL";
			}

			var priceLink1 = BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, periodStart.AddYears(-1));
			var priceLink2 = BillingTestHelper.CreatePriceLink(lic2.Database, priceHeader, periodStart.AddYears(-1));
			priceLink2.PHL_CorePackCode = EdiPriceHeaderLinkCorePackCodeList.Codes.INC;
			priceLink2.PHL_VolumeCode = EdiPriceHeaderLinkVolumeCodeList.Codes.HV;
			priceLink2.PHL_VolumePercent = 25m;

			var priceSetting1 = Factory.New<PriceLicenceSetting>();
			priceSetting1.LS9_LD = lic1.LA_LD;
			priceSetting1.PriceKey = new UsageCodeKey("STL", "USR");
			priceSetting1.LS9_Price = 2;
			priceSetting1.LS9_ValidFrom = periodStart;

			var highVolumeSetting = Factory.New<HighVolumeFeatureSetting>();
			highVolumeSetting.LS9_LD = lic2.LA_LD;
			highVolumeSetting.PriceKey = new UsageCodeKey("STL", "USR");
			highVolumeSetting.LS9_ValidFrom = periodStart;

			var testLic = BillingTestHelper.CreateAnotherDatabase(lic1, "TST");
			testLic.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			testLic.Database.LD_LD_ParentDatabase = lic1.Database.PK;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2, 5);

			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(2, billing.Bills.Count);

			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			var bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
			AssertNoNotifications(bill1);
			AssertNoNotifications(bill2);
			AssertEquals(7 * 2m + 1m, bill1.InvoicePreDiscountTotal);
			AssertEquals(5 * 1m * 0.25m, bill2.InvoicePreDiscountTotal);
			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();
			var invoice2 = bill2.CreateInvoices(ZDateTime.Empty).Single();

			var testLicUsages = Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_Parent, testLic.LA_LD));
			AssertEquals(1, testLicUsages.Length);

			// Reverse invoices so we can bill again
			invoice1.GenerateReverseTransaction(true);
			invoice1.Factory.Save();
			invoice2.GenerateReverseTransaction(true);
			invoice2.Factory.Save();

			// Change billing of test system
			testLic.Database.LD_LD_ParentDatabase = lic2.Database.PK;
			Factory.Save();

			billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(2, billing.Bills.Count);
			bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
			bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
			AssertNoNotifications(bill1);
			AssertNoNotifications(bill2);
			AssertEquals(7 * 2m, bill1.InvoicePreDiscountTotal);
			AssertEquals(5 * 1m * 0.25m + 1m, bill2.InvoicePreDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCorePackExcludedHasNoLicenceUnits()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var chargeCode = BillingTestHelper.CreateChargeCode(Factory, null, "STLUSAGE");
			var prices = lic.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "USD";
			prices.L6_PricelistVersion = "STL V100";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			prices.L6_HasExchangeRates = true;
			prices.L6_Rounding = "V1";
			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = "P01";
			item1.L7_Price = 40m;
			item1.L7_LicenceUnits = 400m;
			item1.L7_ChargeCode = "STLUSAGE";
			var item2 = prices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.STL;
			item2.L7_Code = "USR";
			item2.L7_Price = 3m;
			item2.L7_LicenceUnits = 30m;
			item2.L7_ChargeCode = "STLUSAGE";

			var discount1 = prices.StlDiscounts.AddNew();
			discount1.PHD_Type = "VOL";
			var volumeDiscount = ((VolumeDiscount)discount1.Config);
			volumeDiscount.Lines.RemoveAndDeleteAll();
			var line1 = volumeDiscount.Lines.AddNew();
			line1.UnitCount = 400 * 15 + 1;
			line1.Percent = 20m;
			var line2 = volumeDiscount.Lines.AddNew();
			line2.UnitCount = 400 * 15 + 10;
			line2.Percent = 32m;

			var group1 = prices.StlItemDiscounts.AddNew();
			group1.PGM_GroupCode = "G1";
			group1.PGM_PHD = discount1.PK;
			item1.L7_PGM_DiscountGroupCode = item2.L7_PGM_DiscountGroupCode = "G1";

			var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink.PHL_L6 = prices.PK;
			priceHeaderLink.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);
			item1.L7_ExchangeRateGroupCode = "STL";
			item2.L7_ExchangeRateGroupCode = "STL";
			var rate = prices.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "STL";
			rate.PHE_RX_NKCurrency = "AUD";
			rate.PHE_Rate = 1.0m;
			rate.PHE_UpliftPercent = 0m;
			priceHeaderLink.PHL_CorePackCode = EdiPriceHeaderLinkCorePackCodeList.Codes.EX;
			priceHeaderLink.PHL_VolumeCode = EdiPriceHeaderLinkVolumeCodeList.Codes.STD;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "P01", new ZDateTime(2015, 7, 1), lic.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2015, 7, 1), lic.ClientCompany, 25);
			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory());
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			var monthlyUsage = bill.MonthlyUsages.First();
			AssertNoErrors(monthlyUsage);
			var lines = monthlyUsage.UsageLines.OrderBy(x => x.PriceItemCode).ToArray();
			AssertEquals(2, lines.Length);
			AssertEquals("P01", lines[0].PriceItemCode);
			AssertEquals(40m, lines[0].Price);
			AssertEquals("AUD", lines[0].PriceCurrency);
			AssertEquals(400m, lines[0].LicenceUnits);
			AssertEquals("USR", lines[1].PriceItemCode);
			AssertEquals(3m, lines[1].Price);
			AssertEquals("AUD", lines[1].PriceCurrency);
			AssertEquals(0m, lines[1].LicenceUnits);
			AssertEquals(400m * 15, monthlyUsage.TotalLicenceUnits);
			AssertEquals(40m * 15 + 3m * 25, bill.InvoicePreDiscountTotal);
			AssertEquals(40m * 15 * 0.8m + 3m * 25, bill.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 8, 1)]
		public void TestGenerateReport_SingleCountryDiscount()
		{
			var discountTypes = new CodeDescriptionBoolCollection(EdiPriceHeaderDiscount.Schema.PHD_NameMaxLength);
			discountTypes.Add("TWCUST", (NoResString)"Taiwan Customs");
			EDIDataRegistry.Instance.StlDiscountTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, discountTypes);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "CUS");
			stlPrices.L6_DiscountCode = "STL1";
			stlPrices.L6_UseStandardDiscount = false;

			var taiwanDiscount = stlPrices.StlDiscounts.AddNew();
			taiwanDiscount.PHD_Type = BillingConstants.DiscountCalculator.SingleCountry;
			taiwanDiscount.PHD_Name = "TWCUST";
			taiwanDiscount.PHD_Percent = 50;
			taiwanDiscount.PHD_IsDefaultEnabled = false;
			taiwanDiscount.PHD_Version = "STL1";
			var taiwanDiscountConfig = (SingleCountryDiscount)taiwanDiscount.Config;
			taiwanDiscountConfig.Country = Core.Constants.CountryCodes.Taiwan;

			var customsDiscount = stlPrices.StlItemDiscounts.AddNew();
			customsDiscount.PGM_GroupCode = "Customs";
			customsDiscount.PGM_PHD = taiwanDiscount.PK;

			var customsPrice = stlPrices.Items.FindByCode("CUS");
			customsPrice.L7_Price = 100m;
			customsPrice.L7_LicenceUnits = 1000m;
			customsPrice.L7_PGM_DiscountGroupCode = customsDiscount.PGM_GroupCode;

			stlPrices.Items.FindByCode("USR").L7_Price = 40m;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA", createClientCompany: false);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var clientCompanyTW1 = BillingTestHelper.CreateClientCompany(lic1.Database, "TW1");
			clientCompanyTW1.LCC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var clientCompanyTW2 = BillingTestHelper.CreateClientCompany(lic1.Database, "TW2");
			clientCompanyTW2.LCC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;

			var clientCompanyAU = BillingTestHelper.CreateClientCompany(lic1.Database, "AU1");
			clientCompanyAU.LCC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			var discountSetting = Factory.New<DiscountLicenceSetting>();
			discountSetting.LS9_IsActive = true;
			discountSetting.LS9_LD = lic1.LA_LD;
			discountSetting.LS9_Name = "TWCUST";
			discountSetting.LS9_ValidFrom = periodStart;
			discountSetting.LS9_ValidTo = periodStart.AddMonths(6).AddDays(-1);
			discountSetting.LS9_Percent = 100;
			lic1.Database.LicenceSettings.Add(discountSetting);

			var usageUsrTW1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, clientCompanyTW1, 1);
			var usageUsrTW2 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, clientCompanyTW2, 2);
			var usageUsrAU = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, clientCompanyAU, 3);

			var usageCustomsTW1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "CUS", periodStart, clientCompanyTW1, 11);
			var usageCustomsTW2 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "CUS", periodStart, clientCompanyTW2, 23);
			var usageCustomsAU = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "CUS", periodStart, clientCompanyAU, 37);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill1 = billing.Bills[0];
			AssertNoErrors(bill1);
			AssertEquals((1 + 2 + 3) * 40m + (11 + 23 + 37) * 100, bill1.InvoicePreDiscountTotal);
			AssertEquals((1 + 2 + 3) * 40m + (37 * 100), bill1.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 8, 1)]
		public void TestGenerateReport_OrgMembershipDiscount()
		{
			var discountTypes = new CodeDescriptionBoolCollection(EdiPriceHeaderDiscount.Schema.PHD_NameMaxLength);
			discountTypes.Add("CW1MEM", (NoResString)"CW1 Membership");
			EDIDataRegistry.Instance.StlDiscountTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, discountTypes);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "CUS");
			stlPrices.L6_DiscountCode = "STL1";
			stlPrices.L6_UseStandardDiscount = false;

			var memDiscount = stlPrices.StlDiscounts.AddNew();
			memDiscount.PHD_Type = BillingConstants.DiscountCalculator.OrgMembership;
			memDiscount.PHD_Name = "CW1MEM";
			memDiscount.PHD_Percent = 50;
			memDiscount.PHD_IsDefaultEnabled = true;
			memDiscount.PHD_Version = "STL1";
			var memDiscountConfig = (OrgMembershipDiscount)memDiscount.Config;
			var line = memDiscountConfig.Lines.AddNew();
			line.MembershipType = "FTA";

			var customsDiscount = stlPrices.StlItemDiscounts.AddNew();
			customsDiscount.PGM_GroupCode = "Customs";
			customsDiscount.PGM_PHD = memDiscount.PK;

			var usr = stlPrices.Items.FindByCode("USR");
			usr.L7_Price = 40m;
			usr.L7_PGM_DiscountGroupCode = customsDiscount.PGM_GroupCode;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA", createClientCompany: false);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var membership = lic1.Company.Header.Memberships.AddNew();
			membership.EOR_MembershipType = "FTA";
			membership.EOR_ValidFrom = new ZDate(2020, 1, 1);

			var clientCompanyAU = BillingTestHelper.CreateClientCompany(lic1.Database, "AU1");
			clientCompanyAU.LCC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, periodStart);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, clientCompanyAU, 3);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill1 = billing.Bills[0];
			AssertNoErrors(bill1);
			AssertEquals(3 * 40m, bill1.InvoicePreDiscountTotal);
			AssertEquals(3 * 40m * 0.5m, bill1.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 8, 1)]
		public void TestGenerateReport_MasterOrgDevelopingCountryDiscount()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;

			//CW1
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "CUS");
			stlPrices.L6_DiscountCode = "STL1";
			stlPrices.L6_UseStandardDiscount = false;

			var devDiscount = stlPrices.StlDiscounts.AddNew();
			devDiscount.PHD_Type = BillingConstants.DiscountCalculator.DevelopingCountry;
			devDiscount.PHD_Name = "CW1DEV";
			devDiscount.PHD_IsDefaultEnabled = true;
			devDiscount.PHD_Version = "STL1";
			var devDiscountConfig = (CountryDiscount)devDiscount.Config;
			devDiscountConfig.Lines.RemoveAll();
			var line = devDiscountConfig.Lines.AddNew();
			line.Country = "AU";
			line.Percent = 50;
			line.RequiresDomesticDiscount = false;

			var customsDiscount = stlPrices.StlItemDiscounts.AddNew();
			customsDiscount.PGM_GroupCode = "Customs";
			customsDiscount.PGM_PHD = devDiscount.PK;

			var usr = stlPrices.Items.FindByCode("USR");
			usr.L7_Price = 40m;
			usr.L7_PGM_DiscountGroupCode = customsDiscount.PGM_GroupCode;

			var cw1Lic1 = BillingTestHelper.CreateLicence(Factory, "AAA", createClientCompany: false);
			BillingTestHelper.SetInvoicing(cw1Lic1, Env.CurrentBranch.PK, "AUD");
			cw1Lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var clientCompanyAU = BillingTestHelper.CreateClientCompany(cw1Lic1.Database, "AU1");
			clientCompanyAU.LCC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			BillingTestHelper.CreatePriceLink(cw1Lic1.Database, stlPrices, periodStart);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, clientCompanyAU, 3);

			//BOR
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);
			var mdcDiscount = borderWisePrices.StlDiscounts.AddNew();
			mdcDiscount.PHD_Type = BillingConstants.DiscountCalculator.MasterOrgDevelopingCountry;
			mdcDiscount.PHD_Name = "MDC Discount";
			mdcDiscount.PHD_IsDefaultEnabled = true;
			mdcDiscount.PHD_Version = borderWisePrices.L6_DiscountCode;
			var mdcDiscountConfig = (MasterOrgDevelopingCountryDiscount)mdcDiscount.Config;
			var line1 = mdcDiscountConfig.Lines.AddNew();
			line1.Country = "AU";
			line1.Percent = 15;
			AddDiscountStructure(borderWisePrices, "BorderWise", mdcDiscount);

			foreach (var priceItem in borderWisePrices.Items)
			{
				priceItem.L7_PGM_DiscountGroupCode = "BorderWise";
			}

			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN4", "CO4", "BW4");
			licBOR1.Database.LD_Product = "BOR";
			BillingTestHelper.SetInvoicing(licBOR1, Env.CurrentBranch.PK, "AUD");

			var licBOR2 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN5", "CO5", "BW5");
			licBOR2.Database.LD_Product = "BOR";
			BillingTestHelper.SetInvoicing(licBOR2, Env.CurrentBranch.PK, "AUD");
			licBOR2.Database.LD_OH_WebAccessOrg = cw1Lic1.Database.LD_OH_WebAccessOrg;

			BillingTestHelper.CreateExchangeRate(Factory, "USD", 2);
			BillingTestHelper.AddPriceItemRates(borderWisePrices, "USD", 2);

			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.UserPriceCode, periodStart, licBOR1.LA_LC, 19);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.UserPriceCode, periodStart, licBOR2.LA_LC, 25);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			var billBOR1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licBOR1.Company.LC_OH);
			AssertEquals("billBOR1.InvoicePreDiscountTotal", (19 * 200m), billBOR1.InvoicePreDiscountTotal);
			AssertEquals("billBOR1.InvoicePostDiscountTotal", (19 * 200m * 0.85m), billBOR1.InvoicePostDiscountTotal);

			var billBOR2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licBOR2.Company.LC_OH);
			AssertEquals("billBOR2.InvoicePreDiscountTotal", (25 * 200m), billBOR2.InvoicePreDiscountTotal);
			AssertEquals("billBOR2.InvoicePostDiscountTotal", (25 * 200m * 0.50m), billBOR2.InvoicePostDiscountTotal);

			var billCW1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == cw1Lic1.Company.LC_OH);
			AssertEquals("billCW1.InvoicePreDiscountTotal", (3 * 40m), billCW1.InvoicePreDiscountTotal);
			AssertEquals("billCW1.InvoicePostDiscountTotal", (3 * 40m * 0.50m), billCW1.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 8, 1)]
		public void TestGenerateReport_ProductBundleDiscount()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var discountTypes = new CodeDescriptionBoolCollection(EdiPriceHeaderDiscount.Schema.PHD_NameMaxLength);
			discountTypes.Add("BUNDIS", (NoResString)"Bundle Discount");
			EDIDataRegistry.Instance.StlDiscountTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, discountTypes);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var abcPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany);
			abcPriceHeader.L6_DiscountCode = "STL1";
			abcPriceHeader.L6_UseStandardDiscount = false;
			abcPriceHeader.L6_SystemCode = "DEF";

			var bunDiscount = abcPriceHeader.StlDiscounts.AddNew();
			bunDiscount.PHD_Type = BillingConstants.DiscountCalculator.ProductBundle;
			bunDiscount.PHD_Name = "BUNDIS";
			bunDiscount.PHD_Percent = 50;
			bunDiscount.PHD_IsDefaultEnabled = true;
			bunDiscount.PHD_Version = "STL1";
			var memDiscountConfig = (ProductBundleDiscount)bunDiscount.Config;
			var line1 = memDiscountConfig.Lines.AddNew();
			line1.ProductCode = "CW1";
			var line2 = memDiscountConfig.Lines.AddNew();
			line2.ProductCode = "WTA";

			var customsDiscount = abcPriceHeader.StlItemDiscounts.AddNew();
			customsDiscount.PGM_GroupCode = "Customs";
			customsDiscount.PGM_PHD = bunDiscount.PK;

			var item1 = BillingTestHelper.AddPriceItem(abcPriceHeader, "P01", "", "", 4m, "", 40m);
			item1.L7_PGM_DiscountGroupCode = customsDiscount.PGM_GroupCode;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			item1.L7_Category = "SAT";

			var stlPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "CUS");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var abcDatabase = lic1.Database;
			abcDatabase.LD_Product = "ABC";

			var lic2 = BillingTestHelper.CreateAnotherDatabase(lic1, "CW2");
			var cw1Database = lic2.Database;
			BillingTestHelper.CreatePriceLink(lic1.Database, abcPriceHeader, periodStart);
			BillingTestHelper.CreatePriceLink(lic2.Database, stlPriceHeader, periodStart);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", periodStart, lic1.Database.ClientCompanies[0], 6);
			lic1.Database.LD_OH_WebAccessOrg = lic2.Database.LD_OH_WebAccessOrg = lic1.Company.LC_OH;
			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory());
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill1 = billing.Bills.OfType<StlBill>().Single();
			AssertNoErrors(bill1);
			AssertEquals(4 * 6m, bill1.InvoicePreDiscountTotal);
			AssertEquals(4 * 6m, bill1.InvoicePostDiscountTotal);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2.Database.ClientCompanies[0], 1);
			Factory.Save();
			billing = new StlBilling(new BusinessObjectFactory());
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			bill1 = billing.Bills.OfType<StlBill>().Single();
			AssertNoErrors(bill1);
			AssertEquals(4 * 6m + 1m, bill1.InvoicePreDiscountTotal);
			AssertEquals(4 * 6m * 0.5m + 1m, bill1.InvoicePostDiscountTotal);

			lic2.Database.LD_OH_WebAccessOrg = ZGuid.Empty;
			Factory.Save();
			billing = new StlBilling(new BusinessObjectFactory());
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			bill1 = billing.Bills.OfType<StlBill>().Single();
			AssertNoErrors(bill1);
			AssertEquals(4 * 6m + 1m, bill1.InvoicePreDiscountTotal);
			AssertEquals(4 * 6m + 1m, bill1.InvoicePostDiscountTotal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_UnitCountAdjustment()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices1 = BillingTestHelper.CreateStlPriceList(stdLicCompany, "WTU", "P00", "USR");
			stlPrices1.L6_RX_NKCurrency = "AUD";
			stlPrices1.Items[0].L7_Price = 1.1m;
			stlPrices1.Items[1].L7_Price = 3.5m;
			stlPrices1.Items[2].L7_Price = 1.5m;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "CW1");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices1, periodStart, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "P00", periodStart, lic1.ClientCompany, 113);
			var wtuUsage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "WTU", periodStart, lic1.ClientCompany, 1);
			wtuUsage.U1_UnitCount = 156.87m;

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			AssertEquals(true, EDIDataRegistry.Instance.BillingUnitCountAdjustments.Value.OfType<BillingUnitCountAdjustment>().Any(x => x.PriceCode == "WTU"));

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			CombineAssertions(() =>
			{
				var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
				AssertNoErrors(bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", 1, bill.MonthlyUsages.First().PriceCurrencies.Count());
				AssertEquals("PriceCurrency", "AUD", bill.MonthlyUsages.First().PriceCurrencies.First());
				AssertEquals("1.1*156.87 + 3.5*113 + 1.5*10", 583.06m, bill.InvoicePreDiscountTotal); //

				var lines = bill.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode).ToArray();
				AssertEquals(3, lines.Length);

				AssertEquals("AUD", lines[0].PriceCurrency);
				AssertEquals(3.5m, lines[0].Price);
				AssertEquals(113m, lines[0].UnitCount);
				AssertEquals("P00", lines[0].PriceItemCode);

				AssertEquals("AUD", lines[1].PriceCurrency);
				AssertEquals(1.5m, lines[1].Price);
				AssertEquals(10m, lines[1].UnitCount);
				AssertEquals("USR", lines[1].PriceItemCode);

				AssertEquals("AUD", lines[2].PriceCurrency);
				AssertEquals(1.1m, lines[2].Price);
				AssertEquals(156.87m, lines[2].UnitCount);
				AssertEquals("WTU", lines[2].PriceItemCode);
			});
		}

		public void TestCreateInvoices_UsesSingleInstanceOfUSSalesTaxCalculator()
		{
			BillingTestHelper.CreateChargeCodeForCompany(Factory, GlbCompany.CurrentCompany.PK, null, EDIDataRegistry.Instance.CommentChargeCode.Value)
				.AC_ChargeType = Core.Constants.ChargeType.Comment;

			var lic1 = CreateLicenceWithPrices("AAA");
			var lic2 = CreateLicenceWithPrices("BBB");
			var lic3 = CreateLicenceWithPrices("CCC");

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			var client1 = CreateClient("AAA", branch1);
			var client2 = CreateClient("BBB", branch2);
			var client3 = CreateClient("CCC", branch2);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2016, 6, 1), lic1, 100);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2016, 6, 1), lic2, 200);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2016, 6, 1), lic3, 50);

			Factory.Save();

			var bill1 = new StlBill(Factory, branch1, lic1.Company.Header, "AUD", new ZDateTime(2015, 6, 1), new ZDateTime(2015, 6, 1)) { LocalExchangeRate = 1m };
			var bill2 = new StlBill(Factory, branch2, lic2.Company.Header, "AUD", new ZDateTime(2015, 6, 1), new ZDateTime(2015, 6, 1)) { LocalExchangeRate = 1m };
			var bill3 = new StlBill(Factory, branch2, lic3.Company.Header, "AUD", new ZDateTime(2015, 6, 1), new ZDateTime(2015, 6, 1)) { LocalExchangeRate = 1m };

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			mockCalculator.Setup(x => x.IsEnabled(It.IsAny<GlbBranch>())).Returns(true);
			mockCalculator.Setup(x => x.ShouldSetSalesTaxOnPost(It.IsAny<InvoicingBase>())).Returns(true);
			var calculationResult = new CalculationResult(5m);
			mockCalculator.Setup(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));
			mockCalculator.Setup(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>())).Returns((calculationResult, null));

			var mockCalculatorFactory = new Mock<IUSSalesTaxCalculatorFactory>();
			mockCalculatorFactory.Setup(x => x.Get()).Returns(mockCalculator.Object);

			var factoryList = (System.Collections.ArrayList)CargoWise.Application.ObjectFactory.Get("IUSSalesTaxCalculator_ClientSpecific");
			var oldFactory = factoryList[0];
			factoryList.Clear();
			factoryList.Add(mockCalculatorFactory.Object);
			try
			{
				var beforeCount = Factory.GetDatabaseCount(typeof(AccTransactionHeader));
				AssertEquals("Precondition: no invoices", 0, beforeCount);

				var billing = new StlBilling(Factory);
				billing.CreateInvoices(new[] { bill1, bill2, bill3 }, null);

				var count = Factory.CreateNewFactory().GetDatabaseCount(typeof(AccTransactionHeader));
				AssertEquals("Three invoices should be created", 3, count);

				mockCalculatorFactory.Verify(x => x.Get(), Times.Once(), "One USSalesTaxCalculator should be created, and re-used for all invoices across any branch");
				mockCalculator.Verify(x => x.SetSalesTaxLineItem(It.IsAny<InvoicingBase>(), It.IsAny<decimal>()), Times.Exactly(3), "Sales tax line items should be added for each invoice");
				mockCalculator.Verify(x => x.Dispose(), Times.Once(), "Dispose should be called once, after all invoices are processed");
			}
			finally
			{
				factoryList.Clear();
				factoryList.Add(oldFactory);
			}

			#region Local Helpers

			LicenceHeader CreateLicenceWithPrices(string code)
			{
				var lic = BillingTestHelper.CreateLicence(Factory, code);
				lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

				var prices = lic.Company.PriceHeaders.AddNew();
				var item1 = prices.Items.AddNew();
				item1.L7_Category = BillingConstants.BillingSystem.STL;
				item1.L7_Code = "USR";
				var rate1 = item1.CurrencyRates.AddNew();
				rate1.PIR_RX_NKCurrency = "AUD";
				rate1.PIR_Price = 17.5;

				var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
				priceHeaderLink.PHL_L6 = prices.PK;
				priceHeaderLink.PHL_RX_NKCurrency = "AUD";
				priceHeaderLink.PHL_ValidFrom = new ZDateTime(2016, 1, 1);

				return lic;
			}

			EDIOrgHeader CreateClient(string code, GlbBranch branch)
			{
				EDIOrgHeader client = BillingTestHelper.CreateOrganisation(Factory, code);
				BillingTestHelper.SetInvoicing(client, branch.PK, "AUD");
				client.CompanyData.OB_IsDebtor = true;
				return client;
			}

			#endregion
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_DiscountSuspensionPolicy()
		{
			var periodStart = BillingTestHelper.MonthToday;
			LicenceDatabase licDatabase;

			void assertHasDiscount(bool hasDiscount, string regPolicy, string ls9Policy)
			{
				var collection = new DiscountSuspensionPolicyCollection();
				if (!string.IsNullOrWhiteSpace(regPolicy))
				{
					collection.AddNew("CW1", regPolicy);
				}
				EDIDataRegistry.Instance.StlDiscountSuspensionPolicyDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

				var ls9 = licDatabase.LicenceSettings.FirstOrDefault(x => x.LS9_Type == BillingConstants.LicenceSetting.DiscountSuspensionPolicy);
				ls9?.Delete();

				if (!string.IsNullOrWhiteSpace(ls9Policy))
				{
					var newLS9 = licDatabase.LicenceSettings.AddNew();
					newLS9.LS9_Type = "DSP";
					newLS9.LS9_Name = ls9Policy;
					newLS9.LS9_ValidFrom = new ZDateTime(2000, 1, 1);
				}
				Factory.Save();

				var billing = new StlBilling(new BusinessObjectFactory());
				billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
				billing.GenerateReport(null);
				var bill1 = billing.Bills.Cast<StlBill>().Single();

				if (hasDiscount)
				{
					CombineAssertions(() =>
					{
						AssertNoErrors("bill1", bill1);
						AssertEquals("bill1.InvoicePreDiscountTotal", 50 * 1.00m + 101243 * 0.01m, bill1.InvoicePreDiscountTotal);
						AssertEquals("bill1.InvoicePostDiscountTotal", 50 * 1.00m + Utilities.Round(101243 * 0.01m * 0.95m, 2), bill1.InvoicePostDiscountTotal);
						AssertNotEquals("InvoicePreDiscountTotal != InvoicePostDiscountTotal", bill1.InvoicePreDiscountTotal, bill1.InvoicePostDiscountTotal);
					});
				}
				else
				{
					CombineAssertions(() =>
					{
						AssertNoErrors("bill1", bill1);
						AssertEquals("bill1.InvoicePreDiscountTotal", 50 * 1.00m + 101243 * 0.01m, bill1.InvoicePreDiscountTotal);
						AssertEquals("InvoicePreDiscountTotal == InvoicePostDiscountTotal", bill1.InvoicePreDiscountTotal, bill1.InvoicePostDiscountTotal);
					});
				}
			}

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "#HG");
			stlPrices.L6_DiscountCode = "STL1";
			stlPrices.L6_UseStandardDiscount = false;

			var discount = stlPrices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Special";
			discount.PHD_Percent = 5;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = stlPrices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount.PK;

			var tinyPrice = stlPrices.Items.FindByCode("#HG");
			tinyPrice.L7_Price = 0.01m;
			tinyPrice.L7_LicenceUnits = 0.1m;
			tinyPrice.L7_PGM_DiscountGroupCode = "Standard";

			var licCW1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			stlPrices.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices, periodStart);
			licDatabase = licCW1.Database;

			var usageUsr = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);
			var usageTinyPrice = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "#HG", periodStart, licCW1.ClientCompany, 101243);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			assertHasDiscount(true, null, null);
			assertHasDiscount(true, "NVR", null);
			assertHasDiscount(true, "ALW", "NVR");
			assertHasDiscount(true, null, "NVR");
			assertHasDiscount(true, null, "OVD");

			assertHasDiscount(false, "ALW", null);
			assertHasDiscount(false, "NVR", "ALW");
			assertHasDiscount(false, null, "ALW");
			assertHasDiscount(false, "ALW", "ALW");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_DiscountSuspensionPolicy_DoNotMixDiscounts()
		{
			var periodStart = BillingTestHelper.MonthToday;
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_DiscountCode = "STL1";
			stlPrices.L6_UseStandardDiscount = false;
			stlPrices.L6_RX_NKCurrency = "AUD";

			var discount = stlPrices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Special";
			discount.PHD_Percent = 5;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = stlPrices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Standard";
			itemDiscount.PGM_PHD = discount.PK;

			var usr = stlPrices.Items.FindByCode("USR");
			usr.L7_PGM_DiscountGroupCode = "Standard";

			Factory.Save();

			var licCW1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "CW1");
			licCW1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(licCW1.Database, stlPrices, periodStart);
			var licDatabase1 = licCW1.Database;
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);

			var licCW2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "CW2");
			licCW2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW2, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(licCW2.Database, stlPrices, periodStart);
			var licDatabase2 = licCW2.Database;
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW2.ClientCompany, 100);

			var newLS9 = licDatabase2.LicenceSettings.AddNew();
			newLS9.LS9_Type = "DSP";
			newLS9.LS9_Name = "ALW";
			newLS9.LS9_ValidFrom = new ZDateTime(2000, 1, 1);
			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(new BusinessObjectFactory());
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var monthlyUsages = billing.Bills.Cast<StlBill>().SelectMany(x => x.MonthlyUsages)
				.OrderBy(x => x.ServerCode).ToArray();
			AssertEquals(2, monthlyUsages.Length);
			AssertEquals("CW1", monthlyUsages[0].ServerCode);
			AssertEquals("CW2", monthlyUsages[1].ServerCode);

			var line1 = monthlyUsages[0].UsageLines.Single();
			var line2 = monthlyUsages[1].UsageLines.Single();
			line1.Discounts = line2.Discounts = null;
			line1.SetAmounts(10, 10);
			line2.SetAmounts(10, 10);

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, billing.DateTo);
			StlBilling.CalculateDiscounts(context, monthlyUsages);
			AssertNotNull(line1.Discounts);
			AssertNull(line2.Discounts);
			AssertEquals(47.50m, line1.PostDiscountAmount);
			AssertEquals(100m, line2.PostDiscountAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_CountryTierPricing_MissingRegistryMapping()
		{
			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = BillingConstants.PriceHeaderType.STL;
			usageCodes1.PriceItemCode = "FHR";
			usageCodes1.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			usageCodes1.KeyRefIndex1 = 1;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = usageCodes1.PriceItemCode;

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceList = stdLicCompany.PriceHeaders.AddNew();
			priceList.L6_PricelistVersion = "STL v1";
			priceList.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceList.L6_RX_NKCurrency = "AUD";
			priceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var userItem = priceList.Items.AddNew();
			userItem.L7_Category = BillingConstants.BillingSystem.STL;
			userItem.L7_Code = "USR";
			userItem.L7_Price = 0m;
			userItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			userItem.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			userItem.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			userItem.L7_Order = 1;
			var item1 = priceList.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = usageCodes1.PriceItemCode;
			item1.L7_Price = 2.00m;
			item1.L7_Order = 1;
			item1.L7_FeeType = BillingConstants.FeeType.CountryTier;
			item1.L7_ChargeCode = "STLUSAGE";
			item1.L7_CountryTierCode = "C01";

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_RL_NKClosestPort = "AUSYD";
			org2.OH_FullName = "2nd Company";
			org2.OH_Code = "BBBVVV";

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = licence.LA_LD;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_Name = "BBB" + " Co";
			clientCompany2.LCC_OH = org2.PK;
			clientCompany2.LCC_RN_NKCountryCode = "US";

			BillingTestHelper.CreatePriceLink(licence.Database, priceList, new ZDateTime(2018, 1, 1));

			var units = 800;
			var userUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, DatabaseUsage.ActiveUsersUsageCode, new ZDateTime(2018, 1, 1), licence.ClientCompany, 1);
			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, usageCodes1.PriceItemCode, new ZDateTime(2018, 1, 1), licence.ClientCompany, units);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, usageCodes1.PriceItemCode, new ZDateTime(2018, 1, 1), clientCompany2, units);

			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.DateTo = new ZDateTime(2018, 1, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertHasRowError(bill.MonthlyUsages.First(), FormattableString.Invariant($"System code {BillingConstants.PriceHeaderType.STL} and Price Code {usageCodes1.PriceItemCode} have no mapping for country code {licence.ClientCompany.LCC_RN_NKCountryCode} in the {EDIDataRegistry.Instance.CountryTierPriceCodeMappings.Caption} registry"));
				AssertHasRowError(bill.MonthlyUsages.First(), FormattableString.Invariant($"System code {BillingConstants.PriceHeaderType.STL} and Price Code {usageCodes1.PriceItemCode} have no mapping for country code {clientCompany2.LCC_RN_NKCountryCode} in the {EDIDataRegistry.Instance.CountryTierPriceCodeMappings.Caption} registry"));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_CountryTierPricing_MissingPriceItem()
		{
			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = BillingConstants.PriceHeaderType.STL;
			usageCodes1.PriceItemCode = "FHR";
			usageCodes1.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			usageCodes1.KeyRefIndex1 = 1;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = usageCodes1.PriceItemCode;
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var priceList = stdLicCompany.PriceHeaders.AddNew();
			priceList.L6_PricelistVersion = "STL v1";
			priceList.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceList.L6_RX_NKCurrency = "AUD";
			priceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var userItem = priceList.Items.AddNew();
			userItem.L7_Category = BillingConstants.BillingSystem.STL;
			userItem.L7_Code = "USR";
			userItem.L7_Price = 0m;
			userItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			userItem.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			userItem.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			userItem.L7_Order = 1;
			var item1 = priceList.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = usageCodes1.PriceItemCode;
			item1.L7_Price = 2.00m;
			item1.L7_Order = 1;
			item1.L7_FeeType = BillingConstants.FeeType.CountryTier;
			item1.L7_ChargeCode = "STLUSAGE";
			item1.L7_CountryTierCode = "C01";

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			licence.ClientCompany.LCC_RN_NKCountryCode = "NZ";
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_RL_NKClosestPort = "AUSYD";
			org2.OH_FullName = "2nd Company";
			org2.OH_Code = "BBBVVV";

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = licence.LA_LD;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_Name = "BBB" + " Co";
			clientCompany2.LCC_OH = org2.PK;
			clientCompany2.LCC_RN_NKCountryCode = "AU";

			BillingTestHelper.CreatePriceLink(licence.Database, priceList, new ZDateTime(2018, 1, 1));

			var units = 800;
			var userUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, DatabaseUsage.ActiveUsersUsageCode, new ZDateTime(2018, 1, 1), licence.ClientCompany, 1);
			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, usageCodes1.PriceItemCode, new ZDateTime(2018, 1, 1), licence.ClientCompany, units);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, usageCodes1.PriceItemCode, new ZDateTime(2018, 1, 1), clientCompany2, units);

			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.DateTo = new ZDateTime(2018, 1, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoRowError(bill.MonthlyUsages.First(), FormattableString.Invariant($"Country Tier Code C01 is not mapped to any price items for system code {BillingConstants.PriceHeaderType.STL} and Price Code {usageCodes1.PriceItemCode}"));
				AssertHasRowError(bill.MonthlyUsages.First(), FormattableString.Invariant($"Country Tier Code C02 is not mapped to any price items for system code {BillingConstants.PriceHeaderType.STL} and Price Code {usageCodes1.PriceItemCode}"));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_CountryTierPricing_NonSTL_ShouldNotThrowException()
		{
			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = BillingConstants.PriceHeaderType.BorderWise;
			usageCodes1.PriceItemCode = "FHR";
			usageCodes1.PriceHeaderCode = BillingConstants.PriceHeaderType.BorderWise;
			usageCodes1.KeyRefIndex1 = 1;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.BorderWise;
			mapping1.PriceCode = usageCodes1.PriceItemCode;
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var prices = lic.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_DiscountCode = "STL1";
			prices.L6_UseStandardDiscount = false;
			prices.L6_SystemCode = BillingConstants.PriceHeaderType.EHub;

			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "Prepay";
			discount.PHD_Percent = 10;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = prices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "Prepay Only";
			itemDiscount.PGM_PHD = discount.PK;

			var item1 = prices.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.ClientMapping;
			item1.L7_Code = BillingConstants.BillingSystem.ClientMapping;
			item1.L7_Ref4 = "Interface1";
			item1.L7_Price = 40;
			item1.L7_Order = 1;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_PGM_DiscountGroupCode = "Prepay Only";
			var item2 = prices.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.BorderWise;
			item2.L7_Code = usageCodes1.PriceItemCode;
			item2.L7_Price = 2.00m;
			item2.L7_Order = 1;
			item2.L7_FeeType = BillingConstants.FeeType.CountryTier;
			item2.L7_ChargeCode = "OTHUSAGE";
			item2.L7_CountryTierCode = "C01";

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ClientMapping, item1.L7_Ref4, new ZDateTime(2015, 7, 1), lic.ClientCompany, 10);

			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.DateTo = new ZDateTime(2015, 7, 31);
			AssertNoExceptionThrown(() => { billing.GenerateReport(null); });
			AssertEquals(1, billing.Bills.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_CountryTierPricing()
		{
			var billingCodes = new BillingDbUsageCodesCollection();
			var usageCodes1 = billingCodes.AddNew();
			usageCodes1.Category = BillingConstants.PriceHeaderType.STL;
			usageCodes1.PriceItemCode = "FHR";
			usageCodes1.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			usageCodes1.KeyRefIndex1 = 1;
			EDIDataRegistry.Instance.BillingDbUsageCodesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingCodes);

			var mappingCollection = new CountryTierPriceCodeMappingCollection();
			var mapping1 = new CountryTierPriceCodeMapping();
			mapping1.SystemCode = BillingConstants.PriceHeaderType.STL;
			mapping1.PriceCode = usageCodes1.PriceItemCode;
			mapping1.MappingLines.AddNew("AU", "C01");
			mapping1.MappingLines.AddNew("NZ", "C02");

			mappingCollection.Add(mapping1);

			EDIDataRegistry.Instance.CountryTierPriceCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappingCollection);

			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			var priceList = stdLicCompany.PriceHeaders.AddNew();
			priceList.L6_PricelistVersion = "STL v1";
			priceList.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceList.L6_RX_NKCurrency = "AUD";
			priceList.L6_ValidFrom = new ZDateTime(2018, 1, 1);

			var userItem = priceList.Items.AddNew();
			userItem.L7_Category = BillingConstants.BillingSystem.STL;
			userItem.L7_Code = "USR";
			userItem.L7_Price = 0m;
			userItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			userItem.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			userItem.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			userItem.L7_Order = 1;
			var item1 = priceList.Items.AddNew();
			item1.L7_Category = BillingConstants.BillingSystem.STL;
			item1.L7_Code = usageCodes1.PriceItemCode;
			item1.L7_Price = 2.00m;
			item1.L7_Order = 2;
			item1.L7_FeeType = BillingConstants.FeeType.CountryTier;
			item1.L7_ChargeCode = "STLUSAGE";
			item1.L7_CountryTierCode = "C01";
			var item2 = priceList.Items.AddNew();
			item2.L7_Category = BillingConstants.BillingSystem.STL;
			item2.L7_Code = usageCodes1.PriceItemCode;
			item2.L7_Price = 3.00m;
			item2.L7_Order = 3;
			item2.L7_FeeType = BillingConstants.FeeType.CountryTier;
			item2.L7_ChargeCode = "STLUSAGE";
			item2.L7_CountryTierCode = "C02";

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var licence = BillingTestHelper.CreateLicence(Factory, "AAA");
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "AUD");
			licence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_RL_NKClosestPort = "AUSYD";
			org2.OH_FullName = "2nd Company";
			org2.OH_Code = "BBBVVV";

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = licence.LA_LD;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_Name = "BBB" + " Co";
			clientCompany2.LCC_OH = org2.PK;
			clientCompany2.LCC_RN_NKCountryCode = "NZ";

			BillingTestHelper.CreatePriceLink(licence.Database, priceList, new ZDateTime(2018, 1, 1));

			var units = 800;
			var userUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, DatabaseUsage.ActiveUsersUsageCode, new ZDateTime(2018, 1, 1), licence.ClientCompany, 1);
			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, usageCodes1.PriceItemCode, new ZDateTime(2018, 1, 1), licence.ClientCompany, units);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.PriceHeaderType.STL, usageCodes1.PriceItemCode, new ZDateTime(2018, 1, 1), clientCompany2, units);

			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.DateTo = new ZDateTime(2018, 1, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licence.Company.LC_OH);
			CombineAssertions(() =>
			{
				AssertNoErrors("bill", bill);
				AssertEquals("bill.InvoicePreDiscountTotal should map from login company country to the appropriate tier code", (units * item1.L7_Price) + (units * item2.L7_Price), bill.InvoicePreDiscountTotal);
			});
		}

		public void TestGenerateReport_Validation_PriceHeaderExchangeRates_PriceLinkVolumeCode()
		{
			var period1 = BillingTestHelper.MonthToday;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company, "USR");
			priceHeader.L6_HasExchangeRates = false;
			var link = BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, period1);
			link.PHL_VolumeCode = "HV";

			var itemUSR = priceHeader.Items[0];
			itemUSR.L7_FeeType = BillingConstants.FeeType.Transactional;
			itemUSR.L7_Price = 40m;
			itemUSR.L7_LicenceUnits = 400m;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			var monthlyUsage = bill1.MonthlyUsages.First();
			AssertHasRowError(monthlyUsage, "Price list STL v5.0 is missing exchange rates.");

			link.PHL_VolumeCode = "STD";
			Factory.Save();

			billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			bill1 = billing1.Bills[0];
			monthlyUsage = bill1.MonthlyUsages.First();
			AssertNoRowError(monthlyUsage, "Price list STL v5.0 is missing exchange rates.");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2024, 7, 1)]
		public void TestGenerateReport_GenericUsage_MinimumFee()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("DEF", "DEF");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var thisPeriod = new ZDateTime(TestDateAttribute.Date);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var keyA = new UsageCodeKey("SAT", "P09");
			var priceHeader = BillingTestHelper.CreateValidStlPriceListWithExchangeRates(stdLicCompany, keyA);
			priceHeader.L6_SystemCode = "DEF";
			priceHeader.L6_ValidFrom = thisPeriod;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "SYD");
			BillingTestHelper.SetInvoicing(lic1.Company.Header, Env.CurrentBranchPK, "AUD");
			lic1.Database.LD_Product = "ABC";
			Factory.Save();

			var minimumFeeItem = priceHeader.Items.AddNew();
			minimumFeeItem.L7_Category = "SAT";
			minimumFeeItem.L7_Code = "#MF";
			minimumFeeItem.L7_Price = 150;
			minimumFeeItem.L7_LicenceUnits = 0;
			minimumFeeItem.L7_Order = 3;
			minimumFeeItem.L7_FeeType = BillingConstants.FeeType.MinimumFee;
			minimumFeeItem.L7_ExchangeRateGroupCode = "STL";

			var item1 = priceHeader.Items.AddNew();
			item1.L7_Category = "SAT";
			item1.L7_Code = "P01";
			item1.L7_Price = 4;
			item1.L7_LicenceUnits = 40;
			item1.L7_Order = 1;
			item1.L7_ParentCategory = minimumFeeItem.L7_Category;
			item1.L7_ParentCode = "#MF";
			item1.L7_ExchangeRateGroupCode = "STL";

			var item2 = priceHeader.Items.AddNew();
			item2.L7_Category = "SAT";
			item2.L7_Code = "P02";
			item2.L7_Price = 5;
			item2.L7_LicenceUnits = 50;
			item2.L7_Order = 2;
			item2.L7_FeeType = BillingConstants.FeeType.Transactional;
			item2.L7_ExchangeRateGroupCode = "STL";

			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item2.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			minimumFeeItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", thisPeriod, lic1.ClientCompany, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", thisPeriod, lic1.ClientCompany, 3);
			Factory.Save();

			var billing = new StlBilling(new BusinessObjectFactory());
			billing.DateTo = thisPeriod.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single();
			AssertNoErrors("bill1", bill1);

			var linesAsText = string.Join("\r\n", bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode)
				.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.PostDiscountAmount}"));

			//142 + 8 = 150(#MF)
			AssertEquals(
@"#MF | 142.00 | AUD | 142.00
P01 | 4.00 | AUD | 8.00
P02 | 5.0 | AUD | 15.0", linesAsText);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateReport_CargoWiseNext()
		{
			var periodStart = BillingTestHelper.MonthToday;

			LicenceCompany.ClearStandardPricesCompanyCache();
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.80);

			var cwnPrices = stdLicCompany.PriceHeaders.AddNew();
			cwnPrices.L6_RX_NKCurrency = "USD";
			cwnPrices.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			cwnPrices.L6_ValidFrom = periodStart;

			var mapping = cwnPrices.UsageMaps.AddNew();
			mapping.PUM_PriceCategory = "CWN";
			mapping.PUM_PriceCode = "SHD";
			mapping.PUM_UsageCategory = "STL";
			mapping.PUM_UsageCode = "SHD";

			cwnPrices.L6_HasExchangeRates = true;
			var ex1 = cwnPrices.ExchangeRates.AddNew();
			ex1.PHE_RX_NKCurrency = "AUD";
			ex1.PHE_Rate = 1.25m;
			ex1.PHE_GroupCode = "CWN";
			var ex2 = cwnPrices.ExchangeRates.AddNew();
			ex2.PHE_RX_NKCurrency = "USD";
			ex2.PHE_Rate = 1m;
			ex2.PHE_GroupCode = "CWN";

			var cwnPriceItem = cwnPrices.Items.AddNew();
			cwnPriceItem.L7_Category = BillingConstants.BillingSystem.CargoWiseNext;
			cwnPriceItem.L7_Code = "SHD";
			cwnPriceItem.L7_Price = 2.5m;
			cwnPriceItem.L7_Order = 1;
			cwnPriceItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			cwnPriceItem.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			cwnPriceItem.L7_ExchangeRateGroupCode = "CWN";
			Factory.Save();

			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR");
			stlPrices.L6_RX_NKCurrency = "USD";
			stlPrices.L6_HasExchangeRates = true;
			var audRate = stlPrices.ExchangeRates.AddNew();
			audRate.PHE_RX_NKCurrency = "AUD";
			audRate.PHE_Rate = 1.25m;
			audRate.PHE_GroupCode = "STL";
			foreach (var item in stlPrices.Items)
			{
				item.L7_ExchangeRateGroupCode = "STL";
			}

			BillingTestHelper.CreatePriceLink(lic.Database, stlPrices, periodStart.AddYears(-1));
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic.ClientCompany, 10);
			var shd1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHD", periodStart, lic.ClientCompany, 7);
			shd1.U1_RX_NKCurrency = "USD";
			shd1.U1_TotalPrice = 15.45m;
			shd1.U1_Direction = "IMP";
			var shd2 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHD", periodStart, lic.ClientCompany, 15);
			shd2.U1_RX_NKCurrency = "AUD";
			shd2.U1_TotalPrice = 25.12m;
			var shd3 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHD", periodStart, lic.ClientCompany, 8);
			shd3.U1_RX_NKCurrency = "USD";
			shd3.U1_TotalPrice = 17.66m;
			shd3.U1_Direction = "EXP";

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);

			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic.Company.LC_OH);
			AssertEquals(1, billing.Bills.Count);
			AssertNoErrors("bill1", bill1);

			var linesAsText = string.Join("\r\n", bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode)
				.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.PostDiscountAmount} | {x.Direction}"));

			AssertEquals(
@"SHD | 0 | USD | 15.45 | IMP
SHD | 0 | AUD | 25.12 | 
SHD | 0 | USD | 17.66 | EXP
USR | 1.25 | AUD | 12.50 | ", linesAsText);

			AssertEquals("AUD", bill1.InvoiceCurrencyCode);

			// 25.12(SHD) + 12.50(USR) + 15.45 (SHD.IMP) * 1.25 + 17.66 (SHD.EXP) * 1.25 = 88.41
			AssertEquals(79.01m, bill1.InvoicePreDiscountTotal);
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

		EdiPriceHeaderDiscount AddPercentageDiscount(ClientLicencePriceHeader prices, string name, decimal percent, bool isDefaultEnabled = true)
		{
			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = name;
			discount.PHD_Percent = percent;
			discount.PHD_IsDefaultEnabled = isDefaultEnabled;
			discount.PHD_Version = prices.L6_DiscountCode;
			return discount;
		}

		void AddDiscountStructure(ClientLicencePriceHeader prices, string name, params EdiPriceHeaderDiscount[] discounts)
		{
			foreach (var discount in discounts)
			{
				var itemDiscount = prices.StlItemDiscounts.AddNew();
				itemDiscount.PGM_GroupCode = name;
				itemDiscount.PGM_PHD = discount.PK;
			}
		}

		static List<DetailUsageLineForTest> LoadStlUsageLinesForTest(LicenceCompany licCompany, ZDateTime periodStart)
		{
			var lineCollection = new List<DetailUsageLineForTest>();
			var query = "SELECT LD_PK, LD_ServerCode, LE_EnterpriseCode, LCC_PK, LCC_Code, L7_PK, L7_Description, L7_Order, U1_Code FROM EdiGetStlUsageSummary(@OrgPk, @PeriodStart);";

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, licCompany.LC_OH.ToGuid());
				command.AddParameter("@PeriodStart", SqlDbType.DateTime, periodStart.ToDateTime());

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var line = new DetailUsageLineForTest();

						line.DatabasePK = (Guid)reader[LicenceDatabaseSchema.Constants.PK];
						line.DatabaseServerCode = (string)reader[LicenceDatabaseSchema.Constants.LD_ServerCode];
						line.EnterpriseCode = (string)reader[LicenceEnterpriseSchema.Constants.LE_EnterpriseCode];
						line.PriceItemPK = (Guid)reader[ClientLicencePriceItemSchema.Constants.PK];
						line.PriceItemDescription = (string)reader[ClientLicencePriceItemSchema.Constants.L7_Description];
						line.PriceItemOrder = (short)reader[ClientLicencePriceItemSchema.Constants.L7_Order];
						line.ClientCompanyPK = !reader.IsDBNull(reader.GetOrdinal(ClientCompanySchema.Constants.PK)) ? (Guid)reader[ClientCompanySchema.Constants.PK] : Guid.Empty;
						line.CompanyCode = (string)reader[ClientCompanySchema.Constants.LCC_Code];
						line.SystemCode = (string)reader[ClientChargeableUsageSchema.Constants.U1_Code];

						lineCollection.Add(line);
					}
				}
			}
			return lineCollection;
		}

		List<DetailUsageLineForTest> LoadOdplUsageLinesForTest(CodeDescriptionPairList systemList,
					OrgHeader org,
					ZDateTime periodStart)
		{
			var lineCollection = new List<DetailUsageLineForTest>();

			// Use literals rather than parameters or ediProd will choose bad query plans (e.g., 100x slower)
			var query = "SELECT OH_PK, OH_Code, OH_FullName, U1_Code, ServerCode, CompanyCode, LCC_PK, LC_PK, LD_PK FROM EdiGetSystemUsage('" + org.PK + "', '" + periodStart.SqlFormat + "');";

			using (var command = Db.Connection.Command(query))
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var line = new DetailUsageLineForTest();

						line.OrgPK = (Guid)reader[OrgHeaderSchema.Constants.PK];
						line.OrgName = (string)reader[OrgHeaderSchema.Constants.OH_FullName];
						line.SystemCode = (string)reader[ClientChargeableUsageSchema.Constants.U1_Code];
						line.DatabaseServerCode = (string)reader["ServerCode"];
						line.CompanyCode = (string)reader["CompanyCode"];
						line.ClientCompanyPK = !reader.IsDBNull(reader.GetOrdinal(ClientCompanySchema.Constants.PK)) ? (Guid)reader[ClientCompanySchema.Constants.PK] : Guid.Empty;
						line.LicenceCompanyPK = !reader.IsDBNull(reader.GetOrdinal(LicenceCompanySchema.Constants.PK)) ? (Guid)reader[LicenceCompanySchema.Constants.PK] : Guid.Empty;
						line.DatabasePK = !reader.IsDBNull(reader.GetOrdinal(LicenceDatabaseSchema.Constants.PK)) ? (Guid)reader[LicenceDatabaseSchema.Constants.PK] : Guid.Empty;

						if (systemList.ContainsCode(line.SystemCode))
						{
							lineCollection.Add(line);
						}
					}
				}
			}

			return lineCollection;
		}

		sealed class DetailUsageLineForTest
		{
			public Guid DatabasePK { get; set; }
			public ZString DatabaseServerCode { get; set; }
			public ZString EnterpriseCode { get; set; }
			public ZGuid PriceItemPK { get; set; }
			public ZString PriceItemDescription { get; set; }
			public ZShort PriceItemOrder { get; set; }
			public ZGuid ClientCompanyPK { get; set; }
			public ZString CompanyCode { get; set; }
			public ZString SystemCode { get; set; }
			public ZGuid OrgPK { get; set; }
			public ZString OrgName { get; set; }
			public ZGuid LicenceCompanyPK { get; set; }
		}
	}
}
