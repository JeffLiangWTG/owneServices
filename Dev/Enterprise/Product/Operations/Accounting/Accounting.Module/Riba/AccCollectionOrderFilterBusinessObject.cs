using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccCollectionOrderFilterBusinessObject : FilterStripBusinessObject
	{
		public AccCollectionOrderFilterBusinessObject()
		{ }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
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
				ZQuery filter = base.Filter;

				var query = new ZDBOnlyQuery(typeof(AccCollectionOrder));
				var batchSubQuery = new ZDBOnlySubQuery(typeof(AccCollectionBatch), AccCollectionOrderSchema.ACO_ACB);
				batchSubQuery.AddToFilter(AccCollectionBatchSchema.ACB_GC, GlbCompany.CurrentCompany.PK);
				query.AddSubQuery(batchSubQuery, JoinCondition.And);

				filter.AddToFilter(query);
				return filter;
			}
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var collectionBatchSubGroup = new CollectionBatchSubGroup();

			var batchNumberFilter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.CollectionBatchNumber, AccCollectionBatchSchema.ACB_BatchNumber);
			batchNumberFilter.SubGroup = collectionBatchSubGroup;
			batchNumberFilter.Category = FilterCategories.NumbersAndReferences;
			batchNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionOrderFilter|BatchNumber", "Batch #");
			batchNumberFilter.UseMultiSearch = true;

			var batchTypeFilter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.CollectionBatchType, AccCollectionBatchSchema.ACB_Type);
			batchTypeFilter.SubGroup = collectionBatchSubGroup;
			batchTypeFilter.Category = FilterCategories.ModesAndTypes;
			batchTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionBatchFilter|BatchType", "Batch Type");

			var orderNumberFilter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.OrderNumber, AccCollectionOrderSchema.ACO_OrderNumber);
			orderNumberFilter.Category = FilterCategories.NumbersAndReferences;
			orderNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionOrderFilter|OrderNumber", "Order #");
			orderNumberFilter.UseMultiSearch = true;
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var collectionDateFilter = filters.AddDateFilter("Collection Date", AccCollectionOrderSchema.ACO_CollectionDate);
			collectionDateFilter.Category = FilterCategories.Dates;
			collectionDateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionOrderFilter|CollectionDate", "Collection Date");

			var depositedDateFilter = filters.AddDateFilter("Deposited Date", AccCollectionOrderSchema.ACO_DepositedDate);
			depositedDateFilter.Category = FilterCategories.Dates;
			depositedDateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionOrderFilter|DepositedDate", "Deposited Date");
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var debtorFilter = filters.AddGuidFilter("Debtor", ModuleIDs.Organisation, AccCollectionOrderSchema.ACO_OH_Debtor, DebtorList);
			debtorFilter.Category = FilterCategories.Organisations;
			debtorFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionOrderFilter|Debtor", "Debtor");
		}

		void AddFinancialFilters(ModuleFilterCollection filters)
		{
			var bankAccountFilter = filters.AddGuidFilter("Bank Account", ModuleIDs.AccBankAccount, AccCollectionBatchSchema.ACB_AB, BankAccounts);
			bankAccountFilter.SubGroup = new CollectionBatchSubGroup();
			bankAccountFilter.Category = FilterCategories.FinancialDetails;
			bankAccountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccCollectionOrderFilter|BankAccount", "Bank Account");
		}

		OrgHeaderCollection DebtorList => FindboxLookupCollections.GetDebtorCollection(Factory);

		AccBankAccountCollection BankAccounts => FindboxLookupCollections.GetBankAccounts(Factory, GlbCompany.CurrentCompany);

		class CollectionBatchSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(AccCollectionOrder));

				var batchSubQuery = new ZDBOnlySubQuery(typeof(AccCollectionBatch), AccCollectionOrderSchema.ACO_ACB);
				batchSubQuery.AddToFilter(AccCollectionBatchSchema.ACB_GC, GlbCompany.CurrentCompany.PK);
				batchSubQuery.AddToFilter(filter);

				result.AddSubQuery(batchSubQuery, JoinCondition.And);
				return result;
			}
		}
	}
}
