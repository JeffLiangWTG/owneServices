using Enterprise.Registry.Business;

namespace ServiceManager.Logging.CW
{
	class LoggerNLogSimpleConfiguration : ILoggerNLogConfiguration
	{
		public VerboseLoggingCollection VerboseLogging => new VerboseLoggingCollection();
		public bool InternalNLogLoggingEnabled => false;

		public void Refresh()
		{
		}
	}
}
