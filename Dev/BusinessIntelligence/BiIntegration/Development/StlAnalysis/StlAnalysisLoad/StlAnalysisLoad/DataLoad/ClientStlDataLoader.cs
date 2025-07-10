namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using CargoWise.Common;

	class ClientStlDataLoader
	{
		public ClientStlDataLoader(IEtlLogger etlLogger, IEnumerable<LicensedFeature> billableFeatures, DateTime loadEndDateExclusive)
		{
			this.etlLogger = etlLogger;
			this.billableFeatures = billableFeatures;
			this.lastLoadInfo = GetLastLoadInfo();
			this.loadMinDate = EtlController.GetFirstAnalysisDate();
			this.loadEndDate = loadEndDateExclusive;
		}

		readonly DateTime loadMinDate;
		readonly DateTime loadEndDate;
		readonly Dictionary<Tuple<int, int>, DateTime> lastLoadInfo;
		readonly IEnumerable<LicensedFeature> billableFeatures;
		readonly IEtlLogger etlLogger;
		const int ActiveUserMockFeatureId = -999;

		public void LoadStlDataForClientSafe(HostedClient client)
		{
			try
			{
				LoadStlDataForClientBySourceLocation(client);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				etlLogger.ShowError(String.Format("Failed to run ETL scripts for client {0} => {1}", client.ToString(), ex.Message), ex);
			}
		}

		void LoadStlDataForClientBySourceLocation(HostedClient client)
		{
			LoadActiveUserCountForClient(client);

			foreach (var location in FeatureDataSource.GetAllLocations())
			{
				LoadStlDataForClient(client, location);
			}
		}

		void LoadStlDataForClient(HostedClient client, SourceLocation location)
		{
			var specificLocationFeatures = billableFeatures.Where(f => f.DataSource.Location == location);

			if (specificLocationFeatures.Any())
			{
				var connectionInfo = FeatureDataSource.GetConnectionInfo(location, client);

				using (var etlConnection = DbManager.NewConnectionFromServerInfo(connectionInfo, pooling: (location != SourceLocation.CLIENT)))
				{
					LoadClientDataFromSourceLocation(etlConnection, client, specificLocationFeatures);
				}
			}
		}

		Dictionary<Tuple<int, int>, DateTime> GetLastLoadInfo()
		{
			var lastLoadDictionary = new Dictionary<Tuple<int, int>, DateTime>();

			using (var stlDwConnection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
			{
				const string transactionLastLoadSql = "SELECT ClientId, FeatureId, TransactionDate FROM dbo.ControlLastLoad";

				using (var cmd = DbManager.NewSqlCommand(transactionLastLoadSql, stlDwConnection))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						int clientId = Convert.ToInt32(reader[0]);
						int featureId = Convert.ToInt32(reader[1]);
						DateTime loadDate = Convert.ToDateTime(reader[2]);
						lastLoadDictionary.Add(new Tuple<int, int>(clientId, featureId), loadDate);
					}
				}

				const string activeUserLastLoadSql = "SELECT ClientId, max(MonthStartDate) FROM dbo.FactActiveUsers GROUP BY ClientId";

				using (var cmd = DbManager.NewSqlCommand(activeUserLastLoadSql, stlDwConnection))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						int clientId = Convert.ToInt32(reader[0]);
						DateTime loadDate = Convert.ToDateTime(reader[1]);
						lastLoadDictionary.Add(new Tuple<int, int>(clientId, ActiveUserMockFeatureId), loadDate);
					}
				}
			}

			return lastLoadDictionary;
		}

		DateTime GetLastLoadDateForClientAndFeature(int clientId, int featureId)
		{
			DateTime result = loadMinDate.AddDays(-1);
			var clientAndFeatureKey = new Tuple<int, int>(clientId, featureId);

			if (lastLoadInfo.ContainsKey(clientAndFeatureKey))
			{
				result = lastLoadInfo[clientAndFeatureKey];
			}

			return result;
		}

		#region Load FactTransaction

		void LoadClientDataFromSourceLocation(SqlConnection etlConnection, HostedClient client, IEnumerable<LicensedFeature> specificLocationFeatures)
		{
			foreach (var feature in specificLocationFeatures)
			{
				DateTime lastClientFeatureLoadDate = GetLastLoadDateForClientAndFeature(client.ClientId, feature.FeatureId);
				DateTime startDate = new DateTime(lastClientFeatureLoadDate.Year, lastClientFeatureLoadDate.Month, 1).AddMonths(1);

				if (startDate < loadEndDate)
				{
					etlLogger.StartSubtask(String.Format(
						"Client: {0}\r\n\tMod/Feat.: {1}/{2}\r\n\tStart Year/Month: {3}/{4}",
						client.ToString(),
						feature.Module, feature.Feature,
						startDate.Year.ToString("D4"), startDate.Month.ToString("D2")));

					if (etlConnection.State != ConnectionState.Open)
					{
						etlConnection.Open();
					}

					for (DateTime firstDayOfMonth = startDate; firstDayOfMonth < loadEndDate; firstDayOfMonth = firstDayOfMonth.AddMonths(1))
					{
						bool successful = LoadTransactionDataIntoFactTableSafe(etlConnection, client, feature, firstDayOfMonth);
						// If fails, stops loading data for this client.
						if (!successful)
						{
							break;
						}
					}
				}
			}
		}

		bool LoadTransactionDataIntoFactTableSafe(SqlConnection etlConnection, HostedClient client, LicensedFeature feature, DateTime firstDayOfMonth)
		{
			using (var stlDwConnection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
			{
				try
				{
					LoadTransactionDataIntoFactTableWithAlternativeQueryRetry(stlDwConnection, etlConnection, client, feature, firstDayOfMonth, feature.BillingQuery);
					return true;
				}
				catch (SqlException ex)
				{
					etlLogger.ShowError(
						String.Format(
							"Failed to insert transaction facts for:\r\n\tClient: {0}\r\n\tFeature: {1}\r\n\tYear/Month: {2}{3}\r\n\t=> {4}",
							client.ToString(), feature.Name,
							firstDayOfMonth.Year.ToString("D4"), firstDayOfMonth.Month.ToString("D2"),
							ex.Message),
						ex);
					return false;
				}
			}
		}

		void LoadTransactionDataIntoFactTableWithAlternativeQueryRetry(SqlConnection stlDwConnection, SqlConnection etlConnection, HostedClient client, LicensedFeature feature, DateTime firstDayOfMonth, string stlScriptBatch)
		{
			Exception firstHandledError = null;
			string[] alternativeScripts = stlScriptBatch.Split(AltScriptSeparator, StringSplitOptions.RemoveEmptyEntries);

			foreach (string runnableQuery in alternativeScripts)
			{
				try
				{
					LoadTransactionDataIntoFactTableWithTransactionControl(stlDwConnection, etlConnection, client, feature, firstDayOfMonth, runnableQuery.Trim());
					return;
				}
				catch (SqlException ex)
				{
					if (
						ex.Message.StartsWith("Invalid column name", StringComparison.OrdinalIgnoreCase)
						|| ex.Message.StartsWith("Invalid object name 'dbo.", StringComparison.OrdinalIgnoreCase)
						|| ex.Message.StartsWith("Cannot find either column \"dbo\" or the user-defined function or aggregate", StringComparison.OrdinalIgnoreCase)
					)
					{
						if (firstHandledError == null)
						{
							firstHandledError = ex;
						}
					}
					else
					{
						throw;
					}
				}
			}

			throw firstHandledError ?? new Exception("No query to run");
		}

		void LoadTransactionDataIntoFactTableWithTransactionControl(SqlConnection stlDwConnection, SqlConnection etlConnection, HostedClient client, LicensedFeature feature, DateTime firstDayOfMonth, string stlQuery)
		{
			using (var stlDwTransaction = stlDwConnection.BeginTransaction())
			{
				LoadTransactionDataIntoFactTable(stlDwTransaction, etlConnection, client, feature, firstDayOfMonth, stlQuery.Trim());
				stlDwTransaction.Commit();
			}
		}

		static readonly string[] AltScriptSeparator = new string[] { "[=ALTERNATIVE_SCRIPT=]" };

		void LoadTransactionDataIntoFactTable(SqlTransaction stlDwTransaction, SqlConnection etlConnection, HostedClient client, LicensedFeature feature, DateTime firstDayOfMonth, string stlQuery)
		{
			DateTime firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);
			stlQuery = stlQuery.Trim().TrimEnd(';') + "\r\nOPTION (RECOMPILE)";

			using (var loadCmd = DbManager.NewSqlCommand(stlQuery, etlConnection, timeoutInSeconds: 10 * 60))
			{
				loadCmd.Parameters.Add("@StartDateInclusive", SqlDbType.Date).Value = firstDayOfMonth;
				loadCmd.Parameters.Add("@EndDateExclusive", SqlDbType.Date).Value = firstDayOfNextMonth;

				if (feature.DataSource.IsMultitenanted)
				{
					loadCmd.Parameters.Add("@EnterpriseCode", SqlDbType.VarChar).Value = client.EnterpriseCode;
					loadCmd.Parameters.Add("@DbServerCode", SqlDbType.VarChar).Value = client.DbServerCode;
				}

				using (var reader = loadCmd.ExecuteReader())
				{
					while (reader.Read())
					{
						string companyCode = reader[0].ToString();
						DateTime transactionDate = Convert.ToDateTime(reader[1]);
						long transactionCount = Convert.ToInt64(reader[2]);
						InsertFactTransaction(stlDwTransaction, client, feature, companyCode, transactionDate, transactionCount);
					}
				}
			}

			InsertOrUpdateControlLastLoad(stlDwTransaction, client, feature, firstDayOfNextMonth.AddDays(-1));
		}

		void InsertFactTransaction(SqlTransaction stlDwTransaction, HostedClient client, LicensedFeature feature, string companyCode, DateTime transactionDate, long transactionCount)
		{
			string sqlText = @"
				INSERT dbo.FactTransaction (ClientId, FeatureId, TransactionDate, TransactionCount)
					SELECT dhcc.ClientId, @FeatureId, @TransactionDate, @TransactionCount
					FROM dbo.DimHostedClientCompany dhcc
					WHERE dhcc.EnterpriseCode = @EnterpriseCode
					AND dhcc.DatabaseServerCode = @DbServerCode
					AND dhcc.CompanyCode = @CompanyCode;";

			using (var insertCmd = DbManager.NewSqlCommand(sqlText, stlDwTransaction))
			{
				insertCmd.Parameters.Add("@EnterpriseCode", SqlDbType.Char, 3).Value = client.EnterpriseCode;
				insertCmd.Parameters.Add("@DbServerCode", SqlDbType.Char, 3).Value = client.DbServerCode;
				insertCmd.Parameters.Add("@CompanyCode", SqlDbType.Char, 3).Value = companyCode;
				insertCmd.Parameters.Add("@FeatureId", SqlDbType.Int).Value = feature.FeatureId;
				insertCmd.Parameters.Add("@TransactionDate", SqlDbType.Date).Value = transactionDate;
				insertCmd.Parameters.Add("@TransactionCount", SqlDbType.BigInt).Value = transactionCount;
				insertCmd.ExecuteNonQuery();
			}
		}

		void InsertOrUpdateControlLastLoad(SqlTransaction stlDwTransaction, HostedClient client, LicensedFeature feature, DateTime transactionDate)
		{
			string sqlText = @"
					MERGE dbo.ControlLastLoad AS tgt
						USING (SELECT @ClientId, @FeatureId) AS src (ClientId, FeatureId)
						ON (tgt.ClientId = src.ClientId AND tgt.FeatureId = src.FeatureId)
					WHEN MATCHED THEN
						UPDATE SET TransactionDate = @TransactionDate
					WHEN NOT MATCHED THEN
						INSERT (ClientId, FeatureId, TransactionDate)
						VALUES (src.ClientId, src.FeatureId, @TransactionDate);";

			using (var mergeCmd = DbManager.NewSqlCommand(sqlText, stlDwTransaction))
			{
				mergeCmd.Parameters.Add("@ClientId", SqlDbType.Int).Value = client.ClientId;
				mergeCmd.Parameters.Add("@FeatureId", SqlDbType.Int).Value = feature.FeatureId;
				mergeCmd.Parameters.Add("@TransactionDate", SqlDbType.Date).Value = transactionDate;
				mergeCmd.ExecuteNonQuery();
			}
		}

		#endregion // Load FactTransaction

		#region Load FactActiveUsers

		void LoadActiveUserCountForClient(HostedClient client)
		{
			using (var etlConnection = DbManager.NewConnectionFromServerInfo(client.DatabaseInfo, pooling: false))
			{
				LoadMonthlyActiveUserCount(etlConnection, client);
			}
		}

		void LoadMonthlyActiveUserCount(SqlConnection clientConnection, HostedClient client)
		{
			DateTime lastClientFeatureLoadDate = GetLastLoadDateForClientAndFeature(client.ClientId, ActiveUserMockFeatureId);
			DateTime startDate = new DateTime(lastClientFeatureLoadDate.Year, lastClientFeatureLoadDate.Month, 1).AddMonths(1);

			if (startDate < loadEndDate)
			{
				etlLogger.StartSubtask(String.Format(
					"Client: {0}\r\n\tActive user count\r\n\tStart Year/Month: {1}/{2}",
					client.ToString(),
					startDate.Year.ToString("D4"), startDate.Month.ToString("D2")));

				if (clientConnection.State != ConnectionState.Open)
				{
					clientConnection.Open();
				}

				for (DateTime firstDayOfMonth = startDate; firstDayOfMonth < loadEndDate; firstDayOfMonth = firstDayOfMonth.AddMonths(1))
				{
					bool successful = LoadActiveUserCountIntoFactTableSafe(clientConnection, client, firstDayOfMonth);
					// If fails, skips to next client.
					if (!successful)
					{
						break;
					}
				}
			}
		}

		bool LoadActiveUserCountIntoFactTableSafe(SqlConnection clientConnection, HostedClient client, DateTime firstDayOfMonth)
		{
			using (var stlDwConnection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
			{
				try
				{
					using (var stlDwTransaction = stlDwConnection.BeginTransaction())
					{
						LoadActiveUserCountIntoFactTable(stlDwTransaction, clientConnection, client, firstDayOfMonth);
						stlDwTransaction.Commit();

						return true;
					}
				}
				catch (SqlException ex)
				{
					etlLogger.ShowError(
						String.Format(
							"Failed to insert active user count for Client {0}, Year/Month {1}{2} => {3}",
							client.ToString(),
							firstDayOfMonth.Year.ToString("D4"), firstDayOfMonth.Month.ToString("D2"),
							ex.Message),
						ex);

					return false;
				}
			}
		}

		void LoadActiveUserCountIntoFactTable(SqlTransaction stlDwTransaction, SqlConnection clientConnection, HostedClient client, DateTime firstDayOfMonth)
		{
			string sqlText = @"
				SELECT count(*)
				FROM
					dbo.GlbStaff gs
					INNER JOIN (
						SELECT distinct SL_GS_NKUser
						FROM dbo.StmALog
						WHERE SL_PostedTimeUtc >= @StartDateInclusive
						AND SL_PostedTimeUtc < @EndDateExclusive
					) sl ON gs.GS_Code = sl.SL_GS_NKUser
				WHERE
					cast(gs.GS_IsResource as char(1)) in ('0', 'N')
					AND cast(gs.GS_IsSystemAccount as char(1)) in ('0', 'N')";

			using (var loadCmd = DbManager.NewSqlCommand(sqlText, clientConnection, timeoutInSeconds: 5 * 60))
			{
				loadCmd.Parameters.Add("@StartDateInclusive", SqlDbType.Date).Value = firstDayOfMonth;
				loadCmd.Parameters.Add("@EndDateExclusive", SqlDbType.Date).Value = firstDayOfMonth.AddMonths(1);
				int userCount = Convert.ToInt32(loadCmd.ExecuteScalar());
				InsertFactActiveUsers(stlDwTransaction, client, firstDayOfMonth, userCount);
			}
		}

		void InsertFactActiveUsers(SqlTransaction stlDwTransaction, HostedClient client, DateTime firstDayOfMonth, int userCount)
		{
			string sqlText = @"
				INSERT dbo.FactActiveUsers (ClientId, MonthStartDate, UserCount)
					VALUES (@ClientId, @MonthStartDate, @UserCount);";

			using (var insertCmd = DbManager.NewSqlCommand(sqlText, stlDwTransaction))
			{
				insertCmd.Parameters.Add("@ClientId", SqlDbType.Int).Value = client.ClientId;
				insertCmd.Parameters.Add("@MonthStartDate", SqlDbType.Date).Value = firstDayOfMonth;
				insertCmd.Parameters.Add("@UserCount", SqlDbType.Int).Value = userCount;
				insertCmd.ExecuteNonQuery();
			}
		}

		#endregion // Load FactActiveUsers
	}
}
