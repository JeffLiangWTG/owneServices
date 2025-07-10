using System;
using System.Threading;
using CargoWise.Data;
using Enterprise.Initialisation;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;

namespace ServiceManager.Host.CW
{
	class ServiceManagerHost
	{
		public ServiceManagerHost(IServiceManagerHostOptions hostOptions,
			IEventLogger eventLogger,
			BaseExceptionReporter exceptionReporter,
			IHostStartupCommandResolver commandResolver,
			IDbConnectionSetup dbConnectionSetup,
			IHostRegistry hostRegistry)
		{
			this.hostOptions = hostOptions ?? throw new ArgumentNullException(nameof(hostOptions));
			this.eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
			this.exceptionReporter = exceptionReporter ?? throw new ArgumentNullException(nameof(exceptionReporter));
			this.commandResolver = commandResolver ?? throw new ArgumentNullException(nameof(commandResolver));
			this.dbConnectionSetup = dbConnectionSetup ?? throw new ArgumentNullException(nameof(dbConnectionSetup));
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
		}

		public int Run()
		{
			try
			{
				ValidateHostOptions();

				ConfigureEnvironment();

				InitializeDbAndDependencies();

				return ExecuteStartupCommand();
			}
			catch (Exception ex)
			{
				eventLogger.Log(LogLevel.Error, ex.Message, ex);
			}

			return -1;
		}

		void ValidateHostOptions()
		{
			if (string.IsNullOrEmpty(hostOptions.ServerName) || string.IsNullOrEmpty(hostOptions.DatabaseName))
			{
				throw new ArgumentException("Both -ServerName and -DatabaseName argument must be provided");
			}
		}

		void ConfigureEnvironment()
		{
			DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceHost;

			Db.InitializeDatabaseDetails(Db.GetMachineNameIfLocal(hostOptions.ServerName), hostOptions.DatabaseName);

			if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
			{
				Thread.CurrentThread.Name = DbConnectionConstants.ApplicationNames.ServiceHost;
			}

			Initialiser.InitialiseServiceManager(exceptionReporter, usePooledConnection: true);
		}

		void InitializeDbAndDependencies()
		{
			Db.DisableSchemaVersionCheck();
			using (Db.DisposableActionForDbConnection())
			{
				dbConnectionSetup.TryConnectAndHandleErrors();
				hostRegistry.Initialize();
			}
		}

		int ExecuteStartupCommand()
		{
			return commandResolver.Resolve().Execute();
		}

		readonly IHostRegistry hostRegistry;
		readonly IHostStartupCommandResolver commandResolver;
		readonly IDbConnectionSetup dbConnectionSetup;
		readonly IEventLogger eventLogger;
		readonly BaseExceptionReporter exceptionReporter;
		readonly IServiceManagerHostOptions hostOptions;
	}
}
