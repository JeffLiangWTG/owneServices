namespace XH.XT.Monitoring.ClickToMonitor.Backend
{
  public class MonitoringData : BasicMonitoringData
  {
    public MonitoringData(BasicMonitoringData basicData) 
    {
      Clone(basicData);
    }
    public MonitoringData(BasicMonitoringData basicData, SessionSample session) : this(basicData) 
    {
      Session = session;
    }

    public MonitoringData(BasicMonitoringData basicData, OverdueMessage message) : this(basicData)
    {
      Message = message;
    }

    public MonitoringData(BasicMonitoringData basicData, QueueSample queue) : this(basicData)
    {
      Queue = queue;
    }

    public MonitoringData(BasicMonitoringData basicData, TransportOnHoldSample transport) : this(basicData)
    {
      Transport = transport;
    }

    public SessionSample Session { get; private set; } = new();

    public OverdueMessage Message { get; private set; } = new();

    public QueueSample Queue { get; private set; } = new();

    public TransportOnHoldSample Transport { get; private set; } = new();
  }
}
