using System;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	public class MessageWriterTraceListenerTest : TestCase
	{
		public void TestWrite()
		{
			DummyMessageWriter dummyMessageWriter = new DummyMessageWriter();

			MessageWriterTraceListener listener;
			AssertExceptionThrown<ArgumentNullException>("MessageWriterTraceListener requires non-null IMessageWriter and TraceFilter",
				() => listener = new MessageWriterTraceListener(null, null, null));

			listener = new MessageWriterTraceListener(dummyMessageWriter, "filter", null);

			listener.Write("Something to write");
			AssertEquals("MessageWriterTraceListener Write without prefix", "\r\nSomething to write", dummyMessageWriter.Messages[0]);

			listener.Write("Writing something with the word filter");
			AssertEquals("Filter should not be used by MessageWriterTraceListener directly", "\r\nWriting something with the word filter", dummyMessageWriter.Messages[1]);

			listener = new MessageWriterTraceListener(dummyMessageWriter, "filter", () => "Message prefix:");
			listener.Write("Something to write");
			AssertEquals("MessageWriterTraceListener Write with prefix", "\r\nMessage prefix:\r\nSomething to write", dummyMessageWriter.Messages[2]);
		}

		public void TestWriteLine()
		{
			DummyMessageWriter dummyMessageWriter = new DummyMessageWriter();

			MessageWriterTraceListener listener = new MessageWriterTraceListener(dummyMessageWriter, "filter", () => "Message prefix:");
			listener.WriteLine("Something to write");
			AssertEquals("MessageWriterTraceListener WriteLine", "Something to write\r\n", dummyMessageWriter.Messages[0]);
		}
	}
}
