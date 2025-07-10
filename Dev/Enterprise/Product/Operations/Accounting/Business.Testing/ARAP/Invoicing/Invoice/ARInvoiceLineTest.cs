using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(ARInvoiceLine))]
	public class ARInvoiceLineTest : InvoiceLineTest
	{
		public void TestChargeCodeChangeDoesntChangeOtherFields()
		{
			TestChargeCodeChangeDoesntChangeOtherFields(tpa => TransactionAllocationConverter.ConvertUnallocatedToAR(tpa).Invoice);
		}

		public override void TestAL_JHValidationOnAL_RevRecognitionType()
		{
			Assert("Test is not applicable as revenue recognition validation is not run for this line type", true);
		}

		public override void TestIsStampDutyChargeLine()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Italy);

			var taxRate = AccTaxRate.FindExistingTaxRate(Factory, "ART7", AccTaxRate.Types.Rated, Core.Constants.CountryCodes.Italy);

			var taxRateExempt = AccTaxRate.FindExistingTaxRate(Factory, "EXEMPT", AccTaxRate.Types.Exempt, Core.Constants.CountryCodes.Italy);

			Factory.Save();

			AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxRate.PK.ToString());
			AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1.81m);
			AccountingConfigurationRegistry.Instance.StampDutyThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 77.47m);

			var stampDutyChargeCode = TestObjectCreator.CreateChargeCode("BOLLO", "Stamp Duty", "MRG", 1m, taxRateExempt, TestObjectCreator.WHTFREE1);
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyChargeCode.PK.ToGuid());

			var italyOrg = TestObjectCreator.CreateOrgHeader("ITORG", false, true, "ITROM");

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = italyOrg.PK;
			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1M, 100M, 0M, 0M, 100M, 0M, 0M, stampDutyChargeCode.PK);
			invoiceLine.AL_AT = taxRate.PK;

			AssertEquals(1, invoice.Lines.Count);
			AssertEquals("IsStampDutyChargeLine flag should be false for the this line", false, invoice.Lines[0].IsStampDutyChargeLine());

			Assert("Precondition: Italian stamp duty should be applicable", invoice.ShouldAddStampDuty());

			Factory.Save();

			AssertEquals("stamp duty line created.", 2, invoice.Lines.Count);

			AssertEquals("IsStampDutyChargeLine flag should remain false for the first line", false, invoice.Lines[0].IsStampDutyChargeLine());
			AssertEquals("IsStampDutyChargeLine flag should be set to true for the stamp duty line", true, invoice.Lines[1].IsStampDutyChargeLine());
		}

		[ExpectNoExceptions]
		public void TestCopyFromCreditNoteLine()
		{
			ARInvoiceLine aRInvoiceLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			ARCreditNoteLine aRCreditNoteLine = Factory.NewWithValidTestData<ARCreditNoteLine>();
			aRInvoiceLine.CopyFromCreditNoteLine(aRCreditNoteLine);
		}

		public override void TestTaxRateOverrideWorksForAPInvoices()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestSettingJobSetsFallbackTaxRate()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestSetSupplyTypeTriggerSetTaxRate()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestSetBranchTriggerSetTaxRate()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestChargeCodeTaxRateOverrideWithCustomsStatus()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestChargeCodeTaxRateOverrideAndChargeCodeTaxOverride()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestChargeCodeTaxRateOverride_PlaceOfSupply()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		public override void TestFallbackTaxRate()
		{
			Assert("Test is not applicable for job related AR invoices because tax calculation and invoice posting is done from Job's Billing form", true);
		}

		#region TEST: OSTaxAmount and OSWHTAmount Fields Calculated Based On AH_OSExTaxAmount

		public void TestOSTaxAmountAndOSWHTAmountFieldsCalculatedBasedOnAH_OSExTaxAmount()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();

			AALSHI.CompanyData.SetARTaxApplicable(ZBool.True);
			AALSHI.MiscServ.OM_ARWHTApplicable = ZBool.True;

			invoice.AH_OH = AALSHI.PK;

			invoice.AH_OH = this.AALSHI.PK;
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			line.AL_AC = MRG100.PK;

			AssertEquals("OS Ex Tax Amt", 0.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 0.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 0.00M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 0.00M, line.AL_LocalTaxAmount);

			AssertEquals("Line Amount", 0.00M, line.AL_LineAmount);
			AssertEquals("GST Amount", 0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 0.00M, line.AL_OSAmount);

			line.AL_OSExTaxAmount = 100M;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 16.67M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 8.33M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 183.34M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 16.67M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 8.33M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 110.00M, line.AL_OSAmount);

			line.AL_OSExTaxAmount = 200M;
			AssertEquals("OS Ex Tax Amt", 200.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 20.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 10.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 220.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 333.33M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 33.33M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 16.67M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 366.66M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 333.33M, line.AL_LineAmount);
			AssertEquals("GST Amount", 33.33M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 16.67M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 220.00M, line.AL_OSAmount);
		}

		#endregion

		#region TEST: LocalTaxAmount and LocalWHTAmount Fields Calculated Based On Exchange Rate

		public void TestLocalTaxAmountAndLocalWHTAmountFieldsCalculatedBasedOnExchangeRate()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();

			AALSHI.CompanyData.SetARTaxApplicable(ZBool.True);
			AALSHI.MiscServ.OM_ARWHTApplicable = ZBool.True;

			invoice.AH_OH = AALSHI.PK;
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();

			line.AL_AC = MRG100.PK;
			line.AL_OSExTaxAmount = 100M;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 100.00M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 10.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 5.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 110.00M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 100.00M, line.AL_LineAmount);
			AssertEquals("GST Amount", 10.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 5.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 110.00M, line.AL_OSAmount);

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 16.67M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 8.33M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 183.34M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 16.67M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 8.33M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 110.00M, line.AL_OSAmount);

			line.AL_ExchangeRate = .5M;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 200.00M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 20.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 10.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 220.00M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 200.00M, line.AL_LineAmount);
			AssertEquals("GST Amount", 20.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 10.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 110.00M, line.AL_OSAmount);
		}

		#endregion

		#region TEST: OSTaxAmount and OSWHTAmount Fields Calculated Based On ChargeCode

		public void TestOSTaxAmountAndOSWHTAmountFieldsCalculatedBasedOnChargeCode()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			AALSHI.CompanyData.SetARTaxApplicable(ZBool.True);
			AALSHI.MiscServ.OM_ARWHTApplicable = ZBool.True;
			invoice.AH_OH = AALSHI.PK;
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			line.AL_OSExTaxAmount = 100M;

			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 100.00M, line.AL_OSAmount);

			line.AL_AC = MRG100.PK;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 16.67M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 8.33M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 183.34M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 16.67M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 8.33M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 110.00M, line.AL_OSAmount);

			line.AL_AC = MRG100_1.PK;

			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 5.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 10.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 105.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 8.33M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 16.67M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 175.00M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 8.33M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 16.67M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 105.00M, line.AL_OSAmount);
		}

		#endregion

		#region TEST: OSTaxAmount Calculated Based On TaxRate

		public void TestOSTaxAmountCalculatedBasedOnTaxRate()
		{
			ARInvoice invoice = Factory.New<ARInvoice>();
			AALSHI.CompanyData.SetARTaxApplicable(true);
			invoice.AH_OH = AALSHI.PK;
			ARInvoiceLine line = (ARInvoiceLine)invoice.Lines.AddNew();

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			line.AL_OSExTaxAmount = 100M;

			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 100.00M, line.AL_OSAmount);

			line.AL_AT = GST1.PK;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 10.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 110.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 16.67M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 183.34M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 16.67M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 110.00M, line.AL_OSAmount);

			line.AL_AT = ZGuid.Empty;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", 166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", 0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", 100.00M, line.AL_OSAmount);
		}

		#endregion

		#region TEST: OSWHTAmount Calculated Based On Withholding

		public void TestOSWHTAmountCalculatedBasedOnWithholding()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			AALSHI.CompanyData.SetARTaxApplicable(true);
			invoice.AH_OH = AALSHI.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();

			line.AL_RX_NKTransactionCurrency = USD.RX_Code;
			line.AL_ExchangeRate = .6M;
			line.AL_OSExTaxAmount = 100M;

			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);

			line.AL_AW = WHT1.PK;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 5.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);

			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 8.33M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", 0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -8.33M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);

			line.AL_AW = ZGuid.Empty;
			AssertEquals("OS Ex Tax Amt", 100.00M, line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount", 0.00M, line.AL_OSTaxAmount);
			AssertEquals("OS WHT Amount", 0.00M, line.AL_OSWHTAmount);
			AssertEquals("OS Total Amt ", 100.00M, line.AL_OverseasTotal);
			AssertEquals("Local Ex Tax Amount", 166.67M, line.AL_LocalExTaxAmount);
			AssertEquals("Local Tax Amount   ", 0.00M, line.AL_LocalTaxAmount);
			AssertEquals("Local WHT Amount   ", 0.00M, line.AL_LocalWHTAmount);
			AssertEquals("Local Total Amount ", 166.67M, line.AL_LocalTotalAmount);

			AssertEquals("Line Amount", -166.67M, line.AL_LineAmount);
			AssertEquals("GST Amount", -0.00M, line.AL_GSTVAT);
			AssertEquals("WHT Amount", -0.00M, line.AL_WithholdingTax);
			AssertEquals("OS Total", -100.00M, line.AL_OSAmount);
		}

		#endregion

		#region Implementation

		protected override Type MasterHeaderType
		{
			get { return typeof(ARInvoice); }
		}

		#endregion
	}
}
