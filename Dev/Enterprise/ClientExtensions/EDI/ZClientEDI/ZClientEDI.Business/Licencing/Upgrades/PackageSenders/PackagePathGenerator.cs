using System.IO;
using System.Net;
using CargoWise.Common;
using Enterprise.MailManager.FileDownload;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public static class PackagePathGenerator
	{
		public static string GenerateWebServerRootPath(string enterpriseCode)
		{
			string result;
			if (IsClientSpecific(enterpriseCode))
			{
				result = Path.Combine(UpgradeConstants.WebServerClientSpecificPath, enterpriseCode);
			}
			else
			{
				result = UpgradeConstants.WebServerGenericPath;
			}
			return result;
		}

		public static string GenerateHttpPath(string enterpriseCode, string fileName)
		{
			string rootPath;
			if (IsClientSpecific(enterpriseCode))
			{
				rootPath = CombineWebPath(UpgradeConstants.HttpClientSpecificBaseUrl, enterpriseCode);
			}
			else
			{
				rootPath = UpgradeConstants.HttpGenericBaseUrl;
			}

			string result = CombineWebPath(rootPath, fileName);

			string user = UpgradeConstants.HttpDownloadUserName;
			string password = UpgradeConstants.HttpDownloadPassword;
			if (!string.IsNullOrEmpty(user))
			{
				SecureQueryString qs = new SecureQueryString();
				qs.Add(WebProtocolSupport.UserQueryStringKey, user);
				qs.Add(WebProtocolSupport.PasswordQueryStringKey, password);
				result += '?' + SecureQueryString.QueryStringKey + '=' + WebUtility.UrlEncode(qs.ToString());
			}
			return result;
		}

		static string CombineWebPath(string path1, string path2)
		{
			string result = path1;
			if (path1[path1.Length - 1] != WebPathSeparator)
			{
				result += WebPathSeparator;
			}
			return result + path2;
		}

		static bool IsClientSpecific(string enterpriseCode)
		{
			return (enterpriseCode != null) && (enterpriseCode.Length > 0);
		}

		const char WebPathSeparator = '/';
	}
}

