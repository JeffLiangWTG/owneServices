using System;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	public sealed partial class DbBackupAndRestoreToolConnection : DbConnection<OdysseyAdminCredentials>
	{
		public DbBackupAndRestoreToolConnection(string serverName, string dbName, IErrorReporter errorReporter) : base(
			new SqlDataProviderFactory(
				serviceProvider.Value.GetRequiredService<IProtectedDataServiceFactory>().CreateEnterpriseService(serverName),
				serviceProvider.Value.GetRequiredService<ISqlConnectionProvider>()),
			serverName,
			dbName,
			applicationNameSuffix: null,
			errorReporter: errorReporter)
		{
			AssertErrorReporter(errorReporter);
			noActionConnectionErrorHandler = new NoActionConnectionErrorManager(this);
		}

		// Here we use constructor injection to provide the error reporter though ErrorReporter can be configured by its static property in CargoWise app.
		// The reason is DbBackupAndRestoreToolConnection is used outside CargoWise system, we have to provide the error reporter explicitly and then the error can be redirected to the correct place.
		[Conditional("DEBUG")]
		static void AssertErrorReporter(IErrorReporter errorReporter)
		{
			if (errorReporter is null)
			{
				throw new ArgumentNullException(nameof(errorReporter));
			}
		}

		public override string UserLogin => OdysseyAdminCredentials.AdminUserName;

		protected override void RunTasksAfterOpenConnection()
		{
		}

		public override bool HandleError(System.Data.Common.DbException sqlException)
		{
			return false;
		}

		[ThreadSafe]
		static readonly Lazy<IServiceProvider> serviceProvider = new Lazy<IServiceProvider>(CreateServiceProvider);
		static IServiceProvider CreateServiceProvider()
		{
			var services = new ServiceCollection();
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			return services.BuildServiceProvider();
		}

		readonly ConnectionErrorManager noActionConnectionErrorHandler;
		internal override ConnectionErrorManager ConnectionErrorHandler => noActionConnectionErrorHandler;
	}
}
