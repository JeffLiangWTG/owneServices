using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Riba
{
	public class AddTransactionsToOrderFilterBusinessObject : FilterStripBusinessObject
	{
		public AddTransactionsToOrderFilterBusinessObject()
		{ }

		public AddTransactionsToOrderFilterBusinessObject(AccCollectionOrder order)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "AddTransactionsToOrderForm";
			this.Order = order;
		}

		readonly AccCollectionOrder Order;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var transactionTypefilter = filters.AddTextFilter("Transaction Type", AccTransactionHeaderSchema.AH_TransactionType, TransactionTypeList);
			transactionTypefilter.Category = FilterCategories.StatusAndFlags;
			transactionTypefilter.MultilingualDescription = ResString.GetMultilingualString("4042ee2b-636e-4d5b-bfe9-cf8addc90d7d", "Transaction Type");
			transactionTypefilter.Visibility = FilterVisibility.AlwaysVisible;

			ModuleTextFilter usedByBatchFilter = filters.AddTextFilter("Collection Batch", IncludedInCollectionBatchQuery, CollectionBatchStatusList);
			usedByBatchFilter.Category = FilterCategories.StatusAndFlags;
			usedByBatchFilter.DefaultProperty = "NAB";
			usedByBatchFilter.Visibility = FilterVisibility.AlwaysVisible;
			usedByBatchFilter.ReadOnly = true;
			usedByBatchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AddTransactionsToOrderFilter|CollectionBatch", "Collection Batch");

			var activePaymentMethodsList = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetActiveCodeDescriptionPairList();
			ModuleTextFilter agreedPaymentMethodOrganisationFilter = filters.AddTextFilter("Agreed Payment Method", AccTransactionHeaderSchema.AH_AgreedPaymentMethodOverride, activePaymentMethodsList);

			agreedPaymentMethodOrganisationFilter.Category = FilterCategories.Other;
			agreedPaymentMethodOrganisationFilter.DefaultProperty = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;
			agreedPaymentMethodOrganisationFilter.Visibility = FilterVisibility.AlwaysVisible;
			agreedPaymentMethodOrganisationFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AddTransactionsToOrderFilter|AgreedPaymentMethod", "Agreed Payment Method");

			ModuleTextFilter disbursementInvoiceFilter = filters.AddTextFilter("Disbursement Invoice", DisbursementInvoiceQuery, DisbursementInvoiceList);
			disbursementInvoiceFilter.Category = FilterCategories.StatusAndFlags;
			disbursementInvoiceFilter.DefaultProperty = "ALL";
			disbursementInvoiceFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AddTransactionsToOrderFilter|DisbursementInvoice", "Disbursement Invoice");

			filters.AddDateFilter("Post Date", AccTransactionHeaderSchema.AH_PostDate).MultilingualDescription = ResString.GetMultilingualString("3bbb0771-784d-4035-963d-1f2594146cc4", "Post Date");
			filters.AddDateFilter("Transaction Date", AccTransactionHeaderSchema.AH_InvoiceDate).MultilingualDescription = ResString.GetMultilingualString("e28c98ff-bfa0-4ce9-85ab-3e26c62c982e", "Transaction Date");
			filters.AddDateFilter("Due Date", AccTransactionHeaderSchema.AH_DueDate).MultilingualDescription = ResString.GetMultilingualString("5d9ffa76-93b2-4ae5-86db-d024ead4cf93", "Due Date");

			var departmentfilter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, Departments);
			departmentfilter.Category = FilterCategories.Organisations;
			departmentfilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AddTransactionsToOrderFilter|Department", "Department");

			var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, Branches);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AddTransactionsToOrderFilter|Branch", "Branch");

			return filters;
		}

		ZQuery IncludedInCollectionBatchQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery batchQuery = new ZDBOnlySubQuery(typeof(AccCollectionOrderLine), AccCollectionOrderLineSchema.AOL_AH, true);
			batchQuery.AddToFilter(AccCollectionOrderLineSchema.AOL_IsCancelled, false);
			query.AddSubQuery(batchQuery, JoinCondition.And);
			return query;
		}

		CodeDescriptionPairList CollectionBatchStatusList
		{
			get
			{
				if (fCollectionBatchStatusList == null)
				{
					fCollectionBatchStatusList = new CodeDescriptionPairList();
					fCollectionBatchStatusList.AddPair(Business.AccountingUtils.CollectionBatchStatusTypes.NotIncludeInActiveBatch, Res.GetString("Accounting|AddTransactionsToOrderFilter|NotInCollectionBatch", "Display transactions not included in active collection batch"));
				}
				return fCollectionBatchStatusList;
			}
		}
		CodeDescriptionPairList fCollectionBatchStatusList;

		ZQuery DisbursementInvoiceQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				if (value == "DSB")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, SQLComparisonOperator.Equal, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes);
				}
				else if (value == "STD")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, SQLComparisonOperator.NotEqual, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes);
				}
			}
			return query;
		}

		CodeDescriptionPairList DisbursementInvoiceList
		{
			get
			{
				if (disbursementInvoiceList == null)
				{
					disbursementInvoiceList = new CodeDescriptionPairList();
					disbursementInvoiceList.AddPair("ALL", Res.GetString("Accounting|AddTransactionsToOrderFilter|AllDisbursementAndNonDisbursement", "Both Disbursement and Non Disbursement Transactions"));
					disbursementInvoiceList.AddPair("DSB", Res.GetString("Accounting|AddTransactionsToOrderFilter|Disbursement", "Only Disbursement Transactions"));
					disbursementInvoiceList.AddPair("STD", Res.GetString("Accounting|AddTransactionsToOrderFilter|NonDisbursement", "Only Non-Disbursement Transactions"));
				}
				return disbursementInvoiceList;
			}
		}
		CodeDescriptionPairList disbursementInvoiceList;

		GlbBranchDependentCollection Branches
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		GlbDepartmentCollection Departments
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		#region TransactionTypeList

		CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList();
					fTransactionTypeList.AddPair("", Res.GetString("b08e204b-54c8-4300-8a8d-6af748dab251", "All Transactions"));
					fTransactionTypeList.AddPair("ADJ", Res.GetString("b8ed86fa-ba7e-46ba-99ff-63f5ee662467", "Adjustment Note"));
					fTransactionTypeList.AddPair("CRD", Res.GetString("a72a84b3-a923-4455-997e-99b11efe4070", "Credit Note"));
					fTransactionTypeList.AddPair("INV", Res.GetString("1a6914d6-d507-41fe-8789-174bc078ab77", "Invoice"));
					fTransactionTypeList.AddPair("JNL", Res.GetString("826EEE52-B01F-416B-9116-F3D49ED6D6DF", "Journal"));
				}
				return fTransactionTypeList;
			}
		}

		CodeDescriptionPairList fTransactionTypeList;

		#endregion

		public override ZQuery Filter
		{
			get
			{
				ZQuery filterQuery = base.Filter;
				filterQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				filterQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				filterQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] {
					TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote, TransactionTypes.Journal });
				filterQuery.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, ZDateTime.Empty);
				filterQuery.AddToFilter(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
				if (Order != null)
				{
					filterQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, Order.ACO_OH_Debtor);
					if (Order.ACO_RX_NKCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						filterQuery.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, Order.ACO_RX_NKCurrency);
					}
				}
				return filterQuery;
			}
		}
	}
}
