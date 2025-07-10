using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class DepositBatchFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public DepositBatchFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			ModuleFountainFilter batchNumberFilter = filters.AddFountainFilter("Batch Number", AccTransactionHeaderSchema.AH_TransactionNum, "");
			batchNumberFilter.ComparisonOperator_List.RemoveCode(ModuleFountainFilter.ComparisonConstants.StartsWith);
			batchNumberFilter.ComparisonOperator_List.RemoveCode(ModuleFountainFilter.ComparisonConstants.Contains);
			batchNumberFilter.ComparisonOperator_List.RemoveCode(ModuleFountainFilter.ComparisonConstants.NotStartsWith);
			batchNumberFilter.ComparisonOperator_List.RemoveCode(ModuleFountainFilter.ComparisonConstants.NotContain);
			batchNumberFilter.ComparisonOperator_List.RemoveCode(ModuleFountainFilter.ComparisonConstants.NotEqual);
			batchNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|BatchNumber", "Batch Number");

			filters.AddDateFilter("Deposit Date", AccTransactionHeaderSchema.AH_InvoiceDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|DirectDebitFileFilter|DepositDate", "Deposit Date");

			return filters;
		}
	}
}
