using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(ABMCustomsBill))]
	public class ABMCustomsBillTest : TransactionalSystemBillTestCase<ABMCustomsBill>
	{
		#region Invoicing

		public void TestCreateInvoiceLines()
		{
			var org = SetupPriceList();
			var usageList = SetupUsageList(org);

			var bill = new ABMCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(3, lines.Count);

			CombineAssertions(() =>
				{
					AssertEquals("CTM Charge Code", EDIDataRegistry.Instance.ABMCustomsWareMessagingChargeCode.Value, lines[0].ChargeCodeName);
					AssertEquals("ABM CustomsWare Messaging - 120 Transactions at EUR 1.00 per transaction", lines[0].Description);
					AssertEquals("POC Charge Code", EDIDataRegistry.Instance.ABMMovementMessagingChargeCode.Value, lines[1].ChargeCodeName);
					AssertEquals("ABM Movement Messaging - 80 Transactions at EUR 0.33 per transaction", lines[1].Description);
					AssertEquals("FRP Charge Code", EDIDataRegistry.Instance.ABMFiscalRepInvoiceMessagingChargeCode.Value, lines[2].ChargeCodeName);
					AssertEquals("ABM Fiscal Rep Invoice - 40 Transactions at EUR 1.00 per transaction", lines[2].Description);
				});
		}

		public void TestCreateInvoiceLines_Discount()
		{
			var org = SetupPriceListAndDiscountList();
			var usageList = SetupUsageList(org);

			var bill = new ABMCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(6, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("CTM Charge Code", EDIDataRegistry.Instance.ABMCustomsWareMessagingChargeCode.Value, lines[0].ChargeCodeName);
				AssertEquals("ABM CustomsWare Messaging - 120 Transactions at EUR 1.00 per transaction", lines[0].Description);
				AssertEquals("CTM Discount Charge Code", EDIDataRegistry.Instance.ABMCustomsWareMessagingDiscountChargeCode.Value, lines[1].ChargeCodeName);
				AssertEquals("A Volume Discount of 7.5% and a Commitment Discount of 6% to be applied on ABM CustomsWare Messaging", lines[1].Description);

				AssertEquals("Surcharge Charge Code", EDIDataRegistry.Instance.OdplSurchargeChargeCode.Value, lines[2].ChargeCodeName);
				AssertEquals("A Testing Surcharge of 10% to be applied on ABM CustomsWare Messaging", lines[2].Description);

				AssertEquals("POC Charge Code", EDIDataRegistry.Instance.ABMMovementMessagingChargeCode.Value, lines[3].ChargeCodeName);
				AssertEquals("ABM Movement Messaging - 80 Transactions at EUR 0.33 per transaction", lines[3].Description);
				AssertEquals("POC Discount Charge Code", EDIDataRegistry.Instance.ABMMovementMessagingDiscountChargeCode.Value, lines[4].ChargeCodeName);
				AssertEquals("A Volume Discount of 5% to be applied on ABM Movement Messaging", lines[4].Description);

				AssertEquals("FRP Charge Code", EDIDataRegistry.Instance.ABMFiscalRepInvoiceMessagingChargeCode.Value, lines[5].ChargeCodeName);
				AssertEquals("ABM Fiscal Rep Invoice - 40 Transactions at EUR 1.00 per transaction", lines[5].Description);
			});
		}

		public void TestCreateInvoiceLines_MultipleTaxGroup()
		{
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();

			var org1 = SetupPriceList();
			var user1 = new UsingParty(org1);
			var invoiceDelivery1 = org1.LicCompany.InvoiceDeliveries[0];
			invoiceDelivery1.L9_AT_TaxId = taxRate1.PK;

			var org2 = BillingTestHelper.CreateDependentOrganisation(org1, "BBB");
			var user2 = new UsingParty(org2);
			var invoiceDelivery2 = org2.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery2.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery2.L9_OH_InvoiceTo = org1.PK;
			invoiceDelivery2.L9_AT_TaxId = taxRate2.PK;

			var org3 = BillingTestHelper.CreateDependentOrganisation(org1, "CCC");
			var user3 = new UsingParty(org3);
			var invoiceDelivery3 = org3.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery3.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
			invoiceDelivery3.L9_OH_InvoiceTo = org1.PK;
			invoiceDelivery3.L9_AT_TaxId = taxRate1.PK;

			var usageList = new List<EServicesSystemUsage>();
			usageList.Add(new ABMCustomsUsage("CTM", Factory, user1, new ZDateTime(2014, 02, 28)) { TransactionCount = 10 });
			usageList.Add(new ABMCustomsUsage("CTM", Factory, user2, new ZDateTime(2014, 02, 28)) { TransactionCount = 20 });
			usageList.Add(new ABMCustomsUsage("CTM", Factory, user3, new ZDateTime(2014, 02, 28)) { TransactionCount = 30 });
			usageList.Add(new ABMCustomsUsage("POC", Factory, user1, new ZDateTime(2014, 02, 28)) { TransactionCount = 10 });
			usageList.Add(new ABMCustomsUsage("POC", Factory, user3, new ZDateTime(2014, 02, 28)) { TransactionCount = 30 });

			var bill = new ABMCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(3, lines.Count);

			CombineAssertions(() =>
			{
				AssertEquals("ABM CustomsWare Messaging - 40 Transactions at EUR 1.00 per transaction", lines[0].Description);
				AssertEquals("ABM CustomsWare Messaging - 20 Transactions at EUR 1.00 per transaction", lines[1].Description);
				AssertEquals("ABM Movement Messaging - 40 Transactions at EUR 0.33 per transaction", lines[2].Description);
			});
		}

		#endregion

		#region Summary Report

		public void TestGetGeneralSummarySections()
		{
			var org = SetupPriceList();
			var usageList = SetupUsageList(org);

			var bill = new ABMCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGeneralSummarySections(org.PK);
			AssertEquals(3, sections.Length);
			var section = sections[0];
			CombineAssertions(() =>
				{
					AssertEquals("ABM CustomsWare Messaging Usage", section.Header.MainDescription);
					AssertEquals("ABM CustomsWare Messaging", section.Lines[0].MainDescription);
				});

			section = sections[1];
			CombineAssertions(() =>
				{
					AssertEquals("ABM Movement Messaging Usage", section.Header.MainDescription);
					AssertEquals("ABM Movement Messaging", section.Lines[0].MainDescription);
				});

			section = sections[2];
			CombineAssertions(() =>
				{
					AssertEquals("ABM Fiscal Rep Invoice Usage", section.Header.MainDescription);
					AssertEquals("ABM Fiscal Rep Invoice", section.Lines[0].MainDescription);
				});
		}

		public void TestGetDiscountSummarySections()
		{
			var org = SetupPriceListAndDiscountList();
			var usageList = SetupUsageList(org);

			var bill = new ABMCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			ZDecimal expectedAmount = 120 + (0.33m * 80) + 40;
			AssertEquals("Amount", expectedAmount, bill.Amount);

			ZDecimal expectedDiscount = .135m * 120 + .05m * (0.33m * 80);
			AssertEquals("Discounts for each provider applied", expectedDiscount, bill.DiscountAmount);

			var sections = bill.GetDiscountSummarySections();
			AssertEquals(3, sections[0].Lines.Count);
			CombineAssertions(() =>
				{
					AssertEquals("ABM Customs Amount Calculations Applied", sections[0].Header.MainDescription);
					AssertEquals("CustomsWare Messaging - Commitment Discount: 7.20 (Minimum 100 Units, 6%)", sections[0].Lines[0].MainDescription);
					AssertEquals("CustomsWare Messaging - Volume Discount: -9.00 (-7.5% * 120.00)", sections[0].Lines[1].MainDescription);
					AssertEquals("Movement Messaging - Volume Discount: -1.32 (-5% * 26.40)", sections[0].Lines[2].MainDescription);
				});
		}

		public void TestGetSurchargeSummarySections()
		{
			var org = SetupPriceListAndDiscountList();
			var usageList = SetupUsageList(org);

			var bill = new ABMCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			ZDecimal expectedAmount = 120 + (0.33m * 80) + 40;
			AssertEquals("Amount", expectedAmount, bill.Amount);

			ZDecimal expectedSurcharge = 120 * 0.1m;
			AssertEquals("Surcharge", expectedSurcharge, bill.SurchargeAmount);

			var sections = bill.GetSurchargeSummarySections();
			AssertEquals(1, sections[0].Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("ABM Customs Amount Calculations Applied", sections[0].Header.MainDescription);
				AssertEquals("CustomsWare Messaging - Testing Surcharge: 12.00 (10% * 120.00)", sections[0].Lines[0].MainDescription);
			});
		}

		public void TestGetGroupSummarySections()
		{
			var org = SetupPriceListAndDiscountList();
			var user = new UsingParty(org);
			var usageList = new List<ABMCustomsUsage>();
			usageList.Add(new ABMCustomsUsage("CTM", Factory, user, new ZDateTime(2014, 01, 31)) { TransactionCount = 60 });
			usageList.Add(new ABMCustomsUsage("POC", Factory, user, new ZDateTime(2014, 01, 31)) { TransactionCount = 50 });
			usageList.Add(new ABMCustomsUsage("FRP", Factory, user, new ZDateTime(2014, 01, 31)) { TransactionCount = 20 });
			usageList.Add(new ABMCustomsUsage("CTM", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 70 });
			usageList.Add(new ABMCustomsUsage("POC", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 60 });
			usageList.Add(new ABMCustomsUsage("FRP", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 30 });

			var bill = new ABMCustomsBill(Factory);
			bill.PopulateFromSystemUsages(usageList.ToArray());

			var sections = bill.GetGroupSummarySections();
			AssertEquals(3, sections.Length);
			var section = sections[0];
			CombineAssertions(() =>
				{
					AssertEquals("ABM CustomsWare Messaging Group Summary", section.Header.MainDescription);
					AssertEquals("IPHQFAAAM (IPH-AAM-xxx) Jan 2014", section.Lines[0].MainDescription);
					AssertEquals("IPHQFAAAM (IPH-AAM-xxx) Feb 2014", section.Lines[1].MainDescription);
					AssertEquals("60.00", section.Lines[0].Amount);
					AssertEquals("70.00", section.Lines[1].Amount);
					AssertEquals("Total ABM CustomsWare Messaging (EUR)", section.Header.TotalDescription);
				});

			section = sections[1];
			CombineAssertions(() =>
				{
					AssertEquals("ABM Movement Messaging Group Summary", section.Header.MainDescription);
					AssertEquals("IPHQFAAAM (IPH-AAM-xxx) Jan 2014", section.Lines[0].MainDescription);
					AssertEquals("IPHQFAAAM (IPH-AAM-xxx) Feb 2014", section.Lines[1].MainDescription);
					AssertEquals("16.50", section.Lines[0].Amount);
					AssertEquals("19.80", section.Lines[1].Amount);
					AssertEquals("Total ABM Movement Messaging (EUR)", section.Header.TotalDescription);
				});

			section = sections[2];
			CombineAssertions(() =>
				{
					AssertEquals("ABM Fiscal Rep Invoice Group Summary", section.Header.MainDescription);
					AssertEquals("IPHQFAAAM (IPH-AAM-xxx) Jan 2014", section.Lines[0].MainDescription);
					AssertEquals("IPHQFAAAM (IPH-AAM-xxx) Feb 2014", section.Lines[1].MainDescription);
					AssertEquals("20.00", section.Lines[0].Amount);
					AssertEquals("30.00", section.Lines[1].Amount);
					AssertEquals("Total ABM Fiscal Rep Invoice (EUR)", section.Header.TotalDescription);
				});
		}

		#endregion

		#region Implementation

		EDIOrgHeader SetupPriceList()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "EEECOMSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var priceHeader = stdLicCompany.PriceHeaders.AddNew();
			priceHeader.L6_PricelistVersion = "V1";
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);

			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, "CTM", BillingConstants.FeeType.Transactional, "", 1.0m);
			priceItem1.L7_Description = "ABM CustomsWare Messaging";
			priceItem1.L7_RX_NKCurrency = "EUR";

			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, "POC", BillingConstants.FeeType.Transactional, "", 0.33m);
			priceItem2.L7_Description = "ABM Movement Messaging";
			priceItem2.L7_RX_NKCurrency = "EUR";

			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, "FRP", BillingConstants.FeeType.Transactional, "", 1.0m);
			priceItem3.L7_Description = "ABM Fiscal Rep Invoice";
			priceItem3.L7_RX_NKCurrency = "EUR";

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		EDIOrgHeader SetupPriceListAndDiscountList()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "IPHQFAAAM";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			AddDiscountForTwoMonths(org, ABMCustomsTransactionTypes.Codes.PortCommunity, 50m, 10m, new ZDateTime(2013, 12, 1));
			AddDiscountForTwoMonths(org, ABMCustomsTransactionTypes.Codes.Customs, 100m, 20m, new ZDateTime(2013, 12, 1));

			AddDiscountForTwoMonths(org, ABMCustomsTransactionTypes.Codes.PortCommunity, 50m, 5m, new ZDateTime(2014, 02, 1));
			AddDiscountForTwoMonths(org, ABMCustomsTransactionTypes.Codes.PortCommunity, 100m, 6m, new ZDateTime(2014, 02, 1));
			AddDiscountForTwoMonths(org, ABMCustomsTransactionTypes.Codes.Customs, 50m, 6.5m, new ZDateTime(2014, 02, 1));
			AddDiscountForTwoMonths(org, ABMCustomsTransactionTypes.Codes.Customs, 100m, 7.5m, new ZDateTime(2014, 02, 1));
			AddCommitmentDiscount(org, ABMCustomsTransactionTypes.Codes.Customs, 100, 6m);
			AddSurcharge(org, ABMCustomsTransactionTypes.Codes.Customs, -10m);

			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;

			var priceHeader = stdLicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			priceHeader.L6_ValidFrom = new ZDateTime(2014, 1, 1);
			priceHeader.L6_RX_NKCurrency = "EUR";
			BillingTestHelper.AddPriceItem(priceHeader, "CTM", BillingConstants.FeeType.Transactional, "", 1.0m).L7_Description = "ABM CustomsWare Messaging";
			BillingTestHelper.AddPriceItem(priceHeader, "POC", BillingConstants.FeeType.Transactional, "", 0.33m).L7_Description = "ABM Movement Messaging";
			BillingTestHelper.AddPriceItem(priceHeader, "FRP", BillingConstants.FeeType.Transactional, "", 1.0m).L7_Description = "ABM Fiscal Rep Invoice";

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			Factory.Save();
			return org;
		}

		List<ABMCustomsUsage> SetupUsageList(EDIOrgHeader org)
		{
			var result = new List<ABMCustomsUsage>();
			var user = new UsingParty(org);

			result.Add(new ABMCustomsUsage("CTM", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 60 });
			result.Add(new ABMCustomsUsage("CTM", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 60 });
			result.Add(new ABMCustomsUsage("POC", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 50 });
			result.Add(new ABMCustomsUsage("POC", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 30 });
			result.Add(new ABMCustomsUsage("FRP", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 20 });
			result.Add(new ABMCustomsUsage("FRP", Factory, user, new ZDateTime(2014, 02, 28)) { TransactionCount = 20 });
			return result;
		}

		void AddDiscountForTwoMonths(EDIOrgHeader organisation, ZString subCode, ZDecimal breakAmount, ZDecimal discount, ZDateTime startDate)
		{
			var result = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			result.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			result.L5_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			result.L5_Type = BillingConstants.DiscountType.Volume;
			result.L5_Discount = discount;
			result.L5_StartDate = startDate;
			result.L5_EndDate = startDate.AddMonths(2).AddDays(-1);
			result.L5_SubCode = subCode;
			result.L5_BreakAmount = breakAmount;
		}

		void AddCommitmentDiscount(EDIOrgHeader organisation, ZString subCode, ZInt unitAmount, ZDecimal discount)
		{
			var result = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			result.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			result.L5_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			result.L5_Type = BillingConstants.DiscountType.Commitment;
			result.L5_Discount = discount;
			result.L5_SubCode = subCode;
			result.L5_Units = unitAmount;
		}

		void AddSurcharge(EDIOrgHeader organisation, ZString subCode, ZDecimal discount)
		{
			var result = organisation.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			result.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			result.L5_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			result.L5_Type = BillingConstants.DiscountType.Surcharge;
			result.L5_Discount = discount;
			result.L5_SubCode = subCode;
			result.L5_Description = "Testing";
		}

		#endregion

		#region Overrides

		protected override SystemUsage[] CreateValidSystemUsages()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var user = new UsingParty(organisation);

			return new SystemUsage[]
			{
				new ABMCustomsUsage("CTM", Factory, user, new ZDateTime(2010, 10, 01)) { SubCode = ABMCustomsTransactionTypes.Codes.PortCommunity },
				new ABMCustomsUsage("POC", Factory, user, new ZDateTime(2010, 11, 01)) { SubCode = ABMCustomsTransactionTypes.Codes.Customs }
			};
		}

		protected override ABMCustomsBill GetNewSystemBill()
		{
			return new ABMCustomsBill(Factory);
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
