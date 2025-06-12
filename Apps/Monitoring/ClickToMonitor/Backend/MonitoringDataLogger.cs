using Newtonsoft.Json;
using Serilog;
using XH.Framework.Logging;
using Xware.Xt.ExternalWFService;

namespace XH.XT.Monitoring.ClickToMonitor.Backend
{
  [DefaultWFService]
  public class MonitoringDataLogger : IExternalWFService
  {
    private ILogger? logger;
    protected ILogger Logger
    {
      get
      {
        if (logger == null)
        {
          var extension = new LoggerConfigurationExtensions();
          logger = LoggerFactory.GetLogger(extension);
        }
        return logger;
      }
    }

    public bool InvokeTask(IXtHost serviceInterface, WFObject data, WFObject env, CancellationToken token)
    {
      try
      {
        var obj = JsonConvert.DeserializeObject<ClickToMonitorData>(data.ToJson()) ?? new ClickToMonitorData();
        foreach (var item in obj.OverdueMessages)
        {
          var dataObj = new MonitoringData(obj, item);
          Logger.ForContext("MonitorData", dataObj, true).Information("Message Lifetime monitoring data received");
        }
        foreach (var item in obj.QueueSamples)
        {
          var dataObj = new MonitoringData(obj, item);
          Logger.ForContext("MonitorData", dataObj, true).Information("Error Queue monitoring data received");
        }
        foreach (var item in obj.TransportOnHoldSamples)
        {
          var dataObj = new MonitoringData(obj, item);
          Logger.ForContext("MonitorData", dataObj, true).Information("Transport OnHold monitoring data received");
        }
        foreach (var item in obj.SessionSamples)
        {
          var dataObj = new MonitoringData(obj, item);
          Logger.ForContext("MonitorData", dataObj, true).Information("Session monitoring data received");
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error happened during logging monitoring data");
      }
      return true;
    }
  }
}
