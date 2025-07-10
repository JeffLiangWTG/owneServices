using System;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class GuaranteeConfigurationTest : EU.NCTS.Business.Testing.GuaranteeConfigurationAbstractTest<GuaranteeConfiguration>
{
	protected override Type ExpectedDeparturePhase5GuaranteeValidationDeciderType => typeof(NctsGuaranteeDeparturePhase5ValidationDecider);

	public override void TestOverrideSupport()
	{
		AssertEquals(true, configuration.OverrideSupport(header));
	}

	public override void TestApplySecurityToPW_Override()
	{
		AssertEquals(false, configuration.ApplySecurityToPW_Override(header));
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}
	NctsHeader header;
}
