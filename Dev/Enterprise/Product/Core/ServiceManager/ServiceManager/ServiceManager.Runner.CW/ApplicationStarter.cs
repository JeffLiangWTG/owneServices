using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.ServiceManager.Runner.Extensions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;
using WTG.ApplicationLogging.Abstractions;
using WTG.ApplicationLogging.Builder;

namespace Enterprise.ServiceManager.Runner
{
	public sealed class ApplicationStarter(IApplicationLoggerFactory loggerFactory, IApplicationExceptionHandler exceptionHandler) : IDisposable
	{
		public static int Main(string[] args)
		{
			using var loggerFactory = ApplicationLoggingBuilder.Build(o => o
				.Configure(Product.CargoWise)
				.WithTracing());

			using var starter = new ApplicationStarter(loggerFactory, new ApplicationExceptionHandler(new ErrorReporterProxy()));

			return starter.Run(args);
		}

		public int Run(string[] args)
		{
			using var activity = logger.ActivitySource.StartActivity();

			int result = RunAndHandleAllExceptions(args);
			ShutdownLogging(exceptionHandler.WasDatabaseUpgradeHandledFromInitialization);

			return result;
		}

		int RunAndHandleAllExceptions(string[] args)
		{
			RunnerCommandLineArgsParser commandLineArguments;
			ITaskRunner runner;
			try
			{
				commandLineArguments = new RunnerCommandLineArgsParser(args);
				runner = Initialize(commandLineArguments);
			}
			catch (Exception ex)
			{
				exceptionHandler?.HandleFromInitialization(ex, ResolveLogger);
				return (int)RunnerExitCode.RunnerFailure;
			}

			try
			{
				var isSingleRun = commandLineArguments.SingleRunArgument;
				return (int)runner.Run(isSingleRun, exceptionHandler);
			}
			catch (Exception ex)
			{
				exceptionHandler?.HandleFromTask(ex, ResolveLogger);
				return (int)RunnerExitCode.RunnerFailure;
			}
		}

		ITaskRunner Initialize(RunnerCommandLineArgsParser commandLineArguments)
		{
			ApplicationLoggingBuilder.ConfigureEnvironment(loggerFactory, commandLineArguments.ProductKey);

			Globals.IsUserInteractive = false;

			DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceRunner;

			Db.InitializeDatabaseDetails(commandLineArguments.ServerName, commandLineArguments.DatabaseName);

			var consoleApp = commandLineArguments.OptionDebug;
			var isConnectionPooled = commandLineArguments.EnableConnectionPooling;

			serviceProvider = CompositionRoot.AddRegistrations(new ServiceCollection(), loggerFactory)
				.BuildServiceProvider();

			var errorReporter = consoleApp
				? (BaseExceptionReporter)serviceProvider.GetRequiredService<StdInputRunnerErrorReporter>()
				: serviceProvider.GetRequiredService<GrpcRunnerErrorReporter>();

			Initialisation.Initialiser.InitialiseServiceManager(errorReporter, isConnectionPooled);

			disposableActionForDbConnection = Db.DisposableActionForDbConnection();

			ResolveLogger().Log(LogLevel.Debug, "Initialize Logging");

			return consoleApp
				? CompositionRoot.ResolveInteractiveTaskRunner(serviceProvider)
				: CompositionRoot.ResolveGrpcRunner(serviceProvider, commandLineArguments.OptionGrpcGuid!);
		}

		[SuppressMessage("CargoWiseOne", "CW1106:Do Not Leave In Debug Messages", Justification = "Not debug messages. Consol.Error.WriteLine is used in production as the final logging fallback.")]
		void ShutdownLogging(bool silently)
		{
			try
			{
				if (!silently)
				{
					var runnerLogger = ResolveLogger();
					runnerLogger?.Log(LogLevel.Debug, "Shutdown logging system.");
				}
				serviceProvider!.GetRequiredService<ILoggerFinalizer>().ShutDownLog();
			}
			catch (Exception ex)
			{
				if (!silently && ex is not DatabaseUpgradeException)
				{
					Console.Error.WriteLine("Exception during logging system finalisation:");
					Console.Error.WriteLine(ex);
				}
			}
		}

		IRunnerLogger ResolveLogger() =>
			serviceProvider!.GetRequiredService<IRunnerLogger>();

		ServiceProvider? serviceProvider;
		IDisposable? disposableActionForDbConnection;

		readonly IApplicationLogger logger = loggerFactory.CreateRunnerLogger();

		public void Dispose()
		{
			serviceProvider?.Dispose();
			disposableActionForDbConnection?.Dispose();
		}
	}
}
