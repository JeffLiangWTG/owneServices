using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccCollectionBatchFilterBusinessObject : FilterStripBusinessObject
	{
		public AccCollectionBatchFilterBusinessObject()
		{ }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddOrganisationFilters(filters);
			AddFinancialFilters(filters);
			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(AccCollectionBatchSchema.ACB_GC, GlbCompany.CurrentCompany.PK);
				return query;
			}
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.CollectionBatchNumber, AccCollectionBatchSchema.ACB_BatchNumber);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionBatchFilter|BatchNumber", "Batch #");
			filter.UseMultiSearch = true;
			ModuleFilter filter2 = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.CollectionBatchType, AccCollectionBatchSchema.ACB_Type);
			filter2.Category = FilterCategories.ModesAndTypes;
			filter2.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionBatchFilter|BatchType", "Batch Type");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddDateFilter("Collection Date", GetCollectionDateQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionBatchFilter|CollectionDate", "Collection Date");
		}

		ZQuery GetCollectionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccCollectionBatch));
			ZDBOnlySubQuery orderSubQuery = new ZDBOnlySubQuery(typeof(AccCollectionOrder), AccCollectionOrderSchema.ACO_ACB);
			AddDateTimeRange(orderSubQuery, comparisonOperator, JoinCondition.And, AccCollectionOrderSchema.ACO_CollectionDate, date1, date2);
			result.AddSubQuery(orderSubQuery, JoinCondition.And);
			return result;
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter debtorFilter = filters.AddGuidFilter("Debtor", ModuleIDs.Organisation, GetDebtorQuery, DebtorList);
			debtorFilter.Category = FilterCategories.Organisations;
			debtorFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionBatchFilter|Debtor", "Debtor");
		}

		ZQuery GetDebtorQuery(ZGuid debtorPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccCollectionBatch));
			ZDBOnlySubQuery orderSubQuery = new ZDBOnlySubQuery(typeof(AccCollectionOrder), AccCollectionOrderSchema.ACO_ACB);
			orderSubQuery.AddToFilter(AccCollectionOrderSchema.ACO_OH_Debtor, debtorPK);
			result.AddSubQuery(orderSubQuery, JoinCondition.And);
			return result;
		}

		OrgHeaderCollection debtorList;
		OrgHeaderCollection DebtorList
		{
			get { return debtorList ?? (debtorList = new OrgHeaderCollection(new BusinessObjectFactory(), GetDebtorListFilter())); }
		}

		ZQuery GetDebtorListFilter()
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, true);
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		void AddFinancialFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter bankAccountFilter = filters.AddGuidFilter("Bank Account", ModuleIDs.AccBankAccount, AccCollectionBatchSchema.ACB_AB, BankAccounts);
			bankAccountFilter.Category = FilterCategories.FinancialDetails;
			bankAccountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionBatchFilter|BankAccount", "Bank Account");
		}

		AccBankAccountCollection bankAccounts;
		AccBankAccountCollection BankAccounts
		{
			get
			{
				if (bankAccounts == null)
				{
					bankAccounts = new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany);
				}
				return bankAccounts;
			}
		}

		AccCollectionOrderCollection fBatchCollectionOrders;
		public AccCollectionOrderCollection BatchCollectionOrders
		{
			get
			{
				if (fBatchCollectionOrders == null)
				{
					fBatchCollectionOrders = new AccCollectionOrderCollection(Factory);
					fBatchCollectionOrders.AdditionalFilter = ZQuery.NoResultQuery;
				}
				return fBatchCollectionOrders;
			}
		}
	}
}
