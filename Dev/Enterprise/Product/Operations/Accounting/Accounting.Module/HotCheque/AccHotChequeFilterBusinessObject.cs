using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccHotChequeFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public AccHotChequeFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddNumberFilters(filters);
			AddReferenceFilters(filters);
			AddOtherFilters(filters);

			return filters;
		}

		#region FinancialDetails Category

		FilterCategory fFinancialDetailsCategory;
		protected FilterCategory FinancialDetailsCategory
		{
			get
			{
				if (fFinancialDetailsCategory == null)
				{
					fFinancialDetailsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|AccHotChequeFilter|FinancialDetails", "Financial Details"));
				}
				return fFinancialDetailsCategory;
			}
		}

		#endregion

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter("Master Bill #", AccHotChequeSchema.AQ_MasterBill).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|MasterBill", "Master Bill #");
			filters.AddNumberFilter("House Bill #", AccHotChequeSchema.AQ_HouseBill).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|HouseBill", "House Bill #");
			filters.AddNumberFilter("Check #", AccHotChequeSchema.AQ_ChequeNumber).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|Cheque", "Check #");
			filters.AddNumberFilter("Job Local Reference", GetLocalJobReferenceQuery)
				.WithMaxLengthOf(JobHeaderSchema.JH_JobLocalReference)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|LocalJobReference", "Job Local Reference");
		}

		#region GetLocalJobReferenceQuery

		ZQuery GetLocalJobReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				ZDBOnlyQuery selectAccTransHeaderQuery = new ZDBOnlyQuery(typeof(AccHotCheque));
				ZDBOnlySubQuery selectJobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccHotChequeSchema.AQ_JH);
				selectJobHeaderQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobLocalReference, comparisonOperator, value);
				selectJobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				selectAccTransHeaderQuery.AddSubQuery(selectJobHeaderQuery, JoinCondition.And);
				query.AddToFilter(selectAccTransHeaderQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#endregion

		#region Reference Filter

		void AddReferenceFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddGuidFilter("Creditor", ModuleIDs.Organisation, AccHotChequeSchema.AQ_OH, AQ_OHList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|Creditor", "Creditor");

			filter = filters.AddNkFilter("Staff", AccHotChequeSchema.AQ_GS_NKResponsibleStaff, ModuleIDs.GlbStaff, AQ_GSList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|Staff", "Staff");

			filter = filters.AddGuidFilter("Job", ModuleIDs.JobHeader, AccHotChequeSchema.AQ_JH, AQ_JHList);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|Job", "Job");

			filter = filters.AddNkFilter("Currency", GetCurrencyQuery, ModuleIDs.RefCurrency, CurrencyList);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|Currency", "Currency");
		}

		#endregion

		#region Other Filters

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNumberRangeFilter("Amount", AccHotChequeSchema.AQ_Amount);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|Amount", "Amount");

			filter = filters.AddTextFilter("Check Payee", AccHotChequeSchema.AQ_ChequePayee);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|ChequePayee", "Check Payee");

			filter = filters.AddTextFilter("Status", GetStatusQuery, StatusList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|Status", "Status");
		}

		#endregion

		#region Filter Delegates

		ZQuery GetCurrencyQuery(ZString value)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(AccHotCheque));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccChequeBook), AccHotChequeSchema.AQ_AK);
			ZDBOnlySubQuery subQuery2 = new ZDBOnlySubQuery(typeof(AccBankAccount), AccChequeBookSchema.AK_AB);
			subQuery2.AddToFilter(AccBankAccountSchema.AB_RX_NKAccountCurrency, value);
			subQuery.AddSubQuery(subQuery2, JoinCondition.And);
			dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
			return dBOnlyQuery;
		}

		ZQuery GetStatusQuery(ZString value)
		{
			if (value == "POSTED")
			{
				return new ZQuery(AccHotChequeSchema.AQ_AH, SQLComparisonOperator.NotEqual, null);
			}
			else if (value == "CANCELLED")
			{
				return new ZQuery(AccHotChequeSchema.AQ_Cancelled, true);
			}
			else if (value == "ACTIVE")
			{
				ZQuery filter = new ZQuery(AccHotChequeSchema.AQ_Cancelled, false);
				filter.AddToFilter(JoinCondition.And, AccHotChequeSchema.AQ_AH, SQLComparisonOperator.Equal, null);
				return new ZQuery(filter);
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region List Properties

		#region AQ_OHList

		protected OrgHeaderCollection fAQ_OHList;
		public OrgHeaderCollection AQ_OHList
		{
			get
			{
				if (fAQ_OHList == null)
				{
					fAQ_OHList = new OrgHeaderCollection(Factory);
				}
				return fAQ_OHList;
			}
		}

		#endregion

		#region AQ_GSList

		protected GlbStaffCollection fAQ_GSList;
		public GlbStaffCollection AQ_GSList
		{
			get
			{
				if (fAQ_GSList == null)
				{
					fAQ_GSList = new GlbStaffCollection(Factory);
				}
				return fAQ_GSList;
			}
		}

		#endregion

		#region AQ_JHList

		protected JobHeaderCollection fAQ_JHList;
		public JobHeaderCollection AQ_JHList
		{
			get
			{
				if (fAQ_JHList == null)
				{
					fAQ_JHList = new JobHeaderCollection(Factory);
				}
				return fAQ_JHList;
			}
		}

		#endregion

		#region StatusList

		protected CodeDescriptionPairList fStatusList;
		public CodeDescriptionPairList StatusList
		{
			get
			{
				if (fStatusList == null)
				{
					fStatusList = new CodeDescriptionPairList();
					fStatusList.AddPair("ACTIVE", Res.GetString("Accounting|AccHotChequeFilter|Active", "Active"));
					fStatusList.AddPair("POSTED", Res.GetString("Accounting|AccHotChequeFilter|Posted", "Posted"));
					fStatusList.AddPair("CANCELLED", Res.GetString("Accounting|AccHotChequeFilter|Cancelled", "Canceled"));
				}
				return fStatusList;
			}
		}

		#endregion

		#region CurrencyList

		protected RefCurrencyCollection fCurrencies;
		public RefCurrencyCollection CurrencyList
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}
				return fCurrencies;
			}
		}

		#endregion

		#endregion
	}
}
