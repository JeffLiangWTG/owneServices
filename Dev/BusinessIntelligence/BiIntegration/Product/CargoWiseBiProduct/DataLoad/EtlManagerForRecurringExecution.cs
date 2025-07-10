namespace CargoWise.Bi.Product.DataLoad
{
	using System;
	using CargoWise.Data;
	using Enterprise.Integration;

	#region SuppressResourceStringsCheckRegion

	class EtlManagerForRecurringExecution : EtlExecutionManager
	{
		#region Validation and Construction

		public static EtlManagerForRecurringExecution New(DbConnection mainDbConnection, DbConnection biConnection, ILogger logger)
		{
			CheckConnectionNotNull(mainDbConnection, "mainDbConnection");
			CheckConnectionNotNull(biConnection, "biConnection");
			// The admin connection check is removed as the EDW ETL execution needs admin account in order to truncate tables
			// Bi team will update the store procedure to be able to run without admin account and then will add the check back
			//CheckNotAnAdminConnection(biConnection);
			return new EtlManagerForRecurringExecution(mainDbConnection, biConnection, logger);
		}

		/// <summary>
		/// It cannot use an AdminConnection because we would lose the ability to prevent ETL from connecting by disabling the writer login.
		/// </summary>
		//static void CheckNotAnAdminConnection(DbConnection biConnection)
		//{
		//	if (biConnection != null && typeof(AdminConnection).IsAssignableFrom(biConnection.GetType()))
		//	{
		//		throw new EtlExecutionException("Cannot execute ETL on an admin connection.");
		//	}
		//}

		EtlManagerForRecurringExecution(DbConnection mainDbConnection, DbConnection biConnection, ILogger logger)
			: base(mainDbConnection, biConnection, logger)
		{
		}

		#endregion

		protected override void ExecuteEtl(Action<TsqlScriptRunner> etlAction, TsqlScriptRunner scriptRunner)
		{
			etlAction(scriptRunner);
		}

		protected override bool ExecuteAuditEtlProcess()
		{
			var scriptRunner = new AuditTsqlScriptRunner(biConnection, logger);
			ExecuteTsqlScripts(scriptRunner);

			var transactionsProcessed = scriptRunner.GetProcessedRowsTotal();
			return transactionsProcessed > 0;
		}

		protected override string MessageWhenBiDatabaseDoesNotExist
		{
			get { return "Please upgrade your system"; }
		}
	}

	#endregion
}
