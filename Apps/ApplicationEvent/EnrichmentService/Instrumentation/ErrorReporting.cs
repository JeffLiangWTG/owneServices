using System.Reflection;
using WTG.ErrorReporting;

namespace eServices.ApplicationEvent.EnrichmentService.Instrumentation;

public interface IErrorReporting
{
	void RegisterHandlers();
	Task ReportErrorAsync(Exception exception);
}

public class ErrorReporting(
	IErrorReportingClient errorReportingClient,
	IHostEnvironment hostEnvironment,
	IHostApplicationLifetime hostApplicationLifetime,
	ILogger<ErrorReporting> logger) : IErrorReporting
{
	private readonly DateTime exeCreationTime = new FileInfo(Assembly.GetExecutingAssembly().Location)?.CreationTime ?? DateTime.Now;

	public async Task ReportErrorAsync(Exception exception)
	{
		if (hostApplicationLifetime.ApplicationStopping.IsCancellationRequested
			&& exception is (OperationCanceledException or AggregateException { InnerException: OperationCanceledException }))
			return;

		logger.Error(exception);

		try
		{
			if (errorReportingClient != null)
			{
				var reportingExcpetion = (exception as AggregateException)?.InnerException ?? exception;
				var errorBuilder = new EnterpriseErrorReportBuilder()
					.SetKey($"eHub {hostEnvironment.ApplicationName} {reportingExcpetion.Message}")
					.SetRootException(exception)
					.SetExceptionDescription($"Environment: {hostEnvironment.EnvironmentName}")
					.SetRandomErrorReportID()
					.SetTimeOfException(DateTime.Now)
					.SetExeCreationTime(exeCreationTime);

				await errorReportingClient.PostCrashReportAsync(errorBuilder);
			}
		}
		catch (Exception ex)
		{
			logger.Error(ex);
		}
	}

	public void RegisterHandlers()
	{
		TaskScheduler.UnobservedTaskException
			+= (object? sender, UnobservedTaskExceptionEventArgs e) => logger.Error(e.Exception);
		AppDomain.CurrentDomain.UnhandledException
			+= (object sender, UnhandledExceptionEventArgs e) => logger.Error((Exception)e.ExceptionObject);
	}
}

static partial class ErrorReportingLog
{
	[LoggerMessage(EventId = 51000, Level = LogLevel.Error)]
	public static partial void Error(this ILogger logger, Exception exception);
}