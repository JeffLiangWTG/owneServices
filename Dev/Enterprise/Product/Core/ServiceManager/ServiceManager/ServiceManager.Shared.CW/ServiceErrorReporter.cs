using System;
using System.Diagnostics;
using System.Globalization;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Shared.CW
{
	public abstract class ServiceErrorReporter : BaseExceptionReporter
	{
		protected ServiceErrorReporter(IExceptionHandler exceptionHandler)
			: base(new ServiceHandler())
		{
			this.exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
		}

		protected abstract ILogger CurrentLogger { get; }

		public override void Report(string key, string message, Exception exception)
		{
			if (exception != null)
			{
				if (exceptionHandler.HandleSpecificExceptions(exception, message, new Lazy<ILogger>(() => CurrentLogger)))
				{
					return;
				}

				CurrentLogger.Log(LogLevel.Error, exception, message);
			}
			else
			{
				CurrentLogger.Log(LogLevel.Error, string.Format(CultureInfo.InvariantCulture, "{0}\r\n{1}", message, new StackTrace(2).ToString()));
			}

			base.Report(key, message, exception);
		}

		readonly IExceptionHandler exceptionHandler;

		class ServiceHandler : TopLevelExceptionHandler
		{
			//We don't have a UI so we can't show these messages, and we have already logged the exception, so we are safe to ignore these.

			public override void ShowError(string message) { }
			protected override void ShowError(string message, string caption) { }
			protected override void ShowWarning(string message) { }
		}
	}
}
