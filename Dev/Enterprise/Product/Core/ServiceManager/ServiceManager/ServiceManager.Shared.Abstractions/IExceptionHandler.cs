using System;
using Microsoft.Extensions.Logging;

namespace ServiceManager.Shared.Abstractions
{
	public interface IExceptionHandler
	{
		bool HandleSpecificExceptions(Exception ex, string message, Lazy<ILogger> logger);
	}
}
