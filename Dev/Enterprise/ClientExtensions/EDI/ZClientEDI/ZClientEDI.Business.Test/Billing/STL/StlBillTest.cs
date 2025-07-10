using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlBill))]
	sealed class StlBillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganisationRelated()
		{
			AssertEquals(organisation, billRecipient.Organisation);
			AssertEquals(organisation.LicCompany, billRecipient.LicCompany);
		}

		[TestDate(2016, 3, 9)]
		public void TestFee()
		{
			var periodStart = new ZDateTime(2015, 7, 1);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			BillingTestHelper.CreateExchangeRate(Factory, "EUR", 2, periodStart); // 1 AUD = 2 EUR
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 3, periodStart);

			var feeEUR = BillingTestHelper.CreateLicenceFee(licence.Company, "AA1", "desc 1", 50, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			feeEUR.L8_RX_NKCurrency = "EUR";

			var feeUSD = BillingTestHelper.CreateLicenceFee(licence.Company, "AA2", "desc 2", 75, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			feeUSD.L8_RX_NKCurrency = "USD";

			Factory.Save();

			var bill = NewBill("AUD");
			var feeEURUsage = new FeeUsage(feeEUR);
			feeEURUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);

			var feeUSDUsage = new FeeUsage(feeUSD);
			feeUSDUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);

			bill.AddFees(new[] { feeEURUsage, feeUSDUsage });

			var invoice = bill.CreateInvoices(ZDateTime.Empty).Single();
			ARInvoiceLine[] invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();
			var line1 = invoiceLines.First(x => x.AL_Desc.StartsWith("desc 1"));
			var line2 = invoiceLines.First(x => x.AL_Desc.StartsWith("desc 2"));

			AssertEquals(50 / 2.0m, line1.AL_OSExTaxAmount);
			AssertEquals(75 / 3.0m, line2.AL_OSExTaxAmount);

			TestDateAttribute.Date = new DateTime(2016, 3, 12);
			var chargeableUsages = Factory.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals(2, chargeableUsages.Length);
			var chargeableUsageEUR = chargeableUsages.Single(x => x.U1_Parent == feeEUR.PK);
			var chargeableUsageUSD = chargeableUsages.Single(x => x.U1_Parent == feeUSD.PK);

			bill = NewBill("AUD");
			feeEURUsage = new FeeUsage(feeEUR, chargeableUsageEUR);
			feeEURUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);

			feeUSDUsage = new FeeUsage(feeUSD, chargeableUsageUSD);
			feeUSDUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);
			bill.AddFees(new[] { feeEURUsage, feeUSDUsage });

			var invoice2 = bill.CreateInvoices(ZDateTime.Empty).Single();
			chargeableUsages = new BusinessObjectFactory() { RefreshEnabled = false }.Load<ClientChargeableUsage>(new ZQuery());
			AssertEquals(2, chargeableUsages.Length);
			chargeableUsageEUR = chargeableUsages.Single(x => x.U1_Parent == feeEUR.PK);
			chargeableUsageUSD = chargeableUsages.Single(x => x.U1_Parent == feeUSD.PK);
			AssertEquals(invoice2.PK, chargeableUsageEUR.U1_AH_Invoice);
			AssertEquals(invoice2.PK, chargeableUsageUSD.U1_AH_Invoice);
			AssertEquals(TestDateAttribute.Date, chargeableUsageEUR.U1_UpdateTime);
			AssertEquals(TestDateAttribute.Date, chargeableUsageUSD.U1_UpdateTime);
		}

		[TestDate(2016, 3, 9)]
		public void TestFee_TaxDate()
		{
			var periodStart = new ZDateTime(2015, 7, 1);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var fee1 = BillingTestHelper.CreateLicenceFee(licence.Company, "AA1", "desc 1", 50, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			fee1.L8_RX_NKCurrency = "AUD";
			fee1.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtCurrentDate;

			var fee2 = BillingTestHelper.CreateLicenceFee(licence.Company, "AA2", "desc 2", 75, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			fee2.L8_RX_NKCurrency = "AUD";
			fee2.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtStartDate;

			var fee3 = BillingTestHelper.CreateLicenceFee(licence.Company, "AA3", "desc 3", 100, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			fee3.L8_RX_NKCurrency = "AUD";
			fee3.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate;

			var fee4Annual = BillingTestHelper.CreateLicenceFee(licence.Company, "AA3", "desc 4", 125, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty);
			fee4Annual.L8_RX_NKCurrency = "AUD";
			fee4Annual.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate;
			fee4Annual.L8_RenewalMonths = 12;

			Factory.Save();

			var bill = NewBill("AUD");
			var ownerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);
			var fee1Usage = new FeeUsage(fee1);
			fee1Usage.OwnerDelivery = ownerDelivery;

			var fee2Usage = new FeeUsage(fee2);
			fee2Usage.OwnerDelivery = ownerDelivery;

			var fee3Usage = new FeeUsage(fee3);
			fee3Usage.OwnerDelivery = ownerDelivery;

			var fee4Usage = new FeeUsage(fee4Annual);
			fee4Usage.OwnerDelivery = ownerDelivery;

			bill.AddFees(new[] { fee1Usage, fee2Usage, fee3Usage, fee4Usage });

			var invoice = bill.CreateInvoices(ZDateTime.Empty).Single();
			ARInvoiceLine[] invoiceLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => !x.AL_Desc.IsEmpty).ToArray();
			var line1 = invoiceLines.First(x => x.AL_Desc.StartsWith("desc 1"));
			var line2 = invoiceLines.First(x => x.AL_Desc.StartsWith("desc 2"));
			var line3 = invoiceLines.First(x => x.AL_Desc.StartsWith("desc 3"));
			var line4 = invoiceLines.First(x => x.AL_Desc.StartsWith("desc 4"));
			CombineAssertions(() =>
			{
				AssertEquals("invoice tax date when fee tax date is invoice date", TestDateAttribute.Date, line1.AL_TaxDate.ToDateTime());
				AssertEquals("invoice tax date when fee tax date is period start date", periodStart.ToDateTime(), line2.AL_TaxDate.ToDateTime());
				AssertEquals("invoice tax date when fee tax date is monthly period end date", periodStart.AddMonths(1).AddDays(-1).ToDateTime(), line3.AL_TaxDate.ToDateTime());
				AssertEquals("invoice tax date when fee tax date is annual period end date", periodStart.AddMonths(12).AddDays(-1).ToDateTime(), line4.AL_TaxDate.ToDateTime());
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrepaymentForNextMonth()
		{
			EDIDataRegistry.Instance.PrepaymentInvoiceComment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Prepay me");

			BillingTestHelper.LoadClientSpecificDocuments();

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var userPrice = prices.Items.AddNew();
			userPrice.L7_Code = "USR";
			userPrice.L7_FeeType = BillingConstants.FeeType.Transactional;
			userPrice.L7_Price = 3.00;
			userPrice.L7_Description = "Users";
			userPrice.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			userPrice.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			Factory.Save();

			var monthlyUsage = NewMonthlyUsage();

			var usageLine = new UsageLine(Factory);
			usageLine.PeriodStart = new ZDateTime(2015, 10, 1);
			usageLine.PriceItem = userPrice;
			usageLine.Price = 3.00;
			usageLine.PriceCurrency = "AUD";
			usageLine.TotalUnitCount = 700;
			var discountInfo = new DiscountInfo();
			var setting = Factory.New<DiscountLicenceSetting>();
			setting.LS9_Percent = 10.0;
			discountInfo.Init(null, setting, monthlyUsage);

			monthlyUsage.HasPrepaid = true;
			monthlyUsage.IsPrepaymentDiscountAvailable = true;
			var prepayment = new PrepaymentStlDiscount(discountInfo);
			var discountSet = new StlBilling.DiscountSet(new IStlDiscount[] { prepayment });
			usageLine.Discounts = discountSet;
			monthlyUsage.AddUsageLine(usageLine);
			StlBilling.ApplyDiscounts(usageLine);
			var bill = NewBill("AUD");
			var prepaid = bill.Prepaid = new StlBill.PrepaidDetail();
			prepaid.PredeterminedPrepaidBalance = 199;
			prepaid.PredeterminedPrepaidBalanceCurrency = "AUD";
			prepaid.FuturePredeterminedPrepaidBalance = 299;
			prepaid.FuturePredeterminedPrepaidBalanceCurrency = "AUD";
			prepaid.PrepaidDepositBalance = 255;
			prepaid.PrepaidDepositBalanceCurrency = "AUD";
			prepaid.ShouldUseOutstandingBalanceAsPrepaidDepositBalance = true;
			bill.AddMonthlyUsages(new[] { monthlyUsage });
			bill.OutstandingBalanceExDepositLocal = 100;
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 10, 31));
			var bills = new StlBillCollection(Factory);
			bills.Add(bill);
			StlBilling.CalculateCurrencyAndSurchargeAmounts(context, bills.Cast<StlBill>());

			bill.CreateInvoicesWithoutSave().Single();

			// initial credit     :   100
			// undiscounted amount:  2100
			// undiscounted + tax :  2310 (2100 * 1.1)
			// discounted amount  :  1890  (0.9 * 2100)
			// invoice amount     :  2079  (1890 + 10% tax)
			// balance now        : -1979  (100 - 2079)

			AssertEquals(199m, bill.PrepayNext.PrepaymentBalanceRequired);
			AssertEquals(299m, bill.PrepayNext.FuturePrepaymentBalanceRequired);
			AssertEquals(100m, bill.PrepayNext.CurrentPrepaymentBalance);
			AssertEquals(2079m, bill.PrepayNext.CurrentInvoiceTotalAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrepaymentForNextMonth_ForeignCurrency()
		{
			EDIDataRegistry.Instance.PrepaymentInvoiceComment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Prepay me");

			BillingTestHelper.LoadClientSpecificDocuments();

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.8);
			licence.Company.InvoiceDeliveries[0].L9_RX_NKInvoiceCurrency = "USD";

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "USD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var userPrice = prices.Items.AddNew();
			userPrice.L7_Code = "USR";
			userPrice.L7_FeeType = BillingConstants.FeeType.Transactional;
			userPrice.L7_Price = 3.00;
			userPrice.L7_Description = "Users";
			userPrice.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			userPrice.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			Factory.Save();

			var monthlyUsage = NewMonthlyUsage();

			var usageLine = new UsageLine(Factory);
			usageLine.PeriodStart = new ZDateTime(2015, 10, 1);
			usageLine.PriceItem = userPrice;
			usageLine.Price = 3.00;
			usageLine.PriceCurrency = "USD";
			usageLine.TotalUnitCount = 700;
			var discountInfo = new DiscountInfo();
			var setting = Factory.New<DiscountLicenceSetting>();
			setting.LS9_Percent = 10.0;
			discountInfo.Init(null, setting, monthlyUsage);

			monthlyUsage.HasPrepaid = true;
			monthlyUsage.IsPrepaymentDiscountAvailable = true;
			var prepayment = new PrepaymentStlDiscount(discountInfo);
			var discountSet = new StlBilling.DiscountSet(new IStlDiscount[] { prepayment });
			usageLine.Discounts = discountSet;
			monthlyUsage.AddUsageLine(usageLine);
			StlBilling.ApplyDiscounts(usageLine);
			var bill = NewBill("USD");
			var prepaid = bill.Prepaid = new StlBill.PrepaidDetail();
			prepaid.PredeterminedPrepaidBalance = 299;
			prepaid.PredeterminedPrepaidBalanceCurrency = "AUD";
			prepaid.FuturePredeterminedPrepaidBalance = 399;
			prepaid.FuturePredeterminedPrepaidBalanceCurrency = "AUD";
			prepaid.PrepaidDepositBalance = 545;
			prepaid.PrepaidDepositBalanceCurrency = "AUD";
			bill.AddMonthlyUsages(new[] { monthlyUsage });
			bill.OutstandingBalanceExDepositLocal = 100;
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 10, 31));
			var bills = new StlBillCollection(Factory);
			bills.Add(bill);
			StlBilling.CalculateCurrencyAndSurchargeAmounts(context, bills.Cast<StlBill>());

			bill.CreateInvoicesWithoutSave().Single();

			// USD
			// initial credit     :    80
			// undiscounted amount:  2100
			// undiscounted + tax :  2310 (2100 * 1.1)
			// discounted amount  :  1890  (0.9 * 2100)
			// invoice amount     :  2079  (1890 + 10% tax)
			// balance now        : -1999  (80 - 2079)

			AssertEquals(239.2m, bill.PrepayNext.PrepaymentBalanceRequired);
			AssertEquals(319.2m, bill.PrepayNext.FuturePrepaymentBalanceRequired);
			AssertEquals(80m, bill.PrepayNext.CurrentPrepaymentBalance);
			AssertEquals(2079m, bill.PrepayNext.CurrentInvoiceTotalAmount);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrepaymentDetails()
		{
			EDIDataRegistry.Instance.PrepaymentInvoiceComment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Prepay me");

			BillingTestHelper.LoadClientSpecificDocuments();

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var userPrice = prices.Items.AddNew();
			userPrice.L7_Code = "USR";
			userPrice.L7_FeeType = BillingConstants.FeeType.Transactional;
			userPrice.L7_Price = 3.00;
			userPrice.L7_Description = "Users";
			userPrice.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			userPrice.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;

			Factory.Save();

			var monthlyUsage = NewMonthlyUsage();

			var usageLine = new UsageLine(Factory);
			usageLine.PeriodStart = new ZDateTime(2015, 10, 1);
			usageLine.PriceItem = userPrice;
			usageLine.Price = 3.00;
			usageLine.PriceCurrency = "AUD";
			usageLine.TotalUnitCount = 700;

			monthlyUsage.HasPrepaid = false;
			monthlyUsage.IsPrepaymentDiscountAvailable = false;
			monthlyUsage.AddUsageLine(usageLine);
			var bill = NewBill("AUD");
			var prepaid = bill.Prepaid = new StlBill.PrepaidDetail();
			prepaid.PredeterminedPrepaidBalance = 199;
			prepaid.PredeterminedPrepaidBalanceCurrency = "AUD";
			prepaid.FuturePredeterminedPrepaidBalance = 299;
			prepaid.FuturePredeterminedPrepaidBalanceCurrency = "AUD";
			prepaid.PrepaidDepositBalance = 255;
			prepaid.PrepaidDepositBalanceCurrency = "AUD";
			prepaid.ShouldUseOutstandingBalanceAsPrepaidDepositBalance = true;
			bill.AddMonthlyUsages(new[] { monthlyUsage });
			bill.OutstandingBalanceExDepositLocal = 100;
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 10, 31));
			var bills = new StlBillCollection(Factory);
			bills.Add(bill);
			StlBilling.CalculateCurrencyAndSurchargeAmounts(context, bills.Cast<StlBill>());

			bill.CreateInvoicesWithoutSave().Single();
			AssertEquals(199m, bill.PrepayNext.PrepaymentBalanceRequired);
			AssertEquals(299m, bill.PrepayNext.FuturePrepaymentBalanceRequired);
			AssertEquals(100m, bill.PrepayNext.CurrentPrepaymentBalance);
			AssertEquals(0m, bill.PrepayNext.CurrentInvoiceTotalAmount);
		}

		public void TestCreateInvoice_CommissionCreator()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			var stlBill = NewBill();
			var invoice = stlBill.CreateInvoices(ZDateTime.Empty).Single();
			var commissionCreator = invoice.CommissionCreatorOverride;
			AssertType(typeof(BillingCommissionCreator), commissionCreator);
			AssertType(typeof(StlBilledUsageCommissionGroupsCalculator), ((BillingCommissionCreator)commissionCreator).UsageGroupsCalculator);
		}

		public void TestCreateInvoice_SpecialTax()
		{
			var gstTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var freeTaxRate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "FREEGST", AccTaxRate.Types.Rated, 0);
			var chargeCode = BillingTestHelper.CreateChargeCode(Factory, null, "STLUSAGE");

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

			var key1 = new UsageCodeKey(BillingConstants.BillingSystem.Service, "#HU");
			var prices = BillingTestHelper.CreateStlPriceList(licence.Company);
			BillingTestHelper.AddPriceItem(prices, key1, BillingConstants.FeeType.Transactional, 100m)
				.L7_ChargeCode = "STLUSAGE";
			BillingTestHelper.CreatePriceLink(licence.Database, prices, new ZDateTime(2015, 7, 1));

			LicenceCompany.ClearStandardPricesCompanyCache();
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var ldsPriceList = stdLicCompany.PriceHeaders.AddNew();
			ldsPriceList.L6_PricelistVersion = "V1";
			ldsPriceList.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			ldsPriceList.L6_RX_NKCurrency = "USD";
			ldsPriceList.L6_ValidFrom = new ZDateTime(2015, 1, 1);

			var key2 = new UsageCodeKey(BillingConstants.BillingSystem.Service, "LS3");
			var ldsPriceItem = BillingTestHelper.AddPriceItem(ldsPriceList, key2, BillingConstants.FeeType.PerDevicePerMonth, 100m);
			ldsPriceItem.L7_Description = "Standard HandHeld Scanner Device - MC3200-GL3H24E0A (CargoWise Device)";
			ldsPriceItem.L7_ChargeCode = "STLUSAGE";
			var ldsRate1 = ldsPriceItem.CurrencyRates.AddNew();
			ldsRate1.PIR_RX_NKCurrency = "AUD";
			ldsRate1.PIR_Price = 125m;

			var service1 = licence.Database.PremiumServices.AddNew();
			service1.CPS_Type = key1.Code;
			service1.CPS_Units = 5;
			service1.CPS_StartDate = new ZDateTime(2015, 6, 1);
			service1.CPS_DisplayOrder = 0;

			var service2 = licence.Database.PremiumServices.AddNew();
			service2.CPS_Type = key2.Code;
			service2.CPS_Units = 10;
			service2.CPS_StartDate = new ZDateTime(2015, 6, 1);
			service2.CPS_DisplayOrder = 1;
			service2.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = new ZDateTime(2015, 7, 31);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			var monthlyUsage = bill.MonthlyUsages.First();

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(monthlyUsage, lines, new Dictionary<string, DepositBalance>());

			AssertEquals(2, lines.Count);
			var line1 = lines.First(x => x.Tax.Code == "FREEGST");
			AssertEquals(500m, line1.Amount);
			var line2 = lines.First(x => x.Tax.Code == "GST");
			AssertEquals(1250m, line2.Amount);

			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_MultipleSubAccount()
		{
			var department1 = BillingTestHelper.FindOrCreateDepartment(Factory, "DD1");
			var department2 = BillingTestHelper.FindOrCreateDepartment(Factory, "DD2");
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.SecurityDepositChargeCode);
			BillingTestHelper.LoadClientSpecificDocuments();

			Factory.Save();

			var charges = Factory.Load<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, EDIDataRegistry.Instance.OdplDepositChargeCode.Value));
			AssertEquals(1, charges.Length);
			var subAccount = charges[0].RevenueAccount.SubAccountTypes.AddNew();
			subAccount.ASA_IsSubClassValidationRuleMandatory = true;
			subAccount.ASA_SubClass = OrgHeaderSchema.Constants.Prefix;

			Factory.Save();

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var userPrice = prices.Items.AddNew();
			userPrice.L7_Code = "USR";
			userPrice.L7_FeeType = BillingConstants.FeeType.Transactional;
			userPrice.L7_Price = 3.00;
			userPrice.L7_Description = "Users";
			userPrice.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			userPrice.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			userPrice.L7_DepositChargeCode = EDIDataRegistry.SecurityDepositChargeCode;

			var conversionCredit = Factory.New<ConversionCreditLicenceSetting>();
			licence.Database.LicenceSettings.Add(conversionCredit);
			conversionCredit.LS9_GE_Department1 = department1.PK;
			conversionCredit.LS9_GE_Department2 = department2.PK;
			conversionCredit.CreditChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			conversionCredit.LS9_ValidFrom = new ZDateTime(2015, 1, 1);
			conversionCredit.LS9_Price = 100;
			conversionCredit.LS9_RX_NKPriceCurrency = "AUD";

			Factory.Save();

			BillingTestHelper.SetDepositBalance(licence.Company.LC_OH.ToGuid(), EDIDataRegistry.Instance.OdplDepositChargeCode.Value, 10000m, 1000m, "AUD");

			var usageLine = new UsageLine(Factory);
			usageLine.PeriodStart = new ZDateTime(2015, 10, 1);
			usageLine.PriceItem = userPrice;
			usageLine.Price = 3.00;
			usageLine.PriceCurrency = "AUD";
			usageLine.TotalUnitCount = 700;
			usageLine.SetAmounts(2100, 2100);

			var monthlyUsage = NewMonthlyUsage();
			monthlyUsage.AddUsageLine(usageLine);

			var bill = NewBill("AUD");
			bill.AddMonthlyUsages(new[] { monthlyUsage });
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 10, 31));
			var bills = new StlBillCollection(Factory);
			bills.Add(bill);
			StlBilling.CalculateCurrencyAndSurchargeAmounts(context, bills.Cast<StlBill>());

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var depositLine = invoice.Lines.Cast<ARInvoiceLine>().Single(x => x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
			AssertEquals(1, depositLine.SubAccounts.Count);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, depositLine.SubAccounts[0].AL1_SubClassParentTableCode);
			AssertEquals("Line ORG sub account ID for billing should be same as transaction header GL account PK", invoice.AH_OH, depositLine.SubAccounts[0].AL1_SubClassParentId);
		}

		public void TestCreateInvoice_CalculateOtherTaxes()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			Factory.Save();

			var stlBill = NewBill();
			var invoice = stlBill.CreateInvoices(ZDateTime.Empty).Single();

			var mock = new Mock<ITaxProcessor>();
			mock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("ProcessTaxesOnPosting");
			using (ObjectFactory.Substitute(mock.Object))
			{
				var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
				config.ETC_ParentId = invoice.Company.PK;
				Factory.Save();
				AssertExceptionThrown<InvalidOperationException>("AssertExceptionThrown", "ProcessTaxesOnPosting", () => stlBill.CreateInvoices(ZDateTime.Empty));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown()
		{
			var periodStart = new ZDateTime(2015, 7, 1);
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var eHubChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "EHUB");
			var uHubChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "UHUB");
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			BillingTestHelper.LoadClientSpecificDocuments();

			var feeEUR = BillingTestHelper.CreateLicenceFee(licence.Company, "AA1", "desc 1", 50, "EHUB", periodStart, ZDateTime.Empty);
			feeEUR.L8_RX_NKCurrency = "EUR";

			var feeUSD = BillingTestHelper.CreateLicenceFee(licence.Company, "AA2", "desc 2", 150, "UHUB", periodStart, ZDateTime.Empty);
			feeUSD.L8_RX_NKCurrency = "USD";

			var service = BillingTestHelper.CreatePremiumService(licence.Database, "CCC", periodStart, ZDateTime.Empty);
			service.CPS_Units = 10;

			var npService = BillingTestHelper.CreatePremiumService(licence.Database, "#NP", periodStart, ZDateTime.Empty);
			npService.CPS_Units = 1;
			npService.IsSystemLicenceFee = true;

			BillingTestHelper.CreateExchangeRate(Factory, "EUR", 0.5, periodStart);
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.75, periodStart);

			Factory.Save();

			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "#PA");
			priceItemA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemA.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			var priceItemB = Factory.New<ClientLicencePriceItem>();
			priceItemB.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "#PB");
			priceItemB.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemB.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			var priceItemC = Factory.New<ClientLicencePriceItem>();
			priceItemC.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "CCC");
			priceItemC.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemC.L7_Price = 10m;
			var priceItemD = Factory.New<ClientLicencePriceItem>();
			priceItemD.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "#NP");
			priceItemD.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemD.L7_Price = 150m;

			var bill = NewBill("EUR");

			var chargeableUsageA = Factory.New<ClientChargeableUsage>();
			chargeableUsageA.U1_PeriodStart = periodStart;
			chargeableUsageA.U1_Code = "AAA";
			chargeableUsageA.U1_SubCode = "AA1";

			var chargeableUsageB = Factory.New<ClientChargeableUsage>();
			chargeableUsageB.U1_PeriodStart = periodStart;
			chargeableUsageB.U1_Code = "BBB";
			chargeableUsageB.U1_SubCode = "BB1";

			var chargeableUsageC = Factory.New<ClientChargeableUsage>();
			chargeableUsageC.U1_PeriodStart = periodStart;
			chargeableUsageC.U1_Code = BillingConstants.BillingSystem.Service;
			chargeableUsageC.U1_SubCode = "CCC";

			var chargeableUsageD = Factory.New<ClientChargeableUsage>();
			chargeableUsageD.U1_PeriodStart = periodStart;
			chargeableUsageD.U1_Code = BillingConstants.BillingSystem.Service;
			chargeableUsageD.U1_SubCode = "#NP";

			var monthlyUsage = NewMonthlyUsage();
			{
				var usageLineA = new UsageLine(Factory);
				usageLineA.PriceItem = priceItemA;
				usageLineA.TotalUnitCount = 4;
				usageLineA.Price = 25;
				usageLineA.PriceCurrency = "AUD";
				usageLineA.SetAmounts(100, 75);
				usageLineA.LocalPreDiscountAmount = 200;
				usageLineA.LocalPostDiscountAmount = 150;

				usageLineA.AddUsage(new Usage(chargeableUsageA) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLineA);
			}

			{
				var usageLineB = new UsageLine(Factory);
				usageLineB.PriceItem = priceItemB;
				usageLineB.TotalUnitCount = 2;
				usageLineB.Price = 25;
				usageLineB.PriceCurrency = "AUD";
				usageLineB.SetAmounts(50, 10);
				usageLineB.LocalPreDiscountAmount = 100;
				usageLineB.LocalPostDiscountAmount = 20;

				usageLineB.AddUsage(new Usage(chargeableUsageB) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLineB);
			}

			var discount10 = Factory.New<EdiPriceHeaderDiscount>();
			{
				var usageLineC = new UsageLine(Factory);
				usageLineC.PriceItem = priceItemC;
				usageLineC.TotalUnitCount = 10;
				usageLineC.Price = 10;
				usageLineC.PriceCurrency = "AUD";
				usageLineC.SetAmounts(100, 90);
				usageLineC.LocalPreDiscountAmount = 100;
				usageLineC.LocalPostDiscountAmount = 90;
				usageLineC.AddUsage(new Usage(chargeableUsageC, service, periodStart, null) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				discount10.PHD_Name = "D10";
				discount10.PHD_Version = "STL1";
				discount10.PHD_Type = BillingConstants.DiscountCalculator.Percentage;

				var setting = Factory.New<DiscountLicenceSetting>();
				setting.LS9_Percent = 10m;
				var discountInfo = new DiscountInfo();
				discountInfo.Init(discount10, setting, monthlyUsage, null);
				var stlDiscount = new PercentageStlDiscount(discountInfo);
				usageLineC.Discounts = new StlBilling.DiscountSet(new IStlDiscount[] { stlDiscount });

				monthlyUsage.AddUsageLine(usageLineC);
			}

			{
				var usageLineD = new UsageLine(Factory);
				usageLineD.PriceItem = priceItemD;
				usageLineD.TotalUnitCount = 1;
				usageLineD.Price = 150;
				usageLineD.PriceCurrency = "AUD";
				usageLineD.SetAmounts(150, 150);
				usageLineD.LocalPreDiscountAmount = 150;
				usageLineD.LocalPostDiscountAmount = 150;
				usageLineD.AddUsage(new Usage(chargeableUsageD, npService, periodStart, chargeableUsageD.ClientCompany) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });
				monthlyUsage.AddUsageLine(usageLineD);
			}

			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var feeEURUsage = new FeeUsage(feeEUR);
			feeEURUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);

			var feeUSDUsage = new FeeUsage(feeUSD);
			feeUSDUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);

			bill.AddFees(new[] { feeEURUsage, feeUSDUsage });

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);

			CombineAssertions(() =>
			{
				var priceItemAUsage = billedUsages.Single(x => x.BU9_L7 == priceItemA.PK);
				AssertEquals("BU9_PeriodStart", periodStart, priceItemAUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "AAA", priceItemAUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", priceItemAUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PA", priceItemAUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", priceItemAUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 4m, priceItemAUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemAUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 100m, priceItemAUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 75m, priceItemAUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 200m, priceItemAUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 150m, priceItemAUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemAUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemAUsage.BU9_AC_DiscountChargeCode);
			});

			CombineAssertions(() =>
			{
				var priceItemBUsage = billedUsages.Single(x => x.BU9_L7 == priceItemB.PK);
				AssertEquals("BU9_PeriodStart", periodStart, priceItemBUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "BBB", priceItemBUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "BB1", priceItemBUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PB", priceItemBUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", priceItemBUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 2m, priceItemBUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemBUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 50m, priceItemBUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 10m, priceItemBUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 100m, priceItemBUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 20m, priceItemBUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemBUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemBUsage.BU9_AC_DiscountChargeCode);
			});

			CombineAssertions(() =>
			{
				var feeEhubUsage = billedUsages.Single(x => x.BU9_UsageCode == "FEE" && x.BU9_UsageSubCode == "AA1");
				AssertEquals("BU9_PeriodStart", periodStart, feeEhubUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "FEE", feeEhubUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", feeEhubUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "", feeEhubUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "EUR", feeEhubUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, feeEhubUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 50m, feeEhubUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 50m, feeEhubUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 50m, feeEhubUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 100m, feeEhubUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 100m, feeEhubUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", eHubChargeCode.PK, feeEhubUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", ZGuid.Empty, feeEhubUsage.BU9_AC_DiscountChargeCode);
			});

			CombineAssertions(() =>
			{
				var feeUhubUsage = billedUsages.Single(x => x.BU9_UsageCode == "FEE" && x.BU9_UsageSubCode == "AA2");
				AssertEquals("BU9_PeriodStart", periodStart, feeUhubUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "FEE", feeUhubUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA2", feeUhubUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "", feeUhubUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "EUR", feeUhubUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, feeUhubUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 100m, feeUhubUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 100m, feeUhubUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 100m, feeUhubUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 200m, feeUhubUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 200m, feeUhubUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", uHubChargeCode.PK, feeUhubUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", ZGuid.Empty, feeUhubUsage.BU9_AC_DiscountChargeCode);
			});

			CombineAssertions(() =>
			{
				var serviceUsage = billedUsages.Single(x => x.BU9_UsageCode == "SVC" && x.BU9_PriceCode == "CCC");
				AssertEquals("BU9_PeriodStart", periodStart, serviceUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "SVC", serviceUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "CCC", serviceUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "CCC", serviceUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", serviceUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 10m, serviceUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 10m, serviceUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 100m, serviceUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 90m, serviceUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 100m, serviceUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 90m, serviceUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, serviceUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", ZGuid.Empty, serviceUsage.BU9_AC_DiscountChargeCode);

				var billedDiscounts = serviceUsage.Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, serviceUsage.PK));
				AssertEquals(1, billedDiscounts.Length);
				AssertEquals(10m, billedDiscounts[0].BD9_Percent);
				AssertEquals(discount10.PK, billedDiscounts[0].BD9_PHD_Discount);
				AssertEquals(10m, billedDiscounts[0].BD9_TransactionAmount);
				AssertEquals("", billedDiscounts[0].BD9_Type);
			});

			CombineAssertions(() =>
			{
				var npServiceUsage = billedUsages.Single(x => x.BU9_UsageCode == "#NP" && x.BU9_PriceCode == "#NP");
				AssertEquals("BU9_PeriodStart", periodStart, npServiceUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "#NP", npServiceUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "#NP", npServiceUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#NP", npServiceUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", npServiceUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, npServiceUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 150m, npServiceUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 150m, npServiceUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 150m, npServiceUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 150m, npServiceUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 150m, npServiceUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, npServiceUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", ZGuid.Empty, npServiceUsage.BU9_AC_DiscountChargeCode);
			});

			AssertEquals(6, billedUsages.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 4, 1)]
		public void TestCreateRevenueBreakdown_CoreExcludedFromVolume()
		{
			var periodStart = new ZDateTime(2020, 5, 1);
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "SHP");
			var userPrice = prices.Items[0];
			userPrice.L7_Price = 40m;
			userPrice.L7_LicenceUnits = 400m;
			var shpPrice = prices.Items[1];
			shpPrice.L7_Price = 3m;
			shpPrice.L7_LicenceUnits = 30m;

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK, "AUD");
			var priceLink = BillingTestHelper.CreatePriceLink(lic.Database, prices, periodStart);
			priceLink.PHL_CorePackCode = EdiPriceHeaderLinkCorePackCodeList.Codes.EX;
			priceLink.PHL_VolumeCode = EdiPriceHeaderLinkVolumeCodeList.Codes.STD;

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, lic, 5);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "SHP", periodStart, lic, 100);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill = billing.Bills[0];
			var monthlyUsage = bill.MonthlyUsages.Single();
			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);

			// USR
			CombineAssertions(() =>
			{
				var billedUsage = billedUsages.Single(x => x.BU9_L7 == userPrice.PK);
				AssertEquals("BU9_UsageCode", "STL", billedUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "USR", billedUsage.BU9_UsageSubCode);
				AssertEquals("BU9_UnitCount", 5m, billedUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 40m, billedUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 5 * 40m, billedUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 5 * 40m, billedUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_CommitmentAdjustTransactionPostDiscount", 0m, billedUsage.BU9_CommitmentAdjustTransactionPostDiscount);
				AssertEquals("BU9_TotalDiscountUnits", 0m, billedUsage.BU9_TotalDiscountUnits);
			});

			// SHP
			CombineAssertions(() =>
			{
				var billedUsage = billedUsages.Single(x => x.BU9_L7 == shpPrice.PK);
				AssertEquals("BU9_UsageCode", "STL", billedUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "SHP", billedUsage.BU9_UsageSubCode);
				AssertEquals("BU9_UnitCount", 100m, billedUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 3m, billedUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 100 * 3m, billedUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 100m * 3m, billedUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_CommitmentAdjustTransactionPostDiscount", 0m, billedUsage.BU9_CommitmentAdjustTransactionPostDiscount);
				AssertEquals("BU9_TotalDiscountUnits", 100 * 30m, billedUsage.BU9_TotalDiscountUnits);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown_MultipleUsagesPerItem()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();

			var periodStart = new ZDateTime(2015, 7, 1);
			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Code = "#PA";
			priceItemA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemA.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			priceItemA.L7_LicenceUnits = 250m;
			var priceItemB = Factory.New<ClientLicencePriceItem>();
			priceItemB.L7_Code = "#PB";
			priceItemB.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemB.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			priceItemB.L7_LicenceUnits = 300m;

			var bill = NewBill();

			var monthlyUsage = NewMonthlyUsage();
			{
				var usageLineA1 = new UsageLine(Factory);
				usageLineA1.PriceItem = priceItemA;
				usageLineA1.TotalUnitCount = 4;
				usageLineA1.Price = 25;
				usageLineA1.PriceCurrency = "AUD";
				usageLineA1.SetAmounts(100, 75);
				usageLineA1.LocalPreDiscountAmount = 200;
				usageLineA1.LocalPostDiscountAmount = 150;

				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = "AAA";
				chargeableUsage1.U1_SubCode = "AA1";
				chargeableUsage1.U1_UnitCount = 3;
				usageLineA1.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				var chargeableUsage2 = Factory.New<ClientChargeableUsage>();
				chargeableUsage2.U1_PeriodStart = periodStart;
				chargeableUsage2.U1_Code = "BBB";
				chargeableUsage2.U1_SubCode = "BB1";
				chargeableUsage2.U1_UnitCount = 1;
				usageLineA1.AddUsage(new Usage(chargeableUsage2) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLineA1);
			}

			{
				var usageLineA2 = new UsageLine(Factory);
				usageLineA2.PriceItem = priceItemA;
				usageLineA2.TotalUnitCount = 2;
				usageLineA2.Price = 25;
				usageLineA2.PriceCurrency = "AUD";
				usageLineA2.SetAmounts(50, 10);
				usageLineA2.LocalPreDiscountAmount = 100;
				usageLineA2.LocalPostDiscountAmount = 20;

				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = "AAA";
				chargeableUsage1.U1_SubCode = "AA1";
				chargeableUsage1.U1_UnitCount = 2;
				usageLineA2.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLineA2);
			}

			{
				var usageLineB1 = new UsageLine(Factory);
				usageLineB1.PriceItem = priceItemB;
				usageLineB1.TotalUnitCount = 2;
				usageLineB1.Price = 25;
				usageLineB1.PriceCurrency = "AUD";
				usageLineB1.SetAmounts(50, 10);
				usageLineB1.LocalPreDiscountAmount = 100;
				usageLineB1.LocalPostDiscountAmount = 20;
				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = "AAA";
				chargeableUsage1.U1_SubCode = "AA1";
				chargeableUsage1.U1_UnitCount = 2;
				usageLineB1.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLineB1);
			}

			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);

			CombineAssertions(() =>
			{
				var priceItemAUsageForAAA = billedUsages.Single(x => x.BU9_L7 == priceItemA.PK && x.BU9_UsageCode == "AAA");
				AssertEquals("BU9_PeriodStart", periodStart, priceItemAUsageForAAA.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "AAA", priceItemAUsageForAAA.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", priceItemAUsageForAAA.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PA", priceItemAUsageForAAA.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", priceItemAUsageForAAA.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 5m, priceItemAUsageForAAA.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemAUsageForAAA.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", Utilities.Round((100m + 50m) * 5 / 6, EdiBilledUsageSchema.BU9_TransactionAmountPreDiscount.Scale), priceItemAUsageForAAA.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", Utilities.Round((75m + 10m) * 5 / 6, EdiBilledUsageSchema.BU9_TransactionAmountPostDiscount.Scale), priceItemAUsageForAAA.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", Utilities.Round((200m + 100m) * 5 / 6, EdiBilledUsageSchema.BU9_LocalAmountPreDiscount.Scale), priceItemAUsageForAAA.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", Utilities.Round((150m + 20m) * 5 / 6, EdiBilledUsageSchema.BU9_LocalAmountPostDiscount.Scale), priceItemAUsageForAAA.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemAUsageForAAA.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemAUsageForAAA.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_TotalDiscountUnits", 5 * 250m, priceItemAUsageForAAA.BU9_TotalDiscountUnits);
			});

			CombineAssertions(() =>
			{
				var priceItemAUsageForBBB = billedUsages.Single(x => x.BU9_L7 == priceItemA.PK && x.BU9_UsageCode == "BBB");
				AssertEquals("BU9_PeriodStart", periodStart, priceItemAUsageForBBB.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "BBB", priceItemAUsageForBBB.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "BB1", priceItemAUsageForBBB.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PA", priceItemAUsageForBBB.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", priceItemAUsageForBBB.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, priceItemAUsageForBBB.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemAUsageForBBB.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", Utilities.Round((100m + 50m) * 1 / 6, EdiBilledUsageSchema.BU9_TransactionAmountPreDiscount.Scale), priceItemAUsageForBBB.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", Utilities.Round((75m + 10m) * 1 / 6, EdiBilledUsageSchema.BU9_TransactionAmountPostDiscount.Scale), priceItemAUsageForBBB.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", Utilities.Round((200m + 100m) * 1 / 6, EdiBilledUsageSchema.BU9_LocalAmountPreDiscount.Scale), priceItemAUsageForBBB.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", Utilities.Round((150m + 20m) * 1 / 6, EdiBilledUsageSchema.BU9_LocalAmountPostDiscount.Scale), priceItemAUsageForBBB.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemAUsageForBBB.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemAUsageForBBB.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_TotalDiscountUnits", 1 * 250m, priceItemAUsageForBBB.BU9_TotalDiscountUnits);
			});

			CombineAssertions(() =>
			{
				var priceItemBUsage = billedUsages.Single(x => x.BU9_L7 == priceItemB.PK);
				AssertEquals("BU9_PeriodStart", periodStart, priceItemBUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "AAA", priceItemBUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", priceItemBUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PB", priceItemBUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", priceItemBUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 2m, priceItemBUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemBUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 50m, priceItemBUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 10m, priceItemBUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 100m, priceItemBUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 20m, priceItemBUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemBUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemBUsage.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_TotalDiscountUnits", 2 * 300m, priceItemBUsage.BU9_TotalDiscountUnits);
			});

			AssertEquals(3, billedUsages.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 4, 1)]
		public void TestCreateRevenueBreakdown_Commitment()
		{
			var periodStart = new ZDateTime(2020, 5, 1);
			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var prices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "SHP");

			// 25% discount
			var discount = prices.StlDiscounts.AddNew();
			discount.PHD_Type = BillingConstants.DiscountCalculator.Percentage;
			discount.PHD_Name = "SPECIAL";
			discount.PHD_Percent = 25;
			discount.PHD_IsDefaultEnabled = true;
			discount.PHD_Version = "STL1";

			var itemDiscount = prices.StlItemDiscounts.AddNew();
			itemDiscount.PGM_GroupCode = "STANDARD";
			itemDiscount.PGM_PHD = discount.PK;

			var userPrice = prices.Items[0];
			userPrice.L7_Price = 40m;
			userPrice.L7_LicenceUnits = 400m;
			userPrice.L7_PGM_DiscountGroupCode = itemDiscount.PGM_GroupCode;
			var shpPrice = prices.Items[1];
			shpPrice.L7_Price = 3m;
			shpPrice.L7_LicenceUnits = 30m;
			shpPrice.L7_PGM_DiscountGroupCode = itemDiscount.PGM_GroupCode;

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA", false);
			var clientCompany1 = BillingTestHelper.CreateClientCompany(lic.Database, "AAA");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(lic.Database, "BBB");
			BillingTestHelper.CreatePriceLink(lic.Database, prices, periodStart);
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK, "AUD");

			// Create a commitment for 50% more than they used
			BillingTestHelper.CreateCommitment(lic.Database, periodStart, periodStart.AddYears(1).AddDays(-1), (20 * 400m + 10 * 30m) * 1.5m);

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, clientCompany1, 9);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, clientCompany2, 11);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "SHP", periodStart, clientCompany1, 10);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing.AccumulateMonths = 0;
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			var bill = billing.Bills[0];
			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);

			CombineAssertions(() =>
			{
				var billedUsage = billedUsages.Single(x => x.BU9_L7 == userPrice.PK && x.BU9_LCC == clientCompany1.PK);
				AssertEquals("BU9_PeriodStart", periodStart, billedUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "STL", billedUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "USR", billedUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "USR", billedUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", billedUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 9m, billedUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 40m, billedUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 9 * 40m * 1.5m, billedUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 9 * 40m * 0.75m * 1.5m, billedUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_TotalDiscountUnits", 9 * 400m, billedUsage.BU9_TotalDiscountUnits);
				AssertEquals("BU9_CommitmentAdjustTransactionPostDiscount", 9 * 40m * 0.75m * 0.5m, billedUsage.BU9_CommitmentAdjustTransactionPostDiscount);
			});

			CombineAssertions(() =>
			{
				var billedUsage = billedUsages.Single(x => x.BU9_L7 == userPrice.PK && x.BU9_LCC == clientCompany2.PK);
				AssertEquals("BU9_PeriodStart", periodStart, billedUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "STL", billedUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "USR", billedUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "USR", billedUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", billedUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 11m, billedUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 40m, billedUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 11 * 40m * 1.5m, billedUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 11 * 40m * 0.75m * 1.5m, billedUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_TotalDiscountUnits", 11 * 400m, billedUsage.BU9_TotalDiscountUnits);
				AssertEquals("BU9_CommitmentAdjustTransactionPostDiscount", 11 * 40m * 0.75m * 0.5m, billedUsage.BU9_CommitmentAdjustTransactionPostDiscount);
			});

			CombineAssertions(() =>
			{
				var billedUsage = billedUsages.Single(x => x.BU9_L7 == shpPrice.PK && x.BU9_LCC == clientCompany1.PK);
				AssertEquals("BU9_PeriodStart", periodStart, billedUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "STL", billedUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "SHP", billedUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "SHP", billedUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", billedUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 10m, billedUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 3m, billedUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 10 * 3m * 1.5m, billedUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 10 * 3m * 0.75m * 1.5m, billedUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_TotalDiscountUnits", 10 * 30m, billedUsage.BU9_TotalDiscountUnits);
				AssertEquals("BU9_CommitmentAdjustTransactionPostDiscount", 10 * 3m * 0.75m * 0.5m, billedUsage.BU9_CommitmentAdjustTransactionPostDiscount);
			});

			AssertEquals(3, billedUsages.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown_MultipleUsagesPerItem_HOS()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			var testDatabase = licence.Database.LicEnterprise.Databases.AddNew();
			testDatabase.LD_LicenceType = "TST";
			testDatabase.LD_ServerCode = "T01";
			Factory.Save();

			var periodStart = new ZDateTime(2015, 7, 1);
			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Code = "#HD";
			priceItemA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemA.L7_DiscountChargeCode = discountChargeCode.AC_Code;

			var priceItemHA = Factory.New<ClientLicencePriceItem>();
			priceItemHA.L7_Code = "#HA";
			priceItemHA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemHA.L7_DiscountChargeCode = discountChargeCode.AC_Code;

			var bill = NewBill();
			var monthlyUsage = NewMonthlyUsage();
			{
				var usageLine1 = new UsageLine(Factory);
				usageLine1.PriceItem = priceItemA;
				usageLine1.TotalUnitCount = 100;
				usageLine1.Price = 25;
				usageLine1.PriceCurrency = "AUD";
				usageLine1.SetAmounts(200, 150);
				usageLine1.LocalPreDiscountAmount = 200;
				usageLine1.LocalPostDiscountAmount = 150;

				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = "HOS";
				chargeableUsage1.U1_SubCode = "#HD";
				chargeableUsage1.U1_UnitCount = 200000; //MB
				chargeableUsage1.U1_LD = licence.LA_LD;
				usageLine1.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				var chargeableUsage2 = Factory.New<ClientChargeableUsage>();
				chargeableUsage2.U1_PeriodStart = periodStart;
				chargeableUsage2.U1_Code = "HOS";
				chargeableUsage2.U1_SubCode = "#HD";
				chargeableUsage2.U1_UnitCount = 300000; //MB
				chargeableUsage2.U1_LD = testDatabase.PK;
				usageLine1.AddUsage(new Usage(chargeableUsage2) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				var usageLineHA = new UsageLine(Factory);
				usageLineHA.PriceItem = priceItemHA;
				usageLineHA.TotalUnitCount = 150;
				usageLineHA.Price = 30;
				usageLineHA.PriceCurrency = "AUD";
				usageLineHA.SetAmounts(250, 200);
				usageLineHA.LocalPreDiscountAmount = 250;
				usageLineHA.LocalPostDiscountAmount = 200;

				var chargeableUsageHA = Factory.New<ClientChargeableUsage>();
				chargeableUsageHA.U1_PeriodStart = periodStart;
				chargeableUsageHA.U1_Code = "HOS";
				chargeableUsageHA.U1_SubCode = "#HA";
				chargeableUsageHA.U1_UnitCount = 400000; //MB
				chargeableUsageHA.U1_LD = licence.LA_LD;
				usageLineHA.AddUsage(new Usage(chargeableUsageHA) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLine1);
				monthlyUsage.AddUsageLine(usageLineHA);
			}

			bill.AddMonthlyUsages(new[] { monthlyUsage });
			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query).OrderBy(x => x.BU9_TransactionAmountPreDiscount).ToArray();
			AssertEquals(3, billedUsages.Length);

			CombineAssertions(() =>
			{
				var billed1 = billedUsages[0];
				AssertEquals("BU9_PeriodStart", periodStart, billed1.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "HOS", billed1.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "#HD", billed1.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#HD", billed1.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", billed1.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 40m, billed1.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, billed1.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 80m, billed1.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 60m, billed1.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 80m, billed1.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 60m, billed1.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, billed1.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, billed1.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_LD", licence.Database.PK, billed1.BU9_LD);
			});

			CombineAssertions(() =>
			{
				var billed2 = billedUsages[1];
				AssertEquals("BU9_PeriodStart", periodStart, billed2.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "HOS", billed2.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "#HD", billed2.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#HD", billed2.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", billed2.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 60m, billed2.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, billed2.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 120m, billed2.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 90m, billed2.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 120m, billed2.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 90m, billed2.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, billed2.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, billed2.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_LD", testDatabase.PK, billed2.BU9_LD);
			});

			CombineAssertions(() =>
			{
				var billedHA = billedUsages[2];
				AssertEquals("BU9_PeriodStart", periodStart, billedHA.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "HOS", billedHA.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "#HA", billedHA.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#HA", billedHA.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", billedHA.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 150m, billedHA.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 30m, billedHA.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 250m, billedHA.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 200m, billedHA.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 250m, billedHA.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 200m, billedHA.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, billedHA.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, billedHA.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_LD", licence.Database.PK, billedHA.BU9_LD);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown_MultipleUsagesPerItem_DifferentCurrencies()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();

			var periodStart = new ZDateTime(2015, 7, 1);
			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Code = "#PA";
			priceItemA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemA.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			priceItemA.L7_RX_NKCurrency = "USD";
			var priceItemB = Factory.New<ClientLicencePriceItem>();
			priceItemB.L7_Code = "#PB";
			priceItemB.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemB.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			priceItemB.L7_RX_NKCurrency = "USD";

			var bill = NewBill();

			var monthlyUsage = NewMonthlyUsage();
			{
				var usageLineA1 = new UsageLine(Factory);
				usageLineA1.PriceItem = priceItemA;
				usageLineA1.TotalUnitCount = 4;
				usageLineA1.Price = 25;
				usageLineA1.PriceCurrency = "USD";
				usageLineA1.SetAmounts(100, 75);
				usageLineA1.LocalPreDiscountAmount = 200;
				usageLineA1.LocalPostDiscountAmount = 150;
				usageLineA1.InvoiceCurrencyRateAsMultiplier = 1.5m;
				usageLineA1.InvoiceCurrencyDecimals = 2;

				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = "AAA";
				chargeableUsage1.U1_SubCode = "AA1";
				chargeableUsage1.U1_UnitCount = 3;
				usageLineA1.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				var chargeableUsage2 = Factory.New<ClientChargeableUsage>();
				chargeableUsage2.U1_PeriodStart = periodStart;
				chargeableUsage2.U1_Code = "BBB";
				chargeableUsage2.U1_SubCode = "BB1";
				chargeableUsage2.U1_UnitCount = 1;
				usageLineA1.AddUsage(new Usage(chargeableUsage2) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLineA1);
			}

			{
				var usageLineA2 = new UsageLine(Factory);
				usageLineA2.PriceItem = priceItemA;
				usageLineA2.TotalUnitCount = 2;
				usageLineA2.Price = 25;
				usageLineA2.PriceCurrency = "USD";
				usageLineA2.SetAmounts(50, 10);
				usageLineA2.LocalPreDiscountAmount = 100;
				usageLineA2.LocalPostDiscountAmount = 20;
				usageLineA2.InvoiceCurrencyRateAsMultiplier = 1.5m;
				usageLineA2.InvoiceCurrencyDecimals = 2;

				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = "AAA";
				chargeableUsage1.U1_SubCode = "AA1";
				chargeableUsage1.U1_UnitCount = 2;
				usageLineA2.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLineA2);
			}

			{
				var usageLineB1 = new UsageLine(Factory);
				usageLineB1.PriceItem = priceItemB;
				usageLineB1.TotalUnitCount = 2;
				usageLineB1.Price = 25;
				usageLineB1.PriceCurrency = "USD";
				usageLineB1.SetAmounts(50, 10);
				usageLineB1.LocalPreDiscountAmount = 100;
				usageLineB1.LocalPostDiscountAmount = 20;
				usageLineB1.InvoiceCurrencyRateAsMultiplier = 1.5m;
				usageLineB1.InvoiceCurrencyDecimals = 2;
				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = "AAA";
				chargeableUsage1.U1_SubCode = "AA1";
				chargeableUsage1.U1_UnitCount = 2;
				usageLineB1.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLineB1);
			}

			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);

			CombineAssertions(() =>
			{
				var priceItemAUsageForAAA = billedUsages.Single(x => x.BU9_L7 == priceItemA.PK && x.BU9_UsageCode == "AAA");
				AssertEquals("BU9_PeriodStart", periodStart, priceItemAUsageForAAA.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "AAA", priceItemAUsageForAAA.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", priceItemAUsageForAAA.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PA", priceItemAUsageForAAA.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "USD", priceItemAUsageForAAA.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 5m, priceItemAUsageForAAA.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemAUsageForAAA.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", Utilities.Round((100m + 50m) * 5 / 6 * 1.5m, EdiBilledUsageSchema.BU9_TransactionAmountPreDiscount.Scale), priceItemAUsageForAAA.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", Utilities.Round((75m + 10m) * 5m / 6m * 1.5m, EdiBilledUsageSchema.BU9_TransactionAmountPostDiscount.Scale), priceItemAUsageForAAA.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", Utilities.Round((200m + 100m) * 5 / 6, EdiBilledUsageSchema.BU9_LocalAmountPreDiscount.Scale), priceItemAUsageForAAA.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", Utilities.Round((150m + 20m) * 5 / 6, EdiBilledUsageSchema.BU9_LocalAmountPostDiscount.Scale), priceItemAUsageForAAA.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemAUsageForAAA.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemAUsageForAAA.BU9_AC_DiscountChargeCode);
			});

			CombineAssertions(() =>
			{
				var priceItemAUsageForBBB = billedUsages.Single(x => x.BU9_L7 == priceItemA.PK && x.BU9_UsageCode == "BBB");
				AssertEquals("BU9_PeriodStart", periodStart, priceItemAUsageForBBB.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "BBB", priceItemAUsageForBBB.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "BB1", priceItemAUsageForBBB.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PA", priceItemAUsageForBBB.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "USD", priceItemAUsageForBBB.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, priceItemAUsageForBBB.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemAUsageForBBB.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", Utilities.Round((100m + 50m) * 1m / 6m * 1.5m, EdiBilledUsageSchema.BU9_TransactionAmountPreDiscount.Scale), priceItemAUsageForBBB.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", Utilities.Round((75m + 10m) * 1m / 6m * 1.5m, EdiBilledUsageSchema.BU9_TransactionAmountPostDiscount.Scale), priceItemAUsageForBBB.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", Utilities.Round((200m + 100m) * 1 / 6, EdiBilledUsageSchema.BU9_LocalAmountPreDiscount.Scale), priceItemAUsageForBBB.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", Utilities.Round((150m + 20m) * 1 / 6, EdiBilledUsageSchema.BU9_LocalAmountPostDiscount.Scale), priceItemAUsageForBBB.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemAUsageForBBB.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemAUsageForBBB.BU9_AC_DiscountChargeCode);
			});

			CombineAssertions(() =>
			{
				var priceItemBUsage = billedUsages.Single(x => x.BU9_L7 == priceItemB.PK);
				AssertEquals("BU9_PeriodStart", periodStart, priceItemBUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "AAA", priceItemBUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", priceItemBUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PB", priceItemBUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "USD", priceItemBUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 2m, priceItemBUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemBUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 50m * 1.5m, priceItemBUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 10m * 1.5m, priceItemBUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 100m, priceItemBUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 20m, priceItemBUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemBUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemBUsage.BU9_AC_DiscountChargeCode);
			});

			AssertEquals(3, billedUsages.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown_PerDatabaseItem()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			var lic2 = BillingTestHelper.CreateAnotherLicence(licence, "BBB");
			Factory.Save();

			var periodStart = new ZDateTime(2015, 7, 1);
			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Code = "#PA";
			priceItemA.L7_FeeType = BillingConstants.FeeType.Database;
			priceItemA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemA.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			var bill = NewBill();

			var monthlyUsage = NewMonthlyUsage();
			{
				var usageLine = new UsageLine(Factory);
				usageLine.PriceItem = priceItemA;
				usageLine.TotalUnitCount = 1;
				usageLine.Price = 300;
				usageLine.PriceCurrency = "AUD";
				usageLine.SetAmounts(300, 150);
				usageLine.LocalPreDiscountAmount = 300;
				usageLine.LocalPostDiscountAmount = 150;

				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = "ODM";
				chargeableUsage1.U1_SubCode = "GZH";
				chargeableUsage1.U1_UnitCount = 3;
				chargeableUsage1.U1_LCC = licence.ClientCompany.PK;
				usageLine.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				var chargeableUsage2 = Factory.New<ClientChargeableUsage>();
				chargeableUsage2.U1_PeriodStart = periodStart;
				chargeableUsage2.U1_Code = "ODM";
				chargeableUsage2.U1_SubCode = "GZH";
				chargeableUsage2.U1_UnitCount = 5;
				chargeableUsage2.U1_LCC = lic2.ClientCompany.PK;
				usageLine.AddUsage(new Usage(chargeableUsage2) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLine);
			}

			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);
			AssertEquals(1, billedUsages.Length);
			var billed1 = billedUsages[0];

			CombineAssertions(() =>
			{
				AssertEquals("BU9_PeriodStart", periodStart, billed1.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "ODM", billed1.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "GZH", billed1.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PA", billed1.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", billed1.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, billed1.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 300m, billed1.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 300m, billed1.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 150m, billed1.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 300m, billed1.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 150m, billed1.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, billed1.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, billed1.BU9_AC_DiscountChargeCode);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown_MultipleDiscountsIncluding100Percent()
		{
			var periodStart = new ZDateTime(2015, 7, 1);
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var eHubChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "EHUB");
			var uHubChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "UHUB");
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			BillingTestHelper.LoadClientSpecificDocuments();

			Factory.Save();

			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Code = "#PA";
			priceItemA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemA.L7_DiscountChargeCode = discountChargeCode.AC_Code;

			var bill = NewBill("AUD");

			var chargeableUsageA = Factory.New<ClientChargeableUsage>();
			chargeableUsageA.U1_PeriodStart = periodStart;
			chargeableUsageA.U1_Code = "AAA";
			chargeableUsageA.U1_SubCode = "AA1";

			var monthlyUsage = NewMonthlyUsage();

			var discount10 = Factory.New<EdiPriceHeaderDiscount>();
			discount10.PHD_Name = "D10";
			discount10.PHD_Version = "STL1";
			discount10.PHD_Type = BillingConstants.DiscountCalculator.Percentage;

			var discount15 = Factory.New<EdiPriceHeaderDiscount>();
			discount15.PHD_Name = "D25";
			discount15.PHD_Version = "STL1";
			discount15.PHD_Type = BillingConstants.DiscountCalculator.Percentage;

			var discount100 = Factory.New<EdiPriceHeaderDiscount>();
			discount100.PHD_Name = "D100";
			discount100.PHD_Version = "STL1";
			discount100.PHD_Type = BillingConstants.DiscountCalculator.Percentage;

			var usageLineC = new UsageLine(Factory);
			usageLineC.PriceItem = priceItemA;
			usageLineC.TotalUnitCount = 250;
			usageLineC.Price = 4;
			usageLineC.PriceCurrency = "AUD";
			usageLineC.SetAmounts(1000, 0);
			usageLineC.LocalPreDiscountAmount = 1000;
			usageLineC.LocalPostDiscountAmount = 0;
			usageLineC.AddUsage(new Usage(chargeableUsageA) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

			var setting10 = Factory.New<DiscountLicenceSetting>();
			setting10.LS9_Percent = 10m;
			var discountInfo10 = new DiscountInfo();
			discountInfo10.Init(discount10, setting10, monthlyUsage, null);

			var setting15 = Factory.New<DiscountLicenceSetting>();
			setting15.LS9_Percent = 15m;
			var discountInfo15 = new DiscountInfo();
			discountInfo15.Init(discount15, setting15, monthlyUsage, null);

			var setting100 = Factory.New<DiscountLicenceSetting>();
			setting100.LS9_Percent = 100m;
			var discountInfo100 = new DiscountInfo();
			discountInfo100.Init(discount100, setting100, monthlyUsage, null);

			var stlDiscount10 = new PercentageStlDiscount(discountInfo10);
			var stlDiscount15 = new PercentageStlDiscount(discountInfo15);
			var stlDiscount100 = new PercentageStlDiscount(discountInfo100);

			usageLineC.Discounts = new StlBilling.DiscountSet(new IStlDiscount[] { stlDiscount10, stlDiscount15, stlDiscount100 });
			monthlyUsage.AddUsageLine(usageLineC);

			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);

			CombineAssertions(() =>
			{
				var priceItemAUsage = billedUsages.Single(x => x.BU9_L7 == priceItemA.PK);
				var billedDiscounts = invoiceFactory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, priceItemAUsage.PK))
					.OrderBy(x => x.BD9_Percent)
					.ToArray();

				AssertEquals("BU9_PeriodStart", periodStart, priceItemAUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "AAA", priceItemAUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", priceItemAUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PA", priceItemAUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", priceItemAUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 250m, priceItemAUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 4m, priceItemAUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 1000m, priceItemAUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 0m, priceItemAUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 1000m, priceItemAUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 0m, priceItemAUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemAUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemAUsage.BU9_AC_DiscountChargeCode);

				AssertEquals("billedDiscounts.Length", 3, billedDiscounts.Length);

				AssertEquals("billedDiscounts[0].BD9_Percent", 10m, billedDiscounts[0].BD9_Percent);
				AssertEquals("billedDiscounts[0].BD9_PHD_Discount", discount10.PK, billedDiscounts[0].BD9_PHD_Discount);
				AssertEquals("billedDiscounts[0].BD9_TransactionAmount", 10m / (10m + 15m) * (1 - 0.9m * 0.85m) * 1000m, billedDiscounts[0].BD9_TransactionAmount);
				AssertEquals("billedDiscounts[0].BD9_Type", "", billedDiscounts[0].BD9_Type);

				AssertEquals("billedDiscounts[1].BD9_Percent", 15m, billedDiscounts[1].BD9_Percent);
				AssertEquals("billedDiscounts[1].BD9_PHD_Discount", discount15.PK, billedDiscounts[1].BD9_PHD_Discount);
				AssertEquals("billedDiscounts[1].BD9_TransactionAmount", 15m / (10m + 15m) * (1 - 0.9m * 0.85m) * 1000m, billedDiscounts[1].BD9_TransactionAmount);
				AssertEquals("billedDiscounts[1].BD9_Type", "", billedDiscounts[1].BD9_Type);

				AssertEquals("billedDiscounts[2].BD9_Percent", 100m, billedDiscounts[2].BD9_Percent);
				AssertEquals("billedDiscounts[2].BD9_PHD_Discount", discount100.PK, billedDiscounts[2].BD9_PHD_Discount);
				AssertEquals("billedDiscounts[2].BD9_TransactionAmount", 1000 - billedDiscounts[0].BD9_TransactionAmount - billedDiscounts[1].BD9_TransactionAmount, billedDiscounts[2].BD9_TransactionAmount);
				AssertEquals("billedDiscounts[2].BD9_Type", "", billedDiscounts[2].BD9_Type);
			});

			AssertEquals(1, billedUsages.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown_AccumulatedUsage()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();

			var periodStart = new ZDateTime(2020, 2, 1);
			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.L7_Category = BillingConstants.BillingSystem.HostingDataAccess;
			priceItemA.L7_Code = BillingConstants.Hosting.DataAccessCode;
			priceItemA.L7_FeeType = BillingConstants.FeeType.PerGBPerMonth;
			priceItemA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemA.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			var bill = new StlBill(Factory, GlbBranch.CurrentBranch, organisation, "AUD", periodStart, periodStart);
			bill.LocalExchangeRate = 1;

			var monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(licence, periodStart);
			{
				var usageLine1 = new UsageLine(Factory, periodStart, periodStart, ZString.Empty);
				usageLine1.PriceItem = priceItemA;
				usageLine1.TotalUnitCount = 2;
				usageLine1.Price = 150;
				usageLine1.PriceCurrency = "AUD";
				usageLine1.SetAmounts(300, 300);
				usageLine1.LocalPreDiscountAmount = 300;
				usageLine1.LocalPostDiscountAmount = 300;

				var chargeableUsage1 = Factory.New<ClientChargeableUsage>();
				chargeableUsage1.U1_PeriodStart = periodStart;
				chargeableUsage1.U1_Code = priceItemA.L7_Category;
				chargeableUsage1.U1_SubCode = priceItemA.L7_Code;
				chargeableUsage1.U1_UnitCount = 2048;
				chargeableUsage1.U1_LD = licence.LA_LD;
				chargeableUsage1.U1_LCC = licence.ClientCompany.PK;
				usageLine1.AddUsage(new Usage(chargeableUsage1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLine1);
			}

			// Accumulated usage from previous month
			{
				var usageLine2 = new UsageLine(Factory, periodStart, periodStart.AddMonths(-1), ZString.Empty);
				new UsageLine(Factory);
				usageLine2.PriceItem = priceItemA;
				usageLine2.TotalUnitCount = 1;
				usageLine2.Price = 150;
				usageLine2.PriceCurrency = "AUD";
				usageLine2.SetAmounts(150, 150);
				usageLine2.LocalPreDiscountAmount = 150;
				usageLine2.LocalPostDiscountAmount = 150;

				var chargeableUsage2 = Factory.New<ClientChargeableUsage>();
				chargeableUsage2.U1_PeriodStart = periodStart.AddMonths(-1);
				chargeableUsage2.U1_Code = priceItemA.L7_Category;
				chargeableUsage2.U1_SubCode = priceItemA.L7_Code;
				chargeableUsage2.U1_UnitCount = 2048;
				chargeableUsage2.U1_LD = licence.LA_LD;
				chargeableUsage2.U1_LCC = licence.ClientCompany.PK;
				usageLine2.AddUsage(new Usage(chargeableUsage2) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				monthlyUsage.AddUsageLine(usageLine2);
			}

			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			query.OrderBy = EdiBilledUsageSchema.Constants.BU9_UnitCount;
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);
			AssertEquals(2, billedUsages.Length);
			var billedOld = billedUsages[0];
			var billedNew = billedUsages[1];
			CombineAssertions(() =>
			{
				AssertEquals("billedOld.BU9_UnitCount", 1m, billedOld.BU9_UnitCount);
				AssertEquals("billedOld.BU9_PeriodStart", periodStart.AddMonths(-1).Date, billedOld.BU9_PeriodStart);

				AssertEquals("billedNew.BU9_UnitCount", 2m, billedNew.BU9_UnitCount);
				AssertEquals("billedNew.BU9_PeriodStart", periodStart.Date, billedNew.BU9_PeriodStart);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown_TRA_MultipleUnitBreak_ClientCompany()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "HVP", "HVP", "HVP");
			stlPrices.L6_RX_NKCurrency = "AUD";

			var unitBreak = 0;
			var price = 1m;
			foreach (var hvp in stlPrices.Items.Where(x => x.L7_Code == "HVP"))
			{
				hvp.L7_FeeType = BillingConstants.FeeType.Transactional;
				hvp.L7_UnitBreak = unitBreak;
				hvp.L7_Description = $"L7_UnitBreak = {hvp.L7_UnitBreak}";
				hvp.L7_Price = price;
				unitBreak += 1000;
				price -= 0.05m;
			}

			var licCW = BillingTestHelper.CreateLicence(Factory, "EN1", "CO1", "CW1");
			licCW.LA_AgreedLiveDate = periodStart;
			licCW.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.SetInvoicing(licCW, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreatePriceLink(licCW.Database, stlPrices, periodStart);
			var clientCompany1 = licCW.ClientCompany;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, clientCompany1, 25);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "HVP", periodStart, clientCompany1, 9000);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			AssertNoErrors(bill);

			var invoice = bill.CreateInvoicesWithoutSave().Single();
			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);
			var usageLinesAsText = string.Join("\r\n", billedUsages.Select(x => $"{x.BU9_UsageCode},{x.BU9_UsageSubCode},{x.BU9_PriceCode},{x.PriceItem.L7_Description},{x.BU9_PriceCurrency},{x.BU9_UnitCount},{x.BU9_UnitPrice},{x.BU9_TransactionAmountPreDiscount},{x.BU9_TransactionAmountPostDiscount},{x.BU9_TransactionProcessingAmount},{x.BU9_LocalAmountPreDiscount},{x.BU9_LocalAmountPostDiscount},{x.BU9_LocalProcessingAmount},{x.BU9_LCC}").OrderBy(x => x));
			AssertEquals($@"STL,HVP,HVP,L7_UnitBreak = 0,AUD,1000,1,1000,1000,0,1000,1000,0,{clientCompany1.PK}
STL,HVP,HVP,L7_UnitBreak = 1000,AUD,1000,0.95,950.00,950.00,0,950.00,950.00,0,{clientCompany1.PK}
STL,HVP,HVP,L7_UnitBreak = 2000,AUD,7000,0.90,6300.00,6300.00,0,6300.00,6300.00,0,{clientCompany1.PK}
STL,USR,USR,Item USR,AUD,25,1,25,25,0,25,25,0,{clientCompany1.PK}", usageLinesAsText);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConversionCredit()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.SecurityDepositChargeCode);

			var prices = licence.Company.PriceHeaders.AddNew();
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			var userPrice = prices.Items.AddNew();
			userPrice.L7_Code = "USR";
			userPrice.L7_FeeType = BillingConstants.FeeType.Transactional;
			userPrice.L7_Price = 3.00;
			userPrice.L7_Description = "Users";
			userPrice.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			userPrice.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			userPrice.L7_DepositChargeCode = EDIDataRegistry.SecurityDepositChargeCode;

			var department1 = BillingTestHelper.FindOrCreateDepartment(Factory, "DD1");
			var department2 = BillingTestHelper.FindOrCreateDepartment(Factory, "DD2");

			var conversionCredit = Factory.New<ConversionCreditLicenceSetting>();
			licence.Database.LicenceSettings.Add(conversionCredit);
			conversionCredit.LS9_GE_Department1 = department1.PK;
			conversionCredit.LS9_GE_Department2 = department2.PK;
			conversionCredit.LS9_ValidFrom = new ZDateTime(2015, 1, 1);
			conversionCredit.CreditChargeCode = EDIDataRegistry.Instance.MonthlyUsageProcessingFeeChargeCode.Value;
			conversionCredit.LS9_Price = 100;
			conversionCredit.LS9_RX_NKPriceCurrency = "AUD";

			Factory.Save();

			BillingTestHelper.SetDepositBalance(licence.Company.LC_OH.ToGuid(), EDIDataRegistry.Instance.OdplDepositChargeCode.Value, 10000m, 1000m, "AUD");

			// No conversion credit balance
			{
				var monthlyUsage = NewMonthlyUsage();

				var usageLine = new UsageLine(Factory);
				usageLine.PeriodStart = new ZDateTime(2015, 10, 1);
				usageLine.PriceItem = userPrice;
				usageLine.Price = 3.00;
				usageLine.PriceCurrency = "AUD";
				usageLine.TotalUnitCount = 700;
				usageLine.SetAmounts(2100, 2100);

				monthlyUsage.AddUsageLine(usageLine);

				var bill = NewBill("AUD");
				bill.AddMonthlyUsages(new[] { monthlyUsage });
				var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 10, 31));
				var bills = new StlBillCollection(Factory);
				bills.Add(bill);
				StlBilling.CalculateCurrencyAndSurchargeAmounts(context, bills.Cast<StlBill>());

				var invoice = bill.CreateInvoicesWithoutSave().Single();

				var discountLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDiscountChargeCode.Value).ToArray();
				AssertEquals(2, discountLines.Length);
				AssertEquals(0m, discountLines.Sum(x => x.AL_LineAmount));
				var plusLine = discountLines.Single(x => x.AL_LineAmount > 0);
				var minusLine = discountLines.Single(x => x.AL_LineAmount < 0);
				AssertEquals(department2.PK, plusLine.AL_GE);
				AssertEquals(Env.CurrentDepartment.PK, minusLine.AL_GE);
				AssertEquals(100m, plusLine.AL_OSExTaxAmount);
			}

			// With conversion credit balance
			{
				var monthlyUsage = NewMonthlyUsage();

				var usageLine = new UsageLine(Factory);
				usageLine.PeriodStart = new ZDateTime(2015, 10, 1);
				usageLine.PriceItem = userPrice;
				usageLine.Price = 3.00;
				usageLine.PriceCurrency = "AUD";
				usageLine.TotalUnitCount = 700;
				usageLine.SetAmounts(2100, 2100);

				monthlyUsage.AddUsageLine(usageLine);
				conversionCredit.CreditChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

				var bill = NewBill("AUD");
				bill.AddMonthlyUsages(new[] { monthlyUsage });
				var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 10, 31));
				var bills = new StlBillCollection(Factory);
				bills.Add(bill);
				StlBilling.CalculateCurrencyAndSurchargeAmounts(context, bills.Cast<StlBill>());

				var invoice = bill.CreateInvoicesWithoutSave().Single();

				var discountLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDiscountChargeCode.Value).ToArray();
				AssertEquals(2, discountLines.Length);
				AssertEquals(0m, discountLines.Sum(x => x.AL_LineAmount));
				var plusLine = discountLines.Single(x => x.AL_LineAmount > 0);
				var minusLine = discountLines.Single(x => x.AL_LineAmount < 0);
				AssertEquals(department2.PK, plusLine.AL_GE);
				AssertEquals(Env.CurrentDepartment.PK, minusLine.AL_GE);
				AssertEquals(100m, plusLine.AL_OSExTaxAmount);

				var depositLine = invoice.Lines.Cast<ARInvoiceLine>().Single(x => x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				AssertEquals(-2100m, depositLine.AL_LineAmount);
				AssertEquals(department1.PK, depositLine.AL_GE);
			}

			// BorderWise with conversion credit balance
			{
				var monthlyUsage = NewMonthlyUsage();
				var bwUsage = Factory.New<ClientChargeableUsage>();
				bwUsage.U1_Code = BillingConstants.BillingSystem.BorderWise;
				bwUsage.U1_PeriodStart = new ZDateTime(2015, 10, 1);
				var usage = new Usage(bwUsage);
				usage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);

				var usageLine = new UsageLine(Factory);
				usageLine.PeriodStart = new ZDateTime(2015, 10, 1);
				usageLine.PriceItem = userPrice;
				usageLine.Price = 3.00;
				usageLine.PriceCurrency = "AUD";
				usageLine.TotalUnitCount = 700;
				usageLine.SetAmounts(2100, 2100);
				usageLine.AddUsage(usage);
				AssertEquals(true, usageLine.IsBorderWise);

				monthlyUsage.AddUsageLine(usageLine);
				conversionCredit.CreditChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

				var bill = NewBill("AUD");
				bill.AddMonthlyUsages(new[] { monthlyUsage });
				var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 10, 31));
				var bills = new StlBillCollection(Factory);
				bills.Add(bill);
				StlBilling.CalculateCurrencyAndSurchargeAmounts(context, bills.Cast<StlBill>());

				var invoice = bill.CreateInvoicesWithoutSave().Single();

				var discountLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDiscountChargeCode.Value).ToArray();
				AssertEquals(0, discountLines.Length);

				var depositLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => x.ChargeCode.AC_Code == userPrice.L7_DepositChargeCode);
				AssertEquals(0, depositLines.Count());

				var chargeLine = invoice.Lines.Cast<ARInvoiceLine>().Single(x => x.ChargeCode.AC_Code == userPrice.L7_ChargeCode);
				AssertEquals(2100m, chargeLine.AL_LineAmount);
				AssertEquals(Env.CurrentDepartment.PK, chargeLine.AL_GE);
			}

			// Part conversion credit and part normal deposit
			BillingTestHelper.SetDepositBalance(licence.Company.LC_OH.ToGuid(), EDIDataRegistry.Instance.OdplDepositChargeCode.Value, 1000m, 100m, "AUD");
			BillingTestHelper.SetDepositBalance(licence.Company.LC_OH.ToGuid(), EDIDataRegistry.SecurityDepositChargeCode, 10000m, 1000m, "AUD");
			{
				var monthlyUsage = NewMonthlyUsage();

				var usageLine = new UsageLine(Factory);
				usageLine.PeriodStart = new ZDateTime(2015, 10, 1);
				usageLine.PriceItem = userPrice;
				usageLine.Price = 3.00;
				usageLine.PriceCurrency = "AUD";
				usageLine.TotalUnitCount = 700;
				usageLine.SetAmounts(2100, 2100);

				monthlyUsage.AddUsageLine(usageLine);
				conversionCredit.CreditChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;

				var bill = NewBill("AUD");
				bill.AddMonthlyUsages(new[] { monthlyUsage });
				var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 10, 31));
				var bills = new StlBillCollection(Factory);
				bills.Add(bill);
				StlBilling.CalculateCurrencyAndSurchargeAmounts(context, bills.Cast<StlBill>());

				var invoice = bill.CreateInvoicesWithoutSave().Single();

				var discountLines = invoice.Lines.Cast<ARInvoiceLine>().Where(x => x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDiscountChargeCode.Value).ToArray();
				AssertEquals(2, discountLines.Length);
				AssertEquals(0m, discountLines.Sum(x => x.AL_LineAmount));
				var plusLine = discountLines.Single(x => x.AL_LineAmount > 0);
				var minusLine = discountLines.Single(x => x.AL_LineAmount < 0);
				AssertEquals(department2.PK, plusLine.AL_GE);
				AssertEquals(Env.CurrentDepartment.PK, minusLine.AL_GE);
				AssertEquals(100m, plusLine.AL_OSExTaxAmount);

				var depositLine1 = invoice.Lines.Cast<ARInvoiceLine>().Single(x => x.ChargeCode.AC_Code == EDIDataRegistry.Instance.OdplDepositChargeCode.Value);
				var depositLine2 = invoice.Lines.Cast<ARInvoiceLine>().Single(x => x.ChargeCode.AC_Code == EDIDataRegistry.SecurityDepositChargeCode);
				AssertEquals(-1000m, depositLine1.AL_LineAmount);
				AssertEquals(department1.PK, depositLine1.AL_GE);
				AssertEquals(-1100m, depositLine2.AL_LineAmount);
				AssertEquals(Env.CurrentDepartment.PK, depositLine2.AL_GE);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_BorderWiseWithoutCW1()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);
			var licBOR = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1");
			BillingTestHelper.SetInvoicing(licBOR, Env.CurrentBranch.PK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.UserPriceCode, periodStart, licBOR.LA_LC, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, "BOR", BillingConstants.BorderWise.ExtraMachinePriceCode, periodStart, licBOR.LA_LC, 7);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licBOR.Company.LC_OH);
			AssertNoErrors("bill1", bill1);
			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();
			CombineAssertions(() =>
			{
				AssertEquals("bill1.InvoicePreDiscountTotal", 13 * 200m + 7 * 30m, bill1.InvoicePreDiscountTotal);
				AssertEquals("AH_Desc", "BorderWise Monthly Usage Invoice - " + periodStart.ToString("MMMM yyyy", CultureInfo.InvariantCulture), invoice1.AH_Desc);
				AssertEquals("AL_Desc", invoice1.AH_Desc, invoice1.Lines[0].AL_Desc);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 1, 20)]
		public void TestCreateInvoice_Surcharge()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var surchargeChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.StlSurchargeChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			var feeChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "FEE1");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company);
			BillingTestHelper.AddPriceItem(priceHeader, "C03", BillingConstants.FeeType.Transactional, "", 40m)
				.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C03", period1, lic1.ClientCompany, 3);

			lic1.Company.SelfBilling.L4_ProcessingFee = "MPF";
			lic1.Company.SelfBilling.L4_ProcessingFeePercent = 5m;

			var fee = BillingTestHelper.CreateLicenceFee(lic1.Company, "AA2", "desc 2", 150, "FEE1", period1, ZDateTime.Empty);
			fee.L8_RX_NKCurrency = "AUD";
			var feeUsage = new FeeUsage(fee);
			feeUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(lic1), lic1.Company.InvoiceDeliveries[0], lic1.Company);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertEquals(13.5m, bill1.InvoiceSurchargeTotal);
			AssertEquals(5m, bill1.SurchargePercent);
			AssertEquals("Manual Processing Fee", bill1.SurchargeDescription.ToString());
			AssertEquals(270m, bill1.InvoicePreDiscountTotal);
			AssertEquals(283.5m, bill1.InvoicePostDiscountTotal);

			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();

			AssertEquals(311.85m, invoice1.AH_LocalTotalAmount);
			AssertEquals(283.5m, invoice1.AH_LocalExTaxAmount);
			var lines = invoice1.Lines.OfType<AccTransactionLines>().ToArray();
			var linesAsText = string.Join("\r\n", lines.Select(x => $"{x.ChargeCode.AC_Code} - {x.AL_LineAmount} - {x.AL_Desc}"));
			AssertEquals(@"COMMENT - 0 - STL Monthly Usage Invoice - January 2018
COMMENT - 0 - .
FEE1 - 150.00 - desc 2
Jan 2018
ODPLMTHUSE - 120.00 - ODPLMTHUSE
EXTCRSTL - 13.50 - Manual Processing Fee 5.00%
COMMENT - 0 - If you provide a Simplified Payment Option to ensure you have enough credit funds on your account, this will be processed on the 21st of each month.
COMMENT - 0 - For more detailed breakdown of usage, visit myaccount.cargowise.com > Usage Reports.", linesAsText);

			//breakdown
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice1.PK);
			var billedUsages = invoice1.Factory.Load<EdiBilledUsage>(query).OrderBy(x => x.BU9_TransactionAmountPreDiscount).ToArray();
			var usageLinesAsText = string.Join("\r\n", billedUsages.Select(x => $"{x.BU9_UsageCode},{x.BU9_UsageSubCode},{x.BU9_PriceCode},{x.BU9_PriceCurrency},{x.BU9_UnitCount},{x.BU9_UnitPrice},{x.BU9_TransactionAmountPreDiscount},{x.BU9_TransactionAmountPostDiscount},{x.BU9_TransactionProcessingAmount},{x.BU9_LocalAmountPreDiscount},{x.BU9_LocalAmountPostDiscount},{x.BU9_LocalProcessingAmount}"));
			AssertEquals(@"STL,C03,C03,AUD,3,40.000000,120.00,126.00,6.00,120.00,126.00,6.00
FEE,AA2,,AUD,1,150.0000,150.0000,157.5000,7.50,150.0000,157.5000,7.50", usageLinesAsText);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_NonCurrentVersionSurcharge()
		{
			var period1 = BillingTestHelper.MonthToday;

			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var surchargeChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.StlSurchargeChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			var feeChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "FEE1");
			var versionSurchargeChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, EDIDataRegistry.Instance.VersionSurchargeChargeCode.Value);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			lic1.Database.LD_ReleaseRing = "GP1";
			VersionHistoryTest.CreateBuildsAndSetDatabaseToNonCurrentVersion(lic1.Database, period1, 2);
			var versionSurchargeSetting = Factory.New<VersionSurchargeLicenceSetting>();
			versionSurchargeSetting.LS9_ValidFrom = period1;
			versionSurchargeSetting.LS9_Percent = 5m;
			lic1.Database.LicenceSettings.Add(versionSurchargeSetting);

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company);
			BillingTestHelper.AddPriceItem(priceHeader, "C03", BillingConstants.FeeType.Transactional, "", 100m)
				.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;

			var priceHeaderLink1 = lic1.Database.PriceHeaderLinks.AddNew();
			priceHeaderLink1.PHL_L6 = priceHeader.PK;
			priceHeaderLink1.PHL_RX_NKCurrency = "AUD";
			priceHeaderLink1.PHL_ValidFrom = period1;

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 20);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C03", period1, lic1.ClientCompany, 10);

			lic1.Company.SelfBilling.L4_ProcessingFee = "MPF";
			lic1.Company.SelfBilling.L4_ProcessingFeePercent = 10m;

			var fee = BillingTestHelper.CreateLicenceFee(lic1.Company, "AA2", "desc 2", 150, "FEE1", period1, ZDateTime.Empty);
			fee.L8_RX_NKCurrency = "AUD";
			var feeUsage = new FeeUsage(fee);
			feeUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(lic1), lic1.Company.InvoiceDeliveries[0], lic1.Company);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertEquals("VersionSurchargeTotal = 10 * 100m * 0.05m (5%)", 50m, bill1.VersionSurchargeTotal);
			AssertEquals("InvoiceSurchargeTotal = (150m + 1000m + 50m) * 0.10 (10%)", 120m, bill1.InvoiceSurchargeTotal);
			AssertEquals(10m, (decimal)bill1.SurchargePercent);
			AssertEquals("Manual Processing Fee", bill1.SurchargeDescription.ToString());
			AssertEquals(1150m, (decimal)bill1.InvoicePreDiscountTotal);
			AssertEquals("InvoicePostDiscountTotal = InvoicePreDiscountTotal + InvoiceSurchargeTotal + VersionSurchargeTotal = 1000m + 150m + 50m + 120m", 1320m, bill1.InvoicePostDiscountTotal);

			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();

			AssertEquals(bill1.InvoicePostDiscountTotal, invoice1.AH_LocalExTaxAmount);
			var lines = invoice1.Lines.OfType<AccTransactionLines>().ToArray();
			var linesAsText = string.Join("\r\n", lines.Select(x => $"{x.ChargeCode.AC_Code} - {x.AL_LineAmount} - {x.AL_Desc}"));
			AssertEquals(@"COMMENT - 0 - STL Monthly Usage Invoice - " + period1.ToString("MMMM yyyy") + @"
COMMENT - 0 - .
FEE1 - 150.00 - desc 2
" + period1.ToString("MMM yyyy") + @"
ODPLMTHUSE - 1000.00 - ODPLMTHUSE
EXTCRSTL - 120.00 - Manual Processing Fee 10.00%
GPRSURSTL - 50.00 - Surcharge for Non-Current Version
COMMENT - 0 - If you provide a Simplified Payment Option to ensure you have enough credit funds on your account, this will be processed on the 21st of each month.
COMMENT - 0 - For more detailed breakdown of usage, visit myaccount.cargowise.com > Usage Reports.", linesAsText);

			//breakdown
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice1.PK);
			var billedUsages = invoice1.Factory.Load<EdiBilledUsage>(query).OrderByDescending(x => x.BU9_UsageSubCode).ToArray();
			var usageLinesAsText = string.Join("\r\n", billedUsages.Select(x => $"{x.BU9_UsageCode},{x.BU9_UsageSubCode},{x.BU9_PriceCode},{x.BU9_PriceCurrency},{x.BU9_UnitCount},{x.BU9_UnitPrice},{x.BU9_TransactionAmountPreDiscount},{x.BU9_TransactionAmountPostDiscount},{x.BU9_TransactionProcessingAmount},{x.BU9_LocalAmountPreDiscount},{x.BU9_LocalAmountPostDiscount},{x.BU9_LocalProcessingAmount}"));
			AssertEquals(@"STL,C03,C03,AUD,10,100.000000,1000.00,1100.00,100.00,1000.00,1100.00,100.00
FEE,AA2,,AUD,1,150.0000,150.0000,165.0000,15.00,150.0000,165.0000,15.00", usageLinesAsText);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2018, 1, 20)]
		public void TestCreateInvoice_Translatable()
		{
			EDIDataRegistry.Instance.StlInvoiceSupportedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.SharedConstants.Languages.ChineseSimplified });

			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			BillingTestHelper.LoadClientSpecificDocuments();
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var surchargeChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.StlSurchargeChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			var feeChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "FEE1");

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			lic1.ClientCompany.Org.OH_Language = Core.SharedConstants.Languages.ChineseSimplified;

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company);
			var item1 = BillingTestHelper.AddPriceItem(priceHeader, "C03", BillingConstants.FeeType.Transactional, "", 40m);
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_Description = "E_L7_Description";

			BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, period1);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "C03", period1, lic1.ClientCompany, 3);

			lic1.Company.SelfBilling.L4_ProcessingFee = "MPF";
			lic1.Company.SelfBilling.L4_ProcessingFeePercent = 5m;
			lic1.Company.SelfBilling.L4_InvoiceComment = "E_L4_InvoiceComment";

			var fee = BillingTestHelper.CreateLicenceFee(lic1.Company, "AA2", "desc 2", 150, "FEE1", period1, ZDateTime.Empty);
			fee.L8_RX_NKCurrency = "AUD";
			fee.L8_Description = "E_L8_Description";
			var feeUsage = new FeeUsage(fee);
			feeUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(lic1), lic1.Company.InvoiceDeliveries[0], lic1.Company);

			Factory.Save();

			var resKeyL7_Description = item1.L7_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(item1, "E_L7_Description").ResourceKey;
			var resKeyL4_InvoiceComment = lic1.Company.SelfBilling.L4_InvoiceCommentInfo.CustomizableDataResourceStrings.GetMultilingualString(lic1.Company.SelfBilling, "E_L4_InvoiceComment").ResourceKey;
			var resKeyL8_Description = fee.L8_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(fee, "E_L8_Description").ResourceKey;

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				using (var mockRes = Res.UseMockData())
				{
					mockRes.Put(resKeyL7_Description, new ResourceStringData(resKeyL7_Description, "C_L7_Description"));
					mockRes.Put(resKeyL4_InvoiceComment, new ResourceStringData(resKeyL4_InvoiceComment, "C_L4_InvoiceComment"));
					mockRes.Put(resKeyL8_Description, new ResourceStringData(resKeyL8_Description, "C_L8_Description"));

					AssertEquals("C_L7_Description", item1.L7_DescriptionLocalized);
					AssertEquals("C_L4_InvoiceComment", lic1.Company.SelfBilling.L4_InvoiceCommentMultilingual);
					AssertEquals("C_L8_Description", fee.L8_DescriptionMultilingual);

					var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
					billing1.AccumulateMonths = 0;
					billing1.DateTo = period1.AddMonths(1).AddDays(-1);
					billing1.GenerateReport(null);
					var bill1 = billing1.Bills[0];
					AssertEquals(1, bill1.MonthlyUsages.Count());
					AssertEquals(13.5m, bill1.InvoiceSurchargeTotal);
					AssertEquals(5m, bill1.SurchargePercent);
					AssertEquals("Manual Processing Fee", bill1.SurchargeDescription.ToString());
					AssertEquals(270m, bill1.InvoicePreDiscountTotal);
					AssertEquals(283.5m, bill1.InvoicePostDiscountTotal);

					var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();

					AssertEquals(311.85m, invoice1.AH_LocalTotalAmount);
					AssertEquals(283.5m, invoice1.AH_LocalExTaxAmount);
					var lines = invoice1.Lines.OfType<AccTransactionLines>().ToArray();
					var linesAsText = string.Join("\r\n", lines.Select(x => $"{x.ChargeCode.AC_Code} - {x.AL_LineAmount} - {x.AL_Desc}"));
					AssertEquals(@"COMMENT - 0 - STL Monthly Usage Invoice - 一月 2018
COMMENT - 0 - .
FEE1 - 150.00 - C_L8_Description
1月 2018
ODPLMTHUSE - 120.00 - ODPLMTHUSE
EXTCRSTL - 13.50 - Manual Processing Fee 5.00%
COMMENT - 0 - C_L4_InvoiceComment
COMMENT - 0 - If you provide a Simplified Payment Option to ensure you have enough credit funds on your account, this will be processed on the 21st of each month.
COMMENT - 0 - For more detailed breakdown of usage, visit myaccount.cargowise.com > Usage Reports.", linesAsText);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateInvoice_InvoiceSplit()
		{
			var periodStart = new ZDateTime(2015, 7, 1);
			var rate = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", AccTaxRate.Types.Rated, 10);
			var eHubChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "EHUB");
			var uHubChargeCode = BillingTestHelper.CreateChargeCode(Factory, rate, "UHUB");
			var amountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var commentChargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, rate, EDIDataRegistry.Instance.CommentChargeCode.Value);
			commentChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			BillingTestHelper.LoadClientSpecificDocuments();

			var feeEUR = BillingTestHelper.CreateLicenceFee(licence.Company, "AA1", "desc 1", 50, "EHUB", periodStart, ZDateTime.Empty);
			feeEUR.L8_RX_NKCurrency = "EUR";

			var feeUSD = BillingTestHelper.CreateLicenceFee(licence.Company, "AA2", "desc 2", 150, "UHUB", periodStart, ZDateTime.Empty);
			feeUSD.L8_RX_NKCurrency = "USD";

			var service = BillingTestHelper.CreatePremiumService(licence.Database, "CCC", periodStart, ZDateTime.Empty);
			service.CPS_Units = 10;

			var npService = BillingTestHelper.CreatePremiumService(licence.Database, "#NP", periodStart, ZDateTime.Empty);
			npService.CPS_Units = 1;
			npService.IsSystemLicenceFee = true;

			BillingTestHelper.CreateExchangeRate(Factory, "EUR", 0.5, periodStart);
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.75, periodStart);

			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var priceItemA = Factory.New<ClientLicencePriceItem>();
			priceItemA.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "#PA");
			priceItemA.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemA.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			var priceItemB = Factory.New<ClientLicencePriceItem>();
			priceItemB.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "#PB");
			priceItemB.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemB.L7_DiscountChargeCode = discountChargeCode.AC_Code;
			var priceItemC = Factory.New<ClientLicencePriceItem>();
			priceItemC.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "CCC");
			priceItemC.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemC.L7_Price = 10m;
			var priceItemD = Factory.New<ClientLicencePriceItem>();
			priceItemD.CodeKey = new UsageCodeKey(BillingConstants.BillingSystem.Service, "#NP");
			priceItemD.L7_ChargeCode = amountChargeCode.AC_Code;
			priceItemD.L7_Price = 150m;

			var bill = NewBillForInvoiceSplitTest("EUR");

			var chargeableUsageA = Factory.New<ClientChargeableUsage>();
			chargeableUsageA.U1_PeriodStart = periodStart;
			chargeableUsageA.U1_Code = "AAA";
			chargeableUsageA.U1_SubCode = "AA1";

			var chargeableUsageB = Factory.New<ClientChargeableUsage>();
			chargeableUsageB.U1_PeriodStart = periodStart;
			chargeableUsageB.U1_Code = "BBB";
			chargeableUsageB.U1_SubCode = "BB1";

			var chargeableUsageC = Factory.New<ClientChargeableUsage>();
			chargeableUsageC.U1_PeriodStart = periodStart;
			chargeableUsageC.U1_Code = BillingConstants.BillingSystem.Service;
			chargeableUsageC.U1_SubCode = "CCC";

			var chargeableUsageD = Factory.New<ClientChargeableUsage>();
			chargeableUsageD.U1_PeriodStart = periodStart;
			chargeableUsageD.U1_Code = BillingConstants.BillingSystem.Service;
			chargeableUsageD.U1_SubCode = "#NP";

			var monthlyUsage = NewMonthlyUsage();
			{
				var usageLineA = new UsageLine(Factory);
				usageLineA.PriceItem = priceItemA;
				usageLineA.TotalUnitCount = 4;
				usageLineA.Price = 25;
				usageLineA.PriceCurrency = "AUD";
				usageLineA.SetAmounts(100, 75);
				usageLineA.LocalPreDiscountAmount = 200;
				usageLineA.LocalPostDiscountAmount = 150;
				usageLineA.InvoiceCurrencyRateAsMultiplier = 1;
				usageLineA.AddUsage(new Usage(chargeableUsageA) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });
				monthlyUsage.AddUsageLine(usageLineA);
			}

			{
				var usageLineB = new UsageLine(Factory);
				usageLineB.PriceItem = priceItemB;
				usageLineB.TotalUnitCount = 2;
				usageLineB.Price = 25;
				usageLineB.PriceCurrency = "AUD";
				usageLineB.SetAmounts(50, 10);
				usageLineB.LocalPreDiscountAmount = 100;
				usageLineB.LocalPostDiscountAmount = 20;
				usageLineB.InvoiceCurrencyRateAsMultiplier = 1;
				usageLineB.AddUsage(new Usage(chargeableUsageB) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });
				monthlyUsage.AddUsageLine(usageLineB);
			}

			var discount10 = Factory.New<EdiPriceHeaderDiscount>();
			{
				var usageLineC = new UsageLine(Factory);
				usageLineC.PriceItem = priceItemC;
				usageLineC.TotalUnitCount = 10;
				usageLineC.Price = 10;
				usageLineC.PriceCurrency = "AUD";
				usageLineC.SetAmounts(100, 90);
				usageLineC.LocalPreDiscountAmount = 100;
				usageLineC.LocalPostDiscountAmount = 90;
				usageLineC.InvoiceCurrencyRateAsMultiplier = 1;
				usageLineC.AddUsage(new Usage(chargeableUsageC, service, periodStart, null) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

				discount10.PHD_Name = "D10";
				discount10.PHD_Version = "STL1";
				discount10.PHD_Type = BillingConstants.DiscountCalculator.Percentage;

				var setting = Factory.New<DiscountLicenceSetting>();
				setting.LS9_Percent = 10m;
				var discountInfo = new DiscountInfo();
				discountInfo.Init(discount10, setting, monthlyUsage, null);
				var stlDiscount = new PercentageStlDiscount(discountInfo);
				usageLineC.Discounts = new StlBilling.DiscountSet(new IStlDiscount[] { stlDiscount });

				monthlyUsage.AddUsageLine(usageLineC);
			}

			{
				var usageLineD = new UsageLine(Factory);
				usageLineD.PriceItem = priceItemD;
				usageLineD.TotalUnitCount = 1;
				usageLineD.Price = 150;
				usageLineD.PriceCurrency = "AUD";
				usageLineD.SetAmounts(150, 150);
				usageLineD.LocalPreDiscountAmount = 150;
				usageLineD.LocalPostDiscountAmount = 150;
				usageLineD.InvoiceCurrencyRateAsMultiplier = 1;
				usageLineD.AddUsage(new Usage(chargeableUsageD, npService, periodStart, chargeableUsageD.ClientCompany) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });
				monthlyUsage.AddUsageLine(usageLineD);
			}

			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var feeEURUsage = new FeeUsage(feeEUR);
			feeEURUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);

			var feeUSDUsage = new FeeUsage(feeUSD);
			feeUSDUsage.OwnerDelivery = new UsageOwnerDelivery(new UsageOwner(licence), licence.Company.InvoiceDeliveries[0], licence.Company);

			bill.AddFees(new[] { feeEURUsage, feeUSDUsage });

			var invoices = bill.CreateInvoicesWithoutSave().ToArray();
			var invoiceFactory = invoices.First().Factory;

			AssertEquals(4, invoices.Length);
			AssertEquals(invoices[2].PK, invoiceFactory.Load<ClientChargeableUsage>(chargeableUsageA.PK).U1_AH_Invoice);
			AssertEquals(invoices[2].PK, invoiceFactory.Load<ClientChargeableUsage>(chargeableUsageB.PK).U1_AH_Invoice);
			AssertEquals(invoices[2].PK, invoiceFactory.Load<ClientChargeableUsage>(chargeableUsageC.PK).U1_AH_Invoice);
			AssertEquals(invoices[2].PK, invoiceFactory.Load<ClientChargeableUsage>(chargeableUsageD.PK).U1_AH_Invoice);

			var chargeableUsageFeeEURUsage = invoiceFactory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_Parent, feeEUR.PK)).Single();
			var chargeableUsageFeeUSDUsage = invoiceFactory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_Parent, feeUSD.PK)).Single();
			AssertEquals(invoices[0].PK, chargeableUsageFeeEURUsage.U1_AH_Invoice);
			AssertEquals(invoices[1].PK, chargeableUsageFeeUSDUsage.U1_AH_Invoice);

			AssertEquals("DISCODPL", invoices[3].Lines.OfType<ARInvoiceLine>().Single().AL_Desc);

			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoices[0].PK));
			AssertEquals(1, billedUsages.Length);
			CombineAssertions(() =>
			{
				var feeEhubUsage = billedUsages.Single(x => x.BU9_UsageCode == "FEE" && x.BU9_UsageSubCode == "AA1");
				AssertEquals("BU9_PeriodStart", periodStart, feeEhubUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "FEE", feeEhubUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", feeEhubUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "", feeEhubUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "EUR", feeEhubUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, feeEhubUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 50m, feeEhubUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 50m, feeEhubUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 50m, feeEhubUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 100m, feeEhubUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 100m, feeEhubUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", eHubChargeCode.PK, feeEhubUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", ZGuid.Empty, feeEhubUsage.BU9_AC_DiscountChargeCode);
			});

			billedUsages = invoiceFactory.Load<EdiBilledUsage>(new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoices[1].PK));
			AssertEquals(1, billedUsages.Length);
			CombineAssertions(() =>
			{
				var feeUhubUsage = billedUsages.Single(x => x.BU9_UsageCode == "FEE" && x.BU9_UsageSubCode == "AA2");
				AssertEquals("BU9_PeriodStart", periodStart, feeUhubUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "FEE", feeUhubUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA2", feeUhubUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "", feeUhubUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "EUR", feeUhubUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, feeUhubUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 100m, feeUhubUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 100m, feeUhubUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 100m, feeUhubUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 200m, feeUhubUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 200m, feeUhubUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", uHubChargeCode.PK, feeUhubUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", ZGuid.Empty, feeUhubUsage.BU9_AC_DiscountChargeCode);
			});

			billedUsages = invoiceFactory.Load<EdiBilledUsage>(new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoices[2].PK));
			AssertEquals(4, billedUsages.Length);
			CombineAssertions(() =>
			{
				var priceItemAUsage = billedUsages.Single(x => x.BU9_L7 == priceItemA.PK);
				AssertEquals("BU9_PeriodStart", periodStart, priceItemAUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "AAA", priceItemAUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "AA1", priceItemAUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PA", priceItemAUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", priceItemAUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 4m, priceItemAUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemAUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 100m, priceItemAUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 75m, priceItemAUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 200m, priceItemAUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 150m, priceItemAUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemAUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemAUsage.BU9_AC_DiscountChargeCode);
			});

			CombineAssertions(() =>
			{
				var priceItemBUsage = billedUsages.Single(x => x.BU9_L7 == priceItemB.PK);
				AssertEquals("BU9_PeriodStart", periodStart, priceItemBUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "BBB", priceItemBUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "BB1", priceItemBUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#PB", priceItemBUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", priceItemBUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 2m, priceItemBUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 25m, priceItemBUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 50m, priceItemBUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 10m, priceItemBUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 100m, priceItemBUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 20m, priceItemBUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, priceItemBUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCode.PK, priceItemBUsage.BU9_AC_DiscountChargeCode);
			});

			CombineAssertions(() =>
			{
				var serviceUsage = billedUsages.Single(x => x.BU9_UsageCode == "SVC" && x.BU9_PriceCode == "CCC");
				AssertEquals("BU9_PeriodStart", periodStart, serviceUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "SVC", serviceUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "CCC", serviceUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "CCC", serviceUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", serviceUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 10m, serviceUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 10m, serviceUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 100m, serviceUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 90m, serviceUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 100m, serviceUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 90m, serviceUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, serviceUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", ZGuid.Empty, serviceUsage.BU9_AC_DiscountChargeCode);

				var billedDiscounts = serviceUsage.Factory.Load<EdiBilledDiscount>(new ZQuery(EdiBilledDiscountSchema.BD9_BU9_Usage, serviceUsage.PK));
				AssertEquals(1, billedDiscounts.Length);
				AssertEquals(10m, billedDiscounts[0].BD9_Percent);
				AssertEquals(discount10.PK, billedDiscounts[0].BD9_PHD_Discount);
				AssertEquals(10m, billedDiscounts[0].BD9_TransactionAmount);
				AssertEquals("", billedDiscounts[0].BD9_Type);
			});

			CombineAssertions(() =>
			{
				var npServiceUsage = billedUsages.Single(x => x.BU9_UsageCode == "#NP" && x.BU9_PriceCode == "#NP");
				AssertEquals("BU9_PeriodStart", periodStart, npServiceUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "#NP", npServiceUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "#NP", npServiceUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "#NP", npServiceUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", npServiceUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, npServiceUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 150m, npServiceUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 150m, npServiceUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 150m, npServiceUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 150m, npServiceUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 150m, npServiceUsage.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_AC_AmountChargeCode", amountChargeCode.PK, npServiceUsage.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", ZGuid.Empty, npServiceUsage.BU9_AC_DiscountChargeCode);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 9, 1)]
		public void TestCreateInvoice_MultipleProducts()
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

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "CW1", "CW2", "WTA1", "WTA2" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DISC1", "DISC2", "DISC3", "DISC4" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DEP1", "DEP2", "DEP3", "DEP4" });

			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var abcPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany);
			abcPriceHeader.L6_DiscountCode = "STL1";
			abcPriceHeader.L6_UseStandardDiscount = false;
			abcPriceHeader.L6_SystemCode = "DEF";

			var item1 = BillingTestHelper.AddPriceItem(abcPriceHeader, "P01", "", "", 4m, "", 40m);
			item1.L7_ChargeCode = "WTA1";
			item1.L7_DiscountChargeCode = "DISC1";
			item1.L7_DepositChargeCode = "DEP1";
			item1.L7_Category = "SAT";

			var item2 = BillingTestHelper.AddPriceItem(abcPriceHeader, "P02", "", "", 8m, "", 80m);
			item2.L7_ChargeCode = "WTA2";
			item2.L7_DiscountChargeCode = "DISC2";
			item2.L7_DepositChargeCode = "DEP2";
			item2.L7_Category = "SAT";

			var stlPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "CUS", "US3", "US4");
			var item3 = stlPriceHeader.Items.Single(x => x.L7_Code == "US3");
			item3.L7_ChargeCode = "CW1";
			item3.L7_DiscountChargeCode = "DISC3";
			item3.L7_DepositChargeCode = "DEP3";
			var item4 = stlPriceHeader.Items.Single(x => x.L7_Code == "US4");
			item4.L7_ChargeCode = "CW2";
			item4.L7_DiscountChargeCode = "DISC4";
			item4.L7_DepositChargeCode = "DEP4";

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
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", periodStart, lic1.Database.ClientCompanies[0], 9);
			lic1.Database.LD_OH_WebAccessOrg = lic2.Database.LD_OH_WebAccessOrg = lic1.Company.LC_OH;
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2.Database.ClientCompanies[0], 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "US3", periodStart, lic2.Database.ClientCompanies[0], 2);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "US4", periodStart, lic2.Database.ClientCompanies[0], 3);
			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single();
			AssertNoErrors("bill1", bill1);
			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();
			var invoiceAsString = string.Join("\r\n", invoice1.Lines.OfType<InvoicingLineBase>().OrderBy(x => x.AL_Sequence)
				.Select(x => $"{x.AL_Sequence} - {x.ChargeCode.AC_Code}"));
			AssertEquals(@"1 - COMMENT
2 - COMMENT
3 - ODPLMTHUSE
4 - CW1
5 - CW2
6 - WTA1
7 - WTA2
8 - COMMENT
9 - COMMENT", invoiceAsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 9, 1)]
		public void TestCreateInvoice_InvoiceDescription()
		{
			var invoiceDescriptionValue = new CodeDescriptionBoolCollection(3);
			invoiceDescriptionValue.Add("XYZ", (NoResString)"XYZ Invoice - {Date:MMMM} ⊂(◉‿◉)つ {Date:yyyy}", true);
			EDIDataRegistry.Instance.StlMonthlyUsageInvoiceDescription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, invoiceDescriptionValue);

			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("XYZ", "XYZ Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var categories = new CodeDescriptionPairList(EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value);
			categories.AddPair("XYZ", "XYZ");
			EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "XYZ";
			priceList1.RawUsageCategory = "XYZ";
			priceList1.PriceListCode = "XYZ";
			priceList1.Description = "XYZ - Price v1";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "CW1", "CW2", "XYZ1", "WTA2" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DISC1", "DISC2", "DISC3", "DISC4" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DEP1", "DEP2", "DEP3", "DEP4" });

			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var priceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany);
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_UseStandardDiscount = false;
			priceHeader.L6_SystemCode = "XYZ";

			var item1 = BillingTestHelper.AddPriceItem(priceHeader, "P01", "", "", 4m, "", 40m);
			item1.L7_ChargeCode = "XYZ1";
			item1.L7_DiscountChargeCode = "DISC1";
			item1.L7_DepositChargeCode = "DEP1";
			item1.L7_Category = "XYZ";
 
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var database = lic1.Database;
			database.LD_Product = "XYZ";

			BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, periodStart);
			BillingTestHelper.CreateChargeableUsage(Factory, "XYZ", "P01", periodStart, lic1.Database.ClientCompanies[0], 6);
			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single();
			AssertNoErrors("bill1", bill1);
			var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();
			AssertEquals("XYZ Invoice - September ⊂(◉‿◉)つ 2022", invoice1.AH_Desc);
			var invoiceAsString = string.Join("\r\n", invoice1.Lines.OfType<InvoicingLineBase>().OrderBy(x => x.AL_Sequence)
				.Select(x => $"{x.AL_Sequence} - {x.ChargeCode.AC_Code}"));
			AssertEquals(@"1 - COMMENT
2 - COMMENT
3 - XYZ1
4 - COMMENT
5 - COMMENT", invoiceAsString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 9, 1)]
		public void TestCreateInvoice_Attachments()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("XYZ", "XYZ Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var categories = new CodeDescriptionPairList(EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value);
			categories.AddPair("XYZ", "XYZ");
			EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "XYZ";
			priceList1.RawUsageCategory = "XYZ";
			priceList1.PriceListCode = "XYZ";
			priceList1.Description = "XYZ - Price v1";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "CW1", "CW2", "XYZ1", "WTA2" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DISC1", "DISC2", "DISC3", "DISC4" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DEP1", "DEP2", "DEP3", "DEP4" });

			var periodStart = ZDateTime.Today;
			periodStart = periodStart.AddDays(1 - periodStart.Day);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var priceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany);
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_UseStandardDiscount = false;
			priceHeader.L6_SystemCode = "XYZ";

			var item1 = BillingTestHelper.AddPriceItem(priceHeader, "P01", "", "", 4m, "", 40m);
			item1.L7_ChargeCode = "XYZ1";
			item1.L7_DiscountChargeCode = "DISC1";
			item1.L7_DepositChargeCode = "DEP1";
			item1.L7_Category = "XYZ";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var database = lic1.Database;
			database.LD_Product = "XYZ";

			BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, periodStart);
			BillingTestHelper.CreateChargeableUsage(Factory, "XYZ", "P01", periodStart, lic1.Database.ClientCompanies[0], 6);
			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single();
			AssertNoErrors("bill1", bill1);

			using (Globals.TemporaryOverrideForIsTest(false))
			{
				var invoice1 = bill1.CreateInvoices(ZDateTime.Empty).Single();
				var invoiceAsString = string.Join("\r\n", invoice1.Lines.OfType<InvoicingLineBase>().OrderBy(x => x.AL_Sequence)
					.Select(x => $"{x.AL_Sequence} - {x.ChargeCode.AC_Code}"));
				AssertEquals(@"1 - COMMENT
2 - COMMENT
3 - XYZ1
4 - COMMENT
5 - COMMENT", invoiceAsString);

				var docsAsString = string.Join("\r\n", invoice1.DocManagerInfo.Files.OfType<StorageDocsBase>().Select(x => $"{x.SC_FileNameWithExtension}-{x.SC_DocType}").OrderBy(x => x));
				AssertEquals(@"AAAAAA September 2022 Billing Summary.pdf-OD1
AAAAAA September 2022 Billing Summary.xlsx-OD1", docsAsString);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateRevenueBreakdown_BorderWiseAndCW1WithSamePriceCode()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdLicCompany, "USR", "USW");
			stlPrices.L6_RX_NKCurrency = "AUD";
			var stlPriceItemWithSameCode = stlPrices.Items.FindByCode("USW");
			stlPriceItemWithSameCode.L7_Price = 7m;

			var borderWisePrices = stdLicCompany.PriceHeaders.AddNew();
			borderWisePrices.L6_SystemCode = BillingConstants.PriceHeaderType.BorderWise;
			borderWisePrices.L6_UseStandardDiscount = false;
			borderWisePrices.L6_DiscountCode = "BOR1";
			borderWisePrices.L6_PricelistVersion = "BOR v1";
			borderWisePrices.L6_RX_NKCurrency = "AUD";
			borderWisePrices.L6_LicenceUnitRate = 5m;
			borderWisePrices.L6_ValidFrom = new ZDateTime(2017, 10, 1);
			var borderwisePriceItem = borderWisePrices.Items.AddNew();
			borderwisePriceItem.L7_Category = BillingConstants.BillingSystem.BorderWise;
			borderwisePriceItem.L7_Code = "USW";
			borderwisePriceItem.L7_FeeType = BillingConstants.FeeType.Transactional;
			borderwisePriceItem.L7_Description = "Registered User";
			borderwisePriceItem.L7_Price = 30;
			borderwisePriceItem.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			borderwisePriceItem.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			borderwisePriceItem.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			borderwisePriceItem.L7_LicenceUnits = 5 * 30m;

			var licBOR = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1");
			BillingTestHelper.SetInvoicing(licBOR, Env.CurrentBranch.PK, "AUD");
			var licCW = BillingTestHelper.CreateAnotherDatabase(licBOR, "PRD", false);
			licCW.LA_AgreedLiveDate = periodStart;
			licCW.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			BillingTestHelper.CreatePriceLink(licCW.Database, stlPrices, periodStart);
			var clientCompany1 = BillingTestHelper.CreateClientCompany(licCW.Database, "CO1");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(licCW.Database, "CO2");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, clientCompany1, 25);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", periodStart, clientCompany2, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USW", periodStart, clientCompany1, 25);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USW", periodStart, clientCompany2, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, "USW", periodStart, licBOR.LA_LC, 1);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.Bills.Count);
			var bill = billing.Bills[0];
			AssertNoErrors(bill);
			var invoice = bill.CreateInvoicesWithoutSave().Single();

			var invoiceFactory = invoice.Factory;
			var query = new ZQuery(EdiBilledUsageSchema.BU9_AH_Invoice, invoice.PK);
			var billedUsages = invoiceFactory.Load<EdiBilledUsage>(query);

			CombineAssertions(() =>
			{
				var borderwisePriceItemUsage = billedUsages.Single(x => x.BU9_L7 == borderwisePriceItem.PK);

				AssertEquals("BU9_PeriodStart", periodStart, borderwisePriceItemUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "BOR", borderwisePriceItemUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "USW", borderwisePriceItemUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "USW", borderwisePriceItemUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", borderwisePriceItemUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 1m, borderwisePriceItemUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 30m, borderwisePriceItemUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 30m, borderwisePriceItemUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 30m, borderwisePriceItemUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 30m, borderwisePriceItemUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 30m, borderwisePriceItemUsage.BU9_LocalAmountPostDiscount);
			});

			CombineAssertions(() =>
			{
				var stlPriceItemUsage = billedUsages.Single(x => x.BU9_L7 == stlPriceItemWithSameCode.PK && x.BU9_LCC == clientCompany1.PK);

				AssertEquals("BU9_PeriodStart", periodStart, stlPriceItemUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "STL", stlPriceItemUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "USW", stlPriceItemUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "USW", stlPriceItemUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", stlPriceItemUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 25m, stlPriceItemUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 7m, stlPriceItemUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 25 * 7m, stlPriceItemUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 25 * 7m, stlPriceItemUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 25 * 7m, stlPriceItemUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 25 * 7m, stlPriceItemUsage.BU9_LocalAmountPostDiscount);
			});

			CombineAssertions(() =>
			{
				var stlPriceItemUsage = billedUsages.Single(x => x.BU9_L7 == stlPriceItemWithSameCode.PK && x.BU9_LCC == clientCompany2.PK);

				AssertEquals("BU9_PeriodStart", periodStart, stlPriceItemUsage.BU9_PeriodStart);
				AssertEquals("BU9_UsageCode", "STL", stlPriceItemUsage.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "USW", stlPriceItemUsage.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "USW", stlPriceItemUsage.BU9_PriceCode);
				AssertEquals("BU9_PriceCurrency", "AUD", stlPriceItemUsage.BU9_PriceCurrency);
				AssertEquals("BU9_UnitCount", 3m, stlPriceItemUsage.BU9_UnitCount);
				AssertEquals("BU9_UnitPrice", 7m, stlPriceItemUsage.BU9_UnitPrice);
				AssertEquals("BU9_TransactionAmountPreDiscount", 3 * 7m, stlPriceItemUsage.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 3 * 7m, stlPriceItemUsage.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_LocalAmountPreDiscount", 3 * 7m, stlPriceItemUsage.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 3 * 7m, stlPriceItemUsage.BU9_LocalAmountPostDiscount);
			});

			AssertEquals(5, billedUsages.Length);
		}

		[UseSnapshotProtection]
		public void TestOnInvoiceSaved()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_TransactionNum = "INV1111245";
			invoice.Factory.Save();
			var f1 = invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "aaa.pdf", "BIN");
			var f2 = invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "bbb.pdf", "BIN");
			var dynMethod = billRecipient.GetType().GetMethod("OnInvoiceSaved", BindingFlags.NonPublic | BindingFlags.Instance);

			ExceptionReporterTestListener.Instance.Clear();

			using (Globals.TemporaryOverrideForIsTest(false))
			{
				dynMethod.Invoke(billRecipient, new object[] { invoice });
			}

			var errorMessage = new ZStringBuilder();
			errorMessage.AppendLine($"TX#:{invoice.AH_TransactionNum}");
			errorMessage.AppendLine(",unsavedDocs:" + string.Join("|", invoice.DocManagerInfo.Files.OfType<StorageDocsBase>().Select(x => $"{x.PK}@{x.Name}")));
			errorMessage.AppendLine(",unsavedDocsAfterSave:");

			var reportXml = TestConnection.ExecuteScalar<string>("SELECT TOP 1 QER_ReportXml FROM StmErrorReport WHERE CAST(QER_ReportXml AS VARCHAR(MAX)) LIKE '%StlBill.OnInvoiceSaved.UnsavedDocs%' ORDER BY QER_SystemCreateTimeUtc DESC;");
			AssertContains("InvalidOperationException", reportXml);
			AssertContains(errorMessage.ToString(), reportXml);
			AssertContains("<Key>StlBill.OnInvoiceSaved.UnsavedDocs</Key>", reportXml);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDisbursement()
		{
			var stdLicence = BillingTestHelper.CreateLicence(Factory, "BBB", "SY2", "BBB", false);
			var stdPriceCompany = stdLicence.Company;
			var stlPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "USR", "SHP");
			stlPrices.L6_UseStandardDiscount = false;
			stlPrices.L6_RX_NKCurrency = "USD";
			stlPrices.L6_DiscountCode = "CW1";
			stlPrices.L6_HasExchangeRates = true;
			var rate1 = stlPrices.ExchangeRates.AddNew();
			rate1.PHE_GroupCode = "STL";
			rate1.PHE_RX_NKCurrency = "AUD";
			rate1.PHE_Rate = 1.5;
			rate1 = stlPrices.ExchangeRates.AddNew();
			rate1.PHE_GroupCode = "STL";
			rate1.PHE_RX_NKCurrency = "USD";
			rate1.PHE_Rate = 1;
			var discount1a = AddPercentageDiscount(stlPrices, "Discount 1", 10m);
			AddDiscountStructure(stlPrices, "Standard", discount1a);

			var usr = stlPrices.Items.Single(x => x.L7_Code == "USR");
			usr.L7_ChargeCode = "CW1";
			usr.L7_DiscountChargeCode = "DISC1";
			usr.L7_DepositChargeCode = "DEP1";
			usr.L7_PGM_DiscountGroupCode = "Standard";
			usr.L7_ExchangeRateGroupCode = "STL";
			usr.L7_Price = 2.5m;

			var cwnPrices = BillingTestHelper.CreateStlPriceList(stdPriceCompany, "SHD", "SHX");
			cwnPrices.L6_PricelistVersion = "CWN 1.0";
			cwnPrices.L6_SystemCode = "CWN";
			cwnPrices.L6_UseStandardDiscount = false;
			cwnPrices.L6_DiscountCode = "CWN";
			cwnPrices.L6_RX_NKCurrency = "USD";
			cwnPrices.L6_HasExchangeRates = true;
			var discount1b = AddPercentageDiscount(cwnPrices, "Discount 2", 15m);
			AddDiscountStructure(cwnPrices, "Standard", discount1b);
			var rate = cwnPrices.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "CWN";
			rate.PHE_RX_NKCurrency = "AUD";
			rate.PHE_Rate = 1.5;
			rate = cwnPrices.ExchangeRates.AddNew();
			rate.PHE_GroupCode = "CWN";
			rate.PHE_RX_NKCurrency = "USD";
			rate.PHE_Rate = 1;

			var usageMap = cwnPrices.UsageMaps.AddNew();
			usageMap.PUM_PriceCategory = "CWN";
			usageMap.PUM_PriceCode = "SHD";
			usageMap.PUM_UsageCategory = "STL";
			usageMap.PUM_UsageCode = "SHD";
			var shd = cwnPrices.Items.Single(x => x.L7_Code == "SHD");
			shd.L7_Category = "CWN";
			shd.L7_ChargeCode = "CW2";
			shd.L7_DiscountChargeCode = "DISC2";
			shd.L7_DepositChargeCode = "DEP2";
			shd.L7_PGM_DiscountGroupCode = "Standard";
			shd.L7_ExchangeRateGroupCode = "CWN";
			shd.L7_Price = 3.5m;

			Factory.Save();

			var periodStart = BillingTestHelper.MonthToday;
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SY1", "AAA");
			var lc = licence.Company;
			BillingTestHelper.SetInvoicing(licence, Env.CurrentBranch.PK, "USD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 1983;
			db.LD_HostedLocation = "SYD";
			var clientCompany1 = db.ClientCompanies[0];

			BillingTestHelper.CreatePriceLink(db, stlPrices, periodStart);
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdLicence.LicenceCode);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, clientCompany1, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "US1", periodStart, clientCompany1, 3);
			var shdUsage = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHD", periodStart, clientCompany1, 1);
			shdUsage.U1_RX_NKCurrency = "USD";
			shdUsage.U1_TotalPrice = 13.5m;

			BillingTestHelper.LoadClientSpecificDocuments();
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "CW1", "CW2", "XYZ1", "WTA2" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DISC1", "DISC2", "DISC3", "DISC4" });
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch, new[] { "DEP1", "DEP2", "DEP3", "DEP4" });
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 1, periodStart);

			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single();
			AssertNoErrors("bill1", bill1);
			var invoices = bill1.CreateInvoices(ZDateTime.Empty).ToArray();
			AssertEquals(1, invoices.Length);

			var desc = periodStart.ToString("MMMM yyyy");

			AssertInvoice<EDIARInvoice>(invoices[0],
// 3 unit * 2.5 SHP price * 1.5 fx rate * 1.1 tax = 12.375
// 12.375 * 10% discount = 1.23
// 12.38 - 1.23 = 11.15
// 13.5 USD * 1.1 tax = 14.85
// 14.85 * 15% discount = 2.22
$@"STL Monthly Usage Invoice - {desc}
USD
23.78
LINE: CW1 - USD - 12.38
LINE: CW2 - USD - 14.85
LINE: DISC1 - USD - -1.23
LINE: DISC2 - USD - -2.22
");
			void AssertInvoice<T>(InvoicingBase inv, string invAsString)
			{
				var sb = new StringBuilder();
				AssertType<T>(inv);
				sb.AppendLine(inv.AH_Desc);
				sb.AppendLine(inv.AH_RX_NKTransactionCurrency);
				sb.AppendLine(inv.AH_OSTotalAmount.ToString());
				var linesAsString = string.Join("\r\n", inv.Lines.OfType<InvoicingLineBase>()
					.Where(x => x.AL_OSAmount != 0)
					.Select(x => $"LINE: {x.ChargeCode.AC_Code} - {x.AL_RX_NKTransactionCurrency} - {x.AL_OSAmount}").OrderBy(x => x));
				sb.AppendLine(linesAsString);
				AssertEquals(invAsString, sb.ToString());
			}
		}

		#region Implementation

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

		StlMonthlyUsage NewMonthlyUsage()
		{
			return StlMonthlyUsageTest.CreateMonthlyUsage(licence, new ZDateTime(2015, 7, 1));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewBill();
		}

		StlBill NewBill()
		{
			return NewBill("AUD");
		}

		StlBill NewBill(ZString currencyCode)
		{
			var bill = new StlBill(Factory, GlbBranch.CurrentBranch, organisation, currencyCode, new ZDateTime(2015, 7, 1), new ZDateTime(2015, 7, 1));
			bill.LocalExchangeRate = 1;
			return bill;
		}

		StlBill NewBillForInvoiceSplitTest(ZString currencyCode)
		{
			var bill = new StlBillForInvoiceSplitTest(Factory, GlbBranch.CurrentBranch, organisation, currencyCode, new ZDateTime(2015, 7, 1), new ZDateTime(2015, 7, 1));
			bill.LocalExchangeRate = 1;
			return bill;
		}

		EDIOrgHeader organisation;
		LicenceHeader licence;
		StlBill billRecipient;

		protected override void SetUp()
		{
			base.SetUp();
			licence = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			organisation = licence.Company.Header;
			var delivery = licence.Company.InvoiceDeliveries.AddNew();
			delivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			delivery.L9_RX_NKInvoiceCurrency = "AUD";
			billRecipient = NewBill();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		class StlBillForInvoiceSplitTest : StlBill
		{
			public StlBillForInvoiceSplitTest(BusinessObjectFactory factory, GlbBranch invoicingBranch, EDIOrgHeader org, ZString currencyCode, ZDateTime periodStart, ZDateTime dateForExchangeRate, bool isBilled = true) : base(factory, invoicingBranch, org, currencyCode, periodStart, dateForExchangeRate, isBilled)
			{
			}

			protected override EDIARInvoiceTaxProcessor GetInvoiceTaxProcessor() => new EDIARInvoiceTaxProcessorTest.EDIARInvoiceTaxProcessorForTest();
		}

		#endregion
	}
}

