using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosAccountsAggregator
	{
		public CognosAccountsAggregator(ZDateTime exportStartDateTime, ICognosNotificationSubscriber notifications)
		{
			this.ExportStartDateTime = exportStartDateTime;
			this.Notifications = notifications;
		}

		public virtual void AggregateToExportTempTable()
		{
			AggregateBSHAndPNLRecords();
			AggregateALT_TTL_CFW_CLNRecords();
			ExecuteProcedureNonQuery(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertCognosAccountsIntoCognosExportTempTable, 5);
		}

		#region Executing Stored Procedures

		void AggregateBSHAndPNLRecords()
		{
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts, 5);
			ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts, 5);
			ExecuteProcedureNonQuery(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInvertCognosRawAggregateSignageIfApplicable, 2);
		}

		void AggregateALT_TTL_CFW_CLNRecords()
		{
			ExecuteProcedureNonQuery(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientUpdateCognosRawAggregateRecordsWithALTAccounts, 5);
			AggregateTTLAndCFWAccountsRecords();
			ExecuteProcedureNonQuery(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts, 5);
			ExecuteProcedureNonQuery(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts, 5);
		}

		void AggregateTTLAndCFWAccountsRecords()
		{
			SqlParamInfo[] procedureParams = new SqlParamInfo[]
				{
					new SqlParamInfo("@PnLStartAccount", SqlDbType.VarChar, PnLStartAccount.ToString()),
					new SqlParamInfo("@BSHStartAccount", SqlDbType.VarChar, BSHStartAccount.ToString())
				};
			ExecuteProcedureNonQuery(JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts, 5, procedureParams);
		}

		ZString BSHStartAccount
		{
			get
			{
				if (fBSHStartAccount == null)
				{
					fBSHStartAccount = (AccountType == nameof(AccountOrderType.BalanceSheet))
							? GetFirstLocalAccountNumber()
							: ReportOrderSeparatorAccountNumber;
				}
				return fBSHStartAccount;
			}
		}

		ZString PnLStartAccount
		{
			get
			{
				if (fPnLStartAccount == null)
				{
					fPnLStartAccount = (AccountType == nameof(AccountOrderType.BalanceSheet))
							? ReportOrderSeparatorAccountNumber
							: GetFirstLocalAccountNumber();
				}
				return fPnLStartAccount;
			}
		}

		ZString AccountType
		{
			get { return ReportOrder != null ? ReportOrder.AccountsOrderBeginsWith : ZString.Empty; }
		}

		ReportOrder ReportOrder
		{
			get
			{
				if (fReportOrder == null)
				{
					ReportOrderCollection reportOrders = AccountingConfigurationRegistry.Instance.ReportOrder.Value;
					fReportOrder = reportOrders.FindByLanguageAndCountryCode(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, GlbCompany.CurrentCompany.GC_RN_NKCountryCode) ??
						   reportOrders.FindByLanguageAndCountryCode(Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger, "");
				}
				return fReportOrder;
			}
		}

		ZString GetFirstLocalAccountNumber()
		{
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger);
			filter.OrderBy = AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber + " ASC";

			return GetLocalAccountNumber(filter);
		}

		ZString GetLocalAccountNumber(ZQuery filter)
		{
			ZString result = ZString.Empty;

			AccGLAccountDescriptor descriptor = FactoryForAccGLAccountDescriptor.LoadTop1<AccGLAccountDescriptor>(filter);
			if (descriptor != null)
			{
				result = descriptor.AJ_LocalAccountNumber;
			}

			return result;
		}

		ZGuid GLAccountSecondReportStartsFrom
		{
			get { return ReportOrder != null ? ReportOrder.GLAccountSecondReportStartsFrom : ZGuid.Empty; }
		}

		ZString ReportOrderSeparatorAccountNumber
		{
			get
			{
				if (fReportOrderSeparatorAccountNumber == null)
				{
					AccGLAccountDescriptor localGL = FactoryForAccGLAccountDescriptor.Load<AccGLAccountDescriptor>(GLAccountSecondReportStartsFrom);
					fReportOrderSeparatorAccountNumber = localGL != null ? localGL.AJ_LocalAccountNumber : ZString.Empty;
				}
				return fReportOrderSeparatorAccountNumber;
			}
		}

		BusinessObjectFactory FactoryForAccGLAccountDescriptor
		{
			get
			{
				if (fFactoryForAccGLAccountDescriptor == null)
				{
					fFactoryForAccGLAccountDescriptor = new BusinessObjectFactory();
				}
				return fFactoryForAccGLAccountDescriptor;
			}
		}

		string fBSHStartAccount;
		string fPnLStartAccount;
		string fReportOrderSeparatorAccountNumber;
		ReportOrder fReportOrder;
		BusinessObjectFactory fFactoryForAccGLAccountDescriptor;

		#endregion

		#region Implementation

		string GetStoredProcDescription(string storedProcName)
		{
			string result = "";

			switch (storedProcName)
			{
				#region BSH & PnL

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeader:
					result = "Aggregating AR / AP Journal Transactions";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromHeaderBank:
					result = "Aggregating Payment, Receipt, Cashbook Exchange Difference and Cashbook Transfer Transactions";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentLine:
					result = "Aggregating Direct Receipt, Direct Payment Transactions";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromDirectReceiptPaymentBank:
					result = "Aggregating Bank Direct Receipt, Bank Direct Payment Transactions";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromInvCrdAdj:
					result = "Aggregating Invoices, Credit Notes and Adjustment Notes";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFX:
					result = "Aggregating Job Costing Journal Transactions";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGLJournals:
					result = "Aggregating GL Standard, Auto and Reverse Journal Transactions";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACR:
					result = "Aggregating WIP and Accruals";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromARAPControlAccounts:
					result = "Aggregating AR / AP Control Accounts";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromExchangeDiscountOverpaymentControlAccounts:
					result = "Aggregating Exchange Difference, Discount and Overpayment Control Accounts";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromGSTControlAccounts:
					result = "Aggregating Tax Amounts";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromWIPACRControlAccounts:
					result = "Aggregating WIP and Accruals Control Accounts";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInvertCognosRawAggregateSignageIfApplicable:
					result = "Applying signage conversion for BSH and P&L Accounts (if applicable)";
					break;

				#endregion

				#region TTL, CLN, ALT, CFW

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientUpdateCognosRawAggregateRecordsWithALTAccounts:
					result = "Calculating Alternative Accounts";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCFWAndTTLAccounts:
					result = "Calculating Total Accounts";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_GLAggregationFromCLNAccounts:
					result = "Calculating Consolidation Accounts";
					break;

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertIntoCognosRawAggregateTable_FromSubClassificationAccounts:
					result = "Calculating Sub-Classification Accounts";
					break;

				#endregion

				case JASClientDbSchemaUpgradeInfo.DbObjectNames.ClientInsertCognosAccountsIntoCognosExportTempTable:
					result = "Preparing Cognos Account Records to be Exported";
					break;
			}

			return result;
		}

		internal DbCommand GetStoredProcDbCommand(string procedureName, params SqlParamInfo[] procedureParams)
		{
			DbCommand result = Db.Connection.Command("", 600);

			string paramsAsString = "";
			foreach (SqlParamInfo paramInfo in procedureParams)
			{
				if (paramsAsString.Length > 0)
				{
					paramsAsString += ", ";
				}
				paramsAsString += paramInfo.Name;
				result.AddParameter(paramInfo.Name, paramInfo.Type, 8000, paramInfo.Value);
			}
			result.CommandText = string.Format("EXEC {0} {1}", procedureName, paramsAsString);

			return result;
		}

		void ExecuteProcedureNonQuery(string procedureName, int processPercentage, params SqlParamInfo[] procedureParams)
		{
			string description = GetStoredProcDescription(procedureName);
			Notifications.Notify(new InfoNotification(description));

			DbCommand command = GetStoredProcDbCommand(procedureName, procedureParams);
			command.ExecuteNonQuery();

			Notifications.AdvanceProgressBy(processPercentage);
		}

		void ExecuteProcedureNonQueryWithCompanyPKAndExportStartDateAsParams(string procedureName, int processPercentage)
		{
			SqlParamInfo[] procedureParams = new SqlParamInfo[]
				{
					new SqlParamInfo("@CompanyPK", SqlDbType.UniqueIdentifier, Env.CurrentCompany.PK),
					new SqlParamInfo("@ExportStartDate", SqlDbType.SmallDateTime, ExportStartDateTime.ToDateTime())
				};
			ExecuteProcedureNonQuery(procedureName, processPercentage, procedureParams);
		}

		internal struct SqlParamInfo
		{
			public SqlParamInfo(string name, SqlDbType type, object value)
			{
				this.Name = name;
				this.Type = type;
				this.Value = value;
			}

			public readonly string Name;
			public readonly SqlDbType Type;
			public readonly object Value;
		}

		#endregion

		public readonly ZDateTime ExportStartDateTime;
		public readonly ICognosNotificationSubscriber Notifications;

		internal ZString InternalBSHStartAccountTest => BSHStartAccount;
		internal ZString InternalPnLStartAccountTest => PnLStartAccount;
	}
}
