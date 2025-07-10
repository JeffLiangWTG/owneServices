using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CargoWise.Application;
using Enterprise.ZArchitecture.Core.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	public class TracerTest : TestCase
	{
		public TracerTest()
		{
			MockMessageWriter = new Mock<IMessageWriter>();
		}

		public void TestInitializeTraceSources()
		{
			var tracer = new Tracer();
			var traceSourceConfiguration = SetupTracerConfiguration(MockMessageWriter.Object, GetEmptyMessagePrefix);

			tracer.InitializeTraceSources(traceSourceConfiguration);
			AssertNotNull("Trace source initialized", tracer["source1"]);
			AssertNotNull("Trace source switch initialized", tracer["source1"].Switch);
			AssertEquals("Trace source name initialized", "source1", tracer["source1"].Switch.DisplayName);
			AssertEquals("Trace source level initialized", SourceLevels.All, tracer["source1"].Switch.Level);
			Assert("Trace source listener initialized", tracer["source1"].Listeners.Count == 1);
			var listener = tracer["source1"].Listeners[0] as MessageWriterTraceListener;
			AssertEquals("Trace filter initialized", "filter", listener.TraceFilter);
			AssertEquals("Trace output options initialized", TraceOptions.None, listener.TraceOutputOptions);

			tracer.RemoveAllTraceSources();
			traceSourceConfiguration.LogCallStack = true;
			traceSourceConfiguration.LogDateTime = true;
			traceSourceConfiguration.LogThreadId = true;
			traceSourceConfiguration.LogProcessId = true;
			tracer.InitializeTraceSources(traceSourceConfiguration);
			listener = tracer["source1"].Listeners[0] as MessageWriterTraceListener;
			AssertEquals("Trace output options initialized", TraceOptions.Callstack | TraceOptions.DateTime | TraceOptions.ThreadId | TraceOptions.ProcessId, listener.TraceOutputOptions);
		}

		public void TestGetTraceSource()
		{
			var tracer = new Tracer();
			var traceSourceConfiguration = SetupTracerConfiguration(MockMessageWriter.Object, GetEmptyMessagePrefix);
			tracer.InitializeTraceSources(traceSourceConfiguration);

			AssertNotNull("Get trace source", tracer["source1"]);
			AssertNotNull("Get trace source creates new trace source", tracer["newSource"]);
		}

		public void TestGetTraceFilter()
		{
			var tracer = new Tracer();
			var traceSourceConfiguration = SetupTracerConfiguration(MockMessageWriter.Object, GetEmptyMessagePrefix);
			tracer.InitializeTraceSources(traceSourceConfiguration);

			AssertEquals("Get trace filter", "filter", tracer.GetTraceFilter("source1"));
		}

		public void TestRemoveTraceSource()
		{
			var tracer = new Tracer();
			var traceSourceConfiguration = SetupTracerConfiguration(MockMessageWriter.Object, GetEmptyMessagePrefix);
			tracer.InitializeTraceSources(traceSourceConfiguration);
			Assert("Three trace sources added to tracer", tracer.CountTraceSources == 3);

			tracer.RemoveTraceSource("source4");
			Assert("Remove trace source with wrong source name does not remove any trace sources", tracer.CountTraceSources == 3);

			tracer.RemoveTraceSource("source1");
			Assert("Remove trace source", tracer.CountTraceSources == 2);

			tracer.RemoveAllTraceSources();
			Assert("Remove all trace sources", tracer.CountTraceSources == 0);
		}

		public void TestCheckWhetherTraceSourceExists()
		{
			var tracer = new Tracer();
			var traceSourceConfiguration = SetupTracerConfiguration(MockMessageWriter.Object, GetEmptyMessagePrefix);
			tracer.InitializeTraceSources(traceSourceConfiguration);

			Assert("Check whether trace source returns false", !tracer.CheckWhetherTraceSourceExists("source4"));
			Assert("Check whether trace source returns true", tracer.CheckWhetherTraceSourceExists("source1"));
			Assert("Check whether trace source returns true with multiple source names", tracer.CheckWhetherTraceSourceExists("source1", "source2"));
		}

		public void TestGetITracer()
		{
			var tracer = ObjectFactory.Get<ITracer>();
			var otherTracer = ObjectFactory.Get<ITracer>();

			AssertSame("ITracer is a singleton", tracer, otherTracer);
		}

		public void TestIsEnabled()
		{
			var tracer = new Tracer();
			CombineAssertions("No sources are enabled before the tracer is intialised", () =>
			{
				Assert(!tracer.IsEnabled("source1"));
				Assert(!tracer.IsEnabled("source2"));
				Assert(!tracer.IsEnabled("source3"));
				Assert(!tracer.IsEnabled("NeverInitialisedSource"));
			});

			var msgWriter = new DummyMessageWriter();
			var traceSourceConfiguration = SetupTracerConfiguration(msgWriter, null);
			tracer.InitializeTraceSources(traceSourceConfiguration);

			CombineAssertions("Only specific sources are enabled based on SourceLevel configuration", () =>
			{
				Assert(tracer.IsEnabled("source1"));
				Assert(tracer.IsEnabled("source2"));
				Assert(!tracer.IsEnabled("source3"));
				Assert(!tracer.IsEnabled("NeverInitialisedSource"));
			});
		}

		public void TestTrace()
		{
			var tracer = new Tracer();
			var msgWriter = new DummyMessageWriter();
			var traceSourceConfiguration = SetupTracerConfiguration(msgWriter, null);
			tracer.InitializeTraceSources(traceSourceConfiguration);

			tracer.TraceCriticalEvent("source1", BuildMessage);
			AssertEquals("Trace critical event", "\r\nsource1 Critical: 1 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceErrorEvent("source1", BuildMessage);
			AssertEquals("Trace error event", "\r\nsource1 Error: 2 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceWarningEvent("source1", BuildMessage);
			AssertEquals("Trace warning event", "\r\nsource1 Warning: 3 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceInformation("source1", BuildMessage);
			AssertEquals("Trace information", "\r\nsource1 Information: 4 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceVerbose("source1", BuildMessage);
			AssertEquals("Trace verbose", "\r\nsource1 Verbose: 5 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceStartingOfActivity("source1", BuildMessage);
			AssertEquals("Trace starting of activity", "\r\nsource1 Start: 6 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceStoppingOfActivity("source1", BuildMessage);
			AssertEquals("Trace stopping of activity", "\r\nsource1 Stop: 7 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceSuspensionOfActivity("source1", BuildMessage);
			AssertEquals("Trace suspension of activity", "\r\nsource1 Suspend: 8 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceResumptionOfActivity("source1", BuildMessage);
			AssertEquals("Trace resumption of activity", "\r\nsource1 Resume: 9 : Something to trace\r\n", string.Join("", msgWriter.Messages));

			msgWriter.Messages.Clear();
			tracer.TraceTransferEvent("source1", BuildMessage);
			AssertEquals("Trace transfer event", "\r\nsource1 Transfer: 10 : Something to trace\r\n", string.Join("", msgWriter.Messages));
		}

		public void TestCanTraceAndConfigureFromDifferentThreads()
		{
			var tracer = new Tracer();
			var msgWriter = new DummyMessageWriter();
			var traceSourceConfiguration = SetupTracerConfiguration(msgWriter, null);

			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			var configureThread = new Thread(() =>
			{
				while (stopwatch.Elapsed.TotalSeconds < 10)
				{
					tracer.InitializeTraceSources(traceSourceConfiguration);
					Thread.Sleep(1);

					tracer.RemoveAllTraceSources();
					Thread.Sleep(1);
				}
			});

			var traceThread = new Thread(() =>
			{
				while (stopwatch.Elapsed.TotalSeconds < 10)
				{
					tracer.TraceInformation("source1", BuildMessage);
					Thread.Sleep(1);
				}
			});

			configureThread.Start();
			traceThread.Start();

			Assert("Precondition: Configure Thread completed", configureThread.Join(TimeSpan.FromSeconds(20)));
			Assert("Precondition: Configure Thread completed", traceThread.Join(TimeSpan.FromSeconds(20)));

			AssertGreaterThan("At least one message must be traced", msgWriter.Messages.Count, 0);
		}

		TraceSourceConfiguration SetupTracerConfiguration(IMessageWriter messageWriter, Func<string> getTracePrefix)
		{
			TraceSourceConfiguration traceSourceConfigurations = new TraceSourceConfiguration();
			traceSourceConfigurations.TraceSourceSettings = new List<TraceSourceSettingsConfiguration>();

			TraceSourceSettingsConfiguration source1 = new TraceSourceSettingsConfiguration
			{
				SourceLevel = SourceLevels.All,
				TraceSourceName = "source1",
				TraceSourceDescription = "source description",
				TraceFilter = "filter"
			};

			TraceSourceSettingsConfiguration source2 = new TraceSourceSettingsConfiguration
			{
				SourceLevel = SourceLevels.Information,
				TraceSourceName = "source2",
				TraceSourceDescription = "source description",
				TraceFilter = "filter"
			};

			TraceSourceSettingsConfiguration source3 = new TraceSourceSettingsConfiguration
			{
				SourceLevel = SourceLevels.Off,
				TraceSourceName = "source3",
				TraceSourceDescription = "source description",
				TraceFilter = "filter"
			};

			traceSourceConfigurations.TraceSourceSettings = new List<TraceSourceSettingsConfiguration> { source1, source2, source3 };
			traceSourceConfigurations.MessageWriter = messageWriter;
			traceSourceConfigurations.GetTracePrefix = getTracePrefix;
			traceSourceConfigurations.LogCallStack = false;
			traceSourceConfigurations.LogDateTime = false;
			traceSourceConfigurations.LogThreadId = false;
			traceSourceConfigurations.LogProcessId = false;

			return traceSourceConfigurations;
		}

		string GetEmptyMessagePrefix() => "";
		string BuildMessage() => "Something to trace";

		readonly Mock<IMessageWriter> MockMessageWriter;
	}
}
