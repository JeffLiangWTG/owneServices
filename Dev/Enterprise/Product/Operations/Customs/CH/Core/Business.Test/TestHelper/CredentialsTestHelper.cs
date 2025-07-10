using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public static class CredentialsTestHelper
{
	public static GlbCompanyCredential CreateCurrentCompanyCertificateCredential()
	{
		return CreateCompanyCertificateCredential(GlbCompany.CurrentCompany);
	}

	public static GlbCompanyCredential CreateCompanyCertificateCredential(GlbCompany company)
	{
		var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword;
		credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
		credential.GP_UserID = ClientId;
		credential.CurrentDecryptedPassword = ClientSecret;
		credential.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		company.Factory.Save();

		return credential;
	}

	public static GlbCompanyTokenCredentials CreateCurrentCompanyTokenCredential(string customsCodeType = null, string regNum = null) => CreateCompanyTokenCredential(GlbCompany.CurrentCompany, customsCodeType, regNum);

	public static GlbCompanyTokenCredentials CreateCompanyTokenCredential(GlbCompany company, string customsCodeType = null, string regNum = null)
	{
		var companyWrapper = Enterprise.Customs.CH.Business.GlbCompanyWrapper.GetWrapper<Enterprise.Customs.CH.Business.GlbCompanyWrapper>(company);
		companyWrapper.TokenCredentialsEnabled = true;
		var credential = companyWrapper.TokenCredentials;
		credential.GP_Certificate = new ZBlob(X509Certificate2TestHelper.ValidCertificate);
		credential.GP_UserID = ClientId;
		credential.CurrentDecryptedPassword = ClientSecret;
		credential.RefreshTokenText = RefreshToken;

		if (customsCodeType != null && regNum != null)
		{
			company.OrgProxy.CustomsCodes.AddNew(customsCodeType, regNum);
		}

		company.Factory.Save();

		return credential;
	}

	public const string ClientSecret = "12345679";
	public const string RefreshToken = "_03ca28a0-c74e-3b6d-9453-259a4deb4257";
	public const string ClientId = "fgLj92vLh6bdB0W7XmHXq_T19kMa";

	public const string ExpectedRequestBody = "client_secret=12345679&grant_type=refresh_token&refresh_token=_03ca28a0-c74e-3b6d-9453-259a4deb4257&client_id=fgLj92vLh6bdB0W7XmHXq_T19kMa";
}
