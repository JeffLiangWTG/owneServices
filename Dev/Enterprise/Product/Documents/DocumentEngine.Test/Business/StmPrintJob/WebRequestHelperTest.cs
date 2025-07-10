using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed partial class WebRequestHelperTest : TestCaseWithFactory
	{
		const string ServiceUrl = "http://localhost:54443/";
		const string ServiceHostName = "test.wtg.com";

		public void TestShouldNoExceptionWhenSendNudgeRequestViaIPAddressWithHostnameIsNull()
		{
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = "ServerTest";
			queue.SQ_QueueName = "NameTest";
			queue.SQ_WebPrintServiceAddress = "http://127.0.0.1:80";

			AssertNoExceptionThrown(() => _ = WebRequestHelper.NudgePrintServerAsync(queue, ZGuid.NewZGuid()).Result);
		}

		public void TestShouldNotSendNudgeRequestIfWebPrintServiceAddressHostNameIsEmpty()
		{
			try
			{
				SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest();
				AssertEquals("Should not suspend sending nudge", false, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());

				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = false, SwtichBackToIPAddressIntervalInHours = 24, ChangingToUrlAddressDateTimeUtc = DateTime.UtcNow });
				var queue = Factory.New<StmPrintQueue>();
				queue.SQ_ServerName = "ServerTest";
				queue.SQ_QueueName = "NameTest";
				queue.SQ_WebPrintServiceAddress = ServiceUrl;
				var result = WebRequestHelper.NudgePrintServerAsync(queue, ZGuid.NewZGuid()).Result.Success;

				AssertEquals("Should not report error", string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals("Should not send nudge request", false, result);
			}
			finally
			{
				SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest();
				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			}
		}

		public void TestShouldNotSendNudgeRequestIfWebPrintServiceAddressURLIsInvalid()
		{
			var queue = Factory.New<StmPrintQueue>();
			queue.SQ_ServerName = "ServerTest";
			queue.SQ_QueueName = "NameTest";
			queue.SQ_WebPrintServiceAddress = "http:///";
			var result = WebRequestHelper.NudgePrintServerAsync(queue, ZGuid.NewZGuid()).Result.Success;

			AssertEquals("Should not report error", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Should not send nudge request", false, result);
		}

		public void TestSendNudgeRequestWithRedirectResponse_PermanentRedirect()
		{
			AssertSendNudgeRequestWithRedirectResponse((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), "308"), "http://testnew.wtg.com", "http://testnew.wtg.com:80/", false);
			AssertSendNudgeRequestWithRedirectResponse((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), "308"), "http://testnew.wtg.com", "http://testnew.wtg.com:80/", true);
		}

		public void TestSendNudgeRequestWithRedirectResponse_MovedPermanently()
		{
			AssertSendNudgeRequestWithRedirectResponse(HttpStatusCode.MovedPermanently, "http://testnew.wtg.com", "http://testnew.wtg.com:80/", false);
			AssertSendNudgeRequestWithRedirectResponse(HttpStatusCode.MovedPermanently, "http://testnew.wtg.com", "http://testnew.wtg.com:80/", true);
		}

		public void TestSendNudgeRequestWithRedirectResponse_Found()
		{
			AssertSendNudgeRequestWithRedirectResponse(HttpStatusCode.Found, "http://testnew.wtg.com", null, false);
			AssertSendNudgeRequestWithRedirectResponse(HttpStatusCode.Found, "http://testnew.wtg.com", null, true);
		}

		public void TestSendNudgeRequestWithRedirectResponse_TemporaryRedirect()
		{
			AssertSendNudgeRequestWithRedirectResponse(HttpStatusCode.TemporaryRedirect, "http://testnew.wtg.com", null, false);
			AssertSendNudgeRequestWithRedirectResponse(HttpStatusCode.TemporaryRedirect, "http://testnew.wtg.com", null, true);
		}

		void AssertSendNudgeRequestWithRedirectResponse(HttpStatusCode statusCode, string location, string expectedUrl, bool enableIPAddress)
		{
			try
			{
				SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest();
				AssertEquals("Should not suspend sending nudge", false, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());

				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = enableIPAddress, SwtichBackToIPAddressIntervalInHours = 24, ChangingToUrlAddressDateTimeUtc = DateTime.UtcNow });
				var headers = new WebHeaderCollection();
				headers.Add("Location", location);

				var getResponseCounter = 0;
				var mockHttpWebResponse = new Moq.Mock<HttpWebResponse>();
				mockHttpWebResponse.Setup(o => o.StatusCode)
					.Returns(() =>
					{
						if (getResponseCounter == 1)
						{
							return statusCode;
						}
						else
						{
							return HttpStatusCode.Accepted;
						}
					});
				mockHttpWebResponse.Setup(o => o.Headers).Returns(headers);

				var mockHttpWebRequest = new Moq.Mock<HttpWebRequest>();
				mockHttpWebRequest.Setup(o => o.RequestUri).Returns(new Uri("http://127.0.0.1:80"));
				mockHttpWebRequest.Setup(o => o.GetResponse()).Returns(() =>
				{
					getResponseCounter++;
					return mockHttpWebResponse.Object;
				});

				WebRequestHelper.SentWebRequestForTest.Value = mockHttpWebRequest.Object;
				WebRequestHelper.SkipActualSendingRequestForTest.Value = false;

				var queue = Factory.New<StmPrintQueue>();
				queue.SQ_ServerName = "ServerTest";
				queue.SQ_QueueName = "NameTest";
				queue.SQ_WebPrintServiceAddress = "http://127.0.0.1:80|host:test.wtg.com";
				var result = WebRequestHelper.NudgePrintServerAsync(queue, ZGuid.NewZGuid()).Result.Success;

				AssertEquals("Should not report error", string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals("Should send nudge request successfully", true, result);

				if (!string.IsNullOrEmpty(expectedUrl))
				{
					AssertEquals(expectedUrl, WebRequestRedirectResponseHelper.RedirectUrls["test.wtg.com"]);
				}
				else
				{
					AssertEquals(0, WebRequestRedirectResponseHelper.RedirectUrls.Count);
				}
			}
			finally
			{
				WebRequestRedirectResponseHelper.RedirectUrls.Clear();
				SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest();
				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			}
		}

		public void TestNoExceptionThrownIfNudgeRequestWithRedirectToErrorPageIsSent()
		{
			AssertSendNudgeRequest_RedirectLogsInErrorReport(true, "", "/Error.aspx?data=testdata");
		}

		public void TestSendNudgeRequest_RedirectLogsInErrorReport_ViaIPAddress()
		{
			var expectedMessage = @"Running handle redirect response. URL: http://localhost:54443, Times: 1.
Handling redirect response, and it will send nudge request again if maximum retry count is not reached. ServiceUrl: http://localhost:54443, HostName: test.wtg.com
Running handle redirect response. URL: http://testnew.wtg.com:80/WebPrint, Times: 2.
Handling redirect response, and it will send nudge request again if maximum retry count is not reached. ServiceUrl: http://testnew.wtg.com:80/WebPrint, HostName: test.wtg.com
Running handle redirect response. URL: http://testnew.wtg.com:80/WebPrint, Times: 3.
Handling redirect response, and it will send nudge request again if maximum retry count is not reached. ServiceUrl: http://testnew.wtg.com:80/WebPrint, HostName: test.wtg.com
";
			AssertSendNudgeRequest_RedirectLogsInErrorReport(true, expectedMessage);
		}

		public void TestSendNudgeRequest_RedirectLogsInErrorReport_ViaHostName()
		{
			var expectedMessage = @"Running handle redirect response. URL: http://test.wtg.com:54443/WebPrint, Times: 1.
Handling redirect response, and it will send nudge request again if maximum retry count is not reached. ServiceUrl: http://test.wtg.com:54443/WebPrint, HostName: 
Running handle redirect response. URL: http://testnew.wtg.com:80/WebPrint, Times: 2.
Handling redirect response, and it will send nudge request again if maximum retry count is not reached. ServiceUrl: http://testnew.wtg.com:80/WebPrint, HostName: 
Running handle redirect response. URL: http://testnew.wtg.com:80/WebPrint, Times: 3.
Handling redirect response, and it will send nudge request again if maximum retry count is not reached. ServiceUrl: http://testnew.wtg.com:80/WebPrint, HostName: 
";
			AssertSendNudgeRequest_RedirectLogsInErrorReport(false, expectedMessage);
		}

		void AssertSendNudgeRequest_RedirectLogsInErrorReport(bool enableIPAddress, string expectedMessage, string location = null)
		{
			try
			{
				SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest();
				using (DocumentsDataRegistry.Instance.WebPrintNudge.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
						new WebPrintNudge
						{
							EnableIPAddress = enableIPAddress,
							SwtichBackToIPAddressIntervalInHours = 24,
							ChangingToUrlAddressDateTimeUtc = DateTime.UtcNow
						}))
				{
					var headers = new WebHeaderCollection();
					headers.Add("Location", location ?? "http://testnew.wtg.com/WebPrint/api/Nudge?serverName=9DC8JR3&printQueueName=Microsoft%20Print%20to%20PDF&printJobPK=00000000-0000-0000-0000-000000000000");

					var mockHttpWebResponse = new Moq.Mock<HttpWebResponse>();
					mockHttpWebResponse.Setup(o => o.StatusCode).Returns(HttpStatusCode.Redirect);
					mockHttpWebResponse.Setup(o => o.Headers).Returns(headers);

					var mockHttpWebRequest = new Moq.Mock<HttpWebRequest>();
					var serviceUrl = enableIPAddress ? ServiceUrl : $"{ServiceUrl}WebPrint";
					mockHttpWebRequest.Setup(o => o.RequestUri).Returns(new Uri(serviceUrl));
					mockHttpWebRequest.Setup(o => o.GetResponse()).Returns(mockHttpWebResponse.Object);

					WebRequestHelper.SentWebRequestForTest.Value = mockHttpWebRequest.Object;
					WebRequestHelper.SkipActualSendingRequestForTest.Value = false;

					var queue = Factory.New<StmPrintQueue>();
					queue.SQ_ServerName = "ServerTest";
					queue.SQ_QueueName = "NameTest";
					queue.SQ_WebPrintServiceAddress = serviceUrl + "|" + "host:" + ServiceHostName;
					var result = WebRequestHelper.NudgePrintServerAsync(queue, ZGuid.NewZGuid()).Result.Success;

					AssertEquals("Should report error", expectedMessage, ErrorReporter.LastMessageReported);
					AssertEquals("Should not send nudge request", false, result);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[TestDate(2022, 6, 13, 1, 1, 1, 100)]
		[TestUtcOffset(0, 0, 0)]
		[UseSnapshotProtection]
		public void TestSendNudgeRequest_DisableDirectIPAddress()
		{
			try
			{
				AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.RequestTimeout, false, "http://2.3.4.5:80", "abcdef.WISEGRID.NET");
				AssertEquals("Should disable sending nudge http request via IP address", false, DocumentsDataRegistry.Instance.WebPrintNudge.Value.EnableIPAddress);
				AssertEquals("Enable web service URL address in 24 hours", 24, DocumentsDataRegistry.Instance.WebPrintNudge.Value.SwtichBackToIPAddressIntervalInHours);
				AssertEquals("Should set a valid start date", new DateTime(2022, 6, 13, 1, 1, 1, 100), DocumentsDataRegistry.Instance.WebPrintNudge.Value.ChangingToUrlAddressDateTimeUtc);
			}
			finally
			{
				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			}
		}

		public void TestSendNudgeRequest_ViaURLAddress_Suspend()
		{
			try
			{
				SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest();
				AssertEquals("Should not suspend sending nudge", false, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());

				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = false, SwtichBackToIPAddressIntervalInHours = 24, ChangingToUrlAddressDateTimeUtc = DateTime.UtcNow });
				for (int i = 0; i < 2; i++)
				{
					AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.RequestTimeout, false, "http://2.3.4.5:80", "abcdef.WISEGRID.NET");
					AssertEquals("Should not suspend sending nudge", false, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());
				}

				AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.RequestTimeout, false, "http://2.3.4.5:80", "abcdef.WISEGRID.NET");

				AssertEquals("Should suspend sending nudge", true, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());
			}
			finally
			{
				SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest();
				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			}
		}

		public void TestSendNudgeRequest_ViaURLAddress_HandleErrors()
		{
			try
			{
				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = false, SwtichBackToIPAddressIntervalInHours = 24, ChangingToUrlAddressDateTimeUtc = DateTime.UtcNow });

				AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.LengthRequired, true, "http://2.3.4.5", "abcdef.wisegrid.net");
				AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.PreconditionFailed, true, "http://2.3.4.5", "abcdef.wisegrid.net");
			}
			finally
			{
				DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			}
		}

		public void TestNudgeRequestSend()
		{
			AssertNudgeRequestSend("Server1", "Queue1");
			AssertNudgeRequestSend("Server 2", "Queue 2");
		}

		void AssertNudgeRequestSend(string serverName, string queueName)
		{
			DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			var printQueue = Factory.New<StmPrintQueue>();

			printQueue.SQ_ServerName = serverName;
			printQueue.SQ_QueueName = queueName;
			printQueue.SQ_WebPrintServiceAddress = ServiceUrl + "|" + "host:" + ServiceHostName;

			var printJobPk = ZGuid.NewZGuid();

			using (WebDataRegistry.Instance.WebServiceUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "user1"))
			using (WebDataRegistry.Instance.WebServicePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "pwd"))
			{
				var result = WebRequestHelper.NudgePrintServerAsync(printQueue, printJobPk).Result;
				Assert("Should return successful result in tests", result.Success);
			}

			AssertNotNull(WebRequestHelper.LastSentWebRequestForTest.Value);

			var expectedArguments = $"serverName={WebUtility.UrlEncode(serverName)}&printQueueName={WebUtility.UrlEncode(queueName)}&printJobPK={printJobPk}&isForwarded=False";
			AssertEquals(expectedArguments, WebRequestHelper.LastSentWebRequestArguments.Value);

			var expectedUri = ServiceUrl + "api/Nudge?" + expectedArguments;
			AssertEquals(expectedUri, WebRequestHelper.LastSentWebRequestForTest.Value.RequestUri);

			AssertEquals(ServiceHostName, WebRequestHelper.LastSentWebRequestForTest.Value.Host);
			AssertEquals("POST", WebRequestHelper.LastSentWebRequestForTest.Value.Method);
			AssertEquals("user1", ((NetworkCredential)WebRequestHelper.LastSentWebRequestForTest.Value.Credentials).UserName);
			AssertEquals("pwd", ((NetworkCredential)WebRequestHelper.LastSentWebRequestForTest.Value.Credentials).Password);
			AssertEquals("KeepAlive should be false", false, WebRequestHelper.LastSentWebRequestForTest.Value.KeepAlive);
			AssertEquals("ServicePointManager.Expect100Continue should be true", true, ServicePointManager.Expect100Continue);
			AssertEquals("SystemDefault should be included in allowed security protocols", SecurityProtocolType.SystemDefault, ServicePointManager.SecurityProtocol);
		}

		public void TestNudgeRequestSend_DbUpgradeError_NoErrorReport()
		{
			DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			WebRequestHelper.SkipDisableIPAddressForTest.Value = true;
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage(534, false, ServiceUrl, ServiceHostName);
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage(535, false, ServiceUrl, ServiceHostName);
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage(536, false, ServiceUrl, ServiceHostName);

			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage(535, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage(535, false, "http://abcdef.wisegrid.net", "");
		}

		public void TestNudgeRequestSend_SocketExceptionNoReported()
		{
			DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			WebRequestHelper.SkipDisableIPAddressForTest.Value = true;
			var ex = new WebException($"Unable to connect to the remote server", new SocketException(10060));
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
		}

		public void TestNudgeRequestSend_ProtocolError_WiseGrid_ErrorReport()
		{
			DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			WebRequestHelper.SkipDisableIPAddressForTest.Value = true;
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.ServiceUnavailable, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.RequestTimeout, false, "http://abcdef.wisegrid.net:80", "");
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.NotFound, false, "http://abcdef.Wisegrid.net", "abcdef.wisegrid.net");

			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.LengthRequired, true, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.PreconditionFailed, true, "http://2.3.4.5", "abcdef.wisegrid.net");

			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.ServiceUnavailable, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.RequestTimeout, false, "http://2.3.4.5:80", "abcdef.WISEGRID.NET");
			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.NotFound, false, "http://2.3.4.5", "abcdef.wisegrid.net");

			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.LengthRequired, true, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.PreconditionFailed, true, "http://2.3.4.5", "abcdef.wisegrid.net");
		}

		public void TestNudgeRequestSend_ProtocolError_SelfHosted_ErrorReport()
		{
			DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			WebRequestHelper.SkipDisableIPAddressForTest.Value = true;
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.ServiceUnavailable, false, ServiceUrl, ServiceHostName);
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.RequestTimeout, false, ServiceUrl, ServiceHostName);
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.NotFound, false, ServiceUrl, ServiceHostName);

			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.LengthRequired, false, ServiceUrl, ServiceHostName);
			AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage((int)HttpStatusCode.PreconditionFailed, false, ServiceUrl, ServiceHostName);

			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.ServiceUnavailable, false, ServiceUrl, ServiceHostName);
			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.RequestTimeout, false, ServiceUrl, ServiceHostName);
			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.NotFound, false, ServiceUrl, ServiceHostName);

			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.LengthRequired, false, ServiceUrl, ServiceHostName);
			AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode.PreconditionFailed, false, ServiceUrl, ServiceHostName);
		}

		public void TestNudgeRequestSend_ErrorReport()
		{
			DocumentsDataRegistry.Instance.WebPrintNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new WebPrintNudge { EnableIPAddress = true });
			WebRequestHelper.SkipDisableIPAddressForTest.Value = true;
			var ex = new WebException("The request was aborted: The operation has timed out.", WebExceptionStatus.Timeout);
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(ex, false, ServiceUrl, ServiceHostName);

			AssertHandleOrReportException(new ApplicationException(), true, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(new ApplicationException(), true, ServiceUrl, ServiceHostName);

			ex = new WebException("The server committed a protocol violation.", WebExceptionStatus.ServerProtocolViolation);
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(ex, false, ServiceUrl, ServiceHostName);

			ex = new WebException("The remote name could not be resolved.", WebExceptionStatus.NameResolutionFailure);
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(ex, false, ServiceUrl, ServiceHostName);

			ex = new WebException("The remote service point could not be contacted at the transport level.", WebExceptionStatus.ConnectFailure);
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(ex, false, ServiceUrl, ServiceHostName);

			ex = new WebException("The request was aborted: The request was canceled.", WebExceptionStatus.RequestCanceled);
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(ex, false, ServiceUrl, ServiceHostName);

			ex = new WebException("The underlying connection was closed", WebExceptionStatus.ReceiveFailure);
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(ex, false, ServiceUrl, ServiceHostName);

			ex = new WebException("The underlying connection was closed: The connection was closed unexpectedly.", WebExceptionStatus.ConnectionClosed);
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(ex, false, ServiceUrl, ServiceHostName);

			ex = new WebException("The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.", new AuthenticationException("The remote certificate is invalid according to the validation procedure."));
			AssertHandleOrReportException(ex, false, "http://2.3.4.5", "abcdef.wisegrid.net");
			AssertHandleOrReportException(ex, false, ServiceUrl, ServiceHostName);
		}

		void AssertHandleOrReportException_ProtocolError_StatusCodeInErrorMessage(int statusCode, bool expectErrorReport, string url, string hostName)
		{
			var ex = new WebException($"Something something status {statusCode} Some error", WebExceptionStatus.ProtocolError);
			AssertHandleOrReportException(ex, expectErrorReport, url, hostName);
		}

		void AssertHandleOrReportException_ProtocolError_StatusCodeInHttpWebResponse(HttpStatusCode statusCode, bool expectErrorReport, string url, string hostName)
		{
			var response = new Mock<HttpWebResponse>();
			response.Setup(m => m.StatusCode).Returns(statusCode);
			response.Setup(m => m.Headers).Returns(new WebHeaderCollection());
			var ex = new WebException($"The request failed with HTTP status {HttpStatusCode.RequestEntityTooLarge}: Some error.", null, WebExceptionStatus.ProtocolError, response.Object);
			AssertHandleOrReportException(ex, expectErrorReport, url, hostName);
		}

		void AssertHandleOrReportException(Exception ex, bool expectErrorReport, string url, string hostName)
		{
			WebRequestHelper.ThrowExceptionForTest.Value = ex;

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_ServerName = "Server1";
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_WebPrintServiceAddress = url + "|" + "host:" + hostName;

			var printJobPk = ZGuid.NewZGuid();

			using (WebDataRegistry.Instance.WebServiceUsername.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "user1"))
			using (WebDataRegistry.Instance.WebServicePassword.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "pwd"))
			{
				ErrorReporter.Clear();

				var result = WebRequestHelper.NudgePrintServerAsync(printQueue, printJobPk).Result;

				Assert("Should return failed result in tests", !result.Success);

				if (!expectErrorReport)
				{
					AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				}
				else
				{
					AssertNull(ErrorReporter.LastKeyReported);

					var lastMessageReported = ErrorReporter.LastMessageReported;
					if (DocumentsDataRegistry.Instance.WebPrintNudge.Value.EnableIPAddress)
					{
						AssertContains("Request Url: " + url.TrimEnd('/') + "/api/Nudge?serverName=", lastMessageReported);
						AssertContains("Host Name: " + hostName, lastMessageReported);
					}
					else
					{
						AssertContains("Request Url: http://" + hostName.TrimEnd('/') + "/api/Nudge?serverName=", lastMessageReported);
					}
					AssertContains("Client IP: " + WebRequestHelper.GetHostIpAddress(), lastMessageReported);
					AssertContains("Action: Nudge", lastMessageReported);
					AssertContains("Arguments: serverName=", lastMessageReported);
					AssertContains("Elapsed time: ", lastMessageReported);

					if (ex is WebException)
					{
						AssertContains("WebRequest.Timeout:", lastMessageReported);
						AssertContains("WebRequest.KeepAlive:", lastMessageReported);
						AssertContains("ServicePointManager.SecurityProtocol:", lastMessageReported);
						AssertContains("ServicePointManager.Expect100Continue:", lastMessageReported);
						AssertContains("System.Net.WebException:", lastMessageReported);
						AssertContains("ServicePoint.MaxIdleTime: ", lastMessageReported);
						AssertContains("ServicePoint.ConnectionLeaseTimeout: ", lastMessageReported);
						AssertContains("ServicePoint.IdleSince: ", lastMessageReported);
					}

					ErrorReporter.Clear();
				}
			}
		}

		SecurityProtocolType originalSecurityProtocolType;
		bool originalExpect100Continue;

		protected override void SetUp()
		{
			base.SetUp();
			originalSecurityProtocolType = ServicePointManager.SecurityProtocol;
			originalExpect100Continue = ServicePointManager.Expect100Continue;
		}

		protected override void TearDown()
		{
			ServicePointManager.SecurityProtocol = originalSecurityProtocolType;
			ServicePointManager.Expect100Continue = originalExpect100Continue;

			base.TearDown();
		}
	}
}
