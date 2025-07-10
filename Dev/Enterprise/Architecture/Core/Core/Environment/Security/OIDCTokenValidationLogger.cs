using System;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Enterprise.ZArchitecture.Core
{
	public sealed class OIDCTokenValidationLogger : ILogger
	{
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			var message = formatter(state, exception);

			if ((logLevel == LogLevel.Warning || logLevel == LogLevel.Error) && exception != null)
			{
				throw new SecurityTokenValidationException(message, exception);
			}

			if (logLevel == LogLevel.Error)
			{
				throw new SecurityTokenValidationException(message);
			}
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			// Previous implementations only handled warnings and above
			return logLevel >= LogLevel.Warning;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return NullDisposable.Instance;
		}
	}
}
