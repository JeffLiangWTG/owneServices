using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Services.Protocols;
using System.Xml;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class RetryProcessorTest : TestCase
	{
		public void TestShouldRetryWebException()
		{
			var retryProcessor = new RetryProcessorForTest();

			Assert(!retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.Success)));

			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.MessageLengthLimitExceeded)));
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.ConnectFailure)));
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.ConnectionClosed)));
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.SecureChannelFailure)));
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.SendFailure)));
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.ReceiveFailure)));
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.Timeout)));
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.KeepAliveFailure)));
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.RequestCanceled)));

			Assert(!retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", WebExceptionStatus.TrustFailure)));

			var response = new Mock<HttpWebResponse>();
			response.Setup(m => m.StatusCode).Returns(HttpStatusCode.RequestEntityTooLarge);
			Assert(retryProcessor.ShouldRetryWebException_Exposed(new WebException("Test", null, WebExceptionStatus.ProtocolError, response.Object)));
		}

		public void TestShouldRetryXmlException()
		{
			var retryProcessor = new RetryProcessorForTest();
			Assert(retryProcessor.ShouldRetry_Exposed(new ApplicationException("Test", new XmlException("abc"))));
			Assert(retryProcessor.ShouldRetry_Exposed(new Exception("Test", new Exception("xyz", new XmlException("abc")))));
		}

		public void TestShouldRetrySoapException()
		{
			var retryProcessor = new RetryProcessorForTest();
			Assert(retryProcessor.ShouldRetry_Exposed(new ApplicationException("Test", new SoapException("abc", new XmlQualifiedName("xyz")))));
		}

		public void TestShouldRetryIOException()
		{
			var retryProcessor = new RetryProcessorForTest();
			Assert(retryProcessor.ShouldRetry_Exposed(new ApplicationException("Test", new IOException("abc"))));
		}

		[ExpectNoExceptions]
		public void TestRetryProcessorSuccess()
		{
			for (var i = RetryProcessor.MinRetries; i <= RetryProcessor.MaxRetries + 1; i++)
			{
				AssertRetryProcessorSuccessWithRetryCounter(i);
			}
		}

		[ExpectNoExceptions]
		void AssertRetryProcessorSuccessWithRetryCounter(int retries)
		{
			var callCounter = 0;

			int FaultyRequest()
			{
				callCounter++;
				if (callCounter < retries)
				{
					throw new WebException("Test", WebExceptionStatus.Timeout);
				}
				return callCounter;
			}

			var opertaionName = ((Func<int>)FaultyRequest).Method.Name;
			var retryProcessor = new RetryProcessor();
			retryProcessor.RetrySuccessfullyOperations.Clear();
			if (retries > RetryProcessor.MaxRetries)
			{
				NUnit.Framework.Assert.That(delegate
				{
					retryProcessor.InvokeFunc(FaultyRequest, retries, TimeSpan.Zero);
				}, CustomConstraints.InnermostExceptionThrown(typeof(WebException)));
				Assert("Should contains FaultyRequest", !retryProcessor.RetrySuccessfullyOperations.Contains(opertaionName));
			}
			else
			{
				var result = retryProcessor.InvokeFunc(FaultyRequest, retries, TimeSpan.Zero);

				AssertEquals($"Should repeat {retries} times", retries, callCounter);
				AssertEquals("Should return correct result", retries, result);
				Assert("Should contains FaultyRequest", retryProcessor.RetrySuccessfullyOperations.Contains(opertaionName));
			}
		}

		[ExpectNoExceptions]
		public void TestRetryProcessorFail()
		{
			var callCounter = 0;

			int FaultyRequest()
			{
				callCounter++;
				throw new WebException("Test", WebExceptionStatus.Timeout);
			}

			var retryProcessor = new RetryProcessor();

			NUnit.Framework.Assert.That(delegate
			{
				retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.Zero);
			}, CustomConstraints.InnermostExceptionThrown(typeof(WebException)), "Test");
			AssertEquals($"Should repeat {RetryProcessor.MaxRetries} times", RetryProcessor.MaxRetries, callCounter);
		}

		[ExpectNoExceptions]
		public void TestRetryProcessorDoNotRetry()
		{
			var callCounter = 0;

			int FaultyRequest()
			{
				callCounter++;
				throw new ApplicationException("Test");
			}

			var retryProcessor = new RetryProcessor();

			NUnit.Framework.Assert.That(delegate
			{
				retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.Zero);
			}, CustomConstraints.InnermostExceptionThrown(typeof(ApplicationException)), "Test");
			AssertEquals("Should repeat 1 time", 1, callCounter);
		}

		public void TestRetryProcessorWithSimpleRequest()
		{
			var callCounter = 0;
			int FaultyRequest()
			{
				callCounter++;
				if (callCounter < 2)
				{
					throw new WebException("Test", WebExceptionStatus.MessageLengthLimitExceeded);
				}
				return callCounter;
			}

			var simpleRequestCalled = false;
			int SimpleRequest()
			{
				AssertEquals("FaultyRequest should have been called 1 time before simple request", 1, callCounter);
				simpleRequestCalled = true;
				return 0;
			}

			var retryProcessor = new RetryProcessor();
			retryProcessor.SimpleRequestDelegate = SimpleRequest;

			var result = retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.Zero);

			AssertEquals("Should repeat 2 times", 2, callCounter);
			AssertEquals("Should return correct result", 2, result);
			Assert("Should call simple request", simpleRequestCalled);
		}

		public void TestRetryProcessorWithSimpleRequestNotNeeded()
		{
			var callCounter = 0;
			int FaultyRequest()
			{
				callCounter++;
				if (callCounter < 2)
				{
					throw new WebException("Test", WebExceptionStatus.Timeout);
				}
				return callCounter;
			}

			var simpleRequestCalled = false;
			int SimpleRequest()
			{
				AssertEquals("FaultyRequest should have been called 1 time before simple request", 1, callCounter);
				simpleRequestCalled = true;
				return 0;
			}

			var retryProcessor = new RetryProcessor();
			retryProcessor.SimpleRequestDelegate = SimpleRequest;

			var result = retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.Zero);

			AssertEquals("Should repeat 2 times", 2, callCounter);
			AssertEquals("Should return correct result", 2, result);
			Assert("Should NOT call simple request for WebException.Status != MessageLengthLimitExceeded", !simpleRequestCalled);
		}

		[ExpectNoExceptions]
		public void TestRetry_LogRetryAttempt()
		{
			var callCounter = 0;
			int FaultyRequest()
			{
				callCounter++;
				throw new WebException("Test", WebExceptionStatus.Timeout);
			}

			var notifications = new Notifications();
			var retryProcessor = new RetryProcessor(notifications);

			NUnit.Framework.Assert.That(delegate
			{
				retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.Zero);
			}, CustomConstraints.InnermostExceptionThrown(typeof(WebException)), "Test");
			AssertEquals($"Should repeat {RetryProcessor.MaxRetries} times", RetryProcessor.MaxRetries, callCounter);

			AssertEquals(RetryProcessor.MaxRetries - 1, notifications.Messages.Count);
			for (int i = 1; i < RetryProcessor.MaxRetries; i++)
			{
				AssertCollectionContains($"  - retrying operation, attempt #{i}", notifications.Messages);
			}
		}

		[ExpectNoExceptions]
		public void TestRetryWithMaxRetriesAndMaxTime()
		{
			var callCounter = 0;
			var delay = TimeSpan.FromMilliseconds(10);
			int FaultyRequest()
			{
				callCounter++;
				Thread.Sleep(delay);
				throw new WebException("Test", WebExceptionStatus.Timeout);
			}

			var notifications = new Notifications();
			var retryProcessor = new RetryProcessor(notifications);

			callCounter = 0;
			NUnit.Framework.Assert.That(delegate
			{
				retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.Zero);
			}, CustomConstraints.InnermostExceptionThrown(typeof(WebException)), "Test");
			AssertEquals($"Should repeat {RetryProcessor.MaxRetries} times", RetryProcessor.MaxRetries, callCounter);

			callCounter = 0;
			NUnit.Framework.Assert.That(delegate
			{
				retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.FromMilliseconds(5000));
			}, CustomConstraints.InnermostExceptionThrown(typeof(WebException)), "Test");
			AssertEquals($"Should repeat {RetryProcessor.MaxRetries} times", RetryProcessor.MaxRetries, callCounter);

			callCounter = 0;
			NUnit.Framework.Assert.That(delegate
			{
				retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.FromMilliseconds(115));
			}, CustomConstraints.InnermostExceptionThrown(typeof(WebException)), "Test");
			AssertEquals("Should repeat 2 times", 2, callCounter);

			callCounter = 0;
			NUnit.Framework.Assert.That(delegate
			{
				retryProcessor.InvokeFunc(FaultyRequest, RetryProcessor.MaxRetries, TimeSpan.FromMilliseconds(5));
			}, CustomConstraints.InnermostExceptionThrown(typeof(WebException)), "Test");
			AssertEquals("Should repeat 1 time", 1, callCounter);
		}

		public void TestShouldRetryWithRedirectResponse()
		{
			MethodDelegate retryAction = null;
			Exception outException = null;
			var responseProcessorMock = new Mock<IErrorResponseWebRequestProcessor>();
			responseProcessorMock.Setup(r => r.Process(It.IsAny<MethodDelegate>(), out retryAction, out outException, It.IsAny<bool>()))
				.Throws(new InvalidOperationException("Jerry Test"))
				.Verifiable();

			var retryProcessor = new RetryProcessor();
			retryProcessor.ResponseProcessor = responseProcessorMock.Object;

			AssertExceptionThrown<OperationRetryException>(() => retryProcessor.InvokeAction((content) => { }, 3, TimeSpan.Zero, string.Empty));
			responseProcessorMock.Verify();
		}

		class Notifications : INotifications
		{
			public List<string> Messages { get; } = new List<string>();

			public void AddMessage(string message)
			{
				Messages.Add(message);
			}
		}

		class RetryProcessorForTest : RetryProcessor
		{
			public RetryProcessorForTest(INotifications notifications = null)
				: base(notifications)
			{
			}

			public bool ShouldRetry_Exposed(Exception exception) => ShouldRetry(exception);

			public bool ShouldRetryWebException_Exposed(WebException exception) => ShouldRetryWebException(exception);
		}
	}
}
