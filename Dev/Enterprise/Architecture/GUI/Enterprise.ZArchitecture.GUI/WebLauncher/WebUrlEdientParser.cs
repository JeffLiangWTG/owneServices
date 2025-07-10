using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.WebLauncher
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer")]
	public class WebUrlEdientParser : IWebUrlEdientParser
	{
		readonly Uri[] validUris;

		public WebUrlEdientParser()
		{
			validUris = PopulateValidHosts([DataRegistry.Instance.WebVersionLaunchUrl, WebDataRegistry.Instance.RootServicesUri.Value]).ToArray();
		}

		public WebUrlEdientParser(Uri[] validUris)
		{
			this.validUris = validUris;
		}

		internal IEnumerable<Uri> PopulateValidHosts(string[] urls)
		{
			foreach (var url in urls)
			{
				if (!url.IsNullOrEmpty())
				{
					if (Uri.TryCreate(url,UriKind.Absolute, out var newUri))
					{
						yield return newUri;
					}
				}
			}
		}

		internal Uri MatchValidHost(Uri requestUri)
		{
			foreach (var validUri in validUris)
			{
				if (Uri.Compare(validUri, requestUri, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					if (requestUri.AbsolutePath.StartsWith(validUri.AbsolutePath))
					{
						return validUri;
					}
				}
			}
			return null;
		}

		internal bool InternalParse(string uri, out string[] segments, out string query)
		{
			segments = null;
			query = null;

			if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
			{
				return false;
			}

			Uri matchedUri;
			if ((matchedUri = MatchValidHost(parsedUri)) == null)
			{
				return false;
			}

			var relativeParsedUri = matchedUri.MakeRelativeUri(parsedUri);
			if (!relativeParsedUri.ToString().StartsWith("link/"))
			{
				return false;
			}

			var relativeUriSplit = relativeParsedUri.ToString().Replace("link/", "").Split(['?']);

			segments = relativeUriSplit[0].Split(new char[] { '/' }, options: StringSplitOptions.RemoveEmptyEntries);    // Similar to using Uri.Segments, but gives a cleaner result.
			if (segments.Length != 3)
			{
				return false;
			}

			if (relativeUriSplit.Length == 2 && relativeUriSplit[1].Length > 1)
			{
				query = relativeUriSplit[1];
			}

			return true;
		}

		internal string InternalGenerateEdientUrl(string[] segments, string query)
		{
			var queryString = new QueryString
			{
				{ "Command", segments[0] },
				{ "ControllerID", segments[1] },
				{ "BusinessEntityPK", segments[2] },
				new QueryString(query)
			};
			return UrlHandler.GetUrlFromQueryString(queryString);
		}

		public bool TryParse(string urlString, out string result)
		{
			var success = InternalParse(urlString, out var segments, out var query);
			result = success ? InternalGenerateEdientUrl(segments, query) : null;
			return success;
		}

		public string Parse(string urlString)
		{
			var success = TryParse(urlString, out var result);
			if (!success)
			{
				throw new ArgumentException($"Invalid edient URL: {urlString}", nameof(urlString));
			}
			return result;
		}
	}
}
