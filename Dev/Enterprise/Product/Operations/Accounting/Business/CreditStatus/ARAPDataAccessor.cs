using System;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CreditStatus
{
	public enum InvoiceTerms
	{
		Standard,
		Disbursement,
		Batched
	}

	public class ARAPDataAccessor
	{
		public ARAPDataAccessor()
		{
			PeriodCalculator = new AccountingPeriodCalculator(Factory);
		}

		#region Last Receipt and Payment

		public decimal GetLastReceipt(Guid organisation)
		{
			return GetInvoiceAmountForLastTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, organisation) * -1;
		}

		public decimal GetLastPayment(Guid organisation)
		{
			return GetInvoiceAmountForLastTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, organisation);
		}

		public DateTime GetLastReceiptDate(Guid organisation)
		{
			return GetInvoiceDateForLastTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, organisation);
		}

		public DateTime GetLastPaymentDate(Guid organisation)
		{
			return GetInvoiceDateForLastTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, organisation);
		}

		public decimal GetLastSale(Guid organisation)
		{
			return GetInvoiceAmountForLastTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, organisation);
		}

		public decimal GetLastPurchase(Guid organisation)
		{
			return GetInvoiceAmountForLastTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, organisation) * -1;
		}

		#endregion

		#region Standard Outstanding Amount

		public decimal GetCurrentARAgeOutStandingAmount(Guid organisation, string agingOption)
		{
			return GetStandardAROutstandingAmount(organisation, 0, agingOption);
		}

		public decimal GetCurrentAPAgeOutStandingAmount(Guid organisation, string agingOption)
		{
			return GetStandardAPOutstandingAmount(organisation, 0, agingOption);
		}

		public decimal GetSingleARAgeOutStandingAmount(Guid organisation, string agingOption)
		{
			return GetStandardAROutstandingAmount(organisation, 1, agingOption);
		}

		public decimal GetSingleAPAgeOutStandingAmount(Guid organisation, string agingOption)
		{
			return GetStandardAPOutstandingAmount(organisation, 1, agingOption);
		}

		public decimal GetTwoARAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetStandardAROutstandingAmount(organisation, 2, agingOption);
		}

		public decimal GetTwoAPAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetStandardAPOutstandingAmount(organisation, 2, agingOption);
		}

		public decimal GetThreeARAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetStandardAROutstandingAmount(organisation, 3, agingOption);
		}

		public decimal GetThreeAPAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetStandardAPOutstandingAmount(organisation, 3, agingOption);
		}

		#endregion

		#region Disbursement Outstanding Amount

		public decimal GetDSBCurrentAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetDisbursementOutstandingAmount(organisation, 0, agingOption);
		}

		public decimal GetDSBSingleAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetDisbursementOutstandingAmount(organisation, 1, agingOption);
		}

		public decimal GetDSBTwoAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetDisbursementOutstandingAmount(organisation, 2, agingOption);
		}

		public decimal GetDSBThreeAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetDisbursementOutstandingAmount(organisation, 3, agingOption);
		}

		#endregion

		#region Batched Outstanding Amount

		public decimal GetBatchedCurrentAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetBatchedOutstandingAmount(organisation, 0, agingOption);
		}

		public decimal GetBatchedSingleAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetBatchedOutstandingAmount(organisation, 1, agingOption);
		}

		public decimal GetBatchedTwoAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetBatchedOutstandingAmount(organisation, 2, agingOption);
		}

		public decimal GetBatchedThreeAgeOutstandingAmount(Guid organisation, string agingOption)
		{
			return GetBatchedOutstandingAmount(organisation, 3, agingOption);
		}

		#endregion

		#region Standard Overdue Amount

		public decimal GetStandardAROverdueAmount(Guid organisation)
		{
			return GetOverdueAmount(LedgerTypes.AccountsReceivable, organisation, false);
		}

		#endregion

		#region Disbursement Overdue Amount

		public decimal GetDisbursementAROverdueAmount(Guid organisation)
		{
			return GetOverdueAmount(LedgerTypes.AccountsReceivable, organisation, true);
		}

		#endregion

		#region PTD, YTD and LYR Sales and Purchases

		public decimal GetPTDSales(Guid organisation)
		{
			return GetSumOfTransactionsForPeriod(organisation, LedgerTypes.AccountsReceivable, PeriodCalculator.GetPeriodFromDate(ZDateTime.Now));
		}

		public decimal GetPTDPurchases(Guid organisation)
		{
			return GetSumOfTransactionsForPeriod(organisation, LedgerTypes.AccountsPayable, PeriodCalculator.GetPeriodFromDate(ZDateTime.Now)) * -1;
		}

		public decimal GetYTDSales(Guid organisation)
		{
			return GetSumOfTransactionsForYear(organisation, LedgerTypes.AccountsReceivable, PeriodCalculator.GetPeriodFromDate(ZDateTime.Now) / 100);
		}

		public decimal GetYTDPurchases(Guid organisation)
		{
			return GetSumOfTransactionsForYear(organisation, LedgerTypes.AccountsPayable, PeriodCalculator.GetPeriodFromDate(ZDateTime.Now) / 100) * -1;
		}

		public decimal GetLYRSales(Guid organisation)
		{
			int lastYear = (PeriodCalculator.GetPeriodFromDate(ZDateTime.Now) / 100) - 1;
			return GetSumOfTransactionsForYear(organisation, LedgerTypes.AccountsReceivable, lastYear);
		}

		public decimal GetLYRPurchases(Guid organisation)
		{
			int lastYear = (PeriodCalculator.GetPeriodFromDate(ZDateTime.Now) / 100) - 1;
			return GetSumOfTransactionsForYear(organisation, LedgerTypes.AccountsPayable, lastYear) * -1;
		}

		#endregion

		#region Organisation Credit Limit and Invoice Terms

		public decimal GetARCreditLimit(Guid organisation)
		{
			return GetCreditLimit(organisation, LedgerTypes.AccountsReceivable);
		}

		public decimal GetAPCreditLimit(Guid organisation)
		{
			return GetCreditLimit(organisation, LedgerTypes.AccountsPayable);
		}

		public string GetPaymentTerms(Guid organisation)
		{
			var paymentTerm = GetInvoiceTermsAndDays(organisation, InvoiceTerms.Standard, LedgerTypes.AccountsPayable);
			var isTermWithoutDays = AccountingMasterFilesUtils.IsTermWithoutDays(paymentTerm.Term);
			return FormattableString.Invariant($"{(!isTermWithoutDays ? FormattableString.Invariant($"{paymentTerm.Days}/") : string.Empty)}{paymentTerm.Term}");
		}

		public string GetStandardInvoiceTerms(Guid organisation)
		{
			return GetARTermsAndDaysInfoAsCSV(organisation, InvoiceTerms.Standard);
		}

		public string GetDisbursementInvoiceTerms(Guid organisation)
		{
			return GetARTermsAndDaysInfoAsCSV(organisation, InvoiceTerms.Disbursement);
		}

		public decimal CalculatedCreditBalance(ZDecimal creditLimit, ZDecimal postedRevenue, ZDecimal recognisedWIP, ZDecimal unrecognisedWIP, ZDecimal claim)
		{
			var result = 0M;

			if (!AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				claim = 0;
			}

			var unpostedRevenueRegistrySetting = AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation.Value;

			switch (unpostedRevenueRegistrySetting)
			{
				case Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized:
					result = creditLimit - (postedRevenue - claim) - recognisedWIP - unrecognisedWIP;
					break;
				case Constants.CreditLimitChecking.PostedAndRecognized:
					result = creditLimit - (postedRevenue - claim) - recognisedWIP;
					break;
				case Constants.CreditLimitChecking.Posted:
					result = creditLimit - (postedRevenue - claim);
					break;
				default:
					throw new ArgumentException("Invalid registry IncludeUnpostedRevenueInCreditLimitCalculation.");
			}

			return result;
		}

		public decimal GetSettlementGroupARCreditBalance(OrgHeader org)
		{
			var creditLimit = GetARCreditLimit(org.PK.ToGuid());
			var creditTuple = GetSettlementGroupAmountTuple(org.OH_Code, LedgerTypes.AccountsReceivable);

			return CalculatedCreditBalance(creditLimit, creditTuple.BalanceAmout, creditTuple.RecognizedAmount, creditTuple.UnrecognizedAmount, creditTuple.ClaimAmout);
		}

		public decimal GetAPOutstandingBalance(Guid organisation)
		{
			return GetAPCreditLimit(organisation) - (GetOutstandingBalance(organisation, LedgerTypes.AccountsPayable) - GetClaimTotal(organisation, LedgerTypes.AccountsPayable));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public (decimal PostedRevenue, decimal RecognisedWIP, decimal UnrecognisedWIP, decimal Claim) GetUnpostedRevenueFieldValues(Guid organisation)
		{
			var postedRevenue = 0m;
			var recognisedWIP = 0m;
			var unrecognisedWIP = 0m;
			var claim = 0m;

			var sqlQuery = "SELECT BalanceTotal, RecognizedTotal, UnrecognizedTotal, ClaimTotal FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = @Org AND CompanyPK = @Company AND Ledger = @Ledger";
			using (var sqlCmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
			{
				sqlCmd.AddParameter("@Org", SqlDbType.UniqueIdentifier, organisation);
				sqlCmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				sqlCmd.AddParameter("@Ledger", SqlDbType.Char, LedgerTypes.AccountsReceivable);
				using (var reader = sqlCmd.ExecuteReader(CommandBehavior.SingleRow))
				{
					if (reader.Read())
					{
						postedRevenue = reader.GetDecimal(0);
						recognisedWIP = reader.GetDecimal(1);
						unrecognisedWIP = reader.GetDecimal(2);
						claim = reader.GetDecimal(3);
					}
				}
			}

			return (postedRevenue, recognisedWIP, unrecognisedWIP, claim);
		}

		public static string GetOutstandingBalanceSql()
		{
			return @"SELECT Amount = SUM(AH_OutstandingAmount)
					FROM dbo.AccTransactionHeader 
					WHERE AH_IsCancelled <> 1 
					AND AH_OH in (@OH)
					AND AH_Ledger = @Ledger
					AND AH_GC = @Company
					AND NOT AH_TransactionType = 'INB'";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		decimal GetClaimTotal(Guid orgPK, string ledger)
		{
			var sqlQuery = "SELECT ClaimTotal FROM dbo.vw_AccOrgBalance WHERE OrganizationPK = @Org AND CompanyPK = @Company AND Ledger = @Ledger";
			var claimTotal = 0m;
			if (AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				using (var sqlCmd = ((IDbConnected)Factory).Connection.Command(sqlQuery))
				{
					sqlCmd.AddParameter("@Org", SqlDbType.UniqueIdentifier, orgPK);
					sqlCmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
					sqlCmd.AddParameter("@Ledger", SqlDbType.Char, ledger);
					using (var reader = sqlCmd.ExecuteReader(CommandBehavior.SingleRow))
					{
						if (reader.Read())
						{
							claimTotal = (decimal)reader["ClaimTotal"];
						}
					}
				}
			}
			return claimTotal;
		}

		decimal GetOutstandingBalance(Guid orgPK, string ledger)
		{
			string sqlText = GetOutstandingBalanceSql();

			var parameters = new ZSqlParameterCollection
			{
				{ "@OH", orgPK, AccTransactionHeaderSchema.AH_OH },
				{ "@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@Company",GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC }
			};

			decimal amount = Factory.LoadScalarValue<ZDecimal>(sqlText, parameters);
			return ledger == LedgerTypes.AccountsPayable ? (amount * -1) : amount;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		(decimal BalanceAmout, decimal ClaimAmout, decimal RecognizedAmount, decimal UnrecognizedAmount) GetSettlementGroupAmountTuple(string orgCode, string ledger)
		{
			string sqlText = @"OrgCreditLimitAndBalanceDetails";
			DbCommand sqlCmd = ((IDbConnected)Factory).Connection.Command(sqlText);
			sqlCmd.CommandType = CommandType.StoredProcedure;
			sqlCmd.AddParameterBasedOnDbColumn("@OrgCode", orgCode, OrgHeaderSchema.OH_Code);
			sqlCmd.AddParameterBasedOnDbColumn("@CompanyCode", GlbCompany.CurrentCompany.GC_Code.ToString(), GlbCompanySchema.GC_Code);
			sqlCmd.AddParameterBasedOnDbColumn("@AccLedger", ledger, AccTransactionHeaderSchema.AH_Ledger);
			sqlCmd.AddParameterBasedOnDbColumn("@AgingPeriod", 30, CargoWise.Schema.Schema.GenericIntSchemaColumn);
			sqlCmd.AddParameter("@BalanceOverdueAgingOption", SqlDbType.VarChar, 3, Constants.BalanceOverdueAgingOption.Balance);

			decimal balanceAmout = 0m;
			decimal claimAmout = 0m;
			decimal recognizedAmount = 0m;
			decimal unrecognizedAmount = 0m;
			using (var reader = sqlCmd.ExecuteReader())
			{
				if (reader.Read())
				{
					balanceAmout = GetDecimal(reader, "AccountBalanceTotal");

					claimAmout = GetDecimal(reader, "ClaimTotal");

					recognizedAmount = GetDecimal(reader, "UnpostedRevenueRecognisedTotal");

					unrecognizedAmount = GetDecimal(reader, "UnpostedRevenueUnrecognisedTotal");
				}
			}

			return ledger == LedgerTypes.AccountsPayable ? ((-balanceAmout, -claimAmout, -recognizedAmount, -unrecognizedAmount)) : (balanceAmout, claimAmout, recognizedAmount, unrecognizedAmount);
		}

		decimal GetDecimal(IDataReader reader, string columnName)
		{
			var index = reader.GetOrdinal(columnName);
			var amount = reader.IsDBNull(index) ? 0m : reader.GetDecimal(index);

			return amount;
		}

		#endregion

		#region Implementation

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory() { NameForDebugging = "ARAPDataAccessor_GetFactory" };
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		protected AccountingPeriodCalculator PeriodCalculator;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query")]
		protected decimal GetOverdueAmount(string ledger, Guid organisation, bool isDisbursement)
		{
			decimal overdueAmount = 0;
			var inOrNotInDSBInvoiceCategory = isDisbursement ? "IN" : "NOT IN";
			string query = FormattableString.Invariant($@"
				SELECT SUM(AH_OutstandingAmount) as TotalOutstandingAmount
				FROM dbo.AccTransactionHeader
				WHERE AH_Ledger = @Ledger
						AND AH_OH = @Organisation
						AND AH_GC = @Company
						AND NOT AH_TransactionType = 'INB'
						AND AH_IsCancelled = 0
						AND AH_TransactionCategory {inOrNotInDSBInvoiceCategory} ({DisbursementInvCategories})
						AND AH_DueDate < @CurrentDate");

			var parameters = new ZSqlParameterCollection
			{
				{ "@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@Organisation", organisation, AccTransactionHeaderSchema.AH_OH },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC },
				{ "@CurrentDate", ZDateTime.Now.ToDateTime(), AccTransactionHeaderSchema.AH_DueDate }
			};

			overdueAmount = Factory.LoadScalarValue<ZDecimal>(query, parameters);
			return ledger == LedgerTypes.AccountsReceivable ? overdueAmount : overdueAmount * -1m;
		}

		protected decimal GetStandardAROutstandingAmount(Guid organisation, int age, string agingOption)
		{
			return GetOutstandingAmount(LedgerTypes.AccountsReceivable, organisation, age, InvoiceTerms.Standard, agingOption);
		}

		protected decimal GetStandardAPOutstandingAmount(Guid organisation, int age, string agingOption)
		{
			return GetOutstandingAmount(LedgerTypes.AccountsPayable, organisation, age, InvoiceTerms.Standard, agingOption);
		}

		protected decimal GetDisbursementOutstandingAmount(Guid organisation, int age, string agingOption)
		{
			return GetOutstandingAmount(LedgerTypes.AccountsReceivable, organisation, age, InvoiceTerms.Disbursement, agingOption);
		}

		protected decimal GetBatchedOutstandingAmount(Guid organisation, int age, string agingOption)
		{
			return GetOutstandingAmount(LedgerTypes.AccountsReceivable, organisation, age, InvoiceTerms.Batched, agingOption);
		}

		protected decimal GetSumOfTransactionsForPeriod(Guid organisation, string ledger, int period)
		{
			string sQL = String.Format(@"SELECT SUM(AH_InvoiceAmount) as TotalInvoiceAmount
											FROM dbo.AccTransactionHeader 
											WHERE AH_TransactionType IN ('INV', 'CRD', 'ADJ')
												AND AH_Ledger = @Ledger
												AND AH_OH = @Organisation
												AND AH_GC = @Company
												AND AH_IsCancelled = 0
												AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate)");
			var parameters = new ZSqlParameterCollection
			{
				{ "@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@Organisation", organisation, AccTransactionHeaderSchema.AH_OH },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC }
			};

			ZDateTime startDate = PeriodCalculator.GetFirstDayForPeriod(period);
			ZDateTime endDate = PeriodCalculator.GetLastDayForPeriod(period);

			decimal result = 0m;
			if (startDate.IsValid && endDate.IsValid)
			{
				parameters.Add("@StartDate", startDate.ToDateTime(), AccTransactionHeaderSchema.AH_PostDate);
				parameters.Add("@EndDate", endDate.ToDateTime(), AccTransactionHeaderSchema.AH_PostDate);

				result = Factory.LoadScalarValue<ZDecimal>(sQL, parameters);
			}
			return result;
		}

		protected decimal GetSumOfTransactionsForYear(Guid organisation, string ledger, int year)
		{
			string sQL = String.Format(@"SELECT SUM(AH_InvoiceAmount) as TotalInvoiceAmount
											FROM dbo.AccTransactionHeader 
											WHERE AH_TransactionType IN ('INV','CRD','ADJ')
												AND AH_Ledger = @Ledger
												AND AH_OH = @Organisation
												AND AH_GC = @Company
												AND AH_IsCancelled = 0
												AND (AH_PostDate >= @StartDate AND AH_PostDate <= @EndDate)");

			var parameters = new ZSqlParameterCollection
			{
				{ "@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@Organisation", organisation, AccTransactionHeaderSchema.AH_OH },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC }
			};

			ZInt startPeriod = PeriodCalculator.GetFirstPeriodForYear(year);

			PeriodCalculator = new AccountingPeriodCalculator(Factory);

			ZInt endPeriod = PeriodCalculator.GetLastPeriodForYear(year);

			decimal result = 0m;
			if (startPeriod != Enterprise.MasterFiles.Business.AccountingPeriodCalculator.InvalidPeriod && endPeriod != Enterprise.MasterFiles.Business.AccountingPeriodCalculator.InvalidPeriod)
			{
				ZDateTime startTime = PeriodCalculator.GetFirstDayForPeriod(startPeriod);
				ZDateTime endTime = PeriodCalculator.GetLastDayForPeriod(endPeriod);

				parameters.Add("@StartDate", startTime.ToDateTime(), AccTransactionHeaderSchema.AH_PostDate);
				parameters.Add("@EndDate", endTime.ToDateTime(), AccTransactionHeaderSchema.AH_PostDate);

				result = Factory.LoadScalarValue<ZDecimal>(sQL, parameters);
			}
			return result;
		}

		protected decimal GetInvoiceAmountForLastTransaction(string ledger, string transactionType, Guid organisation)
		{
			string sQL = String.Format(@"SELECT TOP 1 ISNULL(AH_InvoiceAmount, 0) as TotalInvoiceAmount
								FROM dbo.AccTransactionHeader 
								WHERE 
								AH_OH = @Organisation 
								AND AH_Ledger = @LedgerType 
								AND AH_TransactionType = @TransactionType 
								AND AH_GC = @Company
								AND AH_IsCancelled = 0
								ORDER BY AH_InvoiceDate DESC, AH_TransactionNum	DESC");

			var parameters = new ZSqlParameterCollection
			{
				{ "@Organisation", organisation, AccTransactionHeaderSchema.AH_OH },
				{ "@LedgerType", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@TransactionType", transactionType, AccTransactionHeaderSchema.AH_TransactionType },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC }
			};

			var result = Factory.LoadScalarValue<ZDecimal>(sQL, parameters);
			return result;
		}

		#region Global Account Balance and Credit Status

		public ZString GetGlobalCreditGroupName(Guid organisation)
		{
			var org = Factory.Load<OrgHeader>(organisation);
			return Factory.Load<OrgHeader>(org?.MiscServ.ARGlobalCreditGroupForDisplayPK ?? ZGuid.Empty)?.OH_Code ?? ZString.Empty;
		}

		public decimal GetGlobalCreditLimit(Guid organisation)
		{
			var org = Factory.Load<OrgHeader>(organisation);
			return org?.MiscServ.ARGlobalCreditLimit ?? 0;
		}

		public string GetGlobalCreditCurrency(Guid organisation)
		{
			var org = Factory.Load<OrgHeader>(organisation);
			return org?.MiscServ.ARGlobalCreditCurrencyCode ?? string.Empty;
		}

		public decimal GetGlobalCreditAvailable(Guid organisation)
		{
			var org = Factory.Load<OrgHeader>(organisation);
			return GetGlobalCreditLimit(organisation) - org?.CreditChecker?.GetCreditDetails(LedgerTypes.AccountsReceivable)?.GlobalTotalOutstandingAmount ?? 0;
		}

		#endregion

		public int GetARAverageDaysFromInvoiceDateToFullyPaidDate(Guid organisation)
		{
			return GetAverageDaysFromInvoiceDateToFullyPaidDate(LedgerTypes.AccountsReceivable, organisation, null);
		}

		public int GetARAverageDaysFromDueDateToFullyPaidDate(Guid organisation)
		{
			return GetAverageDaysFromDueDateToFullyPaidDate(LedgerTypes.AccountsReceivable, organisation, null);
		}

		public int GetDisbursementARAverageDaysFromDueDateToFullyPaidDate(Guid organisation)
		{
			return GetAverageDaysFromDueDateToFullyPaidDate(LedgerTypes.AccountsReceivable, organisation, true);
		}

		public int GetStandardARAverageDaysFromDueDateToFullyPaidDate(Guid organisation)
		{
			return GetAverageDaysFromDueDateToFullyPaidDate(LedgerTypes.AccountsReceivable, organisation, false);
		}

		public int GetAPAverageDaysFromInvoiceDateToFullyPaidDate(Guid organisation)
		{
			return GetAverageDaysFromInvoiceDateToFullyPaidDate(LedgerTypes.AccountsPayable, organisation, null);
		}

		public int GetAPAverageDaysFromDueDateToFullyPaidDate(Guid organisation)
		{
			return GetAverageDaysFromDueDateToFullyPaidDate(LedgerTypes.AccountsPayable, organisation, null);
		}

		public int GetDisbursementAPAverageDaysFromDueDateToFullyPaidDate(Guid organisation)
		{
			return GetAverageDaysFromDueDateToFullyPaidDate(LedgerTypes.AccountsPayable, organisation, true);
		}

		public int GetStandardAPAverageDaysFromDueDateToFullyPaidDate(Guid organisation)
		{
			return GetAverageDaysFromDueDateToFullyPaidDate(LedgerTypes.AccountsPayable, organisation, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query")]
		protected int GetAverageDaysFromInvoiceDateToFullyPaidDate(string ledger, Guid organisation, bool? isDisbursement)
		{
			string sQL = String.Format(@"
				SELECT AVG(DATEDIFF(d, AH_InvoiceDate, AH_FullyPaidDate)) As AvgDateDiff
					FROM dbo.AccTransactionHeader 
					WHERE AH_Ledger = @Ledger
						AND AH_OH = @Organisation
						AND AH_GC = @Company
						AND AH_TransactionType = 'INV'
						AND AH_IsCancelled = 0
						AND AH_FullyPaidDate IS NOT NULL
						{0}",
						isDisbursement.HasValue ? ("AND AH_TransactionCategory " + (isDisbursement.Value ? "IN" : "NOT IN") + " (" + DisbursementInvCategories + ")") : string.Empty);

			var parameters = new ZSqlParameterCollection
			{
				{ "@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@Organisation", organisation, AccTransactionHeaderSchema.AH_OH },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC }
			};

			int result = Factory.LoadScalarValue<ZInt>(sQL, parameters);
			return result;
		}

		string DisbursementInvCategories
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (string category in InvoiceTypeCalculationProvider.DisbursementInvoiceTypes)
				{
					result.AppendFormat("'{0}', ", category);
				}

				return result.ToString().TrimEnd(',', ' ');
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query")]
		protected int GetAverageDaysFromDueDateToFullyPaidDate(string ledger, Guid organisation, bool? isDisbursement)
		{
			string sQL = String.Format(@"
				SELECT AVG(DATEDIFF(d, AH_DueDate, AH_FullyPaidDate)) AS AvgDateDiff
					FROM dbo.AccTransactionHeader
					WHERE AH_Ledger = @Ledger
						AND AH_OH = @Organisation
						AND AH_GC = @Company
						AND AH_TransactionType = 'INV'
						AND AH_IsCancelled = 0
						AND AH_FullyPaidDate IS NOT NULL
						{0}",
						isDisbursement.HasValue ? ("AND AH_TransactionCategory " + (isDisbursement.Value ? "IN" : "NOT IN") + " (" + DisbursementInvCategories + ")") : string.Empty);

			var parameters = new ZSqlParameterCollection
			{
				{ "@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@Organisation", organisation, AccTransactionHeaderSchema.AH_OH },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(),  AccTransactionHeaderSchema.AH_GC }
			};

			int result = Factory.LoadScalarValue<ZInt>(sQL, parameters);
			return result;
		}

		#region OutstandingAmount

		const string SelectOutstandingAmountSQL = "SELECT SUM(AH_OutstandingAmount) as TotalOutStandingAmount FROM dbo.AccTransactionHeader";
		const string SelectBatchedOutstandingAmountSQL = @"SELECT SUM(AH_OutstandingAmount) as TotalOutStandingAmount
					FROM dbo.AccTransactionHeader
					LEFT JOIN dbo.AccCollectionOrderLine ON AH_PK = AOL_AH
					LEFT JOIN dbo.AccCollectionOrder ON AOL_ACO = ACO_PK
					LEFT JOIN dbo.AccCollectionBatch ON ACO_ACB = ACB_PK";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query")]
		ZString GetOutstandingAmountSQL(int age, InvoiceTerms termsType, string agingOption, out SchemaColumn schemaColumn)
		{
			ZString agingDate;
			switch (agingOption)
			{
				case AccountingConstants.AgingOptions.DueDate:
					agingDate = "AH_DueDate";
					schemaColumn = AccTransactionHeaderSchema.AH_DueDate;
					break;
				case AccountingConstants.AgingOptions.InvoiceDate:
					agingDate = "AH_InvoiceDate";
					schemaColumn = AccTransactionHeaderSchema.AH_InvoiceDate;
					break;
				case AccountingConstants.AgingOptions.PostDate:
					agingDate = "AH_PostDate";
					schemaColumn = AccTransactionHeaderSchema.AH_PostDate;
					break;
				default:
					schemaColumn = null;
					return ZString.Empty;
			}

			var agingDateEndFilter = age != 0 ? string.Format(CultureInfo.InvariantCulture, " AND {0} <= @EndDate", agingDate) : string.Empty;
			var agingDateStartFilter = string.Empty;
			if (age != 3)
			{
				if (age == 0 && agingOption == AccountingConstants.AgingOptions.DueDate)
				{
					agingDateStartFilter = string.Format(CultureInfo.InvariantCulture, " AND ({0} >= @StartDate OR {0} IS NULL)", agingDate);
				}
				else
				{
					agingDateStartFilter = string.Format(CultureInfo.InvariantCulture, " AND {0} >= @StartDate", agingDate);
				}
			}

			var where = $@" WHERE AH_Ledger = @Ledger
						AND AH_OH = @Organisation
						AND AH_GC = @Company
						AND NOT AH_TransactionType = 'INB'
						AND AH_IsCancelled = 0
						{agingDateEndFilter}
						{agingDateStartFilter}";

			ZString sql;
			switch (termsType)
			{
				case InvoiceTerms.Standard:
					sql = SelectOutstandingAmountSQL + where + " AND AH_TransactionCategory NOT IN (" + DisbursementInvCategories + ")";
					break;
				case InvoiceTerms.Disbursement:
					sql = SelectOutstandingAmountSQL + where + " AND AH_TransactionCategory IN (" + DisbursementInvCategories + ")";
					break;
				case InvoiceTerms.Batched:
					sql = SelectBatchedOutstandingAmountSQL + where + " AND ACB_IsCancelled = 0";
					break;
				default:
					return ZString.Empty;
			}

			return sql;
		}

		protected decimal GetOutstandingAmount(string ledger, Guid organisation, int age, InvoiceTerms termsType, string agingOption)
		{
			if (age < 0 || age > 3)
			{
				throw new ArgumentException("Age must be between zero and three");
			}

			var result = 0M;

			SchemaColumn agingDateSchemaColumn;
			var sql = GetOutstandingAmountSQL(age, termsType, agingOption, out agingDateSchemaColumn);
			if (sql.IsEmpty)
			{
				return result;
			}

			var parameters = new ZSqlParameterCollection
			{
				{ "@Ledger", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@Organisation", organisation, AccTransactionHeaderSchema.AH_OH },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC },
			};

			var agePeriod = GetAgePeriod(PeriodCalculator.GetPeriodFromDate(ZDateTime.Now), age);
			if (agePeriod != AccountingPeriodCalculator.InvalidPeriod)
			{
				if (age != 0)
				{
					var endDate = PeriodCalculator.GetLastDayForPeriod(agePeriod);
					parameters.Add("@EndDate", endDate.ToDateTime(), agingDateSchemaColumn);
				}

				if (age != 3)
				{
					var startDate = PeriodCalculator.GetFirstDayForPeriod(agePeriod);
					parameters.Add("@StartDate", startDate.ToDateTime(), agingDateSchemaColumn);
				}

				if (agePeriod != AccountingPeriodCalculator.InvalidPeriod)
				{
					result = Factory.LoadScalarValue<ZDecimal>(sql, parameters);
				}
			}
			return ledger == LedgerTypes.AccountsReceivable ? result : result * -1;
		}

		#endregion

		protected DateTime GetInvoiceDateForLastTransaction(string ledger, string transactionType, Guid organisation)
		{
			string sQL = String.Format(@"SELECT TOP 1 AH_InvoiceDate 
								FROM dbo.AccTransactionHeader 
								WHERE 
								AH_OH = @Organisation 
								AND AH_Ledger = @LedgerType 
								AND AH_TransactionType = @TransactionType 
								AND AH_GC = @Company
								AND AH_IsCancelled = 0
								ORDER BY AH_InvoiceDate DESC");

			var parameters = new ZSqlParameterCollection
			{
				{ "@Organisation", organisation, AccTransactionHeaderSchema.AH_OH },
				{ "@LedgerType", ledger, AccTransactionHeaderSchema.AH_Ledger },
				{ "@TransactionType", transactionType, AccTransactionHeaderSchema.AH_TransactionType },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC }
			};

			var invoiceDate = Factory.LoadScalarValue<ZDateTime>(sQL, parameters);
			var result = invoiceDate.IsValid ? invoiceDate.ToDateTime() : DateTime.MinValue;
			return result;
		}

		protected InvoiceTerm GetInvoiceTermsAndDays(Guid organisation, InvoiceTerms termsType, string ledger)
		{
			return GetInvoiceTermsAndDays(organisation, ledger, null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty, termsType);
		}

		protected InvoiceTerm GetInvoiceTermsAndDays(Guid organisation, string ledger, JobInvoicingConsumerType jobType, string direction, string transportMode, ZGuid branchPK, ZGuid departmentPK, InvoiceTerms termsType)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery query = new ZQuery(OrgCompanyDataSchema.OB_OH, organisation);
			query.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			OrgCompanyData[] data = (OrgCompanyData[])factory.Load(typeof(OrgCompanyData), query);

			InvoiceTerm term = new InvoiceTerm();
			if (data.Length > 0)
			{
				if (ledger == LedgerTypes.AccountsPayable)
				{
					term = data[0].GetAPTerm();
				}
				else if (ledger == LedgerTypes.AccountsReceivable)
				{
					if (termsType == InvoiceTerms.Disbursement)
					{
						term = data[0].GetDisbursementARTerm(jobType, direction, transportMode, branchPK, departmentPK);
					}
					else
					{
						term = data[0].GetARTerm(jobType, direction, transportMode, branchPK, departmentPK);
					}
				}
			}
			return term;
		}

		protected string GetARTermsAndDaysInfoAsCSV(Guid organisation, InvoiceTerms termsType)
		{
			string result = string.Empty;

			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery query = new ZQuery(OrgCompanyDataSchema.OB_OH, organisation);
			query.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			OrgCompanyData[] data = (OrgCompanyData[])factory.Load(typeof(OrgCompanyData), query);

			if (data.Length > 0)
			{
				if (termsType == InvoiceTerms.Disbursement)
				{
					result = data[0].GetOrgARTermsAsCSV(", ", x => x.IsDisbursementTerm || x.IsDefaultTerm);
				}
				else
				{
					result = data[0].GetOrgARTermsAsCSV(", ", x => !x.IsDisbursementTerm || x.IsDefaultTerm);
				}
			}

			return result;
		}

		protected decimal GetCreditLimit(Guid organisation, string ledger)
		{
			string creditLimitFieldName, sql;
			if (ledger == LedgerTypes.AccountsPayable)
			{
				creditLimitFieldName = OrgCompanyDataSchema.Constants.OB_APCreditLimit;
				sql = String.Format("SELECT {0} FROM {1} WHERE {2} = @Organisation AND {3} = @Company", creditLimitFieldName, OrgCompanyDataSchema.Constants.TableName, OrgCompanyDataSchema.Constants.OB_OH, OrgCompanyDataSchema.Constants.OB_GC);
			}
			else
			{
				creditLimitFieldName = OrgCompanyDataSchema.Constants.OB_ARCreditLimit;
				sql = String.Format("SELECT (SELECT AdjustedCreditLimit from OrgAdjustedCreditLimit(OB_ARCreditLimit, OB_ARTemporaryCreditLimitIncrease, OB_ARTemporaryCreditLimitIncreaseExpiry)) AS {0} FROM {1} WHERE {2} = @Organisation AND {3} = @Company", creditLimitFieldName, OrgCompanyDataSchema.Constants.TableName, OrgCompanyDataSchema.Constants.OB_OH, OrgCompanyDataSchema.Constants.OB_GC);
			}

			var parameters = new ZSqlParameterCollection
			{
				{ "@Organisation", organisation, OrgCompanyDataSchema.OB_OH },
				{ "@Company", GlbCompany.CurrentCompany.PK.ToGuid(), OrgCompanyDataSchema.OB_GC }
			};

			decimal result = Factory.LoadScalarValue<ZDecimal>(sql, parameters);
			return result;
		}

		public int GetAgePeriod(int currentPeriod, int age)
		{
			if (age < 0 || age > 3)
			{
				throw new ArgumentException("Age string must be 0, 1, 2 or 3");
			}

			int periodsToAge = Utilities.ConvertToInt32(age);
			int agePeriod = currentPeriod;
			for (int i = 0; i < periodsToAge; i++)
			{
				ZInt proposedAgePeriod = PeriodCalculator.GetPreviousPeriod(agePeriod);
				if (proposedAgePeriod != Enterprise.MasterFiles.Business.AccountingPeriodCalculator.InvalidPeriod)
				{
					agePeriod = proposedAgePeriod;
				}
				else
				{
					agePeriod = Enterprise.MasterFiles.Business.AccountingPeriodCalculator.InvalidPeriod;
					break;
				}
			}
			return agePeriod;
		}

		#endregion
	}
}
