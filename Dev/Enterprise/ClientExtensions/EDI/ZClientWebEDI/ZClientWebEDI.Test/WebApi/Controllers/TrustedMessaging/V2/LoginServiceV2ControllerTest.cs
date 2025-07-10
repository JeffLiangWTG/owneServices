using System;
using System.Collections.Generic;
using System.Net;
using CargoWise.Types;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class LoginServiceV2ControllerTest : TrustedMessagingV2ControllerBaseTest
	{
		public void TestGetAutoLoginUrl()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/my-account/auto-login-url");
			var reqObj = new TrustedAutoLoginInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				TenantId = "DDDABCSYD",
				UserId = "U048173",
				FullName = "User One",
				Email = "user.one@test.com",
				ReturnUrl = new Uri("https://www.cw1.com"),
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
				UserCountry = "AU"
			};

			var (statusCode, content) = CallWebApi(uri, reqObj);
			AssertEquals(HttpStatusCode.OK, statusCode);
			Assert(content.StartsWith("{\"url\":\"https://myaccount-portal.cargowise.com/myaccount/Login/AutoLogin.aspx?token="));
		}

		public void TestUpdateUserAccount()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/my-account/user-account");
			var reqObj = new UpdateUserInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				TenantId = "DDDABCSYD",
				UserId = "U048173",
				FullName = "User One",
				Email = "user.one@test.com",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
				UserCountry = "AU",
				IsActive = true,
			};
			AssertWebApi(uri, reqObj, HttpStatusCode.OK, "");
		}

		public void TestGetOAuthLoginUrl()
		{
			var uri = new Uri("https://unit-testing/api/sso/v2/my-account/oauth-login-url");
			var reqObj = new AuthenticationTokenInfo()
			{
				Product = "CW1",
				SystemId = "8000",
				TenantId = "DDDABCSYD",
				UserId = "U048173",
				FullName = "User One",
				Email = "user.one@test.com",
				InfoTimestamp = ZDateTime.UtcNow.ToDateTime(),
				UserCountry = "AU",
				ResourceProduct = "CW1",
				ResourceSystemId = "8000",
				RedirectUriString = "https://www.cw1.com",
				Claims = new List<Claim>()
				{
					new Claim() { Key = "is_admin", Value = "true" }
				}
			};
			var (statusCode, content) = CallWebApi(uri, reqObj);
			AssertEquals(HttpStatusCode.OK, statusCode);
			Assert(content.StartsWith("{\"url\":\"https://www.cw1.com:443/?token="));
		}

		protected override TrustedController GetTrustedController(NLogWrapper logger)
		{
			return new LoginServiceV2Controller(logger);
		}
	}
}
