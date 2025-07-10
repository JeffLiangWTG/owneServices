using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityApplication.Business;

namespace Enterprise.Client.EDI.IdentityRedirectUrl.Business.Testing
{
	internal class EdiIdentityRedirectUrlValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRedirectType()
		{
			var ediIdentityRedirectUrl = Factory.New<EdiIdentityRedirectUrl>();
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectTypeInfo);

			ediIdentityRedirectUrl.IAR_RedirectType = string.Empty;
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectTypeInfo, "Please enter a value.");

			ediIdentityRedirectUrl.IAR_RedirectType = "Tst";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectTypeInfo, "Enter a valid selection.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectTypeInfo);
		}

		public void TestCheckRedirectUrl()
		{
			var ediIdentityRedirectUrl = Factory.New<EdiIdentityRedirectUrl>();
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);

			ediIdentityRedirectUrl.IAR_RedirectUrl = string.Empty;
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Please enter a value.");

			ediIdentityRedirectUrl.IAR_RedirectUrl = "www.example.com";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Please enter the valid URL.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://contoso.com$/abc/response-oidc";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Please enter the valid URL.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://contoso.com$/abc/response-oidc";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Please enter the valid URL.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://contoso.com$/abc/response-oidc";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Please enter the valid URL.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://contoso.com/abc*/response-oidc";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Does not contain wildcard character.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://contoso.com/abc*/response-oidc";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Does not contain wildcard character.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://contoso.com/abc*/response-oidc";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Does not contain wildcard character.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "http://[::1]:8080/";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Please enter the valid URL.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "http://[::1]:8080/";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Please enter the valid URL.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "http://[::1]:8080/";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Please enter the valid URL.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "http://127.0.0.1/MyApp";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Web/Spa URL must start with https or http://localhost.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://127.0.0.1/MyApp";
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "http://localhost/MyApp";
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "http://127.0.0.1/MyApp";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "Web/Spa URL must start with https or http://localhost.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://127.0.0.1/MyApp";
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "http://localhost/MyApp";
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "fkjdskf://127.0.0.1/MyApp";
			AssertHasError(ediIdentityRedirectUrl.IAR_RedirectUrlInfo, "ICL URL must start with HTTP、HTTPS.");

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://fsdfdskjfhkdj/MyApp";
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);

			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "http://localhostdsfndslkfnl/MyApp";
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);
		}

		public void TestCheckDuplicatedRedirectUrl()
		{
			var ediIdentityApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			var ediIdentityRedirectUrl = ediIdentityApplication.RedirectUrls.AddNew();
			ediIdentityRedirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.InstalledClient;
			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://contoso.com/abc/response-oidc";
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);
			Factory.Save();

			var ediIdentityRedirectUrl2 = ediIdentityApplication.RedirectUrls.AddNew();
			ediIdentityRedirectUrl2.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			ediIdentityRedirectUrl2.IAR_RedirectUrl = "https://contoso.com/abc/response-oidc";
			AssertHasError(ediIdentityRedirectUrl2.IAR_RedirectUrlInfo, "This URL already exists.");

			ediIdentityRedirectUrl.IAR_RedirectUrl = "https://contoso.com/abc/response-oidc";
			AssertNoErrors(ediIdentityRedirectUrl.IAR_RedirectUrlInfo);
		}
	}
}
