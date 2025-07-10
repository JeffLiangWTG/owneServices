using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public abstract class ExceptionHandler : IExceptionHandler
	{
		public enum ExceptionAction
		{
			Report = 0,
			LogError = 1,
			LogWarning = 2,
			Ignore = 3,
		}

		public bool HandleSpecificExceptions(Exception ex, string message, Lazy<ILogger> logger)
		{
			var exceptionAction = GetExceptionAction(ex);
			switch (exceptionAction)
			{
				case ExceptionAction.Ignore:
					return true;
				case ExceptionAction.LogWarning:
					logger.Value.Log(LogLevel.Warning, ex, message);
					return true;
				case ExceptionAction.LogError:
					logger.Value.Log(LogLevel.Error, ex, message);
					return true;
				case ExceptionAction.Report:
					return false;
				default:
					throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "Unsupported Runner Exception Action: {0}", exceptionAction));
			}
		}

		ExceptionAction GetExceptionAction(Exception ex)
		{
			if (ex is AggregateException aggregateEx)
			{
				var worstExceptionAction = ExceptionAction.Ignore;
				foreach (var innerException in aggregateEx.Flatten().InnerExceptions)
				{
					var innerAction = GetExceptionAction(innerException);
					if (innerAction < worstExceptionAction)
					{
						worstExceptionAction = innerAction;
					}
				}

				return worstExceptionAction;
			}

			var exceptionAction = EvaluateExceptionAction(ex);

			if (ex.InnerException != null)
			{
				var innerAction = GetExceptionAction(ex.InnerException);
				return innerAction > exceptionAction ? innerAction : exceptionAction;
			}

			return exceptionAction;
		}

		protected abstract ExceptionAction EvaluateExceptionAction(Exception ex);
	}
}
