using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(DocStlSummary))]
	sealed class DocStlSummaryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSurcharge()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company);
			var item1 = BillingTestHelper.AddPriceItem(priceHeader, "C03", BillingConstants.FeeType.Transactional, "", 40m);
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, period1);

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

			var docStlSummary = DocStlSummary.New(bill1.MonthlyUsages.First(), Factory);
			AssertEquals(1, docStlSummary.PriceSummaryLines.Count);
			var line1 = docStlSummary.PriceSummaryLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("CurrencyCode", "AUD", line1.Currency);
				AssertEquals("UndiscountedAmount", "120.00", line1.UndiscountedAmount);
				AssertEquals("DiscountAmount", "0.00", line1.DiscountAmount);
				AssertEquals("VersionSurchargeDescription", "", line1.VersionSurchargeDescription);
				AssertEquals("VersionSurchargeAmount", "", line1.VersionSurchargeAmount);
				AssertEquals("InvoiceSurchargeDescription", "Surcharge for Manual Processing Fee", line1.InvoiceSurchargeDescription);
				AssertEquals("InvoiceSurchargeAmount", "6.00", line1.InvoiceSurchargeAmount);
				AssertEquals("FinalAmount", "126.00", line1.FinalAmount);
				AssertEquals("GroupBy", "AUD", line1.GroupBy);
			});
		}

		public void TestSurcharge_Fees()
		{
			var period1 = ZDateTime.Today;
			period1 = period1.AddDays(1 - period1.Day);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company);
			var item1 = BillingTestHelper.AddPriceItem(priceHeader, "C03", BillingConstants.FeeType.Transactional, "", 40m);
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, period1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C03", period1, lic1.ClientCompany, 3);

			lic1.Company.SelfBilling.L4_ProcessingFee = "MPF";
			lic1.Company.SelfBilling.L4_ProcessingFeePercent = 5m;

			var fee1 = BillingTestHelper.CreateLicenceFee(lic1.Company, "AAA", 100m);

			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertEquals(11m, bill1.InvoiceSurchargeTotal);
			AssertEquals(5m, bill1.SurchargePercent);
			AssertEquals("Manual Processing Fee", bill1.SurchargeDescription.ToString());
			AssertEquals(220m, bill1.InvoicePreDiscountTotal);
			AssertEquals(231m, bill1.InvoicePostDiscountTotal);

			var docStlSummary = DocStlSummary.New(bill1.MonthlyUsages.First(), Factory);
			AssertEquals(1, docStlSummary.PriceSummaryLines.Count);
			var line1 = docStlSummary.PriceSummaryLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("CurrencyCode", "AUD", line1.Currency);
				AssertEquals("UndiscountedAmount", "220.00", line1.UndiscountedAmount);
				AssertEquals("DiscountAmount", "0.00", line1.DiscountAmount);
				AssertEquals("VersionSurchargeDescription", "", line1.VersionSurchargeDescription);
				AssertEquals("VersionSurchargeAmount", "", line1.VersionSurchargeAmount);
				AssertEquals("InvoiceSurchargeDescription", "Surcharge for Manual Processing Fee", line1.InvoiceSurchargeDescription);
				AssertEquals("InvoiceSurchargeAmount", "11.00", line1.InvoiceSurchargeAmount);
				AssertEquals("FinalAmount", "231.00", line1.FinalAmount);
				AssertEquals("GroupBy", "AUD", line1.GroupBy);
			});

			var feeLine = docStlSummary.FeeLinesWithoutPrepay[0];
			AssertEquals("100.00", feeLine.Amount);
			AssertEquals(",AAA fee description,,,,100.00,,,AUD,100.00,,,,,", feeLine.Code);
		}

		public void TestVersionSurcharge()
		{
			EDIDataRegistry.Instance.VersionSurchargeDescription.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Old Gpr Fee");
			var period1 = new DateTime(2022, 5, 1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			lic1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = lic1.Database;
			db1.LD_ReleaseRing = "GP1";
			VersionHistoryTest.CreateBuildsAndSetDatabaseToNonCurrentVersion(db1, period1, 2);

			var priceHeader = BillingTestHelper.CreateStlPriceList(lic1.Company);
			var item1 = BillingTestHelper.AddPriceItem(priceHeader, "C03", BillingConstants.FeeType.Transactional, "", 40m);
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			BillingTestHelper.CreatePriceLink(lic1.Database, priceHeader, period1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", period1, lic1.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "C03", period1, lic1.ClientCompany, 3);

			var versionSurchargeSetting = Factory.New<VersionSurchargeLicenceSetting>();
			versionSurchargeSetting.LS9_LD = db1.PK;
			versionSurchargeSetting.LS9_ValidFrom = period1;
			versionSurchargeSetting.LS9_Percent = 4m;
			db1.LicenceSettings.Add(versionSurchargeSetting);
			Factory.Save();

			var billing1 = new StlBilling(new BusinessObjectFactory() { RefreshEnabled = false });
			billing1.AccumulateMonths = 0;
			billing1.DateTo = period1.AddMonths(1).AddDays(-1);
			billing1.GenerateReport(null);
			var bill1 = billing1.Bills[0];
			AssertEquals(1, bill1.MonthlyUsages.Count());
			AssertEquals("other surcharge amount", 0m, bill1.InvoiceSurchargeTotal);
			AssertEquals("other surcharge", 0m, bill1.SurchargePercent);
			AssertEquals(120m, bill1.InvoicePreDiscountTotal);
			AssertEquals(124.8m, bill1.InvoicePostDiscountTotal);

			var docStlSummary = DocStlSummary.New(bill1.MonthlyUsages.First(), Factory);
			AssertEquals(1, docStlSummary.PriceSummaryLines.Count);
			var line1 = docStlSummary.PriceSummaryLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("CurrencyCode", "AUD", line1.Currency);
				AssertEquals("UndiscountedAmount", "120.00", line1.UndiscountedAmount);
				AssertEquals("DiscountAmount", "0.00", line1.DiscountAmount);
				AssertEquals("VersionSurchargeDescription", "Old Gpr Fee (22.1.6.500) (4%)", line1.VersionSurchargeDescription);
				AssertEquals("VersionSurchargeAmount", "4.80", line1.VersionSurchargeAmount);
				AssertEquals("InvoiceSurchargeDescription", "", line1.InvoiceSurchargeDescription);
				AssertEquals("InvoiceSurchargeAmount", "", line1.InvoiceSurchargeAmount);
				AssertEquals("FinalAmount", "124.80", line1.FinalAmount);
				AssertEquals("GroupBy", "AUD", line1.GroupBy);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBorderWisePurchased()
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
			var purchaseSetting = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting.LS9_ValidFrom = periodStart;
			purchaseSetting.LS9_LD = licBOR1.LA_LD;
			purchaseSetting.LicenceCount = 4;
			purchaseSetting.PriceCode = BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode;
			licBOR1.Database.LicenceSettings.Add(purchaseSetting);

			var price1 = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode).L7_Price;
			var price2 = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.AUProPackPriceCode).L7_Price;
			Assert(price1 != 0m);
			Assert(price2 != 0m);

			var u1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, licBOR1.LA_LC, 7);
			var u2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, licBOR2.LA_LC, 5);
			var u3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUProPackPriceCode, periodStart, licBOR1.LA_LC, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCW1.ClientCompany, 50);

			// next month they had less users than legacy licences
			var u4 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart.AddMonths(1), licBOR1.LA_LC, 1);
			var u5 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart.AddMonths(1), licBOR2.LA_LC, 2);
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
				AssertEquals("billCW1.InvoicePreDiscountTotal " + price1 + " " + price2, ((7 + 5 - 4) * price1 + 3 * price2 + 50 * 1.00m), bill.InvoicePreDiscountTotal);
				AssertEquals("billCW1.InvoicePostDiscountTotal", bill.InvoicePreDiscountTotal, bill.InvoicePostDiscountTotal);

				var docStlSummary = DocStlSummary.New(bill.MonthlyUsages.First(), Factory);
				AssertEquals("BorderWiseItemLines", 2, docStlSummary.BorderWiseItemLines.Count);
				AssertEquals("ItemLines", 1, docStlSummary.ItemLines.Count);
				var bwLines = docStlSummary.BorderWiseItemLines.Cast<UsageLine>().ToList();
				var bwLine1 = bwLines[0];
				AssertEquals("IncludedUnitCount 1", 4, bwLine1.IncludedUnitCount);
				AssertEquals("TotalUnitCount 1", 12m, bwLine1.TotalUnitCount);
				var bwLine2 = bwLines[1];
				AssertEquals("IncludedUnitCount 2", 0, bwLine2.IncludedUnitCount);
				AssertEquals("TotalUnitCount 2", 3m, bwLine2.TotalUnitCount);
			});

			CombineAssertions(() =>
			{
				var bill = billing2.Bills.Cast<StlBill>().Single(x => x.Organisation.PK == licCW1.Company.LC_OH);
				AssertNoErrors("billCW1", bill);
				AssertEquals("MonthlyUsages", 1, bill.MonthlyUsages.Count());
				AssertEquals("PriceCurrencies", 1, bill.MonthlyUsages.First().PriceCurrencies.Count());
				AssertEquals("PriceCurrency", "AUD", bill.MonthlyUsages.First().PriceCurrencies.First());
				AssertEquals("billCW1.InvoicePreDiscountTotal", 50 * 1.00m, bill.InvoicePreDiscountTotal);
				AssertEquals("billCW1.InvoicePostDiscountTotal", 0, bill.InvoicePreDiscountTotal, bill.InvoicePostDiscountTotal);

				var docStlSummary = DocStlSummary.New(bill.MonthlyUsages.First(), Factory);
				AssertEquals("BorderWiseItemLines", 1, docStlSummary.BorderWiseItemLines.Count);
				AssertEquals("ItemLines", 1, docStlSummary.ItemLines.Count);
				var bwLine = docStlSummary.BorderWiseItemLines.Cast<UsageLine>().First();
				AssertEquals("IncludedUnitCount", 4, bwLine.IncludedUnitCount);
				AssertEquals("TotalUnitCount", 3m, bwLine.TotalUnitCount);
			});
		}

		public void TestPrepaymentSummary()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			StlMonthlyUsage monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(licHeader1, new ZDateTime(2015, 9, 1));
			StlBill bill = new StlBill(Factory, GlbBranch.CurrentBranch, licHeader1.Company.Header, "AUD", new ZDateTime(2015, 9, 1), new ZDateTime(2015, 9, 1));
			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var prepay = bill.PrepayNext;
			prepay.CurrentPrepaymentBalance = 100m;
			prepay.PrepaymentBalanceRequired = 300;
			prepay.FuturePrepaymentBalanceRequired = 400m;
			prepay.CurrentInvoiceTotalAmount = 140m;

			foreach (var isPrepaymentDiscountAvailable in new[] { true, false })
			{
				monthlyUsage.IsPrepaymentDiscountAvailable = isPrepaymentDiscountAvailable;
				var summary = DocStlSummary.New(monthlyUsage, Factory);
				AssertEquals(1, summary.PrepaymentSummaryLines.Count);
				var line = summary.PrepaymentSummaryLines[0];
				AssertEquals("100.00,300.00,,Revised Prepayment Balance - Effective 1st June 2018,440.00,400.00,300.00,AUD,,,,,,", line.UnsortedCode);
			}
		}

		public void TestUsageSummaryLines_SharedCommitment_UsageBelowCommitment()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK);
			var setting = Factory.New<CommitmentLicenceSetting>();
			setting.LS9_LD = lic.LA_LD;
			setting.LS9_ValidFrom = periodStart;
			setting.LS9_ValidTo = periodStart.AddMonths(1).AddDays(-1);
			setting.LicenceUnits = 1000;
			setting.LS9_Name = "SHARED";
			lic.Database.LicenceSettings.Add(setting);
			var monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(lic, periodStart);

			var usageLine = new UsageLine(Factory);
			usageLine.PeriodStart = periodStart;
			usageLine.TotalUnitCount = 1;
			usageLine.SetLicenceUnits(500m);
			monthlyUsage.AddUsageLine(usageLine);

			monthlyUsage.CommitmentGroup = new StlBilling.SharedCommitment()
			{
				TotalUsedLicenceUnits = 600,
				Adjustment = 200
			};
			var bill = new StlBill(Factory, GlbBranch.CurrentBranch, lic.Company.Header, "AUD", periodStart, periodStart);
			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var summary = DocStlSummary.New(monthlyUsage, Factory);
			AssertEquals(1, summary.UsageSummaryLines.Count);
			var line = summary.UsageSummaryLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Used", "500.0", line.Column1);
				AssertEquals("Shared Group Used", "600.0", line.Column4);
				AssertEquals("Commitment", "1,000.0", line.Column2);
				AssertEquals("Buying Group", "", line.Column3);
				AssertEquals("Final Amount Used is individual Used plus half the total shortfall", "700.0", line.FinalAmount);
			});
		}

		public void TestUsageSummaryLines_SharedCommitment_UsageAboveCommitment()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK);
			var setting = Factory.New<CommitmentLicenceSetting>();
			setting.LS9_LD = lic.LA_LD;
			setting.LS9_ValidFrom = periodStart;
			setting.LS9_ValidTo = periodStart.AddMonths(1).AddDays(-1);
			setting.LicenceUnits = 1000;
			setting.LS9_Name = "SHARED";
			lic.Database.LicenceSettings.Add(setting);
			var monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(lic, periodStart);

			var usageLine = new UsageLine(Factory);
			usageLine.PeriodStart = periodStart;
			usageLine.TotalUnitCount = 1;
			usageLine.SetLicenceUnits(500m);
			monthlyUsage.AddUsageLine(usageLine);

			monthlyUsage.CommitmentGroup = new StlBilling.SharedCommitment()
			{
				TotalUsedLicenceUnits = 1100,
				Adjustment = 0
			};
			var bill = new StlBill(Factory, GlbBranch.CurrentBranch, lic.Company.Header, "AUD", periodStart, periodStart);
			bill.AddMonthlyUsages(new[] { monthlyUsage });

			var summary = DocStlSummary.New(monthlyUsage, Factory);
			AssertEquals(1, summary.UsageSummaryLines.Count);
			var line = summary.UsageSummaryLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Used", "500.0", line.Column1);
				AssertEquals("Shared Group Used", "1,100.0", line.Column4);
				AssertEquals("Commitment", "1,000.0", line.Column2);
				AssertEquals("Buying Group", "", line.Column3);
				AssertEquals("Final Amount Used is individual Used plus half the total shortfall", "500.0", line.FinalAmount);
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2022, 9, 1)]
		public void TestThreeDecimal()
		{
			var testList = new CodeDescriptionPairList();
			testList.AddPair("3DP", "0=0.001");
			EDIDataRegistry.Instance.BillingPriceRoundingParams.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testList);

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

			var list = new CodeDescriptionPairList();
			list.AddPair("ABC", ".");
			EDIDataRegistry.Instance.ProductsWithThreeDecimalBillingSummary.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

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
			abcPriceHeader.L6_Rounding = "3DP";

			var item1 = BillingTestHelper.AddPriceItem(abcPriceHeader, "P01", "", "", 1.152456789m, "", 40m);
			item1.L7_ChargeCode = "WTA1";
			item1.L7_DiscountChargeCode = "DISC1";
			item1.L7_DepositChargeCode = "DEP1";
			item1.L7_Category = "SAT";

			var item2 = BillingTestHelper.AddPriceItem(abcPriceHeader, "P02", "", "", 2.687654321m, "", 80m);
			item2.L7_ChargeCode = "WTA2";
			item2.L7_DiscountChargeCode = "DISC2";
			item2.L7_DepositChargeCode = "DEP2";
			item2.L7_Category = "SAT";

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			lic1.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			var abcDatabase = lic1.Database;
			abcDatabase.LD_Product = "ABC";

			BillingTestHelper.CreatePriceLink(lic1.Database, abcPriceHeader, periodStart);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P01", periodStart, lic1.Database.ClientCompanies[0], 6);
			BillingTestHelper.CreateChargeableUsage(Factory, "SAT", "P02", periodStart, lic1.Database.ClientCompanies[0], 9);
			lic1.Database.LD_OH_WebAccessOrg = lic1.Company.LC_OH;
			Factory.Save();

			var billing = new StlBilling(Factory);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);

			AssertEquals(1, billing.Bills.Count);
			var bill1 = billing.Bills.Cast<StlBill>().Single();

			var docStlSummary = DocStlSummary.New(bill1.MonthlyUsages.First(), Factory);
			AssertEquals(1, docStlSummary.PriceSummaryLines.Count);
			var line1 = docStlSummary.PriceSummaryLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("CurrencyCode", "AUD", line1.Currency);
				AssertEquals("UndiscountedAmount", "31.104", line1.UndiscountedAmount);
				AssertEquals("DiscountAmount", "0.000", line1.DiscountAmount);
				AssertEquals("FinalAmount", "31.104", line1.FinalAmount);
				AssertEquals("GroupBy", "AUD", line1.GroupBy);
			});
		}

		public void TestDisbursement()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;

			var stlPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany);
			stlPriceHeader.L6_DiscountCode = "STL1";
			stlPriceHeader.L6_SystemCode = "STL";

			var stlItemUSR = BillingTestHelper.AddPriceItem(stlPriceHeader, "USR", "", "", 1.2m, "", 12m);
			stlItemUSR.L7_ChargeCode = "STLCHARGE";
			stlItemUSR.L7_Description = "stlItemUSR - USR";
			stlItemUSR.L7_DiscountChargeCode = "DISC1";
			stlItemUSR.L7_DepositChargeCode = "DEP1";
			stlItemUSR.L7_Category = "STL";

			var cwnPriceHeader = BillingTestHelper.CreateStlPriceList(stdLicCompany);
			stlPriceHeader.L6_DiscountCode = "CWN1";
			stlPriceHeader.L6_SystemCode = "CWN";

			var cwnItemSHD = BillingTestHelper.AddPriceItem(stlPriceHeader, "SHD", "", "", 1.5m, "", 15m);
			cwnItemSHD.L7_ChargeCode = "CWNCHARGE";
			cwnItemSHD.L7_Description = "cwnItem - SHD";
			cwnItemSHD.L7_DiscountChargeCode = "DISC2";
			cwnItemSHD.L7_DepositChargeCode = "DEP2";
			cwnItemSHD.L7_Category = "CWN";

			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lcc1 = licHeader1.Database.ClientCompanies[0];
			var lcc2 = BillingTestHelper.CreateClientCompany(licHeader1.Database, "BBB");
			Factory.Save();

			var usageUSR = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lcc1, 10);
			var usageSHD1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHD", periodStart, lcc2, 5);
			usageSHD1.U1_TotalPrice = 3;
			usageSHD1.U1_UnitCount = 1;
			usageSHD1.U1_Direction = "EXP";
			var usageSHD2 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHD", periodStart, lcc1, 5);
			usageSHD2.U1_TotalPrice = 12;
			usageSHD2.U1_UnitCount = 4;
			usageSHD2.U1_Direction = "EXP";

			var monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(licHeader1, periodStart);
			var stlUsageLine = new UsageLine(Factory, periodStart, periodStart, "", stlItemUSR, stlItemUSR, stlItemUSR, stlItemUSR);
			stlUsageLine.SetAmounts(10m, 8m);
			stlUsageLine.PriceCurrency = "AUD";
			stlUsageLine.TotalUnitCount = 10;
			stlUsageLine.AddUsage(new Usage(usageUSR) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery });

			var cwnUsageLine = new UsageLine(Factory, periodStart, periodStart, "", cwnItemSHD, cwnItemSHD, cwnItemSHD, cwnItemSHD);
			cwnUsageLine.SetAmounts(15m, 9m);
			cwnUsageLine.PriceCurrency = "USD";
			cwnUsageLine.TotalUnitCount = 5;

			var usage1 = new Usage(usageSHD1) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery };
			var usage2 = new Usage(usageSHD2) { OwnerDelivery = monthlyUsage.DatabaseUsage.OwnerDelivery };
			cwnUsageLine.AddUsages(new[] { usage1, usage2 });
			cwnUsageLine.IsDisbursement = true;
			cwnUsageLine.Direction = "EXP";

			var discountHeader = Factory.New<EdiPriceHeaderDiscount>();
			discountHeader.PHD_Name = "VOLUME";
			stlUsageLine.Discounts = new StlBilling.DiscountSet(new[] { new DummyDiscount() { Percentage = 10, HeaderDiscount = discountHeader } });
			stlUsageLine.Discounts.Key = "A";

			cwnUsageLine.Discounts = new StlBilling.DiscountSet(new[] { new DummyDiscount() { Percentage = 15, HeaderDiscount = discountHeader } });
			cwnUsageLine.Discounts.Key = "B";

			monthlyUsage.AddUsageLine(stlUsageLine);
			monthlyUsage.AddUsageLine(cwnUsageLine);

			var bill = new StlBill(Factory, GlbBranch.CurrentBranch, licHeader1.Company.Header, "AUD", periodStart, periodStart);
			bill.AddMonthlyUsages(new[] { monthlyUsage });
			var summary = DocStlSummary.New(monthlyUsage, Factory);

			AssertEquals(stlUsageLine.PK, summary.ItemLines.Single().PK);
			var disbursementItemLinesAsText = string.Join("\r\n", summary.DisbursementItemLines.Select(x => $"{x.PriceItemDesc}-{x.CompanyCode}-{x.FeeBasisText}-{x.LicenceUnitsText}-{x.UnitCount}-{x.PriceCurrency}-{x.PreDiscountAmountText}-{x.TotalLicenceUnitsText}-{x.DiscountKey}-{x.PostDiscountAmountText}-{x.Direction}").OrderBy(x => x));
			AssertEquals(@"cwnItem - SHD-AAA--15.0-4-USD-12.00-60.0-B-7.20-EXP
cwnItem - SHD-BBB--15.0-1-USD-3.00-15.0-B-1.80-EXP", disbursementItemLinesAsText);
		}

		class DummyDiscount : IStlDiscount
		{
			public EdiPriceHeaderDiscount HeaderDiscount { get; set; }

			public DiscountLicenceSetting Setting => null;

			public StlMonthlyUsage MonthlyUsage => null;

			public bool IsActive => true;

			public decimal Percentage { get; set; }

			public bool RequiredByDependentDiscount => false;

			public bool HasRequiredOtherDiscounts(System.Collections.Generic.IEnumerable<IStlDiscount> potentialDiscountsForDatabase) => false;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			StlMonthlyUsage monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(licHeader1, new ZDateTime(2015, 9, 1));
			StlBill bill = new StlBill(Factory, GlbBranch.CurrentBranch, licHeader1.Company.Header, "AUD", new ZDateTime(2015, 9, 1), new ZDateTime(2015, 9, 1));
			bill.AddMonthlyUsages(new[] { monthlyUsage });
			return DocStlSummary.New(monthlyUsage, Factory);
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
