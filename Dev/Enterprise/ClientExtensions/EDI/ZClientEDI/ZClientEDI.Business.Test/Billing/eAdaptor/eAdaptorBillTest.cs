using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.ODPL;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.eAdaptor.Test
{
	[TestedType(typeof(EAdaptorBill))]
	internal class eAdaptorBillTest : SystemBillTestCase<EAdaptorBill>
	{
		public void TestChargeCodes()
		{
			BillingTestHelper.SetTransactionChargeCode(BillingConstants.BillingSystem.eAdaptor, "AAACHARGE");
			BillingTestHelper.SetTransactionDiscountChargeCode(BillingConstants.BillingSystem.eAdaptor, "AAADISCO");
			BillingTestHelper.SetTransactionChargeCode("BBB", "BBBCHARGE");
			BillingTestHelper.SetTransactionDiscountChargeCode("BBB", "BBBDISCO");

			var bill = new EAdaptorBill(Factory);
			AssertEquals("AAACHARGE", bill.GetAmountChargeCodeName(null));
			AssertEquals("AAADISCO", bill.GetDiscountChargeCodeName(null));
		}

		public void TestDiscountsAndGroupSummarySections()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");

			BillingTestHelper.SetInvoicing(lic.Company.Header, Env.CurrentBranch.PK, "AUD");
			var prices = BillingTestHelper.CreatePriceList(lic.Company);
			prices.L6_LicenceUnitRate = 0.5;
			var priceItem1 = BillingTestHelper.AddPriceItem(prices, "IC1", BillingConstants.FeeType.Transactional, "EAM", 0.10m, 0.02m);
			var priceFee1 = BillingTestHelper.AddPriceItem(prices, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 250m);

			var discounts = lic.Company.SelfBilling.BillingDiscounts;
			var discountBreak15 = discounts.AddNew();
			discountBreak15.L5_SystemCode = BillingConstants.BillingSystem.eAdaptor;
			discountBreak15.L5_StartDate = new ZDateTime(2010, 1, 1);
			discountBreak15.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
			discountBreak15.L5_Type = BillingConstants.DiscountType.Volume;
			discountBreak15.L5_Discount = 15;
			discountBreak15.L5_BreakAmount = 15;

			var discountBreak80 = discounts.AddNew();
			discountBreak80.L5_SystemCode = BillingConstants.BillingSystem.eAdaptor;
			discountBreak80.L5_StartDate = new ZDateTime(2010, 1, 1);
			discountBreak80.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
			discountBreak80.L5_Type = BillingConstants.DiscountType.Volume;
			discountBreak80.L5_Discount = 80;
			discountBreak80.L5_BreakAmount = 80;

			// 2015-1-1
			var chargeable1 = BillingTestHelper.CreateSubUsage("IC1", 1000, priceItem1);
			var usage1 = new EAdaptorUsage(Factory, lic, new ZDateTime(2015, 1, 1));
			var feeUsage1 = new SystemUsage.SubUsage() { PriceItemCode = "EAM", PriceItem = priceFee1, RawUsageCount = 1000, UnitCount = 1, Amount = 250m };
			usage1.SetSubUsage(new[] { chargeable1 });
			usage1.AddDbUsage(new Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>(feeUsage1, new List<EAdaptorUsage>(new[] { usage1 })));
			usage1.CalculateAmount();

			// 2014-12-1
			var chargeable2 = BillingTestHelper.CreateSubUsage("IC1", 1000, priceItem1);
			var usage2 = new EAdaptorUsage(Factory, lic, new ZDateTime(2014, 12, 1));
			var feeUsage2 = new SystemUsage.SubUsage() { PriceItemCode = "EAM", PriceItem = priceFee1, RawUsageCount = 1000, UnitCount = 1, Amount = 250m };
			usage2.SetSubUsage(new[] { chargeable2 });
			usage2.AddDbUsage(new Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>(feeUsage2, new List<EAdaptorUsage>(new[] { usage2 })));
			usage2.CalculateAmount();

			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 1, 31));
			var stubModuleUsers = new Mock<IModuleUsers>();
			stubModuleUsers.Setup(m => m.TotalUserLicenceUnits(lic.Database, new ZDateTime(2015, 1, 1))).Returns(10m);
			stubModuleUsers.Setup(m => m.TotalUserLicenceUnits(lic.Database, new ZDateTime(2014, 12, 1))).Returns(100m);
			context.ModuleUsersService = stubModuleUsers.Object;

			var bill1 = new EAdaptorBill(context);
			bill1.PopulateFromSystemUsages(new[] { usage1 });

			var bill2 = new EAdaptorBill(context);
			bill2.PopulateFromSystemUsages(new[] { usage2 });

			AssertEquals("20+10 licence units hits break 15 and not break 80", 1000 * 0.1m * 0.15m, bill1.DiscountAmount);
			AssertEquals("transactions + fee", 1000 * 0.1m + 250m, bill1.Amount);

			AssertEquals("20+100 licence units hits break 80", 1000 * 0.1m * 0.80m, bill2.DiscountAmount);
			AssertEquals("transactions + fee", 1000 * 0.1m + 250m, bill2.Amount);

			//SummarySections
			var summarySections = bill1.GetGroupSummarySections();
			AssertEquals(2, summarySections.Length);
			var sectionFee = summarySections[0];
			var sectionUsage = summarySections[1];

			AssertEquals("eAdaptor Group Summary (Fees),,,,,,,,,,,0,Total,250.00", sectionFee.Header.UnsortedCode);
			AssertEquals(1, sectionFee.Lines.Count);
			AssertEquals(" (AAA-AAA-AAA),,,,,,,,250.00,,,,,", sectionFee.Lines[0].UnsortedCode);

			AssertEquals("eAdaptor Group Summary (Usage),,,,,,,,,,,20.00,Total,100.00", sectionUsage.Header.UnsortedCode);
			AssertEquals(1, sectionUsage.Lines.Count);
			AssertEquals("AAAAAA (AAA-AAA-AAA),,,,,,,,100.00,,,20.00,,", sectionUsage.Lines[0].UnsortedCode);
		}

		public void TestValidateAll_PriceHeader()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "DDD");
			var childLicence = BillingTestHelper.CreateDependentLicence(licHeader, "DD1");
			BillingTestHelper.SetInvoicing(licHeader, Env.CurrentBranch.PK);

			var usage1 = new EAdaptorUsage(Factory, licHeader, new ZDateTime(2010, 10, 01));
			var usage2 = new EAdaptorUsage(Factory, childLicence, new ZDateTime(2010, 10, 01));
			var bill = new EAdaptorBill(Factory);
			bill.PopulateFromSystemUsages(new[] { usage1, usage2 });
			bill.ValidateAll(bill);
			AssertHasRowErrorContaining(bill, ": No pricelist found for " + licHeader.Company.Header.OH_Code);
			AssertHasRowErrorContaining(bill, ": No pricelist found for " + childLicence.Company.Header.OH_Code);

			var childPriceHeader = childLicence.Company.PriceHeaders.AddNew();
			childPriceHeader.L6_RX_NKCurrency = "AUD";
			childPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			childPriceHeader.Items.AddNew().L7_Code = BillingConstants.CoreModuleCode;
			usage1 = new EAdaptorUsage(Factory, licHeader, new ZDateTime(2010, 10, 01));
			usage2 = new EAdaptorUsage(Factory, childLicence, new ZDateTime(2010, 10, 01));
			bill = new EAdaptorBill(Factory);
			bill.PopulateFromSystemUsages(new[] { usage1, usage2 });
			bill.ValidateAll(bill);
			AssertHasRowErrorContaining(bill, ": No pricelist found for " + licHeader.Company.Header.OH_Code);
			AssertNoRowErrorContaining(bill, ": No pricelist found for " + childLicence.Company.Header.OH_Code);

			var parentPriceHeader = licHeader.Company.PriceHeaders.AddNew();
			parentPriceHeader.L6_RX_NKCurrency = "AUD";
			parentPriceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			parentPriceHeader.Items.AddNew().L7_Code = BillingConstants.CoreModuleCode;
			usage1 = new EAdaptorUsage(Factory, licHeader, new ZDateTime(2010, 10, 01));
			usage2 = new EAdaptorUsage(Factory, childLicence, new ZDateTime(2010, 10, 01));
			bill = new EAdaptorBill(Factory);
			bill.PopulateFromSystemUsages(new[] { usage1, usage2 });
			bill.ValidateAll(bill);
			AssertNoRowErrors(bill);

			childPriceHeader.L6_LicenceUnitRate = 1.1234m;
			parentPriceHeader.L6_LicenceUnitRate = 5.6789m;
			usage1 = new EAdaptorUsage(Factory, licHeader, new ZDateTime(2010, 10, 01));
			usage2 = new EAdaptorUsage(Factory, childLicence, new ZDateTime(2010, 10, 01));
			bill = new EAdaptorBill(Factory);
			bill.PopulateFromSystemUsages(new[] { usage1, usage2 });
			bill.ValidateAll(bill);
			AssertHasRowErrorContaining(bill, ZString.Format(": pricelists for {0} has multiple licence unit rate i.e. {1} and {2}", childLicence.Company.Header.OH_Code, 5.6789m, 1.1234m));
		}

		public void TestValidateAll_UsageWithoutPriceHeader()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "DDD");
			BillingTestHelper.SetInvoicing(licHeader, Env.CurrentBranch.PK);
			var childPriceHeader = licHeader.Company.PriceHeaders.AddNew();
			childPriceHeader.L6_RX_NKCurrency = "AUD";
			childPriceHeader.L6_ValidFrom = new ZDateTime(2010, 5, 1);
			childPriceHeader.Items.AddNew().L7_Code = BillingConstants.CoreModuleCode;

			var usage1 = new EAdaptorUsage(Factory, licHeader, new ZDateTime(2010, 10, 1));
			var usage2 = new EAdaptorUsage(Factory, licHeader, new ZDateTime(2010, 1, 1));
			var bill = new EAdaptorBill(Factory);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1, usage2 });
			bill.ValidateAll(bill);
			AssertHasRowErrorContaining(bill, ": No currency specified for " + licHeader.Company.Header.OH_Code);
			AssertHasRowErrorContaining(bill, ": Different price currencies exist for the billing group");
			AssertHasRowErrorContaining(bill, ": No pricelist found for " + licHeader.Company.Header.OH_Code);
		}

		public void TestCalculateMinimumFeeContribution()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "DDD", "BBB", "MEL");

			var prices1 = BillingTestHelper.CreatePriceList(licence1.Company);
			prices1.L6_LicenceUnitRate = 0.5;
			var priceItem1 = BillingTestHelper.AddPriceItem(prices1, "IC1", BillingConstants.FeeType.Transactional, "EAM", 0.10m, 0.02m);
			var priceFee1 = BillingTestHelper.AddPriceItem(prices1, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 250m);

			var prices2 = BillingTestHelper.CreatePriceList(licence2.Company);
			prices2.L6_LicenceUnitRate = 0.5;
			var priceItem2 = BillingTestHelper.AddPriceItem(prices2, "IC1", BillingConstants.FeeType.Transactional, "EAM", 0.10m, 0.02m);
			var priceFee2 = BillingTestHelper.AddPriceItem(prices2, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 250m);

			Factory.Save();

			var chargeable1 = BillingTestHelper.CreateSubUsage("IC1", 1000, priceItem1);
			var usage1 = new EAdaptorUsage(Factory, licence1, new ZDateTime(2016, 5, 1));
			var feeUsage1 = new SystemUsage.SubUsage() { PriceItemCode = "EAM", PriceItem = priceFee1, RawUsageCount = 1000, UnitCount = 1, Amount = 250m };
			usage1.SetSubUsage(new[] { chargeable1 });
			usage1.AddDbUsage(new Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>(feeUsage1, new List<EAdaptorUsage>(new[] { usage1 })));
			usage1.CalculateAmount();

			var chargeable2 = BillingTestHelper.CreateSubUsage("IC1", 1000, priceItem2);
			var usage2 = new EAdaptorUsage(Factory, licence2, new ZDateTime(2016, 5, 1));
			var feeUsage2 = new SystemUsage.SubUsage() { PriceItemCode = "EAM", PriceItem = priceFee2, RawUsageCount = 1000, UnitCount = 1, Amount = 250m };
			usage2.SetSubUsage(new[] { chargeable2 });
			usage2.AddDbUsage(new Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>(feeUsage2, new List<EAdaptorUsage>(new[] { usage2 })));
			usage2.CalculateAmount();

			var chargeable3 = BillingTestHelper.CreateSubUsage("IC1", 1000, priceItem1);
			var usage3 = new EAdaptorUsage(Factory, licence1, new ZDateTime(2016, 6, 1));
			var feeUsage3 = new SystemUsage.SubUsage() { PriceItemCode = "EAM", PriceItem = priceFee1, RawUsageCount = 1000, UnitCount = 1, Amount = 250m };
			usage3.SetSubUsage(new[] { chargeable3 });
			usage3.AddDbUsage(new Tuple<SystemUsage.SubUsage, List<EAdaptorUsage>>(feeUsage3, new List<EAdaptorUsage>(new[] { usage3 })));
			usage3.CalculateAmount();

			var bill = new EAdaptorBill(Factory);
			bill.PopulateFromSystemUsages(new EAdaptorUsage[] { usage1, usage2, usage3 });

			var minimumFeeContribution = bill.CalculateMinimumFeeContribution().ToArray();
			AssertEquals(3, minimumFeeContribution.Length);
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence2.LA_LD && x.PeriodStart == new ZDateTime(2016, 5, 1)));
			Assert(minimumFeeContribution.Any(x => x.DatabasePk == licence1.LA_LD && x.PeriodStart == new ZDateTime(2016, 6, 1)));
		}

		public void TestDiscountable_IsHostedOnWiseCloud()
		{
			var odplDiscountable = new EAdaptorBill.Discountable(ZDecimal.Zero, ZDecimal.Zero, null, null, ZDateTime.Now) as IOdplDiscountable;
			AssertEquals(false, odplDiscountable.IsHostedOnWiseCloud);

			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			odplDiscountable = new EAdaptorBill.Discountable(ZDecimal.Zero, ZDecimal.Zero, null, licenceDatabase, ZDateTime.Now);
			licenceDatabase.LD_HostedLocation = "@#$";
			AssertEquals(false, licenceDatabase.IsHostedOnWiseCloud);
			AssertEquals(false, odplDiscountable.IsHostedOnWiseCloud);

			licenceDatabase.LD_HostedLocation = "SYD";
			AssertEquals(true, licenceDatabase.IsHostedOnWiseCloud);
			AssertEquals(true, odplDiscountable.IsHostedOnWiseCloud);
		}

		public void TestCreateRevenueBreakdown()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var prices = BillingTestHelper.CreatePriceList(lic1.Company);
			var priceItem1 = BillingTestHelper.AddPriceItem(prices, "IC1", BillingConstants.FeeType.Transactional, "", 0.10m, 0.02m);
			var priceFee4 = BillingTestHelper.AddPriceItem(prices, "M01", BillingConstants.FeeType.Licence, "", 1100m);

			var sub1 = BillingTestHelper.CreateSubUsage("IC1", 77, priceItem1);
			var subFee = BillingTestHelper.CreateSubUsage("M01", 3300, 1, priceFee4);

			Factory.Save();

			var chargeCodes = new CodeDescriptionPairList();
			chargeCodes.AddPair(BillingConstants.BillingSystem.eAdaptor, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			EDIDataRegistry.Instance.TransactionChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCodes);

			var chargeCodePk = BillingInvoicingHelper.GetChargeCodePK(GlbBranch.CurrentBranch, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			var discountChargeCodePk = BillingInvoicingHelper.GetChargeCodePK(GlbBranch.CurrentBranch, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value);
			var periodStart = BillingTestHelper.MonthToday;

			var usage1 = new EAdaptorUsage(Factory, lic1, periodStart, lic1.ClientCompany);
			usage1.SetSubUsage(new[] { sub1, subFee });
			usage1.CalculateAmount();

			var systemBill = new EAdaptorBill(Factory);
			systemBill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = lic1.Company.LC_OH;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranchPK;
			invoice.AH_GC = Env.CurrentCompanyPK;
			invoice.AH_ExchangeRate = 1m;

			systemBill.CreateRevenueBreakdown(invoice, -10m);
			var billedUsages = Factory.Load<EdiBilledUsage>(new ZQuery()).OrderBy(x => x.BU9_PriceCode).ToArray();
			AssertEquals(2, billedUsages.Length);
			var billedUsage1 = billedUsages[0];
			var billedUsage2 = billedUsages[1];

			CombineAssertions(() =>
			{
				AssertEquals("BU9_AC_AmountChargeCode", chargeCodePk, billedUsage1.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCodePk, billedUsage1.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_AH_Invoice", invoice.PK, billedUsage1.BU9_AH_Invoice);
				AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billedUsage1.BU9_BillingModel);
				AssertEquals("BU9_LC", lic1.LA_LC, billedUsage1.BU9_LC);
				AssertEquals("BU9_LCC", lic1.ClientCompany.PK, billedUsage1.BU9_LCC);
				AssertEquals("BU9_LD", lic1.LA_LD, billedUsage1.BU9_LD);

				AssertEquals("BU9_LocalAmountPreDiscount", 7.70m, billedUsage1.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 6.93m, billedUsage1.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_LocalProcessingAmount", -0.77m, billedUsage1.BU9_LocalProcessingAmount);

				AssertEquals("BU9_PeriodStart", periodStart, billedUsage1.BU9_PeriodStart);
				AssertEquals("BU9_PriceCurrency", "AUD", billedUsage1.BU9_PriceCurrency);

				AssertEquals("BU9_TransactionAmountPreDiscount", 7.70m, billedUsage1.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 6.93m, billedUsage1.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_TransactionProcessingAmount", -0.77m, billedUsage1.BU9_TransactionProcessingAmount);

				AssertEquals("BU9_UnitCount", 77m, billedUsage1.BU9_UnitCount);
				AssertEquals("BU9_UsageCode", "EAD", billedUsage1.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "IC1", billedUsage1.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "IC1", billedUsage1.BU9_PriceCode);
				AssertEquals("BU9_L7", priceItem1.PK, billedUsage1.BU9_L7);
			});

			CombineAssertions(() =>
			{
				AssertEquals("BU9_AC_AmountChargeCode", chargeCodePk, billedUsage2.BU9_AC_AmountChargeCode);
				AssertEquals("BU9_AC_DiscountChargeCode", discountChargeCodePk, billedUsage2.BU9_AC_DiscountChargeCode);
				AssertEquals("BU9_AH_Invoice", invoice.PK, billedUsage2.BU9_AH_Invoice);
				AssertEquals("BU9_BillingModel", BillingConstants.PriceHeaderType.ODM, billedUsage2.BU9_BillingModel);
				AssertEquals("BU9_LC", lic1.LA_LC, billedUsage2.BU9_LC);
				AssertEquals("BU9_LCC", lic1.ClientCompany.PK, billedUsage2.BU9_LCC);
				AssertEquals("BU9_LD", lic1.LA_LD, billedUsage2.BU9_LD);

				AssertEquals("BU9_LocalAmountPreDiscount", 1100m, billedUsage2.BU9_LocalAmountPreDiscount);
				AssertEquals("BU9_LocalAmountPostDiscount", 990.0m, billedUsage2.BU9_LocalAmountPostDiscount);
				AssertEquals("BU9_LocalProcessingAmount", -110.0m, billedUsage2.BU9_LocalProcessingAmount);

				AssertEquals("BU9_PeriodStart", periodStart, billedUsage2.BU9_PeriodStart);
				AssertEquals("BU9_PriceCurrency", "AUD", billedUsage2.BU9_PriceCurrency);

				AssertEquals("BU9_TransactionAmountPreDiscount", 1100m, billedUsage2.BU9_TransactionAmountPreDiscount);
				AssertEquals("BU9_TransactionAmountPostDiscount", 990.0m, billedUsage2.BU9_TransactionAmountPostDiscount);
				AssertEquals("BU9_TransactionProcessingAmount", -110.0m, billedUsage2.BU9_TransactionProcessingAmount);

				AssertEquals("BU9_UnitCount", 1m, billedUsage2.BU9_UnitCount);
				AssertEquals("BU9_UsageCode", "EAD", billedUsage2.BU9_UsageCode);
				AssertEquals("BU9_UsageSubCode", "M01", billedUsage2.BU9_UsageSubCode);
				AssertEquals("BU9_PriceCode", "M01", billedUsage2.BU9_PriceCode);
				AssertEquals("BU9_L7", priceFee4.PK, billedUsage2.BU9_L7);
			});
		}

		protected override EAdaptorBill GetNewSystemBill()
		{
			return new EAdaptorBill(Factory);
		}
	}
}
