namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Threading;
	using CargoWise.Common;

	class CsvImporter
	{
		public CsvImporter(IEtlLogger logger)
		{
			this.logger = logger;
			this.minImportDate = EtlController.GetFirstAnalysisDate();
		}

		readonly IEtlLogger logger;
		readonly DateTime minImportDate;
		DirectoryInfo importDir;
		string clientEnterpriseCode;
		string clientDbServerCode;

		static readonly Regex ClientCodeRegex = new Regex("^[a-zA-Z0-9]{6}$", RegexOptions.Compiled);
		static readonly Regex YearMonthStampRegex = new Regex(@"^STL_(?<year>20[1-9][1-9])(?<month>(0[1-9]|1[0-2]))\.txt$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public void ImportTransactionFacts(string importFolder, CancellationToken token)
		{
			try
			{
				SetImportDirectoryAndClientCodeFromPath(importFolder);
				ImportTransactionFactsUnsafe();
				token.ThrowIfCancellationRequested();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.ShowError(String.Format("Failed to import transaction facts => {0}", ex.Message), ex);
				throw;
			}
			finally
			{
				SetImportDirectoryAndClientSystemCode(null, null, null);
			}
		}

		void SetImportDirectoryAndClientCodeFromPath(string importFolder)
		{
			if (!Directory.Exists(importFolder))
			{
				throw new ArgumentException(String.Format("Folder [{0}] does not exist.", importFolder));
			}

			var dir = new DirectoryInfo(importFolder);
			var clientCodeMatch = ClientCodeRegex.Match(dir.Name);

			if (!clientCodeMatch.Success)
			{
				throw new ArgumentException(String.Format("Folder name not in the expected 6-char alphanumeric format [{0}].", dir.Name));
			}

			SetImportDirectoryAndClientSystemCode(dir, dir.Name.Substring(0, 3), dir.Name.Substring(3, 3));
		}

		void SetImportDirectoryAndClientSystemCode(DirectoryInfo importDir, string enterpriseCode, string dbServerCode)
		{
			this.importDir = importDir;
			clientEnterpriseCode = enterpriseCode;
			clientDbServerCode = dbServerCode;
		}

		void ImportTransactionFactsUnsafe()
		{
			using (var stlDwConnection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
			{
				ReloadImportingClientData(stlDwConnection);

				int? clientId;
				string clientOrgName;
				GetSystemLevelClientFromDimension(stlDwConnection, out clientId, out clientOrgName);

				if (clientId == null)
				{
					throw new Exception(String.Format(
						"No production client system found for Enterprise Code = [{0}] and DB Server Code = [{1}].",
						clientEnterpriseCode, clientDbServerCode));
				}
				else
				{
					logger.StartSubtask("CLient: " + clientOrgName);
					ImportTransactionFactFiles(stlDwConnection, clientId.Value);
				}
			}
		}

		#region re-load client info

		void ReloadImportingClientData(SqlConnection stlDwConnection)
		{
			logger.StartSubtask("Re-load client from ediProd");

			try
			{
				using (var stlDwTransaction = stlDwConnection.BeginTransaction())
				{
					LoadImportingClientInfoFromEdiProd(stlDwTransaction);
					stlDwTransaction.Commit();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error loading client data from ediProd. " + ex.Message);
			}
		}

		void LoadImportingClientInfoFromEdiProd(SqlTransaction stlDwTransaction)
		{
			using (var ediProdConnection = DbManager.NewEdiProdReadonlyConnection())
			{
				ediProdConnection.Open();

				var clientLoader = new HostedClientLoader();
				string loadClientSql = clientLoader.GetLoadClientsFromEdiProdScriptWithPredicate(clientEnterpriseCode, clientDbServerCode);
				clientLoader.LoadClientsIntoDimension(stlDwTransaction, ediProdConnection, loadClientSql);
			}
		}

		void GetSystemLevelClientFromDimension(SqlConnection stlDwConnection, out int? clientId, out string clientOrgName)
		{
			clientId = null;
			clientOrgName = null;

			string sqlText = String.Format(@"
				SELECT
					ClientId,
					OrganisationName
				FROM
					DimHostedClientCompany
				WHERE
					EnterpriseCode = '{0}'
					AND DatabaseServerCode = '{1}'
					AND CompanyCode = ''",
				clientEnterpriseCode,
				clientDbServerCode);

			using (var cmd = DbManager.NewSqlCommand(sqlText, stlDwConnection))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					clientId = Convert.ToInt32(reader[0]);
					clientOrgName = reader[1].ToString();
				}
			}
		}

		#endregion

		#region Import Facts

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void ImportTransactionFactFiles(SqlConnection stlDwConnection, int clientId)
		{
			DateTime yesterday = DateTime.UtcNow.Date.AddDays(-1);
			DateTime yesterdayMonthAsDate = new DateTime(yesterday.Year, yesterday.Month, 1);

			var importFiles = importDir.GetFiles("STL_??????.txt", SearchOption.TopDirectoryOnly);

			foreach (var file in importFiles)
			{
				var yearMonthMatch = YearMonthStampRegex.Match(file.Name);

				if (yearMonthMatch.Success)
				{
					int year = Convert.ToInt32(yearMonthMatch.Groups["year"].Value);
					int month = Convert.ToInt32(yearMonthMatch.Groups["month"].Value);
					DateTime monthAsDate = new DateTime(year, month, 1);

					if (monthAsDate >= minImportDate && monthAsDate < yesterdayMonthAsDate)
					{
						ImportFile(stlDwConnection, clientId, year, month, file);
					}
				}
			}
		}

		void ImportFile(SqlConnection stlDwConnection, int clientId, int year, int month, FileInfo file)
		{
			try
			{
				using (var stlDwTransaction = stlDwConnection.BeginTransaction())
				{
					ImportFileInTransaction(stlDwTransaction, clientId, year, month, file);
					stlDwTransaction.Commit();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.ShowError(
					String.Format(
						"Failed to import transaction facts for Year/Month: {0}/{1}\r\n\t=> {2}",
						year.ToString("D4"), month.ToString("D2"),
						ex.Message),
					ex);
			}
		}

		void ImportFileInTransaction(SqlTransaction stlDwTransaction, int clientId, int year, int month, FileInfo file)
		{
			if (HasDataAlreadyBeenLoadedForGivenClientAndMonth(stlDwTransaction, clientId, year, month))
			{
				logger.StartSubtask(
					String.Format(
						"Transactions already imported for Year/Month: {0}/{1}",
						year.ToString("D4"), month.ToString("D2")));
			}
			else
			{
				logger.StartSubtask(String.Format("Importing file [{0}]", file.Name));

				using (var reader = new StreamReader(file.FullName))
				{
					string line;

					while ((line = reader.ReadLine()) != null)
					{
						ImportFactLine(stlDwTransaction, clientId, year, month, line);
					}
				}
			}
		}

		bool HasDataAlreadyBeenLoadedForGivenClientAndMonth(SqlTransaction stlDwTransaction, int clientId, int year, int month)
		{
			string sqlText = String.Format(@"
				IF exists (
					SELECT null
					FROM
						FactTransaction WITH (updlock)
					WHERE
						ClientId = {0}
						AND year(TransactionDate) = {1}
						AND month(TransactionDate) = {2}
				) SELECT 1 ELSE SELECT 0",
				clientId.ToString(),
				year.ToString(),
				month.ToString());

			using (var cmd = DbManager.NewSqlCommand(sqlText, stlDwTransaction))
			{
				return Convert.ToBoolean(cmd.ExecuteScalar());
			}
		}

		void ImportFactLine(SqlTransaction stlDwTransaction, int clientId, int year, int month, string factLine)
		{
			string[] values = factLine.Split(',');

			if (values.Length != 4)
			{
				throw new Exception("Fact line does not contain 4 comma-separated values.\r\n" + factLine);
			}

			int featureId = Convert.ToInt32(values[0]);
			string companyCode = values[1].Trim();
			DateTime transactionDate = DateTime.ParseExact(values[2], "yyyy-MM-dd", CultureInfo.InvariantCulture);
			long transactionCount = Convert.ToInt64(values[3]);

			if (transactionDate.Year != year || transactionDate.Month != month)
			{
				throw new Exception("Transaction date out of the year/month range.\r\n" + factLine);
			}

			InsertFactTransaction(stlDwTransaction, clientId, featureId, companyCode, transactionDate, transactionCount);
		}

		void InsertFactTransaction(SqlTransaction stlDwTransaction, int clientId, int featureId, string companyCode, DateTime transactionDate, long transactionCount)
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
				insertCmd.Parameters.Add("@EnterpriseCode", SqlDbType.Char, 3).Value = clientEnterpriseCode;
				insertCmd.Parameters.Add("@DbServerCode", SqlDbType.Char, 3).Value = clientDbServerCode;
				insertCmd.Parameters.Add("@CompanyCode", SqlDbType.Char, 3).Value = companyCode;
				insertCmd.Parameters.Add("@FeatureId", SqlDbType.Int).Value = featureId;
				insertCmd.Parameters.Add("@TransactionDate", SqlDbType.Date).Value = transactionDate;
				insertCmd.Parameters.Add("@TransactionCount", SqlDbType.BigInt).Value = transactionCount;
				int rowsInserted = insertCmd.ExecuteNonQuery();

				if (rowsInserted == 0)
				{
					logger.ShowError(
						String.Format(
							"Company Code = [{0}] not licensed for client system: Enterprise Code = [{1}], DB Server Code = [{2}]",
							companyCode, clientEnterpriseCode,
							clientDbServerCode),
						null);
				}
			}
		}

		#endregion
	}
}
