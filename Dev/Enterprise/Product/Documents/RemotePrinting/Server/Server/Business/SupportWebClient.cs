using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using CargoWise.Common;
using Enterprise.RemotePrinting.Server.Model;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.RemotePrinting.Server.Support;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.RemotePrinting.Server.Business
{
	public class SupportWebClient
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string HostParamName = "host";

		public SignalRClientInfo[] GetSignalRClients(string webPrintServerAddress)
		{
			var hostName = GetHostName(webPrintServerAddress, out string serviceUrl);
			if (string.IsNullOrEmpty(serviceUrl))
			{
				return Array.Empty<SignalRClientInfo>();
			}

			var webRequest = GetWebRequest(serviceUrl, hostName, nameof(SupportService.GetSignalRClients));

			ArrayOfSignalRClientInfo signalRClients;
			try
			{
				var responseVal = GetResponseContent(webRequest);

				ZXmlSerializer serializer = ZXmlSerializer.New(typeof(ArrayOfSignalRClientInfo));
				StringReader reader = new StringReader(responseVal);

				signalRClients = (ArrayOfSignalRClientInfo)serializer.Deserialize(reader);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				signalRClients = new ArrayOfSignalRClientInfo();
				signalRClients.SignalRClientInfo = new List<SignalRClientInfo>();
				signalRClients.SignalRClientInfo.Add(
					new SignalRClientInfo
					{
						ServerName = ex.GetType().Name + ": " + ex.Message,
						PrintersList = new List<string>(),
					});
			}

			signalRClients.SignalRClientInfo.ForEach(a =>
			{
				a.WebServerAddress = serviceUrl;
				a.WebServerHostName = hostName;
			});

			return signalRClients.SignalRClientInfo.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string RequestClientLogs(string serviceUrl, string hostName, string clientId, ClientLogRequestDetails requestDetails)
		{
			var arguments = new StringBuilder();
			arguments.Append("clientId=").Append(WebUtility.UrlEncode(clientId));
			arguments.Append("&emailAddress=").Append(WebUtility.UrlEncode(requestDetails.EmailAddress));
			arguments.Append("&fromDateString=").Append(WebUtility.UrlEncode(requestDetails.FromDate.ToString(SupportService.DateFormat, CultureInfo.InvariantCulture)));
			arguments.Append("&toDateString=").Append(WebUtility.UrlEncode(requestDetails.ToDate.ToString(SupportService.DateFormat, CultureInfo.InvariantCulture)));
			arguments.Append("&logTypes=").Append(WebUtility.UrlEncode(((int)requestDetails.LogTypes).ToString()));

			var webRequest = GetWebRequest(serviceUrl, hostName, nameof(SupportService.RequestClientLogs), arguments.ToString());
			try
			{
				var webResponse = GetResponse(webRequest);
				if (webResponse is HttpWebResponse httpResponse)
				{
					if (httpResponse.StatusCode == HttpStatusCode.OK)
					{
						return string.Empty;
					}
					else
					{
						return "Web response status: " + httpResponse.StatusCode.ToString();
					}
				}

				return "Web response status unknown.";
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ex.Message;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual HttpWebRequest GetWebRequest(string serviceUrl, string hostName, string methodName, string arguments = null)
		{
			var url = serviceUrl.TrimEnd('/') + SupportServicePath + methodName;

			var webRequest = WebRequest.CreateHttp(url);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/x-www-form-urlencoded";

			if (!string.IsNullOrWhiteSpace(hostName))
			{
				webRequest.Host = hostName;
			}

			var userName = GetUserName();
			webRequest.Credentials = new NetworkCredential(userName, string.Empty);
			webRequest.CookieContainer = new CookieContainer();

#if DEBUG
			if (WTG.TestHelpers.TestingState.IsTest)
			{
				webRequest.ContentLength = arguments?.Length ?? 0;
				// Do not write arguments into request stream as it tries to open connection to web service
				return webRequest;
			}
#endif

			if (!string.IsNullOrEmpty(arguments))
			{
				webRequest.ContentLength = arguments.Length;
				using (var writer = new StreamWriter(webRequest.GetRequestStream()))
				{
					writer.Write(arguments);
				}
			}
			else
			{
				webRequest.ContentLength = 0;
			}

			return webRequest;
		}

		protected virtual string GetResponseContent(HttpWebRequest webRequest)
		{
			var webResponse = GetResponse(webRequest);
			var responseValue = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
			return responseValue;
		}

		protected virtual WebResponse GetResponse(HttpWebRequest webRequest)
		{
			return webRequest.GetResponse();
		}

		const string SupportServicePath = "/support/SupportService.asmx/";

		protected virtual string GetUserName()
		{
			return HttpContext.Current?.User?.Identity?.Name;
		}

		string GetHostName(string serviceAddress, out string serviceUrl)
		{
			var serviceAddressParts = serviceAddress.Split('|');
			serviceUrl = serviceAddressParts.First().TrimEnd('/');

			string hostName = null;
			for (int i = 1; i < serviceAddressParts.Length; i++)
			{
				var addressPart = serviceAddressParts[i];
				var splitterIndex = addressPart.IndexOf(':'); // Don't use .Split(':') in case ':' is also used in value part
				if (splitterIndex >= 0)
				{
					var addressPartName = addressPart.Substring(0, splitterIndex);
					if (addressPartName.Equals(HostParamName, StringComparison.OrdinalIgnoreCase))
					{
						hostName = addressPart.Substring(splitterIndex + 1);
						break;
					}
				}
			}

			if (Uri.IsWellFormedUriString(serviceUrl, UriKind.Absolute))
			{
				using (var connection = DbHelper.NewConnection())
				{
					var originalUri = new Uri(serviceUrl);
					var forceToUseHttps = RegistryData.WebPrintForceToUseHTTPSForWebPrintRequests(connection);
					if (originalUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) && forceToUseHttps && !string.IsNullOrEmpty(hostName))
					{
						var builder = new UriBuilder(Uri.UriSchemeHttps, hostName, originalUri.Port, originalUri.AbsolutePath);
						serviceUrl = builder.ToString();
					}
				}
			}

			return hostName;
		}
	}
}
