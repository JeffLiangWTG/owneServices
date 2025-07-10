using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	public class ApplicationExceptionHandler : IApplicationExceptionHandler
	{
		public ApplicationExceptionHandler(IErrorReporterProxy errorReporterProxy)
		{
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
		}
		public void HandleFromInitialization(Exception ex, Func<IRunnerLogger> loggerProvider)
		{
			// No need to log DatabaseUpgrade when still initializing.
			if (ex.FlattenInnerExceptions().Any(x => x is DatabaseUpgradeException))
			{
				WasDatabaseUpgradeHandledFromInitialization = true;
				return;
			}

			HandleCommon(ex, loggerProvider, fromTask: false);
		}

		public bool WasDatabaseUpgradeHandledFromInitialization { get; private set; }

		public void HandleFromTask(Exception ex, Func<IRunnerLogger> loggerProvider)
		{
			HandleCommon(ex, loggerProvider, fromTask: true);
		}

		[SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "Not debug messages. Consol.Error.WriteLine is used in production as the final logging fallback.")]
		void HandleCommon(Exception ex, Func<IRunnerLogger> loggerProvider, bool fromTask)
		{
			// The database may have upgraded.
			// Disable schema checks so the error can still be reported.
			Db.DisableThreadSchemaVersionCheckPermanently();

			try
			{
				if (ex.IsCriticalException() || ExceptionReporter.Instance.HandleSpecificExceptions(ex))
				{
					var message = GetUnhandledMessage(fromTask);
					var runnerLogger = loggerProvider();
					if (runnerLogger != null)
					{
						runnerLogger.Log(LogLevel.Error, message, ex);
					}
					else
					{
						var writer = Console.Error;
						writer.WriteLine(message + ":");
						writer.WriteLine(ex);
					}
				}
				else
				{
					errorReporterProxy.ReportOnce(ExceptionConstants.RunnerExceptionLocation, ex);
				}
			}
			catch (Exception logException)
			{
				var writer = Console.Error;
				writer.WriteLine(GetUnhandledMessage(fromTask) + ":");
				writer.WriteLine(ex);
				writer.WriteLine("Logging exception:");
				writer.WriteLine(logException);
			}
		}

		static string GetUnhandledMessage(bool fromTask)
		{
			return fromTask
				? "Unhandled exception in Task Runner"
				: "Unhandled exception";
		}

		readonly IErrorReporterProxy errorReporterProxy;
	}
}
