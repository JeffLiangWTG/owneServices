namespace CargoWise.Bi.Product.DataLoad
{
	using System;
	using System.Globalization;
	using CargoWise.Bi.Common;
	using CargoWise.Data;
	using Enterprise.Integration;

	#region SuppressResourceStringsCheckRegion

	class EtlManagerForUpgrade : EtlExecutionManager
	{
		#region Validation and Construction

		public static EtlManagerForUpgrade New(DbConnection mainDbConnection, DbConnection biConnection, ILogger logger)
		{
			CheckConnectionNotNull(mainDbConnection, "mainDbConnection");
			CheckConnectionNotNull(biConnection, "biConnection");
			return new EtlManagerForUpgrade(mainDbConnection, biConnection, logger);
		}

		EtlManagerForUpgrade(DbConnection mainDbConnection, DbConnection biConnection, ILogger logger)
			: base(mainDbConnection, biConnection, logger)
		{
		}

		#endregion

		protected override void ExecuteEtl(Action<TsqlScriptRunner> etlAction, TsqlScriptRunner scriptRunner)
		{
			try
			{
				etlAction(scriptRunner);
			}
			catch (SqlException ex)
				when (IsStoredProcedureRelatedException(ex))
			{
				logger.Log(LogType.Warning, "Stored procedures for ETL execution are not yet deployed.");
			}

			catch (EtlExecutionException ex) when (ex.ExceptionType == DbErrorType.InvalidObjectName)
			{
				logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Skipping {0} execution. CDC is not enabled at table level.", scriptRunner.ScriptName));
			}
		}

		bool IsStoredProcedureRelatedException(SqlException ex)
		{
			var exceptionType = new DbErrorMatch(ex).ExceptionType;
			return
				exceptionType == DbErrorType.IncorrectNumberOfParametersForProcedure
				|| exceptionType == DbErrorType.IncorrectParameterForProcedure
				|| exceptionType == DbErrorType.CouldNotFindStoredProcedure;
		}

		protected override bool ExecuteAuditEtlProcess()
		{
			var scriptRunner = new AuditTsqlScriptRunner(biConnection, logger);
			ExecuteTsqlScripts(scriptRunner);
			return false;
		}

		protected override string MessageWhenBiDatabaseDoesNotExist
		{
			get { return "Skipping ETL execution"; }
		}
	}

	#endregion
}
