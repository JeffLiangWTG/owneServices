using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlPreview))]
	public class StlPreviewTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaults()
		{
			var stdHeader = BillingTestHelper.CreateLicence(Factory, "STD");
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdHeader.LicenceCode);
			Factory.Save();

			var bizo = new StlPreview(new BusinessObjectFactory() { RefreshEnabled = false });
			bizo.SetDefaults();

			AssertEquals("", bizo.StlPriceHeaderVersion);
			AssertNull(bizo.StlPriceHeader);

			var priceHeader1 = BillingTestHelper.CreateStlPriceList(stdHeader.Company);
			priceHeader1.L6_SystemCreateTimeUtc = new ZDateTime(2016, 5, 10);
			priceHeader1.L6_PricelistVersion = "STL v6.0";

			var priceHeader2 = BillingTestHelper.CreateStlPriceList(stdHeader.Company);
			priceHeader2.L6_SystemCreateTimeUtc = new ZDateTime(2016, 5, 11);
			priceHeader2.L6_PricelistVersion = "STL v5.0";

			Factory.Save();

			bizo = new StlPreview(new BusinessObjectFactory() { RefreshEnabled = false });
			bizo.SetDefaults();
			AssertEquals(priceHeader2.PK, bizo.StlPriceHeader.PK);
			AssertEquals("STL v5.0", bizo.StlPriceHeaderVersion);

			AssertEquals(1, bizo.PriceHeaderLinks.Count);
			AssertEquals(false, bizo.PriceHeaderLinks.AllowNew);
			AssertEquals(false, bizo.PriceHeaderLinks.AllowRemove);
			AssertEquals(ZGuid.Empty, bizo.PriceHeaderLinks[0].PHL_LD);
		}

		[TestDate(2016, 5, 1)]
		public void TestGenerateStlReport()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var stdHeader = BillingTestHelper.CreateLicence(Factory, "STD");
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdHeader.LicenceCode);

			var lastPeriodStart = BillingTestHelper.MonthToday.AddMonths(-1);

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

			var item1Aud = item1.CurrencyRates.AddNew();
			item1Aud.PIR_Price = 40;
			item1Aud.PIR_RX_NKCurrency = "AUD";

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart, lic.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart.AddMonths(-1), lic.ClientCompany, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart.AddMonths(-2), lic.ClientCompany, 11);

			Factory.Save();

			var bizo = new StlPreview(Factory);
			bizo.OrganisationPK = lic.Company.LC_OH;
			bizo.CurrencyCode = "AUD";
			bizo.StlPriceHeaderPk = priceHeader.PK;

			bizo.GenerateReport(null);
			var previewLines = bizo.StlPreviewLines;
			AssertEquals(3, previewLines.Count);

			var previewLine1 = previewLines[0];
			var previewLine2 = previewLines[1];
			var previewLine3 = previewLines[2];

			CombineAssertions(() =>
			{
				AssertEquals(15 * 40 * 0.9m, previewLine1.StlPostDiscount);
				AssertEquals(13 * 40 * 0.9m, previewLine2.StlPostDiscount);
				AssertEquals(11 * 40 * 0.9m, previewLine3.StlPostDiscount);

				AssertNoRowErrors(previewLine1);
				AssertNoRowErrors(previewLine2);
				AssertNoRowErrors(previewLine3);

				var bill1 = previewLine1.GetStlBillForSummary(Factory);
				var bill2 = previewLine2.GetStlBillForSummary(Factory);
				var bill3 = previewLine3.GetStlBillForSummary(Factory);

				AssertEquals(15 * 40 * 0.9m, bill1.InvoicePostDiscountTotal);
				AssertEquals(13 * 40 * 0.9m, bill2.InvoicePostDiscountTotal);
				AssertEquals(11 * 40 * 0.9m, bill3.InvoicePostDiscountTotal);

				AssertNoRowErrors(bill1);
				AssertNoRowErrors(bill2);
				AssertNoRowErrors(bill3);
			});

			bizo.OrganisationPK = lic.Company.LC_OH;
			bizo.CurrencyCode = "AUD";
			bizo.EnterprisePK = ZGuid.Empty;
			bizo.DatabaseCode = "";

			bizo.GenerateReport(null);
			previewLines = bizo.StlPreviewLines;
			AssertEquals(3, previewLines.Count);

			previewLine1 = previewLines[0];
			previewLine2 = previewLines[1];
			previewLine3 = previewLines[2];

			CombineAssertions(() =>
			{
				AssertEquals(15 * 40 * 0.9m, previewLine1.StlPostDiscount);
				AssertEquals(13 * 40 * 0.9m, previewLine2.StlPostDiscount);
				AssertEquals(11 * 40 * 0.9m, previewLine3.StlPostDiscount);

				AssertNoRowErrors(previewLine1);
				AssertNoRowErrors(previewLine2);
				AssertNoRowErrors(previewLine3);

				var bill1 = previewLine1.GetStlBillForSummary(Factory);
				var bill2 = previewLine2.GetStlBillForSummary(Factory);
				var bill3 = previewLine3.GetStlBillForSummary(Factory);

				AssertEquals(15 * 40 * 0.9m, bill1.InvoicePostDiscountTotal);
				AssertEquals(13 * 40 * 0.9m, bill2.InvoicePostDiscountTotal);
				AssertEquals(11 * 40 * 0.9m, bill3.InvoicePostDiscountTotal);

				AssertNoRowErrors(bill1);
				AssertNoRowErrors(bill2);
				AssertNoRowErrors(bill3);
			});
		}

		[TestDate(2016, 5, 1)]
		public void TestGenerateOdplReport()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var stdHeader = BillingTestHelper.CreateLicence(Factory, "STD");
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdHeader.LicenceCode);

			var lastPeriodStart = BillingTestHelper.MonthToday.AddMonths(-1);

			var previewPriceHeader = stdHeader.Company.PriceHeaders.AddNew();
			previewPriceHeader.L6_RX_NKCurrency = "AUD";
			previewPriceHeader.L6_ValidFrom = lastPeriodStart.AddYears(-5);
			previewPriceHeader.L6_UseStandardDiscount = false;
			previewPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			previewPriceHeader.L6_DiscountCode = "CW1";

			var previewCorPrice = previewPriceHeader.Items.AddNew();
			previewCorPrice.L7_Category = BillingConstants.BillingSystem.ODM;
			previewCorPrice.L7_Code = "COR";
			previewCorPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			previewCorPrice.L7_Price = 40.00;
			previewCorPrice.L7_Description = "ediCore";

			var previewDataStoragePrice = previewPriceHeader.Items.AddNew();
			previewDataStoragePrice.L7_Category = BillingConstants.BillingSystem.HostingStorage;
			previewDataStoragePrice.L7_Code = BillingConstants.Hosting.DataStorageCode;
			previewDataStoragePrice.L7_FeeType = BillingConstants.FeeType.Per10GBPerMonthMin1GB;
			previewDataStoragePrice.L7_Price = 10.00;
			previewDataStoragePrice.L7_Description = "Storage";

			var previewPrinterPrice = previewPriceHeader.Items.AddNew();
			previewPrinterPrice.L7_Category = BillingConstants.BillingSystem.HostingRemoteDevices;
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

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK, "AUD");
			lic.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
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
			oldCorPrice.L7_Category = BillingConstants.BillingSystem.ODM;
			oldCorPrice.L7_Code = "COR";
			oldCorPrice.L7_FeeType = BillingConstants.FeeType.NamedUser;
			oldCorPrice.L7_Price = 99.00;
			oldCorPrice.L7_Description = "ediCore";

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", lastPeriodStart, lic.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", lastPeriodStart.AddMonths(-1), lic.ClientCompany, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "COR", lastPeriodStart.AddMonths(-2), lic.ClientCompany, 11);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, lastPeriodStart, lic.ClientCompany, 5000);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingRemoteDevices, BillingConstants.Hosting.PrintServersCode, lastPeriodStart, lic.ClientCompany, 3);

			Factory.Save();

			var bizo = new StlPreview(Factory);
			bizo.OrganisationPK = lic.Company.LC_OH;
			bizo.CurrencyCode = "AUD";
			bizo.OdplPriceHeaderPk = previewPriceHeader.PK;

			bizo.GenerateReport(null);
			var previewLines = bizo.StlPreviewLines;
			AssertEquals(3, previewLines.Count);

			var previewLine1 = previewLines[0];
			var previewLine2 = previewLines[1];
			var previewLine3 = previewLines[2];

			CombineAssertions(() =>
			{
				AssertEquals(15 * 40 * 0.9m + 1 * 10m + 3 * 23m, previewLine1.OdplTotalDue);
				AssertEquals(13 * 40 * 0.9m, previewLine2.OdplTotalDue);
				AssertEquals(11 * 40 * 0.9m, previewLine3.OdplTotalDue);

				AssertNoRowErrors(previewLine1);
				AssertNoRowErrors(previewLine2);
				AssertNoRowErrors(previewLine3);

				var bill1 = previewLine1.GetOrganisationBillForSummary(Factory);
				var bill2 = previewLine2.GetOrganisationBillForSummary(Factory);
				var bill3 = previewLine3.GetOrganisationBillForSummary(Factory);

				AssertEquals(15 * 40 * 0.9m + 1 * 10m + 3 * 23m, bill1.TotalAmount);
				AssertEquals(13 * 40 * 0.9m, bill2.TotalAmount);
				AssertEquals(11 * 40 * 0.9m, bill3.TotalAmount);

				AssertNoRowErrors(bill1);
				AssertNoRowErrors(bill2);
				AssertNoRowErrors(bill3);
			});

			bizo.OrganisationPK = ZGuid.Empty;
			bizo.EnterprisePK = ZGuid.Empty;
			bizo.DatabaseCode = "";
			bizo.GenerateReport(null);
			previewLines = bizo.StlPreviewLines;
			AssertEquals(3, previewLines.Count);

			previewLine1 = previewLines[0];
			previewLine2 = previewLines[1];
			previewLine3 = previewLines[2];

			CombineAssertions(() =>
			{
				AssertEquals(15 * 40 * 0.9m + 1 * 10m + 3 * 23m, previewLine1.OdplTotalDue);
				AssertEquals(13 * 40 * 0.9m, previewLine2.OdplTotalDue);
				AssertEquals(11 * 40 * 0.9m, previewLine3.OdplTotalDue);

				AssertNoRowErrors(previewLine1);
				AssertNoRowErrors(previewLine2);
				AssertNoRowErrors(previewLine3);

				var bill1 = previewLine1.GetOrganisationBillForSummary(Factory);
				var bill2 = previewLine2.GetOrganisationBillForSummary(Factory);
				var bill3 = previewLine3.GetOrganisationBillForSummary(Factory);

				AssertEquals(15 * 40 * 0.9m + 1 * 10m + 3 * 23m, bill1.TotalAmount);
				AssertEquals(13 * 40 * 0.9m, bill2.TotalAmount);
				AssertEquals(11 * 40 * 0.9m, bill3.TotalAmount);

				AssertNoRowErrors(bill1);
				AssertNoRowErrors(bill2);
				AssertNoRowErrors(bill3);
			});
		}

		[TestDate(2016, 5, 1)]
		public void TestGenerateStlReport_Uplift()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var stdHeader = BillingTestHelper.CreateLicence(Factory, "STD");
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdHeader.LicenceCode);

			var lastPeriodStart = BillingTestHelper.MonthToday.AddMonths(-1);

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

			var discount1 = priceHeader.StlDiscounts.AddNew();
			discount1.PHD_Type = "VOL";
			var group1 = priceHeader.StlItemDiscounts.AddNew();
			group1.PGM_GroupCode = "G1";
			group1.PGM_PHD = discount1.PK;
			item1.L7_PGM_DiscountGroupCode = "G1";

			priceHeader.L6_HasExchangeRates = true;
			priceHeader.L6_Rounding = "V1";
			var exRate = priceHeader.ExchangeRates.AddNew();
			exRate.PHE_RX_NKCurrency = "AUD";
			exRate.PHE_Rate = 2m;
			exRate.PHE_UpliftPercent = 5m;

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart, lic.ClientCompany, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart.AddMonths(-1), lic.ClientCompany, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.STL, "USR", lastPeriodStart.AddMonths(-2), lic.ClientCompany, 11);

			Factory.Save();

			var bizo = new StlPreview(Factory);
			bizo.OrganisationPK = lic.Company.LC_OH;
			bizo.CurrencyCode = "AUD";
			bizo.StlPriceHeaderPk = priceHeader.PK;
			bizo.PriceHeaderLinks[0].PHL_VolumeCode = "LV";
			bizo.PriceHeaderLinks[0].PHL_VolumePercent = 110m;
			bizo.PriceHeaderLinks[0].PHL_CorePackCode = "UP";
			bizo.PriceHeaderLinks[0].PHL_CoreUpliftPercent = 10m;

			bizo.GenerateReport(null);
			var previewLines = bizo.StlPreviewLines;
			AssertEquals(3, previewLines.Count);

			var previewLine1 = previewLines[0];
			var previewLine2 = previewLines[1];
			var previewLine3 = previewLines[2];

			CombineAssertions(() =>
			{
				AssertEquals(1125m, previewLine1.StlPostDiscount);
				AssertEquals(975m, previewLine2.StlPostDiscount);
				AssertEquals(825m, previewLine3.StlPostDiscount);

				AssertNoRowErrors(previewLine1);
				AssertNoRowErrors(previewLine2);
				AssertNoRowErrors(previewLine3);

				var bill1 = previewLine1.GetStlBillForSummary(Factory);
				var bill2 = previewLine2.GetStlBillForSummary(Factory);
				var bill3 = previewLine3.GetStlBillForSummary(Factory);

				AssertEquals(1125m, bill1.InvoicePostDiscountTotal);
				AssertEquals(975m, bill2.InvoicePostDiscountTotal);
				AssertEquals(825m, bill3.InvoicePostDiscountTotal);

				AssertNoRowErrors(bill1);
				AssertNoRowErrors(bill2);
				AssertNoRowErrors(bill3);
			});
		}

		public void TestDatabaseCodeDescriptionPairList()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			var database1 = org.LicCompany.LicDatabases.AddNew();
			database1.FillWithValidTestData();
			database1.LD_Product = ProductTypes.Codes.Enterprise;
			database1.LD_ServerCode = "001";
			var database2 = org.LicCompany.LicDatabases.AddNew();
			database2.FillWithValidTestData();
			database2.LD_Product = ProductTypes.Codes.CargoWiseOne;
			database2.LD_ServerCode = "002";
			var database3 = org.LicCompany.LicDatabases.AddNew();
			database3.FillWithValidTestData();
			database3.LD_Product = ProductTypes.Codes.ProductivityWise;
			database3.LD_ServerCode = "003";
			var database4 = org.LicCompany.LicDatabases.AddNew();
			database4.FillWithValidTestData();
			database4.LD_Product = ProductTypes.Codes.GLOW;
			database4.LD_ServerCode = "004";
			var database5 = org.LicCompany.LicDatabases.AddNew();
			database5.FillWithValidTestData();
			database5.LD_Product = ProductTypes.Codes.CargoWiseNext;
			database5.LD_ServerCode = "005";
			var database6 = org.LicCompany.LicDatabases.AddNew();
			database6.FillWithValidTestData();
			database6.LD_Product = ProductTypes.Codes.CargoWise;
			database6.LD_ServerCode = "006";

			Factory.Save();
			var bizo = new StlPreview(Factory);
			bizo.OrganisationPK = org.PK;

			var list = bizo.DatabaseCodeDescriptionPairList;
			AssertEquals(5, list.Count);
			AssertEquals(true, list.ContainsCode("001"));
			AssertEquals(true, list.ContainsCode("002"));
			AssertEquals(true, list.ContainsCode("003"));
			AssertEquals(true, list.ContainsCode("005"));
			AssertEquals(true, list.ContainsCode("006"));
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

	[TestedType(typeof(EdiPriceHeaderLinkCollectionForPreview))]
	public class EdiPriceHeaderLinkCollectionForPreviewTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new EdiPriceHeaderLinkCollectionForPreview(Factory);
		}
	}
}
