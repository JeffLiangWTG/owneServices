using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;

namespace Enterprise.Customs.GB.CDS.Declaration.Testing
{
	sealed class CDSApplicationExtenderTest : TestCaseWithFactory
	{
		public void TestGetFeeCodeFromRateCodeCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);

			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "NIIMP";

			var rateCode = appExtender.GetFeeCodeFromRateCode(entryLine, "A00");
			AssertEquals("RateCode should be mapped via CDSFeeCodeFromRateCodeMapper", "A50", rateCode);
		}

		public void TestGetDefaultMethodOfPaymentValueCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.ZG_MethodOfPayment = "E";

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.CompensatoryInterestVat;
			AssertEquals("CDS, No Fiscal References and B10 Charge Type - Should fall back to invoice line ZG_MethodOfPayment", "E", appExtender.GetDefaultMethodOfPaymentValue(fee));

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.Vat;
			AssertEquals("CDS, No Fiscal References and B00 Charge Type - Should fall back to invoice line ZG_MethodOfPayment", "E", appExtender.GetDefaultMethodOfPaymentValue(fee));

			((CusEntryInstruction)invoiceLine.EntryInstruction).FiscalReferences.AddNew();
			AssertEquals("CDS, Has Fiscal References and B00 Charge Type - Should be Empty", ZString.Empty, appExtender.GetDefaultMethodOfPaymentValue(fee));

			fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.VatOnAdditionalDutiesForNorthernIreland;
			AssertEquals("CDS, Has Fiscal References and B05 Charge Type - Should be Empty", ZString.Empty, appExtender.GetDefaultMethodOfPaymentValue(fee));

			((CusEntryInstruction)invoiceLine.EntryInstruction).FiscalReferences.RemoveAndDeleteAll();
			AssertEquals("CDS, No Fiscal References and B05 Charge Type - Should fall back to invoice line ZG_MethodOfPayment", "E", appExtender.GetDefaultMethodOfPaymentValue(fee));
		}

		public void TestGetCDSChargeCodeCore()
		{
			var charge = Factory.New<InvoiceCharge>();

			charge.J7_ChargeType = UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge;
			charge.J7_IsDutiable = false;
			charge.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Value;
			charge.J7_IsIncludedInITOT = false;

			var result = appExtender.GetCDSChargeCode(charge);
			AssertEquals("AR", result);

			charge.J7_IsIncludedInITOT = true;
			result = appExtender.GetCDSChargeCode(charge);
			AssertEquals("BR", result);
		}

		public void TestNoAdditionalProcedureCodesAreE01orE02()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("NoAdditionalProcedureCodesAreE01orE02: with no codes", expected: true, JobComInvoiceLine.NoAdditionalProcedureCodesAreE01orE02(invoiceLine));

			_ = invoiceLine.AdditionalProcedureCodes.AddNew("000");
			AssertEquals("NoAdditionalProcedureCodesAreE01orE02: with 000 code", expected: true, JobComInvoiceLine.NoAdditionalProcedureCodesAreE01orE02(invoiceLine));

			var code2 = invoiceLine.AdditionalProcedureCodes.AddNew("E01");
			AssertEquals("NoAdditionalProcedureCodesAreE01orE02: with E01 code", expected: false, JobComInvoiceLine.NoAdditionalProcedureCodesAreE01orE02(invoiceLine));

			code2.CY_Code = "E02";
			AssertEquals("NoAdditionalProcedureCodesAreE01orE02: with E02 code", expected: false, JobComInvoiceLine.NoAdditionalProcedureCodesAreE01orE02(invoiceLine));

			code2.CY_Code = "E00";
			AssertEquals("NoAdditionalProcedureCodesAreE01orE02: with E00 code", expected: true, JobComInvoiceLine.NoAdditionalProcedureCodesAreE01orE02(invoiceLine));

			invoiceLine.JI_Procedure = "0100E01";
			AssertEquals("NoAdditionalProcedureCodesAreE01orE02: Procedure 0100E01", expected: false, JobComInvoiceLine.NoAdditionalProcedureCodesAreE01orE02(invoiceLine));
		}

		protected override void SetUp()
		{
			appExtender = ApplicationExtender.New(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);
			AssertType<CDSApplicationExtender>("Pre-Condition Expecting CDS Extender", appExtender);
		}

		ApplicationExtender appExtender;
	}
}
