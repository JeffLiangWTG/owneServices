namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Text.RegularExpressions;

	class HostedClientLoader
	{
		public HostedClient LoadClientFromSpecificDatabase(string entepriseCode, string serverCode, string serverName, string databaseName)
		{
			string loadClientSql = GetLoadClientsFromEdiProdScriptWithPredicate(entepriseCode, serverCode);

			return RunClientDimEtl(
				(dwTran, srcConn) => LoadClientsIntoDimension(dwTran, srcConn, loadClientSql),
				(dwConn) => GetHostedClient(dwConn, entepriseCode, serverCode, serverName, databaseName)
			);
		}

		HostedClient RunClientDimEtl(Action<SqlTransaction, SqlConnection> loadClientDimAction, Func<SqlConnection, HostedClient> afterTransactionFunction)
		{
			using (var ediProdConnection = DbManager.NewEdiProdReadonlyConnection())
			{
				ediProdConnection.Open();

				using (var stlDwConnection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
				{
					RunInTransaction(
						stlDwConnection,
						(t) => loadClientDimAction(t, ediProdConnection),
						errorMsgPrefix: "Error loading client licence data.");

					return afterTransactionFunction(stlDwConnection);
				}
			}
		}

		void RunInTransaction(SqlConnection stlDwConnection, Action<SqlTransaction> transactionalAction, string errorMsgPrefix)
		{
			try
			{
				using (var stlDwTransaction = stlDwConnection.BeginTransaction())
				{
					transactionalAction(stlDwTransaction);
					stlDwTransaction.Commit();
				}
			}
			catch (Exception ex)
			{
				throw new Exception(errorMsgPrefix + "\r\n" + ex.Message);
			}
		}

		public void LoadClientsIntoDimension(SqlTransaction stlDwTransaction, SqlConnection ediProdConnection, string loadClientSql)
		{
			using (var loadCmd = DbManager.NewSqlCommand(loadClientSql, ediProdConnection))
			{
				using (var reader = loadCmd.ExecuteReader())
				{
					if (!reader.HasRows)
					{
						throw new Exception("No matching production client licences found in ediProd.");
					}

					while (reader.Read())
					{
						string enterpriseCode = reader[0].ToString().ToUpperInvariant();
						string dbServerCode = reader[1].ToString().ToUpperInvariant();
						string companyCode = reader[2].ToString().ToUpperInvariant();

						string orgCode = reader[3].ToString().ToUpperInvariant();
						string orgName = reader[4].ToString().ToUpperInvariant();
						string companyOrgCode = reader[5].ToString().ToUpperInvariant();
						string companyOrgName = reader[6].ToString().ToUpperInvariant();
						string companyCurrency = reader[7].ToString().ToUpperInvariant();
						string portCode = reader[8].ToString().ToUpperInvariant();
						string portName = reader[9].ToString().ToUpperInvariant();
						string countryCode = reader[10].ToString().ToUpperInvariant();
						string countryName = reader[11].ToString().ToUpperInvariant();
						string hostedLocation = reader[12].ToString().ToUpperInvariant();
						string serverName = reader[13].ToString().ToUpperInvariant();
						string dbName = reader[14].ToString().ToUpperInvariant();

						InsertOrUpdateDimHostedClientCompany(
							stlDwTransaction,
							enterpriseCode, dbServerCode, companyCode,
							orgCode, orgName, companyOrgCode, companyOrgName, companyCurrency,
							portCode, portName, countryCode, countryName,
							hostedLocation, serverName, dbName);
					}
				}
			}
		}

		HostedClient GetHostedClient(SqlConnection stlDwConnection, string entepriseCode, string serverCode, string serverName, string databaseName)
		{
			string sqlText = String.Format(CultureInfo.InvariantCulture, @"
				SELECT TOP 1
					dhcc.ClientId,
					dhcc.CompanyCode,
					dhcc.OrganisationName
				FROM
					dbo.DimHostedClientCompany dhcc
				WHERE
					dhcc.IsCurrent = 1
					AND dhcc.CompanyCode = ''
					AND dhcc.EnterpriseCode = '{0}'
					AND dhcc.DatabaseServerCode = '{1}'",
				entepriseCode, serverCode);

			using (var loadCmd = DbManager.NewSqlCommand(sqlText, stlDwConnection))
			{
				using (var reader = loadCmd.ExecuteReader())
				{
					if (reader.Read())
					{
						int id = Convert.ToInt32(reader[0].ToString());
						string companyCode = reader[1].ToString().ToUpper();
						string name = reader[2].ToString().ToUpper();

						return new HostedClient(id, entepriseCode, serverCode, companyCode, name, serverName, databaseName);
					}
					else
					{
						return null;
					}
				}
			}
		}

		void InsertOrUpdateDimHostedClientCompany(
			SqlTransaction stlDwTransaction,
			string enterpriseCode, string dbServerCode, string companyCode,
			string orgCode, string orgName, string companyOrgCode, string companyOrgName, string companyCurrency,
			string portCode, string portName, string countryCode, string countryName,
			string hostedLocation, string serverName, string dbName)
		{
			string sqlText = EtlController.GetLoadClientScript("MergeDimHostedClientCompany.sql");

			using (var mergeCmd = DbManager.NewSqlCommand(sqlText, stlDwTransaction))
			{
				mergeCmd.Parameters.Add("@EnterpriseCode", SqlDbType.Char, 3).Value = enterpriseCode;
				mergeCmd.Parameters.Add("@DatabaseServerCode", SqlDbType.Char, 3).Value = dbServerCode;
				mergeCmd.Parameters.Add("@CompanyCode", SqlDbType.Char, 3).Value = companyCode;
				mergeCmd.Parameters.Add("@OrganisationCode", SqlDbType.NVarChar).Value = orgCode;
				mergeCmd.Parameters.Add("@OrganisationName", SqlDbType.NVarChar).Value = orgName;
				mergeCmd.Parameters.Add("@CompanyOrgCode", SqlDbType.NVarChar).Value = companyOrgCode;
				mergeCmd.Parameters.Add("@CompanyOrgName", SqlDbType.NVarChar).Value = companyOrgName;
				mergeCmd.Parameters.Add("@CompanyCurrency", SqlDbType.VarChar).Value = companyCurrency;
				mergeCmd.Parameters.Add("@HomePort", SqlDbType.Char, 5).Value = portCode;
				mergeCmd.Parameters.Add("@PortName", SqlDbType.VarChar).Value = portName;
				mergeCmd.Parameters.Add("@HomeCountry", SqlDbType.Char, 2).Value = countryCode;
				mergeCmd.Parameters.Add("@CountryName", SqlDbType.VarChar).Value = countryName;
				mergeCmd.Parameters.Add("@HostedLocation", SqlDbType.Char, 3).Value = hostedLocation;
				mergeCmd.Parameters.Add("@ServerName", SqlDbType.VarChar).Value = serverName;
				mergeCmd.Parameters.Add("@DatabaseName", SqlDbType.VarChar).Value = dbName;
				mergeCmd.ExecuteNonQuery();
			}
		}

		public string GetLoadClientsFromEdiProdScriptWithPredicate(string entepriseCode = null, string serverCode = null)
		{
			string result = EtlController.GetLoadClientScript("LoadHostedClientsFromEdiProd.sql");

			if (!string.IsNullOrWhiteSpace(entepriseCode) && !string.IsNullOrWhiteSpace(serverCode))
			{
				var filter = String.Format(CultureInfo.InvariantCulture, "AND (le.LE_EnterpriseCode = '{0}' AND ld.LD_ServerCode = '{1}')", entepriseCode, serverCode);
				result = ClientPredicateRegex.Replace(result, filter);
			}

			return result;
		}

		static readonly Regex ClientPredicateRegex = new Regex(@"--\s*\[=CLIENT_PREDICATE=START=].+--\s*\[=CLIENT_PREDICATE=END=]", RegexOptions.Compiled | RegexOptions.Singleline);
	}
}
