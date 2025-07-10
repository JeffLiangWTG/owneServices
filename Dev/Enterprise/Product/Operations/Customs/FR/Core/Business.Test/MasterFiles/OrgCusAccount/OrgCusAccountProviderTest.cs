using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	class OrgCusAccountProviderTest : TestCaseWithFactory
	{
		public void TestDefaultingOfType()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			AssertEquals("Account Type should be default to TR for Delta T because TR is the only type available in type list.", OrgCusAccountDeltaTTypeList.Codes.TR, orgCusAccount.CZ_Type);

			var orgCusAccount2 = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount2.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount2.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			AssertEquals("Account Type cannot be defaulted because there are multiple values available in type list.", ZString.Empty, orgCusAccount2.CZ_Type);
		}

		public void TestShouldDefaultTypeWhenAble()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var provider = orgCusAccount.Provider;
			Assert("Defaulting of Type is enabled in FR solution", provider.ShouldDefaultTypeWhenAble);
		}

		public void TestOrgCusAccountProvider()
		{
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var provider = orgCusAccount.Provider;
			AssertType<OrgCusAccountProvider>(provider);
			AssertType<OrgCusAccountLookups>(provider.GetNewLookups(orgCusAccount));
			AssertType<OrgCusAccountValidation>(provider.GetNewValidation(orgCusAccount));
			AssertEquals(nameof(FieldType.TextCodeFindBox), provider.CZ_IssuerFieldType);
		}
	}
}
