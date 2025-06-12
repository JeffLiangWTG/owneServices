using Serilog.Core;
using Serilog.Events;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Logging
{
	public class LogLevelEnricher : ILogEventEnricher
	{
		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			var logLevel = string.Empty;
			switch (logEvent.Level)
			{
				case LogEventLevel.Verbose:
					logLevel = "TRC";
					break;

				case LogEventLevel.Debug:
					logLevel = "DBG";
					break;

				case LogEventLevel.Information:
					logLevel = "INF";
					break;

				case LogEventLevel.Error:
					logLevel = "ERR";
					break;

				case LogEventLevel.Fatal:
					logLevel = "FAL";
					break;

				case LogEventLevel.Warning:
					logLevel = "WRN";
					break;
			}

			logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("eHubLogLevel", logLevel));
		}
	}
}
