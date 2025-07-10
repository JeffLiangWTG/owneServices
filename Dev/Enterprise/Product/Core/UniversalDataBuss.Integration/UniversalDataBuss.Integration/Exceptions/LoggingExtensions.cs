using System.Linq;
using Enterprise.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public static class LoggingExtensions
	{
		public static bool HasErrors(this ISimpleLogResult logger) => logger.Logs.Any(x => x.Type == LogType.Error);
	}
}
