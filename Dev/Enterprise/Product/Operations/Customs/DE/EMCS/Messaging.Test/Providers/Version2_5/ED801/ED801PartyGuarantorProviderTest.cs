using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5.Testing
{
	class ED801PartyGuarantorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, ED801PartyGuarantorProvider.NewOrNull(null));
		}

		public void TestTraderExciseNumber()
		{
			guarantorTrader.TraderExciseNumber = "DETE001";
			AssertEquals("DETE001", eD801PartyGuarantorProvider.TraderExciseNumber);
		}

		public void TestVatNumber()
		{
			guarantorTrader.VatNumber = "DEVN001";
			AssertEquals("VN001", eD801PartyGuarantorProvider.VatNumber);
		}

		public void TestTraderName()
		{
			guarantorTrader.TraderName = "TraderName01";
			AssertEquals("TraderName01", eD801PartyGuarantorProvider.Name);
		}

		public void TestAddress()
		{
			guarantorTrader.StreetName = "Street A";
			guarantorTrader.StreetNumber = "9";
			AssertEquals("Street A 9", eD801PartyGuarantorProvider.Address);
		}

		public void TestCity()
		{
			guarantorTrader.City = "Berlin";
			AssertEquals("Berlin", eD801PartyGuarantorProvider.City);
		}

		public void TestPostcode()
		{
			guarantorTrader.Postcode = "10115";
			AssertEquals("10115", eD801PartyGuarantorProvider.Postcode);
		}

		public void TestCountry()
		{
			guarantorTrader.TraderExciseNumber = "DETE001";
			guarantorTrader.VatNumber = "DEVN001";
			AssertEquals("DE", eD801PartyGuarantorProvider.Country);

			guarantorTrader.TraderExciseNumber = "";
			guarantorTrader.VatNumber = "AUVN001";
			AssertEquals("AU", eD801PartyGuarantorProvider.Country);
		}

		public void TestLanguage()
		{
			CombineAssertions(() =>
			{
				guarantorTrader.NadLng = Core.Constants.CountryCodes.France;
				AssertEquals("With value", Core.Constants.CountryCodes.France, eD801PartyGuarantorProvider.Language);

				guarantorTrader.NadLng = null;
				AssertEquals("Null", string.Empty, eD801PartyGuarantorProvider.Language);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			guarantorTrader = new ED801EBodyEadContainerMovementGuaranteeGuarantorTrader();
			eD801PartyGuarantorProvider = ED801PartyGuarantorProvider.NewOrNull(guarantorTrader);
		}
		ED801EBodyEadContainerMovementGuaranteeGuarantorTrader guarantorTrader;
		IEMCSPartyGuarantor eD801PartyGuarantorProvider;
	}
}

