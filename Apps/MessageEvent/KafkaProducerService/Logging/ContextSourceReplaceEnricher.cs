using Serilog.Core;
using Serilog.Events;
using SConstants = Serilog.Core.Constants;


namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Logging
{
	public class ContextSourceReplaceEnricher : ILogEventEnricher
	{
		public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
		{
			var properties = logEvent.Properties;

			if (properties.ContainsKey("CustomizedSourceContext"))
			{
				logEvent.AddOrUpdateProperty(new LogEventProperty(SConstants.SourceContextPropertyName, properties["CustomizedSourceContext"]));
			}
		}
	}
}
