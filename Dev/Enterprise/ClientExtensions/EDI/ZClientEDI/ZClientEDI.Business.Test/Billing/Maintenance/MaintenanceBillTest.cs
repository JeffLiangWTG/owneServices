using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	[TestedType(typeof(MaintenanceBill))]
	internal class MaintenanceBillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			var prices = licHeader.Company.CreatePriceList(false);
			prices.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			prices.L6_ValidFrom = ZDateTime.Now.AddMonths(-2);
			BillingTestHelper.AddPriceItem(prices, "COR", BillingConstants.FeeType.NamedUser, "", 1000m);

			MaintenanceBillRecipient billRecipient = NewRecipient(licHeader);
			MaintenanceBill bill = new MaintenanceBill(licHeader, billRecipient);
			AssertEquals(licHeader.PK, bill.LicHeader.PK);
			AssertEquals(billRecipient, bill.Recipient);
			AssertEquals(licHeader.Company.Header.OH_Code, bill.OrgCode);
		}

		public void TestRenewalDate()
		{
			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);
			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			AssertEquals(new ZDateTime(2011, 1, 1), bill.RenewalDate);

			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 6, 30);
			bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			AssertEquals(new ZDateTime(2010, 7, 1), bill.RenewalDate);
		}

		public void TestRenewalNoticeDate()
		{
			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);
			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			AssertEquals(new ZDateTime(2010, 11, 1), bill.RenewalNoticeDate);

			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 6, 30);
			bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			AssertEquals(new ZDateTime(2010, 5, 1), bill.RenewalNoticeDate);
		}

		public void TestValidateAll()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			licHeader.LA_ContractExpiryDate = ZDateTime.Empty;
			MaintenanceBillRecipient billRecipient = NewRecipient(licHeader);
			var delivery = licHeader.Company.InvoiceDeliveries[0];
			MaintenanceBill bill = new MaintenanceBill(licHeader, billRecipient, delivery);
			AssertHasMessageError(bill.StatusTextInfo, "No contract expiry date for " + licHeader.Company.Header.OH_Code);
			licHeader.LA_ContractExpiryDate = ZDateTime.Now;

			licHeader.Billing.L0_FixedMaintenanceAmount = 10;
			bill = new MaintenanceBill(licHeader, billRecipient, delivery);
			AssertHasMessageError(bill.StatusTextInfo, "No currency defined for fixed maintenance amount");

			licHeader.Billing.L0_RX_NKFixedMaintenanceCurrency = "AUD";
			bill = new MaintenanceBill(licHeader, billRecipient, delivery);
			AssertNoMessageErrors(bill.StatusTextInfo);

			var payingOrgWithoutLicence = Factory.NewWithValidTestData<EDIOrgHeader>();
			payingOrgWithoutLicence.CompanyData.OB_IsDebtor = true;
			var recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, payingOrgWithoutLicence.PK, "AUD", ZDateTime.Now);
			delivery.L9_OH_InvoiceTo = payingOrgWithoutLicence.PK;
			bill = new MaintenanceBill(licHeader, recipient, delivery);
			AssertNoMessageErrors(bill.StatusTextInfo);
			delivery.L9_OH_InvoiceTo = ZGuid.Empty;

			licHeader.Billing.L0_FixedMaintenanceAmount = 0;
			bill = new MaintenanceBill(licHeader, billRecipient, delivery);
			AssertHasMessageError(bill.StatusTextInfo, "No pricelist found for " + licHeader.Company.Header.OH_Code);

			var prices = BillingTestHelper.CreateMaintenancePriceList(licHeader);
			bill = new MaintenanceBill(licHeader, billRecipient, delivery);
			AssertNoMessageErrors(bill.StatusTextInfo);

			EDIOrgHeader partner = BillingTestHelper.CreateOrganisation(Factory, "PAR");
			partner.LicCompany.SelfBilling.L4_IsPartner = true;
			delivery.L9_OH_InvoiceTo = partner.PK;
			billRecipient = NewRecipient(licHeader);
			bill = new MaintenanceBill(licHeader, billRecipient, delivery);
			AssertHasMessageError(bill.StatusTextInfo, "Partner billing not implemented");

			delivery.L9_OH_InvoiceTo = ZGuid.Empty;
			licHeader.Billing.L0_NextMaintenancePercent = 0m;
			licHeader.Billing.L0_NextNewSeatMaintenancePercent = 0m;
			billRecipient = NewRecipient(licHeader);
			bill = new MaintenanceBill(licHeader, billRecipient, delivery);
			AssertHasMessageError(bill.StatusTextInfo, "Maintenance is zero.");
		}

		public void TestPriceCurrencyCode()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			licHeader.Billing.L0_FixedMaintenanceAmount = 30m;
			MaintenanceBillRecipient billRecipient = NewRecipient(licHeader);
			MaintenanceBill bill = new MaintenanceBill(licHeader, billRecipient, licHeader.Company.InvoiceDeliveries[0]);
			AssertEquals("no currency", "", bill.PriceCurrencyCode);

			licHeader.Billing.L0_RX_NKFixedMaintenanceCurrency = "AUD";
			bill = new MaintenanceBill(licHeader, billRecipient, licHeader.Company.InvoiceDeliveries[0]);
			AssertEquals("price currency from fixed maintenance currency", "AUD", bill.PriceCurrencyCode);

			var prices = licHeader.Company.CreatePriceList(false);
			prices.L6_SystemCode = BillingConstants.BillingSystem.Maintenance;
			prices.L6_ValidFrom = ZDateTime.Now.AddMonths(-2);
			prices.L6_RX_NKCurrency = "USD";
			licHeader.Billing.L0_FixedMaintenanceAmount = 0;
			bill = new MaintenanceBill(licHeader, billRecipient, licHeader.Company.InvoiceDeliveries[0]);
			AssertEquals("price currency from price list", "USD", bill.PriceCurrencyCode);
		}

		public void TestCanInvoice()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "AAA");
			licHeader.Billing.L0_FixedMaintenanceAmount = 30m;
			MaintenanceBillRecipient billRecipient = NewRecipient(licHeader);

			MaintenanceBill bill = new MaintenanceBill(licHeader, billRecipient, licHeader.Company.InvoiceDeliveries[0]);
			Assert("no currency", !bill.CanInvoice);

			licHeader.Billing.L0_RX_NKFixedMaintenanceCurrency = "AUD";
			Factory.Save();
			bill = new MaintenanceBill(licHeader, billRecipient, licHeader.Company.InvoiceDeliveries[0]);
			Assert("can invoice", bill.CanInvoice);

			licHeader.Billing.L0_NextMaintenancePercent = 11;
			bill = new MaintenanceBill(licHeader, billRecipient);
			Assert("not saved", !bill.CanInvoice);
		}

		public void TestPopulateInvoice()
		{
			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);
			TestLicenceHeader.Billing.L0_NextMaintenancePercent = 30m;
			TestLicenceHeader.Billing.L0_NextNewSeatMaintenancePercent = 30m;
			var prices = BillingTestHelper.CreateMaintenancePriceList(TestLicenceHeader);

			TestLicenceHeader.GetCoreModule().LM_UserCount = 11;
			prices.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			ARInvoice invoice = Factory.New<ARInvoice>();
			bill.PopulateInvoice(invoice);

			AssertEquals("lines", 6, invoice.Lines.Count);

			AssertEquals(new ZDecimal(11 * 1000m * 0.3m), invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("ediEnterprise Application Services Renewal", invoice.Lines[0].AL_Desc);
			AssertEquals("ANNMAINT", invoice.Lines[0].GenericChargeBizO.VC_Code);
			AssertEquals(Env.CurrentBranch.PK, invoice.Lines[0].AL_GB);
			AssertEquals(Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue, invoice.Lines[0].AL_LineType);

			AssertEquals("1 January 2011 to 31 December 2011", invoice.Lines[1].AL_Desc);
			AssertEquals("----------------------------------", invoice.Lines[2].AL_Desc);
			AssertEquals(TestLicenceHeader.Company.Header.OH_FullName, invoice.Lines[3].AL_Desc);
			AssertEquals(TestLicenceHeader.Company.Header.OH_Code + " Server " + TestLicenceHeader.Database.LD_ServerCode, invoice.Lines[4].AL_Desc);
			AssertEquals("Core (11)", invoice.Lines[5].AL_Desc);

			// Positive Surcharge
			TestLicenceHeader.Billing.L0_Surcharge = 1;
			TestLicenceHeader.Billing.L0_SurchargeDescription = "Test surcharge description";
			ARInvoice invoiceWithPositiveSurcharge = Factory.New<ARInvoice>();

			bill.PopulateInvoice(invoiceWithPositiveSurcharge);
			AssertEquals("lines", 7, invoiceWithPositiveSurcharge.Lines.Count);
			AssertEquals(new ZDecimal(11 * 1000m * 0.3m * 0.01m), invoiceWithPositiveSurcharge.Lines[6].AL_OSExTaxAmount);
			AssertEquals("Test surcharge description", invoiceWithPositiveSurcharge.Lines[6].AL_Desc);
			AssertEquals("ANNMAINT", invoiceWithPositiveSurcharge.Lines[6].GenericChargeBizO.VC_Code);
			AssertEquals(Env.CurrentBranch.PK, invoiceWithPositiveSurcharge.Lines[6].AL_GB);
			AssertEquals(Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue, invoiceWithPositiveSurcharge.Lines[6].AL_LineType);

			// Negative Surcharge
			TestLicenceHeader.Billing.L0_Surcharge = -1;
			TestLicenceHeader.Billing.L0_SurchargeDescription = "Test surcharge description";
			ARInvoice invoiceWithNegativeSurcharge = Factory.New<ARInvoice>();

			bill.PopulateInvoice(invoiceWithNegativeSurcharge);
			AssertEquals("lines", 7, invoiceWithNegativeSurcharge.Lines.Count);
			AssertEquals(new ZDecimal(11 * 1000m * 0.3m * -0.01m), invoiceWithNegativeSurcharge.Lines[6].AL_OSExTaxAmount);
			AssertEquals("Test surcharge description", invoiceWithNegativeSurcharge.Lines[6].AL_Desc);
			AssertEquals("ANNMAINT", invoiceWithNegativeSurcharge.Lines[6].GenericChargeBizO.VC_Code);
			AssertEquals(Env.CurrentBranch.PK, invoiceWithNegativeSurcharge.Lines[6].AL_GB);
			AssertEquals(Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue, invoiceWithNegativeSurcharge.Lines[6].AL_LineType);

			// OnInvoiceChanged
			AssertEquals(true, Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_AH_Invoice, invoiceWithNegativeSurcharge.PK)).Any());
			var newInvoice = Factory.New<ARInvoice>();
			((IInvoiceUpdateNotifier)bill).OnInvoiceChanged(newInvoice);
			AssertEquals(newInvoice, bill.Invoice);
			AssertEquals(true, Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_AH_Invoice, newInvoice.PK)).Any());
		}

		public void TestPopulateInvoice_FixedMaintenanceAmount()
		{
			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);
			TestLicenceHeader.Billing.L0_FixedMaintenanceAmount = 5000m;
			TestLicenceHeader.Billing.L0_RX_NKFixedMaintenanceCurrency = "AUD";

			TestLicenceHeader.GetCoreModule().LM_UserCount = 11;

			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			ARInvoice invoice = Factory.New<ARInvoice>();
			bill.PopulateInvoice(invoice);

			AssertEquals("lines", 5, invoice.Lines.Count);

			AssertEquals(5000m, invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("ediEnterprise Application Services Renewal", invoice.Lines[0].AL_Desc);
			AssertEquals("ANNMAINT", invoice.Lines[0].GenericChargeBizO.VC_Code);
			AssertEquals(Env.CurrentBranch.PK, invoice.Lines[0].AL_GB);
			AssertEquals(Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue, invoice.Lines[0].AL_LineType);

			AssertEquals("1 January 2011 to 31 December 2011", invoice.Lines[1].AL_Desc);
			AssertEquals("----------------------------------", invoice.Lines[2].AL_Desc);
			AssertEquals(TestLicenceHeader.Company.Header.OH_FullName, invoice.Lines[3].AL_Desc);
			AssertEquals(TestLicenceHeader.Company.Header.OH_Code + " Server " + TestLicenceHeader.Database.LD_ServerCode, invoice.Lines[4].AL_Desc);
		}

		public void TestPopulateInvoice_CurrencyExchange()
		{
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 2);

			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);
			TestLicenceHeader.Billing.L0_NextMaintenancePercent = 30m;
			TestLicenceHeader.Billing.L0_NextNewSeatMaintenancePercent = 30m;
			var prices = BillingTestHelper.CreateMaintenancePriceList(TestLicenceHeader);
			prices.L6_RX_NKCurrency = "USD";

			TestLicenceHeader.GetCoreModule().LM_UserCount = 11;
			prices.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			AssertEquals("exchange rate", 1 / 2m, bill.ExchangeRate);

			ARInvoice invoice = Factory.New<ARInvoice>();
			bill.PopulateInvoice(invoice);

			AssertEquals("lines", 7, invoice.Lines.Count);

			AssertEquals(new ZDecimal(11 * 1000m * 0.3m / 2m), invoice.Lines[0].AL_OSExTaxAmount);
			AssertEquals("1 January 2011 to 31 December 2011", invoice.Lines[1].AL_Desc);
			AssertEquals("----------------------------------", invoice.Lines[2].AL_Desc);
			AssertEquals(TestLicenceHeader.Company.Header.OH_FullName, invoice.Lines[3].AL_Desc);
			AssertEquals(TestLicenceHeader.Company.Header.OH_Code + " Server " + TestLicenceHeader.Database.LD_ServerCode, invoice.Lines[4].AL_Desc);
			AssertEquals("Original currency amount 3,300.00 USD", invoice.Lines[5].AL_Desc);
			AssertEquals("Core (11)", invoice.Lines[6].AL_Desc);
		}

		public void TestPopulateInvoice_UpdateModuleRenewal()
		{
			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);
			TestLicenceHeader.Billing.L0_NextMaintenancePercent = 30m;
			TestLicenceHeader.Billing.L0_NextNewSeatMaintenancePercent = 30m;
			var prices = BillingTestHelper.CreateMaintenancePriceList(TestLicenceHeader);

			TestLicenceHeader.GetCoreModule().LM_UserCount = 11;
			prices.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			ARInvoice invoice = Factory.New<ARInvoice>();

			AssertEquals("pre", 0, TestLicenceHeader.GetCoreModule().LM_RenewalUserCount);
			AssertEquals("pre", (short)0, TestLicenceHeader.GetCoreModule().LM_PartPurchasedCount);
			ClientChargeableUsage[] usages = Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_LD, TestLicenceHeader.LA_LD));
			AssertEquals("pre", 0, usages.Length);
			AssertEquals(0m, bill.PriceOldSeats);
			AssertEquals(11000m, bill.PriceNewSeats);

			bill.PopulateInvoice(invoice);
			AssertEquals("renewal count", 11, TestLicenceHeader.GetCoreModule().LM_RenewalUserCount);
			AssertEquals("previous renewal count", (short)0, TestLicenceHeader.GetCoreModule().LM_PartPurchasedCount);
			usages = Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_LD, TestLicenceHeader.LA_LD));
			AssertEquals("usages created", 1, usages.Length);
			AssertEquals(new ZDateTime(2011, 1, 1), TestLicenceHeader.LA_ContractRenewalIssued);

			bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			AssertEquals("existing invoice found in new bill", invoice.PK, bill.Invoice.PK);
			AssertEquals("price old seats unchanged", 0m, bill.PriceOldSeats);
			AssertEquals("price new seats unchanged", 11000m, bill.PriceNewSeats);

			bill.PopulateInvoice(invoice);
			AssertEquals("renewal count", 11, TestLicenceHeader.GetCoreModule().LM_RenewalUserCount);
			AssertEquals("previous renewal count", (short)0, TestLicenceHeader.GetCoreModule().LM_PartPurchasedCount);
			ClientChargeableUsage[] usages2 = Factory.Load<ClientChargeableUsage>(new ZQuery(ClientChargeableUsageSchema.U1_LD, TestLicenceHeader.LA_LD));
			AssertEquals("usages reloaded", 1, usages2.Length);

			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2011, 12, 31);
			bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			AssertEquals("price old seats updated", 11000m, bill.PriceOldSeats);
			AssertEquals("price new seats updated", 0m, bill.PriceNewSeats);
			AssertNull("no invoice for new renewal period yet", bill.Invoice);
			ARInvoice invoice2 = Factory.New<ARInvoice>();
			bill.PopulateInvoice(invoice2);
			AssertEquals("renewal count", 11, TestLicenceHeader.GetCoreModule().LM_RenewalUserCount);
			AssertEquals("previous renewal count", (short)11, TestLicenceHeader.GetCoreModule().LM_PartPurchasedCount);
		}

		public void TestPopulateInvoice_SalesTax()
		{
			var salesTax = BillingTestHelper.CreateChargeCode(Factory, null, "SALESTAX");
			salesTax.AC_Desc = "Sales Tax 9.5%";
			Factory.Save();

			CodeDescriptionPairList salesTaxRates = new CodeDescriptionPairList();
			salesTaxRates.AddPair(salesTax.AC_Code, "9.5");
			EDIDataRegistry.Instance.SalesTaxRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, salesTaxRates);

			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);
			TestLicenceHeader.Billing.L0_NextMaintenancePercent = 30m;
			TestLicenceHeader.Billing.L0_NextNewSeatMaintenancePercent = 30m;
			TestLicenceHeader.Company.InvoiceDeliveries[0].L9_AC_SalesTaxChargeCode = salesTax.PK;
			var prices = BillingTestHelper.CreateMaintenancePriceList(TestLicenceHeader);

			TestLicenceHeader.GetCoreModule().LM_UserCount = 11;
			prices.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient, TestLicenceHeader.Company.InvoiceDeliveries[0]);
			ARInvoice invoice = Factory.New<ARInvoice>();
			bill.PopulateInvoice(invoice);

			CombineAssertions(() =>
			{
				AssertEquals("lines", 7, invoice.Lines.Count);

				AssertEquals("AH_OSExTaxAmount", new ZDecimal(11 * 1000m * 0.3m * 1.095m), invoice.AH_OSExTaxAmount);
				AssertEquals("Line 0: AL_OSExTaxAmount", new ZDecimal(11 * 1000m * 0.3m), invoice.Lines[0].AL_OSExTaxAmount);
				AssertEquals("Line 6: AL_OSExTaxAmount", new ZDecimal(11 * 1000m * 0.3m * 0.095m), invoice.Lines[6].AL_OSExTaxAmount);
				AssertEquals("AL_Desc", "ediEnterprise Application Services Renewal", invoice.Lines[0].AL_Desc);
				AssertEquals("ChargeCode", "ANNMAINT", invoice.Lines[0].GenericChargeBizO.VC_Code);
				AssertEquals("Branch", Env.CurrentBranch.PK, invoice.Lines[0].AL_GB);
				AssertEquals("AL_LineType", Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue, invoice.Lines[0].AL_LineType);

				AssertEquals("Line1", "1 January 2011 to 31 December 2011", invoice.Lines[1].AL_Desc);
				AssertEquals("Line2", "----------------------------------", invoice.Lines[2].AL_Desc);
				AssertEquals("line3", TestLicenceHeader.Company.Header.OH_FullName, invoice.Lines[3].AL_Desc);
				AssertEquals("Line4", TestLicenceHeader.Company.Header.OH_Code + " Server " + TestLicenceHeader.Database.LD_ServerCode, invoice.Lines[4].AL_Desc);
				AssertEquals("Line5", "Core (11)", invoice.Lines[5].AL_Desc);
				AssertEquals("Line6", "Sales Tax 9.5% (SALESTAX)", invoice.Lines[6].AL_Desc);
			});
		}

		[TestDate(2011, 6, 1)]
		public void TestCalculateDueDate()
		{
			TestLicenceHeader.LA_ContractExpiryDate = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2011, 6, 30), MaintenanceBill.CalculateDueDate(TestLicenceHeader));

			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2011, 4, 30);
			Assert("Precondition", TestLicenceHeader.LA_ContractExpiryDate < TestDateAttribute.Date);
			AssertEquals("date should be expiry date + 1, even if it is in the past, for correct grouping by date",
				new ZDateTime(2011, 5, 1), MaintenanceBill.CalculateDueDate(TestLicenceHeader));
		}

		public void TestOldSeatPercentIncrease()
		{
			TestLicenceHeader.Billing.L0_LastNewSeatMaintenancePercent = 0m;
			TestLicenceHeader.Billing.L0_NextNewSeatMaintenancePercent = 0m;
			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);

			TestLicenceHeader.Billing.L0_LastMaintenancePercent = 10m;
			TestLicenceHeader.Billing.L0_NextMaintenancePercent = 18m;
			AssertEquals(80m, bill.OldSeatPercentIncrease);

			TestLicenceHeader.Billing.L0_LastMaintenancePercent = 0m;
			AssertEquals(0m, bill.OldSeatPercentIncrease);

			TestLicenceHeader.Billing.L0_LastMaintenancePercent = 20m;
			TestLicenceHeader.Billing.L0_NextMaintenancePercent = 21m;
			AssertEquals(5m, bill.OldSeatPercentIncrease);
		}

		public void TestNewSeatPercentIncrease()
		{
			TestLicenceHeader.Billing.L0_LastMaintenancePercent = 0m;
			TestLicenceHeader.Billing.L0_NextMaintenancePercent = 0m;
			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);

			TestLicenceHeader.Billing.L0_LastNewSeatMaintenancePercent = 10m;
			TestLicenceHeader.Billing.L0_NextNewSeatMaintenancePercent = 18m;
			AssertEquals(80m, bill.NewSeatPercentIncrease);

			TestLicenceHeader.Billing.L0_LastNewSeatMaintenancePercent = 0m;
			AssertEquals(0m, bill.NewSeatPercentIncrease);

			TestLicenceHeader.Billing.L0_LastNewSeatMaintenancePercent = 20m;
			TestLicenceHeader.Billing.L0_NextNewSeatMaintenancePercent = 21m;
			AssertEquals(5m, bill.NewSeatPercentIncrease);
		}

		public void TestAllSeatPercentIncrease()
		{
			TestLicenceHeader.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);
			TestLicenceHeader.Billing.L0_LastMaintenancePercent = 10m;
			TestLicenceHeader.Billing.L0_LastNewSeatMaintenancePercent = 20m;
			TestLicenceHeader.Billing.L0_NextMaintenancePercent = 18m;
			TestLicenceHeader.Billing.L0_NextNewSeatMaintenancePercent = 25m;
			var prices = BillingTestHelper.CreateMaintenancePriceList(TestLicenceHeader);

			TestLicenceHeader.GetCoreModule().LM_UserCount = 11;
			TestLicenceHeader.GetCoreModule().LM_RenewalUserCount = 7;
			prices.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenanceBill bill = new MaintenanceBill(TestLicenceHeader, TestBillRecipient);
			ZDecimal from = 7 * 1000 * 0.1m + 4 * 1000 * 0.2m;
			ZDecimal to = 7 * 1000 * 0.18m + 4 * 1000 * 0.25m;
			AssertEquals((to * 100 / from) - 100m, bill.AllSeatPercentIncrease);
			AssertEquals(false, bill.IsFixedMaintenance);
		}

		public void TestFixedMaintenance()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "EEE");
			MaintenanceBillRecipient recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Now);
			licHeader.Billing.L0_FixedMaintenanceAmount = 1232;

			var priceList = BillingTestHelper.CreateMaintenancePriceList(licHeader);
			licHeader.GetCoreModule().LM_UserCount = 11;
			priceList.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenanceBill bill = new MaintenanceBill(licHeader, recipient);

			AssertEquals(true, bill.IsFixedMaintenance);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			LicenceHeader lic = Factory.NewWithValidTestData<LicenceHeader>();
			MaintenanceBillRecipient billRecipient = NewRecipient(lic);
			lic.Billing.L0_LastMaintenancePercent = 30m;
			return new MaintenanceBill(lic, billRecipient);
		}

		MaintenanceBillRecipient NewRecipient(LicenceHeader lic)
		{
			return new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, lic.Company.LC_OH, "AUD", ZDateTime.Now);
		}

		LicenceHeader TestLicenceHeader;
		MaintenanceBillRecipient TestBillRecipient;

		protected override void SetUp()
		{
			base.SetUp();

			BillingTestHelper.CreateChargeCode(Factory, null, "ANNMAINT");
			var chargeCode = BillingTestHelper.CreateChargeCodeForBranch(Factory, GlbBranch.CurrentBranch, null, EDIDataRegistry.Instance.CommentChargeCode.Value);
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();

			TestLicenceHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "EEE");
			TestLicenceHeader.Billing.L0_RenewalMonths = 12;
			TestBillRecipient = NewRecipient(TestLicenceHeader);
		}

		#endregion
	}

	internal class MaintenanceBillForTest : MaintenanceBill
	{
		public MaintenanceBillForTest(LicenceHeader licHeader, MaintenanceBillRecipient billRecipient, ClientInvoiceDelivery invoiceDelivery)
			: base(licHeader, billRecipient, invoiceDelivery)
		{
		}

		public override void PopulateInvoice(ARInvoice invoice)
		{
			PopulateInvoiceCalled++;
			base.PopulateInvoice(invoice);
		}

		public int PopulateInvoiceCalled;
	}
}
