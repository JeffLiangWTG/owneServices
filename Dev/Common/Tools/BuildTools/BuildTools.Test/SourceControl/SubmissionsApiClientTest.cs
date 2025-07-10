using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class SubmissionsApiClientTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestCreateNewSubmission()
		{
			// Arrange
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/new"
						&& m.Method.Method == "POST"
						&& m.Content.ReadAsStringAsync().GetAwaiter().GetResult() == $@"{{""UserName"":""davey"",""Criticality"":"";)"",""ActionType"":""SHV"",""ProcessTaskPK"":""{taskPk}"",""Comments"":""https://github.com/WiseTechGlobal/DevTools/pull/666""}}"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.OK }))
				.Verifiable();

			// Act
			apiClient.CreateNewSubmission(SubmissionType.TestRun, "davey", "https://github.com/WiseTechGlobal/DevTools/pull/666", taskPk, criticality: ";)");

			// Assert
			handlerMock.VerifyAll();
		}

		public void TestCreateNewSubmission_WhenServiceReturnsUnsuccessfulResponse()
		{
			// Arrange
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/new"
						&& m.Method.Method == "POST"
						&& m.Content.ReadAsStringAsync().GetAwaiter().GetResult() == $@"{{""UserName"":""davey"",""Criticality"":"";)"",""ActionType"":""SCH"",""ProcessTaskPK"":""{taskPk}"",""Comments"":""https://github.com/WiseTechGlobal/DevTools/pull/666""}}"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Content = new StringContent("Error from server"),
				}))
				.Verifiable();

			// Act, Assert
			var ex = AssertExceptionThrown<HttpRequestException>(() => apiClient.CreateNewSubmission(SubmissionType.Checkin, "davey", "https://github.com/WiseTechGlobal/DevTools/pull/666", taskPk, criticality: ";)"));
			AssertEquals("Response status code does not indicate success: 500 (Internal Server Error).", ex.Message);
			handlerMock.VerifyAll();
		}

		public void TestGetLatestBuild()
		{
			// Arrange
			var guid = Guid.NewGuid();

			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/latest-build?targetRepository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FDevTools&pullRequestId=123"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(guid.ToString()),
				}))
				.Verifiable();

			// Act
			var pk = apiClient.GetLatestBuild("https://github.com/WiseTechGlobal/DevTools", 123);

			// Assert
			handlerMock.VerifyAll();
			AssertEquals(guid, pk);
		}

		public void TestGetLatestBuild_ServiceReturnsQuotesAroundString()
		{
			// Arrange
			var guid = Guid.NewGuid();

			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/latest-build?targetRepository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FDevTools&pullRequestId=123"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent($"\"{guid}\""),
				}))
				.Verifiable();

			// Act
			var pk = apiClient.GetLatestBuild("https://github.com/WiseTechGlobal/DevTools", 123);

			// Assert
			handlerMock.VerifyAll();
			AssertEquals(guid, pk);
		}

		public void TestGetLatestBuild_WhenServiceReturnsUnsuccessfulResponse()
		{
			// Arrange
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/latest-build?targetRepository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FDevTools&pullRequestId=123"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }))
				.Verifiable();

			// Act, Assert
			var ex = AssertExceptionThrown<HttpRequestException>(() => apiClient.GetLatestBuild("https://github.com/WiseTechGlobal/DevTools", 123));
			AssertEquals("Response status code does not indicate success: 500 (Internal Server Error).", ex.Message);
			handlerMock.VerifyAll();
		}

		public void TestGetLatestBuild_WhenServiceReturnsGarbageInSuccessfulResponse()
		{
			// Arrange
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/latest-build?targetRepository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FDevTools&pullRequestId=123"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("Garbage"),
				}))
				.Verifiable();

			// Act, Assert
			var ex = AssertExceptionThrown<InvalidOperationException>(() => apiClient.GetLatestBuild("https://github.com/WiseTechGlobal/DevTools", 123));
			AssertEquals("Invalid GUID response received from server: OK, Garbage", ex.Message);
			handlerMock.VerifyAll();
		}

		public void TestDownloadLatestTestMethodsFileAsync_WithDefaults()
		{
			// Arrange
			const string testMethodsContent = "Test methods be 'ere";

			using var sourceStream = new MemoryStream();
			using var writer = new StreamWriter(sourceStream);
			writer.Write(testMethodsContent);
			writer.Flush();
			sourceStream.Position = 0;

			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/latest-test-methods?targetRepository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FDevTools&branch=master&path=%2F&buildConfiguration=DEBUG"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StreamContent(sourceStream),
				}))
				.Verifiable();

			// Act
			var downloadedStream = apiClient.DownloadLatestTestMethodsFileAsync("https://github.com/WiseTechGlobal/DevTools").GetAwaiter().GetResult();
			using var streamReader = new StreamReader(downloadedStream);
			var downloadedContent = streamReader.ReadToEnd();

			// Assert
			AssertEquals(testMethodsContent, downloadedContent);
			handlerMock.VerifyAll();
		}

		public void TestDownloadLatestTestMethodsFileAsync_WithInputsNeedingEscape()
		{
			// Arrange
			const string testMethodsContent = "Test methods be 'ere";

			using var sourceStream = new MemoryStream();
			using var writer = new StreamWriter(sourceStream);
			writer.Write(testMethodsContent);
			writer.Flush();
			sourceStream.Position = 0;

			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/latest-test-methods?targetRepository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FDevTools&branch=releases%2Fproduction&path=%2Fsomething%2Fboop&buildConfiguration=RELEASE%2FME"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StreamContent(sourceStream),
				}))
				.Verifiable();

			// Act
			var downloadedStream = apiClient.DownloadLatestTestMethodsFileAsync("https://github.com/WiseTechGlobal/DevTools", branch: "releases/production", path: "/something/boop", buildConfiguration: "RELEASE/ME").GetAwaiter().GetResult();
			using var streamReader = new StreamReader(downloadedStream);
			var downloadedContent = streamReader.ReadToEnd();

			// Assert
			AssertEquals(testMethodsContent, downloadedContent);
			handlerMock.VerifyAll();
		}

		public void TestDownloadLatestTestMethodsFileAsync_WhenServiceReturnsUnsuccessfulResponse()
		{
			// Arrange
			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/latest-test-methods?targetRepository=https%3A%2F%2Fgithub.com%2FWiseTechGlobal%2FDevTools&branch=master&path=%2F&buildConfiguration=DEBUG"
						&& m.Method.Method == "GET"
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Content = new StringContent("Error from server"),
				}))
				.Verifiable();

			// Act, Assert
			var ex = AssertExceptionThrown<HttpRequestException>(() => apiClient.DownloadLatestTestMethodsFileAsync("https://github.com/WiseTechGlobal/DevTools").GetAwaiter().GetResult());
			AssertEquals("Response status code does not indicate success: 500 (Internal Server Error).", ex.Message);
			handlerMock.VerifyAll();
		}

		public void TestUploadTestFailureDataAndGetUrlAsync()
		{
			// Arrange
			const string url = "http://path/to/test.txt";

			using var sourceStream = new MemoryStream();
			using var writer = new StreamWriter(sourceStream);
			writer.Write("123");
			writer.Flush();
			sourceStream.Position = 0;

			handlerMock
				.Protected()
				.Setup<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.Is<HttpRequestMessage>(m =>
						m.RequestUri.AbsoluteUri == "http://nothing/api/submisions/test-failure-data?contentType=text/plain&fileExtension=.txt"
						&& m.Method.Method == "POST"
						&& m.Content is StreamContent
					),
					ItExpr.IsAny<CancellationToken>())
				.Returns(Task.FromResult(new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(url),
				}))
				.Verifiable();

			// Act
			var responseUrl = apiClient.UploadTestFailureDataAndGetUrlAsync(sourceStream).GetAwaiter().GetResult();

			// Assert
			AssertEquals(url, responseUrl);
			handlerMock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();
			taskPk = Guid.NewGuid();
			handlerMock = new Mock<HttpMessageHandler>();
			httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://nothing") };
			apiClient = new SubmissionsApiClient(httpClient);
		}

		protected override void TearDown()
		{
			base.TearDown();
			httpClient?.Dispose();
			httpClient = null;
		}

		Guid taskPk;
		Mock<HttpMessageHandler> handlerMock;
		HttpClient httpClient;
		SubmissionsApiClient apiClient;
	}
}
