using System.Diagnostics.Metrics;

namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public interface IMetrics
{
	ActivitySource? MsgActivitySource { get; }
	void AddMsgsRcvd(long delta);
	void AddMsgsSent(long delta, string[] enrichers);
	void AddMsgsSkip(long delta);
	void AddMsgsFail(long delta);
	void AddFaults(long delta);
}

public class Metrics : IMetrics
{
	public Metrics(IMeterFactory meterFactory, IHostEnvironment hostEnvironment)
	{
		var meter = meterFactory.Create(hostEnvironment.ApplicationName);
		msgsRcvd = meter.CreateCounter<long>(Names.Counter_MsgsRcvd);
		msgsSent = meter.CreateCounter<long>(Names.Counter_MsgsSent);
		msgsSkip = meter.CreateCounter<long>(Names.Counter_MsgsSkip);
		msgsFail = meter.CreateCounter<long>(Names.Counter_MsgsFail);
		faults = meter.CreateCounter<long>(Names.Counter_Faults);
		MsgActivitySource = new ActivitySource(hostEnvironment.ApplicationName);
	}

	private readonly Counter<long>? msgsRcvd = null;
	private readonly Counter<long>? msgsSent = null;
	private readonly Counter<long>? msgsSkip = null;
	private readonly Counter<long>? msgsFail = null;
	private readonly Counter<long>? faults = null;

	public void AddMsgsRcvd(long delta) => msgsRcvd?.Add(delta);

	public void AddMsgsSent(long delta, string[] enrichers)
		=> msgsSent?.Add(delta, new KeyValuePair<string, object?>("Enrichers", enrichers));

	public void AddMsgsSkip(long delta) => msgsSkip?.Add(delta);

	public void AddMsgsFail(long delta) => msgsFail?.Add(delta);

	public void AddFaults(long delta) => faults?.Add(delta);

	public ActivitySource? MsgActivitySource { get; } = null;

	public static class Names
	{
		public const string Counter_MsgsRcvd = "eservices.application_events.msgs_rcvd";
		public const string Counter_MsgsSkip = "eservices.application_events.msgs_skip";
		public const string Counter_MsgsSent = "eservices.application_events.msgs_sent";
		public const string Counter_MsgsFail = "eservices.application_events.msgs_fail";
		public const string Counter_Faults = "eservices.application_events.faults";
		public const string Activity_MsgsHandle = "msgs_handle";
		public const string Activity_MsgsMessageReceived = "msgs_messagereceived";
		public const string Activity_MsgsEnrichersSelected = "msgs_enrichers_selected";
		public const string Activity_MsgsEnrichmentFinished = "msgs_enrichement_finished";
		public const string Activity_MsgsAppEventSending = "msgs_appevent_sending";
		public const string Activity_MsgsAppEventSent = "msgs_appevent_sent";
		public const string Activity_EnricherFactoryGet = "enricherfactory_get";
		public const string Activity_EnricherFactoryMatched = "enricherfactory_matched";
		public const string Activity_EnricherFactoryCriterion = "enricherfactory_criterion";
	}
}
