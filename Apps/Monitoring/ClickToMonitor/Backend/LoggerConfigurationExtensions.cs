using Serilog;

namespace XH.XT.Monitoring.ClickToMonitor.Backend
{
  public class LoggerConfigurationExtensions : Framework.Logging.ILoggerConfigurationExtension
  {
    public LoggerConfigurationExtensions()
    {
    }

    public LoggerConfiguration ConfigureLogger(LoggerConfiguration baseConfiguration)
    {
      return baseConfiguration.Destructure.With<MonitorDestructuringPolicy>().Enrich.FromLogContext();
    }
  }
}
