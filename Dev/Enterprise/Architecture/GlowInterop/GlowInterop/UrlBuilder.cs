using System;
using System.Collections.Generic;
using System.Net.Http;
using CargoWise.Application;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public static class UrlBuilder
	{
		public static Uri GenerateURL(Uri baseURL, string relativePath, string fragment = default, IEnumerable<(string Name, string Value)> additionalQueryStrings = default, Guid? branch = null)
		{
			var token = branch == null ? ObjectFactory.Get<IGlowSingleSignOnTokenProvider>().CreateLimitedToken() :
				ObjectFactory.Get<IGlowSingleSignOnTokenProvider>().CreateLimitedTokenSpecificBranch(branch);

			return GenerateURLCore(baseURL, relativePath, fragment, token, additionalQueryStrings);
		}

		public static Uri GenerateCaptiveSessionURL(Uri baseURL, string relativePath, string fragment = default, IEnumerable<(string Name, string Value)> additionalQueryStrings = default)
		{
			var token = ObjectFactory.Get<IGlowSingleSignOnTokenProvider>().CreateCaptiveLimitedToken();

			return GenerateURLCore(baseURL, relativePath, fragment, token, additionalQueryStrings);
		}

		public static Uri GenerateURLForContact(Guid contactPK, Uri baseURL, string relativePath, string fragment = default, IEnumerable<(string Name, string Value)> additionalQueryStrings = default)
		{
			var token = ObjectFactory.Get<IGlowSingleSignOnTokenProvider>().CreateLimitedTokenForContact(contactPK);

			return GenerateURLCore(baseURL, relativePath, fragment, token, additionalQueryStrings);
		}

		static Uri GenerateURLCore(Uri baseURL, string relativePath, string fragment, string token, IEnumerable<(string Name, string Value)> additionalQueryStrings = default)
		{
			var path = baseURL.AbsolutePath.TrimEnd('/') + "/" + relativePath.TrimStart('/');
			var queryString = UriExtensions.ParseQueryString(baseURL);
			queryString.Add("sso_otp", token); // query string key
			if (additionalQueryStrings != null)
			{
				foreach (var additional in additionalQueryStrings)
				{
					queryString.Add(additional.Name, additional.Value);
				}
			}

			var builder = new UriBuilder(baseURL)
			{
				Path = path,
				Query = queryString.ToString(),
				Fragment = fragment
			};
			return builder.Uri;
		}
	}

	public static class FormFactor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant")]
		public const string Desktop = "Desktop";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant")]
		public const string Mobile = "Mobile";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant")]
		public const string Tablet = "Tablet";
	}
}
