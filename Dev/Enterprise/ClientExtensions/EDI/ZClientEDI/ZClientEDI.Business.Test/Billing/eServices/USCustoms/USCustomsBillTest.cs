using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(USCustomsBill))]
	public class USCustomsBillTest : TransactionalSystemBillTestCase<USCustomsBill>
	{
		public void TestChargeCodes()
		{
			var bill = new USCustomsBill(Factory);
			var systemCode = BillingConstants.BillingSystem.USCustoms;
			AssertEquals(EDIDataRegistry.Instance.TransactionChargeCodes.Value.GetDescriptionFromCode(systemCode), bill.GetAmountChargeCodeName(null));
			AssertEquals(EDIDataRegistry.Instance.TransactionDiscountChargeCodes.Value.GetDescriptionFromCode(systemCode), bill.GetDiscountChargeCodeName(null));
		}

		#region Invoicing

		public void TestCreateInvoiceLines()
		{
			var org1 = SetupOrgWithPriceList("DDDAAASYD", false);
			var org2 = SetupOrgWithPriceList("DDDBBBMEL", true);

			var usageList = new List<USCustomsUsage>();
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org1), new ZDateTime(2014, 12, 31)) { TransactionCount = 50 });
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org2), new ZDateTime(2014, 12, 31)) { TransactionCount = 20 });

			var bill = new USCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals("Should be separate", 2, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("USC Service Bureau Usage - DDDAAASYD - 50 Transactions at USD 0.05 per transaction", lines[0].Description);
				AssertEquals(2.5m, lines[0].Amount);
				AssertEquals("USC Service Bureau Usage - DDDBBBMEL - 20 Transactions at USD 0.10 per transaction. Minimum fee of USD 20.00 per month which includes 200 Transactions", lines[1].Description);
				AssertEquals(20m, lines[1].Amount);
			});
		}

		public void TestCreateInvoiceLines_Discount()
		{
			var org1 = SetupOrgWithPriceListAndDiscount("DDDAAASYD", false);
			var org2 = SetupOrgWithPriceListAndDiscount("DDDBBBMEL", true);
			var org3 = SetupOrgWithPriceListAndDiscount("DDDCCCBNE", false);

			var usageList = new List<USCustomsUsage>();
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org1), new ZDateTime(2014, 12, 31)) { TransactionCount = 501 });
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org2), new ZDateTime(2014, 12, 31)) { TransactionCount = 20 });
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org3), new ZDateTime(2014, 12, 31)) { TransactionCount = 700 });

			var bill = new USCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals("Should be separate", 8, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("USC Service Bureau Usage - DDDAAASYD - 501 Transactions at USD 0.05 per transaction", lines[0].Description);
				AssertEquals(25.05m, lines[0].Amount);
				AssertEquals("USC Service Bureau Usage - DDDAAASYD - Incremental Volume Discount", lines[1].Description);
				AssertEquals(-1.26m, lines[1].Amount);
				AssertEquals("USC Service Bureau Usage - DDDAAASYD - Testing Surcharge of 10%", lines[2].Description);
				AssertEquals(2.51m, lines[2].Amount);

				AssertEquals("USC Service Bureau Usage - DDDBBBMEL - 20 Transactions at USD 0.10 per transaction. Minimum fee of USD 20.00 per month which includes 200 Transactions", lines[3].Description);
				AssertEquals(20m, lines[3].Amount);
				AssertEquals("USC Service Bureau Usage - DDDBBBMEL - Testing Surcharge of 10%", lines[4].Description);
				AssertEquals(2m, lines[4].Amount);

				AssertEquals("USC Service Bureau Usage - DDDCCCBNE - 700 Transactions at USD 0.05 per transaction", lines[5].Description);
				AssertEquals(35m, lines[5].Amount);
				AssertEquals("USC Service Bureau Usage - DDDCCCBNE - Incremental Volume Discount", lines[6].Description);
				AssertEquals(-3.75m, lines[6].Amount);
				AssertEquals("USC Service Bureau Usage - DDDCCCBNE - Testing Surcharge of 10%", lines[7].Description);
				AssertEquals(3.5m, lines[7].Amount);
			});
		}

		#endregion

		#region Summary Report

		public void TestGetGeneralSummarySections()
		{
			var org = SetupOrgWithPriceList("DDDAAASYD", false);

			var usageList = new List<USCustomsUsage>();
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org), new ZDateTime(2014, 12, 31)) { TransactionCount = 700 });

			var bill = new USCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);
			AssertEquals(1, sections.Length);
			CombineAssertions(() =>
			{
				AssertEquals("USC Service Bureau", sections[0].Header.MainDescription);
				AssertEquals("USC Service Bureau Usage", sections[0].Lines[0].MainDescription);
			});
		}

		public void TestGetGroupSummarySections()
		{
			var org1 = SetupOrgWithPriceList("DDDAAASYD", false);
			var org2 = SetupOrgWithPriceList("DDDBBBMEL", true);

			var usageList = new List<USCustomsUsage>();
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org1), new ZDateTime(2014, 11, 30)) { TransactionCount = 35 });
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org2), new ZDateTime(2014, 11, 30)) { TransactionCount = 10 });
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org1), new ZDateTime(2014, 12, 31)) { TransactionCount = 50 });
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org2), new ZDateTime(2014, 12, 31)) { TransactionCount = 20 });

			var bill = new USCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGroupSummarySections();
			AssertEquals(1, sections.Length);
			var section = sections[0];
			CombineAssertions(() =>
			{
				AssertEquals("DDDAAASYD (DDD-SYD-xxx) Nov 2014", section.Lines[0].MainDescription);
				AssertEquals("1.75", section.Lines[0].Amount);
				AssertEquals("DDDAAASYD (DDD-SYD-xxx) Dec 2014", section.Lines[1].MainDescription);
				AssertEquals("2.50", section.Lines[1].Amount);
				AssertEquals("DDDBBBMEL (DDD-MEL-xxx) Nov 2014", section.Lines[2].MainDescription);
				AssertEquals("20.00", section.Lines[2].Amount);
				AssertEquals("DDDBBBMEL (DDD-MEL-xxx) Dec 2014", section.Lines[3].MainDescription);
				AssertEquals("20.00", section.Lines[3].Amount);
			});
		}

		public void TestGetDiscountSummarySections()
		{
			var org1 = SetupOrgWithPriceListAndDiscount("DDDAAASYD", false);
			var org2 = SetupOrgWithPriceListAndDiscount("DDDBBBMEL", true);
			var org3 = SetupOrgWithPriceListAndDiscount("DDDCCCBNE", false);

			var usageList = new List<USCustomsUsage>();
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org1), new ZDateTime(2014, 12, 31)) { TransactionCount = 501 });
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org2), new ZDateTime(2014, 12, 31)) { TransactionCount = 20 });
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org3), new ZDateTime(2014, 12, 31)) { TransactionCount = 700 });

			var bill = new USCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			ZDecimal expectedAmount = 0.05m * 501;
			expectedAmount += 0.10m * 200; // 200 instead of 20 because of minimum fee
			expectedAmount += 0.05m * 700;
			AssertEquals("Amount", expectedAmount, bill.Amount);

			ZDecimal expectedDiscount = 0.1m * (0.05m * 250);
			expectedDiscount += 0.25m * 0.05m;
			expectedDiscount += 0.1m * (0.05m * 250);
			expectedDiscount += 0.25m * (0.05m * 200);
			AssertEquals("Discounts for each provider applied", Utilities.Round(expectedDiscount, BillingConstants.RoundingDecimals), bill.DiscountAmount);

			var sections = bill.GetDiscountSummarySections();
			AssertEquals(4, sections[0].Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("US Customs Amount Calculations Applied", sections[0].Header.MainDescription);
				AssertEquals("USC Service Bureau Usage - DDDAAASYD - Incremental Volume Discount: 0.01 (1 Units @ 25%)", sections[0].Lines[0].MainDescription);
				AssertEquals("USC Service Bureau Usage - DDDAAASYD - Incremental Volume Discount: 1.25 (250 Units @ 10%)", sections[0].Lines[1].MainDescription);
				AssertEquals("USC Service Bureau Usage - DDDCCCBNE - Incremental Volume Discount: 2.50 (200 Units @ 25%)", sections[0].Lines[2].MainDescription);
				AssertEquals("USC Service Bureau Usage - DDDCCCBNE - Incremental Volume Discount: 1.25 (250 Units @ 10%)", sections[0].Lines[3].MainDescription);
			});
		}

		public void TestGetSurchargeSummarySections()
		{
			var org1 = SetupOrgWithPriceListAndDiscount("DDDAAASYD", false);

			var usageList = new List<USCustomsUsage>();
			usageList.Add(new USCustomsUsage("USC", Factory, new UsingParty(org1), new ZDateTime(2014, 12, 31)) { TransactionCount = 200 });

			var bill = new USCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			ZDecimal expectedAmount = 0.05m * 200;
			AssertEquals("Amount", expectedAmount, bill.Amount);

			ZDecimal expectedSurcharge = 0.1m * (0.05m * 200);
			AssertEquals("Surcharge", Utilities.Round(expectedSurcharge, BillingConstants.RoundingDecimals), bill.SurchargeAmount);

			var sections = bill.GetSurchargeSummarySections();
			AssertEquals(1, sections[0].Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("US Customs Amount Calculations Applied", sections[0].Header.MainDescription);
				AssertEquals("USC Service Bureau Usage - DDDAAASYD - Testing Surcharge: 1.00 (10% * 10.00)", sections[0].Lines[0].MainDescription);
			});
		}

		#endregion

		#region Implementation

		EDIOrgHeader SetupOrgWithPriceList(ZString orgCode, bool hasMinimumFee)
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";

			var priceItem = BillingTestHelper.AddPriceItem(priceHeader, "USC", BillingConstants.FeeType.Transactional, "", hasMinimumFee ? 0.10m : 0.05m);
			priceItem.L7_Description = "Messaging Service Bureau";
			priceItem.L7_UnitBreak = hasMinimumFee ? 200 : 0;
			priceItem.L7_RX_NKCurrency = "USD";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		EDIOrgHeader SetupOrgWithPriceListAndDiscount(ZString orgCode, bool hasMinimumFee)
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = orgCode;
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			AddIncrementalDiscountForTwoMonths(org, 250, 10m, new ZDateTime(2014, 12, 1));
			AddIncrementalDiscountForTwoMonths(org, 500, 25m, new ZDateTime(2014, 12, 1));
			AddSurcharge(org, -10m);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			var priceItem = BillingTestHelper.AddPriceItem(priceHeader, "USC", BillingConstants.FeeType.Transactional, "", hasMinimumFee ? 0.10m : 0.05m);
			priceItem.L7_Description = "Messaging Service Bureau";
			priceItem.L7_UnitBreak = hasMinimumFee ? 200 : 0;

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		void AddIncrementalDiscountForTwoMonths(EDIOrgHeader organisation, ZInt units, ZDecimal discount, ZDateTime startDate)
		{
			var result = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			result.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			result.L5_SystemCode = BillingConstants.BillingSystem.USCustoms;
			result.L5_Type = BillingConstants.DiscountType.IncrementalVolume;
			result.L5_Discount = discount;
			result.L5_StartDate = startDate;
			result.L5_EndDate = startDate.AddMonths(2).AddDays(-1);
			result.L5_Units = units;
		}

		void AddSurcharge(EDIOrgHeader organisation, ZDecimal discount)
		{
			var result = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			result.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			result.L5_SystemCode = BillingConstants.BillingSystem.USCustoms;
			result.L5_Type = BillingConstants.DiscountType.Surcharge;
			result.L5_Discount = discount;
			result.L5_Description = "Testing";
		}

		#endregion

		protected override USCustomsBill GetNewSystemBill()
		{
			return new USCustomsBill(Factory);
		}
	}
}
