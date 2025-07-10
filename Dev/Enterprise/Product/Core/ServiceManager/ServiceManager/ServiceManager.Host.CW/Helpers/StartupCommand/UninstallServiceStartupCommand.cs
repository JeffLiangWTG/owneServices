using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceControllerStatus = System.ServiceProcess.ServiceControllerStatus;

namespace ServiceManager.Host.CW
{
	class UninstallServiceStartupCommand : ServiceInstallerStartupCommand
	{
		public UninstallServiceStartupCommand(
			IHostLogger hostLogger,
			IServiceManagerHostOptions hostOptions,
			IManagedInstallerAdapter managedInstallerHelper,
			IServiceControllerManager serviceControllerManager,
			IProductRegistration productRegistration)
			: base(hostLogger, hostOptions, managedInstallerHelper, productRegistration)
		{
			this.serviceControllerManager = serviceControllerManager;
		}

		public override int Execute()
		{
			try
			{
				var result = UninstallService();
				if (result < 0)
				{
					return result;
				}

				hostLogger.LogInformation("Service has been successfully uninstalled.");

				if (hostOptions.RemoveDbRecord)
				{
					DeleteHostDatabaseRecord();
				}

				return result;
			}
			catch (Exception ex)
			{
				hostLogger.Log(LogLevel.Error, $"{DbConnectionConstants.ApplicationNames.ServiceHost} has encountered an uninstall error:{ex}", ex);
			}

			return -1;
		}

		int UninstallService()
		{
			var serviceControllers = serviceControllerManager.GetServiceControllers(hostOptions);
			foreach (var pair in serviceControllers.OrderBy(pair => pair.Key))
			{
				using var sc = pair.Value;
				if (sc != null && sc.Status != ServiceControllerStatus.Stopped)
				{
					serviceControllerManager.Stop(sc);
				}
			}

			var installerArgs = CreateServiceInstallerArguments(hostOptions);
			installerArgs.Add("-u");

			return RunInstaller(installerArgs.ToArray());
		}

		void DeleteHostDatabaseRecord()
		{
			hostLogger.LogInformation($"Database entry {(DeleteHost() ? "deleted" : "deletion FAILURE")}");

			bool DeleteHost()
			{
				var hostName = ServiceManagerHelper.GetHostName();
				using (Db.DisposableActionForDbConnection())
				{
					var rowsAffected = Db.Connection.ExecuteNonQuery($@"
	DELETE FROM [{StmServiceHostSchema.Constants.SqlSchemaName}].[{StmServiceHostSchema.Constants.TableName}]
	WHERE [{StmServiceHostSchema.Constants.SH_HostName}] = @hostName;",
						command => command.AddParameter("@hostName", SqlDbType.NVarChar, hostName));

					return rowsAffected == 1;
				}
			}
		}

		readonly IServiceControllerManager serviceControllerManager;
	}
}
