//using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class PlaceWrapperTest : Customs.Business.Testing.DataProviderTestCase<PlaceWrapper>
	{
		public void TestUNLoCode()
		{
			AssertEquals("UNLoCode should be mapped to wrapper only parameter if valid unloco.", "FRPAR", Provider.UNLoCode);
		}

		public void TestCountry()
		{
			AssertEquals("Country should be mapped to unloco if it is part of the NC008 country list.", "FRPAR", Provider.UNLoCode);
		}

		public void TestLocation()
		{
			AssertEquals("Location should be mapped to the provided port of loading/unloading.", "AUDMA", Provider.Location);
		}

		protected override PlaceWrapper GetProvider()
		{
			return PlaceWrapper.New("FRPAR", "AUDMA");
		}
	}
}
