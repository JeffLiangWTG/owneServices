using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(OceanCarrierMessagingBill))]
	public class OceanCarrierMessagingBillTest : TransactionalSystemBillTestCase<OceanCarrierMessagingBill>
	{
		public void TestChargeCode()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("SHI", "SHICHAR");
			list.AddPair("SHR", "SHRCHAR");
			list.AddPair("CMV", "CMVCHAR");
			EDIDataRegistry.Instance.TransactionChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var bill = new OceanCarrierMessagingBill(Factory);

			var usage1 = new OceanCarrierMessagingUsage("SHI", Factory, null, new ZDateTime(2016, 8, 1));
			AssertEquals("SHICHAR", bill.GetAmountChargeCodeName(usage1));

			var usage2 = new OceanCarrierMessagingUsage("SHR", Factory, null, new ZDateTime(2016, 8, 1));
			AssertEquals("SHRCHAR", bill.GetAmountChargeCodeName(usage2));

			var usage3 = new OceanCarrierMessagingUsage("CMV", Factory, null, new ZDateTime(2016, 8, 1));
			AssertEquals("CMVCHAR", bill.GetAmountChargeCodeName(usage3));
		}

		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList();

			var usageList = new List<OceanCarrierMessagingUsage>();
			usageList.Add(new OceanCarrierMessagingUsage("SHI", Factory, new UsingParty(org), new ZDateTime(2016, 8, 31)) { TransactionCount = 100 });
			usageList.Add(new OceanCarrierMessagingUsage("SHR", Factory, new UsingParty(org), new ZDateTime(2016, 8, 31)) { TransactionCount = 200 });
			usageList.Add(new OceanCarrierMessagingUsage("CMV", Factory, new UsingParty(org), new ZDateTime(2016, 8, 31)) { TransactionCount = 300 });

			var bill = new OceanCarrierMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);
			AssertEquals(1, sections.Length);
			AssertEquals("Ocean Carrier Messaging Usage", sections[0].Header.MainDescription);
			AssertEquals("Shipping Instruction Sent", sections[0].Lines[0].MainDescription);
			AssertEquals("Shipping Instruction Received", sections[0].Lines[1].MainDescription);
			AssertEquals("VGM Received", sections[0].Lines[2].MainDescription);
		}

		public void TestCreateInvoiceLines_Discount()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("SHI", "SHICHAR");
			list.AddPair("SHR", "SHRCHAR");
			list.AddPair("VGM", "VGMCHAR");
			EDIDataRegistry.Instance.TransactionChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			list = new CodeDescriptionPairList();
			list.AddPair("VGM", "VGMDISC");
			EDIDataRegistry.Instance.TransactionDiscountChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var org = SetupOrgWithPriceList();
			var user = new UsingParty(org);
			var usageList = new List<OceanCarrierMessagingUsage>();
			usageList.Add(new OceanCarrierMessagingUsage("SHI", Factory, user, new ZDateTime(2016, 8, 1)) { TransactionCount = 20 });
			usageList.Add(new OceanCarrierMessagingUsage("SHR", Factory, user, new ZDateTime(2016, 8, 1)) { TransactionCount = 30 });
			usageList.Add(new OceanCarrierMessagingUsage("VGM", Factory, user, new ZDateTime(2016, 8, 1)) { TransactionCount = 10 });

			var discount = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.OceanCarrierMessaging;
			discount.L5_SubCode = "VGM";
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 100;
			discount.L5_StartDate = new ZDateTime(2016, 1, 1);

			var bill = new OceanCarrierMessagingBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(4, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("SHICHAR", lines[0].ChargeCodeName);
				AssertEquals("Shipping Instruction Sent - 20 Transactions at USD 0.05 per Transaction", lines[0].Description);
				AssertEquals("SHRCHAR", lines[1].ChargeCodeName);
				AssertEquals("Shipping Instruction Received - 30 Transactions at USD 0.05 per Transaction", lines[1].Description);
				AssertEquals("VGMCHAR", lines[2].ChargeCodeName);
				AssertEquals("VGM Sent - 10 Transactions at USD 0.05 per Transaction", lines[2].Description);
				AssertEquals("VGMDISC", lines[3].ChargeCodeName);
				AssertEquals("VGM Sent Discount", lines[3].Description);
				AssertEquals(-0.50m, lines[3].Amount);
			});
		}

		EDIOrgHeader SetupOrgWithPriceList(string orgCode = "DDDCOMSYD")
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			BillingTestHelper.AddPriceItem(priceHeader, "SHI", BillingConstants.FeeType.Transactional, "", 0.05m);
			BillingTestHelper.AddPriceItem(priceHeader, "SHR", BillingConstants.FeeType.Transactional, "", 0.05m);
			BillingTestHelper.AddPriceItem(priceHeader, "VGM", BillingConstants.FeeType.Transactional, "", 0.05m);
			BillingTestHelper.AddPriceItem(priceHeader, "CMV", BillingConstants.FeeType.Transactional, "", 0.05m);

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		protected override OceanCarrierMessagingBill GetNewSystemBill()
		{
			return new OceanCarrierMessagingBill(Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OceanCarrierMessagingBill(Factory);
		}
	}
}
