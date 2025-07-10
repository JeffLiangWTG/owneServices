namespace ServiceManager.Logging.CW
{
	public interface ILoggerNLogConfigurationFactory
	{
		ILoggerNLogConfiguration GetConfiguration(string currentDatabase);
	}
}
