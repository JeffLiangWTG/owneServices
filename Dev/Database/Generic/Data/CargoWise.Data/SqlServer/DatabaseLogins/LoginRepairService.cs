using System;
using System.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.DataProtection.DefaultSecrets;
using Microsoft.Extensions.DependencyInjection;
namespace CargoWise.Data
{
	// This class is not meant to be used for new developement. Management of sql server security objects has to be done with Cargowise.Security solution.
	public class LoginRepairService
	{
		readonly IServiceProvider serviceProvider;
		readonly IProtectedDataServiceFactory pdsFactory;
		readonly IProtectedDataStateService stateService;
		readonly ProtectedDataServiceCapabilities pdsCaps;
		readonly ISqlServerLoginUtilities sqlServerLoginUtilities;

		public static LoginRepairService Instance => ActivatorUtilities.CreateInstance<LoginRepairService>(ProtectedDataService.GlobalServiceProvider);

		public LoginRepairService(IServiceProvider serviceProvider, IProtectedDataServiceFactory pdsFactory, IProtectedDataAdministrationService adminService, IProtectedDataStateService stateService, ProtectedDataServiceCapabilities pdsCaps, ISqlServerLoginUtilities sqlServerLoginUtilities)
		{
			this.serviceProvider = serviceProvider;
			this.pdsFactory = pdsFactory;
			this.stateService = stateService;
			this.pdsCaps = pdsCaps;
			this.sqlServerLoginUtilities = sqlServerLoginUtilities;
		}

		public void ReviveApplicationLogin<TCredentials>(DbConnection connectionToMainDb, string mainDbName, DbConnection connectionToTargetServer) where TCredentials : DBCredentials
		{
			var targetServerName = connectionToTargetServer.ServerName;
			var mainServerName = connectionToMainDb.ServerName;
			var pds = pdsFactory.CreateSystemService(connectionToMainDb.ServerName, mainDbName);
			var typeName = pdsCaps.GetSecretTypeName<TCredentials>();
			var contextManager = serviceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>() as CargowisePDSAdministrationSqlContextManager;
			using (var scope = contextManager.BeginScope(connectionToMainDb, connectionToTargetServer))
			{
				DBCredentials credentials = null;
#if DEBUG
				if (!connectionToMainDb.Exists("FROM Sys.Databases WHERE name = @mainDbName", cmd => cmd.AddParameter("@mainDbName", SqlDbType.NVarChar, mainDbName)))
				{
					var defaults = pds as IProtectedDataServiceWithDefaults;
					var defaultCredentialsId = defaults.GetDefaultProtectedDataIdFor(typeName);
					credentials = pds.LoadSecret(defaultCredentialsId).Data as DBCredentials;
				}
				else
				{
#endif
					var activeSecretId = stateService.GetActiveProtectedDataId(mainServerName, mainDbName, typeName);
					var activeSecret = pds.LoadSecret(activeSecretId.Value) as ProtectedData<TCredentials>;
					credentials = activeSecret.Value;
#if DEBUG
				}
#endif
				var targetServerContext = contextManager.GetSqlExecutionContext(targetServerName, Db.SqlMasterDb);
				sqlServerLoginUtilities.EnableOrCreateLogin(targetServerContext, credentials.UserName, credentials.Password, true, false);
			}
		}

		public void DropApplicationLogin<TCredentials>(DbConnection connectionToMainDb, string mainDbName, DbConnection connectionToTargetServer) where TCredentials : DBCredentials
		{
			var targetServerName = connectionToTargetServer.ServerName;
			var pds = pdsFactory.CreateSystemService(connectionToMainDb.ServerName, mainDbName);
			var typeName = pdsCaps.GetSecretTypeName<TCredentials>();
			var contextManager = serviceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>() as CargowisePDSAdministrationSqlContextManager;
			using (var scope = contextManager.BeginScope(connectionToMainDb, connectionToTargetServer))
			{
				var sqlContext = contextManager.GetSqlExecutionContext(targetServerName, "Master");
				string loginName;
#if DEBUG
				if (!connectionToMainDb.Exists("FROM Sys.Databases WHERE name = @mainDbName", cmd => cmd.AddParameter("@mainDbName", SqlDbType.NVarChar, mainDbName)))
				{
					var defaults = pds as IProtectedDataServiceWithDefaults;
					var defaultCredentialsId = defaults.GetDefaultProtectedDataIdFor(typeName);
					var defaultCredentials = pds.LoadSecret(defaultCredentialsId).Data as DBCredentials;
					loginName = defaultCredentials.UserName;
				}
				else
				{
#endif
					var activeSecretId = stateService.GetActiveProtectedDataId(connectionToMainDb.ServerName, mainDbName, typeName);
					var activeSecret = pds.LoadSecret(activeSecretId.Value) as ProtectedData<TCredentials>;
					loginName = activeSecret.Value.UserName;
#if DEBUG
				}
#endif
				sqlServerLoginUtilities.DropLogin(sqlContext, loginName);
			}
		}

		public bool LoginExists<TCredentials>(DbConnection connectionToMainDb, string mainDbName, DbConnection connectionToTargetServer) where TCredentials : DBCredentials
		{
			var targetServerName = connectionToTargetServer.ServerName;
			var pds = pdsFactory.CreateSystemService(connectionToMainDb.ServerName, mainDbName);
			var typeName = pdsCaps.GetSecretTypeName<TCredentials>();
			var contextManager = serviceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>() as CargowisePDSAdministrationSqlContextManager;
			using (var scope = contextManager.BeginScope(connectionToMainDb, connectionToTargetServer))
			{
				var activeSecred = stateService.GetActiveProtectedData<TCredentials>(connectionToMainDb.ServerName, mainDbName);
				var sqlContext = contextManager.GetSqlExecutionContext(targetServerName, "Master");
				var result = sqlContext.ExecuteNonQuery(
					$"IF EXISTS(SELECT null FROM sys.server_principals WHERE name = @LoginName) SELECT 1 ELSE SELECT 0", System.Data.CommandType.Text,
					cmd => cmd.AddParameter("@LoginName", System.Data.DbType.String, activeSecred.UserName)
					);
				return Convert.ToBoolean(result);
			}
		}
	}
}
