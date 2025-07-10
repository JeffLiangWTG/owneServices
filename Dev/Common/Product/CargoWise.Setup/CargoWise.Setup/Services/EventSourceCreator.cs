using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CargoWise.Setup.Services;

interface IEventSourceCreator
{
	void CreateEventSource();
}

class EventSourceCreator(ILogger<EventSourceCreator> logger) : IEventSourceCreator
{
	public void CreateEventSource()
	{
		var sourceName = ProductConstants.ProductName;
		if (EventLog.SourceExists(sourceName))
		{
			logger.LogInformation($"EventSource {sourceName} already exists, skipping.");
			return;
		}

		logger.LogInformation($"Creating EventSource {sourceName}");
		EventLog.CreateEventSource(sourceName, "");
	}
}
