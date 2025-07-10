using System;
using System.Web;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Web.GUI.Cookie
{
	public class ZApplicationCookie : ZBaseCookie
	{
		public ZApplicationCookie(string cookieName) : base(cookieName)
		{ }

		public void WriteUser(string companyCode, string userEmail)
		{
			WriteUser(companyCode, userEmail, "");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Don't need a DB hit for setting a cookie")]
		public void WriteUser(string companyCode, string userEmail, string password)
		{
			HttpCookie cookie = new HttpCookie(CookieName);
			cookie.Expires = DateTime.Now.AddMonths(CookieTimeoutInMonths);// Don't need a DB hit for setting a cookie

			cookie.Values.Add(CompanyCodeKey, Encoder.Encrypt(companyCode));
			cookie.Values.Add(UserEmailKey, Encoder.Encrypt(userEmail));
			cookie.Values.Add(UserPasswordKey, Encoder.Encrypt(password));
			ResponseCookies.Add(cookie);
		}

		public string GetCompanyCode()
		{
			return GetValue(CompanyCodeKey);
		}

		public string GetUserEmail()
		{
			return GetValue(UserEmailKey);
		}

		public string GetUserPassword()
		{
			return GetValue(UserPasswordKey);
		}

		const string CompanyCodeKey = "0";
		const string UserEmailKey = "1";
		const string UserPasswordKey = "2";

		string GetValue(string name)
		{
			HttpCookie cookie = RequestCookies[CookieName];
			try
			{
				if (cookie != null)
				{
					return Encoder.Decrypt(cookie.Values[name]);
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
			return "";
		}
	}
}
