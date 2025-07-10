using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class ActiveBorderTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<ActiveBorderTransportMeansWrapper>
	{
		protected override ActiveBorderTransportMeansWrapper GetProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.ZG_Box18TransportNationality = Core.Constants.CountryCodes.France;
			return ActiveBorderTransportMeansWrapper.New(declaration);
		}

		public void TestNationality()
		{
			AssertEquals("Nationality should be equal to ZG_Box18TransportNationality.", Core.Constants.CountryCodes.France, Provider.Nationality);
		}
	}
}
