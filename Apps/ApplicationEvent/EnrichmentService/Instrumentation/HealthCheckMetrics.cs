namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public class HealthCheckMetrics
{
	public DateTimeOffset? EnrichmentHandlerCaptureStart { get; private set; }
	public long EnrichmentHandlerCount { get; private set; } = 0;
	public double EnrichmentHandlerAverageDuration { get; private set; } = 0;

	private long counterMessagesReceived = 0;
	private long counterMessagesSent = 0;
	private long counterMessagesSkipped = 0;
	private long counterMessagesFailed = 0;
	private long counterFaults = 0;
	public ConcurrentDictionary<string, long> CounterPerApplication { get; } = [];
	public DateTimeOffset? LastReceived { get; private set; }
	public DateTimeOffset? LastSent { get; private set; }
	public DateTimeOffset? LastSkipped { get; private set; }
	public DateTimeOffset? LastFailed { get; private set; }
	public DateTimeOffset? LastFaulted { get; private set; }

	public long GetCounterMessagesReceived() => counterMessagesReceived;
	public void AddCounterMessagesReceived(long value)
	{
		Interlocked.Add(ref counterMessagesReceived, value);
		LastReceived = DateTimeOffset.Now;
	}

	public long GetCounterMessagesSent() => counterMessagesSent;
	public void AddCounterMessagesSent(long value, ReadOnlySpan<KeyValuePair<string, object?>> tags)
	{
		Interlocked.Add(ref counterMessagesSent, value);
		if (tags.ToArray().FirstOrDefault(t => t.Key == "Enrichers") is var apps)
		{
			foreach (var app in apps.Value as ICollection<string> ?? [])
			{
				CounterPerApplication.AddOrUpdate(app, value, (key, oldVal) => oldVal + value);
			}
		}
		LastSent = DateTimeOffset.Now;
	}

	public long GetCounterMessagesSkipped() => counterMessagesSkipped;
	public void AddCounterMessagesSkipped(long value)
	{
		Interlocked.Add(ref counterMessagesSkipped, value);
		LastSkipped = DateTimeOffset.Now;
	}

	public long GetCounterMessagesFailed() => counterMessagesFailed;
	public void AddCounterMessagesFailed(long value)
	{
		Interlocked.Add(ref counterMessagesFailed, value);
		LastFailed = DateTimeOffset.Now;
	}

	public long GetCounterFaults() => counterFaults;
	public void AddCounterFaults(long value)
	{
		Interlocked.Add(ref counterFaults, value);
		LastFaulted = DateTimeOffset.Now;
	}

	private readonly object Lock_EnrichmentHandlerActivity = new();

	public void ResetEnrichmentHandlerMetrics()
	{
		if (EnrichmentHandlerCaptureStart.HasValue)
		{
			lock (Lock_EnrichmentHandlerActivity)
			{
				if (EnrichmentHandlerCaptureStart.HasValue)
				{
					EnrichmentHandlerCaptureStart = null;
					EnrichmentHandlerCount = 0;
					EnrichmentHandlerAverageDuration = 0;
				}
			}
		}
	}

	public void RecordEnrichmentHandlerActivity(Activity activity)
	{
		lock (Lock_EnrichmentHandlerActivity)
		{
			EnrichmentHandlerCaptureStart ??= DateTimeOffset.Now;
			EnrichmentHandlerCount++;
			EnrichmentHandlerAverageDuration += ((activity.Duration.TotalMicroseconds - EnrichmentHandlerAverageDuration) / EnrichmentHandlerCount);
		}
	}
}
