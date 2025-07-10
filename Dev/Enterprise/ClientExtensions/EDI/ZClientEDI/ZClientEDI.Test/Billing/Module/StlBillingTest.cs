using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.GUI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.Testing.FilterStripsTestHelper;

namespace Enterprise.Client.EDI.Billing.Module.Testing;

[TestedType(typeof(StlBilling))]
public sealed class StlBillingTest : UsageBillingTest
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestGenerateReport_HUBPrice_PriceItemFilter()
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
		var filter = billing.PriceItemFilter;
		AddPriceItemFilterStrips(filter, new FilterStripDefinition
		{
			FilterStripName = "Code",
			FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "CMP",
		});
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

		var billing2 = new StlBilling(Factory);
		billing2.DateTo = periodStart.AddMonths(1).AddDays(-1);
		var filter2 = billing2.PriceItemFilter;
		AddPriceItemFilterStrips(filter2, new FilterStripDefinition
		{
			FilterStripName = "Code",
			FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "USR",
		});
		billing2.GenerateReport(null);
		AssertEquals(0, billing2.Bills.Count);
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestGenerateReport_Fees_NoStlUsage()
	{
		BillingTestHelper.LoadClientSpecificDocuments();

		BillingTestHelper.CreateExchangeRate(Factory, "EUR", 0.5); // 1 AUD = 0.5 EUR
		BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.8);

		BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
		var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
		var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
		BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
		BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
		lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
		lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

		var periodStart = ZDateTime.Today;
		periodStart = periodStart.AddDays(1 - periodStart.Day);
		var feeEUR = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc EUR", 100, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
		feeEUR.L8_RX_NKCurrency = "EUR";

		var feeAUD = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc AUD", 200, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
		feeAUD.L8_RX_NKCurrency = "AUD";

		var feeUSD = BillingTestHelper.CreateLicenceFee(lic2.Company, "ESV", "desc USD", 500, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
		feeUSD.L8_RX_NKCurrency = "USD";

		Factory.Save();

		var billing = new StlBilling(Factory);
		billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
		billing.GenerateReport(null);
		AssertEquals(2, billing.Bills.Count);

		var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
		var bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
		CombineAssertions(() =>
		{
			AssertNoErrors("bill1", bill1);
			AssertNoErrors("bill2", bill2);
			AssertEquals("bill1.InvoicePostDiscountTotal", 200m + 100m / 0.5m, bill1.InvoicePostDiscountTotal);
			AssertEquals("bill2.InvoicePostDiscountTotal", 500m / 0.8m, bill2.InvoicePostDiscountTotal);
		});

		//PriceItemFilter
		billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
		billing.AccumulateMonths = 0;
		billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
		AddPriceItemFilterStrips(billing.PriceItemFilter, new FilterStripDefinition
		{
			FilterStripName = "Code",
			FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "USR",
		});
		billing.GenerateReport(null);
		AssertEquals(false, billing.Bills.Any());
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestGenerateReport_Fees_WithStlUsage()
	{
		var periodStart = ZDateTime.Today;
		periodStart = periodStart.AddDays(1 - periodStart.Day);

		BillingTestHelper.LoadClientSpecificDocuments();

		BillingTestHelper.CreateExchangeRate(Factory, "EUR", 0.5); // 1 AUD = 0.5 EUR
		BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.8);

		BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
		var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
		var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
		BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
		BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK, "AUD");
		lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
		lic2.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

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
		BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, lic2, 9);

		var feeEUR = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc EUR", 100, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
		feeEUR.L8_RX_NKCurrency = "EUR";

		var feeAUD = BillingTestHelper.CreateLicenceFee(lic1.Company, "ESV", "desc AUD", 200, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
		feeAUD.L8_RX_NKCurrency = "AUD";

		var feeUSD = BillingTestHelper.CreateLicenceFee(lic2.Company, "ESV", "desc USD", 500, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
		feeUSD.L8_RX_NKCurrency = "USD";

		Factory.Save();

		var billing = new StlBilling(Factory);
		billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
		billing.GenerateReport(null);
		AssertEquals(2, billing.Bills.Count);

		var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
		var bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
		CombineAssertions(() =>
		{
			AssertNoErrors("bill1", bill1);
			AssertNoErrors("bill2", bill2);
			AssertEquals("bill1.InvoicePostDiscountTotal", 100m / 0.5m + 200m + 5 * 40m, bill1.InvoicePostDiscountTotal);
			AssertEquals("bill2.InvoicePostDiscountTotal", 500m / 0.8m + 9 * 40m, bill2.InvoicePostDiscountTotal);
		});

		//PriceItemFilter
		billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
		billing.AccumulateMonths = 0;
		billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
		AddPriceItemFilterStrips(billing.PriceItemFilter, new FilterStripDefinition
		{
			FilterStripName = "Code",
			FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "USR",
		});
		billing.GenerateReport(null);
		AssertEquals(2, billing.Bills.Count);

		bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
		bill2 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic2.Company.LC_OH);
		CombineAssertions(() =>
		{
			AssertNoErrors("bill1", bill1);
			AssertNoErrors("bill2", bill2);
			AssertEquals("bill1.InvoicePostDiscountTotal", 5 * 40m, bill1.InvoicePostDiscountTotal);
			AssertEquals("bill2.InvoicePostDiscountTotal", 9 * 40m, bill2.InvoicePostDiscountTotal);
		});
	}

	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestPriceHeaderExchangeRate_MinimumFee()
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
		prices.L6_HasExchangeRates = true;
		prices.L6_Rounding = "V1";
		prices.L6_RX_NKCurrency = "USD";
		prices.L6_PricelistVersion = "STL V100";
		prices.L6_DiscountCode = "STL1";
		prices.L6_UseStandardDiscount = false;

		var rate1 = prices.ExchangeRates.AddNew();
		rate1.PHE_GroupCode = "";
		rate1.PHE_RX_NKCurrency = "NZD";
		rate1.PHE_Rate = 1.45m;
		rate1.PHE_UpliftPercent = 15m;
		var rate2 = prices.ExchangeRates.AddNew();
		rate2.PHE_GroupCode = "";
		rate2.PHE_RX_NKCurrency = "AUD";
		rate2.PHE_Rate = 1.30m;
		rate2.PHE_UpliftPercent = 15m;

		var minimumFeeItem = prices.Items.AddNew();
		minimumFeeItem.L7_Category = "CW1";
		minimumFeeItem.L7_Code = "#MF";
		minimumFeeItem.L7_Price = 150;
		minimumFeeItem.L7_LicenceUnits = 0;
		minimumFeeItem.L7_Order = 3;
		minimumFeeItem.L7_FeeType = BillingConstants.FeeType.MinimumFee;

		var item1 = prices.Items.AddNew();
		item1.L7_Category = BillingConstants.BillingSystem.STL;
		item1.L7_Code = "USR";
		item1.L7_Price = 4;
		item1.L7_LicenceUnits = 40;
		item1.L7_Order = 1;
		item1.L7_ParentCategory = minimumFeeItem.L7_Category;
		item1.L7_ParentCode = "#MF";

		var itemPerDatabase = prices.Items.AddNew();
		itemPerDatabase.L7_Category = BillingConstants.BillingSystem.ODM;
		itemPerDatabase.L7_Code = "GZH";
		itemPerDatabase.L7_Price = 300;
		itemPerDatabase.L7_LicenceUnits = 3000;
		itemPerDatabase.L7_Order = 5;
		itemPerDatabase.L7_FeeType = BillingConstants.FeeType.Database;

		var discount1 = prices.StlDiscounts.AddNew();
		discount1.PHD_Type = "VOL";
		var group1 = prices.StlItemDiscounts.AddNew();
		group1.PGM_GroupCode = "G1";
		group1.PGM_PHD = discount1.PK;
		item1.L7_PGM_DiscountGroupCode = minimumFeeItem.L7_PGM_DiscountGroupCode = itemPerDatabase.L7_PGM_DiscountGroupCode = "G1";

		item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
		itemPerDatabase.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
		minimumFeeItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

		var priceHeaderLink = lic1.Database.PriceHeaderLinks.AddNew();
		priceHeaderLink.PHL_L6 = prices.PK;
		priceHeaderLink.PHL_RX_NKCurrency = "AUD";
		priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);
		priceHeaderLink.PHL_CorePackCode = "UP";
		priceHeaderLink.PHL_VolumeCode = "LV";
		priceHeaderLink.PHL_VolumePercent = 25m;

		var priceHeaderLinkOther = licOther.Database.PriceHeaderLinks.AddNew();
		priceHeaderLinkOther.PHL_L6 = prices.PK;
		priceHeaderLinkOther.PHL_RX_NKCurrency = "AUD";
		priceHeaderLinkOther.PHL_ValidFrom = new ZDateTime(2015, 7, 1);
		priceHeaderLinkOther.PHL_CorePackCode = "UP";
		priceHeaderLinkOther.PHL_VolumeCode = "LV";
		priceHeaderLinkOther.PHL_VolumePercent = 25m;

		BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 5);
		BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 4);
		BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2015, 7, 1), lic3paidBy2.ClientCompany, 6);
		BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", new ZDateTime(2015, 7, 1), licOther.ClientCompany, 1000);

		BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "GZH", new ZDateTime(2015, 7, 1), lic1.ClientCompany, 55);
		BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "GZH", new ZDateTime(2015, 7, 1), lic2.ClientCompany, 77);

		Factory.Save();

		var billing = new StlBilling(new BusinessObjectFactory());
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

		var linesAsText = string.Join("\r\n", bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode)
			.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.LicenceUnits.ToZInt()}"));
		AssertEquals(
@"#MF | 26.00 | NZD | 0
GZH | 110 | AUD | 750
USR | 2.24 | AUD | 15", linesAsText);

		linesAsText = string.Join("\r\n", bill2.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode)
			.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.LicenceUnits}"));
		AssertEquals(@"USR | 2.50 | NZD | 15.0", linesAsText);

		linesAsText = string.Join("\r\n", billOther.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode)
			.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.LicenceUnits}"));
		AssertEquals(@"USR | 2.24 | AUD | 15.0", linesAsText);

		#region High Volume Feature

		priceHeaderLink.PHL_VolumeCode = "HV";
		priceHeaderLink.PHL_VolumePercent = 90m;
		var hvfSetting = Factory.New<HighVolumeFeatureSetting>();
		hvfSetting.LS9_LD = lic1.LA_LD;
		hvfSetting.PriceKey = new UsageCodeKey(minimumFeeItem.L7_Category, minimumFeeItem.L7_Code);
		lic1.Database.LicenceSettings.Add(hvfSetting);

		Factory.Save();

		billing = new StlBilling(new BusinessObjectFactory());
		billing.DateTo = new ZDateTime(2015, 7, 31);
		billing.GenerateReport(null);
		AssertEquals(3, billing.Bills.Count);
		bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
		AssertNoErrors(bill1);
		AssertEquals("AUD", bill1.InvoiceCurrencyCode);
		AssertEquals("AUD", bill1.MonthlyUsages.First().PriceCurrencies.First());

		linesAsText = string.Join("\r\n", bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode)
