using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class TraderWrapperTest : Customs.Business.Testing.DataProviderTestCase<TraderWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should be equal to DestinationTrader EORI.", "FR12345678900001", Provider.IdentificationNumber);
		}

		public void TestCommunicationLanguageAtDestination()
		{
			AssertEquals("CommunicationLanguageAtDestination should be equal to DestinationTrader OH_Language.", "FR", Provider.CommunicationLanguageAtDestination);
		}

		protected override TraderWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var destinationTrader = Factory.New<OrgHeader>();
			destinationTrader.OH_Language = "FR";
			var destinationTraderAddress = destinationTrader.Addresses.AddNew();
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTraderAddress.PK;
			destinationTraderAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			destinationTraderAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);

			destinationTraderAddress.City = "Paris";
			destinationTraderAddress.OA_RN_NKCountryCode = "FR";

			return TraderWrapper.New(nctsHeader.DestinationTrader);
		}
	}
}
