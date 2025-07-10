namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Data;
	using CargoWise.Common;

	class LicensedFeature
	{
		/// <summary>
		/// Client_StlModuleBilling
		/// </summary>
		public LicensedFeature(
			int id, string code,
			string role, string module, string function, string feature,
			bool isSystemLevel, bool isCompanyLevel,
			string stlBasis, string stlEntity, decimal stlUnits,
			string summaryQuery, string detailSqlExpression, string detailCountExpression,
			string stlRules, string dataSource)
		{
			this.featureId = id;
			this.featureCode = code;
			this.roleName = role;
			this.moduleName = module;
			this.functionName = function;
			this.featureName = feature;
			this.isSystemLevel = isSystemLevel;
			this.isCompanyLevel = isCompanyLevel;
			this.billingBasis = stlBasis;
			this.billingEntity = stlEntity;
			this.billingUnits = stlUnits;
			this.summaryQuery = summaryQuery;
			this.detailSqlExpression = detailSqlExpression;
			this.detailCountExpression = detailCountExpression;
			this.billingRules = stlRules;
			this.dataSource = new FeatureDataSource(dataSource);
		}

		#region Properties

		/// <summary>
		/// FeatureId int identity(1,1) NOT NULL
		/// </summary>
		public int FeatureId { get { return featureId; } }
		readonly int featureId;

		/// <summary>
		/// FeatureCode char(3) NOT NULL
		/// </summary>
		public string FeatureCode { get { return featureCode; } }
		readonly string featureCode;

		/// <summary>
		/// RoleName varchar(50) NOT NULL
		/// </summary>
		public string Role { get { return roleName; } }
		readonly string roleName;

		/// <summary>
		/// ModuleName varchar(50) NOT NULL
		/// </summary>
		public string Module { get { return moduleName; } }
		readonly string moduleName;

		/// <summary>
		/// FunctionName varchar(50) NOT NULL
		/// </summary>
		public string Function { get { return functionName; } }
		readonly string functionName;

		/// <summary>
		/// FeatureName varchar(75) NOT NULL
		/// </summary>
		public string Feature { get { return featureName; } }
		readonly string featureName;

		/// <summary>
		/// IsSystemLevel bit NOT NULL
		/// </summary>
		public bool IsSystemLevel { get { return isSystemLevel; } }
		bool isSystemLevel;

		/// <summary>
		/// IsCompanyLevel bit NOT NULL
		/// </summary>
		public bool IsCompanyLevel { get { return isCompanyLevel; } }
		bool isCompanyLevel;

		/// <summary>
		/// StlBasis varchar(35) NOT NULL
		/// </summary>
		public string BillingBasis { get { return billingBasis; } }
		readonly string billingBasis;

		/// <summary>
		/// StlEntityCounted varchar(35) NOT NULL
		/// </summary>
		public string BillingEntity { get { return billingEntity; } }
		readonly string billingEntity;

		/// <summary>
		/// StlUnitWeight decimal(9, 3) NOT NULL
		/// </summary>
		public decimal BillingUnits { get { return billingUnits; } }
		readonly decimal billingUnits;

		/// <summary>
		/// StlRules xml NOT NULL
		/// </summary>
		public string BillingRules
		{
			get { return billingRules; }
		}
		string billingRules;

		/// <summary>
		/// StlQuery varchar(4000) NOT NULL
		/// </summary>
		public string BillingQuery
		{
			get { return summaryQuery; }
		}
		string summaryQuery;

		/// <summary>
		/// StlDetailSqlExpression varchar(500) NOT NULL
		/// </summary>
		public string DetailSqlExpression
		{
			get { return detailSqlExpression; }
		}
		string detailSqlExpression;

		/// <summary>
		/// StlDetailCountExpression varchar(500) NULL
		/// </summary>
		public string DetailCountExpression
		{
			get { return detailCountExpression; }
		}
		string detailCountExpression;

		/// <summary>
		/// DataSource varchar(20) NOT NULL
		/// </summary>
		public FeatureDataSource DataSource
		{
			get { return dataSource; }
		}
		//string dataSource;
		FeatureDataSource dataSource;

		public long TransactionCount
		{
			get { return transactionCount; }
		}
		public void ResetTransactionCount()
		{
			loadBillingDataException = null;
			transactionCount = -1;
		}
		long transactionCount = -1;

		public string UnitCountText
		{
			get
			{
				return (TransactionCount < 0) ? "" : TransactionCount.ToString();
			}
		}

		public string Name
		{
			get
			{
				return String.Format("{0} - {1} - {2} - {3}", Role, Module, Function, Feature);
			}
		}

		#endregion // Properties

		#region Update Lincenced Feature Dimension

		public Exception UpdateFeature(bool isSystem, bool isCompany, string summaryQuery, string detailSqlExpression, string detailCountExpression, string rules, string dataSource)
		{
			try
			{
				UpdateFeatureInTransaction(isSystem, isCompany, summaryQuery, detailSqlExpression, detailCountExpression, rules, dataSource);
				this.isSystemLevel = isSystem;
				this.isCompanyLevel = isCompany;
				this.summaryQuery = summaryQuery;
				this.detailSqlExpression = detailSqlExpression;
				this.detailCountExpression = detailCountExpression;
				this.billingRules = rules;
				this.dataSource = new FeatureDataSource(dataSource);
			}
			catch (Exception ex) // IsCriticalExceptionHandled Reason = return type is Exception
			{
				return ex;
			}

			return null;
		}

		void UpdateFeatureInTransaction(bool isSystem, bool isCompany, string summaryQuery, string detailSqlExpression, string detailCountExpression, string rules, string dataSource)
		{
			using (var stlDwConnection = DbManager.OpenNewStlAnalysisDataWarehouseConnection())
			{
				using (var stlDwTransaction = stlDwConnection.BeginTransaction())
				{
					UpdateDimLicensedFeatureTable(stlDwTransaction, isSystem, isCompany, summaryQuery, detailSqlExpression, detailCountExpression, rules, dataSource);
					stlDwTransaction.Commit();
				}
			}
		}

		void UpdateDimLicensedFeatureTable(SqlTransaction stlDwTransaction, bool isSystem, bool isCompany, string summaryQuery, string detailSqlExpression, string detailCountExpression, string rules, string dataSource)
		{
			string sqlText = @"
				UPDATE dbo.DimLicensedFeature
					SET
						IsSystemLevel = @IsSystem,
						IsCompanyLevel = @IsCompany,
						StlQuery = @SummaryQuery,
						StlDetailSqlExpression = @DetailSqlExpression,
						StlDetailCountExpression = @DetailCountExpression,
						StlRules = @Rules,
						DataSource = @DataSource
				WHERE FeatureId = @FeatureId";

			using (var updateCmd = DbManager.NewSqlCommand(sqlText, stlDwTransaction))
			{
				updateCmd.Parameters.Add("@IsSystem", SqlDbType.Bit).Value = isSystem;
				updateCmd.Parameters.Add("@IsCompany", SqlDbType.Bit).Value = isCompany;
				updateCmd.Parameters.Add("@SummaryQuery", SqlDbType.VarChar).Value = summaryQuery;
				updateCmd.Parameters.Add("@DetailSqlExpression", SqlDbType.VarChar).Value = detailSqlExpression;
				updateCmd.Parameters.Add("@DetailCountExpression", SqlDbType.VarChar).Value = detailCountExpression;
				updateCmd.Parameters.Add("@Rules", SqlDbType.VarChar).Value = rules;
				updateCmd.Parameters.Add("@DataSource", SqlDbType.VarChar).Value = dataSource;
				updateCmd.Parameters.Add("@FeatureId", SqlDbType.Int).Value = FeatureId;
				updateCmd.ExecuteNonQuery();
			}
		}

		#endregion // Update Lincenced Feature Dimension

		#region Collect Billing Data

		public void LoadTransactionCountPerPeriod(HostedClient client, DateTime startDateInclusive, DateTime endDateExclusive)
		{
			loadBillingDataException = null;
			transactionCount = -1;

			if (!String.IsNullOrWhiteSpace(summaryQuery))
			{
				try
				{
					transactionCount = GetTransactionCount(client, startDateInclusive, endDateExclusive);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					loadBillingDataException = ex;
					transactionCount = -1;
					if (ex.IsCriticalException())
					{
						throw;
					}
				}
			}
		}

		long GetTransactionCount(HostedClient client, DateTime startDateInclusive, DateTime endDateExclusive)
		{
			using (var clientConnection = DbManager.NewConnectionFromServerInfo(client.DatabaseInfo))
			{
				clientConnection.Open();

				string sqlText = String.Format(
					"SELECT sum(TransactionCount) FROM ({0}) TransactionPerDate {1}",
					summaryQuery,
					String.IsNullOrWhiteSpace(client.CompanyCode)
						? "" :
						String.Format("WHERE CompanyCode = '{0}'", client.CompanyCode)
				);

				using (var cmd = DbManager.NewSqlCommand(sqlText, clientConnection, timeoutInSeconds: 1 * 60))
				{
					cmd.Parameters.Add("@StartDateInclusive", System.Data.SqlDbType.Date).Value = startDateInclusive;
					cmd.Parameters.Add("@EndDateExclusive", System.Data.SqlDbType.Date).Value = endDateExclusive;
					object objValue = cmd.ExecuteScalar();
					return (objValue == DBNull.Value) ? -1 : Convert.ToInt64(objValue);
				}
			}
		}

		public Exception LoadBillingDataException
		{
			get { return loadBillingDataException; }
		}
		Exception loadBillingDataException;

		#endregion // Collect Billing Data
	}
}
