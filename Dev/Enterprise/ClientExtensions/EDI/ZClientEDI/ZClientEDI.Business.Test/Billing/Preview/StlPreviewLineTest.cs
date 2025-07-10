using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlPreviewLine))]
	public class StlPreviewLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StlPreviewLine(null, null, ZDateTime.Empty, null);
		}

		public void TestGenerateStlReport()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var lastPeriodStart = periodStart.AddMonths(-1);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var stdHeader = BillingTestHelper.CreateLicence(Factory, "STD");
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdHeader.LicenceCode);

			var priceHeader = BillingTestHelper.CreateStlPriceList(stdHeader.Company);
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = lastPeriodStart.AddYears(-5);
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
			item1.L7_Price = 30;
			item1.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			item1.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			item1.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			item1.L7_PGM_DiscountGroupCode = "Prepay Only";

			var priceGPC = priceHeader.Items.AddNew();
			priceGPC.L7_Category = BillingConstants.BillingSystem.ODM;
			priceGPC.L7_Code = "GPC";
			priceGPC.L7_Price = 10;
			priceGPC.L7_UnitBreak = 1;
			priceGPC.L7_ChargeCode = EDIDataRegistry.Instance.OdplUsageChargeCode.Value;
			priceGPC.L7_DiscountChargeCode = EDIDataRegistry.Instance.OdplDiscountChargeCode.Value;
			priceGPC.L7_DepositChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			priceGPC.L7_PGM_DiscountGroupCode = "Prepay Only";
			priceGPC.L7_FeeType = BillingConstants.FeeType.UsersPerCountryVolumeBreak;

			var item1Aud = item1.CurrencyRates.AddNew();
			item1Aud.PIR_Price = 40;
			item1Aud.PIR_RX_NKCurrency = "AUD";

			var priceGpcAud = priceGPC.CurrencyRates.AddNew();
			priceGpcAud.PIR_Price = 15;
			priceGpcAud.PIR_RX_NKCurrency = "AUD";

			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdHeader.Company);
			var bwPriceItem1 = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode);
			bwPriceItem1.L7_Price = 199;
			bwPriceItem1.L7_RX_NKCurrency = "AUD";
			bwPriceItem1.L7_PGM_DiscountGroupCode = "BorderWise";

			var bwDiscount = priceHeader.StlItemDiscounts.AddNew();
			bwDiscount.PGM_GroupCode = "BorderWise";
			bwDiscount.PGM_PHD = discount.PK;

			var bwLic = BillingTestHelper.CreateBorderWiseLicence(Factory, "ENT", "COM", "BOR", LicenceAdvStdOthList.Codes.OnDemand);
			var lic = BillingTestHelper.CreateAnotherDatabase(bwLic, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart, lic.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart.AddMonths(-1), lic.ClientCompany, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart.AddMonths(-2), lic.ClientCompany, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "GPC", lastPeriodStart.AddMonths(-2), lic.ClientCompany, 2);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, lastPeriodStart, bwLic.LA_LC, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, lastPeriodStart.AddMonths(-1), bwLic.LA_LC, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, lastPeriodStart.AddMonths(-2), bwLic.LA_LC, 1);

			var fee1 = BillingTestHelper.CreateLicenceFee(lic.Company, "ESV", "some fee", 500, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, lastPeriodStart, ZDateTime.Empty, "AUD");
			var fee2TooFuture = BillingTestHelper.CreateLicenceFee(lic.Company, "ESV", "some other fee", 700, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty, "AUD");

			Factory.Save();

			var stlDiscounts = new StlDiscountPreviewCollection();
			foreach (var headerDiscount in priceHeader.StlDiscounts)
			{
				var discountPreview = stlDiscounts.AddNew();
				discountPreview.Name = headerDiscount.PHD_Name;
				discountPreview.IsActive = headerDiscount.PHD_IsDefaultEnabled;
				discountPreview.Percent = headerDiscount.PHD_Percent;
			}

			var context = new StlPreviewContext(ZDateTime.Today, priceHeader.PK, ZGuid.Empty, "AUD", "", stlDiscounts.ToArray<StlDiscountPreview>());

			var dbOwner = new StlPreviewLicenceDatabaseOwner(lic);
			var previewLine1 = new StlPreviewLine(Factory, dbOwner, lastPeriodStart, context);
			var previewLine2 = new StlPreviewLine(Factory, dbOwner, lastPeriodStart.AddMonths(-1), context);
			var previewLine3 = new StlPreviewLine(Factory, dbOwner, lastPeriodStart.AddMonths(-2), context);

			CombineAssertions(() =>
			{
				AssertEquals(15 * 40 * 0.9m + 5 * 199 * 0.9m + 500m, previewLine1.StlPostDiscount);
				AssertEquals(13 * 40 * 0.9m + 3 * 199 * 0.9m, previewLine2.StlPostDiscount);
				AssertEquals(11 * 40 * 0.9m + 1 * 199 * 0.9m + 2 * 15 * 0.9m, previewLine3.StlPostDiscount);

				AssertNoRowErrors(previewLine1);
				AssertNoRowErrors(previewLine2);
				AssertNoRowErrors(previewLine3);
			});

			var bill1 = previewLine1.GetStlBillForSummary(Factory);
			var bill2 = previewLine2.GetStlBillForSummary(Factory);
			var bill3 = previewLine3.GetStlBillForSummary(Factory);

			CombineAssertions(() =>
			{
				AssertEquals(15 * 40 * 0.9m + 5 * 199 * 0.9m + 500m, bill1.InvoicePostDiscountTotal);
				AssertEquals(13 * 40 * 0.9m + 3 * 199 * 0.9m, bill2.InvoicePostDiscountTotal);
				AssertEquals(11 * 40 * 0.9m + 1 * 199 * 0.9m + 2 * 15 * 0.9m, bill3.InvoicePostDiscountTotal);

				AssertNoRowErrors(bill1);
				AssertNoRowErrors(bill2);
				AssertNoRowErrors(bill3);
			});
		}

		public void TestGenerateOdplReport()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var lastPeriodStart = periodStart.AddMonths(-1);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var stdHeader = BillingTestHelper.CreateLicence(Factory, "STD");
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdHeader.LicenceCode);

			var previewPriceHeader = stdHeader.Company.PriceHeaders.AddNew();
			previewPriceHeader.L6_RX_NKCurrency = "AUD";
			previewPriceHeader.L6_ValidFrom = lastPeriodStart.AddYears(-5);
			previewPriceHeader.L6_UseStandardDiscount = false;
			previewPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			previewPriceHeader.L6_DiscountCode = "CW1";

			var previewCorPrice = previewPriceHeader.Items.AddNew();
			previewCorPrice.L7_Code = "COR";
			previewCorPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			previewCorPrice.L7_Price = 40.00;
			previewCorPrice.L7_Description = "ediCore";

			var previewGpcPrice = previewPriceHeader.Items.AddNew();
			previewGpcPrice.L7_Code = "GPC";
			previewGpcPrice.L7_FeeType = BillingConstants.FeeType.UsersPerCountryVolumeBreak;
			previewGpcPrice.L7_UnitBreak = 1;
			previewGpcPrice.L7_Price = 15;
			previewGpcPrice.L7_Description = "GPC15";

			var previewDataStoragePrice = previewPriceHeader.Items.AddNew();
			previewDataStoragePrice.L7_Code = BillingConstants.Hosting.DataStorageCode;
			previewDataStoragePrice.L7_FeeType = BillingConstants.FeeType.Per10GBPerMonthMin1GB;
			previewDataStoragePrice.L7_Price = 10.00;
			previewDataStoragePrice.L7_Description = "Storage";

			var previewPrinterPrice = previewPriceHeader.Items.AddNew();
			previewPrinterPrice.L7_Code = BillingConstants.Hosting.PrintServersCode;
			previewPrinterPrice.L7_FeeType = BillingConstants.FeeType.PerDevicePerMonth;
			previewPrinterPrice.L7_Price = 23.00;
			previewPrinterPrice.L7_Description = "Printers";

			var previewDiscount = stdHeader.Company.SelfBilling.BillingDiscounts.AddNew();
			previewDiscount.L5_DiscountCode = "CW1";
			previewDiscount.L5_StartDate = previewPriceHeader.L6_ValidFrom;
			previewDiscount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			previewDiscount.L5_Discount = 10m;
			previewDiscount.L5_Type = BillingConstants.DiscountType.Special;

			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdHeader.Company);
			var bwPriceItem1 = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode);
			bwPriceItem1.L7_Price = 199;
			bwPriceItem1.L7_RX_NKCurrency = "AUD";
			bwPriceItem1.L7_PGM_DiscountGroupCode = "BorderWise";

			var bwLic = BillingTestHelper.CreateBorderWiseLicence(Factory, "ENT", "COM", "BOR", LicenceAdvStdOthList.Codes.OnDemand);
			var lic = BillingTestHelper.CreateAnotherDatabase(bwLic, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			lic.CoreModule.LM_UserCount = 5;
			var oldOnDemandDiscount = lic.Company.SelfBilling.BillingDiscounts.AddNew();
			oldOnDemandDiscount.L5_StartDate = lastPeriodStart.AddYears(-5);
			oldOnDemandDiscount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			oldOnDemandDiscount.L5_Discount = 25m;
			oldOnDemandDiscount.L5_Type = BillingConstants.DiscountType.Special;

			var oldPriceHeader = lic.Company.PriceHeaders.AddNew();
			oldPriceHeader.L6_RX_NKCurrency = "AUD";
			oldPriceHeader.L6_ValidFrom = lastPeriodStart.AddYears(-5);
			oldPriceHeader.L6_UseStandardDiscount = false;
			oldPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;

			var oldCorPrice = oldPriceHeader.Items.AddNew();
			oldCorPrice.L7_Code = "COR";
			oldCorPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			oldCorPrice.L7_Price = 99.00;
			oldCorPrice.L7_Description = "ediCore";

			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", lastPeriodStart, lic.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", lastPeriodStart.AddMonths(-1), lic.ClientCompany, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", lastPeriodStart.AddMonths(-2), lic.ClientCompany, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "GPC", lastPeriodStart.AddMonths(-2), lic.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodStart.AddMonths(-2), lic.ClientCompany, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, lastPeriodStart, bwLic.LA_LC, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, lastPeriodStart.AddMonths(-1), bwLic.LA_LC, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, lastPeriodStart.AddMonths(-2), bwLic.LA_LC, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, lastPeriodStart, lic.ClientCompany, 5000);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingRemoteDevices, BillingConstants.Hosting.PrintServersCode, lastPeriodStart, lic.ClientCompany, 3);

			var fee1 = BillingTestHelper.CreateLicenceFee(lic.Company, "ESV", "some fee", 500, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, lastPeriodStart, ZDateTime.Empty, "AUD");
			var fee2TooFuture = BillingTestHelper.CreateLicenceFee(lic.Company, "ESV", "some other fee", 700, EDIDataRegistry.Instance.OdplUsageChargeCode.Value, periodStart, ZDateTime.Empty, "AUD");

			Factory.Save();

			var context = new StlPreviewContext(ZDateTime.Today, ZGuid.Empty, previewPriceHeader.PK, "AUD", "", Array.Empty<StlDiscountPreview>());

			var dbOwner = new StlPreviewLicenceDatabaseOwner(lic);
			var previewLine1 = new StlPreviewLine(Factory, dbOwner, lastPeriodStart, context);
			var previewLine2 = new StlPreviewLine(Factory, dbOwner, lastPeriodStart.AddMonths(-1), context);
			var previewLine3 = new StlPreviewLine(Factory, dbOwner, lastPeriodStart.AddMonths(-2), context);

			CombineAssertions(() =>
			{
				AssertEquals(15 * 40 * 0.9m + 1 * 10m + 3 * 23m + 5 * 199m * 0.9m + 500m, previewLine1.OdplTotalDue);
				AssertEquals(13 * 40 * 0.9m + 3 * 199m * 0.9m, previewLine2.OdplTotalDue);
				AssertEquals(11 * 40 * 0.9m + 1 * 15 * 0.9m + 1 * 199m * 0.9m, previewLine3.OdplTotalDue);

				AssertNoRowErrors(previewLine1);
				AssertNoRowErrors(previewLine2);
				AssertNoRowErrors(previewLine3);
			});

			var bill1 = previewLine1.GetOrganisationBillForSummary(Factory);
			var bill2 = previewLine2.GetOrganisationBillForSummary(Factory);
			var bill3 = previewLine3.GetOrganisationBillForSummary(Factory);

			CombineAssertions(() =>
			{
				AssertEquals(15 * 40 * 0.9m + 1 * 10m + 3 * 23m + 5 * 199m * 0.9m + 500m, bill1.TotalAmount);
				AssertEquals(13 * 40 * 0.9m + 3 * 199m * 0.9m, bill2.TotalAmount);
				AssertEquals(11 * 40 * 0.9m + 1 * 15 * 0.9m + 1 * 199m * 0.9m, bill3.TotalAmount);

				AssertNoRowErrors(bill1);
				AssertNoRowErrors(bill2);
				AssertNoRowErrors(bill3);
			});
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

	[TestedType(typeof(StlPreviewLineCollection))]
	public class StlPreviewLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StlPreviewLineCollection>
	{
		protected override StlPreviewLineCollection GetCollectionToTest()
		{
			return new StlPreviewLineCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlPreviewLine(null, null, ZDateTime.Empty, null);
		}
	}
}
