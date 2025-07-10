using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class LoggerExtensions
	{
		public static void LogVerboseOnly(this ISimpleLogger logger, LogType type, string message)
		{
			if (eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.Value)
			{
				logger.Log(type, message);
			}
		}
	}
}
