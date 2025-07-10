using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1.Testing
{
	sealed class IE801PartyGuarantorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyGuarantorProvider.NewOrNull(null));
		}

		public void TestTraderExciseNumber()
		{
			guarantorTrader.TraderExciseNumber = "IETE001";
			AssertEquals("IETE001", ie801PartyGuarantorProvider.TraderExciseNumber);
		}

		public void TestVatNumber()
		{
			guarantorTrader.VatNumber = "IEVN001";
			AssertEquals("VN001", ie801PartyGuarantorProvider.VatNumber);
		}

		public void TestTraderName()
		{
			guarantorTrader.TraderName = "TraderName01";
			AssertEquals("TraderName01", ie801PartyGuarantorProvider.Name);
		}

		public void TestStreetAndNumber()
		{
			guarantorTrader.StreetName = "Street A";
			guarantorTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyGuarantorProvider.StreetAndNumber);
		}

		public void TestCity()
		{
			guarantorTrader.City = "Dublin";
			AssertEquals("Dublin", ie801PartyGuarantorProvider.City);
		}

		public void TestPostcode()
		{
			guarantorTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyGuarantorProvider.Postcode);
		}

		public void TestCountry()
		{
			guarantorTrader.TraderExciseNumber = "IETE001";
			guarantorTrader.VatNumber = "IEVN001";
			AssertEquals("IE", ie801PartyGuarantorProvider.Country);

			guarantorTrader.TraderExciseNumber = "";
			guarantorTrader.VatNumber = "AUVN001";
			AssertEquals("AU", ie801PartyGuarantorProvider.Country);
		}

		public void TestLanguage()
		{
			CombineAssertions(() =>
			{
				guarantorTrader.Language = Core.Constants.CountryCodes.France;
				AssertEquals("With value", Core.Constants.CountryCodes.France, ie801PartyGuarantorProvider.Language);

				guarantorTrader.Language = null;
				AssertEquals("Null", string.Empty, ie801PartyGuarantorProvider.Language);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			guarantorTrader = new GuarantorTraderType();
			ie801PartyGuarantorProvider = IE801PartyGuarantorProvider.NewOrNull(guarantorTrader);
		}
		GuarantorTraderType guarantorTrader;
		IEMCSPartyGuarantor ie801PartyGuarantorProvider;
	}
}