.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.LicenceUnits.ToZInt()}"));
		AssertEquals(
@"#MF | 68.75 | NZD | 0
GZH | 450 | AUD | 3000
USR | 9.0 | AUD | 60", linesAsText);

		//PriceItemFilter
		billing = new StlBilling(new BusinessObjectFactory());
		billing.DateTo = new ZDateTime(2015, 7, 31);
		AddPriceItemFilterStrips(billing.PriceItemFilter, new FilterStripDefinition
		{
			FilterStripName = "Code",
			FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "USR",
		});
		billing.GenerateReport(null);
		AssertEquals(3, billing.Bills.Count);
		bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == lic1.Company.LC_OH);
		AssertNoErrors(bill1);
		AssertEquals("AUD", bill1.InvoiceCurrencyCode);
		AssertEquals("AUD", bill1.MonthlyUsages.First().PriceCurrencies.First());

		linesAsText = string.Join("\r\n", bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode)
.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.LicenceUnits.ToZInt()}"));
		AssertEquals(@"USR | 9.0 | AUD | 60", linesAsText);

		#endregion
	}

	public void TestGenerateReport_MinimumFeePerReference()
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
		minFeePrice.L7_FeeType = BillingConstants.FeeType.MinimumFeePerReference;

		var period1 = BillingTestHelper.MonthToday;

		var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
		BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
		lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
		lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
		BillingTestHelper.CreatePriceLink(lic1.Database, stlPrices, period1.AddYears(-1));

		var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "CO2");
		lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;

		var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic1.ClientCompany, 120);
		usage1.U1_Reference1 = "Carrier A";
		var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic1.ClientCompany, 90);
		usage2.U1_Reference1 = "Carrier B";
		var usrUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", period1, lic1.ClientCompany, 10);

		var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic2.ClientCompany, 40);
		usage3.U1_Reference1 = "Carrier A";
		var usage4 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic2.ClientCompany, 80);
		usage4.U1_Reference1 = "Carrier B";

		// Carrier C below min fee
		var usage5 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic1.ClientCompany, 1);
		usage5.U1_Reference1 = "Carrier C";

		// Carrier D below min fee
		var usage6 = BillingTestHelper.CreateChargeableUsage(Factory, TestUsageCategory, "CES", period1, lic1.ClientCompany, 1);
		usage6.U1_Reference1 = "Carrier D";

		Factory.Save();

		EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);

		var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
		billing1.AccumulateMonths = 0;
		billing1.DateTo = period1.AddMonths(1).AddDays(-1);
		billing1.GenerateReport(null);
		var bill1 = billing1.Bills[0];
		AssertNoErrors(bill1);
		AssertEquals(10m + (50 * 50m + 50 * 40m + 60 * 30m) + (50 * 50m + 50 * 40m + 70 * 30m) + (1 * 50m + 50m) + (1 * 50m + 50m), bill1.InvoicePreDiscountTotal);
		var monthlyUsage = bill1.MonthlyUsages.First();
		var minFeeLines = monthlyUsage.UsageLines.Where(x => x.PriceItemCode == "CEM").ToList();
		AssertEquals(2, minFeeLines.Count);
		AssertEquals("Carrier C", minFeeLines[0].AdditionalDescription);
		AssertEquals("Carrier D", minFeeLines[1].AdditionalDescription);

		//PriceItemFilter
		var billing2 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
		billing2.AccumulateMonths = 0;
		billing2.DateTo = period1.AddMonths(1).AddDays(-1);
		AddPriceItemFilterStrips(billing2.PriceItemFilter, new FilterStripDefinition
		{
			FilterStripName = "Code",
			FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "USR",
		});
		billing2.GenerateReport(null);
		var bill2 = billing2.Bills.OfType<StlBill>().Single();
		AssertNoErrors(bill2);
		AssertEquals("USR", bill2.MonthlyUsages.Single().UsageLines.Single().PriceItemCode);
		AssertEquals(10m, bill2.InvoicePreDiscountTotal);
	}

	public void TestGenerateReport_MinSpendSetting()
	{
		var period1 = BillingTestHelper.MonthToday;

		var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
		BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
		lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
		lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

		// USR - has min spend setting, but usage above min.
		// WOD - has min spend setting AND price setting. Usage is below min
		// ETL - has min spend setting, usage below min
		// AAA - has min spend setting and no usage at all
		var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company, "USR", "WOD", "WOD", "ETL", "ETL", "AAA");
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

		var itemETL1 = priceHeader.Items[3];
		itemETL1.L7_FeeType = BillingConstants.FeeType.Transactional;
		itemETL1.L7_Price = 2m;
		itemETL1.L7_LicenceUnits = 20m;
		var itemETL2 = priceHeader.Items[4];
		itemETL2.L7_FeeType = BillingConstants.FeeType.Transactional;
		itemETL2.L7_UnitBreak = 1000;
		itemETL2.L7_Price = 1.5m;
		itemETL2.L7_LicenceUnits = 15m;
		var priceSettingWOD = Factory.New<PriceLicenceSetting>();
		priceSettingWOD.LS9_ValidFrom = period1;
		priceSettingWOD.LS9_LD = lic1.LA_LD;
		priceSettingWOD.PriceKey = new UsageCodeKey(BillingConstants.BillingSystem.STL, "WOD");
		priceSettingWOD.LS9_Price = 0.75;
		priceSettingWOD.EnableUnits = false;
		lic1.Database.LicenceSettings.Add(priceSettingWOD);
		AddMinSpendSetting(lic1.Database, period1, new UsageCodeKey(BillingConstants.BillingSystem.STL, "WOD"), 2000m);
		AddMinSpendSetting(lic1.Database, period1, new UsageCodeKey(BillingConstants.BillingSystem.STL, "ETL"), 5000m);
		AddMinSpendSetting(lic1.Database, period1, new UsageCodeKey(BillingConstants.BillingSystem.STL, "USR"), 1m);
		AddMinSpendSetting(lic1.Database, period1, new UsageCodeKey(BillingConstants.BillingSystem.STL, "AAA"), 1000m);

		// USR = 15 x $40 = $600
		BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
		// WOD = 1500 x 0.75 = $1125 (one volume break)
		BillingTestHelper.CreateChargeableUsage(Factory, "STL", "WOD", period1, lic1.ClientCompany, 1500);
		// ETL = 1000 x $2 + 500 x $1.5 = $2750 (cumulative volume breaks)
		BillingTestHelper.CreateChargeableUsage(Factory, "STL", "ETL", period1, lic1.ClientCompany, 1500);

		Factory.Save();

		var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
		billing1.AccumulateMonths = 0;
		billing1.DateTo = period1.AddMonths(1).AddDays(-1);
		billing1.GenerateReport(null);
		var bill1 = billing1.Bills[0];
		AssertNoNotifications(bill1);
		var monthlyUsage = bill1.MonthlyUsages.First();
		var usageLinesInOrder = monthlyUsage.UsageLinesInOrder.ToList();

		var usageListWOD = usageLinesInOrder.Where(x => x.PriceItemCode == "WOD").ToList();
		var usageWOD = usageListWOD[0];
		AssertEquals(0.75m, usageWOD.Price);
		AssertEquals(1500m, usageWOD.TotalUnitCount);
		AssertEquals("custom price with no licence units uses the units from the lowest break", 10m, usageWOD.LicenceUnits);
		var minSpendWOD = usageListWOD[1];
		AssertEquals(875m, minSpendWOD.Price);
		AssertEquals(875m, minSpendWOD.PreDiscountAmount);
		AssertEquals(875m, minSpendWOD.PostDiscountAmount);
		AssertEquals(1m, minSpendWOD.TotalUnitCount);
		AssertEquals(0m, minSpendWOD.LicenceUnits);
		AssertEquals(2, usageListWOD.Count);

		var usageUSR = usageLinesInOrder.Single(x => x.PriceItemCode == "USR");
		AssertEquals(40m, usageUSR.Price);
		AssertEquals(15m, usageUSR.TotalUnitCount);
		AssertEquals(400m, usageUSR.LicenceUnits);

		var minSpendAAA = usageLinesInOrder.Single(x => x.PriceItemCode == "AAA");
		AssertEquals(1000m, minSpendAAA.Price);
		AssertEquals("AUD", minSpendAAA.PriceCurrency);
		AssertEquals(0m, minSpendAAA.LicenceUnits);
		AssertEquals("min spend line AAA UnitCount", 1m, minSpendAAA.UnitCount);
		AssertEquals("min spend line AAA PreDiscountAmount", 1000m, minSpendAAA.PreDiscountAmount);
		AssertEquals("min spend line AAA PostDiscountAmount", 1000m, minSpendAAA.PostDiscountAmount);

		var usageListETL = usageLinesInOrder.Where(x => x.PriceItemCode == "ETL").ToList();
		AssertEquals(2m, usageListETL[0].Price);
		AssertEquals(20m, usageListETL[0].LicenceUnits);
		AssertEquals(2000m, usageListETL[0].PostDiscountAmount);
		AssertEquals(1.5m, usageListETL[1].Price);
		AssertEquals(15m, usageListETL[1].LicenceUnits);
		AssertEquals(750m, usageListETL[1].PostDiscountAmount);
		AssertEquals("min spend line ETL Price", 2250m, usageListETL[2].Price);
		AssertEquals("min spend line ETL LicenceUnits", 0m, usageListETL[2].LicenceUnits);
		AssertEquals("min spend line ETL PreDiscountAmount", 2250m, usageListETL[2].PreDiscountAmount);
		AssertEquals("min spend line ETL PostDiscountAmount", 2250m, usageListETL[2].PostDiscountAmount);
		AssertEquals("min spend line ETL UnitCount", 1m, usageListETL[2].UnitCount);
		AssertEquals(3, usageListETL.Count);

		AssertEquals(600m + 2000m + 5000m + 1000m, bill1.InvoicePreDiscountTotal);
		AssertEquals(1500m * 10m + 15 * 400 + 1000 * 20 + 500 * 15, bill1.MonthlyUsages.First().TotalUsedLicenceUnits);

		//PriceItemFilter
		billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
		billing1.AccumulateMonths = 0;
		billing1.DateTo = period1.AddMonths(1).AddDays(-1);
		AddPriceItemFilterStrips(billing1.PriceItemFilter, new FilterStripDefinition
		{
			FilterStripName = "Code",
			FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "USR",
		});
		billing1.GenerateReport(null);
		bill1 = billing1.Bills[0];
		AssertNoNotifications(bill1);
		var linesAsText = string.Join("\r\n", bill1.MonthlyUsages.First().UsageLines.OrderBy(x => x.PriceItemCode)
.Select(x => $"{x.PriceItemCode} | {x.Price} | {x.PriceCurrency} | {x.LicenceUnits.ToZInt()}"));
		AssertEquals(@"USR | 40.000000 | AUD | 400", linesAsText);
	}

	public void TestGenerateReport_PriceItemFilter()
	{
		var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
		lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

		var prices = lic.Company.PriceHeaders.AddNew();
		var item1 = prices.Items.AddNew();
		item1.L7_Category = BillingConstants.BillingSystem.STL;
		item1.L7_Code = "USR";
		item1.L7_Order = 1;
		var rate1 = item1.CurrencyRates.AddNew();
		rate1.PIR_RX_NKCurrency = "AUD";
		rate1.PIR_Price = 17.5;

		var item2 = prices.Items.AddNew();
		item2.L7_Category = BillingConstants.BillingSystem.STL;
		item2.L7_Code = "ABC";
		item2.L7_Order = 5;
		var rate2 = item2.CurrencyRates.AddNew();
		rate2.PIR_RX_NKCurrency = "AUD";
		rate2.PIR_Price = 20;

		var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
		priceHeaderLink.PHL_L6 = prices.PK;
		priceHeaderLink.PHL_RX_NKCurrency = "AUD";
		priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

		BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic.ClientCompany, 10);
		BillingTestHelper.CreateChargeableUsage(Factory, "STL", "ABC", new ZDateTime(2015, 7, 1), lic.ClientCompany, 20);

		Factory.Save();

		var billing = new StlBilling(Factory);
		billing.DateTo = new ZDateTime(2015, 7, 31);

		billing.GenerateReport(null);
		AssertEquals(1, billing.Bills.Count);
		AssertEquals(false, billing.IsPriceItemFilterApplicableOnReport);
		var bill = billing.Bills[0];
		var monthlyUsage = bill.MonthlyUsages.First();
		var usageLines = monthlyUsage.UsageLines.ToArray();
		AssertEquals(2, usageLines.Length);
		var line1 = usageLines[0];
		AssertEquals("USR", line1.PriceItemCode);
		AssertEquals(10m, line1.UnitCount);
		var line2 = usageLines[1];
		AssertEquals("ABC", line2.PriceItemCode);
		AssertEquals(20m, line2.UnitCount);

		var filter = billing.PriceItemFilter;
		AddPriceItemFilterStrips(filter, new FilterStripDefinition
		{
			FilterStripName = "Code",
			FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "USR",
		});
		billing.GenerateReport(null);
		AssertEquals(1, billing.Bills.Count);
		AssertEquals(true, billing.IsPriceItemFilterApplicableOnReport);
		bill = billing.Bills[0];
		monthlyUsage = bill.MonthlyUsages.First();
		usageLines = monthlyUsage.UsageLines.ToArray();
		AssertEquals(1, usageLines.Length);
		line1 = usageLines[0];
		AssertEquals("USR", line1.PriceItemCode);
		AssertEquals(10m, line1.UnitCount);
	}

	public void TestCheckPriceItemFilter()
	{
		var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
		lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;

		var prices = lic.Company.PriceHeaders.AddNew();
		var item1 = prices.Items.AddNew();
		item1.L7_Category = BillingConstants.BillingSystem.STL;
		item1.L7_Code = "USR";
		item1.L7_Order = 1;
		var rate1 = item1.CurrencyRates.AddNew();
		rate1.PIR_RX_NKCurrency = "AUD";
		rate1.PIR_Price = 17.5;

		var item2 = prices.Items.AddNew();
		item2.L7_Category = BillingConstants.BillingSystem.STL;
		item2.L7_Code = "ABC";
		item2.L7_Order = 5;
		var rate2 = item2.CurrencyRates.AddNew();
		rate2.PIR_RX_NKCurrency = "AUD";
		rate2.PIR_Price = 20;

		var priceHeaderLink = lic.Database.PriceHeaderLinks.AddNew();
		priceHeaderLink.PHL_L6 = prices.PK;
		priceHeaderLink.PHL_RX_NKCurrency = "AUD";
		priceHeaderLink.PHL_ValidFrom = new ZDateTime(2015, 7, 1);

		BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2015, 7, 1), lic.ClientCompany, 10);
		BillingTestHelper.CreateChargeableUsage(Factory, "STL", "ABC", new ZDateTime(2015, 7, 1), lic.ClientCompany, 20);
		Factory.Save();

		var bizObj = new StlBilling(Factory);
		bizObj.DateTo = new ZDateTime(2015, 7, 31);

		using (var form = new StlBillingForm(bizObj))
		{
			form.Show();
			bizObj.GenerateReport(null);
			AssertEquals(false, bizObj.IsPriceItemFilterApplicableOnReport);
			AssertButtonClick(form, "createForEditButton", "Please select one and only one row without errors and not already invoiced.");
			AssertButtonClick(form, "CreateInvoicesButton", "Please select one or more rows without errors and not already invoiced.");

			var filter = bizObj.PriceItemFilter;
			AddPriceItemFilterStrips(filter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Code",
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "USR",
			});
			bizObj.GenerateReport(null);
			AssertEquals(true, bizObj.IsPriceItemFilterApplicableOnReport);
			AssertButtonClick(form, "createForEditButton", "Invoice(s) cannot be generated or posted while a Price Item Filter is active.");
			AssertButtonClick(form, "CreateInvoicesButton", "Invoice(s) cannot be generated or posted while a Price Item Filter is active.");
		}
	}

	public static void AddPriceItemFilterStrips(StmModuleFilter stmFilter, params FilterStripsTestHelper.FilterStripDefinition[] filterStripDefs)
	{
		var filters = ObjectFactory.Get("BillingPrices_FilterStripBusinessObject") as FilterStripBusinessObject;

		foreach (var filterStripDef in filterStripDefs)
		{
			var filterStrip = filters.FilterStrips.AddNew(filterStripDef.FilterStripName);
			filterStrip.OrCategory = filterStripDef.OrCategory;

			var filter = filterStrip.CurrentModuleFilter;
			filterStripDef.FilterStripValueSetter(filter);
		}

		filters.WriteFilterStripsToXml(stmFilter, filters.FilterStrips, new EmptyLayoutsHelper());
	}

	static MinSpendLicenceSetting AddMinSpendSetting(LicenceDatabase db, ZDateTime validFrom, UsageCodeKey usageCode, decimal price)
	{
		var setting = db.Factory.New<MinSpendLicenceSetting>();
		setting.LS9_LD = db.PK;
		setting.PriceKey = usageCode;
		setting.LS9_Price = price;
		setting.LS9_ValidFrom = validFrom;
		db.LicenceSettings.Add(setting);
		return setting;
	}

	void AssertButtonClick(ZForm form, string buttonName, string lastMessage)
	{
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		UnitTestUserNotification.Instance.AddYesAnswer();
		var button = form.Controls.Find(buttonName, true).OfType<ZButton>().Single();
		button.PerformClick();
		AssertEquals(lastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
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
}

[TestedType(typeof(StlBilling))]
sealed class StlBillingRelatedFilterTest : RelatedModuleFilterSupportableTestCase<StlBilling>
{
	protected override IEnumerable<FilterRuleTestSet> GetFilterRules(StlBilling businessObject)
	{
		return new[] { new FilterRuleTestSet(null, () => businessObject.PriceItemFilter, "BillingPricesFilterBusinessObject") };
	}

	protected override void ValidateBusinessObject(StlBilling businessObject)
	{
		businessObject.ValidateFilterStrips();
	}

	protected override string FilterDescriptionForValidationTest => "Header Created By";

	protected override StlBilling GetNewBusinessObject()
	{
		var bizObj = new StlBilling(Factory);
		return bizObj;
	}

	public override void TestDeleteBusinessObject_ShouldDeleteFilters() => Assert(true);
}
