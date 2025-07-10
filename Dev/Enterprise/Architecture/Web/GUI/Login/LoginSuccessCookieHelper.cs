using System.Linq;
using System.Web;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.GUI.Cookie;

namespace Enterprise.ZArchitecture.Web.GUI
{
	/// <summary>
	/// The link below was used as guide for this implementation, as we should be concerned about DOS attacks when locking out users
	/// https://owasp.org/www-community/Slow_Down_Online_Guessing_Attacks_with_Device_Cookies#register-failed-authentication-attempt
	/// if the link was taken down, a copy of this page can be found in the work item this logic was introduced: "WI00426598 - WebTracker Portal Password Policy - account lockout"
	/// Alternative link: https://web.archive.org/web/20211106080303/https://owasp.org/www-community/Slow_Down_Online_Guessing_Attacks_with_Device_Cookies
	/// </summary>
	class LoginSuccessCookieHelper : ZBaseCookie
	{
		public LoginSuccessCookieHelper()
			: base(string.Empty) // cookie name is generated
		{
		}

		public void WriteLoginHashCookie(string loginName)
		{
			if (!TryGetSecretKey(out var secretKey))
			{
				return;
			}

			var cookie = new HttpCookie(DeviceCookieHelper.GetCookieName(loginName));
			cookie.Expires = Env.Time.CurrentLocalDateTime.AddYears(1); // Don't need a DB hit for setting a cookie

			var path = CookiePath;
			if (!string.IsNullOrEmpty(path))
			{
				cookie.Path = path;
			}

			cookie.Value = DeviceCookieHelper.GenerateDeviceCookieValue(loginName, secretKey);
			ResponseCookies.Set(cookie);
		}

		public byte[] LoadLoginHash(string loginName)
		{
			if (!TryGetSecretKey(out var secretKey))
			{
				return null;
			}

			var cookieValue = RequestCookies[DeviceCookieHelper.GetCookieName(loginName)]?.Value;

			if (cookieValue != null
				&& DeviceCookieHelper.TryParse(loginName, cookieValue, secretKey, out var deviceCookie))
			{
				return deviceCookie.Value.Hash;
			}

			return null;
		}

		string CookiePath => HttpContext.Current?.Request?.ApplicationPath;

		static bool TryGetSecretKey(out byte[] secretkey)
		{
			var registryItem = WebDataRegistry.Instance.LoginFailureAttemptSecretKey;
			if (string.IsNullOrEmpty(registryItem.Value))
			{
				ErrorReporter.ReportOnce("LoginFailureAttemptSecretKeyNotSet", "LoginFailureAttemptSecretKey is not set.");

				secretkey = null;
				return false;
			}

			secretkey = registryItem.DataType.Serialise(registryItem.Value);
			return true;
		}

		#region This is temporary, this method should be remove in the future

		// This method was created to remove old cookies introduced in the initial implementation of this class
		public void TemporaryRemoveOldCookie()
		{
			var cookieKeysToRemove = RequestCookies.AllKeys.Where(c => c.StartsWith("WEB_LOGIN_SUCCESS_HASH"));

			// We can't actually use ResponseCookies.Remove, it doesn't remove the cookie.
			// The way to remove the cookie is to set the Expire date, by doing this, the cookies gets removed from the browser
			// https://stackoverflow.com/questions/6635349/how-to-delete-cookies-on-an-asp-net-website

			var path = CookiePath;
			foreach (var item in cookieKeysToRemove)
			{
				ResponseCookies[item].Expires = Env.Time.CurrentLocalDateTime.AddDays(-1);
				ResponseCookies[item].Path = path;
			}
		}

		#endregion
	}
}
