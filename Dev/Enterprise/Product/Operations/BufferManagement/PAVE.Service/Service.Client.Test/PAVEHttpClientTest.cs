using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Client.Test
{
	public class PAVEHttpClientTest : TestCase
	{
		#region Post

		public void TestPost()
		{
			glowClientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Callback((Uri uri) =>
			{
				AssertEquals(glowUrl, uri.OriginalString);
			}).Returns(glowClientMock.Object);

			var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(JsonConvert.SerializeObject(response))
			};

			glowClientMock.Setup(f => f.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Callback((string relativeAddress, HttpContent content) =>
			{
				AssertEquals("cw1api/PAVE/" + actionName, relativeAddress);
				var stringContent = content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				AssertEquals(JsonConvert.SerializeObject(request), stringContent);
			}).Returns(Task.FromResult(httpResponse)).Verifiable();

			var result = client.Post<DummyResponseModel>(actionName, request);

			AssertEquals(response.Name, result.Name);
			AssertEquals(response.SomeTime, result.SomeTime.ToLocalTime());
			glowClientFactoryMock.Verify();
		}

		public void TestPost_ShouldUnwrapAggregateException()
		{
			glowClientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Returns(glowClientMock.Object);
			glowClientMock.Setup(f => f.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
				.ThrowsAsync(new Exception("On no!"));

			AssertExceptionThrown<Exception>("Should throw On no!", "On no!", () => client.Post<DummyResponseModel>(actionName, request));
		}

		#endregion

		#region PostAsync

		public void TestPostAsync()
		{
			glowClientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Callback((Uri uri) =>
			{
				AssertEquals(glowUrl, uri.OriginalString);
			}).Returns(glowClientMock.Object);

			var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(JsonConvert.SerializeObject(response))
			};

			glowClientMock.Setup(f => f.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Callback((string relativeAddress, HttpContent content) =>
			{
				AssertEquals("cw1api/PAVE/" + actionName, relativeAddress);
				var stringContent = content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				AssertEquals(JsonConvert.SerializeObject(request), stringContent);
			}).Returns(Task.FromResult(httpResponse)).Verifiable();

			var result = client.PostAsync<DummyResponseModel>(actionName, request).ConfigureAwait(false).GetAwaiter().GetResult();

			AssertEquals(response.Name, result.Name);
			AssertEquals(response.SomeTime, result.SomeTime.ToLocalTime());
			glowClientFactoryMock.Verify();
		}

		public void TestPostAsync_ShouldThrowHttpRequestException_WhenResponseNotSuccessul()
		{
			glowClientMock.Setup(f => f.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns(() =>
			{
				var httpResponse = new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.BadRequest,
					Content = new StringContent("SO BAD"),
					RequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{glowUrl}/PAVE/{actionName}")
				};

				return Task.FromResult(httpResponse);
			}).Verifiable();

			var paveClient = new PAVEHttpClient(new Uri(glowUrl));
			var expectedExceptionMessage = "Error sending a POST request to https://iamglowing/PAVE/someAction, Code: 400, Phrase: Bad Request, Response Content: SO BAD";

			AssertExceptionThrown<HttpRequestException>("Should throw HttpRequestException", expectedExceptionMessage, () =>
			{
				paveClient.PostAsync<DummyResponseModel>(actionName, request).ConfigureAwait(false).GetAwaiter().GetResult();
			});

			glowClientFactoryMock.Verify();
		}

		public void TestPostAsync_ShouldReturnNull_WhenNoContent()
		{
			glowClientMock.Setup(f => f.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Returns(() =>
			{
				var httpResponse = new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(string.Empty),
				};

				return Task.FromResult(httpResponse);
			}).Verifiable();

			var result = client.PostAsync<object>(actionName, request).ConfigureAwait(false).GetAwaiter().GetResult();

			AssertNull(result);
		}

		#endregion

		#region SetUp

		const string glowUrl = "https://IAmGlowing";
		const string actionName = "someAction";
		PAVEHttpClient client = new PAVEHttpClient(new Uri(glowUrl));

		Mock<IGlowServiceClient> glowClientMock;
		Mock<IGlowServiceClientFactory> glowClientFactoryMock;

		(string Name, DateTime SomeTime) request = (Name: "Request", SomeTime: DateTime.Now);
		readonly DummyResponseModel response = new DummyResponseModel()
		{
			Name = "Response",
			SomeTime = DateTime.Now
		};

		public class DummyResponseModel
		{
			public string Name { get; set; }
			public DateTime SomeTime { get; set; }
		}

		protected override void SetUp()
		{
			glowClientMock = new Mock<IGlowServiceClient>();
			glowClientFactoryMock = new Mock<IGlowServiceClientFactory>();
			glowClientFactoryMock.Setup(f => f.Create(It.IsAny<Uri>())).Returns(glowClientMock.Object);

			ObjectFactory.Substitute(glowClientFactoryMock.Object);

			client = new PAVEHttpClient(new Uri(glowUrl));

			base.SetUp();
		}

		#endregion
	}
}
