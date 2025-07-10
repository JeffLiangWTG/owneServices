using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core.Diagnostics
{
	public class TraceSourceConfiguration
	{
		public IReadOnlyCollection<TraceSourceSettingsConfiguration> TraceSourceSettings { get; set; }
		public IMessageWriter MessageWriter { get; set; }
		public Func<string> GetTracePrefix { get; set; }
		public ZBool LogCallStack { get; set; }
		public ZBool LogDateTime { get; set; }
		public ZBool LogThreadId { get; set; }
		public ZBool LogProcessId { get; set; }
	}

	public class TraceSourceSettingsConfiguration
	{
		public SourceLevels SourceLevel { get; set; }
		public ZString TraceSourceName { get; set; }
		public ZString TraceSourceDescription { get; set; }
		public string TraceFilter { get; set; }
	}
}
