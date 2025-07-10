using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Authentication.Primitives;
using CargoWise.Resource.Shared;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Glow.Subscribers;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Glow.Test
{
	[TestedType(typeof(GlowIndexUpdateSubscriber))]
	class GlowIndexUpdateSubscriberTest : ChangedTableListOnlyAuditSubscriberTest
	{
		public string ExpectedCode => "GIU";
		public string ExpectedDescription => "GLOW Index Updater";
		public StringRegistryItem ServiceUriRegistryItem => GlowRegistry.Instance.GlowServiceUriRegistryItem;

		protected override ChangedTableListOnlyAuditSubscriber NewChangedTableListSubscriber()
		{
			return new GlowIndexUpdateSubscriber();
		}

		protected override void TearDown()
		{
			GlowIndexUpdateSubscriber.ResetStaticMemebers();
			base.TearDown();
		}

		public void TestSubscribedTablesAreLoaded()
		{
			var fromEmbedded = CdcRequiredTableHelper
				.Load(Assembly.Load("Enterprise.DbUpgrader.Resource"))
				.Select(t => t.TableName)
				.Select(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema).Where(t => t != null);
			var fromSubscriber = NewChangedTableListSubscriber().SubscribedTables;

			AssertGreaterThan(fromEmbedded.Count(), 0);
			AssertContainsExactElementsInAnyOrder(fromEmbedded, fromSubscriber);
		}

		public void TestProperties()
		{
			var subscriber = NewChangedTableListSubscriber();
			AssertEquals(ExpectedCode, subscriber.Code);
			AssertEquals(ExpectedDescription, subscriber.Description);
		}

		public void TestIsRequired()
		{
			var subscriber = NewChangedTableListSubscriber();
			AssertEquals(true, subscriber.IsRequired());
		}

		public void TestIsRequired_GlowRegistryIsNotSetUp()
		{
			var subscriber = NewChangedTableListSubscriber();
			ServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			AssertEquals(false, subscriber.IsRequired());
		}

		public void TestProcessChanges()
		{
			var changedTables = new List<ITableSchema> { GetTableSchema("JobShipment"), GetTableSchema("JobDeclaration"), GetTableSchema("OrgHeader") };
			clientMock.Setup(c => c.PostAsync("odata/Index/ReportTablesChanged", It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
			{
				var contentAsString = content.ReadAsStringAsync().GetAwaiter().GetResult();
				AssertEquals(@"{""tableNames"":[""JobShipment"",""JobDeclaration"",""OrgHeader""]}", contentAsString);
			}).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted))).Verifiable();

			var subscriber = NewChangedTableListSubscriber();
			subscriber.ProcessChanges(null, changedTables);
			clientMock.Verify();
		}

		public void TestProcessChanges_ErrorStatusCode()
		{
			var changedTables = new List<ITableSchema> { GetTableSchema("JobShipment"), GetTableSchema("JobDeclaration"), GetTableSchema("OrgHeader") };
			clientMock.Setup(c => c.PostAsync("odata/Index/ReportTablesChanged", It.IsAny<HttpContent>())).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway))).Verifiable();

			var subscriber = NewChangedTableListSubscriber();
			AssertExceptionThrown<HttpRequestException>(() => subscriber.ProcessChanges(null, changedTables));
			clientMock.Verify();
		}

		public void TestProcessChanges_UnauthorizedExceptionWithLogger_LogonDetailsIncorrect()
		{
			var changedTables = new List<ITableSchema> { GetTableSchema("JobShipment"), GetTableSchema("JobDeclaration"), GetTableSchema("OrgHeader") };
			clientMock.Setup(c => c.PostAsync("odata/Index/ReportTablesChanged", It.IsAny<HttpContent>())).Throws(new AuthorizationFailureException(AuthenticationResult.LogonDetailsIncorrect));
			var logger = new LoggerForTest();

			var subscriber = NewChangedTableListSubscriber();
			subscriber.ProcessChanges(logger, changedTables);

			AssertLogEntries(
				"authentication error log",
				string.Join("\r\n", logger.LogEntries),
				"Authentication Error sending request to odata/Index/ReportTablesChanged... Message: Unexpected authentication result: LogonDetailsIncorrect");
		}

		public void TestProcessChanges_UnauthorizedExceptionWithoutLogger_LogonDetailsIncorrect()
		{
			var changedTables = new List<ITableSchema> { GetTableSchema("JobShipment"), GetTableSchema("JobDeclaration"), GetTableSchema("OrgHeader") };
			clientMock.Setup(c => c.PostAsync("odata/Index/ReportTablesChanged", It.IsAny<HttpContent>())).Throws(new AuthorizationFailureException(AuthenticationResult.LogonDetailsIncorrect));

			var expectedException = new AuthorizationFailureException(AuthenticationResult.LogonDetailsIncorrect);

			var subscriber = NewChangedTableListSubscriber();
			AssertExceptionThrown("Authorization result in message", typeof(HttpRequestException), "Authentication Error sending request to odata/Index/ReportTablesChanged... Message: Unexpected authentication result: LogonDetailsIncorrect", () => subscriber.ProcessChanges(null, changedTables), assertStartsWith: true);
			clientMock.Verify();
		}

		public void TestProcessChanges_UnauthorizedException_AccountLocked()
		{
			var changedTables = new List<ITableSchema> { GetTableSchema("JobShipment"), GetTableSchema("JobDeclaration"), GetTableSchema("OrgHeader") };
			clientMock.Setup(c => c.PostAsync("odata/Index/ReportTablesChanged", It.IsAny<HttpContent>())).Throws(new AuthorizationFailureException(AuthenticationResult.AccountLocked));

			var subscriber = NewChangedTableListSubscriber();
			AssertExceptionThrown("Authorization Exception account locked", typeof(AuthorizationFailureException), "Unexpected authentication result: AccountLocked", () => subscriber.ProcessChanges(null, changedTables), assertStartsWith: true);
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
				clientMock.Setup(c => c.PostAsync("odata/Index/ReportTablesChanged", It.IsAny<HttpContent>())).Returns(Task.FromResult(response));
				NewChangedTableListSubscriber().ProcessChanges(mockLogger.Object, new[] { GetTableSchema("JobShipment") });
			}

			using (var response = new HttpResponseMessage(HttpStatusCode.OK))
			{
				response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo/bar");
				clientMock.Setup(c => c.PostAsync("odata/Index/ReportTablesChanged", It.IsAny<HttpContent>())).Returns(Task.FromResult(response));
				NewChangedTableListSubscriber().ProcessChanges(mockLogger.Object, new[] { GetTableSchema("JobShipment") });
			}

			using (var response = new HttpResponseMessage(HttpStatusCode.Conflict))
			{
				response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://foo/bar");
				clientMock.Setup(c => c.PostAsync("odata/Index/ReportTablesChanged", It.IsAny<HttpContent>())).Returns(Task.FromResult(response));
				NewChangedTableListSubscriber().ProcessChanges(mockLogger.Object, new[] { GetTableSchema("JobShipment") });
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
			NewChangedTableListSubscriber().ProcessChanges(logger, new[] { GetTableSchema("JobShipment") });

			AssertLogEntries(
				"Should be wrapped",
				string.Join("\r\n", logger.LogEntries),
				"Error sending request to odata/Index/ReportTablesChanged... Message: Some Exception");
		}

		public void TestIsLoaded()
		{
			var glowServiceTask = new GlowSubscriberServiceTask();
			var glowSubscribers = new SubscriberLoader().EnumerateSubscribersOfType(glowServiceTask.AssemblyName, glowServiceTask.SubscriberNamespace);
			Assert(glowSubscribers.Any());
			var subscriberLoaded = false;

			foreach (object subscriber in glowSubscribers)
			{
				if (subscriber is GlowIndexUpdateSubscriber)
				{
					subscriberLoaded = true;
				}
			}
			Assert(subscriberLoaded);
		}

		ITableSchema GetTableSchema(string tableName)
		{
			var result = new Mock<ITableSchema>();
			result.Setup(r => r.TableName).Returns(tableName);
			return result.Object;
		}

		Mock<IGlowServiceClient> clientMock;
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
