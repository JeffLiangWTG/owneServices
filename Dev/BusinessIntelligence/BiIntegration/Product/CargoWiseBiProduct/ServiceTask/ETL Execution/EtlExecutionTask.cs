using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace CargoWise.Bi.Product.ServiceTask
{
	public abstract class EtlExecutionTask : ServiceProviderImpl, IBiNotificationSource
	{
		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			try
			{
				RunTaskWithRetry(iDoNotNeedToReactToThisToken);
			}
			catch (SqlException ex)
			{
				var shouldRetry = HandleSqlException(ex);
				if (shouldRetry)
				{
					RunTaskWithRetry(iDoNotNeedToReactToThisToken);
				}
			}
		}

		void RunTaskWithRetry(CancellationToken iDoNotNeedToReactToThisToken)
		{
			try
			{
				Run(iDoNotNeedToReactToThisToken);
			}
			catch (DatabaseUpgradeInProgressException)
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					var lockoutState = adminConnection.CheckLockoutState();
					switch (lockoutState)
					{
						case DbLockoutState.ValidLockout:
							throw;
						case DbLockoutState.InvalidLockout:
							ServiceLogger.Log(LogType.Warning, "Last database upgrade terminated unsuccessfully. Check logs for errors.");
							break;
						default:
							break;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "error message")]
		public bool HandleSqlException(SqlException ex)
		{
			var shouldRetry = false;
			var exceptionType = new DbErrorMatch(ex).ExceptionType;
			switch (exceptionType)
			{
				case DbErrorType.LoginDisabled:
				case DbErrorType.PermissionDeniedOnObject:
					var dbUserManager = new DbUserManager();
					dbUserManager.EnsureApplicationLoginForBiDatabase(BiServerName, BiDatabaseName);
					shouldRetry = true;
					break;
				case DbErrorType.DeadlockError:
					shouldRetry = true;
					break;
				case DbErrorType.InvalidObjectName:
					if (new Regex(string.Format(CultureInfo.InvariantCulture, @"'({0}\.)*cdc\..+?'", Db.DatabaseName), RegexOptions.IgnoreCase).IsMatch(ex.Message))
					{
						var errorMsg = "CDC tables are missing. Skipping ETL execution.";
						ServiceLogger.Log(LogType.Warning, errorMsg, ex);
					}
					else
					{
						throw ex;
					}
					break;
				case DbErrorType.CannotInitOleDbDataSourceObjForLinkedServer:
				case DbErrorType.TCPProviderConnectionAttemptFailed:
				case DbErrorType.InsufficientMemoryInBufferPool:
				case DbErrorType.CouldNotLocateDbInSysdatabases:
					if (!EnvProxy.IsHostedWithCargowise)
					{
						throw new HostedServiceException(ex.Message, ex) { LogException = true };
					}
					else
					{
						throw ex;
					}
				default:
					throw ex;
			}
			return shouldRetry;
		}

		protected abstract string BiServerName { get; }

		protected abstract string BiDatabaseName { get; }

		protected abstract void Run(CancellationToken iDoNotNeedToReactToThisToken);
		string IBiNotificationSource.Code { get; }
		string IBiNotificationSource.Description { get; }
	}
}
