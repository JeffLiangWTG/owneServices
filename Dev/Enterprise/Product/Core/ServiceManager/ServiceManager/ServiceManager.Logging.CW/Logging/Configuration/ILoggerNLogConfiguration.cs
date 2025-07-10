using Enterprise.Registry.Business;

namespace ServiceManager.Logging.CW
{
	public interface ILoggerNLogConfiguration
	{
		VerboseLoggingCollection VerboseLogging { get; }
		bool InternalNLogLoggingEnabled { get; }

		void Refresh();
	}
}
