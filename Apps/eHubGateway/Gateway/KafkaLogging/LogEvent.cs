using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace CargoWise.eHub.Gateway
{
	[JsonObject]
	public class LogEvent
	{
		[JsonProperty("@timestamp")]
		public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

		public string MachineName { get; set; } = Environment.MachineName;

		public int ProcessId { get; set; } = Process.GetCurrentProcess().Id;

		public string ApplicationName { get; set; }

		public string LoggerName { get; set; }

		public string Level { get; set; }

		public string Message { get; set; }

		public string ExceptionID { get; set; }
	}
}
