using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class PersonProvidingGuaranteeWrapperTest : DataProviderTestCase<PersonProvidingGuaranteeWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal empty.", string.Empty, Provider.IdentificationNumber);
		}

		protected override PersonProvidingGuaranteeWrapper GetProvider()
		{
			var personProvidingGuarantee = Factory.NewWithValidTestData<OrgHeader>();
			personProvidingGuarantee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			personProvidingGuarantee.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_ControllingCustomer = personProvidingGuarantee.PK;

			return PersonProvidingGuaranteeWrapper.New();
		}
	}
}
