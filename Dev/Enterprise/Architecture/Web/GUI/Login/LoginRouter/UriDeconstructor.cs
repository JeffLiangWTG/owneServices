using System;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public class UriDeconstructor
	{
		/// <summary>
		/// Allows extraction of Base Url and Query for Absolute and Relative URIs alike
		/// </summary>
		/// <param name="uri"></param>
		public UriDeconstructor(Uri uri)
		{
			var routingUrlString = uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.OriginalString;
			var urlSplit = routingUrlString.Split('?');
			OriginalUri = uri;
			BaseUrl = urlSplit[0];
			Query = urlSplit.Length > 1 ? urlSplit[1] : string.Empty;
		}

		public Uri OriginalUri { get; }
		public string BaseUrl { get; }
		public string Query { get; }

		static UriKind GetUriKind(Uri uri) => uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative;

		public Uri GetUriWithNewQuery(string query) => new Uri(GetUriPathAndQuery(query), GetUriKind(OriginalUri));

		public string GetUriPathAndQuery(string query) => FormattableString.Invariant($"{BaseUrl}?{query}"); // Untranslatable Uri string
	}
}
