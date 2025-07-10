using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestGetInvoicedDocumentaryAmountAndCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2m;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			AssertEquals(3m, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_InvoiceAmount);
			AssertEquals("EUR", declaration.CustomsEntryHeaders[0].MergedLines[0].CL_RX_NKInvoiceAmountCurrency);
		}

		public void TestConfirmedFeesReadOnly()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var confirmedFee = Factory.New<CusEntryLineFee>();
			entryLine.ConfirmedFees.Add(confirmedFee);

			AssertEquals("ConfirmedFees count", 1, entryLine.ConfirmedFeesReadOnly.Count);
			AssertEquals("ConfirmedFee", confirmedFee.PK, entryLine.ConfirmedFeesReadOnly[0].PK);
		}

		public void TestConfirmedFeesReadOnly_Cached()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var confirmedFee = Factory.New<CusEntryLineFee>();
			entryLine.ConfirmedFees.Add(confirmedFee);

			var confirmedFeesReadonly = entryLine.ConfirmedFeesReadOnly;
			AssertSame("ConfirmedFeesReadonly same object", confirmedFeesReadonly, entryLine.ConfirmedFeesReadOnly);
		}

		public void TestConfirmedFeesReadOnly_CacheUpdated()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var confirmedFee = Factory.New<CusEntryLineFee>();

			CombineAssertions(() =>
			{
				AssertEquals("ConfirmedFeesReadonly count 0", 0, entryLine.ConfirmedFeesReadOnly.Count);
				entryLine.ConfirmedFees.Add(confirmedFee);
				AssertEquals("ConfirmedFeesReadonly count 1", 1, entryLine.ConfirmedFeesReadOnly.Count);
			});
		}

		public void TestGetEntryLineVatCalculator()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var vatCalculator = entryLine.GetEntryLineVatCalculator();

			AssertNotNull("VAT Calculator should not be null", vatCalculator);
			AssertType<EntryLineVatCalculator>("VAT Calculator should be of type EntryLineVatCalculator", vatCalculator);
		}

		public void TestDutyDetailsForVAT()
		{
			var factory = Factory;
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusRateTypes.Dty, FeeTypeList.Codes.A00);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusRateTypes.Vat, Constants.EntryLineFee.VATFeeTypeCode);
			var jobDeclaration = factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(FeeTypeList.Codes.A00, 100m);
			entryLine.Fees.AddOrUpdate(Constants.EntryLineFee.VATFeeTypeCode, 250m);
			entryLine.Fees.AddOrUpdate("B05", 300m);

			var dutyDetailsForVAT = entryLine.DutyDetailsForVAT;
			AssertEquals("DutyDetailsForVAT should be the sum of fees included for VAT calculation", 350m, dutyDetailsForVAT);
		}

		public void TestTotalDutyAmount()
		{
			var factory = Factory;
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusRateTypes.Vat, Constants.EntryLineFee.VATFeeTypeCode);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);
			var jobDeclaration = factory.New<JobDeclaration>();
			var cusEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(FeeTypeList.Codes.A00, 100m);
			entryLine.Fees.AddOrUpdate(Constants.EntryLineFee.VATFeeTypeCode, 250m);
			entryLine.Fees.AddOrUpdate("B05", 300m);

			AssertEquals("Total Duty Amount", 350m, entryLine.DutyAmount);
			AssertEquals("GST VAT Amount", 250m, entryLine.GSTVATAmount);
		}

		public void TestCL_InvoiceAmount()
		{
			AssertEntity<CusEntryLine>()
			.HasProperty(x => x.CL_InvoiceAmount)
			.WithAttribute<DecimalPlacesAttribute>(x => x.DecimalPlaces == 2);
		}

		// to be overridden once the Tariff is setup for a new country
		protected override ZString ExpectedFallbackEntrylineDescription => "TARIFF_DESCRIPTION";

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);
	}
}

