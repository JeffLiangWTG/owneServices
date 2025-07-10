using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CusAuthorisationExtensionTest : TestCaseWithFactory
	{
		public void TestIsAuthorizedHolder()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.OrganisationPK = org.PK;

			var countryCode = Core.Constants.CountryCodes.Spain;

			CombineAssertions(() =>
			{
				AssertEquals("IsAuthorizedHolder returns false when JobDocAddress is null", false, (null as JobDocAddress).IsAuthorizedHolder(Factory, countryCode));

				AssertEquals("IsAuthorizedHolder returns false when there are no authorizations declared", false, docAddress.IsAuthorizedHolder(Factory, countryCode));

				var authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				authorisationHeader.CPH_OH_PermitHolder = org.PK;
				authorisationHeader.CPH_RN_NKCountryCode = countryCode;
				AssertEquals("IsAuthorizedHolder returns true when there is an ACE authorization with the docAddress as holder", true, docAddress.IsAuthorizedHolder(Factory, countryCode));

				authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertEquals("IsAuthorizedHolder returns false when there is an authorization with the docAddress as holder but it's not type ACE", false, docAddress.IsAuthorizedHolder(Factory, countryCode));

				authorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				AssertEquals("IsAuthorizedHolder returns true when there is an ACE authorization with the docAddress as holder", true, docAddress.IsAuthorizedHolder(Factory, countryCode));

				authorisationHeader.CPH_OH_PermitHolder = Factory.NewWithValidTestData<OrgHeader>().PK;
				AssertEquals("IsAuthorizedHolder returns false when there is an ACE authorization but the docAddress is not the holder", false, docAddress.IsAuthorizedHolder(Factory, countryCode));
			});
		}
	}
}
