using System;
using System.Web;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.ZArchitecture.Web.GUI.Cookie
{
	/// <summary>
	/// A generic Cookie class that manages an individual Cookie by localizing the
	/// cookie management into a single class. This means the Cookie's name and
	/// and timing is abstracted.
	///
	/// The GetId() method is the key method here which retrieves a Cookie Id.
	/// If the cookie exists it returns the value, otherwise it generates a new
	/// Id and creates the cookie with the specs of the class and
	///
	/// It's recommended you store this class as a static member off another
	/// object to have
	/// </summary>

	public abstract class ZBaseCookie
	{
		/// <summary>
		/// The name of the Cookie that is used. This value should always be set or
		/// overridden via the constructor.
		/// </summary>
		public string CookieName
		{
			get { return fCookieName; }
		}
		readonly string fCookieName;

		/// <summary>
		/// The timeout of a persistent cookie.
		/// </summary>
		public int CookieTimeoutInMonths = 1;

		public ZBaseCookie(string newCookieName)
		{
			this.fCookieName = newCookieName;
		}

		/// <summary>
		/// Writes the cookie into the response stream with the value passed. The value
		/// is always the UserId.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Don't need a DB hit for setting a cookie")]
		public void WriteCookie(string value)
		{
			HttpCookie cookie = new HttpCookie(CookieName, Encoder.Encrypt(value));
			cookie.Expires = DateTime.Now.AddMonths(CookieTimeoutInMonths);// Don't need a DB hit for setting a cookie

			ResponseCookies.Add(cookie);
		}

		public string ReadCookie()
		{
			HttpCookie cookie = RequestCookies[CookieName];
			if (cookie != null)
			{
				try
				{
					return Encoder.Decrypt(cookie.Value);
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}
			return null;
		}

		/// <summary>
		/// Removes the cookie by clearing it out and expiring it immediately.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Don't need a DB hit for setting a cookie")]
		public void Remove()
		{
			HttpCookie cookie = RequestCookies[CookieName];

			if (cookie != null)
			{
				cookie.Expires = DateTime.Now.AddDays(-1);// Don't need a DB hit for setting a cookie
				ResponseCookies.Add(cookie);
			}
		}

		/// <summary>
		/// Determines whether the cookie exists
		/// <seealso>Class wwCookie</seealso>
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Don't need a DB hit for setting a cookie")]
		public bool CookieExist()
		{
			// *** Check to see if we have a cookie we can use
			HttpCookie cookie = RequestCookies[CookieName];
			return (cookie != null &&
				(cookie.Expires == DateTime.MinValue || cookie.Expires > DateTime.Now));// Don't need a DB hit for checking a cookie
																						//The Expires checks seem redundant http://www.nullskull.com/a/810/aspnet-cookies-faq.aspx
																						//"When the browser sends cookie information to the server, the browser does not include the expiration information. (The cookie's Expires property always returns a date-time value of zero.)"
		}

#if DEBUG
		HttpCookieCollection fResponseRequestCookies;

		internal HttpCookieCollection ResponseRequestCookies
		{
			get
			{
				if (fResponseRequestCookies == null)
				{
					fResponseRequestCookies = new HttpCookieCollection();
				}

				return fResponseRequestCookies;
			}
		}
#endif

		protected HttpCookieCollection ResponseCookies
		{
			get
			{
#if DEBUG
				if (Enterprise.ZArchitecture.Environment.Globals.IsTest && HttpContext.Current == null)
				{
					return ResponseRequestCookies;
				}
#endif
				return HttpContext.Current.Response.Cookies;
			}
		}

		protected HttpCookieCollection RequestCookies
		{
			get
			{
#if DEBUG
				if (Enterprise.ZArchitecture.Environment.Globals.IsTest && HttpContext.Current == null)
				{
					return ResponseRequestCookies;
				}
#endif
				return HttpContext.Current.Request.Cookies;
			}
		}

		#region Encoder

		protected internal TwoWayEncoder Encoder
		{
			get
			{
				if (fEncoder == null)
				{
					fEncoder = new TwoWayEncoder(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));
				}

				return fEncoder;
			}
		}
		TwoWayEncoder fEncoder;

		#endregion
	}
}
