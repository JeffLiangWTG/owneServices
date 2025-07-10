using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	[TestedType(typeof(HostingBill))]
	internal class HostingBillTest : SystemBillTestCase<HostingBill>
	{
		public void TestCreateInvoiceLines()
		{
			BillingTestHelper.CreateChargeCode(Factory, null, EDIDataRegistry.Instance.HostingDataStorageChargeCode.Value);
			BillingTestHelper.CreateChargeCode(Factory, null, EDIDataRegistry.Instance.HostingDocsStorageChargeCode.Value);
			BillingTestHelper.CreateChargeCode(Factory, null, EDIDataRegistry.Instance.HostingRemoteDevicesChargeCode.Value);
			BillingTestHelper.CreateChargeCode(Factory, null, EDIDataRegistry.Instance.HostingPrintServersChargeCode.Value);
			BillingTestHelper.CreateChargeCode(Factory, null, EDIDataRegistry.Instance.HostingNonProductionStorageChargeCode.Value);
			Factory.Save();

			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var licTest = BillingTestHelper.CreateAnotherDatabase(lic, "TST", false);
			licTest.Database.LD_LicenceType = DatabaseTypes.Codes.Test;
			licTest.Database.LD_LD_ParentDatabase = lic.LA_LD;
			var user = new UsingParty(lic);
			var userTest = new UsingParty(licTest);
			EDIOrgHeader org = lic.Company.Header;
			BillingTestHelper.SetInvoicing(org, Env.CurrentBranch.PK, Env.CurrentCompany.LocalCurrency.Code);
			var prices = BillingTestHelper.CreatePriceList(lic);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 26m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.eDocsStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 14m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.NonProductionStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 9m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.RemoteDevicesCode, BillingConstants.FeeType.PerDevicePerMonth, "", 9m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.PrintServersCode, BillingConstants.FeeType.PerDevicePerMonth, "", 20m);
			var discount = AddDiscountForMonth(org, 25m, new ZDateTime(2011, 2, 1));

			HostingBill billStorage = new HostingBill(Factory);
			List<SystemBill.BillLine> lines = new List<SystemBill.BillLine>();
			billStorage.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals(0, lines.Count);

			HostingUsage usage1 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 15000);
			HostingUsage usage2 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.eDocsStorageCode, 30000);
			HostingUsage usageTest1 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.NonProductionStorageCode, 20000);

			billStorage.PopulateFromSystemUsages(new SystemUsage[] { usage1, usage2, usageTest1 });

			billStorage.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals(4, lines.Count);
			AssertAmountLine(lines[0], 2 * 26m, EDIDataRegistry.Instance.HostingDataStorageChargeCode.Value, usage1.GetInvoiceLineDescription());
			AssertAmountLine(lines[1], 3 * 14m, EDIDataRegistry.Instance.HostingDocsStorageChargeCode.Value, usage2.GetInvoiceLineDescription());
			AssertAmountLine(lines[2], 2 * 9m, EDIDataRegistry.Instance.HostingNonProductionStorageChargeCode.Value, usageTest1.GetInvoiceLineDescription());
			ZDecimal expectedAmount = 2 * 26m + 3 * 14m + 2 * 9m;
			AssertEquals("WiseCloud Storage Discount", lines[3].Description);
			AssertEquals(-expectedAmount * 0.25m, lines[3].Amount);

			var billRemoteDevices = new HostingBill(Factory);
			HostingUsage usage3 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingRemoteDevices, BillingConstants.Hosting.RemoteDevicesCode, 5);
			HostingUsage usage4 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingRemoteDevices, BillingConstants.Hosting.PrintServersCode, 3);
			billRemoteDevices.SystemUsages.Add(usage3);
			billRemoteDevices.SystemUsages.Add(usage4);
			lines.Clear();
			billRemoteDevices.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals(2, lines.Count);
			AssertAmountLine(lines[0], 5 * 9m, EDIDataRegistry.Instance.HostingRemoteDevicesChargeCode.Value, usage3.GetInvoiceLineDescription());
			AssertAmountLine(lines[1], 3 * 20m, EDIDataRegistry.Instance.HostingPrintServersChargeCode.Value, usage4.GetInvoiceLineDescription());

			discount.L5_Type = BillingConstants.DiscountType.Surcharge;
			discount.L5_Discount = -20m;

			expectedAmount = 2 * 26m + 3 * 14m;
			billStorage = new HostingBill(Factory);
			billStorage.PopulateFromSystemUsages(new SystemUsage[] { usage1, usage2 });
			AssertEquals("Surcharge", expectedAmount * 0.2m, billStorage.SurchargeAmount);

			lines = new List<SystemBill.BillLine>();
			billStorage.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals(EDIDataRegistry.Instance.HostingDiscountChargeCode.Value, lines[2].ChargeCodeName);
			AssertEquals(expectedAmount * 0.2m, lines[2].Amount);
			AssertEquals("WiseCloud Storage Surcharge", lines[2].Description);
		}

		public void TestCalculateGroupAmounts_WithDiscounts()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var user = new UsingParty(lic);
			EDIOrgHeader org = lic.Company.Header;
			BillingTestHelper.SetInvoicing(org, Env.CurrentBranch.PK, Env.CurrentCompany.LocalCurrency.Code);
			var prices = BillingTestHelper.CreatePriceList(lic);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.DataStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 26m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.eDocsStorageCode, BillingConstants.FeeType.Per10GBPerMonthMin1GB, "", 14m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.RemoteDevicesCode, BillingConstants.FeeType.PerDevicePerMonth, "", 9m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.PrintServersCode, BillingConstants.FeeType.PerDevicePerMonth, "", 20m);
			AddDiscountForMonth(org, 25m, new ZDateTime(2011, 2, 1));
			AddSurcharge(org, -10m);

			HostingBill billStorage = new HostingBill(Factory);
			HostingUsage usage1 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 15000);
			HostingUsage usage2 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.eDocsStorageCode, 30000);
			billStorage.PopulateFromSystemUsages(new SystemUsage[] { usage1, usage2 });

			ZDecimal expectedAmount = 2 * 26m + 3 * 14m;
			AssertEquals("Amount", expectedAmount, billStorage.Amount);
			AssertEquals("Discount", expectedAmount * 0.25m, billStorage.DiscountAmount);
			AssertEquals("Surcharge", expectedAmount * 0.10m, billStorage.SurchargeAmount);

			List<SystemBill.BillLine> lines = new List<SystemBill.BillLine>();
			billStorage.CreateInvoiceLines(lines, ZDateTime.Now);
			AssertEquals(EDIDataRegistry.Instance.HostingDataStorageChargeCode.Value, lines[0].ChargeCodeName);
			AssertEquals(2 * 26m, lines[0].Amount);

			AssertEquals(EDIDataRegistry.Instance.HostingDocsStorageChargeCode.Value, lines[1].ChargeCodeName);
			AssertEquals(3 * 14m, lines[1].Amount);

			AssertEquals(EDIDataRegistry.Instance.HostingDiscountChargeCode.Value, lines[2].ChargeCodeName);
			AssertEquals(-expectedAmount * 0.25m, lines[2].Amount);

			AssertEquals(EDIDataRegistry.Instance.HostingDiscountChargeCode.Value, lines[3].ChargeCodeName);
			AssertEquals(expectedAmount * 0.10m, lines[3].Amount);

			AssertEquals(4, lines.Count);
		}

		void AssertAmountLine(SystemBill.BillLine invoiceLine, ZDecimal amount, ZString amountChargeCodeName, ZString description)
		{
			AssertEquals(amount, invoiceLine.Amount);
			AssertEquals(amountChargeCodeName, invoiceLine.ChargeCodeName);
			AssertEquals(description, invoiceLine.Description);
		}

		public void TestValidateAll()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var user = new UsingParty(lic);
			EDIOrgHeader org = lic.Company.Header;
			BillingTestHelper.SetInvoicing(org, Env.CurrentBranch.PK, Env.CurrentCompany.LocalCurrency.Code);
			var prices = BillingTestHelper.CreatePriceList(lic);

			HostingBill bill = new HostingBill(Factory);
			HostingUsage usage1 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 15000);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });
			bill.ValidateAll(bill);
			AssertHasRowError(bill, "WiseCloud Storage: No price found for org " + org.OH_Code + " for usage code " + BillingConstants.Hosting.DataStorageCode);

			var priceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.WiseCloudUserFeeCode, BillingConstants.FeeType.NamedUser, "", 26m);
			bill = new HostingBill(Factory);
			usage1 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.WiseCloudUser, BillingConstants.Hosting.WiseCloudUserFeeCode, 50);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });
			bill.ValidateAll(bill);
			AssertHasRowError(bill, "WiseCloud User: No charge code found for price code COW");

			priceItem.L7_ChargeCode = "HOSTPRE1";
			bill = new HostingBill(Factory);
			usage1 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.WiseCloudUser, BillingConstants.Hosting.WiseCloudUserFeeCode, 50);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });
			bill.ValidateAll(bill);
			AssertNoNotifications(bill);

			lic.LA_AgreedLiveDate = ZDateTime.Empty;
			bill.ValidateAll(bill);
			AssertHasRowWarning(bill, "Blank site-live usage from " + org.OH_Code + " (Server AAA) is included.");
		}

		public void TestValidate_FeeBasis()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var user = new UsingParty(lic);
			EDIOrgHeader org = lic.Company.Header;
			BillingTestHelper.SetInvoicing(org, Env.CurrentBranch.PK, Env.CurrentCompany.LocalCurrency.Code);
			var prices = BillingTestHelper.CreatePriceList(lic);
			var chargeableUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.HostingStorage, "", new ZDateTime(2011, 2, 1), lic, 1);

			var priceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.DataStorageCode, "", "", 26m);

			HostingBill bill = new HostingBill(Factory);
			HostingUsage usage1 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingStorage, BillingConstants.Hosting.DataStorageCode, 15000);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });
			bill.ValidateAll(bill);
			AssertHasRowError(bill, "WiseCloud Storage: Invalid fee basis for org " + org.OH_Code + ", usage code " + BillingConstants.Hosting.DataStorageCode + ": ");

			foreach (CargoWise.Integration.ICodeDescription feeType in BillingConstants.GetFeeTypeList())
			{
				bill.ClearAllNotifications();
				priceItem.L7_FeeType = feeType.Code;
				bill.ValidateAll(bill);
				if (feeType.Code == BillingConstants.FeeType.PerGBPerMonth
					|| feeType.Code == BillingConstants.FeeType.Per10GBPerMonthMin1GB)
				{
					AssertNoNotifications(bill);
				}
				else
				{
					AssertHasRowError(bill, "WiseCloud Storage: Invalid fee basis for org " + org.OH_Code + ", usage code " + BillingConstants.Hosting.DataStorageCode + ": " + feeType.Code);
				}
			}

			priceItem = BillingTestHelper.AddPriceItem(prices, BillingConstants.Hosting.RemoteDevicesCode, "", "", 10m);
			bill = new HostingBill(Factory);
			usage1 = new HostingUsage(Factory, user, new ZDateTime(2011, 2, 1), BillingConstants.BillingSystem.HostingRemoteDevices, BillingConstants.Hosting.RemoteDevicesCode, 2);
			bill.PopulateFromSystemUsages(new SystemUsage[] { usage1 });

			foreach (CargoWise.Integration.ICodeDescription feeType in BillingConstants.GetFeeTypeList())
			{
				bill.ClearAllNotifications();
				priceItem.L7_FeeType = feeType.Code;
				bill.ValidateAll(bill);
				if (feeType.Code == BillingConstants.FeeType.PerDevicePerMonth)
				{
					AssertNoNotifications(bill);
				}
				else
				{
					AssertHasRowError(bill, "WiseCloud Remote Devices: Invalid fee basis for org " + org.OH_Code + ", usage code " + BillingConstants.Hosting.RemoteDevicesCode + ": " + feeType.Code);
				}
			}
		}

		static ClientLicenceBillingDiscount AddDiscountForMonth(EDIOrgHeader organisation, ZDecimal discount, ZDateTime startDate)
		{
			ClientLicenceBillingDiscount result = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			result.L5_SystemCode = BillingConstants.BillingSystem.HostingStorage;
			result.L5_Type = BillingConstants.DiscountType.Special;
			result.L5_Discount = discount;
			result.L5_StartDate = startDate;
			result.L5_EndDate = startDate.AddMonths(1).AddDays(-1);

			return result;
		}

		static ClientLicenceBillingDiscount AddSurcharge(EDIOrgHeader organisation, ZDecimal discount)
		{
			ClientLicenceBillingDiscount result = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			result.L5_SystemCode = BillingConstants.BillingSystem.HostingStorage;
			result.L5_Type = BillingConstants.DiscountType.Surcharge;
			result.L5_Discount = discount;
			result.L5_Description = "Testing";

			return result;
		}

		protected override HostingBill GetNewSystemBill()
		{
			return new HostingBill(Factory);
		}
	}
}
