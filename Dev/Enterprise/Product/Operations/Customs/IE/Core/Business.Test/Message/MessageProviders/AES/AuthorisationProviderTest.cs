using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class AuthorisationProviderTest : DataProviderTestCase<AuthorisationProvider>
	{
		public void TestIdentificationType()
		{
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			AssertEquals("IdentificationType should return AGC_Code", "C520", provider.IdentificationType);
		}

		public void TestUCR()
		{
			authorizationUsage.AGC_Number = "001";
			AssertEquals("UCR should return AGC_Number", "001", provider.UCR);
		}

		public void TestAuthorisatonHolder()
		{
			owner.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222", Core.Constants.CountryCodes.Germany);
			authorizationUsage.AGC_OH_Owner = owner.PK;
			AssertEquals("AuthorisatonHolder should return OrgCusCode.OK_CustomsRegNo where OK_CodeType = 'EOR'", "DEREG222", provider.AuthorisatonHolder);
		}

		protected override AuthorisationProvider GetProvider() => provider;

		protected override void SetUp()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "ACT", "C520", startDate, endDate, "EUN");
			Factory.Save();

			base.SetUp();

			owner = Factory.NewWithValidTestData<OrgHeader>();

			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OH_PermitHolder = owner.PK;

			authorizationUsage = Factory.New<CusAuthorizationUsage>();

			provider = new AuthorisationProvider(authorizationUsage);
		}

		protected OrgHeader owner;
		protected CusAuthorizationUsage authorizationUsage;
		protected AuthorisationProvider provider;
	}
}
