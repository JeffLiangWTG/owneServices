using System;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	sealed class GuaranteeConfigurationTest : EU.NCTS.Business.Testing.GuaranteeConfigurationAbstractTest<GuaranteeConfiguration>
	{
		public override void TestOverrideSupport()
		{
			AssertEquals(true, configuration.OverrideSupport(header));
		}

		public override void TestApplySecurityToPW_Override()
		{
			AssertEquals(true, configuration.ApplySecurityToPW_Override(header));
		}

		public void TestDefaultPercentageForLiabilityAmountCalculation() => AssertEquals(100, configuration.DefaultPercentageForLiabilityAmountCalculation);

		protected override Type ExpectedDeparturePhase5GuaranteeValidationDeciderType => typeof(NctsGuaranteeDeparturePhase5ValidationDecider);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}

		NctsHeader header;
	}
}
