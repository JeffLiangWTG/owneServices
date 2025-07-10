using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.DFD.Export
{
	public class DFDFinancialInvoiceTransactionExportFilterForBatchProcess : DFDFinancialInvoiceTransactionExportFilter
	{
		public DFDFinancialInvoiceTransactionExportFilterForBatchProcess(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider, ZDateTime lastRun, ZDateTime nextRun)
			: base(factory, filterProvider)
		{
			this.LastRun = lastRun;
			this.NextRun = nextRun;
		}

		protected override void AddAdditionalFilter(ZDBOnlyQuery dBOnlyQuery)
		{
			base.AddAdditionalFilter(dBOnlyQuery);

			if (ExportingNewBatch)
			{
				dBOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, LastRun.ToDateTime());
				dBOnlyQuery.AddToFilter(AccTransactionHeaderSchema.AH_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, NextRun.ToDateTime());
			}
		}

		readonly ZDateTime LastRun;
		readonly ZDateTime NextRun;
	}
}
