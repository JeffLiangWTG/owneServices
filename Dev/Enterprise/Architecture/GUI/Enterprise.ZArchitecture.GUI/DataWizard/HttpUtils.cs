using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;

namespace Enterprise.ZArchitecture.GUI
{
	public static class HttpUtils
	{
		/// <summary>
		/// Sends a GET request and awaits a response.
		/// </summary>
		/// <remarks>
		/// This method exists to avoid asynchronous calls (see WI00216983).
		/// </remarks>
		public static HttpResponseMessage Get(IGlowServiceClient client, string relativeAddress)
		{
			return Task.Factory.StartNew(() => client.GetAsync(relativeAddress).GetAwaiter().GetResult(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Sends a POST request and awaits a response.
		/// </summary>
		/// <remarks>
		/// This method exists to avoid asynchronous calls (see WI00216983).
		/// </remarks>
		public static HttpResponseMessage Post(IGlowServiceClient client, string relativeAddress, HttpContent content)
		{
			return Task.Factory.StartNew(() => client.PostAsync(relativeAddress, content).GetAwaiter().GetResult(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Sends a POST request and awaits a response.
		/// </summary>
		/// <remarks>
		/// This method exists to avoid asynchronous calls (see WI00216983).
		/// </remarks>
		public static HttpResponseMessage PostAsJson<T>(IGlowServiceClient client, string relativeAddress, T content)
		{
			return Task.Factory.StartNew(() => client.PostAsJsonAsync(relativeAddress, content).GetAwaiter().GetResult(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Sends a request and awaits a response.
		/// </summary>
		/// <remarks>
		/// This method exists to avoid asynchronous calls (see WI00216983).
		/// </remarks>
		public static HttpResponseMessage Send(IGlowServiceClient client, HttpRequestMessage request, HttpCompletionOption completionOption)
		{
			return Task.Factory.StartNew(() => client.SendAsync(request, completionOption).GetAwaiter().GetResult(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Reads the given response as a JSON object.
		/// </summary>
		public static T ReadAsJson<T>(HttpResponseMessage response)
		{
			var responseAsString = ReadAsString(response);
			return JsonConvert.DeserializeObject<T>(responseAsString);
		}

		/// <summary>
		/// Reads the given response as a string.
		/// </summary>
		public static string ReadAsString(HttpResponseMessage response)
		{
			return response?.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Reads the given response as a Stream object.
		/// </summary>
		public static Stream ReadAsStream(HttpResponseMessage response)
		{
			return response?.Content?.ReadAsStreamAsync().GetAwaiter().GetResult();
		}
	}
}
