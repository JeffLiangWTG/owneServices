using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.RemotePrinting.Server.RPSCore;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class ExceptionHandlingExtensionTest : TestCase
	{
		public void TestProcessMessageNoException()
		{
			AssertProcessMessageNoException(true);
		}

		public void TestProcessMessageNoExceptionStreamNotReady()
		{
			AssertProcessMessageNoException(false);
		}

		void AssertProcessMessageNoException(bool streamIsReady)
		{
			var stream = new TestSoapStream();
			var message = CreateSoapServerMessage(stream: stream);

			stream.IsReady = streamIsReady;

			var handler = new ExceptionHandlingExtension();

			SetSoapMessageStage(message, SoapMessageStage.BeforeDeserialize);
			AssertNoExceptionThrown(() => handler.ProcessMessage(message));

			SetSoapMessageStage(message, SoapMessageStage.AfterDeserialize);
			AssertNoExceptionThrown(() => handler.ProcessMessage(message));

			SetSoapMessageStage(message, SoapMessageStage.BeforeSerialize);
			AssertNoExceptionThrown(() => handler.ProcessMessage(message));

			SetSoapMessageStage(message, SoapMessageStage.AfterSerialize);
			AssertNoExceptionThrown(() => handler.ProcessMessage(message));

			AssertEquals("Should not report any errors", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestProcessMessageWithException()
		{
			AssertProcessMessageWithException(true, SoapMessageStage.BeforeDeserialize, false);
			AssertProcessMessageWithException(true, SoapMessageStage.AfterDeserialize, true, -1, 100, -1, 100);
			AssertProcessMessageWithException(true, SoapMessageStage.BeforeSerialize, true, -1, -1, 100, 100);
			AssertProcessMessageWithException(true, SoapMessageStage.AfterSerialize, false);
		}

		public void TestProcessMessageWithExceptionStreamNotReady()
		{
			AssertProcessMessageWithException(false, SoapMessageStage.BeforeDeserialize, false);
			AssertProcessMessageWithException(false, SoapMessageStage.AfterDeserialize, true, -1, -2, -1, -2);
			AssertProcessMessageWithException(false, SoapMessageStage.BeforeSerialize, true, -1, -1, -2, -2);
			AssertProcessMessageWithException(false, SoapMessageStage.AfterSerialize, false);
		}

		public void AssertProcessMessageWithException(bool streamIsReady, SoapMessageStage stage, bool expectErrorReport, long l1 = 0, long l2 = 0, long l3 = 0, long streamLength = 0)
		{
			var stream = new TestSoapStream();
			var message = CreateSoapServerMessage(stream: stream);

			stream.IsReady = streamIsReady;
			if (streamIsReady)
			{
				stream.SetLength(streamLength);
			}

			message.Exception = new SoapException("hello", new XmlQualifiedName("el1"));

			SetSoapMessageStage(message, stage);

			var handler = new ExceptionHandlingExtension();
			handler.Initialize(new CallInfo(false));
			handler.SkipHttpContextForTesting = true;

			AssertNoExceptionThrown(() => handler.ProcessMessage(message));

			if (expectErrorReport)
			{
				AssertEquals("Should report error", 1, ErrorReporter.TotalErrorCount);
				AssertSame(message.Exception, ErrorReporter.LastExceptionReported);

				var expectedMessage = $@"A Web Print Error occurred.
Soap Message Type: {message.GetType().Name}
Soap Message Stage: {stage.ToString()}
Soap Message Action: action
Soap Message Url: http://localhost/

Soap Request Stream Type: {(stage == SoapMessageStage.AfterDeserialize ? "TestSoapStream" : "")}
Soap Request Stream Length: {(stage == SoapMessageStage.AfterDeserialize ? streamLength.ToString() : "")}
Soap Request Stream Can Seek: {(stage == SoapMessageStage.AfterDeserialize && streamIsReady ? "True" : "")}

Soap Message Stream Type: TestSoapStream
Soap Message Stream Length: {streamLength}
Soap Message Stream Can Seek: {(streamIsReady ? "True" : "")}

Soap Message Stream Length Before Deserialize: {l1}
Soap Message Stream Length After Deserialize: {l2}
Soap Message Stream Length Before Serialize: {l3}";

				if (stage == SoapMessageStage.AfterDeserialize && streamIsReady)
				{
					expectedMessage += "\r\nSoap Request Info:\r\n";
				}

				AssertMultilineASCIIEquals(expectedMessage, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
			else
			{
				AssertEquals($"Should not check exceptions at {stage} stage", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestShouldReportError()
		{
			Assert(!ExceptionHandlingExtension.ShouldReportError(new XmlException("Unexpected end of file has occurred.")));
			Assert(!ExceptionHandlingExtension.ShouldReportError(new XmlException("Unexpected end of file while parsing Name has occurred.")));
			Assert(!ExceptionHandlingExtension.ShouldReportError(new XmlException("Root element is missing.")));
			Assert(!ExceptionHandlingExtension.ShouldReportError(new XmlException("There is an unclosed literal string.")));
			Assert(!ExceptionHandlingExtension.ShouldReportError(new XmlException("Data at the root level is invalid.")));

			Assert(!ExceptionHandlingExtension.ShouldReportError(new ApplicationException("Test", new XmlException("Root element is missing."))));

			Assert(ExceptionHandlingExtension.ShouldReportError(new ApplicationException("Test.")));
			Assert(ExceptionHandlingExtension.ShouldReportError(new XmlException("Test.")));

			Assert(!ExceptionHandlingExtension.ShouldReportError(RemotePrintingDbConnectionException.New(new DatabaseUpgradedException())));

			Assert(!ExceptionHandlingExtension.ShouldReportError(new InvalidOperationException("Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use and max pool size was reached.")));
		}

		#region Implementation

		static SoapServerMessage CreateSoapServerMessage(string action = "action", string url = "http://localhost/", Stream stream = null)
		{
			var method = new SoapServerMethod();
			var actionField = typeof(SoapServerMethod).GetField("action", BindingFlags.Instance | BindingFlags.NonPublic);
			actionField.SetValue(method, action);

			var request = new HttpRequest("file.name", url, "abc");

			var protocol = (SoapServerProtocol)Activator.CreateInstance(
				typeof(SoapServerProtocol),
				BindingFlags.Instance | BindingFlags.NonPublic,
				null,
				null,
				null);

			var methodField = typeof(SoapServerProtocol).GetField("serverMethod", BindingFlags.Instance | BindingFlags.NonPublic);
			methodField.SetValue(protocol, method);

			var requestField = typeof(ServerProtocol).GetField("request", BindingFlags.Instance | BindingFlags.NonPublic);
			requestField.SetValue(protocol, request);

			var message = (SoapServerMessage)Activator.CreateInstance(
				typeof(SoapServerMessage),
				BindingFlags.Instance | BindingFlags.NonPublic,
				null,
				new object[] { protocol },
				null);

			if (stream != null)
			{
				var streamField = typeof(SoapMessage).GetField("stream", BindingFlags.Instance | BindingFlags.NonPublic);
				streamField.SetValue(message, stream);
			}

			return message;
		}

		static void SetSoapMessageStage(SoapMessage message, SoapMessageStage stage)
		{
			var stageField = typeof(SoapMessage).GetField("stage", BindingFlags.Instance | BindingFlags.NonPublic);
			stageField.SetValue(message, stage);
		}

		#endregion

		#region TestSoapStream

		class TestSoapStream : Stream
		{
			public override void Flush()
			{
				EnsureStreamReady();
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				EnsureStreamReady();
				return 0;
			}

			public override void SetLength(long value)
			{
				EnsureStreamReady();
				length = value;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				EnsureStreamReady();
				return 0;
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				EnsureStreamReady();
			}

			public override bool CanRead
			{
				get
				{
					EnsureStreamReady();
					return true;
				}
			}

			public override bool CanSeek
			{
				get
				{
					EnsureStreamReady();
					return true;
				}
			}

			public override bool CanWrite
			{
				get
				{
					EnsureStreamReady();
					return true;
				}
			}

			public override long Length
			{
				get
				{
					EnsureStreamReady();
					return length;
				}
			}
			long length;

			public override long Position
			{
				get
				{
					EnsureStreamReady();
					return position;
				}
				set
				{
					EnsureStreamReady();
					position = value;
				}
			}
			long position;

			void EnsureStreamReady()
			{
				if (!IsReady)
				{
					throw new InvalidOperationException("Stream is not ready");
				}
			}

			public bool IsReady { get; set; }
		}

		#endregion
	}
}
