using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class HttpUtilsTest : TestCase
	{
		public void TestGet()
		{
			var client = new Mock<IGlowServiceClient>();
			client.Setup(c => c.GetAsync(It.IsAny<string>())).Returns((Func<string, Task<HttpResponseMessage>>)(
				relativeAddress => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"GET {relativeAddress}") })
			));

			AssertEquals("GET /test-page", ReadResponse(HttpUtils.Get(client.Object, "/test-page")));
		}

		public void TestPost()
		{
			var client = new Mock<IGlowServiceClient>();
			client.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns((Func<string, HttpContent, Task<HttpResponseMessage>>)(
				(relativeAddress, content) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"POST {relativeAddress} {content.ReadAsStringAsync().GetAwaiter().GetResult()}") })
			));

			AssertEquals("POST /test-page ABC", ReadResponse(HttpUtils.Post(client.Object, "/test-page", new StringContent("ABC"))));
		}

		public void TestReadAsJson()
		{
			Assert(
				AssertExceptionThrown<JsonSerializationException>(() => HttpUtils.ReadAsJson<TestResponseFormat>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") })).Message.StartsWith("Cannot deserialize the current JSON array")
			);

			Assert(
				AssertExceptionThrown<JsonReaderException>(() => HttpUtils.ReadAsJson<TestResponseFormat>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{!:!}") })).Message.StartsWith("Invalid property identifier character")
			);
		}

		public void TestReadAsString()
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Something in the response") };
			AssertEquals("Something in the response", HttpUtils.ReadAsString(response));
		}

		static string ReadResponse(HttpResponseMessage response)
		{
			return response?.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
		}

		public class TestResponseFormat
		{
			public string A { get; set; }
		}
	}
}
