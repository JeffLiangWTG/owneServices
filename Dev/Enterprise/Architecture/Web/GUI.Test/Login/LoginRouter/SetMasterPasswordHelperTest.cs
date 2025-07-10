using System;
using System.Linq;
using System.Net;
using System.Web;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	sealed class SetMasterPasswordHelperTest : TestCaseWithFactory
	{
		public void TestGenerateSetMasterPasswordUrl()
		{
			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_WebAccessEnabled = true;
			Factory.Save();

			Assert("Precondition", activeContact.OC_IsActive);
			Assert("Precondition", activeContact.OC_WebAccessEnabled);

			var supersededContact = Factory.NewWithValidTestData<OrgContact>();
			supersededContact.OC_Email = "trench@coat.com";
			supersededContact.OC_WebAccessEnabled = true;
			supersededContact.OC_PER = activeContact.OC_PER;
			supersededContact.SupersedeWebAccess();
			supersededContact.SetHashedPassword("1234");
			Assert("Precondition", supersededContact.OC_WebAccessEnabled);
			Assert("Precondition", supersededContact.WebAccessSuperseded);

			activeContact.OC_Email = "camo@mile.com";
			Factory.Save();

			var passwordBasePageUrl = new Uri("https://google.com");
			var originalUrl1 = new Uri("https://yahoo.com");
			var url = SetMasterPasswordHelper.GenerateSetMasterPasswordUrl(supersededContact, passwordBasePageUrl, originalUrl1, LoginRouterIdentityManager.GenerateToken(supersededContact));

			var values = HttpUtility.ParseQueryString(url.Query);
			var queryString = new SecureQueryString(WebUtility.UrlDecode(values[SecureQueryString.QueryStringKey]));
			var token = queryString[WebUserAdminManager.SetMasterPasswordKey];

			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.SetMasterPassword);
			var accessTokens = Factory.Load<StmAccessToken>(query);

			AssertEquals(1, accessTokens.Length);
			var activeContactToken = accessTokens.FirstOrDefault(x => x.SAT_Scope == originalUrl1.AbsoluteUri);
			AssertNotNull(activeContactToken);

			AssertEquals("Token in url should match the saved token in the db", token, activeContactToken.SAT_Token);
		}
	}
}
