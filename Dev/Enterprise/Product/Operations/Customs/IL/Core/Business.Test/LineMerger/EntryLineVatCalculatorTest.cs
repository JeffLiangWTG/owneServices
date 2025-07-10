using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class EntryLineVatCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateVatFee()
		{
			var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);
			var invoiceLine = entryLine.Declaration.InvoiceLines[0];
			invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			entryLine.InvoiceLines.Add(invoiceLine);

			var vatCalculator = entryLine.GetEntryLineVatCalculator();

			VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "ADD", true);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 25m, 1212m, 456m, "", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 45m, 771m, 657m, "", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 33m, 123m, 543m, "ADD", true);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 1m, 50m, 123.32m, "ADD", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 1m, 50m, 123.32m, "ADD", true);
			var calculatedVat = vatCalculator.CalculateVatFee();
			AssertNotNull(nameof(calculatedVat), calculatedVat);

			CombineAssertions("Calculated VAT", () =>
			{
				AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
				AssertEquals("BaseValue", 1524.68m, calculatedVat.BaseValue);
				AssertEquals("Amount", 335.4296m, calculatedVat.Amount);
				AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
				AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
			});
		}

		public void TestCalculateVatFeeWithOverridenFees()
		{
			var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);
			var invoiceLine = entryLine.Declaration.InvoiceLines[0];
			invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			entryLine.InvoiceLines.Add(invoiceLine);

			var vatCalculator = entryLine.GetEntryLineVatCalculator();

			VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 10m, 1000m, 100m, "ADD", true);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, "%", 25m, 1212m, 456m, "", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 45m, 771m, 657m, "", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, Constants.EntryLineFee.VATFeeTypeCode, "%", 33m, 123m, 543m, ILRateOverrideReasonList.Codes.Additional, true);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, "%", 1m, 50m, 123.32m, "ADD", false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 1m, 50m, 123.32m, ILRateOverrideReasonList.Codes.Additional, false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, "110", "%", 1m, 50m, 123.32m, ILRateOverrideReasonList.Codes.Additional, true);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, "999", "%", 1m, 50m, 123.32m, ILRateOverrideReasonList.Codes.Override, false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, "999", "%", 1m, 50m, 123.32m, ILRateOverrideReasonList.Codes.Additional, false);
			VatCalculationTestHelper.CreateEntryLineFee(entryLine, "998", "%", 1m, 50m, 123.32m, ILRateOverrideReasonList.Codes.Additional, false);
			var calculatedVat = vatCalculator.CalculateVatFee();
			AssertNotNull(nameof(calculatedVat), calculatedVat);

			CombineAssertions("Calculated VAT", () =>
			{
				AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
				AssertEquals("BaseValue", 1681.04m, calculatedVat.BaseValue);
				AssertEquals("Amount", 369.8288m, calculatedVat.Amount);
				AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
				AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
			});
		}

		public void TestCalculateVatFeeWithoutOtherFees()
		{
			var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);
			var invoiceLine = entryLine.Declaration.InvoiceLines[0];
			invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
			entryLine.InvoiceLines.Add(invoiceLine);

			var vatCalculator = entryLine.GetEntryLineVatCalculator();
			var calculatedVat = vatCalculator.CalculateVatFee();
			AssertNotNull(nameof(calculatedVat), calculatedVat);

			CombineAssertions("Calculated VAT", () =>
			{
				AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
				AssertEquals("BaseValue", 0m, calculatedVat.BaseValue);
				AssertEquals("Amount", 0m, calculatedVat.Amount);
				AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
				AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			ordVat = VatCalculationTestHelper.SetUpAndSaveRefData(Factory);
		}

		Universal.RefCusTaxOrFee ordVat;
	}

	public static class VatCalculationTestHelper
	{
		public static Universal.RefCusTaxOrFee SetUpAndSaveRefData(BusinessObjectFactory factory)
		{
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var refDataHelper = new UniversalReferenceTestDataHelper(factory);
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var ordVat = refDataHelper.CreateTaxOrFee("15", 0.22m, currentCountryCode, startDate, endDate);
			var dtyRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "DTY", description: "Customs duties on industrial products");
			var addRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "ADD", description: "Definitive antidumping duties");
			var cvdRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "CVD", description: "Definitive countervailing duties");
			refDataHelper.CreateCusRateCode(factory, zy1RateCode: "A00", dtyRateType.PK);
			refDataHelper.CreateCusRateCode(factory, zy1RateCode: "A30", addRateType.PK);
			refDataHelper.CreateCusRateCode(factory, zy1RateCode: "A40", cvdRateType.PK);
			var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "", parent: eunDataGrouping);
			factory.Save();
			return ordVat;
		}

		public static CusEntryLine SetUpEntryLine(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryHeader.PK;
			return entryLine;
		}

		public static CusEntryLineFee CreateEntryLineFee(CusEntryLine entryLine, ZString chargeType, ZString methodOfCalculation, ZDecimal rate, ZDecimal baseValue, ZDecimal chargeAmount, ZString rateOverrideReasonCode, ZBool isLandedCostOnly)
		{
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = chargeType;
			fee.CF_MethodOfCalculation = methodOfCalculation;
			fee.CF_Rate = rate;
			fee.CF_BaseValue = baseValue;
			fee.CF_ChargeAmount = chargeAmount;
			fee.CF_RateOverrideReasonCode = rateOverrideReasonCode;
			fee.CF_IsLandedCostOnly = isLandedCostOnly;
			return fee;
		}
	}
}
