namespace Enterprise.Customs.GB.Business.Testing
{
	class GuaranteeConfigurationTest : EU.NCTS.Business.Testing.GuaranteeConfigurationAbstractTest<GuaranteeConfiguration>
	{
		public override void TestApplySecurityToPW_Override()
		{
			AssertEquals(true, configuration.ApplySecurityToPW_Override(header));
		}

		public override void TestOverrideSupport()
		{
			AssertEquals(true, configuration.OverrideSupport(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}

		NctsHeader header;
	}
}
