using System;
using System.Linq;
using System.Net;
#if NET
using System.Net.Sockets;
#endif
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Newtonsoft.Json;
using Polly;
using WTG.TrustedMessaging.Models;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	class SystemToSystemTrustApiHelper : ISystemToSystemTrustApiHelper
	{
		public string SystemToSystemTrustApiEndpoint => $"{WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/')}/api/SystemTrust/";

		public TimeSpan SecondsDelayedBetweenRequests => TimeSpan.FromSeconds(30);

		internal static string FormatApiErrorMessages(int statusCode, string responseContent)
		{
			if (responseContent.IsNullOrEmpty())
			{
				return $"There was an error when calling the MyAccount Web Api. {statusCode} : {(HttpStatusCode)statusCode}";
			}

			ErrorMessages errorMessages;
			try
			{
				errorMessages = JsonConvert.DeserializeObject<ErrorMessages>(responseContent, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
				if (errorMessages != null && (errorMessages.Messages == null || errorMessages.Messages.All(x => x == null)))
				{
					errorMessages = new ErrorMessages(statusCode.ToString(), ((HttpStatusCode)statusCode).ToString());
				}
			}
			catch (JsonException)
			{
				errorMessages = null;
			}

			var errorResponse = string.Join(System.Environment.NewLine,
				(errorMessages ?? new ErrorMessages(statusCode.ToString(), responseContent)).Messages
				.WhereNotNull()
				.Select(x => $"{x.Code} : {x.Message}"));

			return $"There was an error when calling the MyAccount Web Api. {errorResponse}";
		}

		public SystemToSystemTrustApiResponse SystemToSystemTrustApiGet(string relativeUri, string token = null)
		{
			using (var client = new HttpClient())
			{
				if (token != null)
				{
					client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
				}
				var response = TaskRunWaitAndGetResult(() => client.GetAsync(SystemToSystemTrustApiEndpoint + relativeUri));
				var responseContent = TaskRunWaitAndGetResult(() => response.Content.ReadAsStringAsync());
				return new SystemToSystemTrustApiResponse(response.StatusCode,
					response.StatusCode == HttpStatusCode.OK ? responseContent : FormatApiErrorMessages((int)response.StatusCode, responseContent));
			}
		}

		public SystemToSystemTrustApiResponse SystemToSystemTrustApiPost(string relativeUri, StringContent stringContent, string token = null)
		{
			using (var client = new HttpClient())
			{
				if (token != null)
				{
					client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
				}
				var response = TaskRunWaitAndGetResult(() => client.PostAsync(SystemToSystemTrustApiEndpoint + relativeUri, stringContent));
				var responseContent = TaskRunWaitAndGetResult(() => response.Content.ReadAsStringAsync());
				return new SystemToSystemTrustApiResponse(response.StatusCode,
					response.StatusCode == HttpStatusCode.OK ? responseContent : FormatApiErrorMessages((int)response.StatusCode, responseContent));
			}
		}

		TResult TaskRunWaitAndGetResult<TResult>(Func<Task<TResult>> func)
		{
			try
			{
				var policy = Policy
#if NETFRAMEWORK
					.Handle<HttpRequestException>(exception => exception.InnerException is WebException)
					.WaitAndRetry(3, count => TimeSpan.FromSeconds(1));
#else
					.Handle<HttpRequestException>(exception => exception.InnerException is SocketException)
					.WaitAndRetry(3, count => TimeSpan.FromSeconds(1));
#endif
				return policy.Execute(() => func.Invoke().GetAwaiter().GetResult());
			}
#if NETFRAMEWORK
			catch (HttpRequestException ex) when (ex.InnerException is WebException webException)
			{
				throw new SystemToSystemTrustCertificateManagementException(ex.Message, webException);
			}
#else
			catch (HttpRequestException ex) when (ex.InnerException is SocketException socketException)
			{
				throw new SystemToSystemTrustCertificateManagementException(ex.Message, socketException);
			}
#endif
		}
	}
}
