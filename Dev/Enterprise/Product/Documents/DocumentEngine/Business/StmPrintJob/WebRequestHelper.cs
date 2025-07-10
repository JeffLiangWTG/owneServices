using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine
{
	public static class WebRequestHelper
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "WebRequest part")]
		const string RestApiPath = "api";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "WebRequest part")]
		const string Action = "Nudge";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Url parameter name")]
		const string HostParamPrefix = "host:";
		const string ServerName = "serverName";
		const string PrintQueueName = "printQueueName";
		const string PrintJobPK = "printJobPK";
		const string IsForwarded = "isForwarded";

		public static Task<WebRequestResult> NudgePrintServerAsync(StmPrintQueue printQueue, ZGuid printJobPK, bool isForwarded = false)
		{
			var serviceAddress = GetAssociatedWebPrintServiceAddress(printQueue).Trim();

			var printJobPkParam = printJobPK.IsValid ? printJobPK.ToGuid() : Guid.Empty;

			return NudgePrintServerAsync(serviceAddress, printQueue.SQ_ServerName, printQueue.SQ_QueueName, printJobPkParam, isForwarded);
		}

		public static Task<WebRequestResult> NudgePrintServerAsync(string serviceAddress, string serverName, string printQueueName, Guid printJobPK, bool isForwarded)
		{
			var arguments = EncodeNudgeArguments(serverName, printQueueName, printJobPK, isForwarded);

			var serviceAddressParts = serviceAddress.Split('|');
			var url = serviceAddressParts.First().TrimEnd('/');
			if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
			{
				return Task.FromResult(new WebRequestResult());
			}
			var hostName = GetHostName(serviceAddressParts);

			return SendNudgeRequestAsync(url, hostName, arguments);
		}

		static string GetHostName(string[] serviceAddressParts)
		{
			for (int i = 1; i < serviceAddressParts.Length; i++)
			{
				var addressPart = serviceAddressParts[i];
				if (addressPart.StartsWith(HostParamPrefix, StringComparison.OrdinalIgnoreCase))
				{
					return addressPart.Substring(HostParamPrefix.Length);
				}
			}

			return null;
		}

		static string GetAssociatedWebPrintServiceAddress(StmPrintQueue printQueue)
		{
			if (printQueue == null || printQueue.IsDeleted)
			{
				return string.Empty;
			}

			printQueue.ReloadSafe(); // Ensure latest value of SQ_WebPrintServiceAddress

			if (printQueue.IsDeleted)
			{
				return string.Empty;
			}

			return printQueue.SQ_WebPrintServiceAddress.ToString();
		}

		static Task<WebRequestResult> SendNudgeRequestAsync(string url, string hostName, string arguments)
		{
			return Task.Run(() => SendNudgeRequest(url, hostName, arguments));
		}

		static WebRequestResult SendNudgeRequest(string serviceUrl, string hostName, string arguments)
		{
			var requestResult = new WebRequestResult();
			var responseHelper = new WebRequestRedirectResponseHelper(serviceUrl, hostName, RestApiPath);

			if (ShouldSendNudgeViaIPAddress())
			{
				responseHelper.SendNudgeViaIPAddress = true;
				responseHelper.HandleRedirectResponse(url =>
				{
					// Try to send Nudge http request via IP address
					requestResult = SendWebRequest(url, hostName, Action, responseHelper, arguments, DisableSendNudgeViaIPAddress);
				});
				ReportUnhandledErrorIfNeeded(requestResult, responseHelper);
			}
			else if (!SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge() && !string.IsNullOrEmpty(hostName))
			{
				responseHelper.HandleRedirectResponse(url =>
				{
					// Try to send Nudge http request via web service URL address
					requestResult = SendWebRequest(url, string.Empty, Action, responseHelper, arguments, () => SendingNudgeErrorCounter.Instance.AddErrorCount());
				});
				ReportUnhandledErrorIfNeeded(requestResult, responseHelper);
			}

			return requestResult;
		}

		static void ReportUnhandledErrorIfNeeded(WebRequestResult requestResult, WebRequestRedirectResponseHelper helper)
		{
			if (!requestResult.Handled && !requestResult.Success)
			{
				if (helper.ShouldIgnoreRedirectError)
				{
					return;
				}

				var logs = helper.Logs.ToString();
				if (!string.IsNullOrEmpty(logs))
				{
					requestResult.ErrorMessage += logs;
				}
				ErrorReporter.ReportOnce(requestResult.ErrorMessage, requestResult.Exception);
			}
		}

		static bool ShouldSendNudgeViaIPAddress()
		{
			var printNudge = DocumentsDataRegistry.Instance.WebPrintNudge.Value;
			if (!printNudge.EnableIPAddress)
			{
				var shouldChange = true;
				if (printNudge.ChangingToUrlAddressDateTimeUtc > DateTime.MinValue)
				{
					var remainingTime = printNudge.ChangingToUrlAddressDateTimeUtc.AddHours(printNudge.SwtichBackToIPAddressIntervalInHours) - ZDateTime.UtcNow;
					if (remainingTime.TotalMilliseconds > 0)
					{
						shouldChange = false;
					}
				}

				if (shouldChange)
				{
					printNudge.EnableIPAddress = true;
					DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, printNudge);
				}
			}

			return printNudge.EnableIPAddress;
		}

		static void DisableSendNudgeViaIPAddress()
		{
#if DEBUG
			if (Globals.IsTest && SkipDisableIPAddressForTest.Value)
			{
				return;
			}
#endif
			var nudge = DocumentsDataRegistry.Instance.WebPrintNudge.Value;
			nudge.EnableIPAddress = false;
			nudge.ChangingToUrlAddressDateTimeUtc = ZDateTime.UtcNow.ToDateTime();
			if (nudge.SwtichBackToIPAddressIntervalInHours <= 0)
			{
				nudge.SwtichBackToIPAddressIntervalInHours = WebPrintNudgeDefaultValue.SwtichBackToIPAddressIntervalInHours;
			}
			DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nudge);
		}

		static string EncodeNudgeArguments(string serverName, string printQueueName, Guid printJobPK, bool isForwarded)
		{
			var arguments = new StringBuilder();

			arguments.Append(ServerName).Append("=").Append(WebUtility.UrlEncode(serverName));
			arguments.Append("&").Append(PrintQueueName).Append("=").Append(WebUtility.UrlEncode(printQueueName));
			arguments.Append("&").Append(PrintJobPK).Append("=").Append(WebUtility.UrlEncode(printJobPK.ToString()));
			arguments.Append("&").Append(IsForwarded).Append("=").Append(isForwarded.ToString());

			return arguments.ToString();
		}

		static WebRequestResult SendWebRequest(string serviceUrl, string hostName, string methodName, WebRequestRedirectResponseHelper redirectResponseHelper, string arguments = null, Action webExceptionHandledAction = null, bool handleErrors = true)
		{
			var requestResult = new WebRequestResult();
			var requestUrl = serviceUrl;
			var stopwatch = new Stopwatch();

			HttpWebRequest webRequest = null;
			try
			{
				stopwatch.Start();
				ServicePointManager.Expect100Continue = true;

				webRequest = GetWebRequest(serviceUrl, hostName, methodName, arguments);
				if (webRequest != null)
				{
					requestUrl = webRequest.RequestUri.ToString();
				}

#if DEBUG
				if (Globals.IsTest && SkipActualSendingRequestForTest.Value)
				{
					LastSentWebRequestForTest.Value = webRequest;
					LastSentWebRequestArguments.Value = arguments;

					if (ThrowExceptionForTest.Value != null)
					{
						throw ThrowExceptionForTest.Value;
					}

					requestResult.Success = true; // Skip actual sending of http request in tests
					return requestResult;
				}
#endif

				if (webRequest != null)
				{
					requestResult.Success = true;

					using (var response = webRequest.GetResponse())
					{
						if (response is HttpWebResponse httpResponse)
						{
							var statusCode = httpResponse.StatusCode;
							if (redirectResponseHelper.ShouldHandledWithRedirectResponse(httpResponse))
							{
								requestResult.Success = false;
								redirectResponseHelper.Logs.AppendLine($"Handling redirect response, and it will send nudge request again if maximum retry count is not reached. ServiceUrl: {serviceUrl}, HostName: {hostName}");
								return requestResult;
							}

							if (statusCode != HttpStatusCode.Accepted && statusCode != HttpStatusCode.NoContent)
							{
								requestResult.Success = false;
								requestResult.ErrorMessage = $"Response status code: {statusCode}, description: {httpResponse.StatusDescription}, ServiceUrl: {serviceUrl}, HostName: {hostName}";
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				stopwatch.Stop();
				requestResult = GetExceptionHandledResult(ex, requestUrl, hostName, methodName, arguments, webRequest, stopwatch, webExceptionHandledAction, handleErrors);
			}

			return requestResult;
		}

		static HttpWebRequest GetWebRequest(string serviceUrl, string hostName, string methodName, string arguments = null)
		{
#if DEBUG
			if (Globals.IsTest && SentWebRequestForTest.Value != null)
			{
				return SentWebRequestForTest.Value;
			}
#endif

			lock (lockObj)
			{
				var url = serviceUrl.TrimEnd('/') + "/" + RestApiPath + "/" + methodName;

				if (!string.IsNullOrEmpty(arguments))
				{
					url += "?" + arguments;
				}

#pragma warning disable SYSLIB0014 // 'WebRequest.CreateHttp(string)' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
				var webRequest = WebRequest.CreateHttp(url);
#pragma warning restore SYSLIB0014 // 'WebRequest.CreateHttp(string)' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
				webRequest.KeepAlive = false;
				webRequest.Method = "POST";
				webRequest.ContentType = "application/json";
				webRequest.ContentLength = 0;
				webRequest.AllowAutoRedirect = false; // Set it to false implicitly (default value is true)

				if (!string.IsNullOrWhiteSpace(hostName))
				{
					webRequest.Host = hostName;
				}

				webRequest.Credentials = new NetworkCredential(WebDataRegistry.Instance.WebServiceUsername.Value, WebDataRegistry.Instance.WebServicePassword.Value);
				webRequest.CookieContainer = new CookieContainer();

				return webRequest;
			}
		}

		static readonly object lockObj = new object();

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message for web request action and message, Web Exception Status, Response Code, Error report text, Error report, Error message")]
		static WebRequestResult GetExceptionHandledResult(Exception ex, string url, string hostName, string action, string arguments, HttpWebRequest webRequest, Stopwatch stopwatch, Action webExceptionHandledAction = null, bool handleErrors = true)
		{
			var requestResult = new WebRequestResult();
			var responseCode = 0;
			var isWebConnectionError = false;
			requestResult.Exception = ex;

			if (ex is WebException webEx)
			{
				if (webEx.Status == WebExceptionStatus.ProtocolError)
				{
					if ((url == null || url.IndexOf(WiseGridHost, StringComparison.OrdinalIgnoreCase) < 0) && (hostName == null || hostName.IndexOf(WiseGridHost, StringComparison.OrdinalIgnoreCase) < 0))
					{
						// Error with access to self-hosted web service - ignore for now
						requestResult.Handled = true;
					}

					if (webEx.Response is HttpWebResponse response)
					{
						responseCode = (int)response.StatusCode;
					}
					else
					{
						var statusMatch = WebExceptionRegex.Match(webEx.Message);
						if (statusMatch.Success)
						{
							var codeText = statusMatch.Groups["num"].Value;
							if (!int.TryParse(codeText, out responseCode))
							{
								responseCode = 0;
							}
						}
					}

					switch (responseCode)
					{
						case (int)HttpStatusCode.Unauthorized: // These errors will be monitored and reported in other way, to be done in WI00456120
						case (int)HttpStatusCode.NotFound:
						case (int)HttpStatusCode.RequestTimeout:
						case (int)HttpStatusCode.RequestEntityTooLarge:
						case (int)HttpStatusCode.InternalServerError:
						case (int)HttpStatusCode.BadGateway:
						case (int)HttpStatusCode.ServiceUnavailable:
						case (int)HttpStatusCode.GatewayTimeout:
							requestResult.Handled = true;
							isWebConnectionError = true;
							break;

						case 534: // DB upgrade pending
						case 535:
						case 536:
							requestResult.Handled = true;
							break;

						default:
							break;
					}
				}

				if (webEx.Status == WebExceptionStatus.ServerProtocolViolation)
				{
					// The server response was not a valid HTTP response.
					// Can happen if service is offline and returns a web page with text instead of proper http response.
					requestResult.Handled = true;
				}

				if (webEx.Status == WebExceptionStatus.NameResolutionFailure ||
						webEx.Status == WebExceptionStatus.Timeout ||
						webEx.Status == WebExceptionStatus.ConnectFailure ||
						webEx.Status == WebExceptionStatus.ReceiveFailure ||
						webEx.Status == WebExceptionStatus.RequestCanceled ||
						webEx.Status == WebExceptionStatus.ConnectionClosed)
				{
					requestResult.Handled = true;
					isWebConnectionError = true;
				}

				if (webEx.InnerException is SocketException innerException &&
					innerException.SocketErrorCode == SocketError.TimedOut)
				{
					//we do no report instead add to Elastic
					requestResult.Handled = true;
				}

				if (IsAuthenticationExceptionWithInvalidCertificate(webEx))
				{
					requestResult.Handled = true;
					isWebConnectionError = true;
				}
			}

			if (!handleErrors && requestResult.Handled)
			{
				requestResult.Handled = false;
			}

			if (!requestResult.Handled)
			{
				var errorMessage = new StringBuilder();
				errorMessage
					.Append((NoResString)"Request Url: ").AppendLine(url)
					.Append((NoResString)"Host Name: ").AppendLine(hostName ?? string.Empty)
					.Append("Client IP: ").AppendLine(GetHostIpAddress()?.ToString() ?? string.Empty)
					.Append("Action: ").AppendLine(action ?? string.Empty)
					.Append("Arguments: ").AppendLine(arguments ?? string.Empty)
					.Append("Elapsed time: ").AppendLine(stopwatch.Elapsed.ToString());

				if (ex is WebException webEx2)
				{
					var status = webEx2.Status.ToString();

					errorMessage.Append("Web Exception Status: ").AppendLine(status);
					if (webEx2.Response is HttpWebResponse httpResponse)
					{
						errorMessage.Append("Response Code: ").AppendLine(httpResponse.StatusCode.ToString());
					}

					if (webRequest != null)
					{
						errorMessage.Append("WebRequest.Timeout: ").AppendLine(webRequest.Timeout.ToString());
						errorMessage.Append("WebRequest.KeepAlive: ").AppendLine(webRequest.KeepAlive.ToString());
						if (webRequest.ServicePoint != null)
						{
							errorMessage.Append("ServicePoint.MaxIdleTime: ").AppendLine(webRequest.ServicePoint.MaxIdleTime.ToString());
							errorMessage.Append("ServicePoint.ConnectionLeaseTimeout: ").AppendLine(webRequest.ServicePoint.ConnectionLeaseTimeout.ToString());
							errorMessage.Append("ServicePoint.IdleSince: ").AppendLine(webRequest.ServicePoint.IdleSince.ToString());
						}
					}

					errorMessage.Append("ServicePointManager.SecurityProtocol: ").AppendLine(ServicePointManager.SecurityProtocol.ToString());
					errorMessage.Append("ServicePointManager.Expect100Continue: ").AppendLine(ServicePointManager.Expect100Continue.ToString());
				}

				requestResult.ErrorMessage = string.Format("details:\r\n{0}\r\nException:\r\n{1}", errorMessage.ToString(), ex);
			}
			else
			{
				if (isWebConnectionError)
				{
					webExceptionHandledAction?.Invoke();
				}
				requestResult.ErrorMessage = string.Format("was handled and status code is {0}", responseCode);
			}

			return requestResult;
		}

		public static bool IsAuthenticationExceptionWithInvalidCertificate(Exception ex)
		{
			if (ex is WebException webEx)
			{
				if (webEx.InnerException is AuthenticationException authenticationException && authenticationException.Message.Contains((NoResString)"The remote certificate is invalid according to the validation procedure", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		internal static IPAddress GetHostIpAddress()
		{
			var hostName = Dns.GetHostName();

			IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);
			var addressList = ipHostInfo.AddressList;

			return addressList?.FirstOrDefault(adr => adr.AddressFamily == AddressFamily.InterNetwork) // Ip4
				?? addressList?.FirstOrDefault(adr => adr.AddressFamily == AddressFamily.InterNetworkV6) // Ip6
				?? addressList?.FirstOrDefault();
		}

		const string WiseGridHost = "wisegrid.net";

		static readonly Regex WebExceptionRegex = new Regex(@"^(\w+\s+)+status\s+(?<num>401|404|408|413|500|502|503|504|534|535|536)\b[^a-z]*(?<msg>.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public class WebRequestResult
		{
			public bool Success { get; set; }

			public bool Handled { get; set; }

			public string ErrorMessage { get; set; }

			public Exception Exception { get; set; }
		}

		#region Test stuff
#if DEBUG
		public static Overridable<HttpWebRequest> LastSentWebRequestForTest { get; } = new Overridable<HttpWebRequest>(null);

		internal static Overridable<string> LastSentWebRequestArguments { get; } = new Overridable<string>(null);

		public static Overridable<Exception> ThrowExceptionForTest { get; } = new Overridable<Exception>(null);

		internal static Overridable<bool> SkipDisableIPAddressForTest { get; } = new Overridable<bool>(false);

		internal static Overridable<HttpWebRequest> SentWebRequestForTest { get; } = new Overridable<HttpWebRequest>(null);

		internal static Overridable<bool> SkipActualSendingRequestForTest { get; } = new Overridable<bool>(true);
#endif
		#endregion
	}
}
