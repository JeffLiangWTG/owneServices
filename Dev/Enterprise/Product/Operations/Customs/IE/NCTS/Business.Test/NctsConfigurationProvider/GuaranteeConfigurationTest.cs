namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class GuaranteeConfigurationTest : EU.NCTS.Business.Testing.GuaranteeConfigurationAbstractTest<GuaranteeConfiguration>
	{
		public void TestDefaultPercentageForLiabilityAmountCalculation()
		{
			AssertEquals(100, configuration.DefaultPercentageForLiabilityAmountCalculation);
		}

		public override void TestApplySecurityToPW_Override()
		{
			AssertEquals(false, configuration.ApplySecurityToPW_Override(Header));
		}

		public override void TestOverrideSupport()
		{
			AssertEquals(true, configuration.OverrideSupport(Header));
		}

		NctsHeader Header => header ??= Factory.New<NctsHeader>();
		NctsHeader header;
	}
}
