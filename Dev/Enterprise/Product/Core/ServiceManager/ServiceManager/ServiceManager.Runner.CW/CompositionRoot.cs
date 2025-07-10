using CargoWise.Logging;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Logging.CW;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using WTG.ApplicationLogging.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	public static class CompositionRoot
	{
		public static IServiceCollection AddRegistrations(IServiceCollection services, IApplicationLoggerFactory loggerFactory)
		{
			_ = services ?? throw new ArgumentNullException(nameof(services));

			return services
				.AddSingleton(provider => CargoWise.Application.ObjectFactory.Get<IClientHostedServiceAttributeProvider>())
				.AddSingleton<IServiceTaskRunnerWithNextRunTimeCheckFactory>(provider => new ServiceTaskRunnerWithNextRunTimeCheckFactory(provider))

				.AddTransient(provider => loggerFactory)
				.AddTransient(provider => loggerFactory.AsCategorizedApplicationLoggerFactory())

				.AddTransient<NativeServiceTaskRunnerWithNextRunTimeCheck>()

				.AddTransient<IEnvironmentChecker, DbConnectionDisposerChecker>()
				.AddTransient<IEnvironmentChecker, TransactionChecker>()
				.AddTransient<IEnvironmentChecker, UndisposedSqlLockChecker>()
				.AddTransient<IEnvironmentChecker, LoggerConfigurationChecker>()
				.AddTransient<IEnvironmentChecker, UserContextLeakChecker>()
				.AddTransient<IProcessEnvironmentRecorder, ProcessEnvironmentRecorderProvider>()

				.AddTransient<ISqlMutexLocker, SqlMutexLocker>()
				.AddTransient<IServiceTaskLocker, SqlMutexLocker>()
				.AddTransient<IServiceTaskHandlerFactory, ServiceTaskHandlerFactory>()

				.AddTransient<IDefaultScheduleConfigurer, DefaultScheduleConfigurer>()
				.AddTransient<IServiceTaskRequirementsChecker, ServiceTaskRequirementsChecker>()

				.AddTransient<NativeServiceTaskTransactionAdapter>()
				.AddTransient<NativeServiceTaskLoader>()
				.AddSingleton<ITransactionAdapterFactory>(provider => new RunnerTransactionAdapterFactory(provider))
				.AddSingleton<IServiceTaskLoaderFactory>(provider => new RunnerServiceTaskLoaderFactory(provider))
				.AddSingleton<IServiceTaskRunnerStrategy, IServiceTaskRunnerCanceler, ServiceTaskRunnerStrategy>()

				.AddTransient<StdInputRunnerErrorReporter, StdInputRunnerErrorReporter>()
				.AddTransient<GrpcRunnerErrorReporter, GrpcRunnerErrorReporter>()
				.AddTransient<IServiceTaskRunnerQueueService, ServiceTaskRunnerQueueService>()
				.AddTransient<StdInputRunner, StdInputRunner>()

				// Registry
				.AddSingleton<DirectRunnerRegistrySettings>()
				.AddSingleton<IRunnerRegistrySettings>(o => o.GetRequiredService<DirectRunnerRegistrySettings>())
				.AddSingleton<ISharedRegistrySettings>(o => o.GetRequiredService<DirectRunnerRegistrySettings>())

				// Shared
				.AddTransient<IServiceTaskScheduleManager, ServiceTaskScheduleManager>()
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<IServiceTaskScheduleStatusProvider>())
				.AddTransient<IServiceHostMessageDispatcher, ServiceHostMessageDispatcher>()
				.AddTransient<IDateTimeProvider, ServiceManagerDateTimeProvider>()
				.AddSingleton(o => CargoWise.Application.ObjectFactory.Get<INudgingController>())

				// Business
				.AddSingleton<IHostedServiceAttributeProvider, HostedServiceAttributeProvider>()

				// CW1 Services
				.RegisterCommonServices()
				.RegisterRunnerLoggerServices()
				.RegisterSharedServices()
				.RegisterRunnerServices();
		}

		public static ITaskRunner ResolveGrpcRunner(IServiceProvider provider, string grpcGuid) =>
			new GrpcRunner(
				provider.GetRequiredService<IServiceTaskRunnerQueueService>(),
				provider.GetRequiredService<IRunnerLogger>(),
				grpcGuid,
				provider.GetRequiredService<IClientHostedServiceAttributeProvider>(),
				provider.GetRequiredService<IHostCommunicationStrategy>(),
				provider.GetRequiredService<ICommandExecutionStrategy>(),
				provider.GetRequiredService<IRunnerRegistrySettings>(),
				provider.GetRequiredService<INudgingController>());

		public static ITaskRunner ResolveInteractiveTaskRunner(IServiceProvider provider) => provider.GetRequiredService<StdInputRunner>();
	}
}
