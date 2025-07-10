using Enterprise.Integration;

namespace ServiceManager.Integration.ServiceTasks.CW
{
	public interface ILoggerFactory
	{
		ILogger NewServiceTaskLogger(string dbServer, string dbName, string programCode, string logDirectoryPath = null);
		ILogger NewScheduledUpgradeLogger(string dbServer, string dbName);
	}
}
