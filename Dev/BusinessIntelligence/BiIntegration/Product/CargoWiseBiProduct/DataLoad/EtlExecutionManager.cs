namespace CargoWise.Bi.Product.DataLoad
{
	using System;
	using System.Globalization;
	using System.Text.RegularExpressions;
	using CargoWise.Bi.Common;
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Resource.Version;
	using Enterprise.Integration;

	#region SuppressResourceStringsCheckRegion

	#region Interface and Factory

	public interface IEtlAuditExecution
	{
		bool ExecuteAuditEtlProcess();
	}

	public interface IEtlEdwExecution
	{
		bool ExecuteEdwEtlProcess();
	}

	public static class EtlExecutionManagerFactory
	{
		public static IEtlAuditExecution NewForUpgrade(DbConnection mainDbConnection, DbConnection biConnection, ILogger logger)
		{
			return EtlManagerForUpgrade.New(mainDbConnection, biConnection, logger);
		}

		public static IEtlAuditExecution NewForAuditRecurringExecution(DbConnection mainDbConnection, DbConnection biConnection, ILogger logger)
		{
			return EtlManagerForRecurringExecution.New(mainDbConnection, biConnection, logger);
		}

		public static IEtlEdwExecution NewForEdwRecurringExecution(DbConnection mainDbConnection, DbConnection biConnection, ILogger logger)
		{
			return EtlManagerForRecurringExecution.New(mainDbConnection, biConnection, logger);
		}
	}

	#endregion

	abstract class EtlExecutionManager : IEtlAuditExecution, IEtlEdwExecution
	{
		#region Validation and Construction

		protected static void CheckConnectionNotNull(DbConnection connection, string connectionArgumentName)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(connectionArgumentName);
			}
		}

		protected EtlExecutionManager(DbConnection mainDbConnection, DbConnection biConnection, ILogger logger)
		{
			this.mainDbConnection = mainDbConnection;
			this.biConnection = biConnection;
			this.logger = logger;
		}

		#endregion

		readonly DbConnection mainDbConnection;
		protected readonly DbConnection biConnection;
		readonly protected ILogger logger;

		bool IEtlAuditExecution.ExecuteAuditEtlProcess()
		{
			return ExecuteAuditEtlProcess();
		}

		bool IEtlEdwExecution.ExecuteEdwEtlProcess()
		{
			var transactionsProcessed = false;

			var scriptRunner = new EdwTsqlScriptRunner(biConnection, logger);

			var initialLoadRequested = scriptRunner.IsInitialLoad();
			var initialLoadEndDate = scriptRunner.GetInitialLoadEndDateTime();

			ExecuteTsqlScripts(scriptRunner);

			if (!scriptRunner.IsInitialLoad())
			{
				var newInitialLoadEndDate = scriptRunner.GetInitialLoadEndDateTime();
				var incrementalLoadCount = scriptRunner.GetIncrementalLoadRecordCount();

				transactionsProcessed =
					initialLoadRequested ||
					newInitialLoadEndDate > initialLoadEndDate ||
					incrementalLoadCount > 0;
			}

			return transactionsProcessed;
		}

		#region Implementation

		protected abstract void ExecuteEtl(Action<TsqlScriptRunner> etlAction, TsqlScriptRunner scriptRunner);
		protected abstract bool ExecuteAuditEtlProcess();
		protected abstract string MessageWhenBiDatabaseDoesNotExist { get; }

		protected void ExecuteTsqlScripts(TsqlScriptRunner scriptRunner)
		{
			if (biConnection.DatabaseExists(scriptRunner.BiDatabaseName))
			{
				try
				{
					if (ShouldRunEtl(scriptRunner))
					{
						ExecuteEtl(ExecuteScripts, scriptRunner);
					}
				}
				catch (BiServiceLockException ex)
				{
					logger.Log(LogType.Debug, ex.Message);
				}
			}
			else
			{
				LogBiDatabaseDoesNotExistWarning(scriptRunner.BiDatabaseName);
			}
		}

		protected virtual bool ShouldRunEtl(TsqlScriptRunner scriptRunner)
		{
			return scriptRunner.ShouldRunEtl();
		}

		void ExecuteScripts(TsqlScriptRunner scriptRunner)
		{
			if (IsSchemaCompatibleWithMainDb(scriptRunner.BiDatabaseName))
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "*** Executing {0} ***", scriptRunner.ScriptName));
				ExecuteScriptsCore(scriptRunner);
			}
			else
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Skipping {0} execution. [{1}] is incompatible with main database.", scriptRunner.ScriptName, scriptRunner.BiDatabaseName));
			}
		}

		void ExecuteScriptsCore(TsqlScriptRunner scriptRunner)
		{
			try
			{
				scriptRunner.Run();
			}
			catch (SqlException ex)
			{
				var friendlyError = GetFriendlyException(ex);

				if (friendlyError == null)
				{
					throw;
				}
				else
				{
					throw friendlyError;
				}
			}
		}

		EtlExecutionException GetFriendlyException(SqlException ex)
		{
			var errorType = new DbErrorMatch(ex).ExceptionType;

			switch (errorType)
			{
				case DbErrorType.OleDbProviderNotRegistered:
					return GetMissingOleDbProviderFriendlyException(ex, @"The OLE DB provider ""(?<PROVIDER>\w+)"" has not been registered");
				case DbErrorType.CannotCreateOleDbProviderForLinkedServer:
					return GetMissingOleDbProviderFriendlyException(ex, @"Cannot create an instance of OLE DB provider ""(?<Provider>\w+)"" for linked server "".+?""");
				case DbErrorType.CouldNotExecuteOnServerNotConfiguredForRemoteAccess:
					return new EtlExecutionException(ex.Message + "\r\nThe configuration will only take effect after restarting SQL Server.", ex);
				default:
					return null;
			}
		}

		EtlExecutionException GetMissingOleDbProviderFriendlyException(SqlException ex, string regexPattern)
		{
			EtlExecutionException result = null;

			var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
			var match = regex.Match(ex.Message);

			if (match.Success)
			{
				var provider = match.Groups["PROVIDER"].Value;
				result = new EtlExecutionException(
					string.Format(CultureInfo.InvariantCulture,
						"{0} provider is not installed in {1}. Please install it, restart the server, and retry.\r\n{2}",
						provider,
						biConnection.ServerName,
						ex.Message), ex);
			}

			return result;
		}

		void LogBiDatabaseDoesNotExistWarning(string biDbName)
		{
			string warningMsg = string.Format(CultureInfo.InvariantCulture,
				"[{0}] does not exist. {1}.",
				biDbName,
				MessageWhenBiDatabaseDoesNotExist
			);
			logger.Log(LogType.Warning, warningMsg);
		}

		bool IsSchemaCompatibleWithMainDb(string biDatabaseName)
		{
			var result = false;
			var biExtPtyValue = BiMasterState.GetBiDatabaseExtPty(biConnection, biDatabaseName, BiConstants.MainDbSchemaVersionExtPtyName);
			result = biExtPtyValue == GetMainDbSchemaVersion();

			return result;
		}

		string GetMainDbSchemaVersion()
		{
			return new VersionLabel(
				DbRegistry.DatabaseMajorSchemaVersion.LoadValue(mainDbConnection),
				DbRegistry.DatabaseMinorSchemaVersion.LoadValue(mainDbConnection)).ToString();
		}

		#endregion
	}

	#endregion
}
