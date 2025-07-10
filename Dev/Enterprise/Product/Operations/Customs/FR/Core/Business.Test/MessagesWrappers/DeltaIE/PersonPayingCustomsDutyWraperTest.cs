using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class PersonPayingCustomsDutyWrapperTest : DataProviderTestCase<PersonPayingCustomsDutyWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal person EORI.", "FR12345678900001", Provider.IdentificationNumber);
		}

		protected override PersonPayingCustomsDutyWrapper GetProvider()
		{
			var dfpOrg = Factory.NewWithValidTestData<OrgHeader>();
			dfpOrg.OH_Code = "DFRPORG";
			dfpOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			dfpOrg.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			var declaration = Factory.New<JobDeclaration>();

			var docAddress = declaration.DocAddresses.AddNew();
			docAddress.E2_AddressType = "DFP";
			docAddress.E2_OA_Address = dfpOrg.MainAddress.PK;
			return PersonPayingCustomsDutyWrapper.New(declaration);
		}
	}
}
