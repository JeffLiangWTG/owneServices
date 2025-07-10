using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Registry.Business.Testing
{
	internal class CdsCredentialCheckerTest : TestCaseWithFactory
	{
		public void TestMcpPasswordOkAndTopicOk()
		{
			TestRunner(new HttpStatusCode[] { HttpStatusCode.OK },
						new string[] { "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><consumer endpointUrl='https://test.com' authorization='Basic TestCallbackAuthorization'/>" },
						"https://test.com",
						false);
		}

		public void TestMcpPasswordOkButTopicUnknown()
		{
			var forbiddenXml = "<?xml version='1.0' encoding='UTF-8' standalone='yes'?><errorResponse>    <code>NOT_AUTHORISED</code>    <message>You do not have authorization to access the requested TOPIC</message></errorResponse>";
			TestRunner(new HttpStatusCode[] { HttpStatusCode.Forbidden, HttpStatusCode.Forbidden },
						new string[] { forbiddenXml, forbiddenXml },
						"This may mean that the topic was not recognised by the CSP",
						true);
		}
		public void TestMcpPasswordOkAndTopicKNownButNotYetRegistered()
		{
			TestRunner(new HttpStatusCode[] { HttpStatusCode.NotFound, HttpStatusCode.NoContent },
						new string[] { String.Empty, String.Empty },
						null,
						false);
		}

		public void TestMcpPasswordBad()
		{
			TestRunner(new HttpStatusCode[] { HttpStatusCode.Unauthorized },
						new string[] { "<<HTML><HEAD>	<TITLE>Error 401--Unauthorized</TITLE> </HEAD>" },
						"The CSP has refused your username/password.",
						true);
		}

		public void TestCnsPasswordOkAndTopicOk()
		{
			TestRunner(new HttpStatusCode[] { HttpStatusCode.OK },
						new string[] { "<Consumer>	<endpointUrl>https://test.com</endpointUrl>	<authorization>891257f2-12b3-47d4-a801-73d9b54fbcd0</authorization></Consumer>" },
						"https://test.com",
						false);
		}

		public void TestCnsPasswordOkButTopicUnknown()
		{
			var notFOundXml = "<error>    <code>TOPIC_NOT_FOUND</code>    <message>Topic not found</message>    <topic>WISETECHTOPICXXXX</topic></error>";
			TestRunner(new HttpStatusCode[] { HttpStatusCode.NotFound, HttpStatusCode.NotFound },
						new string[] { notFOundXml, notFOundXml },
						"This may mean that the topic was not recognised by the CSP",
						true);
		}

		public void TestCnsPasswordBad()
		{
			TestRunner(new HttpStatusCode[] { HttpStatusCode.Unauthorized },
						new string[] { "<<HTML><HEAD>	<title>Community Network Services : Authentication Error</title>" },
						"The CSP has refused your username/password.",
						true);
		}

		public void TestOtherError()
		{
			TestRunner(new HttpStatusCode[] { HttpStatusCode.ServiceUnavailable },
						new string[] { "<<HTML><HEAD>	<title>Community Network Services : Service unavailable</title>" },
						"ServiceUnavailable",
						true);
		}

		void TestRunner(HttpStatusCode[] testHttpCodes, string[] testResponseTexts, string expectedFriendlyMessage, bool expectErrorIsReturned)
		{
			var checker = new CdsCredentialCheckerForTest();
			checker.MockedResults = new Queue<string>(testResponseTexts);
			checker.ExpectedHttpStatuses = new Queue<HttpStatusCode>(testHttpCodes);
			var result = checker.checkCdsCredentials(null, "", "", "");
			if (expectErrorIsReturned)
			{
				AssertContains(expectedFriendlyMessage, result.errorText);
			}
			else
			{
				if (expectedFriendlyMessage == null)
				{
					AssertNull(result.MessagesArray);
				}
				else
				{
					AssertContains(expectedFriendlyMessage, result.MessagesArray[0]);
				}
				AssertNull(result.errorText);
			}
		}

		class CdsCredentialCheckerForTest : CdsCredentialChecker
		{
			public Queue<string> MockedResults;

			public Queue<HttpStatusCode> ExpectedHttpStatuses;

			protected override IWebClient GetAndConfigureWebClient(ICredentials credentials, string userAgent)
			{
				return new WebClientForTest(MockedResults, ExpectedHttpStatuses);
			}
		}

		class WebClientForTest : IWebClient
		{
			public WebClientForTest(Queue<string> mockedResults, Queue<HttpStatusCode> expectedHttpStatuses)
			{
				this.expectedStatuses = expectedHttpStatuses;
				this.mockedResults = mockedResults;
			}

			string IWebClient.DownloadString(string address)
			{
				nextStatus = expectedStatuses.Dequeue();
				var intStatus = (int)nextStatus;
				if (intStatus >= 200 && intStatus < 300)
				{
					// 200 OK or 204 no content, etc
				}
				else
				{
					throw new WebException("Error for testing.", new Exception(nextStatus.ToString()));
				}
				return mockedResults.Dequeue();
			}
			HttpStatusCode IWebClient.GetStatusCode(WebException ex)
			{
				return nextStatus;
			}
			HttpStatusCode nextStatus;

			public void Dispose()
			{
			}

			public Task<HttpStatusCode> GetStatusCodeAsync(string url)
			{
				return Task.FromResult(nextStatus);
			}

#if NET8_0_OR_GREATER
			public HttpStatusCode GetStatusCode(System.Net.Http.HttpRequestException ex)
			{
				return nextStatus;
			}
#endif

			readonly Queue<string> mockedResults;
			readonly Queue<HttpStatusCode> expectedStatuses;
		}
	}
}
