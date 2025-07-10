using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Authentication.Primitives;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;

namespace Enterprise.AuditDataServices.Glow.Test
{
	abstract class ProcessTaskTemplateGlowSubscriberBaseTest : ActualDataChangesAuditSubscriberTest
	{
		StringRegistryItem ServiceUriRegistryItem => GlowRegistry.Instance.GlowServiceUriRegistryItem;

		public void TestIsRequired()
		{
			var subscriber = NewDataChangeSubscriber();
			AssertEquals(expected: true, subscriber.IsRequired());
		}

		public void TestIsRequired_GlowRegistryIsNotSetUp()
		{
			var subscriber = NewDataChangeSubscriber();
			ServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			AssertEquals(expected: false, subscriber.IsRequired());
		}

		public void TestProcessChanges_ErrorStatusCode()
		{
			var changedTable = NewDataTable();
			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway))).Verifiable();

			var subscriber = NewDataChangeSubscriber();
			AssertExceptionThrown<HttpRequestException>(() => subscriber.ProcessChanges(null, changedTable));
			clientMock.Verify();
		}

		public void TestProcessChanges_UnauthorizedExceptionWithLogger_LogonDetailsIncorrect()
		{
			var changedTable = NewDataTable();
			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Throws(new AuthorizationFailureException(AuthenticationResult.LogonDetailsIncorrect));
			var logger = new LoggerForTest();

			var subscriber = NewDataChangeSubscriber();
			subscriber.ProcessChanges(logger, changedTable);

			AssertLogEntries(
				"authentication error log",
				string.Join("\r\n", logger.LogEntries),
				$"Authentication Error sending request to {ServiceRelativePath}... Message: Unexpected authentication result: LogonDetailsIncorrect");
		}

		public void TestProcessChanges_UnauthorizedExceptionWithoutLogger_LogonDetailsIncorrect()
		{
			var changedTable = NewDataTable();
			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Throws(new AuthorizationFailureException(AuthenticationResult.LogonDetailsIncorrect));

			var expectedException = new AuthorizationFailureException(AuthenticationResult.LogonDetailsIncorrect);

			var subscriber = NewDataChangeSubscriber();
			AssertExceptionThrown("Authorization result in message", typeof(HttpRequestException), $"Authentication Error sending request to {ServiceRelativePath}... Message: Unexpected authentication result: LogonDetailsIncorrect", () => subscriber.ProcessChanges(null, changedTable), assertStartsWith: true);
			clientMock.Verify();
		}

		public void TestProcessChanges_UnauthorizedException_AccountLocked()
		{
			var changedTable = NewDataTable();
			clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Throws(new AuthorizationFailureException(AuthenticationResult.AccountLocked));

			var subscriber = NewDataChangeSubscriber();
			AssertExceptionThrown("Authorization Exception account locked", typeof(AuthorizationFailureException), "Unexpected authentication result: AccountLocked", () => subscriber.ProcessChanges(null, changedTable), assertStartsWith: true);
			clientMock.Verify();
		}

		public void TestLogging_ServerReturnsErrors_WhenProcessingChanges()
		{
			var logEntries = new StringBuilder();
			var mockLogger = new Mock<ILogger>();
			mockLogger
				.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback<LogType, string>((logType, message) => logEntries.AppendLine($"{logType}|{message}"));

			using (var response = new HttpResponseMessage(HttpStatusCode.InternalServerError))
			{
				response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo/bar");
#pragma warning disable WTG2007 // Do not set custom values for the HTTP Reason Phrase.
				response.ReasonPhrase = "Bar";
#pragma warning restore WTG2007 // Do not set custom values for the HTTP Reason Phrase.
				response.Content = new StringContent("Another detailed error message");
				clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Returns(Task.FromResult(response));
				NewDataChangeSubscriber().ProcessChanges(mockLogger.Object, NewDataTable());
			}

			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			{
				response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo/bar");
				clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Returns(Task.FromResult(response));
				NewDataChangeSubscriber().ProcessChanges(mockLogger.Object, NewDataTable());
			}

			using (var response = new HttpResponseMessage(HttpStatusCode.Conflict))
			{
				response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo/bar");
				clientMock.Setup(c => c.PostAsync(ServiceRelativePath, It.IsAny<HttpContent>())).Returns(Task.FromResult(response));
				NewDataChangeSubscriber().ProcessChanges(mockLogger.Object, NewDataTable());
			}
			var actualEntries = logEntries.ToString().Trim();

			AssertLogEntries(
				"Must add log entries up to errors count threshold",
				actualEntries,
				"Warning|Error sending request to http://foo/bar... Code: 500, Phrase: Bar, Content: Another detailed error message",
				@"Warning|Error sending request to http://foo/bar... Code: 409, Phrase: Conflict, Content: ");
		}

		static void AssertLogEntries(string assertMessage, string actualEntries, string logEntry, string logEnding = null)
		{
			var expectedLogEntries = new StringBuilder();
			expectedLogEntries.AppendLine(logEntry);

			if (!string.IsNullOrEmpty(logEnding))
			{
				expectedLogEntries.AppendLine(logEnding);
			}

			AssertMultilineEquals(
				assertMessage,
				expectedLogEntries.ToString().Trim(),
				actualEntries,
				'\n');
		}

		public void TestLogging_RequestThrowsAnException()
		{
			clientMock.Setup(c => c.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Throws(new HttpRequestException("Some Exception"));
			var logger = new LoggerForTest();
			NewDataChangeSubscriber().ProcessChanges(logger, NewDataTable());

			AssertLogEntries(
				"Should be wrapped",
				string.Join("\r\n", logger.LogEntries),
				$"Error sending request to {ServiceRelativePath}... Message: Some Exception");
		}

		protected abstract string ServiceRelativePath { get; }

		protected abstract DataTable NewDataTable();

		protected Mock<IGlowServiceClient> clientMock;

		protected override void SetUp()
		{
			ServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://serviceaddress");
			clientMock = new Mock<IGlowServiceClient>();

			var clientFactoryMock = new Mock<IGlowServiceClientFactory>();
			clientFactoryMock.Setup(f => f.Create(new Uri("https://serviceaddress"))).Returns(clientMock.Object);
			ObjectFactory.Substitute(clientFactoryMock.Object);

			base.SetUp();
		}
	}
}
