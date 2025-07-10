#if NETFRAMEWORK
using System.Net.Http;
using System.Text;
using CargoWise.Data.SqlProxy.Interface.Models;
using Newtonsoft.Json;
#endif

#if NETCOREAPP
using System.IO.Pipes;
using System.Net.Http.Json;
using CargoWise.Data.SqlProxy.Interface.Models;
#endif

namespace CargoWise.Data.SqlProxy.Interface
{
	static class HttpClientExtensions
	{
		public static async Task<HttpResponseMessage> PostAsync(this HttpClient? httpClient, string url,
			SqlProxyRequest? request, CancellationToken cancellationToken)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

#if NETFRAMEWORK
			var content = new StringContent(
				JsonConvert.SerializeObject(request),
				Encoding.UTF8,
				"application/json");

			return await httpClient!.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
#else
			return await httpClient!.PostAsJsonAsync(url, request, cancellationToken).ConfigureAwait(false);
#endif
		}

		public static async Task<string> ReadStringAsync(this HttpContent content, CancellationToken cancellationToken)
		{
#if NETFRAMEWORK
			return await content.ReadAsStringAsync();
#else
			return await content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
		}

		public static async Task<Stream> ReadStreamAsync(this HttpContent content, CancellationToken cancellationToken)
		{
#if NETFRAMEWORK
			return await content.ReadAsStreamAsync().ConfigureAwait(false);
#else
			return await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
		}

		public static HttpMessageHandler CreateHttpMessageHandler(string namedPipeName)
		{
#if NETFRAMEWORK
			return new NamedPipeMessageHandler(namedPipeName);
#else
		return new SocketsHttpHandler
		{
			ConnectCallback = async (ctx, ct) =>
			{
				var pipeClientStream = new NamedPipeClientStream(
					serverName: ".",
					pipeName: namedPipeName,
					PipeDirection.InOut,
					PipeOptions.Asynchronous);

				await pipeClientStream.ConnectAsync(ct).ConfigureAwait(false);

				return pipeClientStream;
			}
		};
#endif
		}

		public static bool IsValidPipeName(this string pipeName)
		{
			if (string.IsNullOrWhiteSpace(pipeName) || pipeName.Length > 256)
			{
				return false;
			}

			return pipeName.All(c => c != '\\');
		}
	}
}
