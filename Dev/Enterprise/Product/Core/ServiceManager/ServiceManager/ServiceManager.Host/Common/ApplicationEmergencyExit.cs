using System;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class ApplicationEmergencyExit : IApplicationEmergencyExit
	{
		public ApplicationEmergencyExit(IEventLogger eventLogger, IApplicationExitProxy applicationExitProxy, IHostLogger hostLogger, IErrorReporterProxy errorReporterProxy)
		{
			this.eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
			this.applicationExitProxy = applicationExitProxy ?? throw new ArgumentNullException(nameof(applicationExitProxy));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
		}

		public void ExitApplicationUnsafe(string message, Exception ex)
		{
			try
			{
				CargoWise.Data.Db.DisableThreadSchemaVersionCheckPermanently();
				ExecuteIgnoringExceptions(() => eventLogger.Log(LogLevel.Error, message, ex));
				ExecuteIgnoringExceptions(() => hostLogger.Log(LogLevel.Error, message, ex));
				ExecuteIgnoringExceptions(() => errorReporterProxy.ReportOnce(message, ex));
			}
			finally
			{
				applicationExitProxy.Exit(-1);
			}
		}

		public void ExitApplicationUnsafe(string message)
		{
			try
			{
				CargoWise.Data.Db.DisableThreadSchemaVersionCheckPermanently();
				ExecuteIgnoringExceptions(() => eventLogger.Log(LogLevel.Warning, message));
				ExecuteIgnoringExceptions(() => hostLogger.Log(LogLevel.Warning, message));
			}
			finally
			{
				applicationExitProxy.Exit(-1);
			}
		}

		static void ExecuteIgnoringExceptions(Action action)
		{
			try
			{
				action?.Invoke();
			}
			catch
			{
				// Bad luck, have to ignore
			}
		}

		readonly IApplicationExitProxy applicationExitProxy;
		readonly IEventLogger eventLogger;
		readonly IHostLogger hostLogger;
		readonly IErrorReporterProxy errorReporterProxy;
	}
}
