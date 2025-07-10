namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	public class EconomicOperatorWrapperTest : Customs.Business.Testing.DataProviderTestCase<EconomicOperatorWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal E2_GovRegNum.", "FRGRN001", Provider.IdentificationNumber);

			var provider = EconomicOperatorWrapper.New("FRGRN001");
			AssertEquals("IdentificationNumber should equal E2_GovRegNum.", "FRGRN001", provider.IdentificationNumber);
		}

		protected override EconomicOperatorWrapper GetProvider()
		{
			return EconomicOperatorWrapper.New("GRN001");
		}
	}
}
