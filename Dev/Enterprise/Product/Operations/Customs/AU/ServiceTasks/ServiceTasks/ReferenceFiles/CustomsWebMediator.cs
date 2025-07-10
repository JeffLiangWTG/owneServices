using System;
#if NETFRAMEWORK
using System.Globalization;
#endif
using System.Net;
#if NETCOREAPP
using System.Net.Http;
using System.Threading.Tasks;
#endif
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.ServiceTasks
{
	public class CustomsWebMediator
	{
		public CustomsWebMediator(BusinessObjectFactory factory, ILogger serviceLogger, bool cmrTestMode)
		{
			this.factory = factory;
			this.serviceLogger = serviceLogger;
			this.cmrTestMode = cmrTestMode;

			WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
		}

		readonly BusinessObjectFactory factory;
		readonly ILogger serviceLogger;
		readonly bool cmrTestMode;
#if NETCOREAPP
		readonly HttpClient httpClient = new();
#endif

		protected internal virtual HttpWebResponse GetWebResponse(HttpWebRequest webRequest)
		{
			var webResponse = (HttpWebResponse)webRequest.GetResponse();
			return webResponse;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public bool GetWebSourceLastModifiedTime(out DateTime lastModified)
		{
			serviceLogger.Log(LogType.Information, "Start to get web source last modified time");
			if (EnvProxy.IsHostedWithCargowise)
			{
				serviceLogger.Log(LogType.Information, "CargoWise One is hosted");
				lastModified = GetLastModifiedTimeCore(AlternateUrl, exception => GetLastModifiedTimeCore(new Uri(Url)));
			}
			else
			{
				serviceLogger.Log(LogType.Information, "CargoWise One is not hosted");
				lastModified = GetLastModifiedTimeCore(new Uri(Url), exception => GetLastModifiedTimeCore(AlternateUrl));
			}
			return lastModified != DateTime.MinValue;
		}

#if NETCOREAPP
		DateTime GetLastModifiedTimeCore(Uri url, Func<HttpRequestException, DateTime> handleException = null)
		{
			serviceLogger.Log(LogType.Information, "Try URL: " + url);
			var lastModified = DateTime.MinValue;

			try
			{
				using var request = new HttpRequestMessage(HttpMethod.Head, url);
				using var response = httpClient.SendAsync(request).Result;

				if (response.IsSuccessStatusCode)
				{
					if (response.Content.Headers.LastModified.HasValue)
					{
						lastModified = response.Content.Headers.LastModified.Value.UtcDateTime;
					}
				}
			}
			catch (HttpRequestException httpEx)
			{
				serviceLogger.Log(LogType.Warning, "HTTP Request Exception: " + httpEx.ToString());
				serviceLogger.Log(LogType.Warning, "URL: " + url);

				if (handleException != null)
				{
					lastModified = handleException.Invoke(httpEx);
				}
			}
			return lastModified;
		}

		public byte[] DownloadReferenceDataZippedFile()
		{
			serviceLogger.Log(LogType.Information, "Start to download reference data zipped file");
			byte[] result;

			if (EnvProxy.IsHostedWithCargowise)
			{
				result = DownloadReferenceDataZippedFileCoreAsync(AlternateUrl, new Uri(Url)).Result;
			}
			else
			{
				result = DownloadReferenceDataZippedFileCoreAsync(new Uri(Url), AlternateUrl).Result;
			}

			return result;
		}

		async Task<byte[]> DownloadReferenceDataZippedFileCoreAsync(Uri url, Uri fallbackUrl)
		{
			byte[] result;
			try
			{
				serviceLogger.Log(LogType.Information, "Try to download from " + url);
				result = await httpClient.GetByteArrayAsync(url);
			}
			catch (HttpRequestException)
			{
				serviceLogger.Log(LogType.Warning, "Fail to download from " + url);
				serviceLogger.Log(LogType.Information, "Try to download from " + fallbackUrl);
				result = await httpClient.GetByteArrayAsync(fallbackUrl);
			}
			return result;
		}
#elif NETFRAMEWORK
		DateTime GetLastModifiedTimeCore(Uri url, Func<WebException, DateTime> handleException = null)
		{
			serviceLogger.Log(LogType.Information, "Try URL: " + url);
			var lastModified = DateTime.MinValue;
			HttpWebRequest webRequest = null;
			try
			{
				webRequest = (HttpWebRequest)WebRequest.Create(url);
				webRequest.Proxy = WebRequest.DefaultWebProxy;

				using (var webResponse = GetWebResponse(webRequest))
				{
					if (webResponse != null)
					{
						lastModified = webResponse.LastModified;
					}
				}
			}
			catch (WebException webEx)
			{
				serviceLogger.Log(LogType.Warning, "Web Exception: " + webEx.ToString());
				serviceLogger.Log(LogType.Warning, "Status: " + webEx.Status.ToString());
				serviceLogger.Log(LogType.Warning, "URL: " + url);

				if (webRequest != null)
				{
					var uri = webRequest.RequestUri;
					var proxy = webRequest.Proxy.GetProxy(uri);
					serviceLogger.Log(LogType.Warning,
						"Proxy: " + (proxy.AbsoluteUri == uri.AbsoluteUri ? "NONE" : proxy.OriginalString));
					serviceLogger.Log(LogType.Warning,
						string.Format(CultureInfo.InvariantCulture, "Credentials: Domain ({0}) UserName ({1})",
							System.Environment.UserDomainName, System.Environment.UserName));
				}
				if (handleException != null)
				{
					lastModified = handleException.Invoke(webEx);
				}
			}
			return lastModified;
		}

		public byte[] DownloadReferenceDataZippedFile()
		{
			serviceLogger.Log(LogType.Information, "Start to download reference data zipped file");
			byte[] result;
			using (var client = new WebClient())
			{
				client.Proxy = WebRequest.DefaultWebProxy;

				if (EnvProxy.IsHostedWithCargowise)
				{
					result = DownloadReferenceDataZippedFileCore(client, AlternateUrl, new Uri(Url));
				}
				else
				{
					result = DownloadReferenceDataZippedFileCore(client, new Uri(Url), AlternateUrl);
				}
			}
			return result;
		}

		byte[] DownloadReferenceDataZippedFileCore(WebClient client, Uri url, Uri fallbackUrl)
		{
			byte[] result;
			try
			{
				serviceLogger.Log(LogType.Information, "Try to download from " + url);
				result = client.DownloadData(url);
			}
			catch (WebException)
			{
				serviceLogger.Log(LogType.Warning, "Fail to download from " + url);
				serviceLogger.Log(LogType.Information, "Try to download from " + fallbackUrl);
				result = client.DownloadData(fallbackUrl);
			}
			return result;
		}
#endif

		public bool IsTestMode
		{
			get
			{
				return cmrTestMode;
			}
		}

		public string Url
		{
			get
			{
				return cmrTestMode ? UrlTest : UrlProduction;
			}
		}

		string ReferenceFileUrl
		{
			get
			{
				if (string.IsNullOrWhiteSpace(referenceFileUrl))
				{
					var urlProvider = new ReferenceFileUrlProvider(factory);
					referenceFileUrl = urlProvider.GetCustomsReferenceFileURL();
				}

				return referenceFileUrl;
			}
		}
		string referenceFileUrl;

		public Uri AlternateUrl
		{
			get
			{
				return new Uri(cmrTestMode ? AlternateUrlTest : AlternateUrlProduction);
			}
		}

		public readonly string AlternateUrlTest = AUCustomsDataRegistry.Instance.AUReferenceFileAlternateDownloadURL.Value + "Q1-MAIN.tar.gz";

		public readonly string AlternateUrlProduction = AUCustomsDataRegistry.Instance.AUReferenceFileAlternateDownloadURL.Value + "P1-MAIN.tar.gz";

		public string UrlTest => ReferenceFileUrl + "/industry_test/main/Q1-MAIN.tar.gz";

		public string UrlProduction => ReferenceFileUrl + "/production/main/P1-MAIN.tar.gz";
	}
}
