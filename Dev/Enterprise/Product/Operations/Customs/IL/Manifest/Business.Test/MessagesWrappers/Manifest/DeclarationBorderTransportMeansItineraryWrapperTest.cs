using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationBorderTransportMeansItineraryWrapperTest : Customs.Business.Testing.DataProviderTestCase<IDeclarationBorderTransportMeansItinerary>
	{
		public void TestNewOrNull()
		{
			AssertNull("When code is null", DeclarationBorderTransportMeansItineraryWrapper.NewOrNull(null));
			AssertNotNull("When code is not null", DeclarationBorderTransportMeansItineraryWrapper.NewOrNull("IL"));
		}

		public void TestRoutingCountryCode()
		{
			var wrapper = Provider;
			AssertEquals("ID should be IL", "IL", wrapper.RoutingCountryCode.Value);
		}

		protected override IDeclarationBorderTransportMeansItinerary GetProvider() => DeclarationBorderTransportMeansItineraryWrapper.NewOrNull("IL");
	}
}
