using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1.Testing
{
	sealed class IE801PartyGuarantorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertEquals(null, IE801PartyGuarantorProvider.NewOrNull(null));
		}

		public void TestTraderExciseNumber()
		{
			guarantorTrader.TraderExciseNumber = "GBTE001";
			AssertEquals("GBTE001", ie801PartyGuarantorProvider.TraderExciseNumber);
		}

		public void TestVatNumber()
		{
			guarantorTrader.VatNumber = "GBVN001";
			AssertEquals("VN001", ie801PartyGuarantorProvider.VatNumber);
		}

		public void TestTraderName()
		{
			guarantorTrader.TraderName = "TraderName01";
			AssertEquals("TraderName01", ie801PartyGuarantorProvider.Name);
		}

		public void TestAddress()
		{
			guarantorTrader.StreetName = "Street A";
			guarantorTrader.StreetNumber = "9";
			AssertEquals("Street A 9", ie801PartyGuarantorProvider.Address);
		}

		public void TestCity()
		{
			guarantorTrader.City = "London";
			AssertEquals("London", ie801PartyGuarantorProvider.City);
		}

		public void TestPostcode()
		{
			guarantorTrader.Postcode = "10115";
			AssertEquals("10115", ie801PartyGuarantorProvider.Postcode);
		}

		public void TestCountry()
		{
			guarantorTrader.TraderExciseNumber = "GBTE001";
			guarantorTrader.VatNumber = "GBVN001";
			AssertEquals("GB", ie801PartyGuarantorProvider.Country);

			guarantorTrader.TraderExciseNumber = "";
			guarantorTrader.VatNumber = "AUVN001";
			ie801PartyGuarantorProvider = IE801PartyGuarantorProvider.NewOrNull(guarantorTrader);
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
