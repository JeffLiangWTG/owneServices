using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4.Testing
{
	class ED813PartyGuarantorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED813PartyGuarantorProvider.NewOrNull(null));
		}

		public void TestTraderExciseNumber()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyGuarantorProvider.TraderExciseNumber);

				guarantorTrader.TraderExciseNumber = "DETE001";
				AssertEquals("DETE001", eD813PartyGuarantorProvider.TraderExciseNumber);
			});
		}

		public void TestVatNumber()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyGuarantorProvider.VatNumber);

				guarantorTrader.VatNumber = "DEVN001";
				AssertEquals("VN001", eD813PartyGuarantorProvider.VatNumber);
			});
		}

		public void TestTraderName()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyGuarantorProvider.Name);

				guarantorTrader.TraderName = "TraderName01";
				AssertEquals("TraderName01", eD813PartyGuarantorProvider.Name);
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", " ", eD813PartyGuarantorProvider.Address);

				guarantorTrader.StreetName = "Street A";
				guarantorTrader.StreetNumber = "9";
				AssertEquals("Street A 9", eD813PartyGuarantorProvider.Address);
			});
		}

		public void TestCity()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyGuarantorProvider.City);

				guarantorTrader.City = "Berlin";
				AssertEquals("Berlin", eD813PartyGuarantorProvider.City);
			});
		}

		public void TestPostcode()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyGuarantorProvider.Postcode);

				guarantorTrader.Postcode = "10115";
				AssertEquals("10115", eD813PartyGuarantorProvider.Postcode);
			});
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not specified, No exception", string.Empty, eD813PartyGuarantorProvider.Country);

				guarantorTrader.TraderExciseNumber = "DETE001";
				guarantorTrader.VatNumber = "DEVN001";
				AssertEquals("CountryCode from TraderExciseNumber", "DE", eD813PartyGuarantorProvider.Country);

				guarantorTrader.TraderExciseNumber = "";
				guarantorTrader.VatNumber = "AUVN001";
				AssertEquals("CountryCode from VatNumber", "AU", eD813PartyGuarantorProvider.Country);
			});
		}

		public void TestLanguage()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmptyOrWhitespace("Not specified, No exception", eD813PartyGuarantorProvider.Language);

				guarantorTrader.NadLng = Core.Constants.CountryCodes.France;
				AssertEquals(Core.Constants.CountryCodes.France, eD813PartyGuarantorProvider.Language);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			guarantorTrader = new ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader();
			eD813PartyGuarantorProvider = ED813PartyGuarantorProvider.NewOrNull(guarantorTrader);
		}
		ED813EBodyChangeOfDestinationDestinationChangedMovementGuaranteeGuarantorTrader guarantorTrader;
		IEMCSPartyGuarantor eD813PartyGuarantorProvider;
	}
}

