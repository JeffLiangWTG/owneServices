using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Enterprise.ProductRegistration.Common;
using Enterprise.Registry.Business;

namespace Enterprise.ProductRegistration.Client
{
	public interface IRegistrationServiceClient
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		RegisterResponse Register(RegisterRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		VerifyResponse Verify(VerifyRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		UnregisterResponse Unregister(UnregisterRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000);
	}

	public class RegistrationServiceClient : IRegistrationServiceClient
	{
		public RegisterResponse Register(RegisterRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000)
		{
			RegisterResponse response = null;

			using (var client = new HttpClient())
			{
				var baseUri = new Uri(SystemDataRegistry.Instance.ProductRegistrationServiceUri.Value);
				var uri = new Uri(baseUri, new Uri("api/Registration/New", UriKind.Relative));

				var task = client.PostAsXmlAsync(uri, request, cancelToken);

				if (!task.Wait(timeoutMs, cancelToken))
				{
					status = HttpStatusCode.RequestTimeout;
				}
				else if (task.Result.IsSuccessStatusCode)
				{
					response = task.Result.Content.ReadAsAsync<RegisterResponse>().Result;
					status = HttpStatusCode.OK;
				}
				else
				{
					status = task.Result.StatusCode;
				}

				return response;
			}
		}

		public VerifyResponse Verify(VerifyRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000)
		{
			VerifyResponse response = null;

			using (var client = new HttpClient())
			{
				var baseUri = new Uri(SystemDataRegistry.Instance.ProductRegistrationServiceUri.Value);
				var uri = new Uri(baseUri, new Uri("api/Registration/Verify", UriKind.Relative));

				var task = client.PostAsXmlAsync(uri, request, cancelToken);

				if (!task.Wait(timeoutMs, cancelToken))
				{
					status = HttpStatusCode.RequestTimeout;
				}
				else if (task.Result.IsSuccessStatusCode)
				{
					response = task.Result.Content.ReadAsAsync<VerifyResponse>().Result;
					status = HttpStatusCode.OK;
				}
				else
				{
					status = task.Result.StatusCode;
				}

				return response;
			}
		}

		public UnregisterResponse Unregister(UnregisterRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000)
		{
			UnregisterResponse response = null;

			using (var client = new HttpClient())
			{
				var baseUri = new Uri(SystemDataRegistry.Instance.ProductRegistrationServiceUri.Value);
				var uri = new Uri(baseUri, new Uri("api/Registration/Unregister", UriKind.Relative));

				var task = client.PostAsXmlAsync(uri, request, cancelToken);

				if (!task.Wait(timeoutMs, cancelToken))
				{
					status = HttpStatusCode.RequestTimeout;
				}
				else if (task.Result.IsSuccessStatusCode)
				{
					response = task.Result.Content.ReadAsAsync<UnregisterResponse>().Result;
					status = HttpStatusCode.OK;
				}
				else
				{
					status = task.Result.StatusCode;
				}

				return response;
			}
		}
	}

#if DEBUG
	public class RegistrationServiceClientForTests : IRegistrationServiceClient
	{
		public RegisterResponse Register(RegisterRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000)
		{
			status = HttpStatusCode.OK;
			return new RegisterResponse() { Status = (int)RegisterStatus.Success };
		}

		public VerifyResponse Verify(VerifyRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000)
		{
			status = HttpStatusCode.OK;
			return new VerifyResponse() { Status = (int)RegisterStatus.Success };
		}

		public UnregisterResponse Unregister(UnregisterRequest request, out HttpStatusCode status, CancellationToken cancelToken, int timeoutMs = 20000)
		{
			status = HttpStatusCode.OK;
			return new UnregisterResponse() { Status = (int)RegisterStatus.Success };
		}
	}
#endif
}
