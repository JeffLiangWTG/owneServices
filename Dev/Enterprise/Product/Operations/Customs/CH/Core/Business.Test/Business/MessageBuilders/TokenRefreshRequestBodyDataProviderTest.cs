using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class TokenRefreshRequestBodyDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Argument == null", () => new TokenRefreshRequestBodyDataProvider(null));
			AssertNotNull("Argument != null", new TokenRefreshRequestBodyDataProvider(token));
		});
	}

	public void TestRefreshTokenRequestBody()
	{
		AssertEquals("Expected Request Body", expectedRequestBody, dataProvider.GetRequestBody());
	}

	protected override void SetUp()
	{
		base.SetUp();
		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_Code = "CHC";
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
		company.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;

		var branch = Factory.NewWithValidTestData<GlbBranch>();
		branch.GB_Code = "CHB";
		branch.GB_GC = company.PK;
		branch.GB_RL_NKHomePort = "AUSYD";

		token = Factory.New<GlbCompanyTokenCredentials>();
		token.GP_GC = company.PK;
		token.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		token.GP_ExpiryDate = ZDateTime.Today.AddDays(1);
		token.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
		token.GP_UserID = clientId;
		token.CurrentDecryptedPassword = clientSecret;

		refreshToken = Factory.New<GlbExternalPassword_CHR>();
		refreshToken.GP_Certificate = ZBlob.FromAscii(refreshTokenValue);
		refreshToken.GP_GC = company.PK;

		Factory.Save();

		dataProvider = new TokenRefreshRequestBodyDataProvider(token);
	}

	const string clientSecret = "12345679";
	const string refreshTokenValue = "_03ca28a0-c74e-3b6d-9453-259a4deb4257";
	const string clientId = "fgLj92vLh6bdB0W7XmHXq_T19kMa";

	const string expectedRequestBody = "client_secret=12345679&grant_type=refresh_token&refresh_token=_03ca28a0-c74e-3b6d-9453-259a4deb4257&client_id=fgLj92vLh6bdB0W7XmHXq_T19kMa";

	GlbCompanyTokenCredentials token;
	GlbExternalPassword refreshToken;
	TokenRefreshRequestBodyDataProvider dataProvider;
}
