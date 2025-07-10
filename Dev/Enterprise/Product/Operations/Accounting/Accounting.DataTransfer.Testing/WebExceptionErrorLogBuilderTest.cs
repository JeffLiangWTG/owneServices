using System.IO;
using System.Net;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class WebExceptionErrorLogBuilderTest : TestCaseWithFactory
	{
		ZString ExceptionMessage => "Exception For WebExceptionErrorLogBuilderTest";

		[ExpectNoExceptions]
		public void TestLogWhenExceptionIsNull()
		{
			var expectedMessage = "Web Exception was null";
			var actualMessage = WebExceptionErrorLogBuilder.BuildErrorLog(null);
			AssertEquals(expectedMessage, actualMessage);
		}

		[ExpectNoExceptions]
		public void TestLogWhenResponseIsNull()
		{
			var testException = new WebException(ExceptionMessage, WebExceptionStatus.UnknownError);
			try
			{ throw testException; }
			catch { /* trying to create stack trace */ }

			var expectedMessage = string.Format(@"Exception Message: Exception For WebExceptionErrorLogBuilderTest
Exception Status: UnknownError
Web Response was null
{0}", testException.StackTrace);

			var actualMessage = WebExceptionErrorLogBuilder.BuildErrorLog(testException);
			AssertEquals(expectedMessage, actualMessage);
		}

		[ExpectNoExceptions]
		public void TestLogWhenContentLength_ContentType_GetResponseStream_WillThrowException()
		{
			var testException = new WebException(ExceptionMessage, null, WebExceptionStatus.UnknownError, new MyWebResponseWithoutChildOverrides());
			try
			{ throw testException; }
			catch { /* trying to create stack trace */ }

			var expectedMessage = string.Format(@"Exception Message: Exception For WebExceptionErrorLogBuilderTest
Exception Status: UnknownError
Web Response Content Length: This property is not implemented by this class.
Web Response Content Type: This property is not implemented by this class.
Get Response Stream: This method is not implemented by this class.
{0}", testException.StackTrace);

			var actualMessage = WebExceptionErrorLogBuilder.BuildErrorLog(testException);
			AssertEquals(expectedMessage, actualMessage);
		}

		[ExpectNoExceptions]
		public void TestLogWhenResponseStreamIsNotReadable()
		{
			MyWebResponseWithChildOverrides response = null;
			using (var memoryStream = new MemoryStream())
			using (var writer = new StreamWriter(memoryStream))
			{
				writer.Write("An unknown error has occurred");
				writer.Flush();
				memoryStream.Position = 0;

				response = new MyWebResponseWithChildOverrides(29, "text/plain", memoryStream);
			}
			var testException = new WebException(ExceptionMessage, null, WebExceptionStatus.UnknownError, response);
			try
			{ throw testException; }
			catch { /* trying to create stack trace */ }

			var expectedMessage = string.Format(@"Exception Message: Exception For WebExceptionErrorLogBuilderTest
Exception Status: UnknownError
Web Response Content Length: 29
Web Response Content Type: text/plain
Response Stream does not support reading
{0}", testException.StackTrace);

			var actualMessage = WebExceptionErrorLogBuilder.BuildErrorLog(testException);
			AssertEquals(expectedMessage, actualMessage);
		}

		[ExpectNoExceptions]
		public void TestLogWhenTryingToReadResponseStreamCanThrowException()
		{
			using (var memoryStream = new MemoryStream())
			using (var writer = new StreamWriter(memoryStream))
			{
				writer.Write("An unknown error has occurred");
				writer.Flush();
				memoryStream.Position = 0;

				var response = new MyWebResponseWithChildOverrides(29, "text/plain", memoryStream);
				var testException = new WebException(ExceptionMessage, null, WebExceptionStatus.UnknownError, response);
				try
				{ throw testException; }
				catch
				{ /* trying to create stack trace */
				}

				var expectedMessage = string.Format(@"Exception Message: Exception For WebExceptionErrorLogBuilderTest
Exception Status: UnknownError
Web Response Content Length: 29
Web Response Content Type: text/plain
Response Stream: Stream was not readable.
{0}", testException.StackTrace);

				WebExceptionErrorLogBuilder.CloseStreamEventHandler_ForTestOnly = s => { s.Close(); };
				var actualMessage = WebExceptionErrorLogBuilder.BuildErrorLog(testException);
				AssertEquals(expectedMessage, actualMessage);
			}
		}

		[ExpectNoExceptions]
		public void TestLogWhenNoExceptionsAreThrown()
		{
			using (var memoryStream = new MemoryStream())
			using (var writer = new StreamWriter(memoryStream))
			{
				writer.Write("An unknown error has occurred");
				writer.Flush();
				memoryStream.Position = 0;

				var response = new MyWebResponseWithChildOverrides(29, "text/plain", memoryStream);
				var testException = new WebException(ExceptionMessage, null, WebExceptionStatus.UnknownError, response);
				try
				{ throw testException; }
				catch { /* trying to create stack trace */ }

				var expectedMessage = string.Format(@"Exception Message: Exception For WebExceptionErrorLogBuilderTest
Exception Status: UnknownError
Web Response Content Length: 29
Web Response Content Type: text/plain
Response Stream: An unknown error has occurred
{0}", testException.StackTrace);

				var actualMessage = WebExceptionErrorLogBuilder.BuildErrorLog(testException);
				AssertEquals(expectedMessage, actualMessage);
			}
		}

		class MyWebResponseWithoutChildOverrides : WebResponse
		{
		}

		class MyWebResponseWithChildOverrides : WebResponse
		{
			readonly long myContentLength;
			readonly string myContentType;
			readonly Stream myStream;

			public MyWebResponseWithChildOverrides(long contentLength, string contentType, Stream responseStream) : base()
			{
				myContentLength = contentLength;
				myContentType = contentType;
				myStream = responseStream;
			}

			public override long ContentLength
			{
				get
				{
					return myContentLength;
				}
			}

			public override string ContentType
			{
				get
				{
					return myContentType;
				}
			}

			public override Stream GetResponseStream()
			{
				return myStream;
			}
		}
	}
}
