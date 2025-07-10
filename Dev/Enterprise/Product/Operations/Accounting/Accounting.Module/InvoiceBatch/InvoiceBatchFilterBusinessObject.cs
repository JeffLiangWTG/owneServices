using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class InvoiceBatchFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public InvoiceBatchFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			ModuleNumberFilter transactionNumberFilter = filters.AddNumberFilter("Batch Number", AccTransactionHeaderSchema.AH_TransactionNum);
			transactionNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoiceBatchFilter|BatchNumber", "Batch Number");
			transactionNumberFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.StartsWith);
			transactionNumberFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.Contains);
			transactionNumberFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotStartsWith);
			transactionNumberFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotContain);
			transactionNumberFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotEqual);

			ModuleFilter filter = filters.AddGuidFilter("Debtor", ModuleIDs.Organisation, AccTransactionHeaderSchema.AH_OH, DebtorsList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoiceBatchFilter|Debtor", "Debtor");

			filters.AddDateFilter("Post Date", AccTransactionHeaderSchema.AH_PostDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoiceBatchFilter|PostDate", "Post Date");
			filters.AddDateFilter("Transaction Date", AccTransactionHeaderSchema.AH_InvoiceDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoiceBatchFilter|TransactionDate", "Transaction Date");

			return filters;
		}

		#region DebtorsList

		public DebtorCollection DebtorsList
		{
			get { return FindboxLookupCollections.GetDebtorCollection(Factory); }
		}

		#endregion
	}
}
