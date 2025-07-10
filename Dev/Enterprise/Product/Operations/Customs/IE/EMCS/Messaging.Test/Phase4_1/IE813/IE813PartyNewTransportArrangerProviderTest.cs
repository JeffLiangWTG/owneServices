using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE813PartyNewTransportArrangerProviderTest : Business.Testing.DataProviderTestCase<IE813PartyNewTransportArrangerProvider>
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE813PartyNewTransportArrangerProvider.NewOrNull(null));
		}

		public void TestVatNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, Provider.VatNumber);

				message.VatNumber = "DETRT001";
				AssertEquals("TRT001", Provider.VatNumber);
			});
		}

		public void TestName()
		{
			message.TraderName = "Transport Arranger 01";
			AssertEquals("Transport Arranger 01", Provider.Name);
		}

		public void TestStreetAndNumber()
		{
			CombineAssertions(() =>
			{
				message.StreetName = "Street A";
				AssertEquals("StreetNumber not specified, No exception", "Street A ", Provider.StreetAndNumber);

				message.StreetNumber = "9";
				AssertEquals("Street A 9", Provider.StreetAndNumber);
			});
		}

		public void TestCity()
		{
			message.City = "Berlin";
			AssertEquals("Berlin", Provider.City);
		}

		public void TestPostcode()
		{
			message.Postcode = "10115";
			AssertEquals("10115", Provider.Postcode);
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, Provider.Country);

				message.VatNumber = "DETRT001";
				AssertEquals("DE", Provider.Country);
			});
		}

		public void TestLanguage()
		{
			message.Language = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, Provider.Language);
		}

		protected override IE813PartyNewTransportArrangerProvider GetProvider() => IE813PartyNewTransportArrangerProvider.NewOrNull(message);

		protected override void SetUp()
		{
			base.SetUp();
			message = new NewTransportArrangerTraderType();
		}
		NewTransportArrangerTraderType message;
	}
}
