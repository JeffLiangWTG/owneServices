using System;
using Enterprise.ZArchitecture.Environment;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	class ApplicationRedirectUrlItem
	{
		public ApplicationRedirectUrlItem(string applicationName, StringRegistryItem redirectUrlRegistryItem, string postPath = "", string redirectUrlType = RedirectUrlTypes.SinglePage) : this(applicationName, GetRedirectUrl(redirectUrlRegistryItem?.Value, postPath), redirectUrlType)
		{
		}

		static string GetRedirectUrl(string redirectUrlRegistryItemValue, string postPath)
		{
			if (!string.IsNullOrEmpty(redirectUrlRegistryItemValue))
			{
				return string.IsNullOrEmpty(postPath)
					? redirectUrlRegistryItemValue
					: redirectUrlRegistryItemValue.TrimEnd('/') + '/' + postPath.TrimStart('/');
			}
			else
			{
				return null;
			}
		}

		public ApplicationRedirectUrlItem(string applicationName, string redirectUrl, string redirectUrlType = RedirectUrlTypes.SinglePage)
		{
			if (RedirectUrlTypes.IsValidType(redirectUrlType))
			{
				RedirectUrlType = redirectUrlType;
			}
			else
			{
				throw new InvalidOperationException($"Invalid redirect type: {redirectUrlType}");
			}

			ApplicationName = applicationName;
			RedirectUrl = redirectUrl;
		}

		public string ApplicationName { get; private set; }
		public string RedirectUrlType { get; private set; }
		public string RedirectUrl { get; private set; }

		public IdentityRedirectUrlRequest Request => new IdentityRedirectUrlRequest()
		{
			ApplicationName = ApplicationName,
			RedirectUrl = RedirectUrl,
			RedirectUrlType = RedirectUrlType
		};
	}

	static class RedirectUrlTypes
	{
		public const string Web = "WEB";
		public const string SinglePage = "SPA";
		public const string InstalledClient = "ICL";

		internal static bool IsValidType(string type)
		{
			return Web == type || SinglePage == type || InstalledClient == type;
		}
	}
}
