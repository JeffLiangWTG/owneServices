using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class PersonPresentingTheGoodsWrapperTest : Customs.Business.Testing.DataProviderTestCase<PersonPresentingTheGoodsWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal EORi number of Presenter, whatever its country when no EORI found for storage country.", "GBDEF", Provider.IdentificationNumber);
			var storageHeader = SetUpTemporaryStorageHeader();
			storageHeader.Presenter.Header.CustomsCodes.AddNew("EOR", "123456789", storageHeader.AMA_RN_NKCountry);
			storageHeader.Presenter.Header.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			var personPresentingTheGoodsWrapper = PersonPresentingTheGoodsWrapper.New(storageHeader);
			AssertEquals("IdentificationNumber should equal EORI for storage country when such code exists", "FR12345678900001", personPresentingTheGoodsWrapper.IdentificationNumber);
		}

		protected override PersonPresentingTheGoodsWrapper GetProvider()
		{
			return PersonPresentingTheGoodsWrapper.New(SetUpTemporaryStorageHeader());
		}

		TemporaryStorageHeader SetUpTemporaryStorageHeader()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.OH_FullName = "ABCD";
			orgAddress.OA_OH = orgHeader.PK;
			storageHeader.AMA_OA_Presenter = orgAddress.PK;

			storageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			orgHeader.CustomsCodes.AddNew("EOR", "DEF", Core.Constants.CountryCodes.UnitedKingdom);
			return storageHeader;
		}
	}
}
