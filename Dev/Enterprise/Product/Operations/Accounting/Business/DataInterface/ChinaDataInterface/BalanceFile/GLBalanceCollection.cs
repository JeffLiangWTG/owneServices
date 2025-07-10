using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class GLBalanceCollection : DynamicBusinessObjectCollection<GLBalance>
	{
		public GLBalanceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			ReportOrder = GetReportOrders();
		}

		public void LoadCollection(int period)
		{
			fPeriod = GetLastYearEndPeriod(period);

			GetAccountRanges();
			Load(SQL, SQLParams);
		}

		#region Implementation

		#region Period Related

		protected int GetLastYearStartPeriod(int period)
		{
			int startPeriod = 0;

			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);
			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, Env.CurrentCompany.PK);
			filter.AddToFilter(AccPeriodManagementSchema.AM_Period, period);
			AccPeriodManagement periodManagement = Factory.LoadTop1(typeof(AccPeriodManagement), filter) as AccPeriodManagement;

			if (periodManagement != null)
			{
				startPeriod = periodCalc.GetFirstPeriodForYear(periodManagement.AM_Year - 1);
			}

			return startPeriod;
		}

		int GetLastYearEndPeriod(int period)
		{
			int startPeriod = 0;

			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(Factory);

			ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(AccPeriodManagementSchema.AM_Period, period);

			AccPeriodManagement periodManagement = Factory.LoadTop1(typeof(AccPeriodManagement), filter) as AccPeriodManagement;

			if (periodManagement != null)
			{
				startPeriod = periodCalc.GetLastPeriodForYear(periodManagement.AM_Year - 1);
			}

			return startPeriod;
		}

		#endregion

		#region PL Appropriation Account

		protected virtual ZGuid GLAccountSecondReportStartsFrom
		{
			get
			{
				return ReportOrder != null ? ReportOrder.GLAccountSecondReportStartsFrom : ZGuid.Empty;
			}
		}

		internal ZString PLAppropriationAccount
		{
			get
			{
				if (fPLAppropriationAccount.IsEmpty)
				{
					AccGLAccountDescriptor localGL = Factory.Load<AccGLAccountDescriptor>(GLAccountSecondReportStartsFrom);
					fPLAppropriationAccount = localGL != null ? localGL.AJ_LocalAccountNumber : ZString.Empty;
				}

				return fPLAppropriationAccount;
			}
		}

		ReportOrder GetReportOrders()
		{
			ReportOrderCollection reportOrders = AccountingConfigurationRegistry.Instance.ReportOrder.Value;
			return reportOrders.FindByLanguageAndCountryCode(Core.Constants.Languages.ChineseSimplified, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		ZString fPLAppropriationAccount;

		internal void GetAccountRanges()
		{
			ZString accountType = ReportOrder != null ? ReportOrder.AccountsOrderBeginsWith : ZString.Empty;

			if (accountType == nameof(AccountOrderType.BalanceSheet))
			{
				// BSH Start - the first Local Account
				// BSH End Account - One Account before Start From Account
				// P&L Start Account - StartFrom
				// P&L End Account - the last Local Account

				BSHEndAccount = GetLocalAccountBeforeThisAccount(PLAppropriationAccount);
				BSHStartAccount = GetFirstLocalAccountDescription();
				PLEndAccount = GetLastLocalAccountDescription();
				PLStartAccount = PLAppropriationAccount;
			}
			else
			{
				// BSH Start - StartFrom
				// BSH End Account - the last Local Account
				// P&L Start Account - the first Local Account
				// P&L End Account - One Account before Start From Account

				BSHEndAccount = GetLastLocalAccountDescription();
				BSHStartAccount = PLAppropriationAccount;
				PLEndAccount = GetLocalAccountBeforeThisAccount(PLAppropriationAccount);
				PLStartAccount = GetFirstLocalAccountDescription();
			}
		}

		#endregion

		#region Account Description Related

		internal ZString GetFirstLocalAccountDescription()
		{
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, GetLocalLanguage());
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.OrderBy = AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber + " ASC";

			return GetLocalAccountDescription(filter);
		}

		internal ZString GetLastLocalAccountDescription()
		{
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, GetLocalLanguage());
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.OrderBy = AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber + " DESC";

			return GetLocalAccountDescription(filter);
		}

		ZString GetLocalAccountBeforeThisAccount(ZString localAccountDescription)
		{
			ZQuery filter = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, GetLocalLanguage());
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.LessThan, localAccountDescription);
			filter.OrderBy = AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber + " DESC";

			return GetLocalAccountDescription(filter);
		}

		ZString GetLocalAccountDescription(ZQuery filter)
		{
			ZString localAccountNumberToReturn = "";
			AccGLAccountDescriptor descriptor = Factory.LoadTop1(typeof(AccGLAccountDescriptor), filter) as AccGLAccountDescriptor;
			if (descriptor != null)
			{
				localAccountNumberToReturn = descriptor.AJ_LocalAccountNumber;
			}
			return localAccountNumberToReturn;
		}

		#endregion

		#region SQL

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query")]
		string SQL
		{
			get
			{
				return @"SELECT AccountNumber AS GLAccountNumber, 
						CurrentBalance AS BalanceAmountInternal,
						AccountType as AccountType,
						LocalDebitCredit as LocalDebitCredit
						FROM 
						dbo.AccMultilingualBalanceSheet(
						@Period,
						@Company,
						@PandLStartAccount,
						@PandLEndAccount,
						@BalanceSheetStartAccount,
						@BalanceSheetEndAccount,
						@Language,
						@CountryCode,
						@IncludeZero
						)
						Where AccountType NOT IN (@TTL, @HDR)
						Order By AccountNumber";
			}
		}

		ZSqlParameterCollection SQLParams
		{
			get
			{
				ZSqlParameterCollection @params = new ZSqlParameterCollection();

				@params.Add("@Period", fPeriod, AccGLAggregateSchema.AA_Period);
				@params.Add("@Language", GetLocalLanguage(), AccGLAccountDescriptorSchema.AJ_Language);
				@params.Add("@CountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance);
				@params.Add("@Company", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK);
				@params.Add("@TTL", "TTL", AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);
				@params.Add("@HDR", "HDR", AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);
				@params.Add("@CR", "CR", AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);

				@params.Add("@PandLStartAccount", PLStartAccount, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);
				@params.Add("@PandLEndAccount", PLEndAccount, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);
				@params.Add("@BalanceSheetStartAccount", BSHStartAccount, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);
				@params.Add("@BalanceSheetEndAccount", BSHEndAccount, AccGLAccountDescriptorSchema.AJ_LocalAccountNumber);

				@params.Add("@IncludeZero", "Y", AccTransactionHeaderSchema.AH_PostToGL); // This is just to provide the correct columne type for the view.

				return @params;
			}
		}

		#endregion

		string GetLocalLanguage()
		{
			return DataInterfaceUtils.GetLocalLanguage();
		}

		int fPeriod;
		internal ZString BSHEndAccount;
		internal ZString BSHStartAccount;
		internal ZString PLEndAccount;
		internal ZString PLStartAccount;

		readonly ReportOrder ReportOrder;

		#endregion
	}
}
