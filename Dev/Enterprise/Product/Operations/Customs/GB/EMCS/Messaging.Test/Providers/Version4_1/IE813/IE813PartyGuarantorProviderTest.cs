using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie813;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
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

				message.TraderExciseNumber = "GBTE001";
				AssertEquals("GBTE001", Provider.TraderExciseNumber);
			});
		}

		public void TestVatNumber()
		{
			AssertEquals("VN001", Provider.VatNumber);
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

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", " ", Provider.Address);

				message.StreetName = "Street A";
				message.StreetNumber = "9";
				AssertEquals("Street A 9", Provider.Address);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", Provider.City);

				message.City = "London";
				AssertEquals("London", Provider.City);
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
			AssertEquals("CountryCode from VatNumber", "GB", Provider.Country);
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
			message = new GuarantorTraderType() { TraderExciseNumber = string.Empty, VatNumber = "GBVN001" };
		}
		GuarantorTraderType message;
	}
}
