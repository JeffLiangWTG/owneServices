using System;
using System.Globalization;
using System.Net;
using System.Web;
using System.Web.Configuration;
using Enterprise.ZArchitecture.Web.Security.Configuration;

namespace Enterprise.ZArchitecture.Web.Security
{
	/// <summary>
	/// Represents an evaluator for requests that 
	/// </summary>
	public static class RequestEvaluator
	{
		/// <summary>
		/// Evaluates a given request against specified settings for the type of security action required
		/// to fulfill the request properly.
		/// </summary>
		/// <param name="request">The request to evaluate.</param>
		/// <param name="settings">The settings to evaluate against.</param>
		/// <param name="forceEvaluation">
		/// A flag indicating whether or not to force evaluation, despite the mode set.
		/// </param>
		/// <returns>A SecurityType value for the appropriate action.</returns>
		public static SecurityType Evaluate(HttpRequest request, SecureWebPageSettings settings, bool forceEvaluation)
		{
			// Initialize the result to Ignore.
			SecurityType result = SecurityType.Ignore;

			// Determine if this request should be ignored based on the settings' Mode.
			if (forceEvaluation || RequestMatchesMode(request, settings.Mode))
			{
				// Make sure the request shouldn't be ignored as a HTTP handler.
				if (settings.IgnoreHandlers == SecureWebPageIgnoreHandlers.BuiltIn && !IsBuiltInHandlerRequest(request) ||
					settings.IgnoreHandlers == SecureWebPageIgnoreHandlers.WithStandardExtensions && !IsStandardHandlerRequest(request) ||
					settings.IgnoreHandlers == SecureWebPageIgnoreHandlers.None)
				{
					// Get the relative file path of the current request from the application root.
					string relativeFilePath = WebUtility.UrlDecode(request.Url.AbsolutePath).Remove(0, request.ApplicationPath.Length).ToLower(CultureInfo.CurrentCulture);
					if (relativeFilePath.StartsWith("/"))
					{
						// Remove any leading "/".
						relativeFilePath = relativeFilePath.Substring(1);
					}

					// Get the relative directory of the current request by removing the last segment of the RelativeFilePath.
					string relativeDirectory = string.Empty;
					int i = relativeFilePath.LastIndexOf('/');
					if (i >= 0)
					{
						relativeDirectory = relativeFilePath.Substring(0, i);
					}

					// Determine if there is a matching file path for the current request.
					i = settings.Files.IndexOf(relativeFilePath);
					if (i >= 0)
					{
						result = settings.Files[i].Secure;
					}
					else
					{
						// Try to find a matching directory path.
						int j = -1;
						i = 0;
						while (i < settings.Directories.Count)
						{
							// Try to match the beginning of the directory if recursion is allowed (partial match).
							if ((settings.Directories[i].Recurse && relativeDirectory.StartsWith(settings.Directories[i].Path, StringComparison.CurrentCultureIgnoreCase) ||
								relativeDirectory.Equals(settings.Directories[i].Path, StringComparison.CurrentCultureIgnoreCase)) &&
								(j == -1 || settings.Directories[i].Path.Length > settings.Directories[j].Path.Length))
							{
								// First or longer partial match found (deepest recursion is the best match).
								j = i;
							}

							i++;
						}

						if (j > -1)
						{
							// Indicate a match for a partially matched directory allowing recursion.
							result = settings.Directories[j].Secure;
						}
						else
						{
							// No match indicates an insecure result.
							result = SecurityType.Insecure;
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Evaluates a given request against configured settings for the type of security action required
		/// to fulfill the request properly.
		/// </summary>
		/// <param name="request">The request to evaluate.</param>
		/// <returns>A SecurityType value for the appropriate action.</returns>
		public static SecurityType Evaluate(HttpRequest request)
		{
			// Get the settings for the secureWebPages section.
			SecureWebPageSettings settings = WebConfigurationManager.GetSection("secureWebPages") as SecureWebPageSettings;

			return Evaluate(request, settings, false);
		}

		/// <summary>
		/// Determines if the specified request is for one of the built-in HTTP handlers.
		/// </summary>
		/// <param name="request">The HttpRequest to test.</param>
		/// <returns>True if the request is for a built-in HTTP handler; false otherwise.</returns>
		static bool IsBuiltInHandlerRequest(HttpRequest request)
		{
			// Get the file name of the request.
			string fileName = request.Url.Segments[request.Url.Segments.Length - 1];
			return (
				string.Compare(fileName, "trace.axd", true, CultureInfo.InvariantCulture) == 0 ||
				string.Compare(fileName, "webresource.axd", true, CultureInfo.InvariantCulture) == 0
			);
		}

		/// <summary>
		/// Determines if the specified request is for a standard HTTP handler (.axd).
		/// </summary>
		/// <param name="request">The HttpRequest to test.</param>
		/// <returns>True if the request is for a standard HTTP handler (.axd or .ashx); false otherwise.</returns>
		static bool IsStandardHandlerRequest(HttpRequest request)
		{
			string path = request.Url.AbsolutePath;
			return (path.EndsWith(".axd", true, CultureInfo.InvariantCulture) || path.EndsWith(".ashx", true, CultureInfo.InvariantCulture));
		}

		/// <summary>
		/// Tests the given request to see if it matches the specified mode.
		/// </summary>
		/// <param name="request">A HttpRequest to test.</param>
		/// <param name="mode">The SecureWebPageMode used in the test.</param>
		/// <returns>
		///		Returns true if the request matches the mode as follows:
		///		<list type="disc">
		///			<item>If mode is On.</item>
		///			<item>If mode is set to RemoteOnly and the request is from a computer other than the server.</item>
		///			<item>If mode is set to LocalOnly and the request is from the server.</item>
		///		</list>
		///	</returns>
		static bool RequestMatchesMode(HttpRequest request, SecureWebPageMode mode)
		{
			switch (mode)
			{
				case SecureWebPageMode.On:
					return true;

				case SecureWebPageMode.RemoteOnly:
					return (request.ServerVariables["REMOTE_ADDR"] != request.ServerVariables["LOCAL_ADDR"]);

				case SecureWebPageMode.LocalOnly:
					return (request.ServerVariables["REMOTE_ADDR"] == request.ServerVariables["LOCAL_ADDR"]);

				default:
					return false;
			}
		}
	}
}
