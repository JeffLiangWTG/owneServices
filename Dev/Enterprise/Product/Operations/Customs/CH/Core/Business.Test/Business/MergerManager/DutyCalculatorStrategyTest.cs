using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DutyCalculatorStrategy))]
sealed class DutyCalculatorStrategyTest : Customs.Business.Testing.DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
{
	protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

	protected override bool ExpectedShouldCalculateDuties => true;

	protected override ZString UniversalTariffType => UniversalReferenceConstants.TariffTypes.ExportTariff;

	public void TestCalculateDutiesForRates_Overridden() => CombineAssertions(() =>
	{
		SetupReferenceData();
		var declaration = (JobDeclaration)Declaration;
		var entryLine1 = (CusEntryLine)GetEntryHeader(declaration, "0101100000").MergedLines[0];
		var invoiceLine1 = entryLine1.RandomLine;
		invoiceLine1.JI_RateOverride = true;
		invoiceLine1.JI_OverriddenRate = 0.4m;

		GetDutyCalculatorStrategy().CalculateDuties();

		AssertEquals("# fees for line 1", 1, entryLine1.Fees.Count);
		var fee1 = entryLine1.Fees[0];
		AssertEquals("CalculatedDutyAmount for line 1", 8.8m, fee1.CF_ChargeAmount);
	});

	public void TestUpdateEntryLineFees()
	{
		SetupReferenceData();
		var declaration = (JobDeclaration)Declaration;
		var entryLine1 = (CusEntryLine)GetEntryHeader(declaration, "0101100000").MergedLines[0];
		var invoiceLine1 = entryLine1.RandomLine;

		GetDutyCalculatorStrategy().CalculateDuties();

		var feeStandardRate = entryLine1.Fees[0];
		AssertEquals("ChargeType for StandardRate - not fix DTY, depends on Tariff", "RC1", feeStandardRate.CF_ChargeType);
		AssertEquals("RateOverrideReasonCode for StandardRate blank", ZString.Empty, feeStandardRate.CF_RateOverrideReasonCode);
		AssertEquals("StandardRate", 0.8m, feeStandardRate.CF_Rate);
		AssertEquals("CalculatedDutyAmount for StandardRate", 17.6m, feeStandardRate.CF_ChargeAmount);
		AssertEquals("BaseValue for StandardRate", 22m, feeStandardRate.CF_BaseValue);

		invoiceLine1.JI_RateOverride = true;
		invoiceLine1.JI_OverriddenRate = 0.4m;

		GetDutyCalculatorStrategy().CalculateDuties();

		var feeOverridenRate = entryLine1.Fees[0];
		AssertEquals("ChargeType for OverriddenRate - not fix DTY, depends on Tariff", "RC1", feeOverridenRate.CF_ChargeType);
		AssertEquals("RateOverrideReasonCode for OverriddenRate OVR", "OVR", feeOverridenRate.CF_RateOverrideReasonCode);
		AssertEquals("OverriddenRate", 0.4m, feeOverridenRate.CF_Rate);
		AssertEquals("CalculatedDutyAmount for OverriddenRate", 8.8m, feeOverridenRate.CF_ChargeAmount);
		AssertEquals("BaseValue for OverriddenRate", 22m, feeOverridenRate.CF_BaseValue);
	}
}
