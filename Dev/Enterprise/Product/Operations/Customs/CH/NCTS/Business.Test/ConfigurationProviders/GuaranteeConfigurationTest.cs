using System;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(GuaranteeConfiguration))]
sealed class GuaranteeConfigurationTest : GuaranteeConfigurationAbstractTest<GuaranteeConfiguration>
{
	public override void TestApplySecurityToPW_Override() => AssertEquals(false, configuration.ApplySecurityToPW_Override(Header));

	public override void TestOverrideSupport() => AssertEquals(true, configuration.OverrideSupport(Header));

	public void TestDefaultPercentageForLiabilityAmountCalculation() => AssertEquals(10, configuration.DefaultPercentageForLiabilityAmountCalculation);

	public void TestAllowDefaultLiabilityAmount() => AssertEquals(true, configuration.AllowDefaultLiabilityAmount);

	public void TestDefaultLiabilityAmount() => AssertEquals(10000m, configuration.DefaultLiabilityAmount);

	protected override Type ExpectedDeparturePhase5GuaranteeValidationDeciderType => typeof(NctsGuaranteeDepartureValidationDecider);

	public void TestUseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods() => AssertEquals(expected: false, configuration.UseDutiesAndTaxesOrMonetaryValueAsTotalValueCalculationMethods);

	NctsHeader Header => header ??= SetUpNctsHeader();
	NctsHeader header;

	NctsHeader SetUpNctsHeader()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return header;
	}
}
