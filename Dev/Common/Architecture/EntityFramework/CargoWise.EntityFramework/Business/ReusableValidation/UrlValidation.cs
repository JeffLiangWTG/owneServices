using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// (Mostly) extracted from OrgHeader.
	/// </summary>
	public class UrlValidation : ValidationProvider
	{
		public static bool IsValidUrl(ZString url)
		{
			ZBool result = false;
			ZString urlLower = url.ToLower();

			if (urlLower.StartsWith("http://") && (url.LastIndexOf(".") > 7) && (url[url.Length - 1] != '.'))
			{
				result = true;
			}
			else if (urlLower.StartsWith("www.") && (url[url.Length - 1] != '.') && !urlLower.StartsWith("www.."))
			{
				result = true;
			}
			else if (urlLower.StartsWith("https://") && (url.LastIndexOf(".") > 8) && (url[url.Length - 1] != '.'))
			{
				result = true;
			}

			return result;
		}

		public static bool IsValidAbsoluteUrl(ZString url, string scheme)
		{
			Uri uri;
			return Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.Scheme == scheme;
		}

		public static bool IsValidAbsoluteHttpOrHttpsUrl(ZString url)
		{
			Uri uri;
			return Uri.TryCreate(url, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
		}
	}
}
