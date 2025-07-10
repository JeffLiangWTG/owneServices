using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using WTG.ApplicationLogging.Abstractions;

namespace CargoWise.Logging
{
	class CategorizedApplicationLoggerFactory : ICategorizedApplicationLoggerFactory
	{
		public CategorizedApplicationLoggerFactory(IApplicationLoggerFactory loggerFactory)
		{
			this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		public IApplicationLogger CreateCategorizedLogger(LoggerCategory category, string name, IEnumerable<KeyValuePair<string, object>> properties)
		{
			return CreateApplicationLogger(name);
		}

		public void AddProvider(ILoggerProvider provider)
		{
			loggerFactory.AddProvider(provider);
		}

		public IApplicationLogger CreateApplicationLogger(string name)
		{
			return loggerFactory.CreateApplicationLogger(name);
		}

		public ILogger CreateLogger(string categoryName)
		{
			return loggerFactory.CreateLogger(categoryName);
		}

		public void Dispose()
		{
			loggerFactory.Dispose();
		}

		readonly IApplicationLoggerFactory loggerFactory;
	}
}
