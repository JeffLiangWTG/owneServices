using System;
using System.Web;
using Enterprise.ZArchitecture.Web.Security.Configuration;

namespace Enterprise.ZArchitecture.Web.Security
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Provides static methods for ensuring that a page is rendered 
	/// securely via SSL or unsecurely.
	/// </summary>
	public sealed class SslHelper
	{
		// Protocol prefixes.
		const string UnsecureProtocolPrefix = "http://";
		const string SecureProtocolPrefix = "https://";

		/// <summary>
		/// Prevent creating an instance of this class.
		/// </summary>
		SslHelper()
		{
		}

		/// <summary>
		/// Determines the secure page that should be requested if a redirect occurs.
		/// </summary>
		/// <param name="settings">The SecureWebPageSettings to use in determining.</param>
		/// <param name="ignoreCurrentProtocol">
		/// A flag indicating whether or not to ingore the current protocol when determining.
		/// </param>
		/// <returns>A string containing the absolute URL of the secure page to redirect to.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string DetermineSecurePage(SecureWebPageSettings settings, bool ignoreCurrentProtocol)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			string result = null;
			HttpRequest request = HttpContext.Current.Request;

			// Is this request already secure?
			string requestPath = request.Url.AbsoluteUri;
			if (ignoreCurrentProtocol || requestPath.StartsWith(UnsecureProtocolPrefix))
			{
				// Is there a different URI to redirect to?
				if (string.IsNullOrEmpty(settings.EncryptedUri))
				{
					// Replace the protocol of the requested URL with "https".
					// * Account for cookieless sessions by applying the application modifier.
					result = string.Concat(
						SecureProtocolPrefix,
						request.Url.Authority,
						HttpContext.Current.Response.ApplyAppPathModifier(request.Path),
						request.Url.Query
					);
				}
				else
				{
					// Build the URL with the "https" protocol.
					result = BuildUrl(true, settings.MaintainPath, settings.EncryptedUri, settings.UnencryptedUri);
				}
			}

			return result;
		}

		/// <summary>
		/// Determines the unsecure page that should be requested if a redirect occurs.
		/// </summary>
		/// <param name="settings">The SecureWebPageSettings to use in determining.</param>
		/// <param name="ignoreCurrentProtocol">
		/// A flag indicating whether or not to ingore the current protocol when determining.
		/// </param>
		/// <returns>A string containing the absolute URL of the unsecure page to redirect to.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static string DetermineUnsecurePage(SecureWebPageSettings settings, bool ignoreCurrentProtocol)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			string result = null;
			HttpRequest request = HttpContext.Current.Request;

			// Is this request secure?
			string requestPath = request.Url.AbsoluteUri;
			if (ignoreCurrentProtocol || requestPath.StartsWith(SecureProtocolPrefix))
			{
				// Is there a different URI to redirect to?
				if (string.IsNullOrEmpty(settings.UnencryptedUri))
				{
					// Replace the protocol of the requested URL with "http".
					// * Account for cookieless sessions by applying the application modifier.
					result = string.Concat(
						UnsecureProtocolPrefix,
						request.Url.Authority,
						HttpContext.Current.Response.ApplyAppPathModifier(request.Path),
						request.Url.Query
					);
				}
				else
				{
					// Build the URL with the "http" protocol.
					result = BuildUrl(false, settings.MaintainPath, settings.EncryptedUri, settings.UnencryptedUri);
				}
			}

			return result;
		}

		/// <summary>
		/// Requests the current page over a secure connection, if it is not already.
		/// </summary>
		/// <param name="settings">The SecureWebPageSettings to use for this request.</param>
		public static void RequestSecurePage(SecureWebPageSettings settings)
		{
			// Determine the response path, if any.
			string responsePath = DetermineSecurePage(settings, false);
			if (!string.IsNullOrEmpty(responsePath))
			{
				// Redirect to the secure page.
				HttpContext.Current.Response.Redirect(responsePath, true);
			}
		}

		/// <summary>
		/// Requests the current page over an unsecure connection, if it is not already.
		/// </summary>
		/// <param name="settings">The SecureWebPageSettings to use for this request.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public static void RequestUnsecurePage(SecureWebPageSettings settings)
		{
			if (settings == null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			// Determine the response path, if any.
			string responsePath = DetermineUnsecurePage(settings, false);
			if (!string.IsNullOrEmpty(responsePath))
			{
				HttpRequest request = HttpContext.Current.Request;

				// Test for the need to bypass a security warning.
				bool bypass;
				if (settings.WarningBypassMode == SecurityWarningBypassMode.AlwaysBypass)
				{
					bypass = true;
				}
				else if (settings.WarningBypassMode == SecurityWarningBypassMode.BypassWithQueryParam &&
						request.QueryString[settings.BypassQueryParamName] != null)
				{
					bypass = true;

					// Remove the bypass query parameter from the URL.
					System.Text.StringBuilder newPath = new System.Text.StringBuilder(responsePath);
					int i = responsePath.LastIndexOf(string.Format("?{0}=", settings.BypassQueryParamName));
					if (i < 0)
					{
						i = responsePath.LastIndexOf(string.Format("&{0}=", settings.BypassQueryParamName));
					}

					newPath.Remove(i, settings.BypassQueryParamName.Length + request.QueryString[settings.BypassQueryParamName].Length + 1);

					// Remove any abandoned "&" character.
					if (i >= newPath.Length)
					{
						i = newPath.Length - 1;
					}

					if (newPath[i] == '&')
					{
						newPath.Remove(i, 1);
					}

					// Remove any abandoned "?" character.
					i = newPath.Length - 1;
					if (newPath[i] == '?')
					{
						newPath.Remove(i, 1);
					}

					responsePath = newPath.ToString();
				}
				else
				{
					bypass = false;
				}

				// Output a redirector for the needed page to avoid a security warning.
				HttpResponse response = HttpContext.Current.Response;
				if (bypass)
				{
					// Clear the current response.
					response.Clear();

					// Add a refresh header to the response for the new path.
					response.AddHeader("Refresh", string.Concat("0;URL=", responsePath));

					// Also, add JavaScript to replace the current location as backup.
					response.Write("<html><head><title></title>");
					response.Write("<!-- <script language=\"javascript\">window.location.replace(\"");
					response.Write(responsePath);
					response.Write("\");</script> -->");
					response.Write("</head><body></body></html>");

					response.End();
				}
				else
				{
					// Redirect to the unsecure page.
					response.Redirect(responsePath, true);
				}
			}
		}

		/// <summary>
		/// Builds a URL from the given protocol and appropriate host path. The resulting URL 
		/// will maintain the current path if requested.
		/// </summary>
		/// <param name="secure">Is this to be a secure URL?</param>
		/// <param name="maintainPath">Should the current path be maintained during transfer?</param>
		/// <param name="encryptedUri">The URI to redirect to for encrypted requests.</param>
		/// <param name="unencryptedUri">The URI to redirect to for standard requests.</param>
		/// <returns></returns>
		static string BuildUrl(bool secure, bool maintainPath, string encryptedUri, string unencryptedUri)
		{
			// Clean the URIs.
			encryptedUri = CleanHostUri(string.IsNullOrEmpty(encryptedUri) ? unencryptedUri : encryptedUri);
			unencryptedUri = CleanHostUri(string.IsNullOrEmpty(unencryptedUri) ? encryptedUri : unencryptedUri);

			// Get the current request.
			HttpRequest request = HttpContext.Current.Request;

			// Prepare to build the needed URL.
			System.Text.StringBuilder url = new System.Text.StringBuilder();

			// Host authority (e.g. secure.mysite.com/).
			if (secure)
			{
				url.Append(encryptedUri);
			}
			else
			{
				url.Append(unencryptedUri);
			}

			if (maintainPath)
			{
				// Append the current file path.
				url.Append(request.CurrentExecutionFilePath).Append(request.Url.Query);
			}
			else
			{
				// Append just the current page
				string currentUrl = request.Url.AbsolutePath;
				url.Append(currentUrl.Substring(currentUrl.LastIndexOf('/') + 1)).Append(request.Url.Query);
			}

			// Replace any double slashes with a single slash.
			url.Replace("//", "/");

			// Prepend the protocol.
			if (secure)
			{
				url.Insert(0, SecureProtocolPrefix);
			}
			else
			{
				url.Insert(0, UnsecureProtocolPrefix);
			}

			return url.ToString();
		}

		/// <summary>
		/// Cleans a host path by stripping out any unneeded elements.
		/// </summary>
		/// <param name="uri">The host URI to validate.</param>
		/// <returns>Returns a string that is stripped as needed.</returns>
		static string CleanHostUri(string uri)
		{
			string result = string.Empty;
			if (!string.IsNullOrEmpty(uri))
			{
				// Ensure there is a protocol or a Uri cannot be constructed.
				if (!uri.StartsWith(UnsecureProtocolPrefix) && !uri.StartsWith(SecureProtocolPrefix))
				{
					uri = UnsecureProtocolPrefix + uri;
				}

				// Extract the authority and path to build a string suitable for our needs.
				Uri hostUri = new Uri(uri);
				result = string.Concat(hostUri.Authority, hostUri.AbsolutePath);
				if (!result.EndsWith("/"))
				{
					result += "/";
				}
			}

			return result;
		}
	}

	#endregion
}
