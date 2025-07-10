using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class EntryLineVatCalculatorTest : TestCaseWithFactory
{
	public void TestCalculateVatFeeOnA35AndA45()
	{
		CusEntryLineFee levyFee;
		CusEntryLineFee a35Fee;
		CusEntryLineFee a45Fee;

		var entryLine = VatCalculationTestHelper.SetUpEntryLine(Factory);
		var invoiceLine = entryLine.Declaration.InvoiceLines[0];
		invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;
		entryLine.InvoiceLines.Add(invoiceLine);

		var vatCalculator = new EntryLineVatCalculator(entryLine);

		using (entryLine.Fees.SuspendSystemAddedVatFeeRecalculation())
		{
			levyFee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, "LEV", "%", 22m, 5000m, 1100m, "LEV", false);
			a35Fee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, RefCusRateCodes.TemporaryAntiDumpingDuty, "%", 12m, 1000m, 120m, "ADD", false);
			a45Fee = VatCalculationTestHelper.CreateEntryLineFee(entryLine, RefCusRateCodes.TemportaryCountervailingDuty, "%", 13m, 1000m, 130m, "CVD", false);
		}

		CombineAssertions("PRE-CONDITION: fees methods of payment", () =>
		{
			AssertNotEquals(DutyMethodOfPayment.SecurityDepositDeferredPaymentR, levyFee.CF_MethodOfPayment);
			AssertEquals(DutyMethodOfPayment.SecurityDepositDeferredPaymentR, a35Fee.CF_MethodOfPayment);
			AssertEquals(DutyMethodOfPayment.SecurityDepositDeferredPaymentR, a45Fee.CF_MethodOfPayment);
		});

		CombineAssertions("Calculated VAT (including all fees)", () =>
		{
			var calculatedVat = vatCalculator.CalculateVatFee();
			AssertNotNull(nameof(calculatedVat), calculatedVat);

			AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
			AssertEquals("BaseValue", 1100.00m, calculatedVat.BaseValue);
			AssertEquals("Amount", 242.00m, calculatedVat.Amount);
			AssertEquals("MethodOfCalculation", "%", calculatedVat.MethodOfCalculation);
			AssertEquals("AdjustedRate", ordVat.ZZF_Value * 100, calculatedVat.AdjustedRate);
		});

		CombineAssertions("Calculated VAT (including only 'A35' and 'A45' fees)", () =>
		{
			entryLine.Fees.RemoveAndDelete(levyFee);
			var calculatedVat = vatCalculator.CalculateVatFee();
			AssertNotNull(nameof(calculatedVat), calculatedVat);

			AssertEquals("Rate", ordVat.ZZF_Value, calculatedVat.Rate);
			AssertEquals("BaseValue", 0.00m, calculatedVat.BaseValue);
			AssertEquals("Amount", 0.00m, calculatedVat.Amount);
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
		var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.22m, currentCountryCode, startDate, endDate);
		var dtyRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "DTY", description: "Customs duties on industrial products");
		var addRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "ADD", description: "Definitive antidumping duties");
		var cvdRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "CVD", description: "Definitive countervailing duties");
		var levRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "LEV", description: "Levies");
		refDataHelper.CreateCusRateCode(factory, zy1RateCode: "A00", dtyRateType.PK);
		refDataHelper.CreateCusRateCode(factory, zy1RateCode: "A35", addRateType.PK);
		refDataHelper.CreateCusRateCode(factory, zy1RateCode: "A45", cvdRateType.PK);
		refDataHelper.CreateCusRateCode(factory, zy1RateCode: "LEV", levRateType.PK);
		var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "", parent: eunDataGrouping);
		factory.Save();
		return ordVat;
	}

	public static CusEntryLine SetUpEntryLine(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
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
