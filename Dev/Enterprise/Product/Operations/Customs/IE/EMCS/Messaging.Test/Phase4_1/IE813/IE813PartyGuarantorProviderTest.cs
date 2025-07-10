using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE813;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE813PartyGuarantorProviderTest : Business.Testing.DataProviderTestCase<IE813PartyGuarantorProvider>
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE813PartyGuarantorProvider.NewOrNull(null));
		}

		public void TestTraderExciseNumber()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.TraderExciseNumber);

				message.TraderExciseNumber = "DETE001";
				AssertEquals("DETE001", Provider.TraderExciseNumber);
			});
		}

		public void TestVatNumber()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.VatNumber);

				message.VatNumber = "DEVN001";
				AssertEquals("VN001", Provider.VatNumber);
			});
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.Name);

				message.TraderName = "TraderName01";
				AssertEquals("TraderName01", Provider.Name);
			});
		}

		public void TestStreetAndNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", " ", Provider.StreetAndNumber);

				message.StreetName = "Street A";
				message.StreetNumber = "9";
				AssertEquals("Street A 9", Provider.StreetAndNumber);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.City);

				message.City = "Berlin";
				AssertEquals("Berlin", Provider.City);
			});
		}

		public void TestPostcode()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.Postcode);

				message.Postcode = "10115";
				AssertEquals("10115", Provider.Postcode);
			});
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, Provider.Country);

				message.TraderExciseNumber = "DETE001";
				message.VatNumber = "DEVN001";
				AssertEquals("CountryCode from TraderExciseNumber", "DE", Provider.Country);

				message.TraderExciseNumber = "";
				message.VatNumber = "AUVN001";
				AssertEquals("CountryCode from VatNumber", "AU", Provider.Country);
			});
		}

		public void TestLanguage()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.Language);

				message.Language = Core.Constants.CountryCodes.France;
				AssertEquals(Core.Constants.CountryCodes.France, Provider.Language);
			});
		}

		protected override IE813PartyGuarantorProvider GetProvider() => IE813PartyGuarantorProvider.NewOrNull(message);

		protected override void SetUp()
		{
			base.SetUp();
			message = new GuarantorTraderType();
		}
		GuarantorTraderType message;
	}
}
