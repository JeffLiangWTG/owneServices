using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Fee.Test
{
	[TestedType(typeof(FeeBill))]
	internal class FeeBillTest : SystemBillTestCase<FeeBill>
	{
		public void TestChargeCodes()
		{
			FeeBill feeBill = new FeeBill(Factory);
			AssertExceptionThrown<System.InvalidOperationException>("GetAmountChargeCodeName", () => feeBill.GetAmountChargeCodeName(null));
			AssertExceptionThrown<System.InvalidOperationException>("GetDiscountChargeCodeName", () => feeBill.GetDiscountChargeCodeName(null));
		}

		public void TestOnInvoiceSaving()
		{
			RunInvoiceSavingTest(false);
			RunInvoiceSavingTest(true);
		}

		void RunInvoiceSavingTest(bool testWithRemitUsage)
		{
			FeeSystemUsage feeUsage1 = FeeSystemUsageTest.CreateFeeUsage(Factory, Organisation, BillingDate, false);
			FeeSystemUsage feeUsage2 = FeeSystemUsageTest.CreateFeeUsage(Factory, ChildOrganisation1, BillingDate, false);
			FeeSystemUsage feeUsage3 = FeeSystemUsageTest.CreateFeeUsage(Factory, ChildOrganisation2, BillingDate, false);

			FeeBill feeBill = new FeeBill(Factory);
			feeBill.PopulateFromSystemUsages(new SystemUsage[] { feeUsage1, feeUsage2, feeUsage3 });

			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();

			ZQuery chargeableUsageQuery = new ZQuery(ClientChargeableUsageSchema.U1_Code, feeBill.SystemCode);
			ClientChargeableUsage[] chargeableUsages = Factory.Load<ClientChargeableUsage>(chargeableUsageQuery);
			AssertEquals("Precondition: no chargeable usages", 0, chargeableUsages.Length);

			feeBill.OnInvoiceFactorySaving(invoice);
			invoiceFactory.Save();

			chargeableUsages = Factory.Load<ClientChargeableUsage>(chargeableUsageQuery);
			AssertEquals("Chargeable usages created", 5, chargeableUsages.Length);
			AssertChargeableUsage(chargeableUsages, feeUsage1, invoice);
			AssertChargeableUsage(chargeableUsages, feeUsage2, invoice);
			AssertChargeableUsage(chargeableUsages, feeUsage3, invoice);

			feeUsage1.ChargeableUsagePKs.AddRange(chargeableUsages.Where(x => x.U1_LC == Organisation.LicCompany.PK).Select(s => s.PK));
			feeUsage2.ChargeableUsagePKs.AddRange(chargeableUsages.Where(x => x.U1_LC == ChildOrganisation1.LicCompany.PK).Select(s => s.PK));
			feeUsage3.ChargeableUsagePKs.AddRange(chargeableUsages.Where(x => x.U1_LC == ChildOrganisation2.LicCompany.PK).Select(s => s.PK));
			feeBill = new FeeBill(Factory);
			feeBill.PopulateFromSystemUsages(new SystemUsage[] { feeUsage1, feeUsage2, feeUsage3 });
			invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice2 = invoiceFactory.NewWithValidTestData<ARInvoice>();
			feeBill.OnInvoiceFactorySaving(invoice2);
			invoiceFactory.Save();
			chargeableUsages = Factory.Load<ClientChargeableUsage>(chargeableUsageQuery);
			AssertEquals("No more usages created", 5, chargeableUsages.Length);
			AssertChargeableUsage(chargeableUsages, feeUsage1, invoice2);
			AssertChargeableUsage(chargeableUsages, feeUsage2, invoice2);
			AssertChargeableUsage(chargeableUsages, feeUsage3, invoice2);

			foreach (var chargeableUsage in chargeableUsages)
			{
				chargeableUsage.Delete();
			}
			Factory.Save();
		}

		void AssertChargeableUsage(ClientChargeableUsage[] chargeableUsages, FeeSystemUsage systemUsage, ARInvoice invoice)
		{
			foreach (ClientLicenceFee fee in systemUsage.Fees)
			{
				ClientChargeableUsage chargeableUsage = chargeableUsages.First(s => s.U1_Parent == fee.PK);
				AssertEquals(BillingConstants.BillingSystem.Fee, chargeableUsage.U1_Code);
				AssertEquals(systemUsage.PeriodStart, chargeableUsage.U1_PeriodStart);
				AssertEquals(fee.L8_Amount, chargeableUsage.U1_UnitPrice);
				AssertEquals(invoice.PK, chargeableUsage.U1_AH_Invoice);
				AssertEquals(fee.L8_LC, chargeableUsage.U1_LC);
			}
		}

		public void TestAddAmountLine()
		{
			BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "ESV", "Quarterly subscription", 75m, "AA1CODE", BillingDate.AddMonths(-3), ZDateTime.Empty)
				.L8_RenewalMonths = 3;
			Factory.Save();

			FeeSystemUsage feeUsage1 = FeeSystemUsageTest.CreateFeeUsage(Factory, Organisation, BillingDate, false);
			FeeSystemUsage feeUsage2 = FeeSystemUsageTest.CreateFeeUsage(Factory, ChildOrganisation1, BillingDate, false);
			FeeSystemUsage feeUsage3 = FeeSystemUsageTest.CreateFeeUsage(Factory, ChildOrganisation2, BillingDate, false);

			FeeBill feeBill = new FeeBill(Factory);
			feeBill.PopulateFromSystemUsages(new SystemUsage[] { feeUsage1, feeUsage2, feeUsage3 });

			List<SystemBill.BillLine> invoiceLines = new List<SystemBill.BillLine>();
			feeBill.CreateInvoiceLines(invoiceLines, ZDateTime.Now);

			AssertEquals("Amount lines for fees", 6, invoiceLines.Count);
			AssertAmountLine(invoiceLines[0], 10m, "AA1CODE", "AA1 fee description\r\nDec 2010");
			AssertAmountLine(invoiceLines[1], 11m, "GENCODE", "GEN fee description\r\nDec 2010");
			AssertAmountLine(invoiceLines[2], 75m, "AA1CODE", "Quarterly subscription\r\nDec 2010 to Feb 2011");
			AssertAmountLine(invoiceLines[3], 20m, "BB1CODE", "BB1 fee description\r\nDec 2010");
			AssertAmountLine(invoiceLines[4], 22m, "BB2CODE", "BB2 fee description\r\nDec 2010");
			AssertAmountLine(invoiceLines[5], 30m, "GENCODE", "GEN fee description\r\nDec 2010");
		}

		public void TestAddAmountLine_IsProcessingFeeExempt()
		{
			var feeNoProcessing = BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "1ST", "Quarterly subscription", 200m, "AA1CODE", BillingDate.AddMonths(-3), ZDateTime.Empty)
				.L8_RenewalMonths = 3;
			Factory.Save();

			FeeSystemUsage feeUsage1 = FeeSystemUsageTest.CreateFeeUsage(Factory, Organisation, BillingDate, false);

			FeeBill feeBill = new FeeBill(Factory);
			feeBill.PopulateFromSystemUsages(new SystemUsage[] { feeUsage1 });

			List<SystemBill.BillLine> invoiceLines = new List<SystemBill.BillLine>();
			feeBill.CreateInvoiceLines(invoiceLines, ZDateTime.Now);

			AssertEquals("Amount lines for fees", 3, invoiceLines.Count);
			AssertEquals(false, invoiceLines[0].IsProcessingFeeExempt);
			AssertEquals(false, invoiceLines[1].IsProcessingFeeExempt);
			AssertEquals(true, invoiceLines[2].IsProcessingFeeExempt);
		}

		public void TestAddAmountLine_TaxDate()
		{
			var fee = BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "1ST", "Quarterly subscription", 200m, "AA1CODE", BillingDate.AddMonths(-3), ZDateTime.Empty);
			fee.L8_RenewalMonths = 3;
			fee.L8_TaxDateCode = BillingConstants.Fee.TaxDateCode.FeeTaxAtEndDate;
			Factory.Save();

			FeeSystemUsage feeUsage1 = FeeSystemUsageTest.CreateFeeUsage(Factory, Organisation, BillingDate, false);

			FeeBill feeBill = new FeeBill(Factory);
			feeBill.PopulateFromSystemUsages(new SystemUsage[] { feeUsage1 });

			List<SystemBill.BillLine> invoiceLines = new List<SystemBill.BillLine>();
			feeBill.CreateInvoiceLines(invoiceLines, ZDateTime.Now);

			AssertEquals("Amount lines for fees", 3, invoiceLines.Count);
			AssertEquals("when FeeTaxAtEndDate", BillingDate.AddMonths(3).AddDays(-1), invoiceLines[2].TaxDate);
		}

		public void TestCreateRevenueBreakdown()
		{
			BillingTestHelper.CreateChargeCodesForBranch(Factory, Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch);

			var licCompany = Organisation.LicCompany;
			var feeNoProcessing = BillingTestHelper.CreateLicenceFee(licCompany, "1ST", "Quarterly subscription", 200m, "AA1CODE", BillingDate, ZDateTime.Empty);
			var feeUSD = BillingTestHelper.CreateLicenceFee(licCompany, "ESV", "Some fee", 77m, "AA1CODE", BillingDate, ZDateTime.Empty, "USD");
			BillingTestHelper.CreateExchangeRate(Factory, "USD", 0.5m);
			Factory.Save();

			var orgFees = licCompany.Fees;
			var feesAUD = orgFees.GetMatched(BillingDate).Where(s => s.L8_SystemCode == BillingConstants.BillingSystem.ODM && s.L8_RX_NKCurrency == "AUD").ToArray();
			var feesUSD = orgFees.GetMatched(BillingDate).Where(s => s.L8_SystemCode == BillingConstants.BillingSystem.ODM && s.L8_RX_NKCurrency == "USD").ToArray();

			var feeUsageAUD = new FeeSystemUsage(Factory, new UsingParty(Organisation), BillingDate, feesAUD, false);
			var feeUsageUSD = new FeeSystemUsage(Factory, new UsingParty(Organisation), BillingDate, feesUSD, false);

			FeeBill feeBill = new FeeBill(Factory);
			feeBill.PopulateFromSystemUsages(new SystemUsage[] { feeUsageAUD, feeUsageUSD });

			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = Organisation.PK;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			invoice.AH_RX_NKTransactionCurrency = "AUD";
			invoice.AH_GB = Env.CurrentBranchPK;
			invoice.AH_GC = Env.CurrentCompanyPK;
			invoice.AH_ExchangeRate = 1m;

			// Fees
			// AA1,  10, AUD
			// GEN,  11, AUD
			// 1ST, 200, AUD - no processing fee
			// ESV,  77, USD - AUD 144
			// 10+11+77 = 98

			feeBill.CreateRevenueBreakdown(invoice, -10m);

			var allBilledUsage = Factory.Load<EdiBilledUsage>(new ZQuery());
			AssertEquals(4, allBilledUsage.Length);
			AssertBilledUsage(allBilledUsage, invoice, feeUSD, feeBill, -10m);
			AssertBilledUsage(allBilledUsage, invoice, feeNoProcessing, feeBill, 0);
			AssertBilledUsage(allBilledUsage, invoice, feesAUD.Single(x => x.L8_Type == "AA1"), feeBill, -10m);
			AssertBilledUsage(allBilledUsage, invoice, feesAUD.Single(x => x.L8_Type == "GEN"), feeBill, -10m);
		}

		void AssertBilledUsage(EdiBilledUsage[] allBilledUsage, ARInvoice invoice, ClientLicenceFee fee, FeeBill bill, decimal signedProcessingFeePercent)
		{
			var dateForExchangeRate = ZDateTime.Now;

			var billed = allBilledUsage.Single(x => x.BU9_UsageSubCode == fee.L8_Type && x.BU9_UnitPrice == fee.L8_Amount);
			AssertEquals(BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, fee.L8_ChargeCode), billed.BU9_AC_AmountChargeCode);
			AssertEquals(invoice.PK, billed.BU9_AH_Invoice);
			AssertEquals(BillingConstants.PriceHeaderType.ODM, billed.BU9_BillingModel);
			AssertEquals(fee.L8_LC, billed.BU9_LC);
			AssertEquals(ZGuid.Empty, billed.BU9_LCC);
			AssertEquals(fee.L8_LD, billed.BU9_LD);
			var processing = fee.L8_Amount * signedProcessingFeePercent / 100m;
			var postDiscount = fee.L8_Amount + processing;
			var expectedLocalPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(fee.L8_Amount, dateForExchangeRate, fee.L8_RX_NKCurrency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
			var expectedLocalPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, fee.L8_RX_NKCurrency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);
			var expectedLocalProcessing = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processing, dateForExchangeRate, fee.L8_RX_NKCurrency, invoice.Company.GC_RX_NKLocalCurrency, invoice.Branch, invoice.Factory);

			if (processing != 0)
			{
				AssertEquals(BillingInvoicingHelper.GetChargeCodePK(invoice.Branch, EDIDataRegistry.Instance.OdplDiscountChargeCode.Value), billed.BU9_AC_DiscountChargeCode);
			}
			else
			{
				AssertEquals(ZGuid.Empty, billed.BU9_AC_DiscountChargeCode);
			}

			AssertEquals(expectedLocalPreDiscount, billed.BU9_LocalAmountPreDiscount);
			AssertEquals(expectedLocalPostDiscount, billed.BU9_LocalAmountPostDiscount);
			AssertEquals(expectedLocalProcessing, billed.BU9_LocalProcessingAmount);

			AssertEquals(bill.PeriodStart, billed.BU9_PeriodStart);
			AssertEquals(fee.L8_RX_NKCurrency, billed.BU9_PriceCurrency);

			var expectedTransactionPreDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(fee.L8_Amount, dateForExchangeRate, fee.L8_RX_NKCurrency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
			var expectedTransactionPostDiscount = BillingInvoicingHelper.GetAmountInInvoiceCurrency(postDiscount, dateForExchangeRate, fee.L8_RX_NKCurrency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);
			var expectedTransactionProcessing = BillingInvoicingHelper.GetAmountInInvoiceCurrency(processing, dateForExchangeRate, fee.L8_RX_NKCurrency, invoice.AH_RX_NKTransactionCurrency, invoice.Branch, invoice.Factory);

			AssertEquals(expectedTransactionPreDiscount, billed.BU9_TransactionAmountPreDiscount);
			AssertEquals(expectedTransactionPostDiscount, billed.BU9_TransactionAmountPostDiscount);
			AssertEquals(expectedTransactionProcessing, billed.BU9_TransactionProcessingAmount);

			AssertEquals(1m, billed.BU9_UnitCount);
			AssertEquals("FEE", billed.BU9_UsageCode);
		}

		void AssertAmountLine(SystemBill.BillLine invoiceLine, ZDecimal amount, ZString amountChargeCodeName, ZString description)
		{
			AssertEquals(amount, invoiceLine.Amount);
			AssertEquals(amountChargeCodeName, invoiceLine.ChargeCodeName);
			AssertEquals(description, invoiceLine.Description);
		}

		#region Implementation

		readonly ZDateTime BillingDate = new ZDateTime(2010, 12, 1);

		EDIOrgHeader Organisation;
		EDIOrgHeader ChildOrganisation1;
		EDIOrgHeader ChildOrganisation2;

		protected override void SetUp()
		{
			base.SetUp();

			Organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			Organisation.LicCompany.SelfBilling.L4_ProcessingFee = "DDE";
			Organisation.LicCompany.SelfBilling.L4_ProcessingFeePercent = 10m;
			BillingTestHelper.SetInvoicing(Organisation, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "AA1", 10m);
			BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "GEN", 11m);
			BillingTestHelper.CreateLicenceFee(Organisation.LicCompany, "XXX", "XXX fee -- should be unmatched", 200m, "XXXCODE", BillingDate.AddMonths(1), ZDateTime.Empty);

			ChildOrganisation1 = BillingTestHelper.CreateDependentOrganisation(Organisation, "BBB");
			BillingTestHelper.SetInvoicing(ChildOrganisation1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreateLicenceFee(ChildOrganisation1.LicCompany, "BB1", 20m);
			BillingTestHelper.CreateLicenceFee(ChildOrganisation1.LicCompany, "BB2", 22m);
			BillingTestHelper.CreateLicenceFee(ChildOrganisation1.LicCompany, "XXX", "XXX fee -- should be unmatched", 200m, "XXXCODE", BillingDate.AddMonths(1), ZDateTime.Empty);

			ChildOrganisation2 = BillingTestHelper.CreateDependentOrganisation(Organisation, "CCC");
			BillingTestHelper.SetInvoicing(ChildOrganisation2, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.CreateLicenceFee(ChildOrganisation2.LicCompany, "GEN", 30m);
			BillingTestHelper.CreateLicenceFee(ChildOrganisation2.LicCompany, "XXX", "XXX fee -- should be unmatched", 200m, "XXXCODE", BillingDate.AddMonths(1), ZDateTime.Empty);

			BillingTestHelper.CreateChargeCode(Factory, null, "AA1CODE");
			BillingTestHelper.CreateChargeCode(Factory, null, "GENCODE");
			BillingTestHelper.CreateChargeCode(Factory, null, "BB1CODE");
			BillingTestHelper.CreateChargeCode(Factory, null, "BB2CODE");

			Factory.Save();
		}

		protected override FeeBill GetNewSystemBill()
		{
			return new FeeBill(Factory);
		}

		#endregion
	}
}
