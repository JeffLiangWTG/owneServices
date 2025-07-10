using System.Collections.Generic;
using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.Logging
{
	public interface ICategorizedApplicationLoggerFactory : IApplicationLoggerFactory
	{
		IApplicationLogger CreateCategorizedLogger(LoggerCategory category, string name, IEnumerable<KeyValuePair<string, object>> properties);
	}
}
