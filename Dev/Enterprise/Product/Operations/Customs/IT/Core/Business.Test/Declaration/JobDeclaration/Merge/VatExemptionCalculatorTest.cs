using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(VatExemptionCalculator))]
sealed class VatExemptionCalculatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new VatExemptionCalculator(null));
		AssertNoExceptionThrown(() => new VatExemptionCalculator(entryLine));
	}

	public void TestRateCode()
	{
		AssertEquals(UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406, vatExemptionCalculator.RateCode);
	}

	public void TestCalculateExtraFees_VatExemptionNotCalculated()
	{
		CombineAssertions("B00 not found ", () =>
		{
			var vatFee = entryLine.Fees.GetElementWithThisCode(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
			AssertNull("PRE-CONDITION", vatFee);
			var intermediateResults = vatExemptionCalculator.CalculateExtraFees();
			AssertEquals("Result", 0, intermediateResults.Count());
		});

		CombineAssertions("B00 of type 'ADD' found", () =>
		{
			var vatFee = entryLine.Fees.AddNew();
			vatFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			vatFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			var intermediateResults = vatExemptionCalculator.CalculateExtraFees();
			AssertEquals("Result", 0, intermediateResults.Count());
		});
	}

	public void TestCalculateExtraFees_VatExemptionCalculated()
	{
		invoiceLine.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;

		var vatFee = entryLine.Fees.AddNew();
		vatFee.CF_BaseValue = 4000m;
		vatFee.CF_ChargeAmount = 123.456m;
		vatFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		vatFee.CF_MethodOfCalculation = "%";
		vatFee.CF_MethodOfPayment = "A";
		vatFee.CF_Rate = 22m;
		vatFee.CF_RateOverrideReasonCode = ZString.Empty;

		var vatFeeOvr = entryLine.Fees.AddNew();
		vatFeeOvr.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		vatFeeOvr.CF_BaseValue = 9999m;

		var intermediateResults = vatExemptionCalculator.CalculateExtraFees();
		AssertVatExemptionFees(intermediateResults, new FeeAssertionObject()
		{
			BaseValue = -vatFee.CF_BaseValue,
			ChargeAmount = -vatFee.CF_ChargeAmount,
			MethodOfCalculation = vatFee.CF_MethodOfCalculation,
			MethodOfPayment = vatFee.CF_MethodOfPayment,
			Rate = vatFee.CF_Rate / 100,
		});

		vatFee.CF_MethodOfCalculation = "XXX";
		intermediateResults = vatExemptionCalculator.CalculateExtraFees();
		AssertVatExemptionFees(intermediateResults, new FeeAssertionObject()
		{
			BaseValue = -vatFee.CF_BaseValue,
			ChargeAmount = -vatFee.CF_ChargeAmount,
			MethodOfCalculation = vatFee.CF_MethodOfCalculation,
			MethodOfPayment = vatFee.CF_MethodOfPayment,
			Rate = vatFee.CF_Rate,
		});
	}

	void AssertVatExemptionFees(IEnumerable<IDutyCalculationIntermediateResult> intermediateResults, FeeAssertionObject feeAssertionObject)
	{
		AssertEquals("Expected cound", 1, intermediateResults.Count());
		var vatExemptionFee = intermediateResults.Single();
		CombineAssertions("Assert VAT Exemption Fee Properties", () =>
		{
			AssertEquals("BaseValue", feeAssertionObject.BaseValue, vatExemptionFee.BaseValue);
			AssertEquals("Amount", feeAssertionObject.ChargeAmount, vatExemptionFee.Amount);
			AssertEquals("MethodOfCalculation", feeAssertionObject.MethodOfCalculation, vatExemptionFee.MethodOfCalculation);
			AssertEquals("MethodOfPayment", feeAssertionObject.MethodOfPayment, vatExemptionFee.MethodOfPayment);
			AssertEquals("Rate", feeAssertionObject.Rate, vatExemptionFee.Rate);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		vatExemptionCalculator = new VatExemptionCalculator(entryLine);
	}

	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	VatExemptionCalculator vatExemptionCalculator;
}
