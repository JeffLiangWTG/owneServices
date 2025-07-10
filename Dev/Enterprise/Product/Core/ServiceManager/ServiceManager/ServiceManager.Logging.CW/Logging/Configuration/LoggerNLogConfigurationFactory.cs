using CargoWise.Data;

namespace ServiceManager.Logging.CW
{
	class LoggerNLogConfigurationFactory : ILoggerNLogConfigurationFactory
	{
		public ILoggerNLogConfiguration GetConfiguration(string currentDatabase)
		{
			return Db.DatabaseName == currentDatabase
				? new LoggerNLogRegistryConfiguration()
				: new LoggerNLogSimpleConfiguration();
		}
	}
}
