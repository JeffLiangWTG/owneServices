using System;
using System.Data;
using System.Web;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Cookie;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[UseSnapshotProtection]
	sealed class LoginSuccessCookieHelperTest : ZBaseCookieTest
	{
		protected override ZBaseCookie GetNewCookie(string cookieName)
		{
			return new LoginSuccessCookieHelperForTest();
		}

		new LoginSuccessCookieHelperForTest TestCookie
		{
			get { return base.TestCookie as LoginSuccessCookieHelperForTest; }
		}

		public void TestWriteReadLoginHashCookie()
		{
			var loginName = "TestCompany";

			TestCookie.WriteLoginHashCookie(loginName);
			AssertEquals(true, TestCookie.CookieExist(loginName));
			AssertEquals(32, TestCookie.LoadLoginHash(loginName).Length);

			var wrongLoginName = "TesCompany";
			AssertEquals(false, TestCookie.CookieExist(wrongLoginName));
			AssertEquals(null, TestCookie.LoadLoginHash(wrongLoginName));
		}

		public void TestWriteLoginHashCookie_NoSecretkey()
		{
			ClearKey(WebDataRegistry.Instance.LoginFailureAttemptSecretKey.Name);
			var loginName = "TestCompany";

			TestCookie.WriteLoginHashCookie(loginName);
			AssertEquals(false, TestCookie.CookieExist(loginName));
			Assert(ErrorReporter.HasBeenReported("LoginFailureAttemptSecretKeyNotSet"));
			ErrorReporter.Clear();
		}

		public void TestWriteReadLoginHashCookie_MultipleCookies()
		{
			var loginName1 = "TestCompany";

			var loginName2 = "AnotherCompany";

			TestCookie.WriteLoginHashCookie(loginName1);
			TestCookie.WriteLoginHashCookie(loginName2);

			var hash1 = TestCookie.LoadLoginHash(loginName1);
			AssertEquals(true, TestCookie.CookieExist(loginName1));
			AssertEquals(32, hash1.Length);

			var hash2 = TestCookie.LoadLoginHash(loginName2);
			AssertEquals(true, TestCookie.CookieExist(loginName2));
			AssertEquals(32, hash2.Length);

			AssertNotEquals(hash1, hash2);
		}

		public void TestWriteReadLoginHashCookie_MultipleTimes()
		{
			var loginName = "TestCompany";

			TestCookie.WriteLoginHashCookie(loginName);
			var hash1 = TestCookie.LoadLoginHash(loginName);

			TestCookie.WriteLoginHashCookie(loginName);
			var hash2 = TestCookie.LoadLoginHash(loginName);

			AssertNotEquals(hash1, hash2);
		}

		public void TestGetCookieName()
		{
			var loginName = "first.last_another@wisetechglobal.com";
			var invalidCharacters = new[] { '+', '=', '@', '!', '#', '$', '^', '&', '*', '(', ')', '<', '>', '?', '/', ':', ';', '.', ',', '"', '{', '}', '[', ']', '\\', '|' };

			var cookieName = DeviceCookieHelper.GetCookieName(loginName);
			AssertEquals(false, ContainsAny(cookieName, invalidCharacters));
			AssertEquals(false, cookieName.Contains("wisetechglobal"));
			AssertEquals(true, cookieName.StartsWith("DeviceCookie_"));

			var loginName2 = "a*b!c+d.e#f$g^h[i{j|k&l,wisetech;n>o)p\tq rs";
			var cookieName2 = DeviceCookieHelper.GetCookieName(loginName2);
			AssertEquals(false, ContainsAny(cookieName2, invalidCharacters));
			AssertEquals(false, cookieName2.Contains("wisetech"));
			AssertEquals(true, cookieName2.StartsWith("DeviceCookie_"));

			var loginNameChinese = "他們爲什麽不說中文";
			var cookieNameChinese = DeviceCookieHelper.GetCookieName(loginNameChinese);
			AssertEquals(false, ContainsAny(cookieNameChinese, invalidCharacters));
			AssertEquals(true, cookieNameChinese.StartsWith("DeviceCookie_"));

			var loginNameArabic = "ليهمابتكلموشعربي؟";
			var cookieNameArabic = DeviceCookieHelper.GetCookieName(loginNameArabic);
			AssertEquals(false, ContainsAny(cookieNameArabic, invalidCharacters));
			AssertEquals(true, cookieNameArabic.StartsWith("DeviceCookie_"));
		}

		public void TestTemporaryRemoveOldCookie()
		{
			var loginName = "first.last_another@wisetechglobal.com";
			TestCookie.WriteLoginHashCookie(loginName);

			var badLoginName = "WEB_LOGIN_SUCCESS_HASHbad@cookie.com";
			var cookie = new HttpCookie(badLoginName);
			cookie.Expires = Env.Time.CurrentLocalDateTime.AddYears(1);
			cookie.Values.Add("1", "somevalue");
			cookie.Values.Add("2", "anyvalue");
			TestCookie.RequestCookies_Exposed.Add(cookie);

			badLoginName = "WEB_LOGIN_SUCCESS_HASH_cookie@to.remove";
			cookie = new HttpCookie(badLoginName);
			cookie.Expires = Env.Time.CurrentLocalDateTime.AddYears(1);
			TestCookie.RequestCookies_Exposed.Add(cookie);

			badLoginName = "WEB_LOGIN_SUCCESS_HASH_has_path";
			cookie = new HttpCookie(badLoginName);
			cookie.Expires = Env.Time.CurrentLocalDateTime.AddYears(1);
			cookie.Path = "/WebSite";
			TestCookie.RequestCookies_Exposed.Add(cookie);

			foreach (string key in TestCookie.ResponseCookies_Exposed)
			{
				Assert(TestCookie.ResponseCookies_Exposed[key].Expires > Env.Time.CurrentLocalDateTime);
			}

			TestCookie.TemporaryRemoveOldCookie();

			foreach (string key in TestCookie.ResponseCookies_Exposed)
			{
				var responseCookie = TestCookie.ResponseCookies_Exposed[key];
				if (responseCookie.Name.StartsWith("WEB_LOGIN_SUCCESS_HASH"))
				{
					Assert("Cookies generated by old logic should be removed", responseCookie.Expires < Env.Time.CurrentLocalDateTime);
				}
				else
				{
					Assert("Other cookies should not change", responseCookie.Expires > Env.Time.CurrentLocalDateTime);
				}
				AssertEquals(TestCookie.RequestCookies_Exposed[key].Path, responseCookie.Path);
			}
		}

		public void TestLoadLoginHash_InvalidCookie()
		{
			var loginName = "testLogin";

			// setup
			TestCookie.WriteLoginHashCookie(loginName);
			var validCookie = TestCookie.ResponseCookies_Exposed[DeviceCookieHelper.GetCookieName(loginName)];
			AssertNotNull(TestCookie.LoadLoginHash(loginName));

			TestCookie.RequestCookies_Exposed.Clear();
			var cookie = new HttpCookie(validCookie.Name);
			cookie.Expires = validCookie.Expires;
			cookie.Values.Add("0", "hash");
			cookie.Values.Add("1", "nonce");
			TestCookie.ResponseCookies_Exposed.Add(cookie);
			AssertNull("Return null if cookie structure is incompatible", TestCookie.LoadLoginHash(loginName));

			TestCookie.RequestCookies_Exposed.Clear();
			cookie = new HttpCookie(loginName);
			cookie.Expires = validCookie.Expires;
			cookie.Value = validCookie.Value;
			TestCookie.ResponseCookies_Exposed.Add(cookie);
			AssertNull("Return null if cookie name is incorrect", TestCookie.LoadLoginHash(loginName));

			TestCookie.RequestCookies_Exposed.Clear();
			cookie = new HttpCookie(validCookie.Name);
			cookie.Expires = validCookie.Expires;
			cookie.Value = validCookie.Value.Substring(1);
			TestCookie.ResponseCookies_Exposed.Add(cookie);
			AssertNull("Return null if cookie value is not valid base64", TestCookie.LoadLoginHash(loginName));

			TestCookie.WriteLoginHashCookie("someone");
			var anotherHashValue = TestCookie.ResponseCookies_Exposed[DeviceCookieHelper.GetCookieName("someone")].Value;

			TestCookie.RequestCookies_Exposed.Clear();
			cookie = new HttpCookie(validCookie.Name);
			cookie.Expires = validCookie.Expires;
			cookie.Value = anotherHashValue;
			TestCookie.ResponseCookies_Exposed.Add(cookie);
			AssertNull("Return null if hash/nonce is not valid for current user", TestCookie.LoadLoginHash(loginName));
		}

		public void TestLoadLoginHash_NoSecretkey()
		{
			var loginName = "testLogin";

			// setup
			TestCookie.WriteLoginHashCookie(loginName);
			AssertNotNull(TestCookie.LoadLoginHash(loginName));

			ClearKey(WebDataRegistry.Instance.LoginFailureAttemptSecretKey.Name);
			AssertNull("Return null if LoginFailureAttemptSecretKey is not set", TestCookie.LoadLoginHash(loginName));
			Assert(ErrorReporter.HasBeenReported("LoginFailureAttemptSecretKeyNotSet"));
			ErrorReporter.Clear();
		}

		[HttpContextEnabledTest]
		public void TestCookiePath()
		{
			var loginName = "first.last_another@wisetechglobal.com";
			TestCookie.WriteLoginHashCookie(loginName);

			var cookie = TestCookie.RequestCookies_Exposed[DeviceCookieHelper.GetCookieName(loginName)];
			AssertEquals(HttpContext.Current.Request.ApplicationPath, cookie.Path);
		}

		bool ContainsAny(string value, char[] invalidCharacters)
		{
			return value.IndexOfAny(invalidCharacters) != -1;
		}

		protected override void SetUp()
		{
			base.SetUp();

			WebDataRegistry.Instance.LoginFailureAttemptSecretKey.SetValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				"8F909455805C9A77DFD61579E86B54880FBBC194CE943F08B006B5A5488FBA1EF805BF4374BED6BF653FA096AF5BE06694459160390312A0C5B69AC2019E8DA5");
		}

		static void ClearKey(string name)
		{
			using (var command = Db.Connection.Command("UPDATE dbo.StmData SET SD_BinaryValue = NULL WHERE SD_Name = @Name"))
			{
				command.AddParameter("@Name", SqlDbType.VarChar, name);
				command.ExecuteNonQuery();
			}

			WebDataRegistry.Instance.RemoveItemFromCacheIfOlderThan(name, TimeSpan.FromSeconds(-1));
		}

		class LoginSuccessCookieHelperForTest : LoginSuccessCookieHelper
		{
			public HttpCookieCollection ResponseCookies_Exposed => ResponseCookies;
			public HttpCookieCollection RequestCookies_Exposed => RequestCookies;

			public bool CookieExist(string loginName)
			{
				var cookie = RequestCookies[DeviceCookieHelper.GetCookieName(loginName)];
				return cookie != null && (cookie.Expires == DateTime.MinValue || cookie.Expires > Env.Time.CurrentLocalDateTime); // Don't need a DB hit for setting a cookie
			}
		}
	}
}
